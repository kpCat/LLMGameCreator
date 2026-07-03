namespace LLMGameCreator.Application.Design.DeterministicVisualMicrotileMaterializer;

public static class DeterministicVisualMicrotileMaterializerVocabulary
{
    public const string GoalId = "goal_086_deterministic_visual_microtile_materializer";
    public const string ProductSmokeRoute = "goal-086-deterministic-visual-microtile-materializer";
    public const string FinalGate = "deterministic_visual_microtile_materializer_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer";
    public const string PreviewRelativeDirectory = "previews";

    public const string RequestSchemaVersion = "visual_microtile_materializer_request_v1";
    public const string PreviewCatalogSchemaVersion = "visual_microtile_preview_catalog_v1";
    public const string MaterializationManifestSchemaVersion = "visual_microtile_materialization_manifest_v1";
    public const string FileLedgerSchemaVersion = "visual_microtile_file_ledger_v1";
    public const string WaterBiomeProofSchemaVersion = "visual_microtile_water_biome_proof_v1";
    public const string LayeringProofSchemaVersion = "visual_microtile_layering_proof_v1";
    public const string NegativeProofSchemaVersion = "visual_microtile_negative_proof_v1";
    public const string QualityGateSchemaVersion = "visual_microtile_quality_gate_scan_v1";
    public const string SourceLineageSchemaVersion = "visual_microtile_source_lineage_v1";
}

public enum VisualMicrotileCategory
{
    Unknown = 0,
    TerrainBiome,
    Water,
    SettlementStructure,
    CreatureNpc,
    UiEffect,
    AdultRating
}

public enum VisualMicrotileProviderState
{
    None = 0,
    MetadataOnly,
    CandidateQuarantine,
    ApprovedOutput,
    Rejected
}

