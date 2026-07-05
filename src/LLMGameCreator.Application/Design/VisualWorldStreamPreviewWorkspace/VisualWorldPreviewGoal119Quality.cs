using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal119AcceptedAlphaUnityPlayableProjectionQuality
        BuildGoal119AcceptedAlphaUnityPlayableProjectionQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "accepted_alpha_unity_playable_projection");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "accepted_alpha_unity_playable_projection_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal119AllowedPath(entry.RelativePath));
        return new Goal119AcceptedAlphaUnityPlayableProjectionQuality(
            GroupPresent: group is not null,
            ProjectionStatus: summary?.AcceptedAlphaUnityPlayableProjectionStatus ?? string.Empty,
            UnityMenuPath: summary?.AcceptedAlphaUnityPlayableProjectionUnityMenuPath ?? string.Empty,
            BaselineId: summary?.AcceptedAlphaUnityPlayableProjectionBaselineId ?? string.Empty,
            AcceptedBaselineReady:
                summary?.AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady == true,
            GeneratedRootName:
                summary?.AcceptedAlphaUnityPlayableProjectionGeneratedRootName ?? string.Empty,
            ScriptInventoryCount:
                summary?.AcceptedAlphaUnityPlayableProjectionScriptInventoryCount ?? 0,
            SmokePlanStepCount:
                summary?.AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount ?? 0,
            ForbiddenUnitySurfaceClean:
                summary?.AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean == true,
            DoNotStartAutomatically:
                summary?.AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal119.projection.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal119AcceptedAlphaUnityPlayableProjectionQualityDiagnostics(
        Goal119AcceptedAlphaUnityPlayableProjectionQuality projection,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(projection.GroupPresent, "goal119.quality.projection_group",
            "accepted_alpha_unity_playable_projection", diagnostics);
        AddIfFalse(
            projection.ProjectionStatus == AcceptedAlphaUnityPlayableProjectionVocabulary.ProjectionStatus,
            "goal119.quality.projection_status",
            "accepted_alpha_unity_playable_projection",
            diagnostics);
        AddIfFalse(
            projection.UnityMenuPath == AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            "goal119.quality.unity_menu_path",
            "accepted_alpha_unity_playable_projection",
            diagnostics);
        AddIfFalse(
            projection.BaselineId == AcceptedAlphaUnityPlayableProjectionVocabulary.BaselineId,
            "goal119.quality.baseline_id",
            "accepted_alpha_unity_playable_projection",
            diagnostics);
        AddIfFalse(projection.AcceptedBaselineReady, "goal119.quality.accepted_baseline_ready",
            "accepted_alpha_unity_playable_projection", diagnostics);
        AddIfFalse(
            projection.GeneratedRootName == AcceptedAlphaUnityPlayableProjectionVocabulary.GeneratedRootName,
            "goal119.quality.generated_root_name",
            "accepted_alpha_unity_playable_projection",
            diagnostics);
        AddIfFalse(projection.ScriptInventoryCount >= 5, "goal119.quality.script_inventory_count",
            "accepted_alpha_unity_playable_projection", diagnostics);
        AddIfFalse(projection.SmokePlanStepCount >= 5, "goal119.quality.smoke_plan_step_count",
            "accepted_alpha_unity_playable_projection", diagnostics);
        AddIfFalse(projection.ForbiddenUnitySurfaceClean,
            "goal119.quality.forbidden_unity_surface_clean",
            "accepted_alpha_unity_playable_projection",
            diagnostics);
        AddIfFalse(projection.DoNotStartAutomatically,
            "goal119.quality.do_not_start_automatically",
            "accepted_alpha_unity_playable_projection",
            diagnostics);
        AddIfFalse(projection.QualityGatePassed, "goal119.quality.quality_gate",
            "accepted_alpha_unity_playable_projection", diagnostics);
        AddIfFalse(projection.RelativePaths, "goal119.quality.relative_paths",
            "accepted_alpha_unity_playable_projection", diagnostics);
        AddIfFalse(binding.PageBindDisplaysAcceptedAlphaUnityPlayableProjection,
            "goal119.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal119AllowedPath(string path) =>
        path.StartsWith(
            AcceptedAlphaUnityPlayableProjectionVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            AcceptedAlphaUnityPlayableProjectionVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal119AcceptedAlphaUnityPlayableProjectionQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal119AcceptedAlphaUnityPlayableProjectionQuality projection,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            AcceptedAlphaUnityPlayableProjectionGroupPresent = projection.GroupPresent,
            AcceptedAlphaUnityPlayableProjectionStatus = projection.ProjectionStatus,
            AcceptedAlphaUnityPlayableProjectionUnityMenuPath = projection.UnityMenuPath,
            AcceptedAlphaUnityPlayableProjectionBaselineId = projection.BaselineId,
            AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady =
                projection.AcceptedBaselineReady,
            AcceptedAlphaUnityPlayableProjectionGeneratedRootName =
                projection.GeneratedRootName,
            AcceptedAlphaUnityPlayableProjectionScriptInventoryCount =
                projection.ScriptInventoryCount,
            AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount =
                projection.SmokePlanStepCount,
            AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean =
                projection.ForbiddenUnitySurfaceClean,
            AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically =
                projection.DoNotStartAutomatically,
            AcceptedAlphaUnityPlayableProjectionQualityGatePassed =
                projection.QualityGatePassed,
            Goal119FilesDiscoveredByRelativePaths = projection.RelativePaths,
            WinFormsAcceptedAlphaUnityPlayableProjectionBindingReal =
                binding.PageBindDisplaysAcceptedAlphaUnityPlayableProjection
        };

    private sealed record Goal119AcceptedAlphaUnityPlayableProjectionQuality(
        bool GroupPresent,
        string ProjectionStatus,
        string UnityMenuPath,
        string BaselineId,
        bool AcceptedBaselineReady,
        string GeneratedRootName,
        int ScriptInventoryCount,
        int SmokePlanStepCount,
        bool ForbiddenUnitySurfaceClean,
        bool DoNotStartAutomatically,
        bool QualityGatePassed,
        bool RelativePaths);
}
