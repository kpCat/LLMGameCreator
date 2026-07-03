namespace LLMGameCreator.Application.Design.DeterministicVisualChunkStreamWindow;

public static class DeterministicVisualChunkStreamWindowVocabulary
{
    public const string GoalId = "goal_091_deterministic_visual_chunk_stream_window";
    public const string ProductSmokeRoute = "goal-091-deterministic-visual-chunk-stream-window";
    public const string FinalGate = "deterministic_visual_chunk_stream_window_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window";
    public const string StreamOverviewRelativeDirectory = "stream-overviews";

    public const string CatalogSchemaVersion = "visual_chunk_stream_window_catalog_v1";
    public const string MaterializationManifestSchemaVersion = "visual_chunk_stream_materialization_manifest_v1";
    public const string FileLedgerSchemaVersion = "visual_chunk_stream_file_ledger_v1";
    public const string DeterminismProofSchemaVersion = "visual_chunk_stream_determinism_proof_v1";
    public const string SeamProofSchemaVersion = "visual_chunk_stream_seam_proof_v1";
    public const string CacheReuseProofSchemaVersion = "visual_chunk_stream_cache_reuse_proof_v1";
    public const string LayerTransitionProofSchemaVersion = "visual_chunk_stream_layer_transition_proof_v1";
    public const string NegativeProofSchemaVersion = "visual_chunk_stream_negative_proof_v1";
    public const string SourceLineageSchemaVersion = "visual_chunk_stream_source_lineage_v1";
    public const string QualityGateSchemaVersion = "visual_chunk_stream_quality_gate_scan_v1";

    public const string DeterministicChunkKeyFormula = "sha256(profileId|worldSeed|generatorVersion|layerId|chunkX|chunkY)";
    public const int MaximumRadiusChunks = 8;
    public const int RawDumpChunkThreshold = 128;
}

public enum VisualChunkStreamWorldMode
{
    Finite = 0,
    HugeSparseFinite,
    Infinite
}

public enum VisualChunkStreamBoundaryPolicy
{
    ClipToFiniteBounds = 0,
    UnboundedInfinite
}

