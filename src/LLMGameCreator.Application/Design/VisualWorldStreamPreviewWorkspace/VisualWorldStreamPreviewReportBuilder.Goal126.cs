namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal126ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            GenericFullPlaythroughStatus = qualityGate.GenericFullPlaythroughStatus,
            GenericFullPlaythroughSamplePackagePath =
                qualityGate.GenericFullPlaythroughSamplePackagePath,
            GenericFullPlaythroughPackageId = qualityGate.GenericFullPlaythroughPackageId,
            GenericFullPlaythroughMapId = qualityGate.GenericFullPlaythroughMapId,
            GenericFullPlaythroughMapPathPreviewPresent =
                qualityGate.GenericFullPlaythroughMapPathPreviewPresent,
            GenericFullPlaythroughSignInteractionApplied =
                qualityGate.GenericFullPlaythroughSignInteractionApplied,
            GenericFullPlaythroughDialogueSummaryPresent =
                qualityGate.GenericFullPlaythroughDialogueSummaryPresent,
            GenericFullPlaythroughQuestObjectiveStatusPresent =
                qualityGate.GenericFullPlaythroughQuestObjectiveStatusPresent,
            GenericFullPlaythroughInventorySummaryPresent =
                qualityGate.GenericFullPlaythroughInventorySummaryPresent,
            GenericFullPlaythroughResourceSummaryPresent =
                qualityGate.GenericFullPlaythroughResourceSummaryPresent,
            GenericFullPlaythroughSystemsSummaryPresent =
                qualityGate.GenericFullPlaythroughSystemsSummaryPresent,
            GenericFullPlaythroughRecipeApplyPassed =
                qualityGate.GenericFullPlaythroughRecipeApplyPassed,
            GenericFullPlaythroughHarvestApplyPassed =
                qualityGate.GenericFullPlaythroughHarvestApplyPassed,
            GenericFullPlaythroughTransactionPreviewPresent =
                qualityGate.GenericFullPlaythroughTransactionPreviewPresent,
            GenericFullPlaythroughCombatRoundPreviewPresent =
                qualityGate.GenericFullPlaythroughCombatRoundPreviewPresent,
            GenericFullPlaythroughEventTranscriptPresent =
                qualityGate.GenericFullPlaythroughEventTranscriptPresent,
            GenericFullPlaythroughUnitySmokeStatus =
                qualityGate.GenericFullPlaythroughUnitySmokeStatus,
            GenericFullPlaythroughCleanupScriptAvailable =
                qualityGate.GenericFullPlaythroughCleanupScriptAvailable,
            GenericFullPlaythroughCleanupCommand =
                qualityGate.GenericFullPlaythroughCleanupCommand,
            GenericFullPlaythroughGoal125StillGreen =
                qualityGate.GenericFullPlaythroughGoal125StillGreen,
            GenericFullPlaythroughProjectionOnly =
                qualityGate.GenericFullPlaythroughProjectionOnly,
            GenericFullPlaythroughSamplePackageReadOnly =
                qualityGate.GenericFullPlaythroughSamplePackageReadOnly,
            GenericFullPlaythroughEvidencePath =
                qualityGate.GenericFullPlaythroughEvidencePath,
            GenericFullPlaythroughExportPath =
                qualityGate.GenericFullPlaythroughExportPath,
            GenericFullPlaythroughNoRuntimeProviderSchemaLuaGeneratorLibrary =
                qualityGate.GenericFullPlaythroughNoRuntimeProviderSchemaLuaGeneratorLibrary,
            GenericFullPlaythroughNoUnityScenePrefabSettingsPackagesStreamingAssets =
                qualityGate.GenericFullPlaythroughNoUnityScenePrefabSettingsPackagesStreamingAssets,
            GenericGamePackageFullPlaythroughQualityGatePassed =
                qualityGate.GenericGamePackageFullPlaythroughQualityGatePassed,
            Goal126FilesDiscoveredByRelativePaths =
                qualityGate.Goal126FilesDiscoveredByRelativePaths
        };
}
