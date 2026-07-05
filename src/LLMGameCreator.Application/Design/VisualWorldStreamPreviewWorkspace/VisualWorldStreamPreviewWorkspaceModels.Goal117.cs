namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string OfflineGeoworldAlphaPostAcceptanceManualGateStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaPostAcceptanceHumanAccepted { get; init; }
    public string OfflineGeoworldAlphaPostAcceptanceManualResultSha256 { get; init; } =
        string.Empty;
    public string OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane { get; init; } =
        string.Empty;
    public string OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId { get; init; } =
        string.Empty;
    public int OfflineGeoworldAlphaPostAcceptanceReadyLaneCount { get; init; }
    public int OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount { get; init; }
    public int OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount { get; init; }
    public bool OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically { get; init; }
    public string OfflineGeoworldAlphaPostAcceptanceEvidencePath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaPostAcceptanceExportPath { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaPostAcceptanceLaneIds { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaPostAcceptanceErrors { get; init; } = string.Empty;
    public string OfflineGeoworldAlphaPostAcceptanceWarnings { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysOfflineGeoworldAlphaPostAcceptanceContinuationSelection { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool OfflineGeoworldAlphaPostAcceptanceContinuationGroupPresent { get; init; }
    public string OfflineGeoworldAlphaPostAcceptanceManualGateStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaPostAcceptanceHumanAccepted { get; init; }
    public string OfflineGeoworldAlphaPostAcceptanceManualResultSha256 { get; init; } =
        string.Empty;
    public string OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane { get; init; } =
        string.Empty;
    public string OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId { get; init; } =
        string.Empty;
    public int OfflineGeoworldAlphaPostAcceptanceReadyLaneCount { get; init; }
    public int OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount { get; init; }
    public int OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount { get; init; }
    public bool OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically { get; init; }
    public bool OfflineGeoworldAlphaPostAcceptanceQualityGatePassed { get; init; }
    public bool Goal117FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsOfflineGeoworldAlphaPostAcceptanceBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string OfflineGeoworldAlphaPostAcceptanceManualGateStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaPostAcceptanceHumanAccepted { get; init; }
    public string OfflineGeoworldAlphaPostAcceptanceManualResultSha256 { get; init; } =
        string.Empty;
    public string OfflineGeoworldAlphaPostAcceptanceRecommendedNextLane { get; init; } =
        string.Empty;
    public string OfflineGeoworldAlphaPostAcceptanceRecommendedNextGoalId { get; init; } =
        string.Empty;
    public int OfflineGeoworldAlphaPostAcceptanceReadyLaneCount { get; init; }
    public int OfflineGeoworldAlphaPostAcceptanceCandidateLaneCount { get; init; }
    public int OfflineGeoworldAlphaPostAcceptanceBlockedLaneCount { get; init; }
    public bool OfflineGeoworldAlphaPostAcceptanceDoNotStartAutomatically { get; init; }
    public bool OfflineGeoworldAlphaPostAcceptanceQualityGatePassed { get; init; }
    public bool Goal117FilesDiscoveredByRelativePaths { get; init; }
}
