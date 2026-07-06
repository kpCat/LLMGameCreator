namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal128ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Parameterized GamePackage Projection Runner",
            string.Empty,
            $"- parameterizedRunnerStatus: {report.ParameterizedGamePackageRunnerStatus}",
            $"- packagePath: {report.ParameterizedGamePackageRunnerPackagePath}",
            $"- packagePathRelative: {report.ParameterizedGamePackageRunnerPackagePathRelative}",
            $"- normalCommand: {report.ParameterizedGamePackageRunnerNormalCommand}",
            $"- exampleCommandWithPackagePath: {report.ParameterizedGamePackageRunnerExampleCommandWithPackagePath}",
            $"- resultPath: {report.ParameterizedGamePackageRunnerResultPath}",
            $"- logPath: {report.ParameterizedGamePackageRunnerLogPath}",
            $"- unityExitCode: {report.ParameterizedGamePackageRunnerUnityExitCode}",
            $"- passMarkerPresent: {report.ParameterizedGamePackageRunnerPassMarkerPresent.ToString().ToLowerInvariant()}",
            $"- cleanupApplied: {report.ParameterizedGamePackageRunnerCleanupApplied.ToString().ToLowerInvariant()}",
            $"- manualUnityOptional: {report.ParameterizedGamePackageRunnerManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {report.ParameterizedGamePackageRunnerProjectionOnly.ToString().ToLowerInvariant()}",
            $"- evidencePath: {report.ParameterizedGamePackageRunnerEvidencePath}",
            $"- exportPath: {report.ParameterizedGamePackageRunnerExportPath}",
            $"- scriptScanPassed: {report.ParameterizedGamePackageRunnerScriptScanPassed.ToString().ToLowerInvariant()}",
            $"- unitySourceScanPassed: {report.ParameterizedGamePackageRunnerUnitySourceScanPassed.ToString().ToLowerInvariant()}",
            $"- resultPassed: {report.ParameterizedGamePackageRunnerResultPassed.ToString().ToLowerInvariant()}",
            $"- logPassed: {report.ParameterizedGamePackageRunnerLogPassed.ToString().ToLowerInvariant()}",
            $"- goal127RunnerGreen: {report.ParameterizedGamePackageRunnerGoal127Green.ToString().ToLowerInvariant()}",
            $"- parameterizedGamePackageRunnerQualityGatePassed: {report.ParameterizedGamePackageRunnerQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal128FilesDiscoveredByRelativePaths: {report.Goal128FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal128QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal128 Quality",
            string.Empty,
            $"- parameterizedGamePackageRunnerGroupPresent: {qualityGate.ParameterizedGamePackageRunnerGroupPresent.ToString().ToLowerInvariant()}",
            $"- parameterizedRunnerStatus: {qualityGate.ParameterizedGamePackageRunnerStatus}",
            $"- packagePathRelative: {qualityGate.ParameterizedGamePackageRunnerPackagePathRelative}",
            $"- normalCommand: {qualityGate.ParameterizedGamePackageRunnerNormalCommand}",
            $"- passMarkerPresent: {qualityGate.ParameterizedGamePackageRunnerPassMarkerPresent.ToString().ToLowerInvariant()}",
            $"- cleanupApplied: {qualityGate.ParameterizedGamePackageRunnerCleanupApplied.ToString().ToLowerInvariant()}",
            $"- manualUnityOptional: {qualityGate.ParameterizedGamePackageRunnerManualUnityOptional.ToString().ToLowerInvariant()}",
            $"- projectionOnly: {qualityGate.ParameterizedGamePackageRunnerProjectionOnly.ToString().ToLowerInvariant()}",
            $"- scriptScanPassed: {qualityGate.ParameterizedGamePackageRunnerScriptScanPassed.ToString().ToLowerInvariant()}",
            $"- unitySourceScanPassed: {qualityGate.ParameterizedGamePackageRunnerUnitySourceScanPassed.ToString().ToLowerInvariant()}",
            $"- resultPassed: {qualityGate.ParameterizedGamePackageRunnerResultPassed.ToString().ToLowerInvariant()}",
            $"- logPassed: {qualityGate.ParameterizedGamePackageRunnerLogPassed.ToString().ToLowerInvariant()}",
            $"- goal127RunnerGreen: {qualityGate.ParameterizedGamePackageRunnerGoal127Green.ToString().ToLowerInvariant()}",
            $"- parameterizedGamePackageRunnerQualityGatePassed: {qualityGate.ParameterizedGamePackageRunnerQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal128FilesDiscoveredByRelativePaths: {qualityGate.Goal128FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsParameterizedGamePackageRunnerBindingReal: {qualityGate.WinFormsParameterizedGamePackageRunnerBindingReal.ToString().ToLowerInvariant()}"
        ]);
}
