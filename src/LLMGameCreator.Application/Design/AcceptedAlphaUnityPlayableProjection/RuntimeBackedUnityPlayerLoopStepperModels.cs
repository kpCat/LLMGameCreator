using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class RuntimeBackedUnityPlayerLoopStepperVocabulary
{
    public const string GoalId =
        "goal_138_runtime_backed_unity_player_loop_stepper_hud_harness";
    public const string ScenarioId =
        "goal-138-runtime-backed-unity-player-loop-stepper-hud-harness";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness";
    public const string DefaultPlaybackFramesPath =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.FramesFileName;
    public const string DefaultPlaybackResultPath =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ResultFileName;
    public const string DefaultCommandLoopSnapshotsPath =
        CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimePlayerCommandLoopVocabulary.SnapshotsFileName;
    public const string DefaultCommandLoopResultPath =
        CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimePlayerCommandLoopVocabulary.ResultFileName;
    public const string DefaultPlayerAdapterContractPath =
        CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal135PlayerAdapterContractPath;
    public const string Goal137AcceptanceDocumentationPath =
        "docs/manual-acceptance/canonical-runtime-unity-player-loop-playback-harness.md";
    public const string DocumentationPath =
        "docs/manual-acceptance/runtime-backed-unity-player-loop-stepper-hud-harness.md";
    public const string ScriptPath =
        ".devflow/scripts/run-runtime-backed-unity-player-loop-stepper.ps1";
    public const string CmdPath =
        ".devflow/scripts/run-runtime-backed-unity-player-loop-stepper.cmd";
    public const string NormalCommand =
        ".devflow\\scripts\\run-runtime-backed-unity-player-loop-stepper.cmd";
    public const string UnityPassMarker =
        "GOAL138_RUNTIME_BACKED_UNITY_PLAYER_LOOP_STEPPER_PASS";
    public const string UnityFailMarker =
        "GOAL138_RUNTIME_BACKED_UNITY_PLAYER_LOOP_STEPPER_FAIL";

    public const string Goal137AcceptanceFileName =
        "goal137-human-acceptance-record.json";
    public const string ModelFileName =
        "runtime-backed-player-loop-stepper-model.json";
    public const string DashboardFileName =
        "runtime-backed-player-loop-stepper-dashboard.json";
    public const string ResultFileName =
        "runtime-backed-player-loop-stepper-result.json";
    public const string FrameIndexFileName =
        "runtime-backed-player-loop-stepper-frame-index.json";
    public const string UnitySmokeFileName =
        "unity-player-loop-stepper-smoke.json";
    public const string NegativeProofFileName =
        "runtime-backed-player-loop-stepper-negative-proof.json";
    public const string FileIndexFileName =
        "runtime-backed-player-loop-stepper-file-index.json";
    public const string ReportJsonFileName =
        "one-click-runtime-backed-player-loop-stepper-report.json";
    public const string ReportMarkdownFileName =
        "one-click-runtime-backed-player-loop-stepper-report.md";

    public const string DashboardRelativePath =
        ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string ReportMarkdownRelativePath =
        ProceduralOutputDirectory + "/" + ReportMarkdownFileName;
    public const string ModelRelativePath =
        ProceduralOutputDirectory + "/" + ModelFileName;

    public static IReadOnlyList<string> RequiredFrameCategories =>
    [
        "load_package",
        "show_start_state",
        "show_map_position",
        "show_interaction_result",
        "show_dialogue",
        "show_quest_state",
        "show_inventory_state",
        "show_crafting_result",
        "show_harvest_result",
        "show_transaction_result",
        "show_encounter_state",
        "show_combat_round",
        "show_final_state"
    ];
}

