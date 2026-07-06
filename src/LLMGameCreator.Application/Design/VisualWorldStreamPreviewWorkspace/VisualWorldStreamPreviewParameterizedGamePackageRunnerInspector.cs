using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildParameterizedGamePackageRunnerGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadParameterizedGamePackageRunnerSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory,
                ParameterizedGamePackageProjectionRunnerVocabulary.GoalId,
                BuildGoal128ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithParameterizedGamePackageRunnerSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal128RunnerProducedProceduralFiles())
        {
            entries.Add(WithParameterizedGamePackageRunnerSummary(
                Goal128FileEntry(
                    projectRoot,
                    ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory
                    + "/"
                    + file.FileName,
                    file.Kind),
                summary));
        }

        foreach (var fileName in ParameterizedGamePackageProjectionRunnerVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithParameterizedGamePackageRunnerSummary(
                Goal128FileEntry(
                    projectRoot,
                    ParameterizedGamePackageProjectionRunnerVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "parameterized_gamepackage_runner_export_file"),
                summary));
        }

        entries.Add(WithParameterizedGamePackageRunnerSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId + ".summary",
                RelativePath =
                    ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory
                    + "/"
                    + ParameterizedGamePackageProjectionRunnerVocabulary.DashboardFileName,
                ArtifactKind = "parameterized_gamepackage_runner_workspace_summary",
                SourceGoalId = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory
                    + "/"
                    + ParameterizedGamePackageProjectionRunnerVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "parameterizedRunnerStatus=" + summary.RunnerStatus
                                    + "; packagePathRelative="
                                    + summary.PackagePathRelative,
                SafeRatingMetadataSummary =
                    "projectionOnly=true; normalCommand=" + summary.NormalCommand
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "parameterized_gamepackage_projection_runner",
            "Goal 128 Parameterized GamePackage Projection Runner",
            ParameterizedGamePackageProjectionRunnerVocabulary.GoalId,
            ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal128ProceduralFiles() =>
    [
        (ParameterizedGamePackageProjectionRunnerVocabulary.DashboardFileName,
            "parameterized_gamepackage_runner_dashboard"),
        (ParameterizedGamePackageProjectionRunnerVocabulary.ScriptScanFileName,
            "parameterized_gamepackage_runner_script_scan"),
        (ParameterizedGamePackageProjectionRunnerVocabulary.UnitySourceScanFileName,
            "parameterized_gamepackage_runner_unity_source_scan"),
        (ParameterizedGamePackageProjectionRunnerVocabulary.LogScanFileName,
            "parameterized_gamepackage_runner_log_scan"),
        (ParameterizedGamePackageProjectionRunnerVocabulary.ReportFileName,
            "parameterized_gamepackage_runner_report"),
        (ParameterizedGamePackageProjectionRunnerVocabulary.NegativeProofFileName,
            "parameterized_gamepackage_runner_negative_proof"),
        (ParameterizedGamePackageProjectionRunnerVocabulary.FileIndexFileName,
            "parameterized_gamepackage_runner_file_index")
    ];

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal128RunnerProducedProceduralFiles() =>
    [
        (ParameterizedGamePackageProjectionRunnerVocabulary.ResultFileName,
            "parameterized_gamepackage_runner_result"),
        (ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeLogFileName,
            "parameterized_gamepackage_runner_unity_log")
    ];

    private static VisualWorldPreviewArtifactEntry WithParameterizedGamePackageRunnerSummary(
        VisualWorldPreviewArtifactEntry entry,
        ParameterizedGamePackageRunnerWorkspaceSummary summary) =>
        entry with
        {
            ParameterizedGamePackageRunnerStatus = summary.RunnerStatus,
            ParameterizedGamePackageRunnerPackagePath = summary.PackagePath,
            ParameterizedGamePackageRunnerPackagePathRelative = summary.PackagePathRelative,
            ParameterizedGamePackageRunnerNormalCommand = summary.NormalCommand,
            ParameterizedGamePackageRunnerExampleCommandWithPackagePath =
                summary.ExampleCommandWithPackagePath,
            ParameterizedGamePackageRunnerResultPath = summary.ResultPath,
            ParameterizedGamePackageRunnerLogPath = summary.LogPath,
            ParameterizedGamePackageRunnerUnityExitCode = summary.UnityExitCode,
            ParameterizedGamePackageRunnerPassMarkerPresent = summary.PassMarkerPresent,
            ParameterizedGamePackageRunnerCleanupApplied = summary.CleanupApplied,
            ParameterizedGamePackageRunnerManualUnityOptional = summary.ManualUnityOptional,
            ParameterizedGamePackageRunnerProjectionOnly = summary.ProjectionOnly,
            ParameterizedGamePackageRunnerEvidencePath = summary.EvidencePath,
            ParameterizedGamePackageRunnerExportPath = summary.ExportPath,
            ParameterizedGamePackageRunnerScriptScanPassed = summary.ScriptScanPassed,
            ParameterizedGamePackageRunnerUnitySourceScanPassed = summary.UnitySourceScanPassed,
            ParameterizedGamePackageRunnerResultPassed = summary.ResultArtifactPassed,
            ParameterizedGamePackageRunnerLogPassed = summary.LogArtifactPassed,
            ParameterizedGamePackageRunnerGoal127Green = summary.Goal127Green,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static ParameterizedGamePackageRunnerWorkspaceSummary
        LoadParameterizedGamePackageRunnerSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + ParameterizedGamePackageProjectionRunnerVocabulary.DashboardFileName,
            diagnostics);
        using var result = TryReadOptionalGoal128Json(
            projectRoot,
            root + "/" + ParameterizedGamePackageProjectionRunnerVocabulary.ResultFileName,
            diagnostics);
        using var logScan = TryReadOptionalGoal128Json(
            projectRoot,
            root + "/" + ParameterizedGamePackageProjectionRunnerVocabulary.LogScanFileName,
            diagnostics);
        return new ParameterizedGamePackageRunnerWorkspaceSummary(
            RunnerStatus: Goal128String(dashboard?.RootElement, "parameterizedRunnerStatus"),
            PackagePath: Goal128String(dashboard?.RootElement, "packagePath"),
            PackagePathRelative: Goal128String(dashboard?.RootElement, "packagePathRelative"),
            NormalCommand: Goal128String(dashboard?.RootElement, "normalCommand"),
            ExampleCommandWithPackagePath:
                Goal128String(dashboard?.RootElement, "exampleCommandWithPackagePath"),
            ResultPath: Goal128String(dashboard?.RootElement, "resultPath"),
            LogPath: Goal128String(dashboard?.RootElement, "logPath"),
            UnityExitCode: dashboard is not null
                ? Goal128Int(dashboard.RootElement, "unityExitCode")
                : -1,
            PassMarkerPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "passMarkerPresent"),
            CleanupApplied:
                dashboard is not null && TryGetBool(dashboard.RootElement, "cleanupApplied"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            EvidencePath: Goal128String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal128String(dashboard?.RootElement, "exportPath"),
            ScriptScanPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "scriptScanPassed"),
            UnitySourceScanPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unitySourceScanPassed"),
            ResultArtifactPassed:
                result is not null && TryGetBool(result.RootElement, "passed"),
            LogArtifactPassed:
                logScan is not null && TryGetBool(logScan.RootElement, "passed"),
            Goal127Green:
                dashboard is not null && TryGetBool(dashboard.RootElement, "goal127RunnerGreen"),
            QualityGatePassed:
                Goal128String(dashboard?.RootElement, "parameterizedRunnerStatus") == "GREEN",
            RelativePaths: Goal128AllPathsRelative(projectRoot));
    }

    private static JsonDocument? TryReadOptionalGoal128Json(
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
                "goal128.json.invalid",
                relativePath,
                ex.Message));
            return null;
        }
    }

    private static VisualWorldPreviewArtifactEntry Goal128FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal128 runner file exists" : "Goal128 runner file missing",
            SafeRatingMetadataSummary = "runnerArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal128AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory,
            ParameterizedGamePackageProjectionRunnerVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal128String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal128Int(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : -1;

    private sealed record ParameterizedGamePackageRunnerWorkspaceSummary(
        string RunnerStatus,
        string PackagePath,
        string PackagePathRelative,
        string NormalCommand,
        string ExampleCommandWithPackagePath,
        string ResultPath,
        string LogPath,
        int UnityExitCode,
        bool PassMarkerPresent,
        bool CleanupApplied,
        bool ManualUnityOptional,
        bool ProjectionOnly,
        string EvidencePath,
        string ExportPath,
        bool ScriptScanPassed,
        bool UnitySourceScanPassed,
        bool ResultArtifactPassed,
        bool LogArtifactPassed,
        bool Goal127Green,
        bool QualityGatePassed,
        bool RelativePaths);
}
