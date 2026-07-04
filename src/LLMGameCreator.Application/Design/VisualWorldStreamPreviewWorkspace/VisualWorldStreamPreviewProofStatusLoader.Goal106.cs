using LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal106OfflineGeoworldSessionProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal106SessionSourceRoot,
                Goal106SessionSourceGoalId,
                "goal106.unity_script_inventory",
                OfflineGeoworldSessionPersistenceReplayVocabulary.UnityScriptInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal106SessionSourceRoot,
                Goal106SessionSourceGoalId,
                "goal106.editor_window_inventory",
                OfflineGeoworldSessionPersistenceReplayVocabulary.EditorWindowInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal106SessionSourceRoot,
                Goal106SessionSourceGoalId,
                "goal106.simulated_save_load_replay",
                OfflineGeoworldSessionPersistenceReplayVocabulary.SimulatedReplayProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal106SessionSourceRoot,
                Goal106SessionSourceGoalId,
                "goal106.negative",
                OfflineGeoworldSessionPersistenceReplayVocabulary.NegativeProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal106SessionSourceRoot,
                Goal106SessionSourceGoalId,
                "goal106.alpha_runtime_bootstrap_unchanged",
                OfflineGeoworldSessionPersistenceReplayVocabulary.QualityGateScanFileName,
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal106SessionSourceRoot,
                Goal106SessionSourceGoalId,
                "goal106.checkpoint_resume",
                OfflineGeoworldSessionPersistenceReplayVocabulary.SimulatedReplayProofFileName,
                "checkpointLoaded",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal106SessionSourceRoot,
                Goal106SessionSourceGoalId,
                "goal106.final_hash",
                OfflineGeoworldSessionPersistenceReplayVocabulary.SimulatedReplayProofFileName,
                "replayResumedToFinalHash",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal106SessionSourceRoot,
                Goal106SessionSourceGoalId,
                "goal106.quality_gate",
                OfflineGeoworldSessionPersistenceReplayVocabulary.QualityGateScanFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];
}
