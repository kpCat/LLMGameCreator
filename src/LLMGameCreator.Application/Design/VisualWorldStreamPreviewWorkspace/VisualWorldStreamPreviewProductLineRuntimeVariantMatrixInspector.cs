using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildProductLineRuntimeVariantMatrixGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadProductLineRuntimeVariantMatrixSummary(projectRoot);
        var entries = BuildGoal142Files()
            .Select(file => WithProductLineRuntimeVariantMatrixSummary(
                Goal142FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary))
            .ToList();

        entries.Add(WithProductLineRuntimeVariantMatrixSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = ProductLineRuntimeVariantMatrixVocabulary.GoalId + ".summary",
                RelativePath = ProductLineRuntimeVariantMatrixVocabulary.DashboardRelativePath,
                ArtifactKind = "product_line_runtime_variant_matrix_workspace_summary",
                SourceGoalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    ProductLineRuntimeVariantMatrixVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "matrixStatus=" + summary.MatrixStatus
                                    + "; candidateCount=" + summary.CandidateCount
                                    + "; selectedCandidateId=" + summary.SelectedCandidateId,
                SafeRatingMetadataSummary =
                    "runtimeAuthority=true; projectionOnly=false; accepted=false"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "product_line_runtime_variant_matrix",
            "Goal 142 Product-Line Runtime Variant Matrix",
            ProductLineRuntimeVariantMatrixVocabulary.GoalId,
            ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal142Files()
    {
        var procedural = ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory;
        var export = ProductLineRuntimeVariantMatrixVocabulary.ExportPackageDirectory;
        var aggregate = new[]
        {
            ProductLineRuntimeVariantMatrixVocabulary.CatalogFileName,
            ProductLineRuntimeVariantMatrixVocabulary.DashboardFileName,
            ProductLineRuntimeVariantMatrixVocabulary.MatrixResultFileName,
            ProductLineRuntimeVariantMatrixVocabulary.MutationSummaryFileName,
            ProductLineRuntimeVariantMatrixVocabulary.DistinctnessProofFileName,
            ProductLineRuntimeVariantMatrixVocabulary.ScoreboardFileName,
            ProductLineRuntimeVariantMatrixVocabulary.NegativeProofFileName,
            ProductLineRuntimeVariantMatrixVocabulary.FileIndexFileName,
            ProductLineRuntimeVariantMatrixVocabulary.OneClickReportJsonFileName,
            ProductLineRuntimeVariantMatrixVocabulary.OneClickReportMarkdownFileName
        };
        var files = aggregate
            .Select(file => (RelativePath: procedural + "/" + file, Kind: "product_line_runtime_variant_matrix_file"))
            .Concat(aggregate.Select(file => (
                RelativePath: export + "/" + file,
                Kind: "product_line_runtime_variant_matrix_export_file")))
            .Concat(new[]
            {
                (RelativePath: ProductLineRuntimeVariantMatrixVocabulary.SelectedHandoffRelativePath,
                    Kind: "selected_runtime_variant_handoff"),
                (RelativePath: procedural + "/selected-runtime-variant/package.json",
                    Kind: "selected_runtime_variant_package"),
                (RelativePath: procedural + "/selected-runtime-variant/runtime-outcome-summary.json",
                    Kind: "selected_runtime_variant_runtime_outcome"),
                (RelativePath: procedural + "/selected-runtime-variant/selection-rationale.md",
                    Kind: "selected_runtime_variant_selection_rationale"),
                (RelativePath: export + "/selected-runtime-variant/selected-runtime-variant-handoff.json",
                    Kind: "selected_runtime_variant_export_handoff"),
                (RelativePath: export + "/selected-runtime-variant/package.json",
                    Kind: "selected_runtime_variant_export_package")
            })
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
        return files;
    }

    private static VisualWorldPreviewArtifactEntry WithProductLineRuntimeVariantMatrixSummary(
        VisualWorldPreviewArtifactEntry entry,
        ProductLineRuntimeVariantMatrixWorkspaceSummary summary) =>
        entry with
        {
            ProductLineRuntimeVariantMatrixStatus = summary.MatrixStatus,
            ProductLineRuntimeVariantCandidateCount = summary.CandidateCount,
            ProductLineRuntimeVariantPassedCandidateCount = summary.PassedCandidateCount,
            ProductLineRuntimeVariantFailedCandidateCount = summary.FailedCandidateCount,
            ProductLineRuntimeVariantRuntimeSignificantCandidateCount =
                summary.RuntimeSignificantCandidateCount,
            ProductLineRuntimeVariantDistinctFinalStateHashCount = summary.DistinctFinalStateHashCount,
            ProductLineRuntimeVariantSelectedCandidateId = summary.SelectedCandidateId,
            ProductLineRuntimeVariantSelectedVariantKind = summary.SelectedVariantKind,
            ProductLineRuntimeVariantSelectedScore = summary.SelectedScore,
            ProductLineRuntimeVariantSourceTemplateUnmodified = summary.SourceTemplateUnmodified,
            ProductLineRuntimeVariantNormalCommand = summary.NormalCommand,
            ProductLineRuntimeVariantMatrixResultPath = summary.MatrixResultPath,
            ProductLineRuntimeVariantSelectedHandoffPath = summary.SelectedHandoffPath,
            ProductLineRuntimeVariantAccepted = summary.Accepted,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static ProductLineRuntimeVariantMatrixWorkspaceSummary LoadProductLineRuntimeVariantMatrixSummary(
        string projectRoot)
    {
        using var dashboard = TryReadJson(
            projectRoot,
            ProductLineRuntimeVariantMatrixVocabulary.DashboardRelativePath,
            []);
        return new ProductLineRuntimeVariantMatrixWorkspaceSummary(
            MatrixStatus: Goal138String(dashboard?.RootElement, "matrixStatus"),
            CandidateCount: dashboard is not null ? Goal138Int(dashboard.RootElement, "candidateCount") : 0,
            PassedCandidateCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "passedCandidateCount")
                : 0,
            FailedCandidateCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "failedCandidateCount")
                : 0,
            RuntimeSignificantCandidateCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "runtimeSignificantCandidateCount")
                : 0,
            DistinctFinalStateHashCount: dashboard is not null
                ? Goal138Int(dashboard.RootElement, "distinctFinalStateHashCount")
                : 0,
            SelectedCandidateId: Goal138String(dashboard?.RootElement, "selectedCandidateId"),
            SelectedVariantKind: Goal138String(dashboard?.RootElement, "selectedVariantKind"),
            SelectedScore: dashboard is not null ? Goal138Int(dashboard.RootElement, "selectedScore") : 0,
            SourceTemplateUnmodified: dashboard is not null
                                      && TryGetBool(dashboard.RootElement, "sourceTemplateUnmodified"),
            NormalCommand: Goal138String(dashboard?.RootElement, "normalCommand"),
            MatrixResultPath: Goal138String(dashboard?.RootElement, "matrixResultPath"),
            SelectedHandoffPath: Goal138String(dashboard?.RootElement, "selectedHandoffPath"),
            Accepted: dashboard is not null && TryGetBool(dashboard.RootElement, "accepted"),
            QualityGatePassed: Goal138String(dashboard?.RootElement, "matrixStatus") == "GREEN",
            RelativePaths: Goal142AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal142FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = ProductLineRuntimeVariantMatrixVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = ProductLineRuntimeVariantMatrixVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists
                ? "Goal142 product-line runtime variant matrix file exists"
                : "Goal142 product-line runtime variant matrix file missing",
            SafeRatingMetadataSummary =
                "productLineRuntimeVariantMatrixArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal142AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            ProductLineRuntimeVariantMatrixVocabulary.ProceduralOutputDirectory,
            ProductLineRuntimeVariantMatrixVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private sealed record ProductLineRuntimeVariantMatrixWorkspaceSummary(
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
        bool QualityGatePassed,
        bool RelativePaths);
}
