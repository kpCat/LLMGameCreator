namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string GenericLoopStatus { get; init; } = string.Empty;
    public string GenericLoopSamplePackagePath { get; init; } = string.Empty;
    public string GenericLoopPackageId { get; init; } = string.Empty;
    public string GenericLoopMapId { get; init; } = string.Empty;
    public bool GenericLoopInteractionPreviewPresent { get; init; }
    public bool GenericLoopInteractionApplyPassed { get; init; }
    public bool GenericLoopDialogueSummaryPresent { get; init; }
    public bool GenericLoopQuestObjectiveSummaryPresent { get; init; }
    public bool GenericLoopInventorySummaryPresent { get; init; }
    public bool GenericLoopResourceSummaryPresent { get; init; }
    public string GenericLoopUnitySmokeStatus { get; init; } = string.Empty;
    public bool GenericLoopCleanupScriptAvailable { get; init; }
    public string GenericLoopCleanupCommand { get; init; } = string.Empty;
    public bool GenericLoopGoal123StillGreen { get; init; }
    public bool GenericLoopProjectionOnly { get; init; }
    public int GenericLoopAppliedInteractionCount { get; init; }
    public int GenericLoopStartedQuestCount { get; init; }
    public string GenericLoopEvidencePath { get; init; } = string.Empty;
    public string GenericLoopExportPath { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysGenericGamePackageLoop { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool GenericGamePackageLoopGroupPresent { get; init; }
    public string GenericLoopStatus { get; init; } = string.Empty;
    public string GenericLoopSamplePackagePath { get; init; } = string.Empty;
    public string GenericLoopPackageId { get; init; } = string.Empty;
    public string GenericLoopMapId { get; init; } = string.Empty;
    public bool GenericLoopInteractionPreviewPresent { get; init; }
    public bool GenericLoopInteractionApplyPassed { get; init; }
    public bool GenericLoopDialogueSummaryPresent { get; init; }
    public bool GenericLoopQuestObjectiveSummaryPresent { get; init; }
    public bool GenericLoopInventorySummaryPresent { get; init; }
    public bool GenericLoopResourceSummaryPresent { get; init; }
    public string GenericLoopUnitySmokeStatus { get; init; } = string.Empty;
    public bool GenericLoopCleanupScriptAvailable { get; init; }
    public string GenericLoopCleanupCommand { get; init; } = string.Empty;
    public bool GenericLoopGoal123StillGreen { get; init; }
    public bool GenericLoopProjectionOnly { get; init; }
    public int GenericLoopAppliedInteractionCount { get; init; }
    public int GenericLoopStartedQuestCount { get; init; }
    public string GenericLoopEvidencePath { get; init; } = string.Empty;
    public string GenericLoopExportPath { get; init; } = string.Empty;
    public bool GenericGamePackageLoopQualityGatePassed { get; init; }
    public bool Goal124FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsGenericGamePackageLoopBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string GenericLoopStatus { get; init; } = string.Empty;
    public string GenericLoopSamplePackagePath { get; init; } = string.Empty;
    public string GenericLoopPackageId { get; init; } = string.Empty;
    public string GenericLoopMapId { get; init; } = string.Empty;
    public bool GenericLoopInteractionPreviewPresent { get; init; }
    public bool GenericLoopInteractionApplyPassed { get; init; }
    public bool GenericLoopDialogueSummaryPresent { get; init; }
    public bool GenericLoopQuestObjectiveSummaryPresent { get; init; }
    public bool GenericLoopInventorySummaryPresent { get; init; }
    public bool GenericLoopResourceSummaryPresent { get; init; }
    public string GenericLoopUnitySmokeStatus { get; init; } = string.Empty;
    public bool GenericLoopCleanupScriptAvailable { get; init; }
    public string GenericLoopCleanupCommand { get; init; } = string.Empty;
    public bool GenericLoopGoal123StillGreen { get; init; }
    public bool GenericLoopProjectionOnly { get; init; }
    public int GenericLoopAppliedInteractionCount { get; init; }
    public int GenericLoopStartedQuestCount { get; init; }
    public string GenericLoopEvidencePath { get; init; } = string.Empty;
    public string GenericLoopExportPath { get; init; } = string.Empty;
    public bool GenericGamePackageLoopQualityGatePassed { get; init; }
    public bool Goal124FilesDiscoveredByRelativePaths { get; init; }
}
