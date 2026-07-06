using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal127UnityProjectionVerificationRunnerProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory;
        var goalId = UnityProjectionVerificationRunnerVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal127.unity_projection_runner.goal126_green",
                UnityProjectionVerificationRunnerVocabulary.DashboardFileName,
                "goal126FullPlaythroughGreen", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal127.unity_projection_runner.script_scan",
                UnityProjectionVerificationRunnerVocabulary.ScriptScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal127.unity_projection_runner.result",
                UnityProjectionVerificationRunnerVocabulary.ResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal127.unity_projection_runner.log_scan",
                UnityProjectionVerificationRunnerVocabulary.LogScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal127.unity_projection_runner.negative_proof",
                UnityProjectionVerificationRunnerVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal127.unity_projection_runner.cleanup_applied",
                UnityProjectionVerificationRunnerVocabulary.DashboardFileName,
                "cleanupApplied", ledger, diagnostics)
        ];
    }
}
