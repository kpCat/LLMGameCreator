using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldPreviewArtifactGroup BuildGenericGamePackageProjectionGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadGenericGamePackageProjectionSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory,
                GenericGamePackageProjectionVocabulary.GoalId,
                BuildGoal123ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithGenericGamePackageProjectionSummary(entry, summary))
            .ToList();

        foreach (var fileName in GenericGamePackageProjectionVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithGenericGamePackageProjectionSummary(
                Goal123FileEntry(
                    projectRoot,
                    GenericGamePackageProjectionVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "generic_gamepackage_projection_export_file"),
                summary));
        }

        entries.Add(WithGenericGamePackageProjectionSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = GenericGamePackageProjectionVocabulary.GoalId + ".summary",
                RelativePath =
                    GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + GenericGamePackageProjectionVocabulary.DashboardFileName,
                ArtifactKind = "generic_gamepackage_projection_workspace_summary",
                SourceGoalId = GenericGamePackageProjectionVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + GenericGamePackageProjectionVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "genericProjectionStatus=" + summary.GenericProjectionStatus
                                    + "; unitySmokeStatus=" + summary.UnitySmokeStatus,
                SafeRatingMetadataSummary =
                    "projectionOnly=true; samplePackageReadOnly=true; manualUnityEditorVerificationOptional=true"
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "generic_gamepackage_projection",
            "Goal 123 Generic GamePackage Projection",
            GenericGamePackageProjectionVocabulary.GoalId,
            GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal123ProceduralFiles() =>
    [
        (GenericGamePackageProjectionVocabulary.DashboardFileName,
            "generic_gamepackage_projection_dashboard"),
        (GenericGamePackageProjectionVocabulary.ScriptInventoryFileName,
            "generic_gamepackage_projection_script_inventory"),
        (GenericGamePackageProjectionVocabulary.SmokePlanFileName,
            "generic_gamepackage_projection_smoke_plan"),
        (GenericGamePackageProjectionVocabulary.LogScanFileName,
            "generic_gamepackage_projection_log_scan"),
        (GenericGamePackageProjectionVocabulary.ReportFileName,
            "generic_gamepackage_projection_report"),
        (GenericGamePackageProjectionVocabulary.NegativeProofFileName,
            "generic_gamepackage_projection_negative_proof"),
        (GenericGamePackageProjectionVocabulary.FileIndexFileName,
            "generic_gamepackage_projection_file_index")
    ];

    private static VisualWorldPreviewArtifactEntry WithGenericGamePackageProjectionSummary(
        VisualWorldPreviewArtifactEntry entry,
        GenericGamePackageProjectionWorkspaceSummary summary) =>
        entry with
        {
            GenericProjectionStatus = summary.GenericProjectionStatus,
            GenericProjectionSamplePackagePath = summary.SamplePackagePath,
            GenericProjectionPackageId = summary.PackageId,
            GenericProjectionPackageTitle = summary.PackageTitle,
            GenericProjectionMapId = summary.MapId,
            GenericProjectionMapSize = summary.MapSize,
            GenericProjectionEntityCount = summary.EntityCount,
            GenericProjectionItemCount = summary.ItemCount,
            GenericProjectionUnitySmokeStatus = summary.UnitySmokeStatus,
            GenericProjectionGoal122StillGreen = summary.Goal122StillGreen,
            GenericProjectionCleanupScriptAvailable = summary.CleanupScriptAvailable,
            GenericProjectionDoNotStartAutomatically = summary.DoNotStartAutomatically,
            GenericProjectionEvidencePath = summary.EvidencePath,
            GenericProjectionExportPath = summary.ExportPath,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static GenericGamePackageProjectionWorkspaceSummary
        LoadGenericGamePackageProjectionSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + GenericGamePackageProjectionVocabulary.DashboardFileName,
            diagnostics);
        return new GenericGamePackageProjectionWorkspaceSummary(
            GenericProjectionStatus: Goal123String(dashboard?.RootElement, "genericProjectionStatus"),
            SamplePackagePath: Goal123String(dashboard?.RootElement, "samplePackagePath"),
            PackageId: Goal123String(dashboard?.RootElement, "packageId"),
            PackageTitle: Goal123String(dashboard?.RootElement, "packageTitle"),
            MapId: Goal123String(dashboard?.RootElement, "mapId"),
            MapSize: Goal123String(dashboard?.RootElement, "mapSize"),
            EntityCount: Goal123Int(dashboard?.RootElement, "entityCount"),
            ItemCount: Goal123Int(dashboard?.RootElement, "itemCount"),
            UnitySmokeStatus: Goal123String(dashboard?.RootElement, "unitySmokeStatus"),
            Goal122StillGreen:
                dashboard is not null && TryGetBool(dashboard.RootElement, "goal122StillGreen"),
            CleanupScriptAvailable:
                dashboard is not null && TryGetBool(dashboard.RootElement, "cleanupScriptAvailable"),
            DoNotStartAutomatically:
                dashboard is not null && TryGetBool(dashboard.RootElement, "doNotStartAutomatically"),
            EvidencePath: Goal123String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal123String(dashboard?.RootElement, "exportPath"),
            QualityGatePassed:
                Goal123String(dashboard?.RootElement, "genericProjectionStatus") == "GREEN",
            RelativePaths: Goal123AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal123FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = GenericGamePackageProjectionVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = GenericGamePackageProjectionVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal123 generic projection file exists" : "Goal123 file missing",
            SafeRatingMetadataSummary = "projectionOnly=true; noManualInput=true"
        };
    }

    private static bool Goal123AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory,
            GenericGamePackageProjectionVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal123String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private static int Goal123Int(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private sealed record GenericGamePackageProjectionWorkspaceSummary(
        string GenericProjectionStatus,
        string SamplePackagePath,
        string PackageId,
        string PackageTitle,
        string MapId,
        string MapSize,
        int EntityCount,
        int ItemCount,
        string UnitySmokeStatus,
        bool Goal122StillGreen,
        bool CleanupScriptAvailable,
        bool DoNotStartAutomatically,
        string EvidencePath,
        string ExportPath,
        bool QualityGatePassed,
        bool RelativePaths);
}
