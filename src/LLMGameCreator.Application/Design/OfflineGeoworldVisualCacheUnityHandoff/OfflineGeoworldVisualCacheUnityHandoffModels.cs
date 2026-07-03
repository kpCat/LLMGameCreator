using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace LLMGameCreator.Application.Design.OfflineGeoworldVisualCacheUnityHandoff;

public static class OfflineGeoworldVisualCacheUnityHandoffVocabulary
{
    public const string GoalId = "goal_100_offline_geoworld_visual_cache_unity_handoff";
    public const string ProductSmokeRoute = "goal-100-offline-geoworld-visual-cache-unity-handoff";
    public const string FinalGate = "offline_geoworld_visual_cache_unity_handoff_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal100";
    public const string UnityStreamingAssetsProbeRoot = "LLMGameCreator/OfflineGeoworldGoal100";
    public const string UnityProbeScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldHandoffProbe.cs";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public const string VisualCacheCatalogSchemaVersion = "offline_geoworld_visual_cache_catalog_v1";
    public const string PackageIndexSchemaVersion = "offline_geoworld_visual_cache_package_index_v1";
    public const string FeatureChunkLedgerSchemaVersion = "offline_geoworld_feature_chunk_ledger_v1";
    public const string UnityHandoffManifestSchemaVersion = "offline_geoworld_unity_handoff_manifest_v1";
    public const string StreamWindowIndexSchemaVersion = "offline_geoworld_stream_window_index_v1";
    public const string RuntimeReadmeSchemaVersion = "offline_geoworld_runtime_readme_v1";
    public const string StreamingAssetsLedgerSchemaVersion =
        "offline_geoworld_unity_streamingassets_ledger_v1";
    public const string ProbeSourceInventorySchemaVersion =
        "offline_geoworld_unity_probe_source_inventory_v1";
    public const string SimulatedReadProofSchemaVersion =
        "offline_geoworld_unity_simulated_read_proof_v1";
    public const string NegativeProofSchemaVersion = "offline_geoworld_goal100_negative_proof_v1";
    public const string WorkspaceBindingSchemaVersion =
        "offline_geoworld_goal100_workspace_binding_inventory_v1";
    public const string SourceLineageSchemaVersion = "offline_geoworld_goal100_source_lineage_v1";
    public const string QualityGateSchemaVersion = "offline_geoworld_goal100_quality_gate_scan_v1";

    public const string HandoffManifestFileName = "offline-geoworld-unity-handoff-manifest.json";
    public const string PackageIndexFileName = "offline-geoworld-package-index.json";
    public const string FeatureChunkLedgerFileName = "offline-geoworld-feature-chunk-ledger.json";
    public const string StreamWindowIndexFileName = "offline-geoworld-stream-window-index.json";
    public const string RuntimeReadmeFileName = "offline-geoworld-runtime-readme.json";

    public const string ReportMarkdownFileName =
        "offline-geoworld-visual-cache-unity-handoff-report.md";
    public const string VisualCacheCatalogFileName = "offline-geoworld-visual-cache-catalog.json";
    public const string VisualCachePackageIndexFileName =
        "offline-geoworld-visual-cache-package-index.json";
    public const string UnityStreamingAssetsLedgerFileName =
        "offline-geoworld-unity-streamingassets-ledger.json";
    public const string UnityProbeSourceInventoryFileName =
        "offline-geoworld-unity-probe-source-inventory.json";
    public const string UnitySimulatedReadProofFileName =
        "offline-geoworld-unity-simulated-read-proof.json";
    public const string NegativeProofFileName = "offline-geoworld-negative-proof.json";
    public const string WorkspaceBindingInventoryFileName =
        "offline-geoworld-workspace-binding-inventory.json";
    public const string SourceLineageFileName = "offline-geoworld-source-lineage.json";
    public const string QualityGateScanFileName = "offline-geoworld-quality-gate-scan.json";

    public static readonly IReadOnlyList<string> RequiredPayloadFileNames =
    [
        HandoffManifestFileName,
        PackageIndexFileName,
        FeatureChunkLedgerFileName,
        StreamWindowIndexFileName,
        RuntimeReadmeFileName
    ];

    public static readonly IReadOnlyList<string> RequiredEvidenceFileNames =
    [
        ReportMarkdownFileName,
        VisualCacheCatalogFileName,
        VisualCachePackageIndexFileName,
        FeatureChunkLedgerFileName,
        HandoffManifestFileName,
        UnityStreamingAssetsLedgerFileName,
        UnityProbeSourceInventoryFileName,
        UnitySimulatedReadProofFileName,
        NegativeProofFileName,
        WorkspaceBindingInventoryFileName,
        SourceLineageFileName,
        QualityGateScanFileName
    ];

