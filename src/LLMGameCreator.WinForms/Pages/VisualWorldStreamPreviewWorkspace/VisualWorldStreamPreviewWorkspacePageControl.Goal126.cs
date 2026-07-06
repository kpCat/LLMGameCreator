using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildGenericGamePackageFullPlaythroughDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "fullPlaythroughStatus="
            + result.Report.GenericFullPlaythroughStatus,
        "samplePackagePath="
            + result.Report.GenericFullPlaythroughSamplePackagePath,
        "packageId="
            + result.Report.GenericFullPlaythroughPackageId,
        "mapId="
            + result.Report.GenericFullPlaythroughMapId,
        "mapPathPreviewPresent="
            + result.Report.GenericFullPlaythroughMapPathPreviewPresent.ToString().ToLowerInvariant(),
        "signInteractionApplied="
            + result.Report.GenericFullPlaythroughSignInteractionApplied.ToString().ToLowerInvariant(),
        "dialogueSummaryPresent="
            + result.Report.GenericFullPlaythroughDialogueSummaryPresent.ToString().ToLowerInvariant(),
        "questObjectiveStatusPresent="
            + result.Report.GenericFullPlaythroughQuestObjectiveStatusPresent.ToString().ToLowerInvariant(),
        "inventorySummaryPresent="
            + result.Report.GenericFullPlaythroughInventorySummaryPresent.ToString().ToLowerInvariant(),
        "resourceSummaryPresent="
            + result.Report.GenericFullPlaythroughResourceSummaryPresent.ToString().ToLowerInvariant(),
        "systemsSummaryPresent="
            + result.Report.GenericFullPlaythroughSystemsSummaryPresent.ToString().ToLowerInvariant(),
        "combatRoundPreviewPresent="
            + result.Report.GenericFullPlaythroughCombatRoundPreviewPresent.ToString().ToLowerInvariant(),
        "eventTranscriptPresent="
            + result.Report.GenericFullPlaythroughEventTranscriptPresent.ToString().ToLowerInvariant(),
        "unitySmokeStatus="
            + result.Report.GenericFullPlaythroughUnitySmokeStatus,
        "cleanupScriptAvailable="
            + result.Report.GenericFullPlaythroughCleanupScriptAvailable.ToString().ToLowerInvariant(),
        "projectionOnly="
            + result.Report.GenericFullPlaythroughProjectionOnly.ToString().ToLowerInvariant(),
        "evidencePath="
            + result.Report.GenericFullPlaythroughEvidencePath,
        "exportPath="
            + result.Report.GenericFullPlaythroughExportPath,
        "goal125StillGreen="
            + result.Report.GenericFullPlaythroughGoal125StillGreen.ToString().ToLowerInvariant(),
        "samplePackageReadOnly="
            + result.Report.GenericFullPlaythroughSamplePackageReadOnly.ToString().ToLowerInvariant(),
        "genericGamePackageFullPlaythroughQualityGatePassed="
            + result.Report.GenericGamePackageFullPlaythroughQualityGatePassed.ToString().ToLowerInvariant(),
        "goal126FilesDiscoveredByRelativePaths="
            + result.Report.Goal126FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildGenericGamePackageFullPlaythroughEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "fullPlaythroughStatus: "
            + entry.GenericFullPlaythroughStatus,
        "samplePackagePath: "
            + entry.GenericFullPlaythroughSamplePackagePath,
        "packageId: "
            + entry.GenericFullPlaythroughPackageId,
        "mapId: "
            + entry.GenericFullPlaythroughMapId,
        "mapPathPreviewPresent: "
            + entry.GenericFullPlaythroughMapPathPreviewPresent.ToString().ToLowerInvariant(),
        "signInteractionApplied: "
            + entry.GenericFullPlaythroughSignInteractionApplied.ToString().ToLowerInvariant(),
        "dialogueSummaryPresent: "
            + entry.GenericFullPlaythroughDialogueSummaryPresent.ToString().ToLowerInvariant(),
        "questObjectiveStatusPresent: "
            + entry.GenericFullPlaythroughQuestObjectiveStatusPresent.ToString().ToLowerInvariant(),
        "inventorySummaryPresent: "
            + entry.GenericFullPlaythroughInventorySummaryPresent.ToString().ToLowerInvariant(),
        "resourceSummaryPresent: "
            + entry.GenericFullPlaythroughResourceSummaryPresent.ToString().ToLowerInvariant(),
        "systemsSummaryPresent: "
            + entry.GenericFullPlaythroughSystemsSummaryPresent.ToString().ToLowerInvariant(),
        "combatRoundPreviewPresent: "
            + entry.GenericFullPlaythroughCombatRoundPreviewPresent.ToString().ToLowerInvariant(),
        "eventTranscriptPresent: "
            + entry.GenericFullPlaythroughEventTranscriptPresent.ToString().ToLowerInvariant(),
        "unitySmokeStatus: "
            + entry.GenericFullPlaythroughUnitySmokeStatus,
        "cleanupScriptAvailable: "
            + entry.GenericFullPlaythroughCleanupScriptAvailable.ToString().ToLowerInvariant(),
        "cleanupCommand: "
            + entry.GenericFullPlaythroughCleanupCommand,
        "goal125StillGreen: "
            + entry.GenericFullPlaythroughGoal125StillGreen.ToString().ToLowerInvariant(),
        "projectionOnly: "
            + entry.GenericFullPlaythroughProjectionOnly.ToString().ToLowerInvariant(),
        "samplePackageReadOnly: "
            + entry.GenericFullPlaythroughSamplePackageReadOnly.ToString().ToLowerInvariant(),
        "evidencePath: "
            + entry.GenericFullPlaythroughEvidencePath,
        "exportPath: "
            + entry.GenericFullPlaythroughExportPath,
        "noRuntimeProviderSchemaLuaGeneratorLibrary: "
            + entry.GenericFullPlaythroughNoRuntimeProviderSchemaLuaGeneratorLibrary.ToString().ToLowerInvariant(),
        "noUnityScenePrefabSettingsPackagesStreamingAssets: "
            + entry.GenericFullPlaythroughNoUnityScenePrefabSettingsPackagesStreamingAssets.ToString().ToLowerInvariant()
    ];
}
