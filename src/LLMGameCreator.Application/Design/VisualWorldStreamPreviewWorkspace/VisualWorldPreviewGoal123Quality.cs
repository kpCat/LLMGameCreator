using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal123GenericGamePackageProjectionQuality
        BuildGoal123GenericGamePackageProjectionQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "generic_gamepackage_projection");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "generic_gamepackage_projection_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal123AllowedPath(entry.RelativePath));
        return new Goal123GenericGamePackageProjectionQuality(
            GroupPresent: group is not null,
            GenericProjectionStatus: summary?.GenericProjectionStatus ?? string.Empty,
            SamplePackagePath: summary?.GenericProjectionSamplePackagePath ?? string.Empty,
            PackageId: summary?.GenericProjectionPackageId ?? string.Empty,
            PackageTitle: summary?.GenericProjectionPackageTitle ?? string.Empty,
            MapId: summary?.GenericProjectionMapId ?? string.Empty,
            MapSize: summary?.GenericProjectionMapSize ?? string.Empty,
            EntityCount: summary?.GenericProjectionEntityCount ?? 0,
            ItemCount: summary?.GenericProjectionItemCount ?? 0,
            UnitySmokeStatus: summary?.GenericProjectionUnitySmokeStatus ?? string.Empty,
            Goal122StillGreen: summary?.GenericProjectionGoal122StillGreen == true,
            CleanupScriptAvailable: summary?.GenericProjectionCleanupScriptAvailable == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal123.generic_projection.sample_package" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal123.generic_projection.goal122_still_green" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal123.generic_projection.cleanup_script" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal123.generic_projection.script_inventory" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal123.generic_projection.negative_proof" && proof.Passed)
                && summary?.GenericProjectionStatus == "GREEN"
                && summary?.GenericProjectionPackageId == "game/minimal-map-game"
                && summary?.GenericProjectionMapSize == "12x8",
            RelativePaths: relativePaths);
    }

    private static void AddGoal123GenericGamePackageProjectionQualityDiagnostics(
        Goal123GenericGamePackageProjectionQuality genericProjection,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(genericProjection.GroupPresent, "goal123.quality.generic_projection_group",
            "generic_gamepackage_projection", diagnostics);
        AddIfFalse(genericProjection.GenericProjectionStatus == "GREEN",
            "goal123.quality.generic_projection_status",
            "generic_gamepackage_projection",
            diagnostics);
        AddIfFalse(
            genericProjection.SamplePackagePath == GenericGamePackageProjectionVocabulary.SamplePackagePath,
            "goal123.quality.sample_package_path",
            "generic_gamepackage_projection",
            diagnostics);
        AddIfFalse(genericProjection.PackageId == "game/minimal-map-game",
            "goal123.quality.package_id",
            "generic_gamepackage_projection",
            diagnostics);
        AddIfFalse(genericProjection.MapSize == "12x8",
            "goal123.quality.map_size",
            "generic_gamepackage_projection",
            diagnostics);
        AddIfFalse(genericProjection.EntityCount >= 2,
            "goal123.quality.entity_count",
            "generic_gamepackage_projection",
            diagnostics);
        AddIfFalse(genericProjection.ItemCount >= 1,
            "goal123.quality.item_count",
            "generic_gamepackage_projection",
            diagnostics);
        AddIfFalse(genericProjection.Goal122StillGreen,
            "goal123.quality.goal122_still_green",
            "generic_gamepackage_projection",
            diagnostics);
        AddIfFalse(genericProjection.CleanupScriptAvailable,
            "goal123.quality.cleanup_script",
            "generic_gamepackage_projection",
            diagnostics);
        AddIfFalse(genericProjection.QualityGatePassed,
            "goal123.quality.quality_gate",
            "generic_gamepackage_projection",
            diagnostics);
        AddIfFalse(genericProjection.RelativePaths,
            "goal123.quality.relative_paths",
            "generic_gamepackage_projection",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysGenericGamePackageProjection,
            "goal123.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal123AllowedPath(string path) =>
        path.StartsWith(
            GenericGamePackageProjectionVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            GenericGamePackageProjectionVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal123GenericGamePackageProjectionQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal123GenericGamePackageProjectionQuality genericProjection,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            GenericGamePackageProjectionGroupPresent = genericProjection.GroupPresent,
            GenericProjectionStatus = genericProjection.GenericProjectionStatus,
            GenericProjectionSamplePackagePath = genericProjection.SamplePackagePath,
            GenericProjectionPackageId = genericProjection.PackageId,
            GenericProjectionPackageTitle = genericProjection.PackageTitle,
            GenericProjectionMapId = genericProjection.MapId,
            GenericProjectionMapSize = genericProjection.MapSize,
            GenericProjectionEntityCount = genericProjection.EntityCount,
            GenericProjectionItemCount = genericProjection.ItemCount,
            GenericProjectionUnitySmokeStatus = genericProjection.UnitySmokeStatus,
            GenericProjectionGoal122StillGreen = genericProjection.Goal122StillGreen,
            GenericProjectionCleanupScriptAvailable = genericProjection.CleanupScriptAvailable,
            GenericGamePackageProjectionQualityGatePassed = genericProjection.QualityGatePassed,
            Goal123FilesDiscoveredByRelativePaths = genericProjection.RelativePaths,
            WinFormsGenericGamePackageProjectionBindingReal =
                binding.PageBindDisplaysGenericGamePackageProjection
        };

    private sealed record Goal123GenericGamePackageProjectionQuality(
        bool GroupPresent,
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
        bool QualityGatePassed,
        bool RelativePaths);
}
