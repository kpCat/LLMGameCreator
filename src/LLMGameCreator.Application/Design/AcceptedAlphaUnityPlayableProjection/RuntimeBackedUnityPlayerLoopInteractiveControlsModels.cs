namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary
{
    public const string GoalId =
        "goal_139_runtime_backed_unity_player_loop_interactive_controls_harness";
    public const string ScenarioId =
        "goal-139-runtime-backed-unity-player-loop-interactive-controls-harness";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness";
    public const string DefaultStepperModelPath =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory
        + "/"
        + RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelFileName;
    public const string DefaultStepperResultPath =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.ProceduralOutputDirectory
        + "/"
        + RuntimeBackedUnityPlayerLoopStepperVocabulary.ResultFileName;
    public const string DefaultPlaybackFramesPath =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FramesFileName;
    public const string DefaultCommandLoopSnapshotsPath =
        CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimePlayerCommandLoopVocabulary.SnapshotsFileName;
    public const string DefaultPlayerAdapterContractPath =
        CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal135PlayerAdapterContractPath;
    public const string Goal138AcceptanceDocumentationPath =
        "docs/manual-acceptance/runtime-backed-unity-player-loop-stepper-hud-harness.md";
    public const string DocumentationPath =
        "docs/manual-acceptance/runtime-backed-unity-player-loop-interactive-controls-harness.md";
    public const string ScriptPath =
        ".devflow/scripts/run-runtime-backed-unity-player-loop-interactive-controls.ps1";
    public const string CmdPath =
        ".devflow/scripts/run-runtime-backed-unity-player-loop-interactive-controls.cmd";
    public const string NormalCommand =
        ".devflow\\scripts\\run-runtime-backed-unity-player-loop-interactive-controls.cmd";
    public const string UnityPassMarker =
        "GOAL139_RUNTIME_BACKED_UNITY_PLAYER_LOOP_INTERACTIVE_CONTROLS_PASS";
    public const string UnityFailMarker =
        "GOAL139_RUNTIME_BACKED_UNITY_PLAYER_LOOP_INTERACTIVE_CONTROLS_FAIL";

    public const string Goal138AcceptanceFileName =
        "goal138-human-acceptance-record.json";
    public const string ModelFileName =
        "runtime-backed-player-loop-interactive-controls-model.json";
    public const string ControlScriptFileName =
        "runtime-backed-player-loop-interactive-controls-script.json";
    public const string SessionFileName =
        "runtime-backed-player-loop-interactive-controls-session.json";
    public const string ResultFileName =
        "runtime-backed-player-loop-interactive-controls-result.json";
    public const string DashboardFileName =
        "runtime-backed-player-loop-interactive-controls-dashboard.json";
    public const string NegativeProofFileName =
        "runtime-backed-player-loop-interactive-controls-negative-proof.json";
    public const string FileIndexFileName =
        "runtime-backed-player-loop-interactive-controls-file-index.json";
    public const string UnitySmokeFileName =
        "unity-player-loop-interactive-controls-smoke.json";
    public const string ReportJsonFileName =
        "one-click-runtime-backed-player-loop-interactive-controls-report.json";
    public const string ReportMarkdownFileName =
        "one-click-runtime-backed-player-loop-interactive-controls-report.md";

    public const string DashboardRelativePath =
        ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string ReportMarkdownRelativePath =
        ProceduralOutputDirectory + "/" + ReportMarkdownFileName;
    public const string ModelRelativePath =
        ProceduralOutputDirectory + "/" + ModelFileName;
    public const string ControlScriptRelativePath =
        ProceduralOutputDirectory + "/" + ControlScriptFileName;

    public static IReadOnlyList<string> RequiredControls =>
    [
        "load_model",
        "first",
        "previous",
        "next",
        "last",
        "autoplay_tick",
        "autoplay_all",
        "copy_current_frame_summary",
        "show_runtime_hash",
        "show_hud_lines"
    ];

    public static IReadOnlyList<string> RequiredScriptActions =>
    [
        "load_model",
        "assert_frame_count",
        "first",
        "next",
        "next",
        "previous",
        "last",
        "first",
        "autoplay_tick",
        "autoplay_tick",
        "autoplay_all",
        "copy_current_frame_summary",
        "assert_final_frame_reachable",
        "assert_runtime_authority_markers"
    ];
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsRequest
{
    public string StepperModelPath { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DefaultStepperModelPath;
    public string StepperResultPath { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DefaultStepperResultPath;
    public string PlaybackFramesPath { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DefaultPlaybackFramesPath;
    public string CommandLoopSnapshotsPath { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DefaultCommandLoopSnapshotsPath;
    public string PlayerAdapterContractPath { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DefaultPlayerAdapterContractPath;
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsInput
{
    public string StepperModelPath { get; init; } = string.Empty;
    public string StepperResultPath { get; init; } = string.Empty;
    public string PlaybackFramesPath { get; init; } = string.Empty;
    public string CommandLoopSnapshotsPath { get; init; } = string.Empty;
    public string PlayerAdapterContractPath { get; init; } = string.Empty;
    public bool StepperModelPathExists { get; init; }
    public bool StepperResultPathExists { get; init; }
    public bool PlaybackFramesPathExists { get; init; }
    public bool CommandLoopSnapshotsPathExists { get; init; }
    public bool PlayerAdapterContractPathExists { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsFrame
{
    public int FrameIndex { get; init; }
    public string FrameCategory { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string PlayerFacingSummary { get; init; } = string.Empty;
    public string CanonicalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> HudLines { get; init; } = [];
    public string SourceSnapshotPath { get; init; } = string.Empty;
    public string SourceFramePath { get; init; } = string.Empty;
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Behavior { get; init; } = string.Empty;
    public bool RuntimeBacked { get; init; } = true;
    public bool MutatesGameplay { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsModel
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public int CurrentFrameIndex { get; init; }
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopInteractiveControlsFrame> Frames { get; init; } = [];
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopInteractiveControlDefinition> Controls { get; init; } = [];
    public bool RequiredControlsPresent { get; init; }
    public IReadOnlyList<string> RequiredControls { get; init; } = [];
    public IReadOnlyList<string> MissingControls { get; init; } = [];
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public string GameplayTruth { get; init; } = "Runtime";
    public string UnityMode { get; init; } = "PlayerAdapter/HUD controls only";
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlScriptStep
{
    public int StepIndex { get; init; }
    public string Action { get; init; } = string.Empty;
    public int? ExpectedFrameIndex { get; init; }
    public string Assertion { get; init; } = string.Empty;
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlScript
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int ExpectedFrameCount { get; init; } = 13;
    public IReadOnlyList<string> RequiredControls { get; init; } = [];
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopInteractiveControlScriptStep> Steps { get; init; } = [];
    public bool Deterministic { get; init; } = true;
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlSessionStep
{
    public int StepIndex { get; init; }
    public string Action { get; init; } = string.Empty;
    public int FrameIndexBefore { get; init; }
    public int FrameIndexAfter { get; init; }
    public bool Passed { get; init; }
    public string CopiedFrameSummary { get; init; } = string.Empty;
    public string RuntimeHash { get; init; } = string.Empty;
    public IReadOnlyList<string> HudLines { get; init; } = [];
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlSession
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public int FinalFrameIndex { get; init; }
    public bool ControlScriptPassed { get; init; }
    public bool FinalFrameReachable { get; init; }
    public bool RuntimeAuthorityMarkersPresent { get; init; }
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopInteractiveControlSessionStep> Steps { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsGoal138AcceptanceRecord
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId;
    public bool Accepted { get; init; } = true;
    public bool AcceptedByHuman { get; init; } = true;
    public bool AcceptedByCodex { get; init; }
    public string SelectedCandidate { get; init; } = string.Empty;
    public int StepperFrames { get; init; }
    public string StepperBatchSmoke { get; init; } = "GREEN";
    public bool ProjectionOnly { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool RawManualInputNotCommitted { get; init; } = true;
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsUnitySmoke
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
    public bool UnityAvailable { get; init; }
    public bool InteractiveModelPathExists { get; init; }
    public bool ControlScriptPathExists { get; init; }
    public bool FrameCountPassed { get; init; }
    public bool RequiredControlsPresent { get; init; }
    public bool ControlScriptPassed { get; init; }
    public bool RuntimeAuthorityMarkersPresent { get; init; }
    public bool InteractiveControlsWindowPresent { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Passed { get; init; }
    public string UnityPath { get; init; } = string.Empty;
    public string UnityLogPath { get; init; } = string.Empty;
    public string InteractiveModelPath { get; init; } = string.Empty;
    public string ControlScriptPath { get; init; } = string.Empty;
    public string Status { get; init; } = "PENDING_UNITY_BATCHMODE";
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsNegativeProof
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
    public bool ManualInputRejected { get; init; }
    public bool RawManualInputNotCommitted { get; init; }
    public bool OutputRootUnderGoal139 { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool RuntimeContractsUnchanged { get; init; }
    public bool GamePackageSchemaUnchanged { get; init; }
    public bool GeneratorLibraryProviderLuaUnchanged { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged { get; init; }
    public bool ControlsConsumeRuntimeBackedArtifacts { get; init; }
    public bool ControlsDoNotExecuteGameplay { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Passed { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsResult
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
    public RuntimeBackedUnityPlayerLoopInteractiveControlsInput Inputs { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopInteractiveControlsGoal138AcceptanceRecord Goal138Acceptance { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopInteractiveControlsModel Model { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopInteractiveControlScript ControlScript { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopInteractiveControlSession Session { get; init; } = new();
    public bool SourceStepperResultGreen { get; init; }
    public bool SourcePlaybackFramesPresent { get; init; }
    public bool SourceCommandLoopSnapshotsPresent { get; init; }
    public bool PlayerAdapterContractPresent { get; init; }
    public bool RequiredControlsPresent { get; init; }
    public bool ControlScriptPassed { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsReport
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public bool AcceptedGoal138 { get; init; }
    public bool RequiredControlsPresent { get; init; }
    public bool ControlScriptPassed { get; init; }
    public bool InteractiveControlsWindowPresent { get; init; }
    public bool UnityInteractiveControlsSmokePassed { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool Accepted { get; init; }
    public string NormalCommand { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ReportMarkdownRelativePath;
    public string ModelPath { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelRelativePath;
    public string ControlScriptPath { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ControlScriptRelativePath;
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsFileIndex
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopInteractiveControlsFileIndexEntry> Files { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsDashboard
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool AcceptedGoal138 { get; init; }
    public string CandidateId { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public bool RequiredControlsPresent { get; init; }
    public bool ControlScriptPassed { get; init; }
    public bool InteractiveControlsWindowPresent { get; init; }
    public bool UnityInteractiveControlsSmokePassed { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public string NormalCommand { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ReportMarkdownRelativePath;
    public string ModelPath { get; init; } =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelRelativePath;
    public IReadOnlyList<string> MissingControls { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopInteractiveControlsWriteResult
{
    public RuntimeBackedUnityPlayerLoopInteractiveControlsDashboard Dashboard { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string Goal138DocumentationPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