public sealed record VisualMicrotileMaterializationRequest
{
    public string SchemaVersion { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.RequestSchemaVersion;
    public string RequestId { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public string OutputRelativeDirectory { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.RelativeOutputDirectory;
    public string SourceOfTruthKind { get; init; } = "metadata_contract";
    public bool PromptTextIsSourceOfTruth { get; init; }
    public bool SourceGoal084And085LineageRequired { get; init; } = true;
    public IReadOnlyList<VisualMicrotilePreviewSpec> Previews { get; init; } = [];
}

public sealed record VisualMicrotilePreviewSpec
{
    public string PreviewId { get; init; } = string.Empty;
    public VisualMicrotileCategory Category { get; init; } = VisualMicrotileCategory.Unknown;
    public string PartPackId { get; init; } = string.Empty;
    public string AssetSlotId { get; init; } = string.Empty;
    public string PaletteProfileId { get; init; } = string.Empty;
    public int Seed { get; init; }
    public string PreviewRelativePath { get; init; } = string.Empty;
    public string ViewBox { get; init; } = "0 0 64 64";
    public IReadOnlyList<VisualMicrotileLayerSpec> LayerStack { get; init; } = [];
    public IReadOnlyList<VisualMicrotilePaletteSwatch> Palette { get; init; } = [];
    public IReadOnlyList<string> MaskIds { get; init; } = [];
    public IReadOnlyList<string> SocketIds { get; init; } = [];
    public IReadOnlyList<string> AnchorIds { get; init; } = [];
    public string SourceGoal084SlotId { get; init; } = string.Empty;
    public string SourceGoal085PackId { get; init; } = string.Empty;
    public string BiomeRuleId { get; init; } = string.Empty;
    public string WaterRuleId { get; init; } = string.Empty;
    public VisualMicrotileWaterAdjacency? WaterLandAdjacency { get; init; }
    public IReadOnlyList<string> FlowConnectors { get; init; } = [];
    public bool AdultMetadataOnly { get; init; }
    public string SafeFallbackPreviewId { get; init; } = string.Empty;
    public VisualMicrotileProviderState ProviderState { get; init; } = VisualMicrotileProviderState.MetadataOnly;
    public bool TreatProviderCandidateAsApprovedOutput { get; init; }
}

public sealed record VisualMicrotileLayerSpec
{
    public string LayerId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string Role { get; init; } = string.Empty;
}

public sealed record VisualMicrotilePaletteSwatch
{
    public string SlotId { get; init; } = string.Empty;
    public string HexColor { get; init; } = string.Empty;
}

public sealed record VisualMicrotileWaterAdjacency
{
    public IReadOnlyList<string> WaterEdges { get; init; } = [];
    public IReadOnlyList<string> LandEdges { get; init; } = [];
}

public sealed record VisualMicrotileValidationResult
{
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<VisualMicrotileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualMicrotileDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static VisualMicrotileDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record VisualMicrotilePreviewCatalog
{
    public string SchemaVersion { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.PreviewCatalogSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PreviewCount { get; init; }
    public IReadOnlyList<VisualMicrotileCategoryCoverage> CategoryCoverage { get; init; } = [];
    public IReadOnlyList<VisualMicrotilePreviewCatalogEntry> Previews { get; init; } = [];
}

public sealed record VisualMicrotileCategoryCoverage
{
    public VisualMicrotileCategory Category { get; init; }
    public int Count { get; init; }
}

public sealed record VisualMicrotilePreviewCatalogEntry
{
    public string PreviewId { get; init; } = string.Empty;
    public VisualMicrotileCategory Category { get; init; }
    public string PreviewRelativePath { get; init; } = string.Empty;
    public string PartPackId { get; init; } = string.Empty;
    public string AssetSlotId { get; init; } = string.Empty;
    public string PaletteProfileId { get; init; } = string.Empty;
    public int Seed { get; init; }
    public string BiomeRuleId { get; init; } = string.Empty;
    public string WaterRuleId { get; init; } = string.Empty;
    public bool AdultMetadataOnly { get; init; }
    public string SafeFallbackPreviewId { get; init; } = string.Empty;
}

public sealed record VisualMicrotileMaterializationManifest
{
    public string SchemaVersion { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.MaterializationManifestSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.GoalId;
    public string GeneratorVersion { get; init; } = string.Empty;
    public string OutputRelativeDirectory { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.RelativeOutputDirectory;
    public int PreviewCount { get; init; }
    public IReadOnlyList<VisualMicrotileMaterializedPreview> Previews { get; init; } = [];
}

public sealed record VisualMicrotileMaterializedPreview
{
    public string PreviewId { get; init; } = string.Empty;
    public string PreviewRelativePath { get; init; } = string.Empty;
    public string SvgSha256 { get; init; } = string.Empty;
    public int ByteLength { get; init; }
    public bool ContainsViewBox { get; init; }
    public bool ExternalResourceFree { get; init; }
    public bool ScriptFree { get; init; }
    public int LayerCount { get; init; }
    public int GeneratedShapeCount { get; init; }
}

public sealed record VisualMicrotileFileLedger
{
    public string SchemaVersion { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.FileLedgerSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ExcludesSelfAndReportByDesign { get; init; } = true;
    public int FileCount { get; init; }
    public IReadOnlyList<VisualMicrotileFileLedgerEntry> Files { get; init; } = [];
}

public sealed record VisualMicrotileFileLedgerEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int ByteLength { get; init; }
}

public sealed record VisualMicrotileWaterBiomeProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.WaterBiomeProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool GrassOverworldCovered { get; init; }
    public bool SnowCovered { get; init; }
    public bool DesertDryCovered { get; init; }
    public bool LavaAshCovered { get; init; }
    public bool ForestOverlayCovered { get; init; }
    public bool MountainRockCovered { get; init; }
    public bool WaterBaseCovered { get; init; }
    public bool CoastTransitionCovered { get; init; }
    public bool RiverSegmentCovered { get; init; }
    public bool LakeEdgeCovered { get; init; }
    public bool MarshSwampCovered { get; init; }
    public bool BridgeDockAnchorMetadataCovered { get; init; }
}

public sealed record VisualMicrotileLayeringProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.LayeringProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.GoalId;
    public bool Passed { get; init; }
    public int PreviewCount { get; init; }
    public bool AllPreviewLayerOrderingStable { get; init; }
    public bool AllPreviewsUsePaletteMasksSocketsAndAnchors { get; init; }
    public IReadOnlyList<VisualMicrotileLayeringProofRow> Rows { get; init; } = [];
}

public sealed record VisualMicrotileLayeringProofRow
{
    public string PreviewId { get; init; } = string.Empty;
    public int LayerCount { get; init; }
    public IReadOnlyList<int> LayerOrders { get; init; } = [];
    public string PaletteProfileId { get; init; } = string.Empty;
    public int MaskCount { get; init; }
    public int SocketCount { get; init; }
    public int AnchorCount { get; init; }
}

public sealed record VisualMicrotileNegativeProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<VisualMicrotileNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record VisualMicrotileNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<VisualMicrotileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualMicrotileQualityGateScan
{
    public string SchemaVersion { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool PreviewCountWithinBounds { get; init; }
    public bool SvgTextOnlyPreviews { get; init; }
    public bool DeterministicRerunStable { get; init; }
    public bool WaterBiomeCoveragePassed { get; init; }
    public bool CreatureEquipmentStateCoveragePassed { get; init; }
    public bool UiEffectWeatherCoveragePassed { get; init; }
    public bool AdultMetadataOnlyFallbackCoveragePassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool NoForbiddenFilesChanged { get; init; } = true;
    public bool NoExternalDependenciesAdded { get; init; } = true;
    public bool NoBinaryMediaAdded { get; init; } = true;
    public bool NoPromptDumps { get; init; } = true;
    public bool NoProviderCalls { get; init; } = true;
    public bool NoExplicitAdultContent { get; init; } = true;
    public bool ArtifactScopeReady { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<VisualMicrotileDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualMicrotileSourceLineage
{
    public string SchemaVersion { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal084ArtifactsGreen { get; init; }
    public bool Goal084AcceptedFalse { get; init; }
    public bool Goal085ArtifactsGreen { get; init; }
    public bool Goal085AcceptedFalse { get; init; }
    public bool DeepsearchDocsExist { get; init; }
    public bool SynthesisExists { get; init; }
    public int SourceRecordCount { get; init; }
    public IReadOnlyList<VisualMicrotileSourceLineageRecord> Records { get; init; } = [];
}

public sealed record VisualMicrotileSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record VisualMicrotileReport
{
    public string GoalId { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualMicrotileMaterializerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PreviewCount { get; init; }
    public int TerrainBiomePreviewCount { get; init; }
    public int WaterPreviewCount { get; init; }
    public int SettlementPreviewCount { get; init; }
    public int CreaturePreviewCount { get; init; }
    public int UiEffectPreviewCount { get; init; }
    public int AdultMetadataOnlyPreviewCount { get; init; }
    public bool ValidationPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public string PreviewCatalogHash { get; init; } = string.Empty;
    public string MaterializationManifestHash { get; init; } = string.Empty;
    public string FileLedgerHash { get; init; } = string.Empty;
    public string WaterBiomeProofHash { get; init; } = string.Empty;
    public string LayeringProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record VisualMicrotileEvidenceResult
{
    public VisualMicrotilePreviewCatalog PreviewCatalog { get; init; } = new();
    public VisualMicrotileMaterializationManifest MaterializationManifest { get; init; } = new();
    public VisualMicrotileFileLedger FileLedger { get; init; } = new();
    public VisualMicrotileWaterBiomeProof WaterBiomeProof { get; init; } = new();
    public VisualMicrotileLayeringProof LayeringProof { get; init; } = new();
    public VisualMicrotileNegativeProof NegativeProof { get; init; } = new();
    public VisualMicrotileQualityGateScan QualityGateScan { get; init; } = new();
    public VisualMicrotileSourceLineage SourceLineage { get; init; } = new();
    public VisualMicrotileReport Report { get; init; } = new();
    public string PreviewCatalogJson { get; init; } = string.Empty;
    public string MaterializationManifestJson { get; init; } = string.Empty;
    public string FileLedgerJson { get; init; } = string.Empty;
    public string WaterBiomeProofJson { get; init; } = string.Empty;
    public string LayeringProofJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string SourceLineageJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> SvgByPreviewId { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

public sealed record VisualMicrotileWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string PreviewDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string PreviewCatalogJsonPath { get; init; } = string.Empty;
    public string MaterializationManifestJsonPath { get; init; } = string.Empty;
    public string FileLedgerJsonPath { get; init; } = string.Empty;
    public string WaterBiomeProofJsonPath { get; init; } = string.Empty;
    public string LayeringProofJsonPath { get; init; } = string.Empty;
    public string NegativeProofJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
    public string SourceLineageJsonPath { get; init; } = string.Empty;
    public IReadOnlyList<string> PreviewSvgPaths { get; init; } = [];
}
