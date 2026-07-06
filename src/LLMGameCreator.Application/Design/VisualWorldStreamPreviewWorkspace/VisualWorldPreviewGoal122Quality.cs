using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal122AcceptedAlphaProjectionActionLoopQuality
        BuildGoal122AcceptedAlphaProjectionActionLoopQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "accepted_alpha_projection_action_loop");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "accepted_alpha_projection_action_loop_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal122AllowedPath(entry.RelativePath));
        return new Goal122AcceptedAlphaProjectionActionLoopQuality(
            GroupPresent: group is not null,
            ActionLoopStatus: summary?.AcceptedAlphaProjectionActionLoopStatus ?? string.Empty,
            WindowPolishStatus: summary?.AcceptedAlphaProjectionActionLoopWindowPolishStatus ?? string.Empty,
            UnityMenuPath: summary?.AcceptedAlphaProjectionActionLoopUnityMenuPath ?? string.Empty,
            OneClickVerificationStillPresent:
                summary?.AcceptedAlphaProjectionActionLoopOneClickVerificationStillPresent == true,
            ProjectionActionPreviewPresent:
                summary?.AcceptedAlphaProjectionActionLoopProjectionActionPreviewPresent == true,
            ProjectionActionApplyPresent:
                summary?.AcceptedAlphaProjectionActionLoopProjectionActionApplyPresent == true,
            ProjectionStateResetPresent:
                summary?.AcceptedAlphaProjectionActionLoopProjectionStateResetPresent == true,
            WindowLayoutPolishPresent:
                summary?.AcceptedAlphaProjectionActionLoopWindowLayoutPolishPresent == true,
            UnitySmokeStatus: summary?.AcceptedAlphaProjectionActionLoopUnitySmokeStatus ?? string.Empty,
            CleanupScriptAvailable:
                summary?.AcceptedAlphaProjectionActionLoopCleanupScriptAvailable == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal122.action_loop.one_click_verification" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal122.action_loop.preview_apply_reset" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal122.action_loop.window_polish" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal122.action_loop.cleanup_script" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal122.action_loop.negative_proof" && proof.Passed)
                && summary?.AcceptedAlphaProjectionActionLoopStatus == "GREEN"
                && summary?.AcceptedAlphaProjectionActionLoopWindowPolishStatus == "GREEN",
            RelativePaths: relativePaths);
    }

    private static void AddGoal122AcceptedAlphaProjectionActionLoopQualityDiagnostics(
        Goal122AcceptedAlphaProjectionActionLoopQuality actionLoop,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(actionLoop.GroupPresent, "goal122.quality.action_loop_group",
            "accepted_alpha_projection_action_loop", diagnostics);
        AddIfFalse(actionLoop.ActionLoopStatus == "GREEN", "goal122.quality.action_loop_status",
            "accepted_alpha_projection_action_loop", diagnostics);
        AddIfFalse(actionLoop.WindowPolishStatus == "GREEN", "goal122.quality.window_polish_status",
            "accepted_alpha_projection_action_loop", diagnostics);
        AddIfFalse(
            actionLoop.UnityMenuPath == AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            "goal122.quality.unity_menu_path",
            "accepted_alpha_projection_action_loop",
            diagnostics);
        AddIfFalse(actionLoop.OneClickVerificationStillPresent,
            "goal122.quality.one_click_verification",
            "accepted_alpha_projection_action_loop",
            diagnostics);
        AddIfFalse(actionLoop.ProjectionActionPreviewPresent,
            "goal122.quality.action_preview",
            "accepted_alpha_projection_action_loop",
            diagnostics);
        AddIfFalse(actionLoop.ProjectionActionApplyPresent,
            "goal122.quality.action_apply",
            "accepted_alpha_projection_action_loop",
            diagnostics);
        AddIfFalse(actionLoop.ProjectionStateResetPresent,
            "goal122.quality.state_reset",
            "accepted_alpha_projection_action_loop",
            diagnostics);
        AddIfFalse(actionLoop.WindowLayoutPolishPresent,
            "goal122.quality.window_layout_polish",
            "accepted_alpha_projection_action_loop",
            diagnostics);
        AddIfFalse(actionLoop.CleanupScriptAvailable,
            "goal122.quality.cleanup_script",
            "accepted_alpha_projection_action_loop",
            diagnostics);
        AddIfFalse(actionLoop.QualityGatePassed,
            "goal122.quality.quality_gate",
            "accepted_alpha_projection_action_loop",
            diagnostics);
        AddIfFalse(actionLoop.RelativePaths,
            "goal122.quality.relative_paths",
            "accepted_alpha_projection_action_loop",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysAcceptedAlphaProjectionActionLoop,
            "goal122.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal122AllowedPath(string path) =>
        path.StartsWith(
            AcceptedAlphaProjectionActionLoopVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            AcceptedAlphaProjectionActionLoopVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal122AcceptedAlphaProjectionActionLoopQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal122AcceptedAlphaProjectionActionLoopQuality actionLoop,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            AcceptedAlphaProjectionActionLoopGroupPresent = actionLoop.GroupPresent,
            AcceptedAlphaProjectionActionLoopStatus = actionLoop.ActionLoopStatus,
            AcceptedAlphaProjectionActionLoopWindowPolishStatus = actionLoop.WindowPolishStatus,
            AcceptedAlphaProjectionActionLoopUnityMenuPath = actionLoop.UnityMenuPath,
            AcceptedAlphaProjectionActionLoopOneClickVerificationStillPresent =
                actionLoop.OneClickVerificationStillPresent,
            AcceptedAlphaProjectionActionLoopProjectionActionPreviewPresent =
                actionLoop.ProjectionActionPreviewPresent,
            AcceptedAlphaProjectionActionLoopProjectionActionApplyPresent =
                actionLoop.ProjectionActionApplyPresent,
            AcceptedAlphaProjectionActionLoopProjectionStateResetPresent =
                actionLoop.ProjectionStateResetPresent,
            AcceptedAlphaProjectionActionLoopWindowLayoutPolishPresent =
                actionLoop.WindowLayoutPolishPresent,
            AcceptedAlphaProjectionActionLoopUnitySmokeStatus = actionLoop.UnitySmokeStatus,
            AcceptedAlphaProjectionActionLoopCleanupScriptAvailable =
                actionLoop.CleanupScriptAvailable,
            AcceptedAlphaProjectionActionLoopQualityGatePassed =
                actionLoop.QualityGatePassed,
            Goal122FilesDiscoveredByRelativePaths = actionLoop.RelativePaths,
            WinFormsAcceptedAlphaProjectionActionLoopBindingReal =
                binding.PageBindDisplaysAcceptedAlphaProjectionActionLoop
        };

    private sealed record Goal122AcceptedAlphaProjectionActionLoopQuality(
        bool GroupPresent,
        string ActionLoopStatus,
        string WindowPolishStatus,
        string UnityMenuPath,
        bool OneClickVerificationStillPresent,
        bool ProjectionActionPreviewPresent,
        bool ProjectionActionApplyPresent,
        bool ProjectionStateResetPresent,
        bool WindowLayoutPolishPresent,
        string UnitySmokeStatus,
        bool CleanupScriptAvailable,
        bool QualityGatePassed,
        bool RelativePaths);
}
