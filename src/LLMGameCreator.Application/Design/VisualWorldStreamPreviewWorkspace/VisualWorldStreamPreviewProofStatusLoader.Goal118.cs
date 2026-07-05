using LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal118OfflineGeoworldAcceptedAlphaBaselineReviewProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory;
        var goalId = OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal118.baseline.ready",
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.DashboardFileName,
                "acceptedBaselineReady", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal118.baseline.quality_gate",
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.QualityGateScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal118.baseline.source_chain",
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceIndexFileName,
                "goal098To117ChainIncluded", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal118.baseline.manual_input_excluded",
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.QualityGateScanFileName,
                "manualInputExcluded", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal118.baseline.negative_proof",
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal118.baseline.not_final_release",
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.QualityGateScanFileName,
                "notFinalReleaseOrRuntimeBuild", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal118.baseline.no_unity_changes",
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.QualityGateScanFileName,
                "noUnityFileChangesRequired", ledger, diagnostics)
        ];
    }
}