public sealed record RuntimeBackedUnityPlayerLoopStepperRequest
{
    public string PlaybackFramesPath { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.DefaultPlaybackFramesPath;
    public string PlaybackResultPath { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.DefaultPlaybackResultPath;
    public string CommandLoopSnapshotsPath { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.DefaultCommandLoopSnapshotsPath;
    public string CommandLoopResultPath { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.DefaultCommandLoopResultPath;
    public string PlayerAdapterContractPath { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.DefaultPlayerAdapterContractPath;
}

public sealed record RuntimeBackedUnityPlayerLoopStepperInput
{
    public string PlaybackFramesPath { get; init; } = string.Empty;
    public string PlaybackResultPath { get; init; } = string.Empty;
    public string CommandLoopSnapshotsPath { get; init; } = string.Empty;
    public string CommandLoopResultPath { get; init; } = string.Empty;
    public string PlayerAdapterContractPath { get; init; } = string.Empty;
    public bool PlaybackFramesPathExists { get; init; }
    public bool PlaybackResultPathExists { get; init; }
    public bool CommandLoopSnapshotsPathExists { get; init; }
    public bool CommandLoopResultPathExists { get; init; }
    public bool PlayerAdapterContractPathExists { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopStepperFrame
{
    public int FrameIndex { get; init; }
    public string FrameCategory { get; init; } = string.Empty;
    public string RuntimeCommandId { get; init; } = string.Empty;
    public string CommandStepId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string PlayerFacingSummary { get; init; } = string.Empty;
    public string CanonicalStateHash { get; init; } = string.Empty;
    public int RuntimeEventCount { get; init; }
    public string MapPositionSummary { get; init; } = string.Empty;
    public string InteractionSummary { get; init; } = string.Empty;
    public string DialogueSummary { get; init; } = string.Empty;
    public string QuestSummary { get; init; } = string.Empty;
    public string InventorySummary { get; init; } = string.Empty;
    public string CombatSummary { get; init; } = string.Empty;
    public IReadOnlyList<string> HudLines { get; init; } = [];
    public string SourceSnapshotPath { get; init; } = string.Empty;
    public string SourceFramePath { get; init; } = string.Empty;
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopStepperModel
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public int CurrentFrameIndex { get; init; }
    public bool RequiredFrameCategoriesPresent { get; init; }
    public IReadOnlyList<string> RequiredFrameCategories { get; init; } = [];
    public IReadOnlyList<string> MissingFrameCategories { get; init; } = [];
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopStepperFrame> Frames { get; init; } = [];
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopStepperFrameIndex
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int CurrentFrameIndex { get; init; }
    public int FrameCount { get; init; }
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopStepperFrameIndexRow> Frames { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopStepperFrameIndexRow
{
    public int FrameIndex { get; init; }
    public string FrameCategory { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string CanonicalStateHash { get; init; } = string.Empty;
}

public sealed record RuntimeBackedUnityPlayerLoopStepperGoal137AcceptanceRecord
{
    public string GoalId { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId;
    public bool Accepted { get; init; } = true;
    public bool AcceptedByHuman { get; init; } = true;
    public bool AcceptedByCodex { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public string SelectedCandidate { get; init; } = string.Empty;
    public int PlaybackFrames { get; init; }
    public string UnityPlaybackSmoke { get; init; } = "GREEN";
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool RawManualInputNotCommitted { get; init; } = true;
}

public sealed record RuntimeBackedUnityPlayerLoopStepperUnitySmoke
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId;
    public bool UnityAvailable { get; init; }
    public bool ModelPathExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public bool FrameCountPassed { get; init; }
    public bool RequiredFrameCategoriesPresent { get; init; }
    public bool RuntimeAuthorityMarkersPresent { get; init; }
    public bool StepperWindowPresent { get; init; }
    public bool StepperBatchSmokePassed { get; init; }
    public bool Passed { get; init; }
    public string UnityPath { get; init; } = string.Empty;
    public string UnityLogPath { get; init; } = string.Empty;
    public string ModelPath { get; init; } = string.Empty;
    public string Status { get; init; } = "PENDING_UNITY_BATCHMODE";
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopStepperNegativeProof
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId;
    public bool ManualInputRejected { get; init; }
    public bool RawManualInputNotCommitted { get; init; }
    public bool OutputRootUnderGoal138 { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool GamePackageSchemaUnchanged { get; init; }
    public bool RuntimeContractsUnchanged { get; init; }
    public bool GeneratorLibraryProviderLuaUnchanged { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged { get; init; }
    public bool StepperConsumesRuntimeBackedArtifacts { get; init; }
    public bool StepperDoesNotExecuteGameplay { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Passed { get; init; }
}

public sealed record RuntimeBackedUnityPlayerLoopStepperResult
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId;
    public RuntimeBackedUnityPlayerLoopStepperInput Inputs { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopStepperGoal137AcceptanceRecord Goal137Acceptance { get; init; } = new();
    public RuntimeBackedUnityPlayerLoopStepperModel Model { get; init; } = new();
    public bool SourcePlaybackResultGreen { get; init; }
    public bool SourceCommandLoopResultGreen { get; init; }
    public bool PlayerAdapterContractPresent { get; init; }
    public bool PlayerAdapterRequiredCategoriesMatch { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopStepperReport
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public bool RequiredFrameCategoriesPresent { get; init; }
    public bool AcceptedGoal137 { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool StepperWindowPresent { get; init; }
    public bool StepperBatchSmokePassed { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool Accepted { get; init; }
    public string NormalCommand { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.ReportMarkdownRelativePath;
    public string ModelPath { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelRelativePath;
}

public sealed record RuntimeBackedUnityPlayerLoopStepperFileIndex
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<RuntimeBackedUnityPlayerLoopStepperFileIndexEntry> Files { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopStepperFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record RuntimeBackedUnityPlayerLoopStepperDashboard
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool AcceptedGoal137 { get; init; }
    public string CandidateId { get; init; } = string.Empty;
    public int FrameCount { get; init; }
    public bool RequiredFrameCategoriesPresent { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool StepperWindowPresent { get; init; }
    public bool StepperBatchSmokePassed { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public string NormalCommand { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.ReportMarkdownRelativePath;
    public string ModelPath { get; init; } =
        RuntimeBackedUnityPlayerLoopStepperVocabulary.ModelRelativePath;
    public IReadOnlyList<string> MissingFrameCategories { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedUnityPlayerLoopStepperWriteResult
{
    public RuntimeBackedUnityPlayerLoopStepperDashboard Dashboard { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string Goal137DocumentationPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
