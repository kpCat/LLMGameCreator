using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal116OfflineGeoworldAlphaManualGateAcceptanceRecordProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory;
        var goalId = OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal116.manual_gate.human_accepted",
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName,
                "humanAccepted", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal116.manual_gate.manual_input_not_committed",
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName,
                "manualInputNotCommitted", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal116.manual_gate.not_final_release",
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName,
                "notFinalReleaseOrRuntimeBuild", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal116.manual_gate.no_runtime_provider_network",
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName,
                "noRuntimeProviderOrNetworkChanges", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal116.manual_gate.no_unity_file_changes",
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName,
                "noUnityFileChangesRequired", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal116.manual_gate.quality_gate",
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.QualityGateScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal116.manual_gate.negative_proof",
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
