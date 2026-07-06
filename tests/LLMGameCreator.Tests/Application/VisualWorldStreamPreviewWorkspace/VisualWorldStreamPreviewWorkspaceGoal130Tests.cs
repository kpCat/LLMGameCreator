using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal130Tests
{
    [Fact]
    public async Task Goal130GamePackageCandidateFactorySurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new GamePackageCandidateFactoryProjectionService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "gamepackage_candidate_factory_and_matrix_pipeline");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind == "gamepackage_candidate_factory_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysGamePackageCandidateFactory);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateFactoryGroupPresent);
        Assert.Equal(
            GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexRelativePath,
            workspace.QualityGateScan.GamePackageCandidateFactoryCandidateIndexPath);
        Assert.Equal(
            GamePackageCandidateFactoryProjectionVocabulary.NormalCommand,
            workspace.QualityGateScan.GamePackageCandidateFactoryNormalCommand);
        Assert.Equal(
            GamePackageCandidateFactoryProjectionVocabulary.FactoryResultRelativePath,
            workspace.QualityGateScan.GamePackageCandidateFactoryResultPath);
        Assert.Equal(
            GamePackageCandidateFactoryProjectionVocabulary.MatrixResultRelativePath,
            workspace.QualityGateScan.GamePackageCandidateFactoryMatrixResultPath);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateFactoryManualUnityOptional);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateFactorySamplePackageUnmodified);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateFactoryProjectionOnly);
        Assert.True(workspace.QualityGateScan.Goal130FilesDiscoveredByRelativePaths);
        Assert.Equal(
            workspace.QualityGateScan.GamePackageCandidateFactoryStatus,
            summary.GamePackageCandidateFactoryStatus);
        Assert.Equal(3, summary.GamePackageCandidateFactoryCandidateCount);

        Assert.Contains(
            "candidateFactoryStatus:",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "normalCommand: .devflow\\scripts\\run-gamepackage-candidate-factory.cmd",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);

        Assert.Equal("GREEN", workspace.QualityGateScan.GamePackageCandidateFactoryStatus);
        Assert.Equal(3, workspace.QualityGateScan.GamePackageCandidateFactoryCandidateCount);
        Assert.Equal(3, workspace.QualityGateScan.GamePackageCandidateFactoryPassedCandidates);
        Assert.Equal(0, workspace.QualityGateScan.GamePackageCandidateFactoryFailedCandidates);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateFactoryMatrixPassed);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateFactoryQualityGatePassed);
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
