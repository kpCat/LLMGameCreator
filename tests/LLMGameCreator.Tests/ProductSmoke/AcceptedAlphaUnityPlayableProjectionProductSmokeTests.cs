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
        var actionLoop = await new AcceptedAlphaProjectionActionLoopService()
            .BuildAndWriteAsync(root);
        var genericProjection = await new GenericGamePackageProjectionService()
            .BuildAndWriteAsync(root);
        var genericLoop = await new GenericGamePackageLoopProjectionService()
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
        Assert.Equal("GREEN", actionLoop.Result.Dashboard.ActionLoopStatus);
        Assert.Equal("GREEN", actionLoop.Result.Dashboard.WindowPolishStatus);
        Assert.True(actionLoop.Result.Dashboard.OneClickVerificationStillPresent);
        Assert.True(actionLoop.Result.Dashboard.ProjectionActionPreviewPresent);
        Assert.True(actionLoop.Result.Dashboard.ProjectionActionApplyPresent);
        Assert.True(actionLoop.Result.Dashboard.ProjectionStateResetPresent);
        Assert.True(actionLoop.Result.Dashboard.WindowLayoutPolishPresent);
        Assert.True(actionLoop.Result.ScriptInventory.Passed);
        Assert.True(actionLoop.Result.NegativeProof.Passed);
        Assert.Contains(actionLoop.WrittenFiles, path =>
            path == AcceptedAlphaProjectionActionLoopVocabulary
                .ProceduralOutputDirectory
            + "/"
            + AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName);
        Assert.Contains(actionLoop.WrittenFiles, path =>
            path == AcceptedAlphaProjectionActionLoopVocabulary
                .ExportPackageDirectory
            + "/"
            + AcceptedAlphaProjectionActionLoopVocabulary.LogScanFileName);
        Assert.Contains(actionLoop.WrittenFiles, path =>
            path == AcceptedAlphaProjectionActionLoopVocabulary.DocumentationPath);
        Assert.DoesNotContain(actionLoop.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", genericProjection.Result.Dashboard.GenericProjectionStatus);
        Assert.Equal("game/minimal-map-game", genericProjection.Result.Dashboard.PackageId);
        Assert.Equal("Minimal Map Game", genericProjection.Result.Dashboard.PackageTitle);
        Assert.Equal("map/village", genericProjection.Result.Dashboard.MapId);
        Assert.True(genericProjection.Result.Dashboard.EntityCount >= 2);
        Assert.True(genericProjection.Result.Dashboard.ItemCount >= 1);
        Assert.True(genericProjection.Result.Dashboard.Goal122StillGreen);
        Assert.True(genericProjection.Result.ScriptInventory.Passed);
        Assert.True(genericProjection.Result.SamplePackage.Passed);
        Assert.True(genericProjection.Result.NegativeProof.Passed);
        Assert.Contains(genericProjection.WrittenFiles, path =>
            path == GenericGamePackageProjectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + GenericGamePackageProjectionVocabulary.DashboardFileName);
        Assert.Contains(genericProjection.WrittenFiles, path =>
            path == GenericGamePackageProjectionVocabulary
                .ExportPackageDirectory
            + "/"
            + GenericGamePackageProjectionVocabulary.LogScanFileName);
        Assert.Contains(genericProjection.WrittenFiles, path =>
            path == GenericGamePackageProjectionVocabulary.DocumentationPath);
        Assert.DoesNotContain(genericProjection.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));
        Assert.Equal("GREEN", genericLoop.Result.Dashboard.GenericLoopStatus);
        Assert.Equal("game/minimal-map-game", genericLoop.Result.Dashboard.PackageId);
        Assert.Equal("map/village", genericLoop.Result.Dashboard.MapId);
        Assert.True(genericLoop.Result.Dashboard.InteractionPreviewPresent);
        Assert.True(genericLoop.Result.Dashboard.InteractionApplyPassed);
        Assert.True(genericLoop.Result.Dashboard.DialogueSummaryPresent);
        Assert.True(genericLoop.Result.Dashboard.QuestObjectiveSummaryPresent);
        Assert.True(genericLoop.Result.Dashboard.InventorySummaryPresent);
        Assert.True(genericLoop.Result.Dashboard.ResourceSummaryPresent);
        Assert.True(genericLoop.Result.Dashboard.Goal123StillGreen);
        Assert.True(genericLoop.Result.ScriptInventory.Passed);
        Assert.True(genericLoop.Result.SamplePackage.Passed);
        Assert.True(genericLoop.Result.NegativeProof.Passed);
        Assert.Contains(genericLoop.WrittenFiles, path =>
            path == GenericGamePackageLoopProjectionVocabulary
                .ProceduralOutputDirectory
            + "/"
            + GenericGamePackageLoopProjectionVocabulary.DashboardFileName);
        Assert.Contains(genericLoop.WrittenFiles, path =>
            path == GenericGamePackageLoopProjectionVocabulary
                .ExportPackageDirectory
            + "/"
            + GenericGamePackageLoopProjectionVocabulary.LogScanFileName);
        Assert.Contains(genericLoop.WrittenFiles, path =>
            path == GenericGamePackageLoopProjectionVocabulary.DocumentationPath);
        Assert.DoesNotContain(genericLoop.WrittenFiles, path =>
            path.StartsWith(".llmgc/manual/", StringComparison.Ordinal));

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(root);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaUnityPlayableProjection);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaProjectionUsability);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaInteractionDrilldown);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysAcceptedAlphaProjectionActionLoop);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGenericGamePackageProjection);
        Assert.True(workspace.WinFormsBindingInventory
            .PageBindDisplaysGenericGamePackageLoop);
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_unity_playable_projection");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_projection_usability");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_interaction_drilldown_verification");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "accepted_alpha_projection_action_loop");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "generic_gamepackage_projection");
        Assert.Contains(workspace.Catalog.Groups, group =>
            group.GroupId == "generic_gamepackage_loop");
        Assert.True(workspace.QualityGateScan.AcceptedAlphaUnityPlayableProjectionQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal119FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaProjectionUsabilityQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal120FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaInteractionDrilldownQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal121FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.AcceptedAlphaProjectionActionLoopQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal122FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.GenericGamePackageProjectionQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal123FilesDiscoveredByRelativePaths);
        Assert.True(workspace.QualityGateScan.GenericGamePackageLoopQualityGatePassed);
        Assert.True(workspace.QualityGateScan.Goal124FilesDiscoveredByRelativePaths);
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
        Assert.Contains(
            "acceptedAlphaProjectionActionLoopStatus: GREEN",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "genericProjectionStatus: GREEN",
            workspace.ReportMarkdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "genericLoopStatus: GREEN",
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
