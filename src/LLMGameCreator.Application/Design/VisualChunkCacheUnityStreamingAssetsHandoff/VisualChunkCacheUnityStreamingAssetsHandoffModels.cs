namespace LLMGameCreator.Application.Design.VisualChunkCacheUnityStreamingAssetsHandoff;

public static class VisualChunkCacheUnityStreamingAssetsHandoffVocabulary
{
    public const string GoalId = "goal_095_visual_chunk_cache_unity_streamingassets_handoff";
    public const string ProductSmokeRoute = "goal-095-visual-chunk-cache-unity-streamingassets-handoff";
    public const string FinalGate = "visual_chunk_cache_unity_streamingassets_handoff_verification";
    public const string RelativeOutputDirectory =
        ".llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff";
    public const string StreamingAssetsRelativeRoot =
        "unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095";
    public const string UnityStreamingAssetsProbeRoot = "LLMGameCreator/VisualChunkCacheGoal095";
    public const string UnityProbeScriptPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs";
    public const string AlphaRuntimeBootstrapPath =
        "unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs";
    public const string AlphaRuntimeBootstrapExpectedHash =
        "f40aa86e269561419fc6ef30fe456d284c5b8c8857de00671269b2b6bb6ccbce";
    public const int AlphaRuntimeBootstrapExpectedLineCount = 3672;

    public static readonly IReadOnlyList<string> RequiredPayloadFileNames =
    [
        "visual-chunk-cache-unity-handoff-manifest.json",
        "visual-chunk-cache-package-index.json",
        "visual-chunk-cache-stream-window-index.json",
        "visual-chunk-cache-chunk-key-ledger.json",
        "visual-chunk-cache-runtime-readme.json"
    ];

    public static readonly IReadOnlyList<string> RequiredEvidenceFileNames =
    [
        "visual-chunk-cache-unity-handoff-report.md",
        "visual-chunk-cache-unity-handoff-manifest.json",
        "visual-chunk-cache-unity-streamingassets-ledger.json",
        "visual-chunk-cache-unity-probe-source-inventory.json",
        "visual-chunk-cache-unity-simulated-read-proof.json",
        "visual-chunk-cache-unity-negative-proof.json",
        "visual-chunk-cache-unity-source-lineage.json",
        "visual-chunk-cache-unity-quality-gate-scan.json",
        "visual-chunk-cache-package-index.json",
        "visual-chunk-cache-stream-window-index.json",
        "visual-chunk-cache-chunk-key-ledger.json",
        "visual-chunk-cache-runtime-readme.json"
    ];

    public static readonly IReadOnlyList<string> RequiredNegativeScenarioIds =
    [
        "missing_manifest",
        "tampered_manifest_hash",
        "missing_package_index",
        "stream_window_count_mismatch",
        "chunk_key_ledger_mismatch",
        "absolute_path_in_payload",
        "raw_full_world_dump_marker",
        "provider_call_marker",
        "fake_success_without_file_read"
    ];
}

