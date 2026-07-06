namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal129ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            GamePackageCandidateMatrixStatus =
                qualityGate.GamePackageCandidateMatrixStatus,
            GamePackageCandidateMatrixCandidateCount =
                qualityGate.GamePackageCandidateMatrixCandidateCount,
            GamePackageCandidateMatrixPassedCandidateCount =
                qualityGate.GamePackageCandidateMatrixPassedCandidateCount,
            GamePackageCandidateMatrixFailedCandidateCount =
                qualityGate.GamePackageCandidateMatrixFailedCandidateCount,
            GamePackageCandidateMatrixCandidateIndexPath =
                qualityGate.GamePackageCandidateMatrixCandidateIndexPath,
            GamePackageCandidateMatrixResultPath =
                qualityGate.GamePackageCandidateMatrixResultPath,
            GamePackageCandidateMatrixNormalCommand =
                qualityGate.GamePackageCandidateMatrixNormalCommand,
            GamePackageCandidateMatrixExampleCommand =
                qualityGate.GamePackageCandidateMatrixExampleCommand,
            GamePackageCandidateMatrixBaselineCandidatePackagePath =
                qualityGate.GamePackageCandidateMatrixBaselineCandidatePackagePath,
            GamePackageCandidateMatrixVariantCandidatePackagePath =
                qualityGate.GamePackageCandidateMatrixVariantCandidatePackagePath,
            GamePackageCandidateMatrixManualUnityOptional =
                qualityGate.GamePackageCandidateMatrixManualUnityOptional,
            GamePackageCandidateMatrixCleanupApplied =
                qualityGate.GamePackageCandidateMatrixCleanupApplied,
            GamePackageCandidateMatrixProjectionOnly =
                qualityGate.GamePackageCandidateMatrixProjectionOnly,
            GamePackageCandidateMatrixScriptScanPassed =
                qualityGate.GamePackageCandidateMatrixScriptScanPassed,
            GamePackageCandidateMatrixResultPassed =
                qualityGate.GamePackageCandidateMatrixResultPassed,
            GamePackageCandidateMatrixLogScanPassed =
                qualityGate.GamePackageCandidateMatrixLogScanPassed,
            GamePackageCandidateMatrixQualityGatePassed =
                qualityGate.GamePackageCandidateMatrixQualityGatePassed,
            Goal129FilesDiscoveredByRelativePaths =
                qualityGate.Goal129FilesDiscoveredByRelativePaths
        };
}
