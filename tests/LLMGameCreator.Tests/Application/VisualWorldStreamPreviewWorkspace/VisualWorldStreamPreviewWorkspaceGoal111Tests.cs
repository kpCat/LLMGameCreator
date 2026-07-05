using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal111Tests
{
    [Fact]
    public async Task Goal111ManualResultIntakeGroupSurfacesPendingDecision()
    {
        var root = ProjectRoot();
        await new OfflineGeoworldAlphaManualResultIntakeService().BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_manual_result_intake");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_alpha_manual_result_intake_workspace_summary");

        Assert.True(summary.OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent);
        Assert.False(summary.OfflineGeoworldAlphaManualResultIntakeResultFilePresent);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            summary.OfflineGeoworldAlphaManualResultIntakeDecisionStatus);
        Assert.False(summary.OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex);
        Assert.True(summary.OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualResultIntakeGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualResultIntakeQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal111FilesDiscoveredByRelativePaths);
        Assert.Contains(
            "offlineGeoworldAlphaManualResultIntakeDecisionStatus: BLOCKED_PENDING_MANUAL_RESULT",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
