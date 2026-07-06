using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal124CleanupCommand =
        ".\\.devflow\\scripts\\clean-unity-editor-noise.cmd";

    private static VisualWorldPreviewArtifactGroup BuildGenericGamePackageLoopGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadGenericGamePackageLoopSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory,
                GenericGamePackageLoopProjectionVocabulary.GoalId,
                BuildGoal124ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithGenericGamePackageLoopSummary(entry, summary))
            .ToList();

        foreach (var fileName in GenericGamePackageLoopProjectionVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithGenericGamePackageLoopSummary(
                Goal124FileEntry(
                    projectRoot,
                    GenericGamePackageLoopProjectionVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "generic_gamepackage_loop_export_file"),
                summary));
        }

        entries.Add(WithGenericGamePackageLoopSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = GenericGamePackageLoopProjectionVocabulary.GoalId + ".summary",
                RelativePath =
                    GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + GenericGamePackageLoopProjectionVocabulary.DashboardFileName,
                ArtifactKind = "generic_gamepackage_loop_workspace_summary",
                SourceGoalId = GenericGamePackageLoopProjectionVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + GenericGamePackageLoopProjectionVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "genericLoopStatus=" + summary.GenericLoopStatus
                                    + "; unitySmokeStatus=" + summary.UnitySmokeStatus,
                SafeRatingMetadataSummary =
                    "projectionOnly=true; samplePackageReadOnly=true; cleanupCommand="
                    + Goal124CleanupCommand
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "generic_gamepackage_loop",
            "Goal 124 Generic GamePackage Loop",
            GenericGamePackageLoopProjectionVocabulary.GoalId,
            GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal124ProceduralFiles() =>
    [
        (GenericGamePackageLoopProjectionVocabulary.DashboardFileName,
            "generic_gamepackage_loop_dashboard"),
        (GenericGamePackageLoopProjectionVocabulary.ScriptInventoryFileName,
            "generic_gamepackage_loop_script_inventory"),
        (GenericGamePackageLoopProjectionVocabulary.SmokePlanFileName,
            "generic_gamepackage_loop_smoke_plan"),
        (GenericGamePackageLoopProjectionVocabulary.LogScanFileName,
            "generic_gamepackage_loop_log_scan"),
        (GenericGamePackageLoopProjectionVocabulary.ReportFileName,
            "generic_gamepackage_loop_report"),
        (GenericGamePackageLoopProjectionVocabulary.NegativeProofFileName,
            "generic_gamepackage_loop_negative_proof"),
        (GenericGamePackageLoopProjectionVocabulary.FileIndexFileName,
            "generic_gamepackage_loop_file_index")
    ];

    private static VisualWorldPreviewArtifactEntry WithGenericGamePackageLoopSummary(
        VisualWorldPreviewArtifactEntry entry,
        GenericGamePackageLoopWorkspaceSummary summary) =>
        entry with
        {
            GenericLoopStatus = summary.GenericLoopStatus,
            GenericLoopSamplePackagePath = summary.SamplePackagePath,
            GenericLoopPackageId = summary.PackageId,
            GenericLoopMapId = summary.MapId,
            GenericLoopInteractionPreviewPresent = summary.InteractionPreviewPresent,
            GenericLoopInteractionApplyPassed = summary.InteractionApplyPassed,
            GenericLoopDialogueSummaryPresent = summary.DialogueSummaryPresent,
            GenericLoopQuestObjectiveSummaryPresent = summary.QuestObjectiveSummaryPresent,
            GenericLoopInventorySummaryPresent = summary.InventorySummaryPresent,
            GenericLoopResourceSummaryPresent = summary.ResourceSummaryPresent,
            GenericLoopUnitySmokeStatus = summary.UnitySmokeStatus,
            GenericLoopCleanupScriptAvailable = summary.CleanupScriptAvailable,
            GenericLoopCleanupCommand = Goal124CleanupCommand,
            GenericLoopGoal123StillGreen = summary.Goal123StillGreen,
            GenericLoopProjectionOnly = summary.ProjectionOnly,
            GenericLoopAppliedInteractionCount = summary.AppliedInteractionCount,
            GenericLoopStartedQuestCount = summary.StartedQuestCount,
            GenericLoopEvidencePath = summary.EvidencePath,
            GenericLoopExportPath = summary.ExportPath,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static GenericGamePackageLoopWorkspaceSummary LoadGenericGamePackageLoopSummary(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + GenericGamePackageLoopProjectionVocabulary.DashboardFileName,
            diagnostics);
        return new GenericGamePackageLoopWorkspaceSummary(
            GenericLoopStatus: Goal124String(dashboard?.RootElement, "genericLoopStatus"),
            SamplePackagePath: Goal124String(dashboard?.RootElement, "samplePackagePath"),
            PackageId: Goal124String(dashboard?.RootElement, "packageId"),
            MapId: Goal124String(dashboard?.RootElement, "mapId"),
            InteractionPreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "interactionPreviewPresent"),
            InteractionApplyPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "interactionApplyPassed"),
            DialogueSummaryPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "dialogueSummaryPresent"),
            QuestObjectiveSummaryPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "questObjectiveSummaryPresent"),
            InventorySummaryPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "inventorySummaryPresent"),
            ResourceSummaryPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "resourceSummaryPresent"),
            UnitySmokeStatus: Goal124String(dashboard?.RootElement, "unitySmokeStatus"),
            CleanupScriptAvailable:
                dashboard is not null && TryGetBool(dashboard.RootElement, "cleanupScriptAvailable"),
            Goal123StillGreen:
                dashboard is not null && TryGetBool(dashboard.RootElement, "goal123StillGreen"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            AppliedInteractionCount: Goal124Int(dashboard?.RootElement, "appliedInteractionCount"),
            StartedQuestCount: Goal124Int(dashboard?.RootElement, "startedQuestCount"),
            EvidencePath: Goal124String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal124String(dashboard?.RootElement, "exportPath"),
            QualityGatePassed:
                Goal124String(dashboard?.RootElement, "genericLoopStatus") == "GREEN",
            RelativePaths: Goal124AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal124FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = GenericGamePackageLoopProjectionVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = GenericGamePackageLoopProjectionVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal124 generic package loop file exists" : "Goal124 file missing",
            SafeRatingMetadataSummary = "projectionOnly=true; noManualInput=true"
        };
    }

    private static bool Goal124AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory,
            GenericGamePackageLoopProjectionVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal124String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal124Int(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record GenericGamePackageLoopWorkspaceSummary(
        string GenericLoopStatus,
        string SamplePackagePath,
        string PackageId,
        string MapId,
        bool InteractionPreviewPresent,
        bool InteractionApplyPassed,
        bool DialogueSummaryPresent,
        bool QuestObjectiveSummaryPresent,
        bool InventorySummaryPresent,
        bool ResourceSummaryPresent,
        string UnitySmokeStatus,
        bool CleanupScriptAvailable,
        bool Goal123StillGreen,
        bool ProjectionOnly,
        int AppliedInteractionCount,
        int StartedQuestCount,
        string EvidencePath,
        string ExportPath,
        bool QualityGatePassed,
        bool RelativePaths);
}
