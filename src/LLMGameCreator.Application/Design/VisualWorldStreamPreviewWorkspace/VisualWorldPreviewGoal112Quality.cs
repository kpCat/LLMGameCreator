using LLMGameCreator.Application.Design.OfflineGeoworldAlphaAcceptanceOperatorPack;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal112AlphaAcceptanceOperatorWorkspaceQuality
        BuildGoal112AlphaAcceptanceOperatorQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "offline_geoworld_alpha_acceptance_operator_pack");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_alpha_acceptance_operator_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal112AllowedPath(entry.RelativePath));
        return new Goal112AlphaAcceptanceOperatorWorkspaceQuality(
            GroupPresent: group is not null,
            OperatorStatus:
                summary?.OfflineGeoworldAlphaAcceptanceOperatorStatus ?? string.Empty,
            Goal111DecisionStatus:
                summary?.OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus ?? string.Empty,
            ManualResultPresent:
                summary?.OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent == true,
            ManualResultAvailableForHumanReview:
                summary?.OfflineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview == true,
            AcceptedByCodex:
                summary?.OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex == true,
            HumanAcceptanceStillRequired:
                summary?.OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired == true,
            ChecklistStepCount:
                summary?.OfflineGeoworldAlphaAcceptanceOperatorChecklistStepCount ?? 0,
            ChecklistHashPresent:
                summary?.OfflineGeoworldAlphaAcceptanceOperatorChecklistHashPresent == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal112.operator.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal112AlphaAcceptanceOperatorQualityDiagnostics(
        Goal112AlphaAcceptanceOperatorWorkspaceQuality operatorPack,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(operatorPack.GroupPresent, "goal112.quality.operator_group",
            "offline_geoworld_alpha_acceptance_operator_pack", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(operatorPack.OperatorStatus),
            "goal112.quality.operator_status",
            "offline_geoworld_alpha_acceptance_operator_pack",
            diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(operatorPack.Goal111DecisionStatus),
            "goal112.quality.goal111_decision_status",
            "offline_geoworld_alpha_acceptance_operator_pack",
            diagnostics);
        AddIfFalse(!operatorPack.AcceptedByCodex, "goal112.quality.accepted_by_codex_false",
            "offline_geoworld_alpha_acceptance_operator_pack", diagnostics);
        AddIfFalse(operatorPack.HumanAcceptanceStillRequired,
            "goal112.quality.human_gate_still_required",
            "offline_geoworld_alpha_acceptance_operator_pack",
            diagnostics);
        AddIfFalse(operatorPack.ChecklistStepCount >= 12,
            "goal112.quality.checklist_step_count",
            "offline_geoworld_alpha_acceptance_operator_pack",
            diagnostics);
        AddIfFalse(operatorPack.ChecklistHashPresent, "goal112.quality.checklist_hash",
            "offline_geoworld_alpha_acceptance_operator_pack", diagnostics);
        AddIfFalse(operatorPack.QualityGatePassed, "goal112.quality.quality_gate",
            "proofStatus", diagnostics);
        AddIfFalse(operatorPack.RelativePaths, "goal112.quality.relative_paths",
            "offline_geoworld_alpha_acceptance_operator_pack", diagnostics);
        AddIfFalse(binding.PageBindDisplaysOfflineGeoworldAlphaAcceptanceOperatorPack,
            "goal112.quality.winforms_offline_geoworld_alpha_acceptance_operator_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal112AllowedPath(string path) =>
        path.StartsWith(
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAlphaAcceptanceOperatorPackVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate ApplyGoal112AlphaAcceptanceOperatorQuality(
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        Goal112AlphaAcceptanceOperatorWorkspaceQuality operatorPack,
        VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            OfflineGeoworldAlphaAcceptanceOperatorPackGroupPresent = operatorPack.GroupPresent,
            OfflineGeoworldAlphaAcceptanceOperatorStatus = operatorPack.OperatorStatus,
            OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus =
                operatorPack.Goal111DecisionStatus,
            OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent =
                operatorPack.ManualResultPresent,
            OfflineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview =
                operatorPack.ManualResultAvailableForHumanReview,
            OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex =
                operatorPack.AcceptedByCodex,
            OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired =
                operatorPack.HumanAcceptanceStillRequired,
            OfflineGeoworldAlphaAcceptanceOperatorChecklistStepCount =
                operatorPack.ChecklistStepCount,
            OfflineGeoworldAlphaAcceptanceOperatorChecklistHashPresent =
                operatorPack.ChecklistHashPresent,
            OfflineGeoworldAlphaAcceptanceOperatorQualityGatePassed =
                operatorPack.QualityGatePassed,
            Goal112FilesDiscoveredByRelativePaths = operatorPack.RelativePaths,
            WinFormsOfflineGeoworldAlphaAcceptanceOperatorBindingReal =
                binding.PageBindDisplaysOfflineGeoworldAlphaAcceptanceOperatorPack
        };

    private sealed record Goal112AlphaAcceptanceOperatorWorkspaceQuality(
        bool GroupPresent,
        string OperatorStatus,
        string Goal111DecisionStatus,
        bool ManualResultPresent,
        bool ManualResultAvailableForHumanReview,
        bool AcceptedByCodex,
        bool HumanAcceptanceStillRequired,
        int ChecklistStepCount,
        bool ChecklistHashPresent,
        bool QualityGatePassed,
        bool RelativePaths);
}
