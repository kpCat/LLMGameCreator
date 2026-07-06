namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal125ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Generic GamePackage Inventory Resource Systems Loop",
            string.Empty,
            $"- genericSystemsStatus: {report.GenericSystemsStatus}",
            $"- samplePackagePath: {report.GenericSystemsSamplePackagePath}",
            $"- packageId: {report.GenericSystemsPackageId}",
            $"- recipePreviewPresent: {report.GenericSystemsRecipePreviewPresent.ToString().ToLowerInvariant()}",
            $"- recipeApplyPassed: {report.GenericSystemsRecipeApplyPassed.ToString().ToLowerInvariant()}",
            $"- harvestPreviewPresent: {report.GenericSystemsHarvestPreviewPresent.ToString().ToLowerInvariant()}",
            $"- harvestApplyPassed: {report.GenericSystemsHarvestApplyPassed.ToString().ToLowerInvariant()}",
            $"- transactionPreviewPresent: {report.GenericSystemsTransactionPreviewPresent.ToString().ToLowerInvariant()}",
            $"- encounterPreviewPresent: {report.GenericSystemsEncounterPreviewPresent.ToString().ToLowerInvariant()}",
            $"- combatRoundPreviewPresent: {report.GenericSystemsCombatRoundPreviewPresent.ToString().ToLowerInvariant()}",
            $"- inventorySummaryPresent: {report.GenericSystemsInventorySummaryPresent.ToString().ToLowerInvariant()}",
            $"- resourceSummaryPresent: {report.GenericSystemsResourceSummaryPresent.ToString().ToLowerInvariant()}",
            $"- systemsEventLogPresent: {report.GenericSystemsEventLogPresent.ToString().ToLowerInvariant()}",
            $"- unitySmokeStatus: {report.GenericSystemsUnitySmokeStatus}",
            $"- cleanupScriptAvailable: {report.GenericSystemsCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- cleanupCommand: {report.GenericSystemsCleanupCommand}",
            $"- goal124StillGreen: {report.GenericSystemsGoal124StillGreen.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.GenericSystemsProjectionOnly.ToString().ToLowerInvariant()}",
            $"- samplePackageReadOnly: {report.GenericSystemsSamplePackageReadOnly.ToString().ToLowerInvariant()}",
            $"- evidencePath: {report.GenericSystemsEvidencePath}",
            $"- exportPath: {report.GenericSystemsExportPath}",
            $"- noRuntimeProviderSchemaLuaGeneratorLibrary: {report.GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary.ToString().ToLowerInvariant()}",
            $"- noUnityScenePrefabSettingsPackagesStreamingAssets: {report.GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets.ToString().ToLowerInvariant()}",
            $"- genericGamePackageSystemsQualityGatePassed: {report.GenericGamePackageSystemsQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal125FilesDiscoveredByRelativePaths: {report.Goal125FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal125QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal125 Quality",
            string.Empty,
            $"- genericGamePackageSystemsGroupPresent: {qualityGate.GenericGamePackageSystemsGroupPresent.ToString().ToLowerInvariant()}",
            $"- genericSystemsStatus: {qualityGate.GenericSystemsStatus}",
            $"- samplePackagePath: {qualityGate.GenericSystemsSamplePackagePath}",
            $"- packageId: {qualityGate.GenericSystemsPackageId}",
            $"- recipePreviewPresent: {qualityGate.GenericSystemsRecipePreviewPresent.ToString().ToLowerInvariant()}",
            $"- recipeApplyPassed: {qualityGate.GenericSystemsRecipeApplyPassed.ToString().ToLowerInvariant()}",
            $"- harvestPreviewPresent: {qualityGate.GenericSystemsHarvestPreviewPresent.ToString().ToLowerInvariant()}",
            $"- harvestApplyPassed: {qualityGate.GenericSystemsHarvestApplyPassed.ToString().ToLowerInvariant()}",
            $"- transactionPreviewPresent: {qualityGate.GenericSystemsTransactionPreviewPresent.ToString().ToLowerInvariant()}",
            $"- encounterPreviewPresent: {qualityGate.GenericSystemsEncounterPreviewPresent.ToString().ToLowerInvariant()}",
            $"- combatRoundPreviewPresent: {qualityGate.GenericSystemsCombatRoundPreviewPresent.ToString().ToLowerInvariant()}",
            $"- inventorySummaryPresent: {qualityGate.GenericSystemsInventorySummaryPresent.ToString().ToLowerInvariant()}",
            $"- resourceSummaryPresent: {qualityGate.GenericSystemsResourceSummaryPresent.ToString().ToLowerInvariant()}",
            $"- systemsEventLogPresent: {qualityGate.GenericSystemsEventLogPresent.ToString().ToLowerInvariant()}",
            $"- unitySmokeStatus: {qualityGate.GenericSystemsUnitySmokeStatus}",
            $"- cleanupScriptAvailable: {qualityGate.GenericSystemsCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- cleanupCommand: {qualityGate.GenericSystemsCleanupCommand}",
            $"- goal124StillGreen: {qualityGate.GenericSystemsGoal124StillGreen.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.GenericSystemsProjectionOnly.ToString().ToLowerInvariant()}",
            $"- samplePackageReadOnly: {qualityGate.GenericSystemsSamplePackageReadOnly.ToString().ToLowerInvariant()}",
            $"- genericGamePackageSystemsQualityGatePassed: {qualityGate.GenericGamePackageSystemsQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal125FilesDiscoveredByRelativePaths: {qualityGate.Goal125FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsGenericGamePackageSystemsBindingReal: {qualityGate.WinFormsGenericGamePackageSystemsBindingReal.ToString().ToLowerInvariant()}"
        ]);
}
