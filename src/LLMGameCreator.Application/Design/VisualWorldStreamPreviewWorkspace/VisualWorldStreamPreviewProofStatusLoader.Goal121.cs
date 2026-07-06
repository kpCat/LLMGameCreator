using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal121AcceptedAlphaInteractionDrilldownProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory;
        var goalId = AcceptedAlphaInteractionDrilldownVerificationVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal121.full_verification.one_click_button",
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
                "oneClickButtonPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal121.full_verification.drilldown_fields",
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
                "drilldownFieldsPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal121.full_verification.interaction_preview",
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
                "interactionPreviewPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal121.full_verification.objective_replay_details",
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
                "objectiveReplayDetailsPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal121.full_verification.cleanup_script",
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
                "cleanupScriptAvailable", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal121.full_verification.material_warning_guard",
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
                "materialWarningGuardPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal121.full_verification.human_steps_one_button",
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.DashboardFileName,
                "humanManualStepsReducedToOneButton", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal121.full_verification.negative_proof",
                AcceptedAlphaInteractionDrilldownVerificationVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
