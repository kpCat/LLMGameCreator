using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.ProjectStandaloneBuild;

public sealed class ProjectStandaloneOutputLocationService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
    private readonly string _root;
    private readonly Action<string>? _beforePublicationStage;

    public ProjectStandaloneOutputLocationService(string? rootOverride = null, Action<string>? beforePublicationStage = null)
    {
        var configured = rootOverride ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LGC", "O");
        if (configured.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(segment => segment == ".."))
            throw new InvalidOperationException("standalone.output.path_escape");
        _root = Path.GetFullPath(configured).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        RejectReparsePoint(_root);
        _beforePublicationStage = beforePublicationStage;
    }

    public string Root => _root;

    public ProjectStandaloneOutputLocation Resolve(string projectFolder, string packageId, string attemptToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFolder);
        if (string.IsNullOrWhiteSpace(attemptToken) || attemptToken.Any(character => !char.IsAsciiLetterOrDigit(character)))
            throw new InvalidOperationException("standalone.output.path_escape");
        var normalizedProject = Path.GetFullPath(projectFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var tokenInput = OperatingSystem.IsWindows() ? normalizedProject.ToUpperInvariant() + "\n" + packageId : normalizedProject + "\n" + packageId;
        var projectToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokenInput))).ToLowerInvariant()[..16];
        var projectRoot = Confined(_root, projectToken);
        var runs = Confined(projectRoot, "runs");
        var runName = "r-" + attemptToken.ToLowerInvariant()[..Math.Min(12, attemptToken.Length)];
        var run = Confined(runs, runName);
        return new ProjectStandaloneOutputLocation
        {
            Root = _root, ProjectToken = projectToken, ProjectRoot = projectRoot, RunsFolder = runs,
            RunDirectoryName = runName, RunOutputFolder = run, CurrentPointerPath = Confined(projectRoot, "current.json"),
            // Compatibility-only paths for pre-Goal161S diagnostics. Production never uses them.
            CurrentOutputFolder = Confined(projectRoot, "current"),
            StagingOutputFolder = Confined(projectRoot, "s-" + attemptToken[..Math.Min(12, attemptToken.Length)]),
            BackupOutputFolder = Confined(projectRoot, "b-" + attemptToken[..Math.Min(12, attemptToken.Length)])
        };
    }

    public ProjectStandaloneRunLocation ResolveRun(string projectFolder, string packageId, string attemptToken)
    {
        var location = Resolve(projectFolder, packageId, attemptToken);
        return new ProjectStandaloneRunLocation { ProjectToken = location.ProjectToken, ProjectRoot = location.ProjectRoot, RunsFolder = location.RunsFolder, RunDirectoryName = location.RunDirectoryName, RunOutputFolder = location.RunOutputFolder, CurrentPointerPath = location.CurrentPointerPath };
    }

    public ProjectStandaloneOutputPathBudgetResult ValidatePlayerPathBudget(string outputFolder, string markerLogPath, string playerLogPath, string? currentPointerPath = null, string? runStatusPath = null)
    {
        var root = Path.GetFullPath(outputFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var candidates = new List<(string Absolute, string Relative)>
        {
            (Path.Combine(root, "g.exe"), "g.exe"), (Path.Combine(root, "g_Data"), "g_Data"),
            (Path.Combine(root, "UnityPlayer.dll"), "UnityPlayer.dll"), (Path.Combine(root, "MonoBleedingEdge"), "MonoBleedingEdge"),
            (Path.Combine(root, "build-manifest.json"), "build-manifest.json"), (Path.GetFullPath(markerLogPath), "smoke-marker/" + Path.GetFileName(markerLogPath)),
            (Path.GetFullPath(playerLogPath), "player-log/" + Path.GetFileName(playerLogPath))
        };
        if (!string.IsNullOrWhiteSpace(currentPointerPath)) candidates.Add((Path.GetFullPath(currentPointerPath), "current-pointer/" + Path.GetFileName(currentPointerPath)));
        if (!string.IsNullOrWhiteSpace(runStatusPath)) candidates.Add((Path.GetFullPath(runStatusPath), "run-status/" + Path.GetFileName(runStatusPath)));
        var payload = Path.Combine(root, "g_Data", "StreamingAssets", "LLMGameCreatorProject");
        if (Directory.Exists(payload)) candidates.AddRange(Directory.GetFiles(payload, "*", SearchOption.AllDirectories).Select(path => (path, Path.GetRelativePath(root, path).Replace('\\', '/'))));
        var longest = candidates.OrderByDescending(item => item.Absolute.Length).ThenBy(item => item.Relative, StringComparer.Ordinal).FirstOrDefault();
        return new ProjectStandaloneOutputPathBudgetResult
        {
            MaximumAbsolutePathLength = longest.Absolute?.Length ?? 0, LongestRelativePath = longest.Relative ?? string.Empty,
            Passed = candidates.All(item => item.Absolute.Length <= ProjectStandaloneBuildVocabulary.PlayerPathBudgetLimit),
            Diagnostics = candidates.Where(item => item.Absolute.Length > ProjectStandaloneBuildVocabulary.PlayerPathBudgetLimit).OrderBy(item => item.Relative, StringComparer.Ordinal).Select(item => "standalone.output.player_path_budget_exceeded:" + item.Relative + ":" + item.Absolute.Length).ToList()
        };
    }

    public void WriteRunStatus(ProjectStandaloneOutputLocation location, ProjectStandaloneRunStatus status)
    {
        var path = Confined(location.RunOutputFolder, "run-status.json");
        AtomicWrite(path, JsonSerializer.Serialize(status, JsonOptions));
    }

    public ProjectStandalonePublicationResult PublishCurrentPointer(ProjectStandaloneOutputLocation location, ProjectStandaloneCurrentPointer pointer)
    {
        var prior = File.Exists(location.CurrentPointerPath) ? File.ReadAllBytes(location.CurrentPointerPath) : null;
        var temporary = location.CurrentPointerPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            ValidatePointer(location, pointer);
            _beforePublicationStage?.Invoke("temp_pointer_write");
            WriteThrough(temporary, JsonSerializer.Serialize(pointer, JsonOptions));
            _beforePublicationStage?.Invoke("temporary_pointer_validation");
            var temporaryPointer = ReadPointer(temporary);
            ValidatePointer(location, temporaryPointer);
            _beforePublicationStage?.Invoke("atomic_replace");
            File.Move(temporary, location.CurrentPointerPath, true);
            _beforePublicationStage?.Invoke("published_pointer_validation");
            var current = ReadPointer(location.CurrentPointerPath);
            ValidatePointer(location, current);
            return new ProjectStandalonePublicationResult { Passed = true, Stage = "published_pointer_validation", CurrentPointerPath = location.CurrentPointerPath, CurrentPointerSha256 = HashFile(location.CurrentPointerPath), PriorCurrentPreserved = false };
        }
        catch (Exception exception)
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            var preserved = prior is null ? !File.Exists(location.CurrentPointerPath) : File.Exists(location.CurrentPointerPath) && prior.SequenceEqual(File.ReadAllBytes(location.CurrentPointerPath));
            return new ProjectStandalonePublicationResult { Passed = false, Stage = PublicationStage(exception), Diagnostic = exception.GetType().Name + ": " + exception.Message, CurrentPointerPath = location.CurrentPointerPath, PriorCurrentPreserved = preserved };
        }
    }

    public ProjectStandaloneCurrentOutputReadResult LoadCurrentOutput(string projectFolder, string packageId)
    {
        var location = Resolve(projectFolder, packageId, "000000000000");
        if (!File.Exists(location.CurrentPointerPath)) return new ProjectStandaloneCurrentOutputReadResult { Diagnostic = "standalone.current_pointer_missing" };
        try
        {
            var pointer = ReadPointer(location.CurrentPointerPath);
            ValidatePointer(location, pointer);
            var run = Confined(location.RunsFolder, pointer.RunDirectoryName);
            return new ProjectStandaloneCurrentOutputReadResult { Passed = true, Pointer = pointer, RunOutputFolder = run, ExecutablePath = Path.Combine(run, pointer.ExecutableRelativePath) };
        }
        catch (Exception exception) { return new ProjectStandaloneCurrentOutputReadResult { Diagnostic = exception.GetType().Name + ": " + exception.Message }; }
    }

    private static void ValidatePointer(ProjectStandaloneOutputLocation location, ProjectStandaloneCurrentPointer pointer)
    {
        if (pointer.SchemaVersion != "standalone_current_output_v1" || pointer.ProjectToken != location.ProjectToken || !SafeRunName(pointer.RunDirectoryName) || pointer.ExecutableRelativePath != "g.exe" || pointer.BuildManifestRelativePath != "build-manifest.json") throw new InvalidOperationException("standalone.current_pointer_invalid");
        var run = Confined(location.RunsFolder, pointer.RunDirectoryName);
        RejectReparsePoint(run);
        foreach (var path in new[] { Path.Combine(run, "g.exe"), Path.Combine(run, "UnityPlayer.dll"), Path.Combine(run, "build-manifest.json"), Path.Combine(run, "run-status.json") }) if (!File.Exists(path)) throw new InvalidOperationException("standalone.current_pointer_run_incomplete");
        if (!Directory.Exists(Path.Combine(run, "g_Data")) || !Directory.Exists(Path.Combine(run, "MonoBleedingEdge"))) throw new InvalidOperationException("standalone.current_pointer_run_incomplete");
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(run, "build-manifest.json")));
        using var project = JsonDocument.Parse(File.ReadAllText(Path.Combine(run, "g_Data", "StreamingAssets", "LLMGameCreatorProject", "project-manifest.json")));
        var root = project.RootElement;
        if (manifest.RootElement.GetProperty("schemaVersion").GetString() != "llmgc_project_standalone_build_v1" || root.GetProperty("packageSha256").GetString() != pointer.PackageSha256 || root.GetProperty("compositionPackageSha256").GetString() != pointer.CompositionPackageSha256 || root.GetProperty("finalStateHash").GetString() != pointer.FinalStateHash) throw new InvalidOperationException("standalone.current_pointer_hash_mismatch");
        var selfCheck = new ProjectStandalonePayloadSelfCheckService().CheckOutput(run, Path.Combine(run, "g.exe"));
        var status = JsonSerializer.Deserialize<ProjectStandaloneRunStatus>(File.ReadAllText(Path.Combine(run, "run-status.json")), JsonOptions) ?? throw new InvalidOperationException("standalone.current_pointer_status_invalid");
        if (!selfCheck.Passed || status.SchemaVersion != "standalone_run_status_v1" || status.Status != "GREEN" || !status.PayloadSelfCheckPassed || !status.LegacyParserCompatibilityPassed || !status.SmokeMarkersPassed || !status.PlayerLogPresent || status.SmokeExitCode != 0 || status.PackageSha256 != pointer.PackageSha256 || status.FinalStateHash != pointer.FinalStateHash) throw new InvalidOperationException("standalone.current_pointer_status_invalid");
    }

    private static ProjectStandaloneCurrentPointer ReadPointer(string path) => JsonSerializer.Deserialize<ProjectStandaloneCurrentPointer>(File.ReadAllText(path), JsonOptions) ?? throw new InvalidOperationException("standalone.current_pointer_invalid");
    private static string PublicationStage(Exception exception) => exception.Message.Contains("temp_pointer_write", StringComparison.Ordinal) ? "temp_pointer_write" : exception.Message.Contains("temporary_pointer_validation", StringComparison.Ordinal) ? "temporary_pointer_validation" : exception.Message.Contains("atomic_replace", StringComparison.Ordinal) ? "atomic_replace" : exception.Message.Contains("published_pointer_validation", StringComparison.Ordinal) ? "published_pointer_validation" : "validate_immutable_run";
    private static bool SafeRunName(string value) => value.Length == 14 && value.StartsWith("r-", StringComparison.Ordinal) && value[2..].All(character => char.IsAsciiDigit(character) || (char.IsAsciiLetter(character) && char.IsLower(character)));
    private static void AtomicWrite(string path, string text) { var temp = path + ".tmp-" + Guid.NewGuid().ToString("N"); try { WriteThrough(temp, text); File.Move(temp, path, true); } finally { if (File.Exists(temp)) File.Delete(temp); } }
    private static void WriteThrough(string path, string text) { Directory.CreateDirectory(Path.GetDirectoryName(path)!); using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough); using var writer = new StreamWriter(stream, new UTF8Encoding(false)); writer.Write(text); writer.Flush(); stream.Flush(true); }
    private static string HashFile(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static string Confined(string root, string child) { var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar; var path = Path.GetFullPath(Path.Combine(fullRoot, child)); if (!path.StartsWith(fullRoot, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) throw new InvalidOperationException("standalone.output.path_escape"); return path; }
    private static void RejectReparsePoint(string path) { if (Directory.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("standalone.output.path_escape"); }
}
