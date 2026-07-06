using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualWorldStreamPreviewWorkspace;

public sealed class VisualWorldStreamPreviewWorkspaceGoal121Tests
{
    [Fact]
    public async Task Goal121InteractionDrilldownSurfacesInWorkspaceAndUiBinding()
    {
        var root = ProjectRoot();
        await new AcceptedAlphaInteractionDrilldownVerificationService()
            .BuildAndWriteAsync(root);

        var workspace = new VisualWorldStreamPreviewWorkspaceService()
            .Build(root);
        var group = Assert.Single(
            workspace.Catalog.Groups,
            item => item.GroupId == "accepted_alpha_interaction_drilldown_verification");
        var summary = Assert.Single(
            group.Entries,
            entry => entry.ArtifactKind == "accepted_alpha_interaction_drilldown_workspace_summary");

        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaInteractionDrilldown);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaInteractionDrilldownGroupPresent);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaInteractionDrilldownQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal121FilesDiscoveredByRelativePaths);
        Assert.Equal(
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.FullVerificationStatus,
            summary.AcceptedAlphaInteractionDrilldownFullVerificationStatus);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            summary.AcceptedAlphaInteractionDrilldownUnityMenuPath);
        Assert.True(summary.AcceptedAlphaInteractionDrilldownOneClickButtonPresent);
        Assert.True(summary.AcceptedAlphaInteractionDrilldownDrilldownFieldsPresent);
        Assert.True(summary.AcceptedAlphaInteractionDrilldownInteractionPreviewPresent);
        Assert.True(summary.AcceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent);
        Assert.Equal(
            "GOAL121_FULL_PROJECTION_VERIFICATION_PASS",
            summary.AcceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker);
        Assert.True(summary.AcceptedAlphaInteractionDrilldownCleanupScriptAvailable);
        Assert.True(summary.AcceptedAlphaInteractionDrilldownMaterialWarningGuardPresent);
        Assert.True(summary.AcceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton);
        Assert.Equal(
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory,
            summary.AcceptedAlphaInteractionDrilldownEvidencePath);
        Assert.Equal(
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ExportPackageDirectory,
            summary.AcceptedAlphaInteractionDrilldownExportPath);
        Assert.Contains(
            "acceptedAlphaInteractionDrilldownFullVerificationStatus: GREEN",
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
