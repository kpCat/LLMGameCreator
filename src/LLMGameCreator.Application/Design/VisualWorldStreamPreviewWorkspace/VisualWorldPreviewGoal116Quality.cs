using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualGateAcceptanceRecord;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal116AlphaManualGateAcceptanceQuality
        BuildGoal116AlphaManualGateAcceptanceQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "offline_geoworld_alpha_manual_gate_acceptance_record");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_alpha_manual_gate_acceptance_record_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal116AllowedPath(entry.RelativePath));
        return new Goal116AlphaManualGateAcceptanceQuality(
            GroupPresent: group is not null,
            ManualGate: summary?.OfflineGeoworldAlphaManualGateAcceptanceManualGate ?? string.Empty,
            ManualGateStatus:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceManualGateStatus ?? string.Empty,
            HumanAccepted:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceHumanAccepted == true,
            HumanDecisionStatement:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceHumanDecisionStatement ?? string.Empty,
            SourceDecisionStatus:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceSourceDecisionStatus ?? string.Empty,
            ManualResultSha256:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceManualResultSha256 ?? string.Empty,
            AcceptedByCodex:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceAcceptedByCodex == true,
            ManualInputNotCommitted:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted == true,
            RawManualResultEmbeddedInArtifacts:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts == true,
            RecommendedNextDecision:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceRecommendedNextDecision ?? string.Empty,
            NotFinalReleaseOrRuntimeBuild:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceNotFinalReleaseOrRuntimeBuild == true,
            NoRuntimeProviderOrNetworkChanges:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceNoRuntimeProviderOrNetworkChanges == true,
            NoUnityFileChangesRequired:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceNoUnityFileChangesRequired == true,
            RequiredStepCount:
                summary?.OfflineGeoworldAlphaManualGateAcceptanceRequiredStepCount ?? 0,
            PassedStepCount:
                summary?.OfflineGeoworldAlphaManualGateAcceptancePassedStepCount ?? 0,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal116.manual_gate.quality_gate" && proof.Passed),
            RelativePaths: relativePaths);
    }

    private static void AddGoal116AlphaManualGateAcceptanceQualityDiagnostics(
        Goal116AlphaManualGateAcceptanceQuality acceptance,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(acceptance.GroupPresent, "goal116.quality.acceptance_group",
            "offline_geoworld_alpha_manual_gate_acceptance_record", diagnostics);
        AddIfFalse(
            acceptance.ManualGate
            == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGate,
            "goal116.quality.manual_gate",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(
            acceptance.ManualGateStatus
            == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ManualGateStatusAccepted,
            "goal116.quality.manual_gate_status",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(acceptance.HumanAccepted, "goal116.quality.human_accepted",
            "offline_geoworld_alpha_manual_gate_acceptance_record", diagnostics);
        AddIfFalse(
            acceptance.HumanDecisionStatement
            == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.HumanDecisionStatement,
            "goal116.quality.human_decision_statement",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(
            acceptance.SourceDecisionStatus
            == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary
                .SourceDecisionStatusGreenCandidate,
            "goal116.quality.source_decision_green",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(
            acceptance.ManualResultSha256
            == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExpectedManualResultSha256,
            "goal116.quality.manual_result_sha",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(!acceptance.AcceptedByCodex, "goal116.quality.accepted_by_codex_false",
            "offline_geoworld_alpha_manual_gate_acceptance_record", diagnostics);
        AddIfFalse(acceptance.ManualInputNotCommitted,
            "goal116.quality.manual_input_not_committed",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(!acceptance.RawManualResultEmbeddedInArtifacts,
            "goal116.quality.raw_manual_result_not_embedded",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(
            acceptance.RecommendedNextDecision
            == OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.RecommendedNextDecision,
            "goal116.quality.recommended_next_decision",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(acceptance.NotFinalReleaseOrRuntimeBuild,
            "goal116.quality.not_final_release",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(acceptance.NoRuntimeProviderOrNetworkChanges,
            "goal116.quality.no_runtime_provider_network",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(acceptance.NoUnityFileChangesRequired,
            "goal116.quality.no_unity_file_changes",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(acceptance.RequiredStepCount == 12,
            "goal116.quality.required_step_count",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(acceptance.PassedStepCount == 12,
            "goal116.quality.passed_step_count",
            "offline_geoworld_alpha_manual_gate_acceptance_record",
            diagnostics);
        AddIfFalse(acceptance.QualityGatePassed, "goal116.quality.quality_gate",
            "proofStatus", diagnostics);
        AddIfFalse(acceptance.RelativePaths, "goal116.quality.relative_paths",
            "offline_geoworld_alpha_manual_gate_acceptance_record", diagnostics);
        AddIfFalse(binding.PageBindDisplaysOfflineGeoworldAlphaManualGateAcceptanceRecord,
            "goal116.quality.winforms_offline_geoworld_alpha_manual_gate_acceptance_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal116AllowedPath(string path) =>
        path.StartsWith(
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAlphaManualGateAcceptanceRecordVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate ApplyGoal116AlphaManualGateAcceptanceQuality(
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        Goal116AlphaManualGateAcceptanceQuality acceptance,
        VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            OfflineGeoworldAlphaManualGateAcceptanceGroupPresent = acceptance.GroupPresent,
            OfflineGeoworldAlphaManualGateAcceptanceManualGate = acceptance.ManualGate,
            OfflineGeoworldAlphaManualGateAcceptanceManualGateStatus =
                acceptance.ManualGateStatus,
            OfflineGeoworldAlphaManualGateAcceptanceHumanAccepted = acceptance.HumanAccepted,
            OfflineGeoworldAlphaManualGateAcceptanceHumanDecisionStatement =
                acceptance.HumanDecisionStatement,
            OfflineGeoworldAlphaManualGateAcceptanceSourceDecisionStatus =
                acceptance.SourceDecisionStatus,
            OfflineGeoworldAlphaManualGateAcceptanceManualResultSha256 =
                acceptance.ManualResultSha256,
            OfflineGeoworldAlphaManualGateAcceptanceAcceptedByCodex =
                acceptance.AcceptedByCodex,
            OfflineGeoworldAlphaManualGateAcceptanceManualInputNotCommitted =
                acceptance.ManualInputNotCommitted,
            OfflineGeoworldAlphaManualGateAcceptanceRawManualResultEmbeddedInArtifacts =
                acceptance.RawManualResultEmbeddedInArtifacts,
            OfflineGeoworldAlphaManualGateAcceptanceRecommendedNextDecision =
                acceptance.RecommendedNextDecision,
            OfflineGeoworldAlphaManualGateAcceptanceNotFinalReleaseOrRuntimeBuild =
                acceptance.NotFinalReleaseOrRuntimeBuild,
            OfflineGeoworldAlphaManualGateAcceptanceNoRuntimeProviderOrNetworkChanges =
                acceptance.NoRuntimeProviderOrNetworkChanges,
            OfflineGeoworldAlphaManualGateAcceptanceNoUnityFileChangesRequired =
                acceptance.NoUnityFileChangesRequired,
            OfflineGeoworldAlphaManualGateAcceptanceRequiredStepCount =
                acceptance.RequiredStepCount,
            OfflineGeoworldAlphaManualGateAcceptancePassedStepCount =
                acceptance.PassedStepCount,
            OfflineGeoworldAlphaManualGateAcceptanceQualityGatePassed =
                acceptance.QualityGatePassed,
            Goal116FilesDiscoveredByRelativePaths = acceptance.RelativePaths,
            WinFormsOfflineGeoworldAlphaManualGateAcceptanceBindingReal =
                binding.PageBindDisplaysOfflineGeoworldAlphaManualGateAcceptanceRecord
        };

    private sealed record Goal116AlphaManualGateAcceptanceQuality(
        bool GroupPresent,
        string ManualGate,
        string ManualGateStatus,
        bool HumanAccepted,
        string HumanDecisionStatement,
        string SourceDecisionStatus,
        string ManualResultSha256,
        bool AcceptedByCodex,
        bool ManualInputNotCommitted,
        bool RawManualResultEmbeddedInArtifacts,
        string RecommendedNextDecision,
        bool NotFinalReleaseOrRuntimeBuild,
        bool NoRuntimeProviderOrNetworkChanges,
        bool NoUnityFileChangesRequired,
        int RequiredStepCount,
        int PassedStepCount,
        bool QualityGatePassed,
        bool RelativePaths);
}
