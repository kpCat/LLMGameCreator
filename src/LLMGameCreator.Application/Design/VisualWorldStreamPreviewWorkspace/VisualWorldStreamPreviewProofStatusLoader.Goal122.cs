using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal122AcceptedAlphaProjectionActionLoopProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory;
        var goalId = AcceptedAlphaProjectionActionLoopVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal122.action_loop.one_click_verification",
                AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName,
                "oneClickVerificationStillPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal122.action_loop.preview_apply_reset",
                AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName,
                "projectionStateResetPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal122.action_loop.window_polish",
                AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName,
                "windowLayoutPolishPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal122.action_loop.cleanup_script",
                AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName,
                "cleanupScriptAvailable", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal122.action_loop.negative_proof",
                AcceptedAlphaProjectionActionLoopVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
