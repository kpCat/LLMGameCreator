namespace LLMGameCreator.Application.Design.VisualChunkCacheExportContract;

public static class VisualChunkCacheExportContractVocabulary
{
    public const string GoalId = "goal_093_visual_chunk_cache_export_contract";
    public const string ProductSmokeRoute = "goal-093-visual-chunk-cache-export-contract";
    public const string FinalGate = "visual_chunk_cache_export_contract_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-093-visual-chunk-cache-export-contract";

    public const string ManifestSchemaVersion = "visual_chunk_cache_export_manifest_v1";
    public const string FileLedgerSchemaVersion = "visual_chunk_cache_file_ledger_v1";
    public const string RuntimeHandoffSidecarSchemaVersion = "visual_chunk_cache_runtime_handoff_sidecar_v1";
    public const string InvalidationMatrixSchemaVersion = "visual_chunk_cache_invalidation_matrix_v1";
    public const string ReadbackProofSchemaVersion = "visual_chunk_cache_readback_proof_v1";
    public const string OverlapReuseProofSchemaVersion = "visual_chunk_cache_overlap_reuse_proof_v1";
    public const string NegativeProofSchemaVersion = "visual_chunk_cache_negative_proof_v1";
    public const string SourceLineageSchemaVersion = "visual_chunk_cache_source_lineage_v1";
    public const string QualityGateSchemaVersion = "visual_chunk_cache_quality_gate_scan_v1";

    public const string FinitePackageId = "finite_custom_255x257_window_cache_export";
    public const string HugeSparsePackageId = "huge_sparse_100000x100000_window_cache_export";
    public const string InfiniteOverlapPackageId = "infinite_streaming_overlap_cache_export";
    public const string LayerTransitionPackageId = "layer_transition_runtime_handoff_sidecar";

    public const string ExpectedGeneratorVersion = "visual-profile-seam-v1";
}

public enum VisualChunkCacheExportTargetKind
{
    EditorReview = 0,
    RuntimeHandoff,
    UnityStreamingAssetsCandidate
}

