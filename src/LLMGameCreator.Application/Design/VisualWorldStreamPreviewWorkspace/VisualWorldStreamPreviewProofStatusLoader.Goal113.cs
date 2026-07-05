using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal113OfflineGeoworldAlphaManualResultWorkbenchProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory;
        var goalId = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal113.workbench.dashboard",
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DashboardFileName,
                "humanAcceptanceStillRequired", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal113.workbench.quality_gate",
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.QualityGateScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal113.workbench.no_result_no_acceptance",
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.NegativeNoResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal113.workbench.invalid_result_rejected",
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.NegativeInvalidResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal113.workbench.draft_template_only",
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DraftTemplateFileName,
                "draftTemplateOnly", ledger, diagnostics)
        ];
    }
}
