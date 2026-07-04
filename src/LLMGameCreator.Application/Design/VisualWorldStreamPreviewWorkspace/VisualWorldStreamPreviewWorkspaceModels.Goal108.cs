namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public int OfflineGeoworldAlphaSliceComponentCount { get; init; }
    public int OfflineGeoworldAlphaSliceReadyComponentCount { get; init; }
    public int OfflineGeoworldAlphaSliceObjectiveCount { get; init; }
    public int OfflineGeoworldAlphaSliceCompletedObjectiveCount { get; init; }
    public string OfflineGeoworldAlphaSliceFinalStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaSliceUnityToolReady { get; init; }
    public bool OfflineGeoworldAlphaSliceAcceptanceRunbookReady { get; init; }
    public bool OfflineGeoworldAlphaSliceFinalProofPassed { get; init; }
    public bool OfflineGeoworldAlphaSliceNegativeProofPassed { get; init; }
    public bool OfflineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldAlphaSliceQualityGatePassed { get; init; }
    public string OfflineGeoworldAlphaSliceRemainingWarnings { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysOfflineGeoworldAlphaSlice { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool OfflineGeoworldAlphaSliceGroupPresent { get; init; }
    public int OfflineGeoworldAlphaSliceComponentCount { get; init; }
    public int OfflineGeoworldAlphaSliceReadyComponentCount { get; init; }
    public int OfflineGeoworldAlphaSliceObjectiveCount { get; init; }
    public int OfflineGeoworldAlphaSliceCompletedObjectiveCount { get; init; }
    public string OfflineGeoworldAlphaSliceFinalStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaSliceUnityToolReady { get; init; }
    public bool OfflineGeoworldAlphaSliceAcceptanceRunbookReady { get; init; }
    public bool OfflineGeoworldAlphaSliceFinalProofPassed { get; init; }
    public bool OfflineGeoworldAlphaSliceNegativeProofPassed { get; init; }
    public bool OfflineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldAlphaSliceQualityGatePassed { get; init; }
    public bool Goal108FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsOfflineGeoworldAlphaSliceBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public int OfflineGeoworldAlphaSliceComponentCount { get; init; }
    public int OfflineGeoworldAlphaSliceReadyComponentCount { get; init; }
    public int OfflineGeoworldAlphaSliceObjectiveCount { get; init; }
    public int OfflineGeoworldAlphaSliceCompletedObjectiveCount { get; init; }
    public string OfflineGeoworldAlphaSliceFinalStatus { get; init; } = string.Empty;
    public bool OfflineGeoworldAlphaSliceUnityToolReady { get; init; }
    public bool OfflineGeoworldAlphaSliceAcceptanceRunbookReady { get; init; }
    public bool OfflineGeoworldAlphaSliceFinalProofPassed { get; init; }
    public bool OfflineGeoworldAlphaSliceNegativeProofPassed { get; init; }
    public bool OfflineGeoworldAlphaSliceAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldAlphaSliceQualityGatePassed { get; init; }
    public bool Goal108FilesDiscoveredByRelativePaths { get; init; }
}
