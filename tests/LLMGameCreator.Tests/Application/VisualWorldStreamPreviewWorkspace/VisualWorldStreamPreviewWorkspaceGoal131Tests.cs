using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal131Tests
{
    [Fact]
    public async Task Goal131GamePackageCandidateRecipePipelineSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new GamePackageCandidateRecipePipelineService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "gamepackage_candidate_recipe_catalog_scoring_and_promotion");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind == "gamepackage_candidate_recipe_pipeline_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysGamePackageCandidateRecipePipeline);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateRecipePipelineGroupPresent);
        Assert.Equal(
            GamePackageCandidateRecipePipelineVocabulary.RecipeCatalogRelativePath,
            workspace.QualityGateScan.GamePackageCandidateRecipePipelineRecipeCatalogPath);
        Assert.Equal(
            GamePackageCandidateRecipePipelineVocabulary.CandidateIndexRelativePath,
            workspace.QualityGateScan.GamePackageCandidateRecipePipelineCandidateIndexPath);
        Assert.Equal(
            GamePackageCandidateRecipePipelineVocabulary.NormalCommand,
            workspace.QualityGateScan.GamePackageCandidateRecipePipelineNormalCommand);
        Assert.Equal(
            GamePackageCandidateRecipePipelineVocabulary.PipelineResultRelativePath,
            workspace.QualityGateScan.GamePackageCandidateRecipePipelineResultPath);
        Assert.Equal(
            GamePackageCandidateRecipePipelineVocabulary.ScoringResultRelativePath,
            workspace.QualityGateScan.GamePackageCandidateRecipePipelineScoringResultPath);
        Assert.Equal(
            GamePackageCandidateRecipePipelineVocabulary.MatrixResultRelativePath,
            workspace.QualityGateScan.GamePackageCandidateRecipePipelineMatrixResultPath);
        Assert.Equal(
            GamePackageCandidateRecipePipelineVocabulary.SelectedCandidatePackageRelativePath,
            workspace.QualityGateScan.GamePackageCandidateRecipePipelineSelectedCandidatePackagePath);
        Assert.Equal(
            GamePackageCandidateRecipePipelineVocabulary.SelectedCandidateHandoffRelativePath,
            workspace.QualityGateScan.GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateRecipePipelineManualUnityOptional);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateRecipePipelineSamplePackageUnmodified);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateRecipePipelineProjectionOnly);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation);
        Assert.True(workspace.QualityGateScan.Goal131FilesDiscoveredByRelativePaths);
        Assert.Equal(
            workspace.QualityGateScan.GamePackageCandidateRecipePipelineStatus,
            summary.GamePackageCandidateRecipePipelineStatus);
        Assert.Equal(4, summary.GamePackageCandidateRecipePipelineRecipeCount);
        Assert.Equal(4, summary.GamePackageCandidateRecipePipelineCandidateCount);

        Assert.Contains(
            "recipePipelineStatus:",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "normalCommand: .devflow\\scripts\\run-gamepackage-candidate-recipe-pipeline.cmd",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "selectedCandidateId:",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);

        Assert.Equal("GREEN", workspace.QualityGateScan.GamePackageCandidateRecipePipelineStatus);
        Assert.Equal(4, workspace.QualityGateScan.GamePackageCandidateRecipePipelineRecipeCount);
        Assert.Equal(4, workspace.QualityGateScan.GamePackageCandidateRecipePipelineCandidateCount);
        Assert.Equal(4, workspace.QualityGateScan.GamePackageCandidateRecipePipelinePassedCandidates);
        Assert.Equal(0, workspace.QualityGateScan.GamePackageCandidateRecipePipelineFailedCandidates);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateRecipePipelineMatrixPassed);
        Assert.False(string.IsNullOrWhiteSpace(
            workspace.QualityGateScan.GamePackageCandidateRecipePipelineSelectedCandidateId));
        Assert.True(workspace.QualityGateScan.GamePackageCandidateRecipePipelineSelectedCandidateScore > 0);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateRecipePipelineQualityGatePassed);
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
