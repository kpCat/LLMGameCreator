using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldAlphaAcceptanceOperatorPackGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldAlphaAcceptanceOperatorPackSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory,
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId,
                BuildGoal112ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldAlphaAcceptanceOperatorSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithOfflineGeoworldAlphaAcceptanceOperatorSummary(
                Goal112FileEntry(
                    projectRoot,
                    OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "offline_geoworld_alpha_acceptance_operator_export_file"),
                summary));
        }

        entries.Add(WithOfflineGeoworldAlphaAcceptanceOperatorSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId + ".summary",
                RelativePath = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory
                               + "/"
                               + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.DashboardFileName,
                ArtifactKind = "offline_geoworld_alpha_acceptance_operator_workspace_summary",
                SourceGoalId = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory
                    + "/"
                    + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "operatorStatus=" + summary.OperatorStatus
                                    + "; goal111DecisionStatus=" + summary.Goal111DecisionStatus,
                SafeRatingMetadataSummary =
                    "acceptedByCodex=false; humanAcceptanceStillRequired=true; notFinalRelease=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_alpha_acceptance_operator_pack",
            "Goal 112 Offline Geoworld Alpha Acceptance Operator Pack",
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId,
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal112ProceduralFiles() =>
    [
        (OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.DashboardFileName,
            "offline_geoworld_alpha_acceptance_operator_dashboard"),
        (OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RunbookFileName,
            "offline_geoworld_alpha_acceptance_operator_runbook"),
        (OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ResultPathMapFileName,
            "offline_geoworld_alpha_acceptance_operator_path_map"),
        (OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PreflightReportFileName,
            "offline_geoworld_alpha_acceptance_operator_preflight"),
        (OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.NotaryBoundaryFileName,
            "offline_geoworld_alpha_acceptance_operator_notary_boundary"),
        (OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.QualityGateScanFileName,
            "offline_geoworld_alpha_acceptance_operator_quality_gate"),
        (OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.NegativeProofFileName,
            "offline_geoworld_alpha_acceptance_operator_negative_proof"),
        (OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.PendingResultTemplateCopyFileName,
            "offline_geoworld_alpha_acceptance_operator_pending_template_copy"),
        (OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.FileIndexFileName,
            "offline_geoworld_alpha_acceptance_operator_file_index")
    ];

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldAlphaAcceptanceOperatorSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldAlphaAcceptanceOperatorWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldAlphaAcceptanceOperatorStatus = summary.OperatorStatus,
            OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus =
                summary.Goal111DecisionStatus,
            OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent =
                summary.ManualResultPresent,
            OfflineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview =
                summary.ManualResultAvailableForHumanReview,
            OfflineGeoworldAlphaAcceptanceOperatorPreferredManualResultPath =
                summary.PreferredManualResultPath,
            OfflineGeoworldAlphaAcceptanceOperatorCandidateManualResultPaths =
                summary.CandidateManualResultPaths,
            OfflineGeoworldAlphaAcceptanceOperatorChecklistStepCount =
                summary.ChecklistStepCount,
            OfflineGeoworldAlphaAcceptanceOperatorChecklistHashPresent =
                summary.ChecklistHashPresent,
            OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex = false,
            OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired = true,
            OfflineGeoworldAlphaAcceptanceOperatorNextHumanActions = summary.NextHumanActions,
            OfflineGeoworldAlphaAcceptanceOperatorDoNotStartYet = summary.DoNotStartYet,
            OfflineGeoworldAlphaAcceptanceOperatorEvidencePath =
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaAcceptanceOperatorExportPath =
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ExportPackageDirectory,
            OfflineGeoworldAlphaAcceptanceOperatorRunbookPath =
                OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory
                + "/"
                + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.RunbookFileName,
            OfflineGeoworldAlphaAcceptanceOperatorTopErrors = summary.TopErrors,
            OfflineGeoworldAlphaAcceptanceOperatorTopWarnings = summary.TopWarnings,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldAlphaAcceptanceOperatorWorkspaceSummary
        LoadOfflineGeoworldAlphaAcceptanceOperatorPackSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.DashboardFileName, diagnostics);
        using var quality = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.QualityGateScanFileName, diagnostics);
        var checklistHash = Goal112String(dashboard?.RootElement, "checklistHash");
        var summary = new OfflineGeoworldAlphaAcceptanceOperatorWorkspaceSummary(
            OperatorStatus: Goal112String(dashboard?.RootElement, "operatorStatus"),
            Goal111DecisionStatus: Goal112String(dashboard?.RootElement, "decisionStatusFromGoal111"),
            ManualResultPresent: dashboard is not null
                                 && TryGetBool(dashboard.RootElement, "manualResultPresent"),
            ManualResultAvailableForHumanReview: dashboard is not null
                                                 && TryGetBool(
                                                     dashboard.RootElement,
                                                     "manualResultAvailableForHumanReview"),
            PreferredManualResultPath: Goal112String(dashboard?.RootElement, "preferredManualResultPath"),
            CandidateManualResultPaths: Goal112Join(dashboard?.RootElement, "candidateManualResultPaths"),
            ChecklistStepCount: Goal112Int(dashboard?.RootElement, "checklistStepCount"),
            ChecklistHashPresent: !string.IsNullOrWhiteSpace(checklistHash),
            NextHumanActions: Goal112Join(dashboard?.RootElement, "nextHumanActions"),
            DoNotStartYet: Goal112Join(dashboard?.RootElement, "doNotDoYet"),
            TopErrors: Goal112Join(dashboard?.RootElement, "errors"),
            TopWarnings: Goal112Join(dashboard?.RootElement, "warnings"),
            QualityGatePassed: quality is not null && TryGetBool(quality.RootElement, "passed"),
            RelativePaths: Goal112AllPathsRelative(projectRoot));
        AddIfFalse(summary.QualityGatePassed, "goal112.workspace.summary_failed",
            "offline_geoworld_alpha_acceptance_operator_pack", diagnostics);
        return summary;
    }

    private static VisualWorldPreviewArtifactEntry Goal112FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal112 operator file exists" : "Goal112 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; humanAcceptanceStillRequired=true"
        };
    }

    private static bool Goal112AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(IsSafeRelativePath));
    }

    private static string Goal112String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string Goal112Join(JsonElement? element, string propertyName)
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
                .Take(10));
    }

    private static int Goal112Int(JsonElement? element, string propertyName) =>
        element is not null && TryGetInt(element.Value, propertyName, out var value) ? value : 0;

    private sealed record OfflineGeoworldAlphaAcceptanceOperatorWorkspaceSummary(
        string OperatorStatus,
        string Goal111DecisionStatus,
        bool ManualResultPresent,
        bool ManualResultAvailableForHumanReview,
        string PreferredManualResultPath,
        string CandidateManualResultPaths,
        int ChecklistStepCount,
        bool ChecklistHashPresent,
        string NextHumanActions,
        string DoNotStartYet,
        string TopErrors,
        string TopWarnings,
        bool QualityGatePassed,
        bool RelativePaths);
}
