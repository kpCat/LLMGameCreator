using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal110OfflineGeoworldAlphaManualAcceptanceProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory;
        var goalId = OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal110.manual_acceptance.manifest",
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ManifestFileName,
                "manualAcceptancePending", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal110.manual_acceptance.checklist",
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ChecklistFileName,
                "manualAcceptancePending", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal110.manual_acceptance.result_template",
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ResultTemplateFileName,
                "manualAcceptancePending", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal110.manual_acceptance.dashboard",
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.DashboardFileName,
                "manualAcceptancePending", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal110.manual_acceptance.unity_scripts",
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityScriptInventoryFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal110.manual_acceptance.editor_window",
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.EditorWindowInventoryFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal110.manual_acceptance.simulated_proof",
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.SimulatedProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal110.manual_acceptance.negative_proof",
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal110.manual_acceptance.workspace_binding",
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.WorkspaceBindingInventoryFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal110.manual_acceptance.quality_gate",
                OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.QualityGateScanFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
