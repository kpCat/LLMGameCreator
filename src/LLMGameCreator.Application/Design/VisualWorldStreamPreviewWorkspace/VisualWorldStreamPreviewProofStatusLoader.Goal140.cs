using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal140RuntimeBackedUnityPlayerLoopControlsUxProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory;
        var goalId = RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
        if (!Goal140DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal140.runtime_backed_controls_ux.dashboard",
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DashboardFileName,
                "unityControlsUxSmokePassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal140.runtime_backed_controls_ux.model",
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ModelFileName,
                "controlsUxPolished", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal140.runtime_backed_controls_ux.script",
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ScriptFileName,
                "deterministic", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal140.runtime_backed_controls_ux.result",
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ResultFileName,
                "runtimeAuthority", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal140.runtime_backed_controls_ux.unity_smoke",
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.UnitySmokeFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal140.runtime_backed_controls_ux.unity_noise_classification",
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.UnityNoiseClassificationFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal140.runtime_backed_controls_ux.negative_proof",
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal140.runtime_backed_controls_ux.goal139_acceptance",
                RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.Goal139AcceptanceFileName,
                "acceptedByHuman", ledger, diagnostics)
        ];
    }

    private static bool Goal140DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal138String(dashboard.RootElement, "status") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
