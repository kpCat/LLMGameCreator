using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal126GenericGamePackageFullPlaythroughQuality
        BuildGoal126GenericGamePackageFullPlaythroughQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "generic_gamepackage_full_playthrough");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "generic_gamepackage_full_playthrough_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal126AllowedPath(entry.RelativePath));
        return new Goal126GenericGamePackageFullPlaythroughQuality(
            GroupPresent: group is not null,
            FullPlaythroughStatus: summary?.GenericFullPlaythroughStatus ?? string.Empty,
            SamplePackagePath: summary?.GenericFullPlaythroughSamplePackagePath ?? string.Empty,
            PackageId: summary?.GenericFullPlaythroughPackageId ?? string.Empty,
            MapId: summary?.GenericFullPlaythroughMapId ?? string.Empty,
            MapPathPreviewPresent: summary?.GenericFullPlaythroughMapPathPreviewPresent == true,
            SignInteractionApplied: summary?.GenericFullPlaythroughSignInteractionApplied == true,
            DialogueSummaryPresent: summary?.GenericFullPlaythroughDialogueSummaryPresent == true,
            QuestObjectiveStatusPresent:
                summary?.GenericFullPlaythroughQuestObjectiveStatusPresent == true,
            InventorySummaryPresent: summary?.GenericFullPlaythroughInventorySummaryPresent == true,
            ResourceSummaryPresent: summary?.GenericFullPlaythroughResourceSummaryPresent == true,
            SystemsSummaryPresent: summary?.GenericFullPlaythroughSystemsSummaryPresent == true,
            RecipeApplyPassed: summary?.GenericFullPlaythroughRecipeApplyPassed == true,
            HarvestApplyPassed: summary?.GenericFullPlaythroughHarvestApplyPassed == true,
            TransactionPreviewPresent:
                summary?.GenericFullPlaythroughTransactionPreviewPresent == true,
            CombatRoundPreviewPresent:
                summary?.GenericFullPlaythroughCombatRoundPreviewPresent == true,
            EventTranscriptPresent: summary?.GenericFullPlaythroughEventTranscriptPresent == true,
            UnitySmokeStatus: summary?.GenericFullPlaythroughUnitySmokeStatus ?? string.Empty,
            CleanupScriptAvailable: summary?.GenericFullPlaythroughCleanupScriptAvailable == true,
            CleanupCommand: summary?.GenericFullPlaythroughCleanupCommand ?? string.Empty,
            Goal125StillGreen: summary?.GenericFullPlaythroughGoal125StillGreen == true,
            ProjectionOnly: summary?.GenericFullPlaythroughProjectionOnly == true,
            SamplePackageReadOnly: summary?.GenericFullPlaythroughSamplePackageReadOnly == true,
            EvidencePath: summary?.GenericFullPlaythroughEvidencePath ?? string.Empty,
            ExportPath: summary?.GenericFullPlaythroughExportPath ?? string.Empty,
            NoRuntimeProviderSchemaLuaGeneratorLibrary:
                summary?.GenericFullPlaythroughNoRuntimeProviderSchemaLuaGeneratorLibrary == true,
            NoUnityScenePrefabSettingsPackagesStreamingAssets:
                summary?.GenericFullPlaythroughNoUnityScenePrefabSettingsPackagesStreamingAssets == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal126.generic_full_playthrough.sample_package" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal126.generic_full_playthrough.goal125_still_green" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal126.generic_full_playthrough.cleanup_script" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal126.generic_full_playthrough.script_inventory" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal126.generic_full_playthrough.negative_proof" && proof.Passed)
                && summary?.GenericFullPlaythroughStatus == "GREEN"
                && summary?.GenericFullPlaythroughPackageId == "game/minimal-map-game"
                && summary?.GenericFullPlaythroughMapId == "map/village"
                && summary?.GenericFullPlaythroughMapPathPreviewPresent == true
                && summary?.GenericFullPlaythroughSignInteractionApplied == true
                && summary?.GenericFullPlaythroughDialogueSummaryPresent == true
                && summary?.GenericFullPlaythroughQuestObjectiveStatusPresent == true
                && summary?.GenericFullPlaythroughInventorySummaryPresent == true
                && summary?.GenericFullPlaythroughResourceSummaryPresent == true
                && summary?.GenericFullPlaythroughSystemsSummaryPresent == true
                && summary?.GenericFullPlaythroughCombatRoundPreviewPresent == true
                && summary?.GenericFullPlaythroughEventTranscriptPresent == true
                && summary?.GenericFullPlaythroughGoal125StillGreen == true
                && summary?.GenericFullPlaythroughProjectionOnly == true
                && summary?.GenericFullPlaythroughSamplePackageReadOnly == true
                && summary?.GenericFullPlaythroughNoRuntimeProviderSchemaLuaGeneratorLibrary == true
                && summary?.GenericFullPlaythroughNoUnityScenePrefabSettingsPackagesStreamingAssets == true,
            RelativePaths: relativePaths);
    }

    private static void AddGoal126GenericGamePackageFullPlaythroughQualityDiagnostics(
        Goal126GenericGamePackageFullPlaythroughQuality fullPlaythrough,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(fullPlaythrough.GroupPresent, "goal126.quality.full_playthrough_group",
            "generic_gamepackage_full_playthrough", diagnostics);
        AddIfFalse(fullPlaythrough.FullPlaythroughStatus == "GREEN",
            "goal126.quality.full_playthrough_status",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(
            fullPlaythrough.SamplePackagePath
            == GenericGamePackageFullPlaythroughProjectionVocabulary.SamplePackagePath,
            "goal126.quality.sample_package_path",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.PackageId == "game/minimal-map-game",
            "goal126.quality.package_id",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.MapId == "map/village",
            "goal126.quality.map_id",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.MapPathPreviewPresent,
            "goal126.quality.map_path_preview",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.SignInteractionApplied,
            "goal126.quality.sign_interaction",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.DialogueSummaryPresent,
            "goal126.quality.dialogue_summary",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.QuestObjectiveStatusPresent,
            "goal126.quality.quest_objective",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.InventorySummaryPresent,
            "goal126.quality.inventory_summary",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.ResourceSummaryPresent,
            "goal126.quality.resource_summary",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.SystemsSummaryPresent,
            "goal126.quality.systems_summary",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.RecipeApplyPassed,
            "goal126.quality.recipe_apply",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.HarvestApplyPassed,
            "goal126.quality.harvest_apply",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.TransactionPreviewPresent,
            "goal126.quality.transaction_preview",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.CombatRoundPreviewPresent,
            "goal126.quality.combat_round",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.EventTranscriptPresent,
            "goal126.quality.event_transcript",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.Goal125StillGreen,
            "goal126.quality.goal125_still_green",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.SamplePackageReadOnly,
            "goal126.quality.sample_package_read_only",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.CleanupScriptAvailable,
            "goal126.quality.cleanup_script",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.ProjectionOnly,
            "goal126.quality.projection_only",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.NoRuntimeProviderSchemaLuaGeneratorLibrary,
            "goal126.quality.forbidden_runtime_provider_schema_lua_generator",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.NoUnityScenePrefabSettingsPackagesStreamingAssets,
            "goal126.quality.forbidden_unity_payload",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.QualityGatePassed,
            "goal126.quality.quality_gate",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(fullPlaythrough.RelativePaths,
            "goal126.quality.relative_paths",
            "generic_gamepackage_full_playthrough",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysGenericGamePackageFullPlaythrough,
            "goal126.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal126AllowedPath(string path) =>
        path.StartsWith(
            GenericGamePackageFullPlaythroughProjectionVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            GenericGamePackageFullPlaythroughProjectionVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal126GenericGamePackageFullPlaythroughQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal126GenericGamePackageFullPlaythroughQuality fullPlaythrough,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            GenericGamePackageFullPlaythroughGroupPresent = fullPlaythrough.GroupPresent,
            GenericFullPlaythroughStatus = fullPlaythrough.FullPlaythroughStatus,
            GenericFullPlaythroughSamplePackagePath = fullPlaythrough.SamplePackagePath,
            GenericFullPlaythroughPackageId = fullPlaythrough.PackageId,
            GenericFullPlaythroughMapId = fullPlaythrough.MapId,
            GenericFullPlaythroughMapPathPreviewPresent = fullPlaythrough.MapPathPreviewPresent,
            GenericFullPlaythroughSignInteractionApplied = fullPlaythrough.SignInteractionApplied,
            GenericFullPlaythroughDialogueSummaryPresent = fullPlaythrough.DialogueSummaryPresent,
            GenericFullPlaythroughQuestObjectiveStatusPresent =
                fullPlaythrough.QuestObjectiveStatusPresent,
            GenericFullPlaythroughInventorySummaryPresent = fullPlaythrough.InventorySummaryPresent,
            GenericFullPlaythroughResourceSummaryPresent = fullPlaythrough.ResourceSummaryPresent,
            GenericFullPlaythroughSystemsSummaryPresent = fullPlaythrough.SystemsSummaryPresent,
            GenericFullPlaythroughRecipeApplyPassed = fullPlaythrough.RecipeApplyPassed,
            GenericFullPlaythroughHarvestApplyPassed = fullPlaythrough.HarvestApplyPassed,
            GenericFullPlaythroughTransactionPreviewPresent =
                fullPlaythrough.TransactionPreviewPresent,
            GenericFullPlaythroughCombatRoundPreviewPresent =
                fullPlaythrough.CombatRoundPreviewPresent,
            GenericFullPlaythroughEventTranscriptPresent = fullPlaythrough.EventTranscriptPresent,
            GenericFullPlaythroughUnitySmokeStatus = fullPlaythrough.UnitySmokeStatus,
            GenericFullPlaythroughCleanupScriptAvailable = fullPlaythrough.CleanupScriptAvailable,
            GenericFullPlaythroughCleanupCommand = fullPlaythrough.CleanupCommand,
            GenericFullPlaythroughGoal125StillGreen = fullPlaythrough.Goal125StillGreen,
            GenericFullPlaythroughProjectionOnly = fullPlaythrough.ProjectionOnly,
            GenericFullPlaythroughSamplePackageReadOnly = fullPlaythrough.SamplePackageReadOnly,
            GenericFullPlaythroughEvidencePath = fullPlaythrough.EvidencePath,
            GenericFullPlaythroughExportPath = fullPlaythrough.ExportPath,
            GenericFullPlaythroughNoRuntimeProviderSchemaLuaGeneratorLibrary =
                fullPlaythrough.NoRuntimeProviderSchemaLuaGeneratorLibrary,
            GenericFullPlaythroughNoUnityScenePrefabSettingsPackagesStreamingAssets =
                fullPlaythrough.NoUnityScenePrefabSettingsPackagesStreamingAssets,
            GenericGamePackageFullPlaythroughQualityGatePassed =
                fullPlaythrough.QualityGatePassed,
            Goal126FilesDiscoveredByRelativePaths = fullPlaythrough.RelativePaths,
            WinFormsGenericGamePackageFullPlaythroughBindingReal =
                binding.PageBindDisplaysGenericGamePackageFullPlaythrough
        };

    private sealed record Goal126GenericGamePackageFullPlaythroughQuality(
        bool GroupPresent,
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
        string CleanupCommand,
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
