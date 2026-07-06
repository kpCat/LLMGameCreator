namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string GenericProjectionStatus { get; init; } = string.Empty;
    public string GenericProjectionSamplePackagePath { get; init; } = string.Empty;
    public string GenericProjectionPackageId { get; init; } = string.Empty;
    public string GenericProjectionPackageTitle { get; init; } = string.Empty;
    public string GenericProjectionMapId { get; init; } = string.Empty;
    public string GenericProjectionMapSize { get; init; } = string.Empty;
    public int GenericProjectionEntityCount { get; init; }
    public int GenericProjectionItemCount { get; init; }
    public string GenericProjectionUnitySmokeStatus { get; init; } = string.Empty;
    public bool GenericProjectionGoal122StillGreen { get; init; }
    public bool GenericProjectionCleanupScriptAvailable { get; init; }
    public bool GenericProjectionDoNotStartAutomatically { get; init; }
    public string GenericProjectionEvidencePath { get; init; } = string.Empty;
    public string GenericProjectionExportPath { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysGenericGamePackageProjection { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool GenericGamePackageProjectionGroupPresent { get; init; }
    public string GenericProjectionStatus { get; init; } = string.Empty;
    public string GenericProjectionSamplePackagePath { get; init; } = string.Empty;
    public string GenericProjectionPackageId { get; init; } = string.Empty;
    public string GenericProjectionPackageTitle { get; init; } = string.Empty;
    public string GenericProjectionMapId { get; init; } = string.Empty;
    public string GenericProjectionMapSize { get; init; } = string.Empty;
    public int GenericProjectionEntityCount { get; init; }
    public int GenericProjectionItemCount { get; init; }
    public string GenericProjectionUnitySmokeStatus { get; init; } = string.Empty;
    public bool GenericProjectionGoal122StillGreen { get; init; }
    public bool GenericProjectionCleanupScriptAvailable { get; init; }
    public bool GenericGamePackageProjectionQualityGatePassed { get; init; }
    public bool Goal123FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsGenericGamePackageProjectionBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string GenericProjectionStatus { get; init; } = string.Empty;
    public string GenericProjectionSamplePackagePath { get; init; } = string.Empty;
    public string GenericProjectionPackageId { get; init; } = string.Empty;
    public string GenericProjectionPackageTitle { get; init; } = string.Empty;
    public string GenericProjectionMapId { get; init; } = string.Empty;
    public string GenericProjectionMapSize { get; init; } = string.Empty;
    public int GenericProjectionEntityCount { get; init; }
    public int GenericProjectionItemCount { get; init; }
    public string GenericProjectionUnitySmokeStatus { get; init; } = string.Empty;
    public bool GenericProjectionGoal122StillGreen { get; init; }
    public bool GenericProjectionCleanupScriptAvailable { get; init; }
    public bool GenericGamePackageProjectionQualityGatePassed { get; init; }
    public bool Goal123FilesDiscoveredByRelativePaths { get; init; }
}
