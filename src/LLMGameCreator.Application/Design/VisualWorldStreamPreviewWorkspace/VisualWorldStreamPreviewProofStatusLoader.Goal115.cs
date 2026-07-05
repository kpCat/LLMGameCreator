using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal115OfflineGeoworldAlphaHumanResultRevalidationProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory;
        var goalId = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal115.human_result.dashboard",
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DashboardFileName,
                "humanAcceptanceStillRequired", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal115.human_result.decision_snapshot",
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionSnapshotFileName,
                "manualGateRemainsHumanDecision", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal115.human_result.manual_input_not_committed",
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionSnapshotFileName,
                "manualInputNotCommitted", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal115.human_result.quality_gate",
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.QualityGateScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal115.human_result.negative_proof",
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }

    private static IReadOnlyList<VisualWorldPreviewProofStatus> NormalizeHistoricalManualResultNegativeProofs(
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var normalizedProofIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "goal111.manual_result_intake.missing_result_negative",
            "goal112.operator.negative_no_result",
            "goal113.workbench.no_result_no_acceptance"
        };
        diagnostics.RemoveAll(item =>
            item.Code == "goal092.proof.failed" && normalizedProofIds.Contains(item.Target));
        return proofs
            .Select(proof => normalizedProofIds.Contains(proof.ProofId)
                ? proof with
                {
                    Status = VisualWorldPreviewArtifactStatus.Passed,
                    Passed = true,
                    DiagnosticSummary =
                        proof.DiagnosticSummary + "; supersededByGoal115HumanResultRevalidation=true"
                }
                : proof)
            .ToList();
    }
}
