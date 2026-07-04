using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.OfflineGeoworldInteractionPlayableProbe;

public static class OfflineGeoworldInteractionPlayableProbeVocabulary
{
    public const string GoalId = "goal_105_offline_geoworld_interaction_playable_probe";
    public const string ProductSmokeRoute =
        "goal-105-offline-geoworld-interaction-playable-probe";
    public const string FinalGate =
        "offline_geoworld_interaction_playable_probe_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-105-offline-geoworld-interaction-playable-probe";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal105";
    public const string UnityStreamingAssetsProbeRoot =
        "LLMGameCreator/OfflineGeoworldGoal105";
    public const string Goal104SourceRoot =
        ".llmgc/procedural/goal-104-offline-geoworld-interactive-travel-preview";

    public const string UnityControllerScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionController.cs";
    public const string UnityTargetScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractionTarget.cs";
    public const string UnityStateDeltaLogScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldStateDeltaLog.cs";
    public const string UnityEditorWindowScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractionProbeWindow.cs";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string ManifestSchemaVersion =
        "offline_geoworld_interaction_manifest_v1";
    public const string TargetsSchemaVersion =
        "offline_geoworld_interaction_targets_v1";
    public const string ActionsSchemaVersion =
        "offline_geoworld_interaction_actions_v1";
    public const string SessionScriptSchemaVersion =
        "offline_geoworld_interaction_session_script_v1";
    public const string StateDeltaPlanSchemaVersion =
        "offline_geoworld_interaction_state_delta_plan_v1";
    public const string ReadmeSchemaVersion =
        "offline_geoworld_interaction_readme_v1";
    public const string UnityScriptInventorySchemaVersion =
        "offline_geoworld_interaction_unity_script_inventory_v1";
    public const string EditorWindowInventorySchemaVersion =
        "offline_geoworld_interaction_editor_window_inventory_v1";
    public const string SimulatedSessionProofSchemaVersion =
        "offline_geoworld_interaction_simulated_session_proof_v1";
    public const string NegativeProofSchemaVersion =
        "offline_geoworld_interaction_negative_proof_v1";
    public const string WorkspaceBindingSchemaVersion =
        "offline_geoworld_interaction_workspace_binding_inventory_v1";
    public const string SourceLineageSchemaVersion =
        "offline_geoworld_interaction_source_lineage_v1";
    public const string QualityGateSchemaVersion =
        "offline_geoworld_interaction_quality_gate_scan_v1";

    public const string ManifestFileName =
        "offline-geoworld-interaction-manifest.json";
    public const string TargetsFileName =
        "offline-geoworld-interaction-targets.json";
    public const string ActionsFileName =
        "offline-geoworld-interaction-actions.json";
    public const string SessionScriptFileName =
        "offline-geoworld-interaction-session-script.json";
    public const string StateDeltaPlanFileName =
        "offline-geoworld-interaction-state-delta-plan.json";
    public const string ReadmeFileName =
        "offline-geoworld-interaction-readme.json";
    public const string ReportMarkdownFileName =
        "offline-geoworld-interaction-report.md";
    public const string UnityScriptInventoryFileName =
        "offline-geoworld-interaction-unity-script-inventory.json";
    public const string EditorWindowInventoryFileName =
        "offline-geoworld-interaction-editor-window-inventory.json";
    public const string SimulatedSessionProofFileName =
        "offline-geoworld-interaction-simulated-session-proof.json";
    public const string NegativeProofFileName =
        "offline-geoworld-interaction-negative-proof.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-interaction-workspace-binding-inventory.json";
    public const string SourceLineageFileName =
        "offline-geoworld-interaction-source-lineage.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-interaction-quality-gate-scan.json";

    public static readonly IReadOnlyList<string> RequiredActionKinds =
    [
        "inspect",
        "enter_or_focus",
        "mark_visited",
        "toggle_blocked",
        "collect_sample"
    ];

    public static readonly IReadOnlyList<string> RequiredPayloadFileNames =
    [
        ManifestFileName,
        TargetsFileName,
        ActionsFileName,
        SessionScriptFileName,
        StateDeltaPlanFileName,
        ReadmeFileName
    ];

