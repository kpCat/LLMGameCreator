namespace LLMGameCreator.Application.Design.DeterministicVisualMapPatchComposer;

public static class DeterministicVisualMapPatchComposerVocabulary
{
    public const string GoalId = "goal_087_deterministic_visual_map_patch_composer";
    public const string ProductSmokeRoute = "goal-087-deterministic-visual-map-patch-composer";
    public const string FinalGate = "deterministic_visual_map_patch_composer_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer";
    public const string PatchRelativeDirectory = "patches";

    public const string RequestSchemaVersion = "visual_map_patch_composer_request_v1";
    public const string CatalogSchemaVersion = "visual_map_patch_catalog_v1";
    public const string ManifestSchemaVersion = "visual_map_patch_materialization_manifest_v1";
    public const string FileLedgerSchemaVersion = "visual_map_patch_file_ledger_v1";
    public const string WaterFlowProofSchemaVersion = "visual_map_patch_water_flow_proof_v1";
    public const string ReachabilityProofSchemaVersion = "visual_map_patch_reachability_proof_v1";
    public const string LayeringProofSchemaVersion = "visual_map_patch_layering_proof_v1";
    public const string NegativeProofSchemaVersion = "visual_map_patch_negative_proof_v1";
    public const string SourceLineageSchemaVersion = "visual_map_patch_source_lineage_v1";
    public const string QualityGateSchemaVersion = "visual_map_patch_quality_gate_scan_v1";
}

public enum VisualMapPatchTerrainBiome
{
    Unknown = 0,
    Grass,
    Forest,
    Mountain,
    Snow,
    Desert,
    LavaAsh,
    Water,
    Marsh
}

public enum VisualMapPatchWaterKind
{
    None = 0,
    Sea,
    Coast,
    River,
    Lake,
    Marsh
}

public enum VisualMapPatchTransitionKind
{
    None = 0,
    BiomeEdge,
    Coast,
    River,
    LakeEdge,
    MarshEdge,
    Road
}

public enum VisualMapPatchLayerKind
{
    Terrain = 0,
    Water,
    Road,
    Object,
    Settlement,
    Creature,
    Overlay,
    RatingFallback
}

public enum VisualMapPatchConnector
{
    North = 0,
    East,
    South,
    West
}

public enum VisualMapPatchProviderState
{
    None = 0,
    MetadataOnly,
    CandidateQuarantine,
    ApprovedOutput,
    Rejected
}

public sealed record VisualMapPatchComposerRequest
{
    public string SchemaVersion { get; init; } = DeterministicVisualMapPatchComposerVocabulary.RequestSchemaVersion;
    public string RequestId { get; init; } = string.Empty;
    public string GeneratorVersion { get; init; } = string.Empty;
    public string OutputRelativeDirectory { get; init; } = DeterministicVisualMapPatchComposerVocabulary.RelativeOutputDirectory;
    public string SourceOfTruthKind { get; init; } = "metadata_contract";
    public bool PromptTextIsSourceOfTruth { get; init; }
    public bool SourceGoal084085086LineageRequired { get; init; } = true;
    public IReadOnlyList<VisualMapPatchDefinition> Patches { get; init; } = [];
}

