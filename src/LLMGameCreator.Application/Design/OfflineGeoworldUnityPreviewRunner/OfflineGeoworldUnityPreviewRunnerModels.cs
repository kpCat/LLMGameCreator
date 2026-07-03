using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace LLMGameCreator.Application.Design.OfflineGeoworldUnityPreviewRunner;

public static class OfflineGeoworldUnityPreviewRunnerVocabulary
{
    public const string GoalId = "goal_101_offline_geoworld_unity_preview_runner";
    public const string ProductSmokeRoute = "goal-101-offline-geoworld-unity-preview-runner";
    public const string FinalGate = "offline_geoworld_unity_preview_runner_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal101";
    public const string UnityStreamingAssetsProbeRoot = "LLMGameCreator/OfflineGeoworldGoal101";
    public const string UnityPreviewRunnerScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewRunner.cs";
    public const string UnityPrimitiveFactoryScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewPrimitiveFactory.cs";
    public const string UnityTravelWindowScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldPreviewTravelWindow.cs";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string ManifestSchemaVersion =
        "offline_geoworld_preview_runner_manifest_v1";
    public const string FeatureCommandsSchemaVersion =
        "offline_geoworld_preview_feature_commands_v1";
    public const string TravelWindowSchemaVersion =
        "offline_geoworld_preview_travel_window_script_v1";
    public const string StyleLegendSchemaVersion =
        "offline_geoworld_preview_style_legend_v1";
    public const string ReadmeSchemaVersion =
        "offline_geoworld_preview_readme_v1";
    public const string StreamingAssetsLedgerSchemaVersion =
        "offline_geoworld_preview_streamingassets_ledger_v1";
    public const string UnityScriptInventorySchemaVersion =
        "offline_geoworld_preview_unity_script_inventory_v1";
    public const string SimulatedCommandProofSchemaVersion =
        "offline_geoworld_preview_simulated_command_proof_v1";
    public const string NegativeProofSchemaVersion =
        "offline_geoworld_preview_negative_proof_v1";
    public const string WorkspaceBindingSchemaVersion =
        "offline_geoworld_preview_workspace_binding_inventory_v1";
    public const string SourceLineageSchemaVersion =
        "offline_geoworld_preview_source_lineage_v1";
    public const string QualityGateSchemaVersion =
        "offline_geoworld_preview_quality_gate_scan_v1";

    public const string ManifestFileName = "offline-geoworld-preview-runner-manifest.json";
    public const string FeatureCommandsFileName =
        "offline-geoworld-preview-feature-commands.json";
    public const string TravelWindowScriptFileName =
        "offline-geoworld-preview-travel-window-script.json";
    public const string StyleLegendFileName = "offline-geoworld-preview-style-legend.json";
    public const string ReadmeFileName = "offline-geoworld-preview-readme.json";

    public const string ReportMarkdownFileName =
        "offline-geoworld-unity-preview-runner-report.md";
    public const string CommandCatalogFileName =
        "offline-geoworld-preview-command-catalog.json";
    public const string StreamingAssetsLedgerFileName =
        "offline-geoworld-preview-streamingassets-ledger.json";
    public const string UnityScriptInventoryFileName =
        "offline-geoworld-preview-unity-script-inventory.json";
    public const string SimulatedCommandProofFileName =
        "offline-geoworld-preview-simulated-command-proof.json";
    public const string NegativeProofFileName =
        "offline-geoworld-preview-negative-proof.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-preview-workspace-binding-inventory.json";
    public const string SourceLineageFileName =
        "offline-geoworld-preview-source-lineage.json";
    public const string QualityGateScanFileName =
        "offline-geoworld-preview-quality-gate-scan.json";

    public static readonly IReadOnlyList<string> RequiredPayloadFileNames =
    [
        ManifestFileName,
        FeatureCommandsFileName,
        TravelWindowScriptFileName,
        StyleLegendFileName,
        ReadmeFileName
    ];

