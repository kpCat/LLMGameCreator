using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal121AcceptedAlphaInteractionDrilldownQuality
        BuildGoal121AcceptedAlphaInteractionDrilldownQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "accepted_alpha_interaction_drilldown_verification");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "accepted_alpha_interaction_drilldown_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal121AllowedPath(entry.RelativePath));
        return new Goal121AcceptedAlphaInteractionDrilldownQuality(
            GroupPresent: group is not null,
            FullVerificationStatus:
                summary?.AcceptedAlphaInteractionDrilldownFullVerificationStatus ?? string.Empty,
            UnityMenuPath: summary?.AcceptedAlphaInteractionDrilldownUnityMenuPath ?? string.Empty,
            OneClickButtonPresent: summary?.AcceptedAlphaInteractionDrilldownOneClickButtonPresent == true,
            DrilldownFieldsPresent: summary?.AcceptedAlphaInteractionDrilldownDrilldownFieldsPresent == true,
            InteractionPreviewPresent:
                summary?.AcceptedAlphaInteractionDrilldownInteractionPreviewPresent == true,
            ObjectiveReplayDetailsPresent:
                summary?.AcceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent == true,
            BatchmodeFullVerificationMarker:
                summary?.AcceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker ?? string.Empty,
            CleanupScriptAvailable:
                summary?.AcceptedAlphaInteractionDrilldownCleanupScriptAvailable == true,
            MaterialWarningGuardPresent:
                summary?.AcceptedAlphaInteractionDrilldownMaterialWarningGuardPresent == true,
            HumanManualStepsReducedToOneButton:
                summary?.AcceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton == true,
            UnityBatchmodeLogStatus:
                summary?.AcceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus ?? string.Empty,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal121.full_verification.one_click_button" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal121.full_verification.drilldown_fields" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal121.full_verification.interaction_preview" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal121.full_verification.objective_replay_details" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal121.full_verification.cleanup_script" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal121.full_verification.material_warning_guard" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal121.full_verification.human_steps_one_button" && proof.Passed)
                && proofs.Any(proof =>
                    proof.ProofId == "goal121.full_verification.negative_proof" && proof.Passed)
                && (summary?.AcceptedAlphaInteractionDrilldownFullVerificationStatus
                    == AcceptedAlphaInteractionDrilldownVerificationVocabulary.FullVerificationStatus),
            RelativePaths: relativePaths);
    }

    private static void AddGoal121AcceptedAlphaInteractionDrilldownQualityDiagnostics(
        Goal121AcceptedAlphaInteractionDrilldownQuality drilldown,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(drilldown.GroupPresent, "goal121.quality.drilldown_group",
            "accepted_alpha_interaction_drilldown_verification", diagnostics);
        AddIfFalse(
            drilldown.FullVerificationStatus
            == AcceptedAlphaInteractionDrilldownVerificationVocabulary.FullVerificationStatus,
            "goal121.quality.full_verification_status",
            "accepted_alpha_interaction_drilldown_verification",
            diagnostics);
        AddIfFalse(
            drilldown.UnityMenuPath == AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            "goal121.quality.unity_menu_path",
            "accepted_alpha_interaction_drilldown_verification",
            diagnostics);
        AddIfFalse(drilldown.OneClickButtonPresent, "goal121.quality.one_click_button",
            "accepted_alpha_interaction_drilldown_verification", diagnostics);
        AddIfFalse(drilldown.DrilldownFieldsPresent, "goal121.quality.drilldown_fields",
            "accepted_alpha_interaction_drilldown_verification", diagnostics);
        AddIfFalse(drilldown.InteractionPreviewPresent, "goal121.quality.interaction_preview",
            "accepted_alpha_interaction_drilldown_verification", diagnostics);
        AddIfFalse(drilldown.ObjectiveReplayDetailsPresent, "goal121.quality.objective_replay_details",
            "accepted_alpha_interaction_drilldown_verification", diagnostics);
        AddIfFalse(
            drilldown.BatchmodeFullVerificationMarker == "GOAL121_FULL_PROJECTION_VERIFICATION_PASS",
            "goal121.quality.batchmode_marker",
            "accepted_alpha_interaction_drilldown_verification",
            diagnostics);
        AddIfFalse(drilldown.CleanupScriptAvailable, "goal121.quality.cleanup_script",
            "accepted_alpha_interaction_drilldown_verification", diagnostics);
        AddIfFalse(drilldown.MaterialWarningGuardPresent, "goal121.quality.material_warning_guard",
            "accepted_alpha_interaction_drilldown_verification", diagnostics);
        AddIfFalse(drilldown.HumanManualStepsReducedToOneButton, "goal121.quality.one_button_manual_path",
            "accepted_alpha_interaction_drilldown_verification", diagnostics);
        AddIfFalse(drilldown.QualityGatePassed, "goal121.quality.quality_gate",
            "accepted_alpha_interaction_drilldown_verification", diagnostics);
        AddIfFalse(drilldown.RelativePaths, "goal121.quality.relative_paths",
            "accepted_alpha_interaction_drilldown_verification", diagnostics);
        AddIfFalse(binding.PageBindDisplaysAcceptedAlphaInteractionDrilldown,
            "goal121.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal121AllowedPath(string path) =>
        path.StartsWith(
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            AcceptedAlphaInteractionDrilldownVerificationVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal121AcceptedAlphaInteractionDrilldownQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal121AcceptedAlphaInteractionDrilldownQuality drilldown,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            AcceptedAlphaInteractionDrilldownGroupPresent = drilldown.GroupPresent,
            AcceptedAlphaInteractionDrilldownFullVerificationStatus =
                drilldown.FullVerificationStatus,
            AcceptedAlphaInteractionDrilldownUnityMenuPath = drilldown.UnityMenuPath,
            AcceptedAlphaInteractionDrilldownOneClickButtonPresent =
                drilldown.OneClickButtonPresent,
            AcceptedAlphaInteractionDrilldownDrilldownFieldsPresent =
                drilldown.DrilldownFieldsPresent,
            AcceptedAlphaInteractionDrilldownInteractionPreviewPresent =
                drilldown.InteractionPreviewPresent,
            AcceptedAlphaInteractionDrilldownObjectiveReplayDetailsPresent =
                drilldown.ObjectiveReplayDetailsPresent,
            AcceptedAlphaInteractionDrilldownBatchmodeFullVerificationMarker =
                drilldown.BatchmodeFullVerificationMarker,
            AcceptedAlphaInteractionDrilldownCleanupScriptAvailable =
                drilldown.CleanupScriptAvailable,
            AcceptedAlphaInteractionDrilldownMaterialWarningGuardPresent =
                drilldown.MaterialWarningGuardPresent,
            AcceptedAlphaInteractionDrilldownHumanManualStepsReducedToOneButton =
                drilldown.HumanManualStepsReducedToOneButton,
            AcceptedAlphaInteractionDrilldownUnityBatchmodeLogStatus =
                drilldown.UnityBatchmodeLogStatus,
            AcceptedAlphaInteractionDrilldownQualityGatePassed =
                drilldown.QualityGatePassed,
            Goal121FilesDiscoveredByRelativePaths = drilldown.RelativePaths,
            WinFormsAcceptedAlphaInteractionDrilldownBindingReal =
                binding.PageBindDisplaysAcceptedAlphaInteractionDrilldown
        };

    private sealed record Goal121AcceptedAlphaInteractionDrilldownQuality(
        bool GroupPresent,
        string FullVerificationStatus,
        string UnityMenuPath,
        bool OneClickButtonPresent,
        bool DrilldownFieldsPresent,
        bool InteractionPreviewPresent,
        bool ObjectiveReplayDetailsPresent,
        string BatchmodeFullVerificationMarker,
        bool CleanupScriptAvailable,
        bool MaterialWarningGuardPresent,
        bool HumanManualStepsReducedToOneButton,
        string UnityBatchmodeLogStatus,
        bool QualityGatePassed,
        bool RelativePaths);
}
