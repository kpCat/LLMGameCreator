namespace LLMGameCreator.Application.Design.GeoworldSourceAdapterStreamingContract;

public static class GeoworldSourceAdapterStreamingContractVocabulary
{
    public const string GoalId = "goal_098_geoworld_source_adapter_streaming_contract";
    public const string ProductSmokeRoute = "goal-098-geoworld-source-adapter-streaming-contract";
    public const string FinalGate = "geoworld_source_adapter_streaming_contract_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract";

    public const string CatalogSchemaVersion = "geoworld_source_adapter_catalog_v1";
    public const string TaxonomySchemaVersion = "geoworld_normalized_feature_taxonomy_v1";
    public const string StreamingPolicyMatrixSchemaVersion = "geoworld_streaming_policy_matrix_v1";
    public const string NegativeProofSchemaVersion = "geoworld_negative_proof_v1";
    public const string LfzPatternLineageSchemaVersion = "geoworld_lfz_pattern_lineage_v1";
    public const string QualityGateSchemaVersion = "geoworld_quality_gate_scan_v1";
}

public enum GeoSourceAdapterKind
{
    Unspecified = 0,
    OfflineOsmExtract,
    UserProvidedMapBundle,
    LicensedVectorTileAdapterSpec,
    RuntimeOnlineOptionalPolicy,
    OcrGeoreferenceFallbackFutureOnly,
    SelfGeneratedRealismWorldSource,
    OfficialApiFuturePolicy,
    PublicTileServerScrape,
    BulkPublicTileArchive
}

public enum GeoTileScheme
{
    WebMercatorGoogle = 0,
    WebMercatorTms
}

public enum GeoFeatureKind
{
    Unspecified = 0,
    Building,
    Road,
    Water,
    LandUse,
    Poi,
    Barrier,
    Bridge,
    Vegetation,
    TerrainHint,
    Transit,
    AdministrativeArea
}

public enum GeoTileCacheMode
{
    Unspecified = 0,
    ImportTimeLocalCache,
    UserProvidedBundle,
    LicensedVectorCache,
    RuntimeOnlineBlockedByDefault,
    SelfGeneratedSeedCache
}

public enum GeoNetworkIoMode
{
    None = 0,
    RuntimeOptionalBlockedByDefault,
    LiveNetworkFetch
}

public sealed record GeoCoordinate
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

public sealed record GeoBounds
{
    public GeoCoordinate SouthWest { get; init; } = new();
    public GeoCoordinate NorthEast { get; init; } = new();
}

public sealed record GeoTileKey
{
    public GeoTileScheme Scheme { get; init; } = GeoTileScheme.WebMercatorGoogle;
    public int Zoom { get; init; }
    public long X { get; init; }
    public long Y { get; init; }
}

public sealed record GeoTileGridRequest
{
    public string RequestId { get; init; } = string.Empty;
    public GeoTileKey CenterTile { get; init; } = new();
    public int RadiusTiles { get; init; }
    public int BoundaryPrefetchTiles { get; init; }
    public GeoBounds Bounds { get; init; } = new();
}

public sealed record GeoStreamWindowRequest
{
    public string WindowId { get; init; } = string.Empty;
    public GeoTileGridRequest GridRequest { get; init; } = new();
    public bool BoundaryPrefetchEnabled { get; init; }
    public string QueuePolicyId { get; init; } = string.Empty;
    public bool MaterializesOnlyRequestedWindow { get; init; } = true;
}

public sealed record GeoSourceLicensePolicy
{
    public string PolicyId { get; init; } = string.Empty;
    public string LicenseId { get; init; } = string.Empty;
    public string AttributionText { get; init; } = string.Empty;
    public string RedistributionPolicy { get; init; } = string.Empty;
    public bool AttributionRequired { get; init; } = true;
    public bool RuntimeOnlineExplicitPolicyAllowed { get; init; }
    public bool ContainsAdultOrRatingMetadata { get; init; }
    public string SafeFallbackPolicyId { get; init; } = string.Empty;
}

public sealed record GeoSourceProvenance
{
    public string ProvenanceId { get; init; } = string.Empty;
    public string SourceDocumentPath { get; init; } = string.Empty;
    public string SourceReference { get; init; } = string.Empty;
    public string ContentHash { get; init; } = string.Empty;
    public string AdapterVersion { get; init; } = string.Empty;
    public string NormalizationVersion { get; init; } = string.Empty;
    public string SourceOfTruthKind { get; init; } = "documented_metadata_contract";
    public bool PromptTextIsSourceOfTruth { get; init; }
    public bool ContainsLfzCopiedCodeMarker { get; init; }
}

