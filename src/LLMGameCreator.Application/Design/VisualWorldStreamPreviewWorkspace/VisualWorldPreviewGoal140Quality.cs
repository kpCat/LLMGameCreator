using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal140RuntimeBackedUnityPlayerLoopControlsUxQuality
        BuildGoal140RuntimeBackedUnityPlayerLoopControlsUxQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "runtime_backed_unity_player_loop_controls_ux");
        var summary = group?.Entries.FirstOrDefault(item =>
            item.ArtifactKind == "runtime_backed_unity_player_loop_controls_ux_workspace_summary");
        var proofPassed = proofs.Any(item =>
            item.ProofId.StartsWith("goal140.runtime_backed_controls_ux.", StringComparison.Ordinal)
            && item.Passed);
        var relativePaths = group?.Entries.Count > 0
                            && group.Entries.All(entry => Goal140AllowedPath(entry.RelativePath));
        var qualityPassed =
            group is not null
            && summary is not null
            && summary.RuntimeBackedUnityPlayerLoopControlsUxAcceptedGoal139
            && !string.IsNullOrWhiteSpace(summary.RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate)
            && summary.RuntimeBackedUnityPlayerLoopControlsUxFrameCount == 13
            && summary.RuntimeBackedUnityPlayerLoopControlsUxHumanReadableFrameNumbering
            && summary.RuntimeBackedUnityPlayerLoopControlsUxStepOnceSemanticsClear
            && summary.RuntimeBackedUnityPlayerLoopControlsUxPlayAllToEndSemanticsClear
            && summary.RuntimeBackedUnityPlayerLoopControlsUxKnownUnityEditorNoiseClassified
            && summary.RuntimeBackedUnityPlayerLoopControlsUxBlockingUnityErrorCount == 0
            && summary.RuntimeBackedUnityPlayerLoopControlsUxUnclassifiedUnityErrorCount == 0
            && summary.RuntimeBackedUnityPlayerLoopControlsUxUnitySmokePassed
            && summary.RuntimeBackedUnityPlayerLoopControlsUxRuntimeAuthority
            && !summary.RuntimeBackedUnityPlayerLoopControlsUxUnityGameplayTruth
            && !summary.RuntimeBackedUnityPlayerLoopControlsUxProjectionOnly
            && relativePaths
            && proofPassed;

        return new Goal140RuntimeBackedUnityPlayerLoopControlsUxQuality(
            GroupPresent: group is not null,
            AcceptedGoal139: summary?.RuntimeBackedUnityPlayerLoopControlsUxAcceptedGoal139 == true,
            SelectedCandidate:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate ?? string.Empty,
            FrameCount: summary?.RuntimeBackedUnityPlayerLoopControlsUxFrameCount ?? 0,
            HumanReadableFrameNumbering:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxHumanReadableFrameNumbering == true,
            StepOnceSemanticsClear:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxStepOnceSemanticsClear == true,
            PlayAllToEndSemanticsClear:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxPlayAllToEndSemanticsClear == true,
            KnownUnityEditorNoiseClassified:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxKnownUnityEditorNoiseClassified == true,
            BlockingUnityErrorCount:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxBlockingUnityErrorCount ?? 0,
            UnclassifiedUnityErrorCount:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxUnclassifiedUnityErrorCount ?? 0,
            UnityControlsUxSmokePassed:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxUnitySmokePassed == true,
            RuntimeAuthority:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxRuntimeAuthority == true,
            UnityGameplayTruth:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxUnityGameplayTruth == true,
            ProjectionOnly:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxProjectionOnly == true,
            NormalCommand:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxNormalCommand ?? string.Empty,
            ReportPath:
                summary?.RuntimeBackedUnityPlayerLoopControlsUxReportPath ?? string.Empty,
            Accepted: summary?.RuntimeBackedUnityPlayerLoopControlsUxAccepted == true,
            RelativePaths: relativePaths,
            QualityGatePassed: qualityPassed);
    }

    private static void AddGoal140RuntimeBackedUnityPlayerLoopControlsUxQualityDiagnostics(
        Goal140RuntimeBackedUnityPlayerLoopControlsUxQuality controls,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!controls.GroupPresent)
        {
            return;
        }

        AddIfFalse(controls.AcceptedGoal139,
            "goal140.quality.goal139_acceptance",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(controls.FrameCount == 13,
            "goal140.quality.frame_count",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(controls.HumanReadableFrameNumbering,
            "goal140.quality.human_frame_numbering",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(controls.StepOnceSemanticsClear,
            "goal140.quality.step_once",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(controls.PlayAllToEndSemanticsClear,
            "goal140.quality.play_all_to_end",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(controls.KnownUnityEditorNoiseClassified,
            "goal140.quality.known_unity_noise",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(controls.BlockingUnityErrorCount == 0,
            "goal140.quality.blocking_unity_errors",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(controls.UnclassifiedUnityErrorCount == 0,
            "goal140.quality.unclassified_unity_errors",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(controls.UnityControlsUxSmokePassed,
            "goal140.quality.unity_controls_ux_smoke",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(controls.RuntimeAuthority,
            "goal140.quality.runtime_authority",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(!controls.UnityGameplayTruth,
            "goal140.quality.unity_gameplay_truth",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(!controls.ProjectionOnly,
            "goal140.quality.projection_only",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(!controls.Accepted,
            "goal140.quality.accepted_must_stay_false",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopControlsUx,
            "goal140.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(controls.RelativePaths,
            "goal140.quality.relative_paths",
            "runtime_backed_unity_player_loop_controls_ux",
            diagnostics);
    }

    private static bool Goal140AllowedPath(string path) =>
        path.StartsWith(
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal140RuntimeBackedUnityPlayerLoopControlsUxQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal140RuntimeBackedUnityPlayerLoopControlsUxQuality controls,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            RuntimeBackedUnityPlayerLoopControlsUxGroupPresent = controls.GroupPresent,
            RuntimeBackedUnityPlayerLoopControlsUxAcceptedGoal139 = controls.AcceptedGoal139,
            RuntimeBackedUnityPlayerLoopControlsUxSelectedCandidate = controls.SelectedCandidate,
            RuntimeBackedUnityPlayerLoopControlsUxFrameCount = controls.FrameCount,
            RuntimeBackedUnityPlayerLoopControlsUxHumanReadableFrameNumbering =
                controls.HumanReadableFrameNumbering,
            RuntimeBackedUnityPlayerLoopControlsUxStepOnceSemanticsClear =
                controls.StepOnceSemanticsClear,
            RuntimeBackedUnityPlayerLoopControlsUxPlayAllToEndSemanticsClear =
                controls.PlayAllToEndSemanticsClear,
            RuntimeBackedUnityPlayerLoopControlsUxKnownUnityEditorNoiseClassified =
                controls.KnownUnityEditorNoiseClassified,
            RuntimeBackedUnityPlayerLoopControlsUxBlockingUnityErrorCount =
                controls.BlockingUnityErrorCount,
            RuntimeBackedUnityPlayerLoopControlsUxUnclassifiedUnityErrorCount =
                controls.UnclassifiedUnityErrorCount,
            RuntimeBackedUnityPlayerLoopControlsUxUnitySmokePassed =
                controls.UnityControlsUxSmokePassed,
            RuntimeBackedUnityPlayerLoopControlsUxRuntimeAuthority = controls.RuntimeAuthority,
            RuntimeBackedUnityPlayerLoopControlsUxUnityGameplayTruth = controls.UnityGameplayTruth,
            RuntimeBackedUnityPlayerLoopControlsUxProjectionOnly = controls.ProjectionOnly,
            RuntimeBackedUnityPlayerLoopControlsUxNormalCommand = controls.NormalCommand,
            RuntimeBackedUnityPlayerLoopControlsUxReportPath = controls.ReportPath,
            RuntimeBackedUnityPlayerLoopControlsUxAccepted = controls.Accepted,
            RuntimeBackedUnityPlayerLoopControlsUxFilesDiscoveredByRelativePaths =
                controls.RelativePaths,
            RuntimeBackedUnityPlayerLoopControlsUxWinFormsBindingReal =
                binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopControlsUx,
            RuntimeBackedUnityPlayerLoopControlsUxQualityGatePassed =
                controls.QualityGatePassed
                && binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopControlsUx,
            Passed = qualityGate.Passed
                     && (!controls.GroupPresent
                         || controls.QualityGatePassed
                         && binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopControlsUx)
        };

    private sealed record Goal140RuntimeBackedUnityPlayerLoopControlsUxQuality(
        bool GroupPresent,
        bool AcceptedGoal139,
        string SelectedCandidate,
        int FrameCount,
        bool HumanReadableFrameNumbering,
        bool StepOnceSemanticsClear,
        bool PlayAllToEndSemanticsClear,
        bool KnownUnityEditorNoiseClassified,
        int BlockingUnityErrorCount,
        int UnclassifiedUnityErrorCount,
        bool UnityControlsUxSmokePassed,
        bool RuntimeAuthority,
        bool UnityGameplayTruth,
        bool ProjectionOnly,
        string NormalCommand,
        string ReportPath,
        bool Accepted,
        bool RelativePaths,
        bool QualityGatePassed);
}
