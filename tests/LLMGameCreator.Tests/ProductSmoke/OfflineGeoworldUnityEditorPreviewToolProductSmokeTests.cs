using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldUnityEditorPreviewTool;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldUnityEditorPreviewToolProductSmokeTests
{
    private static readonly HashSet<string> ForbiddenOutputExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".osm",
        ".pbf",
        ".mbtiles",
        ".gpkg",
        ".geojson",
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes",
        ".unity",
        ".prefab"
    };

    [Fact]
    public async Task Goal102OfflineGeoworldUnityEditorPreviewToolProductSmoke()
    {
        await Task.CompletedTask;
        var repoRoot = FindRepoRoot();
        var result = new OfflineGeoworldUnityEditorPreviewToolEvidenceService()
            .Build(repoRoot);

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.True(result.QualityGateScan.Passed);
        Assert.Equal(18, result.Report.CommandCount);
        Assert.Equal(10, result.Report.CommandKindCount);
        Assert.True(result.Report.TravelWindowStepCount >= 4);
        Assert.Equal(18, result.Report.ExpectedObjectCount);
        Assert.True(result.Report.EditorWindowScriptReady);
        Assert.True(result.Report.SimulatedActionProofPassed);
        Assert.True(result.Report.ClearOperationProofPassed);
        Assert.True(result.Report.NegativeProofPassed);
        Assert.True(result.Report.AlphaRuntimeBootstrapUnchanged);

        using var inventory = JsonDocument.Parse(
            result.EvidenceJsonByFileName[OfflineGeoworldUnityEditorPreviewToolVocabulary.ToolInventoryFileName]);
        using var proof = JsonDocument.Parse(
            result.EvidenceJsonByFileName[OfflineGeoworldUnityEditorPreviewToolVocabulary.SimulatedActionProofFileName]);
        using var quality = JsonDocument.Parse(
            result.EvidenceJsonByFileName[OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName]);

        Assert.True(inventory.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(inventory.RootElement.GetProperty("menuItemMarkerPresent").GetBoolean());
        Assert.True(inventory.RootElement.GetProperty("streamingAssetsPathMarkerPresent").GetBoolean());
        Assert.True(inventory.RootElement.GetProperty("goal101PayloadPathMarkerPresent").GetBoolean());
        Assert.True(inventory.RootElement.GetProperty("createPreviewObjectsMethodPresent").GetBoolean());
        Assert.True(inventory.RootElement.GetProperty("clearPreviewObjectsMethodPresent").GetBoolean());
        Assert.True(proof.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(proof.RootElement.GetProperty("createOperationModelPassed").GetBoolean());
        Assert.True(proof.RootElement.GetProperty("clearOperationModelPassed").GetBoolean());
        Assert.True(proof.RootElement.GetProperty("noScenePrefabSettingsChangeMarkers").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noScenePrefabSettingsChanges").GetBoolean());

        var editorScriptPath = Path.Combine(
            repoRoot,
            "unity",
            "LLMGameCreatorAlpha",
            "Assets",
            "Editor",
            "OfflineGeoworldPreviewWindow.cs");
        var editorScript = await File.ReadAllTextAsync(editorScriptPath);
        Assert.Contains("LLMGameCreator/Offline Geoworld Preview", editorScript, StringComparison.Ordinal);
        Assert.Contains("Application.streamingAssetsPath", editorScript, StringComparison.Ordinal);
        Assert.Contains("LLMGameCreator/OfflineGeoworldGoal101", editorScript, StringComparison.Ordinal);
        Assert.Contains("CreatePreviewObjects", editorScript, StringComparison.Ordinal);
        Assert.Contains("ClearPreviewObjects", editorScript, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", editorScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UnityWebRequest", editorScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AlphaRuntimeBootstrap", editorScript, StringComparison.Ordinal);
        Assert.DoesNotContain("PrefabUtility", editorScript, StringComparison.Ordinal);
        Assert.DoesNotContain("EditorBuildSettings", editorScript, StringComparison.Ordinal);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        Assert.True(workspace.QualityGateScan.Passed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldUnityEditorPreviewGroupPresent);
        Assert.Equal(18, workspace.QualityGateScan.OfflineGeoworldUnityEditorPreviewCommandCount);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldUnityEditorPreviewEditorWindowScriptReady);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldUnityEditorPreviewSimulatedActionProofPassed);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldUnityEditorPreviewClearOperationProofPassed);
        Assert.True(workspace.QualityGateScan.Goal102FilesDiscoveredByRelativePaths);
        Assert.Contains(
            workspace.ProofStatus.Proofs,
            item => item.ProofId == "goal102.quality_gate" && item.Passed);

        Assert.DoesNotContain(result.EvidenceJsonByFileName.Keys, path =>
            ForbiddenOutputExtensions.Contains(Path.GetExtension(path)));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.StartsWith("src/LLMGameCreator.Runtime", StringComparison.Ordinal));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.StartsWith("src/LLMGameCreator.GamePackage", StringComparison.Ordinal));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
            || item.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase));

        var report = result.ReportMarkdown;
        Assert.Contains("offline_geoworld_unity_editor_preview_tool_verification required", report);
        Assert.Contains("commandCount: 18", report);
        Assert.Contains("expectedObjectCount: 18", report);
        Assert.Contains("editorWindowScriptReady: true", report);
        Assert.Contains("clearOperationProofPassed: true", report);
        Assert.Contains("noNetworkOrProviderImplementation: true", report);
        Assert.Contains("noScenePrefabSettingsChanges: true", report);
        Assert.DoesNotContain(repoRoot, report, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Goal102AUnityEditorSourceFormatGuardProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldUnityEditorSourceFormatGuardEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("GREEN", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(
            OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.FinalGate,
            result.Report.ManualGate);
        Assert.True(result.Report.QualityGatePassed);
        Assert.True(result.Report.SourceFormatBeforeAfterPassed);
        Assert.True(result.Report.NegativeProofPassed);
        Assert.True(result.Report.BeforeEditorWindowMalformedDetected);
        Assert.True(result.Report.AfterSourceFormatPassed);
        Assert.True(result.Report.AlphaRuntimeBootstrapUnchanged);

        var beforeAfterPath = Path.Combine(
            write.OutputDirectoryPath,
            OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.ScanBeforeAfterFileName);
        var qualityPath = Path.Combine(
            write.OutputDirectoryPath,
            OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.QualityGateFileName);
        var negativePath = Path.Combine(
            write.OutputDirectoryPath,
            OfflineGeoworldUnityEditorSourceFormatGuardVocabulary.NegativeProofFileName);

        Assert.True(File.Exists(beforeAfterPath));
        Assert.True(File.Exists(qualityPath));
        Assert.True(File.Exists(negativePath));

        using var beforeAfter = JsonDocument.Parse(await File.ReadAllTextAsync(beforeAfterPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(qualityPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(negativePath));

        Assert.True(beforeAfter.RootElement.GetProperty("beforeEditorWindowMalformedDetected").GetBoolean());
        Assert.True(beforeAfter.RootElement.GetProperty("afterEditorWindowRepaired").GetBoolean());
        Assert.True(beforeAfter.RootElement.GetProperty("alphaRuntimeBootstrap").GetProperty("unchanged").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("allScannedGoal102CSharpFilesPass").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("requiredGoal102SourceScopeScanned").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noForbiddenAreasChanged").GetBoolean());
        Assert.Equal(0, quality.RootElement.GetProperty("zeroLfSourceFileCount").GetInt32());
        Assert.Equal(0, quality.RootElement.GetProperty("crOnlySourceFileCount").GetInt32());
        Assert.Equal(0, quality.RootElement.GetProperty("rawPhysicalOneLineSourceFileCount").GetInt32());
        Assert.Equal(0, quality.RootElement.GetProperty("minifiedSourceFileCount").GetInt32());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(7, negative.RootElement.GetProperty("scenarioCount").GetInt32());

        var editorScriptPath = Path.Combine(
            repoRoot,
            "unity",
            "LLMGameCreatorAlpha",
            "Assets",
            "Editor",
            "OfflineGeoworldPreviewWindow.cs");
        var editorScriptBytes = await File.ReadAllBytesAsync(editorScriptPath);
        var editorScriptText = await File.ReadAllTextAsync(editorScriptPath);
        var lfCount = editorScriptBytes.Count(value => value == (byte)'\n');
        var maxPhysicalLineLength = editorScriptText.Split('\n').Max(line => line.Length);

        Assert.True(lfCount > 0);
        Assert.True(editorScriptText.Split('\n').Length > 1);
        Assert.True(maxPhysicalLineLength <= OfflineGeoworldUnityEditorSourceFormatGuardScanner.MaxAllowedPhysicalLineLength);

        var outputFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .ToArray();
        Assert.DoesNotContain(outputFiles, path => ForbiddenOutputExtensions.Contains(Path.GetExtension(path)));
        Assert.DoesNotContain(outputFiles, path => path.Contains("AlphaRuntimeBootstrap.cs", StringComparison.OrdinalIgnoreCase));

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("unity_editor_source_format_guard_verification required", report);
        Assert.Contains("beforeEditorWindowMalformedDetected: true", report);
        Assert.Contains("afterSourceFormatPassed: true", report);
        Assert.Contains("alphaRuntimeBootstrapUnchanged: true", report);
        Assert.DoesNotContain(repoRoot, report, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Goal102BActualUnityEditorSourceReformatProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldActualUnityEditorSourceReformatEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

        Assert.Equal("BLOCKED", result.Report.ImplementationStatus);
        Assert.False(result.Report.Accepted);
        Assert.Equal(
            OfflineGeoworldActualUnityEditorSourceReformatVocabulary.FinalGate,
            result.Report.ManualGate);
        Assert.False(result.Report.QualityGatePassed);
        Assert.True(result.Report.NegativeProofPassed);
        Assert.True(result.Report.TrustAuditPassed);
        Assert.True(result.Report.AlphaRuntimeBootstrapUnchanged);
        Assert.True(result.BeforeAfter.ActualHeadBeforeBlobRead);
        Assert.False(result.BeforeAfter.ActualHeadBeforeMalformedDetected);
        Assert.True(result.BeforeAfter.WorkingTreeSourceReadable);
        Assert.False(result.BeforeAfter.TargetFileChanged);
        Assert.True(result.TrustAudit.Goal102AEvidenceTrustDefectRecorded);
        Assert.True(result.TrustAudit.Goal102AEvidenceConflictsWithActualHead);
        Assert.True(result.QualityGate.NoForbiddenAreasChanged);
        Assert.Empty(result.QualityGate.ForbiddenChangedPaths);
        Assert.Contains(
            result.NegativeProof.Scenarios,
            item => item.ScenarioId == "before_scan_uses_only_synthetic_sample"
                    && item.ActualStatus == "rejected");
        Assert.Contains(
            result.NegativeProof.Scenarios,
            item => item.ScenarioId == "streamingassets_payload_changed_marker"
                    && item.ActualStatus == "rejected");

        foreach (var fileName in OfflineGeoworldActualUnityEditorSourceReformatVocabulary.RequiredEvidenceFileNames)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), fileName);
        }

        using var beforeAfter = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(
            write.OutputDirectoryPath,
            OfflineGeoworldActualUnityEditorSourceReformatVocabulary.BeforeAfterFileName)));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(
            write.OutputDirectoryPath,
            OfflineGeoworldActualUnityEditorSourceReformatVocabulary.QualityGateFileName)));
        using var trustAudit = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(
            write.OutputDirectoryPath,
            OfflineGeoworldActualUnityEditorSourceReformatVocabulary.TrustAuditFileName)));

        Assert.Equal("actual_git_head_blob", beforeAfter.RootElement.GetProperty("beforeSource").GetString());
        Assert.True(beforeAfter.RootElement.GetProperty("actualHeadBeforeBlobRead").GetBoolean());
        Assert.False(beforeAfter.RootElement.GetProperty("actualHeadBeforeMalformedDetected").GetBoolean());
        Assert.True(beforeAfter.RootElement.GetProperty("workingTreeSourceReadable").GetBoolean());
        Assert.Equal("BLOCKED", quality.RootElement.GetProperty("implementationStatus").GetString());
        Assert.False(quality.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(trustAudit.RootElement.GetProperty("goal102AEvidenceTrustDefectRecorded").GetBoolean());
        Assert.True(trustAudit.RootElement.GetProperty("supersededByGoal102B").GetBoolean());

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("actual_unity_editor_source_reformat_verification required", report);
        Assert.Contains("implementationStatus: BLOCKED", report);
        Assert.Contains("actualHeadBeforeMalformedDetected: false", report);
        Assert.Contains("goal102aEvidenceTrustDefectRecorded: true", report);
        Assert.DoesNotContain(repoRoot, report, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