    public static readonly IReadOnlyList<string> RequiredEvidenceFileNames =
    [
        ReportMarkdownFileName,
        ManifestFileName,
        TargetsFileName,
        ActionsFileName,
        SessionScriptFileName,
        StateDeltaPlanFileName,
        UnityScriptInventoryFileName,
        EditorWindowInventoryFileName,
        SimulatedSessionProofFileName,
        NegativeProofFileName,
        WorkspaceBindingInventoryFileName,
        SourceLineageFileName,
        QualityGateScanFileName
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_goal104_payload",
        "interaction_target_referencing_unknown_object",
        "action_missing_target",
        "unavailable_action_accepted_outside_radius",
        "state_delta_mutates_base_data_directly",
        "fake_success_without_file_reads",
        "absolute_path",
        "raw_geodata_leak",
        "network_provider_marker",
        "alpha_runtime_bootstrap_dependency_marker",
        "scene_prefab_settings_mutation_marker",
        "binary_raster_media_marker",
        "external_dependency_new_input_system_marker"
    ];
}

public sealed record OfflineGeoworldInteractionDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static OfflineGeoworldInteractionDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record OfflineGeoworldInteractionManifest
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.ManifestSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PayloadFileCount { get; init; }
    public int TargetCount { get; init; }
    public int ActionCount { get; init; }
    public int ActionKindCount { get; init; }
    public int ScriptedEventCount { get; init; }
    public int StateDeltaCount { get; init; }
    public int SourceGoal104ObjectCount { get; init; }
    public int SourceGoal104MovementSampleCount { get; init; }
    public int SourceGoal104BoundaryCrossingCount { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.UnityStreamingAssetsProbeRoot;
    public bool MetadataOnly { get; init; } = true;
    public bool StateDeltasSeparateFromBaseData { get; init; } = true;
    public bool NoRawGeodata { get; init; } = true;
    public bool NoAbsolutePaths { get; init; } = true;
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public bool NoProviderOrNetworkMarkers { get; init; } = true;
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsFinalGameplay { get; init; }
    public bool ContainsRealGeodataFetch { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string TargetsHash { get; init; } = string.Empty;
    public string ActionsHash { get; init; } = string.Empty;
    public string SessionScriptHash { get; init; } = string.Empty;
    public string StateDeltaPlanHash { get; init; } = string.Empty;
    public string ReadmeHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldInteractionTargetsDocument
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.TargetsSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int TargetCount { get; init; }
    public int SourceGoal104VisibleObjectCount { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractionTargetRecord> Targets { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionTargetRecord
{
    public string TargetId { get; init; } = string.Empty;
    public string TargetName { get; init; } = string.Empty;
    public string SourceObjectId { get; init; } = string.Empty;
    public string SourceObjectName { get; init; } = string.Empty;
    public string SourceCommandId { get; init; } = string.Empty;
    public string CommandKind { get; init; } = string.Empty;
    public string SourceChunkKey { get; init; } = string.Empty;
    public int GridX { get; init; }
    public int GridZ { get; init; }
    public int Elevation { get; init; }
    public double InteractionRadius { get; init; }
    public bool BindById { get; init; } = true;
    public bool BindByName { get; init; } = true;
    public bool MetadataOnly { get; init; } = true;
    public bool RawGeodataIncluded { get; init; }
    public IReadOnlyList<int> VisibleStepIndexes { get; init; } = [];
    public IReadOnlyList<string> ActionIds { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionActionsDocument
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.ActionsSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int ActionCount { get; init; }
    public int ActionKindCount { get; init; }
    public IReadOnlyList<string> ActionKinds { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractionActionRecord> Actions { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionActionRecord
{
    public string ActionId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string ActionKind { get; init; } = string.Empty;
    public string DisplayLabel { get; init; } = string.Empty;
    public double RequiredRadius { get; init; }
    public bool RequiresProximity { get; init; } = true;
    public bool ProducesStateDelta { get; init; } = true;
    public string StateDeltaKind { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldInteractionSessionScript
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.SessionScriptSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int EventCount { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractionScriptedEvent> Events { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionScriptedEvent
{
    public int EventIndex { get; init; }
    public string EventId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string ActionId { get; init; } = string.Empty;
    public string ActionKind { get; init; } = string.Empty;
    public int PlayerGridX { get; init; }
    public int PlayerGridZ { get; init; }
    public double DistanceToTarget { get; init; }
    public double RequiredRadius { get; init; }
    public bool AvailableByDistance { get; init; }
    public string ExpectedStateHashBefore { get; init; } = string.Empty;
    public string ExpectedStateHashAfter { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldInteractionStateDeltaPlan
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.StateDeltaPlanSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool MutatesBaseDataDirectly { get; init; }
    public int StateDeltaCount { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> StateHashChain { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractionStateDelta> Deltas { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionStateDelta
{
    public int DeltaIndex { get; init; }
    public string EventId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string ActionId { get; init; } = string.Empty;
    public string ActionKind { get; init; } = string.Empty;
    public string DeltaKind { get; init; } = string.Empty;
    public string StateKey { get; init; } = string.Empty;
    public string StateValue { get; init; } = string.Empty;
    public string PreviousStateHash { get; init; } = string.Empty;
    public string DeterministicStateHash { get; init; } = string.Empty;
    public bool MutatesBaseDataDirectly { get; init; }
}

public sealed record OfflineGeoworldInteractionReadme
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.ReadmeSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool OfflineSyntheticOnly { get; init; } = true;
    public bool MetadataOnly { get; init; } = true;
    public bool StateDeltasInMemoryOnlyInUnity { get; init; } = true;
    public bool ImplementsFinalRuntimeGameplay { get; init; }
    public bool ImplementsFinalArt { get; init; }
    public bool ImplementsRealGeodataFetching { get; init; }
    public bool ImplementsReleaseBuildBehavior { get; init; }
    public bool UsesRelativePathsOnly { get; init; } = true;
    public string ScopeSummary { get; init; } =
        "Unity Alpha interaction probe over real Goal104 metadata with separate state deltas.";
}

public sealed record OfflineGeoworldInteractionSourceFile
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

public sealed record OfflineGeoworldInteractionUnityScriptInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.UnityScriptInventorySchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScannedUnitySourceFileCount { get; init; }
    public bool ControllerExists { get; init; }
    public bool TargetScriptExists { get; init; }
    public bool StateDeltaLogExists { get; init; }
    public bool ControllerUsesApplicationStreamingAssetsPath { get; init; }
    public bool ControllerReadsGoal105Root { get; init; }
    public bool ControllerBindsTargetsByIdOrName { get; init; }
    public bool ControllerSupportsNearestTargetSelection { get; init; }
    public bool ControllerExecutesScriptedAndManualActions { get; init; }
    public bool StateDeltaLogInMemoryOnly { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractionSourceFile> Files { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionEditorWindowInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.EditorWindowInventorySchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool EditorWindowScriptExists { get; init; }
    public bool MenuItemMarkerPresent { get; init; }
    public bool StreamingAssetsPathMarkerPresent { get; init; }
    public bool Goal105PayloadPathMarkerPresent { get; init; }
    public bool CreateRigMethodPresent { get; init; }
    public bool ClearRigMethodPresent { get; init; }
    public bool PayloadReadinessUiPresent { get; init; }
    public bool ManualButtonOnly { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
    public bool HasNoAutoRunImportMarker { get; init; }
    public OfflineGeoworldInteractionSourceFile SourceFile { get; init; } = new();
    public IReadOnlyList<OfflineGeoworldInteractionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionSimulatedSessionProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.SimulatedSessionProofSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool ManifestRead { get; init; }
    public bool TargetsRead { get; init; }
    public bool ActionsRead { get; init; }
    public bool SessionScriptRead { get; init; }
    public bool StateDeltaPlanRead { get; init; }
    public bool PayloadHashesMatchManifest { get; init; }
    public bool TargetBindingByIdOrNamePassed { get; init; }
    public bool ActionAvailabilityByDistancePassed { get; init; }
    public bool ScriptedInteractionsApplied { get; init; }
    public bool StateDeltaAppendPassed { get; init; }
    public bool DeterministicStateHashChainPassed { get; init; }
    public bool UnavailableActionRejected { get; init; }
    public bool StateDeltasSeparateFromBaseData { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoProviderOrNetworkMarkers { get; init; }
    public int TargetCount { get; init; }
    public int ActionKindCount { get; init; }
    public int ScriptedEventCount { get; init; }
    public int StateDeltaCount { get; init; }
    public string InitialStateHash { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> StateHashChain { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<OfflineGeoworldInteractionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionNegativeProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractionNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionWorkspaceBindingInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.WorkspaceBindingSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool WorkspaceCatalogIncludesInteractionGroup { get; init; }
    public bool WorkspaceReadsGoal105EvidenceByRelativePath { get; init; }
    public bool WinFormsPageDisplaysInteractionFields { get; init; }
    public bool ShowsTargetCount { get; init; }
    public bool ShowsActionKindCount { get; init; }
    public bool ShowsScriptedEventCount { get; init; }
    public bool ShowsStateDeltaCount { get; init; }
    public bool ShowsDeterministicHashChainStatus { get; init; }
    public bool ShowsUnityScriptReadiness { get; init; }
    public bool ShowsEditorHelperReadiness { get; init; }
    public bool ShowsUnitySafetyScanStatus { get; init; }
    public bool ShowsSimulatedSessionProofStatus { get; init; }
    public bool ShowsAlphaRuntimeBootstrapUnchangedStatus { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldInteractionSourceLineage
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal104AcceptedFalsePreserved { get; init; }
    public bool Goal104PayloadConsumed { get; init; }
    public bool Goal104UnityScriptEvidenceConsumed { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractionSourceLineageRecord> Records { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionQualityGateScan
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal104Consumed { get; init; }
    public bool InteractionPayloadCreated { get; init; }
    public bool TargetGraphBuilt { get; init; }
    public bool ActionGraphBuilt { get; init; }
    public bool SessionScriptBuilt { get; init; }
    public bool StateDeltaPlanBuilt { get; init; }
    public bool StateHashChainPassed { get; init; }
    public bool UnityScriptsReady { get; init; }
    public bool EditorWindowReady { get; init; }
    public bool UnityScriptInventorySafetyPassed { get; init; }
    public bool SimulatedSessionProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
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
    public int TargetCount { get; init; }
    public int ActionKindCount { get; init; }
    public int ActionCount { get; init; }
    public int ScriptedEventCount { get; init; }
    public int StateDeltaCount { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractionReport
{
    public string GoalId { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldInteractionPlayableProbeVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int TargetCount { get; init; }
    public int ActionKindCount { get; init; }
    public int ActionCount { get; init; }
    public int ScriptedEventCount { get; init; }
    public int StateDeltaCount { get; init; }
    public bool DeterministicStateHashChainPassed { get; init; }
    public bool UnityScriptsReady { get; init; }
    public bool EditorWindowReady { get; init; }
    public bool UnityScriptInventorySafetyPassed { get; init; }
    public bool SimulatedSessionProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool QualityGatePassed { get; init; }
    public string FinalStateHash { get; init; } = string.Empty;
    public string ManifestHash { get; init; } = string.Empty;
    public string TargetsHash { get; init; } = string.Empty;
    public string ActionsHash { get; init; } = string.Empty;
    public string SessionScriptHash { get; init; } = string.Empty;
    public string StateDeltaPlanHash { get; init; } = string.Empty;
    public string UnityScriptInventoryHash { get; init; } = string.Empty;
    public string EditorWindowInventoryHash { get; init; } = string.Empty;
    public string SimulatedSessionProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string WorkspaceBindingInventoryHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldInteractionBuildResult
{
    public OfflineGeoworldInteractionManifest Manifest { get; init; } = new();
    public OfflineGeoworldInteractionTargetsDocument Targets { get; init; } = new();
    public OfflineGeoworldInteractionActionsDocument Actions { get; init; } = new();
    public OfflineGeoworldInteractionSessionScript SessionScript { get; init; } = new();
    public OfflineGeoworldInteractionStateDeltaPlan StateDeltaPlan { get; init; } = new();
    public OfflineGeoworldInteractionReadme Readme { get; init; } = new();
    public OfflineGeoworldInteractionUnityScriptInventory UnityScriptInventory { get; init; } = new();
    public OfflineGeoworldInteractionEditorWindowInventory EditorWindowInventory { get; init; } = new();
    public OfflineGeoworldInteractionSimulatedSessionProof SimulatedSessionProof { get; init; } = new();
    public OfflineGeoworldInteractionNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldInteractionWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldInteractionSourceLineage SourceLineage { get; init; } = new();
    public OfflineGeoworldInteractionQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldInteractionReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldInteractionWriteResult
{
    public OfflineGeoworldInteractionBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal static class OfflineGeoworldInteractionJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options) + Environment.NewLine;

    public static T? Deserialize<T>(string json) =>
        string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, Options);
}

internal static class OfflineGeoworldInteractionHash
{
    public static string Sha256Text(string text) =>
        Sha256Bytes(Encoding.UTF8.GetBytes(text));

    public static string Sha256Bytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string Sha256File(string path) =>
        Sha256Bytes(File.ReadAllBytes(path));
}
