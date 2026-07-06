using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal125GenericGamePackageSystemsProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory;
        var goalId = GenericGamePackageSystemsProjectionVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal125.generic_systems.sample_package",
                GenericGamePackageSystemsProjectionVocabulary.DashboardFileName,
                "samplePackageReadOnly", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal125.generic_systems.goal124_still_green",
                GenericGamePackageSystemsProjectionVocabulary.DashboardFileName,
                "goal124StillGreen", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal125.generic_systems.cleanup_script",
                GenericGamePackageSystemsProjectionVocabulary.DashboardFileName,
                "cleanupScriptAvailable", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal125.generic_systems.script_inventory",
                GenericGamePackageSystemsProjectionVocabulary.ScriptInventoryFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal125.generic_systems.negative_proof",
                GenericGamePackageSystemsProjectionVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
