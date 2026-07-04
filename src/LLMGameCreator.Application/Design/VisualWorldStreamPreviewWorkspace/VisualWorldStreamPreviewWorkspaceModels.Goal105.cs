namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public int OfflineGeoworldInteractionTargetCount { get; init; }
    public int OfflineGeoworldInteractionActionKindCount { get; init; }
    public int OfflineGeoworldInteractionActionCount { get; init; }
    public int OfflineGeoworldInteractionScriptedEventCount { get; init; }
    public int OfflineGeoworldInteractionStateDeltaCount { get; init; }
    public string OfflineGeoworldInteractionFinalStateHash { get; init; } = string.Empty;
    public bool OfflineGeoworldInteractionStateHashChainPassed { get; init; }
    public bool OfflineGeoworldInteractionUnityScriptsReady { get; init; }
    public bool OfflineGeoworldInteractionEditorWindowReady { get; init; }
    public bool OfflineGeoworldInteractionUnitySafetyScanPassed { get; init; }
    public bool OfflineGeoworldInteractionSimulatedSessionProofPassed { get; init; }
    public bool OfflineGeoworldInteractionNegativeProofPassed { get; init; }
    public bool OfflineGeoworldInteractionAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldInteractionQualityGatePassed { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysOfflineGeoworldInteractions { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool OfflineGeoworldInteractionGroupPresent { get; init; }
    public int OfflineGeoworldInteractionTargetCount { get; init; }
    public int OfflineGeoworldInteractionActionKindCount { get; init; }
    public int OfflineGeoworldInteractionActionCount { get; init; }
    public int OfflineGeoworldInteractionScriptedEventCount { get; init; }
    public int OfflineGeoworldInteractionStateDeltaCount { get; init; }
    public string OfflineGeoworldInteractionFinalStateHash { get; init; } = string.Empty;
    public bool OfflineGeoworldInteractionStateHashChainPassed { get; init; }
    public bool OfflineGeoworldInteractionUnityScriptsReady { get; init; }
    public bool OfflineGeoworldInteractionEditorWindowReady { get; init; }
    public bool OfflineGeoworldInteractionUnitySafetyScanPassed { get; init; }
    public bool OfflineGeoworldInteractionSimulatedSessionProofPassed { get; init; }
    public bool OfflineGeoworldInteractionNegativeProofPassed { get; init; }
    public bool OfflineGeoworldInteractionAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldInteractionQualityGatePassed { get; init; }
    public bool Goal105FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsOfflineGeoworldInteractionBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public int OfflineGeoworldInteractionTargetCount { get; init; }
    public int OfflineGeoworldInteractionActionKindCount { get; init; }
    public int OfflineGeoworldInteractionActionCount { get; init; }
    public int OfflineGeoworldInteractionScriptedEventCount { get; init; }
    public int OfflineGeoworldInteractionStateDeltaCount { get; init; }
    public string OfflineGeoworldInteractionFinalStateHash { get; init; } = string.Empty;
    public bool OfflineGeoworldInteractionStateHashChainPassed { get; init; }
    public bool OfflineGeoworldInteractionUnityScriptsReady { get; init; }
    public bool OfflineGeoworldInteractionEditorWindowReady { get; init; }
    public bool OfflineGeoworldInteractionUnitySafetyScanPassed { get; init; }
    public bool OfflineGeoworldInteractionSimulatedSessionProofPassed { get; init; }
    public bool OfflineGeoworldInteractionNegativeProofPassed { get; init; }
    public bool OfflineGeoworldInteractionAlphaRuntimeBootstrapUnchanged { get; init; }
    public bool OfflineGeoworldInteractionQualityGatePassed { get; init; }
    public bool Goal105FilesDiscoveredByRelativePaths { get; init; }
}