    public static readonly IReadOnlyList<string> RequiredVisualFeatureKinds =
    [
        "administrativeHint",
        "barrier",
        "bridge",
        "buildingFootprint",
        "landUse",
        "poi",
        "roadSegment",
        "terrainHint",
        "vegetation",
        "waterBody"
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_goal099_world_graph",
        "unmapped_feature_kind",
        "raw_geodata_leak",
        "missing_license_provenance",
        "absolute_path",
        "live_network_fetch",
        "public_tile_scraping_marker",
        "lfz_copied_code_marker",
        "raw_full_area_or_planet_dump",
        "fake_unity_success_without_file_read",
        "missing_streamingassets_manifest",
        "tampered_manifest_hash",
        "unity_probe_provider_network_marker",
        "adult_rating_metadata_without_safe_fallback"
    ];
}

public sealed record OfflineGeoworldVisualCacheDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static OfflineGeoworldVisualCacheDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record OfflineGeoworldVisualCacheRecord
{
    public string RecordId { get; init; } = string.Empty;
    public string SourceFeatureId { get; init; } = string.Empty;
    public string SourceFeatureKind { get; init; } = string.Empty;
    public string FeatureKind { get; init; } = string.Empty;
    public string SourceChunkKey { get; init; } = string.Empty;
    public string SourceChunkId { get; init; } = string.Empty;
    public string VisualChunkKey { get; init; } = string.Empty;
    public string VisualLayerId { get; init; } = string.Empty;
    public string ProjectionStatus { get; init; } = "projected_metadata_only";
    public string CacheRecordHash { get; init; } = string.Empty;
    public string Goal098Lineage { get; init; } =
        "goal_098_geoworld_source_adapter_streaming_contract";
    public string Goal099Lineage { get; init; } =
        "goal_099_offline_geoworld_worldsourcegraph_streaming";
    public string SafeRatingMetadataStatus { get; init; } =
        "safe_public_geoworld_fallback; no_adult_rating_metadata";
    public string LicenseProvenanceSummary { get; init; } = string.Empty;
    public bool MetadataOnly { get; init; } = true;
    public bool RawGeodataIncluded { get; init; }
}

public sealed record OfflineGeoworldVisualCachePackage
{
    public string PackageId { get; init; } = string.Empty;
    public string TargetKind { get; init; } = string.Empty;
    public bool MetadataOnly { get; init; } = true;
    public bool ContainsRawGeodata { get; init; }
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public int FeatureCount { get; init; }
    public int VisualCacheRecordCount { get; init; }
    public int SourceChunkCount { get; init; }
    public int StreamWindowChunkCount { get; init; }
    public string PackageHash { get; init; } = string.Empty;
    public IReadOnlyList<string> IncludedVisualLayerIds { get; init; } = [];
}

public sealed record OfflineGeoworldVisualCacheCatalog
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.VisualCacheCatalogSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public string SourceBundleId { get; init; } = string.Empty;
    public int FeatureCount { get; init; }
    public int FeatureKindCount { get; init; }
    public int SourceChunkCount { get; init; }
    public int StreamWindowChunkCount { get; init; }
    public int VisualCacheRecordCount { get; init; }
    public int PackageCount { get; init; }
    public IReadOnlyDictionary<string, int> FeatureCountByKind { get; init; } =
        new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<OfflineGeoworldVisualCachePackage> Packages { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldVisualCacheRecord> Records { get; init; } = [];
}

public sealed record OfflineGeoworldVisualCachePackageIndex
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.PackageIndexSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int PackageCount { get; init; }
    public int FeatureCount { get; init; }
    public int VisualCacheRecordCount { get; init; }
    public IReadOnlyList<OfflineGeoworldVisualCachePackage> Packages { get; init; } = [];
}

public sealed record OfflineGeoworldFeatureChunkLedger
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.FeatureChunkLedgerSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public bool Accepted { get; init; }
    public int FeatureCount { get; init; }
    public int VisualCacheRecordCount { get; init; }
    public int SourceChunkCount { get; init; }
    public bool NoRawGeodata { get; init; } = true;
    public IReadOnlyDictionary<string, int> FeatureCountByKind { get; init; } =
        new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<OfflineGeoworldVisualCacheRecord> Records { get; init; } = [];
}