public sealed record VisualChunkStreamRequest
{
    public string FixtureId { get; init; } = string.Empty;
    public string WindowId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public IReadOnlyList<string> LayerIds { get; init; } = [];
    public string WorldSeed { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public VisualChunkStreamWorldMode Mode { get; init; }
    public long CenterChunkX { get; init; }
    public long CenterChunkY { get; init; }
    public int RadiusChunks { get; init; }
    public VisualChunkStreamBoundaryPolicy BoundaryPolicy { get; init; } = VisualChunkStreamBoundaryPolicy.ClipToFiniteBounds;
    public int? FiniteWidthOverride { get; init; }
    public int? FiniteHeightOverride { get; init; }
    public string FiniteSizeId { get; init; } = string.Empty;
    public string CachePolicy { get; init; } = "deterministic_chunk_key_reuse";
    public bool AttemptsRawFullWorldDump { get; init; }
    public bool PromptTextIsSourceOfTruth { get; init; }
    public string SourceOfTruthKind { get; init; } = "metadata_contract";
    public bool ContainsAbsolutePath { get; init; }
    public bool ContainsAdultRatingMetadata { get; init; }
    public string SafeFallbackRefId { get; init; } = string.Empty;
    public VisualChunkStreamDeltaOverlay? DeltaOverlay { get; init; }
}

public sealed record VisualChunkStreamWindow
{
    public string FixtureId { get; init; } = string.Empty;
    public string WindowId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public VisualChunkStreamWorldMode Mode { get; init; }
    public string LayerId { get; init; } = string.Empty;
    public IReadOnlyList<string> LayerIds { get; init; } = [];
    public string WorldSeed { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public long CenterChunkX { get; init; }
    public long CenterChunkY { get; init; }
    public int RadiusChunks { get; init; }
    public VisualChunkStreamBoundaryPolicy BoundaryPolicy { get; init; }
    public long RequestedMinChunkX { get; init; }
    public long RequestedMinChunkY { get; init; }
    public long RequestedMaxChunkX { get; init; }
    public long RequestedMaxChunkY { get; init; }
    public long MaterializedMinChunkX { get; init; }
    public long MaterializedMinChunkY { get; init; }
    public long MaterializedMaxChunkX { get; init; }
    public long MaterializedMaxChunkY { get; init; }
    public bool ClippedAtFiniteBoundary { get; init; }
    public int? EffectiveFiniteWidth { get; init; }
    public int? EffectiveFiniteHeight { get; init; }
    public long? EffectiveChunkColumns { get; init; }
    public long? EffectiveChunkRows { get; init; }
    public long? EstimatedFullWorldChunkCapacity { get; init; }
    public int ChunkCount { get; init; }
    public bool NoRawFullWorldDump { get; init; } = true;
    public bool AttemptsRawFullWorldDump { get; init; }
    public bool PromptTextIsSourceOfTruth { get; init; }
    public string SourceOfTruthKind { get; init; } = "metadata_contract";
    public bool ContainsAbsolutePath { get; init; }
    public bool ContainsAdultRatingMetadata { get; init; }
    public string SafeFallbackRefId { get; init; } = string.Empty;
    public IReadOnlyList<VisualChunkStreamLayerRef> Layers { get; init; } = [];
    public IReadOnlyList<VisualChunkStreamChunkRef> Chunks { get; init; } = [];
    public IReadOnlyList<VisualChunkStreamSeam> Seams { get; init; } = [];
    public IReadOnlyList<VisualChunkStreamLayerPortalRef> LayerLinks { get; init; } = [];
    public IReadOnlyList<VisualChunkStreamDeltaOverlay> DeltaOverlays { get; init; } = [];
    public string WindowHash { get; init; } = string.Empty;
}

public sealed record VisualChunkStreamLayerRef
{
    public string LayerId { get; init; } = string.Empty;
    public string LayerKind { get; init; } = string.Empty;
    public int Order { get; init; }
    public string MaterializationRole { get; init; } = string.Empty;
    public string SafeFallbackRefId { get; init; } = string.Empty;
    public IReadOnlyList<string> LinkedLayerIds { get; init; } = [];
}

public sealed record VisualChunkStreamLayerPortalRef
{
    public string LinkId { get; init; } = string.Empty;
    public string FromLayerId { get; init; } = string.Empty;
    public string ToLayerId { get; init; } = string.Empty;
    public string LinkKind { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

public sealed record VisualChunkStreamChunkRef
{
    public string WindowId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public long ChunkX { get; init; }
    public long ChunkY { get; init; }
    public string ChunkKey { get; init; } = string.Empty;
    public string DeterministicChunkHash { get; init; } = string.Empty;
    public VisualChunkStreamNeighborSeamKeys NeighborSeamKeys { get; init; } = new();
    public string WaterContinuitySummary { get; init; } = string.Empty;
    public string RoadContinuitySummary { get; init; } = string.Empty;
    public string BiomeContinuitySummary { get; init; } = string.Empty;
}

public sealed record VisualChunkStreamNeighborSeamKeys
{
    public string North { get; init; } = string.Empty;
    public string South { get; init; } = string.Empty;
    public string East { get; init; } = string.Empty;
    public string West { get; init; } = string.Empty;
}

public sealed record VisualChunkStreamSeam
{
    public string WindowId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string FromChunkKey { get; init; } = string.Empty;
    public string ToChunkKey { get; init; } = string.Empty;
    public long FromChunkX { get; init; }
    public long FromChunkY { get; init; }
    public long ToChunkX { get; init; }
    public long ToChunkY { get; init; }
    public string SeamKey { get; init; } = string.Empty;
    public string WaterConnector { get; init; } = string.Empty;
    public string RoadConnector { get; init; } = string.Empty;
    public string BiomeBand { get; init; } = string.Empty;
    public bool WaterContinuityPassed { get; init; }
    public bool RoadContinuityPassed { get; init; }
    public bool BiomeContinuityPassed { get; init; }
}

public sealed record VisualChunkStreamCacheRecord
{
    public string ChunkKey { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public long ChunkX { get; init; }
    public long ChunkY { get; init; }
    public string FirstWindowId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequestedByWindowIds { get; init; } = [];
    public IReadOnlyList<string> ReusedInWindowIds { get; init; } = [];
    public int RequestCount { get; init; }
    public int MaterializationCount { get; init; }
    public bool Reused { get; init; }
    public string CachePolicy { get; init; } = string.Empty;
}

public sealed record VisualChunkStreamDeltaOverlay
{
    public string OverlayId { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public int ChangedChunkCount { get; init; }
    public bool ContainsRawCellPayload { get; init; }
    public string StableHash { get; init; } = string.Empty;
}

public sealed record VisualChunkStreamDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static VisualChunkStreamDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record VisualChunkStreamValidationResult
{
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<VisualChunkStreamDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkStreamCatalog
{
    public string SchemaVersion { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.CatalogSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int FixtureCount { get; init; }
    public int WindowCount { get; init; }
    public IReadOnlyList<VisualChunkStreamCatalogFixture> Fixtures { get; init; } = [];
}

public sealed record VisualChunkStreamCatalogFixture
{
    public string FixtureId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public VisualChunkStreamWorldMode Mode { get; init; }
    public IReadOnlyList<string> LayerIds { get; init; } = [];
    public IReadOnlyList<string> WindowIds { get; init; } = [];
    public int WindowCount { get; init; }
    public int TotalMaterializedChunks { get; init; }
    public bool BoundaryClippingExplicit { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public string OverviewSvgRelativePath { get; init; } = string.Empty;
}

public sealed record VisualChunkStreamMaterializationManifest
{
    public string SchemaVersion { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.MaterializationManifestSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int FixtureCount { get; init; }
    public int WindowCount { get; init; }
    public int TotalMaterializedChunks { get; init; }
    public int UniqueChunkKeyCount { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public IReadOnlyList<VisualChunkStreamWindow> Windows { get; init; } = [];
}

public sealed record VisualChunkStreamFileLedger
{
    public string SchemaVersion { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.FileLedgerSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FileCount { get; init; }
    public IReadOnlyList<VisualChunkStreamFileLedgerEntry> Files { get; init; } = [];
}

public sealed record VisualChunkStreamFileLedgerEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int ByteLength { get; init; }
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record VisualChunkStreamDeterminismProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.DeterminismProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool StableChunkKeysAcrossReruns { get; init; }
    public bool StableEvidenceAcrossReruns { get; init; }
    public IReadOnlyList<VisualChunkStreamDeterminismProofRow> Rows { get; init; } = [];
}

public sealed record VisualChunkStreamDeterminismProofRow
{
    public string WindowId { get; init; } = string.Empty;
    public string FirstWindowHash { get; init; } = string.Empty;
    public string SecondWindowHash { get; init; } = string.Empty;
    public bool Stable { get; init; }
    public int ChunkCount { get; init; }
}

public sealed record VisualChunkStreamSeamProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.SeamProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public bool Passed { get; init; }
    public int SeamCount { get; init; }
    public bool WaterContinuityPassed { get; init; }
    public bool RoadContinuityPassed { get; init; }
    public bool BiomeContinuityPassed { get; init; }
    public IReadOnlyList<VisualChunkStreamSeamProofRow> Rows { get; init; } = [];
}

public sealed record VisualChunkStreamSeamProofRow
{
    public string WindowId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string SeamKey { get; init; } = string.Empty;
    public string WaterConnector { get; init; } = string.Empty;
    public string RoadConnector { get; init; } = string.Empty;
    public string BiomeBand { get; init; } = string.Empty;
}

public sealed record VisualChunkStreamCacheReuseProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.CacheReuseProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public bool Passed { get; init; }
    public int CacheRecordCount { get; init; }
    public int ReusedChunkKeyCount { get; init; }
    public int InfiniteOverlapReusedChunkKeyCount { get; init; }
    public IReadOnlyList<VisualChunkStreamCacheRecord> Records { get; init; } = [];
}

public sealed record VisualChunkStreamLayerTransitionProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.LayerTransitionProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool DataDrivenLayerLinksPassed { get; init; }
    public bool NotHardcodedSurfaceUndergroundOnly { get; init; }
    public int PortalOrTransitionLinkCount { get; init; }
    public IReadOnlyList<VisualChunkStreamLayerTransitionProofRow> Rows { get; init; } = [];
}

public sealed record VisualChunkStreamLayerTransitionProofRow
{
    public string FixtureId { get; init; } = string.Empty;
    public string WindowId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> LayerIds { get; init; } = [];
    public IReadOnlyList<VisualChunkStreamLayerPortalRef> LayerLinks { get; init; } = [];
    public bool IncludesWaterLayer { get; init; }
}

public sealed record VisualChunkStreamNegativeProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<VisualChunkStreamNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record VisualChunkStreamNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<VisualChunkStreamDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkStreamSourceLineage
{
    public string SchemaVersion { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public bool Passed { get; init; }
    public int SourceRecordCount { get; init; }
    public bool Goal090LineagePresent { get; init; }
    public IReadOnlyList<VisualChunkStreamSourceLineageRecord> Records { get; init; } = [];
}

public sealed record VisualChunkStreamSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record VisualChunkStreamQualityGateScan
{
    public string SchemaVersion { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool AllFixtureWindowsMaterialized { get; init; }
    public bool DeterminismProofPassed { get; init; }
    public bool SeamProofPassed { get; init; }
    public bool CacheReuseProofPassed { get; init; }
    public bool LayerTransitionProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool BoundaryClippingExplicit { get; init; }
    public bool HugeSparseNoRawDump { get; init; }
    public bool InfiniteOverlapReuseProven { get; init; }
    public bool SvgTextOnlyPreviews { get; init; }
    public bool NoRuntimeUnityProviderSchemaProjectDependencyChanges { get; init; } = true;
    public bool NoBinaryOrRasterMediaAdded { get; init; } = true;
    public bool NoPromptDumps { get; init; } = true;
    public bool NoExplicitAdultContent { get; init; } = true;
    public bool ArtifactScopeReady { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<VisualChunkStreamDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkStreamReport
{
    public string GoalId { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualChunkStreamWindowVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int FixtureCount { get; init; }
    public int WindowCount { get; init; }
    public int TotalMaterializedChunks { get; init; }
    public int UniqueChunkKeyCount { get; init; }
    public bool DeterminismProofPassed { get; init; }
    public bool SeamProofPassed { get; init; }
    public bool CacheReuseProofPassed { get; init; }
    public bool LayerTransitionProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public string CatalogHash { get; init; } = string.Empty;
    public string MaterializationManifestHash { get; init; } = string.Empty;
    public string FileLedgerHash { get; init; } = string.Empty;
    public string DeterminismProofHash { get; init; } = string.Empty;
    public string SeamProofHash { get; init; } = string.Empty;
    public string CacheReuseProofHash { get; init; } = string.Empty;
    public string LayerTransitionProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record VisualChunkStreamEvidenceResult
{
    public VisualChunkStreamCatalog Catalog { get; init; } = new();
    public VisualChunkStreamMaterializationManifest MaterializationManifest { get; init; } = new();
    public VisualChunkStreamFileLedger FileLedger { get; init; } = new();
    public VisualChunkStreamDeterminismProof DeterminismProof { get; init; } = new();
    public VisualChunkStreamSeamProof SeamProof { get; init; } = new();
    public VisualChunkStreamCacheReuseProof CacheReuseProof { get; init; } = new();
    public VisualChunkStreamLayerTransitionProof LayerTransitionProof { get; init; } = new();
    public VisualChunkStreamNegativeProof NegativeProof { get; init; } = new();
    public VisualChunkStreamSourceLineage SourceLineage { get; init; } = new();
    public VisualChunkStreamQualityGateScan QualityGateScan { get; init; } = new();
    public VisualChunkStreamReport Report { get; init; } = new();
    public string CatalogJson { get; init; } = string.Empty;
    public string MaterializationManifestJson { get; init; } = string.Empty;
    public string FileLedgerJson { get; init; } = string.Empty;
    public string DeterminismProofJson { get; init; } = string.Empty;
    public string SeamProofJson { get; init; } = string.Empty;
    public string CacheReuseProofJson { get; init; } = string.Empty;
    public string LayerTransitionProofJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string SourceLineageJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> OverviewSvgByFixtureId { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

public sealed record VisualChunkStreamWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamOverviewDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string CatalogJsonPath { get; init; } = string.Empty;
    public string MaterializationManifestJsonPath { get; init; } = string.Empty;
    public string FileLedgerJsonPath { get; init; } = string.Empty;
    public string DeterminismProofJsonPath { get; init; } = string.Empty;
    public string SeamProofJsonPath { get; init; } = string.Empty;
    public string CacheReuseProofJsonPath { get; init; } = string.Empty;
    public string LayerTransitionProofJsonPath { get; init; } = string.Empty;
    public string NegativeProofJsonPath { get; init; } = string.Empty;
    public string SourceLineageJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
    public IReadOnlyList<string> OverviewSvgPaths { get; init; } = [];
}
