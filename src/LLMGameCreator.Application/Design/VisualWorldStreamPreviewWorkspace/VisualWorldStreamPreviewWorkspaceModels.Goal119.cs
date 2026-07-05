namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string AcceptedAlphaUnityPlayableProjectionStatus { get; init; } = string.Empty;
    public string AcceptedAlphaUnityPlayableProjectionUnityMenuPath { get; init; } = string.Empty;
    public string AcceptedAlphaUnityPlayableProjectionBaselineId { get; init; } = string.Empty;
    public bool AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady { get; init; }
    public string AcceptedAlphaUnityPlayableProjectionGeneratedRootName { get; init; } = string.Empty;
    public int AcceptedAlphaUnityPlayableProjectionScriptInventoryCount { get; init; }
    public int AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount { get; init; }
    public bool AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean { get; init; }
    public bool AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically { get; init; }
    public string AcceptedAlphaUnityPlayableProjectionEvidencePath { get; init; } = string.Empty;
    public string AcceptedAlphaUnityPlayableProjectionExportPath { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysAcceptedAlphaUnityPlayableProjection { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool AcceptedAlphaUnityPlayableProjectionGroupPresent { get; init; }
    public string AcceptedAlphaUnityPlayableProjectionStatus { get; init; } = string.Empty;
    public string AcceptedAlphaUnityPlayableProjectionUnityMenuPath { get; init; } = string.Empty;
    public string AcceptedAlphaUnityPlayableProjectionBaselineId { get; init; } = string.Empty;
    public bool AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady { get; init; }
    public string AcceptedAlphaUnityPlayableProjectionGeneratedRootName { get; init; } = string.Empty;
    public int AcceptedAlphaUnityPlayableProjectionScriptInventoryCount { get; init; }
    public int AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount { get; init; }
    public bool AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean { get; init; }
    public bool AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically { get; init; }
    public bool AcceptedAlphaUnityPlayableProjectionQualityGatePassed { get; init; }
    public bool Goal119FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsAcceptedAlphaUnityPlayableProjectionBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string AcceptedAlphaUnityPlayableProjectionStatus { get; init; } = string.Empty;
    public string AcceptedAlphaUnityPlayableProjectionUnityMenuPath { get; init; } = string.Empty;
    public string AcceptedAlphaUnityPlayableProjectionBaselineId { get; init; } = string.Empty;
    public bool AcceptedAlphaUnityPlayableProjectionAcceptedBaselineReady { get; init; }
    public string AcceptedAlphaUnityPlayableProjectionGeneratedRootName { get; init; } = string.Empty;
    public int AcceptedAlphaUnityPlayableProjectionScriptInventoryCount { get; init; }
    public int AcceptedAlphaUnityPlayableProjectionSmokePlanStepCount { get; init; }
    public bool AcceptedAlphaUnityPlayableProjectionForbiddenUnitySurfaceClean { get; init; }
    public bool AcceptedAlphaUnityPlayableProjectionDoNotStartAutomatically { get; init; }
    public bool AcceptedAlphaUnityPlayableProjectionQualityGatePassed { get; init; }
    public bool Goal119FilesDiscoveredByRelativePaths { get; init; }
}