public sealed record OfflineGeoworldStreamWindowIndex
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamWindowIndexSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public bool Accepted { get; init; }
    public string CenterChunkKey { get; init; } = string.Empty;
    public int RequiredChunkCount { get; init; }
    public int BoundaryPrefetchChunkCount { get; init; }
    public string BoundaryPrefetchStatus { get; init; } = string.Empty;
    public bool NetworkFetchAttempted { get; init; }
    public IReadOnlyList<string> RequiredChunkKeys { get; init; } = [];
    public IReadOnlyList<string> BoundaryPrefetchChunkKeys { get; init; } = [];
    public IReadOnlyList<string> SourceGraphChunkKeys { get; init; } = [];
}

public sealed record OfflineGeoworldRuntimeReadme
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.RuntimeReadmeSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public bool OfflineSyntheticOnly { get; init; } = true;
    public bool MetadataOnly { get; init; } = true;
    public bool ImplementsRuntimeConsumption { get; init; }
    public bool ImplementsLiveUnityRendering { get; init; }
    public bool ImplementsNetworkProvider { get; init; }
    public bool ImplementsRawGeodataImport { get; init; }
    public bool UsesRelativePathsOnly { get; init; } = true;
}

public sealed record OfflineGeoworldUnityHandoffManifest
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.UnityHandoffManifestSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PayloadFileCount { get; init; }
    public int PackageCount { get; init; }
    public int FeatureCount { get; init; }
    public int FeatureKindCount { get; init; }
    public int VisualCacheRecordCount { get; init; }
    public int SourceChunkCount { get; init; }
    public int StreamWindowChunkCount { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        "LLMGameCreator/OfflineGeoworldGoal100";
    public bool MetadataOnly { get; init; } = true;
    public bool NoRawGeodata { get; init; } = true;
    public bool NoRawFullWorldDump { get; init; } = true;
    public bool NoAbsolutePaths { get; init; } = true;
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public bool NoProviderOrNetworkMarkers { get; init; } = true;
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsUnityGameplayImplementation { get; init; }
    public string PackageIndexHash { get; init; } = string.Empty;
    public string FeatureChunkLedgerHash { get; init; } = string.Empty;
    public string StreamWindowIndexHash { get; init; } = string.Empty;
    public string RuntimeReadmeHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldUnityStreamingAssetsLedger
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.StreamingAssetsLedgerSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        "LLMGameCreator/OfflineGeoworldGoal100";
    public int PayloadFileCount { get; init; }
    public IReadOnlyList<OfflineGeoworldUnityPayloadFile> Files { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldVisualCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnityPayloadFile
{
    public string RelativePath { get; init; } = string.Empty;
    public string RepositoryRelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int ByteCount { get; init; }
    public bool Exists { get; init; } = true;
}

public sealed record OfflineGeoworldUnityProbeSourceInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.ProbeSourceInventorySchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ProbeExists { get; init; }
    public string ProbeSha256 { get; init; } = string.Empty;
    public int ProbeLineCount { get; init; }
    public bool UsesApplicationStreamingAssetsPath { get; init; }
    public bool UsesExpectedPayloadRoot { get; init; }
    public bool ExposesInspectorResultFields { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderLlmNetworkMarkers { get; init; }
    public IReadOnlyList<OfflineGeoworldVisualCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldUnitySimulatedReadProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.SimulatedReadProofSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool ManifestRead { get; init; }
    public bool RequiredPayloadFilesPresent { get; init; }
    public bool PayloadHashesMatchManifest { get; init; }
    public bool CountsMatchVisualCacheCatalog { get; init; }
    public bool NoRawGeodata { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoProviderOrNetworkMarkers { get; init; }
    public int PayloadFileCount { get; init; }
    public int PackageCount { get; init; }
    public int FeatureCount { get; init; }
    public int FeatureKindCount { get; init; }
    public int VisualCacheRecordCount { get; init; }
    public int SourceChunkCount { get; init; }
    public int StreamWindowChunkCount { get; init; }
    public IReadOnlyList<OfflineGeoworldVisualCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<OfflineGeoworldVisualCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldNegativeProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<OfflineGeoworldNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldWorkspaceBindingInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.WorkspaceBindingSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool WorkspaceCatalogIncludesOfflineGeoworldHandoffGroup { get; init; }
    public bool WorkspaceReadsGoal100EvidenceByRelativePath { get; init; }
    public bool WinFormsPageDisplaysOfflineGeoworldHandoffFields { get; init; }
    public bool ShowsPackageCount { get; init; }
    public bool ShowsFeatureCountByKind { get; init; }
    public bool ShowsUnityPayloadCount { get; init; }
    public bool ShowsAlphaRuntimeBootstrapUnchangedStatus { get; init; }
    public IReadOnlyList<OfflineGeoworldVisualCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public string Purpose { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldSourceLineage
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal099AcceptedFalsePreserved { get; init; }
    public bool Goal099WorldSourceGraphConsumed { get; init; }
    public bool Goal099NoNetworkProviderProven { get; init; }
    public bool Goal099NoLfzCodeCopiedProven { get; init; }
    public bool ExistingVisualCacheHandoffArtifactsObserved { get; init; }
    public IReadOnlyList<OfflineGeoworldSourceLineageRecord> Records { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldVisualCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldQualityGateScan
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool VisualCacheRecordsBuilt { get; init; }
    public bool AllFeatureKindsMapped { get; init; }
    public bool PackagesCreated { get; init; }
    public bool UnityPayloadCreated { get; init; }
    public bool SimulatedReadProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string AlphaRuntimeBootstrapAfterHash { get; init; } = string.Empty;
    public int AlphaRuntimeBootstrapAfterLineCount { get; init; }
    public bool NoNetworkOrProviderImplementation { get; init; }
    public bool NoLfzCodeCopied { get; init; }
    public bool NoRawGeodataDump { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoRuntimePublicSchemaProjectDependencyChanges { get; init; } = true;
    public int FeatureCount { get; init; }
    public int PackageCount { get; init; }
    public int VisualCacheRecordCount { get; init; }
    public int SourceChunkCount { get; init; }
    public int StreamWindowChunkCount { get; init; }
    public int UnityPayloadFileCount { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldVisualCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldReport
{
    public string GoalId { get; init; } = OfflineGeoworldVisualCacheUnityHandoffVocabulary.GoalId;
    public string ManualGate { get; init; } =
        OfflineGeoworldVisualCacheUnityHandoffVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PackageCount { get; init; }
    public int FeatureCount { get; init; }
    public int FeatureKindCount { get; init; }
    public int VisualCacheRecordCount { get; init; }
    public int SourceChunkCount { get; init; }
    public int StreamWindowChunkCount { get; init; }
    public int UnityPayloadFileCount { get; init; }
    public bool SimulatedReadProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public bool QualityGatePassed { get; init; }
    public string VisualCacheCatalogHash { get; init; } = string.Empty;
    public string PackageIndexHash { get; init; } = string.Empty;
    public string FeatureChunkLedgerHash { get; init; } = string.Empty;
    public string HandoffManifestHash { get; init; } = string.Empty;
    public string StreamingAssetsLedgerHash { get; init; } = string.Empty;
    public string ProbeSourceInventoryHash { get; init; } = string.Empty;
    public string SimulatedReadProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string WorkspaceBindingInventoryHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldBuildResult
{
    public OfflineGeoworldVisualCacheCatalog VisualCacheCatalog { get; init; } = new();
    public OfflineGeoworldVisualCachePackageIndex PackageIndex { get; init; } = new();
    public OfflineGeoworldFeatureChunkLedger FeatureChunkLedger { get; init; } = new();
    public OfflineGeoworldUnityHandoffManifest HandoffManifest { get; init; } = new();
    public OfflineGeoworldStreamWindowIndex StreamWindowIndex { get; init; } = new();
    public OfflineGeoworldRuntimeReadme RuntimeReadme { get; init; } = new();
    public OfflineGeoworldUnityStreamingAssetsLedger StreamingAssetsLedger { get; init; } = new();
    public OfflineGeoworldUnityProbeSourceInventory ProbeSourceInventory { get; init; } = new();
    public OfflineGeoworldUnitySimulatedReadProof SimulatedReadProof { get; init; } = new();
    public OfflineGeoworldNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldSourceLineage SourceLineage { get; init; } = new();
    public OfflineGeoworldQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record OfflineGeoworldWriteResult
{
    public OfflineGeoworldBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal static class OfflineGeoworldVisualCacheUnityHandoffJson
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
        string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, Options);
}

internal static class OfflineGeoworldVisualCacheUnityHandoffHash
{
    public static string Sha256Text(string text) =>
        Sha256Bytes(Encoding.UTF8.GetBytes(text));

    public static string Sha256Bytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    public static string Sha256File(string path) =>
        Sha256Bytes(File.ReadAllBytes(path));
}
