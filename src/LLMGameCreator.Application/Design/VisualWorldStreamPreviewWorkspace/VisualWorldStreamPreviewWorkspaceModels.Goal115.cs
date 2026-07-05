namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualResultPresent { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualResultJsonValid { get; init; }
    public string OfflineGeoworldAlphaHumanResultRevalidationManualResultPath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaHumanResultRevalidationManualResultSha256 { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate { get; init; }
    public string OfflineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision { get; init; }
    public int OfflineGeoworldAlphaHumanResultRevalidationRequiredStepCount { get; init; }
    public int OfflineGeoworldAlphaHumanResultRevalidationPassedStepCount { get; init; }
    public int OfflineGeoworldAlphaHumanResultRevalidationBlockingStepIssueCount { get; init; }
    public string OfflineGeoworldAlphaHumanResultRevalidationStepSummary { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaHumanResultRevalidationErrors { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaHumanResultRevalidationWarnings { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaHumanResultRevalidationProceduralPath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaHumanResultRevalidationExportPath { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysOfflineGeoworldAlphaHumanResultRevalidation { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool OfflineGeoworldAlphaHumanResultRevalidationGroupPresent { get; init; }
    public string OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualResultPresent { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualResultJsonValid { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate { get; init; }
    public string OfflineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision { get; init; }
    public int OfflineGeoworldAlphaHumanResultRevalidationRequiredStepCount { get; init; }
    public int OfflineGeoworldAlphaHumanResultRevalidationPassedStepCount { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationQualityGatePassed { get; init; }
    public bool Goal115FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsOfflineGeoworldAlphaHumanResultRevalidationBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string OfflineGeoworldAlphaHumanResultRevalidationDecisionStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaHumanResultRevalidationGoal111DecisionStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualResultPresent { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualResultJsonValid { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationAcceptableCandidate { get; init; }
    public string OfflineGeoworldAlphaHumanResultRevalidationRecommendedHumanDecision { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaHumanResultRevalidationAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationHumanAcceptanceStillRequired { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualGateRemainsHumanDecision { get; init; }
    public int OfflineGeoworldAlphaHumanResultRevalidationRequiredStepCount { get; init; }
    public int OfflineGeoworldAlphaHumanResultRevalidationPassedStepCount { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationManualInputNotCommitted { get; init; }
    public bool OfflineGeoworldAlphaHumanResultRevalidationQualityGatePassed { get; init; }
    public bool Goal115FilesDiscoveredByRelativePaths { get; init; }
}
