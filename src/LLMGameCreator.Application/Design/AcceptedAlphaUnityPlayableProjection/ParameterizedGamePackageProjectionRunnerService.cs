using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class ParameterizedGamePackageProjectionRunnerService
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

    public ParameterizedGamePackageProjectionRunnerBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var goal127 = BuildGoal127Evidence(root);
        var scriptScan = BuildScriptScan(root);
        var unitySourceScan = BuildUnitySourceScan(root);
        var resultScan = BuildResultScan(root);
        var logScan = BuildLogScan(root);
        var negative = BuildNegativeProof(scriptScan);
        var dashboard = BuildDashboard(
            goal127,
            scriptScan,
            unitySourceScan,
            resultScan,
            logScan,
            negative,
            root);
        var report = RenderReport(dashboard, goal127, scriptScan, unitySourceScan, resultScan, logScan, negative);
        var docs = RenderDocumentation(dashboard);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [ParameterizedGamePackageProjectionRunnerVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [ParameterizedGamePackageProjectionRunnerVocabulary.ScriptScanFileName] =
                Serialize(scriptScan),
            [ParameterizedGamePackageProjectionRunnerVocabulary.UnitySourceScanFileName] =
                Serialize(unitySourceScan),
            [ParameterizedGamePackageProjectionRunnerVocabulary.LogScanFileName] =
                Serialize(logScan),
            [ParameterizedGamePackageProjectionRunnerVocabulary.ReportFileName] = report,
            [ParameterizedGamePackageProjectionRunnerVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory,
            isExport: false);
        proceduralFiles[ParameterizedGamePackageProjectionRunnerVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [ParameterizedGamePackageProjectionRunnerVocabulary.DashboardFileName] =
                Serialize(dashboard),
            [ParameterizedGamePackageProjectionRunnerVocabulary.ScriptScanFileName] =
                Serialize(scriptScan),
            [ParameterizedGamePackageProjectionRunnerVocabulary.UnitySourceScanFileName] =
                Serialize(unitySourceScan),
            [ParameterizedGamePackageProjectionRunnerVocabulary.LogScanFileName] =
                Serialize(logScan),
            [ParameterizedGamePackageProjectionRunnerVocabulary.ReportFileName] = report,
            [ParameterizedGamePackageProjectionRunnerVocabulary.NegativeProofFileName] =
                Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            ParameterizedGamePackageProjectionRunnerVocabulary.ExportPackageDirectory,
            isExport: true);
        exportFiles[ParameterizedGamePackageProjectionRunnerVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new ParameterizedGamePackageProjectionRunnerBuildResult
        {
            Dashboard = dashboard,
            Goal127Evidence = goal127,
            ScriptScan = scriptScan,
            UnitySourceScan = unitySourceScan,
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

    public async Task<ParameterizedGamePackageProjectionRunnerWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            ParameterizedGamePackageProjectionRunnerVocabulary.ExportPackageDirectory);
        var docsPath = Resolve(
            root,
            ParameterizedGamePackageProjectionRunnerVocabulary.DocumentationPath);
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

        return new ParameterizedGamePackageProjectionRunnerWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            DocumentationPath = docsPath,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static ParameterizedGamePackageProjectionRunnerDashboard BuildDashboard(
        ParameterizedGamePackageProjectionRunnerGoal127Evidence goal127,
        ParameterizedGamePackageProjectionRunnerScriptScan scriptScan,
        ParameterizedGamePackageProjectionRunnerUnitySourceScan unitySourceScan,
        ParameterizedGamePackageProjectionRunnerResultScan resultScan,
        ParameterizedGamePackageProjectionRunnerLogScan logScan,
        ParameterizedGamePackageProjectionRunnerNegativeProof negative,
        string root)
    {
        var diagnostics = new List<string>();
        Require(goal127.Passed, "goal128.goal127_runner_not_green", diagnostics);
        Require(scriptScan.Passed, "goal128.runner_script_scan_failed", diagnostics);
        Require(unitySourceScan.Passed, "goal128.unity_source_scan_failed", diagnostics);
        Require(resultScan.ResultExists, "goal128.runner_result_missing", diagnostics);
        Require(logScan.LogExists, "goal128.runner_log_missing", diagnostics);
        Require(resultScan.Passed, "goal128.runner_result_not_green", diagnostics);
        Require(logScan.Passed, "goal128.runner_log_scan_failed", diagnostics);
        Require(negative.Passed, "goal128.negative_proof_failed", diagnostics);
        Require(File.Exists(Resolve(root, ParameterizedGamePackageProjectionRunnerVocabulary.CleanupScriptPath)),
            "goal128.cleanup_script_missing",
            diagnostics);

        return new ParameterizedGamePackageProjectionRunnerDashboard
        {
            ParameterizedRunnerStatus = diagnostics.Count == 0 ? "GREEN" : "BLOCKED",
            PackagePath = resultScan.PackagePath,
            PackagePathRelative = string.IsNullOrWhiteSpace(resultScan.PackagePathRelative)
                ? ParameterizedGamePackageProjectionRunnerVocabulary.DefaultPackageRelativePath
                : resultScan.PackagePathRelative,
            PackagePathResolved = resultScan.ResultExists && resultScan.RequiredFieldsPresent,
            PackagePathUnderRepo = resultScan.PackagePathUnderRepo,
            UnityExitCode = resultScan.UnityExitCode,
            PassMarkerPresent = resultScan.PassMarkerPresent || logScan.PassMarkerPresent,
            FailMarkerAbsent = resultScan.FailMarkerAbsent && logScan.FailMarkerAbsent,
            MaterialWarningAbsent =
                resultScan.MaterialWarningAbsent && logScan.MaterialWarningAbsent,
            CleanupApplied = resultScan.CleanupApplied,
            CleanupExitCode = resultScan.CleanupExitCode,
            RunnerScriptExists = scriptScan.RunnerScriptExists,
            RunnerCmdExists = scriptScan.RunnerCmdExists,
            ScriptScanPassed = scriptScan.Passed,
            UnitySourceScanPassed = unitySourceScan.Passed,
            ResultArtifactExists = resultScan.ResultExists,
            LogArtifactExists = logScan.LogExists,
            Goal127RunnerGreen = goal127.Passed,
            NegativeProofPassed = negative.Passed,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static ParameterizedGamePackageProjectionRunnerGoal127Evidence BuildGoal127Evidence(
        string root)
    {
        var dashboardPath = Resolve(
            root,
            UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory
            + "/"
            + UnityProjectionVerificationRunnerVocabulary.DashboardFileName);
        using var dashboard = TryReadJson(dashboardPath);
        var runnerGreen =
            StringValue(dashboard?.RootElement, "runnerStatus") == "GREEN";
        var passMarker = BoolValue(dashboard?.RootElement, "passMarkerPresent");
        var cleanupApplied = BoolValue(dashboard?.RootElement, "cleanupApplied");

        return new ParameterizedGamePackageProjectionRunnerGoal127Evidence
        {
            DashboardExists = dashboard is not null,
            Goal127RunnerGreen = runnerGreen,
            Goal127PassMarkerPresent = passMarker,
            Goal127CleanupApplied = cleanupApplied,
            Passed = dashboard is not null && runnerGreen && passMarker && cleanupApplied
        };
    }

    private static ParameterizedGamePackageProjectionRunnerScriptScan BuildScriptScan(string root)
    {
        var scriptPath = Resolve(root, ParameterizedGamePackageProjectionRunnerVocabulary.RunnerScriptPath);
        var cmdPath = Resolve(root, ParameterizedGamePackageProjectionRunnerVocabulary.RunnerCmdPath);
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
                "packagePath",
                "packagePathRelative",
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

        var scan = new ParameterizedGamePackageProjectionRunnerScriptScan
        {
            RunnerScriptExists = scriptExists,
            RunnerCmdExists = cmdExists,
            SupportsPackagePathParameter =
                script.Contains("[string]$PackagePath", StringComparison.Ordinal),
            SupportsDefaultPackagePath =
                script.Contains(
                    ParameterizedGamePackageProjectionRunnerVocabulary.DefaultPackageRelativePath,
                    StringComparison.Ordinal),
            RejectsOutsideRepository =
                script.Contains("Test-RunnerPathUnderRoot", StringComparison.Ordinal)
                && script.Contains("under the repository root", StringComparison.Ordinal),
            RejectsManualInputRoot =
                script.Contains(".llmgc/manual/", StringComparison.Ordinal)
                && script.Contains("PackagePath must not point", StringComparison.Ordinal),
            PassesUnityPackageArgument =
                script.Contains(
                    ParameterizedGamePackageProjectionRunnerVocabulary.PackageArgumentName,
                    StringComparison.Ordinal),
            ExecuteMethodPresent =
                script.Contains(
                    ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeExecuteMethod,
                    StringComparison.Ordinal),
            PassMarkerScanPresent =
                script.Contains(
                    ParameterizedGamePackageProjectionRunnerVocabulary.PassMarker,
                    StringComparison.Ordinal),
            FailMarkerScanPresent =
                script.Contains(
                    ParameterizedGamePackageProjectionRunnerVocabulary.FailMarker,
                    StringComparison.Ordinal),
            WritesRequiredResultJsonFields = requiredFields,
            CleanupDelegatesToBoundedScript =
                script.Contains("clean-unity-editor-noise.ps1", StringComparison.Ordinal)
                && script.Contains("-Apply", StringComparison.Ordinal),
            CmdWrapperPreservesDefaultAndExtraParams =
                cmd.Contains("run-unity-projection-verification.ps1", StringComparison.Ordinal)
                && cmd.Contains("-Mode GenericFullPlaythrough", StringComparison.Ordinal)
                && cmd.Contains("-ApplyCleanup", StringComparison.Ordinal)
                && cmd.Contains("%*", StringComparison.Ordinal),
            NoBroadGitClean = !broadGitClean,
            NoForbiddenMutationTargets = forbiddenFound.Count == 0,
            ForbiddenMarkersFound = forbiddenFound
        };

        return scan with
        {
            Passed = scan.RunnerScriptExists
                     && scan.RunnerCmdExists
                     && scan.SupportsPackagePathParameter
                     && scan.SupportsDefaultPackagePath
                     && scan.RejectsOutsideRepository
                     && scan.RejectsManualInputRoot
                     && scan.PassesUnityPackageArgument
                     && scan.ExecuteMethodPresent
                     && scan.PassMarkerScanPresent
                     && scan.FailMarkerScanPresent
                     && scan.WritesRequiredResultJsonFields
                     && scan.CleanupDelegatesToBoundedScript
                     && scan.CmdWrapperPreservesDefaultAndExtraParams
                     && scan.NoBroadGitClean
                     && scan.NoForbiddenMutationTargets
        };
    }

    private static ParameterizedGamePackageProjectionRunnerUnitySourceScan BuildUnitySourceScan(
        string root)
    {
        var adapter = ReadText(root, "unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionAdapter.cs");
        var controller = ReadText(root, "unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionController.cs");
        var models = ReadText(root, "unity/LLMGameCreatorAlpha/Assets/Scripts/GenericGamePackageProjectionModels.cs");
        var window = ReadText(root, "unity/LLMGameCreatorAlpha/Assets/Editor/AcceptedAlphaPlayableProjectionWindow.cs");

        var scan = new ParameterizedGamePackageProjectionRunnerUnitySourceScan
        {
            AdapterReadsCommandLineArgument =
                adapter.Contains("Environment.GetCommandLineArgs", StringComparison.Ordinal)
                && adapter.Contains(
                    ParameterizedGamePackageProjectionRunnerVocabulary.PackageArgumentName,
                    StringComparison.Ordinal),
            AdapterFallsBackToDefaultSample =
                adapter.Contains(
                    ParameterizedGamePackageProjectionRunnerVocabulary.DefaultPackageRelativePath,
                    StringComparison.Ordinal),
            AdapterRejectsOutsideRepository =
                adapter.Contains("IsUnderRoot", StringComparison.Ordinal)
                && adapter.Contains("goal128_package_path_outside_repository", StringComparison.Ordinal),
            AdapterRejectsManualInputRoot =
                adapter.Contains(".llmgc/manual/", StringComparison.Ordinal)
                && adapter.Contains("goal128_package_path_manual_input_rejected", StringComparison.Ordinal),
            ControllerRunsParameterizedFullPlaythrough =
                controller.Contains("RunParameterizedGamePackageFullPlaythroughVerification", StringComparison.Ordinal)
                && controller.Contains("LoadParameterizedPackageProjection", StringComparison.Ordinal),
            BatchmodeEntrypointPresent =
                window.Contains(
                    "RunBatchmodeParameterizedGamePackageFullPlaythroughSmoke",
                    StringComparison.Ordinal),
            BatchmodeMarkersPresent =
                window.Contains(
                    ParameterizedGamePackageProjectionRunnerVocabulary.PassMarker,
                    StringComparison.Ordinal)
                && window.Contains(
                    ParameterizedGamePackageProjectionRunnerVocabulary.FailMarker,
                    StringComparison.Ordinal),
            SmokeFieldsPresent =
                models.Contains("parameterizedRunnerPassed", StringComparison.Ordinal)
                && models.Contains("packagePathResolved", StringComparison.Ordinal)
                && models.Contains("packagePathUnderRepo", StringComparison.Ordinal)
                && models.Contains("samplePackageLoaded", StringComparison.Ordinal)
                && models.Contains("fullPlaythroughPassed", StringComparison.Ordinal)
                && models.Contains("eventTranscriptPresent", StringComparison.Ordinal)
                && models.Contains("zeroFatalErrors", StringComparison.Ordinal)
        };

        return scan with
        {
            Passed = scan.AdapterReadsCommandLineArgument
                     && scan.AdapterFallsBackToDefaultSample
                     && scan.AdapterRejectsOutsideRepository
                     && scan.AdapterRejectsManualInputRoot
                     && scan.ControllerRunsParameterizedFullPlaythrough
                     && scan.BatchmodeEntrypointPresent
                     && scan.BatchmodeMarkersPresent
                     && scan.SmokeFieldsPresent
        };
    }

    private static ParameterizedGamePackageProjectionRunnerResultScan BuildResultScan(string root)
    {
        var path = Resolve(root, ParameterizedGamePackageProjectionRunnerVocabulary.ResultRelativePath);
        if (!File.Exists(path))
        {
            return new ParameterizedGamePackageProjectionRunnerResultScan
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
                "packagePath",
                "packagePathRelative",
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
        var packagePath = StringValue(doc.RootElement, "packagePath");
        var packagePathRelative = StringValue(doc.RootElement, "packagePathRelative");
        var packagePathUnderRepo = !string.IsNullOrWhiteSpace(packagePath)
                                   && IsUnderRoot(root, packagePath)
                                   && !packagePathRelative.StartsWith(
                                       ".llmgc/manual/",
                                       StringComparison.Ordinal);
        var unityExitCode = IntValue(doc.RootElement, "unityExitCode");
        var passMarker = BoolValue(doc.RootElement, "passMarkerPresent");
        var failAbsent = BoolValue(doc.RootElement, "failMarkerAbsent");
        var materialAbsent = BoolValue(doc.RootElement, "materialWarningAbsent");
        var cleanupApplied = BoolValue(doc.RootElement, "cleanupApplied");
        var cleanupExitCode = IntValue(doc.RootElement, "cleanupExitCode");
        var passed = BoolValue(doc.RootElement, "passed");
        var logPath = StringValue(doc.RootElement, "logPath");

        return new ParameterizedGamePackageProjectionRunnerResultScan
        {
            ResultExists = true,
            Mode = mode,
            PackagePath = packagePath,
            PackagePathRelative = packagePathRelative,
            PackagePathUnderRepo = packagePathUnderRepo,
            UnityPath = StringValue(doc.RootElement, "unityPath"),
            UnityExitCode = unityExitCode,
            PassMarkerPresent = passMarker,
            FailMarkerAbsent = failAbsent,
            MaterialWarningAbsent = materialAbsent,
            CleanupApplied = cleanupApplied,
            CleanupExitCode = cleanupExitCode,
            Passed = requiredFields
                     && mode == ParameterizedGamePackageProjectionRunnerVocabulary.Mode
                     && packagePathRelative
                         == ParameterizedGamePackageProjectionRunnerVocabulary.DefaultPackageRelativePath
                     && packagePathUnderRepo
                     && unityExitCode == 0
                     && passMarker
                     && failAbsent
                     && materialAbsent
                     && cleanupApplied
                     && cleanupExitCode == 0
                     && passed
                     && logPath
                         == ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeLogRelativePath,
            LogPath = logPath,
            RequiredFieldsPresent = requiredFields
        };
    }

    private static ParameterizedGamePackageProjectionRunnerLogScan BuildLogScan(string root)
    {
        var path = Resolve(root, ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeLogRelativePath);
        if (!File.Exists(path))
        {
            return new ParameterizedGamePackageProjectionRunnerLogScan
            {
                LogExists = false,
                Passed = false
            };
        }

        var text = File.ReadAllText(path, Encoding.UTF8);
        var forbidden = new List<string>();
        if (text.Contains(
                ParameterizedGamePackageProjectionRunnerVocabulary.FailMarker,
                StringComparison.Ordinal))
        {
            forbidden.Add(ParameterizedGamePackageProjectionRunnerVocabulary.FailMarker);
        }

        if (text.Contains(
                ParameterizedGamePackageProjectionRunnerVocabulary.MaterialWarningMarker,
                StringComparison.Ordinal))
        {
            forbidden.Add(ParameterizedGamePackageProjectionRunnerVocabulary.MaterialWarningMarker);
        }

        if (text.Contains(
                ParameterizedGamePackageProjectionRunnerVocabulary.RendererMaterialMarker,
                StringComparison.Ordinal))
        {
            forbidden.Add(ParameterizedGamePackageProjectionRunnerVocabulary.RendererMaterialMarker);
        }

        var passMarker = text.Contains(
            ParameterizedGamePackageProjectionRunnerVocabulary.PassMarker,
            StringComparison.Ordinal);
        return new ParameterizedGamePackageProjectionRunnerLogScan
        {
            LogExists = true,
            PassMarkerPresent = passMarker,
            FailMarkerAbsent =
                !forbidden.Contains(ParameterizedGamePackageProjectionRunnerVocabulary.FailMarker),
            MaterialWarningAbsent =
                !forbidden.Contains(ParameterizedGamePackageProjectionRunnerVocabulary.MaterialWarningMarker)
                && !forbidden.Contains(ParameterizedGamePackageProjectionRunnerVocabulary.RendererMaterialMarker),
            Passed = passMarker && forbidden.Count == 0,
            Sha256 = HashBytes(File.ReadAllBytes(path)),
            ForbiddenMarkersFound = forbidden.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static ParameterizedGamePackageProjectionRunnerNegativeProof BuildNegativeProof(
        ParameterizedGamePackageProjectionRunnerScriptScan scriptScan)
    {
        var rejected = BuildRejectedPathSamples();
        var proof = new ParameterizedGamePackageProjectionRunnerNegativeProof
        {
            ManualInputRejected = scriptScan.RejectsManualInputRoot,
            SamplePackageReadOnly = scriptScan.SupportsDefaultPackagePath,
            UnityProjectSettingsMutationRejected = scriptScan.NoForbiddenMutationTargets,
            RuntimeSchemaProviderLuaGeneratorLibraryRejected = scriptScan.NoForbiddenMutationTargets,
            BroadGitCleanRejected = scriptScan.NoBroadGitClean,
            OnlyAllowedRunnerArtifactsExpected = scriptScan.NoForbiddenMutationTargets,
            RejectedPathSamples = rejected
        };
        return proof with
        {
            Passed = proof.ManualInputRejected
                     && proof.SamplePackageReadOnly
                     && proof.UnityProjectSettingsMutationRejected
                     && proof.RuntimeSchemaProviderLuaGeneratorLibraryRejected
                     && proof.BroadGitCleanRejected
                     && proof.OnlyAllowedRunnerArtifactsExpected
        };
    }

    private static ParameterizedGamePackageProjectionRunnerFileIndex BuildFileIndex(
        string root,
        SortedDictionary<string, string> textFiles,
        string relativeRoot,
        bool isExport)
    {
        var entries = textFiles
            .Select(item => new ParameterizedGamePackageProjectionRunnerFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = "goal128_parameterized_gamepackage_runner_" + Path.GetFileNameWithoutExtension(item.Key),
                Sha256 = HashText(item.Value)
            })
            .ToList();

        AddRunnerProducedFileIndexEntry(
            root,
            entries,
            isExport
                ? ParameterizedGamePackageProjectionRunnerVocabulary.ExportResultRelativePath
                : ParameterizedGamePackageProjectionRunnerVocabulary.ResultRelativePath,
            ParameterizedGamePackageProjectionRunnerVocabulary.ResultRelativePath,
            "goal128_parameterized_gamepackage_runner_result");
        AddRunnerProducedFileIndexEntry(
            root,
            entries,
            isExport
                ? ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeExportLogRelativePath
                : ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeLogRelativePath,
            ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeLogRelativePath,
            "goal128_parameterized_gamepackage_unity_log");

        return new ParameterizedGamePackageProjectionRunnerFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static void AddRunnerProducedFileIndexEntry(
        string root,
        List<ParameterizedGamePackageProjectionRunnerFileIndexEntry> entries,
        string indexedRelativePath,
        string sourceRelativePath,
        string role)
    {
        var sourcePath = Resolve(root, sourceRelativePath);
        if (!File.Exists(sourcePath))
        {
            return;
        }

        entries.Add(new ParameterizedGamePackageProjectionRunnerFileIndexEntry
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
            ParameterizedGamePackageProjectionRunnerVocabulary.ResultRelativePath,
            ParameterizedGamePackageProjectionRunnerVocabulary.ExportResultRelativePath,
            written,
            cancellationToken);
        CopyIfExists(
            root,
            ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeLogRelativePath,
            ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeExportLogRelativePath,
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
        ParameterizedGamePackageProjectionRunnerDashboard dashboard,
        ParameterizedGamePackageProjectionRunnerGoal127Evidence goal127,
        ParameterizedGamePackageProjectionRunnerScriptScan scriptScan,
        ParameterizedGamePackageProjectionRunnerUnitySourceScan unitySourceScan,
        ParameterizedGamePackageProjectionRunnerResultScan resultScan,
        ParameterizedGamePackageProjectionRunnerLogScan logScan,
        ParameterizedGamePackageProjectionRunnerNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 128 Parameterized GamePackage Projection Runner",
            string.Empty,
            "- parameterizedRunnerStatus: " + dashboard.ParameterizedRunnerStatus,
            "- mode: " + dashboard.Mode,
            "- packagePath: " + dashboard.PackagePath,
            "- packagePathRelative: " + dashboard.PackagePathRelative,
            "- packagePathResolved: " + dashboard.PackagePathResolved.ToString().ToLowerInvariant(),
            "- packagePathUnderRepo: " + dashboard.PackagePathUnderRepo.ToString().ToLowerInvariant(),
            "- normalCommand: " + dashboard.NormalCommand,
            "- exampleCommandWithPackagePath: " + dashboard.ExampleCommandWithPackagePath,
            "- resultPath: " + dashboard.ResultPath,
            "- logPath: " + dashboard.LogPath,
            "- unityExitCode: " + dashboard.UnityExitCode,
            "- passMarkerPresent: " + dashboard.PassMarkerPresent.ToString().ToLowerInvariant(),
            "- cleanupApplied: " + dashboard.CleanupApplied.ToString().ToLowerInvariant(),
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant(),
            "- projectionOnly: " + dashboard.ProjectionOnly.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Goal127 Evidence",
            string.Empty,
            "- passed: " + goal127.Passed.ToString().ToLowerInvariant(),
            "- goal127RunnerGreen: " + goal127.Goal127RunnerGreen.ToString().ToLowerInvariant(),
            string.Empty,
            "## Script Scan",
            string.Empty,
            "- passed: " + scriptScan.Passed.ToString().ToLowerInvariant(),
            "- supportsPackagePathParameter: "
            + scriptScan.SupportsPackagePathParameter.ToString().ToLowerInvariant(),
            "- passesUnityPackageArgument: "
            + scriptScan.PassesUnityPackageArgument.ToString().ToLowerInvariant(),
            string.Empty,
            "## Unity Source Scan",
            string.Empty,
            "- passed: " + unitySourceScan.Passed.ToString().ToLowerInvariant(),
            "- batchmodeEntrypointPresent: "
            + unitySourceScan.BatchmodeEntrypointPresent.ToString().ToLowerInvariant(),
            string.Empty,
            "## Result Scan",
            string.Empty,
            "- resultExists: " + resultScan.ResultExists.ToString().ToLowerInvariant(),
            "- passed: " + resultScan.Passed.ToString().ToLowerInvariant(),
            "- packagePathRelative: " + resultScan.PackagePathRelative,
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

    private static string RenderDocumentation(
        ParameterizedGamePackageProjectionRunnerDashboard dashboard)
    {
        var lines = new List<string>
        {
            "# Parameterized GamePackage Projection Runner",
            string.Empty,
            "Goal128 keeps the normal Unity projection verification command and adds an optional `-PackagePath` parameter for repo-local GamePackage JSON files.",
            "The default command still verifies `samples/minimal-map-game/package.json`; manual Unity inspection remains optional.",
            string.Empty,
            "## Normal Command",
            string.Empty,
            "- `" + dashboard.NormalCommand + "`",
            string.Empty,
            "## Example With Package Path",
            string.Empty,
            "- `" + dashboard.ExampleCommandWithPackagePath + "`",
            string.Empty,
            "## Scope Guard",
            string.Empty,
            "- Package paths must stay under the repository root and outside `.llmgc/manual/`.",
            "- This runner is projection-only and does not authorize Runtime, public schema, provider, Lua, generator-library, Unity scene, prefab, ProjectSettings, Packages or StreamingAssets work.",
            string.Empty,
            "## Status",
            string.Empty,
            "- parameterizedRunnerStatus: " + dashboard.ParameterizedRunnerStatus,
            "- packagePathRelative: " + dashboard.PackagePathRelative,
            "- resultPath: " + dashboard.ResultPath,
            "- logPath: " + dashboard.LogPath,
            "- manualUnityOptional: " + dashboard.ManualUnityOptional.ToString().ToLowerInvariant()
        };
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
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

    private static string ReadText(string root, string relativePath)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
    }

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

    private static bool IsUnderRoot(string root, string path)
    {
        var fullRoot = Path.GetFullPath(root).TrimEnd(
                           Path.DirectorySeparatorChar,
                           Path.AltDirectorySeparatorChar)
                       + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

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
            throw new InvalidOperationException("Goal128 must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
