using LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal117Tests
{
    [Fact]
    public async Task Goal117ContinuationSelectionSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId
                    == "offline_geoworld_alpha_post_acceptance_continuation_selection");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind
                    == "offline_geoworld_alpha_post_acceptance_continuation_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysOfflineGeoworldAlphaPostAcceptanceContinuationSelection);
        Assert.True(workspace.QualityGateScan
            .OfflineGeoworldAlphaPostAcceptanceContinuationGroupPresent);
        Assert.True(workspace.QualityGateScan
            .OfflineGeoworldAlphaPostAcceptanceQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal117FilesDiscoveredByRelativePaths);
        Assert.True(summary.OfflineGeoworldAlphaPostAcceptanceHumanAccepted);
        Assert.Equal(
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .LaneAcceptedAlphaBaselineReview,
            summary.OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane);
        Assert.Equal(
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.RecommendedNextGoalId,
            summary.OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId);
        Assert.True(summary.OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically);
        Assert.Equal(1, summary.OfflineGeoworldAlphaPostAcceptanceReadyLaneCount);
        Assert.Equal(3, summary.OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount);
        Assert.Equal(3, summary.OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount);
        Assert.Contains(
            "offlineGeoworldAlphaPostAcceptanceRecommendedNextLane: "
            + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .LaneAcceptedAlphaBaselineReview,
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
