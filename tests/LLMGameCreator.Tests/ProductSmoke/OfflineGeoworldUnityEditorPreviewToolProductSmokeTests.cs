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
        var repoRoot = FindRepoRoot();
        var write = await new OfflineGeoworldUnityEditorPreviewToolEvidenceService()
            .BuildAndWriteAsync(repoRoot);
        var result = write.Result;

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

        using var inventory = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.OutputDirectoryPath,
                OfflineGeoworldUnityEditorPreviewToolVocabulary.ToolInventoryFileName)));
        using var proof = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.OutputDirectoryPath,
                OfflineGeoworldUnityEditorPreviewToolVocabulary.SimulatedActionProofFileName)));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(write.OutputDirectoryPath,
                OfflineGeoworldUnityEditorPreviewToolVocabulary.QualityGateScanFileName)));

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

        var outputFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .ToArray();
        Assert.DoesNotContain(outputFiles, path => ForbiddenOutputExtensions.Contains(Path.GetExtension(path)));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.StartsWith("src/LLMGameCreator.Runtime", StringComparison.Ordinal));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.StartsWith("src/LLMGameCreator.GamePackage", StringComparison.Ordinal));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.QualityGateScan.ExpectedChangedPathPrefixes, item =>
            item.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
            || item.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase));

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("offline_geoworld_unity_editor_preview_tool_verification required", report);
        Assert.Contains("commandCount: 18", report);
        Assert.Contains("expectedObjectCount: 18", report);
        Assert.Contains("editorWindowScriptReady: true", report);
        Assert.Contains("clearOperationProofPassed: true", report);
        Assert.Contains("noNetworkOrProviderImplementation: true", report);
        Assert.Contains("noScenePrefabSettingsChanges: true", report);
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
