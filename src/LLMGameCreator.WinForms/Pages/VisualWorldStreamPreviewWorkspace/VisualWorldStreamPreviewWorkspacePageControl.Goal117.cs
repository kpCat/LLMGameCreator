using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaPostAcceptanceDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "offlineGeoworldAlphaPostAcceptanceManualGateStatus="
            + result.Report.OfflineGeoworldAlphaPostAcceptanceManualGateStatus,
        "offlineGeoworldAlphaPostAcceptanceHumanAccepted="
            + result.Report.OfflineGeoworldAlphaPostAcceptanceHumanAccepted
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaPostAcceptanceManualResultSha256="
            + result.Report.OfflineGeoworldAlphaPostAcceptanceManualResultSha256,
        "offlineGeoworldAlphaPostAcceptanceRecommendedNextLane="
            + result.Report.OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane,
        "offlineGeoworldAlphaPostAcceptanceRecommendedNextGoalId="
            + result.Report.OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId,
        "offlineGeoworldAlphaPostAcceptanceReadyLaneCount="
            + result.Report.OfflineGeoworldAlphaPostAcceptanceReadyLaneCount,
        "offlineGeoworldAlphaPostAcceptanceCandidateLaneCount="
            + result.Report.OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount,
        "offlineGeoworldAlphaPostAcceptanceBlockedLaneCount="
            + result.Report.OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount,
        "offlineGeoworldAlphaPostAcceptanceDoNotStartAutomatically="
            + result.Report.OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaPostAcceptanceQualityGatePassed="
            + result.Report.OfflineGeoworldAlphaPostAcceptanceQualityGatePassed
                .ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaPostAcceptanceEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "offlineGeoworldAlphaPostAcceptanceManualGateStatus: "
            + entry.OfflineGeoworldAlphaPostAcceptanceManualGateStatus,
        "offlineGeoworldAlphaPostAcceptanceHumanAccepted: "
            + entry.OfflineGeoworldAlphaPostAcceptanceHumanAccepted
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaPostAcceptanceManualResultSha256: "
            + entry.OfflineGeoworldAlphaPostAcceptanceManualResultSha256,
        "offlineGeoworldAlphaPostAcceptanceRecommendedNextLane: "
            + entry.OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane,
        "offlineGeoworldAlphaPostAcceptanceRecommendedNextGoalId: "
            + entry.OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId,
        "offlineGeoworldAlphaPostAcceptanceReadyLaneCount: "
            + entry.OfflineGeoworldAlphaPostAcceptanceReadyLaneCount,
        "offlineGeoworldAlphaPostAcceptanceCandidateLaneCount: "
            + entry.OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount,
        "offlineGeoworldAlphaPostAcceptanceBlockedLaneCount: "
            + entry.OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount,
        "offlineGeoworldAlphaPostAcceptanceDoNotStartAutomatically: "
            + entry.OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaPostAcceptanceEvidencePath: "
            + entry.OfflineGeoworldAlphaPostAcceptanceEvidencePath,
        "offlineGeoworldAlphaPostAcceptanceExportPath: "
            + entry.OfflineGeoworldAlphaPostAcceptanceExportPath,
        "offlineGeoworldAlphaPostAcceptanceLaneIds: "
            + entry.OfflineGeoworldAlphaPostAcceptanceLaneIds,
        "offlineGeoworldAlphaPostAcceptanceErrors: "
            + entry.OfflineGeoworldAlphaPostAcceptanceErrors,
        "offlineGeoworldAlphaPostAcceptanceWarnings: "
            + entry.OfflineGeoworldAlphaPostAcceptanceWarnings
    ];
}
