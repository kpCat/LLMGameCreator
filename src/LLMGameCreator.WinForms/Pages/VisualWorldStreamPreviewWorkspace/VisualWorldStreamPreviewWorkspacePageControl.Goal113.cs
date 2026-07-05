using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaManualResultWorkbenchDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "offlineGeoworldAlphaManualResultWorkbenchStatus="
            + result.Report.OfflineGeoworldAlphaManualResultWorkbenchStatus,
        "offlineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus="
            + result.Report.OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus,
        "offlineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus="
            + result.Report.OfflineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus,
        "offlineGeoworldAlphaManualResultWorkbenchManualResultPresent="
            + result.Report.OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultWorkbenchAcceptedByCodex="
            + result.Report.OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired="
            + result.Report.OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly="
            + result.Report.OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultWorkbenchChecklistStepCount="
            + result.Report.OfflineGeoworldAlphaManualResultWorkbenchChecklistStepCount,
        "offlineGeoworldAlphaManualResultWorkbenchQualityGatePassed="
            + result.Report.OfflineGeoworldAlphaManualResultWorkbenchQualityGatePassed
                .ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaManualResultWorkbenchEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "offlineGeoworldAlphaManualResultWorkbenchStatus: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchStatus,
        "offlineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus,
        "offlineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus,
        "offlineGeoworldAlphaManualResultWorkbenchManualResultPresent: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultWorkbenchPreferredManualResultPath: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchPreferredManualResultPath,
        "offlineGeoworldAlphaManualResultWorkbenchDraftTemplatePath: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchDraftTemplatePath,
        "offlineGeoworldAlphaManualResultWorkbenchCandidateManualResultPaths: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchCandidateManualResultPaths,
        "offlineGeoworldAlphaManualResultWorkbenchChecklistStepCount: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchChecklistStepCount,
        "offlineGeoworldAlphaManualResultWorkbenchChecklistHash: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchChecklistHash,
        "offlineGeoworldAlphaManualResultWorkbenchRequiredStepsSummary: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchRequiredStepsSummary,
        "offlineGeoworldAlphaManualResultWorkbenchValidationErrors: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchValidationErrors,
        "offlineGeoworldAlphaManualResultWorkbenchValidationWarnings: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchValidationWarnings,
        "offlineGeoworldAlphaManualResultWorkbenchAcceptedByCodex: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaManualResultWorkbenchNextHumanActions: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchNextHumanActions,
        "offlineGeoworldAlphaManualResultWorkbenchDoNotStartYet: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchDoNotStartYet,
        "offlineGeoworldAlphaManualResultWorkbenchProceduralPath: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchProceduralPath,
        "offlineGeoworldAlphaManualResultWorkbenchExportPath: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchExportPath,
        "offlineGeoworldAlphaManualResultWorkbenchRunbookPath: "
            + entry.OfflineGeoworldAlphaManualResultWorkbenchRunbookPath
    ];
}
