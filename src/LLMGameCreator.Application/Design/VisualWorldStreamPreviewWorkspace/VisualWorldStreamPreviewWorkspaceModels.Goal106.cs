namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public int OfflineGeoworldSessionReplayStepCount { get; init; }
    public int OfflineGeoworldSessionStateDeltaCount { get; init; }
    public int OfflineGeoworldSessionCheckpointStepIndex { get; init; }
    public int OfflineGeoworldSessionAcceptanceChecklistStepCount { get; init; }
    public string OfflineGeoworldSessionCheckpointStateHash { get; init; } = string.Empty;
    public string OfflineGeoworldSessionFinalStateHash { get; init; } = string.Empty;
    public bool OfflineGeoworldSessionUnityScriptsReady { get; init; }
    public bool OfflineGeoworldSessionEditorWindowReady { get; init; }
    public bool OfflineGeoworldSessionSimulatedReplayProofPassed { get; init; }
    public bool OfflineGeoworldSessionNegativeProofPassed { get; init; }
    public bool OfflineGeoworldSessionAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldSessionQualityGatePassed { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysOfflineGeoworldSessionReplay { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool OfflineGeoworldSessionReplayGroupPresent { get; init; }
    public int OfflineGeoworldSessionReplayStepCount { get; init; }
    public int OfflineGeoworldSessionStateDeltaCount { get; init; }
    public int OfflineGeoworldSessionCheckpointStepIndex { get; init; }
    public int OfflineGeoworldSessionAcceptanceChecklistStepCount { get; init; }
    public string OfflineGeoworldSessionFinalStateHash { get; init; } = string.Empty;
    public bool OfflineGeoworldSessionUnityScriptsReady { get; init; }
    public bool OfflineGeoworldSessionEditorWindowReady { get; init; }
    public bool OfflineGeoworldSessionSimulatedReplayProofPassed { get; init; }
    public bool OfflineGeoworldSessionNegativeProofPassed { get; init; }
    public bool OfflineGeoworldSessionAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldSessionQualityGatePassed { get; init; }
    public bool Goal106FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsOfflineGeoworldSessionReplayBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public int OfflineGeoworldSessionReplayStepCount { get; init; }
    public int OfflineGeoworldSessionStateDeltaCount { get; init; }
    public int OfflineGeoworldSessionCheckpointStepIndex { get; init; }
    public int OfflineGeoworldSessionAcceptanceChecklistStepCount { get; init; }
    public string OfflineGeoworldSessionFinalStateHash { get; init; } = string.Empty;
    public bool OfflineGeoworldSessionUnityScriptsReady { get; init; }
    public bool OfflineGeoworldSessionEditorWindowReady { get; init; }
    public bool OfflineGeoworldSessionSimulatedReplayProofPassed { get; init; }
    public bool OfflineGeoworldSessionNegativeProofPassed { get; init; }
    public bool OfflineGeoworldSessionAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldSessionQualityGatePassed { get; init; }
    public bool Goal106FilesDiscoveredByRelativePaths { get; init; }
}
