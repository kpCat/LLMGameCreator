using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal125GenericGamePackageSystemsQuality
        BuildGoal125GenericGamePackageSystemsQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "generic_gamepackage_systems_loop");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "generic_gamepackage_systems_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal125AllowedPath(entry.RelativePath));
        return new Goal125GenericGamePackageSystemsQuality(
            GroupPresent: group is not null,
            GenericSystemsStatus: summary?.GenericSystemsStatus ?? string.Empty,
            SamplePackagePath: summary?.GenericSystemsSamplePackagePath ?? string.Empty,
            PackageId: summary?.GenericSystemsPackageId ?? string.Empty,
            RecipePreviewPresent: summary?.GenericSystemsRecipePreviewPresent == true,
            RecipeApplyPassed: summary?.GenericSystemsRecipeApplyPassed == true,
            HarvestPreviewPresent: summary?.GenericSystemsHarvestPreviewPresent == true,
            HarvestApplyPassed: summary?.GenericSystemsHarvestApplyPassed == true,
            TransactionPreviewPresent: summary?.GenericSystemsTransactionPreviewPresent == true,
            EncounterPreviewPresent: summary?.GenericSystemsEncounterPreviewPresent == true,
            CombatRoundPreviewPresent: summary?.GenericSystemsCombatRoundPreviewPresent == true,
            InventorySummaryPresent: summary?.GenericSystemsInventorySummaryPresent == true,
            ResourceSummaryPresent: summary?.GenericSystemsResourceSummaryPresent == true,
            SystemsEventLogPresent: summary?.GenericSystemsEventLogPresent == true,
            UnitySmokeStatus: summary?.GenericSystemsUnitySmokeStatus ?? string.Empty,
            CleanupScriptAvailable: summary?.GenericSystemsCleanupScriptAvailable == true,
            CleanupCommand: summary?.GenericSystemsCleanupCommand ?? string.Empty,
            Goal124StillGreen: summary?.GenericSystemsGoal124StillGreen == true,
            ProjectionOnly: summary?.GenericSystemsProjectionOnly == true,
            SamplePackageReadOnly: summary?.GenericSystemsSamplePackageReadOnly == true,
            EvidencePath: summary?.GenericSystemsEvidencePath ?? string.Empty,
            ExportPath: summary?.GenericSystemsExportPath ?? string.Empty,
            NoRuntimeProviderSchemaLuaGeneratorLibrary:
                summary?.GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary == true,
            NoUnityScenePrefabSettingsPackagesStreamingAssets:
                summary?.GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal125.generic_systems.sample_package" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal125.generic_systems.goal124_still_green" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal125.generic_systems.cleanup_script" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal125.generic_systems.script_inventory" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal125.generic_systems.negative_proof" && proof.Passed)
                && summary?.GenericSystemsStatus == "GREEN"
                && summary?.GenericSystemsPackageId == "game/minimal-map-game"
                && summary?.GenericSystemsRecipePreviewPresent == true
                && summary?.GenericSystemsRecipeApplyPassed == true
                && summary?.GenericSystemsHarvestPreviewPresent == true
                && summary?.GenericSystemsHarvestApplyPassed == true
                && summary?.GenericSystemsTransactionPreviewPresent == true
                && summary?.GenericSystemsEncounterPreviewPresent == true
                && summary?.GenericSystemsCombatRoundPreviewPresent == true
                && summary?.GenericSystemsInventorySummaryPresent == true
                && summary?.GenericSystemsResourceSummaryPresent == true
                && summary?.GenericSystemsEventLogPresent == true
                && summary?.GenericSystemsGoal124StillGreen == true
                && summary?.GenericSystemsProjectionOnly == true
                && summary?.GenericSystemsSamplePackageReadOnly == true
                && summary?.GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary == true
                && summary?.GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets == true,
            RelativePaths: relativePaths);
    }

    private static void AddGoal125GenericGamePackageSystemsQualityDiagnostics(
        Goal125GenericGamePackageSystemsQuality genericSystems,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(genericSystems.GroupPresent, "goal125.quality.generic_systems_group",
            "generic_gamepackage_systems_loop", diagnostics);
        AddIfFalse(genericSystems.GenericSystemsStatus == "GREEN",
            "goal125.quality.generic_systems_status",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(
            genericSystems.SamplePackagePath == GenericGamePackageSystemsProjectionVocabulary.SamplePackagePath,
            "goal125.quality.sample_package_path",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.PackageId == "game/minimal-map-game",
            "goal125.quality.package_id",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.RecipePreviewPresent,
            "goal125.quality.recipe_preview",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.RecipeApplyPassed,
            "goal125.quality.recipe_apply",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.HarvestPreviewPresent,
            "goal125.quality.harvest_preview",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.HarvestApplyPassed,
            "goal125.quality.harvest_apply",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.TransactionPreviewPresent,
            "goal125.quality.transaction_preview",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.EncounterPreviewPresent,
            "goal125.quality.encounter_preview",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.CombatRoundPreviewPresent,
            "goal125.quality.combat_round_preview",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.InventorySummaryPresent,
            "goal125.quality.inventory_summary",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.ResourceSummaryPresent,
            "goal125.quality.resource_summary",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.SystemsEventLogPresent,
            "goal125.quality.systems_event_log",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.Goal124StillGreen,
            "goal125.quality.goal124_still_green",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.SamplePackageReadOnly,
            "goal125.quality.sample_package_read_only",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.CleanupScriptAvailable,
            "goal125.quality.cleanup_script",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.ProjectionOnly,
            "goal125.quality.projection_only",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.NoRuntimeProviderSchemaLuaGeneratorLibrary,
            "goal125.quality.forbidden_runtime_provider_schema_lua_generator",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.NoUnityScenePrefabSettingsPackagesStreamingAssets,
            "goal125.quality.forbidden_unity_payload",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.QualityGatePassed,
            "goal125.quality.quality_gate",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(genericSystems.RelativePaths,
            "goal125.quality.relative_paths",
            "generic_gamepackage_systems_loop",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysGenericGamePackageSystems,
            "goal125.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal125AllowedPath(string path) =>
        path.StartsWith(
            GenericGamePackageSystemsProjectionVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            GenericGamePackageSystemsProjectionVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal125GenericGamePackageSystemsQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal125GenericGamePackageSystemsQuality genericSystems,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            GenericGamePackageSystemsGroupPresent = genericSystems.GroupPresent,
            GenericSystemsStatus = genericSystems.GenericSystemsStatus,
            GenericSystemsSamplePackagePath = genericSystems.SamplePackagePath,
            GenericSystemsPackageId = genericSystems.PackageId,
            GenericSystemsRecipePreviewPresent = genericSystems.RecipePreviewPresent,
            GenericSystemsRecipeApplyPassed = genericSystems.RecipeApplyPassed,
            GenericSystemsHarvestPreviewPresent = genericSystems.HarvestPreviewPresent,
            GenericSystemsHarvestApplyPassed = genericSystems.HarvestApplyPassed,
            GenericSystemsTransactionPreviewPresent = genericSystems.TransactionPreviewPresent,
            GenericSystemsEncounterPreviewPresent = genericSystems.EncounterPreviewPresent,
            GenericSystemsCombatRoundPreviewPresent = genericSystems.CombatRoundPreviewPresent,
            GenericSystemsInventorySummaryPresent = genericSystems.InventorySummaryPresent,
            GenericSystemsResourceSummaryPresent = genericSystems.ResourceSummaryPresent,
            GenericSystemsEventLogPresent = genericSystems.SystemsEventLogPresent,
            GenericSystemsUnitySmokeStatus = genericSystems.UnitySmokeStatus,
            GenericSystemsCleanupScriptAvailable = genericSystems.CleanupScriptAvailable,
            GenericSystemsCleanupCommand = genericSystems.CleanupCommand,
            GenericSystemsGoal124StillGreen = genericSystems.Goal124StillGreen,
            GenericSystemsProjectionOnly = genericSystems.ProjectionOnly,
            GenericSystemsSamplePackageReadOnly = genericSystems.SamplePackageReadOnly,
            GenericSystemsEvidencePath = genericSystems.EvidencePath,
            GenericSystemsExportPath = genericSystems.ExportPath,
            GenericSystemsNoRuntimeProviderSchemaLuaGeneratorLibrary =
                genericSystems.NoRuntimeProviderSchemaLuaGeneratorLibrary,
            GenericSystemsNoUnityScenePrefabSettingsPackagesStreamingAssets =
                genericSystems.NoUnityScenePrefabSettingsPackagesStreamingAssets,
            GenericGamePackageSystemsQualityGatePassed = genericSystems.QualityGatePassed,
            Goal125FilesDiscoveredByRelativePaths = genericSystems.RelativePaths,
            WinFormsGenericGamePackageSystemsBindingReal =
                binding.PageBindDisplaysGenericGamePackageSystems
        };

    private sealed record Goal125GenericGamePackageSystemsQuality(
        bool GroupPresent,
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
        string CleanupCommand,
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
