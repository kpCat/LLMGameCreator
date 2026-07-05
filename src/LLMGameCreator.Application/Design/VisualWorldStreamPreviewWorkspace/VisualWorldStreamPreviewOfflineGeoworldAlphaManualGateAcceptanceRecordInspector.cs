using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup
        BuildOfflineGeoworldAlphaManualGateAcceptanceRecordGroup(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldAlphaManualGateAcceptanceRecordSummary(
            projectRoot,
            groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory,
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId,
                BuildGoal116ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldAlphaManualGateAcceptanceRecordSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                     .RequiredExportFileNames)
        {
            entries.Add(WithOfflineGeoworldAlphaManualGateAcceptanceRecordSummary(
                Goal116FileEntry(
                    projectRoot,
                    OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "offline_geoworld_alpha_manual_gate_acceptance_record_export_file"),
                summary));
        }

        entries.Add(WithOfflineGeoworldAlphaManualGateAcceptanceRecordSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId + ".summary",
                RelativePath = OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                    .ProceduralOutputDirectory
                               + "/"
                               + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.DashboardFileName,
                ArtifactKind = "offline_geoworld_alpha_manual_gate_acceptance_record_workspace_summary",
                SourceGoalId = OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory
                    + "/"
                    + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "manualGateStatus=" + summary.ManualGateStatus
                                    + "; humanAccepted="
                                    + summary.HumanAccepted.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "acceptedByCodex=false; manualInputNotCommitted=true; rawManualResultEmbedded=false"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            "Goal 116 Offline Geoworld Alpha Manual Gate Acceptance Record",
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal116ProceduralFiles() =>
    [
        (OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.AcceptanceRecordFileName,
            "offline_geoworld_alpha_manual_gate_acceptance_record"),
        (OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.DashboardFileName,
            "offline_geoworld_alpha_manual_gate_acceptance_dashboard"),
        (OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ReportFileName,
            "offline_geoworld_alpha_manual_gate_acceptance_report"),
        (OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.FileIndexFileName,
            "offline_geoworld_alpha_manual_gate_acceptance_file_index"),
        (OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.QualityGateScanFileName,
            "offline_geoworld_alpha_manual_gate_acceptance_quality_gate"),
        (OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.NegativeProofFileName,
            "offline_geoworld_alpha_manual_gate_acceptance_negative_proof")
    ];

    private static VisualWorldPreviewArtifactEntry
        WithOfflineGeoworldAlphaManualGateAcceptanceRecordSummary(
            VisualWorldPreviewArtifactEntry entry,
            OfflineGeoworldAlphaManualGateAcceptanceRecordWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldAlphaManualGateAcceptanceManualGate = summary.ManualGate,
            OfflineGeoworldAlphaManualGateAcceptanceManualGateStatus =
                summary.ManualGateStatus,
            OfflineGeoworldAlphaManualGateAcceptanceHumanAccepted = summary.HumanAccepted,
            OfflineGeoworldAlphaManualGateAcceptanceHumanDecisionStatement =
                summary.HumanDecisionStatement,
            OfflineGeoworldAlphaManualGateAcceptanceSourceDecisionStatus =
                summary.SourceDecisionStatus,
            OfflineGeoworldAlphaManualGateAcceptanceManualResultSha256 =
                summary.ManualResultSha256,
            OfflineGeoworldAlphaManualGateAcceptanceAcceptedByCodex = false,
            OfflineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted =
                summary.ManualInputNotCommitted,
            OfflineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts =
                summary.RawManualResultEmbeddedInArtifacts,
            OfflineGeoworldAlphaManualGateAcceptanceRecommendedNextDecision =
                summary.RecommendedNextDecision,
            OfflineGeoworldAlphaManualGateAcceptanceNotFinalReleaseOrRuntimeBuild =
                summary.NotFinalReleaseOrRuntimeBuild,
            OfflineGeoworldAlphaManualGateAcceptanceNoRuntimeProviderOrNetworkChanges =
                summary.NoRuntimeProviderOrNetworkChanges,
            OfflineGeoworldAlphaManualGateAcceptanceNoUnityFileChangesRequired =
                summary.NoUnityFileChangesRequired,
            OfflineGeoworldAlphaManualGateAcceptanceRequiredStepCount =
                summary.RequiredStepCount,
            OfflineGeoworldAlphaManualGateAcceptancePassedStepCount =
                summary.PassedStepCount,
            OfflineGeoworldAlphaManualGateAcceptanceProceduralPath =
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaManualGateAcceptanceExportPath =
                OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExportPackageDirectory,
            OfflineGeoworldAlphaManualGateAcceptanceErrors = summary.Errors,
            OfflineGeoworldAlphaManualGateAcceptanceWarnings = summary.Warnings,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldAlphaManualGateAcceptanceRecordWorkspaceSummary
        LoadOfflineGeoworldAlphaManualGateAcceptanceRecordSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory;
        using var record = TryReadJson(
            projectRoot,
            root + "/" + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                .AcceptanceRecordFileName,
            diagnostics);
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.DashboardFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            root + "/" + OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                .QualityGateScanFileName,
            diagnostics);
        var summary = new OfflineGeoworldAlphaManualGateAcceptanceRecordWorkspaceSummary(
            ManualGate: Goal116String(record?.RootElement, "manualGate"),
            ManualGateStatus: Goal116String(record?.RootElement, "manualGateStatus"),
            HumanAccepted: record is not null && TryGetBool(record.RootElement, "humanAccepted"),
            HumanDecisionStatement: Goal116String(record?.RootElement, "humanDecisionStatement"),
            SourceDecisionStatus: Goal116String(record?.RootElement, "sourceDecisionStatus"),
            ManualResultSha256: Goal116String(record?.RootElement, "manualResultSha256"),
            ManualInputNotCommitted:
                record is not null && TryGetBool(record.RootElement, "manualInputNotCommitted"),
            RawManualResultEmbeddedInArtifacts:
                record is not null
                && TryGetBool(record.RootElement, "rawManualResultEmbeddedInArtifacts"),
            RecommendedNextDecision: Goal116String(record?.RootElement, "recommendedNextDecision"),
            NotFinalReleaseOrRuntimeBuild:
                record is not null && TryGetBool(record.RootElement, "notFinalReleaseOrRuntimeBuild"),
            NoRuntimeProviderOrNetworkChanges:
                record is not null
                && TryGetBool(record.RootElement, "noRuntimeProviderOrNetworkChanges"),
            NoUnityFileChangesRequired:
                record is not null && TryGetBool(record.RootElement, "noUnityFileChangesRequired"),
            RequiredStepCount: Goal116Int(dashboard?.RootElement, "requiredStepCount"),
            PassedStepCount: Goal116Int(dashboard?.RootElement, "passedStepCount"),
            Errors: Goal116Join(dashboard?.RootElement, "errors"),
            Warnings: Goal116Join(dashboard?.RootElement, "warnings"),
            QualityGatePassed: quality is not null && TryGetBool(quality.RootElement, "passed"),
            RelativePaths: Goal116AllPathsRelative(projectRoot));
        AddIfFalse(summary.QualityGatePassed, "goal116.workspace.summary_failed",
            "offline_geoworld_alpha_manual_gate_acceptance_record", diagnostics);
        return summary;
    }

    private static VisualWorldPreviewArtifactEntry Goal116FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal116 acceptance file exists" : "Goal116 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; noRawManualResult=true"
        };
    }

    private static bool Goal116AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal116String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string Goal116Join(JsonElement? element, string propertyName)
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
                .Take(12));
    }

    private static int Goal116Int(JsonElement? element, string propertyName) =>
        element is not null && TryGetInt(element.Value, propertyName, out var value) ? value : 0;

    private sealed record OfflineGeoworldAlphaManualGateAcceptanceRecordWorkspaceSummary(
        string ManualGate,
        string ManualGateStatus,
        bool HumanAccepted,
        string HumanDecisionStatement,
        string SourceDecisionStatus,
        string ManualResultSha256,
        bool ManualInputNotCommitted,
        bool RawManualResultEmbeddedInArtifacts,
        string RecommendedNextDecision,
        bool NotFinalReleaseOrRuntimeBuild,
        bool NoRuntimeProviderOrNetworkChanges,
        bool NoUnityFileChangesRequired,
        int RequiredStepCount,
        int PassedStepCount,
        string Errors,
        string Warnings,
        bool QualityGatePassed,
        bool RelativePaths);
}
