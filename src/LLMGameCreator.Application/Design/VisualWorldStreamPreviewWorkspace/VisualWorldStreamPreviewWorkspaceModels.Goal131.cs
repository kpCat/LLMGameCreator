namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string GamePackageCandidateRecipePipelineStatus { get; init; } = string.Empty;
    public int GamePackageCandidateRecipePipelineRecipeCount { get; init; }
    public int GamePackageCandidateRecipePipelineCandidateCount { get; init; }
    public int GamePackageCandidateRecipePipelinePassedCandidates { get; init; }
    public int GamePackageCandidateRecipePipelineFailedCandidates { get; init; }
    public bool GamePackageCandidateRecipePipelineMatrixPassed { get; init; }
    public string GamePackageCandidateRecipePipelineSelectedCandidateId { get; init; } = string.Empty;
    public int GamePackageCandidateRecipePipelineSelectedCandidateScore { get; init; }
    public string GamePackageCandidateRecipePipelineRecipeCatalogPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineCandidateIndexPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineNormalCommand { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineScoringResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineMatrixResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineSelectedCandidatePackagePath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath { get; init; } = string.Empty;
    public bool GamePackageCandidateRecipePipelineManualUnityOptional { get; init; }
    public bool GamePackageCandidateRecipePipelineSamplePackageUnmodified { get; init; }
    public bool GamePackageCandidateRecipePipelineProjectionOnly { get; init; }
    public bool GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation { get; init; }
    public string GamePackageCandidateRecipePipelineEvidencePath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineExportPath { get; init; } = string.Empty;
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysGamePackageCandidateRecipePipeline { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool GamePackageCandidateRecipePipelineGroupPresent { get; init; }
    public string GamePackageCandidateRecipePipelineStatus { get; init; } = string.Empty;
    public int GamePackageCandidateRecipePipelineRecipeCount { get; init; }
    public int GamePackageCandidateRecipePipelineCandidateCount { get; init; }
    public int GamePackageCandidateRecipePipelinePassedCandidates { get; init; }
    public int GamePackageCandidateRecipePipelineFailedCandidates { get; init; }
    public bool GamePackageCandidateRecipePipelineMatrixPassed { get; init; }
    public string GamePackageCandidateRecipePipelineSelectedCandidateId { get; init; } = string.Empty;
    public int GamePackageCandidateRecipePipelineSelectedCandidateScore { get; init; }
    public string GamePackageCandidateRecipePipelineRecipeCatalogPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineCandidateIndexPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineNormalCommand { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineScoringResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineMatrixResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineSelectedCandidatePackagePath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath { get; init; } = string.Empty;
    public bool GamePackageCandidateRecipePipelineManualUnityOptional { get; init; }
    public bool GamePackageCandidateRecipePipelineSamplePackageUnmodified { get; init; }
    public bool GamePackageCandidateRecipePipelineProjectionOnly { get; init; }
    public bool GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation { get; init; }
    public string GamePackageCandidateRecipePipelineEvidencePath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineExportPath { get; init; } = string.Empty;
    public bool GamePackageCandidateRecipePipelineQualityGatePassed { get; init; }
    public bool Goal131FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsGamePackageCandidateRecipePipelineBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string GamePackageCandidateRecipePipelineStatus { get; init; } = string.Empty;
    public int GamePackageCandidateRecipePipelineRecipeCount { get; init; }
    public int GamePackageCandidateRecipePipelineCandidateCount { get; init; }
    public int GamePackageCandidateRecipePipelinePassedCandidates { get; init; }
    public int GamePackageCandidateRecipePipelineFailedCandidates { get; init; }
    public bool GamePackageCandidateRecipePipelineMatrixPassed { get; init; }
    public string GamePackageCandidateRecipePipelineSelectedCandidateId { get; init; } = string.Empty;
    public int GamePackageCandidateRecipePipelineSelectedCandidateScore { get; init; }
    public string GamePackageCandidateRecipePipelineRecipeCatalogPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineCandidateIndexPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineNormalCommand { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineScoringResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineMatrixResultPath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineSelectedCandidatePackagePath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath { get; init; } = string.Empty;
    public bool GamePackageCandidateRecipePipelineManualUnityOptional { get; init; }
    public bool GamePackageCandidateRecipePipelineSamplePackageUnmodified { get; init; }
    public bool GamePackageCandidateRecipePipelineProjectionOnly { get; init; }
    public bool GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation { get; init; }
    public string GamePackageCandidateRecipePipelineEvidencePath { get; init; } = string.Empty;
    public string GamePackageCandidateRecipePipelineExportPath { get; init; } = string.Empty;
    public bool GamePackageCandidateRecipePipelineQualityGatePassed { get; init; }
    public bool Goal131FilesDiscoveredByRelativePaths { get; init; }
}
