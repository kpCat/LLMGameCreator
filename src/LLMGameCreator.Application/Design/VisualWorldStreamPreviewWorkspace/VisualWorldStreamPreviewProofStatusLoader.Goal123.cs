using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal123GenericGamePackageProjectionProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory;
        var goalId = GenericGamePackageProjectionVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal123.generic_projection.sample_package",
                GenericGamePackageProjectionVocabulary.DashboardFileName,
                "samplePackageReadOnly", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal123.generic_projection.goal122_still_green",
                GenericGamePackageProjectionVocabulary.DashboardFileName,
                "goal122StillGreen", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal123.generic_projection.cleanup_script",
                GenericGamePackageProjectionVocabulary.DashboardFileName,
                "cleanupScriptAvailable", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal123.generic_projection.script_inventory",
                GenericGamePackageProjectionVocabulary.ScriptInventoryFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal123.generic_projection.negative_proof",
                GenericGamePackageProjectionVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
