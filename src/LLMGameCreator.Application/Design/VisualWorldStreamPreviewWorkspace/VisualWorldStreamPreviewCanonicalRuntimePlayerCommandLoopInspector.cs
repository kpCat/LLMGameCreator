using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildCanonicalRuntimePlayerCommandLoopGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadCanonicalRuntimePlayerCommandLoopSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory,
                CanonicalRuntimePlayerCommandLoopVocabulary.GoalId,
                BuildGoal136ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithCanonicalRuntimePlayerCommandLoopSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal136ExportFiles())
        {
            entries.Add(WithCanonicalRuntimePlayerCommandLoopSummary(
                Goal136FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithCanonicalRuntimePlayerCommandLoopSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId + ".summary",
                RelativePath = CanonicalRuntimePlayerCommandLoopVocabulary.DashboardRelativePath,
                ArtifactKind = "canonical_runtime_player_command_loop_workspace_summary",
                SourceGoalId = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    CanonicalRuntimePlayerCommandLoopVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "candidateId=" + summary.CandidateId
                                    + "; playerCommandCount=" + summary.PlayerCommandCount
                                    + "; snapshotCount=" + summary.SnapshotCount
                                    + "; unityPlayerConsumedCommandLoopSnapshots="
                                    + summary.UnityConsumedSnapshots.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "projectionOnly=false; unityGameplayTruth=false; commandLoopCoverage=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "canonical_runtime_player_command_loop",
            "Goal 136 Canonical Runtime Player Command Loop",
            CanonicalRuntimePlayerCommandLoopVocabulary.GoalId,
            CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal136ProceduralFiles() =>
    [
        (CanonicalRuntimePlayerCommandLoopVocabulary.DashboardFileName,
            "canonical_runtime_player_command_loop_dashboard"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.InputsFileName,
            "canonical_runtime_player_command_loop_inputs"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.PlanFileName,
            "canonical_runtime_player_command_loop_plan"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.SnapshotsFileName,
            "canonical_runtime_player_command_loop_snapshots"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.ResultFileName,
            "canonical_runtime_player_command_loop_result"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.MatrixResultFileName,
            "canonical_runtime_player_command_loop_matrix_result"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.DiagnosticClassificationFileName,
            "canonical_runtime_player_command_loop_diagnostic_classification"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.UnitySmokeFileName,
            "unity_player_command_loop_smoke"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.ReportJsonFileName,
            "player_command_loop_one_click_report_json"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.ReportMarkdownFileName,
            "player_command_loop_one_click_report_markdown"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.NegativeProofFileName,
            "canonical_runtime_player_command_loop_negative_proof"),
        (CanonicalRuntimePlayerCommandLoopVocabulary.FileIndexFileName,
            "canonical_runtime_player_command_loop_file_index")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal136ExportFiles() =>
        BuildGoal136ProceduralFiles()
            .Select(item => (
                RelativePath:
                CanonicalRuntimePlayerCommandLoopVocabulary.ExportPackageDirectory
                + "/"
                + item.FileName,
                Kind: "canonical_runtime_player_command_loop_export_file"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

    private static VisualWorldPreviewArtifactEntry WithCanonicalRuntimePlayerCommandLoopSummary(
        VisualWorldPreviewArtifactEntry entry,
        CanonicalRuntimePlayerCommandLoopWorkspaceSummary summary) =>
        entry with
        {
            CanonicalRuntimePlayerCommandLoopCandidateId = summary.CandidateId,
            CanonicalRuntimePlayerCommandLoopPassed = summary.PlayerCommandLoopPassed,
            CanonicalRuntimePlayerCommandCount = summary.PlayerCommandCount,
            CanonicalRuntimePlayerSnapshotCount = summary.SnapshotCount,
            CanonicalRuntimePlayerCommandLoopRuntimeEventCount = summary.RuntimeEventCount,
            CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent =
                summary.AllRequiredCategoriesPresent,
            CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots =
                summary.UnityConsumedSnapshots,
            CanonicalRuntimePlayerCommandLoopProjectionOnly = summary.ProjectionOnly,
            CanonicalRuntimePlayerCommandLoopUnityGameplayTruth = summary.UnityGameplayTruth,
            CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors =
                summary.NoUnclassifiedErrorDiagnostics,
            CanonicalRuntimePlayerCommandLoopNormalCommand = summary.NormalCommand,
            CanonicalRuntimePlayerCommandLoopReportPath = summary.ReportPath,
            CanonicalRuntimePlayerCommandLoopMatrixResultPath = summary.MatrixResultPath,
            CanonicalRuntimePlayerCommandLoopManualUnityOptional = summary.ManualUnityOptional,
            CanonicalRuntimePlayerCommandLoopAccepted = summary.Accepted,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static CanonicalRuntimePlayerCommandLoopWorkspaceSummary
        LoadCanonicalRuntimePlayerCommandLoopSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var dashboard = TryReadJson(
            projectRoot,
            CanonicalRuntimePlayerCommandLoopVocabulary.DashboardRelativePath,
            diagnostics);
        return new CanonicalRuntimePlayerCommandLoopWorkspaceSummary(
            CandidateId: Goal136String(dashboard?.RootElement, "candidateId"),
            PlayerCommandLoopPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "playerCommandLoopPassed"),
            PlayerCommandCount: dashboard is not null
                ? Goal136Int(dashboard.RootElement, "playerCommandCount")
                : 0,
            SnapshotCount: dashboard is not null
                ? Goal136Int(dashboard.RootElement, "snapshotCount")
                : 0,
            RuntimeEventCount: dashboard is not null
                ? Goal136Int(dashboard.RootElement, "runtimeEventCount")
                : 0,
            AllRequiredCategoriesPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "allRequiredCategoriesPresent"),
            UnityConsumedSnapshots:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityPlayerConsumedCommandLoopSnapshots"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            UnityGameplayTruth:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityGameplayTruth"),
            NoUnclassifiedErrorDiagnostics:
                dashboard is not null && TryGetBool(dashboard.RootElement, "noUnclassifiedErrorDiagnostics"),
            NormalCommand: Goal136String(dashboard?.RootElement, "normalCommand"),
            ReportPath: Goal136String(dashboard?.RootElement, "reportPath"),
            MatrixResultPath: Goal136String(dashboard?.RootElement, "matrixResultPath"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            Accepted:
                dashboard is not null && TryGetBool(dashboard.RootElement, "accepted"),
            QualityGatePassed:
                Goal136String(dashboard?.RootElement, "status") == "GREEN",
            RelativePaths: Goal136AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal136FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = CanonicalRuntimePlayerCommandLoopVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal136 player command-loop file exists" : "Goal136 player command-loop file missing",
            SafeRatingMetadataSummary = "canonicalRuntimePlayerCommandLoopArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal136AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimePlayerCommandLoopVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal136String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal136Int(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record CanonicalRuntimePlayerCommandLoopWorkspaceSummary(
        string CandidateId,
        bool PlayerCommandLoopPassed,
        int PlayerCommandCount,
        int SnapshotCount,
        int RuntimeEventCount,
        bool AllRequiredCategoriesPresent,
        bool UnityConsumedSnapshots,
        bool ProjectionOnly,
        bool UnityGameplayTruth,
        bool NoUnclassifiedErrorDiagnostics,
        string NormalCommand,
        string ReportPath,
        string MatrixResultPath,
        bool ManualUnityOptional,
        bool Accepted,
        bool QualityGatePassed,
        bool RelativePaths);
}
