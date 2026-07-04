using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityPlayModeTravelPreview;

public static class OfflineGeoworldPlayModeTravelPreviewVocabulary
{
    public const string GoalId = "goal_103_offline_geoworld_playmode_travel_preview";
    public const string ProductSmokeRoute =
        "goal-103-offline-geoworld-playmode-travel-preview";
    public const string FinalGate =
        "offline_geoworld_playmode_travel_preview_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal103";
    public const string UnityStreamingAssetsProbeRoot =
        "LLMGameCreator/OfflineGeoworldGoal103";

    public const string Goal101SourceRoot =
        ".llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner";
    public const string Goal102SourceRoot =
        ".llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool";
    public const string Goal102BSourceRoot =
        ".llmgc/procedural/goal-102b-actual-unity-editor-source-reformat";

    public const string UnityControllerScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelController.cs";
    public const string UnityStateScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeTravelState.cs";
    public const string UnityChunkVisibilityScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPlayModeChunkVisibility.cs";
    public const string UnityEditorWindowScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPlayModeTravelWindow.cs";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string ManifestSchemaVersion =
        "offline_geoworld_playmode_travel_manifest_v1";
    public const string StepsSchemaVersion =
        "offline_geoworld_playmode_travel_steps_v1";
    public const string ChunkVisibilitySchemaVersion =
        "offline_geoworld_playmode_chunk_visibility_v1";
    public const string ObjectStateIndexSchemaVersion =
        "offline_geoworld_playmode_object_state_index_v1";
    public const string ReadmeSchemaVersion =
        "offline_geoworld_playmode_readme_v1";
    public const string UnityScriptInventorySchemaVersion =
        "offline_geoworld_playmode_unity_script_inventory_v1";
    public const string EditorWindowInventorySchemaVersion =
        "offline_geoworld_playmode_editor_window_inventory_v1";
    public const string SimulatedExecutionProofSchemaVersion =
        "offline_geoworld_playmode_simulated_execution_proof_v1";
    public const string NegativeProofSchemaVersion =
        "offline_geoworld_playmode_negative_proof_v1";
    public const string WorkspaceBindingSchemaVersion =
        "offline_geoworld_playmode_workspace_binding_inventory_v1";
    public const string SourceLineageSchemaVersion =
        "offline_geoworld_playmode_source_lineage_v1";
    public const string QualityGateSchemaVersion =
        "offline_geoworld_playmode_quality_gate_scan_v1";
    public const string Goal102BClosureSchemaVersion =
        "goal102b_false_positive_closure_v1";

    public const string ManifestFileName =
        "offline-geoworld-playmode-travel-manifest.json";
    public const string StepsFileName =
        "offline-geoworld-playmode-steps.json";
    public const string ChunkVisibilityFileName =
        "offline-geoworld-playmode-chunk-visibility.json";
    public const string ObjectStateIndexFileName =
        "offline-geoworld-playmode-object-state-index.json";
    public const string ReadmeFileName =
        "offline-geoworld-playmode-readme.json";
    public const string ReportMarkdownFileName =
        "offline-geoworld-playmode-travel-report.md";
    public const string UnityScriptInventoryFileName =
        "offline-geoworld-playmode-unity-script-inventory.json";
    public const string EditorWindowInventoryFileName =
        "offline-geoworld-playmode-editor-window-inventory.json";
    public const string SimulatedExecutionProofFileName =
        "offline-geoworld-playmode-simulated-execution-proof.json";
    public const string NegativeProofFileName =
        "offline-geoworld-playmode-negative-proof.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-playmode-workspace-binding-inventory.json";
    public const string SourceLineageFileName =
        "offline-geoworld-playmode-source-lineage.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-playmode-quality-gate-scan.json";
    public const string Goal102BClosureFileName =
        "goal102b-false-positive-closure.json";

    public static readonly IReadOnlyList<string> RequiredPayloadFileNames =
    [
        ManifestFileName,
        StepsFileName,
        ChunkVisibilityFileName,
        ObjectStateIndexFileName,
        ReadmeFileName
    ];

