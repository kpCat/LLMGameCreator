using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaAcceptanceOperatorDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "offlineGeoworldAlphaAcceptanceOperatorStatus="
            + result.Report.OfflineGeoworldAlphaAcceptanceOperatorStatus,
        "offlineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus="
            + result.Report.OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus,
        "offlineGeoworldAlphaAcceptanceOperatorManualResultPresent="
            + result.Report.OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview="
            + result.Report.OfflineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaAcceptanceOperatorAcceptedByCodex="
            + result.Report.OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired="
            + result.Report.OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaAcceptanceOperatorChecklistStepCount="
            + result.Report.OfflineGeoworldAlphaAcceptanceOperatorChecklistStepCount,
        "offlineGeoworldAlphaAcceptanceOperatorChecklistHashPresent="
            + result.Report.OfflineGeoworldAlphaAcceptanceOperatorChecklistHashPresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaAcceptanceOperatorQualityGatePassed="
            + result.Report.OfflineGeoworldAlphaAcceptanceOperatorQualityGatePassed
                .ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaAcceptanceOperatorEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "offlineGeoworldAlphaAcceptanceOperatorStatus: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorStatus,
        "offlineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus,
        "offlineGeoworldAlphaAcceptanceOperatorManualResultPresent: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaAcceptanceOperatorPreferredManualResultPath: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorPreferredManualResultPath,
        "offlineGeoworldAlphaAcceptanceOperatorCandidateManualResultPaths: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorCandidateManualResultPaths,
        "offlineGeoworldAlphaAcceptanceOperatorChecklistStepCount: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorChecklistStepCount,
        "offlineGeoworldAlphaAcceptanceOperatorChecklistHashPresent: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorChecklistHashPresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaAcceptanceOperatorAcceptedByCodex: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaAcceptanceOperatorNextHumanActions: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorNextHumanActions,
        "offlineGeoworldAlphaAcceptanceOperatorDoNotStartYet: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorDoNotStartYet,
        "offlineGeoworldAlphaAcceptanceOperatorEvidencePath: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorEvidencePath,
        "offlineGeoworldAlphaAcceptanceOperatorExportPath: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorExportPath,
        "offlineGeoworldAlphaAcceptanceOperatorRunbookPath: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorRunbookPath,
        "offlineGeoworldAlphaAcceptanceOperatorTopErrors: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorTopErrors,
        "offlineGeoworldAlphaAcceptanceOperatorTopWarnings: "
            + entry.OfflineGeoworldAlphaAcceptanceOperatorTopWarnings
    ];
}
