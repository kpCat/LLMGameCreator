using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaHumanResultRevalidationDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "offlineGeoworldAlphaHumanResultRevalidationDecisionStatus="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus,
        "offlineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus,
        "offlineGeoworldAlphaHumanResultRevalidationManualResultPresent="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationManualResultPresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationManualResultJsonValid="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationManualResultJsonValid
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationAcceptableCandidate="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision,
        "offlineGeoworldAlphaHumanResultRevalidationAcceptedByCodex="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationRequiredStepCount="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationRequiredStepCount,
        "offlineGeoworldAlphaHumanResultRevalidationPassedStepCount="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationPassedStepCount,
        "offlineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationQualityGatePassed="
            + result.Report.OfflineGeoworldAlphaHumanResultRevalidationQualityGatePassed
                .ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildOfflineGeoworldAlphaHumanResultRevalidationEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "offlineGeoworldAlphaHumanResultRevalidationDecisionStatus: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus,
        "offlineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus,
        "offlineGeoworldAlphaHumanResultRevalidationManualResultPresent: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationManualResultPresent
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationManualResultJsonValid: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationManualResultJsonValid
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationManualResultPath: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationManualResultPath,
        "offlineGeoworldAlphaHumanResultRevalidationManualResultSha256: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationManualResultSha256,
        "offlineGeoworldAlphaHumanResultRevalidationAcceptableCandidate: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision,
        "offlineGeoworldAlphaHumanResultRevalidationAcceptedByCodex: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision
                .ToString().ToLowerInvariant(),
        "offlineGeoworldAlphaHumanResultRevalidationRequiredStepCount: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationRequiredStepCount,
        "offlineGeoworldAlphaHumanResultRevalidationPassedStepCount: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationPassedStepCount,
        "offlineGeoworldAlphaHumanResultRevalidationBlockingStepIssueCount: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationBlockingStepIssueCount,
        "offlineGeoworldAlphaHumanResultRevalidationStepSummary: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationStepSummary,
        "offlineGeoworldAlphaHumanResultRevalidationErrors: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationErrors,
        "offlineGeoworldAlphaHumanResultRevalidationWarnings: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationWarnings,
        "offlineGeoworldAlphaHumanResultRevalidationProceduralPath: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationProceduralPath,
        "offlineGeoworldAlphaHumanResultRevalidationExportPath: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationExportPath,
        "offlineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted: "
            + entry.OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted
                .ToString().ToLowerInvariant()
    ];
}
