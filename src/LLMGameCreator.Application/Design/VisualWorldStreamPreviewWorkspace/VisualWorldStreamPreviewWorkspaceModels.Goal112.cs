namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string OfflineGeoworldAlphaAcceptanceOperatorStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview { get; init; }
    public string OfflineGeoworldAlphaAcceptanceOperatorPreferredManualResultPath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaAcceptanceOperatorCandidateManualResultPaths { get; init; } = string.Empty;
    public int OfflineGeoworldAlphaAcceptanceOperatorChecklistStepCount { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorChecklistHashPresent { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired { get; init; }
    public string OfflineGeoworldAlphaAcceptanceOperatorNextHumanActions { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaAcceptanceOperatorDoNotStartYet { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaAcceptanceOperatorEvidencePath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaAcceptanceOperatorExportPath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaAcceptanceOperatorRunbookPath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaAcceptanceOperatorTopErrors { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaAcceptanceOperatorTopWarnings { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysOfflineGeoworldAlphaAcceptanceOperatorPack { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool OfflineGeoworldAlphaAcceptanceOperatorPackGroupPresent { get; init; }
    public string OfflineGeoworldAlphaAcceptanceOperatorStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired { get; init; }
    public int OfflineGeoworldAlphaAcceptanceOperatorChecklistStepCount { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorChecklistHashPresent { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorQualityGatePassed { get; init; }
    public bool Goal112FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsOfflineGeoworldAlphaAcceptanceOperatorBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string OfflineGeoworldAlphaAcceptanceOperatorStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaAcceptanceOperatorGoal111DecisionStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaAcceptanceOperatorManualResultPresent { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorManualResultAvailableForHumanReview { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorAcceptedByCodex { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorHumanAcceptanceStillRequired { get; init; }
    public int OfflineGeoworldAlphaAcceptanceOperatorChecklistStepCount { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorChecklistHashPresent { get; init; }
    public bool OfflineGeoworldAlphaAcceptanceOperatorQualityGatePassed { get; init; }
    public bool Goal112FilesDiscoveredByRelativePaths { get; init; }
}
