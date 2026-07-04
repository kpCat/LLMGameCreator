namespace LLMGameCreator.Application.Design.OfflineGeoworldAlphaSliceOrchestrator;

public static class OfflineGeoworldAlphaSliceVocabulary
{
    public const string GoalId = "goal_108_offline_geoworld_alpha_slice_orchestrator";
    public const string ProductSmokeRoute = "goal-108-offline-geoworld-alpha-slice-orchestrator";
    public const string FinalGate = "offline_geoworld_alpha_slice_orchestrator_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-108-offline-geoworld-alpha-slice-orchestrator";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal108";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string ManifestFileName = "offline-geoworld-alpha-slice-manifest.json";
    public const string ComponentsFileName = "offline-geoworld-alpha-slice-components.json";
    public const string AcceptanceRunbookFileName = "offline-geoworld-alpha-slice-acceptance-runbook.json";
    public const string ReadinessMatrixFileName = "offline-geoworld-alpha-slice-readiness-matrix.json";
    public const string ReadmeFileName = "offline-geoworld-alpha-slice-readme.json";
    public const string ReportMarkdownFileName = "offline-geoworld-alpha-slice-report.md";
    public const string UnityScriptInventoryFileName = "offline-geoworld-alpha-slice-unity-script-inventory.json";
    public const string EditorWindowInventoryFileName = "offline-geoworld-alpha-slice-editor-window-inventory.json";
    public const string SimulatedProofFileName = "offline-geoworld-alpha-slice-simulated-proof.json";
    public const string NegativeProofFileName = "offline-geoworld-alpha-slice-negative-proof.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-alpha-slice-workspace-binding-inventory.json";
    public const string QualityGateScanFileName = "offline-geoworld-alpha-slice-quality-gate-scan.json";

    public const string UnityCoordinatorScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSliceCoordinator.cs";
    public const string UnityEditorWindowScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaSliceWindow.cs";

    public static IReadOnlyList<string> RequiredPayloadFileNames =>
    [
        ManifestFileName,
        ComponentsFileName,
        AcceptanceRunbookFileName,
        ReadinessMatrixFileName,
        ReadmeFileName
    ];

    public static IReadOnlyList<string> RequiredEvidenceFileNames =>
    [
        UnityScriptInventoryFileName,
        EditorWindowInventoryFileName,
        SimulatedProofFileName,
        NegativeProofFileName,
        WorkspaceBindingInventoryFileName,
        QualityGateScanFileName
    ];

    public static IReadOnlyList<string> RequiredNegativeScenarioIds =>
    [
        "missing_goal107_payload",
        "missing_component",
        "accepted_true_fake_promotion",
        "historical_artifact_rewrite_attempt",
        "component_hash_mismatch",
        "one_click_setup_without_file_reads",
        "missing_clear_method",
        "objective_final_status_not_completed",
        "absolute_path_marker",
        "raw_geodata_leak",
        "network_provider_marker",
        "alpha_runtime_bootstrap_dependency",
        "scene_prefab_settings_mutation_marker",
        "binary_raster_media_marker",
        "external_dependency_new_input_system_marker"
    ];

    public static IReadOnlyList<string> ForbiddenBinaryOrRasterExtensions =>
    [
        ".osm",
        ".pbf",
        ".mbtiles",
        ".gpkg",
        ".geojson",
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes"
    ];

    public static IReadOnlyList<string> SourceHealthFiles =>
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator/OfflineGeoworldAlphaSliceOrchestratorEvidenceService.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator/OfflineGeoworldAlphaSliceOrchestratorEvidenceService.Components.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator/OfflineGeoworldAlphaSliceOrchestratorEvidenceService.Utilities.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator/OfflineGeoworldAlphaSliceOrchestratorModels.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator/OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditService.cs",
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator/OfflineGeoworldAlphaSliceSourceSplitImmutabilityAuditModels.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSliceCoordinator.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaSliceWindow.cs",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs"
    ];

    public static IReadOnlyList<string> ExpectedChangedPathPrefixes =>
    [
        "src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceOrchestrator/",
        "src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/",
        "src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaSliceOrchestrator/",
        "tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/",
        "tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaSliceOrchestratorProductSmokeTests.cs",
        "tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs",
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaSliceCoordinator.cs",
        "unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaSliceWindow.cs",
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal108/",
        ".llmgc/procedural/goal-108-offline-geoworld-alpha-slice-orchestrator/",
        "docs/agent-tasks/goal-108-offline-geoworld-alpha-slice-orchestrator/",
        "docs/CURRENT_GENERATOR_STATE.md",
        "docs/CURRENT_GENERATOR_STATE.json",
        "docs/CONTEXT_INDEX.md",
        "docs/FULL_GENERATOR_GOAL_QUEUE.md",
        "docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md",
        ".devflow/artifact-scope/artifact-scope-policy.json"
    ];
}

