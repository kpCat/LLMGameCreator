namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static VisualWorldStreamPreviewWorkspaceReport WithGoal128ReportFields(
        VisualWorldStreamPreviewWorkspaceReport report,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        report with
        {
            ParameterizedGamePackageRunnerStatus =
                qualityGate.ParameterizedGamePackageRunnerStatus,
            ParameterizedGamePackageRunnerPackagePath =
                qualityGate.ParameterizedGamePackageRunnerPackagePath,
            ParameterizedGamePackageRunnerPackagePathRelative =
                qualityGate.ParameterizedGamePackageRunnerPackagePathRelative,
            ParameterizedGamePackageRunnerNormalCommand =
                qualityGate.ParameterizedGamePackageRunnerNormalCommand,
            ParameterizedGamePackageRunnerExampleCommandWithPackagePath =
                qualityGate.ParameterizedGamePackageRunnerExampleCommandWithPackagePath,
            ParameterizedGamePackageRunnerResultPath =
                qualityGate.ParameterizedGamePackageRunnerResultPath,
            ParameterizedGamePackageRunnerLogPath =
                qualityGate.ParameterizedGamePackageRunnerLogPath,
            ParameterizedGamePackageRunnerUnityExitCode =
                qualityGate.ParameterizedGamePackageRunnerUnityExitCode,
            ParameterizedGamePackageRunnerPassMarkerPresent =
                qualityGate.ParameterizedGamePackageRunnerPassMarkerPresent,
            ParameterizedGamePackageRunnerCleanupApplied =
                qualityGate.ParameterizedGamePackageRunnerCleanupApplied,
            ParameterizedGamePackageRunnerManualUnityOptional =
                qualityGate.ParameterizedGamePackageRunnerManualUnityOptional,
            ParameterizedGamePackageRunnerProjectionOnly =
                qualityGate.ParameterizedGamePackageRunnerProjectionOnly,
            ParameterizedGamePackageRunnerEvidencePath =
                qualityGate.ParameterizedGamePackageRunnerEvidencePath,
            ParameterizedGamePackageRunnerExportPath =
                qualityGate.ParameterizedGamePackageRunnerExportPath,
            ParameterizedGamePackageRunnerScriptScanPassed =
                qualityGate.ParameterizedGamePackageRunnerScriptScanPassed,
            ParameterizedGamePackageRunnerUnitySourceScanPassed =
                qualityGate.ParameterizedGamePackageRunnerUnitySourceScanPassed,
            ParameterizedGamePackageRunnerResultPassed =
                qualityGate.ParameterizedGamePackageRunnerResultPassed,
            ParameterizedGamePackageRunnerLogPassed =
                qualityGate.ParameterizedGamePackageRunnerLogPassed,
            ParameterizedGamePackageRunnerGoal127Green =
                qualityGate.ParameterizedGamePackageRunnerGoal127Green,
            ParameterizedGamePackageRunnerQualityGatePassed =
                qualityGate.ParameterizedGamePackageRunnerQualityGatePassed,
            Goal128FilesDiscoveredByRelativePaths =
                qualityGate.Goal128FilesDiscoveredByRelativePaths
        };
}
