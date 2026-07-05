using LLMGameCreator.Application.Design.OfflineGeoworldAlphaPostAcceptanceContinuationSelection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal117AlphaPostAcceptanceContinuationQuality
        BuildGoal117AlphaPostAcceptanceContinuationQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "offline_geoworld_alpha_post_acceptance_continuation_selection");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind
            == "offline_geoworld_alpha_post_acceptance_continuation_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal117AllowedPath(entry.RelativePath));
        return new Goal117AlphaPostAcceptanceContinuationQuality(
            GroupPresent: group is not null,
            ManualGateStatus:
                summary?.OfflineGeoworldAlphaPostAcceptanceManualGateStatus ?? string.Empty,
            HumanAccepted:
                summary?.OfflineGeoworldAlphaPostAcceptanceHumanAccepted == true,
            ManualResultSha256:
                summary?.OfflineGeoworldAlphaPostAcceptanceManualResultSha256 ?? string.Empty,
            RecommendedNextLane:
                summary?.OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane ?? string.Empty,
            RecommendedNextGoalId:
                summary?.OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId ?? string.Empty,
            ReadyLaneCount:
                summary?.OfflineGeoworldAlphaPostAcceptanceReadyLaneCount ?? 0,
            CandidateLaneCount:
                summary?.OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount ?? 0,
            BlockedLaneCount:
                summary?.OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount ?? 0,
            DoNotStartAutomatically:
                summary?.OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal117.continuation.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal117AlphaPostAcceptanceContinuationQualityDiagnostics(
        Goal117AlphaPostAcceptanceContinuationQuality continuation,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(continuation.GroupPresent, "goal117.quality.continuation_group",
            "offline_geoworld_alpha_post_acceptance_continuation_selection", diagnostics);
        AddIfFalse(
            continuation.RecommendedNextLane
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .LaneAcceptedAlphaBaselineReview,
            "goal117.quality.recommended_lane",
            "offline_geoworld_alpha_post_acceptance_continuation_selection",
            diagnostics);
        AddIfFalse(
            continuation.RecommendedNextGoalId
            == OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .RecommendedNextGoalId,
            "goal117.quality.recommended_next_goal",
            "offline_geoworld_alpha_post_acceptance_continuation_selection",
            diagnostics);
        AddIfFalse(continuation.ReadyLaneCount == 1, "goal117.quality.ready_lane_count",
            "offline_geoworld_alpha_post_acceptance_continuation_selection", diagnostics);
        AddIfFalse(
            continuation.CandidateLaneCount == 3,
            "goal117.quality.candidate_lane_count",
            "offline_geoworld_alpha_post_acceptance_continuation_selection",
            diagnostics);
        AddIfFalse(continuation.BlockedLaneCount == 3, "goal117.quality.blocked_lane_count",
            "offline_geoworld_alpha_post_acceptance_continuation_selection", diagnostics);
        AddIfFalse(
            continuation.DoNotStartAutomatically,
            "goal117.quality.do_not_start_automatically",
            "offline_geoworld_alpha_post_acceptance_continuation_selection",
            diagnostics);
        AddIfFalse(continuation.RelativePaths, "goal117.quality.relative_paths",
            "offline_geoworld_alpha_post_acceptance_continuation_selection", diagnostics);
        AddIfFalse(binding.PageBindDisplaysOfflineGeoworldAlphaPostAcceptanceContinuationSelection,
            "goal117.quality.winforms_offline_geoworld_alpha_post_acceptance_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal117AllowedPath(string path) =>
        path.StartsWith(
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAlphaPostAcceptanceContinuationSelectionVocabulary
                .ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal117AlphaPostAcceptanceContinuationQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal117AlphaPostAcceptanceContinuationQuality continuation,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            OfflineGeoworldAlphaPostAcceptanceContinuationGroupPresent =
                continuation.GroupPresent,
            OfflineGeoworldAlphaPostAcceptanceManualGateStatus =
                continuation.ManualGateStatus,
            OfflineGeoworldAlphaPostAcceptanceHumanAccepted =
                continuation.HumanAccepted,
            OfflineGeoworldAlphaPostAcceptanceManualResultSha256 =
                continuation.ManualResultSha256,
            OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane =
                continuation.RecommendedNextLane,
            OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId =
                continuation.RecommendedNextGoalId,
            OfflineGeoworldAlphaPostAcceptanceReadyLaneCount =
                continuation.ReadyLaneCount,
            OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount =
                continuation.CandidateLaneCount,
            OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount =
                continuation.BlockedLaneCount,
            OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically =
                continuation.DoNotStartAutomatically,
            OfflineGeoworldAlphaPostAcceptanceQualityGatePassed =
                continuation.QualityGatePassed,
            Goal117FilesDiscoveredByRelativePaths = continuation.RelativePaths,
            WinFormsOfflineGeoworldAlphaPostAcceptanceBindingReal =
                binding.PageBindDisplaysOfflineGeoworldAlphaPostAcceptanceContinuationSelection
        };

    private sealed record Goal117AlphaPostAcceptanceContinuationQuality(
        bool GroupPresent,
        string ManualGateStatus,
        bool HumanAccepted,
        string ManualResultSha256,
        string RecommendedNextLane,
        string RecommendedNextGoalId,
        int ReadyLaneCount,
        int CandidateLaneCount,
        int BlockedLaneCount,
        bool DoNotStartAutomatically,
        bool QualityGatePassed,
        bool RelativePaths);
}
