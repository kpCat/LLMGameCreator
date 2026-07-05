using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildAcceptedAlphaUnityPlayableProjectionGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadAcceptedAlphaUnityPlayableProjectionSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory,
                AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId,
                BuildGoal119ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithAcceptedAlphaUnityPlayableProjectionSummary(entry, summary))
            .ToList();

        foreach (var fileName in AcceptedAlphaUnityPlayableProjectionVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithAcceptedAlphaUnityPlayableProjectionSummary(
                Goal119FileEntry(
                    projectRoot,
                    AcceptedAlphaUnityPlayableProjectionVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "accepted_alpha_unity_playable_projection_export_file"),
                summary));
        }

        entries.Add(WithAcceptedAlphaUnityPlayableProjectionSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId + ".summary",
                RelativePath =
                    AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + AcceptedAlphaUnityPlayableProjectionVocabulary.DashboardFileName,
                ArtifactKind = "accepted_alpha_unity_playable_projection_workspace_summary",
                SourceGoalId = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + AcceptedAlphaUnityPlayableProjectionVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "projectionStatus=" + summary.ProjectionStatus
                                    + "; acceptedBaselineReady="
                                    + summary.AcceptedBaselineReady.ToString().ToLowerInvariant(),
                SafeRatingMetadataSummary =
                    "temporaryUnityEditorProjection=true; noRuntimeProviderSchemaLua=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "accepted_alpha_unity_playable_projection",
            "Goal 119 Accepted Alpha Unity Playable Projection",
            AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId,
            AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal119ProceduralFiles() =>
    [
        (AcceptedAlphaUnityPlayableProjectionVocabulary.DashboardFileName,
            "accepted_alpha_unity_playable_projection_dashboard"),
        (AcceptedAlphaUnityPlayableProjectionVocabulary.ScriptInventoryFileName,
            "accepted_alpha_unity_playable_projection_script_inventory"),
        (AcceptedAlphaUnityPlayableProjectionVocabulary.SmokePlanFileName,
            "accepted_alpha_unity_playable_projection_smoke_plan"),
        (AcceptedAlphaUnityPlayableProjectionVocabulary.ReportFileName,
            "accepted_alpha_unity_playable_projection_report"),
        (AcceptedAlphaUnityPlayableProjectionVocabulary.QualityGateScanFileName,
            "accepted_alpha_unity_playable_projection_quality_gate"),
        (AcceptedAlphaUnityPlayableProjectionVocabulary.NegativeProofFileName,
            "accepted_alpha_unity_playable_projection_negative_proof"),
        (AcceptedAlphaUnityPlayableProjectionVocabulary.FileIndexFileName,
            "accepted_alpha_unity_playable_projection_file_index")
    ];

    private static VisualWorldPreviewArtifactEntry WithAcceptedAlphaUnityPlayableProjectionSummary(
        VisualWorldPreviewArtifactEntry entry,
        AcceptedAlphaUnityPlayableProjectionWorkspaceSummary summary) =>
        entry with
        {
            AcceptedAlphaUnityPlayableProjectionStatus = summary.ProjectionStatus,
            AcceptedAlphaUnityPlayableProjectionUnityMenuPath = summary.UnityMenuPath,
            AcceptedAlphaUnityPlayableProjectionBaselineId = summary.BaselineId,
            AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady =
                summary.AcceptedBaselineReady,
            AcceptedAlphaUnityPlayableProjectionGeneratedRootName =
                summary.GeneratedRootName,
            AcceptedAlphaUnityPlayableProjectionScriptInventoryCount =
                summary.ScriptInventoryCount,
            AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount =
                summary.SmokePlanStepCount,
            AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean =
                summary.ForbiddenUnitySurfaceClean,
            AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically =
                summary.DoNotStartAutomatically,
            AcceptedAlphaUnityPlayableProjectionEvidencePath = summary.EvidencePath,
            AcceptedAlphaUnityPlayableProjectionExportPath = summary.ExportPath,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static AcceptedAlphaUnityPlayableProjectionWorkspaceSummary
        LoadAcceptedAlphaUnityPlayableProjectionSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + AcceptedAlphaUnityPlayableProjectionVocabulary.DashboardFileName,
            diagnostics);
        using var quality = TryReadJson(
            projectRoot,
            root + "/" + AcceptedAlphaUnityPlayableProjectionVocabulary.QualityGateScanFileName,
            diagnostics);
        return new AcceptedAlphaUnityPlayableProjectionWorkspaceSummary(
            ProjectionStatus: Goal119String(dashboard?.RootElement, "projectionStatus"),
            UnityMenuPath: Goal119String(dashboard?.RootElement, "unityMenuPath"),
            BaselineId: Goal119String(dashboard?.RootElement, "baselineId"),
            AcceptedBaselineReady:
                dashboard is not null && TryGetBool(dashboard.RootElement, "acceptedBaselineReady"),
            GeneratedRootName: Goal119String(dashboard?.RootElement, "expectedGeneratedRootName"),
            ScriptInventoryCount: Goal119Int(dashboard?.RootElement, "scriptInventoryCount"),
            SmokePlanStepCount: Goal119Int(dashboard?.RootElement, "smokePlanStepCount"),
            ForbiddenUnitySurfaceClean:
                dashboard is not null && TryGetBool(dashboard.RootElement, "forbiddenUnitySurfaceClean"),
            DoNotStartAutomatically:
                dashboard is not null && TryGetBool(dashboard.RootElement, "doNotStartAutomatically"),
            EvidencePath: Goal119String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal119String(dashboard?.RootElement, "exportPath"),
            QualityGatePassed: quality is not null && TryGetBool(quality.RootElement, "passed"),
            RelativePaths: Goal119AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal119FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = AcceptedAlphaUnityPlayableProjectionVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal119 projection file exists" : "Goal119 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; noManualInput=true"
        };
    }

    private static bool Goal119AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory,
            AcceptedAlphaUnityPlayableProjectionVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal119String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal119Int(JsonElement? element, string propertyName) =>
        element is not null && TryGetInt(element.Value, propertyName, out var value) ? value : 0;

    private sealed record AcceptedAlphaUnityPlayableProjectionWorkspaceSummary(
        string ProjectionStatus,
        string UnityMenuPath,
        string BaselineId,
        bool AcceptedBaselineReady,
        string GeneratedRootName,
        int ScriptInventoryCount,
        int SmokePlanStepCount,
        bool ForbiddenUnitySurfaceClean,
        bool DoNotStartAutomatically,
        string EvidencePath,
        string ExportPath,
        bool QualityGatePassed,
        bool RelativePaths);
}
