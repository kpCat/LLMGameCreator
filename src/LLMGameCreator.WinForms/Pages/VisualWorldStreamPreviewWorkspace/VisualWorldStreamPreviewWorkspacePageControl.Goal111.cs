using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaManualResultIntakeDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "offlineGeoworldAlphaManualResultIntakeDecisionStatus="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeDecisionStatus,
        "offlineGeoworldAlphaManualResultIntakeResultFilePresent="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeResultFilePresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakeAcceptableCandidate="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeAcceptableCandidate
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakeAcceptedByCodex="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakeChecklistHashMatched="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeChecklistHashMatched
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakePassedStepCount="
            + result.Report.OfflineGeoworldAlphaManualResultIntakePassedStepCount,
        "offlineGeoworldAlphaManualResultIntakeFailedStepCount="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeFailedStepCount,
        "offlineGeoworldAlphaManualResultIntakePendingStepCount="
            + result.Report.OfflineGeoworldAlphaManualResultIntakePendingStepCount,
        "offlineGeoworldAlphaManualResultIntakeSkippedStepCount="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeSkippedStepCount,
        "offlineGeoworldAlphaManualResultIntakeMissingStepCount="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeMissingStepCount,
        "offlineGeoworldAlphaManualResultIntakeDuplicateStepCount="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeDuplicateStepCount,
        "offlineGeoworldAlphaManualResultIntakeQualityGatePassed="
            + result.Report.OfflineGeoworldAlphaManualResultIntakeQualityGatePassed
                .ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaManualResultIntakeEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "offlineGeoworldAlphaManualResultIntakeGoal110PackagePresent: "
            + entry.OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakeResultFilePresent: "
            + entry.OfflineGeoworldAlphaManualResultIntakeResultFilePresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakeDecisionStatus: "
            + entry.OfflineGeoworldAlphaManualResultIntakeDecisionStatus,
        "offlineGeoworldAlphaManualResultIntakeAcceptableCandidate: "
            + entry.OfflineGeoworldAlphaManualResultIntakeAcceptableCandidate
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakeAcceptedByCodex: "
            + entry.OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired: "
            + entry.OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakeChecklistHashMatched: "
            + entry.OfflineGeoworldAlphaManualResultIntakeChecklistHashMatched
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultIntakePassedStepCount: "
            + entry.OfflineGeoworldAlphaManualResultIntakePassedStepCount,
        "offlineGeoworldAlphaManualResultIntakeFailedStepCount: "
            + entry.OfflineGeoworldAlphaManualResultIntakeFailedStepCount,
        "offlineGeoworldAlphaManualResultIntakePendingStepCount: "
            + entry.OfflineGeoworldAlphaManualResultIntakePendingStepCount,
        "offlineGeoworldAlphaManualResultIntakeSkippedStepCount: "
            + entry.OfflineGeoworldAlphaManualResultIntakeSkippedStepCount,
        "offlineGeoworldAlphaManualResultIntakeMissingStepCount: "
            + entry.OfflineGeoworldAlphaManualResultIntakeMissingStepCount,
        "offlineGeoworldAlphaManualResultIntakeDuplicateStepCount: "
            + entry.OfflineGeoworldAlphaManualResultIntakeDuplicateStepCount,
        "offlineGeoworldAlphaManualResultIntakeTopErrors: "
            + entry.OfflineGeoworldAlphaManualResultIntakeTopErrors,
        "offlineGeoworldAlphaManualResultIntakeTopWarnings: "
            + entry.OfflineGeoworldAlphaManualResultIntakeTopWarnings,
        "offlineGeoworldAlphaManualResultIntakeDecisionPath: "
            + entry.OfflineGeoworldAlphaManualResultIntakeDecisionPath,
        "offlineGeoworldAlphaManualResultIntakeExportPath: "
            + entry.OfflineGeoworldAlphaManualResultIntakeExportPath
    ];
}
