using LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceManualAcceptanceGate;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal110AlphaManualAcceptanceWorkspaceQuality BuildGoal110AlphaManualAcceptanceQuality(
        IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item => item.GroupId == "offline_geoworld_alpha_manual_acceptance");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "offline_geoworld_alpha_manual_acceptance_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal110AllowedPath(entry.RelativePath));
        return new Goal110AlphaManualAcceptanceWorkspaceQuality(
            GroupPresent: group is not null,
            ChecklistStepCount: summary?.OfflineGeoworldAlphaManualAcceptanceChecklistStepCount ?? 0,
            PayloadFileCount: summary?.OfflineGeoworldAlphaManualAcceptancePayloadFileCount ?? 0,
            ExportFileCount: summary?.OfflineGeoworldAlphaManualAcceptanceExportFileCount ?? 0,
            AutomatedGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal110.manual_acceptance.manifest" && proof.Passed),
            ManualPending: summary?.OfflineGeoworldAlphaManualAcceptanceManualPending == true,
            UnityRunnerReady: summary?.OfflineGeoworldAlphaManualAcceptanceUnityRunnerReady == true,
            SimulatedProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal110.manual_acceptance.simulated_proof" && proof.Passed),
            NegativeProofPassed: proofs.Any(proof =>
                proof.ProofId == "goal110.manual_acceptance.negative_proof" && proof.Passed),
            WorkspaceBindingPassed: proofs.Any(proof =>
                proof.ProofId == "goal110.manual_acceptance.workspace_binding" && proof.Passed),
            AlphaRuntimeBootstrapUnchanged:
                summary?.OfflineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal110.manual_acceptance.quality_gate" && proof.Passed),
            ResultTemplatePath: summary?.OfflineGeoworldAlphaManualAcceptanceResultTemplatePath ?? string.Empty,
            ReleaseRiskLinks: summary?.OfflineGeoworldAlphaManualAcceptanceReleaseRiskLinks ?? string.Empty,
            MilestoneGateLinks: summary?.OfflineGeoworldAlphaManualAcceptanceMilestoneGateLinks ?? string.Empty,
            RelativePaths: relativePaths);
    }

    private static void AddGoal110AlphaManualAcceptanceQualityDiagnostics(
        Goal110AlphaManualAcceptanceWorkspaceQuality manualAcceptance,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(manualAcceptance.GroupPresent, "goal110.quality.manual_acceptance_group",
            "offline_geoworld_alpha_manual_acceptance", diagnostics);
        AddIfFalse(manualAcceptance.ChecklistStepCount >= 12, "goal110.quality.checklist_step_count",
            "offline_geoworld_alpha_manual_acceptance", diagnostics);
        AddIfFalse(manualAcceptance.PayloadFileCount == 5, "goal110.quality.payload_file_count",
            "offline_geoworld_alpha_manual_acceptance", diagnostics);
        AddIfFalse(manualAcceptance.ExportFileCount == 7, "goal110.quality.export_file_count",
            "offline_geoworld_alpha_manual_acceptance", diagnostics);
        AddIfFalse(manualAcceptance.AutomatedGatePassed, "goal110.quality.automated_gate",
            "proofStatus", diagnostics);
        AddIfFalse(manualAcceptance.ManualPending, "goal110.quality.manual_pending",
            "offline_geoworld_alpha_manual_acceptance", diagnostics);
        AddIfFalse(manualAcceptance.UnityRunnerReady, "goal110.quality.unity_runner",
            "offline_geoworld_alpha_manual_acceptance", diagnostics);
        AddIfFalse(manualAcceptance.SimulatedProofPassed, "goal110.quality.simulated_proof",
            "proofStatus", diagnostics);
        AddIfFalse(manualAcceptance.NegativeProofPassed, "goal110.quality.negative_proof",
            "proofStatus", diagnostics);
        AddIfFalse(manualAcceptance.WorkspaceBindingPassed, "goal110.quality.workspace_binding",
            "proofStatus", diagnostics);
        AddIfFalse(manualAcceptance.AlphaRuntimeBootstrapUnchanged,
            "goal110.quality.alpha_bootstrap", "offline_geoworld_alpha_manual_acceptance", diagnostics);
        AddIfFalse(manualAcceptance.QualityGatePassed, "goal110.quality.quality_gate",
            "proofStatus", diagnostics);
        AddIfFalse(!string.IsNullOrWhiteSpace(manualAcceptance.ResultTemplatePath),
            "goal110.quality.result_template_path", "offline_geoworld_alpha_manual_acceptance", diagnostics);
        AddIfFalse(manualAcceptance.ReleaseRiskLinks.Contains("playable_quality", StringComparison.Ordinal),
            "goal110.quality.release_risk_links", "offline_geoworld_alpha_manual_acceptance", diagnostics);
        AddIfFalse(manualAcceptance.MilestoneGateLinks.Contains("vertical_slice", StringComparison.Ordinal),
            "goal110.quality.milestone_links", "offline_geoworld_alpha_manual_acceptance", diagnostics);
        AddIfFalse(manualAcceptance.RelativePaths, "goal110.quality.relative_paths",
            "offline_geoworld_alpha_manual_acceptance", diagnostics);
    }

    private static bool Goal110AllowedPath(string path) =>
        path.StartsWith(
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.StreamingAssetsRelativeRoot + "/",
            StringComparison.Ordinal)
        || path == OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultScriptPath
        || path == OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityResultStoreScriptPath
        || path == OfflineGeoworldAlphaSliceManualAcceptanceGateVocabulary.UnityEditorWindowScriptPath;

    private static VisualWorldPreviewWorkspaceQualityGate ApplyGoal110AlphaManualAcceptanceQuality(
        VisualWorldPreviewWorkspaceQualityGate qualityGate,
        Goal110AlphaManualAcceptanceWorkspaceQuality manualAcceptance,
        VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            OfflineGeoworldAlphaManualAcceptanceGroupPresent = manualAcceptance.GroupPresent,
            OfflineGeoworldAlphaManualAcceptanceChecklistStepCount =
                manualAcceptance.ChecklistStepCount,
            OfflineGeoworldAlphaManualAcceptancePayloadFileCount = manualAcceptance.PayloadFileCount,
            OfflineGeoworldAlphaManualAcceptanceExportFileCount = manualAcceptance.ExportFileCount,
            OfflineGeoworldAlphaManualAcceptanceAutomatedGatePassed =
                manualAcceptance.AutomatedGatePassed,
            OfflineGeoworldAlphaManualAcceptanceManualPending = manualAcceptance.ManualPending,
            OfflineGeoworldAlphaManualAcceptanceUnityRunnerReady =
                manualAcceptance.UnityRunnerReady,
            OfflineGeoworldAlphaManualAcceptanceSimulatedProofPassed =
                manualAcceptance.SimulatedProofPassed,
            OfflineGeoworldAlphaManualAcceptanceNegativeProofPassed =
                manualAcceptance.NegativeProofPassed,
            OfflineGeoworldAlphaManualAcceptanceWorkspaceBindingPassed =
                manualAcceptance.WorkspaceBindingPassed,
            OfflineGeoworldAlphaManualAcceptanceAlphaRuntimeBootstrapUnchanged =
                manualAcceptance.AlphaRuntimeBootstrapUnchanged,
            OfflineGeoworldAlphaManualAcceptanceQualityGatePassed =
                manualAcceptance.QualityGatePassed,
            OfflineGeoworldAlphaManualAcceptanceResultTemplatePath =
                manualAcceptance.ResultTemplatePath,
            OfflineGeoworldAlphaManualAcceptanceReleaseRiskLinks =
                manualAcceptance.ReleaseRiskLinks,
            OfflineGeoworldAlphaManualAcceptanceMilestoneGateLinks =
                manualAcceptance.MilestoneGateLinks,
            Goal110FilesDiscoveredByRelativePaths = manualAcceptance.RelativePaths,
            WinFormsOfflineGeoworldAlphaManualAcceptanceBindingReal =
                binding.PageBindDisplaysOfflineGeoworldAlphaManualAcceptance
        };

    private sealed record Goal110AlphaManualAcceptanceWorkspaceQuality(
        bool GroupPresent,
        int ChecklistStepCount,
        int PayloadFileCount,
        int ExportFileCount,
        bool AutomatedGatePassed,
        bool ManualPending,
        bool UnityRunnerReady,
        bool SimulatedProofPassed,
        bool NegativeProofPassed,
        bool WorkspaceBindingPassed,
        bool AlphaRuntimeBootstrapUnchanged,
        bool QualityGatePassed,
        string ResultTemplatePath,
        string ReleaseRiskLinks,
        string MilestoneGateLinks,
        bool RelativePaths);
}
