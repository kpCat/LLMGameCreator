using LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal117OfflineGeoworldAlphaPostAcceptanceContinuationProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
            .ProceduralOutputDirectory;
        var goalId = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal117.continuation.do_not_start",
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .DashboardFileName,
                "doNotStartAutomatically", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal117.continuation.recommended_lane",
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .QualityGateScanFileName,
                "recommendedLaneSelected", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal117.continuation.required_lanes",
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .QualityGateScanFileName,
                "allRequiredLanesPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal117.continuation.manual_input_excluded",
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .QualityGateScanFileName,
                "manualInputExcluded", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal117.continuation.quality_gate",
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .QualityGateScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal117.continuation.no_goal118_task_files",
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .QualityGateScanFileName,
                "noGoal118TaskFilesCreated", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal117.continuation.negative_proof",
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
