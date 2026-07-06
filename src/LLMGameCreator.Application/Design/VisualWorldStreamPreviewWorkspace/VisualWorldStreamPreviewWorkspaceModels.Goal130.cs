namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string GamePackageCandidateFactoryStatus { get; init; } = string.Empty;
    public int GamePackageCandidateFactoryCandidateCount { get; init; }
    public int GamePackageCandidateFactoryPassedCandidates { get; init; }
    public int GamePackageCandidateFactoryFailedCandidates { get; init; }
    public bool GamePackageCandidateFactoryMatrixPassed { get; init; }
    public string GamePackageCandidateFactoryCandidateIndexPath { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryNormalCommand { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryMatrixResultPath { get; init; } = string.Empty;
    public bool GamePackageCandidateFactoryManualUnityOptional { get; init; }
    public bool GamePackageCandidateFactorySamplePackageUnmodified { get; init; }
    public bool GamePackageCandidateFactoryProjectionOnly { get; init; }
    public string GamePackageCandidateFactoryEvidencePath { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryExportPath { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysGamePackageCandidateFactory { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool GamePackageCandidateFactoryGroupPresent { get; init; }
    public string GamePackageCandidateFactoryStatus { get; init; } = string.Empty;
    public int GamePackageCandidateFactoryCandidateCount { get; init; }
    public int GamePackageCandidateFactoryPassedCandidates { get; init; }
    public int GamePackageCandidateFactoryFailedCandidates { get; init; }
    public bool GamePackageCandidateFactoryMatrixPassed { get; init; }
    public string GamePackageCandidateFactoryCandidateIndexPath { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryNormalCommand { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryMatrixResultPath { get; init; } = string.Empty;
    public bool GamePackageCandidateFactoryManualUnityOptional { get; init; }
    public bool GamePackageCandidateFactorySamplePackageUnmodified { get; init; }
    public bool GamePackageCandidateFactoryProjectionOnly { get; init; }
    public string GamePackageCandidateFactoryEvidencePath { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryExportPath { get; init; } = string.Empty;
    public bool GamePackageCandidateFactoryQualityGatePassed { get; init; }
    public bool Goal130FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsGamePackageCandidateFactoryBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string GamePackageCandidateFactoryStatus { get; init; } = string.Empty;
    public int GamePackageCandidateFactoryCandidateCount { get; init; }
    public int GamePackageCandidateFactoryPassedCandidates { get; init; }
    public int GamePackageCandidateFactoryFailedCandidates { get; init; }
    public bool GamePackageCandidateFactoryMatrixPassed { get; init; }
    public string GamePackageCandidateFactoryCandidateIndexPath { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryNormalCommand { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryMatrixResultPath { get; init; } = string.Empty;
    public bool GamePackageCandidateFactoryManualUnityOptional { get; init; }
    public bool GamePackageCandidateFactorySamplePackageUnmodified { get; init; }
    public bool GamePackageCandidateFactoryProjectionOnly { get; init; }
    public string GamePackageCandidateFactoryEvidencePath { get; init; } = string.Empty;
    public string GamePackageCandidateFactoryExportPath { get; init; } = string.Empty;
    public bool GamePackageCandidateFactoryQualityGatePassed { get; init; }
    public bool Goal130FilesDiscoveredByRelativePaths { get; init; }
}
