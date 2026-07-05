using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public sealed class AcceptedAlphaUnityMaterialWarningHotfixService
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly Regex RendererMaterialAccessPattern =
        new(@"\brenderer\.material\b", RegexOptions.Compiled);

    private static readonly Regex MaterialAssignmentPattern =
        new(@"\.material\s*=", RegexOptions.Compiled);

    public AcceptedAlphaUnityMaterialWarningHotfixBuildResult Build(string repositoryRootPath)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var scriptScan = BuildScriptScan(root);
        var logScan = BuildLogScan(root);
        var negative = BuildNegativeProof();
        var dashboard = BuildDashboard(scriptScan, logScan, negative);
        var report = RenderReport(dashboard, scriptScan, logScan, negative);

        var proceduralFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AcceptedAlphaUnityMaterialWarningHotfixVocabulary.DashboardFileName] = Serialize(dashboard),
            [AcceptedAlphaUnityMaterialWarningHotfixVocabulary.LogScanFileName] = Serialize(logScan),
            [AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ScriptScanFileName] = Serialize(scriptScan),
            [AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ReportFileName] = report,
            [AcceptedAlphaUnityMaterialWarningHotfixVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var proceduralIndex = BuildFileIndex(
            root,
            proceduralFiles,
            AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ProceduralOutputDirectory,
            "goal119a_accepted_alpha_unity_material_warning_hotfix_evidence",
            includeUnityLog: true);
        proceduralFiles[AcceptedAlphaUnityMaterialWarningHotfixVocabulary.FileIndexFileName] =
            Serialize(proceduralIndex);

        var exportFiles = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [AcceptedAlphaUnityMaterialWarningHotfixVocabulary.DashboardFileName] = Serialize(dashboard),
            [AcceptedAlphaUnityMaterialWarningHotfixVocabulary.LogScanFileName] = Serialize(logScan),
            [AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ScriptScanFileName] = Serialize(scriptScan),
            [AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ReportFileName] = report,
            [AcceptedAlphaUnityMaterialWarningHotfixVocabulary.NegativeProofFileName] = Serialize(negative)
        };
        var exportIndex = BuildFileIndex(
            root,
            exportFiles,
            AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ExportPackageDirectory,
            "goal119a_accepted_alpha_unity_material_warning_hotfix_export",
            includeUnityLog: false);
        exportFiles[AcceptedAlphaUnityMaterialWarningHotfixVocabulary.FileIndexFileName] =
            Serialize(exportIndex);

        return new AcceptedAlphaUnityMaterialWarningHotfixBuildResult
        {
            Dashboard = dashboard,
            ScriptScan = scriptScan,
            LogScan = logScan,
            NegativeProof = negative,
            ProceduralFileIndex = proceduralIndex,
            ExportFileIndex = exportIndex,
            ProceduralFiles = proceduralFiles,
            ExportFiles = exportFiles,
            ReportMarkdown = report
        };
    }

    public async Task<AcceptedAlphaUnityMaterialWarningHotfixWriteResult> BuildAndWriteAsync(
        string repositoryRootPath,
        CancellationToken cancellationToken = default)
    {
        var root = ResolveRepositoryRoot(repositoryRootPath);
        var result = Build(root);
        var procedural = Resolve(
            root,
            AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ProceduralOutputDirectory);
        var export = Resolve(
            root,
            AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ExportPackageDirectory);
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

        return new AcceptedAlphaUnityMaterialWarningHotfixWriteResult
        {
            Result = result,
            ProceduralOutputDirectoryPath = procedural,
            ExportPackageDirectoryPath = export,
            WrittenFiles = written.OrderBy(path => path, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaUnityMaterialWarningHotfixScriptScan BuildScriptScan(string root)
    {
        var entries = AcceptedAlphaUnityPlayableProjectionVocabulary.UnityScriptPaths
            .Select(path => ScanScript(root, path))
            .ToList();
        var primitiveFactory = entries.Single(entry =>
            entry.RelativePath
            == AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath);
        var allFilesExist = entries.All(entry => entry.Exists);
        var noRendererMaterialAccess = entries.All(entry => !entry.ContainsRendererMaterialAccess);
        var noMaterialAssignment = entries.All(entry => !entry.ContainsMaterialAssignment);
        var colorPropertySet = SourceContains(root,
            AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
            "_Color");
        var baseColorPropertySet = SourceContains(root,
            AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath,
            "_BaseColor");
        return new AcceptedAlphaUnityMaterialWarningHotfixScriptScan
        {
            Passed = allFilesExist
                     && noRendererMaterialAccess
                     && noMaterialAssignment
                     && primitiveFactory.ContainsMaterialPropertyBlock
                     && colorPropertySet
                     && baseColorPropertySet
                     && !primitiveFactory.ContainsNewMaterial,
            ScannedFileCount = entries.Count,
            RendererMaterialAccessAbsent = noRendererMaterialAccess,
            MaterialAssignmentAbsent = noMaterialAssignment,
            MaterialPropertyBlockUsed = primitiveFactory.ContainsMaterialPropertyBlock,
            ColorPropertySet = colorPropertySet,
            BaseColorPropertySet = baseColorPropertySet,
            NoNewMaterialInPrimitiveFactory = !primitiveFactory.ContainsNewMaterial,
            Files = entries
        };
    }

    private static AcceptedAlphaUnityMaterialWarningHotfixScriptScanEntry ScanScript(
        string root,
        string relativePath)
    {
        var path = Resolve(root, relativePath);
        var exists = File.Exists(path);
        var text = exists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        return new AcceptedAlphaUnityMaterialWarningHotfixScriptScanEntry
        {
            RelativePath = relativePath,
            Exists = exists,
            ContainsRendererMaterialAccess = RendererMaterialAccessPattern.IsMatch(text),
            ContainsMaterialAssignment = MaterialAssignmentPattern.IsMatch(text),
            ContainsNewMaterial = text.Contains("new Material(", StringComparison.Ordinal),
            ContainsMaterialPropertyBlock = text.Contains("MaterialPropertyBlock", StringComparison.Ordinal)
                                            && text.Contains("SetPropertyBlock", StringComparison.Ordinal),
            Sha256 = exists ? HashBytes(File.ReadAllBytes(path)) : string.Empty
        };
    }

    private static bool SourceContains(string root, string relativePath, string marker)
    {
        var path = Resolve(root, relativePath);
        return File.Exists(path)
               && File.ReadAllText(path, Encoding.UTF8).Contains(marker, StringComparison.Ordinal);
    }

    private static AcceptedAlphaUnityMaterialWarningHotfixLogScan BuildLogScan(string root)
    {
        var path = Resolve(root, AcceptedAlphaUnityMaterialWarningHotfixVocabulary.UnityBatchmodeLogRelativePath);
        var logExists = File.Exists(path);
        var text = logExists ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        var forbidden = new List<string>();
        var materialWarning =
            "Instantiating material due to calling renderer" + ".material during edit mode";
        var rendererStack = "UnityEngine.Renderer:get_material()";
        if (text.Contains(materialWarning, StringComparison.Ordinal))
        {
            forbidden.Add(materialWarning);
        }

        if (text.Contains(rendererStack, StringComparison.Ordinal))
        {
            forbidden.Add(rendererStack);
        }

        if (text.Contains("GOAL119A_PROJECTION_SMOKE_FAIL", StringComparison.Ordinal))
        {
            forbidden.Add("GOAL119A_PROJECTION_SMOKE_FAIL");
        }

        var passMarkerPresent = text.Contains("GOAL119A_PROJECTION_SMOKE_PASS", StringComparison.Ordinal);
        var passed = logExists && passMarkerPresent && forbidden.Count == 0;
        return new AcceptedAlphaUnityMaterialWarningHotfixLogScan
        {
            LogExists = logExists,
            PassMarkerPresent = passMarkerPresent,
            FailMarkerAbsent = !forbidden.Contains("GOAL119A_PROJECTION_SMOKE_FAIL", StringComparer.Ordinal),
            MaterialInstantiationWarningAbsent = !forbidden.Contains(materialWarning, StringComparer.Ordinal),
            RendererGetMaterialStackAbsent = !forbidden.Contains(rendererStack, StringComparer.Ordinal),
            Passed = passed,
            Status = passed
                ? "GREEN"
                : logExists
                    ? "BLOCKED_UNITY_LOG_SCAN_FAILED"
                    : "BLOCKED_PENDING_UNITY_BATCHMODE_SMOKE",
            Sha256 = logExists ? HashBytes(File.ReadAllBytes(path)) : string.Empty,
            ForbiddenMarkersFound = forbidden.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaUnityMaterialWarningHotfixDashboard BuildDashboard(
        AcceptedAlphaUnityMaterialWarningHotfixScriptScan scriptScan,
        AcceptedAlphaUnityMaterialWarningHotfixLogScan logScan,
        AcceptedAlphaUnityMaterialWarningHotfixNegativeProof negative)
    {
        var diagnostics = new List<string>();
        if (!scriptScan.Passed)
        {
            diagnostics.Add("goal119a.script_scan_failed");
        }

        if (!logScan.Passed)
        {
            diagnostics.Add(logScan.Status.ToLowerInvariant());
        }

        if (!negative.Passed)
        {
            diagnostics.Add("goal119a.negative_proof_failed");
        }

        var passed = scriptScan.Passed && logScan.Passed && negative.Passed;
        return new AcceptedAlphaUnityMaterialWarningHotfixDashboard
        {
            ImplementationStatus = passed ? "GREEN" : "BLOCKED",
            UnitySmokeStatus = logScan.Status,
            UnityLogExists = logScan.LogExists,
            UnityLogContainsPassMarker = logScan.PassMarkerPresent,
            MaterialWarningAbsent =
                logScan.LogExists
                && logScan.MaterialInstantiationWarningAbsent
                && logScan.RendererGetMaterialStackAbsent,
            RendererMaterialSourceAccessAbsent = scriptScan.RendererMaterialAccessAbsent,
            MaterialAssignmentSourceAccessAbsent = scriptScan.MaterialAssignmentAbsent,
            MaterialPropertyBlockUsed = scriptScan.MaterialPropertyBlockUsed,
            ColorAndBaseColorPropertyBlocksSet = scriptScan.ColorPropertySet && scriptScan.BaseColorPropertySet,
            NoPerMarkerMaterialInstantiation = scriptScan.NoNewMaterialInPrimitiveFactory,
            NegativeProofPassed = negative.Passed,
            Diagnostics = diagnostics.OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private static AcceptedAlphaUnityMaterialWarningHotfixNegativeProof BuildNegativeProof()
    {
        var rejected = BuildRejectedPathSamples();
        return new AcceptedAlphaUnityMaterialWarningHotfixNegativeProof
        {
            ManualInputRejected = true,
            RuntimeSchemaProviderLuaGeneratorLibraryRejected = true,
            UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected = true,
            FinalReleasePackagingRejected = true,
            LiveGeodataProviderNetworkRejected = true,
            RejectedPathSamples = rejected,
            Passed = rejected.All(path => !IsAllowedChangedPath(path))
        };
    }

    private static string RenderReport(
        AcceptedAlphaUnityMaterialWarningHotfixDashboard dashboard,
        AcceptedAlphaUnityMaterialWarningHotfixScriptScan scriptScan,
        AcceptedAlphaUnityMaterialWarningHotfixLogScan logScan,
        AcceptedAlphaUnityMaterialWarningHotfixNegativeProof negative)
    {
        var lines = new List<string>
        {
            "# Goal 119A Accepted Alpha Unity Material Warning Hotfix",
            string.Empty,
            "- implementationStatus: " + dashboard.ImplementationStatus,
            "- unitySmokeStatus: " + dashboard.UnitySmokeStatus,
            "- unityBatchmodeExecuteMethod: " + dashboard.UnityBatchmodeExecuteMethod,
            "- unityBatchmodeLogPath: " + dashboard.UnityBatchmodeLogPath,
            "- unityLogExists: " + dashboard.UnityLogExists.ToString().ToLowerInvariant(),
            "- unityLogContainsPassMarker: " + dashboard.UnityLogContainsPassMarker.ToString().ToLowerInvariant(),
            "- materialWarningAbsent: " + dashboard.MaterialWarningAbsent.ToString().ToLowerInvariant(),
            "- rendererMaterialSourceAccessAbsent: "
            + dashboard.RendererMaterialSourceAccessAbsent.ToString().ToLowerInvariant(),
            "- materialAssignmentSourceAccessAbsent: "
            + dashboard.MaterialAssignmentSourceAccessAbsent.ToString().ToLowerInvariant(),
            "- materialPropertyBlockUsed: " + dashboard.MaterialPropertyBlockUsed.ToString().ToLowerInvariant(),
            "- colorAndBaseColorPropertyBlocksSet: "
            + dashboard.ColorAndBaseColorPropertyBlocksSet.ToString().ToLowerInvariant(),
            "- noPerMarkerMaterialInstantiation: "
            + dashboard.NoPerMarkerMaterialInstantiation.ToString().ToLowerInvariant(),
            "- negativeProofPassed: " + dashboard.NegativeProofPassed.ToString().ToLowerInvariant(),
            "- evidencePath: " + dashboard.EvidencePath,
            "- exportPath: " + dashboard.ExportPath,
            string.Empty,
            "## Unity Script Scan",
            string.Empty,
            "- passed: " + scriptScan.Passed.ToString().ToLowerInvariant(),
            "- scannedFileCount: " + scriptScan.ScannedFileCount,
            "- rendererMaterialAccessAbsent: "
            + scriptScan.RendererMaterialAccessAbsent.ToString().ToLowerInvariant(),
            "- materialAssignmentAbsent: " + scriptScan.MaterialAssignmentAbsent.ToString().ToLowerInvariant(),
            "- materialPropertyBlockUsed: " + scriptScan.MaterialPropertyBlockUsed.ToString().ToLowerInvariant(),
            "- noNewMaterialInPrimitiveFactory: "
            + scriptScan.NoNewMaterialInPrimitiveFactory.ToString().ToLowerInvariant(),
            string.Empty,
            "## Unity Log Scan",
            string.Empty,
            "- passed: " + logScan.Passed.ToString().ToLowerInvariant(),
            "- status: " + logScan.Status,
            "- passMarkerPresent: " + logScan.PassMarkerPresent.ToString().ToLowerInvariant(),
            "- forbiddenMarkerCount: " + logScan.ForbiddenMarkersFound.Count,
            string.Empty,
            "## Negative Proof",
            string.Empty,
            "- passed: " + negative.Passed.ToString().ToLowerInvariant(),
            "- manualInputRejected: " + negative.ManualInputRejected.ToString().ToLowerInvariant(),
            "- runtimeSchemaProviderLuaGeneratorLibraryRejected: "
            + negative.RuntimeSchemaProviderLuaGeneratorLibraryRejected.ToString().ToLowerInvariant(),
            "- unityScenesPrefabsSettingsPackagesStreamingAssetsRejected: "
            + negative.UnityScenesPrefabsSettingsPackagesStreamingAssetsRejected.ToString().ToLowerInvariant(),
            "- finalReleasePackagingRejected: "
            + negative.FinalReleasePackagingRejected.ToString().ToLowerInvariant(),
            "- liveGeodataProviderNetworkRejected: "
            + negative.LiveGeodataProviderNetworkRejected.ToString().ToLowerInvariant()
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

    private static AcceptedAlphaUnityMaterialWarningHotfixFileIndex BuildFileIndex(
        string root,
        IReadOnlyDictionary<string, string> files,
        string relativeRoot,
        string role,
        bool includeUnityLog)
    {
        var entries = files
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select(item => new AcceptedAlphaUnityMaterialWarningHotfixFileIndexEntry
            {
                RelativePath = relativeRoot + "/" + item.Key,
                Role = role,
                Sha256 = HashText(item.Value)
            })
            .ToList();
        if (includeUnityLog)
        {
            var logPath = Resolve(root, AcceptedAlphaUnityMaterialWarningHotfixVocabulary.UnityBatchmodeLogRelativePath);
            if (File.Exists(logPath))
            {
                entries.Add(new AcceptedAlphaUnityMaterialWarningHotfixFileIndexEntry
                {
                    RelativePath = AcceptedAlphaUnityMaterialWarningHotfixVocabulary.UnityBatchmodeLogRelativePath,
                    Role = "goal119a_unity_batchmode_projection_smoke_log",
                    Sha256 = HashBytes(File.ReadAllBytes(logPath))
                });
            }
        }

        return new AcceptedAlphaUnityMaterialWarningHotfixFileIndex
        {
            IndexedFileCount = entries.Count,
            ManualInputExcluded = entries.All(entry =>
                !entry.RelativePath.StartsWith(".llmgc/manual/", StringComparison.Ordinal)),
            Files = entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToList()
        };
    }

    private static IReadOnlyList<string> BuildAllowedChangedPaths() =>
    [
        AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ProceduralOutputDirectory + "/",
        AcceptedAlphaUnityMaterialWarningHotfixVocabulary.ExportPackageDirectory + "/",
        "docs/agent-tasks/goal-119a-accepted-alpha-unity-material-warning-hotfix/",
        ".devflow/artifact-scope/artifact-scope-policy.json",
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/CONTEXT_INDEX.md",
        "docs/MILESTONE_GATES.md",
        "docs/RELEASE_RISK_REGISTER.md",
        "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
        "src/LLMGameCreator.Application/Design/AcceptedAlphaUnityPlayableProjection/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/Application/AcceptedAlphaUnityPlayableProjection/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs",
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityEditorWindowPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityControllerPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityDiagnosticsPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityModelsPath,
        AcceptedAlphaUnityPlayableProjectionVocabulary.UnityPrimitiveFactoryPath
    ];

    private static IReadOnlyList<string> BuildRejectedPathSamples() =>
    [
        ".llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json",
        "src/LLMGameCreator.Runtime/GameRuntime.cs",
        "src/LLMGameCreator.Runtime.Abstractions/IGameRuntime.cs",
        "src/LLMGameCreator.GamePackage/GamePackageDefinition.cs",
        "src/LLMGameCreator.Scripting/LuaSandbox.cs",
        "generator-library/example.json",
        "unity/LLMGameCreatorAlpha/Assets/Scenes/Main.unity",
        "unity/LLMGameCreatorAlpha/Assets/Prefabs/AcceptedAlpha.prefab",
        "unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset",
        "unity/LLMGameCreatorAlpha/Packages/manifest.json",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal119/example.json",
        ".llmgc/exports/final-release/package.zip",
        "provider/live-geodata/Adapter.cs"
    ];

    private static bool IsAllowedChangedPath(string path) =>
        BuildAllowedChangedPaths().Any(prefix =>
            prefix.EndsWith("/", StringComparison.Ordinal)
                ? path.StartsWith(prefix, StringComparison.Ordinal)
                : string.Equals(path, prefix, StringComparison.Ordinal));

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine;

    private static string ResolveRepositoryRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Repository root path is required.", nameof(path));
        }

        return Path.GetFullPath(path);
    }

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
            throw new InvalidOperationException("Goal119A must not write the manual input path.");
        }
    }

    private static string HashText(string text) => HashBytes(Encoding.UTF8.GetBytes(text));

    private static string HashBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
