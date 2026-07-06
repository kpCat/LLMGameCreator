using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal120Tests
{
    [Fact]
    public void Goal120UsabilitySurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        new AcceptedAlphaProjectionUsabilityService()
            .Build(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "accepted_alpha_projection_usability");
        var summary = Assert.Single(
            group.Entries,
            item => item.ArtifactKind
                    == "accepted_alpha_projection_usability_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaProjectionUsability);
        Assert.True(workspace.QualityGateScan
            .AcceptedAlphaProjectionUsabilityGroupPresent);
        Assert.True(workspace.QualityGateScan
            .AcceptedAlphaProjectionUsabilityQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal120FilesDiscoveredByRelativePaths);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            summary.AcceptedAlphaProjectionUsabilityUnityMenuPath);
        Assert.Equal(
            AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
            summary.AcceptedAlphaProjectionUsabilityCleanupScriptPath);
        Assert.Equal(
            AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath,
            summary.AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath);
        Assert.True(summary.AcceptedAlphaProjectionUsabilityLegendPresent);
        Assert.True(summary.AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent);
        Assert.True(summary.AcceptedAlphaProjectionUsabilitySelectionControlsPresent);
        Assert.True(summary.AcceptedAlphaProjectionUsabilityFocusCameraControlPresent);
        Assert.True(summary.AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent);
        Assert.True(summary.AcceptedAlphaProjectionUsabilityDoNotStartAutomatically);
        Assert.Contains(
            "acceptedAlphaProjectionUsabilityUnityMenuPath: "
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
