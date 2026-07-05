using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal119AcceptedAlphaUnityPlayableProjectionProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory;
        var goalId = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal119.projection.accepted_baseline",
                AcceptedAlphaUnityPlayableProjectionVocabulary.DashboardFileName,
                "acceptedBaselineReady", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal119.projection.quality_gate",
                AcceptedAlphaUnityPlayableProjectionVocabulary.QualityGateScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal119.projection.menu_path",
                AcceptedAlphaUnityPlayableProjectionVocabulary.ScriptInventoryFileName,
                "menuPathExistsExactly", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal119.projection.unity_scripts",
                AcceptedAlphaUnityPlayableProjectionVocabulary.ScriptInventoryFileName,
                "allScriptsPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal119.projection.smoke_baseline",
                AcceptedAlphaUnityPlayableProjectionVocabulary.SmokePlanFileName,
                "baselineLoaded", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal119.projection.smoke_player",
                AcceptedAlphaUnityPlayableProjectionVocabulary.SmokePlanFileName,
                "hasPlayerProxyStep", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal119.projection.smoke_chunk",
                AcceptedAlphaUnityPlayableProjectionVocabulary.SmokePlanFileName,
                "hasChunkWindowStep", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal119.projection.smoke_interaction_objective",
                AcceptedAlphaUnityPlayableProjectionVocabulary.SmokePlanFileName,
                "hasInteractionOrObjectiveStep", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal119.projection.negative_proof",
                AcceptedAlphaUnityPlayableProjectionVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal119.projection.manual_input_excluded",
                AcceptedAlphaUnityPlayableProjectionVocabulary.QualityGateScanFileName,
                "manualInputExcluded", ledger, diagnostics)
        ];
    }
}
