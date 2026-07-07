using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal138RuntimeBackedUnityPlayerLoopStepperQuality
        BuildGoal138RuntimeBackedUnityPlayerLoopStepperQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "runtime_backed_unity_player_loop_stepper");
        var summary = group?.Entries.FirstOrDefault(item =>
            item.ArtifactKind == "runtime_backed_unity_player_loop_stepper_workspace_summary");
        var proofPassed = proofs.Any(item =>
            item.ProofId.StartsWith("goal138.runtime_backed_stepper.", StringComparison.Ordinal)
            && item.Passed);
        var relativePaths = group?.Entries.Count > 0
                            && group.Entries.All(entry => Goal138AllowedPath(entry.RelativePath));
        var qualityPassed =
            group is not null
            && summary is not null
            && summary.RuntimeBackedUnityPlayerLoopStepperAcceptedGoal137
            && !string.IsNullOrWhiteSpace(summary.RuntimeBackedUnityPlayerLoopStepperCandidateId)
            && summary.RuntimeBackedUnityPlayerLoopStepperFrameCount == 13
            && summary.RuntimeBackedUnityPlayerLoopStepperRequiredCategoriesPresent
            && summary.RuntimeBackedUnityPlayerLoopStepperRuntimeAuthority
            && !summary.RuntimeBackedUnityPlayerLoopStepperUnityGameplayTruth
            && !summary.RuntimeBackedUnityPlayerLoopStepperProjectionOnly
            && summary.RuntimeBackedUnityPlayerLoopStepperWindowPresent
            && summary.RuntimeBackedUnityPlayerLoopStepperBatchSmokePassed
            && summary.RuntimeBackedUnityPlayerLoopStepperManualUnityOptional
            && relativePaths
            && proofPassed;

        return new Goal138RuntimeBackedUnityPlayerLoopStepperQuality(
            GroupPresent: group is not null,
            AcceptedGoal137:
                summary?.RuntimeBackedUnityPlayerLoopStepperAcceptedGoal137 == true,
            CandidateId:
                summary?.RuntimeBackedUnityPlayerLoopStepperCandidateId ?? string.Empty,
            FrameCount:
                summary?.RuntimeBackedUnityPlayerLoopStepperFrameCount ?? 0,
            RequiredCategoriesPresent:
                summary?.RuntimeBackedUnityPlayerLoopStepperRequiredCategoriesPresent == true,
            RuntimeAuthority:
                summary?.RuntimeBackedUnityPlayerLoopStepperRuntimeAuthority == true,
            UnityGameplayTruth:
                summary?.RuntimeBackedUnityPlayerLoopStepperUnityGameplayTruth == true,
            ProjectionOnly:
                summary?.RuntimeBackedUnityPlayerLoopStepperProjectionOnly == true,
            StepperWindowPresent:
                summary?.RuntimeBackedUnityPlayerLoopStepperWindowPresent == true,
            StepperBatchSmokePassed:
                summary?.RuntimeBackedUnityPlayerLoopStepperBatchSmokePassed == true,
            NormalCommand:
                summary?.RuntimeBackedUnityPlayerLoopStepperNormalCommand ?? string.Empty,
            ReportPath:
                summary?.RuntimeBackedUnityPlayerLoopStepperReportPath ?? string.Empty,
            ManualUnityOptional:
                summary?.RuntimeBackedUnityPlayerLoopStepperManualUnityOptional == true,
            Accepted:
                summary?.RuntimeBackedUnityPlayerLoopStepperAccepted == true,
            RelativePaths: relativePaths,
            QualityGatePassed: qualityPassed);
    }

    private static void AddGoal138RuntimeBackedUnityPlayerLoopStepperQualityDiagnostics(
        Goal138RuntimeBackedUnityPlayerLoopStepperQuality stepper,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!stepper.GroupPresent)
        {
            return;
        }

        AddIfFalse(stepper.AcceptedGoal137,
            "goal138.quality.goal137_acceptance",
            "runtime_backed_unity_player_loop_stepper",
            diagnostics);
        AddIfFalse(stepper.FrameCount == 13,
            "goal138.quality.frame_count",
            "runtime_backed_unity_player_loop_stepper",
            diagnostics);
        AddIfFalse(stepper.RequiredCategoriesPresent,
            "goal138.quality.required_frame_categories",
            "runtime_backed_unity_player_loop_stepper",
            diagnostics);
        AddIfFalse(stepper.RuntimeAuthority,
            "goal138.quality.runtime_authority",
            "runtime_backed_unity_player_loop_stepper",
            diagnostics);
        AddIfFalse(!stepper.UnityGameplayTruth,
            "goal138.quality.unity_gameplay_truth",
            "runtime_backed_unity_player_loop_stepper",
            diagnostics);
        AddIfFalse(!stepper.ProjectionOnly,
            "goal138.quality.projection_only",
            "runtime_backed_unity_player_loop_stepper",
            diagnostics);
        AddIfFalse(stepper.StepperWindowPresent,
            "goal138.quality.stepper_window",
            "runtime_backed_unity_player_loop_stepper",
            diagnostics);
        AddIfFalse(stepper.StepperBatchSmokePassed,
            "goal138.quality.stepper_batch_smoke",
            "runtime_backed_unity_player_loop_stepper",
            diagnostics);
        AddIfFalse(!stepper.Accepted,
            "goal138.quality.accepted_must_stay_false",
            "runtime_backed_unity_player_loop_stepper",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopStepper,
            "goal138.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(stepper.RelativePaths,
            "goal138.quality.relative_paths",
            "runtime_backed_unity_player_loop_stepper",
            diagnostics);
    }

    private static bool Goal138AllowedPath(string path) =>
        path.StartsWith(
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            RuntimeBackedUnityPlayerLoopStepperVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal138RuntimeBackedUnityPlayerLoopStepperQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal138RuntimeBackedUnityPlayerLoopStepperQuality stepper,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            RuntimeBackedUnityPlayerLoopStepperGroupPresent = stepper.GroupPresent,
            RuntimeBackedUnityPlayerLoopStepperAcceptedGoal137 = stepper.AcceptedGoal137,
            RuntimeBackedUnityPlayerLoopStepperCandidateId = stepper.CandidateId,
            RuntimeBackedUnityPlayerLoopStepperFrameCount = stepper.FrameCount,
            RuntimeBackedUnityPlayerLoopStepperRequiredCategoriesPresent =
                stepper.RequiredCategoriesPresent,
            RuntimeBackedUnityPlayerLoopStepperRuntimeAuthority = stepper.RuntimeAuthority,
            RuntimeBackedUnityPlayerLoopStepperUnityGameplayTruth = stepper.UnityGameplayTruth,
            RuntimeBackedUnityPlayerLoopStepperProjectionOnly = stepper.ProjectionOnly,
            RuntimeBackedUnityPlayerLoopStepperWindowPresent = stepper.StepperWindowPresent,
            RuntimeBackedUnityPlayerLoopStepperBatchSmokePassed =
                stepper.StepperBatchSmokePassed,
            RuntimeBackedUnityPlayerLoopStepperNormalCommand = stepper.NormalCommand,
            RuntimeBackedUnityPlayerLoopStepperReportPath = stepper.ReportPath,
            RuntimeBackedUnityPlayerLoopStepperManualUnityOptional =
                stepper.ManualUnityOptional,
            RuntimeBackedUnityPlayerLoopStepperAccepted = stepper.Accepted,
            RuntimeBackedUnityPlayerLoopStepperFilesDiscoveredByRelativePaths =
                stepper.RelativePaths,
            RuntimeBackedUnityPlayerLoopStepperWinFormsBindingReal =
                binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopStepper,
            RuntimeBackedUnityPlayerLoopStepperQualityGatePassed =
                stepper.QualityGatePassed
                && binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopStepper,
            Passed = qualityGate.Passed
                     && (!stepper.GroupPresent
                         || stepper.QualityGatePassed
                         && binding.PageBindDisplaysRuntimeBackedUnityPlayerLoopStepper)
        };

    private sealed record Goal138RuntimeBackedUnityPlayerLoopStepperQuality(
        bool GroupPresent,
        bool AcceptedGoal137,
        string CandidateId,
        int FrameCount,
        bool RequiredCategoriesPresent,
        bool RuntimeAuthority,
        bool UnityGameplayTruth,
        bool ProjectionOnly,
        bool StepperWindowPresent,
        bool StepperBatchSmokePassed,
        string NormalCommand,
        string ReportPath,
        bool ManualUnityOptional,
        bool Accepted,
        bool RelativePaths,
        bool QualityGatePassed);
}
