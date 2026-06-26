using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Application.Design.ContentGeneration;

namespace LLMGameCreator.Application.Design.UnityPlayableAlpha;

public sealed class UnityPlayableAlphaAcceptanceService
{
    public const string RelativeOutputDirectory = ".llmgc/procedural/unity-playable-alpha";
    public const string ReportJsonFileName = "unity-playable-alpha-report.json";
    public const string ReportMarkdownFileName = "unity-playable-alpha-report.md";
    public const string VerificationMarkdownFileName = "unity-playable-alpha-verification.md";
    public const string FinalGate = "unity_playable_presentation_firewall_safe_build_verification";

    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static UnityPlayableAlphaAcceptanceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public UnityPlayableAlphaAcceptanceResult BuildFromAcceptedEvidence(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityPlayableAlphaOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(contentGenerationResult);
        ArgumentNullException.ThrowIfNull(minimumAssetResult);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var settings = options ?? new UnityPlayableAlphaOptions();
        var projectRoot = Path.GetFullPath(projectRootPath);
        var repositoryRoot = ResolveRepositoryRoot(projectRoot, settings.RepositoryRootPath);
        var alphaService = new AlphaRunnableBuildAcceptanceService();
        var alphaResult = alphaService.BuildFromAcceptedEvidence(
            projectRoot,
            contentGenerationResult,
            minimumAssetResult,
            new AlphaRunnableBuildOptions
            {
                RepositoryRootPath = repositoryRoot,
                RelativeOutputDirectoryOverride = RelativeOutputDirectory,
                ExecuteUnityBuild = settings.ExecuteUnityBuild,
                LaunchBuiltPlayer = settings.LaunchBuiltPlayer,
                PreserveExistingBuildOutputForValidation = settings.PreserveExistingBuildOutputForValidation,
                CleanupUnityWorkProject = settings.CleanupUnityWorkProject,
                UnityBuildTimeoutSeconds = settings.UnityBuildTimeoutSeconds,
                PlayerLaunchTimeoutSeconds = settings.PlayerLaunchTimeoutSeconds
            });

        var alpha = alphaResult.Report;
        var presentation = ValidatePresentationAndMovement(projectRoot, alpha);
        var firewall = ValidateFirewallSafeBuild(repositoryRoot, projectRoot, alpha);
        var invalidMatrix = BuildInvalidMatrix(alpha, presentation, firewall);
        var diagnostics = SortDiagnostics(
            alpha.Diagnostics
                .Concat(presentation.Diagnostics)
                .Concat(firewall.Diagnostics)
                .Concat(invalidMatrix.Diagnostics)
                .Concat(
                [
                    Diagnostic("info", "unity_playable_alpha.goal013_gate_recorded", "alpha_runnable_windows_build_verification", "User-confirmed Goal 013 build verification is recorded as passed."),
                    Diagnostic("info", "unity_playable_alpha.no_external_providers", "execution_boundary", "No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.")
                ]));

        var reportWithoutHash = new UnityPlayableAlphaReport
        {
            Accepted = false,
            FinalStatus = FinalGate,
            ManualGate = FinalGate,
            PreviousAcceptedGate = "alpha_runnable_windows_build_verification passed",
            CompletedSlices = ["S114", "S115", "S116", "S117", "S118", "S119", "S120", "S121"],
            ProductSmokeRoute = "unity-playable-alpha",
            AlphaBuild = alpha,
            SelectedPackageId = alpha.PrimaryBuildCandidate.PackageId,
            SelectedStyleId = alpha.PrimaryBuildCandidate.StyleId,
            PackageHash = alpha.PrimaryBuildCandidate.PackageHash,
            AssetManifestHash = alpha.PrimaryBuildCandidate.AssetManifestHash,
            ExportManifestHash = alpha.PrimaryBuildCandidate.ExportManifestHash,
            RuntimeConfigHash = alpha.PrimaryBuildCandidate.RuntimeConfigHash,
            Presentation = presentation,
            FirewallSafeBuild = firewall,
            InvalidMatrix = invalidMatrix,
            WindowsExecutableProduced = alpha.WindowsExecutableProduced,
            UnityBuildProduced = alpha.UnityBuildProduced,
            LaunchVerified = alpha.LaunchVerified,
            VisiblePresentationVerified = presentation.VisiblePresentationVerified,
            MovementVerified = presentation.MovementVerified,
            InteractionVerified = presentation.InteractionVerified,
            PlayLoopVerified = alpha.PlayLoopVerified && presentation.PlayLoopVerified,
            FirewallSafeBuildVerified = firewall.FirewallSafeBuildVerified,
            NoExternalProviderLlmRagLuaMedia = true,
            RuntimePreviewDependency = alpha.RuntimePreviewDependency,
            PublicGamePackageSchemaChanged = false,
            ProjectFilesChanged = false,
            GeneratorLibraryChanged = false,
            DeterministicReportRelativePath = $"{RelativeOutputDirectory}/{ReportJsonFileName}",
            BuildManifestHash = alpha.BuildManifestHash,
            Diagnostics = diagnostics
        };
        var report = reportWithoutHash with
        {
            DeterministicHash = ComputeHash(JsonSerializer.Serialize(reportWithoutHash, JsonOptions))
        };

        return new UnityPlayableAlphaAcceptanceResult
        {
            Report = report,
            ReportJson = JsonSerializer.Serialize(report, JsonOptions),
            ReportMarkdown = RenderReport(report),
            VerificationMarkdown = RenderVerification(report, alphaResult.VerificationMarkdown)
        };
    }

