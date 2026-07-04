namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public int OfflineGeoworldObjectiveCount { get; init; }
    public int OfflineGeoworldObjectiveCompletedCount { get; init; }
    public int OfflineGeoworldObjectiveReplayStepCount { get; init; }
    public int OfflineGeoworldObjectiveStateDeltaCount { get; init; }
    public int OfflineGeoworldObjectiveCheckpointStepIndex { get; init; }
    public string OfflineGeoworldObjectiveFinalStatus { get; init; } = string.Empty;
    public string OfflineGeoworldObjectiveFinalStateHash { get; init; } = string.Empty;
    public bool OfflineGeoworldObjectiveUnityScriptsReady { get; init; }
    public bool OfflineGeoworldObjectiveEditorWindowReady { get; init; }
    public bool OfflineGeoworldObjectiveReplayAcceptanceProofPassed { get; init; }
    public bool OfflineGeoworldObjectiveNegativeProofPassed { get; init; }
    public bool OfflineGeoworldObjectiveAlphaQualityConsolidationPassed { get; init; }
    public bool OfflineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldObjectiveQualityGatePassed { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysOfflineGeoworldObjectiveAcceptance { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool OfflineGeoworldObjectiveAcceptanceGroupPresent { get; init; }
    public int OfflineGeoworldObjectiveCount { get; init; }
    public int OfflineGeoworldObjectiveCompletedCount { get; init; }
    public int OfflineGeoworldObjectivePayloadFileCount { get; init; }
    public int OfflineGeoworldObjectiveReplayStepCount { get; init; }
    public int OfflineGeoworldObjectiveStateDeltaCount { get; init; }
    public int OfflineGeoworldObjectiveCheckpointStepIndex { get; init; }
    public string OfflineGeoworldObjectiveFinalStatus { get; init; } = string.Empty;
    public string OfflineGeoworldObjectiveFinalStateHash { get; init; } = string.Empty;
    public bool OfflineGeoworldObjectiveUnityScriptsReady { get; init; }
    public bool OfflineGeoworldObjectiveEditorWindowReady { get; init; }
    public bool OfflineGeoworldObjectiveReplayAcceptanceProofPassed { get; init; }
    public bool OfflineGeoworldObjectiveNegativeProofPassed { get; init; }
    public bool OfflineGeoworldObjectiveAlphaQualityConsolidationPassed { get; init; }
    public bool OfflineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldObjectiveQualityGatePassed { get; init; }
    public bool Goal107FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsOfflineGeoworldObjectiveAcceptanceBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public int OfflineGeoworldObjectiveCount { get; init; }
    public int OfflineGeoworldObjectiveCompletedCount { get; init; }
    public int OfflineGeoworldObjectivePayloadFileCount { get; init; }
    public int OfflineGeoworldObjectiveReplayStepCount { get; init; }
    public int OfflineGeoworldObjectiveStateDeltaCount { get; init; }
    public int OfflineGeoworldObjectiveCheckpointStepIndex { get; init; }
    public string OfflineGeoworldObjectiveFinalStatus { get; init; } = string.Empty;
    public string OfflineGeoworldObjectiveFinalStateHash { get; init; } = string.Empty;
    public bool OfflineGeoworldObjectiveUnityScriptsReady { get; init; }
    public bool OfflineGeoworldObjectiveEditorWindowReady { get; init; }
    public bool OfflineGeoworldObjectiveReplayAcceptanceProofPassed { get; init; }
    public bool OfflineGeoworldObjectiveNegativeProofPassed { get; init; }
    public bool OfflineGeoworldObjectiveAlphaQualityConsolidationPassed { get; init; }
    public bool OfflineGeoworldObjectiveAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldObjectiveQualityGatePassed { get; init; }
    public bool Goal107FilesDiscoveredByRelativePaths { get; init; }
}
