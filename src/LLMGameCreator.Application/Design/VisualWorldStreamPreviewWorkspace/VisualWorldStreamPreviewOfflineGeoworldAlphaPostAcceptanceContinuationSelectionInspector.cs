using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup
        BuildOfflineGeoworldAlphaPostAcceptanceContinuationSelectionGroup(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldAlphaPostAcceptanceContinuationSummary(
            projectRoot,
            groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                    .ProceduralOutputDirectory,
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId,
                BuildGoal117ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldAlphaPostAcceptanceContinuationSummary(
                entry,
                summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                     .RequiredExportFileNames)
        {
            entries.Add(WithOfflineGeoworldAlphaPostAcceptanceContinuationSummary(
                Goal117FileEntry(
                    projectRoot,
                    OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                        .ExportPackageDirectory
                    + "/"
                    + fileName,
                    "offline_geoworld_alpha_post_acceptance_continuation_export_file"),
                summary));
        }

        entries.Add(WithOfflineGeoworldAlphaPostAcceptanceContinuationSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId
                     + ".summary",
                RelativePath = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                                   .ProceduralOutputDirectory
                               + "/"
                               + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                                   .DashboardFileName,
                ArtifactKind =
                    "offline_geoworld_alpha_post_acceptance_continuation_workspace_summary",
                SourceGoalId =
                    OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                        .ProceduralOutputDirectory
                    + "/"
                    + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                        .DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "recommendedNextLane=" + summary.RecommendedNextLane
                                    + "; recommendedNextGoalId="
                                    + summary.RecommendedNextGoalId,
                SafeRatingMetadataSummary =
                    "doNotStartAutomatically=true; noRawManualResult=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_alpha_post_acceptance_continuation_selection",
            "Goal 117 Offline Geoworld Alpha Post-Acceptance Continuation Selection",
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal117ProceduralFiles() =>
    [
        (OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.DashboardFileName,
            "offline_geoworld_alpha_post_acceptance_continuation_dashboard"),
        (OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.MatrixFileName,
            "offline_geoworld_alpha_post_acceptance_continuation_matrix"),
        (OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.ReportFileName,
            "offline_geoworld_alpha_post_acceptance_continuation_report"),
        (OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.FileIndexFileName,
            "offline_geoworld_alpha_post_acceptance_continuation_file_index"),
        (OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.QualityGateScanFileName,
            "offline_geoworld_alpha_post_acceptance_continuation_quality_gate"),
        (OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.NegativeProofFileName,
            "offline_geoworld_alpha_post_acceptance_continuation_negative_proof")
    ];

    private static VisualWorldPreviewArtifactEntry
        WithOfflineGeoworldAlphaPostAcceptanceContinuationSummary(
            VisualWorldPreviewArtifactEntry entry,
            OfflineGeoworldAlphaPostAcceptanceContinuationWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldAlphaPostAcceptanceManualGateStatus =
                summary.ManualGateStatus,
            OfflineGeoworldAlphaPostAcceptanceHumanAccepted = summary.HumanAccepted,
            OfflineGeoworldAlphaPostAcceptanceManualResultSha256 =
                summary.ManualResultSha256,
            OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane =
                summary.RecommendedNextLane,
            OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId =
                summary.RecommendedNextGoalId,
            OfflineGeoworldAlphaPostAcceptanceReadyLaneCount =
                summary.ReadyLaneCount,
            OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount =
                summary.CandidateLaneCount,
            OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount =
                summary.BlockedLaneCount,
            OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically =
                summary.DoNotStartAutomatically,
            OfflineGeoworldAlphaPostAcceptanceEvidencePath = summary.EvidencePath,
            OfflineGeoworldAlphaPostAcceptanceExportPath = summary.ExportPath,
            OfflineGeoworldAlphaPostAcceptanceLaneIds = summary.LaneIds,
            OfflineGeoworldAlphaPostAcceptanceErrors = summary.Errors,
            OfflineGeoworldAlphaPostAcceptanceWarnings = summary.Warnings,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldAlphaPostAcceptanceContinuationWorkspaceSummary
        LoadOfflineGeoworldAlphaPostAcceptanceContinuationSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
            .ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .DashboardFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            root + "/" + OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .QualityGateScanFileName,
            diagnostics);
        var summary = new OfflineGeoworldAlphaPostAcceptanceContinuationWorkspaceSummary(
            ManualGateStatus: Goal117String(dashboard?.RootElement, "manualGateStatus"),
            HumanAccepted: dashboard is not null && TryGetBool(dashboard.RootElement, "humanAccepted"),
            ManualResultSha256: Goal117String(dashboard?.RootElement, "manualResultSha256"),
            RecommendedNextLane: Goal117String(dashboard?.RootElement, "recommendedNextLane"),
            RecommendedNextGoalId: Goal117String(dashboard?.RootElement, "recommendedNextGoalId"),
            ReadyLaneCount: Goal117Int(dashboard?.RootElement, "readyLaneCount"),
            CandidateLaneCount: Goal117Int(dashboard?.RootElement, "candidateLaneCount"),
            BlockedLaneCount: Goal117Int(dashboard?.RootElement, "blockedLaneCount"),
            DoNotStartAutomatically:
                dashboard is not null && TryGetBool(dashboard.RootElement, "doNotStartAutomatically"),
            EvidencePath: Goal117String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal117String(dashboard?.RootElement, "exportPath"),
            LaneIds: Goal117Join(dashboard?.RootElement, "laneIds"),
            Errors: Goal117Join(dashboard?.RootElement, "errors"),
            Warnings: Goal117Join(dashboard?.RootElement, "warnings"),
            QualityGatePassed: quality is not null && TryGetBool(quality.RootElement, "passed"),
            RelativePaths: Goal117AllPathsRelative(projectRoot));
        return summary;
    }

    private static VisualWorldPreviewArtifactEntry Goal117FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId =
                OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal117 continuation file exists" : "Goal117 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; noRawManualResult=true"
        };
    }

    private static bool Goal117AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ProceduralOutputDirectory,
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal117String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string Goal117Join(JsonElement? element, string propertyName)
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

    private static int Goal117Int(JsonElement? element, string propertyName) =>
        element is not null && TryGetInt(element.Value, propertyName, out var value) ? value : 0;

    private sealed record OfflineGeoworldAlphaPostAcceptanceContinuationWorkspaceSummary(
        string ManualGateStatus,
        bool HumanAccepted,
        string ManualResultSha256,
        string RecommendedNextLane,
        string RecommendedNextGoalId,
        int ReadyLaneCount,
        int CandidateLaneCount,
        int BlockedLaneCount,
        bool DoNotStartAutomatically,
        string EvidencePath,
        string ExportPath,
        string LaneIds,
        string Errors,
        string Warnings,
        bool QualityGatePassed,
        bool RelativePaths);
}
