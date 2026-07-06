using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal131GamePackageCandidateRecipePipelineQuality
        BuildGoal131GamePackageCandidateRecipePipelineQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "gamepackage_candidate_recipe_catalog_scoring_and_promotion");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "gamepackage_candidate_recipe_pipeline_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal131AllowedPath(entry.RelativePath));
        var qualityGatePassed =
            ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.catalog")
            && ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.candidate_index")
            && ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.script_scan")
            && ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.pipeline_result")
            && ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.scoring_result")
            && ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.matrix_result")
            && ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.selected_handoff")
            && ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.log_scan")
            && ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.negative_proof")
            && ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.sample_unmodified")
            && ProofPassed(proofs, "goal131.gamepackage_candidate_recipe_pipeline.metadata_only")
            && summary?.GamePackageCandidateRecipePipelineStatus == "GREEN"
            && summary?.GamePackageCandidateRecipePipelineRecipeCount >= 4
            && summary?.GamePackageCandidateRecipePipelineCandidateCount >= 4
            && summary?.GamePackageCandidateRecipePipelinePassedCandidates
                == summary?.GamePackageCandidateRecipePipelineCandidateCount
            && summary?.GamePackageCandidateRecipePipelineFailedCandidates == 0
            && summary?.GamePackageCandidateRecipePipelineMatrixPassed == true
            && !string.IsNullOrWhiteSpace(
                summary?.GamePackageCandidateRecipePipelineSelectedCandidateId)
            && summary?.GamePackageCandidateRecipePipelineSelectedCandidateScore > 0
            && summary?.GamePackageCandidateRecipePipelineRecipeCatalogPath
                == GamePackageCandidateRecipePipelineVocabulary.RecipeCatalogRelativePath
            && summary?.GamePackageCandidateRecipePipelineCandidateIndexPath
                == GamePackageCandidateRecipePipelineVocabulary.CandidateIndexRelativePath
            && summary?.GamePackageCandidateRecipePipelineNormalCommand
                == GamePackageCandidateRecipePipelineVocabulary.NormalCommand
            && summary?.GamePackageCandidateRecipePipelineResultPath
                == GamePackageCandidateRecipePipelineVocabulary.PipelineResultRelativePath
            && summary?.GamePackageCandidateRecipePipelineScoringResultPath
                == GamePackageCandidateRecipePipelineVocabulary.ScoringResultRelativePath
            && summary?.GamePackageCandidateRecipePipelineMatrixResultPath
                == GamePackageCandidateRecipePipelineVocabulary.MatrixResultRelativePath
            && summary?.GamePackageCandidateRecipePipelineSelectedCandidatePackagePath
                == GamePackageCandidateRecipePipelineVocabulary.SelectedCandidatePackageRelativePath
            && summary?.GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath
                == GamePackageCandidateRecipePipelineVocabulary.SelectedCandidateHandoffRelativePath
            && summary?.GamePackageCandidateRecipePipelineManualUnityOptional == true
            && summary?.GamePackageCandidateRecipePipelineSamplePackageUnmodified == true
            && summary?.GamePackageCandidateRecipePipelineProjectionOnly == true
            && summary?.GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation == true
            && summary?.GamePackageCandidateRecipePipelineEvidencePath
                == GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory
            && summary?.GamePackageCandidateRecipePipelineExportPath
                == GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory
            && relativePaths;

        return new Goal131GamePackageCandidateRecipePipelineQuality(
            GroupPresent: group is not null,
            RecipePipelineStatus:
                summary?.GamePackageCandidateRecipePipelineStatus ?? string.Empty,
            RecipeCount: summary?.GamePackageCandidateRecipePipelineRecipeCount ?? 0,
            CandidateCount: summary?.GamePackageCandidateRecipePipelineCandidateCount ?? 0,
            PassedCandidates: summary?.GamePackageCandidateRecipePipelinePassedCandidates ?? 0,
            FailedCandidates: summary?.GamePackageCandidateRecipePipelineFailedCandidates ?? 0,
            MatrixPassed: summary?.GamePackageCandidateRecipePipelineMatrixPassed == true,
            SelectedCandidateId:
                summary?.GamePackageCandidateRecipePipelineSelectedCandidateId ?? string.Empty,
            SelectedCandidateScore:
                summary?.GamePackageCandidateRecipePipelineSelectedCandidateScore ?? 0,
            RecipeCatalogPath:
                summary?.GamePackageCandidateRecipePipelineRecipeCatalogPath ?? string.Empty,
            CandidateIndexPath:
                summary?.GamePackageCandidateRecipePipelineCandidateIndexPath ?? string.Empty,
            NormalCommand:
                summary?.GamePackageCandidateRecipePipelineNormalCommand ?? string.Empty,
            PipelineResultPath:
                summary?.GamePackageCandidateRecipePipelineResultPath ?? string.Empty,
            ScoringResultPath:
                summary?.GamePackageCandidateRecipePipelineScoringResultPath ?? string.Empty,
            MatrixResultPath:
                summary?.GamePackageCandidateRecipePipelineMatrixResultPath ?? string.Empty,
            SelectedCandidatePackagePath:
                summary?.GamePackageCandidateRecipePipelineSelectedCandidatePackagePath
                ?? string.Empty,
            SelectedCandidateHandoffPath:
                summary?.GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath
                ?? string.Empty,
            ManualUnityOptional:
                summary?.GamePackageCandidateRecipePipelineManualUnityOptional == true,
            SamplePackageUnmodified:
                summary?.GamePackageCandidateRecipePipelineSamplePackageUnmodified == true,
            ProjectionOnly:
                summary?.GamePackageCandidateRecipePipelineProjectionOnly == true,
            MetadataOnlyRecipeMutation:
                summary?.GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation == true,
            EvidencePath:
                summary?.GamePackageCandidateRecipePipelineEvidencePath ?? string.Empty,
            ExportPath:
                summary?.GamePackageCandidateRecipePipelineExportPath ?? string.Empty,
            QualityGatePassed: qualityGatePassed,
            RelativePaths: relativePaths);
    }

    private static void AddGoal131GamePackageCandidateRecipePipelineQualityDiagnostics(
        Goal131GamePackageCandidateRecipePipelineQuality pipeline,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(binding.PageBindDisplaysGamePackageCandidateRecipePipeline,
            "goal131.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(pipeline.GroupPresent, "goal131.quality.group_present",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);

        if (!pipeline.GroupPresent || pipeline.RecipePipelineStatus != "GREEN")
        {
            return;
        }

        AddIfFalse(pipeline.RecipeCount >= 4, "goal131.quality.recipe_count",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.CandidateCount >= 4, "goal131.quality.candidate_count",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.PassedCandidates == pipeline.CandidateCount,
            "goal131.quality.passed_candidates",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.FailedCandidates == 0, "goal131.quality.failed_candidates",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.MatrixPassed, "goal131.quality.matrix_passed",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(pipeline.SelectedCandidateId),
            "goal131.quality.selected_candidate",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.SelectedCandidateScore > 0, "goal131.quality.selected_score",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.NormalCommand
                   == GamePackageCandidateRecipePipelineVocabulary.NormalCommand,
            "goal131.quality.normal_command",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.MetadataOnlyRecipeMutation,
            "goal131.quality.metadata_only_recipe_mutation",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.ManualUnityOptional, "goal131.quality.manual_unity_optional",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.SamplePackageUnmodified, "goal131.quality.sample_unmodified",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.ProjectionOnly, "goal131.quality.projection_only",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.QualityGatePassed, "goal131.quality.quality_gate",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
        AddIfFalse(pipeline.RelativePaths, "goal131.quality.relative_paths",
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion", diagnostics);
    }

    private static bool Goal131AllowedPath(string path) =>
        path.StartsWith(
            GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal131GamePackageCandidateRecipePipelineQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal131GamePackageCandidateRecipePipelineQuality pipeline,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            GamePackageCandidateRecipePipelineGroupPresent = pipeline.GroupPresent,
            GamePackageCandidateRecipePipelineStatus = pipeline.RecipePipelineStatus,
            GamePackageCandidateRecipePipelineRecipeCount = pipeline.RecipeCount,
            GamePackageCandidateRecipePipelineCandidateCount = pipeline.CandidateCount,
            GamePackageCandidateRecipePipelinePassedCandidates = pipeline.PassedCandidates,
            GamePackageCandidateRecipePipelineFailedCandidates = pipeline.FailedCandidates,
            GamePackageCandidateRecipePipelineMatrixPassed = pipeline.MatrixPassed,
            GamePackageCandidateRecipePipelineSelectedCandidateId = pipeline.SelectedCandidateId,
            GamePackageCandidateRecipePipelineSelectedCandidateScore =
                pipeline.SelectedCandidateScore,
            GamePackageCandidateRecipePipelineRecipeCatalogPath = pipeline.RecipeCatalogPath,
            GamePackageCandidateRecipePipelineCandidateIndexPath = pipeline.CandidateIndexPath,
            GamePackageCandidateRecipePipelineNormalCommand = pipeline.NormalCommand,
            GamePackageCandidateRecipePipelineResultPath = pipeline.PipelineResultPath,
            GamePackageCandidateRecipePipelineScoringResultPath = pipeline.ScoringResultPath,
            GamePackageCandidateRecipePipelineMatrixResultPath = pipeline.MatrixResultPath,
            GamePackageCandidateRecipePipelineSelectedCandidatePackagePath =
                pipeline.SelectedCandidatePackagePath,
            GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath =
                pipeline.SelectedCandidateHandoffPath,
            GamePackageCandidateRecipePipelineManualUnityOptional = pipeline.ManualUnityOptional,
            GamePackageCandidateRecipePipelineSamplePackageUnmodified =
                pipeline.SamplePackageUnmodified,
            GamePackageCandidateRecipePipelineProjectionOnly = pipeline.ProjectionOnly,
            GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation =
                pipeline.MetadataOnlyRecipeMutation,
            GamePackageCandidateRecipePipelineEvidencePath = pipeline.EvidencePath,
            GamePackageCandidateRecipePipelineExportPath = pipeline.ExportPath,
            GamePackageCandidateRecipePipelineQualityGatePassed = pipeline.QualityGatePassed,
            Goal131FilesDiscoveredByRelativePaths = pipeline.RelativePaths,
            WinFormsGamePackageCandidateRecipePipelineBindingReal =
                binding.PageBindDisplaysGamePackageCandidateRecipePipeline
        };

    private sealed record Goal131GamePackageCandidateRecipePipelineQuality(
        bool GroupPresent,
        string RecipePipelineStatus,
        int RecipeCount,
        int CandidateCount,
        int PassedCandidates,
        int FailedCandidates,
        bool MatrixPassed,
        string SelectedCandidateId,
        int SelectedCandidateScore,
        string RecipeCatalogPath,
        string CandidateIndexPath,
        string NormalCommand,
        string PipelineResultPath,
        string ScoringResultPath,
        string MatrixResultPath,
        string SelectedCandidatePackagePath,
        string SelectedCandidateHandoffPath,
        bool ManualUnityOptional,
        bool SamplePackageUnmodified,
        bool ProjectionOnly,
        bool MetadataOnlyRecipeMutation,
        string EvidencePath,
        string ExportPath,
        bool QualityGatePassed,
        bool RelativePaths);
}
