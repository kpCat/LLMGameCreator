namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal121ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Accepted Alpha Interaction Drilldown",
            string.Empty,
            $"- acceptedAlphaInteractionDrilldownFullVerificationStatus: {report.AcceptedAlphaInteractionDrilldownFullVerificationStatus}",
            $"- acceptedAlphaInteractionDrilldownUnityMenuPath: {report.AcceptedAlphaInteractionDrilldownUnityMenuPath}",
            $"- acceptedAlphaInteractionDrilldownOneClickButtonPresent: {report.AcceptedAlphaInteractionDrilldownOneClickButtonPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownDrilldownFieldsPresent: {report.AcceptedAlphaInteractionDrilldownDrilldownFieldsPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownInteractionPreviewPresent: {report.AcceptedAlphaInteractionDrilldownInteractionPreviewPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent: {report.AcceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker: {report.AcceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker}",
            $"- acceptedAlphaInteractionDrilldownCleanupScriptAvailable: {report.AcceptedAlphaInteractionDrilldownCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownMaterialWarningGuardPresent: {report.AcceptedAlphaInteractionDrilldownMaterialWarningGuardPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton: {report.AcceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus: {report.AcceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus}",
            $"- acceptedAlphaInteractionDrilldownQualityGatePassed: {report.AcceptedAlphaInteractionDrilldownQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal121FilesDiscoveredByRelativePaths: {report.Goal121FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal121QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal121 Quality",
            string.Empty,
            $"- acceptedAlphaInteractionDrilldownGroupPresent: {qualityGate.AcceptedAlphaInteractionDrilldownGroupPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownFullVerificationStatus: {qualityGate.AcceptedAlphaInteractionDrilldownFullVerificationStatus}",
            $"- acceptedAlphaInteractionDrilldownUnityMenuPath: {qualityGate.AcceptedAlphaInteractionDrilldownUnityMenuPath}",
            $"- acceptedAlphaInteractionDrilldownOneClickButtonPresent: {qualityGate.AcceptedAlphaInteractionDrilldownOneClickButtonPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownDrilldownFieldsPresent: {qualityGate.AcceptedAlphaInteractionDrilldownDrilldownFieldsPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownInteractionPreviewPresent: {qualityGate.AcceptedAlphaInteractionDrilldownInteractionPreviewPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent: {qualityGate.AcceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker: {qualityGate.AcceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker}",
            $"- acceptedAlphaInteractionDrilldownCleanupScriptAvailable: {qualityGate.AcceptedAlphaInteractionDrilldownCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownMaterialWarningGuardPresent: {qualityGate.AcceptedAlphaInteractionDrilldownMaterialWarningGuardPresent.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton: {qualityGate.AcceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton.ToString().ToLowerInvariant()}",
            $"- acceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus: {qualityGate.AcceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus}",
            $"- acceptedAlphaInteractionDrilldownQualityGatePassed: {qualityGate.AcceptedAlphaInteractionDrilldownQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal121FilesDiscoveredByRelativePaths: {qualityGate.Goal121FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);
}
