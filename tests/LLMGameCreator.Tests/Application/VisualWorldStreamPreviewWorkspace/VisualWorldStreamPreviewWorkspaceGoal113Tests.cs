using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal113Tests
{
    [Fact]
    public async Task Goal113WorkbenchGroupSurfacesPendingHumanResult()
    {
        var root = ProjectRoot();
        await new OfflineGeoworldAlphaManualResultWorkbenchService().BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_alpha_manual_result_workbench");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind == "offline_geoworld_alpha_manual_result_workbench_workspace_summary");

        Assert.Equal(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.WorkbenchStatusReadyPendingHumanResult,
            summary.OfflineGeoworldAlphaManualResultWorkbenchStatus);
        Assert.Equal(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionStatusPending,
            summary.OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus);
        Assert.False(summary.OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent);
        Assert.False(summary.OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex);
        Assert.True(summary.OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired);
        Assert.True(summary.OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualResultWorkbenchGroupPresent);
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAlphaManualResultWorkbenchQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal113FilesDiscoveredByRelativePaths);
        Assert.Contains(
            "offlineGeoworldAlphaManualResultWorkbenchStatus: WORKBENCH_READY_PENDING_HUMAN_RESULT",
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