public sealed record VisualChunkCacheExportPackage
{
    public string PackageId { get; init; } = string.Empty;
    public VisualChunkCacheExportTargetKind ExportTargetKind { get; init; }
    public string SourceFixtureId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string WorldSeed { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public string CachePolicy { get; init; } = "deterministic_chunk_key_reuse";
    public int StreamWindowCount { get; init; }
    public int ExportedRecordCount { get; init; }
    public int SourceMaterializedChunkCount { get; init; }
    public long? EstimatedFullWorldChunkCapacity { get; init; }
    public bool NoRawFullWorldDump { get; init; } = true;
    public bool OnlyMaterializedChunksExported { get; init; } = true;
    public bool MetadataOnly { get; init; } = true;
    public IReadOnlyList<string> SourceGoalIds { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheSourceHash> SourceHashes { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheStreamWindowRef> StreamWindows { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheRecord> Records { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheArtifactRef> ArtifactRefs { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheInvalidationRule> InvalidationRules { get; init; } = [];
}

public sealed record VisualChunkCacheExportManifest
{
    public string SchemaVersion { get; init; } = VisualChunkCacheExportContractVocabulary.ManifestSchemaVersion;
    public string GoalId { get; init; } = VisualChunkCacheExportContractVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualChunkCacheExportContractVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PackageCount { get; init; }
    public int ExportRecordCount { get; init; }
    public int SourceMaterializedChunkCount { get; init; }
    public int SourceUniqueChunkKeyCount { get; init; }
    public bool NoAbsolutePaths { get; init; } = true;
    public bool NoRawFullWorldDump { get; init; } = true;
    public bool NoBinaryOrRasterMedia { get; init; } = true;
    public bool NoPromptDumps { get; init; } = true;
    public bool MetadataOnlyRuntimeHandoff { get; init; } = true;
    public IReadOnlyList<VisualChunkCacheExportPackage> Packages { get; init; } = [];
}

public sealed record VisualChunkCacheRecord
{
    public string PackageId { get; init; } = string.Empty;
    public VisualChunkCacheExportTargetKind ExportTargetKind { get; init; }
    public string SourceFixtureId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string WorldSeed { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public VisualChunkCacheLayerRef Layer { get; init; } = new();
    public VisualChunkCacheKey CacheKey { get; init; } = new();
    public string ChunkHash { get; init; } = string.Empty;
    public IReadOnlyList<string> StreamWindowIds { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheArtifactRef> ArtifactRefs { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheInvalidationRule> InvalidationRules { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheDeltaOverlayRef> DeltaOverlays { get; init; } = [];
    public string CachePolicy { get; init; } = "deterministic_chunk_key_reuse";
    public VisualChunkCacheRatingMetadataSummary RatingMetadata { get; init; } = new();
    public bool NoRawFullWorldDump { get; init; } = true;
    public bool ContainsRawFullWorldCellDump { get; init; }
    public bool PromptTextIsSourceOfTruth { get; init; }
}

public sealed record VisualChunkCacheKey
{
    public string ProfileId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public long ChunkX { get; init; }
    public long ChunkY { get; init; }
    public string ChunkKey { get; init; } = string.Empty;
}

public sealed record VisualChunkCacheArtifactRef
{
    public string RelativePath { get; init; } = string.Empty;
    public string ArtifactKind { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
    public bool IsPreviewTextSvg { get; init; }
    public bool IsBinaryOrRaster { get; init; }
    public bool IsPromptDump { get; init; }
}

public sealed record VisualChunkCacheStreamWindowRef
{
    public string FixtureId { get; init; } = string.Empty;
    public string WindowId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public IReadOnlyList<string> LayerIds { get; init; } = [];
    public int SourceChunkCount { get; init; }
    public int ExportedRecordCount { get; init; }
    public string MembershipStableHash { get; init; } = string.Empty;
}

public sealed record VisualChunkCacheLayerRef
{
    public string LayerId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> LinkedLayerIds { get; init; } = [];
    public string SafeFallbackRefId { get; init; } = string.Empty;
}

public sealed record VisualChunkCacheInvalidationRule
{
    public string RuleId { get; init; } = string.Empty;
    public string InvalidationKey { get; init; } = string.Empty;
    public string SourceValueHash { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public sealed record VisualChunkCacheDeltaOverlayRef
{
    public string OverlayId { get; init; } = string.Empty;
    public string StableHash { get; init; } = string.Empty;
    public int ChangedChunkCount { get; init; }
    public bool ContainsRawCellPayload { get; init; }
}

public sealed record VisualChunkCacheRatingMetadataSummary
{
    public bool ContainsAdultRatingMetadata { get; init; }
    public string SafeFallbackRefId { get; init; } = string.Empty;
    public bool SafeFallbackPresent { get; init; } = true;
    public string Summary { get; init; } = "safe_or_non_adult_metadata_only";
}

public sealed record VisualChunkCacheRuntimeHandoffSidecar
{
    public string SchemaVersion { get; init; } = VisualChunkCacheExportContractVocabulary.RuntimeHandoffSidecarSchemaVersion;
    public string GoalId { get; init; } = VisualChunkCacheExportContractVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualChunkCacheExportContractVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public string SidecarId { get; init; } = "goal093_runtime_handoff_sidecar";
    public string PackageId { get; init; } = VisualChunkCacheExportContractVocabulary.LayerTransitionPackageId;
    public VisualChunkCacheExportTargetKind ExportTargetKind { get; init; } = VisualChunkCacheExportTargetKind.RuntimeHandoff;
    public bool MetadataOnly { get; init; } = true;
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsUnityImplementation { get; init; }
    public bool ContainsPromptText { get; init; }
    public int RecordCount { get; init; }
    public IReadOnlyList<string> StreamWindowIds { get; init; } = [];
    public IReadOnlyList<string> LayerIds { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheArtifactRef> ArtifactRefs { get; init; } = [];
}

public sealed record VisualChunkCacheSourceHash
{
    public string SourceGoalId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record VisualChunkCacheFileLedger
{
    public string SchemaVersion { get; init; } = VisualChunkCacheExportContractVocabulary.FileLedgerSchemaVersion;
    public string GoalId { get; init; } = VisualChunkCacheExportContractVocabulary.GoalId;
    public bool Passed { get; init; }
    public int FileCount { get; init; }
    public IReadOnlyList<VisualChunkCacheFileLedgerEntry> Files { get; init; } = [];
}

public sealed record VisualChunkCacheFileLedgerEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int ByteLength { get; init; }
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record VisualChunkCacheInvalidationMatrix
{
    public string SchemaVersion { get; init; } = VisualChunkCacheExportContractVocabulary.InvalidationMatrixSchemaVersion;
    public string GoalId { get; init; } = VisualChunkCacheExportContractVocabulary.GoalId;
    public bool Passed { get; init; }
    public int PackageCount { get; init; }
    public int RuleCount { get; init; }
    public IReadOnlyList<VisualChunkCacheInvalidationMatrixRow> Rows { get; init; } = [];
}

public sealed record VisualChunkCacheInvalidationMatrixRow
{
    public string PackageId { get; init; } = string.Empty;
    public string RuleId { get; init; } = string.Empty;
    public string InvalidationKey { get; init; } = string.Empty;
    public string SourceValueHash { get; init; } = string.Empty;
    public bool KnownKey { get; init; }
}

public sealed record VisualChunkCacheReadbackProof
{
    public string SchemaVersion { get; init; } = VisualChunkCacheExportContractVocabulary.ReadbackProofSchemaVersion;
    public string GoalId { get; init; } = VisualChunkCacheExportContractVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ManifestRoundTripPassed { get; init; }
    public bool RuntimeHandoffSidecarRoundTripPassed { get; init; }
    public bool ManifestValidationPassed { get; init; }
    public bool RuntimeHandoffSidecarValidationPassed { get; init; }
    public int PackageCount { get; init; }
    public int ExportRecordCount { get; init; }
    public IReadOnlyList<VisualChunkCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheOverlapReuseProof
{
    public string SchemaVersion { get; init; } = VisualChunkCacheExportContractVocabulary.OverlapReuseProofSchemaVersion;
    public string GoalId { get; init; } = VisualChunkCacheExportContractVocabulary.GoalId;
    public bool Passed { get; init; }
    public string PackageId { get; init; } = VisualChunkCacheExportContractVocabulary.InfiniteOverlapPackageId;
    public int SourceGoal091ReusedChunkKeyCount { get; init; }
    public int ExportReusedChunkKeyCount { get; init; }
    public IReadOnlyList<VisualChunkCacheOverlapReuseRow> Rows { get; init; } = [];
}

public sealed record VisualChunkCacheOverlapReuseRow
{
    public string ChunkKey { get; init; } = string.Empty;
    public string ChunkHash { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public long ChunkX { get; init; }
    public long ChunkY { get; init; }
    public IReadOnlyList<string> StreamWindowIds { get; init; } = [];
}

public sealed record VisualChunkCacheNegativeProof
{
    public string SchemaVersion { get; init; } = VisualChunkCacheExportContractVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = VisualChunkCacheExportContractVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<VisualChunkCacheNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record VisualChunkCacheNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<VisualChunkCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheSourceLineage
{
    public string SchemaVersion { get; init; } = VisualChunkCacheExportContractVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } = VisualChunkCacheExportContractVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal090LineagePresent { get; init; }
    public bool Goal091LineagePresent { get; init; }
    public bool Goal092PreviewLineagePresent { get; init; }
    public int SourceRecordCount { get; init; }
    public IReadOnlyList<VisualChunkCacheSourceHash> Records { get; init; } = [];
}

public sealed record VisualChunkCacheQualityGateScan
{
    public string SchemaVersion { get; init; } = VisualChunkCacheExportContractVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = VisualChunkCacheExportContractVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualChunkCacheExportContractVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool FiniteExportExists { get; init; }
    public bool HugeSparseExportExists { get; init; }
    public bool InfiniteOverlapExportExists { get; init; }
    public bool LayerTransitionRuntimeHandoffExists { get; init; }
    public bool ReadbackProofPassed { get; init; }
    public bool OverlapReuseProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public bool NoBinaryOrRasterMediaAdded { get; init; }
    public bool NoPromptDumps { get; init; }
    public bool RuntimeHandoffSidecarMetadataOnly { get; init; }
    public bool NoRuntimeUnityProviderSchemaProjectDependencyChanges { get; init; } = true;
    public bool ArtifactScopeReady { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheExportReport
{
    public string GoalId { get; init; } = VisualChunkCacheExportContractVocabulary.GoalId;
    public string ManualGate { get; init; } = VisualChunkCacheExportContractVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PackageCount { get; init; }
    public int ExportRecordCount { get; init; }
    public int SourceMaterializedChunkCount { get; init; }
    public bool ReadbackProofPassed { get; init; }
    public bool OverlapReuseProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public string ManifestHash { get; init; } = string.Empty;
    public string FileLedgerHash { get; init; } = string.Empty;
    public string RuntimeHandoffSidecarHash { get; init; } = string.Empty;
    public string InvalidationMatrixHash { get; init; } = string.Empty;
    public string ReadbackProofHash { get; init; } = string.Empty;
    public string OverlapReuseProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record VisualChunkCacheDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static VisualChunkCacheDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record VisualChunkCacheValidationResult
{
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<VisualChunkCacheDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheEvidenceResult
{
    public VisualChunkCacheExportManifest Manifest { get; init; } = new();
    public VisualChunkCacheFileLedger FileLedger { get; init; } = new();
    public VisualChunkCacheRuntimeHandoffSidecar RuntimeHandoffSidecar { get; init; } = new();
    public VisualChunkCacheInvalidationMatrix InvalidationMatrix { get; init; } = new();
    public VisualChunkCacheReadbackProof ReadbackProof { get; init; } = new();
    public VisualChunkCacheOverlapReuseProof OverlapReuseProof { get; init; } = new();
    public VisualChunkCacheNegativeProof NegativeProof { get; init; } = new();
    public VisualChunkCacheSourceLineage SourceLineage { get; init; } = new();
    public VisualChunkCacheQualityGateScan QualityGateScan { get; init; } = new();
    public VisualChunkCacheExportReport Report { get; init; } = new();
    public string ManifestJson { get; init; } = string.Empty;
    public string FileLedgerJson { get; init; } = string.Empty;
    public string RuntimeHandoffSidecarJson { get; init; } = string.Empty;
    public string InvalidationMatrixJson { get; init; } = string.Empty;
    public string ReadbackProofJson { get; init; } = string.Empty;
    public string OverlapReuseProofJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string SourceLineageJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record VisualChunkCacheWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string ManifestJsonPath { get; init; } = string.Empty;
    public string FileLedgerJsonPath { get; init; } = string.Empty;
    public string RuntimeHandoffSidecarJsonPath { get; init; } = string.Empty;
    public string InvalidationMatrixJsonPath { get; init; } = string.Empty;
    public string ReadbackProofJsonPath { get; init; } = string.Empty;
    public string OverlapReuseProofJsonPath { get; init; } = string.Empty;
    public string NegativeProofJsonPath { get; init; } = string.Empty;
    public string SourceLineageJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
}
