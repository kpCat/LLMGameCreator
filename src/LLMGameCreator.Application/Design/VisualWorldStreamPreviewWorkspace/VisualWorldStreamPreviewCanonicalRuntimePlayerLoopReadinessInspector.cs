using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildCanonicalRuntimePlayerLoopReadinessGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadCanonicalRuntimePlayerLoopSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory,
                CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId,
                BuildGoal135ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithCanonicalRuntimePlayerLoopSummary(entry, summary))
            .ToList();

        foreach (var file in BuildGoal135ExportFiles())
        {
            entries.Add(WithCanonicalRuntimePlayerLoopSummary(
                Goal135FileEntry(projectRoot, file.RelativePath, file.Kind),
                summary));
        }

        entries.Add(WithCanonicalRuntimePlayerLoopSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId + ".summary",
                RelativePath = CanonicalRuntimePlayerLoopReadinessVocabulary.DashboardRelativePath,
                ArtifactKind = "canonical_runtime_player_loop_readiness_workspace_summary",
                SourceGoalId = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    CanonicalRuntimePlayerLoopReadinessVocabulary.DashboardRelativePath,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "candidateId=" + summary.CandidateId
                                    + "; playerLoopStepCount=" + summary.PlayerLoopStepCount
                                    + "; unityPlayerLoopReadinessPassed="
                                    + summary.UnityPlayerLoopReadinessPassed.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "canonicalRuntimeSource=true; unityGameplayTruth=false; projectionOnly=false"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "canonical_runtime_player_loop_readiness",
            "Goal 135 Canonical Runtime Player Loop Readiness",
            CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId,
            CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal135ProceduralFiles() =>
    [
        (CanonicalRuntimePlayerLoopReadinessVocabulary.AdapterContractFileName,
            "canonical_runtime_player_adapter_contract"),
        (CanonicalRuntimePlayerLoopReadinessVocabulary.PlayerLoopPlanFileName,
            "canonical_runtime_player_loop_plan"),
        (CanonicalRuntimePlayerLoopReadinessVocabulary.ReadinessResultFileName,
            "canonical_runtime_player_loop_readiness_result"),
        (CanonicalRuntimePlayerLoopReadinessVocabulary.DashboardFileName,
            "canonical_runtime_player_loop_readiness_dashboard"),
        (CanonicalRuntimePlayerLoopReadinessVocabulary.MatrixResultFileName,
            "canonical_runtime_player_loop_matrix_result"),
        (CanonicalRuntimePlayerLoopReadinessVocabulary.DiagnosticClassificationFileName,
            "canonical_runtime_diagnostic_classification"),
        (CanonicalRuntimePlayerLoopReadinessVocabulary.UnitySmokeFileName,
            "unity_player_loop_readiness_smoke"),
        (CanonicalRuntimePlayerLoopReadinessVocabulary.ReportJsonFileName,
            "player_loop_readiness_one_click_report_json"),
        (CanonicalRuntimePlayerLoopReadinessVocabulary.ReportMarkdownFileName,
            "player_loop_readiness_one_click_report_markdown"),
        (CanonicalRuntimePlayerLoopReadinessVocabulary.NegativeProofFileName,
            "canonical_runtime_player_loop_negative_proof"),
        (CanonicalRuntimePlayerLoopReadinessVocabulary.FileIndexFileName,
            "canonical_runtime_player_loop_file_index")
    ];

    private static IReadOnlyList<(string RelativePath, string Kind)> BuildGoal135ExportFiles() =>
        BuildGoal135ProceduralFiles()
            .Select(item => (
                RelativePath:
                CanonicalRuntimePlayerLoopReadinessVocabulary.ExportPackageDirectory
                + "/"
                + item.FileName,
                Kind: "canonical_runtime_player_loop_export_file"))
            .OrderBy(item => item.RelativePath, StringComparer.Ordinal)
            .ToList();

    private static VisualWorldPreviewArtifactEntry WithCanonicalRuntimePlayerLoopSummary(
        VisualWorldPreviewArtifactEntry entry,
        CanonicalRuntimePlayerLoopWorkspaceSummary summary) =>
        entry with
        {
            CanonicalRuntimePlayerLoopCandidateId = summary.CandidateId,
            CanonicalRuntimePlayerLoopAdapterContractPresent =
                summary.PlayerAdapterContractPresent,
            CanonicalRuntimePlayerLoopStepCount = summary.PlayerLoopStepCount,
            CanonicalRuntimePlayerLoopRequiredCategoriesPresent =
                summary.RequiredStepCategoriesPresent,
            CanonicalRuntimePlayerLoopUnityReadinessPassed =
                summary.UnityPlayerLoopReadinessPassed,
            CanonicalRuntimePlayerLoopSource = summary.CanonicalRuntimeSource,
            CanonicalRuntimePlayerLoopUnityGameplayTruth = summary.UnityGameplayTruth,
            CanonicalRuntimePlayerLoopProjectionOnly = summary.ProjectionOnly,
            CanonicalRuntimePlayerLoopNoUnclassifiedErrors =
                summary.NoUnclassifiedErrorDiagnostics,
            CanonicalRuntimePlayerLoopNormalCommand = summary.NormalCommand,
            CanonicalRuntimePlayerLoopReportPath = summary.ReportPath,
            CanonicalRuntimePlayerLoopManualUnityOptional = summary.ManualUnityOptional,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static CanonicalRuntimePlayerLoopWorkspaceSummary
        LoadCanonicalRuntimePlayerLoopSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        using var dashboard = TryReadJson(
            projectRoot,
            CanonicalRuntimePlayerLoopReadinessVocabulary.DashboardRelativePath,
            diagnostics);
        return new CanonicalRuntimePlayerLoopWorkspaceSummary(
            CandidateId: Goal135String(dashboard?.RootElement, "candidateId"),
            PlayerAdapterContractPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "playerAdapterContractPresent"),
            PlayerLoopStepCount: dashboard is not null
                ? Goal135Int(dashboard.RootElement, "playerLoopStepCount")
                : 0,
            RequiredStepCategoriesPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "requiredStepCategoriesPresent"),
            UnityPlayerLoopReadinessPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityPlayerLoopReadinessPassed"),
            CanonicalRuntimeSource:
                dashboard is not null && TryGetBool(dashboard.RootElement, "canonicalRuntimeSource"),
            UnityGameplayTruth:
                dashboard is not null && TryGetBool(dashboard.RootElement, "unityGameplayTruth"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            NoUnclassifiedErrorDiagnostics:
                dashboard is not null && TryGetBool(dashboard.RootElement, "noUnclassifiedErrorDiagnostics"),
            NormalCommand: Goal135String(dashboard?.RootElement, "normalCommand"),
            ReportPath: Goal135String(dashboard?.RootElement, "reportPath"),
            ManualUnityOptional:
                dashboard is not null && TryGetBool(dashboard.RootElement, "manualUnityOptional"),
            QualityGatePassed:
                Goal135String(dashboard?.RootElement, "status") == "GREEN",
            RelativePaths: Goal135AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal135FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = CanonicalRuntimePlayerLoopReadinessVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal135 player-loop readiness file exists" : "Goal135 player-loop readiness file missing",
            SafeRatingMetadataSummary = "canonicalRuntimePlayerLoopArtifact=true; noManualInput=true"
        };
    }

    private static bool Goal135AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            CanonicalRuntimePlayerLoopReadinessVocabulary.ProceduralOutputDirectory,
            CanonicalRuntimePlayerLoopReadinessVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.AllDirectories)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal135String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal135Int(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record CanonicalRuntimePlayerLoopWorkspaceSummary(
        string CandidateId,
        bool PlayerAdapterContractPresent,
        int PlayerLoopStepCount,
        bool RequiredStepCategoriesPresent,
        bool UnityPlayerLoopReadinessPassed,
        bool CanonicalRuntimeSource,
        bool UnityGameplayTruth,
        bool ProjectionOnly,
        bool NoUnclassifiedErrorDiagnostics,
        string NormalCommand,
        string ReportPath,
        bool ManualUnityOptional,
        bool QualityGatePassed,
        bool RelativePaths);
}
