using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal129GamePackageCandidateMatrixQuality
        BuildGoal129GamePackageCandidateMatrixQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "gamepackage_candidate_matrix_projection_runner");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "gamepackage_candidate_matrix_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal129AllowedPath(entry.RelativePath));
        var qualityGatePassed =
            ProofPassed(proofs, "goal129.gamepackage_candidate_matrix.candidate_index")
            && ProofPassed(proofs, "goal129.gamepackage_candidate_matrix.script_scan")
            && ProofPassed(proofs, "goal129.gamepackage_candidate_matrix.matrix_result")
            && ProofPassed(proofs, "goal129.gamepackage_candidate_matrix.log_scan")
            && ProofPassed(proofs, "goal129.gamepackage_candidate_matrix.negative_proof")
            && ProofPassed(proofs, "goal129.gamepackage_candidate_matrix.cleanup_applied")
            && ProofPassed(proofs, "goal129.gamepackage_candidate_matrix.sample_unmodified")
            && summary?.GamePackageCandidateMatrixStatus == "GREEN"
            && summary?.GamePackageCandidateMatrixCandidateCount >= 2
            && summary?.GamePackageCandidateMatrixPassedCandidateCount
                == summary?.GamePackageCandidateMatrixCandidateCount
            && summary?.GamePackageCandidateMatrixFailedCandidateCount == 0
            && summary?.GamePackageCandidateMatrixCandidateIndexPath
                == GamePackageCandidateMatrixProjectionVocabulary.CandidateIndexRelativePath
            && summary?.GamePackageCandidateMatrixResultPath
                == GamePackageCandidateMatrixProjectionVocabulary.MatrixResultRelativePath
            && summary?.GamePackageCandidateMatrixNormalCommand
                == GamePackageCandidateMatrixProjectionVocabulary.NormalCommand
            && summary?.GamePackageCandidateMatrixBaselineCandidatePackagePath
                == GamePackageCandidateMatrixProjectionVocabulary.BaselineCandidatePackagePath
            && summary?.GamePackageCandidateMatrixVariantCandidatePackagePath
                == GamePackageCandidateMatrixProjectionVocabulary.VariantCandidatePackagePath
            && summary?.GamePackageCandidateMatrixManualUnityOptional == true
            && summary?.GamePackageCandidateMatrixCleanupApplied == true
            && summary?.GamePackageCandidateMatrixProjectionOnly == true
            && summary?.GamePackageCandidateMatrixScriptScanPassed == true
            && summary?.GamePackageCandidateMatrixResultPassed == true
            && summary?.GamePackageCandidateMatrixLogScanPassed == true
            && relativePaths;

        return new Goal129GamePackageCandidateMatrixQuality(
            GroupPresent: group is not null,
            MatrixStatus: summary?.GamePackageCandidateMatrixStatus ?? string.Empty,
            CandidateCount: summary?.GamePackageCandidateMatrixCandidateCount ?? 0,
            PassedCandidateCount: summary?.GamePackageCandidateMatrixPassedCandidateCount ?? 0,
            FailedCandidateCount: summary?.GamePackageCandidateMatrixFailedCandidateCount ?? 0,
            CandidateIndexPath: summary?.GamePackageCandidateMatrixCandidateIndexPath
                                ?? string.Empty,
            MatrixResultPath: summary?.GamePackageCandidateMatrixResultPath ?? string.Empty,
            NormalCommand: summary?.GamePackageCandidateMatrixNormalCommand ?? string.Empty,
            ExampleCommand: summary?.GamePackageCandidateMatrixExampleCommand ?? string.Empty,
            BaselineCandidatePackagePath:
                summary?.GamePackageCandidateMatrixBaselineCandidatePackagePath ?? string.Empty,
            VariantCandidatePackagePath:
                summary?.GamePackageCandidateMatrixVariantCandidatePackagePath ?? string.Empty,
            ManualUnityOptional:
                summary?.GamePackageCandidateMatrixManualUnityOptional == true,
            CleanupApplied: summary?.GamePackageCandidateMatrixCleanupApplied == true,
            ProjectionOnly: summary?.GamePackageCandidateMatrixProjectionOnly == true,
            ScriptScanPassed: summary?.GamePackageCandidateMatrixScriptScanPassed == true,
            MatrixResultPassed: summary?.GamePackageCandidateMatrixResultPassed == true,
            LogScanPassed: summary?.GamePackageCandidateMatrixLogScanPassed == true,
            QualityGatePassed: qualityGatePassed,
            RelativePaths: relativePaths);
    }

    private static void AddGoal129GamePackageCandidateMatrixQualityDiagnostics(
        Goal129GamePackageCandidateMatrixQuality matrix,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(binding.PageBindDisplaysGamePackageCandidateMatrix,
            "goal129.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(matrix.GroupPresent, "goal129.quality.group_present",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);

        if (!matrix.GroupPresent || matrix.MatrixStatus != "GREEN")
        {
            return;
        }

        AddIfFalse(matrix.CandidateCount >= 2, "goal129.quality.candidate_count",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.PassedCandidateCount == matrix.CandidateCount,
            "goal129.quality.passed_candidate_count",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.FailedCandidateCount == 0,
            "goal129.quality.failed_candidate_count",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.CandidateIndexPath
                   == GamePackageCandidateMatrixProjectionVocabulary.CandidateIndexRelativePath,
            "goal129.quality.candidate_index_path",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.MatrixResultPath
                   == GamePackageCandidateMatrixProjectionVocabulary.MatrixResultRelativePath,
            "goal129.quality.matrix_result_path",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.NormalCommand
                   == GamePackageCandidateMatrixProjectionVocabulary.NormalCommand,
            "goal129.quality.normal_command",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.ManualUnityOptional, "goal129.quality.manual_unity_optional",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.CleanupApplied, "goal129.quality.cleanup_applied",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.ProjectionOnly, "goal129.quality.projection_only",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.ScriptScanPassed, "goal129.quality.script_scan",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.MatrixResultPassed, "goal129.quality.matrix_result",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.LogScanPassed, "goal129.quality.log_scan",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.QualityGatePassed, "goal129.quality.quality_gate",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
        AddIfFalse(matrix.RelativePaths, "goal129.quality.relative_paths",
            "gamepackage_candidate_matrix_projection_runner", diagnostics);
    }

    private static bool Goal129AllowedPath(string path) =>
        path.StartsWith(
            GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            GamePackageCandidateMatrixProjectionVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static bool ProofPassed(
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs,
        string proofId) =>
        proofs.Any(proof => proof.ProofId == proofId && proof.Passed);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal129GamePackageCandidateMatrixQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal129GamePackageCandidateMatrixQuality matrix,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            GamePackageCandidateMatrixGroupPresent = matrix.GroupPresent,
            GamePackageCandidateMatrixStatus = matrix.MatrixStatus,
            GamePackageCandidateMatrixCandidateCount = matrix.CandidateCount,
            GamePackageCandidateMatrixPassedCandidateCount = matrix.PassedCandidateCount,
            GamePackageCandidateMatrixFailedCandidateCount = matrix.FailedCandidateCount,
            GamePackageCandidateMatrixCandidateIndexPath = matrix.CandidateIndexPath,
            GamePackageCandidateMatrixResultPath = matrix.MatrixResultPath,
            GamePackageCandidateMatrixNormalCommand = matrix.NormalCommand,
            GamePackageCandidateMatrixExampleCommand = matrix.ExampleCommand,
            GamePackageCandidateMatrixBaselineCandidatePackagePath =
                matrix.BaselineCandidatePackagePath,
            GamePackageCandidateMatrixVariantCandidatePackagePath =
                matrix.VariantCandidatePackagePath,
            GamePackageCandidateMatrixManualUnityOptional = matrix.ManualUnityOptional,
            GamePackageCandidateMatrixCleanupApplied = matrix.CleanupApplied,
            GamePackageCandidateMatrixProjectionOnly = matrix.ProjectionOnly,
            GamePackageCandidateMatrixScriptScanPassed = matrix.ScriptScanPassed,
            GamePackageCandidateMatrixResultPassed = matrix.MatrixResultPassed,
            GamePackageCandidateMatrixLogScanPassed = matrix.LogScanPassed,
            GamePackageCandidateMatrixQualityGatePassed = matrix.QualityGatePassed,
            Goal129FilesDiscoveredByRelativePaths = matrix.RelativePaths,
            WinFormsGamePackageCandidateMatrixBindingReal =
                binding.PageBindDisplaysGamePackageCandidateMatrix
        };

    private sealed record Goal129GamePackageCandidateMatrixQuality(
        bool GroupPresent,
        string MatrixStatus,
        int CandidateCount,
        int PassedCandidateCount,
        int FailedCandidateCount,
        string CandidateIndexPath,
        string MatrixResultPath,
        string NormalCommand,
        string ExampleCommand,
        string BaselineCandidatePackagePath,
        string VariantCandidatePackagePath,
        bool ManualUnityOptional,
        bool CleanupApplied,
        bool ProjectionOnly,
        bool ScriptScanPassed,
        bool MatrixResultPassed,
        bool LogScanPassed,
        bool QualityGatePassed,
        bool RelativePaths);
}
