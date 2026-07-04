using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.OfflineGeoworldSessionPersistenceReplay;

public static class OfflineGeoworldSessionPersistenceReplayVocabulary
{
    public const string GoalId = "goal_106_offline_geoworld_session_persistence_replay";
    public const string ProductSmokeRoute = "goal-106-offline-geoworld-session-persistence-replay";
    public const string FinalGate = "offline_geoworld_session_persistence_replay_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-106-offline-geoworld-session-persistence-replay";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal106";
    public const string UnityStreamingAssetsProbeRoot = "LLMGameCreator/OfflineGeoworldGoal106";
    public const string SourceGoal105Root =
        ".llmgc/procedural/goal-105-offline-geoworld-interaction-playable-probe";

    public const string UnitySaveLoadControllerScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSaveLoadController.cs";
    public const string UnityReplayControllerScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionReplayController.cs";
    public const string UnitySnapshotScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldSessionSnapshot.cs";
    public const string UnityEditorWindowScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldSessionReplayWindow.cs";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string ManifestFileName = "offline-geoworld-session-manifest.json";
    public const string InitialStateFileName = "offline-geoworld-session-initial-state.json";
    public const string DeltaLogFileName = "offline-geoworld-session-delta-log.json";
    public const string ReplayScriptFileName = "offline-geoworld-session-replay-script.json";
    public const string AcceptanceChecklistFileName =
        "offline-geoworld-session-acceptance-checklist.json";
    public const string ReadmeFileName = "offline-geoworld-session-readme.json";
    public const string ReportMarkdownFileName = "offline-geoworld-session-report.md";
    public const string UnityScriptInventoryFileName =
        "offline-geoworld-session-unity-script-inventory.json";
    public const string EditorWindowInventoryFileName =
        "offline-geoworld-session-editor-window-inventory.json";
    public const string SimulatedReplayProofFileName =
        "offline-geoworld-session-simulated-save-load-replay-proof.json";
    public const string NegativeProofFileName = "offline-geoworld-session-negative-proof.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-session-workspace-binding-inventory.json";
    public const string SourceLineageFileName = "offline-geoworld-session-source-lineage.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-session-quality-gate-scan.json";

    public static readonly IReadOnlyList<string> RequiredPayloadFileNames =
    [
        ManifestFileName,
        InitialStateFileName,
        DeltaLogFileName,
        ReplayScriptFileName,
        AcceptanceChecklistFileName,
        ReadmeFileName
    ];

    public static readonly IReadOnlyList<string> RequiredEvidenceFileNames =
    [
        ReportMarkdownFileName,
        ManifestFileName,
        InitialStateFileName,
        DeltaLogFileName,
        ReplayScriptFileName,
        AcceptanceChecklistFileName,
        ReadmeFileName,
        UnityScriptInventoryFileName,
        EditorWindowInventoryFileName,
        SimulatedReplayProofFileName,
        NegativeProofFileName,
        WorkspaceBindingInventoryFileName,
        SourceLineageFileName,
        QualityGateScanFileName
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_goal105_payload",
        "missing_delta_log",
        "checkpoint_without_prior_deltas",
        "load_snapshot_hash_mismatch",
        "corrupted_snapshot_accepted",
        "replay_final_hash_mismatch",
        "duplicate_replay_mutates_state_non_deterministically",
        "absolute_path",
        "raw_geodata_leak",
        "network_provider_marker",
        "alpha_runtime_bootstrap_dependency_marker",
        "scene_prefab_settings_mutation_marker",
        "binary_raster_media_marker",
        "external_dependency_new_input_system_marker"
    ];
}

public sealed record OfflineGeoworldSessionDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static OfflineGeoworldSessionDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record OfflineGeoworldSessionManifest
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PayloadFileCount { get; init; }
    public int SourceGoal105TargetCount { get; init; }
    public int SourceGoal105ActionCount { get; init; }
    public int SourceGoal105ActionKindCount { get; init; }
    public int ReplayStepCount { get; init; }
    public int StateDeltaCount { get; init; }
    public int CheckpointAfterEventCount { get; init; }
    public int CheckpointStepIndex { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string CheckpointStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string StreamingAssetsRelativeRoot { get; init; } =
        OfflineGeoworldSessionPersistenceReplayVocabulary.UnityStreamingAssetsProbeRoot;
    public bool MetadataOnly { get; init; } = true;
    public bool NoRawGeodata { get; init; } = true;
    public bool NoAbsolutePaths { get; init; } = true;
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public bool NoProviderOrNetworkMarkers { get; init; } = true;
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsFinalGameplay { get; init; }
    public bool ContainsRealGeodataFetch { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string InitialStateHashFile { get; init; } = string.Empty;
    public string DeltaLogHash { get; init; } = string.Empty;
    public string ReplayScriptHash { get; init; } = string.Empty;
    public string AcceptanceChecklistHash { get; init; } = string.Empty;
    public string ReadmeHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldSessionInitialState
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public string SourceGoalId { get; init; } = "goal_105_offline_geoworld_interaction_playable_probe";
    public bool Accepted { get; init; }
    public bool MetadataOnly { get; init; } = true;
    public bool ImmutableBaseData { get; init; } = true;
    public bool NoRawGeodata { get; init; } = true;
    public int TargetCount { get; init; }
    public int ActionCount { get; init; }
    public int ScriptedEventCount { get; init; }
    public int StateDeltaCount { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string SourceFinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<OfflineGeoworldSessionTargetLineage> Targets { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldSessionActionLineage> Actions { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldSessionEventLineage> SessionEvents { get; init; } = [];
}

public sealed record OfflineGeoworldSessionTargetLineage
{
    public string TargetId { get; init; } = string.Empty;
    public string TargetName { get; init; } = string.Empty;
    public string SourceObjectId { get; init; } = string.Empty;
    public string SourceObjectName { get; init; } = string.Empty;
    public string SourceChunkKey { get; init; } = string.Empty;
    public bool RawGeodataIncluded { get; init; }
}

public sealed record OfflineGeoworldSessionActionLineage
{
    public string ActionId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string ActionKind { get; init; } = string.Empty;
    public string StateDeltaKind { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldSessionEventLineage
{
    public int EventIndex { get; init; }
    public string EventId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string ActionId { get; init; } = string.Empty;
    public string ExpectedStateHashBefore { get; init; } = string.Empty;
    public string ExpectedStateHashAfter { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldSessionDeltaLog
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int DeltaCount { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> StateHashChain { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldSessionDeltaRecord> Deltas { get; init; } = [];
}

public sealed record OfflineGeoworldSessionDeltaRecord
{
    public int DeltaIndex { get; init; }
    public int ReplayStepIndex { get; init; }
    public string EventId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string ActionId { get; init; } = string.Empty;
    public string ActionKind { get; init; } = string.Empty;
    public string DeltaKind { get; init; } = string.Empty;
    public string StateKey { get; init; } = string.Empty;
    public string StateValue { get; init; } = string.Empty;
    public string StateHashBefore { get; init; } = string.Empty;
    public string StateHashAfter { get; init; } = string.Empty;
    public bool MutatesBaseDataDirectly { get; init; }
}

public sealed record OfflineGeoworldSessionReplayScript
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int ReplayStepCount { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public OfflineGeoworldSessionCheckpoint Checkpoint { get; init; } = new();
    public IReadOnlyList<OfflineGeoworldSessionReplayStep> Steps { get; init; } = [];
    public string DuplicateReplayPolicy { get; init; } = "reject_already_applied_step";
}

public sealed record OfflineGeoworldSessionCheckpoint
{
    public int AfterEventCount { get; init; }
    public int StepIndex { get; init; }
    public string StateHash { get; init; } = string.Empty;
    public string SnapshotHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldSessionReplayStep
{
    public int StepIndex { get; init; }
    public string EventId { get; init; } = string.Empty;
    public string DeltaId { get; init; } = string.Empty;
    public string StateHashBefore { get; init; } = string.Empty;
    public string StateHashAfter { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldSessionAcceptanceChecklist
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public string UnityMenuPath { get; init; } = "LLMGameCreator/Offline Geoworld Session Replay";
    public int StepCount { get; init; }
    public IReadOnlyList<OfflineGeoworldSessionAcceptanceStep> Steps { get; init; } = [];
}

public sealed record OfflineGeoworldSessionAcceptanceStep
{
    public int StepIndex { get; init; }
    public string Instruction { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldSessionReadme
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public bool OfflineSyntheticOnly { get; init; } = true;
    public bool MetadataOnly { get; init; } = true;
    public bool AlphaToolingOnly { get; init; } = true;
    public bool ImplementsFinalRuntimeSaveSystem { get; init; }
    public bool UsesRealGeodata { get; init; }
    public bool UsesProviderOrNetworkCalls { get; init; }
    public string ScopeSummary { get; init; } =
        "Unity Alpha save/load/replay harness over Goal105 metadata-only interaction deltas.";
}

public sealed record OfflineGeoworldSessionSourceFile
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public int MaxLineLength { get; init; }
    public bool NotMinified { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
}

public sealed record OfflineGeoworldSessionUnityScriptInventory
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScannedUnitySourceFileCount { get; init; }
    public bool SaveLoadControllerExists { get; init; }
    public bool ReplayControllerExists { get; init; }
    public bool SnapshotModelExists { get; init; }
    public bool ReadsApplicationStreamingAssetsPath { get; init; }
    public bool UsesApplicationPersistentDataPath { get; init; }
    public bool ReadsGoal106Root { get; init; }
    public bool IntegratesGoal105ControllerAndDeltaLog { get; init; }
    public bool SupportsSaveLoadDeleteSnapshot { get; init; }
    public bool SupportsReplayStepping { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
    public IReadOnlyList<OfflineGeoworldSessionSourceFile> Files { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldSessionEditorWindowInventory
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool EditorWindowScriptExists { get; init; }
    public bool MenuItemMarkerPresent { get; init; }
    public bool StreamingAssetsPathMarkerPresent { get; init; }
    public bool Goal106PayloadPathMarkerPresent { get; init; }
    public bool CreateRigMethodPresent { get; init; }
    public bool ClearRigMethodPresent { get; init; }
    public bool AcceptanceChecklistUiPresent { get; init; }
    public bool ManualButtonOnly { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
    public bool HasNoAutoRunImportMarker { get; init; }
    public OfflineGeoworldSessionSourceFile SourceFile { get; init; } = new();
    public IReadOnlyList<OfflineGeoworldSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldSessionSimulatedReplayProof
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool ManifestRead { get; init; }
    public bool InitialStateRead { get; init; }
    public bool DeltaLogRead { get; init; }
    public bool ReplayScriptRead { get; init; }
    public bool AcceptanceChecklistRead { get; init; }
    public bool PayloadHashesMatchManifest { get; init; }
    public bool FirstHalfReplayApplied { get; init; }
    public bool CheckpointSaved { get; init; }
    public bool CheckpointLoaded { get; init; }
    public bool ReplayResumedToFinalHash { get; init; }
    public bool DuplicateReplayRejected { get; init; }
    public bool CorruptedSnapshotRejected { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoProviderOrNetworkMarkers { get; init; }
    public int ReplayStepCount { get; init; }
    public int StateDeltaCount { get; init; }
    public int CheckpointStepIndex { get; init; }
    public string CheckpointStateHash { get; init; } = string.Empty;
    public string SavedSnapshotHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ReplayStateHashChain { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldSessionNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<OfflineGeoworldSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldSessionNegativeProof
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<OfflineGeoworldSessionNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldSessionWorkspaceBindingInventory
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool WorkspaceCatalogIncludesSessionReplayGroup { get; init; }
    public bool WorkspaceReadsGoal106EvidenceByRelativePath { get; init; }
    public bool WinFormsPageDisplaysSessionReplayFields { get; init; }
    public bool ShowsDeltaCount { get; init; }
    public bool ShowsReplayStepCount { get; init; }
    public bool ShowsCheckpointStep { get; init; }
    public bool ShowsFinalHash { get; init; }
    public bool ShowsUnityScriptReadiness { get; init; }
    public bool ShowsEditorHelperReadiness { get; init; }
    public bool ShowsSimulatedReplayProofStatus { get; init; }
    public bool ShowsAcceptanceChecklistSummary { get; init; }
    public bool ShowsAlphaRuntimeBootstrapUnchangedStatus { get; init; }
    public IReadOnlyList<OfflineGeoworldSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldSessionSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldSessionSourceLineage
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal105AcceptedFalsePreserved { get; init; }
    public bool Goal105PayloadConsumed { get; init; }
    public bool Goal105UnityEvidenceConsumed { get; init; }
    public IReadOnlyList<OfflineGeoworldSessionSourceLineageRecord> Records { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldSessionQualityGateScan
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal105Consumed { get; init; }
    public bool SessionPayloadCreated { get; init; }
    public bool SaveLoadReplayProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool UnityScriptsReady { get; init; }
    public bool EditorWindowReady { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string AlphaRuntimeBootstrapAfterHash { get; init; } = string.Empty;
    public int AlphaRuntimeBootstrapAfterLineCount { get; init; }
    public bool NoNetworkOrProviderImplementation { get; init; }
    public bool NoRawGeodataDump { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoScenePrefabSettingsChanges { get; init; }
    public bool NoExternalDependenciesOrNewInputSystem { get; init; }
    public bool NoRuntimePublicSchemaProjectDependencyChanges { get; init; } = true;
    public int ReplayStepCount { get; init; }
    public int StateDeltaCount { get; init; }
    public int CheckpointStepIndex { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldSessionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldSessionReport
{
    public string GoalId { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldSessionPersistenceReplayVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int ReplayStepCount { get; init; }
    public int StateDeltaCount { get; init; }
    public int CheckpointStepIndex { get; init; }
    public string CheckpointStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public bool UnityScriptsReady { get; init; }
    public bool EditorWindowReady { get; init; }
    public bool SimulatedSaveLoadReplayProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool QualityGatePassed { get; init; }
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldSessionBuildResult
{
    public OfflineGeoworldSessionManifest Manifest { get; init; } = new();
    public OfflineGeoworldSessionInitialState InitialState { get; init; } = new();
    public OfflineGeoworldSessionDeltaLog DeltaLog { get; init; } = new();
    public OfflineGeoworldSessionReplayScript ReplayScript { get; init; } = new();
    public OfflineGeoworldSessionAcceptanceChecklist AcceptanceChecklist { get; init; } = new();
    public OfflineGeoworldSessionReadme Readme { get; init; } = new();
    public OfflineGeoworldSessionUnityScriptInventory UnityScriptInventory { get; init; } = new();
    public OfflineGeoworldSessionEditorWindowInventory EditorWindowInventory { get; init; } = new();
    public OfflineGeoworldSessionSimulatedReplayProof SimulatedReplayProof { get; init; } = new();
    public OfflineGeoworldSessionNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldSessionWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldSessionSourceLineage SourceLineage { get; init; } = new();
    public OfflineGeoworldSessionQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldSessionReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldSessionWriteResult
{
    public OfflineGeoworldSessionBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal static class OfflineGeoworldSessionJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options) + Environment.NewLine;

    public static T? Deserialize<T>(string json) =>
        string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, Options);
}

internal static class OfflineGeoworldSessionHash
{
    public static string Sha256Text(string text) => Sha256Bytes(Encoding.UTF8.GetBytes(text));

    public static string Sha256Bytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string Sha256File(string path) => Sha256Bytes(File.ReadAllBytes(path));
}
