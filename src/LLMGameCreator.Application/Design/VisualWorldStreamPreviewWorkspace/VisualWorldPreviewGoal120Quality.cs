using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static Goal120AcceptedAlphaProjectionUsabilityQuality
        BuildGoal120AcceptedAlphaProjectionUsabilityQuality(
            IReadOnlyList<VisualWorldPreviewArtifactGroup> groups,
            IReadOnlyList<VisualWorldPreviewProofStatus> proofs)
    {
        var group = groups.FirstOrDefault(item =>
            item.GroupId == "accepted_alpha_projection_usability");
        var entries = group?.Entries.ToList() ?? [];
        var summary = entries.FirstOrDefault(entry =>
            entry.ArtifactKind == "accepted_alpha_projection_usability_workspace_summary");
        var relativePaths = entries.Count > 0
                            && entries.All(entry =>
                                IsSafeRelativePath(entry.RelativePath)
                                && Goal120AllowedPath(entry.RelativePath));
        return new Goal120AcceptedAlphaProjectionUsabilityQuality(
            GroupPresent: group is not null,
            UsabilityStatus: summary?.AcceptedAlphaProjectionUsabilityStatus ?? string.Empty,
            UnityMenuPath: summary?.AcceptedAlphaProjectionUsabilityUnityMenuPath ?? string.Empty,
            CleanupScriptPath: summary?.AcceptedAlphaProjectionUsabilityCleanupScriptPath ?? string.Empty,
            CleanupScriptCmdPath: summary?.AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath ?? string.Empty,
            LegendPresent: summary?.AcceptedAlphaProjectionUsabilityLegendPresent == true,
            MarkerDescriptorPresent:
                summary?.AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent == true,
            SelectionControlsPresent:
                summary?.AcceptedAlphaProjectionUsabilitySelectionControlsPresent == true,
            FocusCameraControlPresent:
                summary?.AcceptedAlphaProjectionUsabilityFocusCameraControlPresent == true,
            MaterialWarningGuardPresent:
                summary?.AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent == true,
            UnitySmokeStatus: summary?.AcceptedAlphaProjectionUsabilityUnitySmokeStatus ?? string.Empty,
            DoNotStartAutomatically:
                summary?.AcceptedAlphaProjectionUsabilityDoNotStartAutomatically == true,
            QualityGatePassed: proofs.Any(proof =>
                proof.ProofId == "goal120.usability.legend" && proof.Passed)
                && proofs.Any(proof => proof.ProofId == "goal120.usability.marker_descriptor" && proof.Passed)
                && proofs.Any(proof => proof.ProofId == "goal120.usability.selection_controls" && proof.Passed)
                && proofs.Any(proof => proof.ProofId == "goal120.usability.cleanup_script" && proof.Passed)
                && (summary?.AcceptedAlphaProjectionUsabilityStatus
                    == AcceptedAlphaProjectionUsabilityVocabulary.UsabilityStatus),
            RelativePaths: relativePaths);
    }

    private static void AddGoal120AcceptedAlphaProjectionUsabilityQualityDiagnostics(
        Goal120AcceptedAlphaProjectionUsabilityQuality usability,
        VisualWorldPreviewWinFormsBindingInventory binding,
        List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        AddIfFalse(usability.GroupPresent, "goal120.quality.usability_group",
            "accepted_alpha_projection_usability", diagnostics);
        AddIfFalse(
            usability.UsabilityStatus == AcceptedAlphaProjectionUsabilityVocabulary.UsabilityStatus,
            "goal120.quality.usability_status",
            "accepted_alpha_projection_usability",
            diagnostics);
        AddIfFalse(
            usability.UnityMenuPath == AcceptedAlphaUnityPlayableProjectionVocabulary.UnityMenuPath,
            "goal120.quality.unity_menu_path",
            "accepted_alpha_projection_usability",
            diagnostics);
        AddIfFalse(
            usability.CleanupScriptPath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptPath,
            "goal120.quality.cleanup_script_path",
            "accepted_alpha_projection_usability",
            diagnostics);
        AddIfFalse(
            usability.CleanupScriptCmdPath == AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptCmdPath,
            "goal120.quality.cleanup_script_cmd_path",
            "accepted_alpha_projection_usability",
            diagnostics);
        AddIfFalse(usability.LegendPresent, "goal120.quality.legend",
            "accepted_alpha_projection_usability", diagnostics);
        AddIfFalse(usability.MarkerDescriptorPresent, "goal120.quality.marker_descriptor",
            "accepted_alpha_projection_usability", diagnostics);
        AddIfFalse(usability.SelectionControlsPresent, "goal120.quality.selection_controls",
            "accepted_alpha_projection_usability", diagnostics);
        AddIfFalse(usability.FocusCameraControlPresent, "goal120.quality.focus_camera",
            "accepted_alpha_projection_usability", diagnostics);
        AddIfFalse(usability.MaterialWarningGuardPresent, "goal120.quality.material_warning_guard",
            "accepted_alpha_projection_usability", diagnostics);
        AddIfFalse(usability.DoNotStartAutomatically, "goal120.quality.do_not_start_automatically",
            "accepted_alpha_projection_usability", diagnostics);
        AddIfFalse(usability.QualityGatePassed, "goal120.quality.quality_gate",
            "accepted_alpha_projection_usability", diagnostics);
        AddIfFalse(usability.RelativePaths, "goal120.quality.relative_paths",
            "accepted_alpha_projection_usability", diagnostics);
        AddIfFalse(binding.PageBindDisplaysAcceptedAlphaProjectionUsability,
            "goal120.quality.winforms_binding",
            "winformsBinding",
            diagnostics);
    }

    private static bool Goal120AllowedPath(string path) =>
        path.StartsWith(
            AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory + "/",
            StringComparison.Ordinal)
        || path.StartsWith(
            AcceptedAlphaProjectionUsabilityVocabulary.ExportPackageDirectory + "/",
            StringComparison.Ordinal);

    private static VisualWorldPreviewWorkspaceQualityGate
        ApplyGoal120AcceptedAlphaProjectionUsabilityQuality(
            VisualWorldPreviewWorkspaceQualityGate qualityGate,
            Goal120AcceptedAlphaProjectionUsabilityQuality usability,
            VisualWorldPreviewWinFormsBindingInventory binding) =>
        qualityGate with
        {
            AcceptedAlphaProjectionUsabilityGroupPresent = usability.GroupPresent,
            AcceptedAlphaProjectionUsabilityStatus = usability.UsabilityStatus,
            AcceptedAlphaProjectionUsabilityUnityMenuPath = usability.UnityMenuPath,
            AcceptedAlphaProjectionUsabilityCleanupScriptPath = usability.CleanupScriptPath,
            AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath = usability.CleanupScriptCmdPath,
            AcceptedAlphaProjectionUsabilityLegendPresent = usability.LegendPresent,
            AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent =
                usability.MarkerDescriptorPresent,
            AcceptedAlphaProjectionUsabilitySelectionControlsPresent =
                usability.SelectionControlsPresent,
            AcceptedAlphaProjectionUsabilityFocusCameraControlPresent =
                usability.FocusCameraControlPresent,
            AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent =
                usability.MaterialWarningGuardPresent,
            AcceptedAlphaProjectionUsabilityUnitySmokeStatus = usability.UnitySmokeStatus,
            AcceptedAlphaProjectionUsabilityDoNotStartAutomatically =
                usability.DoNotStartAutomatically,
            AcceptedAlphaProjectionUsabilityQualityGatePassed =
                usability.QualityGatePassed,
            Goal120FilesDiscoveredByRelativePaths = usability.RelativePaths,
            WinFormsAcceptedAlphaProjectionUsabilityBindingReal =
                binding.PageBindDisplaysAcceptedAlphaProjectionUsability
        };

    private sealed record Goal120AcceptedAlphaProjectionUsabilityQuality(
        bool GroupPresent,
        string UsabilityStatus,
        string UnityMenuPath,
        string CleanupScriptPath,
        string CleanupScriptCmdPath,
        bool LegendPresent,
        bool MarkerDescriptorPresent,
        bool SelectionControlsPresent,
        bool FocusCameraControlPresent,
        bool MaterialWarningGuardPresent,
        string UnitySmokeStatus,
        bool DoNotStartAutomatically,
        bool QualityGatePassed,
        bool RelativePaths);
}
