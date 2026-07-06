namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal126ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Generic GamePackage Full Playthrough",
            string.Empty,
            $"- fullPlaythroughStatus: {report.GenericFullPlaythroughStatus}",
            $"- samplePackagePath: {report.GenericFullPlaythroughSamplePackagePath}",
            $"- packageId: {report.GenericFullPlaythroughPackageId}",
            $"- mapId: {report.GenericFullPlaythroughMapId}",
            $"- mapPathPreviewPresent: {report.GenericFullPlaythroughMapPathPreviewPresent.ToString().ToLowerInvariant()}",
            $"- signInteractionApplied: {report.GenericFullPlaythroughSignInteractionApplied.ToString().ToLowerInvariant()}",
            $"- dialogueSummaryPresent: {report.GenericFullPlaythroughDialogueSummaryPresent.ToString().ToLowerInvariant()}",
            $"- questObjectiveStatusPresent: {report.GenericFullPlaythroughQuestObjectiveStatusPresent.ToString().ToLowerInvariant()}",
            $"- inventorySummaryPresent: {report.GenericFullPlaythroughInventorySummaryPresent.ToString().ToLowerInvariant()}",
            $"- resourceSummaryPresent: {report.GenericFullPlaythroughResourceSummaryPresent.ToString().ToLowerInvariant()}",
            $"- systemsSummaryPresent: {report.GenericFullPlaythroughSystemsSummaryPresent.ToString().ToLowerInvariant()}",
            $"- recipeApplyPassed: {report.GenericFullPlaythroughRecipeApplyPassed.ToString().ToLowerInvariant()}",
            $"- harvestApplyPassed: {report.GenericFullPlaythroughHarvestApplyPassed.ToString().ToLowerInvariant()}",
            $"- transactionPreviewPresent: {report.GenericFullPlaythroughTransactionPreviewPresent.ToString().ToLowerInvariant()}",
            $"- combatRoundPreviewPresent: {report.GenericFullPlaythroughCombatRoundPreviewPresent.ToString().ToLowerInvariant()}",
            $"- eventTranscriptPresent: {report.GenericFullPlaythroughEventTranscriptPresent.ToString().ToLowerInvariant()}",
            $"- unitySmokeStatus: {report.GenericFullPlaythroughUnitySmokeStatus}",
            $"- cleanupScriptAvailable: {report.GenericFullPlaythroughCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- cleanupCommand: {report.GenericFullPlaythroughCleanupCommand}",
            $"- goal125StillGreen: {report.GenericFullPlaythroughGoal125StillGreen.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.GenericFullPlaythroughProjectionOnly.ToString().ToLowerInvariant()}",
            $"- samplePackageReadOnly: {report.GenericFullPlaythroughSamplePackageReadOnly.ToString().ToLowerInvariant()}",
            $"- evidencePath: {report.GenericFullPlaythroughEvidencePath}",
            $"- exportPath: {report.GenericFullPlaythroughExportPath}",
            $"- noRuntimeProviderSchemaLuaGeneratorLibrary: {report.GenericFullPlaythroughNoRuntimeProviderSchemaLuaGeneratorLibrary.ToString().ToLowerInvariant()}",
            $"- noUnityScenePrefabSettingsPackagesStreamingAssets: {report.GenericFullPlaythroughNoUnityScenePrefabSettingsPackagesStreamingAssets.ToString().ToLowerInvariant()}",
            $"- genericGamePackageFullPlaythroughQualityGatePassed: {report.GenericGamePackageFullPlaythroughQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal126FilesDiscoveredByRelativePaths: {report.Goal126FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal126QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal126 Quality",
            string.Empty,
            $"- genericGamePackageFullPlaythroughGroupPresent: {qualityGate.GenericGamePackageFullPlaythroughGroupPresent.ToString().ToLowerInvariant()}",
            $"- fullPlaythroughStatus: {qualityGate.GenericFullPlaythroughStatus}",
            $"- samplePackagePath: {qualityGate.GenericFullPlaythroughSamplePackagePath}",
            $"- packageId: {qualityGate.GenericFullPlaythroughPackageId}",
            $"- mapId: {qualityGate.GenericFullPlaythroughMapId}",
            $"- mapPathPreviewPresent: {qualityGate.GenericFullPlaythroughMapPathPreviewPresent.ToString().ToLowerInvariant()}",
            $"- signInteractionApplied: {qualityGate.GenericFullPlaythroughSignInteractionApplied.ToString().ToLowerInvariant()}",
            $"- dialogueSummaryPresent: {qualityGate.GenericFullPlaythroughDialogueSummaryPresent.ToString().ToLowerInvariant()}",
            $"- questObjectiveStatusPresent: {qualityGate.GenericFullPlaythroughQuestObjectiveStatusPresent.ToString().ToLowerInvariant()}",
            $"- inventorySummaryPresent: {qualityGate.GenericFullPlaythroughInventorySummaryPresent.ToString().ToLowerInvariant()}",
            $"- resourceSummaryPresent: {qualityGate.GenericFullPlaythroughResourceSummaryPresent.ToString().ToLowerInvariant()}",
            $"- systemsSummaryPresent: {qualityGate.GenericFullPlaythroughSystemsSummaryPresent.ToString().ToLowerInvariant()}",
            $"- combatRoundPreviewPresent: {qualityGate.GenericFullPlaythroughCombatRoundPreviewPresent.ToString().ToLowerInvariant()}",
            $"- eventTranscriptPresent: {qualityGate.GenericFullPlaythroughEventTranscriptPresent.ToString().ToLowerInvariant()}",
            $"- unitySmokeStatus: {qualityGate.GenericFullPlaythroughUnitySmokeStatus}",
            $"- cleanupScriptAvailable: {qualityGate.GenericFullPlaythroughCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- cleanupCommand: {qualityGate.GenericFullPlaythroughCleanupCommand}",
            $"- goal125StillGreen: {qualityGate.GenericFullPlaythroughGoal125StillGreen.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.GenericFullPlaythroughProjectionOnly.ToString().ToLowerInvariant()}",
            $"- samplePackageReadOnly: {qualityGate.GenericFullPlaythroughSamplePackageReadOnly.ToString().ToLowerInvariant()}",
            $"- genericGamePackageFullPlaythroughQualityGatePassed: {qualityGate.GenericGamePackageFullPlaythroughQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal126FilesDiscoveredByRelativePaths: {qualityGate.Goal126FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsGenericGamePackageFullPlaythroughBindingReal: {qualityGate.WinFormsGenericGamePackageFullPlaythroughBindingReal.ToString().ToLowerInvariant()}"
        ]);
}
