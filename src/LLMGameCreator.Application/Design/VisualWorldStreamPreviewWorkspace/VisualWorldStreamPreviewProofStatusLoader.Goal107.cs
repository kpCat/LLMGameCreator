using LLMGameCreator.Application.Design.OfflineGeoworldObjectiveAcceptanceRun;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal107OfflineGeoworldObjectiveProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal107ObjectiveSourceRoot,
                Goal107ObjectiveSourceGoalId,
                "goal107.unity_script_inventory",
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.UnityScriptInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal107ObjectiveSourceRoot,
                Goal107ObjectiveSourceGoalId,
                "goal107.editor_window_inventory",
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.EditorWindowInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal107ObjectiveSourceRoot,
                Goal107ObjectiveSourceGoalId,
                "goal107.replay_acceptance",
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.SimulatedAcceptanceProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal107ObjectiveSourceRoot,
                Goal107ObjectiveSourceGoalId,
                "goal107.negative",
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.NegativeProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal107ObjectiveSourceRoot,
                Goal107ObjectiveSourceGoalId,
                "goal107.checkpoint_resume",
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.SimulatedAcceptanceProofFileName,
                "checkpointResumeApplied",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal107ObjectiveSourceRoot,
                Goal107ObjectiveSourceGoalId,
                "goal107.completion_transitions",
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.SimulatedAcceptanceProofFileName,
                "completionTransitionsPassed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal107ObjectiveSourceRoot,
                Goal107ObjectiveSourceGoalId,
                "goal107.alpha_quality_consolidation",
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.AlphaQualityConsolidationFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal107ObjectiveSourceRoot,
                Goal107ObjectiveSourceGoalId,
                "goal107.alpha_runtime_bootstrap_unchanged",
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.QualityGateScanFileName,
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal107ObjectiveSourceRoot,
                Goal107ObjectiveSourceGoalId,
                "goal107.quality_gate",
                OfflineGeoworldObjectiveAcceptanceRunVocabulary.QualityGateScanFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];
}
