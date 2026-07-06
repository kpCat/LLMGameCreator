namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string ParameterizedGamePackageRunnerStatus { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerPackagePath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerPackagePathRelative { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerNormalCommand { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerExampleCommandWithPackagePath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerResultPath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerLogPath { get; init; } = string.Empty;
    public int ParameterizedGamePackageRunnerUnityExitCode { get; init; } = -1;
    public bool ParameterizedGamePackageRunnerPassMarkerPresent { get; init; }
    public bool ParameterizedGamePackageRunnerCleanupApplied { get; init; }
    public bool ParameterizedGamePackageRunnerManualUnityOptional { get; init; }
    public bool ParameterizedGamePackageRunnerProjectionOnly { get; init; }
    public string ParameterizedGamePackageRunnerEvidencePath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerExportPath { get; init; } = string.Empty;
    public bool ParameterizedGamePackageRunnerScriptScanPassed { get; init; }
    public bool ParameterizedGamePackageRunnerUnitySourceScanPassed { get; init; }
    public bool ParameterizedGamePackageRunnerResultPassed { get; init; }
    public bool ParameterizedGamePackageRunnerLogPassed { get; init; }
    public bool ParameterizedGamePackageRunnerGoal127Green { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysParameterizedGamePackageRunner { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool ParameterizedGamePackageRunnerGroupPresent { get; init; }
    public string ParameterizedGamePackageRunnerStatus { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerPackagePath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerPackagePathRelative { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerNormalCommand { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerExampleCommandWithPackagePath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerResultPath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerLogPath { get; init; } = string.Empty;
    public int ParameterizedGamePackageRunnerUnityExitCode { get; init; } = -1;
    public bool ParameterizedGamePackageRunnerPassMarkerPresent { get; init; }
    public bool ParameterizedGamePackageRunnerCleanupApplied { get; init; }
    public bool ParameterizedGamePackageRunnerManualUnityOptional { get; init; }
    public bool ParameterizedGamePackageRunnerProjectionOnly { get; init; }
    public string ParameterizedGamePackageRunnerEvidencePath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerExportPath { get; init; } = string.Empty;
    public bool ParameterizedGamePackageRunnerScriptScanPassed { get; init; }
    public bool ParameterizedGamePackageRunnerUnitySourceScanPassed { get; init; }
    public bool ParameterizedGamePackageRunnerResultPassed { get; init; }
    public bool ParameterizedGamePackageRunnerLogPassed { get; init; }
    public bool ParameterizedGamePackageRunnerGoal127Green { get; init; }
    public bool ParameterizedGamePackageRunnerQualityGatePassed { get; init; }
    public bool Goal128FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsParameterizedGamePackageRunnerBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string ParameterizedGamePackageRunnerStatus { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerPackagePath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerPackagePathRelative { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerNormalCommand { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerExampleCommandWithPackagePath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerResultPath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerLogPath { get; init; } = string.Empty;
    public int ParameterizedGamePackageRunnerUnityExitCode { get; init; } = -1;
    public bool ParameterizedGamePackageRunnerPassMarkerPresent { get; init; }
    public bool ParameterizedGamePackageRunnerCleanupApplied { get; init; }
    public bool ParameterizedGamePackageRunnerManualUnityOptional { get; init; }
    public bool ParameterizedGamePackageRunnerProjectionOnly { get; init; }
    public string ParameterizedGamePackageRunnerEvidencePath { get; init; } = string.Empty;
    public string ParameterizedGamePackageRunnerExportPath { get; init; } = string.Empty;
    public bool ParameterizedGamePackageRunnerScriptScanPassed { get; init; }
    public bool ParameterizedGamePackageRunnerUnitySourceScanPassed { get; init; }
    public bool ParameterizedGamePackageRunnerResultPassed { get; init; }
    public bool ParameterizedGamePackageRunnerLogPassed { get; init; }
    public bool ParameterizedGamePackageRunnerGoal127Green { get; init; }
    public bool ParameterizedGamePackageRunnerQualityGatePassed { get; init; }
    public bool Goal128FilesDiscoveredByRelativePaths { get; init; }
}