    public static readonly IReadOnlyList<string> RequiredEvidenceFileNames =
    [
        ReportMarkdownFileName,
        ManifestFileName,
        StepsFileName,
        ChunkVisibilityFileName,
        ObjectStateIndexFileName,
        UnityScriptInventoryFileName,
        EditorWindowInventoryFileName,
        SimulatedExecutionProofFileName,
        NegativeProofFileName,
        WorkspaceBindingInventoryFileName,
        SourceLineageFileName,
        QualityGateScanFileName,
        Goal102BClosureFileName
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_goal101_travel_payload",
        "missing_goal103_manifest",
        "unsupported_travel_step",
        "active_chunk_missing_from_chunk_visibility",
        "object_state_references_unknown_object",
        "fake_success_without_reading_files",
        "absolute_path_in_payload",
        "raw_geodata_leaked_into_playmode_plan",
        "network_provider_marker_in_unity_scripts",
        "alpha_runtime_bootstrap_dependency_marker",
        "scene_prefab_project_settings_mutation_marker",
        "binary_raster_media_marker",
        "goal102b_closure_without_actual_evidence"
    ];
}

public sealed record OfflineGeoworldPlayModeDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static OfflineGeoworldPlayModeDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record OfflineGeoworldPlayModeTravelManifest
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.ManifestSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PayloadFileCount { get; init; }
    public int StepCount { get; init; }
    public int ObjectCount { get; init; }
    public int SourceCommandCount { get; init; }
    public int SourceTravelWindowStepCount { get; init; }
    public int MaxActiveChunkCount { get; init; }
    public int MaxBoundaryPrefetchChunkCount { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityStreamingAssetsProbeRoot;
    public bool MetadataOnly { get; init; } = true;
    public bool NoRawGeodata { get; init; } = true;
    public bool NoAbsolutePaths { get; init; } = true;
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public bool NoProviderOrNetworkMarkers { get; init; } = true;
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsFinalArt { get; init; }
    public bool ContainsRealGeodataFetch { get; init; }
    public bool Goal102BFalsePositiveClosureRecorded { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string StepsHash { get; init; } = string.Empty;
    public string ChunkVisibilityHash { get; init; } = string.Empty;
    public string ObjectStateIndexHash { get; init; } = string.Empty;
    public string ReadmeHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldPlayModeTravelStepsDocument
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.StepsSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int StepCount { get; init; }
    public IReadOnlyList<OfflineGeoworldPlayModeTravelStep> Steps { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeTravelStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string SourceGoal101StepId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string CenterChunkKey { get; init; } = string.Empty;
    public IReadOnlyList<string> ActiveChunkKeys { get; init; } = [];
    public IReadOnlyList<string> BoundaryPrefetchChunkKeys { get; init; } = [];
    public IReadOnlyList<string> VisibleObjectIds { get; init; } = [];
    public IReadOnlyList<string> HiddenObjectIds { get; init; } = [];
    public IReadOnlyList<string> NewlyVisibleObjectIds { get; init; } = [];
    public IReadOnlyList<string> NewlyHiddenObjectIds { get; init; } = [];
    public int ExpectedVisibleObjectCount { get; init; }
    public string PreviousStateHash { get; init; } = string.Empty;
    public string DeterministicStateHash { get; init; } = string.Empty;
    public bool MetadataOnly { get; init; } = true;
}

public sealed record OfflineGeoworldPlayModeChunkVisibilityDocument
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.ChunkVisibilitySchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int StepCount { get; init; }
    public IReadOnlyList<OfflineGeoworldPlayModeChunkVisibilityStep> Steps { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeChunkVisibilityStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public IReadOnlyList<string> ActiveChunkKeys { get; init; } = [];
    public IReadOnlyList<string> BoundaryPrefetchChunkKeys { get; init; } = [];
    public int ActiveChunkCount { get; init; }
    public int BoundaryPrefetchChunkCount { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> VisibleObjectIdsByChunk { get; init; } =
        new SortedDictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldPlayModeObjectStateIndex
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.ObjectStateIndexSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int ObjectCount { get; init; }
    public IReadOnlyList<OfflineGeoworldPlayModeObjectState> Objects { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeObjectState
{
    public string ObjectId { get; init; } = string.Empty;
    public string ObjectName { get; init; } = string.Empty;
    public string SourceCommandId { get; init; } = string.Empty;
    public string CommandKind { get; init; } = string.Empty;
    public string SourceChunkKey { get; init; } = string.Empty;
    public int GridX { get; init; }
    public int GridZ { get; init; }
    public int Elevation { get; init; }
    public IReadOnlyList<int> VisibleStepIndexes { get; init; } = [];
    public bool MetadataOnly { get; init; } = true;
    public bool RawGeodataIncluded { get; init; }
}

public sealed record OfflineGeoworldPlayModeReadme
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.ReadmeSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool OfflineSyntheticOnly { get; init; } = true;
    public bool MetadataOnly { get; init; } = true;
    public bool ImplementsFinalRuntimeGameplay { get; init; }
    public bool ImplementsFinalArt { get; init; }
    public bool ImplementsRealGeodataFetching { get; init; }
    public bool ImplementsReleaseBuildBehavior { get; init; }
    public bool UsesRelativePathsOnly { get; init; } = true;
    public string ScopeSummary { get; init; } =
        "Unity Alpha play-mode travel preview tooling over Goal101 metadata only.";
}

public sealed record OfflineGeoworldPlayModeSourceFile
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
}

public sealed record OfflineGeoworldPlayModeUnityScriptInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.UnityScriptInventorySchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ControllerExists { get; init; }
    public bool StateExists { get; init; }
    public bool ChunkVisibilityExists { get; init; }
    public bool ControllerUsesApplicationStreamingAssetsPath { get; init; }
    public bool ControllerReadsGoal103Root { get; init; }
    public bool ControllerExposesInspectorFields { get; init; }
    public bool ControllerSupportsManualAndTimerSteps { get; init; }
    public bool ControllerActivatesObjectsByMetadata { get; init; }
    public bool ControllerToleratesMissingObjects { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public IReadOnlyList<OfflineGeoworldPlayModeSourceFile> Files { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldPlayModeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeEditorWindowInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.EditorWindowInventorySchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool EditorWindowScriptExists { get; init; }
    public bool MenuItemMarkerPresent { get; init; }
    public bool StreamingAssetsPathMarkerPresent { get; init; }
    public bool Goal103PayloadPathMarkerPresent { get; init; }
    public bool CreateControllerMethodPresent { get; init; }
    public bool ClearControllerMethodPresent { get; init; }
    public bool PayloadReadinessUiPresent { get; init; }
    public bool ManualButtonOnly { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
    public bool HasNoAutoRunImportMarker { get; init; }
    public OfflineGeoworldPlayModeSourceFile SourceFile { get; init; } = new();
    public IReadOnlyList<OfflineGeoworldPlayModeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeSimulatedExecutionProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.SimulatedExecutionProofSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool ManifestRead { get; init; }
    public bool StepsFileRead { get; init; }
    public bool ChunkVisibilityFileRead { get; init; }
    public bool ObjectStateIndexRead { get; init; }
    public bool PayloadHashesMatchManifest { get; init; }
    public bool StepByStepVisibleCountsPassed { get; init; }
    public bool BoundaryPrefetchProgressionRepresented { get; init; }
    public bool DeterministicStateHashChainPassed { get; init; }
    public bool NoUnsupportedStep { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoProviderOrNetworkMarkers { get; init; }
    public int StepCount { get; init; }
    public int ObjectCount { get; init; }
    public IReadOnlyList<int> ExpectedVisibleObjectCountsByStep { get; init; } = [];
    public IReadOnlyList<string> StateHashChain { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldPlayModeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<OfflineGeoworldPlayModeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeNegativeProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<OfflineGeoworldPlayModeNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeWorkspaceBindingInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.WorkspaceBindingSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool WorkspaceCatalogIncludesPlayModeTravelGroup { get; init; }
    public bool WorkspaceReadsGoal103EvidenceByRelativePath { get; init; }
    public bool WinFormsPageDisplaysPlayModeTravelFields { get; init; }
    public bool ShowsTravelStepCount { get; init; }
    public bool ShowsActiveChunkCounts { get; init; }
    public bool ShowsBoundaryPrefetchCounts { get; init; }
    public bool ShowsExpectedVisibleObjectCounts { get; init; }
    public bool ShowsUnityScriptReadiness { get; init; }
    public bool ShowsEditorLaunchHelperReadiness { get; init; }
    public bool ShowsSimulatedPlayModeProofStatus { get; init; }
    public bool ShowsGoal102BFalsePositiveClosureStatus { get; init; }
    public bool ShowsAlphaRuntimeBootstrapUnchangedStatus { get; init; }
    public IReadOnlyList<OfflineGeoworldPlayModeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldPlayModeSourceLineage
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal101AcceptedFalsePreserved { get; init; }
    public bool Goal101PayloadConsumed { get; init; }
    public bool Goal102EvidenceConsumed { get; init; }
    public bool Goal102BBlockedStatusPreserved { get; init; }
    public bool Goal102BActualEvidenceConsumed { get; init; }
    public IReadOnlyList<OfflineGeoworldPlayModeSourceLineageRecord> Records { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldPlayModeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record Goal102BFalsePositiveClosure
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.Goal102BClosureSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal102BRemainsBlocked { get; init; }
    public bool ProductSourceBlockerClosed { get; init; }
    public bool Goal102ANotMarkedGreenByThisGoal { get; init; }
    public bool Goal102BNotMarkedGreen { get; init; }
    public bool ActualHeadBeforeEvidenceRead { get; init; }
    public bool ActualHeadBeforeMalformedDetected { get; init; }
    public bool WorkingTreeSourceReadable { get; init; }
    public int ActualHeadRawPhysicalLineCount { get; init; }
    public int ActualHeadMaxPhysicalLineLength { get; init; }
    public bool FutureGatesRequireActualTargetBytes { get; init; } = true;
    public string DecisionSummary { get; init; } =
        "Goal102B remains BLOCKED, but the product/source blocker is closed as a false-positive because actual target source bytes are already readable.";
    public IReadOnlyList<OfflineGeoworldPlayModeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeQualityGateScan
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal101Consumed { get; init; }
    public bool Goal102BClosureRecorded { get; init; }
    public bool PlayModePayloadCreated { get; init; }
    public bool TravelStepPlanBuilt { get; init; }
    public bool BoundaryPrefetchRepresented { get; init; }
    public bool ObjectVisibilityDiffsBuilt { get; init; }
    public bool UnityScriptsReady { get; init; }
    public bool EditorWindowReady { get; init; }
    public bool SimulatedExecutionProofPassed { get; init; }
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
    public bool NoRuntimePublicSchemaProjectDependencyChanges { get; init; } = true;
    public int StepCount { get; init; }
    public int ObjectCount { get; init; }
    public int MaxActiveChunkCount { get; init; }
    public int MaxBoundaryPrefetchChunkCount { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldPlayModeDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPlayModeTravelReport
{
    public string GoalId { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldPlayModeTravelPreviewVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int StepCount { get; init; }
    public int ObjectCount { get; init; }
    public int MaxActiveChunkCount { get; init; }
    public int MaxBoundaryPrefetchChunkCount { get; init; }
    public bool UnityScriptsReady { get; init; }
    public bool EditorWindowReady { get; init; }
    public bool SimulatedExecutionProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool Goal102BClosureRecorded { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool QualityGatePassed { get; init; }
    public string ManifestHash { get; init; } = string.Empty;
    public string StepsHash { get; init; } = string.Empty;
    public string ChunkVisibilityHash { get; init; } = string.Empty;
    public string ObjectStateIndexHash { get; init; } = string.Empty;
    public string UnityScriptInventoryHash { get; init; } = string.Empty;
    public string EditorWindowInventoryHash { get; init; } = string.Empty;
    public string SimulatedExecutionProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string WorkspaceBindingInventoryHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string Goal102BClosureHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldPlayModeBuildResult
{
    public OfflineGeoworldPlayModeTravelManifest Manifest { get; init; } = new();
    public OfflineGeoworldPlayModeTravelStepsDocument Steps { get; init; } = new();
    public OfflineGeoworldPlayModeChunkVisibilityDocument ChunkVisibility { get; init; } = new();
    public OfflineGeoworldPlayModeObjectStateIndex ObjectStateIndex { get; init; } = new();
    public OfflineGeoworldPlayModeReadme Readme { get; init; } = new();
    public OfflineGeoworldPlayModeUnityScriptInventory UnityScriptInventory { get; init; } = new();
    public OfflineGeoworldPlayModeEditorWindowInventory EditorWindowInventory { get; init; } = new();
    public OfflineGeoworldPlayModeSimulatedExecutionProof SimulatedExecutionProof { get; init; } = new();
    public OfflineGeoworldPlayModeNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldPlayModeWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldPlayModeSourceLineage SourceLineage { get; init; } = new();
    public Goal102BFalsePositiveClosure Goal102BClosure { get; init; } = new();
    public OfflineGeoworldPlayModeQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldPlayModeTravelReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldPlayModeWriteResult
{
    public OfflineGeoworldPlayModeBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal static class OfflineGeoworldPlayModeJson
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

internal static class OfflineGeoworldPlayModeHash
{
    public static string Sha256Text(string text) =>
        Sha256Bytes(Encoding.UTF8.GetBytes(text));

    public static string Sha256Bytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string Sha256File(string path) =>
        Sha256Bytes(File.ReadAllBytes(path));
}