    public static readonly IReadOnlyList<string> RequiredEvidenceFileNames =
    [
        ReportMarkdownFileName,
        CommandCatalogFileName,
        StyleLegendFileName,
        TravelWindowScriptFileName,
        StreamingAssetsLedgerFileName,
        UnityScriptInventoryFileName,
        SimulatedCommandProofFileName,
        NegativeProofFileName,
        WorkspaceBindingInventoryFileName,
        SourceLineageFileName,
        QualityGateScanFileName
    ];

    public static readonly IReadOnlyList<string> RequiredCommandKinds =
    [
        "administrative_hint_marker",
        "barrier_line",
        "bridge_marker",
        "building_footprint_marker",
        "land_use_area_plane",
        "poi_marker",
        "road_segment_line",
        "terrain_hint_marker",
        "vegetation_area_marker",
        "water_body_plane"
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_goal100_payload",
        "unsupported_feature_command_kind",
        "raw_geodata_leaked_into_command",
        "missing_style_legend",
        "missing_travel_window_script",
        "absolute_path_in_payload",
        "network_provider_marker_in_unity_script",
        "fake_success_without_file_read",
        "alpha_runtime_bootstrap_changed_marker",
        "binary_raster_media_marker",
        "rating_metadata_missing_safe_fallback"
    ];
}

public sealed record OfflineGeoworldUnityPreviewDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static OfflineGeoworldUnityPreviewDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record OfflineGeoworldPreviewFeatureCommand
{
    public string CommandId { get; init; } = string.Empty;
    public string CommandKind { get; init; } = string.Empty;
    public string SourceCacheRecordId { get; init; } = string.Empty;
    public string SourceFeatureId { get; init; } = string.Empty;
    public string SourceFeatureKind { get; init; } = string.Empty;
    public string SourceChunkKey { get; init; } = string.Empty;
    public string VisualChunkKey { get; init; } = string.Empty;
    public string VisualLayerId { get; init; } = string.Empty;
    public string StyleKey { get; init; } = string.Empty;
    public int GridX { get; init; }
    public int GridZ { get; init; }
    public int Elevation { get; init; }
    public int ExpectedObjectCount { get; init; } = 1;
    public bool MetadataOnly { get; init; } = true;
    public bool RawGeodataIncluded { get; init; }
    public string SafeRatingMetadataStatus { get; init; } =
        "safe_public_geoworld_fallback";
}

