using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildGamePackageCandidateMatrixGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadGamePackageCandidateMatrixSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory,
                GamePackageCandidateMatrixProjectionVocabulary.GoalId,
                BuildGoal129ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithGamePackageCandidateMatrixSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal129NestedFiles(projectRoot, isExport: false))
        {
            entries.Add(WithGamePackageCandidateMatrixSummary(
                Goal129FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        foreach (var file in BuildGoal129ExportFiles(projectRoot))
        {
            entries.Add(WithGamePackageCandidateMatrixSummary(
                Goal129FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithGamePackageCandidateMatrixSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = GamePackageCandidateMatrixProjectionVocabulary.GoalId + ".summary",
                RelativePath =
                    GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + GamePackageCandidateMatrixProjectionVocabulary.DashboardFileName,
                ArtifactKind = "gamepackage_candidate_matrix_workspace_summary",
                SourceGoalId = GamePackageCandidateMatrixProjectionVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + GamePackageCandidateMatrixProjectionVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "matrixStatus=" + summary.MatrixStatus
                                    + "; candidateCount="
                                    + summary.CandidateCount,
                SafeRatingMetadataSummary =
                    "projectionOnly=true; normalCommand=" + summary.NormalCommand
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "gamepackage_candidate_matrix_projection_runner",
            "Goal 129 GamePackage Candidate Matrix Projection Runner",
            GamePackageCandidateMatrixProjectionVocabulary.GoalId,
            GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal129ProceduralFiles() =>
    [
        (GamePackageCandidateMatrixProjectionVocabulary.CandidateIndexFileName,
            "gamepackage_candidate_matrix_candidate_index"),
        (GamePackageCandidateMatrixProjectionVocabulary.DashboardFileName,
            "gamepackage_candidate_matrix_dashboard"),
        (GamePackageCandidateMatrixProjectionVocabulary.ScriptScanFileName,
            "gamepackage_candidate_matrix_script_scan"),
        (GamePackageCandidateMatrixProjectionVocabulary.LogScanFileName,
            "gamepackage_candidate_matrix_log_scan"),
        (GamePackageCandidateMatrixProjectionVocabulary.NegativeProofFileName,
            "gamepackage_candidate_matrix_negative_proof"),
        (GamePackageCandidateMatrixProjectionVocabulary.ReportFileName,
            "gamepackage_candidate_matrix_report"),
        (GamePackageCandidateMatrixProjectionVocabulary.FileIndexFileName,
            "gamepackage_candidate_matrix_file_index")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal129NestedFiles(
        string projectRoot,
        bool isExport)
    {
        var root = isExport
            ? GamePackageCandidateMatrixProjectionVocabulary.ExportPackageDirectory
            : GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory;
        var fullRoot = Resolve(projectRoot, root);
        if (!Directory.Exists(fullRoot))
        {
            return [];
        }

        return Directory.EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
            .Select(path => Relative(projectRoot, path))
            .Where(path =>
                path.EndsWith("/package.json", StringComparison.Ordinal)
                || path.EndsWith("/" + GamePackageCandidateMatrixProjectionVocabulary.MatrixResultFileName,
                    StringComparison.Ordinal)
                || path.EndsWith("/runner-result.json", StringComparison.Ordinal)
                || path.EndsWith("/log-scan.json", StringComparison.Ordinal))
            .Select(path => (
                RelativePath: path,
                Kind: path.EndsWith("/package.json", StringComparison.Ordinal)
                    ? "gamepackage_candidate_matrix_candidate_package"
                    : "gamepackage_candidate_matrix_runner_artifact"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal129ExportFiles(
        string projectRoot)
    {
        var files = BuildGoal129ProceduralFiles()
            .Select(item => (
                RelativePath:
                GamePackageCandidateMatrixProjectionVocabulary.ExportPackageDirectory + "/" + item.FileName,
                Kind: "gamepackage_candidate_matrix_export_file"))
            .ToList();
        files.AddRange(BuildGoal129NestedFiles(projectRoot, isExport: true));
        return files
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();
    }

    private static VisualWorldPreviewArtifactEntry WithGamePackageCandidateMatrixSummary(
        VisualWorldPreviewArtifactEntry entry,
        GamePackageCandidateMatrixWorkspaceSummary summary) =>
        entry with
        {
            GamePackageCandidateMatrixStatus = summary.MatrixStatus,
            GamePackageCandidateMatrixCandidateCount = summary.CandidateCount,
            GamePackageCandidateMatrixPassedCandidateCount = summary.PassedCandidateCount,
            GamePackageCandidateMatrixFailedCandidateCount = summary.FailedCandidateCount,
            GamePackageCandidateMatrixCandidateIndexPath = summary.CandidateIndexPath,
            GamePackageCandidateMatrixResultPath = summary.MatrixResultPath,
            GamePackageCandidateMatrixNormalCommand = summary.NormalCommand,
            GamePackageCandidateMatrixExampleCommand = summary.ExampleCommand,
            GamePackageCandidateMatrixBaselineCandidatePackagePath =
                summary.BaselineCandidatePackagePath,
            GamePackageCandidateMatrixVariantCandidatePackagePath =
                summary.VariantCandidatePackagePath,
            GamePackageCandidateMatrixManualUnityOptional = summary.ManualUnityOptional,
            GamePackageCandidateMatrixCleanupApplied = summary.CleanupApplied,
            GamePackageCandidateMatrixProjectionOnly = summary.ProjectionOnly,
            GamePackageCandidateMatrixScriptScanPassed = summary.ScriptScanPassed,
            GamePackageCandidateMatrixResultPassed = summary.MatrixResultPassed,
            GamePackageCandidateMatrixLogScanPassed = summary.LogScanPassed,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static GamePackageCandidateMatrixWorkspaceSummary
        LoadGamePackageCandidateMatrixSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + GamePackageCandidateMatrixProjectionVocabulary.DashboardFileName,
            diagnostics);
        using var scriptScan = TryReadOptionalGoal129Json(
            projectRoot,
            root + "/" + GamePackageCandidateMatrixProjectionVocabulary.ScriptScanFileName,
            diagnostics);
        using var matrixResult = TryReadOptionalGoal129Json(
            projectRoot,
            root + "/" + GamePackageCandidateMatrixProjectionVocabulary.MatrixResultFileName,
            diagnostics);
        using var logScan = TryReadOptionalGoal129Json(
            projectRoot,
            root + "/" + GamePackageCandidateMatrixProjectionVocabulary.LogScanFileName,
            diagnostics);
        return new GamePackageCandidateMatrixWorkspaceSummary(
            MatrixStatus: Goal129String(dashboard?.RootElement, "matrixStatus"),
            CandidateCount: dashboard is not null
                ? Goal129Int(dashboard.RootElement, "candidateCount")
                : 0,
            PassedCandidateCount: dashboard is not null
                ? Goal129Int(dashboard.RootElement, "passedCandidateCount")
                : 0,
            FailedCandidateCount: dashboard is not null
                ? Goal129Int(dashboard.RootElement, "failedCandidateCount")
                : 0,
            CandidateIndexPath: Goal129String(dashboard?.RootElement, "candidateIndexPath"),
            MatrixResultPath: Goal129String(dashboard?.RootElement, "matrixResultPath"),
            NormalCommand: Goal129String(dashboard?.RootElement, "normalCommand"),
            ExampleCommand: Goal129String(dashboard?.RootElement, "exampleCommand"),
            BaselineCandidatePackagePath:
                Goal129String(dashboard?.RootElement, "baselineCandidatePackagePath"),
            VariantCandidatePackagePath:
                Goal129String(dashboard?.RootElement, "variantCandidatePackagePath"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            CleanupApplied:
                dashboard is not null && TryGetBool(dashboard.RootElement, "cleanupApplied"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            ScriptScanPassed:
                scriptScan is not null && TryGetBool(scriptScan.RootElement, "passed"),
            MatrixResultPassed:
                matrixResult is not null && Goal129String(matrixResult.RootElement, "matrixStatus") == "GREEN",
            LogScanPassed:
                logScan is not null && TryGetBool(logScan.RootElement, "passed"),
            QualityGatePassed: Goal129String(dashboard?.RootElement, "matrixStatus") == "GREEN",
            RelativePaths: Goal129AllPathsRelative(projectRoot));
    }

    private static JsonDocument? TryReadOptionalGoal129Json(
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
                "goal129.json.invalid",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static VisualWorldPreviewArtifactEntry Goal129FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = GamePackageCandidateMatrixProjectionVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = GamePackageCandidateMatrixProjectionVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal129 matrix file exists" : "Goal129 matrix file missing",
            SafeRatingMetadataSummary = "matrixArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal129AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            GamePackageCandidateMatrixProjectionVocabulary.ProceduralOutputDirectory,
            GamePackageCandidateMatrixProjectionVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal129String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string Goal129String(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal129Int(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record GamePackageCandidateMatrixWorkspaceSummary(
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
