namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal131ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            GamePackageCandidateRecipePipelineStatus =
                qualityGate.GamePackageCandidateRecipePipelineStatus,
            GamePackageCandidateRecipePipelineRecipeCount =
                qualityGate.GamePackageCandidateRecipePipelineRecipeCount,
            GamePackageCandidateRecipePipelineCandidateCount =
                qualityGate.GamePackageCandidateRecipePipelineCandidateCount,
            GamePackageCandidateRecipePipelinePassedCandidates =
                qualityGate.GamePackageCandidateRecipePipelinePassedCandidates,
            GamePackageCandidateRecipePipelineFailedCandidates =
                qualityGate.GamePackageCandidateRecipePipelineFailedCandidates,
            GamePackageCandidateRecipePipelineMatrixPassed =
                qualityGate.GamePackageCandidateRecipePipelineMatrixPassed,
            GamePackageCandidateRecipePipelineSelectedCandidateId =
                qualityGate.GamePackageCandidateRecipePipelineSelectedCandidateId,
            GamePackageCandidateRecipePipelineSelectedCandidateScore =
                qualityGate.GamePackageCandidateRecipePipelineSelectedCandidateScore,
            GamePackageCandidateRecipePipelineRecipeCatalogPath =
                qualityGate.GamePackageCandidateRecipePipelineRecipeCatalogPath,
            GamePackageCandidateRecipePipelineCandidateIndexPath =
                qualityGate.GamePackageCandidateRecipePipelineCandidateIndexPath,
            GamePackageCandidateRecipePipelineNormalCommand =
                qualityGate.GamePackageCandidateRecipePipelineNormalCommand,
            GamePackageCandidateRecipePipelineResultPath =
                qualityGate.GamePackageCandidateRecipePipelineResultPath,
            GamePackageCandidateRecipePipelineScoringResultPath =
                qualityGate.GamePackageCandidateRecipePipelineScoringResultPath,
            GamePackageCandidateRecipePipelineMatrixResultPath =
                qualityGate.GamePackageCandidateRecipePipelineMatrixResultPath,
            GamePackageCandidateRecipePipelineSelectedCandidatePackagePath =
                qualityGate.GamePackageCandidateRecipePipelineSelectedCandidatePackagePath,
            GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath =
                qualityGate.GamePackageCandidateRecipePipelineSelectedCandidateHandoffPath,
            GamePackageCandidateRecipePipelineManualUnityOptional =
                qualityGate.GamePackageCandidateRecipePipelineManualUnityOptional,
            GamePackageCandidateRecipePipelineSamplePackageUnmodified =
                qualityGate.GamePackageCandidateRecipePipelineSamplePackageUnmodified,
            GamePackageCandidateRecipePipelineProjectionOnly =
                qualityGate.GamePackageCandidateRecipePipelineProjectionOnly,
            GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation =
                qualityGate.GamePackageCandidateRecipePipelineMetadataOnlyRecipeMutation,
            GamePackageCandidateRecipePipelineEvidencePath =
                qualityGate.GamePackageCandidateRecipePipelineEvidencePath,
            GamePackageCandidateRecipePipelineExportPath =
                qualityGate.GamePackageCandidateRecipePipelineExportPath,
            GamePackageCandidateRecipePipelineQualityGatePassed =
                qualityGate.GamePackageCandidateRecipePipelineQualityGatePassed,
            Goal131FilesDiscoveredByRelativePaths =
                qualityGate.Goal131FilesDiscoveredByRelativePaths
        };
}
