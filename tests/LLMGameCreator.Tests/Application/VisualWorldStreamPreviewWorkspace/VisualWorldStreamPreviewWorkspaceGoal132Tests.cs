using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal132Tests
{
    [Fact]
    public async Task Goal132CandidatePipelineOperatorSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new GamePackageCandidateRecipePipelineService()
            .BuildAndWriteAsync(root);
        await new GamePackageCandidatePipelineOperatorService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(workspace.Catalog.Groups, item =>
            item.GroupId == "candidate_pipeline_operator_panel");
        var summary = Assert.Single(group.Entries, entry =>
            entry.ArtifactKind == "candidate_pipeline_operator_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory.PageBindDisplaysCandidatePipelineOperator);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorGroupPresent);
        Assert.Equal(
            GamePackageCandidatePipelineOperatorVocabulary.NormalCommand,
            workspace.QualityGateScan.CandidatePipelineOperatorNormalCommand);
        Assert.Equal(
            GamePackageCandidatePipelineOperatorVocabulary.DryRunCommand,
            workspace.QualityGateScan.CandidatePipelineOperatorDryRunCommand);
        Assert.Equal(
            GamePackageCandidatePipelineOperatorVocabulary.Goal131ResultPath,
            workspace.QualityGateScan.CandidatePipelineOperatorResultPath);
        Assert.False(string.IsNullOrWhiteSpace(
            workspace.QualityGateScan.CandidatePipelineOperatorSelectedCandidateId));
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorSelectedCandidateScore > 0);
        Assert.Equal(4, workspace.QualityGateScan.CandidatePipelineOperatorCandidateCount);
        Assert.Equal(4, workspace.QualityGateScan.CandidatePipelineOperatorPassedCandidates);
        Assert.Equal(0, workspace.QualityGateScan.CandidatePipelineOperatorFailedCandidates);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorMatrixPassed);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorManualUnityOptional);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorProjectionOnly);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorSamplePackageReadOnly);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorWinFormsPanelPresent);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorRefreshButtonPresent);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorCopyCommandButtonPresent);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorDryRunButtonPresent);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorRunButtonPresent);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorAsyncRunPresent);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorResultPresent);
        Assert.True(workspace.QualityGateScan.Goal132FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.CandidatePipelineOperatorQualityGatePassed);
        Assert.Equal(
            workspace.QualityGateScan.CandidatePipelineOperatorStatus,
            summary.CandidatePipelineOperatorStatus);

        Assert.Contains(
            "candidatePipelineOperatorStatus: GREEN_READY",
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