public sealed record OfflineGeoworldPreviewFeatureCommandCatalog
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.FeatureCommandsSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int CommandCount { get; init; }
    public int CommandKindCount { get; init; }
    public int ExpectedObjectCount { get; init; }
    public IReadOnlyDictionary<string, int> CommandCountByKind { get; init; } =
        new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<OfflineGeoworldPreviewFeatureCommand> Commands { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewStyleLegendEntry
{
    public string StyleKey { get; init; } = string.Empty;
    public string CommandKind { get; init; } = string.Empty;
    public string PrimitiveHint { get; init; } = string.Empty;
    public string ColorHex { get; init; } = string.Empty;
    public decimal ScaleX { get; init; }
    public decimal ScaleY { get; init; }
    public decimal ScaleZ { get; init; }
    public decimal LineWidth { get; init; }
    public bool MetadataOnly { get; init; } = true;
}

public sealed record OfflineGeoworldPreviewStyleLegend
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.StyleLegendSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int StyleCount { get; init; }
    public IReadOnlyList<OfflineGeoworldPreviewStyleLegendEntry> Styles { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewTravelWindowStep
{
    public int StepIndex { get; init; }
    public string StepId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string CenterChunkKey { get; init; } = string.Empty;
    public IReadOnlyList<string> VisibleCommandKinds { get; init; } = [];
    public IReadOnlyList<string> VisibleCommandIds { get; init; } = [];
    public bool MetadataOnly { get; init; } = true;
}

public sealed record OfflineGeoworldPreviewTravelWindowScript
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.TravelWindowSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int StepCount { get; init; }
    public int CommandCoverageCount { get; init; }
    public IReadOnlyList<OfflineGeoworldPreviewTravelWindowStep> Steps { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewReadme
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.ReadmeSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public bool OfflineSyntheticOnly { get; init; } = true;
    public bool MetadataOnly { get; init; } = true;
    public bool ImplementsRuntimeConsumption { get; init; }
    public bool ImplementsFinalArt { get; init; }
    public bool ImplementsSceneOrPrefabProduction { get; init; }
    public bool ImplementsNetworkProvider { get; init; }
    public bool UsesRelativePathsOnly { get; init; } = true;
    public string ScopeSummary { get; init; } =
        "Unity Alpha preview runner reads metadata commands and creates placeholder objects only.";
}

public sealed record OfflineGeoworldPreviewRunnerManifest
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.ManifestSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PayloadFileCount { get; init; }
    public int CommandCount { get; init; }
    public int CommandKindCount { get; init; }
    public int TravelWindowStepCount { get; init; }
    public int StyleCount { get; init; }
    public int ExpectedObjectCount { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        "LLMGameCreator/OfflineGeoworldGoal101";
    public bool MetadataOnly { get; init; } = true;
    public bool NoRawGeodata { get; init; } = true;
    public bool NoAbsolutePaths { get; init; } = true;
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public bool NoProviderOrNetworkMarkers { get; init; } = true;
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsFinalArt { get; init; }
    public string FeatureCommandsHash { get; init; } = string.Empty;
    public string TravelWindowScriptHash { get; init; } = string.Empty;
    public string StyleLegendHash { get; init; } = string.Empty;
    public string ReadmeHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldPreviewStreamingAssetsLedger
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.StreamingAssetsLedgerSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public bool Passed { get; init; }
    public int PayloadFileCount { get; init; }
    public IReadOnlyList<OfflineGeoworldPreviewPayloadFile> Files { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldUnityPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewPayloadFile
{
    public string RelativePath { get; init; } = string.Empty;
    public string RepositoryRelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int ByteCount { get; init; }
    public bool Exists { get; init; }
}

public sealed record OfflineGeoworldPreviewUnityScriptFile
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public bool HasNoProviderLlmNetworkMarkers { get; init; }
}

public sealed record OfflineGeoworldPreviewUnityScriptInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.UnityScriptInventorySchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool RunnerExists { get; init; }
    public bool FactoryExists { get; init; }
    public bool TravelWindowExists { get; init; }
    public bool RunnerUsesApplicationStreamingAssetsPath { get; init; }
    public bool RunnerReadsGoal101Root { get; init; }
    public bool RunnerExposesInspectorFields { get; init; }
    public bool FactoryCreatesPrimitivePlaceholders { get; init; }
    public bool TravelWindowSupportsDemoSteps { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderLlmNetworkMarkers { get; init; }
    public IReadOnlyList<OfflineGeoworldPreviewUnityScriptFile> Files { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldUnityPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewSimulatedCommandProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.SimulatedCommandProofSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool ManifestRead { get; init; }
    public bool CommandFileRead { get; init; }
    public bool StyleLegendRead { get; init; }
    public bool TravelWindowScriptRead { get; init; }
    public bool PayloadHashesMatchManifest { get; init; }
    public bool AllRequiredCommandKindsRepresented { get; init; }
    public bool NoUnsupportedCommandKind { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoProviderOrNetworkMarkers { get; init; }
    public int CommandCount { get; init; }
    public int CommandKindCount { get; init; }
    public int TravelWindowStepCount { get; init; }
    public int ExpectedObjectCount { get; init; }
    public IReadOnlyDictionary<string, int> CommandCountByKind { get; init; } =
        new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<OfflineGeoworldUnityPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<OfflineGeoworldUnityPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewNegativeProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<OfflineGeoworldPreviewNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewWorkspaceBindingInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.WorkspaceBindingSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool WorkspaceCatalogIncludesUnityPreviewGroup { get; init; }
    public bool WorkspaceReadsGoal101EvidenceByRelativePath { get; init; }
    public bool WinFormsPageDisplaysUnityPreviewFields { get; init; }
    public bool ShowsPreviewCommandCount { get; init; }
    public bool ShowsCommandKindCoverage { get; init; }
    public bool ShowsTravelWindowSteps { get; init; }
    public bool ShowsUnityScriptsReady { get; init; }
    public bool ShowsSimulatedCommandProof { get; init; }
    public bool ShowsAlphaRuntimeBootstrapUnchangedStatus { get; init; }
    public bool ShowsNegativeProofStatus { get; init; }
    public IReadOnlyList<OfflineGeoworldUnityPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldPreviewSourceLineage
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal100AcceptedFalsePreserved { get; init; }
    public bool Goal100PayloadConsumed { get; init; }
    public bool Goal100SimulatedReadProofPassed { get; init; }
    public bool Goal100AlphaRuntimeBootstrapUnchanged { get; init; }
    public IReadOnlyList<OfflineGeoworldPreviewSourceLineageRecord> Records { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldUnityPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewQualityGateScan
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool Goal100Consumed { get; init; }
    public bool PreviewCommandsBuilt { get; init; }
    public bool AllCommandKindsMapped { get; init; }
    public bool TravelWindowDemoBuilt { get; init; }
    public bool UnityPayloadCreated { get; init; }
    public bool UnityScriptsReady { get; init; }
    public bool SimulatedCommandProofPassed { get; init; }
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
    public bool NoRuntimePublicSchemaProjectDependencyChanges { get; init; } = true;
    public int CommandCount { get; init; }
    public int CommandKindCount { get; init; }
    public int TravelWindowStepCount { get; init; }
    public int UnityPayloadFileCount { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldUnityPreviewDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldPreviewReport
{
    public string GoalId { get; init; } = OfflineGeoworldUnityPreviewRunnerVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldUnityPreviewRunnerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int CommandCount { get; init; }
    public int CommandKindCount { get; init; }
    public int TravelWindowStepCount { get; init; }
    public int UnityPayloadFileCount { get; init; }
    public bool SimulatedCommandProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool UnityScriptsReady { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool QualityGatePassed { get; init; }
    public string CommandCatalogHash { get; init; } = string.Empty;
    public string StyleLegendHash { get; init; } = string.Empty;
    public string TravelWindowScriptHash { get; init; } = string.Empty;
    public string ManifestHash { get; init; } = string.Empty;
    public string StreamingAssetsLedgerHash { get; init; } = string.Empty;
    public string UnityScriptInventoryHash { get; init; } = string.Empty;
    public string SimulatedCommandProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string WorkspaceBindingInventoryHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldUnityPreviewBuildResult
{
    public OfflineGeoworldPreviewRunnerManifest Manifest { get; init; } = new();
    public OfflineGeoworldPreviewFeatureCommandCatalog CommandCatalog { get; init; } = new();
    public OfflineGeoworldPreviewStyleLegend StyleLegend { get; init; } = new();
    public OfflineGeoworldPreviewTravelWindowScript TravelWindowScript { get; init; } = new();
    public OfflineGeoworldPreviewReadme Readme { get; init; } = new();
    public OfflineGeoworldPreviewStreamingAssetsLedger StreamingAssetsLedger { get; init; } = new();
    public OfflineGeoworldPreviewUnityScriptInventory UnityScriptInventory { get; init; } = new();
    public OfflineGeoworldPreviewSimulatedCommandProof SimulatedCommandProof { get; init; } = new();
    public OfflineGeoworldPreviewNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldPreviewWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldPreviewSourceLineage SourceLineage { get; init; } = new();
    public OfflineGeoworldPreviewQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldPreviewReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldUnityPreviewWriteResult
{
    public OfflineGeoworldUnityPreviewBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal static class OfflineGeoworldUnityPreviewJson
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

internal static class OfflineGeoworldUnityPreviewHash
{
    public static string Sha256Text(string text) =>
        Sha256Bytes(Encoding.UTF8.GetBytes(text));

    public static string Sha256Bytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string Sha256File(string path) =>
        Sha256Bytes(File.ReadAllBytes(path));
}
