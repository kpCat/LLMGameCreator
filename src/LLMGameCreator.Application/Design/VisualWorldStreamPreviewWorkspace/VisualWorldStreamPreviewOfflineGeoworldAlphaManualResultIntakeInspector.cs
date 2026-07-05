using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldAlphaManualResultIntakeGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldAlphaManualResultIntakeSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory,
                OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId,
                BuildGoal111ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldAlphaManualResultIntakeSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldAlphaManualResultIntakeVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithOfflineGeoworldAlphaManualResultIntakeSummary(
                Goal111FileEntry(
                    projectRoot,
                    OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "offline_geoworld_alpha_manual_result_intake_export_file"),
                summary));
        }

        entries.Add(WithOfflineGeoworldAlphaManualResultIntakeSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId + ".summary",
                RelativePath = OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory
                               + "/"
                               + OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName,
                ArtifactKind = "offline_geoworld_alpha_manual_result_intake_workspace_summary",
                SourceGoalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory
                    + "/"
                    + OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "decisionStatus=" + summary.DecisionStatus
                                    + "; acceptableCandidate=" + summary.AcceptableCandidate,
                SafeRatingMetadataSummary =
                    "acceptedByCodex=false; humanAcceptanceStillRequired=true; notFinalRelease=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_alpha_manual_result_intake",
            "Goal 111 Offline Geoworld Alpha Manual Result Intake",
            OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId,
            OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal111ProceduralFiles() =>
    [
        (OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName,
            "offline_geoworld_alpha_manual_result_intake_decision"),
        (OfflineGeoworldAlphaManualResultIntakeVocabulary.ReportFileName,
            "offline_geoworld_alpha_manual_result_intake_report"),
        (OfflineGeoworldAlphaManualResultIntakeVocabulary.FileIndexFileName,
            "offline_geoworld_alpha_manual_result_intake_file_index"),
        (OfflineGeoworldAlphaManualResultIntakeVocabulary.QualityGateScanFileName,
            "offline_geoworld_alpha_manual_result_intake_quality_gate"),
        (OfflineGeoworldAlphaManualResultIntakeVocabulary.MissingResultProofFileName,
            "offline_geoworld_alpha_manual_result_intake_missing_result_negative_proof"),
        (OfflineGeoworldAlphaManualResultIntakeVocabulary.InvalidResultProofFileName,
            "offline_geoworld_alpha_manual_result_intake_invalid_result_negative_proof"),
        (OfflineGeoworldAlphaManualResultIntakeVocabulary.ValidSampleResultFileName,
            "offline_geoworld_alpha_manual_result_intake_valid_sample_fixture")
    ];

    private static VisualWorldPreviewArtifactEntry WithOfflineGeoworldAlphaManualResultIntakeSummary(
        VisualWorldPreviewArtifactEntry entry,
        OfflineGeoworldAlphaManualResultIntakeWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent =
                summary.Goal110PackagePresent,
            OfflineGeoworldAlphaManualResultIntakeResultFilePresent =
                summary.ResultFilePresent,
            OfflineGeoworldAlphaManualResultIntakeDecisionStatus = summary.DecisionStatus,
            OfflineGeoworldAlphaManualResultIntakeAcceptableCandidate =
                summary.AcceptableCandidate,
            OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex = false,
            OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired = true,
            OfflineGeoworldAlphaManualResultIntakeChecklistHashMatched =
                summary.ChecklistHashMatched,
            OfflineGeoworldAlphaManualResultIntakePassedStepCount = summary.PassedStepCount,
            OfflineGeoworldAlphaManualResultIntakeFailedStepCount = summary.FailedStepCount,
            OfflineGeoworldAlphaManualResultIntakePendingStepCount = summary.PendingStepCount,
            OfflineGeoworldAlphaManualResultIntakeSkippedStepCount = summary.SkippedStepCount,
            OfflineGeoworldAlphaManualResultIntakeMissingStepCount = summary.MissingStepCount,
            OfflineGeoworldAlphaManualResultIntakeDuplicateStepCount = summary.DuplicateStepCount,
            OfflineGeoworldAlphaManualResultIntakeTopErrors = summary.TopErrors,
            OfflineGeoworldAlphaManualResultIntakeTopWarnings = summary.TopWarnings,
            OfflineGeoworldAlphaManualResultIntakeDecisionPath =
                OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory
                + "/"
                + OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName,
            OfflineGeoworldAlphaManualResultIntakeExportPath =
                OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportPackageDirectory
                + "/"
                + OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportDashboardFileName,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldAlphaManualResultIntakeWorkspaceSummary
        LoadOfflineGeoworldAlphaManualResultIntakeSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory;
        using var decision = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName, diagnostics);
        using var quality = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaManualResultIntakeVocabulary.QualityGateScanFileName, diagnostics);
        var lineage = ChildObject(decision?.RootElement, "inputPackageLineage");
        var stepSummary = ChildObject(decision?.RootElement, "stepSummary");
        var expected = DecisionString(decision?.RootElement, "checklistHashExpected");
        var actual = DecisionString(decision?.RootElement, "checklistHashActual");
        var summary = new OfflineGeoworldAlphaManualResultIntakeWorkspaceSummary(
            Goal110PackagePresent: quality is not null
                                   && TryGetBool(quality.RootElement, "goal110PackagePresent"),
            ResultFilePresent: decision is not null
                               && TryGetBool(decision.RootElement, "resultFilePresent"),
            DecisionStatus: DecisionString(decision?.RootElement, "decisionStatus"),
            AcceptableCandidate: decision is not null
                                 && TryGetBool(decision.RootElement, "acceptableCandidate"),
            ChecklistHashMatched: !string.IsNullOrWhiteSpace(expected)
                                  && string.Equals(expected, actual, StringComparison.Ordinal),
            PassedStepCount: Goal111Int(stepSummary, "passedCount"),
            FailedStepCount: Goal111Int(stepSummary, "failedCount"),
            PendingStepCount: Goal111Int(stepSummary, "pendingCount"),
            SkippedStepCount: Goal111Int(stepSummary, "skippedCount"),
            MissingStepCount: Goal111Int(stepSummary, "missingCount"),
            DuplicateStepCount: Goal111Int(stepSummary, "duplicateCount"),
            TopErrors: JoinStringArray(decision?.RootElement, "errors"),
            TopWarnings: JoinStringArray(decision?.RootElement, "warnings"),
            QualityGatePassed: quality is not null && TryGetBool(quality.RootElement, "passed"),
            RelativePaths: Goal111AllPathsRelative(projectRoot),
            LoadedMetadataFileCount: lineage is null
                ? 0
                : Goal111Int(lineage.Value, "loadedMetadataFileCount"));
        AddIfFalse(summary.QualityGatePassed, "goal111.workspace.summary_failed",
            "offline_geoworld_alpha_manual_result_intake", diagnostics);
        return summary;
    }

    private static VisualWorldPreviewArtifactEntry Goal111FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal111 manual result intake file exists" : "Goal111 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; humanAcceptanceStillRequired=true"
        };
    }

    private static bool Goal111AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(IsSafeRelativePath));
    }

    private static string DecisionString(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static JsonElement? ChildObject(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Object
            ? property
            : null;

    private static string JoinStringArray(JsonElement? element, string propertyName)
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
                .Take(3));
    }

    private static int Goal111Int(JsonElement? element, string propertyName) =>
        element is not null && TryGetInt(element.Value, propertyName, out var value) ? value : 0;

    private sealed record OfflineGeoworldAlphaManualResultIntakeWorkspaceSummary(
        bool Goal110PackagePresent,
        bool ResultFilePresent,
        string DecisionStatus,
        bool AcceptableCandidate,
        bool ChecklistHashMatched,
        int PassedStepCount,
        int FailedStepCount,
        int PendingStepCount,
        int SkippedStepCount,
        int MissingStepCount,
        int DuplicateStepCount,
        string TopErrors,
        string TopWarnings,
        bool QualityGatePassed,
        bool RelativePaths,
        int LoadedMetadataFileCount);
}
