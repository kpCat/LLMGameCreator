using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal122Tests
{
    [Fact]
    public async Task Goal122ProjectionActionLoopSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new AcceptedAlphaProjectionActionLoopService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService()
            .Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "accepted_alpha_projection_action_loop");
        var summary = Assert.Single(
            group.Entries,
            entry => entry.ArtifactKind == "accepted_alpha_projection_action_loop_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaProjectionActionLoop);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaProjectionActionLoopGroupPresent);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaProjectionActionLoopQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal122FilesDiscoveredByRelativePaths);
        Assert.Equal("GREEN", summary.AcceptedAlphaProjectionActionLoopStatus);
        Assert.Equal("GREEN", summary.AcceptedAlphaProjectionActionLoopWindowPolishStatus);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            summary.AcceptedAlphaProjectionActionLoopUnityMenuPath);
        Assert.True(summary.AcceptedAlphaProjectionActionLoopOneClickVerificationStillPresent);
        Assert.True(summary.AcceptedAlphaProjectionActionLoopProjectionActionPreviewPresent);
        Assert.True(summary.AcceptedAlphaProjectionActionLoopProjectionActionApplyPresent);
        Assert.True(summary.AcceptedAlphaProjectionActionLoopProjectionStateResetPresent);
        Assert.True(summary.AcceptedAlphaProjectionActionLoopWindowLayoutPolishPresent);
        Assert.True(summary.AcceptedAlphaProjectionActionLoopCleanupScriptAvailable);
        Assert.True(summary.AcceptedAlphaProjectionActionLoopDoNotStartAutomatically);
        Assert.Equal(
            AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory,
            summary.AcceptedAlphaProjectionActionLoopEvidencePath);
        Assert.Equal(
            AcceptedAlphaProjectionActionLoopVocabulary.ExportPackageDirectory,
            summary.AcceptedAlphaProjectionActionLoopExportPath);
        Assert.Contains(
            "acceptedAlphaProjectionActionLoopStatus: GREEN",
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
