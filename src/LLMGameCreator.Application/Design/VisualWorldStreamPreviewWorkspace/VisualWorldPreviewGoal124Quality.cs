using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal124GenericGamePackageLoopQuality
        BuildGoal124GenericGamePackageLoopQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "generic_gamepackage_loop");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "generic_gamepackage_loop_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal124AllowedPath(entry.RelativePath));
        return new Goal124GenericGamePackageLoopQuality(
            GroupPresent: group is not null,
            GenericLoopStatus: summary?.GenericLoopStatus ?? string.Empty,
            SamplePackagePath: summary?.GenericLoopSamplePackagePath ?? string.Empty,
            PackageId: summary?.GenericLoopPackageId ?? string.Empty,
            MapId: summary?.GenericLoopMapId ?? string.Empty,
            InteractionPreviewPresent: summary?.GenericLoopInteractionPreviewPresent == true,
            InteractionApplyPassed: summary?.GenericLoopInteractionApplyPassed == true,
            DialogueSummaryPresent: summary?.GenericLoopDialogueSummaryPresent == true,
            QuestObjectiveSummaryPresent: summary?.GenericLoopQuestObjectiveSummaryPresent == true,
            InventorySummaryPresent: summary?.GenericLoopInventorySummaryPresent == true,
            ResourceSummaryPresent: summary?.GenericLoopResourceSummaryPresent == true,
            UnitySmokeStatus: summary?.GenericLoopUnitySmokeStatus ?? string.Empty,
            CleanupScriptAvailable: summary?.GenericLoopCleanupScriptAvailable == true,
            CleanupCommand: summary?.GenericLoopCleanupCommand ?? string.Empty,
            Goal123StillGreen: summary?.GenericLoopGoal123StillGreen == true,
            ProjectionOnly: summary?.GenericLoopProjectionOnly == true,
            AppliedInteractionCount: summary?.GenericLoopAppliedInteractionCount ?? 0,
            StartedQuestCount: summary?.GenericLoopStartedQuestCount ?? 0,
            EvidencePath: summary?.GenericLoopEvidencePath ?? string.Empty,
            ExportPath: summary?.GenericLoopExportPath ?? string.Empty,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal124.generic_loop.sample_package" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal124.generic_loop.goal123_still_green" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal124.generic_loop.cleanup_script" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal124.generic_loop.script_inventory" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal124.generic_loop.negative_proof" && proof.Passed)
                && summary?.GenericLoopStatus == "GREEN"
                && summary?.GenericLoopPackageId == "game/minimal-map-game"
                && summary?.GenericLoopMapId == "map/village"
                && summary?.GenericLoopInteractionPreviewPresent == true
                && summary?.GenericLoopInteractionApplyPassed == true
                && summary?.GenericLoopDialogueSummaryPresent == true
                && summary?.GenericLoopQuestObjectiveSummaryPresent == true
                && summary?.GenericLoopInventorySummaryPresent == true
                && summary?.GenericLoopResourceSummaryPresent == true
                && summary?.GenericLoopGoal123StillGreen == true
                && summary?.GenericLoopProjectionOnly == true,
            RelativePaths: relativePaths);
    }

    private static void AddGoal124GenericGamePackageLoopQualityDiagnostics(
        Goal124GenericGamePackageLoopQuality genericLoop,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(genericLoop.GroupPresent, "goal124.quality.generic_loop_group",
            "generic_gamepackage_loop", diagnostics);
        AddIfFalse(genericLoop.GenericLoopStatus == "GREEN",
            "goal124.quality.generic_loop_status",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(
            genericLoop.SamplePackagePath == GenericGamePackageLoopProjectionVocabulary.SamplePackagePath,
            "goal124.quality.sample_package_path",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.PackageId == "game/minimal-map-game",
            "goal124.quality.package_id",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.MapId == "map/village",
            "goal124.quality.map_id",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.InteractionPreviewPresent,
            "goal124.quality.interaction_preview",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.InteractionApplyPassed,
            "goal124.quality.interaction_apply",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.DialogueSummaryPresent,
            "goal124.quality.dialogue_summary",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.QuestObjectiveSummaryPresent,
            "goal124.quality.quest_objective",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.InventorySummaryPresent,
            "goal124.quality.inventory_summary",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.ResourceSummaryPresent,
            "goal124.quality.resource_summary",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.Goal123StillGreen,
            "goal124.quality.goal123_still_green",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.CleanupScriptAvailable,
            "goal124.quality.cleanup_script",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.ProjectionOnly,
            "goal124.quality.projection_only",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.QualityGatePassed,
            "goal124.quality.quality_gate",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(genericLoop.RelativePaths,
            "goal124.quality.relative_paths",
            "generic_gamepackage_loop",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysGenericGamePackageLoop,
            "goal124.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal124AllowedPath(string path) =>
        path.StartsWith(
            GenericGamePackageLoopProjectionVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            GenericGamePackageLoopProjectionVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal124GenericGamePackageLoopQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal124GenericGamePackageLoopQuality genericLoop,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            GenericGamePackageLoopGroupPresent = genericLoop.GroupPresent,
            GenericLoopStatus = genericLoop.GenericLoopStatus,
            GenericLoopSamplePackagePath = genericLoop.SamplePackagePath,
            GenericLoopPackageId = genericLoop.PackageId,
            GenericLoopMapId = genericLoop.MapId,
            GenericLoopInteractionPreviewPresent = genericLoop.InteractionPreviewPresent,
            GenericLoopInteractionApplyPassed = genericLoop.InteractionApplyPassed,
            GenericLoopDialogueSummaryPresent = genericLoop.DialogueSummaryPresent,
            GenericLoopQuestObjectiveSummaryPresent = genericLoop.QuestObjectiveSummaryPresent,
            GenericLoopInventorySummaryPresent = genericLoop.InventorySummaryPresent,
            GenericLoopResourceSummaryPresent = genericLoop.ResourceSummaryPresent,
            GenericLoopUnitySmokeStatus = genericLoop.UnitySmokeStatus,
            GenericLoopCleanupScriptAvailable = genericLoop.CleanupScriptAvailable,
            GenericLoopCleanupCommand = genericLoop.CleanupCommand,
            GenericLoopGoal123StillGreen = genericLoop.Goal123StillGreen,
            GenericLoopProjectionOnly = genericLoop.ProjectionOnly,
            GenericLoopAppliedInteractionCount = genericLoop.AppliedInteractionCount,
            GenericLoopStartedQuestCount = genericLoop.StartedQuestCount,
            GenericLoopEvidencePath = genericLoop.EvidencePath,
            GenericLoopExportPath = genericLoop.ExportPath,
            GenericGamePackageLoopQualityGatePassed = genericLoop.QualityGatePassed,
            Goal124FilesDiscoveredByRelativePaths = genericLoop.RelativePaths,
            WinFormsGenericGamePackageLoopBindingReal =
                binding.PageBindDisplaysGenericGamePackageLoop
        };

    private sealed record Goal124GenericGamePackageLoopQuality(
        bool GroupPresent,
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
        string CleanupCommand,
        bool Goal123StillGreen,
        bool ProjectionOnly,
        int AppliedInteractionCount,
        int StartedQuestCount,
        string EvidencePath,
        string ExportPath,
        bool QualityGatePassed,
        bool RelativePaths);
}
