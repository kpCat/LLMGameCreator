using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal139RuntimeBackedUnityPlayerLoopInteractiveControlsProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory;
        var goalId = RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
        if (!Goal139DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal139.runtime_backed_interactive_controls.dashboard",
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DashboardFileName,
                "unityInteractiveControlsSmokePassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal139.runtime_backed_interactive_controls.model",
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelFileName,
                "requiredControlsPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal139.runtime_backed_interactive_controls.script",
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ControlScriptFileName,
                "deterministic", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal139.runtime_backed_interactive_controls.session",
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.SessionFileName,
                "controlScriptPassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal139.runtime_backed_interactive_controls.result",
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ResultFileName,
                "runtimeAuthority", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal139.runtime_backed_interactive_controls.unity_smoke",
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.UnitySmokeFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal139.runtime_backed_interactive_controls.negative_proof",
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal139.runtime_backed_interactive_controls.goal138_acceptance",
                RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.Goal138AcceptanceFileName,
                "acceptedByHuman", ledger, diagnostics)
        ];
    }

    private static bool Goal139DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DashboardFileName;
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
