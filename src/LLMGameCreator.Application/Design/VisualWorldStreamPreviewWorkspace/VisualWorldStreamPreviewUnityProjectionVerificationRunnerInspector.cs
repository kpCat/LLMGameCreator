using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildUnityProjectionVerificationRunnerGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadUnityProjectionVerificationRunnerSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory,
                UnityProjectionVerificationRunnerVocabulary.GoalId,
                BuildGoal127ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithUnityProjectionVerificationRunnerSummary(entry, summary))
            .ToList();

        foreach (var fileName in UnityProjectionVerificationRunnerVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithUnityProjectionVerificationRunnerSummary(
                Goal127FileEntry(
                    projectRoot,
                    UnityProjectionVerificationRunnerVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "unity_projection_verification_runner_export_file"),
                summary));
        }

        entries.Add(WithUnityProjectionVerificationRunnerSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = UnityProjectionVerificationRunnerVocabulary.GoalId + ".summary",
                RelativePath =
                    UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory
                    + "/"
                    + UnityProjectionVerificationRunnerVocabulary.DashboardFileName,
                ArtifactKind = "unity_projection_verification_runner_workspace_summary",
                SourceGoalId = UnityProjectionVerificationRunnerVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory
                    + "/"
                    + UnityProjectionVerificationRunnerVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "runnerStatus=" + summary.RunnerStatus
                                    + "; passMarkerPresent="
                                    + summary.PassMarkerPresent.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "manualUnityClickingRequired=false; runnerCommand="
                    + summary.RunnerCommand
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "unity_projection_verification_runner",
            "Goal 127 Unity Projection Verification Runner",
            UnityProjectionVerificationRunnerVocabulary.GoalId,
            UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal127ProceduralFiles() =>
    [
        (UnityProjectionVerificationRunnerVocabulary.DashboardFileName,
            "unity_projection_verification_runner_dashboard"),
        (UnityProjectionVerificationRunnerVocabulary.ScriptScanFileName,
            "unity_projection_verification_runner_script_scan"),
        (UnityProjectionVerificationRunnerVocabulary.ResultFileName,
            "unity_projection_verification_runner_result"),
        (UnityProjectionVerificationRunnerVocabulary.LogScanFileName,
            "unity_projection_verification_runner_log_scan"),
        (UnityProjectionVerificationRunnerVocabulary.ReportFileName,
            "unity_projection_verification_runner_report"),
        (UnityProjectionVerificationRunnerVocabulary.NegativeProofFileName,
            "unity_projection_verification_runner_negative_proof"),
        (UnityProjectionVerificationRunnerVocabulary.FileIndexFileName,
            "unity_projection_verification_runner_file_index"),
        (UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeLogFileName,
            "unity_projection_verification_runner_unity_log")
    ];

    private static VisualWorldPreviewArtifactEntry WithUnityProjectionVerificationRunnerSummary(
        VisualWorldPreviewArtifactEntry entry,
        UnityProjectionVerificationRunnerWorkspaceSummary summary) =>
        entry with
        {
            UnityProjectionVerificationRunnerStatus = summary.RunnerStatus,
            UnityProjectionVerificationRunnerMode = summary.Mode,
            UnityProjectionVerificationRunnerScriptPath = summary.RunnerScriptPath,
            UnityProjectionVerificationRunnerCmdPath = summary.RunnerCmdPath,
            UnityProjectionVerificationRunnerCommand = summary.RunnerCommand,
            UnityProjectionVerificationRunnerExecuteMethod = summary.UnityExecuteMethod,
            UnityProjectionVerificationRunnerResultPath = summary.LastResultPath,
            UnityProjectionVerificationRunnerLogPath = summary.LastLogPath,
            UnityProjectionVerificationRunnerPassMarkerPresent = summary.PassMarkerPresent,
            UnityProjectionVerificationRunnerCleanupApplied = summary.CleanupApplied,
            UnityProjectionVerificationRunnerCleanupScriptAvailable =
                summary.CleanupScriptAvailable,
            UnityProjectionVerificationRunnerCleanupCommand = summary.CleanupCommand,
            UnityProjectionVerificationRunnerManualUnityClickingRequired =
                summary.ManualUnityClickingRequired,
            UnityProjectionVerificationRunnerEvidencePath = summary.EvidencePath,
            UnityProjectionVerificationRunnerExportPath = summary.ExportPath,
            UnityProjectionVerificationRunnerScriptScanPassed = summary.ScriptScanPassed,
            UnityProjectionVerificationRunnerResultPassed = summary.ResultArtifactPassed,
            UnityProjectionVerificationRunnerLogPassed = summary.LogArtifactPassed,
            UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen =
                summary.Goal126FullPlaythroughGreen,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static UnityProjectionVerificationRunnerWorkspaceSummary
        LoadUnityProjectionVerificationRunnerSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + UnityProjectionVerificationRunnerVocabulary.DashboardFileName,
            diagnostics);
        using var result = TryReadJson(
            projectRoot,
            root + "/" + UnityProjectionVerificationRunnerVocabulary.ResultFileName,
            diagnostics);
        using var logScan = TryReadJson(
            projectRoot,
            root + "/" + UnityProjectionVerificationRunnerVocabulary.LogScanFileName,
            diagnostics);
        return new UnityProjectionVerificationRunnerWorkspaceSummary(
            RunnerStatus: Goal127String(dashboard?.RootElement, "runnerStatus"),
            Mode: Goal127String(dashboard?.RootElement, "mode"),
            RunnerScriptPath: Goal127String(dashboard?.RootElement, "runnerScriptPath"),
            RunnerCmdPath: Goal127String(dashboard?.RootElement, "runnerCmdPath"),
            RunnerCommand: Goal127String(dashboard?.RootElement, "runnerCommand"),
            UnityExecuteMethod: Goal127String(dashboard?.RootElement, "unityExecuteMethod"),
            LastResultPath: Goal127String(dashboard?.RootElement, "lastResultPath"),
            LastLogPath: Goal127String(dashboard?.RootElement, "lastLogPath"),
            PassMarkerPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "passMarkerPresent"),
            CleanupApplied:
                dashboard is not null && TryGetBool(dashboard.RootElement, "cleanupApplied"),
            CleanupScriptAvailable:
                dashboard is not null && TryGetBool(dashboard.RootElement, "cleanupScriptAvailable"),
            CleanupCommand: Goal127String(dashboard?.RootElement, "cleanupCommand"),
            ManualUnityClickingRequired:
                dashboard is not null
                && TryGetBool(dashboard.RootElement, "manualUnityClickingRequired"),
            EvidencePath: Goal127String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal127String(dashboard?.RootElement, "exportPath"),
            ScriptScanPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "scriptScanPassed"),
            ResultArtifactPassed:
                result is not null && TryGetBool(result.RootElement, "passed"),
            LogArtifactPassed:
                logScan is not null && TryGetBool(logScan.RootElement, "passed"),
            Goal126FullPlaythroughGreen:
                dashboard is not null
                && TryGetBool(dashboard.RootElement, "goal126FullPlaythroughGreen"),
            QualityGatePassed:
                Goal127String(dashboard?.RootElement, "runnerStatus") == "GREEN",
            RelativePaths: Goal127AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal127FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = UnityProjectionVerificationRunnerVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = UnityProjectionVerificationRunnerVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal127 runner file exists" : "Goal127 runner file missing",
            SafeRatingMetadataSummary = "runnerArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal127AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory,
            UnityProjectionVerificationRunnerVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal127String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record UnityProjectionVerificationRunnerWorkspaceSummary(
        string RunnerStatus,
        string Mode,
        string RunnerScriptPath,
        string RunnerCmdPath,
        string RunnerCommand,
        string UnityExecuteMethod,
        string LastResultPath,
        string LastLogPath,
        bool PassMarkerPresent,
        bool CleanupApplied,
        bool CleanupScriptAvailable,
        string CleanupCommand,
        bool ManualUnityClickingRequired,
        string EvidencePath,
        string ExportPath,
        bool ScriptScanPassed,
        bool ResultArtifactPassed,
        bool LogArtifactPassed,
        bool Goal126FullPlaythroughGreen,
        bool QualityGatePassed,
        bool RelativePaths);
}
