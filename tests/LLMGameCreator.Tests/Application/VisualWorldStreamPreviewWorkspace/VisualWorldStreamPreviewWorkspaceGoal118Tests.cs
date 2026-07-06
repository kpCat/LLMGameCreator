using LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal118Tests
{
    [Fact]
    public void Goal118AcceptedBaselineSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        new OfflineGeoworldAcceptedAlphaBaselineReviewService()
            .Build(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "offline_geoworld_accepted_alpha_baseline_review");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind
                    == "offline_geoworld_accepted_alpha_baseline_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysOfflineGeoworldAcceptedAlphaBaselineReview);
        Assert.True(workspace.QualityGateScan
            .OfflineGeoworldAcceptedAlphaBaselineGroupPresent);
        Assert.True(workspace.QualityGateScan
            .OfflineGeoworldAcceptedAlphaQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal118FilesDiscoveredByRelativePaths);
        Assert.Equal(
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.BaselineId,
            summary.OfflineGeoworldAcceptedAlphaBaselineId);
        Assert.True(summary.OfflineGeoworldAcceptedAlphaBaselineReady);
        Assert.Equal(
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.RecommendedNextDecision,
            summary.OfflineGeoworldAcceptedAlphaRecommendedNextDecision);
        Assert.Equal(
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceGoalIds.Count,
            summary.OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount);
        Assert.True(summary.OfflineGeoworldAcceptedAlphaDoNotStartAutomatically);
        Assert.Contains(
            "offlineGeoworldAcceptedAlphaBaselineId: "
            + OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.BaselineId,
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