public sealed record OfflineGeoworldAlphaSliceBuildResult
{
    public OfflineGeoworldAlphaSliceManifest Manifest { get; init; } = new();
    public OfflineGeoworldAlphaSliceComponentsDocument Components { get; init; } = new();
    public OfflineGeoworldAlphaSliceAcceptanceRunbook AcceptanceRunbook { get; init; } = new();
    public OfflineGeoworldAlphaSliceReadinessMatrix ReadinessMatrix { get; init; } = new();
    public OfflineGeoworldAlphaSliceReadme Readme { get; init; } = new();
    public OfflineGeoworldAlphaSliceUnityScriptInventory UnityScriptInventory { get; init; } = new();
    public OfflineGeoworldAlphaSliceEditorWindowInventory EditorWindowInventory { get; init; } = new();
    public OfflineGeoworldAlphaSliceSimulatedProof SimulatedProof { get; init; } = new();
    public OfflineGeoworldAlphaSliceNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldAlphaSliceWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldAlphaSliceQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldAlphaSliceReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldAlphaSliceWriteResult
{
    public OfflineGeoworldAlphaSliceBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceManifest
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldAlphaSliceVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PayloadFileCount { get; init; }
    public int ComponentCount { get; init; }
    public int ReadyComponentCount { get; init; }
    public int MissingComponentCount { get; init; }
    public int ObjectiveCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string FinalAcceptanceHash { get; init; } = string.Empty;
    public string ComponentAggregateHash { get; init; } = string.Empty;
    public string StreamingAssetsRelativeRoot { get; init; } = "LLMGameCreator/OfflineGeoworldGoal108";
    public bool MetadataOnly { get; init; } = true;
    public bool AlphaToolingOnly { get; init; } = true;
    public bool NoRawGeodata { get; init; } = true;
    public bool NoAbsolutePaths { get; init; } = true;
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public bool NoProviderOrNetworkMarkers { get; init; } = true;
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsFinalGameplay { get; init; }
    public bool ContainsRealGeodataFetch { get; init; }
    public string AlphaRuntimeBootstrapHash { get; init; } = string.Empty;
    public int AlphaRuntimeBootstrapLineCount { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public IReadOnlyList<string> NotFinalWarnings { get; init; } =
    [
        "Manual gate offline_geoworld_alpha_slice_orchestrator_verification remains required.",
        "Offline geoworld Alpha Slice is one-click Alpha tooling, not final Runtime or release build.",
        "Real geodata, providers, final art, scene/prefab changes and public schema changes remain separate future gates."
    ];
}

public sealed record OfflineGeoworldAlphaSliceComponentsDocument
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int ComponentCount { get; init; }
    public int ReadyComponentCount { get; init; }
    public int MissingComponentCount { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaSliceComponent> Components { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceComponent
{
    public string ComponentId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string SourceGoalId { get; init; } = string.Empty;
    public string SourceArtifactRoot { get; init; } = string.Empty;
    public string StreamingAssetsRoot { get; init; } = string.Empty;
    public string ManualGate { get; init; } = string.Empty;
    public string ImplementationStatus { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public bool Ready { get; init; }
    public bool RequiredArtifactFilesPresent { get; init; }
    public bool QualityGatePassed { get; init; }
    public bool UnityScriptsReady { get; init; }
    public IReadOnlyList<string> UnityPayloadPaths { get; init; } = [];
    public IReadOnlyList<string> UnityScriptPaths { get; init; } = [];
    public IReadOnlyDictionary<string, string> SourceArtifactHashes { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public string AggregateHash { get; init; } = string.Empty;
    public int ObjectiveCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string FinalStateHash { get; init; } = string.Empty;
    public string FinalAcceptanceHash { get; init; } = string.Empty;
    public IReadOnlyList<string> NotFinalWarnings { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceAcceptanceRunbook
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldAlphaSliceVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public int StepCount { get; init; }
    public IReadOnlyList<string> Steps { get; init; } = [];
    public IReadOnlyList<string> ComponentIds { get; init; } = [];
    public IReadOnlyList<string> NotFinalWarnings { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceReadinessMatrix
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaSliceReadinessRow> Rows { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceReadinessRow
{
    public string ComponentId { get; init; } = string.Empty;
    public bool Ready { get; init; }
    public bool ArtifactFilesPresent { get; init; }
    public bool QualityGatePassed { get; init; }
    public bool AcceptedFalse { get; init; }
    public int UnityPayloadFileCount { get; init; }
    public bool UnityScriptsReady { get; init; }
    public string ManualGate { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceReadme
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public bool Accepted { get; init; }
    public string Summary { get; init; } = string.Empty;
    public int ComponentCount { get; init; }
    public int ReadyComponentCount { get; init; }
    public string ManualGate { get; init; } = string.Empty;
    public string StreamingAssetsRoot { get; init; } = string.Empty;
    public IReadOnlyList<string> NotFinalWarnings { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceSimulatedProof
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool SourceGoal101To107PayloadsRead { get; init; }
    public bool SetupPreviewPassed { get; init; }
    public bool TravelPassed { get; init; }
    public bool InteractionPassed { get; init; }
    public bool SavePassed { get; init; }
    public bool LoadPassed { get; init; }
    public bool ReplayPassed { get; init; }
    public bool CompleteObjectivesPassed { get; init; }
    public bool FinalHashPropagationPassed { get; init; }
    public bool HistoricalArtifactsUnchanged { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoNetworkProviderMarkers { get; init; }
    public IReadOnlyList<string> Sequence { get; init; } = [];
    public string FinalStateHash { get; init; } = string.Empty;
    public string FinalAcceptanceHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceNegativeProof
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public IReadOnlyList<OfflineGeoworldAlphaSliceNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = string.Empty;
    public string Diagnostic { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceUnityScriptInventory
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool CoordinatorExists { get; init; }
    public string CoordinatorRelativePath { get; init; } = string.Empty;
    public bool ReadsApplicationStreamingAssetsPath { get; init; }
    public bool ReadsGoal108Root { get; init; }
    public bool FindsGoal101To107Controllers { get; init; }
    public bool RefreshStatusMethodPresent { get; init; }
    public bool VerifySliceMethodPresent { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
    public int LineCount { get; init; }
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceEditorWindowInventory
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool EditorWindowExists { get; init; }
    public string EditorWindowRelativePath { get; init; } = string.Empty;
    public bool MenuItemMarkerPresent { get; init; }
    public bool ReadsManifestBeforeSetup { get; init; }
    public bool CreateRigMethodPresent { get; init; }
    public bool ClearRigMethodPresent { get; init; }
    public bool VerifyMethodPresent { get; init; }
    public bool ManualButtonOnly { get; init; }
    public bool HasNoAutoRunImportMarker { get; init; }
    public bool HasNoProviderNetworkMarkers { get; init; }
    public bool HasNoExternalDependencyMarkers { get; init; }
    public int LineCount { get; init; }
    public string Sha256 { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceWorkspaceBindingInventory
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool WorkspaceGroupPresent { get; init; }
    public bool ProofStatusPresent { get; init; }
    public bool PageBindDisplaysAlphaSlice { get; init; }
    public string PageRelativePath { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldAlphaSliceQualityGateScan
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldAlphaSliceVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int ComponentCount { get; init; }
    public int ReadyComponentCount { get; init; }
    public bool AllSevenComponentsRepresented { get; init; }
    public bool AllComponentsReady { get; init; }
    public int ObjectiveCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public bool FinalStatusCompleted { get; init; }
    public bool UnityScriptInventoryPassed { get; init; }
    public bool EditorWindowInventoryPassed { get; init; }
    public bool SimulatedProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool HistoricalArtifactsUnchanged { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoNetworkProviderMarkers { get; init; }
    public bool NoAlphaRuntimeBootstrapDependency { get; init; }
    public bool NoScenePrefabSettingsProjectPackageMutation { get; init; }
    public bool NoExternalDependencyOrNewInputSystemMarkers { get; init; }
    public bool SourceHealthLimitsPassed { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public string PayloadAggregateHash { get; init; } = string.Empty;
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldAlphaSliceReport
{
    public string GoalId { get; init; } = OfflineGeoworldAlphaSliceVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldAlphaSliceVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int ComponentCount { get; init; }
    public int ReadyComponentCount { get; init; }
    public int ObjectiveCount { get; init; }
    public int CompletedObjectiveCount { get; init; }
    public string FinalStatus { get; init; } = string.Empty;
    public string FinalAcceptanceHash { get; init; } = string.Empty;
    public bool UnityScriptInventoryPassed { get; init; }
    public bool EditorWindowInventoryPassed { get; init; }
    public bool SimulatedProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public int NegativeRejectedCount { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool HistoricalArtifactsUnchanged { get; init; }
    public string DeterministicReportHash { get; init; } = string.Empty;
}
