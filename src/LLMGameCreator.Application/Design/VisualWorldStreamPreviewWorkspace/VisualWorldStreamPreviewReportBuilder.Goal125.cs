namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal125ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            GenericSystemsStatus = qualityGate.GenericSystemsStatus,
            GenericSystemsSamplePackagePath = qualityGate.GenericSystemsSamplePackagePath,
            GenericSystemsPackageId = qualityGate.GenericSystemsPackageId,
            GenericSystemsRecipePreviewPresent = qualityGate.GenericSystemsRecipePreviewPresent,
            GenericSystemsRecipeApplyPassed = qualityGate.GenericSystemsRecipeApplyPassed,
            GenericSystemsHarvestPreviewPresent = qualityGate.GenericSystemsHarvestPreviewPresent,
            GenericSystemsHarvestApplyPassed = qualityGate.GenericSystemsHarvestApplyPassed,
            GenericSystemsTransactionPreviewPresent = qualityGate.GenericSystemsTransactionPreviewPresent,
            GenericSystemsEncounterPreviewPresent = qualityGate.GenericSystemsEncounterPreviewPresent,
            GenericSystemsCombatRoundPreviewPresent = qualityGate.GenericSystemsCombatRoundPreviewPresent,
            GenericSystemsInventorySummaryPresent = qualityGate.GenericSystemsInventorySummaryPresent,
            GenericSystemsResourceSummaryPresent = qualityGate.GenericSystemsResourceSummaryPresent,
            GenericSystemsEventLogPresent = qualityGate.GenericSystemsEventLogPresent,
            GenericSystemsUnitySmokeStatus = qualityGate.GenericSystemsUnitySmokeStatus,
            GenericSystemsCleanupScriptAvailable = qualityGate.GenericSystemsCleanupScriptAvailable,
            GenericSystemsCleanupCommand = qualityGate.GenericSystemsCleanupCommand,
            GenericSystemsGoal124StillGreen = qualityGate.GenericSystemsGoal124StillGreen,
            GenericSystemsProjectionOnly = qualityGate.GenericSystemsProjectionOnly,
            GenericSystemsSamplePackageReadOnly = qualityGate.GenericSystemsSamplePackageReadOnly,
            GenericSystemsEvidencePath = qualityGate.GenericSystemsEvidencePath,
            GenericSystemsExportPath = qualityGate.GenericSystemsExportPath,
            GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary =
                qualityGate.GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary,
            GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets =
                qualityGate.GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets,
            GenericGamePackageSystemsQualityGatePassed =
                qualityGate.GenericGamePackageSystemsQualityGatePassed,
            Goal125FilesDiscoveredByRelativePaths = qualityGate.Goal125FilesDiscoveredByRelativePaths
        };
}
