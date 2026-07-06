using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

namespace LLMGameCreator.WinForms.Pages;

public sealed partial class VisualWorldStreamPreviewWorkspacePageControl
{
    private static IReadOnlyList<string> BuildParameterizedGamePackageRunnerDiagnosticLines(
        VisualWorldStreamPreviewWorkspaceResult result) =>
    [
        "parameterizedRunnerStatus="
            + result.Report.ParameterizedGamePackageRunnerStatus,
        "packagePath="
            + result.Report.ParameterizedGamePackageRunnerPackagePath,
        "packagePathRelative="
            + result.Report.ParameterizedGamePackageRunnerPackagePathRelative,
        "normalCommand="
            + result.Report.ParameterizedGamePackageRunnerNormalCommand,
        "exampleCommandWithPackagePath="
            + result.Report.ParameterizedGamePackageRunnerExampleCommandWithPackagePath,
        "resultPath="
            + result.Report.ParameterizedGamePackageRunnerResultPath,
        "logPath="
            + result.Report.ParameterizedGamePackageRunnerLogPath,
        "unityExitCode="
            + result.Report.ParameterizedGamePackageRunnerUnityExitCode,
        "passMarkerPresent="
            + result.Report.ParameterizedGamePackageRunnerPassMarkerPresent.ToString().ToLowerInvariant(),
        "cleanupApplied="
            + result.Report.ParameterizedGamePackageRunnerCleanupApplied.ToString().ToLowerInvariant(),
        "manualUnityOptional="
            + result.Report.ParameterizedGamePackageRunnerManualUnityOptional.ToString().ToLowerInvariant(),
        "projectionOnly="
            + result.Report.ParameterizedGamePackageRunnerProjectionOnly.ToString().ToLowerInvariant(),
        "evidencePath="
            + result.Report.ParameterizedGamePackageRunnerEvidencePath,
        "exportPath="
            + result.Report.ParameterizedGamePackageRunnerExportPath,
        "parameterizedGamePackageRunnerQualityGatePassed="
            + result.Report.ParameterizedGamePackageRunnerQualityGatePassed.ToString().ToLowerInvariant(),
        "goal128FilesDiscoveredByRelativePaths="
            + result.Report.Goal128FilesDiscoveredByRelativePaths.ToString().ToLowerInvariant()
    ];

    private static IReadOnlyList<string> BuildParameterizedGamePackageRunnerEntryLines(
        VisualWorldPreviewArtifactEntry entry) =>
    [
        "parameterizedRunnerStatus: "
            + entry.ParameterizedGamePackageRunnerStatus,
        "packagePath: "
            + entry.ParameterizedGamePackageRunnerPackagePath,
        "packagePathRelative: "
            + entry.ParameterizedGamePackageRunnerPackagePathRelative,
        "normalCommand: "
            + entry.ParameterizedGamePackageRunnerNormalCommand,
        "exampleCommandWithPackagePath: "
            + entry.ParameterizedGamePackageRunnerExampleCommandWithPackagePath,
        "resultPath: "
            + entry.ParameterizedGamePackageRunnerResultPath,
        "logPath: "
            + entry.ParameterizedGamePackageRunnerLogPath,
        "unityExitCode: "
            + entry.ParameterizedGamePackageRunnerUnityExitCode,
        "passMarkerPresent: "
            + entry.ParameterizedGamePackageRunnerPassMarkerPresent.ToString().ToLowerInvariant(),
        "cleanupApplied: "
            + entry.ParameterizedGamePackageRunnerCleanupApplied.ToString().ToLowerInvariant(),
        "manualUnityOptional: "
            + entry.ParameterizedGamePackageRunnerManualUnityOptional.ToString().ToLowerInvariant(),
        "projectionOnly: "
            + entry.ParameterizedGamePackageRunnerProjectionOnly.ToString().ToLowerInvariant(),
        "evidencePath: "
            + entry.ParameterizedGamePackageRunnerEvidencePath,
        "exportPath: "
            + entry.ParameterizedGamePackageRunnerExportPath,
        "scriptScanPassed: "
            + entry.ParameterizedGamePackageRunnerScriptScanPassed.ToString().ToLowerInvariant(),
        "unitySourceScanPassed: "
            + entry.ParameterizedGamePackageRunnerUnitySourceScanPassed.ToString().ToLowerInvariant(),
        "resultPassed: "
            + entry.ParameterizedGamePackageRunnerResultPassed.ToString().ToLowerInvariant(),
        "logPassed: "
            + entry.ParameterizedGamePackageRunnerLogPassed.ToString().ToLowerInvariant(),
        "goal127RunnerGreen: "
            + entry.ParameterizedGamePackageRunnerGoal127Green.ToString().ToLowerInvariant()
    ];
}
