using System.Text;
using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildCanonicalRuntimeSelectedCandidateGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadCanonicalRuntimeSelectedCandidateSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory,
                CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId,
                BuildGoal134ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithCanonicalRuntimeSelectedCandidateSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal134ExportFiles())
        {
            entries.Add(WithCanonicalRuntimeSelectedCandidateSummary(
                Goal134FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithCanonicalRuntimeSelectedCandidateSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId + ".summary",
                RelativePath = CanonicalRuntimeSelectedCandidatePlaythroughVocabulary
                    .ProceduralOutputDirectory
                    + "/"
                    + CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DashboardFileName,
                ArtifactKind = "canonical_runtime_selected_candidate_workspace_summary",
                SourceGoalId = CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory
                    + "/"
                    + CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "candidateId=" + summary.CandidateId
                                    + "; runtimeCommandCount=" + summary.RuntimeCommandCount
                                    + "; runtimeEventCount=" + summary.RuntimeEventCount,
                SafeRatingMetadataSummary =
                    "projectionOnly=false; selectedCandidateExecutedByRuntime="
                    + summary.SelectedCandidateExecutedByRuntime.ToString().ToLowerInvariant()
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "canonical_runtime_selected_candidate_playthrough",
            "Goal 134 Canonical Runtime Selected Candidate Playthrough",
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal134ProceduralFiles() =>
    [
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.PackageValidationFileName,
            "canonical_runtime_package_validation"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.PlaythroughScriptFileName,
            "canonical_runtime_playthrough_script"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.TranscriptFileName,
            "canonical_runtime_transcript"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.StateSummaryFileName,
            "canonical_runtime_state_summary"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.SaveLoadReplayResultFileName,
            "canonical_runtime_save_load_replay_result"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.MatrixResultFileName,
            "canonical_runtime_matrix_result"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.UnitySmokeFileName,
            "canonical_runtime_unity_transcript_smoke"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ReportJsonFileName,
            "canonical_runtime_one_click_report_json"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ReportMarkdownFileName,
            "canonical_runtime_one_click_report_markdown"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.NegativeProofFileName,
            "canonical_runtime_negative_proof"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.FileIndexFileName,
            "canonical_runtime_file_index"),
        (CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DashboardFileName,
            "canonical_runtime_dashboard")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal134ExportFiles() =>
        BuildGoal134ProceduralFiles()
            .Select(item => (
                RelativePath:
                CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ExportPackageDirectory
                + "/"
                + item.FileName,
                Kind: "canonical_runtime_export_file"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

    private static VisualWorldPreviewArtifactEntry WithCanonicalRuntimeSelectedCandidateSummary(
        VisualWorldPreviewArtifactEntry entry,
        CanonicalRuntimeSelectedCandidateWorkspaceSummary summary) =>
        entry with
        {
            CanonicalRuntimeCandidateId = summary.CandidateId,
            CanonicalRuntimePackageValidationPassed = summary.PackageValidationPassed,
            CanonicalRuntimePassed = summary.CanonicalRuntimePassed,
            CanonicalRuntimeCommandCount = summary.RuntimeCommandCount,
            CanonicalRuntimeEventCount = summary.RuntimeEventCount,
            CanonicalRuntimeSaveLoadReplayPassed = summary.SaveLoadReplayPassed,
            CanonicalRuntimeUnityPlayerConsumedTranscript =
                summary.UnityPlayerConsumedCanonicalTranscript,
            CanonicalRuntimeProjectionOnly = summary.ProjectionOnly,
            CanonicalRuntimeSelectedCandidateExecutedByRuntime =
                summary.SelectedCandidateExecutedByRuntime,
            CanonicalRuntimeNormalCommand = summary.NormalCommand,
            CanonicalRuntimeReportPath = summary.ReportPath,
            CanonicalRuntimeMatrixResultPath = summary.MatrixResultPath,
            CanonicalRuntimeManualUnityOptional = summary.ManualUnityOptional,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static CanonicalRuntimeSelectedCandidateWorkspaceSummary
        LoadCanonicalRuntimeSelectedCandidateSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var dashboardRelative =
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory
            + "/"
            + CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.DashboardFileName;
        using var dashboard = TryReadJson(projectRoot, dashboardRelative, diagnostics);
        return new CanonicalRuntimeSelectedCandidateWorkspaceSummary(
            CandidateId: Goal134String(dashboard?.RootElement, "candidateId"),
            PackageValidationPassed: dashboard is not null
                                     && TryGetBool(dashboard.RootElement, "packageValidationPassed"),
            CanonicalRuntimePassed: dashboard is not null
                                    && TryGetBool(dashboard.RootElement, "canonicalRuntimePassed"),
            RuntimeCommandCount: dashboard is not null
                ? Goal134Int(dashboard.RootElement, "runtimeCommandCount")
                : 0,
            RuntimeEventCount: dashboard is not null
                ? Goal134Int(dashboard.RootElement, "runtimeEventCount")
                : 0,
            SaveLoadReplayPassed: dashboard is not null
                                  && TryGetBool(dashboard.RootElement, "saveLoadReplayPassed"),
            UnityPlayerConsumedCanonicalTranscript:
                dashboard is not null
                && TryGetBool(dashboard.RootElement, "unityConsumedCanonicalTranscript"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            SelectedCandidateExecutedByRuntime:
                dashboard is not null
                && TryGetBool(dashboard.RootElement, "selectedCandidateExecutedByRuntime"),
            NormalCommand: Goal134String(dashboard?.RootElement, "normalCommand"),
            ReportPath: Goal134String(dashboard?.RootElement, "reportPath"),
            MatrixResultPath: Goal134String(dashboard?.RootElement, "matrixResultPath"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            QualityGatePassed:
                Goal134String(dashboard?.RootElement, "status") == "GREEN",
            RelativePaths: Goal134AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal134FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal134 canonical runtime file exists" : "Goal134 canonical runtime file missing",
            SafeRatingMetadataSummary = "canonicalRuntimeArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal134AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimeSelectedCandidatePlaythroughVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal134String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal134Int(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record CanonicalRuntimeSelectedCandidateWorkspaceSummary(
        string CandidateId,
        bool PackageValidationPassed,
        bool CanonicalRuntimePassed,
        int RuntimeCommandCount,
        int RuntimeEventCount,
        bool SaveLoadReplayPassed,
        bool UnityPlayerConsumedCanonicalTranscript,
        bool ProjectionOnly,
        bool SelectedCandidateExecutedByRuntime,
        string NormalCommand,
        string ReportPath,
        string MatrixResultPath,
        bool ManualUnityOptional,
        bool QualityGatePassed,
        bool RelativePaths);
}
