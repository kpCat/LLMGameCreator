namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string CanonicalRuntimePlayerCommandLoopCandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerCommandLoopPassed { get; init; }
    public int CanonicalRuntimePlayerCommandCount { get; init; }
    public int CanonicalRuntimePlayerSnapshotCount { get; init; }
    public int CanonicalRuntimePlayerCommandLoopRuntimeEventCount { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopProjectionOnly { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopUnityGameplayTruth { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors { get; init; }
    public string CanonicalRuntimePlayerCommandLoopNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimePlayerCommandLoopReportPath { get; init; } = string.Empty;
    public string CanonicalRuntimePlayerCommandLoopMatrixResultPath { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerCommandLoopManualUnityOptional { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopAccepted { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysCanonicalRuntimePlayerCommandLoop { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool CanonicalRuntimePlayerCommandLoopGroupPresent { get; init; }
    public string CanonicalRuntimePlayerCommandLoopCandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerCommandLoopPassed { get; init; }
    public int CanonicalRuntimePlayerCommandCount { get; init; }
    public int CanonicalRuntimePlayerSnapshotCount { get; init; }
    public int CanonicalRuntimePlayerCommandLoopRuntimeEventCount { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopProjectionOnly { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopUnityGameplayTruth { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors { get; init; }
    public string CanonicalRuntimePlayerCommandLoopNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimePlayerCommandLoopReportPath { get; init; } = string.Empty;
    public string CanonicalRuntimePlayerCommandLoopMatrixResultPath { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerCommandLoopManualUnityOptional { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopAccepted { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopGoal136FilesDiscoveredByRelativePaths { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopWinFormsBindingReal { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopQualityGatePassed { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string CanonicalRuntimePlayerCommandLoopCandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerCommandLoopPassed { get; init; }
    public int CanonicalRuntimePlayerCommandCount { get; init; }
    public int CanonicalRuntimePlayerSnapshotCount { get; init; }
    public int CanonicalRuntimePlayerCommandLoopRuntimeEventCount { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopAllRequiredCategoriesPresent { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopUnityConsumedSnapshots { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopProjectionOnly { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopUnityGameplayTruth { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopNoUnclassifiedErrors { get; init; }
    public string CanonicalRuntimePlayerCommandLoopNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimePlayerCommandLoopReportPath { get; init; } = string.Empty;
    public string CanonicalRuntimePlayerCommandLoopMatrixResultPath { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerCommandLoopManualUnityOptional { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopAccepted { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopQualityGatePassed { get; init; }
    public bool CanonicalRuntimePlayerCommandLoopGoal136FilesDiscoveredByRelativePaths { get; init; }
}
