using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultWorkbench;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal113AlphaManualResultWorkbenchQuality
        BuildGoal113AlphaManualResultWorkbenchQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "offline_geoworld_alpha_manual_result_workbench");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_alpha_manual_result_workbench_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal113AllowedPath(entry.RelativePath));
        return new Goal113AlphaManualResultWorkbenchQuality(
            GroupPresent: group is not null,
            WorkbenchStatus:
                summary?.OfflineGeoworldAlphaManualResultWorkbenchStatus ?? string.Empty,
            Goal111DecisionStatus:
                summary?.OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus ?? string.Empty,
            Goal112OperatorStatus:
                summary?.OfflineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus ?? string.Empty,
            ManualResultPresent:
                summary?.OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent == true,
            AcceptedByCodex:
                summary?.OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex == true,
            HumanAcceptanceStillRequired:
                summary?.OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired == true,
            DraftTemplateOnly:
                summary?.OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly == true,
            ChecklistStepCount:
                summary?.OfflineGeoworldAlphaManualResultWorkbenchChecklistStepCount ?? 0,
            ChecklistHashPresent:
                !string.IsNullOrWhiteSpace(
                    summary?.OfflineGeoworldAlphaManualResultWorkbenchChecklistHash),
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal113.workbench.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal113AlphaManualResultWorkbenchQualityDiagnostics(
        Goal113AlphaManualResultWorkbenchQuality workbench,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(workbench.GroupPresent, "goal113.quality.workbench_group",
            "offline_geoworld_alpha_manual_result_workbench", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(workbench.WorkbenchStatus),
            "goal113.quality.workbench_status",
            "offline_geoworld_alpha_manual_result_workbench",
            diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(workbench.Goal111DecisionStatus),
            "goal113.quality.goal111_decision_status",
            "offline_geoworld_alpha_manual_result_workbench",
            diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(workbench.Goal112OperatorStatus),
            "goal113.quality.goal112_operator_status",
            "offline_geoworld_alpha_manual_result_workbench",
            diagnostics);
        AddIfFalse(!workbench.AcceptedByCodex, "goal113.quality.accepted_by_codex_false",
            "offline_geoworld_alpha_manual_result_workbench", diagnostics);
        AddIfFalse(workbench.HumanAcceptanceStillRequired,
            "goal113.quality.human_gate_still_required",
            "offline_geoworld_alpha_manual_result_workbench",
            diagnostics);
        AddIfFalse(workbench.DraftTemplateOnly,
            "goal113.quality.draft_template_only",
            "offline_geoworld_alpha_manual_result_workbench",
            diagnostics);
        AddIfFalse(workbench.ChecklistStepCount >= 12,
            "goal113.quality.checklist_step_count",
            "offline_geoworld_alpha_manual_result_workbench",
            diagnostics);
        AddIfFalse(workbench.ChecklistHashPresent, "goal113.quality.checklist_hash",
            "offline_geoworld_alpha_manual_result_workbench", diagnostics);
        AddIfFalse(workbench.QualityGatePassed, "goal113.quality.quality_gate",
            "proofStatus", diagnostics);
        AddIfFalse(workbench.RelativePaths, "goal113.quality.relative_paths",
            "offline_geoworld_alpha_manual_result_workbench", diagnostics);
        AddIfFalse(binding.PageBindDisplaysOfflineGeoworldAlphaManualResultWorkbench,
            "goal113.quality.winforms_offline_geoworld_alpha_manual_result_workbench_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal113AllowedPath(string path) =>
        path.StartsWith(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAlphaManualResultWorkbenchVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate ApplyGoal113AlphaManualResultWorkbenchQuality(
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        Goal113AlphaManualResultWorkbenchQuality workbench,
        VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            OfflineGeoworldAlphaManualResultWorkbenchGroupPresent = workbench.GroupPresent,
            OfflineGeoworldAlphaManualResultWorkbenchStatus = workbench.WorkbenchStatus,
            OfflineGeoworldAlphaManualResultWorkbenchGoal111DecisionStatus =
                workbench.Goal111DecisionStatus,
            OfflineGeoworldAlphaManualResultWorkbenchGoal112OperatorStatus =
                workbench.Goal112OperatorStatus,
            OfflineGeoworldAlphaManualResultWorkbenchManualResultPresent =
                workbench.ManualResultPresent,
            OfflineGeoworldAlphaManualResultWorkbenchAcceptedByCodex =
                workbench.AcceptedByCodex,
            OfflineGeoworldAlphaManualResultWorkbenchHumanAcceptanceStillRequired =
                workbench.HumanAcceptanceStillRequired,
            OfflineGeoworldAlphaManualResultWorkbenchDraftTemplateOnly =
                workbench.DraftTemplateOnly,
            OfflineGeoworldAlphaManualResultWorkbenchChecklistStepCount =
                workbench.ChecklistStepCount,
            OfflineGeoworldAlphaManualResultWorkbenchChecklistHashPresent =
                workbench.ChecklistHashPresent,
            OfflineGeoworldAlphaManualResultWorkbenchQualityGatePassed =
                workbench.QualityGatePassed,
            Goal113FilesDiscoveredByRelativePaths = workbench.RelativePaths,
            WinFormsOfflineGeoworldAlphaManualResultWorkbenchBindingReal =
                binding.PageBindDisplaysOfflineGeoworldAlphaManualResultWorkbench
        };

    private sealed record Goal113AlphaManualResultWorkbenchQuality(
        bool GroupPresent,
        string WorkbenchStatus,
        string Goal111DecisionStatus,
        string Goal112OperatorStatus,
        bool ManualResultPresent,
        bool AcceptedByCodex,
        bool HumanAcceptanceStillRequired,
        bool DraftTemplateOnly,
        int ChecklistStepCount,
        bool ChecklistHashPresent,
        bool QualityGatePassed,
        bool RelativePaths);
}
