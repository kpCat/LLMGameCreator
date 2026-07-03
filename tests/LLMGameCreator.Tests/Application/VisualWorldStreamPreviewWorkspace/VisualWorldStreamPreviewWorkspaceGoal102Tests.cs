using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceServiceTests
{
    [Fact]
    public void OfflineGeoworldUnityEditorPreviewGroupSurfacesGoal102EditorToolReadiness()
    {
        var result = Build();
        var editorGroup = Assert.Single(
            result.Catalog.Groups,
            group => group.GroupId == "offline_geoworld_unity_editor_preview");
        var summary = Assert.Single(
            editorGroup.Entries,
            entry => entry.ArtifactKind == "offline_geoworld_unity_editor_preview_workspace_summary");
        var scripts = editorGroup.Entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_unity_editor_preview_script")
            .ToArray();

        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewGroupPresent);
        Assert.Equal(18, result.QualityGateScan.OfflineGeoworldUnityEditorPreviewCommandCount);
        Assert.Equal(10, result.QualityGateScan.OfflineGeoworldUnityEditorPreviewCommandKindCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewTravelWindowStepCount >= 4);
        Assert.Equal(18, result.QualityGateScan.OfflineGeoworldUnityEditorPreviewExpectedObjectCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewToolInventoryPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewEditorWindowScriptReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewSimulatedActionProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewClearOperationProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldUnityEditorPreviewQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal102FilesDiscoveredByRelativePaths);
        Assert.Single(scripts);
        Assert.Equal(18, summary.OfflineGeoworldUnityEditorPreviewCommandCount);
        Assert.Equal(18, summary.OfflineGeoworldUnityEditorPreviewExpectedObjectCount);
        Assert.Contains(
            "OfflineGeoworldPreviewWindow.cs",
            summary.OfflineGeoworldUnityEditorPreviewEditorWindowScriptPath,
            StringComparison.Ordinal);
        Assert.Contains(
            "LLMGameCreator/Offline Geoworld Preview",
            summary.OfflineGeoworldUnityEditorPreviewMenuItemMarker,
            StringComparison.Ordinal);
        Assert.Contains(
            "OfflineGeoworldGoal101",
            summary.OfflineGeoworldUnityEditorPreviewPayloadPath,
            StringComparison.Ordinal);
        Assert.True(summary.OfflineGeoworldUnityEditorPreviewSimulatedActionProofPassed);
        Assert.True(summary.OfflineGeoworldUnityEditorPreviewClearOperationProofPassed);
        Assert.True(summary.OfflineGeoworldUnityEditorPreviewQualityGatePassed);
        Assert.All(editorGroup.Entries, entry =>
            Assert.False(Path.IsPathFullyQualified(entry.RelativePath), entry.RelativePath));
    }
}
