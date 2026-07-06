using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal126GenericGamePackageFullPlaythroughProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory;
        var goalId = GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal126.generic_full_playthrough.sample_package",
                GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName,
                "samplePackageReadOnly", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal126.generic_full_playthrough.goal125_still_green",
                GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName,
                "goal125StillGreen", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal126.generic_full_playthrough.cleanup_script",
                GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName,
                "cleanupScriptAvailable", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal126.generic_full_playthrough.script_inventory",
                GenericGamePackageFullPlaythroughProjectionVocabulary.ScriptInventoryFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal126.generic_full_playthrough.negative_proof",
                GenericGamePackageFullPlaythroughProjectionVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
