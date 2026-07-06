using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildGamePackageCandidateRecipePipelineGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadGamePackageCandidateRecipePipelineSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory,
                GamePackageCandidateRecipePipelineVocabulary.GoalId,
                BuildGoal131ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithGamePackageCandidateRecipePipelineSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal131NestedFiles(projectRoot, isExport: false))
        {
            entries.Add(WithGamePackageCandidateRecipePipelineSummary(
                Goal131FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        foreach (var file in BuildGoal131ExportFiles(projectRoot))
        {
            entries.Add(WithGamePackageCandidateRecipePipelineSummary(
                Goal131FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithGamePackageCandidateRecipePipelineSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = GamePackageCandidateRecipePipelineVocabulary.GoalId + ".summary",
                RelativePath =
                    GamePackageCandidateRecipePipelineVocabulary.DashboardRelativePath,
                ArtifactKind = "gamepackage_candidate_recipe_pipeline_workspace_summary",
                SourceGoalId = GamePackageCandidateRecipePipelineVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    GamePackageCandidateRecipePipelineVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "recipePipelineStatus=" + summary.RecipePipelineStatus
                                    + "; candidateCount="
                                    + summary.CandidateCount
                                    + "; selectedCandidateId="
                                    + summary.SelectedCandidateId,
                SafeRatingMetadataSummary =
                    "projectionOnly=true; metadataOnlyRecipeMutation=true; normalCommand="
                    + summary.NormalCommand
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "gamepackage_candidate_recipe_catalog_scoring_and_promotion",
            "Goal 131 GamePackage Candidate Recipe Catalog Scoring and Promotion",
            GamePackageCandidateRecipePipelineVocabulary.GoalId,
            GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal131ProceduralFiles() =>
    [
        (GamePackageCandidateRecipePipelineVocabulary.RecipeCatalogFileName,
            "gamepackage_candidate_recipe_pipeline_catalog"),
        (GamePackageCandidateRecipePipelineVocabulary.CandidateIndexFileName,
            "gamepackage_candidate_recipe_pipeline_candidate_index"),
        (GamePackageCandidateRecipePipelineVocabulary.PipelineResultFileName,
            "gamepackage_candidate_recipe_pipeline_result"),
        (GamePackageCandidateRecipePipelineVocabulary.ScoringResultFileName,
            "gamepackage_candidate_recipe_pipeline_scoring_result"),
        (GamePackageCandidateRecipePipelineVocabulary.MatrixResultFileName,
            "gamepackage_candidate_recipe_pipeline_matrix_result"),
        (GamePackageCandidateRecipePipelineVocabulary.DashboardFileName,
            "gamepackage_candidate_recipe_pipeline_dashboard"),
        (GamePackageCandidateRecipePipelineVocabulary.ScriptScanFileName,
            "gamepackage_candidate_recipe_pipeline_script_scan"),
        (GamePackageCandidateRecipePipelineVocabulary.LogScanFileName,
            "gamepackage_candidate_recipe_pipeline_log_scan"),
        (GamePackageCandidateRecipePipelineVocabulary.NegativeProofFileName,
            "gamepackage_candidate_recipe_pipeline_negative_proof"),
        (GamePackageCandidateRecipePipelineVocabulary.ReportFileName,
            "gamepackage_candidate_recipe_pipeline_report"),
        (GamePackageCandidateRecipePipelineVocabulary.FileIndexFileName,
            "gamepackage_candidate_recipe_pipeline_file_index")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal131NestedFiles(
        string projectRoot,
        bool isExport)
    {
        var root = isExport
            ? GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory
            : GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory;
        var fullRoot = Resolve(projectRoot, root);
        if (!Directory.Exists(fullRoot))
        {
            return [];
        }

        return Directory.EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
            .Select(path => Relative(projectRoot, path))
            .Where(path =>
                path.EndsWith("/package.json", StringComparison.Ordinal)
                || path.EndsWith("/selected-candidate-handoff.json", StringComparison.Ordinal)
                || path.EndsWith("/runner-result.json", StringComparison.Ordinal)
                || path.EndsWith("/log-scan.json", StringComparison.Ordinal))
            .Select(path => (
                RelativePath: path,
                Kind: path.EndsWith("/package.json", StringComparison.Ordinal)
                    ? "gamepackage_candidate_recipe_pipeline_candidate_package"
                    : path.EndsWith("/selected-candidate-handoff.json", StringComparison.Ordinal)
                        ? "gamepackage_candidate_recipe_pipeline_selected_handoff"
                        : "gamepackage_candidate_recipe_pipeline_runner_artifact"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal131ExportFiles(
        string projectRoot)
    {
        var files = BuildGoal131ProceduralFiles()
            .Select(item => (
                RelativePath:
                GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory + "/" + item.FileName,
                Kind: "gamepackage_candidate_recipe_pipeline_export_file"))
            .ToList();
        files.AddRange(BuildGoal131NestedFiles(projectRoot, isExport: true));
        return files
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static VisualWorldPreviewArtifactEntry WithGamePackageCandidateRecipePipelineSummary(
        VisualWorldPreviewArtifactEntry entry,
        GamePackageCandidateRecipePipelineWorkspaceSummary summary) =>
        entry with
        {
            GamePackageCandidateRecipePipelineStatus = summary.RecipePipelineStatus,
            GamePackageCandidateRecipePipelineRecipeCount = summary.RecipeCount,
            GamePackageCandidateRecipePipelineCandidateCount = summary.CandidateCount,
            GamePackageCandidateRecipePipelinePassedCandidates = summary.PassedCandidates,
            GamePackageCandidateRecipePipelineFailedCandidates = summary.FailedCandidates,
            GamePackageCandidateRecipePipelineMatrixPassed = summary.MatrixPassed,
            GamePackageCandidateRecipePipelineSelectedCandidateId = summary.SelectedCandidateId,
            GamePackageCandidateRecipePipelineSelectedCandidateScore = summary.SelectedCandidateScore,
            GamePackageCandidateRecipePipelineRecipeCatalogPath = summary.RecipeCatalogPath,
            GamePackageCandidateRecipePipelineCandidateIndexPath = summary.CandidateIndexPath,
            GamePackageCandidateRecipePipelineNormalCommand = summary.NormalCommand,
            GamePackageCandidateRecipePipelineResultPath = summary.PipelineResultPath,
            GamePackageCandidateRecipePipelineScoringResultPath = summary.ScoringResultPath,
            GamePackageCandidateRecipePipelineMatrixResultPath = summary.MatrixResultPath,
            GamePackageCandidateRecipePipelineSelectedCandidatePackagePath =
                summary.SelectedCandidatePackagePath,
            GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath =
                summary.SelectedCandidateHandoffPath,
            GamePackageCandidateRecipePipelineManualUnityOptional = summary.ManualUnityOptional,
            GamePackageCandidateRecipePipelineSamplePackageUnmodified =
                summary.SamplePackageUnmodified,
            GamePackageCandidateRecipePipelineProjectionOnly = summary.ProjectionOnly,
            GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation =
                summary.MetadataOnlyRecipeMutation,
            GamePackageCandidateRecipePipelineEvidencePath = summary.EvidencePath,
            GamePackageCandidateRecipePipelineExportPath = summary.ExportPath,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static GamePackageCandidateRecipePipelineWorkspaceSummary
        LoadGamePackageCandidateRecipePipelineSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + GamePackageCandidateRecipePipelineVocabulary.DashboardFileName,
            diagnostics);
        using var pipelineResult = TryReadOptionalGoal131Json(
            projectRoot,
            GamePackageCandidateRecipePipelineVocabulary.PipelineResultRelativePath,
            diagnostics);
        using var scoringResult = TryReadOptionalGoal131Json(
            projectRoot,
            GamePackageCandidateRecipePipelineVocabulary.ScoringResultRelativePath,
            diagnostics);
        using var matrixResult = TryReadOptionalGoal131Json(
            projectRoot,
            GamePackageCandidateRecipePipelineVocabulary.MatrixResultRelativePath,
            diagnostics);

        return new GamePackageCandidateRecipePipelineWorkspaceSummary(
            RecipePipelineStatus: Goal131String(dashboard?.RootElement, "recipePipelineStatus"),
            RecipeCount: dashboard is not null
                ? Goal131Int(dashboard.RootElement, "recipeCount")
                : 0,
            CandidateCount: dashboard is not null
                ? Goal131Int(dashboard.RootElement, "candidateCount")
                : 0,
            PassedCandidates: dashboard is not null
                ? Goal131Int(dashboard.RootElement, "passedCandidates")
                : 0,
            FailedCandidates: dashboard is not null
                ? Goal131Int(dashboard.RootElement, "failedCandidates")
                : 0,
            MatrixPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "matrixPassed"),
            SelectedCandidateId: Goal131String(dashboard?.RootElement, "selectedCandidateId"),
            SelectedCandidateScore: dashboard is not null
                ? Goal131Int(dashboard.RootElement, "selectedCandidateScore")
                : 0,
            RecipeCatalogPath: Goal131String(dashboard?.RootElement, "recipeCatalogPath"),
            CandidateIndexPath: Goal131String(dashboard?.RootElement, "candidateIndexPath"),
            NormalCommand: Goal131String(dashboard?.RootElement, "normalCommand"),
            PipelineResultPath: Goal131String(dashboard?.RootElement, "pipelineResultPath"),
            ScoringResultPath: Goal131String(dashboard?.RootElement, "scoringResultPath"),
            MatrixResultPath: Goal131String(dashboard?.RootElement, "matrixResultPath"),
            SelectedCandidatePackagePath:
                Goal131String(dashboard?.RootElement, "selectedCandidatePackagePath"),
            SelectedCandidateHandoffPath:
                Goal131String(dashboard?.RootElement, "selectedCandidateHandoffPath"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            SamplePackageUnmodified:
                dashboard is not null && TryGetBool(dashboard.RootElement, "samplePackageUnmodified"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            MetadataOnlyRecipeMutation:
                dashboard is not null
                && TryGetBool(dashboard.RootElement, "metadataOnlyRecipeMutation"),
            EvidencePath: Goal131String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal131String(dashboard?.RootElement, "exportPath"),
            PipelineResultPassed:
                pipelineResult is not null
                && Goal131String(pipelineResult.RootElement, "recipePipelineStatus") == "GREEN",
            ScoringResultPassed:
                scoringResult is not null
                && Goal131String(scoringResult.RootElement, "scoringStatus") == "GREEN",
            MatrixResultPassed:
                matrixResult is not null
                && Goal131String(matrixResult.RootElement, "matrixStatus") == "GREEN",
            QualityGatePassed: Goal131String(dashboard?.RootElement, "recipePipelineStatus") == "GREEN",
            RelativePaths: Goal131AllPathsRelative(projectRoot));
    }

    private static JsonDocument? TryReadOptionalGoal131Json(
        string projectRoot,
        string relativePath,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var fullPath = Resolve(projectRoot, relativePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        try
        {
            return JsonDocument.Parse(File.ReadAllText(fullPath, Encoding.UTF8));
        }
        catch (JsonException ex)
        {
            diagnostics.Add(VisualWorldPreviewDiagnostic.Error(
                "goal131.json.invalid",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static VisualWorldPreviewArtifactEntry Goal131FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = GamePackageCandidateRecipePipelineVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = GamePackageCandidateRecipePipelineVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal131 recipe pipeline file exists" : "Goal131 recipe pipeline file missing",
            SafeRatingMetadataSummary = "recipePipelineArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal131AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            GamePackageCandidateRecipePipelineVocabulary.ProceduralOutputDirectory,
            GamePackageCandidateRecipePipelineVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal131String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string Goal131String(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal131Int(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record GamePackageCandidateRecipePipelineWorkspaceSummary(
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
        bool PipelineResultPassed,
        bool ScoringResultPassed,
        bool MatrixResultPassed,
        bool QualityGatePassed,
        bool RelativePaths);
}
