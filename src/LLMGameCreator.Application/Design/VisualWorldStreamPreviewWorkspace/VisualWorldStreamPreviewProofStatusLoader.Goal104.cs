using LLMGameCreator.Application.Design.OfflineGeoworldInteractiveTravelPreview;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal104OfflineGeoworldInteractiveTravelProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.unity_script_inventory",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityScriptInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.editor_window_inventory",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.EditorWindowInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.simulated_execution",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.SimulatedExecutionProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.negative",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.NegativeProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.alpha_runtime_bootstrap_unchanged",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.boundary_crossings",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                "boundaryZonesBuilt",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.prefetch_plan",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                "prefetchPlanBuilt",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal104InteractiveTravelSourceRoot,
                Goal104InteractiveTravelSourceGoalId,
                "goal104.quality_gate",
                OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateScanFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];
}