public sealed record VisualMapPatchDefinition
{
    public string PatchId { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public int Seed { get; init; }
    public string PatchSvgRelativePath { get; init; } = string.Empty;
    public IReadOnlyList<VisualMapPatchLayer> Layers { get; init; } = [];
    public IReadOnlyList<VisualMapPatchCell> Cells { get; init; } = [];
    public IReadOnlyList<VisualMapPatchObjectAnchor> ObjectAnchors { get; init; } = [];
    public IReadOnlyList<VisualMapPatchRoadPath> RoadPaths { get; init; } = [];
    public IReadOnlyList<VisualMapPatchWaterFlow> WaterFlows { get; init; } = [];
    public IReadOnlyList<VisualMapPatchBiomeTransition> BiomeTransitions { get; init; } = [];
    public IReadOnlyList<VisualMapPatchSettlementAnchor> SettlementAnchors { get; init; } = [];
    public IReadOnlyList<VisualMapPatchCreatureMarker> CreatureMarkers { get; init; } = [];
    public IReadOnlyList<VisualMapPatchOverlay> Overlays { get; init; } = [];
    public IReadOnlyList<VisualMapPatchSourceReference> SourceReferences { get; init; } = [];
}

public sealed record VisualMapPatchCell
{
    public string CellId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public VisualMapPatchTerrainBiome TerrainBiome { get; init; }
    public VisualMapPatchWaterKind WaterKind { get; init; }
    public VisualMapPatchTransitionKind TransitionKind { get; init; }
    public bool IsPassable { get; init; }
    public string SourceMicrotilePreviewId { get; init; } = string.Empty;
    public IReadOnlyList<VisualMapPatchTileRef> TileRefs { get; init; } = [];
    public IReadOnlyList<VisualMapPatchConnector> Connectors { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
}

public sealed record VisualMapPatchTileRef
{
    public string PreviewId { get; init; } = string.Empty;
    public VisualMapPatchLayerKind LayerKind { get; init; }
    public int Order { get; init; }
}

public sealed record VisualMapPatchLayer
{
    public string LayerId { get; init; } = string.Empty;
    public VisualMapPatchLayerKind Kind { get; init; }
    public int Order { get; init; }
    public string Description { get; init; } = string.Empty;
}

public sealed record VisualMapPatchObjectAnchor
{
    public string AnchorId { get; init; } = string.Empty;
    public string ObjectKind { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string SourceMicrotilePreviewId { get; init; } = string.Empty;
    public bool RequiresWaterAdjacency { get; init; }
    public bool RequiresRoadAdjacency { get; init; }
    public bool RequiresLandCell { get; init; } = true;
    public IReadOnlyList<string> MetadataTags { get; init; } = [];
}

public sealed record VisualMapPatchRoadPath
{
    public string PathId { get; init; } = string.Empty;
    public string PathKind { get; init; } = "road";
    public IReadOnlyList<VisualMapPatchPathNode> Nodes { get; init; } = [];
}

public sealed record VisualMapPatchWaterFlow
{
    public string FlowId { get; init; } = string.Empty;
    public VisualMapPatchWaterKind WaterKind { get; init; }
    public IReadOnlyList<VisualMapPatchPathNode> Nodes { get; init; } = [];
}

public sealed record VisualMapPatchPathNode
{
    public int X { get; init; }
    public int Y { get; init; }
    public IReadOnlyList<VisualMapPatchConnector> Connectors { get; init; } = [];
}

public sealed record VisualMapPatchBiomeTransition
{
    public string TransitionId { get; init; } = string.Empty;
    public VisualMapPatchTerrainBiome FromBiome { get; init; }
    public VisualMapPatchTerrainBiome ToBiome { get; init; }
    public IReadOnlyList<VisualMapPatchCoordinate> Cells { get; init; } = [];
}

public sealed record VisualMapPatchSettlementAnchor
{
    public string SettlementId { get; init; } = string.Empty;
    public string SettlementRole { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string SourceMicrotilePreviewId { get; init; } = string.Empty;
    public string NearPathId { get; init; } = string.Empty;
    public string NearResourceAnchorId { get; init; } = string.Empty;
}

public sealed record VisualMapPatchCreatureMarker
{
    public string MarkerId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public string SourceMicrotilePreviewId { get; init; } = string.Empty;
    public string BodyPlanId { get; init; } = string.Empty;
    public string EquipmentProfileId { get; init; } = string.Empty;
    public string StateMetadataId { get; init; } = string.Empty;
    public bool RatingSafe { get; init; } = true;
}

public sealed record VisualMapPatchOverlay
{
    public string OverlayId { get; init; } = string.Empty;
    public string OverlayKind { get; init; } = string.Empty;
    public string SourceMicrotilePreviewId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; } = 1;
    public int Height { get; init; } = 1;
    public string DayNightMetadata { get; init; } = string.Empty;
    public string WeatherMetadata { get; init; } = string.Empty;
    public string EffectMetadata { get; init; } = string.Empty;
    public bool AdultMetadataOnly { get; init; }
    public string SafeFallbackMicrotilePreviewId { get; init; } = string.Empty;
    public VisualMapPatchProviderState ProviderState { get; init; } = VisualMapPatchProviderState.MetadataOnly;
    public bool TreatProviderCandidateAsApprovedOutput { get; init; }
}

public sealed record VisualMapPatchSourceReference
{
    public string SourceKind { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
}

public sealed record VisualMapPatchCoordinate
{
    public int X { get; init; }
    public int Y { get; init; }
}

public sealed record VisualMapPatchValidationResult
{
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<VisualMapPatchDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualMapPatchDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static VisualMapPatchDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record VisualMapPatchCatalog
{
    public string SchemaVersion { get; init; } = DeterministicVisualMapPatchComposerVocabulary.CatalogSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMapPatchComposerVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualMapPatchComposerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PatchCount { get; init; }
    public int TotalCellCount { get; init; }
    public IReadOnlyList<VisualMapPatchCatalogEntry> Patches { get; init; } = [];
}

public sealed record VisualMapPatchCatalogEntry
{
    public string PatchId { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public int Seed { get; init; }
    public string PatchSvgRelativePath { get; init; } = string.Empty;
    public int CellCount { get; init; }
    public int ObjectAnchorCount { get; init; }
    public int RoadPathCount { get; init; }
    public int WaterFlowCount { get; init; }
    public int SettlementAnchorCount { get; init; }
    public int CreatureMarkerCount { get; init; }
    public int OverlayCount { get; init; }
    public IReadOnlyList<string> ReferencedMicrotilePreviewIds { get; init; } = [];
    public IReadOnlyList<VisualMapPatchCell> Cells { get; init; } = [];
}

public sealed record VisualMapPatchMaterializationManifest
{
    public string SchemaVersion { get; init; } = DeterministicVisualMapPatchComposerVocabulary.ManifestSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMapPatchComposerVocabulary.GoalId;
    public string GeneratorVersion { get; init; } = string.Empty;
    public string OutputRelativeDirectory { get; init; } = DeterministicVisualMapPatchComposerVocabulary.RelativeOutputDirectory;
    public int PatchCount { get; init; }
    public IReadOnlyList<VisualMapPatchMaterializedPatch> Patches { get; init; } = [];
}

public sealed record VisualMapPatchMaterializedPatch
{
    public string PatchId { get; init; } = string.Empty;
    public string PatchSvgRelativePath { get; init; } = string.Empty;
    public string SvgSha256 { get; init; } = string.Empty;
    public int ByteLength { get; init; }
    public bool ContainsViewBox { get; init; }
    public bool ExternalResourceFree { get; init; }
    public bool ScriptFree { get; init; }
    public int RectCount { get; init; }
}

public sealed record VisualMapPatchFileLedger
{
    public string SchemaVersion { get; init; } = DeterministicVisualMapPatchComposerVocabulary.FileLedgerSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMapPatchComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool ExcludesSelfAndReportByDesign { get; init; } = true;
    public int FileCount { get; init; }
    public IReadOnlyList<VisualMapPatchFileLedgerEntry> Files { get; init; } = [];
}

public sealed record VisualMapPatchFileLedgerEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Sha256 { get; init; } = string.Empty;
    public int ByteLength { get; init; }
}

public sealed record VisualMapPatchWaterFlowProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualMapPatchComposerVocabulary.WaterFlowProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMapPatchComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool SeaCovered { get; init; }
    public bool CoastCovered { get; init; }
    public bool RiverCovered { get; init; }
    public bool LakeCovered { get; init; }
    public bool MarshCovered { get; init; }
    public bool BridgeCovered { get; init; }
    public bool DockCovered { get; init; }
    public int FlowConnectorCount { get; init; }
    public IReadOnlyList<VisualMapPatchWaterFlowProofRow> Rows { get; init; } = [];
}

public sealed record VisualMapPatchWaterFlowProofRow
{
    public string PatchId { get; init; } = string.Empty;
    public int WaterCellCount { get; init; }
    public int CoastCellCount { get; init; }
    public int RiverNodeCount { get; init; }
    public int LakeCellCount { get; init; }
    public int MarshCellCount { get; init; }
}

public sealed record VisualMapPatchReachabilityProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualMapPatchComposerVocabulary.ReachabilityProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMapPatchComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool RoadsConnected { get; init; }
    public bool SettlementsReachable { get; init; }
    public bool ObjectsReachable { get; init; }
    public int RoadNodeCount { get; init; }
    public int SettlementAnchorCount { get; init; }
    public int ObjectAnchorCount { get; init; }
    public IReadOnlyList<VisualMapPatchReachabilityProofRow> Rows { get; init; } = [];
}

public sealed record VisualMapPatchReachabilityProofRow
{
    public string PatchId { get; init; } = string.Empty;
    public int RoadPathCount { get; init; }
    public int RoadNodeCount { get; init; }
    public int SettlementAnchorCount { get; init; }
    public int ObjectAnchorCount { get; init; }
}

public sealed record VisualMapPatchLayeringProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualMapPatchComposerVocabulary.LayeringProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMapPatchComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool LayerOrderingStable { get; init; }
    public bool TerrainWaterRoadObjectSettlementCreatureOverlayLayersCovered { get; init; }
    public bool AdultMetadataFallbackBound { get; init; }
    public IReadOnlyList<VisualMapPatchLayeringProofRow> Rows { get; init; } = [];
}

public sealed record VisualMapPatchLayeringProofRow
{
    public string PatchId { get; init; } = string.Empty;
    public IReadOnlyList<VisualMapPatchLayerKind> LayerKinds { get; init; } = [];
    public int OverlayCount { get; init; }
    public int AdultMetadataOnlyOverlayCount { get; init; }
}

public sealed record VisualMapPatchNegativeProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualMapPatchComposerVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMapPatchComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<VisualMapPatchNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record VisualMapPatchNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<VisualMapPatchDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualMapPatchSourceLineage
{
    public string SchemaVersion { get; init; } = DeterministicVisualMapPatchComposerVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMapPatchComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal084ArtifactsGreen { get; init; }
    public bool Goal084AcceptedFalse { get; init; }
    public bool Goal085ArtifactsGreen { get; init; }
    public bool Goal085AcceptedFalse { get; init; }
    public bool Goal086ArtifactsGreen { get; init; }
    public bool Goal086AcceptedFalse { get; init; }
    public bool Goal086CatalogRead { get; init; }
    public int SourceRecordCount { get; init; }
    public IReadOnlyList<VisualMapPatchSourceLineageRecord> Records { get; init; } = [];
}

public sealed record VisualMapPatchSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record VisualMapPatchQualityGateScan
{
    public string SchemaVersion { get; init; } = DeterministicVisualMapPatchComposerVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualMapPatchComposerVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualMapPatchComposerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool PatchCountPassed { get; init; }
    public bool DeterministicRerunStable { get; init; }
    public bool SvgTextOnlyPreviews { get; init; }
    public bool AllReferencesKnownGoal086Microtiles { get; init; }
    public bool WaterFlowProofPassed { get; init; }
    public bool ReachabilityProofPassed { get; init; }
    public bool LayeringProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool NoForbiddenFilesChanged { get; init; } = true;
    public bool NoExternalDependenciesAdded { get; init; } = true;
    public bool NoBinaryOrRasterMediaAdded { get; init; } = true;
    public bool NoPromptDumps { get; init; } = true;
    public bool NoProviderCalls { get; init; } = true;
    public bool NoExplicitAdultContent { get; init; } = true;
    public bool ArtifactScopeReady { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<VisualMapPatchDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualMapPatchReport
{
    public string GoalId { get; init; } = DeterministicVisualMapPatchComposerVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualMapPatchComposerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public int PatchCount { get; init; }
    public int TotalCellCount { get; init; }
    public bool ValidationPassed { get; init; }
    public bool WaterFlowProofPassed { get; init; }
    public bool ReachabilityProofPassed { get; init; }
    public bool LayeringProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public string CatalogHash { get; init; } = string.Empty;
    public string MaterializationManifestHash { get; init; } = string.Empty;
    public string FileLedgerHash { get; init; } = string.Empty;
    public string WaterFlowProofHash { get; init; } = string.Empty;
    public string ReachabilityProofHash { get; init; } = string.Empty;
    public string LayeringProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record VisualMapPatchEvidenceResult
{
    public VisualMapPatchCatalog Catalog { get; init; } = new();
    public VisualMapPatchMaterializationManifest MaterializationManifest { get; init; } = new();
    public VisualMapPatchFileLedger FileLedger { get; init; } = new();
    public VisualMapPatchWaterFlowProof WaterFlowProof { get; init; } = new();
    public VisualMapPatchReachabilityProof ReachabilityProof { get; init; } = new();
    public VisualMapPatchLayeringProof LayeringProof { get; init; } = new();
    public VisualMapPatchNegativeProof NegativeProof { get; init; } = new();
    public VisualMapPatchSourceLineage SourceLineage { get; init; } = new();
    public VisualMapPatchQualityGateScan QualityGateScan { get; init; } = new();
    public VisualMapPatchReport Report { get; init; } = new();
    public string CatalogJson { get; init; } = string.Empty;
    public string MaterializationManifestJson { get; init; } = string.Empty;
    public string FileLedgerJson { get; init; } = string.Empty;
    public string WaterFlowProofJson { get; init; } = string.Empty;
    public string ReachabilityProofJson { get; init; } = string.Empty;
    public string LayeringProofJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string SourceLineageJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> SvgByPatchId { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

public sealed record VisualMapPatchWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string PatchDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string CatalogJsonPath { get; init; } = string.Empty;
    public string MaterializationManifestJsonPath { get; init; } = string.Empty;
    public string FileLedgerJsonPath { get; init; } = string.Empty;
    public string WaterFlowProofJsonPath { get; init; } = string.Empty;
    public string ReachabilityProofJsonPath { get; init; } = string.Empty;
    public string LayeringProofJsonPath { get; init; } = string.Empty;
    public string NegativeProofJsonPath { get; init; } = string.Empty;
    public string SourceLineageJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
    public IReadOnlyList<string> PatchSvgPaths { get; init; } = [];
}
