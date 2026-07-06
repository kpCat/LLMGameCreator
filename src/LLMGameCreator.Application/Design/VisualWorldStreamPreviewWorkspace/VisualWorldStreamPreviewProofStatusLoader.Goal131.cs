using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal131GamePackageCandidateRecipePipelineProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory;
        var goalId = GamePackageCandidateRecipePipelineVocabulary.GoalId;
        if (!Goal131DashboardIsGreen(projectRoot, root))
        {
            return [];
        }

        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.catalog",
                GamePackageCandidateRecipePipelineVocabulary.RecipeCatalogFileName,
                "deterministic", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.candidate_index",
                GamePackageCandidateRecipePipelineVocabulary.CandidateIndexFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.script_scan",
                GamePackageCandidateRecipePipelineVocabulary.ScriptScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.pipeline_result",
                GamePackageCandidateRecipePipelineVocabulary.PipelineResultFileName,
                "matrixPassed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.scoring_result",
                GamePackageCandidateRecipePipelineVocabulary.ScoringResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.matrix_result",
                GamePackageCandidateRecipePipelineVocabulary.MatrixResultFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.selected_handoff",
                GamePackageCandidateRecipePipelineVocabulary.SelectedCandidateHandoffFileName,
                "samplePackageUnmodified", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.log_scan",
                GamePackageCandidateRecipePipelineVocabulary.LogScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.negative_proof",
                GamePackageCandidateRecipePipelineVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.sample_unmodified",
                GamePackageCandidateRecipePipelineVocabulary.DashboardFileName,
                "samplePackageUnmodified", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId,
                "goal131.gamepackage_candidate_recipe_pipeline.metadata_only",
                GamePackageCandidateRecipePipelineVocabulary.DashboardFileName,
                "metadataOnlyRecipeMutation", ledger, diagnostics)
        ];
    }

    private static bool Goal131DashboardIsGreen(string projectRoot, string root)
    {
        var relativePath =
            root + "/" + GamePackageCandidateRecipePipelineVocabulary.DashboardFileName;
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return false;
        }

        try
        {
            using var dashboard = JsonDocument.Parse(File.ReadAllText(fullPath));
            return Goal131String(dashboard.RootElement, "recipePipelineStatus") == "GREEN";
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
