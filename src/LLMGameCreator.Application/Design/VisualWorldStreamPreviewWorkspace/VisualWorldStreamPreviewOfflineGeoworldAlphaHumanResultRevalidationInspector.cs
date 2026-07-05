using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildOfflineGeoworldAlphaHumanResultRevalidationGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadOfflineGeoworldAlphaHumanResultRevalidationSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory,
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId,
                BuildGoal115ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithOfflineGeoworldAlphaHumanResultRevalidationSummary(entry, summary))
            .ToList();

        foreach (var fileName in OfflineGeoworldAlphaHumanResultRevalidationVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithOfflineGeoworldAlphaHumanResultRevalidationSummary(
                Goal115FileEntry(
                    projectRoot,
                    OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "offline_geoworld_alpha_human_result_revalidation_export_file"),
                summary));
        }

        entries.Add(WithOfflineGeoworldAlphaHumanResultRevalidationSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId + ".summary",
                RelativePath = OfflineGeoworldAlphaHumanResultRevalidationVocabulary
                    .ProceduralOutputDirectory
                               + "/"
                               + OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DashboardFileName,
                ArtifactKind = "offline_geoworld_alpha_human_result_revalidation_workspace_summary",
                SourceGoalId = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory
                    + "/"
                    + OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "decisionStatus=" + summary.DecisionStatus
                                    + "; acceptableCandidate="
                                    + summary.AcceptableCandidate.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "acceptedByCodex=false; humanAcceptanceStillRequired=true; manualGateRemainsHumanDecision=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "offline_geoworld_alpha_human_result_revalidation",
            "Goal 115 Offline Geoworld Alpha Human Result Revalidation",
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId,
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal115ProceduralFiles() =>
    [
        (OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DashboardFileName,
            "offline_geoworld_alpha_human_result_revalidation_dashboard"),
        (OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionSnapshotFileName,
            "offline_geoworld_alpha_human_result_revalidation_decision_snapshot"),
        (OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ReportFileName,
            "offline_geoworld_alpha_human_result_revalidation_report"),
        (OfflineGeoworldAlphaHumanResultRevalidationVocabulary.FileIndexFileName,
            "offline_geoworld_alpha_human_result_revalidation_file_index"),
        (OfflineGeoworldAlphaHumanResultRevalidationVocabulary.QualityGateScanFileName,
            "offline_geoworld_alpha_human_result_revalidation_quality_gate"),
        (OfflineGeoworldAlphaHumanResultRevalidationVocabulary.NegativeProofFileName,
            "offline_geoworld_alpha_human_result_revalidation_negative_proof")
    ];

    private static VisualWorldPreviewArtifactEntry
        WithOfflineGeoworldAlphaHumanResultRevalidationSummary(
            VisualWorldPreviewArtifactEntry entry,
            OfflineGeoworldAlphaHumanResultRevalidationWorkspaceSummary summary) =>
        entry with
        {
            OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus =
                summary.DecisionStatus,
            OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus =
                summary.Goal111DecisionStatus,
            OfflineGeoworldAlphaHumanResultRevalidationManualResultPresent =
                summary.ManualResultPresent,
            OfflineGeoworldAlphaHumanResultRevalidationManualResultJsonValid =
                summary.ManualResultJsonValid,
            OfflineGeoworldAlphaHumanResultRevalidationManualResultPath =
                summary.ManualResultRelativePath,
            OfflineGeoworldAlphaHumanResultRevalidationManualResultSha256 =
                summary.ManualResultSha256,
            OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate =
                summary.AcceptableCandidate,
            OfflineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision =
                summary.RecommendedHumanDecision,
            OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex = false,
            OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired = true,
            OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision = true,
            OfflineGeoworldAlphaHumanResultRevalidationRequiredStepCount =
                summary.RequiredStepCount,
            OfflineGeoworldAlphaHumanResultRevalidationPassedStepCount =
                summary.PassedStepCount,
            OfflineGeoworldAlphaHumanResultRevalidationBlockingStepIssueCount =
                summary.BlockingStepIssueCount,
            OfflineGeoworldAlphaHumanResultRevalidationStepSummary = summary.StepSummary,
            OfflineGeoworldAlphaHumanResultRevalidationErrors = summary.Errors,
            OfflineGeoworldAlphaHumanResultRevalidationWarnings = summary.Warnings,
            OfflineGeoworldAlphaHumanResultRevalidationProceduralPath =
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaHumanResultRevalidationExportPath =
                OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ExportPackageDirectory,
            OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted = true,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static OfflineGeoworldAlphaHumanResultRevalidationWorkspaceSummary
        LoadOfflineGeoworldAlphaHumanResultRevalidationSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DashboardFileName, diagnostics);
        using var snapshot = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaHumanResultRevalidationVocabulary.DecisionSnapshotFileName, diagnostics);
        using var quality = TryReadJson(projectRoot, root + "/"
            + OfflineGeoworldAlphaHumanResultRevalidationVocabulary.QualityGateScanFileName, diagnostics);
        var summary = new OfflineGeoworldAlphaHumanResultRevalidationWorkspaceSummary(
            DecisionStatus: Goal115String(snapshot?.RootElement, "decisionStatus"),
            Goal111DecisionStatus: Goal115String(snapshot?.RootElement, "goal111DecisionStatus"),
            ManualResultPresent: snapshot is not null
                                 && TryGetBool(snapshot.RootElement, "manualResultPresent"),
            ManualResultJsonValid: snapshot is not null
                                   && TryGetBool(snapshot.RootElement, "manualResultJsonValid"),
            ManualResultRelativePath: Goal115String(snapshot?.RootElement, "manualResultRelativePath"),
            ManualResultSha256: Goal115String(snapshot?.RootElement, "manualResultSha256"),
            AcceptableCandidate: snapshot is not null
                                 && TryGetBool(snapshot.RootElement, "acceptableCandidate"),
            RecommendedHumanDecision: Goal115String(snapshot?.RootElement, "recommendedHumanDecision"),
            RequiredStepCount: Goal115Int(dashboard?.RootElement, "requiredStepCount"),
            PassedStepCount: Goal115Int(dashboard?.RootElement, "passedStepCount"),
            BlockingStepIssueCount: Goal115Int(dashboard?.RootElement, "blockingStepIssueCount"),
            StepSummary: Goal115StepSummary(snapshot?.RootElement),
            Errors: Goal115Join(snapshot?.RootElement, "errors"),
            Warnings: Goal115Join(snapshot?.RootElement, "warnings"),
            QualityGatePassed: quality is not null && TryGetBool(quality.RootElement, "passed"),
            RelativePaths: Goal115AllPathsRelative(projectRoot));
        AddIfFalse(summary.QualityGatePassed, "goal115.workspace.summary_failed",
            "offline_geoworld_alpha_human_result_revalidation", diagnostics);
        return summary;
    }

    private static VisualWorldPreviewArtifactEntry Goal115FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = OfflineGeoworldAlphaHumanResultRevalidationVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal115 revalidation file exists" : "Goal115 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; noRawManualResult=true"
        };
    }

    private static bool Goal115AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory,
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal115String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static string Goal115Join(JsonElement? element, string propertyName)
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

    private static string Goal115StepSummary(JsonElement? element)
    {
        if (element is null
            || !element.Value.TryGetProperty("stepSummary", out var summary)
            || summary.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        return "required=" + Goal115Int(summary, "requiredStepCount")
               + "; result=" + Goal115Int(summary, "resultStepCount")
               + "; passed=" + Goal115Int(summary, "passedCount")
               + "; failed=" + Goal115Int(summary, "failedCount")
               + "; pending=" + Goal115Int(summary, "pendingCount")
               + "; skipped=" + Goal115Int(summary, "skippedCount")
               + "; missing=" + Goal115Int(summary, "missingCount")
               + "; duplicate=" + Goal115Int(summary, "duplicateCount")
               + "; invalidStatus=" + Goal115Int(summary, "invalidStatusCount");
    }

    private static int Goal115Int(JsonElement? element, string propertyName) =>
        element is not null && TryGetInt(element.Value, propertyName, out var value) ? value : 0;

    private sealed record OfflineGeoworldAlphaHumanResultRevalidationWorkspaceSummary(
        string DecisionStatus,
        string Goal111DecisionStatus,
        bool ManualResultPresent,
        bool ManualResultJsonValid,
        string ManualResultRelativePath,
        string ManualResultSha256,
        bool AcceptableCandidate,
        string RecommendedHumanDecision,
        int RequiredStepCount,
        int PassedStepCount,
        int BlockingStepIssueCount,
        string StepSummary,
        string Errors,
        string Warnings,
        bool QualityGatePassed,
        bool RelativePaths);
}
