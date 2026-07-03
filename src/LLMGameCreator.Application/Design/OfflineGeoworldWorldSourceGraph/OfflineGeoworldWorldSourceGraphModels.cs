namespace LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

public static class OfflineGeoworldWorldSourceGraphVocabulary
{
    public const string GoalId = "goal_099_offline_geoworld_worldsourcegraph_streaming";
    public const string ProductSmokeRoute = "goal-099-offline-geoworld-worldsourcegraph-streaming";
    public const string FinalGate = "offline_geoworld_worldsourcegraph_streaming_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming";

    public const string BundleCatalogSchemaVersion = "offline_geoworld_bundle_catalog_v1";
    public const string NormalizedFeaturesSchemaVersion = "offline_geoworld_normalized_features_v1";
    public const string WorldSourceGraphSchemaVersion = "offline_geoworld_worldsourcegraph_v1";
    public const string StreamWindowPlanSchemaVersion = "offline_geoworld_stream_window_plan_v1";
    public const string BoundaryPrefetchProofSchemaVersion = "offline_geoworld_boundary_prefetch_proof_v1";
    public const string VisualProjectionSchemaVersion = "offline_geoworld_visual_projection_summary_v1";
    public const string NegativeProofSchemaVersion = "offline_geoworld_negative_proof_v1";
    public const string WorkspaceBindingSchemaVersion = "offline_geoworld_workspace_binding_inventory_v1";
    public const string SourceLineageSchemaVersion = "offline_geoworld_source_lineage_v1";
    public const string QualityGateSchemaVersion = "offline_geoworld_quality_gate_scan_v1";
}

public enum OfflineGeoFeatureKind
{
    Unknown = 0,
    Building,
    Road,
    Water,
    LandUse,
    Poi,
    Bridge,
    Barrier,
    Vegetation,
    TerrainHint,
    AdministrativeArea
}

public enum OfflineGeoGeometryKind
{
    Unknown = 0,
    Point,
    LineString,
    Polygon,
    AreaHint
}

public sealed record OfflineGeoTileKey
{
    public string Scheme { get; init; } = "syntheticWebMercator";
    public int Zoom { get; init; } = 14;
    public int X { get; init; }
    public int Y { get; init; }
    public string Key { get; init; } = string.Empty;

    public static OfflineGeoTileKey Create(int x, int y) =>
        new() { X = x, Y = y, Key = $"z14/x{x}/y{y}" };
}

