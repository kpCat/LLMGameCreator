namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string OfflineGeoworldAcceptedAlphaBaselineId { get; init; } = string.Empty;
    public string OfflineGeoworldAcceptedAlphaBaselineHash { get; init; } = string.Empty;
    public bool OfflineGeoworldAcceptedAlphaBaselineReady { get; init; }
    public string OfflineGeoworldAcceptedAlphaManualGateStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAcceptedAlphaRecommendedNextDecision { get; init; } =
        string.Empty;
    public int OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount { get; init; }
    public int OfflineGeoworldAcceptedAlphaAcceptedEvidenceRootCount { get; init; }
    public int OfflineGeoworldAcceptedAlphaProducedOnlyRootCount { get; init; }
    public int OfflineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount { get; init; }
    public bool OfflineGeoworldAcceptedAlphaDoNotStartAutomatically { get; init; }
    public string OfflineGeoworldAcceptedAlphaEvidencePath { get; init; } = string.Empty;
    public string OfflineGeoworldAcceptedAlphaExportPath { get; init; } = string.Empty;
    public string OfflineGeoworldAcceptedAlphaErrors { get; init; } = string.Empty;
    public string OfflineGeoworldAcceptedAlphaWarnings { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysOfflineGeoworldAcceptedAlphaBaselineReview { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool OfflineGeoworldAcceptedAlphaBaselineGroupPresent { get; init; }
    public string OfflineGeoworldAcceptedAlphaBaselineId { get; init; } = string.Empty;
    public string OfflineGeoworldAcceptedAlphaBaselineHash { get; init; } = string.Empty;
    public bool OfflineGeoworldAcceptedAlphaBaselineReady { get; init; }
    public string OfflineGeoworldAcceptedAlphaManualGateStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAcceptedAlphaRecommendedNextDecision { get; init; } =
        string.Empty;
    public int OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount { get; init; }
    public int OfflineGeoworldAcceptedAlphaAcceptedEvidenceRootCount { get; init; }
    public int OfflineGeoworldAcceptedAlphaProducedOnlyRootCount { get; init; }
    public int OfflineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount { get; init; }
    public bool OfflineGeoworldAcceptedAlphaDoNotStartAutomatically { get; init; }
    public bool OfflineGeoworldAcceptedAlphaQualityGatePassed { get; init; }
    public bool Goal118FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsOfflineGeoworldAcceptedAlphaBaselineBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string OfflineGeoworldAcceptedAlphaBaselineId { get; init; } = string.Empty;
    public string OfflineGeoworldAcceptedAlphaBaselineHash { get; init; } = string.Empty;
    public bool OfflineGeoworldAcceptedAlphaBaselineReady { get; init; }
    public string OfflineGeoworldAcceptedAlphaManualGateStatus { get; init; } = string.Empty;
    public string OfflineGeoworldAcceptedAlphaRecommendedNextDecision { get; init; } =
        string.Empty;
    public int OfflineGeoworldAcceptedAlphaIncludedSourceGoalCount { get; init; }
    public int OfflineGeoworldAcceptedAlphaAcceptedEvidenceRootCount { get; init; }
    public int OfflineGeoworldAcceptedAlphaProducedOnlyRootCount { get; init; }
    public int OfflineGeoworldAcceptedAlphaBlockedOrSupersededNoteCount { get; init; }
    public bool OfflineGeoworldAcceptedAlphaDoNotStartAutomatically { get; init; }
    public bool OfflineGeoworldAcceptedAlphaQualityGatePassed { get; init; }
    public bool Goal118FilesDiscoveredByRelativePaths { get; init; }
}
