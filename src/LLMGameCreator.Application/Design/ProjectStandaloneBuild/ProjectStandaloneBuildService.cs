using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;

namespace LLMGameCreator.Application.Design.ProjectStandaloneBuild;

public sealed class ProjectStandaloneBuildService : IProjectStandaloneBuildService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _repositoryRoot;
    private readonly object _gate = new();
    private Process? _ownedUnityProcess;
    private CancellationTokenSource? _buildCancellation;

    public ProjectStandaloneBuildService(string repositoryRoot)
    {
        _repositoryRoot = Path.GetFullPath(repositoryRoot);
    }

    public bool BuildRunning { get { lock (_gate) return _buildCancellation is not null; } }
    public ProjectStandaloneBuildResult? LastResult { get; private set; }

    public ProjectStandaloneBuildSettings LoadSettings(string projectFolder)
    {
        var path = Confined(projectFolder, ProjectStandaloneBuildVocabulary.SettingsRelativePath);
        if (!File.Exists(path)) return new();
        return JsonSerializer.Deserialize<ProjectStandaloneBuildSettings>(File.ReadAllText(path), JsonOptions) ?? new();
    }

    public ProjectStandaloneBuildSettings SaveSettings(string projectFolder, ProjectStandaloneBuildSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var path = Confined(projectFolder, ProjectStandaloneBuildVocabulary.SettingsRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(settings, JsonOptions), new UTF8Encoding(false));
        return settings;
    }

    public ProjectStandaloneBuildResult Build(ProjectStandaloneBuildRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ProjectFolder) || !Directory.Exists(request.ProjectFolder))
            return Finish(Fail(request, "validate_current_project", "Standalone build requires an opened project folder."));
        if (string.IsNullOrWhiteSpace(request.PackageSha256) || string.IsNullOrWhiteSpace(request.FinalStateHash))
            return Finish(Fail(request, "qualify_current_project", "Current project must pass Runtime qualification before standalone assembly."));

        lock (_gate)
        {
            if (_buildCancellation is not null)
                return Finish(Fail(request, "validate_current_project", "Standalone build is already running for this controller."));
            _buildCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        var started = Stopwatch.StartNew();
        var attemptId = Guid.NewGuid().ToString("N");
        try
        {
            var token = _buildCancellation!.Token;
            token.ThrowIfCancellationRequested();
            var packagePath = Path.Combine(request.ProjectFolder, "package.json");
            if (!File.Exists(packagePath)) return Finish(Fail(request, "validate_current_project", "Current project package.json is missing.", attemptId, started));
            if (!string.Equals(HashFile(packagePath), request.PackageSha256, StringComparison.Ordinal))
                return Finish(Fail(request, "qualify_current_project", "Package changed after qualification; standalone assembly was rejected.", attemptId, started));

            var settings = LoadSettings(request.ProjectFolder);
            var unity = ResolveUnityEditor(settings.UnityEditorPath);
            if (unity is null) return Finish(Fail(request, "resolve_unity_editor", "Unity Editor was not resolved. Choose a valid Editor executable or set UNITY_EDITOR_PATH.", attemptId, started));
            var unityVersion = FileVersionInfo.GetVersionInfo(unity).ProductVersion ?? "unknown";
            var cacheKey = HostCacheKey(unityVersion, settings);
            var cacheRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ProjectStandaloneBuildVocabulary.HostCacheRootName, cacheKey);
            var hostRoot = Path.Combine(cacheRoot, "host");
            var rebuilt = false;
            if (!HostIsComplete(hostRoot, cacheKey))
            {
                rebuilt = true;
                BuildHost(unity, hostRoot, cacheKey, token);
            }

            token.ThrowIfCancellationRequested();
            var payload = CreatePayload(request, packagePath, attemptId, unityVersion, cacheKey);
            var output = AssembleProjectOutput(request, hostRoot, payload, token);
            ValidateOutput(output, payload);
            var smoke = RunSmoke(output.ExecutablePath, token);
            if (!smoke.Passed)
                return Finish(Fail(request, "launch_smoke", smoke.Diagnostic, attemptId, started, output.OutputFolder, output.ExecutablePath, unity, unityVersion, cacheKey, rebuilt));

            var result = new ProjectStandaloneBuildResult
            {
                AttemptId = attemptId,
                Status = "GREEN",
                Stage = "publish_success",
                Diagnostics = ["Windows standalone Alpha", "Runtime-backed PlayerAdapter", "Gameplay truth: Runtime"],
                ProjectFolder = request.ProjectFolder,
                OutputFolder = output.OutputFolder,
                ExecutablePath = output.ExecutablePath,
                PackageSha256 = request.PackageSha256,
                FinalStateHash = request.FinalStateHash,
                SelectedModuleCount = request.SelectedModuleIds.Count,
                ConfiguredParameterCount = request.Parameters.Count,
                RuntimePlanId = request.RuntimePlanId,
                CapabilityCount = request.CapabilityCount,
                FrameCount = payload.Frames.Count,
                SelfCheckPassedCount = StandaloneSelfCheck.RequiredMarkers.Length,
                SelfCheckTotalCount = StandaloneSelfCheck.RequiredMarkers.Length,
                UnityEditorPath = unity,
                UnityVersion = unityVersion,
                HostCacheKey = cacheKey,
                HostRebuilt = rebuilt,
                HostReused = !rebuilt,
                LaunchSmokePassed = true,
                BuildManifestPath = output.ManifestPath,
                Duration = started.Elapsed
            };
            WriteHistory(request.ProjectFolder, result);
            return Finish(result);
        }
        catch (OperationCanceledException)
        {
            return Finish(Fail(request, "cancelled", "Standalone build was cancelled; prior successful output was preserved.", attemptId, started));
        }
        catch (Exception exception)
        {
            return Finish(Fail(request, "resolve_or_build_host_cache", exception.Message, attemptId, started));
        }
        finally
        {
            lock (_gate)
            {
                _ownedUnityProcess?.Dispose();
                _ownedUnityProcess = null;
                _buildCancellation?.Dispose();
                _buildCancellation = null;
            }
        }
    }

    public void Cancel()
    {
        lock (_gate)
        {
            _buildCancellation?.Cancel();
            if (_ownedUnityProcess is { HasExited: false } process)
                process.Kill(entireProcessTree: true);
        }
    }

    public void LaunchLastBuild()
    {
        if (LastResult is not { Status: "GREEN" } result || !File.Exists(result.ExecutablePath))
            throw new InvalidOperationException("No successful standalone executable is available.");
        Process.Start(new ProcessStartInfo(result.ExecutablePath) { UseShellExecute = true });
    }

    public void OpenLastBuildFolder()
    {
        if (LastResult is not { Status: "GREEN" } result || !Directory.Exists(result.OutputFolder))
            throw new InvalidOperationException("No successful standalone output folder is available.");
        Process.Start(new ProcessStartInfo(result.OutputFolder) { UseShellExecute = true });
    }

    private void BuildHost(string unityPath, string hostRoot, string cacheKey, CancellationToken token)
    {
        var workspaceService = new UnityHostBuildWorkspaceService(_repositoryRoot);
        var workspace = workspaceService.Prepare(token);
        var entrypoint = Path.Combine(workspace.ProjectPath, "Assets", "Editor", "ProjectStandaloneBuildEntrypoint.cs");
        if (!File.Exists(entrypoint)) throw new FileNotFoundException("Project standalone Unity build entrypoint is missing.", entrypoint);
        var cacheParent = Path.GetDirectoryName(hostRoot)!;
        var temporary = hostRoot + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(cacheParent);
        var log = Path.Combine(temporary, "unity-build.log");
        Directory.CreateDirectory(temporary);
        var arguments = workspaceService.CreateUnityArguments(Path.Combine(temporary, "host", ProjectStandaloneBuildVocabulary.HostExecutableName), log, workspace.ProjectPath);
        using var process = Process.Start(new ProcessStartInfo(unityPath, arguments)
        {
            UseShellExecute = false,
            CreateNoWindow = true
        }) ?? throw new InvalidOperationException("Unity Editor could not be started.");
        lock (_gate) _ownedUnityProcess = process;
        while (!process.WaitForExit(250)) token.ThrowIfCancellationRequested();
        lock (_gate) _ownedUnityProcess = null;
        if (process.ExitCode != 0 || !HostFilesPresent(Path.Combine(temporary, "host")))
        {
            var detail = File.Exists(log) ? File.ReadLines(log).TakeLast(20).Aggregate(new StringBuilder(), (builder, line) => builder.AppendLine(line)).ToString() : string.Empty;
            Directory.Delete(temporary, true);
            throw new InvalidOperationException("Unity host build failed: " + detail);
        }
        if (Directory.Exists(hostRoot)) Directory.Delete(hostRoot, true);
        Directory.Move(Path.Combine(temporary, "host"), hostRoot);
        File.WriteAllText(Path.Combine(hostRoot, "host-cache-manifest.json"), JsonSerializer.Serialize(new { schemaVersion = "llmgc_standalone_host_cache_v1", cacheKey }, JsonOptions), new UTF8Encoding(false));
        Directory.Delete(temporary, true);
    }

    private string HostCacheKey(string unityVersion, ProjectStandaloneBuildSettings settings)
    {
        var inputs = new[]
        {
            unityVersion,
            HashFile(Path.Combine(_repositoryRoot, "unity", "LLMGameCreatorAlpha", "Assets", "Scripts", "ProjectStandalonePlayerAdapterBootstrap.cs")),
            HashFile(Path.Combine(_repositoryRoot, "unity", "LLMGameCreatorAlpha", "Assets", "Editor", "ProjectStandaloneBuildEntrypoint.cs")),
            HashFile(Path.Combine(_repositoryRoot, "unity", "LLMGameCreatorAlpha", "Packages", "manifest.json")),
            UnityProjectVersionIdentity(Path.Combine(_repositoryRoot, "unity", "LLMGameCreatorAlpha", "ProjectSettings", "ProjectVersion.txt")),
            "StandaloneWindows64",
            settings.DevelopmentBuild.ToString(), settings.AllowDebugging.ToString(), settings.ConnectProfiler.ToString()
        };
        return HashText(string.Join("\n", inputs))[..32];
    }

    private static bool HostFilesPresent(string root) => File.Exists(Path.Combine(root, ProjectStandaloneBuildVocabulary.HostExecutableName))
        && Directory.Exists(Path.Combine(root, ProjectStandaloneBuildVocabulary.HostDataDirectoryName))
        && File.Exists(Path.Combine(root, "UnityPlayer.dll"))
        && Directory.Exists(Path.Combine(root, "MonoBleedingEdge"));

    private static bool HostIsComplete(string root, string cacheKey)
    {
        if (!HostFilesPresent(root)) return false;
        var manifest = Path.Combine(root, "host-cache-manifest.json");
        return File.Exists(manifest) && File.ReadAllText(manifest).Contains("\"cacheKey\": \"" + cacheKey + "\"", StringComparison.Ordinal);
    }

    private static string UnityProjectVersionIdentity(string projectVersionPath)
    {
        var line = File.ReadLines(projectVersionPath).FirstOrDefault(value => value.StartsWith("m_EditorVersion:", StringComparison.Ordinal));
        if (string.IsNullOrWhiteSpace(line)) throw new InvalidOperationException("Unity ProjectVersion.txt does not contain m_EditorVersion.");
        return line.Trim();
    }

    private static string? ResolveUnityEditor(string saved)
    {
        var candidates = new List<string> { saved, Environment.GetEnvironmentVariable("UNITY_EDITOR_PATH") ?? string.Empty };
        var hub = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Unity", "Hub", "Editor");
        if (Directory.Exists(hub)) candidates.AddRange(Directory.GetDirectories(hub).OrderBy(path => path, StringComparer.Ordinal).Select(path => Path.Combine(path, "Editor", "Unity.exe")));
        return candidates.Where(File.Exists).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
    }

    private static StandalonePayload CreatePayload(ProjectStandaloneBuildRequest request, string packagePath, string attemptId, string unityVersion, string cacheKey)
    {
        var packageJson = File.ReadAllText(packagePath);
        if (request.RuntimeFrames.Count == 0) throw new InvalidOperationException("Runtime qualification did not produce PlayerAdapter frames.");
        var frames = request.RuntimeFrames.OrderBy(frame => frame.Index).ThenBy(frame => frame.ActionId, StringComparer.Ordinal)
            .Select(frame => new StandaloneFrame
            {
                Index = frame.Index,
                Title = frame.Title,
                Category = frame.Category,
                StateHash = string.IsNullOrWhiteSpace(frame.StateHash) ? request.FinalStateHash : frame.StateHash
            }).ToList();
        return new StandalonePayload
        {
            ProjectManifest = new { schemaVersion = "llmgc_project_standalone_v2", request.ProjectPackageId, request.ProjectTitle, request.ProjectVersion, request.CompositionId, request.PackageSha256, request.CompositionPackageSha256, request.FinalStateHash, selectedModuleIds = request.SelectedModuleIds, effectiveParameters = request.Parameters, request.RequiredMechanicCount, request.SelectedOptionalMechanicCount, request.ActiveMechanicCount, request.ConfiguredParameterCount, request.PlannedActionCount, request.CheckpointActionCount, request.FinalReplayActionCount, request.RuntimePlanId, request.CapabilityCount, runtimeAuthority = true, unityGameplayTruth = false, projectionOnly = false, sourceCommit = Environment.GetEnvironmentVariable("GIT_COMMIT") ?? string.Empty, unityVersion, cacheKey, attemptId },
            PackageJson = packageJson,
            Frames = frames,
            Model = new { schemaVersion = "llmgc_player_adapter_model_v2", request.EquipmentSummary, request.AttributesSummary, request.ProgressionSummary, request.EquipmentDamageBonus, request.StatDamageBonus, request.TotalAdditionalDamage, humanReviewFacts = request.HumanReviewFacts, request.FinalStateHash, mapSummary = "Runtime-derived project package", inventorySummary = "Runtime-derived player state", questSummary = "Runtime-derived player state", combatSummary = "Runtime-derived player state", runtimeAuthority = true, unityGameplayTruth = false, projectionOnly = false },
            Launch = new { schemaVersion = "llmgc_standalone_launch_v2", smokeArguments = new[] { "-batchmode", "-nographics", "-llmgcStandaloneSmokeExit", "-llmgcStandaloneSmokeLogPath" }, runtimeAuthority = true, unityGameplayTruth = false, projectionOnly = false }
        };
    }

    private static OutputAssembly AssembleProjectOutput(ProjectStandaloneBuildRequest request, string hostRoot, StandalonePayload payload, CancellationToken token)
    {
        var slug = SafeSlug(string.IsNullOrWhiteSpace(request.ProjectPackageId) ? Path.GetFileName(request.ProjectFolder) : request.ProjectPackageId);
        var windowsRoot = Path.Combine(request.ProjectFolder, "Builds", "Windows");
        Directory.CreateDirectory(windowsRoot);
        var output = Path.Combine(windowsRoot, slug);
        var staging = Path.Combine(windowsRoot, "." + slug + ".staging-" + Guid.NewGuid().ToString("N"));
        CopyDirectory(hostRoot, staging, token);
        var oldExe = Path.Combine(staging, ProjectStandaloneBuildVocabulary.HostExecutableName);
        var oldData = Path.Combine(staging, ProjectStandaloneBuildVocabulary.HostDataDirectoryName);
        var exe = Path.Combine(staging, slug + ".exe");
        var data = Path.Combine(staging, slug + "_Data");
        File.Move(oldExe, exe, true);
        Directory.Move(oldData, data);
        var streaming = Path.Combine(data, "StreamingAssets", "LLMGameCreatorProject");
        Directory.CreateDirectory(streaming);
        File.WriteAllText(Path.Combine(streaming, "project-manifest.json"), JsonSerializer.Serialize(payload.ProjectManifest, JsonOptions), new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(streaming, "game-package.json"), payload.PackageJson, new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(streaming, "player-adapter-model.json"), JsonSerializer.Serialize(payload.Model, JsonOptions), new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(streaming, "player-adapter-frames.json"), JsonSerializer.Serialize(payload.Frames, JsonOptions), new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(streaming, "standalone-launch.json"), JsonSerializer.Serialize(payload.Launch, JsonOptions), new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(staging, "README.txt"), "Windows standalone Alpha\r\nRuntime-backed PlayerAdapter\r\nGameplay truth: Runtime\r\n", new UTF8Encoding(false));
        var manifest = Path.Combine(staging, "build-manifest.json");
        var hashes = Directory.GetFiles(staging, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal).ToDictionary(path => Path.GetRelativePath(staging, path).Replace('\\', '/'), HashFile, StringComparer.Ordinal);
        File.WriteAllText(manifest, JsonSerializer.Serialize(new { schemaVersion = "llmgc_project_standalone_build_v1", projectPackageId = request.ProjectPackageId, packageSha256 = request.PackageSha256, finalStateHash = request.FinalStateHash, files = hashes }, JsonOptions), new UTF8Encoding(false));
        var backup = output + ".previous-" + Guid.NewGuid().ToString("N");
        try
        {
            if (Directory.Exists(output)) Directory.Move(output, backup);
            Directory.Move(staging, output);
            if (Directory.Exists(backup)) Directory.Delete(backup, true);
        }
        catch
        {
            if (Directory.Exists(output)) Directory.Delete(output, true);
            if (Directory.Exists(backup)) Directory.Move(backup, output);
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
            throw;
        }
        return new OutputAssembly(output, Path.Combine(output, slug + ".exe"), Path.Combine(output, "build-manifest.json"));
    }

    private static void ValidateOutput(OutputAssembly output, StandalonePayload payload)
    {
        if (!File.Exists(output.ExecutablePath) || !File.Exists(output.ManifestPath)) throw new InvalidOperationException("Standalone output manifest or executable is missing.");
        var root = Path.GetDirectoryName(output.ExecutablePath)!;
        var data = Path.Combine(root, Path.GetFileNameWithoutExtension(output.ExecutablePath) + "_Data");
        if (!Directory.Exists(data) || !File.Exists(Path.Combine(root, "UnityPlayer.dll")) || !Directory.Exists(Path.Combine(root, "MonoBleedingEdge"))) throw new InvalidOperationException("Standalone output is not a complete Unity player set.");
        if (payload.Frames.Count == 0) throw new InvalidOperationException("PlayerAdapter payload has no frames.");
    }

    private static SmokeResult RunSmoke(string executable, CancellationToken token)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LLMGameCreator", "S");
        Directory.CreateDirectory(root);
        var log = Path.Combine(root, "standalone-smoke-" + Guid.NewGuid().ToString("N") + ".log");
        using var process = Process.Start(new ProcessStartInfo(executable, "-batchmode -nographics -llmgcStandaloneSmokeExit -llmgcStandaloneSmokeLogPath \"" + log + "\"") { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = Path.GetDirectoryName(executable)! }) ?? throw new InvalidOperationException("Standalone executable could not be started.");
        while (!process.WaitForExit(250)) token.ThrowIfCancellationRequested();
        var contents = File.Exists(log) ? File.ReadAllText(log) : string.Empty;
        var passed = process.ExitCode == 0 && StandaloneSelfCheck.RequiredMarkers.All(marker => contents.Contains(marker, StringComparison.Ordinal));
        return new SmokeResult(passed, passed ? string.Empty : "Standalone smoke failed with exit code " + process.ExitCode + ".");
    }

    private static void CopyDirectory(string source, string destination, CancellationToken token)
    {
        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories)) Directory.CreateDirectory(directory.Replace(source, destination, StringComparison.Ordinal));
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories)) { token.ThrowIfCancellationRequested(); var target = file.Replace(source, destination, StringComparison.Ordinal); Directory.CreateDirectory(Path.GetDirectoryName(target)!); File.Copy(file, target, true); }
    }

    private static string Confined(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative));
        if (!path.StartsWith(fullRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) throw new InvalidOperationException("Project path escape rejected.");
        return path;
    }

    private void WriteHistory(string projectFolder, ProjectStandaloneBuildResult result)
    {
        var path = Confined(projectFolder, ProjectStandaloneBuildVocabulary.HistoryRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var history = File.Exists(path) ? JsonSerializer.Deserialize<List<ProjectStandaloneBuildResult>>(File.ReadAllText(path), JsonOptions) ?? [] : [];
        history.Add(result); File.WriteAllText(path, JsonSerializer.Serialize(history.TakeLast(20), JsonOptions), new UTF8Encoding(false));
    }

    private ProjectStandaloneBuildResult Finish(ProjectStandaloneBuildResult result) { LastResult = result; return result; }
    private static ProjectStandaloneBuildResult Fail(ProjectStandaloneBuildRequest request, string stage, string diagnostic, string attemptId = "", Stopwatch? duration = null, string output = "", string executable = "", string unity = "", string version = "", string cacheKey = "", bool rebuilt = false) => new() { AttemptId = attemptId, Status = stage == "cancelled" ? "CANCELLED" : "FAILED", Stage = stage, Diagnostics = [diagnostic], ProjectFolder = request.ProjectFolder, OutputFolder = output, ExecutablePath = executable, PackageSha256 = request.PackageSha256, FinalStateHash = request.FinalStateHash, SelectedModuleCount = request.SelectedModuleIds.Count, ConfiguredParameterCount = request.Parameters.Count, UnityEditorPath = unity, UnityVersion = version, HostCacheKey = cacheKey, HostRebuilt = rebuilt, Duration = duration?.Elapsed ?? TimeSpan.Zero };
    private static string HashFile(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static string HashText(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static string SafeSlug(string value) { var builder = new StringBuilder(); foreach (var c in value.ToLowerInvariant()) builder.Append(char.IsLetterOrDigit(c) ? c : '-'); return builder.ToString().Trim('-') is { Length: > 0 } slug ? slug : "game"; }
    private sealed record StandaloneFrame { public int Index { get; init; } public string Title { get; init; } = string.Empty; public string Category { get; init; } = string.Empty; public string StateHash { get; init; } = string.Empty; }
    private sealed record StandalonePayload { public object ProjectManifest { get; init; } = new(); public string PackageJson { get; init; } = string.Empty; public object Model { get; init; } = new(); public List<StandaloneFrame> Frames { get; init; } = []; public object Launch { get; init; } = new(); }
    private sealed record OutputAssembly(string OutputFolder, string ExecutablePath, string ManifestPath);
    private sealed record SmokeResult(bool Passed, string Diagnostic);
}

internal static class StandaloneSelfCheck
{
    public static readonly string[] RequiredMarkers = [
        "LLMGC_PROJECT_STANDALONE_LOAD_PASS",
        "LLMGC_PROJECT_STANDALONE_INTEGRITY_PASS",
        "LLMGC_PROJECT_STANDALONE_NAVIGATION_PASS",
        "LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS",
        "LLMGC_PROJECT_STANDALONE_SMOKE_PASS"
    ];
}
