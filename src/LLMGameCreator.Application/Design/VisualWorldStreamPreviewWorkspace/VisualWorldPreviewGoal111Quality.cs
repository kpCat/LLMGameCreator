using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal111AlphaManualResultIntakeWorkspaceQuality
        BuildGoal111AlphaManualResultIntakeQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "offline_geoworld_alpha_manual_result_intake");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_alpha_manual_result_intake_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal111AllowedPath(entry.RelativePath));
        return new Goal111AlphaManualResultIntakeWorkspaceQuality(
            GroupPresent: group is not null,
            Goal110PackagePresent:
                summary?.OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent == true,
            ResultFilePresent:
                summary?.OfflineGeoworldAlphaManualResultIntakeResultFilePresent == true,
            DecisionStatus:
                summary?.OfflineGeoworldAlphaManualResultIntakeDecisionStatus ?? string.Empty,
            AcceptableCandidate:
                summary?.OfflineGeoworldAlphaManualResultIntakeAcceptableCandidate == true,
            AcceptedByCodex:
                summary?.OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex == true,
            HumanAcceptanceStillRequired:
                summary?.OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired == true,
            ChecklistHashMatched:
                summary?.OfflineGeoworldAlphaManualResultIntakeChecklistHashMatched == true,
            PassedStepCount:
                summary?.OfflineGeoworldAlphaManualResultIntakePassedStepCount ?? 0,
            FailedStepCount:
                summary?.OfflineGeoworldAlphaManualResultIntakeFailedStepCount ?? 0,
            PendingStepCount:
                summary?.OfflineGeoworldAlphaManualResultIntakePendingStepCount ?? 0,
            SkippedStepCount:
                summary?.OfflineGeoworldAlphaManualResultIntakeSkippedStepCount ?? 0,
            MissingStepCount:
                summary?.OfflineGeoworldAlphaManualResultIntakeMissingStepCount ?? 0,
            DuplicateStepCount:
                summary?.OfflineGeoworldAlphaManualResultIntakeDuplicateStepCount ?? 0,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal111.manual_result_intake.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal111AlphaManualResultIntakeQualityDiagnostics(
        Goal111AlphaManualResultIntakeWorkspaceQuality resultIntake,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(resultIntake.GroupPresent, "goal111.quality.manual_result_intake_group",
            "offline_geoworld_alpha_manual_result_intake", diagnostics);
        AddIfFalse(resultIntake.Goal110PackagePresent, "goal111.quality.goal110_package",
            "offline_geoworld_alpha_manual_result_intake", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(resultIntake.DecisionStatus),
            "goal111.quality.decision_status", "offline_geoworld_alpha_manual_result_intake", diagnostics);
        AddIfFalse(!resultIntake.AcceptedByCodex, "goal111.quality.accepted_by_codex_false",
            "offline_geoworld_alpha_manual_result_intake", diagnostics);
        AddIfFalse(resultIntake.HumanAcceptanceStillRequired,
            "goal111.quality.human_gate_still_required",
            "offline_geoworld_alpha_manual_result_intake",
            diagnostics);
        AddIfFalse(resultIntake.ChecklistHashMatched, "goal111.quality.checklist_hash",
            "offline_geoworld_alpha_manual_result_intake", diagnostics);
        AddIfFalse(resultIntake.QualityGatePassed, "goal111.quality.quality_gate",
            "proofStatus", diagnostics);
        AddIfFalse(resultIntake.RelativePaths, "goal111.quality.relative_paths",
            "offline_geoworld_alpha_manual_result_intake", diagnostics);
        AddIfFalse(binding.PageBindDisplaysOfflineGeoworldAlphaManualResultIntake,
            "goal111.quality.winforms_offline_geoworld_alpha_manual_result_intake_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal111AllowedPath(string path) =>
        path.StartsWith(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAlphaManualResultIntakeVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate ApplyGoal111AlphaManualResultIntakeQuality(
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        Goal111AlphaManualResultIntakeWorkspaceQuality resultIntake,
        VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            OfflineGeoworldAlphaManualResultIntakeGroupPresent = resultIntake.GroupPresent,
            OfflineGeoworldAlphaManualResultIntakeGoal110PackagePresent =
                resultIntake.Goal110PackagePresent,
            OfflineGeoworldAlphaManualResultIntakeResultFilePresent =
                resultIntake.ResultFilePresent,
            OfflineGeoworldAlphaManualResultIntakeDecisionStatus = resultIntake.DecisionStatus,
            OfflineGeoworldAlphaManualResultIntakeAcceptableCandidate =
                resultIntake.AcceptableCandidate,
            OfflineGeoworldAlphaManualResultIntakeAcceptedByCodex =
                resultIntake.AcceptedByCodex,
            OfflineGeoworldAlphaManualResultIntakeHumanAcceptanceStillRequired =
                resultIntake.HumanAcceptanceStillRequired,
            OfflineGeoworldAlphaManualResultIntakeChecklistHashMatched =
                resultIntake.ChecklistHashMatched,
            OfflineGeoworldAlphaManualResultIntakePassedStepCount =
                resultIntake.PassedStepCount,
            OfflineGeoworldAlphaManualResultIntakeFailedStepCount =
                resultIntake.FailedStepCount,
            OfflineGeoworldAlphaManualResultIntakePendingStepCount =
                resultIntake.PendingStepCount,
            OfflineGeoworldAlphaManualResultIntakeSkippedStepCount =
                resultIntake.SkippedStepCount,
            OfflineGeoworldAlphaManualResultIntakeMissingStepCount =
                resultIntake.MissingStepCount,
            OfflineGeoworldAlphaManualResultIntakeDuplicateStepCount =
                resultIntake.DuplicateStepCount,
            OfflineGeoworldAlphaManualResultIntakeQualityGatePassed =
                resultIntake.QualityGatePassed,
            Goal111FilesDiscoveredByRelativePaths = resultIntake.RelativePaths,
            WinFormsOfflineGeoworldAlphaManualResultIntakeBindingReal =
                binding.PageBindDisplaysOfflineGeoworldAlphaManualResultIntake
        };

    private sealed record Goal111AlphaManualResultIntakeWorkspaceQuality(
        bool GroupPresent,
        bool Goal110PackagePresent,
        bool ResultFilePresent,
        string DecisionStatus,
        bool AcceptableCandidate,
        bool AcceptedByCodex,
        bool HumanAcceptanceStillRequired,
        bool ChecklistHashMatched,
        int PassedStepCount,
        int FailedStepCount,
        int PendingStepCount,
        int SkippedStepCount,
        int MissingStepCount,
        int DuplicateStepCount,
        bool QualityGatePassed,
        bool RelativePaths);
}
