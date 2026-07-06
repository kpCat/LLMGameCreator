using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;
using System.Text.Json;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal128ParameterizedGamePackageRunnerProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory;
        var goalId = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId;
        if (!Goal128DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal128.parameterized_gamepackage_runner.goal127_green",
                ParameterizedGamePackageProjectionRunnerVocabulary.DashboardFileName,
                "goal127RunnerGreen", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal128.parameterized_gamepackage_runner.script_scan",
                ParameterizedGamePackageProjectionRunnerVocabulary.ScriptScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal128.parameterized_gamepackage_runner.unity_source_scan",
                ParameterizedGamePackageProjectionRunnerVocabulary.UnitySourceScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal128.parameterized_gamepackage_runner.result",
                ParameterizedGamePackageProjectionRunnerVocabulary.ResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal128.parameterized_gamepackage_runner.log_scan",
                ParameterizedGamePackageProjectionRunnerVocabulary.LogScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal128.parameterized_gamepackage_runner.negative_proof",
                ParameterizedGamePackageProjectionRunnerVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal128.parameterized_gamepackage_runner.cleanup_applied",
                ParameterizedGamePackageProjectionRunnerVocabulary.DashboardFileName,
                "cleanupApplied", ledger, diagnostics)
        ];
    }

    private static bool Goal128DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + ParameterizedGamePackageProjectionRunnerVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal128String(dashboard.RootElement, "parameterizedRunnerStatus") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
