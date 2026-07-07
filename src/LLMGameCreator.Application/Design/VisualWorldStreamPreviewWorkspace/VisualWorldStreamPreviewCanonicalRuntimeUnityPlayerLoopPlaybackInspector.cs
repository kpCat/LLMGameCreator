using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildCanonicalRuntimeUnityPlayerLoopPlaybackGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadCanonicalRuntimeUnityPlayerLoopPlaybackSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory,
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId,
                BuildGoal137ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithCanonicalRuntimeUnityPlayerLoopPlaybackSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal137ExportFiles())
        {
            entries.Add(WithCanonicalRuntimeUnityPlayerLoopPlaybackSummary(
                Goal137FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithCanonicalRuntimeUnityPlayerLoopPlaybackSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId + ".summary",
                RelativePath = CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DashboardRelativePath,
                ArtifactKind = "canonical_runtime_unity_player_loop_playback_workspace_summary",
                SourceGoalId = CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "candidateId=" + summary.CandidateId
                                    + "; playbackFrameCount=" + summary.PlaybackFrameCount
                                    + "; unityPlayerLoopPlaybackPassed="
                                    + summary.UnityPlayerLoopPlaybackPassed.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "runtimeSnapshotSource=true; projectionOnly=false; unityGameplayTruth=false"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "canonical_runtime_unity_player_loop_playback",
            "Goal 137 Canonical Runtime Unity Player Loop Playback",
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal137ProceduralFiles() =>
    [
        (CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DashboardFileName,
            "canonical_runtime_unity_player_loop_playback_dashboard"),
        (CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ResultFileName,
            "canonical_runtime_unity_player_loop_playback_result"),
        (CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.PlanFileName,
            "canonical_runtime_unity_player_loop_playback_plan"),
        (CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FramesFileName,
            "canonical_runtime_unity_player_loop_playback_frames"),
        (CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.MatrixResultFileName,
            "canonical_runtime_unity_player_loop_playback_matrix_result"),
        (CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.UnitySmokeFileName,
            "unity_player_loop_playback_smoke"),
        (CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.NegativeProofFileName,
            "canonical_runtime_unity_player_loop_playback_negative_proof"),
        (CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FileIndexFileName,
            "canonical_runtime_unity_player_loop_playback_file_index"),
        (CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ReportJsonFileName,
            "unity_player_loop_playback_one_click_report_json"),
        (CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ReportMarkdownFileName,
            "unity_player_loop_playback_one_click_report_markdown")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal137ExportFiles() =>
        BuildGoal137ProceduralFiles()
            .Select(item => (
                RelativePath:
                CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ExportPackageDirectory
                + "/"
                + item.FileName,
                Kind: "canonical_runtime_unity_player_loop_playback_export_file"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

    private static VisualWorldPreviewArtifactEntry WithCanonicalRuntimeUnityPlayerLoopPlaybackSummary(
        VisualWorldPreviewArtifactEntry entry,
        CanonicalRuntimeUnityPlayerLoopPlaybackWorkspaceSummary summary) =>
        entry with
        {
            CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId = summary.CandidateId,
            CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount = summary.PlaybackFrameCount,
            CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent =
                summary.RequiredFrameCategoriesPresent,
            CanonicalRuntimeUnityPlayerLoopPlaybackPassed = summary.UnityPlayerLoopPlaybackPassed,
            CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource =
                summary.RuntimeSnapshotSource,
            CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth =
                summary.UnityGameplayTruth,
            CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly = summary.ProjectionOnly,
            CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime =
                summary.SelectedCandidateExecutedByRuntime,
            CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand = summary.NormalCommand,
            CanonicalRuntimeUnityPlayerLoopPlaybackReportPath = summary.ReportPath,
            CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath = summary.MatrixResultPath,
            CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional =
                summary.ManualUnityOptional,
            CanonicalRuntimeUnityPlayerLoopPlaybackAccepted = summary.Accepted,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static CanonicalRuntimeUnityPlayerLoopPlaybackWorkspaceSummary
        LoadCanonicalRuntimeUnityPlayerLoopPlaybackSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var dashboard = TryReadJson(
            projectRoot,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DashboardRelativePath,
            diagnostics);
        return new CanonicalRuntimeUnityPlayerLoopPlaybackWorkspaceSummary(
            CandidateId: Goal137String(dashboard?.RootElement, "candidateId"),
            PlaybackFrameCount: dashboard is not null
                ? Goal137Int(dashboard.RootElement, "playbackFrameCount")
                : 0,
            RequiredFrameCategoriesPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "requiredFrameCategoriesPresent"),
            UnityPlayerLoopPlaybackPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityPlayerLoopPlaybackPassed"),
            RuntimeSnapshotSource:
                dashboard is not null && TryGetBool(dashboard.RootElement, "runtimeSnapshotSource"),
            UnityGameplayTruth:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityGameplayTruth"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            SelectedCandidateExecutedByRuntime:
                dashboard is not null && TryGetBool(dashboard.RootElement, "selectedCandidateExecutedByRuntime"),
            NormalCommand: Goal137String(dashboard?.RootElement, "normalCommand"),
            ReportPath: Goal137String(dashboard?.RootElement, "reportPath"),
            MatrixResultPath: Goal137String(dashboard?.RootElement, "matrixResultPath"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            Accepted:
                dashboard is not null && TryGetBool(dashboard.RootElement, "accepted"),
            QualityGatePassed:
                Goal137String(dashboard?.RootElement, "status") == "GREEN",
            RelativePaths: Goal137AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal137FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists
                ? "Goal137 Unity/player loop playback file exists"
                : "Goal137 Unity/player loop playback file missing",
            SafeRatingMetadataSummary = "canonicalRuntimeUnityPlayerLoopPlaybackArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal137AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal137String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal137Int(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record CanonicalRuntimeUnityPlayerLoopPlaybackWorkspaceSummary(
        string CandidateId,
        int PlaybackFrameCount,
        bool RequiredFrameCategoriesPresent,
        bool UnityPlayerLoopPlaybackPassed,
        bool RuntimeSnapshotSource,
        bool UnityGameplayTruth,
        bool ProjectionOnly,
        bool SelectedCandidateExecutedByRuntime,
        string NormalCommand,
        string ReportPath,
        string MatrixResultPath,
        bool ManualUnityOptional,
        bool Accepted,
        bool QualityGatePassed,
        bool RelativePaths);
}
