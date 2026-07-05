namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string AcceptedAlphaProjectionUsabilityStatus { get; init; } = string.Empty;
    public string AcceptedAlphaProjectionUsabilityUnityMenuPath { get; init; } = string.Empty;
    public string AcceptedAlphaProjectionUsabilityCleanupScriptPath { get; init; } = string.Empty;
    public string AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath { get; init; } = string.Empty;
    public bool AcceptedAlphaProjectionUsabilityLegendPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilitySelectionControlsPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilityFocusCameraControlPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent { get; init; }
    public string AcceptedAlphaProjectionUsabilityUnitySmokeStatus { get; init; } = string.Empty;
    public bool AcceptedAlphaProjectionUsabilityDoNotStartAutomatically { get; init; }
    public string AcceptedAlphaProjectionUsabilityEvidencePath { get; init; } = string.Empty;
    public string AcceptedAlphaProjectionUsabilityExportPath { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysAcceptedAlphaProjectionUsability { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool AcceptedAlphaProjectionUsabilityGroupPresent { get; init; }
    public string AcceptedAlphaProjectionUsabilityStatus { get; init; } = string.Empty;
    public string AcceptedAlphaProjectionUsabilityUnityMenuPath { get; init; } = string.Empty;
    public string AcceptedAlphaProjectionUsabilityCleanupScriptPath { get; init; } = string.Empty;
    public string AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath { get; init; } = string.Empty;
    public bool AcceptedAlphaProjectionUsabilityLegendPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilitySelectionControlsPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilityFocusCameraControlPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent { get; init; }
    public string AcceptedAlphaProjectionUsabilityUnitySmokeStatus { get; init; } = string.Empty;
    public bool AcceptedAlphaProjectionUsabilityDoNotStartAutomatically { get; init; }
    public bool AcceptedAlphaProjectionUsabilityQualityGatePassed { get; init; }
    public bool Goal120FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsAcceptedAlphaProjectionUsabilityBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string AcceptedAlphaProjectionUsabilityStatus { get; init; } = string.Empty;
    public string AcceptedAlphaProjectionUsabilityUnityMenuPath { get; init; } = string.Empty;
    public string AcceptedAlphaProjectionUsabilityCleanupScriptPath { get; init; } = string.Empty;
    public string AcceptedAlphaProjectionUsabilityCleanupScriptCmdPath { get; init; } = string.Empty;
    public bool AcceptedAlphaProjectionUsabilityLegendPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilityMarkerDescriptorPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilitySelectionControlsPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilityFocusCameraControlPresent { get; init; }
    public bool AcceptedAlphaProjectionUsabilityMaterialWarningGuardPresent { get; init; }
    public string AcceptedAlphaProjectionUsabilityUnitySmokeStatus { get; init; } = string.Empty;
    public bool AcceptedAlphaProjectionUsabilityDoNotStartAutomatically { get; init; }
    public bool AcceptedAlphaProjectionUsabilityQualityGatePassed { get; init; }
    public bool Goal120FilesDiscoveredByRelativePaths { get; init; }
}
