using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal119Tests
{
    [Fact]
    public void Goal119ProjectionSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        new AcceptedAlphaUnityPlayableProjectionService()
            .Build(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "accepted_alpha_unity_playable_projection");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind
                    == "accepted_alpha_unity_playable_projection_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaUnityPlayableProjection);
        Assert.True(workspace.QualityGateScan
            .AcceptedAlphaUnityPlayableProjectionGroupPresent);
        Assert.True(workspace.QualityGateScan
            .AcceptedAlphaUnityPlayableProjectionQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal119FilesDiscoveredByRelativePaths);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            summary.AcceptedAlphaUnityPlayableProjectionUnityMenuPath);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.GeneratedRootName,
            summary.AcceptedAlphaUnityPlayableProjectionGeneratedRootName);
        Assert.True(summary.AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady);
        Assert.True(summary.AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean);
        Assert.True(summary.AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically);
        Assert.Contains(
            "acceptedAlphaUnityPlayableProjectionUnityMenuPath: "
            + AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
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
