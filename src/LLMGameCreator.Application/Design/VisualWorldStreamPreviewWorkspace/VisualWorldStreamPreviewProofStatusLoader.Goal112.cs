using LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal112OfflineGeoworldAlphaAcceptanceOperatorPackProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory;
        var goalId = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal112.operator.dashboard",
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.DashboardFileName,
                "humanAcceptanceStillRequired", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal112.operator.quality_gate",
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.QualityGateScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal112.operator.negative_no_result",
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal112.operator.notary_boundary",
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.NotaryBoundaryFileName,
                "humanAcceptanceStillRequired", ledger, diagnostics)
        ];
    }
}
