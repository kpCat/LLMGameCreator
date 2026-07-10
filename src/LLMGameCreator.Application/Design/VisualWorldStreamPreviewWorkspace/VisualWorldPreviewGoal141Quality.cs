using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal141RuntimeBackedPlayerCommandRoundtripQuality
        BuildGoal141RuntimeBackedPlayerCommandRoundtripQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "runtime_backed_player_command_roundtrip");
        var summary = group?.Entries.FirstOrDefault(item =>
            item.ArtifactKind == "runtime_backed_player_command_roundtrip_workspace_summary");
        var proofPassed = proofs.Any(item =>
            item.ProofId.StartsWith(
                "goal141.runtime_backed_player_command_roundtrip.",
                StringComparison.Ordinal)
            && item.Passed);
        var relativePaths = group?.Entries.Count > 0
                            && group.Entries.All(entry => Goal141AllowedPath(entry.RelativePath));
        var qualityPassed =
            group is not null
            && summary is not null
            && summary.RuntimeBackedPlayerCommandRoundtripGoal140Accepted
            && !string.IsNullOrWhiteSpace(summary.RuntimeBackedPlayerCommandRoundtripCandidateId)
            && summary.RuntimeBackedPlayerCommandRoundtripTotalControlRequestCount == 6
            && summary.RuntimeBackedPlayerCommandRoundtripRequestCount == 6
            && summary.RuntimeBackedPlayerCommandRoundtripRuntimeRoutedRequestCount == 4
            && summary.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRequestCount == 2
            && summary.RuntimeBackedPlayerCommandRoundtripExecutedRequestCount == 4
            && summary.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRuntimeExecutionCount == 0
            && summary.RuntimeBackedPlayerCommandRoundtripRuntimeMutatingPresentationRequestCount == 0
            && summary.RuntimeBackedPlayerCommandRoundtripResponseCount == 6
            && summary.RuntimeBackedPlayerCommandRoundtripSnapshotCount
            >= summary.RuntimeBackedPlayerCommandRoundtripExecutedRequestCount
            && summary.RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent
            && summary.RuntimeBackedPlayerCommandRoundtripStateHashChainPresent
            && summary.RuntimeBackedPlayerCommandRoundtripRequestResponseCorrelationPassed
            && summary.RuntimeBackedPlayerCommandRoundtripSequentialCursorContinuityPassed
            && summary.RuntimeBackedPlayerCommandRoundtripStateHashContinuityPassed
            && summary.RuntimeBackedPlayerCommandRoundtripCopySummaryStateUnchanged
            && summary.RuntimeBackedPlayerCommandRoundtripLoadModelStateUnchanged
            && summary.RuntimeBackedPlayerCommandRoundtripNoUnrelatedGameplayMapping
            && summary.RuntimeBackedPlayerCommandRoundtripSemanticCorrectnessPassed
            && summary.RuntimeBackedPlayerCommandRoundtripRuntimeAuthority
            && !summary.RuntimeBackedPlayerCommandRoundtripProjectionOnly
            && !summary.RuntimeBackedPlayerCommandRoundtripUnityGameplayTruth
            && summary.RuntimeBackedPlayerCommandRoundtripUnityConsumesRoundtripResult
            && summary.RuntimeBackedPlayerCommandRoundtripManualUnityOptional
            && !summary.RuntimeBackedPlayerCommandRoundtripAccepted
            && relativePaths
            && proofPassed;

        return new Goal141RuntimeBackedPlayerCommandRoundtripQuality(
            GroupPresent: group is not null,
            Goal140Accepted: summary?.RuntimeBackedPlayerCommandRoundtripGoal140Accepted == true,
            CandidateId: summary?.RuntimeBackedPlayerCommandRoundtripCandidateId ?? string.Empty,
            TotalControlRequestCount:
                summary?.RuntimeBackedPlayerCommandRoundtripTotalControlRequestCount ?? 0,
            RequestCount: summary?.RuntimeBackedPlayerCommandRoundtripRequestCount ?? 0,
            RuntimeRoutedRequestCount:
                summary?.RuntimeBackedPlayerCommandRoundtripRuntimeRoutedRequestCount ?? 0,
            PresentationOnlyRequestCount:
                summary?.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRequestCount ?? 0,
            ExecutedRequestCount:
                summary?.RuntimeBackedPlayerCommandRoundtripExecutedRequestCount ?? 0,
            PresentationOnlyRuntimeExecutionCount:
                summary?.RuntimeBackedPlayerCommandRoundtripPresentationOnlyRuntimeExecutionCount ?? 0,
            RuntimeMutatingPresentationRequestCount:
                summary?.RuntimeBackedPlayerCommandRoundtripRuntimeMutatingPresentationRequestCount ?? 0,
            ResponseCount: summary?.RuntimeBackedPlayerCommandRoundtripResponseCount ?? 0,
            SnapshotCount: summary?.RuntimeBackedPlayerCommandRoundtripSnapshotCount ?? 0,
            ControlRequestBridgePresent:
                summary?.RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent == true,
            StateHashChainPresent:
                summary?.RuntimeBackedPlayerCommandRoundtripStateHashChainPresent == true,
            RequestResponseCorrelationPassed:
                summary?.RuntimeBackedPlayerCommandRoundtripRequestResponseCorrelationPassed == true,
            SequentialCursorContinuityPassed:
                summary?.RuntimeBackedPlayerCommandRoundtripSequentialCursorContinuityPassed == true,
            StateHashContinuityPassed:
                summary?.RuntimeBackedPlayerCommandRoundtripStateHashContinuityPassed == true,
            CopySummaryStateUnchanged:
                summary?.RuntimeBackedPlayerCommandRoundtripCopySummaryStateUnchanged == true,
            LoadModelStateUnchanged:
                summary?.RuntimeBackedPlayerCommandRoundtripLoadModelStateUnchanged == true,
            NoUnrelatedGameplayMapping:
                summary?.RuntimeBackedPlayerCommandRoundtripNoUnrelatedGameplayMapping == true,
            SemanticCorrectnessPassed:
                summary?.RuntimeBackedPlayerCommandRoundtripSemanticCorrectnessPassed == true,
            RuntimeAuthority: summary?.RuntimeBackedPlayerCommandRoundtripRuntimeAuthority == true,
            ProjectionOnly: summary?.RuntimeBackedPlayerCommandRoundtripProjectionOnly == true,
            UnityGameplayTruth:
                summary?.RuntimeBackedPlayerCommandRoundtripUnityGameplayTruth == true,
            UnityConsumesRoundtripResult:
                summary?.RuntimeBackedPlayerCommandRoundtripUnityConsumesRoundtripResult == true,
            NormalCommand: summary?.RuntimeBackedPlayerCommandRoundtripNormalCommand ?? string.Empty,
            ReportPath: summary?.RuntimeBackedPlayerCommandRoundtripReportPath ?? string.Empty,
            ManualUnityOptional:
                summary?.RuntimeBackedPlayerCommandRoundtripManualUnityOptional == true,
            Accepted: summary?.RuntimeBackedPlayerCommandRoundtripAccepted == true,
            RelativePaths: relativePaths,
            QualityGatePassed: qualityPassed);
    }

    private static void AddGoal141RuntimeBackedPlayerCommandRoundtripQualityDiagnostics(
        Goal141RuntimeBackedPlayerCommandRoundtripQuality roundtrip,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        if (!roundtrip.GroupPresent)
        {
            return;
        }

        AddIfFalse(roundtrip.Goal140Accepted,
            "goal141.quality.goal140_acceptance",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.TotalControlRequestCount == 6,
            "goal141.quality.total_control_request_count",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.RequestCount == 6,
            "goal141.quality.request_count",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.RuntimeRoutedRequestCount == 4,
            "goal141.quality.runtime_routed_request_count",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.PresentationOnlyRequestCount == 2,
            "goal141.quality.presentation_only_request_count",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.ExecutedRequestCount == 4,
            "goal141.quality.executed_request_count",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.PresentationOnlyRuntimeExecutionCount == 0,
            "goal141.quality.presentation_only_runtime_execution_count",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.RuntimeMutatingPresentationRequestCount == 0,
            "goal141.quality.runtime_mutating_presentation_request_count",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.ResponseCount == 6,
            "goal141.quality.response_count",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.SnapshotCount >= roundtrip.ExecutedRequestCount,
            "goal141.quality.snapshot_count",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.ControlRequestBridgePresent,
            "goal141.quality.control_request_bridge",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.StateHashChainPresent,
            "goal141.quality.state_hash_chain",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.RequestResponseCorrelationPassed,
            "goal141.quality.request_response_correlation",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.SequentialCursorContinuityPassed,
            "goal141.quality.sequential_cursor_continuity",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.StateHashContinuityPassed,
            "goal141.quality.state_hash_continuity",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.CopySummaryStateUnchanged,
            "goal141.quality.copy_summary_state_unchanged",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.LoadModelStateUnchanged,
            "goal141.quality.load_model_state_unchanged",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.NoUnrelatedGameplayMapping,
            "goal141.quality.no_unrelated_gameplay_mapping",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.SemanticCorrectnessPassed,
            "goal141.quality.semantic_correctness",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.RuntimeAuthority,
            "goal141.quality.runtime_authority",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(!roundtrip.ProjectionOnly,
            "goal141.quality.projection_only",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(!roundtrip.UnityGameplayTruth,
            "goal141.quality.unity_gameplay_truth",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(roundtrip.UnityConsumesRoundtripResult,
            "goal141.quality.unity_consumes_roundtrip_result",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(!roundtrip.Accepted,
            "goal141.quality.accepted_must_stay_false",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
        AddIfFalse(binding.PageBindDisplaysRuntimeBackedPlayerCommandRoundtrip,
            "goal141.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
        AddIfFalse(roundtrip.RelativePaths,
            "goal141.quality.relative_paths",
            "runtime_backed_player_command_roundtrip",
            diagnostics);
    }

    private static void AddGoal138Through141RuntimeBackedQualityDiagnostics(
        Goal138RuntimeBackedUnityPlayerLoopStepperQuality stepper,
        Goal139RuntimeBackedUnityPlayerLoopInteractiveControlsQuality interactiveControls,
        Goal140RuntimeBackedUnityPlayerLoopControlsUxQuality controlsUx,
        Goal141RuntimeBackedPlayerCommandRoundtripQuality roundtrip,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddGoal138RuntimeBackedUnityPlayerLoopStepperQualityDiagnostics(stepper, binding, diagnostics);
        AddGoal139RuntimeBackedUnityPlayerLoopInteractiveControlsQualityDiagnostics(
            interactiveControls,
            binding,
            diagnostics);
        AddGoal140RuntimeBackedUnityPlayerLoopControlsUxQualityDiagnostics(
            controlsUx,
            binding,
            diagnostics);
        AddGoal141RuntimeBackedPlayerCommandRoundtripQualityDiagnostics(roundtrip, binding, diagnostics);
    }

    private static bool Goal141AllowedPath(string path) =>
        path.StartsWith(
            RuntimeBackedPlayerCommandRoundtripVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            RuntimeBackedPlayerCommandRoundtripVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal141RuntimeBackedPlayerCommandRoundtripQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal141RuntimeBackedPlayerCommandRoundtripQuality roundtrip,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            RuntimeBackedPlayerCommandRoundtripGroupPresent = roundtrip.GroupPresent,
            RuntimeBackedPlayerCommandRoundtripGoal140Accepted = roundtrip.Goal140Accepted,
            RuntimeBackedPlayerCommandRoundtripCandidateId = roundtrip.CandidateId,
            RuntimeBackedPlayerCommandRoundtripTotalControlRequestCount =
                roundtrip.TotalControlRequestCount,
            RuntimeBackedPlayerCommandRoundtripRequestCount = roundtrip.RequestCount,
            RuntimeBackedPlayerCommandRoundtripRuntimeRoutedRequestCount =
                roundtrip.RuntimeRoutedRequestCount,
            RuntimeBackedPlayerCommandRoundtripPresentationOnlyRequestCount =
                roundtrip.PresentationOnlyRequestCount,
            RuntimeBackedPlayerCommandRoundtripExecutedRequestCount = roundtrip.ExecutedRequestCount,
            RuntimeBackedPlayerCommandRoundtripPresentationOnlyRuntimeExecutionCount =
                roundtrip.PresentationOnlyRuntimeExecutionCount,
            RuntimeBackedPlayerCommandRoundtripRuntimeMutatingPresentationRequestCount =
                roundtrip.RuntimeMutatingPresentationRequestCount,
            RuntimeBackedPlayerCommandRoundtripResponseCount = roundtrip.ResponseCount,
            RuntimeBackedPlayerCommandRoundtripSnapshotCount = roundtrip.SnapshotCount,
            RuntimeBackedPlayerCommandRoundtripControlRequestBridgePresent =
                roundtrip.ControlRequestBridgePresent,
            RuntimeBackedPlayerCommandRoundtripStateHashChainPresent =
                roundtrip.StateHashChainPresent,
            RuntimeBackedPlayerCommandRoundtripRequestResponseCorrelationPassed =
                roundtrip.RequestResponseCorrelationPassed,
            RuntimeBackedPlayerCommandRoundtripSequentialCursorContinuityPassed =
                roundtrip.SequentialCursorContinuityPassed,
            RuntimeBackedPlayerCommandRoundtripStateHashContinuityPassed =
                roundtrip.StateHashContinuityPassed,
            RuntimeBackedPlayerCommandRoundtripCopySummaryStateUnchanged =
                roundtrip.CopySummaryStateUnchanged,
            RuntimeBackedPlayerCommandRoundtripLoadModelStateUnchanged =
                roundtrip.LoadModelStateUnchanged,
            RuntimeBackedPlayerCommandRoundtripNoUnrelatedGameplayMapping =
                roundtrip.NoUnrelatedGameplayMapping,
            RuntimeBackedPlayerCommandRoundtripSemanticCorrectnessPassed =
                roundtrip.SemanticCorrectnessPassed,
            RuntimeBackedPlayerCommandRoundtripRuntimeAuthority = roundtrip.RuntimeAuthority,
            RuntimeBackedPlayerCommandRoundtripProjectionOnly = roundtrip.ProjectionOnly,
            RuntimeBackedPlayerCommandRoundtripUnityGameplayTruth = roundtrip.UnityGameplayTruth,
            RuntimeBackedPlayerCommandRoundtripUnityConsumesRoundtripResult =
                roundtrip.UnityConsumesRoundtripResult,
            RuntimeBackedPlayerCommandRoundtripNormalCommand = roundtrip.NormalCommand,
            RuntimeBackedPlayerCommandRoundtripReportPath = roundtrip.ReportPath,
            RuntimeBackedPlayerCommandRoundtripManualUnityOptional = roundtrip.ManualUnityOptional,
            RuntimeBackedPlayerCommandRoundtripAccepted = roundtrip.Accepted,
            RuntimeBackedPlayerCommandRoundtripFilesDiscoveredByRelativePaths =
                roundtrip.RelativePaths,
            RuntimeBackedPlayerCommandRoundtripWinFormsBindingReal =
                binding.PageBindDisplaysRuntimeBackedPlayerCommandRoundtrip,
            RuntimeBackedPlayerCommandRoundtripQualityGatePassed =
                roundtrip.QualityGatePassed
                && binding.PageBindDisplaysRuntimeBackedPlayerCommandRoundtrip,
            Passed = qualityGate.Passed
                     && (!roundtrip.GroupPresent
                         || roundtrip.QualityGatePassed
                         && binding.PageBindDisplaysRuntimeBackedPlayerCommandRoundtrip)
        };

    private sealed record Goal141RuntimeBackedPlayerCommandRoundtripQuality(
        bool GroupPresent,
        bool Goal140Accepted,
        string CandidateId,
        int TotalControlRequestCount,
        int RequestCount,
        int RuntimeRoutedRequestCount,
        int PresentationOnlyRequestCount,
        int ExecutedRequestCount,
        int PresentationOnlyRuntimeExecutionCount,
        int RuntimeMutatingPresentationRequestCount,
        int ResponseCount,
        int SnapshotCount,
        bool ControlRequestBridgePresent,
        bool StateHashChainPresent,
        bool RequestResponseCorrelationPassed,
        bool SequentialCursorContinuityPassed,
        bool StateHashContinuityPassed,
        bool CopySummaryStateUnchanged,
        bool LoadModelStateUnchanged,
        bool NoUnrelatedGameplayMapping,
        bool SemanticCorrectnessPassed,
        bool RuntimeAuthority,
        bool ProjectionOnly,
        bool UnityGameplayTruth,
        bool UnityConsumesRoundtripResult,
        string NormalCommand,
        string ReportPath,
        bool ManualUnityOptional,
        bool Accepted,
        bool RelativePaths,
        bool QualityGatePassed);
}
