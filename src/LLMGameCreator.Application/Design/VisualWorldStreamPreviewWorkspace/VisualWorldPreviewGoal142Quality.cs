using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal142ProductLineRuntimeVariantMatrixQuality BuildGoal142ProductLineRuntimeVariantMatrixQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "product_line_runtime_variant_matrix");
        var summary = group?.Entries.FirstOrDefault(item =>
            item.ArtifactKind == "product_line_runtime_variant_matrix_workspace_summary");
        var proofPassed = proofs.Any(item =>
            item.ProofId.StartsWith("goal142.product_line_runtime_variant_matrix.", StringComparison.Ordinal)
            && item.Passed);
        var relativePaths = group?.Entries.Count > 0
                            && group.Entries.All(entry => Goal142AllowedPath(entry.RelativePath));
        var qualityPassed =
            group is not null
            && summary is not null
            && summary.ProductLineRuntimeVariantMatrixStatus == "GREEN"
            && summary.ProductLineRuntimeVariantCandidateCount == 4
            && summary.ProductLineRuntimeVariantPassedCandidateCount == 4
            && summary.ProductLineRuntimeVariantFailedCandidateCount == 0
            && summary.ProductLineRuntimeVariantRuntimeSignificantCandidateCount == 4
            && summary.ProductLineRuntimeVariantDistinctFinalStateHashCount >= 3
            && !string.IsNullOrWhiteSpace(summary.ProductLineRuntimeVariantSelectedCandidateId)
            && summary.ProductLineRuntimeVariantSelectedScore > 0
            && summary.ProductLineRuntimeVariantSourceTemplateUnmodified
            && !summary.ProductLineRuntimeVariantAccepted
            && relativePaths
            && proofPassed;

        return new Goal142ProductLineRuntimeVariantMatrixQuality(
            GroupPresent: group is not null,
            MatrixStatus: summary?.ProductLineRuntimeVariantMatrixStatus ?? string.Empty,
            CandidateCount: summary?.ProductLineRuntimeVariantCandidateCount ?? 0,
            PassedCandidateCount: summary?.ProductLineRuntimeVariantPassedCandidateCount ?? 0,
            FailedCandidateCount: summary?.ProductLineRuntimeVariantFailedCandidateCount ?? 0,
            RuntimeSignificantCandidateCount:
                summary?.ProductLineRuntimeVariantRuntimeSignificantCandidateCount ?? 0,
            DistinctFinalStateHashCount:
                summary?.ProductLineRuntimeVariantDistinctFinalStateHashCount ?? 0,
            SelectedCandidateId: summary?.ProductLineRuntimeVariantSelectedCandidateId ?? string.Empty,
            SelectedVariantKind: summary?.ProductLineRuntimeVariantSelectedVariantKind ?? string.Empty,
            SelectedScore: summary?.ProductLineRuntimeVariantSelectedScore ?? 0,
            SourceTemplateUnmodified:
                summary?.ProductLineRuntimeVariantSourceTemplateUnmodified == true,
            NormalCommand: summary?.ProductLineRuntimeVariantNormalCommand ?? string.Empty,
            MatrixResultPath: summary?.ProductLineRuntimeVariantMatrixResultPath ?? string.Empty,
            SelectedHandoffPath:
                summary?.ProductLineRuntimeVariantSelectedHandoffPath ?? string.Empty,
            Accepted: summary?.ProductLineRuntimeVariantAccepted == true,
            RelativePaths: relativePaths,
            QualityGatePassed: qualityPassed);
    }

    private static void AddGoal142ProductLineRuntimeVariantMatrixQualityDiagnostics(
        Goal142ProductLineRuntimeVariantMatrixQuality matrix,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!matrix.GroupPresent)
        {
            return;
        }

        AddIfFalse(matrix.MatrixStatus == "GREEN",
            "goal142.quality.matrix_status",
            "product_line_runtime_variant_matrix",
            diagnostics);
        AddIfFalse(matrix.CandidateCount == 4,
            "goal142.quality.candidate_count",
            "product_line_runtime_variant_matrix",
            diagnostics);
        AddIfFalse(matrix.PassedCandidateCount == 4,
            "goal142.quality.passed_candidate_count",
            "product_line_runtime_variant_matrix",
            diagnostics);
        AddIfFalse(matrix.FailedCandidateCount == 0,
            "goal142.quality.failed_candidate_count",
            "product_line_runtime_variant_matrix",
            diagnostics);
        AddIfFalse(matrix.RuntimeSignificantCandidateCount == 4,
            "goal142.quality.runtime_significant_candidate_count",
            "product_line_runtime_variant_matrix",
            diagnostics);
        AddIfFalse(matrix.DistinctFinalStateHashCount >= 3,
            "goal142.quality.distinct_final_state_hash_count",
            "product_line_runtime_variant_matrix",
            diagnostics);
        AddIfFalse(matrix.SourceTemplateUnmodified,
            "goal142.quality.source_template_unmodified",
            "product_line_runtime_variant_matrix",
            diagnostics);
        AddIfFalse(!matrix.Accepted,
            "goal142.quality.accepted_must_stay_false",
            "product_line_runtime_variant_matrix",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysProductLineRuntimeVariantMatrix,
            "goal142.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(matrix.RelativePaths,
            "goal142.quality.relative_paths",
            "product_line_runtime_variant_matrix",
            diagnostics);
    }

    private static bool Goal142AllowedPath(string path) =>
        path.StartsWith(
            ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory
            + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            ProductLineRuntimeVariantMatrixVocabulary.ExportPackageDirectory
            + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate ApplyGoal142ProductLineRuntimeVariantMatrixQuality(
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        Goal142ProductLineRuntimeVariantMatrixQuality matrix,
        VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            ProductLineRuntimeVariantMatrixGroupPresent = matrix.GroupPresent,
            ProductLineRuntimeVariantMatrixStatus = matrix.MatrixStatus,
            ProductLineRuntimeVariantCandidateCount = matrix.CandidateCount,
            ProductLineRuntimeVariantPassedCandidateCount = matrix.PassedCandidateCount,
            ProductLineRuntimeVariantFailedCandidateCount = matrix.FailedCandidateCount,
            ProductLineRuntimeVariantRuntimeSignificantCandidateCount =
                matrix.RuntimeSignificantCandidateCount,
            ProductLineRuntimeVariantDistinctFinalStateHashCount = matrix.DistinctFinalStateHashCount,
            ProductLineRuntimeVariantSelectedCandidateId = matrix.SelectedCandidateId,
            ProductLineRuntimeVariantSelectedVariantKind = matrix.SelectedVariantKind,
            ProductLineRuntimeVariantSelectedScore = matrix.SelectedScore,
            ProductLineRuntimeVariantSourceTemplateUnmodified = matrix.SourceTemplateUnmodified,
            ProductLineRuntimeVariantNormalCommand = matrix.NormalCommand,
            ProductLineRuntimeVariantMatrixResultPath = matrix.MatrixResultPath,
            ProductLineRuntimeVariantSelectedHandoffPath = matrix.SelectedHandoffPath,
            ProductLineRuntimeVariantAccepted = matrix.Accepted,
            ProductLineRuntimeVariantFilesDiscoveredByRelativePaths = matrix.RelativePaths,
            ProductLineRuntimeVariantWinFormsBindingReal =
                binding.PageBindDisplaysProductLineRuntimeVariantMatrix,
            ProductLineRuntimeVariantQualityGatePassed =
                matrix.QualityGatePassed
                && binding.PageBindDisplaysProductLineRuntimeVariantMatrix,
            Passed = qualityGate.Passed
                     && (!matrix.GroupPresent
                         || matrix.QualityGatePassed
                         && binding.PageBindDisplaysProductLineRuntimeVariantMatrix)
        };

    private sealed record Goal142ProductLineRuntimeVariantMatrixQuality(
        bool GroupPresent,
        string MatrixStatus,
        int CandidateCount,
        int PassedCandidateCount,
        int FailedCandidateCount,
        int RuntimeSignificantCandidateCount,
        int DistinctFinalStateHashCount,
        string SelectedCandidateId,
        string SelectedVariantKind,
        int SelectedScore,
        bool SourceTemplateUnmodified,
        string NormalCommand,
        string MatrixResultPath,
        string SelectedHandoffPath,
        bool Accepted,
        bool RelativePaths,
        bool QualityGatePassed);
}
