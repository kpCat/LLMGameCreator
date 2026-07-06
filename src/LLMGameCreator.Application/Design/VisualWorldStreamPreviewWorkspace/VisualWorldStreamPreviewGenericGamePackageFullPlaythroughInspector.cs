using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal126CleanupCommand =
        ".\\.devflow\\scripts\\clean-unity-editor-noise.cmd";

    private static VisualWorldPreviewArtifactGroup BuildGenericGamePackageFullPlaythroughGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadGenericGamePackageFullPlaythroughSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory,
                GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId,
                BuildGoal126ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithGenericGamePackageFullPlaythroughSummary(entry, summary))
            .ToList();

        foreach (var fileName in GenericGamePackageFullPlaythroughProjectionVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithGenericGamePackageFullPlaythroughSummary(
                Goal126FileEntry(
                    projectRoot,
                    GenericGamePackageFullPlaythroughProjectionVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "generic_gamepackage_full_playthrough_export_file"),
                summary));
        }

        entries.Add(WithGenericGamePackageFullPlaythroughSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId + ".summary",
                RelativePath =
                    GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName,
                ArtifactKind = "generic_gamepackage_full_playthrough_workspace_summary",
                SourceGoalId = GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "fullPlaythroughStatus=" + summary.FullPlaythroughStatus
                                    + "; unitySmokeStatus=" + summary.UnitySmokeStatus,
                SafeRatingMetadataSummary =
                    "projectionOnly=true; samplePackageReadOnly=true; cleanupCommand="
                    + Goal126CleanupCommand
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "generic_gamepackage_full_playthrough",
            "Goal 126 Generic GamePackage Full Playthrough",
            GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId,
            GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal126ProceduralFiles() =>
    [
        (GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName,
            "generic_gamepackage_full_playthrough_dashboard"),
        (GenericGamePackageFullPlaythroughProjectionVocabulary.ScriptInventoryFileName,
            "generic_gamepackage_full_playthrough_script_inventory"),
        (GenericGamePackageFullPlaythroughProjectionVocabulary.SmokePlanFileName,
            "generic_gamepackage_full_playthrough_smoke_plan"),
        (GenericGamePackageFullPlaythroughProjectionVocabulary.LogScanFileName,
            "generic_gamepackage_full_playthrough_log_scan"),
        (GenericGamePackageFullPlaythroughProjectionVocabulary.ReportFileName,
            "generic_gamepackage_full_playthrough_report"),
        (GenericGamePackageFullPlaythroughProjectionVocabulary.NegativeProofFileName,
            "generic_gamepackage_full_playthrough_negative_proof"),
        (GenericGamePackageFullPlaythroughProjectionVocabulary.FileIndexFileName,
            "generic_gamepackage_full_playthrough_file_index")
    ];

    private static VisualWorldPreviewArtifactEntry WithGenericGamePackageFullPlaythroughSummary(
        VisualWorldPreviewArtifactEntry entry,
        GenericGamePackageFullPlaythroughWorkspaceSummary summary) =>
        entry with
        {
            GenericFullPlaythroughStatus = summary.FullPlaythroughStatus,
            GenericFullPlaythroughSamplePackagePath = summary.SamplePackagePath,
            GenericFullPlaythroughPackageId = summary.PackageId,
            GenericFullPlaythroughMapId = summary.MapId,
            GenericFullPlaythroughMapPathPreviewPresent = summary.MapPathPreviewPresent,
            GenericFullPlaythroughSignInteractionApplied = summary.SignInteractionApplied,
            GenericFullPlaythroughDialogueSummaryPresent = summary.DialogueSummaryPresent,
            GenericFullPlaythroughQuestObjectiveStatusPresent = summary.QuestObjectiveStatusPresent,
            GenericFullPlaythroughInventorySummaryPresent = summary.InventorySummaryPresent,
            GenericFullPlaythroughResourceSummaryPresent = summary.ResourceSummaryPresent,
            GenericFullPlaythroughSystemsSummaryPresent = summary.SystemsSummaryPresent,
            GenericFullPlaythroughRecipeApplyPassed = summary.RecipeApplyPassed,
            GenericFullPlaythroughHarvestApplyPassed = summary.HarvestApplyPassed,
            GenericFullPlaythroughTransactionPreviewPresent = summary.TransactionPreviewPresent,
            GenericFullPlaythroughCombatRoundPreviewPresent = summary.CombatRoundPreviewPresent,
            GenericFullPlaythroughEventTranscriptPresent = summary.EventTranscriptPresent,
            GenericFullPlaythroughUnitySmokeStatus = summary.UnitySmokeStatus,
            GenericFullPlaythroughCleanupScriptAvailable = summary.CleanupScriptAvailable,
            GenericFullPlaythroughCleanupCommand = Goal126CleanupCommand,
            GenericFullPlaythroughGoal125StillGreen = summary.Goal125StillGreen,
            GenericFullPlaythroughProjectionOnly = summary.ProjectionOnly,
            GenericFullPlaythroughSamplePackageReadOnly = summary.SamplePackageReadOnly,
            GenericFullPlaythroughEvidencePath = summary.EvidencePath,
            GenericFullPlaythroughExportPath = summary.ExportPath,
            GenericFullPlaythroughNoRuntimeProviderSchemaLuaGeneratorLibrary =
                summary.NoRuntimeProviderSchemaLuaGeneratorLibrary,
            GenericFullPlaythroughNoUnityScenePrefabSettingsPackagesStreamingAssets =
                summary.NoUnityScenePrefabSettingsPackagesStreamingAssets,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static GenericGamePackageFullPlaythroughWorkspaceSummary
        LoadGenericGamePackageFullPlaythroughSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + GenericGamePackageFullPlaythroughProjectionVocabulary.DashboardFileName,
            diagnostics);
        return new GenericGamePackageFullPlaythroughWorkspaceSummary(
            FullPlaythroughStatus: Goal126String(dashboard?.RootElement, "fullPlaythroughStatus"),
            SamplePackagePath: Goal126String(dashboard?.RootElement, "samplePackagePath"),
            PackageId: Goal126String(dashboard?.RootElement, "packageId"),
            MapId: Goal126String(dashboard?.RootElement, "mapId"),
            MapPathPreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "mapPathPreviewPresent"),
            SignInteractionApplied:
                dashboard is not null && TryGetBool(dashboard.RootElement, "signInteractionApplied"),
            DialogueSummaryPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "dialogueSummaryPresent"),
            QuestObjectiveStatusPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "questObjectiveStatusPresent"),
            InventorySummaryPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "inventorySummaryPresent"),
            ResourceSummaryPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "resourceSummaryPresent"),
            SystemsSummaryPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "systemsSummaryPresent"),
            RecipeApplyPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "recipeApplyPassed"),
            HarvestApplyPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "harvestApplyPassed"),
            TransactionPreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "transactionPreviewPresent"),
            CombatRoundPreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "combatRoundPreviewPresent"),
            EventTranscriptPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "eventTranscriptPresent"),
            UnitySmokeStatus: Goal126String(dashboard?.RootElement, "unitySmokeStatus"),
            CleanupScriptAvailable:
                dashboard is not null && TryGetBool(dashboard.RootElement, "cleanupScriptAvailable"),
            Goal125StillGreen:
                dashboard is not null && TryGetBool(dashboard.RootElement, "goal125StillGreen"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            SamplePackageReadOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "samplePackageReadOnly"),
            EvidencePath: Goal126String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal126String(dashboard?.RootElement, "exportPath"),
            NoRuntimeProviderSchemaLuaGeneratorLibrary:
                dashboard is not null
                && TryGetBool(dashboard.RootElement, "noRuntimeProviderSchemaLuaGeneratorLibrary"),
            NoUnityScenePrefabSettingsPackagesStreamingAssets:
                dashboard is not null
                && TryGetBool(dashboard.RootElement, "noUnityScenePrefabSettingsPackagesStreamingAssets"),
            QualityGatePassed:
                Goal126String(dashboard?.RootElement, "fullPlaythroughStatus") == "GREEN",
            RelativePaths: Goal126AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal126FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = GenericGamePackageFullPlaythroughProjectionVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal126 generic full playthrough file exists" : "Goal126 file missing",
            SafeRatingMetadataSummary = "projectionOnly=true; noManualInput=true"
        };
    }

    private static bool Goal126AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory,
            GenericGamePackageFullPlaythroughProjectionVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal126String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record GenericGamePackageFullPlaythroughWorkspaceSummary(
        string FullPlaythroughStatus,
        string SamplePackagePath,
        string PackageId,
        string MapId,
        bool MapPathPreviewPresent,
        bool SignInteractionApplied,
        bool DialogueSummaryPresent,
        bool QuestObjectiveStatusPresent,
        bool InventorySummaryPresent,
        bool ResourceSummaryPresent,
        bool SystemsSummaryPresent,
        bool RecipeApplyPassed,
        bool HarvestApplyPassed,
        bool TransactionPreviewPresent,
        bool CombatRoundPreviewPresent,
        bool EventTranscriptPresent,
        string UnitySmokeStatus,
        bool CleanupScriptAvailable,
        bool Goal125StillGreen,
        bool ProjectionOnly,
        bool SamplePackageReadOnly,
        string EvidencePath,
        string ExportPath,
        bool NoRuntimeProviderSchemaLuaGeneratorLibrary,
        bool NoUnityScenePrefabSettingsPackagesStreamingAssets,
        bool QualityGatePassed,
        bool RelativePaths);
}
