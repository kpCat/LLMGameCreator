using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildAcceptedAlphaProjectionActionLoopGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadAcceptedAlphaProjectionActionLoopSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory,
                AcceptedAlphaProjectionActionLoopVocabulary.GoalId,
                BuildGoal122ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithAcceptedAlphaProjectionActionLoopSummary(entry, summary))
            .ToList();

        foreach (var fileName in AcceptedAlphaProjectionActionLoopVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithAcceptedAlphaProjectionActionLoopSummary(
                Goal122FileEntry(
                    projectRoot,
                    AcceptedAlphaProjectionActionLoopVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "accepted_alpha_projection_action_loop_export_file"),
                summary));
        }

        entries.Add(WithAcceptedAlphaProjectionActionLoopSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = AcceptedAlphaProjectionActionLoopVocabulary.GoalId + ".summary",
                RelativePath =
                    AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory
                    + "/"
                    + AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName,
                ArtifactKind = "accepted_alpha_projection_action_loop_workspace_summary",
                SourceGoalId = AcceptedAlphaProjectionActionLoopVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory
                    + "/"
                    + AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "actionLoopStatus=" + summary.ActionLoopStatus
                                    + "; unitySmokeStatus=" + summary.UnitySmokeStatus,
                SafeRatingMetadataSummary =
                    "manualUnityEditorVerificationOnly=true; projectionOnlyState=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "accepted_alpha_projection_action_loop",
            "Goal 122 Accepted Alpha Projection Action Loop",
            AcceptedAlphaProjectionActionLoopVocabulary.GoalId,
            AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal122ProceduralFiles() =>
    [
        (AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName,
            "accepted_alpha_projection_action_loop_dashboard"),
        (AcceptedAlphaProjectionActionLoopVocabulary.ScriptInventoryFileName,
            "accepted_alpha_projection_action_loop_script_inventory"),
        (AcceptedAlphaProjectionActionLoopVocabulary.SmokePlanFileName,
            "accepted_alpha_projection_action_loop_smoke_plan"),
        (AcceptedAlphaProjectionActionLoopVocabulary.LogScanFileName,
            "accepted_alpha_projection_action_loop_log_scan"),
        (AcceptedAlphaProjectionActionLoopVocabulary.ReportFileName,
            "accepted_alpha_projection_action_loop_report"),
        (AcceptedAlphaProjectionActionLoopVocabulary.NegativeProofFileName,
            "accepted_alpha_projection_action_loop_negative_proof"),
        (AcceptedAlphaProjectionActionLoopVocabulary.FileIndexFileName,
            "accepted_alpha_projection_action_loop_file_index")
    ];

    private static VisualWorldPreviewArtifactEntry WithAcceptedAlphaProjectionActionLoopSummary(
        VisualWorldPreviewArtifactEntry entry,
        AcceptedAlphaProjectionActionLoopWorkspaceSummary summary) =>
        entry with
        {
            AcceptedAlphaProjectionActionLoopStatus = summary.ActionLoopStatus,
            AcceptedAlphaProjectionActionLoopWindowPolishStatus = summary.WindowPolishStatus,
            AcceptedAlphaProjectionActionLoopUnityMenuPath = summary.UnityMenuPath,
            AcceptedAlphaProjectionActionLoopOneClickVerificationStillPresent =
                summary.OneClickVerificationStillPresent,
            AcceptedAlphaProjectionActionLoopProjectionActionPreviewPresent =
                summary.ProjectionActionPreviewPresent,
            AcceptedAlphaProjectionActionLoopProjectionActionApplyPresent =
                summary.ProjectionActionApplyPresent,
            AcceptedAlphaProjectionActionLoopProjectionStateResetPresent =
                summary.ProjectionStateResetPresent,
            AcceptedAlphaProjectionActionLoopWindowLayoutPolishPresent =
                summary.WindowLayoutPolishPresent,
            AcceptedAlphaProjectionActionLoopUnitySmokeStatus = summary.UnitySmokeStatus,
            AcceptedAlphaProjectionActionLoopCleanupScriptAvailable =
                summary.CleanupScriptAvailable,
            AcceptedAlphaProjectionActionLoopDoNotStartAutomatically =
                summary.DoNotStartAutomatically,
            AcceptedAlphaProjectionActionLoopEvidencePath = summary.EvidencePath,
            AcceptedAlphaProjectionActionLoopExportPath = summary.ExportPath,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static AcceptedAlphaProjectionActionLoopWorkspaceSummary
        LoadAcceptedAlphaProjectionActionLoopSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + AcceptedAlphaProjectionActionLoopVocabulary.DashboardFileName,
            diagnostics);
        return new AcceptedAlphaProjectionActionLoopWorkspaceSummary(
            ActionLoopStatus: Goal122String(dashboard?.RootElement, "actionLoopStatus"),
            WindowPolishStatus: Goal122String(dashboard?.RootElement, "windowPolishStatus"),
            UnityMenuPath: Goal122String(dashboard?.RootElement, "unityMenuPath"),
            OneClickVerificationStillPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "oneClickVerificationStillPresent"),
            ProjectionActionPreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionActionPreviewPresent"),
            ProjectionActionApplyPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionActionApplyPresent"),
            ProjectionStateResetPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionStateResetPresent"),
            WindowLayoutPolishPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "windowLayoutPolishPresent"),
            UnitySmokeStatus: Goal122String(dashboard?.RootElement, "unitySmokeStatus"),
            CleanupScriptAvailable:
                dashboard is not null && TryGetBool(dashboard.RootElement, "cleanupScriptAvailable"),
            DoNotStartAutomatically:
                dashboard is not null && TryGetBool(dashboard.RootElement, "doNotStartAutomatically"),
            EvidencePath: Goal122String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal122String(dashboard?.RootElement, "exportPath"),
            QualityGatePassed:
                Goal122String(dashboard?.RootElement, "actionLoopStatus") == "GREEN"
                && Goal122String(dashboard?.RootElement, "windowPolishStatus") == "GREEN",
            RelativePaths: Goal122AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal122FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = AcceptedAlphaProjectionActionLoopVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = AcceptedAlphaProjectionActionLoopVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal122 action loop file exists" : "Goal122 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; noManualInput=true"
        };
    }

    private static bool Goal122AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory,
            AcceptedAlphaProjectionActionLoopVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal122String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record AcceptedAlphaProjectionActionLoopWorkspaceSummary(
        string ActionLoopStatus,
        string WindowPolishStatus,
        string UnityMenuPath,
        bool OneClickVerificationStillPresent,
        bool ProjectionActionPreviewPresent,
        bool ProjectionActionApplyPresent,
        bool ProjectionStateResetPresent,
        bool WindowLayoutPolishPresent,
        string UnitySmokeStatus,
        bool CleanupScriptAvailable,
        bool DoNotStartAutomatically,
        string EvidencePath,
        string ExportPath,
        bool QualityGatePassed,
        bool RelativePaths);
}
