using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceServiceTests
{
    [Fact]
    public void OfflineGeoworldPlayModeTravelGroupSurfacesGoal103Readiness()
    {
        var result = Build();
        var playModeGroup = Assert.Single(
            result.Catalog.Groups,
            group => group.GroupId == "offline_geoworld_playmode_travel");
        var summary = Assert.Single(
            playModeGroup.Entries,
            entry => entry.ArtifactKind == "offline_geoworld_playmode_travel_workspace_summary");
        var scripts = playModeGroup.Entries
            .Where(entry => entry.ArtifactKind == "offline_geoworld_playmode_unity_script")
            .ToArray();

        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelGroupPresent);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelStepCount >= 4);
        Assert.Equal(18, result.QualityGateScan.OfflineGeoworldPlayModeTravelObjectCount);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelUnityScriptsReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelEditorWindowReady);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelSimulatedExecutionProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelNegativeProofPassed);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelGoal102BClosureRecorded);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelAlphaRuntimeBootstrapUnchanged);
        Assert.True(result.QualityGateScan.OfflineGeoworldPlayModeTravelQualityGatePassed);
        Assert.True(result.QualityGateScan.Goal103FilesDiscoveredByRelativePaths);
        Assert.Equal(3, scripts.Length);
        Assert.Equal(18, summary.OfflineGeoworldPlayModeTravelObjectCount);
        Assert.Contains(
            "0:",
            summary.OfflineGeoworldPlayModeTravelBoundaryPrefetchCounts,
            StringComparison.Ordinal);
        Assert.True(summary.OfflineGeoworldPlayModeTravelSimulatedExecutionProofPassed);
        Assert.True(summary.OfflineGeoworldPlayModeTravelGoal102BClosureRecorded);
        Assert.All(playModeGroup.Entries, entry =>
            Assert.False(Path.IsPathFullyQualified(entry.RelativePath), entry.RelativePath));
    }
}
