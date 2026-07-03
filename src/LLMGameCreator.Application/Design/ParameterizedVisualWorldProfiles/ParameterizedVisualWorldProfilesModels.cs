namespace LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;

public static class ParameterizedVisualWorldProfilesVocabulary
{
    public const string GoalId = "goal_090_parameterized_visual_world_profiles";
    public const string ProductSmokeRoute = "goal-090-parameterized-visual-world-profiles";
    public const string FinalGate = "parameterized_visual_world_profiles_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-090-parameterized-visual-world-profiles";

    public const string CatalogSchemaVersion = "visual_world_profile_catalog_v1";
    public const string SizeMatrixSchemaVersion = "visual_world_profile_size_matrix_v1";
    public const string ValidationMatrixSchemaVersion = "visual_world_profile_validation_matrix_v1";
    public const string NegativeProofSchemaVersion = "visual_world_profile_negative_proof_v1";
    public const string ChunkAddressProofSchemaVersion = "visual_world_profile_chunk_address_proof_v1";
    public const string SparseWorldProofSchemaVersion = "visual_world_profile_sparse_world_proof_v1";
    public const string LayerModelProofSchemaVersion = "visual_world_profile_layer_model_proof_v1";
    public const string SourceLineageSchemaVersion = "visual_world_profile_source_lineage_v1";
    public const string QualityGateSchemaVersion = "visual_world_profile_quality_gate_scan_v1";

    public const int MinimumFiniteDimension = 1;
    public const int MaximumFiniteDimension = 1_000_000;
    public const long RawDumpLogicalCellThreshold = 1_000_000;
    public const string DeterministicChunkKeyFormula = "sha256(profileId|worldSeed|generatorVersion|layerId|chunkX|chunkY)";
}

public enum VisualWorldProfileMode
{
    Finite = 0,
    HugeSparseFinite,
    Infinite
}

public enum VisualLayerLinkKind
{
    Portal = 0,
    Overlay,
    Transition,
    StreamHint
}

public sealed record VisualWorldProfile
{
    public string ProfileId { get; init; } = string.Empty;
    public string WorldSeed { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public string CoordinateOrigin { get; init; } = "center_zero";
    public VisualWorldProfileMode Mode { get; init; }
    public int? FiniteWidth { get; init; }
    public int? FiniteHeight { get; init; }
    public VisualVirtualWorldBounds VirtualBounds { get; init; } = new();
    public bool IsInfinite { get; init; }
    public bool IsBenchmarkProfile { get; init; }
    public string BenchmarkNote { get; init; } = string.Empty;
    public VisualDimensionRange ValidationBounds { get; init; } = VisualDimensionRange.Default();
    public VisualChunkProfile ChunkProfile { get; init; } = new();
    public VisualPatchProfile PatchProfile { get; init; } = new();
    public long? LogicalCellCount { get; init; }
    public bool RawCellDumpAllowed { get; init; }
    public bool ClaimsGenericButUsesFixedSizeAllowlist { get; init; }
    public bool RequiresSurfaceUndergroundOnly { get; init; }
    public bool PromptTextIsSourceOfTruth { get; init; }
    public string SourceOfTruthKind { get; init; } = "metadata_contract";
    public string OutputRelativeDirectory { get; init; } = ParameterizedVisualWorldProfilesVocabulary.RelativeOutputDirectory;
    public IReadOnlyList<string> FixedSizeAllowlist { get; init; } = [];
    public IReadOnlyList<VisualWorldLayerProfile> Layers { get; init; } = [];
    public IReadOnlyList<VisualRegionSize> FiniteSizeSamples { get; init; } = [];
    public VisualSparseRegionIndex SparseRegionIndex { get; init; } = new();
    public IReadOnlyList<VisualStreamWindow> StreamWindows { get; init; } = [];
    public IReadOnlyList<VisualLayerLink> LayerLinks { get; init; } = [];
    public IReadOnlyList<VisualRatingMetadata> RatingMetadata { get; init; } = [];
    public IReadOnlyList<string> SourceLineageGoalIds { get; init; } = [];
}

public sealed record VisualWorldLayerProfile
{
    public string LayerId { get; init; } = string.Empty;
    public string LayerKind { get; init; } = string.Empty;
    public int Order { get; init; }
    public string MaterializationRole { get; init; } = "logical_visual_metadata";
    public string SafeFallbackRefId { get; init; } = string.Empty;
}

public sealed record VisualRegionSize
{
    public string SizeId { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public int LayerCount { get; init; }
    public long LogicalCellCount => (long)Width * Height * LayerCount;
}

public sealed record VisualDimensionRange
{
    public int MinimumWidth { get; init; } = ParameterizedVisualWorldProfilesVocabulary.MinimumFiniteDimension;
    public int MinimumHeight { get; init; } = ParameterizedVisualWorldProfilesVocabulary.MinimumFiniteDimension;
    public int MaximumWidth { get; init; } = ParameterizedVisualWorldProfilesVocabulary.MaximumFiniteDimension;
    public int MaximumHeight { get; init; } = ParameterizedVisualWorldProfilesVocabulary.MaximumFiniteDimension;

