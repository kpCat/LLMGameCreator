using LLMGameCreator.Application.Design.OfflineGeoworldAcceptedAlphaBaselineReview;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal118AcceptedAlphaBaselineQuality BuildGoal118AcceptedAlphaBaselineQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "offline_geoworld_accepted_alpha_baseline_review");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_accepted_alpha_baseline_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal118AllowedPath(entry.RelativePath));
        return new Goal118AcceptedAlphaBaselineQuality(
            GroupPresent: group is not null,
            BaselineId: summary?.OfflineGeoworldAcceptedAlphaBaselineId ?? string.Empty,
            BaselineHash: summary?.OfflineGeoworldAcceptedAlphaBaselineHash ?? string.Empty,
            AcceptedBaselineReady:
                summary?.OfflineGeoworldAcceptedAlphaBaselineReady == true,
            ManualGateStatus:
                summary?.OfflineGeoworldAcceptedAlphaManualGateStatus ?? string.Empty,
            RecommendedNextDecision:
                summary?.OfflineGeoworldAcceptedAlphaRecommendedNextDecision ?? string.Empty,
            IncludedSourceGoalCount:
                summary?.OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount ?? 0,
            AcceptedEvidenceRootCount:
                summary?.OfflineGeoworldAcceptedAlphaAcceptedEvidenceRootCount ?? 0,
            ProducedOnlyRootCount:
                summary?.OfflineGeoworldAcceptedAlphaProducedOnlyRootCount ?? 0,
            BlockedOrSupersededNoteCount:
                summary?.OfflineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount ?? 0,
            DoNotStartAutomatically:
                summary?.OfflineGeoworldAcceptedAlphaDoNotStartAutomatically == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal118.baseline.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal118AcceptedAlphaBaselineQualityDiagnostics(
        Goal118AcceptedAlphaBaselineQuality baseline,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(baseline.GroupPresent, "goal118.quality.baseline_group",
            "offline_geoworld_accepted_alpha_baseline_review", diagnostics);
        AddIfFalse(
            baseline.BaselineId
            == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.BaselineId,
            "goal118.quality.baseline_id",
            "offline_geoworld_accepted_alpha_baseline_review",
            diagnostics);
        AddIfFalse(baseline.AcceptedBaselineReady, "goal118.quality.baseline_ready",
            "offline_geoworld_accepted_alpha_baseline_review", diagnostics);
        AddIfFalse(
            baseline.ManualGateStatus
            == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ManualGateStatusAccepted,
            "goal118.quality.manual_gate_status",
            "offline_geoworld_accepted_alpha_baseline_review",
            diagnostics);
        AddIfFalse(
            baseline.RecommendedNextDecision
            == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.RecommendedNextDecision,
            "goal118.quality.recommended_next_decision",
            "offline_geoworld_accepted_alpha_baseline_review",
            diagnostics);
        AddIfFalse(
            baseline.IncludedSourceGoalCount
            == OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.SourceGoalIds.Count,
            "goal118.quality.included_source_count",
            "offline_geoworld_accepted_alpha_baseline_review",
            diagnostics);
        AddIfFalse(baseline.AcceptedEvidenceRootCount >= 6,
            "goal118.quality.accepted_evidence_roots",
            "offline_geoworld_accepted_alpha_baseline_review",
            diagnostics);
        AddIfFalse(baseline.ProducedOnlyRootCount > 0,
            "goal118.quality.produced_only_roots",
            "offline_geoworld_accepted_alpha_baseline_review",
            diagnostics);
        AddIfFalse(baseline.BlockedOrSupersededNoteCount >= 6,
            "goal118.quality.blocked_notes",
            "offline_geoworld_accepted_alpha_baseline_review",
            diagnostics);
        AddIfFalse(baseline.DoNotStartAutomatically,
            "goal118.quality.do_not_start_automatically",
            "offline_geoworld_accepted_alpha_baseline_review",
            diagnostics);
        AddIfFalse(baseline.RelativePaths, "goal118.quality.relative_paths",
            "offline_geoworld_accepted_alpha_baseline_review", diagnostics);
        AddIfFalse(binding.PageBindDisplaysOfflineGeoworldAcceptedAlphaBaselineReview,
            "goal118.quality.winforms_offline_geoworld_accepted_alpha_baseline_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal118AllowedPath(string path) =>
        path.StartsWith(
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAcceptedAlphaBaselineReviewVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate ApplyGoal118AcceptedAlphaBaselineQuality(
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        Goal118AcceptedAlphaBaselineQuality baseline,
        VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            OfflineGeoworldAcceptedAlphaBaselineGroupPresent = baseline.GroupPresent,
            OfflineGeoworldAcceptedAlphaBaselineId = baseline.BaselineId,
            OfflineGeoworldAcceptedAlphaBaselineHash = baseline.BaselineHash,
            OfflineGeoworldAcceptedAlphaBaselineReady = baseline.AcceptedBaselineReady,
            OfflineGeoworldAcceptedAlphaManualGateStatus = baseline.ManualGateStatus,
            OfflineGeoworldAcceptedAlphaRecommendedNextDecision =
                baseline.RecommendedNextDecision,
            OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount =
                baseline.IncludedSourceGoalCount,
            OfflineGeoworldAcceptedAlphaAcceptedEvidenceRootCount =
                baseline.AcceptedEvidenceRootCount,
            OfflineGeoworldAcceptedAlphaProducedOnlyRootCount = baseline.ProducedOnlyRootCount,
            OfflineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount =
                baseline.BlockedOrSupersededNoteCount,
            OfflineGeoworldAcceptedAlphaDoNotStartAutomatically =
                baseline.DoNotStartAutomatically,
            OfflineGeoworldAcceptedAlphaQualityGatePassed = baseline.QualityGatePassed,
            Goal118FilesDiscoveredByRelativePaths = baseline.RelativePaths,
            WinFormsOfflineGeoworldAcceptedAlphaBaselineBindingReal =
                binding.PageBindDisplaysOfflineGeoworldAcceptedAlphaBaselineReview
        };

    private sealed record Goal118AcceptedAlphaBaselineQuality(
        bool GroupPresent,
        string BaselineId,
        string BaselineHash,
        bool AcceptedBaselineReady,
        string ManualGateStatus,
        string RecommendedNextDecision,
        int IncludedSourceGoalCount,
        int AcceptedEvidenceRootCount,
        int ProducedOnlyRootCount,
        int BlockedOrSupersededNoteCount,
        bool DoNotStartAutomatically,
        bool QualityGatePassed,
        bool RelativePaths);
}
