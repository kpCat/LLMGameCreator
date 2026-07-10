using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Design.AcceptedAlphaUnityPlayableProjection;

public static class RuntimeBackedPlayerCommandRoundtripVocabulary
{
    public const string GoalId =
        "goal_141_runtime_backed_unity_player_command_roundtrip_bridge";
    public const string ScenarioId =
        "goal-141-runtime-backed-unity-player-command-roundtrip-bridge";
    public const string ProceduralOutputDirectory =
        ".llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge";
    public const string ExportPackageDirectory =
        ".llmgc/exports/goal-141-runtime-backed-unity-player-command-roundtrip-bridge";
    public const string DefaultSelectedCandidatePackagePath =
        CanonicalRuntimePlayerCommandLoopVocabulary.DefaultSelectedCandidatePackagePath;
    public const string DefaultSelectedCandidateHandoffPath =
        CanonicalRuntimePlayerCommandLoopVocabulary.DefaultSelectedCandidateHandoffPath;
    public const string DefaultControlsUxModelPath =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ModelRelativePath;
    public const string DefaultControlsUxResultPath =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory
        + "/"
        + RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ResultFileName;
    public const string DefaultControlsUxScriptPath =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ProceduralOutputDirectory
        + "/"
        + RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.ScriptFileName;
    public const string DefaultCommandLoopSnapshotsPath =
        CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimePlayerCommandLoopVocabulary.SnapshotsFileName;
    public const string DefaultCommandLoopResultPath =
        CanonicalRuntimePlayerCommandLoopVocabulary.ProceduralOutputDirectory
        + "/"
        + CanonicalRuntimePlayerCommandLoopVocabulary.ResultFileName;
    public const string Goal140DocumentationPath =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.DocumentationPath;
    public const string DocumentationPath =
        "docs/manual-acceptance/runtime-backed-unity-player-command-roundtrip-bridge.md";
    public const string ScriptPath =
        ".devflow/scripts/run-runtime-backed-player-command-roundtrip.ps1";
    public const string CmdPath =
        ".devflow/scripts/run-runtime-backed-player-command-roundtrip.cmd";
    public const string NormalCommand =
        ".devflow\\scripts\\run-runtime-backed-player-command-roundtrip.cmd";
    public const string UnityPassMarker =
        "GOAL141_RUNTIME_BACKED_PLAYER_COMMAND_ROUNDTRIP_PASS";
    public const string UnityFailMarker =
        "GOAL141_RUNTIME_BACKED_PLAYER_COMMAND_ROUNDTRIP_FAIL";

    public const string Goal140AcceptanceFileName =
        "goal140-human-acceptance-record.json";
    public const string RequestFileName =
        "runtime-backed-player-command-roundtrip-request.json";
    public const string ResultFileName =
        "runtime-backed-player-command-roundtrip-result.json";
    public const string SessionFileName =
        "runtime-backed-player-command-roundtrip-session.json";
    public const string SnapshotsFileName =
        "runtime-backed-player-command-roundtrip-snapshots.json";
    public const string ModelFileName =
        "runtime-backed-player-command-roundtrip-model.json";
    public const string DashboardFileName =
        "runtime-backed-player-command-roundtrip-dashboard.json";
    public const string NegativeProofFileName =
        "runtime-backed-player-command-roundtrip-negative-proof.json";
    public const string FileIndexFileName =
        "runtime-backed-player-command-roundtrip-file-index.json";
    public const string UnitySmokeFileName =
        "unity-player-command-roundtrip-smoke.json";
    public const string ReportJsonFileName =
        "one-click-runtime-backed-player-command-roundtrip-report.json";
    public const string ReportMarkdownFileName =
        "one-click-runtime-backed-player-command-roundtrip-report.md";

    public const string DashboardRelativePath = ProceduralOutputDirectory + "/" + DashboardFileName;
    public const string ModelRelativePath = ProceduralOutputDirectory + "/" + ModelFileName;
    public const string ResultRelativePath = ProceduralOutputDirectory + "/" + ResultFileName;
    public const string ReportMarkdownRelativePath =
        ProceduralOutputDirectory + "/" + ReportMarkdownFileName;