    public static VisualDimensionRange Default() => new();
}

public sealed record VisualChunkProfile
{
    public int ChunkWidth { get; init; }
    public int ChunkHeight { get; init; }
    public bool RequiresPatchAlignment { get; init; } = true;
    public bool UsesDeterministicChunkKeys { get; init; } = true;
    public string DeterministicKeyFormula { get; init; } = ParameterizedVisualWorldProfilesVocabulary.DeterministicChunkKeyFormula;
}

public sealed record VisualPatchProfile
{
    public int PatchWidth { get; init; }
    public int PatchHeight { get; init; }
}

public sealed record VisualVirtualWorldBounds
{
    public bool IsInfinite { get; init; }
    public long? MinimumX { get; init; }
    public long? MinimumY { get; init; }
    public long? MaximumX { get; init; }
    public long? MaximumY { get; init; }
}

public sealed record VisualSparseRegionIndex
{
    public bool SparseOnly { get; init; }
    public bool AttemptsRawCellDump { get; init; }
    public bool FiniteOnlyMaterialization { get; init; }
    public IReadOnlyList<VisualChunkSample> MaterializedChunks { get; init; } = [];
    public IReadOnlyList<string> AnchorIds { get; init; } = [];
}

public sealed record VisualChunkSample
{
    public string SampleId { get; init; } = string.Empty;
    public VisualChunkAddress Address { get; init; } = new();
    public VisualChunkKey ChunkKey { get; init; } = new();
    public string SampleRole { get; init; } = string.Empty;
}

public sealed record VisualChunkAddress
{
    public string LayerId { get; init; } = string.Empty;
    public long ChunkX { get; init; }
    public long ChunkY { get; init; }
}

public sealed record VisualChunkKey
{
    public string ProfileId { get; init; } = string.Empty;
    public string WorldSeed { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public long ChunkX { get; init; }
    public long ChunkY { get; init; }
    public string Formula { get; init; } = ParameterizedVisualWorldProfilesVocabulary.DeterministicChunkKeyFormula;
    public string Key { get; init; } = string.Empty;
}

public sealed record VisualStreamWindow
{
    public string WindowId { get; init; } = string.Empty;
    public long? CenterChunkX { get; init; }
    public long? CenterChunkY { get; init; }
    public int RadiusChunks { get; init; }
    public int WindowChunkCount { get; init; }
    public IReadOnlyList<VisualChunkAddress> SampledAddresses { get; init; } = [];
}

public sealed record VisualLayerLink
{
    public string LinkId { get; init; } = string.Empty;
    public string FromLayerId { get; init; } = string.Empty;
    public string ToLayerId { get; init; } = string.Empty;
    public VisualLayerLinkKind LinkKind { get; init; }
}

public sealed record VisualRatingMetadata
{
    public string MetadataId { get; init; } = string.Empty;
    public string RatingKind { get; init; } = string.Empty;
    public string SafeFallbackRefId { get; init; } = string.Empty;
}

public sealed record VisualWorldProfileValidationResult
{
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<VisualWorldProfileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualWorldProfileDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static VisualWorldProfileDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record VisualWorldProfileCatalog
{
    public string SchemaVersion { get; init; } = ParameterizedVisualWorldProfilesVocabulary.CatalogSchemaVersion;
    public string GoalId { get; init; } = ParameterizedVisualWorldProfilesVocabulary.GoalId;
    public string ManualGate { get; init; } = ParameterizedVisualWorldProfilesVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public IReadOnlyList<VisualWorldProfile> Profiles { get; init; } = [];
}

public sealed record VisualWorldProfileSizeMatrix
{
    public string SchemaVersion { get; init; } = ParameterizedVisualWorldProfilesVocabulary.SizeMatrixSchemaVersion;
    public string GoalId { get; init; } = ParameterizedVisualWorldProfilesVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<VisualWorldProfileSizeMatrixRow> Rows { get; init; } = [];
}

public sealed record VisualWorldProfileSizeMatrixRow
{
    public string ProfileId { get; init; } = string.Empty;
    public string SizeId { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public int LayerCount { get; init; }
    public long LogicalCellCount { get; init; }
    public bool ValidatorPassed { get; init; }
    public IReadOnlyList<VisualWorldProfileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualWorldProfileValidationMatrix
{
    public string SchemaVersion { get; init; } = ParameterizedVisualWorldProfilesVocabulary.ValidationMatrixSchemaVersion;
    public string GoalId { get; init; } = ParameterizedVisualWorldProfilesVocabulary.GoalId;
    public bool Passed { get; init; }
    public IReadOnlyList<VisualWorldProfileValidationMatrixRow> Rows { get; init; } = [];
}

public sealed record VisualWorldProfileValidationMatrixRow
{
    public string ProfileId { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public string Mode { get; init; } = string.Empty;
    public bool IsBenchmark { get; init; }
    public bool RawCellDumpAllowed { get; init; }
}

public sealed record VisualWorldProfileNegativeProof
{
    public string SchemaVersion { get; init; } = ParameterizedVisualWorldProfilesVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = ParameterizedVisualWorldProfilesVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<VisualWorldProfileNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record VisualWorldProfileNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<VisualWorldProfileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualWorldProfileChunkAddressProof
{
    public string SchemaVersion { get; init; } = ParameterizedVisualWorldProfilesVocabulary.ChunkAddressProofSchemaVersion;
    public string GoalId { get; init; } = ParameterizedVisualWorldProfilesVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool StableAcrossReruns { get; init; }
    public bool DiffersBySeedLayerChunkAndVersion { get; init; }
    public IReadOnlyList<VisualChunkKeyProofRow> Rows { get; init; } = [];
}

public sealed record VisualChunkKeyProofRow
{
    public string ProfileId { get; init; } = string.Empty;
    public VisualChunkAddress Address { get; init; } = new();
    public string FirstKey { get; init; } = string.Empty;
    public string SecondKey { get; init; } = string.Empty;
    public string VariantSeedKey { get; init; } = string.Empty;
    public string VariantLayerKey { get; init; } = string.Empty;
    public string VariantChunkKey { get; init; } = string.Empty;
    public string VariantVersionKey { get; init; } = string.Empty;
}

public sealed record VisualWorldProfileSparseWorldProof
{
    public string SchemaVersion { get; init; } = ParameterizedVisualWorldProfilesVocabulary.SparseWorldProofSchemaVersion;
    public string GoalId { get; init; } = ParameterizedVisualWorldProfilesVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool HugeSparseProfilePassed { get; init; }
    public bool InfiniteProfilePassed { get; init; }
    public IReadOnlyList<VisualSparseWorldProofRow> Rows { get; init; } = [];
}

public sealed record VisualSparseWorldProofRow
{
    public string ProfileId { get; init; } = string.Empty;
    public bool IsInfinite { get; init; }
    public long? LogicalCellCount { get; init; }
    public long? EstimatedChunkCapacity { get; init; }
    public int MaterializedChunkCount { get; init; }
    public bool SparseOnly { get; init; }
    public bool RawCellDumpAllowed { get; init; }
}

public sealed record VisualWorldProfileLayerModelProof
{
    public string SchemaVersion { get; init; } = ParameterizedVisualWorldProfilesVocabulary.LayerModelProofSchemaVersion;
    public string GoalId { get; init; } = ParameterizedVisualWorldProfilesVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool DataDrivenLayerSetsPassed { get; init; }
    public bool NotRestrictedToSurfaceUnderground { get; init; }
    public IReadOnlyList<VisualLayerModelProofRow> Rows { get; init; } = [];
}

public sealed record VisualLayerModelProofRow
{
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> LayerIds { get; init; } = [];
    public int LayerCount { get; init; }
    public bool UsesOnlySurfaceUnderground { get; init; }
}

public sealed record VisualWorldProfileSourceLineage
{
    public string SchemaVersion { get; init; } = ParameterizedVisualWorldProfilesVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } = ParameterizedVisualWorldProfilesVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal087LineagePresent { get; init; }
    public bool Goal088LineagePresent { get; init; }
    public int SourceRecordCount { get; init; }
    public IReadOnlyList<VisualWorldProfileSourceLineageRecord> Records { get; init; } = [];
}

public sealed record VisualWorldProfileSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record VisualWorldProfileQualityGateScan
{
    public string SchemaVersion { get; init; } = ParameterizedVisualWorldProfilesVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = ParameterizedVisualWorldProfilesVocabulary.GoalId;
    public string ManualGate { get; init; } = ParameterizedVisualWorldProfilesVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool ValidationMatrixPassed { get; init; }
    public bool SizeMatrixPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool ChunkAddressProofPassed { get; init; }
    public bool SparseWorldProofPassed { get; init; }
    public bool LayerModelProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool Benchmark144OnlyFixturePassed { get; init; }
    public bool NoRawHeavyCellDump { get; init; }
    public bool NoRuntimeUnityProviderSchemaProjectDependencyChanges { get; init; } = true;
    public bool NoBinaryOrRasterMediaAdded { get; init; } = true;
    public bool NoPromptDumps { get; init; } = true;
    public bool NoExplicitAdultContent { get; init; } = true;
    public bool ArtifactScopeReady { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<VisualWorldProfileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualWorldProfileReport
{
    public string GoalId { get; init; } = ParameterizedVisualWorldProfilesVocabulary.GoalId;
    public string ManualGate { get; init; } = ParameterizedVisualWorldProfilesVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int ProfileCount { get; init; }
    public bool ValidationPassed { get; init; }
    public bool SizeMatrixPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool ChunkAddressProofPassed { get; init; }
    public bool SparseWorldProofPassed { get; init; }
    public bool LayerModelProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public string CatalogHash { get; init; } = string.Empty;
    public string SizeMatrixHash { get; init; } = string.Empty;
    public string ValidationMatrixHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string ChunkAddressProofHash { get; init; } = string.Empty;
    public string SparseWorldProofHash { get; init; } = string.Empty;
    public string LayerModelProofHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record VisualWorldProfileEvidenceResult
{
    public VisualWorldProfileCatalog Catalog { get; init; } = new();
    public VisualWorldProfileSizeMatrix SizeMatrix { get; init; } = new();
    public VisualWorldProfileValidationMatrix ValidationMatrix { get; init; } = new();
    public VisualWorldProfileNegativeProof NegativeProof { get; init; } = new();
    public VisualWorldProfileChunkAddressProof ChunkAddressProof { get; init; } = new();
    public VisualWorldProfileSparseWorldProof SparseWorldProof { get; init; } = new();
    public VisualWorldProfileLayerModelProof LayerModelProof { get; init; } = new();
    public VisualWorldProfileSourceLineage SourceLineage { get; init; } = new();
    public VisualWorldProfileQualityGateScan QualityGateScan { get; init; } = new();
    public VisualWorldProfileReport Report { get; init; } = new();
    public string CatalogJson { get; init; } = string.Empty;
    public string SizeMatrixJson { get; init; } = string.Empty;
    public string ValidationMatrixJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string ChunkAddressProofJson { get; init; } = string.Empty;
    public string SparseWorldProofJson { get; init; } = string.Empty;
    public string LayerModelProofJson { get; init; } = string.Empty;
    public string SourceLineageJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> OverviewSvgByRelativePath { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

public sealed record VisualWorldProfileWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string CatalogJsonPath { get; init; } = string.Empty;
    public string SizeMatrixJsonPath { get; init; } = string.Empty;
    public string ValidationMatrixJsonPath { get; init; } = string.Empty;
    public string NegativeProofJsonPath { get; init; } = string.Empty;
    public string ChunkAddressProofJsonPath { get; init; } = string.Empty;
    public string SparseWorldProofJsonPath { get; init; } = string.Empty;
    public string LayerModelProofJsonPath { get; init; } = string.Empty;
    public string SourceLineageJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
    public IReadOnlyList<string> OverviewSvgPaths { get; init; } = [];
}
