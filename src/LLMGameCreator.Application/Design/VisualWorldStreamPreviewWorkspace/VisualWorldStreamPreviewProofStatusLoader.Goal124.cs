using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal124GenericGamePackageLoopProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory;
        var goalId = GenericGamePackageLoopProjectionVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal124.generic_loop.sample_package",
                GenericGamePackageLoopProjectionVocabulary.DashboardFileName,
                "samplePackageReadOnly", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal124.generic_loop.goal123_still_green",
                GenericGamePackageLoopProjectionVocabulary.DashboardFileName,
                "goal123StillGreen", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal124.generic_loop.cleanup_script",
                GenericGamePackageLoopProjectionVocabulary.DashboardFileName,
                "cleanupScriptAvailable", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal124.generic_loop.script_inventory",
                GenericGamePackageLoopProjectionVocabulary.ScriptInventoryFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal124.generic_loop.negative_proof",
                GenericGamePackageLoopProjectionVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