public sealed record VisualChunkCacheUnityHandoffDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static VisualChunkCacheUnityHandoffDiagnostic Error(
        string code,
        string target,
        string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record VisualChunkCacheUnityHandoffManifest
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_handoff_manifest_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityStreamingAssetsProbeRoot;
    public int PayloadFileCount { get; init; }
    public int PackageCount { get; init; }
    public int ExportRecordCount { get; init; }
    public int SourceMaterializedChunkCount { get; init; }
    public int StreamWindowCount { get; init; }
    public int UniqueChunkKeyCount { get; init; }
    public bool Goal093AcceptedFalse { get; init; }
    public bool Goal094AcceptedFalse { get; init; }
    public bool RuntimeHandoffSidecarMetadataOnly { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoPromptDumps { get; init; }
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsUnityGameplayImplementation { get; init; }
    public string PackageIndexHash { get; init; } = string.Empty;
    public string StreamWindowIndexHash { get; init; } = string.Empty;
    public string ChunkKeyLedgerHash { get; init; } = string.Empty;
    public string RuntimeReadmeHash { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredPayloadFiles { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames;
}

public sealed record VisualChunkCacheUnityPackageIndex
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_package_index_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public int PackageCount { get; init; }
    public int ExportRecordCount { get; init; }
    public IReadOnlyList<VisualChunkCacheUnityPackageSummary> Packages { get; init; } = [];
}

public sealed record VisualChunkCacheUnityPackageSummary
{
    public string PackageId { get; init; } = string.Empty;
    public string ExportTargetKind { get; init; } = string.Empty;
    public string SourceFixtureId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string WorldSeed { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public int StreamWindowCount { get; init; }
    public int ExportedRecordCount { get; init; }
    public int SourceMaterializedChunkCount { get; init; }
    public long? EstimatedFullWorldChunkCapacity { get; init; }
    public bool MetadataOnly { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public bool OnlyMaterializedChunksExported { get; init; }
    public string PackageMembershipHash { get; init; } = string.Empty;
}

public sealed record VisualChunkCacheUnityStreamWindowIndex
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_stream_window_index_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public int StreamWindowCount { get; init; }
    public IReadOnlyList<VisualChunkCacheUnityStreamWindowSummary> StreamWindows { get; init; } = [];
}

public sealed record VisualChunkCacheUnityStreamWindowSummary
{
    public string WindowId { get; init; } = string.Empty;
    public string FixtureId { get; init; } = string.Empty;
    public string PackageId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> LayerIds { get; init; } = [];
    public int SourceChunkCount { get; init; }
    public int ExportedRecordCount { get; init; }
    public string MembershipStableHash { get; init; } = string.Empty;
}

public sealed record VisualChunkCacheUnityChunkKeyLedger
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_chunk_key_ledger_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public int UniqueChunkKeyCount { get; init; }
    public int ExportRecordCount { get; init; }
    public bool CompactMetadataOnly { get; init; } = true;
    public bool NoRawFullWorldDump { get; init; } = true;
    public IReadOnlyList<VisualChunkCacheUnityChunkKeyEntry> Entries { get; init; } = [];
}

public sealed record VisualChunkCacheUnityChunkKeyEntry
{
    public string ChunkKey { get; init; } = string.Empty;
    public string ChunkHash { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public long ChunkX { get; init; }
    public long ChunkY { get; init; }
    public IReadOnlyList<string> PackageIds { get; init; } = [];
    public IReadOnlyList<string> StreamWindowIds { get; init; } = [];
}

public sealed record VisualChunkCacheUnityRuntimeReadme
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_runtime_readme_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public string Summary { get; init; } =
        "Unity Alpha handoff/probe only; metadata-only payload for cache inspection.";
    public bool ImplementsRuntimeConsumption { get; init; }
    public bool ImplementsLiveUnityRendering { get; init; }
    public bool ImplementsFinalAtlas { get; init; }
    public bool ImplementsRuntimeStreaming { get; init; }
    public bool RequiresProviderCalls { get; init; }
    public bool RequiresLlmCalls { get; init; }
    public IReadOnlyList<string> RequiredPayloadFiles { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.RequiredPayloadFileNames;
}

public sealed record VisualChunkCacheUnityFileLedger
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_streamingassets_ledger_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityStreamingAssetsProbeRoot;
    public int FileCount { get; init; }
    public IReadOnlyList<VisualChunkCacheUnityFileEntry> Files { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheUnityFileEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public long ByteCount { get; init; }
}

public sealed record VisualChunkCacheUnitySourceLineage
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_source_lineage_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal093LineagePresent { get; init; }
    public bool Goal094LineagePresent { get; init; }
    public bool Goal093AcceptedFalse { get; init; }
    public bool Goal094AcceptedFalse { get; init; }
    public int SourceRecordCount { get; init; }
    public IReadOnlyList<VisualChunkCacheUnitySourceArtifactReference> Records { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheUnitySourceArtifactReference
{
    public string SourceGoal { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public long ByteCount { get; init; }
    public bool Exists { get; init; }
}

public sealed record VisualChunkCacheUnityProbeSourceInventory
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_probe_source_inventory_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public string ProbeRelativePath { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityProbeScriptPath;
    public bool ProbeExists { get; init; }
    public string ProbeSha256 { get; init; } = string.Empty;
    public int ProbeLineCount { get; init; }
    public bool UsesApplicationStreamingAssetsPath { get; init; }
    public bool UsesExpectedPayloadRoot { get; init; }
    public bool ExposesInspectorResultFields { get; init; }
    public bool DoesNotReferenceAlphaRuntimeBootstrap { get; init; }
    public bool HasNoProviderLlmNetworkMarkers { get; init; }
    public IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheUnitySimulatedReadProof
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_simulated_read_proof_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool PayloadReadAttempted { get; init; }
    public bool ManifestRead { get; init; }
    public bool RequiredPayloadFilesPresent { get; init; }
    public bool PayloadHashesMatchManifest { get; init; }
    public bool PackageCountMatchesGoal093AndGoal094 { get; init; }
    public bool StreamWindowsRepresented { get; init; }
    public bool ChunkKeysRepresented { get; init; }
    public bool RuntimeHandoffSidecarMetadataOnly { get; init; }
    public bool CountsMatch { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoProviderLlmNetworkMarkers { get; init; }
    public int PackageCount { get; init; }
    public int ExportRecordCount { get; init; }
    public int StreamWindowCount { get; init; }
    public int UniqueChunkKeyCount { get; init; }
    public IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheUnityNegativeProof
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_negative_proof_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public IReadOnlyList<VisualChunkCacheUnityNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record VisualChunkCacheUnityNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = "rejected";
    public IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheUnityQualityGateScan
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_quality_gate_scan_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public string ManualGate { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool StreamingAssetsMirrorPassed { get; init; }
    public bool SimulatedReadProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool UnityProbeSourcePassed { get; init; }
    public bool AlphaRuntimeBootstrapUnchanged { get; init; }
    public string AlphaRuntimeBootstrapBeforeHash { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapExpectedHash;
    public string AlphaRuntimeBootstrapAfterHash { get; init; } = string.Empty;
    public int AlphaRuntimeBootstrapBeforeLineCount { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapExpectedLineCount;
    public int AlphaRuntimeBootstrapAfterLineCount { get; init; }
    public int ScannedCSharpFileCount { get; init; }
    public int MaxLogicalLineCount { get; init; }
    public int FilesOver700LogicalLinesCount { get; init; }
    public int FilesOver1000LogicalLinesCount { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoPromptDumps { get; init; }
    public bool NoRuntimeProviderSchemaProjectDependencyChanges { get; init; } = true;
    public bool NoForbiddenUnityAreasChanged { get; init; } = true;
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheUnitySourceFileScan> SourceFiles { get; init; } = [];
    public IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheUnitySourceFileScan
{
    public string RelativePath { get; init; } = string.Empty;
    public int LogicalLineCount { get; init; }
    public int MaxLineLength { get; init; }
}

public sealed record VisualChunkCacheUnityHandoffReport
{
    public string SchemaVersion { get; init; } = "visual_chunk_cache_unity_handoff_report_v1";
    public string GoalId { get; init; } = VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.GoalId;
    public string ProductSmokeRoute { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.ProductSmokeRoute;
    public string ManualGate { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PackageCount { get; init; }
    public int ExportRecordCount { get; init; }
    public int StreamWindowCount { get; init; }
    public int UniqueChunkKeyCount { get; init; }
    public string StreamingAssetsRelativeRoot { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.UnityStreamingAssetsProbeRoot;
    public string HandoffManifestHash { get; init; } = string.Empty;
    public string PackageIndexHash { get; init; } = string.Empty;
    public string StreamWindowIndexHash { get; init; } = string.Empty;
    public string ChunkKeyLedgerHash { get; init; } = string.Empty;
    public string RuntimeReadmeHash { get; init; } = string.Empty;
    public string StreamingAssetsLedgerHash { get; init; } = string.Empty;
    public string ProbeSourceInventoryHash { get; init; } = string.Empty;
    public string SimulatedReadProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateScanHash { get; init; } = string.Empty;
    public string AlphaRuntimeBootstrapBeforeHash { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapExpectedHash;
    public string AlphaRuntimeBootstrapAfterHash { get; init; } = string.Empty;
    public int AlphaRuntimeBootstrapBeforeLineCount { get; init; } =
        VisualChunkCacheUnityStreamingAssetsHandoffVocabulary.AlphaRuntimeBootstrapExpectedLineCount;
    public int AlphaRuntimeBootstrapAfterLineCount { get; init; }
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualChunkCacheUnityBuildResult
{
    public VisualChunkCacheUnitySourceLineage SourceLineage { get; init; } = new();
    public VisualChunkCacheUnityHandoffManifest HandoffManifest { get; init; } = new();
    public VisualChunkCacheUnityPackageIndex PackageIndex { get; init; } = new();
    public VisualChunkCacheUnityStreamWindowIndex StreamWindowIndex { get; init; } = new();
    public VisualChunkCacheUnityChunkKeyLedger ChunkKeyLedger { get; init; } = new();
    public VisualChunkCacheUnityRuntimeReadme RuntimeReadme { get; init; } = new();
    public VisualChunkCacheUnityFileLedger StreamingAssetsLedger { get; init; } = new();
    public VisualChunkCacheUnityProbeSourceInventory ProbeSourceInventory { get; init; } = new();
    public VisualChunkCacheUnitySimulatedReadProof SimulatedReadProof { get; init; } = new();
    public VisualChunkCacheUnityNegativeProof NegativeProof { get; init; } = new();
    public VisualChunkCacheUnityQualityGateScan QualityGateScan { get; init; } = new();
    public VisualChunkCacheUnityHandoffReport Report { get; init; } = new();
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> PayloadJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, string> EvidenceJsonByFileName { get; init; } =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}

public sealed record VisualChunkCacheUnityWriteResult
{
    public VisualChunkCacheUnityBuildResult Result { get; init; } = new();
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string StreamingAssetsDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
}

internal sealed record Goal095SourceContext
{
    public string RootPath { get; init; } = string.Empty;
    public Goal093ManifestSource Goal093Manifest { get; init; } = new();
    public Goal093RuntimeHandoffSidecarSource Goal093Sidecar { get; init; } = new();
    public Goal094QualityGateSource Goal094QualityGate { get; init; } = new();
    public VisualChunkCacheUnitySourceLineage SourceLineage { get; init; } = new();
    public int PackageCount { get; init; }
    public int ExportRecordCount { get; init; }
    public int SourceMaterializedChunkCount { get; init; }
    public int StreamWindowCount { get; init; }
    public int UniqueChunkKeyCount { get; init; }
    public bool Goal093AcceptedFalse { get; init; }
    public bool Goal094AcceptedFalse { get; init; }
    public bool RuntimeHandoffSidecarMetadataOnly { get; init; }
    public IReadOnlyList<VisualChunkCacheUnityHandoffDiagnostic> Diagnostics { get; init; } = [];
}

internal sealed record Goal093ManifestSource
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string GoalId { get; init; } = string.Empty;
    public string ImplementationStatus { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public int PackageCount { get; init; }
    public int ExportRecordCount { get; init; }
    public int SourceMaterializedChunkCount { get; init; }
    public int SourceUniqueChunkKeyCount { get; init; }
    public bool NoAbsolutePaths { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public bool NoBinaryOrRasterMedia { get; init; }
    public bool NoPromptDumps { get; init; }
    public bool MetadataOnlyRuntimeHandoff { get; init; }
    public IReadOnlyList<Goal093PackageSource> Packages { get; init; } = [];
}

internal sealed record Goal093PackageSource
{
    public string PackageId { get; init; } = string.Empty;
    public string ExportTargetKind { get; init; } = string.Empty;
    public string SourceFixtureId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string WorldSeed { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public int StreamWindowCount { get; init; }
    public int ExportedRecordCount { get; init; }
    public int SourceMaterializedChunkCount { get; init; }
    public long? EstimatedFullWorldChunkCapacity { get; init; }
    public bool NoRawFullWorldDump { get; init; }
    public bool OnlyMaterializedChunksExported { get; init; }
    public bool MetadataOnly { get; init; }
    public IReadOnlyList<Goal093StreamWindowSource> StreamWindows { get; init; } = [];
    public IReadOnlyList<Goal093RecordSource> Records { get; init; } = [];
}

internal sealed record Goal093StreamWindowSource
{
    public string FixtureId { get; init; } = string.Empty;
    public string WindowId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public IReadOnlyList<string> LayerIds { get; init; } = [];
    public int SourceChunkCount { get; init; }
    public int ExportedRecordCount { get; init; }
    public string MembershipStableHash { get; init; } = string.Empty;
}

internal sealed record Goal093RecordSource
{
    public Goal093CacheKeySource CacheKey { get; init; } = new();
    public string ChunkHash { get; init; } = string.Empty;
    public IReadOnlyList<string> StreamWindowIds { get; init; } = [];
    public bool NoRawFullWorldDump { get; init; }
    public bool ContainsRawFullWorldCellDump { get; init; }
    public bool PromptTextIsSourceOfTruth { get; init; }
}

internal sealed record Goal093CacheKeySource
{
    public string ProfileId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public long ChunkX { get; init; }
    public long ChunkY { get; init; }
    public string ChunkKey { get; init; } = string.Empty;
}

internal sealed record Goal093RuntimeHandoffSidecarSource
{
    public bool Accepted { get; init; }
    public bool MetadataOnly { get; init; }
    public bool ContainsRuntimeExecution { get; init; }
    public bool ContainsProviderCalls { get; init; }
    public bool ContainsUnityImplementation { get; init; }
    public bool ContainsPromptText { get; init; }
    public int RecordCount { get; init; }
}

internal sealed record Goal094QualityGateSource
{
    public bool Accepted { get; init; }
    public bool Passed { get; init; }
    public int CacheExportPackageCount { get; init; }
    public int CacheExportRecordCount { get; init; }
    public int CacheExportSourceChunkCount { get; init; }
    public int CacheExportStreamWindowCount { get; init; }
    public bool RuntimeHandoffSidecarMetadataOnly { get; init; }
    public bool CacheNoRawFullWorldDump { get; init; }
}
