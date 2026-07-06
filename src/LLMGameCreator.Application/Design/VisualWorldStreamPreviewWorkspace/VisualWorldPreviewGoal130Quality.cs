using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal130GamePackageCandidateFactoryQuality
        BuildGoal130GamePackageCandidateFactoryQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "gamepackage_candidate_factory_and_matrix_pipeline");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "gamepackage_candidate_factory_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal130AllowedPath(entry.RelativePath));
        var qualityGatePassed =
            ProofPassed(proofs, "goal130.gamepackage_candidate_factory.candidate_index")
            && ProofPassed(proofs, "goal130.gamepackage_candidate_factory.script_scan")
            && ProofPassed(proofs, "goal130.gamepackage_candidate_factory.factory_result")
            && ProofPassed(proofs, "goal130.gamepackage_candidate_factory.matrix_result")
            && ProofPassed(proofs, "goal130.gamepackage_candidate_factory.log_scan")
            && ProofPassed(proofs, "goal130.gamepackage_candidate_factory.negative_proof")
            && ProofPassed(proofs, "goal130.gamepackage_candidate_factory.sample_unmodified")
            && summary?.GamePackageCandidateFactoryStatus == "GREEN"
            && summary?.GamePackageCandidateFactoryCandidateCount >= 3
            && summary?.GamePackageCandidateFactoryPassedCandidates
                == summary?.GamePackageCandidateFactoryCandidateCount
            && summary?.GamePackageCandidateFactoryFailedCandidates == 0
            && summary?.GamePackageCandidateFactoryMatrixPassed == true
            && summary?.GamePackageCandidateFactoryCandidateIndexPath
                == GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexRelativePath
            && summary?.GamePackageCandidateFactoryNormalCommand
                == GamePackageCandidateFactoryProjectionVocabulary.NormalCommand
            && summary?.GamePackageCandidateFactoryResultPath
                == GamePackageCandidateFactoryProjectionVocabulary.FactoryResultRelativePath
            && summary?.GamePackageCandidateFactoryMatrixResultPath
                == GamePackageCandidateFactoryProjectionVocabulary.MatrixResultRelativePath
            && summary?.GamePackageCandidateFactoryManualUnityOptional == true
            && summary?.GamePackageCandidateFactorySamplePackageUnmodified == true
            && summary?.GamePackageCandidateFactoryProjectionOnly == true
            && summary?.GamePackageCandidateFactoryEvidencePath
                == GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory
            && summary?.GamePackageCandidateFactoryExportPath
                == GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory
            && relativePaths;

        return new Goal130GamePackageCandidateFactoryQuality(
            GroupPresent: group is not null,
            CandidateFactoryStatus:
                summary?.GamePackageCandidateFactoryStatus ?? string.Empty,
            CandidateCount: summary?.GamePackageCandidateFactoryCandidateCount ?? 0,
            PassedCandidates: summary?.GamePackageCandidateFactoryPassedCandidates ?? 0,
            FailedCandidates: summary?.GamePackageCandidateFactoryFailedCandidates ?? 0,
            MatrixPassed: summary?.GamePackageCandidateFactoryMatrixPassed == true,
            CandidateIndexPath:
                summary?.GamePackageCandidateFactoryCandidateIndexPath ?? string.Empty,
            NormalCommand:
                summary?.GamePackageCandidateFactoryNormalCommand ?? string.Empty,
            FactoryResultPath:
                summary?.GamePackageCandidateFactoryResultPath ?? string.Empty,
            MatrixResultPath:
                summary?.GamePackageCandidateFactoryMatrixResultPath ?? string.Empty,
            ManualUnityOptional:
                summary?.GamePackageCandidateFactoryManualUnityOptional == true,
            SamplePackageUnmodified:
                summary?.GamePackageCandidateFactorySamplePackageUnmodified == true,
            ProjectionOnly: summary?.GamePackageCandidateFactoryProjectionOnly == true,
            EvidencePath: summary?.GamePackageCandidateFactoryEvidencePath ?? string.Empty,
            ExportPath: summary?.GamePackageCandidateFactoryExportPath ?? string.Empty,
            QualityGatePassed: qualityGatePassed,
            RelativePaths: relativePaths);
    }

    private static void AddGoal130GamePackageCandidateFactoryQualityDiagnostics(
        Goal130GamePackageCandidateFactoryQuality factory,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(binding.PageBindDisplaysGamePackageCandidateFactory,
            "goal130.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(factory.GroupPresent, "goal130.quality.group_present",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);

        if (!factory.GroupPresent || factory.CandidateFactoryStatus != "GREEN")
        {
            return;
        }

        AddIfFalse(factory.CandidateCount >= 3, "goal130.quality.candidate_count",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.PassedCandidates == factory.CandidateCount,
            "goal130.quality.passed_candidates",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.FailedCandidates == 0, "goal130.quality.failed_candidates",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.MatrixPassed, "goal130.quality.matrix_passed",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.CandidateIndexPath
                   == GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexRelativePath,
            "goal130.quality.candidate_index_path",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.NormalCommand
                   == GamePackageCandidateFactoryProjectionVocabulary.NormalCommand,
            "goal130.quality.normal_command",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.FactoryResultPath
                   == GamePackageCandidateFactoryProjectionVocabulary.FactoryResultRelativePath,
            "goal130.quality.factory_result_path",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.MatrixResultPath
                   == GamePackageCandidateFactoryProjectionVocabulary.MatrixResultRelativePath,
            "goal130.quality.matrix_result_path",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.ManualUnityOptional, "goal130.quality.manual_unity_optional",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.SamplePackageUnmodified, "goal130.quality.sample_unmodified",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.ProjectionOnly, "goal130.quality.projection_only",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.QualityGatePassed, "goal130.quality.quality_gate",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
        AddIfFalse(factory.RelativePaths, "goal130.quality.relative_paths",
            "gamepackage_candidate_factory_and_matrix_pipeline", diagnostics);
    }

    private static bool Goal130AllowedPath(string path) =>
        path.StartsWith(
            GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal130GamePackageCandidateFactoryQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal130GamePackageCandidateFactoryQuality factory,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            GamePackageCandidateFactoryGroupPresent = factory.GroupPresent,
            GamePackageCandidateFactoryStatus = factory.CandidateFactoryStatus,
            GamePackageCandidateFactoryCandidateCount = factory.CandidateCount,
            GamePackageCandidateFactoryPassedCandidates = factory.PassedCandidates,
            GamePackageCandidateFactoryFailedCandidates = factory.FailedCandidates,
            GamePackageCandidateFactoryMatrixPassed = factory.MatrixPassed,
            GamePackageCandidateFactoryCandidateIndexPath = factory.CandidateIndexPath,
            GamePackageCandidateFactoryNormalCommand = factory.NormalCommand,
            GamePackageCandidateFactoryResultPath = factory.FactoryResultPath,
            GamePackageCandidateFactoryMatrixResultPath = factory.MatrixResultPath,
            GamePackageCandidateFactoryManualUnityOptional = factory.ManualUnityOptional,
            GamePackageCandidateFactorySamplePackageUnmodified =
                factory.SamplePackageUnmodified,
            GamePackageCandidateFactoryProjectionOnly = factory.ProjectionOnly,
            GamePackageCandidateFactoryEvidencePath = factory.EvidencePath,
            GamePackageCandidateFactoryExportPath = factory.ExportPath,
            GamePackageCandidateFactoryQualityGatePassed = factory.QualityGatePassed,
            Goal130FilesDiscoveredByRelativePaths = factory.RelativePaths,
            WinFormsGamePackageCandidateFactoryBindingReal =
                binding.PageBindDisplaysGamePackageCandidateFactory
        };

    private sealed record Goal130GamePackageCandidateFactoryQuality(
        bool GroupPresent,
        string CandidateFactoryStatus,
        int CandidateCount,
        int PassedCandidates,
        int FailedCandidates,
        bool MatrixPassed,
        string CandidateIndexPath,
        string NormalCommand,
        string FactoryResultPath,
        string MatrixResultPath,
        bool ManualUnityOptional,
        bool SamplePackageUnmodified,
        bool ProjectionOnly,
        string EvidencePath,
        string ExportPath,
        bool QualityGatePassed,
        bool RelativePaths);
}