public sealed record GeoTileCachePolicy
{
    public string PolicyId { get; init; } = string.Empty;
    public GeoTileCacheMode Mode { get; init; } = GeoTileCacheMode.ImportTimeLocalCache;
    public string RelativeCacheRoot { get; init; } = string.Empty;
    public bool CacheFirst { get; init; } = true;
    public bool HasEvictionPolicy { get; init; } = true;
    public bool PublicTileBulkArchiveMode { get; init; }
    public bool NoRawPublicTilePreseed { get; init; } = true;
}

public sealed record GeoFetchPlan
{
    public string PlanId { get; init; } = string.Empty;
    public GeoNetworkIoMode NetworkIoMode { get; init; } = GeoNetworkIoMode.None;
    public bool PerformsNetworkIo { get; init; }
    public bool RuntimeOnlineModeEnabled { get; init; }
    public bool RuntimeOnlinePolicyExplicitlyEnabled { get; init; }
    public bool PublicTileServerScrapeAttempted { get; init; }
    public bool BulkPublicTileArchiveMode { get; init; }
    public bool ProviderOrApiHardcodedIntoCore { get; init; }
    public bool FullPlanetRawDumpRequested { get; init; }
    public bool OcrFallbackIsPrimaryPath { get; init; }
}

public sealed record GeoFetchResult
{
    public string FixtureId { get; init; } = string.Empty;
    public bool MetadataOnly { get; init; } = true;
    public bool NetworkIoPerformed { get; init; }
    public bool RawGeodataDumpPresent { get; init; }
    public int BinaryMediaFileCount { get; init; }
    public string ResultHash { get; init; } = string.Empty;
}

public sealed record GeoFeatureRawDescriptor
{
    public string RawDescriptorId { get; init; } = string.Empty;
    public string SourceTagFamily { get; init; } = string.Empty;
    public IReadOnlyList<string> RawTagKeys { get; init; } = [];
    public GeoFeatureKind NormalizedKind { get; init; }
    public bool ConsumedDirectlyByGameplay { get; init; }
    public bool PreservedAsRawPayload { get; init; }
}

public sealed record GeoFeatureNormalized
{
    public string FeatureId { get; init; } = string.Empty;
    public GeoFeatureKind Kind { get; init; }
    public string SourceRawDescriptorId { get; init; } = string.Empty;
    public string GameSemanticFeature { get; init; } = string.Empty;
    public bool HasNeutralGeometryContract { get; init; } = true;
    public bool ContainsRawSourceTags { get; init; }
}

public sealed record WorldSourceGraph
{
    public string GraphId { get; init; } = string.Empty;
    public bool BaseDataImmutable { get; init; } = true;
    public bool GameplayDeltasSeparate { get; init; } = true;
    public bool ContractOnly { get; init; } = true;
    public bool NoFullPlanetRawDump { get; init; } = true;
    public IReadOnlyList<WorldSourceGraphChunk> Chunks { get; init; } = [];
}

public sealed record WorldSourceGraphChunk
{
    public string ChunkId { get; init; } = string.Empty;
    public GeoTileKey TileKey { get; init; } = new();
    public IReadOnlyList<string> FeatureIds { get; init; } = [];
    public bool HasBoundaryPrefetchFeatures { get; init; }
    public bool UsesRelativeRefsOnly { get; init; } = true;
}

public sealed record GeoStreamingPolicy
{
    public string PolicyId { get; init; } = string.Empty;
    public GeoStreamWindowRequest StreamWindowRequest { get; init; } = new();
    public bool BoundaryPrefetchRequired { get; init; } = true;
    public bool RuntimeOnlineBlockedByDefault { get; init; } = true;
    public bool FullPlanetRawDumpForbidden { get; init; } = true;
    public bool FutureRuntimeStreamingContractOnly { get; init; } = true;
}

public sealed record GeoSourceAdapterSpec
{
    public string SpecId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public GeoSourceAdapterKind AdapterKind { get; init; }
    public bool MetadataOnly { get; init; } = true;
    public bool OcrFallbackFutureOnly { get; init; }
    public GeoSourceLicensePolicy? LicensePolicy { get; init; }
    public GeoSourceProvenance? Provenance { get; init; }
    public GeoTileCachePolicy? CachePolicy { get; init; }
    public GeoFetchPlan FetchPlan { get; init; } = new();
    public GeoFetchResult FetchResult { get; init; } = new();
    public GeoStreamingPolicy? StreamingPolicy { get; init; }
    public IReadOnlyList<GeoFeatureRawDescriptor> RawDescriptors { get; init; } = [];
    public IReadOnlyList<GeoFeatureNormalized> NormalizedFeatures { get; init; } = [];
    public WorldSourceGraph WorldSourceGraph { get; init; } = new();
}

