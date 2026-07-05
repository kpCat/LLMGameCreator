using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildOfflineGeoworldAcceptedAlphaBaselineDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "offlineGeoworldAcceptedAlphaBaselineId="
            + result.Report.OfflineGeoworldAcceptedAlphaBaselineId,
        "offlineGeoworldAcceptedAlphaBaselineHash="
            + result.Report.OfflineGeoworldAcceptedAlphaBaselineHash,
        "offlineGeoworldAcceptedAlphaBaselineReady="
            + result.Report.OfflineGeoworldAcceptedAlphaBaselineReady
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAcceptedAlphaManualGateStatus="
            + result.Report.OfflineGeoworldAcceptedAlphaManualGateStatus,
        "offlineGeoworldAcceptedAlphaRecommendedNextDecision="
            + result.Report.OfflineGeoworldAcceptedAlphaRecommendedNextDecision,
        "offlineGeoworldAcceptedAlphaIncludedSourceGoalCount="
            + result.Report.OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount,
        "offlineGeoworldAcceptedAlphaAcceptedEvidenceRootCount="
            + result.Report.OfflineGeoworldAcceptedAlphaAcceptedEvidenceRootCount,
        "offlineGeoworldAcceptedAlphaProducedOnlyRootCount="
            + result.Report.OfflineGeoworldAcceptedAlphaProducedOnlyRootCount,
        "offlineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount="
            + result.Report.OfflineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount,
        "offlineGeoworldAcceptedAlphaDoNotStartAutomatically="
            + result.Report.OfflineGeoworldAcceptedAlphaDoNotStartAutomatically
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAcceptedAlphaQualityGatePassed="
            + result.Report.OfflineGeoworldAcceptedAlphaQualityGatePassed
                .ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildOfflineGeoworldAcceptedAlphaBaselineEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "offlineGeoworldAcceptedAlphaBaselineId: "
            + entry.OfflineGeoworldAcceptedAlphaBaselineId,
        "offlineGeoworldAcceptedAlphaBaselineHash: "
            + entry.OfflineGeoworldAcceptedAlphaBaselineHash,
        "offlineGeoworldAcceptedAlphaBaselineReady: "
            + entry.OfflineGeoworldAcceptedAlphaBaselineReady.ToString().ToLowerInvariant(),
        "offlineGeoworldAcceptedAlphaManualGateStatus: "
            + entry.OfflineGeoworldAcceptedAlphaManualGateStatus,
        "offlineGeoworldAcceptedAlphaRecommendedNextDecision: "
            + entry.OfflineGeoworldAcceptedAlphaRecommendedNextDecision,
        "offlineGeoworldAcceptedAlphaIncludedSourceGoalCount: "
            + entry.OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount,
        "offlineGeoworldAcceptedAlphaAcceptedEvidenceRootCount: "
            + entry.OfflineGeoworldAcceptedAlphaAcceptedEvidenceRootCount,
        "offlineGeoworldAcceptedAlphaProducedOnlyRootCount: "
            + entry.OfflineGeoworldAcceptedAlphaProducedOnlyRootCount,
        "offlineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount: "
            + entry.OfflineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount,
        "offlineGeoworldAcceptedAlphaDoNotStartAutomatically: "
            + entry.OfflineGeoworldAcceptedAlphaDoNotStartAutomatically
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAcceptedAlphaEvidencePath: "
            + entry.OfflineGeoworldAcceptedAlphaEvidencePath,
        "offlineGeoworldAcceptedAlphaExportPath: "
            + entry.OfflineGeoworldAcceptedAlphaExportPath,
        "offlineGeoworldAcceptedAlphaErrors: "
            + entry.OfflineGeoworldAcceptedAlphaErrors,
        "offlineGeoworldAcceptedAlphaWarnings: "
            + entry.OfflineGeoworldAcceptedAlphaWarnings
    ];
}
