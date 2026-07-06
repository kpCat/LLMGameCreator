namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class ParameterizedGamePackageProjectionRunnerVocabulary
{
    public const string GoalId =
        "goal_128_parameterized_gamepackage_projection_runner_and_winforms_command_surface";
    public const string ScenarioId =
        "goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface";
    public const string Mode = "GenericFullPlaythrough";
    public const string DefaultPackageRelativePath = "samples/minimal-map-game/package.json";
    public const string PackageArgumentName = "-llmgcPackagePath";
    public const string UnityBatchmodeExecuteMethod =
        "LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeParameterizedGamePackageFullPlaythroughSmoke";
    public const string PassMarker =
        "GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS";
    public const string FailMarker =
        "GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_FAIL";
    public const string MaterialWarningMarker =
        "Instantiating material due to calling renderer.material during edit mode";
    public const string RendererMaterialMarker =
        "UnityEngine.Renderer:get_material()";
    public const string RunnerScriptPath =
        ".devflow/scripts/run-unity-projection-verification.ps1";
    public const string RunnerCmdPath =
        ".devflow/scripts/run-unity-projection-verification.cmd";
    public const string NormalCommand =
        ".devflow\\scripts\\run-unity-projection-verification.cmd";
    public const string ExampleCommandWithPackagePath =
        ".devflow\\scripts\\run-unity-projection-verification.cmd -PackagePath samples\\minimal-map-game\\package.json";
    public const string CleanupScriptPath =
        ".devflow/scripts/clean-unity-editor-noise.ps1";
    public const string CleanupCommand =
        ".\\.devflow\\scripts\\clean-unity-editor-noise.ps1 -Apply";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface";
    public const string DocumentationPath =
        "docs/manual-acceptance/parameterized-gamepackage-projection-runner-and-winforms-command-surface.md";

    public const string DashboardFileName =
        "parameterized-gamepackage-runner-dashboard.json";
    public const string ScriptScanFileName =
        "parameterized-gamepackage-runner-script-scan.json";
    public const string UnitySourceScanFileName =
        "parameterized-gamepackage-runner-unity-source-scan.json";
    public const string ResultFileName =
        "parameterized-gamepackage-runner-result.json";
    public const string LogScanFileName =
        "parameterized-gamepackage-runner-log-scan.json";
    public const string ReportFileName =
        "parameterized-gamepackage-runner-report.md";
    public const string NegativeProofFileName =
        "parameterized-gamepackage-runner-negative-proof.json";
    public const string FileIndexFileName =
        "parameterized-gamepackage-runner-file-index.json";
    public const string UnityBatchmodeLogFileName =
        "unity-batchmode-parameterized-gamepackage-full-playthrough.log";

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
        UnitySourceScanFileName,
        ResultFileName,
        LogScanFileName,
        ReportFileName,
        NegativeProofFileName,
        FileIndexFileName,
        UnityBatchmodeLogFileName
    ];

    public static IReadOnlyList<string> RequiredExportFileNames => RequiredProceduralFileNames;
}

