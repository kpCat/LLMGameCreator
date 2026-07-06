namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string GamePackageCandidateMatrixStatus { get; init; } = string.Empty;
    public int GamePackageCandidateMatrixCandidateCount { get; init; }
    public int GamePackageCandidateMatrixPassedCandidateCount { get; init; }
    public int GamePackageCandidateMatrixFailedCandidateCount { get; init; }
    public string GamePackageCandidateMatrixCandidateIndexPath { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixNormalCommand { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixExampleCommand { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixBaselineCandidatePackagePath { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixVariantCandidatePackagePath { get; init; } = string.Empty;
    public bool GamePackageCandidateMatrixManualUnityOptional { get; init; }
    public bool GamePackageCandidateMatrixCleanupApplied { get; init; }
    public bool GamePackageCandidateMatrixProjectionOnly { get; init; }
    public bool GamePackageCandidateMatrixScriptScanPassed { get; init; }
    public bool GamePackageCandidateMatrixResultPassed { get; init; }
    public bool GamePackageCandidateMatrixLogScanPassed { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysGamePackageCandidateMatrix { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool GamePackageCandidateMatrixGroupPresent { get; init; }
    public string GamePackageCandidateMatrixStatus { get; init; } = string.Empty;
    public int GamePackageCandidateMatrixCandidateCount { get; init; }
    public int GamePackageCandidateMatrixPassedCandidateCount { get; init; }
    public int GamePackageCandidateMatrixFailedCandidateCount { get; init; }
    public string GamePackageCandidateMatrixCandidateIndexPath { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixNormalCommand { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixExampleCommand { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixBaselineCandidatePackagePath { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixVariantCandidatePackagePath { get; init; } = string.Empty;
    public bool GamePackageCandidateMatrixManualUnityOptional { get; init; }
    public bool GamePackageCandidateMatrixCleanupApplied { get; init; }
    public bool GamePackageCandidateMatrixProjectionOnly { get; init; }
    public bool GamePackageCandidateMatrixScriptScanPassed { get; init; }
    public bool GamePackageCandidateMatrixResultPassed { get; init; }
    public bool GamePackageCandidateMatrixLogScanPassed { get; init; }
    public bool GamePackageCandidateMatrixQualityGatePassed { get; init; }
    public bool Goal129FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsGamePackageCandidateMatrixBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string GamePackageCandidateMatrixStatus { get; init; } = string.Empty;
    public int GamePackageCandidateMatrixCandidateCount { get; init; }
    public int GamePackageCandidateMatrixPassedCandidateCount { get; init; }
    public int GamePackageCandidateMatrixFailedCandidateCount { get; init; }
    public string GamePackageCandidateMatrixCandidateIndexPath { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixNormalCommand { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixExampleCommand { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixBaselineCandidatePackagePath { get; init; } = string.Empty;
    public string GamePackageCandidateMatrixVariantCandidatePackagePath { get; init; } = string.Empty;
    public bool GamePackageCandidateMatrixManualUnityOptional { get; init; }
    public bool GamePackageCandidateMatrixCleanupApplied { get; init; }
    public bool GamePackageCandidateMatrixProjectionOnly { get; init; }
    public bool GamePackageCandidateMatrixScriptScanPassed { get; init; }
    public bool GamePackageCandidateMatrixResultPassed { get; init; }
    public bool GamePackageCandidateMatrixLogScanPassed { get; init; }
    public bool GamePackageCandidateMatrixQualityGatePassed { get; init; }
    public bool Goal129FilesDiscoveredByRelativePaths { get; init; }
}
