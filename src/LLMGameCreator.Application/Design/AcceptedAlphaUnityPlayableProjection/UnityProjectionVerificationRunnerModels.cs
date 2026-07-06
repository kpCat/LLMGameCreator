namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class UnityProjectionVerificationRunnerVocabulary
{
    public const string GoalId =
        "goal_127_winforms_unity_projection_verification_runner";
    public const string ScenarioId =
        "goal-127-winforms-unity-projection-verification-runner";
    public const string Mode = "GenericFullPlaythrough";
    public const string UnityBatchmodeExecuteMethod =
        "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageFullPlaythroughSmoke";
    public const string PassMarker =
        "GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS";
    public const string FailMarker =
        "GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL";
    public const string MaterialWarningMarker =
        "Instantiating material due to calling renderer.material during edit mode";
    public const string RendererMaterialMarker =
        "UnityEngine.Renderer:get_material()";
    public const string RunnerScriptPath =
        ".devflow/scripts/run-unity-projection-verification.ps1";
    public const string RunnerCmdPath =
        ".devflow/scripts/run-unity-projection-verification.cmd";
    public const string RunnerCommand =
        ".devflow\\scripts\\run-unity-projection-verification.cmd";
    public const string CleanupScriptPath =
        ".devflow/scripts/clean-unity-editor-noise.ps1";
    public const string CleanupCommand =
        ".\\.devflow\\scripts\\clean-unity-editor-noise.ps1 -Apply";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-127-winforms-unity-projection-verification-runner";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-127-winforms-unity-projection-verification-runner";
    public const string DocumentationPath =
        "docs/manual-acceptance/winforms-unity-projection-verification-runner.md";

    public const string DashboardFileName =
        "unity-projection-verification-runner-dashboard.json";
    public const string ScriptScanFileName =
        "unity-projection-verification-runner-script-scan.json";
    public const string ResultFileName =
        "unity-projection-verification-runner-result.json";
    public const string LogScanFileName =
        "unity-projection-verification-runner-log-scan.json";
    public const string ReportFileName =
        "unity-projection-verification-runner-report.md";
    public const string NegativeProofFileName =
        "unity-projection-verification-runner-negative-proof.json";
    public const string FileIndexFileName =
        "unity-projection-verification-runner-file-index.json";
    public const string UnityBatchmodeLogFileName =
        "unity-batchmode-generic-full-playthrough-runner.log";

    public const string ResultRelativePath =
        ProceduralOutputDirectory + "/" + ResultFileName;
    public const string UnityBatchmodeLogRelativePath =
        ProceduralOutputDirectory + "/" + UnityBatchmodeLogFileName;
    public const string ExportResultRelativePath =
        ExportPackageDirectory + "/" + ResultFileName;
    public const string UnityBatchmodeExportLogRelativePath =
        ExportPackageDirectory + "/" + UnityBatchmodeLogFileName;

    public static IReadOnlyList<string> RequiredProceduralFileNames =>
    [
        DashboardFileName,
        ScriptScanFileName,
        ResultFileName,
        LogScanFileName,
        ReportFileName,
        NegativeProofFileName,
        FileIndexFileName,
        UnityBatchmodeLogFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames => RequiredProceduralFileNames;
}

public sealed record UnityProjectionVerificationRunnerBuildResult
{
    public UnityProjectionVerificationRunnerDashboard Dashboard { get; init; } = new();
    public UnityProjectionVerificationRunnerGoal126Evidence Goal126Evidence { get; init; } = new();
    public UnityProjectionVerificationRunnerScriptScan ScriptScan { get; init; } = new();
    public UnityProjectionVerificationRunnerResultScan ResultScan { get; init; } = new();
    public UnityProjectionVerificationRunnerLogScan LogScan { get; init; } = new();
    public UnityProjectionVerificationRunnerNegativeProof NegativeProof { get; init; } = new();
    public UnityProjectionVerificationRunnerFileIndex ProceduralFileIndex { get; init; } = new();
    public UnityProjectionVerificationRunnerFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record UnityProjectionVerificationRunnerWriteResult
{
    public UnityProjectionVerificationRunnerBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record UnityProjectionVerificationRunnerDashboard
{
    public string GoalId { get; init; } = UnityProjectionVerificationRunnerVocabulary.GoalId;
    public string RunnerStatus { get; init; } = "BLOCKED";
    public string Mode { get; init; } = UnityProjectionVerificationRunnerVocabulary.Mode;
    public string RunnerScriptPath { get; init; } =
        UnityProjectionVerificationRunnerVocabulary.RunnerScriptPath;
    public string RunnerCmdPath { get; init; } =
        UnityProjectionVerificationRunnerVocabulary.RunnerCmdPath;
    public string RunnerCommand { get; init; } =
        UnityProjectionVerificationRunnerVocabulary.RunnerCommand;
    public string UnityExecuteMethod { get; init; } =
        UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeExecuteMethod;
    public string LastResultPath { get; init; } =
        UnityProjectionVerificationRunnerVocabulary.ResultRelativePath;
    public string LastLogPath { get; init; } =
        UnityProjectionVerificationRunnerVocabulary.UnityBatchmodeLogRelativePath;
    public bool Goal126FullPlaythroughGreen { get; init; }
    public bool RunnerScriptExists { get; init; }
    public bool RunnerCmdExists { get; init; }
    public bool ScriptScanPassed { get; init; }
    public bool ResultArtifactExists { get; init; }
    public bool LogArtifactExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; }
    public bool MaterialWarningAbsent { get; init; }
    public bool CleanupApplied { get; init; }
    public int CleanupExitCode { get; init; } = -1;
    public bool CleanupScriptAvailable { get; init; }
    public string CleanupCommand { get; init; } =
        UnityProjectionVerificationRunnerVocabulary.CleanupCommand;
    public bool ManualUnityClickingRequired { get; init; } = true;
    public string EvidencePath { get; init; } =
        UnityProjectionVerificationRunnerVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        UnityProjectionVerificationRunnerVocabulary.ExportPackageDirectory;
    public bool NoRuntimeProviderSchemaLuaGeneratorLibrary { get; init; } = true;
    public bool NoSamplePackageUnityProjectSettingsOrManualMutation { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record UnityProjectionVerificationRunnerGoal126Evidence
{
    public string GoalId { get; init; } = UnityProjectionVerificationRunnerVocabulary.GoalId;
    public bool DashboardExists { get; init; }
    public bool LogScanExists { get; init; }
    public bool FullPlaythroughStatusGreen { get; init; }
    public bool Goal126PassMarkerPresent { get; init; }
    public bool Goal126FailMarkerAbsent { get; init; }
    public bool Passed { get; init; }
}

public sealed record UnityProjectionVerificationRunnerScriptScan
{
    public string GoalId { get; init; } = UnityProjectionVerificationRunnerVocabulary.GoalId;
    public bool RunnerScriptExists { get; init; }
    public bool RunnerCmdExists { get; init; }
    public bool SupportsGenericFullPlaythroughMode { get; init; }
    public bool SupportsUnityPath { get; init; }
    public bool SupportsDryRun { get; init; }
    public bool SupportsApplyCleanup { get; init; }
    public bool ExecuteMethodPresent { get; init; }
    public bool PassMarkerScanPresent { get; init; }
    public bool FailMarkerScanPresent { get; init; }
    public bool MaterialWarningScanPresent { get; init; }
    public bool CleanupDelegatesToBoundedScript { get; init; }
    public bool CmdWrapperUsesApplyCleanup { get; init; }
    public bool NoBroadGitClean { get; init; }
    public bool NoForbiddenMutationTargets { get; init; }
    public bool WritesRequiredResultJsonFields { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record UnityProjectionVerificationRunnerResultScan
{
    public string GoalId { get; init; } = UnityProjectionVerificationRunnerVocabulary.GoalId;
    public bool ResultExists { get; init; }
    public string Mode { get; init; } = string.Empty;
    public string UnityPath { get; init; } = string.Empty;
    public int UnityExitCode { get; init; } = -1;
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; }
    public bool MaterialWarningAbsent { get; init; }
    public bool CleanupApplied { get; init; }
    public int CleanupExitCode { get; init; } = -1;
    public bool Passed { get; init; }
    public string LogPath { get; init; } = string.Empty;
    public bool RequiredFieldsPresent { get; init; }
}

public sealed record UnityProjectionVerificationRunnerLogScan
{
    public string GoalId { get; init; } = UnityProjectionVerificationRunnerVocabulary.GoalId;
    public bool LogExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; } = true;
    public bool MaterialWarningAbsent { get; init; } = true;
    public bool Passed { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record UnityProjectionVerificationRunnerNegativeProof
{
    public string GoalId { get; init; } = UnityProjectionVerificationRunnerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool SamplePackageMutationRejected { get; init; }
    public bool UnityProjectSettingsMutationRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool BroadGitCleanRejected { get; init; }
    public bool OnlyAllowedRunnerArtifactsExpected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record UnityProjectionVerificationRunnerFileIndex
{
    public string GoalId { get; init; } = UnityProjectionVerificationRunnerVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<UnityProjectionVerificationRunnerFileIndexEntry> Files { get; init; } = [];
}

public sealed record UnityProjectionVerificationRunnerFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
