namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal124ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Generic GamePackage Quest Dialogue Interaction Loop",
            string.Empty,
            $"- genericLoopStatus: {report.GenericLoopStatus}",
            $"- samplePackagePath: {report.GenericLoopSamplePackagePath}",
            $"- packageId: {report.GenericLoopPackageId}",
            $"- mapId: {report.GenericLoopMapId}",
            $"- interactionPreviewPresent: {report.GenericLoopInteractionPreviewPresent.ToString().ToLowerInvariant()}",
            $"- interactionApplyPassed: {report.GenericLoopInteractionApplyPassed.ToString().ToLowerInvariant()}",
            $"- dialogueSummaryPresent: {report.GenericLoopDialogueSummaryPresent.ToString().ToLowerInvariant()}",
            $"- questObjectiveSummaryPresent: {report.GenericLoopQuestObjectiveSummaryPresent.ToString().ToLowerInvariant()}",
            $"- inventorySummaryPresent: {report.GenericLoopInventorySummaryPresent.ToString().ToLowerInvariant()}",
            $"- resourceSummaryPresent: {report.GenericLoopResourceSummaryPresent.ToString().ToLowerInvariant()}",
            $"- unitySmokeStatus: {report.GenericLoopUnitySmokeStatus}",
            $"- cleanupScriptAvailable: {report.GenericLoopCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- cleanupCommand: {report.GenericLoopCleanupCommand}",
            $"- goal123StillGreen: {report.GenericLoopGoal123StillGreen.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.GenericLoopProjectionOnly.ToString().ToLowerInvariant()}",
            $"- appliedInteractionCount: {report.GenericLoopAppliedInteractionCount}",
            $"- startedQuestCount: {report.GenericLoopStartedQuestCount}",
            $"- evidencePath: {report.GenericLoopEvidencePath}",
            $"- exportPath: {report.GenericLoopExportPath}",
            $"- genericGamePackageLoopQualityGatePassed: {report.GenericGamePackageLoopQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal124FilesDiscoveredByRelativePaths: {report.Goal124FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal124QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal124 Quality",
            string.Empty,
            $"- genericGamePackageLoopGroupPresent: {qualityGate.GenericGamePackageLoopGroupPresent.ToString().ToLowerInvariant()}",
            $"- genericLoopStatus: {qualityGate.GenericLoopStatus}",
            $"- samplePackagePath: {qualityGate.GenericLoopSamplePackagePath}",
            $"- packageId: {qualityGate.GenericLoopPackageId}",
            $"- mapId: {qualityGate.GenericLoopMapId}",
            $"- interactionPreviewPresent: {qualityGate.GenericLoopInteractionPreviewPresent.ToString().ToLowerInvariant()}",
            $"- interactionApplyPassed: {qualityGate.GenericLoopInteractionApplyPassed.ToString().ToLowerInvariant()}",
            $"- dialogueSummaryPresent: {qualityGate.GenericLoopDialogueSummaryPresent.ToString().ToLowerInvariant()}",
            $"- questObjectiveSummaryPresent: {qualityGate.GenericLoopQuestObjectiveSummaryPresent.ToString().ToLowerInvariant()}",
            $"- inventorySummaryPresent: {qualityGate.GenericLoopInventorySummaryPresent.ToString().ToLowerInvariant()}",
            $"- resourceSummaryPresent: {qualityGate.GenericLoopResourceSummaryPresent.ToString().ToLowerInvariant()}",
            $"- unitySmokeStatus: {qualityGate.GenericLoopUnitySmokeStatus}",
            $"- cleanupScriptAvailable: {qualityGate.GenericLoopCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- cleanupCommand: {qualityGate.GenericLoopCleanupCommand}",
            $"- goal123StillGreen: {qualityGate.GenericLoopGoal123StillGreen.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.GenericLoopProjectionOnly.ToString().ToLowerInvariant()}",
            $"- genericGamePackageLoopQualityGatePassed: {qualityGate.GenericGamePackageLoopQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal124FilesDiscoveredByRelativePaths: {qualityGate.Goal124FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsGenericGamePackageLoopBindingReal: {qualityGate.WinFormsGenericGamePackageLoopBindingReal.ToString().ToLowerInvariant()}"
        ]);
}
