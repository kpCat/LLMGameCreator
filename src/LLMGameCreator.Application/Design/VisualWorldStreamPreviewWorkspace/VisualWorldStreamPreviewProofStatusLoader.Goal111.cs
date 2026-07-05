using LLMGameCreator.Application.Design.OfflineGeoworldAlphaManualResultIntake;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static IReadOnlyList<VisualWorldPreviewProofStatus>
        BuildGoal111OfflineGeoworldAlphaManualResultIntakeProofStatus(
            string projectRoot,
            List<VisualWorldPreviewDiagnostic> diagnostics)
    {
        var root = OfflineGeoworldAlphaManualResultIntakeVocabulary.ProceduralOutputDirectory;
        var goalId = OfflineGeoworldAlphaManualResultIntakeVocabulary.GoalId;
        var ledger = new Dictionary<string, string>(StringComparer.Ordinal);
        return
        [
            BuildProof(projectRoot, root, goalId, "goal111.manual_result_intake.decision",
                OfflineGeoworldAlphaManualResultIntakeVocabulary.DecisionFileName,
                "humanAcceptanceStillRequired", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal111.manual_result_intake.quality_gate",
                OfflineGeoworldAlphaManualResultIntakeVocabulary.QualityGateScanFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal111.manual_result_intake.missing_result_negative",
                OfflineGeoworldAlphaManualResultIntakeVocabulary.MissingResultProofFileName,
                "passed", ledger, diagnostics),
            BuildProof(projectRoot, root, goalId, "goal111.manual_result_intake.invalid_result_negative",
                OfflineGeoworldAlphaManualResultIntakeVocabulary.InvalidResultProofFileName,
                "passed", ledger, diagnostics)
        ];
    }
}
