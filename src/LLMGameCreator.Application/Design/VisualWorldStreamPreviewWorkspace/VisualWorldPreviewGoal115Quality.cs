using LLMGameCreator.Application.Design.OfflineGeoworldAlphaHumanResultRevalidation;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal115AlphaHumanResultRevalidationQuality
        BuildGoal115AlphaHumanResultRevalidationQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "offline_geoworld_alpha_human_result_revalidation");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_alpha_human_result_revalidation_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal115AllowedPath(entry.RelativePath));
        return new Goal115AlphaHumanResultRevalidationQuality(
            GroupPresent: group is not null,
            DecisionStatus:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus ?? string.Empty,
            Goal111DecisionStatus:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus ?? string.Empty,
            ManualResultPresent:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationManualResultPresent == true,
            ManualResultJsonValid:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationManualResultJsonValid == true,
            AcceptableCandidate:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate == true,
            RecommendedHumanDecision:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision ?? string.Empty,
            AcceptedByCodex:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex == true,
            HumanAcceptanceStillRequired:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired == true,
            ManualGateRemainsHumanDecision:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision == true,
            RequiredStepCount:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationRequiredStepCount ?? 0,
            PassedStepCount:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationPassedStepCount ?? 0,
            ManualInputNotCommitted:
                summary?.OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal115.human_result.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal115AlphaHumanResultRevalidationQualityDiagnostics(
        Goal115AlphaHumanResultRevalidationQuality revalidation,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(revalidation.GroupPresent, "goal115.quality.revalidation_group",
            "offline_geoworld_alpha_human_result_revalidation", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(revalidation.DecisionStatus),
            "goal115.quality.decision_status",
            "offline_geoworld_alpha_human_result_revalidation",
            diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(revalidation.Goal111DecisionStatus),
            "goal115.quality.goal111_decision_status",
            "offline_geoworld_alpha_human_result_revalidation",
            diagnostics);
        AddIfFalse(revalidation.ManualResultPresent,
            "goal115.quality.manual_result_present",
            "offline_geoworld_alpha_human_result_revalidation",
            diagnostics);
        AddIfFalse(revalidation.ManualResultJsonValid,
            "goal115.quality.manual_result_json_valid",
            "offline_geoworld_alpha_human_result_revalidation",
            diagnostics);
        AddIfFalse(!revalidation.AcceptedByCodex, "goal115.quality.accepted_by_codex_false",
            "offline_geoworld_alpha_human_result_revalidation", diagnostics);
        AddIfFalse(revalidation.HumanAcceptanceStillRequired,
            "goal115.quality.human_gate_still_required",
            "offline_geoworld_alpha_human_result_revalidation",
            diagnostics);
        AddIfFalse(revalidation.ManualGateRemainsHumanDecision,
            "goal115.quality.manual_gate_human_decision",
            "offline_geoworld_alpha_human_result_revalidation",
            diagnostics);
        AddIfFalse(revalidation.RequiredStepCount >= 12,
            "goal115.quality.required_step_count",
            "offline_geoworld_alpha_human_result_revalidation",
            diagnostics);
        AddIfFalse(revalidation.ManualInputNotCommitted,
            "goal115.quality.manual_input_not_committed",
            "offline_geoworld_alpha_human_result_revalidation",
            diagnostics);
        AddIfFalse(revalidation.QualityGatePassed, "goal115.quality.quality_gate",
            "proofStatus", diagnostics);
        AddIfFalse(revalidation.RelativePaths, "goal115.quality.relative_paths",
            "offline_geoworld_alpha_human_result_revalidation", diagnostics);
        AddIfFalse(binding.PageBindDisplaysOfflineGeoworldAlphaHumanResultRevalidation,
            "goal115.quality.winforms_offline_geoworld_alpha_human_result_revalidation_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal115AllowedPath(string path) =>
        path.StartsWith(
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAlphaHumanResultRevalidationVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate ApplyGoal115AlphaHumanResultRevalidationQuality(
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        Goal115AlphaHumanResultRevalidationQuality revalidation,
        VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            OfflineGeoworldAlphaHumanResultRevalidationGroupPresent = revalidation.GroupPresent,
            OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus = revalidation.DecisionStatus,
            OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus =
                revalidation.Goal111DecisionStatus,
            OfflineGeoworldAlphaHumanResultRevalidationManualResultPresent =
                revalidation.ManualResultPresent,
            OfflineGeoworldAlphaHumanResultRevalidationManualResultJsonValid =
                revalidation.ManualResultJsonValid,
            OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate =
                revalidation.AcceptableCandidate,
            OfflineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision =
                revalidation.RecommendedHumanDecision,
            OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex =
                revalidation.AcceptedByCodex,
            OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired =
                revalidation.HumanAcceptanceStillRequired,
            OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision =
                revalidation.ManualGateRemainsHumanDecision,
            OfflineGeoworldAlphaHumanResultRevalidationRequiredStepCount =
                revalidation.RequiredStepCount,
            OfflineGeoworldAlphaHumanResultRevalidationPassedStepCount =
                revalidation.PassedStepCount,
            OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted =
                revalidation.ManualInputNotCommitted,
            OfflineGeoworldAlphaHumanResultRevalidationQualityGatePassed =
                revalidation.QualityGatePassed,
            Goal115FilesDiscoveredByRelativePaths = revalidation.RelativePaths,
            WinFormsOfflineGeoworldAlphaHumanResultRevalidationBindingReal =
                binding.PageBindDisplaysOfflineGeoworldAlphaHumanResultRevalidation
        };

    private sealed record Goal115AlphaHumanResultRevalidationQuality(
        bool GroupPresent,
        string DecisionStatus,
        string Goal111DecisionStatus,
        bool ManualResultPresent,
        bool ManualResultJsonValid,
        bool AcceptableCandidate,
        string RecommendedHumanDecision,
        bool AcceptedByCodex,
        bool HumanAcceptanceStillRequired,
        bool ManualGateRemainsHumanDecision,
        int RequiredStepCount,
        int PassedStepCount,
        bool ManualInputNotCommitted,
        bool QualityGatePassed,
        bool RelativePaths);
}
