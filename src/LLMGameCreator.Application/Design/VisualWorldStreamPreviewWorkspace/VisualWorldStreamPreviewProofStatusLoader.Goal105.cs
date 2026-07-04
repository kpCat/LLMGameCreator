using LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal105OfflineGeoworldInteractionProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal105InteractionSourceRoot,
                Goal105InteractionSourceGoalId,
                "goal105.unity_script_inventory",
                OfflineGeoworldInteractionPlayableProbeVocabulary.UnityScriptInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal105InteractionSourceRoot,
                Goal105InteractionSourceGoalId,
                "goal105.editor_window_inventory",
                OfflineGeoworldInteractionPlayableProbeVocabulary.EditorWindowInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal105InteractionSourceRoot,
                Goal105InteractionSourceGoalId,
                "goal105.simulated_session",
                OfflineGeoworldInteractionPlayableProbeVocabulary.SimulatedSessionProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal105InteractionSourceRoot,
                Goal105InteractionSourceGoalId,
                "goal105.negative",
                OfflineGeoworldInteractionPlayableProbeVocabulary.NegativeProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal105InteractionSourceRoot,
                Goal105InteractionSourceGoalId,
                "goal105.alpha_runtime_bootstrap_unchanged",
                OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateScanFileName,
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal105InteractionSourceRoot,
                Goal105InteractionSourceGoalId,
                "goal105.state_hash_chain",
                OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateScanFileName,
                "stateHashChainPassed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal105InteractionSourceRoot,
                Goal105InteractionSourceGoalId,
                "goal105.quality_gate",
                OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateScanFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];
}
