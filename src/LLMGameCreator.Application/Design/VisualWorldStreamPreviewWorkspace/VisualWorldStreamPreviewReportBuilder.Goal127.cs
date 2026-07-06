namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal127ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            UnityProjectionVerificationRunnerStatus =
                qualityGate.UnityProjectionVerificationRunnerStatus,
            UnityProjectionVerificationRunnerMode =
                qualityGate.UnityProjectionVerificationRunnerMode,
            UnityProjectionVerificationRunnerScriptPath =
                qualityGate.UnityProjectionVerificationRunnerScriptPath,
            UnityProjectionVerificationRunnerCmdPath =
                qualityGate.UnityProjectionVerificationRunnerCmdPath,
            UnityProjectionVerificationRunnerCommand =
                qualityGate.UnityProjectionVerificationRunnerCommand,
            UnityProjectionVerificationRunnerExecuteMethod =
                qualityGate.UnityProjectionVerificationRunnerExecuteMethod,
            UnityProjectionVerificationRunnerResultPath =
                qualityGate.UnityProjectionVerificationRunnerResultPath,
            UnityProjectionVerificationRunnerLogPath =
                qualityGate.UnityProjectionVerificationRunnerLogPath,
            UnityProjectionVerificationRunnerPassMarkerPresent =
                qualityGate.UnityProjectionVerificationRunnerPassMarkerPresent,
            UnityProjectionVerificationRunnerCleanupApplied =
                qualityGate.UnityProjectionVerificationRunnerCleanupApplied,
            UnityProjectionVerificationRunnerCleanupScriptAvailable =
                qualityGate.UnityProjectionVerificationRunnerCleanupScriptAvailable,
            UnityProjectionVerificationRunnerCleanupCommand =
                qualityGate.UnityProjectionVerificationRunnerCleanupCommand,
            UnityProjectionVerificationRunnerManualUnityClickingRequired =
                qualityGate.UnityProjectionVerificationRunnerManualUnityClickingRequired,
            UnityProjectionVerificationRunnerEvidencePath =
                qualityGate.UnityProjectionVerificationRunnerEvidencePath,
            UnityProjectionVerificationRunnerExportPath =
                qualityGate.UnityProjectionVerificationRunnerExportPath,
            UnityProjectionVerificationRunnerScriptScanPassed =
                qualityGate.UnityProjectionVerificationRunnerScriptScanPassed,
            UnityProjectionVerificationRunnerResultPassed =
                qualityGate.UnityProjectionVerificationRunnerResultPassed,
            UnityProjectionVerificationRunnerLogPassed =
                qualityGate.UnityProjectionVerificationRunnerLogPassed,
            UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen =
                qualityGate.UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen,
            UnityProjectionVerificationRunnerQualityGatePassed =
                qualityGate.UnityProjectionVerificationRunnerQualityGatePassed,
            Goal127FilesDiscoveredByRelativePaths =
                qualityGate.Goal127FilesDiscoveredByRelativePaths
        };
}
