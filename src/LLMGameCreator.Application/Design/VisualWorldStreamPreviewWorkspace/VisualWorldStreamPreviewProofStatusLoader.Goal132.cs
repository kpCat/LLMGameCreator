using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal132CandidatePipelineOperatorProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GamePackageCandidatePipelineOperatorVocabulary.ProceduralOutputDirectory;
        var goalId = GamePackageCandidatePipelineOperatorVocabulary.GoalId;
        if (!Goal132DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.dashboard",
                GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName,
                "winFormsPanelPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.refresh_button",
                GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName,
                "refreshButtonPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.copy_button",
                GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName,
                "copyCommandButtonPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.dry_run_button",
                GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName,
                "dryRunButtonPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.run_button",
                GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName,
                "runButtonPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.async_run",
                GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName,
                "asyncRunPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.result",
                GamePackageCandidatePipelineOperatorVocabulary.ResultFileName,
                "operatorResultCaptured", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.script_scan",
                GamePackageCandidatePipelineOperatorVocabulary.ScriptScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.winforms_scan",
                GamePackageCandidatePipelineOperatorVocabulary.WinFormsScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.negative_proof",
                GamePackageCandidatePipelineOperatorVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.manual_unity_optional",
                GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName,
                "manualUnityOptional", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.projection_only",
                GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName,
                "projectionOnly", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal132.candidate_pipeline_operator.sample_read_only",
                GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName,
                "samplePackageReadOnly", ledger, diagnostics)
        ];
    }

    private static bool Goal132DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + GamePackageCandidatePipelineOperatorVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal132String(dashboard.RootElement, "operatorStatus") == "GREEN_READY";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
