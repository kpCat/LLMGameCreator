namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId { get; init; } = string.Empty;
    public int CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackPassed { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime { get; init; }
    public string CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimeUnityPlayerLoopPlaybackReportPath { get; init; } = string.Empty;
    public string CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath { get; init; } = string.Empty;
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackAccepted { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysCanonicalRuntimeUnityPlayerLoopPlayback { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackGroupPresent { get; init; }
    public string CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId { get; init; } = string.Empty;
    public int CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackPassed { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime { get; init; }
    public string CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimeUnityPlayerLoopPlaybackReportPath { get; init; } = string.Empty;
    public string CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath { get; init; } = string.Empty;
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackAccepted { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackGoal137FilesDiscoveredByRelativePaths { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackWinFormsBindingReal { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string CanonicalRuntimeUnityPlayerLoopPlaybackCandidateId { get; init; } = string.Empty;
    public int CanonicalRuntimeUnityPlayerLoopPlaybackFrameCount { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackRequiredCategoriesPresent { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackPassed { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackRuntimeSnapshotSource { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackUnityGameplayTruth { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackProjectionOnly { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackSelectedCandidateExecutedByRuntime { get; init; }
    public string CanonicalRuntimeUnityPlayerLoopPlaybackNormalCommand { get; init; } = string.Empty;
    public string CanonicalRuntimeUnityPlayerLoopPlaybackReportPath { get; init; } = string.Empty;
    public string CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResultPath { get; init; } = string.Empty;
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackManualUnityOptional { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackAccepted { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackQualityGatePassed { get; init; }
    public bool CanonicalRuntimeUnityPlayerLoopPlaybackGoal137FilesDiscoveredByRelativePaths { get; init; }
}
