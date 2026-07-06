using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildUnityProjectionVerificationRunnerDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "runnerStatus="
            + result.Report.UnityProjectionVerificationRunnerStatus,
        "mode="
            + result.Report.UnityProjectionVerificationRunnerMode,
        "runnerScriptPath="
            + result.Report.UnityProjectionVerificationRunnerScriptPath,
        "runnerCmdPath="
            + result.Report.UnityProjectionVerificationRunnerCmdPath,
        "runnerCommand="
            + result.Report.UnityProjectionVerificationRunnerCommand,
        "unityExecuteMethod="
            + result.Report.UnityProjectionVerificationRunnerExecuteMethod,
        "resultPath="
            + result.Report.UnityProjectionVerificationRunnerResultPath,
        "logPath="
            + result.Report.UnityProjectionVerificationRunnerLogPath,
        "passMarkerPresent="
            + result.Report.UnityProjectionVerificationRunnerPassMarkerPresent.ToString().ToLowerInvariant(),
        "cleanupApplied="
            + result.Report.UnityProjectionVerificationRunnerCleanupApplied.ToString().ToLowerInvariant(),
        "cleanupScriptAvailable="
            + result.Report.UnityProjectionVerificationRunnerCleanupScriptAvailable.ToString().ToLowerInvariant(),
        "cleanupCommand="
            + result.Report.UnityProjectionVerificationRunnerCleanupCommand,
        "manualUnityClickingRequired="
            + result.Report.UnityProjectionVerificationRunnerManualUnityClickingRequired.ToString().ToLowerInvariant(),
        "evidencePath="
            + result.Report.UnityProjectionVerificationRunnerEvidencePath,
        "exportPath="
            + result.Report.UnityProjectionVerificationRunnerExportPath,
        "unityProjectionVerificationRunnerQualityGatePassed="
            + result.Report.UnityProjectionVerificationRunnerQualityGatePassed.ToString().ToLowerInvariant(),
        "goal127FilesDiscoveredByRelativePaths="
            + result.Report.Goal127FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildUnityProjectionVerificationRunnerEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "runnerStatus: "
            + entry.UnityProjectionVerificationRunnerStatus,
        "mode: "
            + entry.UnityProjectionVerificationRunnerMode,
        "runnerScriptPath: "
            + entry.UnityProjectionVerificationRunnerScriptPath,
        "runnerCmdPath: "
            + entry.UnityProjectionVerificationRunnerCmdPath,
        "runnerCommand: "
            + entry.UnityProjectionVerificationRunnerCommand,
        "unityExecuteMethod: "
            + entry.UnityProjectionVerificationRunnerExecuteMethod,
        "resultPath: "
            + entry.UnityProjectionVerificationRunnerResultPath,
        "logPath: "
            + entry.UnityProjectionVerificationRunnerLogPath,
        "passMarkerPresent: "
            + entry.UnityProjectionVerificationRunnerPassMarkerPresent.ToString().ToLowerInvariant(),
        "cleanupApplied: "
            + entry.UnityProjectionVerificationRunnerCleanupApplied.ToString().ToLowerInvariant(),
        "cleanupScriptAvailable: "
            + entry.UnityProjectionVerificationRunnerCleanupScriptAvailable.ToString().ToLowerInvariant(),
        "cleanupCommand: "
            + entry.UnityProjectionVerificationRunnerCleanupCommand,
        "manualUnityClickingRequired: "
            + entry.UnityProjectionVerificationRunnerManualUnityClickingRequired.ToString().ToLowerInvariant(),
        "evidencePath: "
            + entry.UnityProjectionVerificationRunnerEvidencePath,
        "exportPath: "
            + entry.UnityProjectionVerificationRunnerExportPath,
        "scriptScanPassed: "
            + entry.UnityProjectionVerificationRunnerScriptScanPassed.ToString().ToLowerInvariant(),
        "resultPassed: "
            + entry.UnityProjectionVerificationRunnerResultPassed.ToString().ToLowerInvariant(),
        "logPassed: "
            + entry.UnityProjectionVerificationRunnerLogPassed.ToString().ToLowerInvariant(),
        "goal126FullPlaythroughGreen: "
            + entry.UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen.ToString().ToLowerInvariant()
    ];
}
