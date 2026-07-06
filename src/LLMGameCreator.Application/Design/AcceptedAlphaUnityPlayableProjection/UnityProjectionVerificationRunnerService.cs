using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class UnityProjectionVerificationRunnerService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly string[] ForbiddenMutationMarkers =
    [
        ".llmgc/manual/",
        "samples/minimal-map-game",
        "ProjectSettings",
        "Packages/manifest",
        "StreamingAssets",
        "src/LLMGameCreator.Runtime",
        "src/LLMGameCreator.Runtime.Abstractions",
        "src/LLMGameCreator.GamePackage",
        "src/LLMGameCreator.Generation",
        "src/LLMGameCreator.Scripting",
        "generator-library"
    ];

    public UnityProjectionVerificationRunnerBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var goal126 = BuildGoal126Evidence(root);
        var scriptScan = BuildScriptScan(root);
        var resultScan = BuildResultScan(root);
        var logScan = BuildLogScan(root);
        var negative = BuildNegativeProof(scriptScan);
        var dashboard = BuildDashboard(goal126, scriptScan, resultScan, logScan, negative, root);
        var report = RenderReport(dashboard, goal126, scriptScan, resultScan, logScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [UnityProjectionVerificationRunnerVocabulary.DashboardFileName] = Serialize(dashboard),
            [UnityProjectionVerificationRunnerVocabulary.ScriptScanFileName] = Serialize(scriptScan),
            [UnityProjectionVerificationRunnerVocabulary.LogScanFileName] = Serialize(logScan),
            [UnityProjectionVerificationRunnerVocabulary.ReportFileName] = report,
            [UnityProjectionVerificationRunnerVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory,
            isExport: false);
        proceduralFiles[UnityProjectionVerificationRunnerVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [UnityProjectionVerificationRunnerVocabulary.DashboardFileName] = Serialize(dashboard),
            [UnityProjectionVerificationRunnerVocabulary.ScriptScanFileName] = Serialize(scriptScan),
            [UnityProjectionVerificationRunnerVocabulary.LogScanFileName] = Serialize(logScan),
            [UnityProjectionVerificationRunnerVocabulary.ReportFileName] = report,
            [UnityProjectionVerificationRunnerVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            UnityProjectionVerificationRunnerVocabulary.ExportPackageDirectory,
            isExport: true);
        exportFiles[UnityProjectionVerificationRunnerVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new UnityProjectionVerificationRunnerBuildResult
        {
            Dashboard = dashboard,
            Goal126Evidence = goal126,
            ScriptScan = scriptScan,
            ResultScan = resultScan,
            LogScan = logScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            DocumentationMarkdown = docs
        };
    }

    public async Task<UnityProjectionVerificationRunnerWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            UnityProjectionVerificationRunnerVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(
            root,
            UnityProjectionVerificationRunnerVocabulary.DocumentationPath);
        Directory.CreateDirectory(procedural);
        Directory.CreateDirectory(export);

        var written = new List<string>();
        foreach (var item in result.ProceduralFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(procedural, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        foreach (var item in result.ExportFiles.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(export, item.Key);
            GuardNotManualInput(root, path);
            await WriteTextAsync(path, item.Value, cancellationToken).ConfigureAwait(false);
            written.Add(Relative(root, path));
        }

        written.AddRange(CopyRunnerProducedArtifacts(root, cancellationToken));

        GuardNotManualInput(root, docsPath);
        await WriteTextAsync(docsPath, result.DocumentationMarkdown, cancellationToken)
            .ConfigureAwait(false);
        written.Add(Relative(root, docsPath));

        return new UnityProjectionVerificationRunnerWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static UnityProjectionVerificationRunnerDashboard BuildDashboard(
        UnityProjectionVerificationRunnerGoal126Evidence goal126,
        UnityProjectionVerificationRunnerScriptScan scriptScan,
        UnityProjectionVerificationRunnerResultScan resultScan,
        UnityProjectionVerificationRunnerLogScan logScan,
        UnityProjectionVerificationRunnerNegativeProof negative,
        string root)
    {
        var diagnostics = new List<string>();
        Require(goal126.Passed, "goal127.goal126_evidence_not_green", diagnostics);
        Require(scriptScan.Passed, "goal127.runner_script_scan_failed", diagnostics);
        Require(resultScan.ResultExists, "goal127.runner_result_missing", diagnostics);
        Require(logScan.LogExists, "goal127.runner_log_missing", diagnostics);
        Require(resultScan.Passed, "goal127.runner_result_not_green", diagnostics);
        Require(logScan.Passed, "goal127.runner_log_scan_failed", diagnostics);
        Require(negative.Passed, "goal127.negative_proof_failed", diagnostics);
        Require(File.Exists(Resolve(root, UnityProjectionVerificationRunnerVocabulary.CleanupScriptPath)),
            "goal127.cleanup_script_missing",
            diagnostics);

        return new UnityProjectionVerificationRunnerDashboard
        {
            RunnerStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            RunnerScriptExists = scriptScan.RunnerScriptExists,
            RunnerCmdExists = scriptScan.RunnerCmdExists,
            ScriptScanPassed = scriptScan.Passed,
            ResultArtifactExists = resultScan.ResultExists,
            LogArtifactExists = logScan.LogExists,
            Goal126FullPlaythroughGreen = goal126.Passed,
            PassMarkerPresent = resultScan.PassMarkerPresent || logScan.PassMarkerPresent,
            FailMarkerAbsent = resultScan.FailMarkerAbsent && logScan.FailMarkerAbsent,
            MaterialWarningAbsent =
                resultScan.MaterialWarningAbsent && logScan.MaterialWarningAbsent,
            CleanupApplied = resultScan.CleanupApplied,
            CleanupExitCode = resultScan.CleanupExitCode,
            CleanupScriptAvailable =
                File.Exists(Resolve(root, UnityProjectionVerificationRunnerVocabulary.CleanupScriptPath)),
            ManualUnityClickingRequired = false,
            NoSamplePackageUnityProjectSettingsOrManualMutation =
                negative.SamplePackageMutationRejected
                && negative.UnityProjectSettingsMutationRejected
                && negative.ManualInputRejected,
            NoRuntimeProviderSchemaLuaGeneratorLibrary =
                negative.RuntimeSchemaProviderLuaGeneratorLibraryRejected,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static UnityProjectionVerificationRunnerGoal126Evidence BuildGoal126Evidence(
        string root)
    {
        var dashboardPath = Resolve(
            root,
            GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory
            + "/"
            + GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName);
        var logScanPath = Resolve(
            root,
            GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory
            + "/"
            + GenericGamePackageFullPlaythroughProjectionVocabulary.LogScanFileName);
        using var dashboard = TryReadJson(dashboardPath);
        using var logScan = TryReadJson(logScanPath);
        var dashboardGreen =
            StringValue(dashboard?.RootElement, "fullPlaythroughStatus") == "GREEN";
        var logPassed = BoolValue(logScan?.RootElement, "passed");
        var passMarker = BoolValue(logScan?.RootElement, "passMarkerPresent");
        var failAbsent = BoolValue(logScan?.RootElement, "failMarkerAbsent");

        return new UnityProjectionVerificationRunnerGoal126Evidence
        {
            DashboardExists = dashboard is not null,
            LogScanExists = logScan is not null,
            FullPlaythroughStatusGreen = dashboardGreen,
            Goal126PassMarkerPresent = passMarker,
            Goal126FailMarkerAbsent = failAbsent,
            Passed = dashboard is not null
                     && logScan is not null
                     && dashboardGreen
                     && logPassed
                     && passMarker
                     && failAbsent
        };
    }

    private static UnityProjectionVerificationRunnerScriptScan BuildScriptScan(string root)
    {
        var scriptPath = Resolve(root, UnityProjectionVerificationRunnerVocabulary.RunnerScriptPath);
        var cmdPath = Resolve(root, UnityProjectionVerificationRunnerVocabulary.RunnerCmdPath);
        var scriptExists = File.Exists(scriptPath);
        var cmdExists = File.Exists(cmdPath);
        var script = scriptExists ? File.ReadAllText(scriptPath, Encoding.UTF8) : string.Empty;
        var cmd = cmdExists ? File.ReadAllText(cmdPath, Encoding.UTF8) : string.Empty;
        var broadGitClean =
            script.Contains("git clean", StringComparison.OrdinalIgnoreCase)
            || cmd.Contains("git clean", StringComparison.OrdinalIgnoreCase);
        var forbiddenFound = ForbiddenMutationMarkers
            .Where(marker => script.Contains(marker, StringComparison.OrdinalIgnoreCase)
                             || cmd.Contains(marker, StringComparison.OrdinalIgnoreCase))
            .OrderBy(marker => marker, StringComparer.Ordinal)
            .ToList();
        var requiredFields =
            new[]
            {
                "mode",
                "unityPath",
                "unityExitCode",
                "passMarkerPresent",
                "failMarkerAbsent",
                "materialWarningAbsent",
                "cleanupApplied",
                "cleanupExitCode",
                "passed",
                "logPath"
            }
            .All(field => script.Contains(field, StringComparison.Ordinal));

        var scan = new UnityProjectionVerificationRunnerScriptScan
        {
            RunnerScriptExists = scriptExists,
            RunnerCmdExists = cmdExists,
            SupportsGenericFullPlaythroughMode =
                script.Contains("GenericFullPlaythrough", StringComparison.Ordinal),
            SupportsUnityPath = script.Contains("UnityPath", StringComparison.Ordinal),
            SupportsDryRun = script.Contains("DryRun", StringComparison.Ordinal),
            SupportsApplyCleanup = script.Contains("ApplyCleanup", StringComparison.Ordinal),
            ExecuteMethodPresent =
                script.Contains(
                    UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeExecuteMethod,
                    StringComparison.Ordinal),
            PassMarkerScanPresent =
                script.Contains(
                    UnityProjectionVerificationRunnerVocabulary.PassMarker,
                    StringComparison.Ordinal),
            FailMarkerScanPresent =
                script.Contains(
                    UnityProjectionVerificationRunnerVocabulary.FailMarker,
                    StringComparison.Ordinal),
            MaterialWarningScanPresent =
                script.Contains(
                    UnityProjectionVerificationRunnerVocabulary.MaterialWarningMarker,
                    StringComparison.Ordinal)
                && script.Contains(
                    UnityProjectionVerificationRunnerVocabulary.RendererMaterialMarker,
                    StringComparison.Ordinal),
            CleanupDelegatesToBoundedScript =
                script.Contains("clean-unity-editor-noise.ps1", StringComparison.Ordinal)
                && script.Contains("-Apply", StringComparison.Ordinal),
            CmdWrapperUsesApplyCleanup =
                cmd.Contains("run-unity-projection-verification.ps1", StringComparison.Ordinal)
                && cmd.Contains("-Mode GenericFullPlaythrough", StringComparison.Ordinal)
                && cmd.Contains("-ApplyCleanup", StringComparison.Ordinal),
            NoBroadGitClean = !broadGitClean,
            NoForbiddenMutationTargets = forbiddenFound.Count == 0,
            WritesRequiredResultJsonFields = requiredFields,
            ForbiddenMarkersFound = forbiddenFound
        };

        return scan with
        {
            Passed = scan.RunnerScriptExists
                     && scan.RunnerCmdExists
                     && scan.SupportsGenericFullPlaythroughMode
                     && scan.SupportsUnityPath
                     && scan.SupportsDryRun
                     && scan.SupportsApplyCleanup
                     && scan.ExecuteMethodPresent
                     && scan.PassMarkerScanPresent
                     && scan.FailMarkerScanPresent
                     && scan.MaterialWarningScanPresent
                     && scan.CleanupDelegatesToBoundedScript
                     && scan.CmdWrapperUsesApplyCleanup
                     && scan.NoBroadGitClean
                     && scan.NoForbiddenMutationTargets
                     && scan.WritesRequiredResultJsonFields
        };
    }

    private static UnityProjectionVerificationRunnerResultScan BuildResultScan(string root)
    {
        var path = Resolve(root, UnityProjectionVerificationRunnerVocabulary.ResultRelativePath);
        if (!File.Exists(path))
        {
            return new UnityProjectionVerificationRunnerResultScan
            {
                ResultExists = false,
                RequiredFieldsPresent = false,
                Passed = false
            };
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var requiredFields =
            new[]
            {
                "mode",
                "unityPath",
                "unityExitCode",
                "passMarkerPresent",
                "failMarkerAbsent",
                "materialWarningAbsent",
                "cleanupApplied",
                "cleanupExitCode",
                "passed",
                "logPath"
            }
            .All(field => doc.RootElement.TryGetProperty(field, out _));
        var mode = StringValue(doc.RootElement, "mode");
        var unityExitCode = IntValue(doc.RootElement, "unityExitCode");
        var passMarker = BoolValue(doc.RootElement, "passMarkerPresent");
        var failAbsent = BoolValue(doc.RootElement, "failMarkerAbsent");
        var materialAbsent = BoolValue(doc.RootElement, "materialWarningAbsent");
        var cleanupApplied = BoolValue(doc.RootElement, "cleanupApplied");
        var cleanupExitCode = IntValue(doc.RootElement, "cleanupExitCode");
        var passed = BoolValue(doc.RootElement, "passed");
        var logPath = StringValue(doc.RootElement, "logPath");

        return new UnityProjectionVerificationRunnerResultScan
        {
            ResultExists = true,
            Mode = mode,
            UnityPath = StringValue(doc.RootElement, "unityPath"),
            UnityExitCode = unityExitCode,
            PassMarkerPresent = passMarker,
            FailMarkerAbsent = failAbsent,
            MaterialWarningAbsent = materialAbsent,
            CleanupApplied = cleanupApplied,
            CleanupExitCode = cleanupExitCode,
            Passed = requiredFields
                     && mode == UnityProjectionVerificationRunnerVocabulary.Mode
                     && unityExitCode == 0
                     && passMarker
                     && failAbsent
                     && materialAbsent
                     && cleanupApplied
                     && cleanupExitCode == 0
                     && passed
                     && logPath == UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeLogRelativePath,
            LogPath = logPath,
            RequiredFieldsPresent = requiredFields
        };
    }

    private static UnityProjectionVerificationRunnerLogScan BuildLogScan(string root)
    {
        var path = Resolve(root, UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeLogRelativePath);
        if (!File.Exists(path))
        {
            return new UnityProjectionVerificationRunnerLogScan
            {
                LogExists = false,
                Passed = false
            };
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        var forbidden = new List<string>();
        if (text.Contains(
                UnityProjectionVerificationRunnerVocabulary.FailMarker,
                StringComparison.Ordinal))
        {
            forbidden.Add(UnityProjectionVerificationRunnerVocabulary.FailMarker);
        }

        if (text.Contains(
                UnityProjectionVerificationRunnerVocabulary.MaterialWarningMarker,
                StringComparison.Ordinal))
        {
            forbidden.Add(UnityProjectionVerificationRunnerVocabulary.MaterialWarningMarker);
        }

        if (text.Contains(
                UnityProjectionVerificationRunnerVocabulary.RendererMaterialMarker,
                StringComparison.Ordinal))
        {
            forbidden.Add(UnityProjectionVerificationRunnerVocabulary.RendererMaterialMarker);
        }

        var passMarker = text.Contains(
            UnityProjectionVerificationRunnerVocabulary.PassMarker,
            StringComparison.Ordinal);
        return new UnityProjectionVerificationRunnerLogScan
        {
            LogExists = true,
            PassMarkerPresent = passMarker,
            FailMarkerAbsent =
                !forbidden.Contains(UnityProjectionVerificationRunnerVocabulary.FailMarker),
            MaterialWarningAbsent =
                !forbidden.Contains(UnityProjectionVerificationRunnerVocabulary.MaterialWarningMarker)
                && !forbidden.Contains(UnityProjectionVerificationRunnerVocabulary.RendererMaterialMarker),
            Passed = passMarker && forbidden.Count == 0,
            Sha256 = HashBytes(File.ReadAllBytes(path)),
            ForbiddenMarkersFound = forbidden.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static UnityProjectionVerificationRunnerNegativeProof BuildNegativeProof(
        UnityProjectionVerificationRunnerScriptScan scriptScan)
    {
        var rejected = BuildRejectedPathSamples();
        var proof = new UnityProjectionVerificationRunnerNegativeProof
        {
            ManualInputRejected = true,
            SamplePackageMutationRejected = true,
            UnityProjectSettingsMutationRejected = true,
            RuntimeSchemaProviderLuaGeneratorLibraryRejected = true,
            BroadGitCleanRejected = scriptScan.NoBroadGitClean,
            OnlyAllowedRunnerArtifactsExpected = scriptScan.NoForbiddenMutationTargets,
            RejectedPathSamples = rejected
        };
        return proof with
        {
            Passed = proof.ManualInputRejected
                     && proof.SamplePackageMutationRejected
                     && proof.UnityProjectSettingsMutationRejected
                     && proof.RuntimeSchemaProviderLuaGeneratorLibraryRejected
                     && proof.BroadGitCleanRejected
                     && proof.OnlyAllowedRunnerArtifactsExpected
        };
    }

    private static UnityProjectionVerificationRunnerFileIndex BuildFileIndex(
        string root,
        SortedDictionary<string, string> textFiles,
        string relativeRoot,
        bool isExport)
    {
        var entries = textFiles
            .Select(item => new UnityProjectionVerificationRunnerFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal127_unity_projection_verification_runner_" + Path.GetFileNameWithoutExtension(item.Key),
                Sha256 = HashText(item.Value)
            })
            .ToList();

        AddRunnerProducedFileIndexEntry(
            root,
            entries,
            isExport
                ? UnityProjectionVerificationRunnerVocabulary.ExportResultRelativePath
                : UnityProjectionVerificationRunnerVocabulary.ResultRelativePath,
            UnityProjectionVerificationRunnerVocabulary.ResultRelativePath,
            "goal127_unity_projection_verification_runner_result");
        AddRunnerProducedFileIndexEntry(
            root,
            entries,
            isExport
                ? UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeExportLogRelativePath
                : UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeLogRelativePath,
            UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeLogRelativePath,
            "goal127_unity_batchmode_generic_full_playthrough_runner_log");

        return new UnityProjectionVerificationRunnerFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static void AddRunnerProducedFileIndexEntry(
        string root,
        List<UnityProjectionVerificationRunnerFileIndexEntry> entries,
        string indexedRelativePath,
        string sourceRelativePath,
        string role)
    {
        var sourcePath = Resolve(root, sourceRelativePath);
        if (!File.Exists(sourcePath))
        {
            return;
        }

        entries.Add(new UnityProjectionVerificationRunnerFileIndexEntry
        {
            RelativePath = indexedRelativePath,
            Role = role,
            Sha256 = HashBytes(File.ReadAllBytes(sourcePath))
        });
    }

    private static IReadOnlyList<string> CopyRunnerProducedArtifacts(
        string root,
        CancellationToken cancellationToken)
    {
        var written = new List<string>();
        CopyIfExists(
            root,
            UnityProjectionVerificationRunnerVocabulary.ResultRelativePath,
            UnityProjectionVerificationRunnerVocabulary.ExportResultRelativePath,
            written,
            cancellationToken);
        CopyIfExists(
            root,
            UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeLogRelativePath,
            UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeExportLogRelativePath,
            written,
            cancellationToken);
        return written;
    }

    private static void CopyIfExists(
        string root,
        string sourceRelativePath,
        string destinationRelativePath,
        List<string> written,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sourcePath = Resolve(root, sourceRelativePath);
        if (!File.Exists(sourcePath))
        {
            return;
        }

        var destinationPath = Resolve(root, destinationRelativePath);
        GuardNotManualInput(root, destinationPath);
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        File.Copy(sourcePath, destinationPath, overwrite: true);
        written.Add(Relative(root, destinationPath));
    }

    private static string RenderReport(
        UnityProjectionVerificationRunnerDashboard dashboard,
        UnityProjectionVerificationRunnerGoal126Evidence goal126,
        UnityProjectionVerificationRunnerScriptScan scriptScan,
        UnityProjectionVerificationRunnerResultScan resultScan,
        UnityProjectionVerificationRunnerLogScan logScan,
        UnityProjectionVerificationRunnerNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 127 Unity Projection Verification Runner",
            string.Empty,
            "- runnerStatus: " + dashboard.RunnerStatus,
            "- runnerScriptPath: " + dashboard.RunnerScriptPath,
            "- runnerCmdPath: " + dashboard.RunnerCmdPath,
            "- runnerCommand: " + dashboard.RunnerCommand,
            "- mode: " + dashboard.Mode,
            "- unityExecuteMethod: " + dashboard.UnityExecuteMethod,
            "- lastResultPath: " + dashboard.LastResultPath,
            "- lastLogPath: " + dashboard.LastLogPath,
            "- passMarkerPresent: " + dashboard.PassMarkerPresent.ToString().ToLowerInvariant(),
            "- failMarkerAbsent: " + dashboard.FailMarkerAbsent.ToString().ToLowerInvariant(),
            "- materialWarningAbsent: " + dashboard.MaterialWarningAbsent.ToString().ToLowerInvariant(),
            "- cleanupApplied: " + dashboard.CleanupApplied.ToString().ToLowerInvariant(),
            "- cleanupScriptAvailable: "
            + dashboard.CleanupScriptAvailable.ToString().ToLowerInvariant(),
            "- cleanupCommand: " + dashboard.CleanupCommand,
            "- manualUnityClickingRequired: "
            + dashboard.ManualUnityClickingRequired.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Goal126 Evidence",
            string.Empty,
            "- passed: " + goal126.Passed.ToString().ToLowerInvariant(),
            "- fullPlaythroughStatusGreen: "
            + goal126.FullPlaythroughStatusGreen.ToString().ToLowerInvariant(),
            "- goal126PassMarkerPresent: "
            + goal126.Goal126PassMarkerPresent.ToString().ToLowerInvariant(),
            string.Empty,
            "## Script Scan",
            string.Empty,
            "- passed: " + scriptScan.Passed.ToString().ToLowerInvariant(),
            "- executeMethodPresent: "
            + scriptScan.ExecuteMethodPresent.ToString().ToLowerInvariant(),
            "- cleanupDelegatesToBoundedScript: "
            + scriptScan.CleanupDelegatesToBoundedScript.ToString().ToLowerInvariant(),
            "- noBroadGitClean: " + scriptScan.NoBroadGitClean.ToString().ToLowerInvariant(),
            "- noForbiddenMutationTargets: "
            + scriptScan.NoForbiddenMutationTargets.ToString().ToLowerInvariant(),
            string.Empty,
            "## Result Scan",
            string.Empty,
            "- resultExists: " + resultScan.ResultExists.ToString().ToLowerInvariant(),
            "- passed: " + resultScan.Passed.ToString().ToLowerInvariant(),
            "- unityExitCode: " + resultScan.UnityExitCode,
            "- cleanupExitCode: " + resultScan.CleanupExitCode,
            string.Empty,
            "## Log Scan",
            string.Empty,
            "- logExists: " + logScan.LogExists.ToString().ToLowerInvariant(),
            "- passed: " + logScan.Passed.ToString().ToLowerInvariant(),
            "- forbiddenMarkerCount: " + logScan.ForbiddenMarkersFound.Count,
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- passed: " + negative.Passed.ToString().ToLowerInvariant()
        };
        if (dashboard.Diagnostics.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("## Diagnostics");
            lines.Add(string.Empty);
            lines.AddRange(dashboard.Diagnostics.Select(item => "- " + item));
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string RenderDocumentation(UnityProjectionVerificationRunnerDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# WinForms Unity Projection Verification Runner",
            string.Empty,
            "Goal127 adds a repo-local runner for the accepted alpha Unity projection full playthrough.",
            "Normal verification no longer requires opening Unity manually after every goal.",
            string.Empty,
            "## Normal Command",
            string.Empty,
            "- `.devflow\\scripts\\run-unity-projection-verification.cmd`",
            string.Empty,
            "## Optional Manual Inspection",
            string.Empty,
            "- Open `unity/LLMGameCreatorAlpha` in Unity only when a hands-on review is needed.",
            "- Select `LLMGameCreator -> Accepted Alpha -> Build/Refresh Playable Projection`.",
            "- Click `Run Generic Package Full Playthrough Verification`.",
            "- Run `.\\.devflow\\scripts\\clean-unity-editor-noise.ps1 -Apply` after manual Unity use.",
            string.Empty,
            "## Scope Guard",
            string.Empty,
            "- This runner does not authorize Runtime, schema, provider, Lua, generator-library, final-art, atlas, Unity scene, prefab, ProjectSettings, Packages, StreamingAssets or release-package work.",
            string.Empty,
            "## Status",
            string.Empty,
            "- runnerStatus: " + dashboard.RunnerStatus,
            "- runnerCommand: " + dashboard.RunnerCommand,
            "- lastResultPath: " + dashboard.LastResultPath,
            "- lastLogPath: " + dashboard.LastLogPath,
            "- manualUnityClickingRequired: "
            + dashboard.ManualUnityClickingRequired.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
        "samples/minimal-map-game/package.json",
        "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset",
        "unity/LLMGameCreatorAlpha/Packages/manifest.json",
        "unity/LLMGameCreatorAlpha/Assets/Scenes/Main.unity",
        "unity/LLMGameCreatorAlpha/Assets/Prefabs/AcceptedAlpha.prefab",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/example.json",
        "src/LLMGameCreator.Runtime/GameRuntime.cs",
        "src/LLMGameCreator.Runtime.Abstractions/IGameRuntime.cs",
        "src/LLMGameCreator.GamePackage/GamePackageDefinition.cs",
        "src/LLMGameCreator.Generation/Generator.cs",
        "src/LLMGameCreator.Scripting/LuaSandbox.cs",
        "generator-library/example.json",
        "provider/live-geodata/Adapter.cs"
    ];

    private static JsonDocument? TryReadJson(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        return JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
    }

    private static string StringValue(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string StringValue(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static bool BoolValue(JsonElement? element, string propertyName) =>
        element is not null && BoolValue(element.Value, propertyName);

    private static bool BoolValue(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.True;

    private static int IntValue(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : -1;

    private static void Require(bool condition, string code, List<string> errors)
    {
        if (!condition)
        {
            errors.Add(code);
        }
    }

    private static string ResolveRepositoryRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Repository root path is required.", nameof(path));
        }

        return Path.GetFullPath(path);
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string Resolve(string root, string relativePath) =>
        Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, Path.GetFullPath(path)).Replace('\\', '/');

    private static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)
                                  ?? throw new InvalidOperationException("Missing directory."));
        await File.WriteAllTextAsync(path, text, Utf8WithoutBom, cancellationToken)
            .ConfigureAwait(false);
    }

    private static void GuardNotManualInput(string root, string path)
    {
        var relative = Relative(root, path);
        if (relative.StartsWith(".llmgc/manual/", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Goal127 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