public sealed record GeoworldContractDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static GeoworldContractDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record GeoworldContractValidationResult
{
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<GeoworldContractDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GeoworldSourceAdapterCatalog
{
    public string SchemaVersion { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.CatalogSchemaVersion;
    public string GoalId { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.GoalId;
    public string ManualGate { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int FixtureCount { get; init; }
    public IReadOnlyList<string> FixtureIds { get; init; } = [];
    public IReadOnlyList<GeoSourceAdapterSpec> Fixtures { get; init; } = [];
}

public sealed record GeoworldNormalizedFeatureTaxonomy
{
    public string SchemaVersion { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.TaxonomySchemaVersion;
    public string GoalId { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.GoalId;
    public IReadOnlyList<GeoworldFeatureTaxonomyRow> Rows { get; init; } = [];
}

public sealed record GeoworldFeatureTaxonomyRow
{
    public GeoFeatureKind Kind { get; init; }
    public string NeutralFeatureContract { get; init; } = string.Empty;
    public string RawSourcePolicy { get; init; } = string.Empty;
    public bool GameplayConsumesNormalizedFeatureOnly { get; init; } = true;
}

public sealed record GeoworldStreamingPolicyMatrix
{
    public string SchemaVersion { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.StreamingPolicyMatrixSchemaVersion;
    public string GoalId { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<GeoworldStreamingPolicyRow> Rows { get; init; } = [];
}

public sealed record GeoworldStreamingPolicyRow
{
    public string FixtureId { get; init; } = string.Empty;
    public int RadiusTiles { get; init; }
    public int BoundaryPrefetchTiles { get; init; }
    public bool BoundaryPrefetchEnabled { get; init; }
    public bool MaterializesOnlyRequestedWindow { get; init; }
    public bool RuntimeOnlineBlockedByDefault { get; init; }
    public bool FutureRuntimeStreamingContractOnly { get; init; }
}

public sealed record GeoworldNegativeProof
{
    public string SchemaVersion { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<GeoworldNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record GeoworldNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<GeoworldContractDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GeoworldLfzPatternLineage
{
    public string SchemaVersion { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.LfzPatternLineageSchemaVersion;
    public string GoalId { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool LfzDocsConsumedAsLineage { get; init; }
    public bool LfzArchiveNotRequired { get; init; } = true;
    public bool LfzSourceCodeNotCopied { get; init; } = true;
    public IReadOnlyList<GeoworldLineageRecord> Records { get; init; } = [];
}

public sealed record GeoworldLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record GeoworldQualityGateScan
{
    public string SchemaVersion { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.GoalId;
    public string ManualGate { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool ValidFixturesPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool NormalizedTaxonomyPassed { get; init; }
    public bool RuntimeBoundaryPrefetchContractPresent { get; init; }
    public bool LfzDocsConsumedAsLineage { get; init; }
    public bool NoLfzCodeCopied { get; init; }
    public bool NoNetworkOrProviderImplementation { get; init; }
    public bool NoRuntimeUnitySchemaChanges { get; init; } = true;
    public bool FutureRuntimeStreamingContractsOnly { get; init; }
    public bool NoRawGeodataDumps { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public bool ArtifactScopeReady { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<GeoworldContractDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record GeoworldContractReport
{
    public string GoalId { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.GoalId;
    public string ManualGate { get; init; } = GeoworldSourceAdapterStreamingContractVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int FixtureCount { get; init; }
    public int TaxonomyKindCount { get; init; }
    public int NegativeScenarioCount { get; init; }
    public bool ValidFixturesPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool LfzLineagePassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public string CatalogHash { get; init; } = string.Empty;
    public string TaxonomyHash { get; init; } = string.Empty;
    public string StreamingPolicyMatrixHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string LfzPatternLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record GeoworldContractEvidenceResult
{
    public GeoworldSourceAdapterCatalog Catalog { get; init; } = new();
    public GeoworldNormalizedFeatureTaxonomy Taxonomy { get; init; } = new();
    public GeoworldStreamingPolicyMatrix StreamingPolicyMatrix { get; init; } = new();
    public GeoworldNegativeProof NegativeProof { get; init; } = new();
    public GeoworldLfzPatternLineage LfzPatternLineage { get; init; } = new();
    public GeoworldQualityGateScan QualityGateScan { get; init; } = new();
    public GeoworldContractReport Report { get; init; } = new();
    public string CatalogJson { get; init; } = string.Empty;
    public string TaxonomyJson { get; init; } = string.Empty;
    public string StreamingPolicyMatrixJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string LfzPatternLineageJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record GeoworldContractWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string CatalogJsonPath { get; init; } = string.Empty;
    public string TaxonomyJsonPath { get; init; } = string.Empty;
    public string StreamingPolicyMatrixJsonPath { get; init; } = string.Empty;
    public string NegativeProofJsonPath { get; init; } = string.Empty;
    public string LfzPatternLineageJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
}
