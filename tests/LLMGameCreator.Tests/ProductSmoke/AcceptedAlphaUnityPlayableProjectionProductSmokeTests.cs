using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class AcceptedAlphaUnityPlayableProjectionProductSmokeTests
{
    [Fact]
    public async Task ProductSmokeWritesProjectionArtifactsAndWorkspaceSurface()
    {
        var root = ProjectRoot();
        var projection = new AcceptedAlphaUnityPlayableProjectionService()
            .Build(root);
        var hotfix = new AcceptedAlphaUnityMaterialWarningHotfixService()
            .Build(root);
        var usability = new AcceptedAlphaProjectionUsabilityService()
            .Build(root);
        var drilldown = await new AcceptedAlphaInteractionDrilldownVerificationService()
            .BuildAndWriteAsync(root);

        Assert.Equal("GREEN", projection.QualityGateScan.ImplementationStatus);
        Assert.True(projection.QualityGateScan.Passed);
        Assert.True(projection.Dashboard.AcceptedBaselineReady);
        Assert.Equal(
            AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            projection.Dashboard.UnityMenuPath);
        Assert.True(hotfix.ScriptScan.Passed);
        Assert.True(hotfix.Dashboard.RendererMaterialSourceAccessAbsent);
        Assert.True(hotfix.Dashboard.MaterialAssignmentSourceAccessAbsent);
        Assert.True(hotfix.Dashboard.MaterialPropertyBlockUsed);
        Assert.True(hotfix.NegativeProof.Passed);
        Assert.Equal("GREEN", usability.Dashboard.UsabilityStatus);
        Assert.True(usability.Dashboard.LegendPresent);
        Assert.True(usability.Dashboard.MarkerDescriptorPresent);
        Assert.True(usability.Dashboard.SelectionControlsPresent);
        Assert.True(usability.Dashboard.FocusCameraControlPresent);
        Assert.True(usability.Dashboard.CleanupScriptContractPassed);
        Assert.Equal("GREEN", drilldown.Result.Dashboard.FullVerificationStatus);
        Assert.True(drilldown.Result.Dashboard.OneClickButtonPresent);
        Assert.True(drilldown.Result.Dashboard.DrilldownFieldsPresent);
        Assert.True(drilldown.Result.Dashboard.InteractionPreviewPresent);
        Assert.True(drilldown.Result.Dashboard.ObjectiveReplayDetailsPresent);
        Assert.True(drilldown.Result.Dashboard.CleanupScriptAvailable);
        Assert.True(drilldown.Result.Dashboard.MaterialWarningGuardPresent);
        Assert.True(drilldown.Result.Dashboard.HumanManualStepsReducedToOneButton);
        Assert.True(drilldown.Result.ScriptInventory.Passed);
        Assert.True(drilldown.Result.NegativeProof.Passed);
        Assert.Contains(drilldown.WrittenFiles, path =>
            path == AcceptedAlphaInteractionDrilldownVerificationVocabulary
                .ProceduralOutputDirectory
            + "/"
            + AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName);
        Assert.Contains(drilldown.WrittenFiles, path =>
            path == AcceptedAlphaInteractionDrilldownVerificationVocabulary
                .ExportPackageDirectory
            + "/"
            + AcceptedAlphaInteractionDrilldownVerificationVocabulary.LogScanFileName);
        Assert.Contains(drilldown.WrittenFiles, path =>
            path == AcceptedAlphaInteractionDrilldownVerificationVocabulary.DocumentationPath);
        Assert.DoesNotContain(drilldown.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaUnityPlayableProjection);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaProjectionUsability);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaInteractionDrilldown);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_unity_playable_projection");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_projection_usability");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_interaction_drilldown_verification");
        Assert.True(workspace.QualityGateScan.AcceptedAlphaUnityPlayableProjectionQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal119FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaProjectionUsabilityQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal120FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaInteractionDrilldownQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal121FilesDiscoveredByRelativePaths);
        Assert.Contains(
            "acceptedAlphaUnityPlayableProjectionGeneratedRootName: "
            + AcceptedAlphaUnityPlayableProjectionVocabulary.GeneratedRootName,
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "acceptedAlphaProjectionUsabilityCleanupScriptPath: "
            + AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "acceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton: true",
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
