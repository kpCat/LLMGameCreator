using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldAlphaManualResultWorkbenchGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldAlphaManualResultWorkbenchSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory,
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId,
                BuildGoal113ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldAlphaManualResultWorkbenchSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldAlphaManualResultWorkbenchVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithOfflineGeoworldAlphaManualResultWorkbenchSummary(
                Goal113FileEntry(
                    projectRoot,
                    OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "offline_geoworld_alpha_manual_result_workbench_export_file"),
                summary));
        }

        entries.Add(WithOfflineGeoworldAlphaManualResultWorkbenchSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId + ".summary",
                RelativePath = OfflineGeoworldAlphaManualResultWorkbenchVocabulary
                    .ProceduralOutputDirectory
                               + "/"
                               + OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DashboardFileName,
                ArtifactKind = "offline_geoworld_alpha_manual_result_workbench_workspace_summary",
                SourceGoalId = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory
                    + "/"
                    + OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "workbenchStatus=" + summary.WorkbenchStatus
                                    + "; manualResultPresent="
                                    + summary.ManualResultPresent.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "acceptedByCodex=false; humanAcceptanceStillRequired=true; draftTemplateOnly=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_alpha_manual_result_workbench",
            "Goal 113 Offline Geoworld Alpha Manual Result Workbench",
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId,
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal113ProceduralFiles() =>
    [
        (OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DashboardFileName,
            "offline_geoworld_alpha_manual_result_workbench_dashboard"),
        (OfflineGeoworldAlphaManualResultWorkbenchVocabulary.FileIndexFileName,
            "offline_geoworld_alpha_manual_result_workbench_file_index"),
        (OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ReportFileName,
            "offline_geoworld_alpha_manual_result_workbench_report"),
        (OfflineGeoworldAlphaManualResultWorkbenchVocabulary.RunbookFileName,
            "offline_geoworld_alpha_manual_result_workbench_runbook"),
        (OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DraftTemplateFileName,
            "offline_geoworld_alpha_manual_result_workbench_draft_template"),
        (OfflineGeoworldAlphaManualResultWorkbenchVocabulary.FieldMapFileName,
            "offline_geoworld_alpha_manual_result_workbench_field_map"),
        (OfflineGeoworldAlphaManualResultWorkbenchVocabulary.QualityGateScanFileName,
            "offline_geoworld_alpha_manual_result_workbench_quality_gate"),
        (OfflineGeoworldAlphaManualResultWorkbenchVocabulary.NegativeNoResultFileName,
            "offline_geoworld_alpha_manual_result_workbench_negative_no_result"),
        (OfflineGeoworldAlphaManualResultWorkbenchVocabulary.NegativeInvalidResultFileName,
            "offline_geoworld_alpha_manual_result_workbench_negative_invalid_result")
    ];

    private static VisualWorldPreviewArtifactEntry
        WithOfflineGeoworldAlphaManualResultWorkbenchSummary(
            VisualWorldPreviewArtifactEntry entry,
            OfflineGeoworldAlphaManualResultWorkbenchWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldAlphaManualResultWorkbenchStatus = summary.WorkbenchStatus,
            OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus =
                summary.Goal111DecisionStatus,
            OfflineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus =
                summary.Goal112OperatorStatus,
            OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent =
                summary.ManualResultPresent,
            OfflineGeoworldAlphaManualResultWorkbenchPreferredManualResultPath =
                summary.PreferredManualResultPath,
            OfflineGeoworldAlphaManualResultWorkbenchDraftTemplatePath =
                summary.DraftTemplatePath,
            OfflineGeoworldAlphaManualResultWorkbenchCandidateManualResultPaths =
                summary.CandidateManualResultPaths,
            OfflineGeoworldAlphaManualResultWorkbenchChecklistStepCount =
                summary.ChecklistStepCount,
            OfflineGeoworldAlphaManualResultWorkbenchChecklistHash = summary.ChecklistHash,
            OfflineGeoworldAlphaManualResultWorkbenchRequiredStepsSummary =
                summary.RequiredStepsSummary,
            OfflineGeoworldAlphaManualResultWorkbenchValidationErrors =
                summary.ValidationErrors,
            OfflineGeoworldAlphaManualResultWorkbenchValidationWarnings =
                summary.ValidationWarnings,
            OfflineGeoworldAlphaManualResultWorkbenchNextHumanActions =
                summary.NextHumanActions,
            OfflineGeoworldAlphaManualResultWorkbenchDoNotStartYet =
                summary.DoNotStartYet,
            OfflineGeoworldAlphaManualResultWorkbenchProceduralPath =
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaManualResultWorkbenchExportPath =
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ExportPackageDirectory,
            OfflineGeoworldAlphaManualResultWorkbenchRunbookPath =
                OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory
                + "/"
                + OfflineGeoworldAlphaManualResultWorkbenchVocabulary.RunbookFileName,
            OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex = false,
            OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired = true,
            OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly = true,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldAlphaManualResultWorkbenchWorkspaceSummary
        LoadOfflineGeoworldAlphaManualResultWorkbenchSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaManualResultWorkbenchVocabulary.DashboardFileName, diagnostics);
        using var quality = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaManualResultWorkbenchVocabulary.QualityGateScanFileName, diagnostics);
        var summary = new OfflineGeoworldAlphaManualResultWorkbenchWorkspaceSummary(
            WorkbenchStatus: Goal113String(dashboard?.RootElement, "workbenchStatus"),
            Goal111DecisionStatus: Goal113String(dashboard?.RootElement, "goal111DecisionStatus"),
            Goal112OperatorStatus: Goal113String(dashboard?.RootElement, "goal112OperatorStatus"),
            ManualResultPresent: dashboard is not null
                                 && TryGetBool(dashboard.RootElement, "manualResultPresent"),
            PreferredManualResultPath: Goal113String(dashboard?.RootElement, "preferredManualResultPath"),
            DraftTemplatePath: Goal113String(dashboard?.RootElement, "draftTemplatePath"),
            CandidateManualResultPaths: Goal113Join(dashboard?.RootElement, "candidateManualResultPaths"),
            ChecklistStepCount: Goal113Int(dashboard?.RootElement, "checklistStepCount"),
            ChecklistHash: Goal113String(dashboard?.RootElement, "checklistHash"),
            RequiredStepsSummary: Goal113StepSummary(dashboard?.RootElement),
            ValidationErrors: Goal113Join(dashboard?.RootElement, "errors"),
            ValidationWarnings: Goal113Join(dashboard?.RootElement, "warnings"),
            NextHumanActions: Goal113Join(dashboard?.RootElement, "nextHumanActions"),
            DoNotStartYet: Goal113Join(dashboard?.RootElement, "doNotStartYet"),
            QualityGatePassed: quality is not null && TryGetBool(quality.RootElement, "passed"),
            RelativePaths: Goal113AllPathsRelative(projectRoot));
        AddIfFalse(summary.QualityGatePassed, "goal113.workspace.summary_failed",
            "offline_geoworld_alpha_manual_result_workbench", diagnostics);
        return summary;
    }

    private static VisualWorldPreviewArtifactEntry Goal113FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = OfflineGeoworldAlphaManualResultWorkbenchVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal113 workbench file exists" : "Goal113 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; humanAcceptanceStillRequired=true"
        };
    }

    private static bool Goal113AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(IsSafeRelativePath));
    }

    private static string Goal113String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string Goal113Join(JsonElement? element, string propertyName)
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

    private static string Goal113StepSummary(JsonElement? element)
    {
        if (element is null
            || !element.Value.TryGetProperty("requiredSteps", out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        return string.Join(
            " | ",
            array.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Object)
                .Select(item => Goal113String(item, "stepId") + ": " + Goal113String(item, "title"))
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Take(12));
    }

    private static int Goal113Int(JsonElement? element, string propertyName) =>
        element is not null && TryGetInt(element.Value, propertyName, out var value) ? value : 0;

    private sealed record OfflineGeoworldAlphaManualResultWorkbenchWorkspaceSummary(
        string WorkbenchStatus,
        string Goal111DecisionStatus,
        string Goal112OperatorStatus,
        bool ManualResultPresent,
        string PreferredManualResultPath,
        string DraftTemplatePath,
        string CandidateManualResultPaths,
        int ChecklistStepCount,
        string ChecklistHash,
        string RequiredStepsSummary,
        string ValidationErrors,
        string ValidationWarnings,
        string NextHumanActions,
        string DoNotStartYet,
        bool QualityGatePassed,
        bool RelativePaths);
}