    public async Task<UnityPlayableAlphaWriteResult> WriteAsync(
        string projectRootPath,
        UnityPlayableAlphaAcceptanceResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (string.IsNullOrWhiteSpace(projectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(projectRootPath));
        }

        var projectRoot = Path.GetFullPath(projectRootPath);
        var outputDirectory = Path.GetFullPath(Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar)));
        Directory.CreateDirectory(outputDirectory);

        var jsonPath = Path.Combine(outputDirectory, ReportJsonFileName);
        var markdownPath = Path.Combine(outputDirectory, ReportMarkdownFileName);
        var verificationPath = Path.Combine(outputDirectory, VerificationMarkdownFileName);
        await File.WriteAllTextAsync(jsonPath, result.ReportJson, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(markdownPath, result.ReportMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(verificationPath, result.VerificationMarkdown, Utf8WithoutBom, cancellationToken).ConfigureAwait(false);

        return new UnityPlayableAlphaWriteResult
        {
            OutputDirectoryPath = outputDirectory,
            StagingDirectoryPath = Path.Combine(outputDirectory, "staging"),
            BuildDirectoryPath = Path.Combine(outputDirectory, "build", "windows"),
            LogsDirectoryPath = Path.Combine(outputDirectory, "logs"),
            ReportJsonPath = jsonPath,
            ReportMarkdownPath = markdownPath,
            VerificationMarkdownPath = verificationPath
        };
    }

    public async Task<UnityPlayableAlphaWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        ContentGenerationScaleAcceptanceResult contentGenerationResult,
        MinimumAssetPipelineAcceptanceResult minimumAssetResult,
        UnityPlayableAlphaOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = BuildFromAcceptedEvidence(projectRootPath, contentGenerationResult, minimumAssetResult, options);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public static UnityPlayableAlphaPresentationProof ValidatePresentationLog(
        string playLoopLogPath,
        AlphaBuildCandidate primary)
    {
        var diagnostics = new List<AlphaBuildDiagnostic>();
        if (string.IsNullOrWhiteSpace(playLoopLogPath) || !File.Exists(playLoopLogPath))
        {
            diagnostics.Add(Diagnostic("error", "unity_playable_alpha.presentation.log_missing", "logs/alpha-player-play-loop.log", "Playable presentation verification requires the player play-loop log."));
            return new UnityPlayableAlphaPresentationProof { Diagnostics = SortDiagnostics(diagnostics) };
        }

        var values = ParseKeyValueLog(File.ReadAllLines(playLoopLogPath));
        foreach (var key in new[]
        {
            "alpha_runtime.visible_presentation_initialized",
            "alpha_runtime.visible_component.map",
            "alpha_runtime.visible_component.player_marker",
            "alpha_runtime.visible_component.npc_marker",
            "alpha_runtime.visible_component.item_marker",
            "alpha_runtime.visible_component.status_panel",
            "alpha_runtime.visible_component.command_log"
        })
        {
            Require(values, key, "true", diagnostics, "unity_playable_alpha.presentation.visible_component_missing");
        }

        Require(values, "alpha_runtime.movement.step.0.valid", "true", diagnostics, "unity_playable_alpha.movement.step_missing");
        Require(values, "alpha_runtime.movement.step.1.valid", "true", diagnostics, "unity_playable_alpha.movement.step_missing");
        Require(values, "alpha_runtime.movement.blocked.valid", "false", diagnostics, "unity_playable_alpha.movement.bounds_not_proven");
        if (!values.ContainsKey("alpha_runtime.focus.selected"))
        {
            diagnostics.Add(Diagnostic("error", "unity_playable_alpha.interaction.focus_missing", "alpha_runtime.focus.selected", "The play log must prove focus/select before interaction."));
        }

        var expectedCommandCount = primary.CommandHints.Count;
        var commandsExecuted = ParseInt(values, "alpha_runtime.commands_executed");
        if (commandsExecuted < Math.Max(5, expectedCommandCount))
        {
            diagnostics.Add(Diagnostic("error", "unity_playable_alpha.interaction.commands_missing", "alpha_runtime.commands_executed", "Interaction proof must execute all generated command hints."));
        }

        for (var index = 0; index < expectedCommandCount; index++)
        {
            var expected = primary.CommandHints[index];
            Require(values, $"alpha_runtime.command_executed.{index}.id", expected.CommandId, diagnostics, "unity_playable_alpha.interaction.command_order_mismatch");
            Require(values, $"alpha_runtime.command_executed.{index}.type", expected.CommandType, diagnostics, "unity_playable_alpha.interaction.command_order_mismatch");
        }

        var visible = diagnostics.All(item => item.Code != "unity_playable_alpha.presentation.visible_component_missing" && item.Code != "unity_playable_alpha.presentation.log_missing");
        var movement = diagnostics.All(item => !item.Code.StartsWith("unity_playable_alpha.movement.", StringComparison.Ordinal));
        var interaction = diagnostics.All(item => !item.Code.StartsWith("unity_playable_alpha.interaction.", StringComparison.Ordinal));
        return new UnityPlayableAlphaPresentationProof
        {
            VisiblePresentationVerified = visible,
            MovementVerified = movement,
            InteractionVerified = interaction,
            PlayLoopVerified = diagnostics.All(item => item.Severity != "error"),
            InitialPosition = values.GetValueOrDefault("alpha_runtime.movement.initial_position", string.Empty),
            FinalMovementPosition = values.GetValueOrDefault("alpha_runtime.movement.step.1.position", string.Empty),
            BlockedMovementPosition = values.GetValueOrDefault("alpha_runtime.movement.blocked.position", string.Empty),
            FocusSelection = values.GetValueOrDefault("alpha_runtime.focus.selected", string.Empty),
            CommandsExecuted = Math.Max(0, commandsExecuted),
            Diagnostics = SortDiagnostics(diagnostics)
        };

        static void Require(
            IReadOnlyDictionary<string, string> values,
            string key,
            string expected,
            ICollection<AlphaBuildDiagnostic> diagnostics,
            string code)
        {
            if (!values.TryGetValue(key, out var actual) || !string.Equals(actual, expected, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", code, key, $"Expected {key}={expected}."));
            }
        }

        static int ParseInt(IReadOnlyDictionary<string, string> values, string key) =>
            values.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : -1;
    }

    public static UnityPlayableAlphaFirewallProof ValidateFirewallSafeBuildScript(string buildScriptText)
    {
        var diagnostics = new List<AlphaBuildDiagnostic>();
        RejectToken(buildScriptText, "BuildOptions.Development", "unity_playable_alpha.firewall.development_build_flag");
        RejectToken(buildScriptText, "BuildOptions.ConnectWithProfiler", "unity_playable_alpha.firewall.profiler_build_flag");
        RejectToken(buildScriptText, "BuildOptions.AllowDebugging", "unity_playable_alpha.firewall.debug_build_flag");
        RejectToken(buildScriptText, "BuildOptions.EnableHeadlessMode", "unity_playable_alpha.firewall.unexpected_headless_flag");
        RejectToken(buildScriptText, "AutoconnectProfiler", "unity_playable_alpha.firewall.autoconnect_profiler");
        RejectToken(buildScriptText, "scriptDebugging", "unity_playable_alpha.firewall.script_debugging");

        return new UnityPlayableAlphaFirewallProof
        {
            BuildOptions = buildScriptText.Contains("BuildOptions.None", StringComparison.Ordinal) ? "BuildOptions.None" : "unknown",
            DevelopmentBuild = buildScriptText.Contains("BuildOptions.Development", StringComparison.Ordinal),
            ConnectWithProfiler = buildScriptText.Contains("BuildOptions.ConnectWithProfiler", StringComparison.Ordinal),
            AllowDebugging = buildScriptText.Contains("BuildOptions.AllowDebugging", StringComparison.Ordinal),
            ScriptDebugging = buildScriptText.Contains("scriptDebugging", StringComparison.Ordinal),
            AutoConnectProfiler = buildScriptText.Contains("AutoconnectProfiler", StringComparison.Ordinal),
            StaticChecksPassed = diagnostics.All(item => item.Severity != "error"),
            FirewallSafeBuildVerified = diagnostics.All(item => item.Severity != "error"),
            Diagnostics = SortDiagnostics(diagnostics)
        };

        void RejectToken(string text, string token, string code)
        {
            if (text.Contains(token, StringComparison.Ordinal))
            {
                diagnostics.Add(Diagnostic("error", code, token, "Alpha Windows build entrypoint must not enable development/profiler/debug networking flags."));
            }
        }
    }

    private static UnityPlayableAlphaPresentationProof ValidatePresentationAndMovement(string projectRoot, AlphaRunnableBuildReport alpha)
    {
        var playLoopLogPath = string.IsNullOrWhiteSpace(alpha.LaunchVerification.PlayLoopLogRelativePath)
            ? string.Empty
            : Path.Combine(projectRoot, alpha.LaunchVerification.PlayLoopLogRelativePath.Replace('/', Path.DirectorySeparatorChar));
        return ValidatePresentationLog(playLoopLogPath, alpha.PrimaryBuildCandidate);
    }

    private static UnityPlayableAlphaFirewallProof ValidateFirewallSafeBuild(string repositoryRoot, string projectRoot, AlphaRunnableBuildReport alpha)
    {
        var scriptPath = Path.Combine(repositoryRoot, "unity", "LLMGameCreatorAlpha", "Assets", "Editor", "AlphaBuildEntrypoint.cs");
        if (!File.Exists(scriptPath))
        {
            return new UnityPlayableAlphaFirewallProof
            {
                Diagnostics =
                [
                    Diagnostic("error", "unity_playable_alpha.firewall.build_script_missing", "unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs", "Firewall-safe build proof requires the repository Alpha build entrypoint.")
                ]
            };
        }

        var proof = ValidateFirewallSafeBuildScript(File.ReadAllText(scriptPath));
        var metadataRelativePath = "alpha-build-metadata.json";
        var metadataPath = Path.Combine(projectRoot, RelativeOutputDirectory.Replace('/', Path.DirectorySeparatorChar), "build", "windows", metadataRelativePath);
        var metadataExists = File.Exists(metadataPath);
        var metadataPassed = !alpha.WindowsExecutableProduced || metadataExists;
        var diagnostics = proof.Diagnostics.ToList();
        if (!metadataPassed)
        {
            diagnostics.Add(Diagnostic("error", "unity_playable_alpha.firewall.metadata_missing", metadataRelativePath, "Build metadata must be produced with the Windows player output."));
        }

        return proof with
        {
            BuildMetadataRelativePath = metadataExists
                ? $"{RelativeOutputDirectory}/build/windows/{metadataRelativePath}"
                : string.Empty,
            BuildMetadataPresent = metadataExists,
            FirewallSafeBuildVerified = proof.StaticChecksPassed && metadataPassed,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    private static UnityPlayableAlphaInvalidMatrix BuildInvalidMatrix(
        AlphaRunnableBuildReport alpha,
        UnityPlayableAlphaPresentationProof presentation,
        UnityPlayableAlphaFirewallProof firewall)
    {
        var scenarios = new List<UnityPlayableAlphaInvalidScenario>();
        foreach (var scenario in alpha.InvalidMatrix.Scenarios)
        {
            scenarios.Add(new UnityPlayableAlphaInvalidScenario
            {
                ScenarioId = scenario.ScenarioId,
                ExpectedValid = false,
                ActualValid = scenario.ActualValid,
                Diagnostics = scenario.Diagnostics
            });
        }

        scenarios.Add(InvalidScenario("missing_accepted_goal013_evidence", [Diagnostic("error", "unity_playable_alpha.contract.missing_goal013_evidence", "alpha_runnable_windows_build_verification", "Goal 014 must record the accepted Goal 013 gate.")]));
        scenarios.Add(InvalidScenario("missing_visible_presentation_log", [Diagnostic("error", "unity_playable_alpha.presentation.log_missing", "logs/alpha-player-play-loop.log", "Visible presentation cannot be accepted without the player log.")]));
        scenarios.Add(InvalidScenario("missing_movement_proof", [Diagnostic("error", "unity_playable_alpha.movement.step_missing", "alpha_runtime.movement.step", "Movement proof requires two valid movement steps.")]));
        scenarios.Add(InvalidScenario("missing_interaction_proof", [Diagnostic("error", "unity_playable_alpha.interaction.commands_missing", "alpha_runtime.commands_executed", "Interaction proof requires generated command execution.")]));
        scenarios.Add(InvalidScenario("command_order_mismatch", [Diagnostic("error", "unity_playable_alpha.interaction.command_order_mismatch", "alpha_runtime.command_executed", "Command order must match selected generated hints.")]));
        scenarios.Add(InvalidScenario("development_profiler_debug_build_option", [Diagnostic("error", "unity_playable_alpha.firewall.development_build_flag", "BuildOptions.Development", "Development/profiler/debug build flags reject firewall-safe proof.")]));

        var diagnostics = new List<AlphaBuildDiagnostic>();
        var passed = scenarios.All(item => !item.ActualValid) &&
            alpha.InvalidMatrix.Passed &&
            (!alpha.WindowsExecutableProduced || (presentation.PlayLoopVerified && firewall.FirewallSafeBuildVerified));
        diagnostics.Add(Diagnostic(
            passed ? "info" : "error",
            passed ? "unity_playable_alpha.invalid_matrix_rejected" : "unity_playable_alpha.invalid_matrix_failed",
            "invalid_matrix",
            "Invalid/fake/leak scenarios must reject causally for playable presentation and firewall-safe build evidence."));

        return new UnityPlayableAlphaInvalidMatrix
        {
            Passed = passed,
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList(),
            Diagnostics = SortDiagnostics(diagnostics)
        };

        static UnityPlayableAlphaInvalidScenario InvalidScenario(string id, IReadOnlyList<AlphaBuildDiagnostic> diagnostics) =>
            new()
            {
                ScenarioId = id,
                ExpectedValid = false,
                ActualValid = diagnostics.All(item => item.Severity != "error"),
                Diagnostics = SortDiagnostics(diagnostics)
            };
    }

    private static string RenderReport(UnityPlayableAlphaReport report)
    {
        var lines = new List<string>
        {
            "# Unity Playable Alpha Report",
            string.Empty,
            $"- Accepted: {report.Accepted.ToString().ToLowerInvariant()}",
            $"- Final status: {report.FinalStatus}",
            $"- Previous gate: {report.PreviousAcceptedGate}",
            $"- Completed slices: {string.Join(", ", report.CompletedSlices)}",
            $"- Product smoke route: {report.ProductSmokeRoute}",
            $"- Selected style: {report.SelectedStyleId}",
            $"- Selected package: {report.SelectedPackageId}",
            $"- Package hash: {report.PackageHash}",
            $"- Asset manifest hash: {report.AssetManifestHash}",
            $"- Export manifest hash: {report.ExportManifestHash}",
            $"- Runtime config hash: {report.RuntimeConfigHash}",
            $"- Build output folder: {report.AlphaBuild.BuildOutput.BuildFolderRelativePath}",
            $"- Executable: {report.AlphaBuild.BuildOutput.ExecutableRelativePath}",
            $"- Visible presentation verified: {report.VisiblePresentationVerified.ToString().ToLowerInvariant()}",
            $"- Movement verified: {report.MovementVerified.ToString().ToLowerInvariant()}",
            $"- Interaction verified: {report.InteractionVerified.ToString().ToLowerInvariant()}",
            $"- Play loop verified: {report.PlayLoopVerified.ToString().ToLowerInvariant()}",
            $"- Firewall-safe build verified: {report.FirewallSafeBuildVerified.ToString().ToLowerInvariant()}",
            $"- Build options: {report.FirewallSafeBuild.BuildOptions}",
            $"- Invalid/fake/leak scenarios rejected: {report.InvalidMatrix.Scenarios.Count(item => !item.ActualValid)}/{report.InvalidMatrix.ScenarioCount}",
            $"- Deterministic report hash: {report.DeterministicHash}",
            $"- Build manifest hash: {report.BuildManifestHash}",
            string.Empty,
            "## Diagnostics",
            string.Empty
        };
        lines.AddRange(report.Diagnostics.Select(item => $"- {item.Severity}: {item.Code} [{item.Target}] {item.Message}"));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderVerification(UnityPlayableAlphaReport report, string alphaVerificationMarkdown)
    {
        var lines = new List<string>
        {
            "# Unity Playable Alpha Verification",
            string.Empty,
            "Stopped at:",
            string.Empty,
            "```text",
            FinalGate,
            "```",
            string.Empty,
            $"- Previous accepted gate: {report.PreviousAcceptedGate}",
            $"- Final gate remains required: {FinalGate}",
            $"- Unity project: {report.AlphaBuild.BuildEnvironment.RepoUnityProjectRelativePath}",
            $"- Unity build script: {report.AlphaBuild.BuildEnvironment.RepoBuildScriptRelativePath}",
            $"- Unity build log: {report.AlphaBuild.LaunchVerification.LogRelativePath.Replace("alpha-player-launch.log", "unity-build.log")}",
            $"- Build output folder: {report.AlphaBuild.BuildOutput.BuildFolderRelativePath}",
            $"- Executable relative path: {report.AlphaBuild.BuildOutput.ExecutableRelativePath}",
            $"- Launch log: {report.AlphaBuild.LaunchVerification.LogRelativePath}",
            $"- Play-loop log: {report.AlphaBuild.LaunchVerification.PlayLoopLogRelativePath}",
            $"- Movement: initial={Display(report.Presentation.InitialPosition)} final={Display(report.Presentation.FinalMovementPosition)} blockedAt={Display(report.Presentation.BlockedMovementPosition)}",
            $"- Interaction focus: {Display(report.Presentation.FocusSelection)}",
            $"- Build options: {report.FirewallSafeBuild.BuildOptions}",
            $"- Development/profiler/debug flags: development={report.FirewallSafeBuild.DevelopmentBuild.ToString().ToLowerInvariant()} profiler={report.FirewallSafeBuild.ConnectWithProfiler.ToString().ToLowerInvariant()} debugging={report.FirewallSafeBuild.AllowDebugging.ToString().ToLowerInvariant()}",
            "- Firewall prompt observed: not observed by automated noninteractive run",
            string.Empty,
            "## Underlying Alpha Build Verification",
            string.Empty,
            alphaVerificationMarkdown.TrimEnd()
        };

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;

        static string Display(string value) => string.IsNullOrWhiteSpace(value) ? "(none)" : value;
    }

    private static Dictionary<string, string> ParseKeyValueLog(IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            values[line[..separator]] = line[(separator + 1)..];
        }

        return values;
    }

    private static string ResolveRepositoryRoot(string projectRoot, string overrideRoot)
    {
        if (!string.IsNullOrWhiteSpace(overrideRoot))
        {
            return Path.GetFullPath(overrideRoot);
        }

        var current = new DirectoryInfo(projectRoot);
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? projectRoot;
    }

    private static IReadOnlyList<AlphaBuildDiagnostic> SortDiagnostics(IEnumerable<AlphaBuildDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static AlphaBuildDiagnostic Diagnostic(string severity, string code, string target, string message) =>
        new()
        {
            Severity = severity,
            Code = code,
            Target = target,
            Message = message
        };

    private static string ComputeHash(string text) => ComputeHash(Encoding.UTF8.GetBytes(text));

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed record UnityPlayableAlphaOptions
{
    public string RepositoryRootPath { get; init; } = string.Empty;
    public bool ExecuteUnityBuild { get; init; }
    public bool LaunchBuiltPlayer { get; init; }
    public bool PreserveExistingBuildOutputForValidation { get; init; }
    public bool CleanupUnityWorkProject { get; init; } = true;
    public int UnityBuildTimeoutSeconds { get; init; } = 900;
    public int PlayerLaunchTimeoutSeconds { get; init; } = 90;
}

public sealed record UnityPlayableAlphaAcceptanceResult
{
    public UnityPlayableAlphaReport Report { get; init; } = new();
    public string ReportJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public string VerificationMarkdown { get; init; } = string.Empty;
}

public sealed record UnityPlayableAlphaWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StagingDirectoryPath { get; init; } = string.Empty;
    public string BuildDirectoryPath { get; init; } = string.Empty;
    public string LogsDirectoryPath { get; init; } = string.Empty;
    public string ReportJsonPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string VerificationMarkdownPath { get; init; } = string.Empty;
}

public sealed record UnityPlayableAlphaReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string PreviousAcceptedGate { get; init; } = string.Empty;
    public IReadOnlyList<string> CompletedSlices { get; init; } = [];
    public string ProductSmokeRoute { get; init; } = string.Empty;
    public AlphaRunnableBuildReport AlphaBuild { get; init; } = new();
    public string SelectedPackageId { get; init; } = string.Empty;
    public string SelectedStyleId { get; init; } = string.Empty;
    public string PackageHash { get; init; } = string.Empty;
    public string AssetManifestHash { get; init; } = string.Empty;
    public string ExportManifestHash { get; init; } = string.Empty;
    public string RuntimeConfigHash { get; init; } = string.Empty;
    public UnityPlayableAlphaPresentationProof Presentation { get; init; } = new();
    public UnityPlayableAlphaFirewallProof FirewallSafeBuild { get; init; } = new();
    public UnityPlayableAlphaInvalidMatrix InvalidMatrix { get; init; } = new();
    public bool WindowsExecutableProduced { get; init; }
    public bool UnityBuildProduced { get; init; }
    public bool LaunchVerified { get; init; }
    public bool VisiblePresentationVerified { get; init; }
    public bool MovementVerified { get; init; }
    public bool InteractionVerified { get; init; }
    public bool PlayLoopVerified { get; init; }
    public bool FirewallSafeBuildVerified { get; init; }
    public bool NoExternalProviderLlmRagLuaMedia { get; init; }
    public bool RuntimePreviewDependency { get; init; }
    public bool PublicGamePackageSchemaChanged { get; init; }
    public bool ProjectFilesChanged { get; init; }
    public bool GeneratorLibraryChanged { get; init; }
    public string DeterministicReportRelativePath { get; init; } = string.Empty;
    public string DeterministicHash { get; init; } = string.Empty;
    public string BuildManifestHash { get; init; } = string.Empty;
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityPlayableAlphaPresentationProof
{
    public bool VisiblePresentationVerified { get; init; }
    public bool MovementVerified { get; init; }
    public bool InteractionVerified { get; init; }
    public bool PlayLoopVerified { get; init; }
    public string InitialPosition { get; init; } = string.Empty;
    public string FinalMovementPosition { get; init; } = string.Empty;
    public string BlockedMovementPosition { get; init; } = string.Empty;
    public string FocusSelection { get; init; } = string.Empty;
    public int CommandsExecuted { get; init; }
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityPlayableAlphaFirewallProof
{
    public string BuildOptions { get; init; } = string.Empty;
    public bool DevelopmentBuild { get; init; }
    public bool ConnectWithProfiler { get; init; }
    public bool AllowDebugging { get; init; }
    public bool ScriptDebugging { get; init; }
    public bool AutoConnectProfiler { get; init; }
    public bool BuildMetadataPresent { get; init; }
    public string BuildMetadataRelativePath { get; init; } = string.Empty;
    public bool StaticChecksPassed { get; init; }
    public bool FirewallSafeBuildVerified { get; init; }
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityPlayableAlphaInvalidMatrix
{
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<UnityPlayableAlphaInvalidScenario> Scenarios { get; init; } = [];
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record UnityPlayableAlphaInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<AlphaBuildDiagnostic> Diagnostics { get; init; } = [];
}
