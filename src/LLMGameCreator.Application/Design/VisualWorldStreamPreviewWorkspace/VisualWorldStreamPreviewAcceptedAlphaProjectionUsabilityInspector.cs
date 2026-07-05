using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildAcceptedAlphaProjectionUsabilityGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadAcceptedAlphaProjectionUsabilitySummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory,
                AcceptedAlphaProjectionUsabilityVocabulary.GoalId,
                BuildGoal120ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithAcceptedAlphaProjectionUsabilitySummary(entry, summary))
            .ToList();

        foreach (var fileName in AcceptedAlphaProjectionUsabilityVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithAcceptedAlphaProjectionUsabilitySummary(
                Goal120FileEntry(
                    projectRoot,
                    AcceptedAlphaProjectionUsabilityVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "accepted_alpha_projection_usability_export_file"),
                summary));
        }

        entries.Add(WithAcceptedAlphaProjectionUsabilitySummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = AcceptedAlphaProjectionUsabilityVocabulary.GoalId + ".summary",
                RelativePath =
                    AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory
                    + "/"
                    + AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName,
                ArtifactKind = "accepted_alpha_projection_usability_workspace_summary",
                SourceGoalId = AcceptedAlphaProjectionUsabilityVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory
                    + "/"
                    + AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "usabilityStatus=" + summary.UsabilityStatus
                                    + "; unitySmokeStatus=" + summary.UnitySmokeStatus,
                SafeRatingMetadataSummary =
                    "manualUnityEditorUsabilityOnly=true; noRuntimeProviderSchemaLua=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "accepted_alpha_projection_usability",
            "Goal 120 Accepted Alpha Projection Usability",
            AcceptedAlphaProjectionUsabilityVocabulary.GoalId,
            AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal120ProceduralFiles() =>
    [
        (AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName,
            "accepted_alpha_projection_usability_dashboard"),
        (AcceptedAlphaProjectionUsabilityVocabulary.ScriptInventoryFileName,
            "accepted_alpha_projection_usability_script_inventory"),
        (AcceptedAlphaProjectionUsabilityVocabulary.SmokePlanFileName,
            "accepted_alpha_projection_usability_smoke_plan"),
        (AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptScanFileName,
            "accepted_alpha_projection_cleanup_script_scan"),
        (AcceptedAlphaProjectionUsabilityVocabulary.ReportFileName,
            "accepted_alpha_projection_usability_report"),
        (AcceptedAlphaProjectionUsabilityVocabulary.NegativeProofFileName,
            "accepted_alpha_projection_usability_negative_proof"),
        (AcceptedAlphaProjectionUsabilityVocabulary.FileIndexFileName,
            "accepted_alpha_projection_usability_file_index")
    ];

    private static VisualWorldPreviewArtifactEntry WithAcceptedAlphaProjectionUsabilitySummary(
        VisualWorldPreviewArtifactEntry entry,
        AcceptedAlphaProjectionUsabilityWorkspaceSummary summary) =>
        entry with
        {
            AcceptedAlphaProjectionUsabilityStatus = summary.UsabilityStatus,
            AcceptedAlphaProjectionUsabilityUnityMenuPath = summary.UnityMenuPath,
            AcceptedAlphaProjectionUsabilityCleanupScriptPath = summary.CleanupScriptPath,
            AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath = summary.CleanupScriptCmdPath,
            AcceptedAlphaProjectionUsabilityLegendPresent = summary.LegendPresent,
            AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent =
                summary.MarkerDescriptorPresent,
            AcceptedAlphaProjectionUsabilitySelectionControlsPresent =
                summary.SelectionControlsPresent,
            AcceptedAlphaProjectionUsabilityFocusCameraControlPresent =
                summary.FocusCameraControlPresent,
            AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent =
                summary.MaterialWarningGuardPresent,
            AcceptedAlphaProjectionUsabilityUnitySmokeStatus = summary.UnitySmokeStatus,
            AcceptedAlphaProjectionUsabilityDoNotStartAutomatically =
                summary.DoNotStartAutomatically,
            AcceptedAlphaProjectionUsabilityEvidencePath = summary.EvidencePath,
            AcceptedAlphaProjectionUsabilityExportPath = summary.ExportPath,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static AcceptedAlphaProjectionUsabilityWorkspaceSummary
        LoadAcceptedAlphaProjectionUsabilitySummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName,
            diagnostics);
        return new AcceptedAlphaProjectionUsabilityWorkspaceSummary(
            UsabilityStatus: Goal120String(dashboard?.RootElement, "usabilityStatus"),
            UnityMenuPath: Goal120String(dashboard?.RootElement, "unityMenuPath"),
            CleanupScriptPath: Goal120String(dashboard?.RootElement, "cleanupScriptPath"),
            CleanupScriptCmdPath: Goal120String(dashboard?.RootElement, "cleanupScriptCmdPath"),
            LegendPresent: dashboard is not null && TryGetBool(dashboard.RootElement, "legendPresent"),
            MarkerDescriptorPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "markerDescriptorPresent"),
            SelectionControlsPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "selectionControlsPresent"),
            FocusCameraControlPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "focusCameraControlPresent"),
            MaterialWarningGuardPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "materialWarningGuardPresent"),
            UnitySmokeStatus: Goal120String(dashboard?.RootElement, "unitySmokeStatus"),
            DoNotStartAutomatically:
                dashboard is not null && TryGetBool(dashboard.RootElement, "doNotStartAutomatically"),
            EvidencePath: Goal120String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal120String(dashboard?.RootElement, "exportPath"),
            QualityGatePassed:
                Goal120String(dashboard?.RootElement, "usabilityStatus")
                == AcceptedAlphaProjectionUsabilityVocabulary.UsabilityStatus,
            RelativePaths: Goal120AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal120FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = AcceptedAlphaProjectionUsabilityVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = AcceptedAlphaProjectionUsabilityVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal120 usability file exists" : "Goal120 file missing",
            SafeRatingMetadataSummary = "metadataOnly=true; noManualInput=true"
        };
    }

    private static bool Goal120AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory,
            AcceptedAlphaProjectionUsabilityVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal120String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record AcceptedAlphaProjectionUsabilityWorkspaceSummary(
        string UsabilityStatus,
        string UnityMenuPath,
        string CleanupScriptPath,
        string CleanupScriptCmdPath,
        bool LegendPresent,
        bool MarkerDescriptorPresent,
        bool SelectionControlsPresent,
        bool FocusCameraControlPresent,
        bool MaterialWarningGuardPresent,
        string UnitySmokeStatus,
        bool DoNotStartAutomatically,
        string EvidencePath,
        string ExportPath,
        bool QualityGatePassed,
        bool RelativePaths);
}
