using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal137CanonicalRuntimeUnityPlayerLoopPlaybackProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory;
        var goalId = CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId;
        if (!Goal137DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal137.unity_player_loop_playback.dashboard",
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DashboardFileName,
                "unityPlayerLoopPlaybackPassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal137.unity_player_loop_playback.plan",
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.PlanFileName,
                "requiredFrameCategoriesPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal137.unity_player_loop_playback.result",
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ResultFileName,
                "runtimeSnapshotSource", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal137.unity_player_loop_playback.matrix",
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.MatrixResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal137.unity_player_loop_playback.unity_smoke",
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.UnitySmokeFileName,
                "unityPlayerLoopPlaybackPassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal137.unity_player_loop_playback.negative_proof",
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }

    private static bool Goal137DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal137String(dashboard.RootElement, "status") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
