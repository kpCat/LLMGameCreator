using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal129Tests
{
    [Fact]
    public async Task Goal129GamePackageCandidateMatrixSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new GamePackageCandidateMatrixProjectionService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "gamepackage_candidate_matrix_projection_runner");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind == "gamepackage_candidate_matrix_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysGamePackageCandidateMatrix);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixGroupPresent);
        Assert.Equal(
            GamePackageCandidateMatrixProjectionVocabulary.CandidateIndexRelativePath,
            workspace.QualityGateScan.GamePackageCandidateMatrixCandidateIndexPath);
        Assert.Equal(
            GamePackageCandidateMatrixProjectionVocabulary.MatrixResultRelativePath,
            workspace.QualityGateScan.GamePackageCandidateMatrixResultPath);
        Assert.Equal(
            GamePackageCandidateMatrixProjectionVocabulary.NormalCommand,
            workspace.QualityGateScan.GamePackageCandidateMatrixNormalCommand);
        Assert.Equal(
            GamePackageCandidateMatrixProjectionVocabulary.BaselineCandidatePackagePath,
            workspace.QualityGateScan.GamePackageCandidateMatrixBaselineCandidatePackagePath);
        Assert.Equal(
            GamePackageCandidateMatrixProjectionVocabulary.VariantCandidatePackagePath,
            workspace.QualityGateScan.GamePackageCandidateMatrixVariantCandidatePackagePath);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixManualUnityOptional);
        Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixProjectionOnly);
        Assert.True(workspace.QualityGateScan.Goal129FilesDiscoveredByRelativePaths);
        Assert.Equal(
            workspace.QualityGateScan.GamePackageCandidateMatrixStatus,
            summary.GamePackageCandidateMatrixStatus);
        Assert.True(summary.GamePackageCandidateMatrixCandidateCount >= 2);

        Assert.Contains(
            "gamePackageCandidateMatrixStatus:",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "normalCommand: .devflow\\scripts\\run-gamepackage-projection-matrix.cmd",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);

        if (workspace.QualityGateScan.GamePackageCandidateMatrixStatus == "GREEN")
        {
            Assert.Equal(2, workspace.QualityGateScan.GamePackageCandidateMatrixCandidateCount);
            Assert.Equal(2, workspace.QualityGateScan.GamePackageCandidateMatrixPassedCandidateCount);
            Assert.Equal(0, workspace.QualityGateScan.GamePackageCandidateMatrixFailedCandidateCount);
            Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixCleanupApplied);
            Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixScriptScanPassed);
            Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixResultPassed);
            Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixLogScanPassed);
            Assert.True(workspace.QualityGateScan.GamePackageCandidateMatrixQualityGatePassed);
        }
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