public sealed record RawGeoFeatureDescriptor
{
    public string RawDescriptorId { get; init; } = string.Empty;
    public OfflineGeoFeatureKind NormalizedKind { get; init; }
    public OfflineGeoGeometryKind GeometryKind { get; init; }
    public string GeometrySummary { get; init; } = string.Empty;
    public string SourceTagFamily { get; init; } = string.Empty;
    public IReadOnlyList<string> RawTagKeys { get; init; } = [];
    public IReadOnlyList<string> IntersectingChunkKeys { get; init; } = [];
    public bool CrossesChunkBoundary { get; init; }
    public bool ConsumedDirectlyByGameplay { get; init; }
    public bool PreservedAsRawPayload { get; init; }
    public string SourceLineage { get; init; } = string.Empty;
    public string LicenseProvenanceSummary { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldBundle
{
    public string BundleId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public bool MetadataOnly { get; init; } = true;
    public bool SyntheticOnly { get; init; } = true;
    public bool ContainsRealMapData { get; init; }
    public bool ContainsRawOsmDump { get; init; }
    public bool ContainsRawFullAreaDump { get; init; }
    public bool PublicTileScrapingAttempted { get; init; }
    public bool RuntimeOnlineFetchAttempted { get; init; }
    public bool ContainsLfzCopiedCodeMarker { get; init; }
    public bool PromptTextIsSourceOfTruth { get; init; }
    public bool RealGeodataDumpMarkerPresent { get; init; }
    public bool ContainsAdultOrRatingMetadata { get; init; }
    public string SafeFallbackPolicyId { get; init; } = "safe_public_geoworld_fallback";
    public string SourceLineage { get; init; } = string.Empty;
    public string LicenseProvenanceSummary { get; init; } = string.Empty;
    public IReadOnlyList<RawGeoFeatureDescriptor> RawDescriptors { get; init; } = [];
}

public sealed record OfflineGeoworldBundleCatalog
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldWorldSourceGraphVocabulary.BundleCatalogSchemaVersion;

    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int BundleCount { get; init; }
    public IReadOnlyList<string> BundleIds { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldBundle> Bundles { get; init; } = [];
}

public sealed record NormalizedGeoFeature
{
    public string FeatureId { get; init; } = string.Empty;
    public OfflineGeoFeatureKind Kind { get; init; }
    public string SourceRawDescriptorId { get; init; } = string.Empty;
    public string NormalizedGeometrySummary { get; init; } = string.Empty;
    public string SourceLineage { get; init; } = string.Empty;
    public string LicenseProvenanceSummary { get; init; } = string.Empty;
    public bool GameplaySafe { get; init; }
    public bool ContainsRawSourceTags { get; init; }
    public string RawTagSummary { get; init; } = string.Empty;
    public IReadOnlyList<string> ChunkKeys { get; init; } = [];
    public bool CrossesChunkBoundary { get; init; }
    public IReadOnlyList<string> CrossChunkReferenceIds { get; init; } = [];
}

public sealed record OfflineGeoworldNormalizedFeatureSet
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldWorldSourceGraphVocabulary.NormalizedFeaturesSchemaVersion;

    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public string BundleId { get; init; } = string.Empty;
    public int FeatureCount { get; init; }
    public bool GameplaySafeOnlyAfterNormalization { get; init; }
    public bool RawTagsMappedNotPassedDirectly { get; init; }
    public IReadOnlyList<string> FeatureKindsCovered { get; init; } = [];
    public IReadOnlyList<NormalizedGeoFeature> Features { get; init; } = [];
}

public sealed record WorldSourceGraphCrossChunkReference
{
    public string ReferenceId { get; init; } = string.Empty;
    public string FeatureId { get; init; } = string.Empty;
    public OfflineGeoFeatureKind FeatureKind { get; init; }
    public string FromChunkId { get; init; } = string.Empty;
    public string ToChunkId { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public sealed record WorldSourceGraphChunk
{
    public string ChunkId { get; init; } = string.Empty;
    public OfflineGeoTileKey TileKey { get; init; } = new();
    public IReadOnlyList<string> FeatureIds { get; init; } = [];
    public IReadOnlyList<string> CrossChunkReferenceIds { get; init; } = [];
    public string SourceProvenance { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldWorldSourceGraph
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldWorldSourceGraphVocabulary.WorldSourceGraphSchemaVersion;

    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public string GraphId { get; init; } = string.Empty;
    public string BundleId { get; init; } = string.Empty;
    public bool BaseDataImmutable { get; init; } = true;
    public bool GameplayDeltasSeparate { get; init; } = true;
    public int DeltaCount { get; init; }
    public bool NoRawFullAreaDump { get; init; } = true;
    public string SourceProvenance { get; init; } = string.Empty;
    public IReadOnlyList<WorldSourceGraphChunk> Chunks { get; init; } = [];
    public IReadOnlyList<WorldSourceGraphCrossChunkReference> CrossChunkReferences { get; init; } = [];
}

public sealed record OfflineGeoworldStreamWindowRequest
{
    public string RequestId { get; init; } = "stream_window/synthetic_city_radius";
    public string CenterChunkKey { get; init; } = string.Empty;
    public int RadiusChunks { get; init; } = 1;
    public int BoundaryPrefetchBandChunks { get; init; } = 1;
    public bool RuntimeTravelModeRequested { get; init; } = true;
    public bool BoundaryPrefetchEnabled { get; init; } = true;
}

public sealed record OfflineGeoworldChunkCacheState
{
    public string ChunkKey { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public bool LoadedFromOfflineBundle { get; init; }
    public bool ScheduledNoNetwork { get; init; }
}

public sealed record OfflineGeoworldStreamWindowPlan
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldWorldSourceGraphVocabulary.StreamWindowPlanSchemaVersion;

    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public OfflineGeoworldStreamWindowRequest Request { get; init; } = new();
    public IReadOnlyList<string> RequiredChunkKeys { get; init; } = [];
    public IReadOnlyList<string> BoundaryPrefetchChunkKeys { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldChunkCacheState> CacheStates { get; init; } = [];
    public IReadOnlyList<string> MissingScheduledChunkKeys { get; init; } = [];
    public bool NetworkFetchAttempted { get; init; }
    public string BoundaryPrefetchStatus { get; init; } = string.Empty;
    public string CacheStateSummary { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldBoundaryPrefetchProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldWorldSourceGraphVocabulary.BoundaryPrefetchProofSchemaVersion;

    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public bool Passed { get; init; }
    public string CenterChunkKey { get; init; } = string.Empty;
    public int RequiredChunkCount { get; init; }
    public int BoundaryPrefetchChunkCount { get; init; }
    public int MissingScheduledChunkCount { get; init; }
    public bool BoundaryPrefetchEnabled { get; init; }
    public bool RuntimeTravelModeRequested { get; init; }
    public bool NoNetworkFetch { get; init; }
    public string DiagnosticSummary { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldChunkProjectionSummary
{
    public string ChunkKey { get; init; } = string.Empty;
    public int FeatureCount { get; init; }
    public bool HasBuildings { get; init; }
    public bool HasRoads { get; init; }
    public bool HasWater { get; init; }
    public bool HasPoi { get; init; }
    public bool HasBridge { get; init; }
    public bool HasBarrier { get; init; }
    public bool HasVegetation { get; init; }
}

public sealed record OfflineGeoworldVisualProjectionSummary
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldWorldSourceGraphVocabulary.VisualProjectionSchemaVersion;

    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool NoRasterImages { get; init; } = true;
    public bool NoUnityOutput { get; init; } = true;
    public string OverviewSvgRelativePath { get; init; } = "overviews/synthetic_city_radius_stream_window.svg";
    public string CompactOverviewEntry { get; init; } = string.Empty;
    public IReadOnlyList<OfflineGeoworldChunkProjectionSummary> Chunks { get; init; } = [];
}

public sealed record OfflineGeoworldDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static OfflineGeoworldDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record OfflineGeoworldValidationResult
{
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<OfflineGeoworldDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<OfflineGeoworldDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldNegativeProof
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldWorldSourceGraphVocabulary.NegativeProofSchemaVersion;

    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<OfflineGeoworldNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record OfflineGeoworldWorkspaceBindingInventory
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldWorldSourceGraphVocabulary.WorkspaceBindingSchemaVersion;

    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool WorkspaceServiceReadsGoal099Evidence { get; init; }
    public bool WorkspaceCatalogIncludesGeoworldGroup { get; init; }
    public bool WinFormsPageDisplaysGeoworldFields { get; init; }
    public bool UsesRepositoryRelativeGoal099Paths { get; init; }
    public IReadOnlyList<OfflineGeoworldDiagnostic> Diagnostics { get; init; } = [];
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
        OfflineGeoworldWorldSourceGraphVocabulary.SourceLineageSchemaVersion;

    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal098AcceptedFalsePreserved { get; init; }
    public bool Goal098NoLfzCodeCopiedProven { get; init; }
    public bool Goal098NoNetworkImplementationProven { get; init; }
    public bool Goal098TaxonomyExists { get; init; }
    public IReadOnlyList<OfflineGeoworldSourceLineageRecord> Records { get; init; } = [];
}

public sealed record OfflineGeoworldQualityGateScan
{
    public string SchemaVersion { get; init; } =
        OfflineGeoworldWorldSourceGraphVocabulary.QualityGateSchemaVersion;

    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool OfflineSyntheticBundleOnly { get; init; }
    public bool ValidBundleNormalizes { get; init; }
    public bool WorldSourceGraphBuilds { get; init; }
    public bool StreamWindowAndBoundaryPrefetchPass { get; init; }
    public bool VisualProjectionPasses { get; init; }
    public bool WorkspaceBindingInventoryPasses { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool NoNetworkOrProviderImplementation { get; init; }
    public bool NoLfzCodeCopied { get; init; }
    public bool NoRuntimeUnitySchemaChanges { get; init; } = true;
    public bool NoRawGeodataDump { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public int NormalizedFeatureCount { get; init; }
    public int WorldSourceGraphChunkCount { get; init; }
    public int StreamWindowChunkCount { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<OfflineGeoworldDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record OfflineGeoworldReport
{
    public string GoalId { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.GoalId;
    public string ManualGate { get; init; } = OfflineGeoworldWorldSourceGraphVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int BundleCount { get; init; }
    public string OfflineBundleId { get; init; } = string.Empty;
    public int RawDescriptorCount { get; init; }
    public int NormalizedFeatureCount { get; init; }
    public int WorldSourceGraphChunkCount { get; init; }
    public int StreamWindowChunkCount { get; init; }
    public int BoundaryPrefetchChunkCount { get; init; }
    public bool BoundaryPrefetchPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool WorkspaceBindingPassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public string BundleCatalogHash { get; init; } = string.Empty;
    public string NormalizedFeaturesHash { get; init; } = string.Empty;
    public string WorldSourceGraphHash { get; init; } = string.Empty;
    public string StreamWindowPlanHash { get; init; } = string.Empty;
    public string BoundaryPrefetchProofHash { get; init; } = string.Empty;
    public string VisualProjectionSummaryHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string WorkspaceBindingInventoryHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldBuildResult
{
    public OfflineGeoworldBundleCatalog BundleCatalog { get; init; } = new();
    public OfflineGeoworldNormalizedFeatureSet NormalizedFeatures { get; init; } = new();
    public OfflineGeoworldWorldSourceGraph WorldSourceGraph { get; init; } = new();
    public OfflineGeoworldStreamWindowPlan StreamWindowPlan { get; init; } = new();
    public OfflineGeoworldBoundaryPrefetchProof BoundaryPrefetchProof { get; init; } = new();
    public OfflineGeoworldVisualProjectionSummary VisualProjectionSummary { get; init; } = new();
    public OfflineGeoworldNegativeProof NegativeProof { get; init; } = new();
    public OfflineGeoworldWorkspaceBindingInventory WorkspaceBindingInventory { get; init; } = new();
    public OfflineGeoworldSourceLineage SourceLineage { get; init; } = new();
    public OfflineGeoworldQualityGateScan QualityGateScan { get; init; } = new();
    public OfflineGeoworldReport Report { get; init; } = new();
    public string OverviewSvgText { get; init; } = string.Empty;
    public string BundleCatalogJson { get; init; } = string.Empty;
    public string NormalizedFeaturesJson { get; init; } = string.Empty;
    public string WorldSourceGraphJson { get; init; } = string.Empty;
    public string StreamWindowPlanJson { get; init; } = string.Empty;
    public string BoundaryPrefetchProofJson { get; init; } = string.Empty;
    public string VisualProjectionSummaryJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string WorkspaceBindingInventoryJson { get; init; } = string.Empty;
    public string SourceLineageJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record OfflineGeoworldWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string BundleCatalogJsonPath { get; init; } = string.Empty;
    public string NormalizedFeaturesJsonPath { get; init; } = string.Empty;
    public string WorldSourceGraphJsonPath { get; init; } = string.Empty;
    public string StreamWindowPlanJsonPath { get; init; } = string.Empty;
    public string BoundaryPrefetchProofJsonPath { get; init; } = string.Empty;
    public string VisualProjectionSummaryJsonPath { get; init; } = string.Empty;
    public string NegativeProofJsonPath { get; init; } = string.Empty;
    public string WorkspaceBindingInventoryJsonPath { get; init; } = string.Empty;
    public string SourceLineageJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
    public string OverviewSvgPath { get; init; } = string.Empty;
    public OfflineGeoworldBuildResult Result { get; init; } = new();
}
