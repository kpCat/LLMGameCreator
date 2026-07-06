using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildGamePackageCandidateFactoryGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadGamePackageCandidateFactorySummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory,
                GamePackageCandidateFactoryProjectionVocabulary.GoalId,
                BuildGoal130ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithGamePackageCandidateFactorySummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal130NestedFiles(projectRoot, isExport: false))
        {
            entries.Add(WithGamePackageCandidateFactorySummary(
                Goal130FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        foreach (var file in BuildGoal130ExportFiles(projectRoot))
        {
            entries.Add(WithGamePackageCandidateFactorySummary(
                Goal130FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithGamePackageCandidateFactorySummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = GamePackageCandidateFactoryProjectionVocabulary.GoalId + ".summary",
                RelativePath =
                    GamePackageCandidateFactoryProjectionVocabulary.DashboardRelativePath,
                ArtifactKind = "gamepackage_candidate_factory_workspace_summary",
                SourceGoalId = GamePackageCandidateFactoryProjectionVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    GamePackageCandidateFactoryProjectionVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "candidateFactoryStatus=" + summary.CandidateFactoryStatus
                                    + "; candidateCount="
                                    + summary.CandidateCount,
                SafeRatingMetadataSummary =
                    "projectionOnly=true; normalCommand=" + summary.NormalCommand
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "gamepackage_candidate_factory_and_matrix_pipeline",
            "Goal 130 GamePackage Candidate Factory and Matrix Pipeline",
            GamePackageCandidateFactoryProjectionVocabulary.GoalId,
            GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal130ProceduralFiles() =>
    [
        (GamePackageCandidateFactoryProjectionVocabulary.CandidateIndexFileName,
            "gamepackage_candidate_factory_candidate_index"),
        (GamePackageCandidateFactoryProjectionVocabulary.FactoryResultFileName,
            "gamepackage_candidate_factory_result"),
        (GamePackageCandidateFactoryProjectionVocabulary.DashboardFileName,
            "gamepackage_candidate_factory_dashboard"),
        (GamePackageCandidateFactoryProjectionVocabulary.ScriptScanFileName,
            "gamepackage_candidate_factory_script_scan"),
        (GamePackageCandidateFactoryProjectionVocabulary.LogScanFileName,
            "gamepackage_candidate_factory_log_scan"),
        (GamePackageCandidateFactoryProjectionVocabulary.NegativeProofFileName,
            "gamepackage_candidate_factory_negative_proof"),
        (GamePackageCandidateFactoryProjectionVocabulary.ReportFileName,
            "gamepackage_candidate_factory_report"),
        (GamePackageCandidateFactoryProjectionVocabulary.FileIndexFileName,
            "gamepackage_candidate_factory_file_index")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal130NestedFiles(
        string projectRoot,
        bool isExport)
    {
        var root = isExport
            ? GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory
            : GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory;
        var fullRoot = Resolve(projectRoot, root);
        if (!Directory.Exists(fullRoot))
        {
            return [];
        }

        return Directory.EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
            .Select(path => Relative(projectRoot, path))
            .Where(path =>
                path.EndsWith("/package.json", StringComparison.Ordinal)
                || path.EndsWith("/" + GamePackageCandidateFactoryProjectionVocabulary.MatrixResultFileName,
                    StringComparison.Ordinal)
                || path.EndsWith("/runner-result.json", StringComparison.Ordinal)
                || path.EndsWith("/log-scan.json", StringComparison.Ordinal))
            .Select(path => (
                RelativePath: path,
                Kind: path.EndsWith("/package.json", StringComparison.Ordinal)
                    ? "gamepackage_candidate_factory_candidate_package"
                    : "gamepackage_candidate_factory_runner_artifact"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal130ExportFiles(
        string projectRoot)
    {
        var files = BuildGoal130ProceduralFiles()
            .Select(item => (
                RelativePath:
                GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory + "/" + item.FileName,
                Kind: "gamepackage_candidate_factory_export_file"))
            .ToList();
        files.AddRange(BuildGoal130NestedFiles(projectRoot, isExport: true));
        return files
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static VisualWorldPreviewArtifactEntry WithGamePackageCandidateFactorySummary(
        VisualWorldPreviewArtifactEntry entry,
        GamePackageCandidateFactoryWorkspaceSummary summary) =>
        entry with
        {
            GamePackageCandidateFactoryStatus = summary.CandidateFactoryStatus,
            GamePackageCandidateFactoryCandidateCount = summary.CandidateCount,
            GamePackageCandidateFactoryPassedCandidates = summary.PassedCandidates,
            GamePackageCandidateFactoryFailedCandidates = summary.FailedCandidates,
            GamePackageCandidateFactoryMatrixPassed = summary.MatrixPassed,
            GamePackageCandidateFactoryCandidateIndexPath = summary.CandidateIndexPath,
            GamePackageCandidateFactoryNormalCommand = summary.NormalCommand,
            GamePackageCandidateFactoryResultPath = summary.FactoryResultPath,
            GamePackageCandidateFactoryMatrixResultPath = summary.MatrixResultPath,
            GamePackageCandidateFactoryManualUnityOptional = summary.ManualUnityOptional,
            GamePackageCandidateFactorySamplePackageUnmodified = summary.SamplePackageUnmodified,
            GamePackageCandidateFactoryProjectionOnly = summary.ProjectionOnly,
            GamePackageCandidateFactoryEvidencePath = summary.EvidencePath,
            GamePackageCandidateFactoryExportPath = summary.ExportPath,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static GamePackageCandidateFactoryWorkspaceSummary
        LoadGamePackageCandidateFactorySummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + GamePackageCandidateFactoryProjectionVocabulary.DashboardFileName,
            diagnostics);
        using var factoryResult = TryReadOptionalGoal130Json(
            projectRoot,
            GamePackageCandidateFactoryProjectionVocabulary.FactoryResultRelativePath,
            diagnostics);
        using var matrixResult = TryReadOptionalGoal130Json(
            projectRoot,
            GamePackageCandidateFactoryProjectionVocabulary.MatrixResultRelativePath,
            diagnostics);

        return new GamePackageCandidateFactoryWorkspaceSummary(
            CandidateFactoryStatus: Goal130String(dashboard?.RootElement, "candidateFactoryStatus"),
            CandidateCount: dashboard is not null
                ? Goal130Int(dashboard.RootElement, "candidateCount")
                : 0,
            PassedCandidates: dashboard is not null
                ? Goal130Int(dashboard.RootElement, "passedCandidates")
                : 0,
            FailedCandidates: dashboard is not null
                ? Goal130Int(dashboard.RootElement, "failedCandidates")
                : 0,
            MatrixPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "matrixPassed"),
            CandidateIndexPath: Goal130String(dashboard?.RootElement, "candidateIndexPath"),
            NormalCommand: Goal130String(dashboard?.RootElement, "normalCommand"),
            FactoryResultPath: Goal130String(dashboard?.RootElement, "factoryResultPath"),
            MatrixResultPath: Goal130String(dashboard?.RootElement, "matrixResultPath"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            SamplePackageUnmodified:
                dashboard is not null && TryGetBool(dashboard.RootElement, "samplePackageUnmodified"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            EvidencePath: Goal130String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal130String(dashboard?.RootElement, "exportPath"),
            FactoryResultPassed:
                factoryResult is not null
                && Goal130String(factoryResult.RootElement, "candidateFactoryStatus") == "GREEN",
            MatrixResultPassed:
                matrixResult is not null
                && Goal130String(matrixResult.RootElement, "matrixStatus") == "GREEN",
            QualityGatePassed: Goal130String(dashboard?.RootElement, "candidateFactoryStatus") == "GREEN",
            RelativePaths: Goal130AllPathsRelative(projectRoot));
    }

    private static JsonDocument? TryReadOptionalGoal130Json(
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
                "goal130.json.invalid",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static VisualWorldPreviewArtifactEntry Goal130FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = GamePackageCandidateFactoryProjectionVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = GamePackageCandidateFactoryProjectionVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal130 factory file exists" : "Goal130 factory file missing",
            SafeRatingMetadataSummary = "factoryArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal130AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            GamePackageCandidateFactoryProjectionVocabulary.ProceduralOutputDirectory,
            GamePackageCandidateFactoryProjectionVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal130String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string Goal130String(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal130Int(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record GamePackageCandidateFactoryWorkspaceSummary(
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
        bool FactoryResultPassed,
        bool MatrixResultPassed,
        bool QualityGatePassed,
        bool RelativePaths);
}
