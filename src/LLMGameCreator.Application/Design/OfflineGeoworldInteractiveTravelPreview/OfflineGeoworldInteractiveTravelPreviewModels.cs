using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.OfflineGeoworldInteractiveTravelPreview;

public static class OfflineGeoworldInteractiveTravelPreviewVocabulary
{
    public const string GoalId = "goal_104_offline_geoworld_interactive_travel_preview";
    public const string ProductSmokeRoute =
        "goal-104-offline-geoworld-interactive-travel-preview";
    public const string FinalGate =
        "offline_geoworld_interactive_travel_preview_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-104-offline-geoworld-interactive-travel-preview";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal104";
    public const string UnityStreamingAssetsProbeRoot =
        "LLMGameCreator/OfflineGeoworldGoal104";

    public const string Goal103SourceRoot =
        ".llmgc/procedural/goal-103-offline-geoworld-playmode-travel-preview";

    public const string UnityControllerScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldInteractiveTravelController.cs";
    public const string UnityStateScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPlayerMotor.cs";
    public const string UnityChunkVisibilityScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldBoundaryPrefetchState.cs";
    public const string UnityEditorWindowScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldInteractiveTravelWindow.cs";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string ManifestSchemaVersion =
        "offline_geoworld_interactive_travel_manifest_v1";
    public const string StepsSchemaVersion =
        "offline_geoworld_interactive_movement_path_v1";
    public const string ChunkVisibilitySchemaVersion =
        "offline_geoworld_interactive_boundary_zones_v1";
    public const string ObjectStateIndexSchemaVersion =
        "offline_geoworld_interactive_prefetch_plan_v1";
    public const string ReadmeSchemaVersion =
        "offline_geoworld_interactive_readme_v1";
    public const string UnityScriptInventorySchemaVersion =
        "offline_geoworld_interactive_unity_script_inventory_v1";
    public const string EditorWindowInventorySchemaVersion =
        "offline_geoworld_interactive_editor_window_inventory_v1";
    public const string SimulatedExecutionProofSchemaVersion =
        "offline_geoworld_interactive_simulated_execution_proof_v1";
    public const string NegativeProofSchemaVersion =
        "offline_geoworld_interactive_negative_proof_v1";
    public const string WorkspaceBindingSchemaVersion =
        "offline_geoworld_interactive_workspace_binding_inventory_v1";
    public const string SourceLineageSchemaVersion =
        "offline_geoworld_interactive_source_lineage_v1";
    public const string QualityGateSchemaVersion =
        "offline_geoworld_interactive_quality_gate_scan_v1";
    public const string ManifestFileName =
        "offline-geoworld-interactive-travel-manifest.json";
    public const string StepsFileName =
        "offline-geoworld-interactive-movement-path.json";
    public const string ChunkVisibilityFileName =
        "offline-geoworld-interactive-boundary-zones.json";
    public const string ObjectStateIndexFileName =
        "offline-geoworld-interactive-prefetch-plan.json";
    public const string ReadmeFileName =
        "offline-geoworld-interactive-readme.json";
    public const string ReportMarkdownFileName =
        "offline-geoworld-interactive-travel-report.md";
    public const string UnityScriptInventoryFileName =
        "offline-geoworld-interactive-unity-script-inventory.json";
    public const string EditorWindowInventoryFileName =
        "offline-geoworld-interactive-editor-window-inventory.json";
    public const string SimulatedExecutionProofFileName =
        "offline-geoworld-interactive-simulated-execution-proof.json";
    public const string NegativeProofFileName =
        "offline-geoworld-interactive-negative-proof.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-interactive-workspace-binding-inventory.json";
    public const string SourceLineageFileName =
        "offline-geoworld-interactive-source-lineage.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-interactive-quality-gate-scan.json";

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
        QualityGateScanFileName
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_goal103_payload",
        "missing_goal104_manifest",
        "movement_path_without_boundary_crossings",
        "boundary_crossing_without_prefetch_plan",
        "object_visibility_diff_references_unknown_object",
        "fake_success_without_reading_files",
        "absolute_path_in_payload",
        "raw_geodata_leak",
        "network_provider_marker_in_unity_scripts",
        "alpha_runtime_bootstrap_dependency_marker",
        "scene_prefab_project_settings_mutation_marker",
        "binary_raster_media_marker",
        "new_input_system_or_external_dependency_marker"
    ];
}

public sealed record OfflineGeoworldInteractiveDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static OfflineGeoworldInteractiveDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record OfflineGeoworldInteractiveTravelManifest
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.ManifestSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PayloadFileCount { get; init; }
    public int StepCount { get; init; }
    public int MovementSampleCount { get; init; }
    public int BoundaryCrossingCount { get; init; }
    public int PrefetchPlanCount { get; init; }
    public int ObjectCount { get; init; }
    public int SourceCommandCount { get; init; }
    public int SourceTravelWindowStepCount { get; init; }
    public int SourceGoal103StepCount { get; init; }
    public int SourceGoal103ObjectCount { get; init; }
    public int MaxActiveChunkCount { get; init; }
    public int MaxBoundaryPrefetchChunkCount { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityStreamingAssetsProbeRoot;
    public bool MetadataOnly { get; init; } = true;
    public bool NoRawGeodata { get; init; } = true;
    public bool NoAbsolutePaths { get; init; } = true;
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public bool NoProviderOrNetworkMarkers { get; init; } = true;
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsFinalArt { get; init; }
    public bool ContainsRealGeodataFetch { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string StepsHash { get; init; } = string.Empty;
    public string ChunkVisibilityHash { get; init; } = string.Empty;
    public string ObjectStateIndexHash { get; init; } = string.Empty;
    public string MovementPathHash { get; init; } = string.Empty;
    public string BoundaryZonesHash { get; init; } = string.Empty;
    public string PrefetchPlanHash { get; init; } = string.Empty;
    public string ReadmeHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldInteractiveTravelStepsDocument
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.StepsSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int StepCount { get; init; }
    public int MovementSampleCount { get; init; }
    public int ObjectCount { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractiveTravelStep> Steps { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractiveTravelStep> MovementSamples { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractiveObjectState> Objects { get; init; } = [];
}

public sealed record OfflineGeoworldInteractiveTravelStep
{
    public int StepIndex { get; init; }
    public int MovementSampleIndex => StepIndex;
    public string StepId { get; init; } = string.Empty;
    public string MovementSampleId => StepId;
    public string SourceGoal101StepId { get; init; } = string.Empty;
    public string SourceGoal103StepId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string MovementKind => Action;
    public string CenterChunkKey { get; init; } = string.Empty;
    public string SyntheticChunkKey => CenterChunkKey;
    public bool BoundaryBand { get; init; }
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

public sealed record OfflineGeoworldInteractiveChunkVisibilityDocument
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.ChunkVisibilitySchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int StepCount { get; init; }
    public int BoundaryCrossingCount { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractiveChunkVisibilityStep> Steps { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractiveBoundaryZone> BoundaryZones { get; init; } = [];
}

public sealed record OfflineGeoworldInteractiveChunkVisibilityStep
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

public sealed record OfflineGeoworldInteractiveBoundaryZone
{
    public int CrossingIndex { get; init; }
    public string CrossingId { get; init; } = string.Empty;
    public int FromMovementSampleIndex { get; init; }
    public int ToMovementSampleIndex { get; init; }
    public string FromChunkKey { get; init; } = string.Empty;
    public string ToChunkKey { get; init; } = string.Empty;
    public string BoundaryAxis { get; init; } = string.Empty;
    public IReadOnlyList<string> ActiveChunkKeysBefore { get; init; } = [];
    public IReadOnlyList<string> ActiveChunkKeysAfter { get; init; } = [];
    public IReadOnlyList<string> PrefetchChunkKeys { get; init; } = [];
    public IReadOnlyList<string> VisibleObjectIdsBefore { get; init; } = [];
    public IReadOnlyList<string> VisibleObjectIdsAfter { get; init; } = [];
    public IReadOnlyList<string> NewlyVisibleObjectIds { get; init; } = [];
    public IReadOnlyList<string> NewlyHiddenObjectIds { get; init; } = [];
    public bool MetadataOnly { get; init; } = true;
}

public sealed record OfflineGeoworldInteractiveObjectStateIndex
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.ObjectStateIndexSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int ObjectCount { get; init; }
    public int PrefetchPlanCount { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractiveObjectState> Objects { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractivePrefetchPlan> Plans { get; init; } = [];
}

public sealed record OfflineGeoworldInteractivePrefetchPlan
{
    public int CrossingIndex { get; init; }
    public string CrossingId { get; init; } = string.Empty;
    public IReadOnlyList<string> ActiveChunkKeysBefore { get; init; } = [];
    public IReadOnlyList<string> ActiveChunkKeysAfter { get; init; } = [];
    public IReadOnlyList<string> PrefetchChunkKeys { get; init; } = [];
    public IReadOnlyList<string> AddedPrefetchChunkKeys { get; init; } = [];
    public IReadOnlyList<string> RemovedPrefetchChunkKeys { get; init; } = [];
    public IReadOnlyList<string> NewlyVisibleObjectIds { get; init; } = [];
    public IReadOnlyList<string> NewlyHiddenObjectIds { get; init; } = [];
    public bool MetadataOnly { get; init; } = true;
}

public sealed record OfflineGeoworldInteractiveObjectState
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

public sealed record OfflineGeoworldInteractiveReadme
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.ReadmeSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public bool OfflineSyntheticOnly { get; init; } = true;
    public bool MetadataOnly { get; init; } = true;
    public bool ImplementsFinalRuntimeGameplay { get; init; }
    public bool ImplementsFinalArt { get; init; }
    public bool ImplementsRealGeodataFetching { get; init; }
    public bool ImplementsReleaseBuildBehavior { get; init; }
    public bool UsesRelativePathsOnly { get; init; } = true;
    public string ScopeSummary { get; init; } =
        "Unity Alpha interactive travel preview tooling over real Goal103 metadata only.";
}

public sealed record OfflineGeoworldInteractiveSourceFile
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
}

public sealed record OfflineGeoworldInteractiveUnityScriptInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.UnityScriptInventorySchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ControllerExists { get; init; }
    public bool StateExists { get; init; }
    public bool ChunkVisibilityExists { get; init; }
    public bool ControllerUsesApplicationStreamingAssetsPath { get; init; }
    public bool ControllerReadsGoal104Root { get; init; }
    public bool ControllerExposesInspectorFields { get; init; }
    public bool ControllerSupportsManualAndTimerSteps { get; init; }
    public bool ControllerActivatesObjectsByMetadata { get; init; }
    public bool ControllerToleratesMissingObjects { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractiveSourceFile> Files { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractiveEditorWindowInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.EditorWindowInventorySchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool EditorWindowScriptExists { get; init; }
    public bool MenuItemMarkerPresent { get; init; }
    public bool StreamingAssetsPathMarkerPresent { get; init; }
    public bool Goal104PayloadPathMarkerPresent { get; init; }
    public bool CreateControllerMethodPresent { get; init; }
    public bool ClearControllerMethodPresent { get; init; }
    public bool PayloadReadinessUiPresent { get; init; }
    public bool ManualButtonOnly { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoScenePrefabSettingsMutationMarkers { get; init; }
    public bool HasNoAutoRunImportMarker { get; init; }
    public OfflineGeoworldInteractiveSourceFile SourceFile { get; init; } = new();
    public IReadOnlyList<OfflineGeoworldInteractiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractiveSimulatedExecutionProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.SimulatedExecutionProofSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool ManifestRead { get; init; }
    public bool StepsFileRead { get; init; }
    public bool ChunkVisibilityFileRead { get; init; }
    public bool ObjectStateIndexRead { get; init; }
    public bool PayloadHashesMatchManifest { get; init; }
    public bool StepByStepVisibleCountsPassed { get; init; }
    public bool MovementSampleCountPassed { get; init; }
    public bool BoundaryCrossingCountPassed { get; init; }
    public bool PrefetchPlanCoveragePassed { get; init; }
    public bool ObjectVisibilityDiffsPassed { get; init; }
    public bool BoundaryPrefetchProgressionRepresented { get; init; }
    public bool DeterministicStateHashChainPassed { get; init; }
    public bool NoUnsupportedStep { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoProviderOrNetworkMarkers { get; init; }
    public int StepCount { get; init; }
    public int MovementSampleCount { get; init; }
    public int BoundaryCrossingCount { get; init; }
    public int PrefetchPlanCount { get; init; }
    public int ObjectCount { get; init; }
    public IReadOnlyList<int> ExpectedVisibleObjectCountsByStep { get; init; } = [];
    public IReadOnlyList<string> StateHashChain { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractiveNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<OfflineGeoworldInteractiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractiveNegativeProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractiveNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldInteractiveWorkspaceBindingInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.WorkspaceBindingSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool WorkspaceCatalogIncludesInteractiveTravelGroup { get; init; }
    public bool WorkspaceReadsGoal104EvidenceByRelativePath { get; init; }
    public bool WinFormsPageDisplaysInteractiveTravelFields { get; init; }
    public bool ShowsMovementSampleCount { get; init; }
    public bool ShowsBoundaryCrossingCount { get; init; }
    public bool ShowsActiveChunkCounts { get; init; }
    public bool ShowsBoundaryPrefetchCounts { get; init; }
    public bool ShowsExpectedVisibleObjectCounts { get; init; }
    public bool ShowsUnityScriptReadiness { get; init; }
    public bool ShowsEditorLaunchHelperReadiness { get; init; }
    public bool ShowsSimulatedMovementProofStatus { get; init; }
    public bool ShowsAlphaRuntimeBootstrapUnchangedStatus { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractiveSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldInteractiveSourceLineage
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal103AcceptedFalsePreserved { get; init; }
    public bool Goal103PayloadConsumed { get; init; }
    public bool Goal103UnityScriptEvidenceConsumed { get; init; }
    public IReadOnlyList<OfflineGeoworldInteractiveSourceLineageRecord> Records { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractiveQualityGateScan
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal103Consumed { get; init; }
    public bool InteractivePayloadCreated { get; init; }
    public bool MovementPathBuilt { get; init; }
    public bool BoundaryZonesBuilt { get; init; }
    public bool PrefetchPlanBuilt { get; init; }
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
    public int MovementSampleCount { get; init; }
    public int BoundaryCrossingCount { get; init; }
    public int PrefetchPlanCount { get; init; }
    public int ObjectCount { get; init; }
    public int MaxActiveChunkCount { get; init; }
    public int MaxBoundaryPrefetchChunkCount { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldInteractiveDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldInteractiveTravelReport
{
    public string GoalId { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldInteractiveTravelPreviewVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int StepCount { get; init; }
    public int MovementSampleCount { get; init; }
    public int BoundaryCrossingCount { get; init; }
    public int PrefetchPlanCount { get; init; }
    public int ObjectCount { get; init; }
    public int MaxActiveChunkCount { get; init; }
    public int MaxBoundaryPrefetchChunkCount { get; init; }
    public bool UnityScriptsReady { get; init; }
    public bool EditorWindowReady { get; init; }
    public bool SimulatedExecutionProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool QualityGatePassed { get; init; }
    public string ManifestHash { get; init; } = string.Empty;
    public string StepsHash { get; init; } = string.Empty;
    public string ChunkVisibilityHash { get; init; } = string.Empty;
    public string ObjectStateIndexHash { get; init; } = string.Empty;
    public string MovementPathHash { get; init; } = string.Empty;
    public string BoundaryZonesHash { get; init; } = string.Empty;
    public string PrefetchPlanHash { get; init; } = string.Empty;
    public string UnityScriptInventoryHash { get; init; } = string.Empty;
    public string EditorWindowInventoryHash { get; init; } = string.Empty;
    public string SimulatedExecutionProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string WorkspaceBindingInventoryHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldInteractiveBuildResult
{
    public OfflineGeoworldInteractiveTravelManifest Manifest { get; init; } = new();
    public OfflineGeoworldInteractiveTravelStepsDocument Steps { get; init; } = new();
    public OfflineGeoworldInteractiveChunkVisibilityDocument ChunkVisibility { get; init; } = new();
    public OfflineGeoworldInteractiveObjectStateIndex ObjectStateIndex { get; init; } = new();
    public OfflineGeoworldInteractiveReadme Readme { get; init; } = new();
    public OfflineGeoworldInteractiveUnityScriptInventory UnityScriptInventory { get; init; } = new();
    public OfflineGeoworldInteractiveEditorWindowInventory EditorWindowInventory { get; init; } = new();
    public OfflineGeoworldInteractiveSimulatedExecutionProof SimulatedExecutionProof { get; init; } = new();
    public OfflineGeoworldInteractiveNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldInteractiveWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldInteractiveSourceLineage SourceLineage { get; init; } = new();
    public OfflineGeoworldInteractiveQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldInteractiveTravelReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldInteractiveWriteResult
{
    public OfflineGeoworldInteractiveBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal static class OfflineGeoworldInteractiveJson
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

internal static class OfflineGeoworldInteractiveHash
{
    public static string Sha256Text(string text) =>
        Sha256Bytes(Encoding.UTF8.GetBytes(text));

    public static string Sha256Bytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string Sha256File(string path) =>
        Sha256Bytes(File.ReadAllBytes(path));
}
