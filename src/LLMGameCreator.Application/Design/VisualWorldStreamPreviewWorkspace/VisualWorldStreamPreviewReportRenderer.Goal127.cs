namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    private static void AddGoal127ReportLines(
        List<string> lines,
        VisualWorldStreamPreviewWorkspaceReport report) =>
        lines.AddRange(
        [
            string.Empty,
            "## Unity Projection Verification Runner",
            string.Empty,
            $"- runnerStatus: {report.UnityProjectionVerificationRunnerStatus}",
            $"- mode: {report.UnityProjectionVerificationRunnerMode}",
            $"- runnerScriptPath: {report.UnityProjectionVerificationRunnerScriptPath}",
            $"- runnerCmdPath: {report.UnityProjectionVerificationRunnerCmdPath}",
            $"- runnerCommand: {report.UnityProjectionVerificationRunnerCommand}",
            $"- unityExecuteMethod: {report.UnityProjectionVerificationRunnerExecuteMethod}",
            $"- resultPath: {report.UnityProjectionVerificationRunnerResultPath}",
            $"- logPath: {report.UnityProjectionVerificationRunnerLogPath}",
            $"- passMarkerPresent: {report.UnityProjectionVerificationRunnerPassMarkerPresent.ToString().ToLowerInvariant()}",
            $"- cleanupApplied: {report.UnityProjectionVerificationRunnerCleanupApplied.ToString().ToLowerInvariant()}",
            $"- cleanupScriptAvailable: {report.UnityProjectionVerificationRunnerCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- cleanupCommand: {report.UnityProjectionVerificationRunnerCleanupCommand}",
            $"- manualUnityClickingRequired: {report.UnityProjectionVerificationRunnerManualUnityClickingRequired.ToString().ToLowerInvariant()}",
            $"- evidencePath: {report.UnityProjectionVerificationRunnerEvidencePath}",
            $"- exportPath: {report.UnityProjectionVerificationRunnerExportPath}",
            $"- scriptScanPassed: {report.UnityProjectionVerificationRunnerScriptScanPassed.ToString().ToLowerInvariant()}",
            $"- resultPassed: {report.UnityProjectionVerificationRunnerResultPassed.ToString().ToLowerInvariant()}",
            $"- logPassed: {report.UnityProjectionVerificationRunnerLogPassed.ToString().ToLowerInvariant()}",
            $"- goal126FullPlaythroughGreen: {report.UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen.ToString().ToLowerInvariant()}",
            $"- unityProjectionVerificationRunnerQualityGatePassed: {report.UnityProjectionVerificationRunnerQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal127FilesDiscoveredByRelativePaths: {report.Goal127FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}"
        ]);

    private static void AddGoal127QualityLines(
        List<string> lines,
        VisualWorldPreviewWorkspaceQualityGate qualityGate) =>
        lines.AddRange(
        [
            string.Empty,
            "## Goal127 Quality",
            string.Empty,
            $"- unityProjectionVerificationRunnerGroupPresent: {qualityGate.UnityProjectionVerificationRunnerGroupPresent.ToString().ToLowerInvariant()}",
            $"- runnerStatus: {qualityGate.UnityProjectionVerificationRunnerStatus}",
            $"- mode: {qualityGate.UnityProjectionVerificationRunnerMode}",
            $"- runnerCommand: {qualityGate.UnityProjectionVerificationRunnerCommand}",
            $"- passMarkerPresent: {qualityGate.UnityProjectionVerificationRunnerPassMarkerPresent.ToString().ToLowerInvariant()}",
            $"- cleanupApplied: {qualityGate.UnityProjectionVerificationRunnerCleanupApplied.ToString().ToLowerInvariant()}",
            $"- cleanupScriptAvailable: {qualityGate.UnityProjectionVerificationRunnerCleanupScriptAvailable.ToString().ToLowerInvariant()}",
            $"- manualUnityClickingRequired: {qualityGate.UnityProjectionVerificationRunnerManualUnityClickingRequired.ToString().ToLowerInvariant()}",
            $"- scriptScanPassed: {qualityGate.UnityProjectionVerificationRunnerScriptScanPassed.ToString().ToLowerInvariant()}",
            $"- resultPassed: {qualityGate.UnityProjectionVerificationRunnerResultPassed.ToString().ToLowerInvariant()}",
            $"- logPassed: {qualityGate.UnityProjectionVerificationRunnerLogPassed.ToString().ToLowerInvariant()}",
            $"- goal126FullPlaythroughGreen: {qualityGate.UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen.ToString().ToLowerInvariant()}",
            $"- unityProjectionVerificationRunnerQualityGatePassed: {qualityGate.UnityProjectionVerificationRunnerQualityGatePassed.ToString().ToLowerInvariant()}",
            $"- goal127FilesDiscoveredByRelativePaths: {qualityGate.Goal127FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()}",
            $"- winFormsUnityProjectionVerificationRunnerBindingReal: {qualityGate.WinFormsUnityProjectionVerificationRunnerBindingReal.ToString().ToLowerInvariant()}"
        ]);
}
