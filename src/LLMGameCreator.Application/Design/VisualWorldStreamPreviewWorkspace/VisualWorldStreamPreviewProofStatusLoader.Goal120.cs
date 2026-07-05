using LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal120AcceptedAlphaProjectionUsabilityProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = AcceptedAlphaProjectionUsabilityVocabulary.ProceduralOutputDirectory;
        var goalId = AcceptedAlphaProjectionUsabilityVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal120.usability.legend",
                AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName,
                "legendPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal120.usability.marker_descriptor",
                AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName,
                "markerDescriptorPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal120.usability.selection_controls",
                AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName,
                "selectionControlsPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal120.usability.focus_camera",
                AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName,
                "focusCameraControlPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal120.usability.material_warning_guard",
                AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName,
                "materialWarningGuardPresent", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal120.usability.cleanup_script",
                AcceptedAlphaProjectionUsabilityVocabulary.CleanupScriptScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal120.usability.negative_proof",
                AcceptedAlphaProjectionUsabilityVocabulary.NegativeProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal120.usability.do_not_start",
                AcceptedAlphaProjectionUsabilityVocabulary.DashboardFileName,
                "doNotStartAutomatically", ledger, diagnostics)
        ];
    }
}
