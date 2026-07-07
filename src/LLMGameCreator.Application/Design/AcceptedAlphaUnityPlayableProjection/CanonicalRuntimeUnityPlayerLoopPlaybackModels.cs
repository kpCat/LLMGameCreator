using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary
{
    public const string GoalId =
        "goal_137_canonical_runtime_unity_player_loop_playback_harness";
    public const string ScenarioId =
        "goal-137-canonical-runtime-unity-player-loop-playback-harness";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-137-canonical-runtime-unity-player-loop-playback-harness";
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
    public const string DefaultStateSummaryPath =
        CanonicalRuntimePlayerCommandLoopVocabulary.DefaultGoal134StateSummaryPath;
    public const string DocumentationPath =
        "docs/manual-acceptance/canonical-runtime-unity-player-loop-playback-harness.md";
    public const string ScriptPath =
        ".devflow/scripts/run-canonical-runtime-unity-player-loop-playback.ps1";
    public const string CmdPath =
        ".devflow/scripts/run-canonical-runtime-unity-player-loop-playback.cmd";
    public const string NormalCommand =
        ".devflow\\scripts\\run-canonical-runtime-unity-player-loop-playback.cmd";
    public const string UnityPassMarker =
        "GOAL137_CANONICAL_RUNTIME_UNITY_PLAYER_LOOP_PLAYBACK_PASS";
    public const string UnityFailMarker =
        "GOAL137_CANONICAL_RUNTIME_UNITY_PLAYER_LOOP_PLAYBACK_FAIL";

    public const string DashboardFileName =
        "canonical-runtime-unity-player-loop-playback-dashboard.json";
    public const string ResultFileName =
        "canonical-runtime-unity-player-loop-playback-result.json";
    public const string PlanFileName =
        "canonical-runtime-unity-player-loop-playback-plan.json";
    public const string FramesFileName =
        "canonical-runtime-unity-player-loop-playback-frames.json";
    public const string MatrixResultFileName =
        "canonical-runtime-unity-player-loop-playback-matrix-result.json";
    public const string UnitySmokeFileName =
        "unity-player-loop-playback-smoke.json";
    public const string NegativeProofFileName =
        "canonical-runtime-unity-player-loop-playback-negative-proof.json";
    public const string FileIndexFileName =
        "canonical-runtime-unity-player-loop-playback-file-index.json";
    public const string ReportJsonFileName =
        "one-click-unity-player-loop-playback-report.json";
    public const string ReportMarkdownFileName =
        "one-click-unity-player-loop-playback-report.md";

    public const string DashboardRelativePath =
        ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string ReportMarkdownRelativePath =
        ProceduralOutputDirectory + "/" + ReportMarkdownFileName;
    public const string MatrixResultRelativePath =
        ProceduralOutputDirectory + "/" + MatrixResultFileName;

    public static IReadOnlyList<string> RequiredFrameCategories =>
    [
        "hud",
        "player_position",
        "interaction",
        "dialogue",
        "quest",
        "inventory",
        "crafting",
        "harvest",
        "transaction",
        "encounter",
        "combat",
        "final_state"
    ];
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackRequest
{
    public string CommandLoopSnapshotsPath { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DefaultCommandLoopSnapshotsPath;
    public string CommandLoopResultPath { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DefaultCommandLoopResultPath;
    public string PlayerAdapterContractPath { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DefaultPlayerAdapterContractPath;
    public string StateSummaryPath { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.DefaultStateSummaryPath;
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackInput
{
    public string CommandLoopSnapshotsPath { get; init; } = string.Empty;
    public string CommandLoopResultPath { get; init; } = string.Empty;
    public string PlayerAdapterContractPath { get; init; } = string.Empty;
    public string StateSummaryPath { get; init; } = string.Empty;
    public bool CommandLoopSnapshotsPathExists { get; init; }
    public bool CommandLoopResultPathExists { get; init; }
    public bool PlayerAdapterContractPathExists { get; init; }
    public bool StateSummaryPathExists { get; init; }
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackFrame
{
    public int FrameIndex { get; init; }
    public string FrameId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int SourceSnapshotStepIndex { get; init; }
    public string SourceSnapshotStepId { get; init; } = string.Empty;
    public string SourceSnapshotCategory { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string PlayerPositionSummary { get; init; } = string.Empty;
    public string HudSummary { get; init; } = string.Empty;
    public string InteractionSummary { get; init; } = string.Empty;
    public string DialogueSummary { get; init; } = string.Empty;
    public string QuestSummary { get; init; } = string.Empty;
    public string InventorySummary { get; init; } = string.Empty;
    public string CombatSummary { get; init; } = string.Empty;
    public int RuntimeEventCount { get; init; }
    public IReadOnlyList<string> RuntimeEventMessages { get; init; } = [];
    public string StateHashBefore { get; init; } = string.Empty;
    public string StateHashAfter { get; init; } = string.Empty;
    public bool RuntimeSnapshotSource { get; init; } = true;
    public bool CanonicalRuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackPlan
{
    public string GoalId { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public bool CanonicalRuntimeSource { get; init; } = true;
    public bool RuntimeSnapshotSource { get; init; } = true;
    public bool UnityConsumesRuntimeSnapshots { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public IReadOnlyList<string> RequiredFrameCategories { get; init; } = [];
    public bool RequiredFrameCategoriesPresent { get; init; }
    public int PlaybackFrameCount { get; init; }
    public IReadOnlyList<CanonicalRuntimeUnityPlayerLoopPlaybackFrame> Frames { get; init; } = [];
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackResult
{
    public string GoalId { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public CanonicalRuntimeUnityPlayerLoopPlaybackInput Inputs { get; init; } = new();
    public bool ProjectionOnly { get; init; }
    public bool CanonicalRuntimeSource { get; init; } = true;
    public bool RuntimeSnapshotSource { get; init; } = true;
    public bool UnityConsumesRuntimeSnapshots { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public int PlaybackFrameCount { get; init; }
    public bool PlayerPositionFramesPresent { get; init; }
    public bool HudFramesPresent { get; init; }
    public bool RequiredFrameCategoriesPresent { get; init; }
    public IReadOnlyList<string> RequiredFrameCategories { get; init; } = [];
    public IReadOnlyList<string> MissingFrameCategories { get; init; } = [];
    public IReadOnlyList<CanonicalRuntimeUnityPlayerLoopPlaybackFrame> Frames { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackUnitySmoke
{
    public string GoalId { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId;
    public bool UnityAvailable { get; init; }
    public bool FramesPathExists { get; init; }
    public bool ResultPathExists { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public bool FrameCountPassed { get; init; }
    public bool RequiredFrameCategoriesPresent { get; init; }
    public bool RuntimeAuthorityMarkersPresent { get; init; }
    public bool UnityPlayerLoopPlaybackPassed { get; init; }
    public bool Passed { get; init; }
    public string UnityPath { get; init; } = string.Empty;
    public string UnityLogPath { get; init; } = string.Empty;
    public string FramesPath { get; init; } = string.Empty;
    public string ResultPath { get; init; } = string.Empty;
    public string Status { get; init; } = "PENDING_UNITY_BATCHMODE";
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackMatrixResult
{
    public string GoalId { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<CanonicalRuntimeUnityPlayerLoopPlaybackMatrixRow> Rows { get; init; } = [];
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackMatrixRow
{
    public string CandidateId { get; init; } = string.Empty;
    public int PlaybackFrameCount { get; init; }
    public bool RequiredFrameCategoriesPresent { get; init; }
    public bool UnityPlayerLoopPlaybackPassed { get; init; }
    public bool RuntimeSnapshotSource { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public bool Passed { get; init; }
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackNegativeProof
{
    public string GoalId { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId;
    public bool ManualInputRejected { get; init; }
    public bool OutputRootUnderGoal137 { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool GamePackageSchemaUnchanged { get; init; }
    public bool RuntimeContractsUnchanged { get; init; }
    public bool GeneratorLibraryProviderLuaUnchanged { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged { get; init; }
    public bool PlaybackConsumesRuntimeSnapshots { get; init; }
    public bool PlaybackDoesNotRecomputeGameplay { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Passed { get; init; }
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackReport
{
    public string GoalId { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int PlaybackFrameCount { get; init; }
    public bool RequiredFrameCategoriesPresent { get; init; }
    public bool UnityPlayerLoopPlaybackPassed { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool CanonicalRuntimeSource { get; init; }
    public bool RuntimeSnapshotSource { get; init; }
    public bool UnityConsumesRuntimeSnapshots { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool Accepted { get; init; }
    public bool NoUnclassifiedErrorDiagnostics { get; init; }
    public string ReportPath { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ReportMarkdownRelativePath;
    public string MatrixResultPath { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.MatrixResultRelativePath;
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackFileIndex
{
    public string GoalId { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<CanonicalRuntimeUnityPlayerLoopPlaybackFileIndexEntry> Files { get; init; } = [];
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackDashboard
{
    public string GoalId { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public string CandidateId { get; init; } = string.Empty;
    public bool ProjectionOnly { get; init; }
    public bool CanonicalRuntimeSource { get; init; } = true;
    public bool RuntimeSnapshotSource { get; init; } = true;
    public bool UnityConsumesRuntimeSnapshots { get; init; } = true;
    public int PlaybackFrameCount { get; init; }
    public bool RequiredFrameCategoriesPresent { get; init; }
    public bool UnityPlayerLoopPlaybackPassed { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool SelectedCandidateExecutedByRuntime { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public bool Accepted { get; init; }
    public bool NoUnclassifiedErrorDiagnostics { get; init; }
    public string NormalCommand { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.ReportMarkdownRelativePath;
    public string MatrixResultPath { get; init; } =
        CanonicalRuntimeUnityPlayerLoopPlaybackVocabulary.MatrixResultRelativePath;
    public IReadOnlyList<string> MissingFrameCategories { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record CanonicalRuntimeUnityPlayerLoopPlaybackWriteResult
{
    public CanonicalRuntimeUnityPlayerLoopPlaybackDashboard Dashboard { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
