using LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldAcceptedAlphaBaselineReviewProductSmokeTests
{
    [Fact]
    public async Task ProductSmokeWritesBaselineArtifactsAndWorkspaceSurface()
    {
        var root = ProjectRoot();
        var write = await new OfflineGeoworldAcceptedAlphaBaselineReviewService()
            .BuildAndWriteAsync(root);

        Assert.Equal("GREEN", write.Result.QualityGateScan.ImplementationStatus);
        Assert.True(write.Result.QualityGateScan.Passed);
        Assert.True(write.Result.Dashboard.AcceptedBaselineReady);
        Assert.Equal(
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.BaselineId,
            write.Result.Dashboard.BaselineId);
        Assert.Contains(write.WrittenFiles, path =>
            path == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary
                .ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.DashboardFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary
                .ExportPackageDirectory
            + "/"
            + OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ManifestFileName);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith("unity/", StringComparison.Ordinal));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysOfflineGeoworldAcceptedAlphaBaselineReview);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "offline_geoworld_accepted_alpha_baseline_review");
        Assert.True(workspace.QualityGateScan.OfflineGeoworldAcceptedAlphaQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal118FilesDiscoveredByRelativePaths);
        Assert.Contains(
            "offlineGeoworldAcceptedAlphaRecommendedNextDecision: "
            + OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.RecommendedNextDecision,
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
