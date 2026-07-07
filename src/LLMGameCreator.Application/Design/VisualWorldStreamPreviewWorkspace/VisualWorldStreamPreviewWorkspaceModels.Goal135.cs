namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string CanonicalRuntimePlayerLoopCandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerLoopAdapterContractPresent { get; init; }
    public int CanonicalRuntimePlayerLoopStepCount { get; init; }
    public bool CanonicalRuntimePlayerLoopRequiredCategoriesPresent { get; init; }
    public bool CanonicalRuntimePlayerLoopUnityReadinessPassed { get; init; }
    public bool CanonicalRuntimePlayerLoopSource { get; init; }
    public bool CanonicalRuntimePlayerLoopUnityGameplayTruth { get; init; }
    public bool CanonicalRuntimePlayerLoopProjectionOnly { get; init; }
    public bool CanonicalRuntimePlayerLoopNoUnclassifiedErrors { get; init; }
    public string CanonicalRuntimePlayerLoopNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimePlayerLoopReportPath { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerLoopManualUnityOptional { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysCanonicalRuntimePlayerLoopReadiness { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool CanonicalRuntimePlayerLoopGroupPresent { get; init; }
    public string CanonicalRuntimePlayerLoopCandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerLoopAdapterContractPresent { get; init; }
    public int CanonicalRuntimePlayerLoopStepCount { get; init; }
    public bool CanonicalRuntimePlayerLoopRequiredCategoriesPresent { get; init; }
    public bool CanonicalRuntimePlayerLoopUnityReadinessPassed { get; init; }
    public bool CanonicalRuntimePlayerLoopSource { get; init; }
    public bool CanonicalRuntimePlayerLoopUnityGameplayTruth { get; init; }
    public bool CanonicalRuntimePlayerLoopProjectionOnly { get; init; }
    public bool CanonicalRuntimePlayerLoopNoUnclassifiedErrors { get; init; }
    public string CanonicalRuntimePlayerLoopNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimePlayerLoopReportPath { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerLoopManualUnityOptional { get; init; }
    public bool CanonicalRuntimePlayerLoopGoal135FilesDiscoveredByRelativePaths { get; init; }
    public bool CanonicalRuntimePlayerLoopWinFormsBindingReal { get; init; }
    public bool CanonicalRuntimePlayerLoopQualityGatePassed { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string CanonicalRuntimePlayerLoopCandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerLoopAdapterContractPresent { get; init; }
    public int CanonicalRuntimePlayerLoopStepCount { get; init; }
    public bool CanonicalRuntimePlayerLoopRequiredCategoriesPresent { get; init; }
    public bool CanonicalRuntimePlayerLoopUnityReadinessPassed { get; init; }
    public bool CanonicalRuntimePlayerLoopSource { get; init; }
    public bool CanonicalRuntimePlayerLoopUnityGameplayTruth { get; init; }
    public bool CanonicalRuntimePlayerLoopProjectionOnly { get; init; }
    public bool CanonicalRuntimePlayerLoopNoUnclassifiedErrors { get; init; }
    public string CanonicalRuntimePlayerLoopNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimePlayerLoopReportPath { get; init; } = string.Empty;
    public bool CanonicalRuntimePlayerLoopManualUnityOptional { get; init; }
    public bool CanonicalRuntimePlayerLoopQualityGatePassed { get; init; }
    public bool CanonicalRuntimePlayerLoopGoal135FilesDiscoveredByRelativePaths { get; init; }
}
