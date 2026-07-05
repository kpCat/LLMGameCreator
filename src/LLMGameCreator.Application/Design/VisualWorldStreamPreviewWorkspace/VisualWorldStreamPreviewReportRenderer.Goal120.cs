namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal120ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Accepted Alpha Projection Usability",
            string.Empty,
            $"- acceptedAlphaProjectionUsabilityStatus: {report.AcceptedAlphaProjectionUsabilityStatus}",
            $"- acceptedAlphaProjectionUsabilityUnityMenuPath: {report.AcceptedAlphaProjectionUsabilityUnityMenuPath}",
            $"- acceptedAlphaProjectionUsabilityCleanupScriptPath: {report.AcceptedAlphaProjectionUsabilityCleanupScriptPath}",
            $"- acceptedAlphaProjectionUsabilityCleanupScriptCmdPath: {report.AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath}",
            $"- acceptedAlphaProjectionUsabilityLegendPresent: {report.AcceptedAlphaProjectionUsabilityLegendPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityMarkerDescriptorPresent: {report.AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilitySelectionControlsPresent: {report.AcceptedAlphaProjectionUsabilitySelectionControlsPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityFocusCameraControlPresent: {report.AcceptedAlphaProjectionUsabilityFocusCameraControlPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityMaterialWarningGuardPresent: {report.AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityUnitySmokeStatus: {report.AcceptedAlphaProjectionUsabilityUnitySmokeStatus}",
            $"- acceptedAlphaProjectionUsabilityDoNotStartAutomatically: {report.AcceptedAlphaProjectionUsabilityDoNotStartAutomatically.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityQualityGatePassed: {report.AcceptedAlphaProjectionUsabilityQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal120FilesDiscoveredByRelativePaths: {report.Goal120FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal120QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal120 Quality",
            string.Empty,
            $"- acceptedAlphaProjectionUsabilityGroupPresent: {qualityGate.AcceptedAlphaProjectionUsabilityGroupPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityStatus: {qualityGate.AcceptedAlphaProjectionUsabilityStatus}",
            $"- acceptedAlphaProjectionUsabilityUnityMenuPath: {qualityGate.AcceptedAlphaProjectionUsabilityUnityMenuPath}",
            $"- acceptedAlphaProjectionUsabilityCleanupScriptPath: {qualityGate.AcceptedAlphaProjectionUsabilityCleanupScriptPath}",
            $"- acceptedAlphaProjectionUsabilityCleanupScriptCmdPath: {qualityGate.AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath}",
            $"- acceptedAlphaProjectionUsabilityLegendPresent: {qualityGate.AcceptedAlphaProjectionUsabilityLegendPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityMarkerDescriptorPresent: {qualityGate.AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilitySelectionControlsPresent: {qualityGate.AcceptedAlphaProjectionUsabilitySelectionControlsPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityFocusCameraControlPresent: {qualityGate.AcceptedAlphaProjectionUsabilityFocusCameraControlPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityMaterialWarningGuardPresent: {qualityGate.AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityUnitySmokeStatus: {qualityGate.AcceptedAlphaProjectionUsabilityUnitySmokeStatus}",
            $"- acceptedAlphaProjectionUsabilityDoNotStartAutomatically: {qualityGate.AcceptedAlphaProjectionUsabilityDoNotStartAutomatically.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaProjectionUsabilityQualityGatePassed: {qualityGate.AcceptedAlphaProjectionUsabilityQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal120FilesDiscoveredByRelativePaths: {qualityGate.Goal120FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);
}
