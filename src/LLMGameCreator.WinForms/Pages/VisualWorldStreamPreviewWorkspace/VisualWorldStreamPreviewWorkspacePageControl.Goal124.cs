using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildGenericGamePackageLoopDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "genericLoopStatus="
            + result.Report.GenericLoopStatus,
        "samplePackagePath="
            + result.Report.GenericLoopSamplePackagePath,
        "packageId="
            + result.Report.GenericLoopPackageId,
        "mapId="
            + result.Report.GenericLoopMapId,
        "interactionPreviewPresent="
            + result.Report.GenericLoopInteractionPreviewPresent.ToString().ToLowerInvariant(),
        "interactionApplyPassed="
            + result.Report.GenericLoopInteractionApplyPassed.ToString().ToLowerInvariant(),
        "dialogueSummaryPresent="
            + result.Report.GenericLoopDialogueSummaryPresent.ToString().ToLowerInvariant(),
        "questObjectiveSummaryPresent="
            + result.Report.GenericLoopQuestObjectiveSummaryPresent.ToString().ToLowerInvariant(),
        "inventorySummaryPresent="
            + result.Report.GenericLoopInventorySummaryPresent.ToString().ToLowerInvariant(),
        "resourceSummaryPresent="
            + result.Report.GenericLoopResourceSummaryPresent.ToString().ToLowerInvariant(),
        "unitySmokeStatus="
            + result.Report.GenericLoopUnitySmokeStatus,
        "cleanupCommand="
            + result.Report.GenericLoopCleanupCommand,
        "goal123StillGreen="
            + result.Report.GenericLoopGoal123StillGreen.ToString().ToLowerInvariant(),
        "projectionOnly="
            + result.Report.GenericLoopProjectionOnly.ToString().ToLowerInvariant(),
        "appliedInteractionCount="
            + result.Report.GenericLoopAppliedInteractionCount,
        "startedQuestCount="
            + result.Report.GenericLoopStartedQuestCount,
        "evidencePath="
            + result.Report.GenericLoopEvidencePath,
        "exportPath="
            + result.Report.GenericLoopExportPath,
        "genericGamePackageLoopQualityGatePassed="
            + result.Report.GenericGamePackageLoopQualityGatePassed.ToString().ToLowerInvariant(),
        "goal124FilesDiscoveredByRelativePaths="
            + result.Report.Goal124FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildGenericGamePackageLoopEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "genericLoopStatus: "
            + entry.GenericLoopStatus,
        "samplePackagePath: "
            + entry.GenericLoopSamplePackagePath,
        "packageId: "
            + entry.GenericLoopPackageId,
        "mapId: "
            + entry.GenericLoopMapId,
        "interactionPreviewPresent: "
            + entry.GenericLoopInteractionPreviewPresent.ToString().ToLowerInvariant(),
        "interactionApplyPassed: "
            + entry.GenericLoopInteractionApplyPassed.ToString().ToLowerInvariant(),
        "dialogueSummaryPresent: "
            + entry.GenericLoopDialogueSummaryPresent.ToString().ToLowerInvariant(),
        "questObjectiveSummaryPresent: "
            + entry.GenericLoopQuestObjectiveSummaryPresent.ToString().ToLowerInvariant(),
        "inventorySummaryPresent: "
            + entry.GenericLoopInventorySummaryPresent.ToString().ToLowerInvariant(),
        "resourceSummaryPresent: "
            + entry.GenericLoopResourceSummaryPresent.ToString().ToLowerInvariant(),
        "unitySmokeStatus: "
            + entry.GenericLoopUnitySmokeStatus,
        "cleanupScriptAvailable: "
            + entry.GenericLoopCleanupScriptAvailable.ToString().ToLowerInvariant(),
        "cleanupCommand: "
            + entry.GenericLoopCleanupCommand,
        "goal123StillGreen: "
            + entry.GenericLoopGoal123StillGreen.ToString().ToLowerInvariant(),
        "projectionOnly: "
            + entry.GenericLoopProjectionOnly.ToString().ToLowerInvariant(),
        "appliedInteractionCount: "
            + entry.GenericLoopAppliedInteractionCount,
        "startedQuestCount: "
            + entry.GenericLoopStartedQuestCount,
        "evidencePath: "
            + entry.GenericLoopEvidencePath,
        "exportPath: "
            + entry.GenericLoopExportPath
    ];
}
