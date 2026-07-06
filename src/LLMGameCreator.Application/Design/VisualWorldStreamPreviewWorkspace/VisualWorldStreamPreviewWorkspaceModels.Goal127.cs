namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial record VisualWorldPreviewArtifactEntry
{
    public string UnityProjectionVerificationRunnerStatus { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerMode { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerScriptPath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerCmdPath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerCommand { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerExecuteMethod { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerResultPath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerLogPath { get; init; } = string.Empty;
    public bool UnityProjectionVerificationRunnerPassMarkerPresent { get; init; }
    public bool UnityProjectionVerificationRunnerCleanupApplied { get; init; }
    public bool UnityProjectionVerificationRunnerCleanupScriptAvailable { get; init; }
    public string UnityProjectionVerificationRunnerCleanupCommand { get; init; } = string.Empty;
    public bool UnityProjectionVerificationRunnerManualUnityClickingRequired { get; init; }
    public string UnityProjectionVerificationRunnerEvidencePath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerExportPath { get; init; } = string.Empty;
    public bool UnityProjectionVerificationRunnerScriptScanPassed { get; init; }
    public bool UnityProjectionVerificationRunnerResultPassed { get; init; }
    public bool UnityProjectionVerificationRunnerLogPassed { get; init; }
    public bool UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen { get; init; }
}

public sealed partial record VisualWorldPreviewWinFormsBindingInventory
{
    public bool PageBindDisplaysUnityProjectionVerificationRunner { get; init; }
}

public sealed partial record VisualWorldPreviewWorkspaceQualityGate
{
    public bool UnityProjectionVerificationRunnerGroupPresent { get; init; }
    public string UnityProjectionVerificationRunnerStatus { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerMode { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerScriptPath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerCmdPath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerCommand { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerExecuteMethod { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerResultPath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerLogPath { get; init; } = string.Empty;
    public bool UnityProjectionVerificationRunnerPassMarkerPresent { get; init; }
    public bool UnityProjectionVerificationRunnerCleanupApplied { get; init; }
    public bool UnityProjectionVerificationRunnerCleanupScriptAvailable { get; init; }
    public string UnityProjectionVerificationRunnerCleanupCommand { get; init; } = string.Empty;
    public bool UnityProjectionVerificationRunnerManualUnityClickingRequired { get; init; } = true;
    public string UnityProjectionVerificationRunnerEvidencePath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerExportPath { get; init; } = string.Empty;
    public bool UnityProjectionVerificationRunnerScriptScanPassed { get; init; }
    public bool UnityProjectionVerificationRunnerResultPassed { get; init; }
    public bool UnityProjectionVerificationRunnerLogPassed { get; init; }
    public bool UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen { get; init; }
    public bool UnityProjectionVerificationRunnerQualityGatePassed { get; init; }
    public bool Goal127FilesDiscoveredByRelativePaths { get; init; }
    public bool WinFormsUnityProjectionVerificationRunnerBindingReal { get; init; }
}

public sealed partial record VisualWorldStreamPreviewWorkspaceReport
{
    public string UnityProjectionVerificationRunnerStatus { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerMode { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerScriptPath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerCmdPath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerCommand { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerExecuteMethod { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerResultPath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerLogPath { get; init; } = string.Empty;
    public bool UnityProjectionVerificationRunnerPassMarkerPresent { get; init; }
    public bool UnityProjectionVerificationRunnerCleanupApplied { get; init; }
    public bool UnityProjectionVerificationRunnerCleanupScriptAvailable { get; init; }
    public string UnityProjectionVerificationRunnerCleanupCommand { get; init; } = string.Empty;
    public bool UnityProjectionVerificationRunnerManualUnityClickingRequired { get; init; } = true;
    public string UnityProjectionVerificationRunnerEvidencePath { get; init; } = string.Empty;
    public string UnityProjectionVerificationRunnerExportPath { get; init; } = string.Empty;
    public bool UnityProjectionVerificationRunnerScriptScanPassed { get; init; }
    public bool UnityProjectionVerificationRunnerResultPassed { get; init; }
    public bool UnityProjectionVerificationRunnerLogPassed { get; init; }
    public bool UnityProjectionVerificationRunnerGoal126FullPlaythroughGreen { get; init; }
    public bool UnityProjectionVerificationRunnerQualityGatePassed { get; init; }
    public bool Goal127FilesDiscoveredByRelativePaths { get; init; }
}