    public static IReadOnlyList<string> RequiredControlIntents =>
    [
        "load_model",
        "reset_first",
        "step_once",
        "next_frame",
        "play_all_to_end",
        "copy_frame_summary"
    ];

    public static IReadOnlyList<string> RequiredRuntimeCommandCoverage =>
    [
        "load_package_or_session",
        "show_or_select_start_state",
        "advance_to_interaction",
        "advance_to_dialogue_or_quest",
        "advance_to_inventory_or_crafting",
        "advance_to_combat_or_final_state"
    ];
}

public sealed record RuntimeBackedPlayerCommandRoundtripGoal140AcceptanceRecord
{
    public string GoalId { get; init; } =
        RuntimeBackedUnityPlayerLoopControlsUxPolishVocabulary.GoalId;
    public bool Accepted { get; init; } = true;
    public bool AcceptedByHuman { get; init; } = true;
    public bool AcceptedByCodex { get; init; }
    public bool RawManualInputNotCommitted { get; init; } = true;
    public string SelectedCandidate { get; init; } = string.Empty;
    public int Frames { get; init; }
    public bool HumanReadableFrameNumbering { get; init; }
    public bool StepOnceSemanticsClear { get; init; }
    public bool PlayAllToEndSemanticsClear { get; init; }
    public bool CopyFrameSummaryStatusPresent { get; init; }
    public bool KnownUnityEditorNoiseClassified { get; init; }
    public int BlockingUnityErrorCount { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool UnityGameplayTruth { get; init; }
}

public sealed record RuntimeBackedPlayerCommandRoundtripRequestArtifact
{
    public string GoalId { get; init; } = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId;
    public RuntimeBackedPlayerCommandRoundtripInput Inputs { get; init; } = new();
    public bool ControlRequestBridgePresent { get; init; }
    public IReadOnlyList<string> RequiredControlIntents { get; init; } = [];
    public IReadOnlyList<string> RequiredRuntimeCommandCoverage { get; init; } = [];
    public IReadOnlyList<RuntimeBackedPlayerCommandRoundtripControlRequest> Requests { get; init; } = [];
}

public sealed record RuntimeBackedPlayerCommandRoundtripModel
{
    public string GoalId { get; init; } = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId;
    public string CandidateId { get; init; } = string.Empty;
    public int RoundtripRequestCount { get; init; }
    public int RuntimeExecutedRequestCount { get; init; }
    public int RoundtripSnapshotCount { get; init; }
    public RuntimeBackedPlayerCommandRoundtripControlRequest CurrentRequest { get; init; } = new();
    public RuntimeBackedPlayerCommandRoundtripSnapshot CurrentResponseSnapshot { get; init; } = new();
    public string Status { get; init; } = "BLOCKED";
    public IReadOnlyList<RuntimeBackedPlayerCommandRoundtripControlRequest> Requests { get; init; } = [];
    public IReadOnlyList<RuntimeBackedPlayerCommandRoundtripResponse> Responses { get; init; } = [];
    public bool StateHashChainPresent { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool ControlRequestBridgePresent { get; init; }
    public bool UnityConsumesRoundtripResult { get; init; } = true;
    public string GameplayTruth { get; init; } = "Runtime";
    public string UnityMode { get; init; } = "PlayerAdapter command request/response only";
}

public sealed record RuntimeBackedPlayerCommandRoundtripUnitySmoke
{
    public string GoalId { get; init; } = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId;
    public bool UnityAvailable { get; init; }
    public bool ModelPathExists { get; init; }
    public bool RoundtripRequestCountPassed { get; init; }
    public bool RuntimeSnapshotResponsePresent { get; init; }
    public bool RuntimeAuthorityMarkersPresent { get; init; }
    public bool UnityConsumesRoundtripResult { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool PassMarkerPresent { get; init; }
    public bool FailMarkerPresent { get; init; }
    public bool Passed { get; init; }
    public string UnityPath { get; init; } = string.Empty;
    public string UnityLogPath { get; init; } = string.Empty;
    public string ModelPath { get; init; } = string.Empty;
    public string ResultPath { get; init; } = string.Empty;
    public string Status { get; init; } = "PENDING_UNITY_BATCHMODE";
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedPlayerCommandRoundtripNegativeProof
{
    public string GoalId { get; init; } = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId;
    public bool ManualInputRejected { get; init; }
    public bool RawManualInputNotCommitted { get; init; }
    public bool OutputRootUnderGoal141 { get; init; }
    public bool SamplePackageReadOnly { get; init; }
    public bool GamePackageSchemaUnchanged { get; init; }
    public bool GeneratorLibraryProviderLuaUnchanged { get; init; }
    public bool UnityScenesPrefabsSettingsPackagesStreamingAssetsUnchanged { get; init; }
    public bool RuntimeOwnsRoundtripExecution { get; init; }
    public bool UnityConsumesResultOnly { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool Passed { get; init; }
}

public sealed record RuntimeBackedPlayerCommandRoundtripReport
{
    public string GoalId { get; init; } = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal140Accepted { get; init; }
    public string CandidateId { get; init; } = string.Empty;
    public int RoundtripRequestCount { get; init; }
    public int RuntimeExecutedRequestCount { get; init; }
    public int RoundtripSnapshotCount { get; init; }
    public bool ControlRequestBridgePresent { get; init; }
    public bool StateHashChainPresent { get; init; }
    public bool RuntimeAuthority { get; init; }
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool UnityConsumesRoundtripResult { get; init; }
    public bool UnitySmokePassed { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public string NormalCommand { get; init; } =
        RuntimeBackedPlayerCommandRoundtripVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        RuntimeBackedPlayerCommandRoundtripVocabulary.ReportMarkdownRelativePath;
}

public sealed record RuntimeBackedPlayerCommandRoundtripDashboard
{
    public string GoalId { get; init; } = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId;
    public string Status { get; init; } = "BLOCKED";
    public bool Accepted { get; init; }
    public bool Goal140Accepted { get; init; }
    public string CandidateId { get; init; } = string.Empty;
    public int RoundtripRequestCount { get; init; }
    public int RuntimeExecutedRequestCount { get; init; }
    public int RoundtripSnapshotCount { get; init; }
    public bool ControlRequestBridgePresent { get; init; }
    public bool StateHashChainPresent { get; init; }
    public bool RuntimeAuthority { get; init; } = true;
    public bool ProjectionOnly { get; init; }
    public bool UnityGameplayTruth { get; init; }
    public bool UnityConsumesRoundtripResult { get; init; }
    public bool UnitySmokePassed { get; init; }
    public bool NoUnclassifiedErrorDiagnostics { get; init; }
    public bool ManualUnityOptional { get; init; } = true;
    public string NormalCommand { get; init; } =
        RuntimeBackedPlayerCommandRoundtripVocabulary.NormalCommand;
    public string ReportPath { get; init; } =
        RuntimeBackedPlayerCommandRoundtripVocabulary.ReportMarkdownRelativePath;
    public IReadOnlyList<string> MissingControlIntents { get; init; } = [];
    public IReadOnlyList<string> MissingRuntimeCommandCoverage { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record RuntimeBackedPlayerCommandRoundtripFileIndexEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record RuntimeBackedPlayerCommandRoundtripFileIndex
{
    public string GoalId { get; init; } = RuntimeBackedPlayerCommandRoundtripVocabulary.GoalId;
    public string RootPath { get; init; } = string.Empty;
    public int IndexedFileCount { get; init; }
    public bool ManualInputExcluded { get; init; }
    public IReadOnlyList<RuntimeBackedPlayerCommandRoundtripFileIndexEntry> Files { get; init; } = [];
}

public sealed record RuntimeBackedPlayerCommandRoundtripWriteResult
{
    public RuntimeBackedPlayerCommandRoundtripDashboard Dashboard { get; init; } = new();
    public string ProceduralOutputDirectoryPath { get; init; } = string.Empty;
    public string ExportPackageDirectoryPath { get; init; } = string.Empty;
    public string Goal140DocumentationPath { get; init; } = string.Empty;
    public string DocumentationPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}
