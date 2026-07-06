namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal130ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            GamePackageCandidateFactoryStatus =
                qualityGate.GamePackageCandidateFactoryStatus,
            GamePackageCandidateFactoryCandidateCount =
                qualityGate.GamePackageCandidateFactoryCandidateCount,
            GamePackageCandidateFactoryPassedCandidates =
                qualityGate.GamePackageCandidateFactoryPassedCandidates,
            GamePackageCandidateFactoryFailedCandidates =
                qualityGate.GamePackageCandidateFactoryFailedCandidates,
            GamePackageCandidateFactoryMatrixPassed =
                qualityGate.GamePackageCandidateFactoryMatrixPassed,
            GamePackageCandidateFactoryCandidateIndexPath =
                qualityGate.GamePackageCandidateFactoryCandidateIndexPath,
            GamePackageCandidateFactoryNormalCommand =
                qualityGate.GamePackageCandidateFactoryNormalCommand,
            GamePackageCandidateFactoryResultPath =
                qualityGate.GamePackageCandidateFactoryResultPath,
            GamePackageCandidateFactoryMatrixResultPath =
                qualityGate.GamePackageCandidateFactoryMatrixResultPath,
            GamePackageCandidateFactoryManualUnityOptional =
                qualityGate.GamePackageCandidateFactoryManualUnityOptional,
            GamePackageCandidateFactorySamplePackageUnmodified =
                qualityGate.GamePackageCandidateFactorySamplePackageUnmodified,
            GamePackageCandidateFactoryProjectionOnly =
                qualityGate.GamePackageCandidateFactoryProjectionOnly,
            GamePackageCandidateFactoryEvidencePath =
                qualityGate.GamePackageCandidateFactoryEvidencePath,
            GamePackageCandidateFactoryExportPath =
                qualityGate.GamePackageCandidateFactoryExportPath,
            GamePackageCandidateFactoryQualityGatePassed =
                qualityGate.GamePackageCandidateFactoryQualityGatePassed,
            Goal130FilesDiscoveredByRelativePaths =
                qualityGate.Goal130FilesDiscoveredByRelativePaths
        };
}
