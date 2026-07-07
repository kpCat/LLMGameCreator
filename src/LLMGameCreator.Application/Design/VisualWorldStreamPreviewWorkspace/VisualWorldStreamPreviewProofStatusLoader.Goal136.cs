using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal136CanonicalRuntimePlayerCommandLoopProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory;
        var goalId = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId;
        if (!Goal136DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal136.player_command_loop.dashboard",
                CanonicalRuntimePlayerCommandLoopVocabulary.DashboardFileName,
                "playerCommandLoopPassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal136.player_command_loop.plan",
                CanonicalRuntimePlayerCommandLoopVocabulary.PlanFileName,
                "allRequiredCategoriesPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal136.player_command_loop.matrix",
                CanonicalRuntimePlayerCommandLoopVocabulary.MatrixResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal136.player_command_loop.unity_smoke",
                CanonicalRuntimePlayerCommandLoopVocabulary.UnitySmokeFileName,
                "unityPlayerConsumedCommandLoopSnapshots", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal136.player_command_loop.diagnostic_classification",
                CanonicalRuntimePlayerCommandLoopVocabulary.DiagnosticClassificationFileName,
                "noUnclassifiedErrorDiagnostics", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal136.player_command_loop.negative_proof",
                CanonicalRuntimePlayerCommandLoopVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics)
        ];
    }

    private static bool Goal136DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + CanonicalRuntimePlayerCommandLoopVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal136String(dashboard.RootElement, "status") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
