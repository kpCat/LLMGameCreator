using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal138RuntimeBackedUnityPlayerLoopStepperProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory;
        var goalId = RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId;
        if (!Goal138DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal138.runtime_backed_stepper.dashboard",
                RuntimeBackedUnityPlayerLoopStepperVocabulary.DashboardFileName,
                "stepperBatchSmokePassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal138.runtime_backed_stepper.model",
                RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelFileName,
                "requiredFrameCategoriesPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal138.runtime_backed_stepper.result",
                RuntimeBackedUnityPlayerLoopStepperVocabulary.ResultFileName,
                "runtimeAuthority", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal138.runtime_backed_stepper.unity_smoke",
                RuntimeBackedUnityPlayerLoopStepperVocabulary.UnitySmokeFileName,
                "stepperBatchSmokePassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal138.runtime_backed_stepper.negative_proof",
                RuntimeBackedUnityPlayerLoopStepperVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal138.runtime_backed_stepper.goal137_acceptance",
                RuntimeBackedUnityPlayerLoopStepperVocabulary.Goal137AcceptanceFileName,
                "acceptedByHuman", ledger, diagnostics)
        ];
    }

    private static bool Goal138DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + RuntimeBackedUnityPlayerLoopStepperVocabulary.DashboardFileName;
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
