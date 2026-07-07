using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal139RuntimeBackedUnityPlayerLoopInteractiveControlsQuality
        BuildGoal139RuntimeBackedUnityPlayerLoopInteractiveControlsQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "runtime_backed_unity_player_loop_interactive_controls");
        var summary = group?.Entries.FirstOrDefault(item =>
            item.ArtifactKind == "runtime_backed_unity_player_loop_interactive_controls_workspace_summary");
        var proofPassed = proofs.Any(item =>
            item.ProofId.StartsWith("goal139.runtime_backed_interactive_controls.", StringComparison.Ordinal)
            && item.Passed);
        var relativePaths = group?.Entries.Count > 0
                            && group.Entries.All(entry => Goal139AllowedPath(entry.RelativePath));
        var qualityPassed =
            group is not null
            && summary is not null
            && summary.RuntimeBackedUnityPlayerLoopInteractiveControlsAcceptedGoal138
            && !string.IsNullOrWhiteSpace(summary.RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId)
            && summary.RuntimeBackedUnityPlayerLoopInteractiveControlsFrameCount == 13
            && summary.RuntimeBackedUnityPlayerLoopInteractiveControlsRequiredControlsPresent
            && summary.RuntimeBackedUnityPlayerLoopInteractiveControlsControlScriptPassed
            && summary.RuntimeBackedUnityPlayerLoopInteractiveControlsWindowPresent
            && summary.RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmokePassed
            && summary.RuntimeBackedUnityPlayerLoopInteractiveControlsRuntimeAuthority
            && !summary.RuntimeBackedUnityPlayerLoopInteractiveControlsUnityGameplayTruth
            && !summary.RuntimeBackedUnityPlayerLoopInteractiveControlsProjectionOnly
            && summary.RuntimeBackedUnityPlayerLoopInteractiveControlsManualUnityOptional
            && relativePaths
            && proofPassed;

        return new Goal139RuntimeBackedUnityPlayerLoopInteractiveControlsQuality(
            GroupPresent: group is not null,
            AcceptedGoal138:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsAcceptedGoal138 == true,
            CandidateId:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId ?? string.Empty,
            FrameCount:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsFrameCount ?? 0,
            RequiredControlsPresent:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsRequiredControlsPresent == true,
            ControlScriptPassed:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsControlScriptPassed == true,
            InteractiveControlsWindowPresent:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsWindowPresent == true,
            UnityInteractiveControlsSmokePassed:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmokePassed == true,
            RuntimeAuthority:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsRuntimeAuthority == true,
            UnityGameplayTruth:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsUnityGameplayTruth == true,
            ProjectionOnly:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsProjectionOnly == true,
            NormalCommand:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsNormalCommand ?? string.Empty,
            ReportPath:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsReportPath ?? string.Empty,
            ManualUnityOptional:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsManualUnityOptional == true,
            Accepted:
                summary?.RuntimeBackedUnityPlayerLoopInteractiveControlsAccepted == true,
            RelativePaths: relativePaths,
            QualityGatePassed: qualityPassed);
    }

    private static void AddGoal139RuntimeBackedUnityPlayerLoopInteractiveControlsQualityDiagnostics(
        Goal139RuntimeBackedUnityPlayerLoopInteractiveControlsQuality controls,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!controls.GroupPresent)
        {
            return;
        }

        AddIfFalse(controls.AcceptedGoal138,
            "goal139.quality.goal138_acceptance",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
        AddIfFalse(controls.FrameCount == 13,
            "goal139.quality.frame_count",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
        AddIfFalse(controls.RequiredControlsPresent,
            "goal139.quality.required_controls",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
        AddIfFalse(controls.ControlScriptPassed,
            "goal139.quality.control_script",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
        AddIfFalse(controls.InteractiveControlsWindowPresent,
            "goal139.quality.interactive_controls_window",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
        AddIfFalse(controls.UnityInteractiveControlsSmokePassed,
            "goal139.quality.unity_interactive_controls_smoke",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
        AddIfFalse(controls.RuntimeAuthority,
            "goal139.quality.runtime_authority",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
        AddIfFalse(!controls.UnityGameplayTruth,
            "goal139.quality.unity_gameplay_truth",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
        AddIfFalse(!controls.ProjectionOnly,
            "goal139.quality.projection_only",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
        AddIfFalse(!controls.Accepted,
            "goal139.quality.accepted_must_stay_false",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopInteractiveControls,
            "goal139.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(controls.RelativePaths,
            "goal139.quality.relative_paths",
            "runtime_backed_unity_player_loop_interactive_controls",
            diagnostics);
    }

    private static bool Goal139AllowedPath(string path) =>
        path.StartsWith(
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal139RuntimeBackedUnityPlayerLoopInteractiveControlsQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal139RuntimeBackedUnityPlayerLoopInteractiveControlsQuality controls,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            RuntimeBackedUnityPlayerLoopInteractiveControlsGroupPresent = controls.GroupPresent,
            RuntimeBackedUnityPlayerLoopInteractiveControlsAcceptedGoal138 = controls.AcceptedGoal138,
            RuntimeBackedUnityPlayerLoopInteractiveControlsCandidateId = controls.CandidateId,
            RuntimeBackedUnityPlayerLoopInteractiveControlsFrameCount = controls.FrameCount,
            RuntimeBackedUnityPlayerLoopInteractiveControlsRequiredControlsPresent =
                controls.RequiredControlsPresent,
            RuntimeBackedUnityPlayerLoopInteractiveControlsControlScriptPassed =
                controls.ControlScriptPassed,
            RuntimeBackedUnityPlayerLoopInteractiveControlsWindowPresent =
                controls.InteractiveControlsWindowPresent,
            RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmokePassed =
                controls.UnityInteractiveControlsSmokePassed,
            RuntimeBackedUnityPlayerLoopInteractiveControlsRuntimeAuthority =
                controls.RuntimeAuthority,
            RuntimeBackedUnityPlayerLoopInteractiveControlsUnityGameplayTruth =
                controls.UnityGameplayTruth,
            RuntimeBackedUnityPlayerLoopInteractiveControlsProjectionOnly =
                controls.ProjectionOnly,
            RuntimeBackedUnityPlayerLoopInteractiveControlsNormalCommand =
                controls.NormalCommand,
            RuntimeBackedUnityPlayerLoopInteractiveControlsReportPath =
                controls.ReportPath,
            RuntimeBackedUnityPlayerLoopInteractiveControlsManualUnityOptional =
                controls.ManualUnityOptional,
            RuntimeBackedUnityPlayerLoopInteractiveControlsAccepted = controls.Accepted,
            RuntimeBackedUnityPlayerLoopInteractiveControlsFilesDiscoveredByRelativePaths =
                controls.RelativePaths,
            RuntimeBackedUnityPlayerLoopInteractiveControlsWinFormsBindingReal =
                binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopInteractiveControls,
            RuntimeBackedUnityPlayerLoopInteractiveControlsQualityGatePassed =
                controls.QualityGatePassed
                && binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopInteractiveControls,
            Passed = qualityGate.Passed
                     && (!controls.GroupPresent
                         || controls.QualityGatePassed
                         && binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopInteractiveControls)
        };

    private sealed record Goal139RuntimeBackedUnityPlayerLoopInteractiveControlsQuality(
        bool GroupPresent,
        bool AcceptedGoal138,
        string CandidateId,
        int FrameCount,
        bool RequiredControlsPresent,
        bool ControlScriptPassed,
        bool InteractiveControlsWindowPresent,
        bool UnityInteractiveControlsSmokePassed,
        bool RuntimeAuthority,
        bool UnityGameplayTruth,
        bool ProjectionOnly,
        string NormalCommand,
        string ReportPath,
        bool ManualUnityOptional,
        bool Accepted,
        bool RelativePaths,
        bool QualityGatePassed);
}
