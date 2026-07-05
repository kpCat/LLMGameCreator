namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal119ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Accepted Alpha Unity Playable Projection",
            string.Empty,
            $"- acceptedAlphaUnityPlayableProjectionStatus: {report.AcceptedAlphaUnityPlayableProjectionStatus}",
            $"- acceptedAlphaUnityPlayableProjectionUnityMenuPath: {report.AcceptedAlphaUnityPlayableProjectionUnityMenuPath}",
            $"- acceptedAlphaUnityPlayableProjectionBaselineId: {report.AcceptedAlphaUnityPlayableProjectionBaselineId}",
            $"- acceptedAlphaUnityPlayableProjectionAcceptedBaselineReady: {report.AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaUnityPlayableProjectionGeneratedRootName: {report.AcceptedAlphaUnityPlayableProjectionGeneratedRootName}",
            $"- acceptedAlphaUnityPlayableProjectionScriptInventoryCount: {report.AcceptedAlphaUnityPlayableProjectionScriptInventoryCount}",
            $"- acceptedAlphaUnityPlayableProjectionSmokePlanStepCount: {report.AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount}",
            $"- acceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean: {report.AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaUnityPlayableProjectionDoNotStartAutomatically: {report.AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaUnityPlayableProjectionQualityGatePassed: {report.AcceptedAlphaUnityPlayableProjectionQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal119FilesDiscoveredByRelativePaths: {report.Goal119FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal119QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal119 Quality",
            string.Empty,
            $"- acceptedAlphaUnityPlayableProjectionGroupPresent: {qualityGate.AcceptedAlphaUnityPlayableProjectionGroupPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaUnityPlayableProjectionStatus: {qualityGate.AcceptedAlphaUnityPlayableProjectionStatus}",
            $"- acceptedAlphaUnityPlayableProjectionUnityMenuPath: {qualityGate.AcceptedAlphaUnityPlayableProjectionUnityMenuPath}",
            $"- acceptedAlphaUnityPlayableProjectionBaselineId: {qualityGate.AcceptedAlphaUnityPlayableProjectionBaselineId}",
            $"- acceptedAlphaUnityPlayableProjectionAcceptedBaselineReady: {qualityGate.AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaUnityPlayableProjectionGeneratedRootName: {qualityGate.AcceptedAlphaUnityPlayableProjectionGeneratedRootName}",
            $"- acceptedAlphaUnityPlayableProjectionScriptInventoryCount: {qualityGate.AcceptedAlphaUnityPlayableProjectionScriptInventoryCount}",
            $"- acceptedAlphaUnityPlayableProjectionSmokePlanStepCount: {qualityGate.AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount}",
            $"- acceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean: {qualityGate.AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaUnityPlayableProjectionDoNotStartAutomatically: {qualityGate.AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaUnityPlayableProjectionQualityGatePassed: {qualityGate.AcceptedAlphaUnityPlayableProjectionQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal119FilesDiscoveredByRelativePaths: {qualityGate.Goal119FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);
}
