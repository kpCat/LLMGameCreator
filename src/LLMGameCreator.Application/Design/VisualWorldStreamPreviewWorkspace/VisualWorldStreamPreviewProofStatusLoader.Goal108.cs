using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus> BuildGoal108OfflineGeoworldAlphaSliceProofStatus(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics) =>
        [
            BuildProof(
                projectRoot,
                Goal108AlphaSliceSourceRoot,
                Goal108AlphaSliceSourceGoalId,
                "goal108.alpha_slice.unity_script_inventory",
                OfflineGeoworldAlphaSliceVocabulary.UnityScriptInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal108AlphaSliceSourceRoot,
                Goal108AlphaSliceSourceGoalId,
                "goal108.alpha_slice.editor_window_inventory",
                OfflineGeoworldAlphaSliceVocabulary.EditorWindowInventoryFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal108AlphaSliceSourceRoot,
                Goal108AlphaSliceSourceGoalId,
                "goal108.alpha_slice.full_slice_simulated_proof",
                OfflineGeoworldAlphaSliceVocabulary.SimulatedProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal108AlphaSliceSourceRoot,
                Goal108AlphaSliceSourceGoalId,
                "goal108.alpha_slice.negative_proof",
                OfflineGeoworldAlphaSliceVocabulary.NegativeProofFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal108AlphaSliceSourceRoot,
                Goal108AlphaSliceSourceGoalId,
                "goal108.alpha_slice.alpha_runtime_bootstrap_unchanged",
                OfflineGeoworldAlphaSliceVocabulary.QualityGateScanFileName,
                "alphaRuntimeBootstrapUnchanged",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics),
            BuildProof(
                projectRoot,
                Goal108AlphaSliceSourceRoot,
                Goal108AlphaSliceSourceGoalId,
                "goal108.alpha_slice.quality_gate",
                OfflineGeoworldAlphaSliceVocabulary.QualityGateScanFileName,
                "passed",
                new Dictionary<string, string>(StringComparer.Ordinal),
                diagnostics)
        ];
}
