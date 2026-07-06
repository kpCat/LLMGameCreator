using System.Text.Json;
using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private const string Goal125CleanupCommand =
        ".\\.devflow\\scripts\\clean-unity-editor-noise.cmd";

    private static VisualWorldPreviewArtifactGroup BuildGenericGamePackageSystemsGroup(
        string projectRoot,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var groupDiagnostics = new List<VisualWorldPreviewDiagnostic>();
        var summary = LoadGenericGamePackageSystemsSummary(projectRoot, groupDiagnostics);
        var entries = BuildCoreEntries(
                projectRoot,
                GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory,
                GenericGamePackageSystemsProjectionVocabulary.GoalId,
                BuildGoal125ProceduralFiles(),
                new Dictionary<string, string>(StringComparer.Ordinal),
                groupDiagnostics)
            .Select(entry => WithGenericGamePackageSystemsSummary(entry, summary))
            .ToList();

        foreach (var fileName in GenericGamePackageSystemsProjectionVocabulary.RequiredExportFileNames)
        {
            entries.Add(WithGenericGamePackageSystemsSummary(
                Goal125FileEntry(
                    projectRoot,
                    GenericGamePackageSystemsProjectionVocabulary.ExportPackageDirectory
                    + "/"
                    + fileName,
                    "generic_gamepackage_systems_export_file"),
                summary));
        }

        entries.Add(WithGenericGamePackageSystemsSummary(
            new VisualWorldPreviewArtifactEntry
            {
                Id = GenericGamePackageSystemsProjectionVocabulary.GoalId + ".summary",
                RelativePath =
                    GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + GenericGamePackageSystemsProjectionVocabulary.DashboardFileName,
                ArtifactKind = "generic_gamepackage_systems_workspace_summary",
                SourceGoalId = GenericGamePackageSystemsProjectionVocabulary.GoalId,
                Sha256 = HashFor(
                    projectRoot,
                    GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory
                    + "/"
                    + GenericGamePackageSystemsProjectionVocabulary.DashboardFileName,
                    new Dictionary<string, string>(StringComparer.Ordinal)),
                Status = summary.QualityGatePassed
                    ? VisualWorldPreviewArtifactStatus.Passed
                    : VisualWorldPreviewArtifactStatus.Failed,
                DiagnosticSummary = "genericSystemsStatus=" + summary.GenericSystemsStatus
                                    + "; unitySmokeStatus=" + summary.UnitySmokeStatus,
                SafeRatingMetadataSummary =
                    "projectionOnly=true; samplePackageReadOnly=true; cleanupCommand="
                    + Goal125CleanupCommand
            },
            summary));

        diagnostics.AddRange(groupDiagnostics);
        return Group(
            "generic_gamepackage_systems_loop",
            "Goal 125 Generic GamePackage Systems Loop",
            GenericGamePackageSystemsProjectionVocabulary.GoalId,
            GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory,
            entries,
            groupDiagnostics);
    }

    private static IReadOnlyList<(string FileName, string Kind)> BuildGoal125ProceduralFiles() =>
    [
        (GenericGamePackageSystemsProjectionVocabulary.DashboardFileName,
            "generic_gamepackage_systems_dashboard"),
        (GenericGamePackageSystemsProjectionVocabulary.ScriptInventoryFileName,
            "generic_gamepackage_systems_script_inventory"),
        (GenericGamePackageSystemsProjectionVocabulary.SmokePlanFileName,
            "generic_gamepackage_systems_smoke_plan"),
        (GenericGamePackageSystemsProjectionVocabulary.LogScanFileName,
            "generic_gamepackage_systems_log_scan"),
        (GenericGamePackageSystemsProjectionVocabulary.ReportFileName,
            "generic_gamepackage_systems_report"),
        (GenericGamePackageSystemsProjectionVocabulary.NegativeProofFileName,
            "generic_gamepackage_systems_negative_proof"),
        (GenericGamePackageSystemsProjectionVocabulary.FileIndexFileName,
            "generic_gamepackage_systems_file_index")
    ];

    private static VisualWorldPreviewArtifactEntry WithGenericGamePackageSystemsSummary(
        VisualWorldPreviewArtifactEntry entry,
        GenericGamePackageSystemsWorkspaceSummary summary) =>
        entry with
        {
            GenericSystemsStatus = summary.GenericSystemsStatus,
            GenericSystemsSamplePackagePath = summary.SamplePackagePath,
            GenericSystemsPackageId = summary.PackageId,
            GenericSystemsRecipePreviewPresent = summary.RecipePreviewPresent,
            GenericSystemsRecipeApplyPassed = summary.RecipeApplyPassed,
            GenericSystemsHarvestPreviewPresent = summary.HarvestPreviewPresent,
            GenericSystemsHarvestApplyPassed = summary.HarvestApplyPassed,
            GenericSystemsTransactionPreviewPresent = summary.TransactionPreviewPresent,
            GenericSystemsEncounterPreviewPresent = summary.EncounterPreviewPresent,
            GenericSystemsCombatRoundPreviewPresent = summary.CombatRoundPreviewPresent,
            GenericSystemsInventorySummaryPresent = summary.InventorySummaryPresent,
            GenericSystemsResourceSummaryPresent = summary.ResourceSummaryPresent,
            GenericSystemsEventLogPresent = summary.SystemsEventLogPresent,
            GenericSystemsUnitySmokeStatus = summary.UnitySmokeStatus,
            GenericSystemsCleanupScriptAvailable = summary.CleanupScriptAvailable,
            GenericSystemsCleanupCommand = Goal125CleanupCommand,
            GenericSystemsGoal124StillGreen = summary.Goal124StillGreen,
            GenericSystemsProjectionOnly = summary.ProjectionOnly,
            GenericSystemsSamplePackageReadOnly = summary.SamplePackageReadOnly,
            GenericSystemsEvidencePath = summary.EvidencePath,
            GenericSystemsExportPath = summary.ExportPath,
            GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary =
                summary.NoRuntimeProviderSchemaLuaGeneratorLibrary,
            GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets =
                summary.NoUnityScenePrefabSettingsPackagesStreamingAssets,
            MetadataOnly = true,
            NoRawFullWorldDump = true
        };

    private static GenericGamePackageSystemsWorkspaceSummary
        LoadGenericGamePackageSystemsSummary(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory;
        using var dashboard = TryReadJson(
            projectRoot,
            root + "/" + GenericGamePackageSystemsProjectionVocabulary.DashboardFileName,
            diagnostics);
        return new GenericGamePackageSystemsWorkspaceSummary(
            GenericSystemsStatus: Goal125String(dashboard?.RootElement, "genericSystemsStatus"),
            SamplePackagePath: Goal125String(dashboard?.RootElement, "samplePackagePath"),
            PackageId: Goal125String(dashboard?.RootElement, "packageId"),
            RecipePreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "recipePreviewPresent"),
            RecipeApplyPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "recipeApplyPassed"),
            HarvestPreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "harvestPreviewPresent"),
            HarvestApplyPassed:
                dashboard is not null && TryGetBool(dashboard.RootElement, "harvestApplyPassed"),
            TransactionPreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "transactionPreviewPresent"),
            EncounterPreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "encounterPreviewPresent"),
            CombatRoundPreviewPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "combatRoundPreviewPresent"),
            InventorySummaryPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "inventorySummaryPresent"),
            ResourceSummaryPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "resourceSummaryPresent"),
            SystemsEventLogPresent:
                dashboard is not null && TryGetBool(dashboard.RootElement, "systemsEventLogPresent"),
            UnitySmokeStatus: Goal125String(dashboard?.RootElement, "unitySmokeStatus"),
            CleanupScriptAvailable:
                dashboard is not null && TryGetBool(dashboard.RootElement, "cleanupScriptAvailable"),
            Goal124StillGreen:
                dashboard is not null && TryGetBool(dashboard.RootElement, "goal124StillGreen"),
            ProjectionOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "projectionOnly"),
            SamplePackageReadOnly:
                dashboard is not null && TryGetBool(dashboard.RootElement, "samplePackageReadOnly"),
            EvidencePath: Goal125String(dashboard?.RootElement, "evidencePath"),
            ExportPath: Goal125String(dashboard?.RootElement, "exportPath"),
            NoRuntimeProviderSchemaLuaGeneratorLibrary:
                dashboard is not null
                && TryGetBool(dashboard.RootElement, "noRuntimeProviderSchemaLuaGeneratorLibrary"),
            NoUnityScenePrefabSettingsPackagesStreamingAssets:
                dashboard is not null
                && TryGetBool(dashboard.RootElement, "noUnityScenePrefabSettingsPackagesStreamingAssets"),
            QualityGatePassed:
                Goal125String(dashboard?.RootElement, "genericSystemsStatus") == "GREEN",
            RelativePaths: Goal125AllPathsRelative(projectRoot));
    }

    private static VisualWorldPreviewArtifactEntry Goal125FileEntry(
        string projectRoot,
        string relativePath,
        string kind)
    {
        var exists = File.Exists(Resolve(projectRoot, relativePath));
        return new VisualWorldPreviewArtifactEntry
        {
            Id = GenericGamePackageSystemsProjectionVocabulary.GoalId
                 + ".file."
                 + kind
                 + "."
                 + Path.GetFileNameWithoutExtension(relativePath),
            RelativePath = relativePath,
            ArtifactKind = kind,
            SourceGoalId = GenericGamePackageSystemsProjectionVocabulary.GoalId,
            Sha256 = exists
                ? HashFor(projectRoot, relativePath, new Dictionary<string, string>(StringComparer.Ordinal))
                : string.Empty,
            Status = exists ? VisualWorldPreviewArtifactStatus.Passed : VisualWorldPreviewArtifactStatus.Failed,
            DiagnosticSummary = exists ? "Goal125 generic systems file exists" : "Goal125 file missing",
            SafeRatingMetadataSummary = "projectionOnly=true; noManualInput=true"
        };
    }

    private static bool Goal125AllPathsRelative(string projectRoot)
    {
        var roots = new[]
        {
            GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory,
            GenericGamePackageSystemsProjectionVocabulary.ExportPackageDirectory
        };
        return roots.All(IsSafeRelativePath)
               && roots.All(root =>
                   !Directory.Exists(Resolve(projectRoot, root))
                   || Directory.EnumerateFiles(Resolve(projectRoot, root), "*", SearchOption.TopDirectoryOnly)
                       .Select(path => Relative(projectRoot, path))
                       .All(path => IsSafeRelativePath(path)
                                    && !path.StartsWith(".llmgc/manual/", StringComparison.Ordinal)));
    }

    private static string Goal125String(JsonElement? element, string propertyName) =>
        element is not null
        && element.Value.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private sealed record GenericGamePackageSystemsWorkspaceSummary(
        string GenericSystemsStatus,
        string SamplePackagePath,
        string PackageId,
        bool RecipePreviewPresent,
        bool RecipeApplyPassed,
        bool HarvestPreviewPresent,
        bool HarvestApplyPassed,
        bool TransactionPreviewPresent,
        bool EncounterPreviewPresent,
        bool CombatRoundPreviewPresent,
        bool InventorySummaryPresent,
        bool ResourceSummaryPresent,
        bool SystemsEventLogPresent,
        string UnitySmokeStatus,
        bool CleanupScriptAvailable,
        bool Goal124StillGreen,
        bool ProjectionOnly,
        bool SamplePackageReadOnly,
        string EvidencePath,
        string ExportPath,
        bool NoRuntimeProviderSchemaLuaGeneratorLibrary,
        bool NoUnityScenePrefabSettingsPackagesStreamingAssets,
        bool QualityGatePassed,
        bool RelativePaths);
}
