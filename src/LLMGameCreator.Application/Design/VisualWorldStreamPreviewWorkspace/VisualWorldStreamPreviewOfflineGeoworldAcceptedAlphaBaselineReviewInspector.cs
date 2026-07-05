using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldAcceptedAlphaBaselineReviewGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldAcceptedAlphaBaselineSummary(
            projectRoot,
            groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory,
                OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId,
                BuildGoal118ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldAcceptedAlphaBaselineSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary
                     .RequiredExportFileNames)
        {
            entries.Add(WithOfflineGeoworldAcceptedAlphaBaselineSummary(
                Goal118FileEntry(
                    projectRoot,
                    OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "offline_geoworld_accepted_alpha_baseline_export_file"),
                summary));
        }

        entries.Add(WithOfflineGeoworldAcceptedAlphaBaselineSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId + ".summary",
                RelativePath =
                    OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory
                    + "/"
                    + OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.DashboardFileName,
                ArtifactKind = "offline_geoworld_accepted_alpha_baseline_workspace_summary",
                SourceGoalId = OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory
                    + "/"
                    + OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "baselineId=" + summary.BaselineId
                                    + "; acceptedBaselineReady="
                                    + summary.AcceptedBaselineReady.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "notFinalRelease=true; noRuntimeProviderNetwork=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_accepted_alpha_baseline_review",
            "Goal 118 Offline Geoworld Accepted Alpha Baseline Review",
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId,
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal118ProceduralFiles() =>
    [
        (OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.DashboardFileName,
            "offline_geoworld_accepted_alpha_baseline_dashboard"),
        (OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ManifestFileName,
            "offline_geoworld_accepted_alpha_baseline_manifest"),
        (OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceIndexFileName,
            "offline_geoworld_accepted_alpha_baseline_source_index"),
        (OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ReportFileName,
            "offline_geoworld_accepted_alpha_baseline_report"),
        (OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.QualityGateScanFileName,
            "offline_geoworld_accepted_alpha_baseline_quality_gate"),
        (OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.NegativeProofFileName,
            "offline_geoworld_accepted_alpha_baseline_negative_proof"),
        (OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.FileIndexFileName,
            "offline_geoworld_accepted_alpha_baseline_file_index")
    ];

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldAcceptedAlphaBaselineSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldAcceptedAlphaBaselineWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldAcceptedAlphaBaselineId = summary.BaselineId,
            OfflineGeoworldAcceptedAlphaBaselineHash = summary.BaselineHash,
            OfflineGeoworldAcceptedAlphaBaselineReady = summary.AcceptedBaselineReady,
            OfflineGeoworldAcceptedAlphaManualGateStatus = summary.ManualGateStatus,
            OfflineGeoworldAcceptedAlphaRecommendedNextDecision =
                summary.RecommendedNextDecision,
            OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount =
                summary.IncludedSourceGoalCount,
            OfflineGeoworldAcceptedAlphaAcceptedEvidenceRootCount =
                summary.AcceptedEvidenceRootCount,
            OfflineGeoworldAcceptedAlphaProducedOnlyRootCount =
                summary.ProducedOnlyRootCount,
            OfflineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount =
                summary.BlockedOrSupersededNoteCount,
            OfflineGeoworldAcceptedAlphaDoNotStartAutomatically =
                summary.DoNotStartAutomatically,
            OfflineGeoworldAcceptedAlphaEvidencePath = summary.EvidencePath,
            OfflineGeoworldAcceptedAlphaExportPath = summary.ExportPath,
            OfflineGeoworldAcceptedAlphaErrors = summary.Errors,
            OfflineGeoworldAcceptedAlphaWarnings = summary.Warnings,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldAcceptedAlphaBaselineWorkspaceSummary
        LoadOfflineGeoworldAcceptedAlphaBaselineSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.DashboardFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            root + "/" + OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.QualityGateScanFileName,
            diagnostics);
        return new OfflineGeoworldAcceptedAlphaBaselineWorkspaceSummary(
            BaselineId: Goal118String(dashboard?.RootElement, "baselineId"),
            BaselineHash: Goal118String(dashboard?.RootElement, "baselineHash"),
            AcceptedBaselineReady:
                dashboard is not null && TryGetBool(dashboard.RootElement, "acceptedBaselineReady"),
            ManualGateStatus: Goal118String(dashboard?.RootElement, "manualGateStatus"),
            RecommendedNextDecision: Goal118String(dashboard?.RootElement, "recommendedNextDecision"),
            IncludedSourceGoalCount: Goal118Int(dashboard?.RootElement, "includedSourceGoalCount"),
            AcceptedEvidenceRootCount: Goal118Int(dashboard?.RootElement, "acceptedEvidenceRootCount"),
            ProducedOnlyRootCount: Goal118Int(dashboard?.RootElement, "producedOnlyRootCount"),
            BlockedOrSupersededNoteCount:
                Goal118Int(dashboard?.RootElement, "blockedOrSupersededNoteCount"),
            DoNotStartAutomatically:
                dashboard is not null && TryGetBool(dashboard.RootElement, "doNotStartAutomatically"),
            EvidencePath: Goal118String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal118String(dashboard?.RootElement, "exportPath"),
            Errors: Goal118Join(dashboard?.RootElement, "errors"),
            Warnings: Goal118Join(dashboard?.RootElement, "warnings"),
            QualityGatePassed: quality is not null && TryGetBool(quality.RootElement, "passed"),
            RelativePaths: Goal118AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal118FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal118 baseline file exists" : "Goal118 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; noRawManualResult=true"
        };
    }

    private static bool Goal118AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal118String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string Goal118Join(JsonElement? element, string propertyName)
    {
        if (element is null
            || !element.Value.TryGetProperty(propertyName, out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        return string.Join(
            " | ",
            array.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString() ?? string.Empty)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Take(16));
    }

    private static int Goal118Int(JsonElement? element, string propertyName) =>
        element is not null && TryGetInt(element.Value, propertyName, out var value) ? value : 0;

    private sealed record OfflineGeoworldAcceptedAlphaBaselineWorkspaceSummary(
        string BaselineId,
        string BaselineHash,
        bool AcceptedBaselineReady,
        string ManualGateStatus,
        string RecommendedNextDecision,
        int IncludedSourceGoalCount,
        int AcceptedEvidenceRootCount,
        int ProducedOnlyRootCount,
        int BlockedOrSupersededNoteCount,
        bool DoNotStartAutomatically,
        string EvidencePath,
        string ExportPath,
        string Errors,
        string Warnings,
        bool QualityGatePassed,
        bool RelativePaths);
}
