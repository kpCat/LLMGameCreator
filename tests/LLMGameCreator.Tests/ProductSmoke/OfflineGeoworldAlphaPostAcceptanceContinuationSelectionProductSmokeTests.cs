using LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldAlphaPostAcceptanceContinuationSelectionProductSmokeTests
{
    [Fact]
    public async Task ProductSmokeWritesContinuationArtifactsAndWorkspaceSurface()
    {
        var root = ProjectRoot();
        var write = await new OfflineGeoworldAlphaPostAcceptanceContinuationSelectionService()
            .BuildAndWriteAsync(root);

        Assert.Equal("GREEN", write.Result.QualityGateScan.ImplementationStatus);
        Assert.True(write.Result.QualityGateScan.Passed);
        Assert.Contains(write.WrittenFiles, path =>
            path == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.DashboardFileName);
        Assert.Contains(write.WrittenFiles, path =>
            path == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ExportPackageDirectory
            + "/"
            + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.MatrixFileName);
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.DoesNotContain(write.WrittenFiles, path =>
            path.StartsWith("unity/", StringComparison.Ordinal));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysOfflineGeoworldAlphaPostAcceptanceContinuationSelection);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "offline_geoworld_alpha_post_acceptance_continuation_selection");
        Assert.True(workspace.QualityGateScan
            .OfflineGeoworldAlphaPostAcceptanceQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal117FilesDiscoveredByRelativePaths);
        Assert.Contains(
            "offlineGeoworldAlphaPostAcceptanceRecommendedNextGoalId: "
            + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.RecommendedNextGoalId,
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
