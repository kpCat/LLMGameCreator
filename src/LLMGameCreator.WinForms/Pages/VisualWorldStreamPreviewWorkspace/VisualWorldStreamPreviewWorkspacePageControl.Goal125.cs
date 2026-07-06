using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildGenericGamePackageSystemsDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "genericSystemsStatus="
            + result.Report.GenericSystemsStatus,
        "samplePackagePath="
            + result.Report.GenericSystemsSamplePackagePath,
        "packageId="
            + result.Report.GenericSystemsPackageId,
        "recipePreviewPresent="
            + result.Report.GenericSystemsRecipePreviewPresent.ToString().ToLowerInvariant(),
        "recipeApplyPassed="
            + result.Report.GenericSystemsRecipeApplyPassed.ToString().ToLowerInvariant(),
        "harvestPreviewPresent="
            + result.Report.GenericSystemsHarvestPreviewPresent.ToString().ToLowerInvariant(),
        "harvestApplyPassed="
            + result.Report.GenericSystemsHarvestApplyPassed.ToString().ToLowerInvariant(),
        "transactionPreviewPresent="
            + result.Report.GenericSystemsTransactionPreviewPresent.ToString().ToLowerInvariant(),
        "encounterPreviewPresent="
            + result.Report.GenericSystemsEncounterPreviewPresent.ToString().ToLowerInvariant(),
        "combatRoundPreviewPresent="
            + result.Report.GenericSystemsCombatRoundPreviewPresent.ToString().ToLowerInvariant(),
        "inventorySummaryPresent="
            + result.Report.GenericSystemsInventorySummaryPresent.ToString().ToLowerInvariant(),
        "resourceSummaryPresent="
            + result.Report.GenericSystemsResourceSummaryPresent.ToString().ToLowerInvariant(),
        "systemsEventLogPresent="
            + result.Report.GenericSystemsEventLogPresent.ToString().ToLowerInvariant(),
        "unitySmokeStatus="
            + result.Report.GenericSystemsUnitySmokeStatus,
        "cleanupCommand="
            + result.Report.GenericSystemsCleanupCommand,
        "goal124StillGreen="
            + result.Report.GenericSystemsGoal124StillGreen.ToString().ToLowerInvariant(),
        "projectionOnly="
            + result.Report.GenericSystemsProjectionOnly.ToString().ToLowerInvariant(),
        "samplePackageReadOnly="
            + result.Report.GenericSystemsSamplePackageReadOnly.ToString().ToLowerInvariant(),
        "evidencePath="
            + result.Report.GenericSystemsEvidencePath,
        "exportPath="
            + result.Report.GenericSystemsExportPath,
        "noRuntimeProviderSchemaLuaGeneratorLibrary="
            + result.Report.GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary.ToString().ToLowerInvariant(),
        "noUnityScenePrefabSettingsPackagesStreamingAssets="
            + result.Report.GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets.ToString().ToLowerInvariant(),
        "genericGamePackageSystemsQualityGatePassed="
            + result.Report.GenericGamePackageSystemsQualityGatePassed.ToString().ToLowerInvariant(),
        "goal125FilesDiscoveredByRelativePaths="
            + result.Report.Goal125FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildGenericGamePackageSystemsEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "genericSystemsStatus: "
            + entry.GenericSystemsStatus,
        "samplePackagePath: "
            + entry.GenericSystemsSamplePackagePath,
        "packageId: "
            + entry.GenericSystemsPackageId,
        "recipePreviewPresent: "
            + entry.GenericSystemsRecipePreviewPresent.ToString().ToLowerInvariant(),
        "recipeApplyPassed: "
            + entry.GenericSystemsRecipeApplyPassed.ToString().ToLowerInvariant(),
        "harvestPreviewPresent: "
            + entry.GenericSystemsHarvestPreviewPresent.ToString().ToLowerInvariant(),
        "harvestApplyPassed: "
            + entry.GenericSystemsHarvestApplyPassed.ToString().ToLowerInvariant(),
        "transactionPreviewPresent: "
            + entry.GenericSystemsTransactionPreviewPresent.ToString().ToLowerInvariant(),
        "encounterPreviewPresent: "
            + entry.GenericSystemsEncounterPreviewPresent.ToString().ToLowerInvariant(),
        "combatRoundPreviewPresent: "
            + entry.GenericSystemsCombatRoundPreviewPresent.ToString().ToLowerInvariant(),
        "inventorySummaryPresent: "
            + entry.GenericSystemsInventorySummaryPresent.ToString().ToLowerInvariant(),
        "resourceSummaryPresent: "
            + entry.GenericSystemsResourceSummaryPresent.ToString().ToLowerInvariant(),
        "systemsEventLogPresent: "
            + entry.GenericSystemsEventLogPresent.ToString().ToLowerInvariant(),
        "unitySmokeStatus: "
            + entry.GenericSystemsUnitySmokeStatus,
        "cleanupScriptAvailable: "
            + entry.GenericSystemsCleanupScriptAvailable.ToString().ToLowerInvariant(),
        "cleanupCommand: "
            + entry.GenericSystemsCleanupCommand,
        "goal124StillGreen: "
            + entry.GenericSystemsGoal124StillGreen.ToString().ToLowerInvariant(),
        "projectionOnly: "
            + entry.GenericSystemsProjectionOnly.ToString().ToLowerInvariant(),
        "samplePackageReadOnly: "
            + entry.GenericSystemsSamplePackageReadOnly.ToString().ToLowerInvariant(),
        "evidencePath: "
            + entry.GenericSystemsEvidencePath,
        "exportPath: "
            + entry.GenericSystemsExportPath,
        "noRuntimeProviderSchemaLuaGeneratorLibrary: "
            + entry.GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary.ToString().ToLowerInvariant(),
        "noUnityScenePrefabSettingsPackagesStreamingAssets: "
            + entry.GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets.ToString().ToLowerInvariant()
    ];
}