public sealed record ParameterizedGamePackageProjectionRunnerBuildResult
{
    public ParameterizedGamePackageProjectionRunnerDashboard Dashboard { get; init; } = new();
    public ParameterizedGamePackageProjectionRunnerGoal127Evidence Goal127Evidence { get; init; } = new();
    public ParameterizedGamePackageProjectionRunnerScriptScan ScriptScan { get; init; } = new();
    public ParameterizedGamePackageProjectionRunnerUnitySourceScan UnitySourceScan { get; init; } = new();
    public ParameterizedGamePackageProjectionRunnerResultScan ResultScan { get; init; } = new();
    public ParameterizedGamePackageProjectionRunnerLogScan LogScan { get; init; } = new();
    public ParameterizedGamePackageProjectionRunnerNegativeProof NegativeProof { get; init; } = new();
    public ParameterizedGamePackageProjectionRunnerFileIndex ProceduralFileIndex { get; init; } = new();
    public ParameterizedGamePackageProjectionRunnerFileIndex ExportFileIndex { get; init; } = new();
    public IReadOnlyDictionary<string, string> ProceduralFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> ExportFiles { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string DocumentationMarkdown { get; init; } = string.Empty;
}

public sealed record ParameterizedGamePackageProjectionRunnerWriteResult
{
    public ParameterizedGamePackageProjectionRunnerBuildResult Result { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record ParameterizedGamePackageProjectionRunnerDashboard
{
    public string GoalId { get; init; } = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId;
    public string ParameterizedRunnerStatus { get; init; } = "BLOCKED";
    public string Mode { get; init; } = ParameterizedGamePackageProjectionRunnerVocabulary.Mode;
    public string PackagePath { get; init; } = string.Empty;
    public string PackagePathRelative { get; init; } =
        ParameterizedGamePackageProjectionRunnerVocabulary.DefaultPackageRelativePath;
    public bool PackagePathResolved { get; init; }
    public bool PackagePathUnderRepo { get; init; }
    public string NormalCommand { get; init; } =
        ParameterizedGamePackageProjectionRunnerVocabulary.NormalCommand;
    public string ExampleCommandWithPackagePath { get; init; } =
        ParameterizedGamePackageProjectionRunnerVocabulary.ExampleCommandWithPackagePath;
    public string ResultPath { get; init; } =
        ParameterizedGamePackageProjectionRunnerVocabulary.ResultRelativePath;
    public string LogPath { get; init; } =
        ParameterizedGamePackageProjectionRunnerVocabulary.UnityBatchmodeLogRelativePath;
    public int UnityExitCode { get; init; } = -1;
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; } = true;
    public bool MaterialWarningAbsent { get; init; } = true;
    public bool CleanupApplied { get; init; }
    public int CleanupExitCode { get; init; } = -1;
    public bool ManualUnityOptional { get; init; } = true;
    public bool ProjectionOnly { get; init; } = true;
    public bool RunnerScriptExists { get; init; }
    public bool RunnerCmdExists { get; init; }
    public bool ScriptScanPassed { get; init; }
    public bool UnitySourceScanPassed { get; init; }
    public bool ResultArtifactExists { get; init; }
    public bool LogArtifactExists { get; init; }
    public bool Goal127RunnerGreen { get; init; }
    public bool NegativeProofPassed { get; init; }
    public string EvidencePath { get; init; } =
        ParameterizedGamePackageProjectionRunnerVocabulary.ProceduralOutputDirectory;
    public string ExportPath { get; init; } =
        ParameterizedGamePackageProjectionRunnerVocabulary.ExportPackageDirectory;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record ParameterizedGamePackageProjectionRunnerGoal127Evidence
{
    public string GoalId { get; init; } = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId;
    public bool DashboardExists { get; init; }
    public bool Goal127RunnerGreen { get; init; }
    public bool Goal127PassMarkerPresent { get; init; }
    public bool Goal127CleanupApplied { get; init; }
    public bool Passed { get; init; }
}

public sealed record ParameterizedGamePackageProjectionRunnerScriptScan
{
    public string GoalId { get; init; } = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId;
    public bool RunnerScriptExists { get; init; }
    public bool RunnerCmdExists { get; init; }
    public bool SupportsPackagePathParameter { get; init; }
    public bool SupportsDefaultPackagePath { get; init; }
    public bool RejectsOutsideRepository { get; init; }
    public bool RejectsManualInputRoot { get; init; }
    public bool PassesUnityPackageArgument { get; init; }
    public bool ExecuteMethodPresent { get; init; }
    public bool PassMarkerScanPresent { get; init; }
    public bool FailMarkerScanPresent { get; init; }
    public bool WritesRequiredResultJsonFields { get; init; }
    public bool CleanupDelegatesToBoundedScript { get; init; }
    public bool CmdWrapperPreservesDefaultAndExtraParams { get; init; }
    public bool NoBroadGitClean { get; init; }
    public bool NoForbiddenMutationTargets { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record ParameterizedGamePackageProjectionRunnerUnitySourceScan
{
    public string GoalId { get; init; } = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId;
    public bool AdapterReadsCommandLineArgument { get; init; }
    public bool AdapterFallsBackToDefaultSample { get; init; }
    public bool AdapterRejectsOutsideRepository { get; init; }
    public bool AdapterRejectsManualInputRoot { get; init; }
    public bool ControllerRunsParameterizedFullPlaythrough { get; init; }
    public bool BatchmodeEntrypointPresent { get; init; }
    public bool BatchmodeMarkersPresent { get; init; }
    public bool SmokeFieldsPresent { get; init; }
    public bool Passed { get; init; }
}

public sealed record ParameterizedGamePackageProjectionRunnerResultScan
{
    public string GoalId { get; init; } = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId;
    public bool ResultExists { get; init; }
    public string Mode { get; init; } = string.Empty;
    public string PackagePath { get; init; } = string.Empty;
    public string PackagePathRelative { get; init; } = string.Empty;
    public bool PackagePathUnderRepo { get; init; }
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

public sealed record ParameterizedGamePackageProjectionRunnerLogScan
{
    public string GoalId { get; init; } = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId;
    public bool LogExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerAbsent { get; init; } = true;
    public bool MaterialWarningAbsent { get; init; } = true;
    public bool Passed { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> ForbiddenMarkersFound { get; init; } = [];
}

public sealed record ParameterizedGamePackageProjectionRunnerNegativeProof
{
    public string GoalId { get; init; } = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManualInputRejected { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool UnityProjectSettingsMutationRejected { get; init; }
    public bool RuntimeSchemaProviderLuaGeneratorLibraryRejected { get; init; }
    public bool BroadGitCleanRejected { get; init; }
    public bool OnlyAllowedRunnerArtifactsExpected { get; init; }
    public IReadOnlyList<string> RejectedPathSamples { get; init; } = [];
}

public sealed record ParameterizedGamePackageProjectionRunnerFileIndex
{
    public string GoalId { get; init; } = ParameterizedGamePackageProjectionRunnerVocabulary.GoalId;
    public bool PackageRelativePathsOnly { get; init; } = true;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; } = true;
    public IReadOnlyList<ParameterizedGamePackageProjectionRunnerFileIndexEntry> Files { get; init; } = [];
}

public sealed record ParameterizedGamePackageProjectionRunnerFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool Required { get; init; } = true;
    public string Sha256 { get; init; } = string.Empty;
}
