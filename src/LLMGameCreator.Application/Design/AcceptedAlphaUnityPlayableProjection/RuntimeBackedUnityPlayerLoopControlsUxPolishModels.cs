namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary
{
    public const string GoalId =
        "goal_140_runtime_backed_unity_player_loop_controls_ux_polish_and_noise_guard";
    public const string SourceGoal139Id =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.GoalId;
    public const string ScenarioId =
        "goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard";
    public const string DefaultInteractiveControlsModelPath =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory
        + "/"
        + RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ModelFileName;
    public const string DefaultInteractiveControlsResultPath =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory
        + "/"
        + RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ResultFileName;
    public const string DefaultInteractiveControlsScriptPath =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ProceduralOutputDirectory
        + "/"
        + RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.ControlScriptFileName;
    public const string Goal139DocumentationPath =
        RuntimeBackedUnityPlayerLoopInteractiveControlsVocabulary.DocumentationPath;
    public const string DocumentationPath =
        "docs/manual-acceptance/runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard.md";
    public const string ScriptPath =
        ".devflow/scripts/run-runtime-backed-unity-player-loop-controls-ux-polish.ps1";
    public const string CmdPath =
        ".devflow/scripts/run-runtime-backed-unity-player-loop-controls-ux-polish.cmd";
    public const string NormalCommand =
        ".devflow\\scripts\\run-runtime-backed-unity-player-loop-controls-ux-polish.cmd";
    public const string UnityPassMarker =
        "GOAL140_RUNTIME_BACKED_UNITY_PLAYER_LOOP_CONTROLS_UX_PASS";
    public const string UnityFailMarker =
        "GOAL140_RUNTIME_BACKED_UNITY_PLAYER_LOOP_CONTROLS_UX_FAIL";

    public const string Goal139AcceptanceFileName =
        "goal139-human-acceptance-record.json";
    public const string DashboardFileName =
        "runtime-backed-player-loop-controls-ux-dashboard.json";
    public const string ResultFileName =
        "runtime-backed-player-loop-controls-ux-result.json";
    public const string ModelFileName =
        "runtime-backed-player-loop-controls-ux-model.json";
    public const string ScriptFileName =
        "runtime-backed-player-loop-controls-ux-script.json";
    public const string UnitySmokeFileName =
        "unity-player-loop-controls-ux-smoke.json";
    public const string UnityNoiseClassificationFileName =
        "unity-editor-noise-classification.json";
    public const string ReportJsonFileName =
        "one-click-runtime-backed-player-loop-controls-ux-report.json";
    public const string ReportMarkdownFileName =
        "one-click-runtime-backed-player-loop-controls-ux-report.md";
    public const string NegativeProofFileName =
        "runtime-backed-player-loop-controls-ux-negative-proof.json";
    public const string FileIndexFileName =
        "runtime-backed-player-loop-controls-ux-file-index.json";

    public const string DashboardRelativePath = ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string ReportMarkdownRelativePath = ProceduralOutputDirectory + "/" + ReportMarkdownFileName;
    public const string ModelRelativePath = ProceduralOutputDirectory + "/" + ModelFileName;

    public static IReadOnlyList<string> RequiredControls =>
    [
        "load_model",
        "first",
        "previous",
        "next",
        "last",
        "step_once",
        "play_all_to_end",
        "copy_current_frame_summary",
        "show_runtime_hash",
        "show_hud_lines"
    ];

    public static IReadOnlyList<string> RequiredScriptActions =>
    [
        "load_model",
        "assert_frame_count",
        "assert_human_readable_frame_numbering",
        "first",
        "next",
        "previous",
        "step_once",
        "step_once",
        "play_all_to_end",
        "copy_current_frame_summary",
        "assert_copy_frame_summary_status",
        "first",
        "assert_reset_first_status",
        "assert_runtime_authority_markers"
    ];
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxPolishRequest
{
    public string InteractiveControlsModelPath { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DefaultInteractiveControlsModelPath;
    public string InteractiveControlsResultPath { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DefaultInteractiveControlsResultPath;
    public string InteractiveControlsScriptPath { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DefaultInteractiveControlsScriptPath;
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxPolishInput
{
    public string InteractiveControlsModelPath { get; init; } = string.Empty;
    public string InteractiveControlsResultPath { get; init; } = string.Empty;
    public string InteractiveControlsScriptPath { get; init; } = string.Empty;
    public bool InteractiveControlsModelPathExists { get; init; }
    public bool InteractiveControlsResultPathExists { get; init; }
    public bool InteractiveControlsScriptPathExists { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxPolishGoal139AcceptanceRecord
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.SourceGoal139Id;
    public bool Accepted { get; init; } = true;
    public bool AcceptedByHuman { get; init; } = true;
    public bool AcceptedByCodex { get; init; }
    public string SelectedCandidate { get; init; } = string.Empty;
    public int Frames { get; init; }
    public string InteractiveControlsSmoke { get; init; } = "GREEN";
    public bool RequiredControlsPresent { get; init; }
    public bool ControlsWork { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool AutoStepAutoPlayAllUxAcceptedWithFollowUpDebt { get; init; } = true;
    public bool RawManualInputNotCommitted { get; init; } = true;
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxFrame
{
    public int FrameIndex { get; init; }
    public int HumanFrameNumber { get; init; }
    public string CurrentFrameLabel { get; init; } = string.Empty;
    public string FrameIndexLabel { get; init; } = string.Empty;
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

public sealed record RuntimeBackedUnityPlayerLoopControlsUxControlDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Behavior { get; init; } = string.Empty;
    public string LastControlAction { get; init; } = string.Empty;
    public string StatusAfterAction { get; init; } = string.Empty;
    public bool RuntimeBacked { get; init; } = true;
    public bool MutatesGameplay { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxModel
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public int CurrentFrameIndex { get; init; }
    public string CurrentFrameLabel { get; init; } = string.Empty;
    public string FrameIndexLabel { get; init; } = string.Empty;
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopControlsUxFrame> Frames { get; init; } = [];
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopControlsUxControlDefinition> Controls { get; init; } = [];
    public IReadOnlyList<string> RequiredControls { get; init; } = [];
    public IReadOnlyList<string> MissingControls { get; init; } = [];
    public bool RequiredControlsPresent { get; init; }
    public bool HumanReadableFrameNumbering { get; init; }
    public bool StepOnceSemanticsClear { get; init; }
    public bool PlayAllToEndSemanticsClear { get; init; }
    public bool CopyFrameSummaryStatusPresent { get; init; }
    public bool ResetFirstStatusPresent { get; init; }
    public bool LastControlActionPresent { get; init; }
    public bool ControlsUxPolished { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public string GameplayTruth { get; init; } = "Runtime";
    public string UnityMode { get; init; } = "PlayerAdapter/HUD controls only";
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxScriptStep
{
    public int StepIndex { get; init; }
    public string Action { get; init; } = string.Empty;
    public int? ExpectedFrameIndex { get; init; }
    public string ExpectedLastControlAction { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = string.Empty;
    public string Assertion { get; init; } = string.Empty;
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxScript
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int ExpectedFrameCount { get; init; } = 13;
    public IReadOnlyList<string> RequiredControls { get; init; } = [];
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopControlsUxScriptStep> Steps { get; init; } = [];
    public bool Deterministic { get; init; } = true;
    public bool HumanReadableFrameNumberingRequired { get; init; } = true;
    public bool StepOnceSemanticsClear { get; init; } = true;
    public bool PlayAllToEndSemanticsClear { get; init; } = true;
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxSessionStep
{
    public int StepIndex { get; init; }
    public string Action { get; init; } = string.Empty;
    public int FrameIndexBefore { get; init; }
    public int FrameIndexAfter { get; init; }
    public string CurrentFrameLabelAfter { get; init; } = string.Empty;
    public string FrameIndexLabelAfter { get; init; } = string.Empty;
    public string LastControlAction { get; init; } = string.Empty;
    public string StatusAfterAction { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxSession
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public int FinalFrameIndex { get; init; }
    public bool ControlScriptPassed { get; init; }
    public bool HumanReadableFrameNumberingPassed { get; init; }
    public bool StepOnceSemanticsPassed { get; init; }
    public bool PlayAllToEndSemanticsPassed { get; init; }
    public bool CopyFrameSummaryStatusPassed { get; init; }
    public bool ResetFirstStatusPassed { get; init; }
    public bool RuntimeAuthorityMarkersPresent { get; init; }
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopControlsUxSessionStep> Steps { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public bool UnityAvailable { get; init; }
    public bool ModelPathExists { get; init; }
    public bool FrameCountPassed { get; init; }
    public bool RequiredControlsPresent { get; init; }
    public bool HumanReadableFrameNumberingPresent { get; init; }
    public bool StepOnceSemanticsClear { get; init; }
    public bool PlayAllToEndSemanticsClear { get; init; }
    public bool CopyFrameSummaryStatusPresent { get; init; }
    public bool RuntimeAuthorityMarkersPresent { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Passed { get; init; }
    public string UnityPath { get; init; } = string.Empty;
    public string UnityLogPath { get; init; } = string.Empty;
    public string ModelPath { get; init; } = string.Empty;
    public string ScriptPath { get; init; } = string.Empty;
    public string Status { get; init; } = "PENDING_UNITY_BATCHMODE";
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public bool KnownUnityEditorBuildProfileNoiseClassified { get; init; }
    public int KnownUnityEditorNoiseCount { get; init; }
    public int BlockingUnityErrorCount { get; init; }
    public int UnclassifiedUnityErrorCount { get; init; }
    public bool FixtureKnownUnityEditorBuildProfileNoiseClassified { get; init; }
    public string SourceLogPath { get; init; } = string.Empty;
    public IReadOnlyList<string> KnownMarkers { get; init; } = [];
    public IReadOnlyList<string> BlockingMarkers { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
    public bool Passed { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxNegativeProof
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public bool ManualInputRejected { get; init; }
    public bool RawManualInputNotCommitted { get; init; }
    public bool OutputRootUnderGoal140 { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool RuntimeContractsUnchanged { get; init; }
    public bool GamePackageSchemaUnchanged { get; init; }
    public bool GeneratorLibraryProviderLuaUnchanged { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged { get; init; }
    public bool ControlsConsumeRuntimeBackedArtifacts { get; init; }
    public bool ControlsDoNotExecuteGameplay { get; init; }
    public bool KnownUnityEditorNoiseIsBounded { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Passed { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxResult
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public RuntimeBackedUnityPlayerLoopControlsUxPolishInput Inputs { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopControlsUxPolishGoal139AcceptanceRecord Goal139Acceptance { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopControlsUxModel Model { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopControlsUxScript Script { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopControlsUxSession Session { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopControlsUxUnitySmoke UnitySmoke { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopControlsUxUnityNoiseClassification UnityNoiseClassification { get; init; } = new();
    public bool RequiredControlsPresent { get; init; }
    public bool ControlsUxPolished { get; init; }
    public bool UnityControlsUxSmokePassed { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxReport
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool AcceptedGoal139 { get; init; }
    public string SelectedCandidate { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public bool HumanReadableFrameNumbering { get; init; }
    public bool StepOnceSemanticsClear { get; init; }
    public bool PlayAllToEndSemanticsClear { get; init; }
    public bool CopyFrameSummaryStatusPresent { get; init; }
    public bool RequiredControlsPresent { get; init; }
    public bool ControlsUxPolished { get; init; }
    public bool UnityControlsUxSmokePassed { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool KnownUnityEditorNoiseClassified { get; init; }
    public int KnownUnityEditorNoiseCount { get; init; }
    public int BlockingUnityErrorCount { get; init; }
    public int UnclassifiedUnityErrorCount { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public string NormalCommand { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ReportMarkdownRelativePath;
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxFileIndex
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopControlsUxFileIndexEntry> Files { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxDashboard
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool AcceptedGoal139 { get; init; }
    public string SelectedCandidate { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public bool HumanReadableFrameNumbering { get; init; }
    public bool StepOnceSemanticsClear { get; init; }
    public bool PlayAllToEndSemanticsClear { get; init; }
    public bool CopyFrameSummaryStatusPresent { get; init; }
    public bool RequiredControlsPresent { get; init; }
    public bool ControlsUxPolished { get; init; }
    public bool UnityControlsUxSmokePassed { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool KnownUnityEditorNoiseClassified { get; init; }
    public int KnownUnityEditorNoiseCount { get; init; }
    public int BlockingUnityErrorCount { get; init; }
    public int UnclassifiedUnityErrorCount { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public string NormalCommand { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ReportMarkdownRelativePath;
    public IReadOnlyList<string> MissingControls { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopControlsUxWriteResult
{
    public RuntimeBackedUnityPlayerLoopControlsUxDashboard Dashboard { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string Goal139DocumentationPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
