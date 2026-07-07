using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal135CanonicalRuntimePlayerLoopReadinessProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory;
        var goalId = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId;
        if (!Goal135DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal135.player_loop.dashboard",
                CanonicalRuntimePlayerLoopReadinessVocabulary.DashboardFileName,
                "canonicalRuntimeSource", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal135.player_loop.plan",
                CanonicalRuntimePlayerLoopReadinessVocabulary.PlayerLoopPlanFileName,
                "requiredStepCategoriesPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal135.player_loop.diagnostic_classification",
                CanonicalRuntimePlayerLoopReadinessVocabulary.DiagnosticClassificationFileName,
                "noUnclassifiedErrorDiagnostics", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal135.player_loop.unity_readiness_smoke",
                CanonicalRuntimePlayerLoopReadinessVocabulary.UnitySmokeFileName,
                "unityPlayerLoopReadinessPassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal135.player_loop.matrix",
                CanonicalRuntimePlayerLoopReadinessVocabulary.MatrixResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal135.player_loop.negative_proof",
                CanonicalRuntimePlayerLoopReadinessVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }

    private static bool Goal135DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + CanonicalRuntimePlayerLoopReadinessVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal135String(dashboard.RootElement, "status") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
