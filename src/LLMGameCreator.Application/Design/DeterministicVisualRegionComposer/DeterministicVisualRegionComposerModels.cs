namespace LLMGameCreator.Application.Design.DeterministicVisualRegionComposer;

public static class DeterministicVisualRegionComposerVocabulary
{
    public const string GoalId = "goal_088_deterministic_visual_region_composer";
    public const string ProductSmokeRoute = "goal-088-deterministic-visual-region-composer";
    public const string FinalGate = "deterministic_visual_region_composer_verification";
    public const string RelativeOutputDirectory = ".llmgc/procedural/goal-088-deterministic-visual-region-composer";

    public const string RegionId = "heroes_scale_surface_underground_144x144";
    public const string SurfaceLayerId = "surface";
    public const string UndergroundLayerId = "underground";

    public const string DefinitionSchemaVersion = "visual_region_definition_v1";
    public const string PatchPlacementIndexSchemaVersion = "visual_region_patch_placement_index_v1";
    public const string ChunkIndexSchemaVersion = "visual_region_chunk_index_v1";
    public const string BiomeDistributionProofSchemaVersion = "visual_region_biome_distribution_proof_v1";
    public const string WaterNetworkProofSchemaVersion = "visual_region_water_network_proof_v1";
    public const string RoadReachabilityProofSchemaVersion = "visual_region_road_reachability_proof_v1";
    public const string LayerTransitionProofSchemaVersion = "visual_region_layer_transition_proof_v1";
    public const string ObjectPlacementProofSchemaVersion = "visual_region_object_placement_proof_v1";
    public const string NegativeProofSchemaVersion = "visual_region_negative_proof_v1";
    public const string SourceLineageSchemaVersion = "visual_region_source_lineage_v1";
    public const string QualityGateSchemaVersion = "visual_region_quality_gate_scan_v1";

    public const int RegionWidth = 144;
    public const int RegionHeight = 144;
    public const int PatchWidth = 24;
    public const int PatchHeight = 16;
    public const int PatchGridColumns = 6;
    public const int PatchGridRows = 9;
    public const int LayerCount = 2;
    public const int PatchPlacementsPerLayer = PatchGridColumns * PatchGridRows;
    public const int TotalPatchPlacements = PatchPlacementsPerLayer * LayerCount;
    public const int DerivedLogicalCellCount = RegionWidth * RegionHeight * LayerCount;
}

public enum VisualRegionProviderState
{
    None = 0,
    MetadataOnly,
    CandidateQuarantine,
    ApprovedOutput,
    Rejected
}

public sealed record VisualRegionDefinition
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.DefinitionSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualRegionComposerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public string RegionId { get; init; } = DeterministicVisualRegionComposerVocabulary.RegionId;
    public int Seed { get; init; }
    public int Width { get; init; } = DeterministicVisualRegionComposerVocabulary.RegionWidth;
    public int Height { get; init; } = DeterministicVisualRegionComposerVocabulary.RegionHeight;
    public int LayerCount { get; init; } = DeterministicVisualRegionComposerVocabulary.LayerCount;
    public int PatchWidth { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchWidth;
    public int PatchHeight { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchHeight;
    public int PatchGridColumns { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchGridColumns;
    public int PatchGridRows { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchGridRows;
    public int DerivedLogicalCellCount { get; init; } = DeterministicVisualRegionComposerVocabulary.DerivedLogicalCellCount;
    public int ExplicitRawCellRecordCount { get; init; }
    public bool HeavyRawCellMode { get; init; }
    public string OutputRelativeDirectory { get; init; } = DeterministicVisualRegionComposerVocabulary.RelativeOutputDirectory;
    public string SourceOfTruthKind { get; init; } = "metadata_contract";
    public bool PromptTextIsSourceOfTruth { get; init; }
    public bool TreatProviderCandidateAsApprovedOutput { get; init; }
    public bool SourceGoal084085086087LineageRequired { get; init; } = true;
    public IReadOnlyList<VisualRegionLayer> Layers { get; init; } = [];
    public IReadOnlyList<VisualRegionBiomeBand> BiomeBands { get; init; } = [];
    public VisualRegionWaterNetwork WaterNetwork { get; init; } = new();
    public VisualRegionRoadNetwork RoadNetwork { get; init; } = new();
    public IReadOnlyList<VisualRegionSettlementPlacement> Settlements { get; init; } = [];
    public IReadOnlyList<VisualRegionGateTransition> GateTransitions { get; init; } = [];
    public IReadOnlyList<VisualRegionObjectPlacement> ObjectPlacements { get; init; } = [];
    public IReadOnlyList<VisualRegionCreaturePlacement> CreaturePlacements { get; init; } = [];
    public IReadOnlyList<VisualRegionOverlay> Overlays { get; init; } = [];
    public IReadOnlyList<VisualRegionSourceReference> SourceReferences { get; init; } = [];
}

public sealed record VisualRegionLayer
{
    public string LayerId { get; init; } = string.Empty;
    public int Width { get; init; } = DeterministicVisualRegionComposerVocabulary.RegionWidth;
    public int Height { get; init; } = DeterministicVisualRegionComposerVocabulary.RegionHeight;
    public int PatchGridColumns { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchGridColumns;
    public int PatchGridRows { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchGridRows;
    public IReadOnlyList<VisualRegionPatchPlacement> PatchPlacements { get; init; } = [];
    public IReadOnlyList<VisualRegionChunk> Chunks { get; init; } = [];
}

public sealed record VisualRegionPatchPlacement
{
    public string PlacementId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string SourceGoal087PatchId { get; init; } = string.Empty;
    public int GridX { get; init; }
    public int GridY { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchWidth;
    public int Height { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchHeight;
    public VisualRegionPatchTransform Transform { get; init; } = new();
    public IReadOnlyList<string> DeclaredBiomes { get; init; } = [];
    public IReadOnlyList<string> DeclaredWaterKinds { get; init; } = [];
    public VisualRegionEdgeConnectors WaterConnectors { get; init; } = new();
    public VisualRegionEdgeConnectors RoadConnectors { get; init; } = new();
    public IReadOnlyList<string> MetadataTags { get; init; } = [];
}

public sealed record VisualRegionPatchTransform
{
    public int RotationDegrees { get; init; }
    public bool MirrorX { get; init; }
    public bool MirrorY { get; init; }
    public string RepaletteProfileId { get; init; } = "palette/default";
}

public sealed record VisualRegionEdgeConnectors
{
    public string North { get; init; } = "none";
    public string East { get; init; } = "none";
    public string South { get; init; } = "none";
    public string West { get; init; } = "none";
}

public sealed record VisualRegionChunk
{
    public string ChunkId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string PlacementId { get; init; } = string.Empty;
    public int GridX { get; init; }
    public int GridY { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchWidth;
    public int Height { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchHeight;
    public string DominantBiome { get; init; } = string.Empty;
    public string DominantWaterKind { get; init; } = "none";
    public IReadOnlyList<string> CompactRleRows { get; init; } = [];
    public IReadOnlyList<string> SummaryTags { get; init; } = [];
}

public sealed record VisualRegionBiomeBand
{
    public string BandId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string BiomeId { get; init; } = string.Empty;
    public int EstimatedCellCount { get; init; }
    public IReadOnlyList<string> CompactRleRows { get; init; } = [];
}

public sealed record VisualRegionWaterNetwork
{
    public bool DeclaresWater { get; init; }
    public bool DeclaresLavaBoundaryMetadata { get; init; }
    public IReadOnlyList<VisualRegionWaterSegment> Segments { get; init; } = [];
}

public sealed record VisualRegionWaterSegment
{
    public string SegmentId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string WaterKind { get; init; } = string.Empty;
    public IReadOnlyList<string> ConnectedPlacementIds { get; init; } = [];
    public IReadOnlyList<string> CrossingObjectIds { get; init; } = [];
    public bool BoundaryConnectorsValid { get; init; }
}

public sealed record VisualRegionRoadNetwork
{
    public string NetworkId { get; init; } = "region_road_network";
    public IReadOnlyList<VisualRegionRoadNode> Nodes { get; init; } = [];
    public IReadOnlyList<VisualRegionRoadEdge> Edges { get; init; } = [];
}

public sealed record VisualRegionRoadNode
{
    public string NodeId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string PlacementId { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool RequiredAnchor { get; init; }
}

public sealed record VisualRegionRoadEdge
{
    public string EdgeId { get; init; } = string.Empty;
    public string FromNodeId { get; init; } = string.Empty;
    public string ToNodeId { get; init; } = string.Empty;
    public string EdgeKind { get; init; } = "road";
}

public sealed record VisualRegionSettlementPlacement
{
    public string SettlementId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string PlacementId { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string TerrainKind { get; init; } = string.Empty;
    public string RoadNodeId { get; init; } = string.Empty;
}

public sealed record VisualRegionGateTransition
{
    public string TransitionId { get; init; } = string.Empty;
    public string SurfacePlacementId { get; init; } = string.Empty;
    public string UndergroundPlacementId { get; init; } = string.Empty;
    public string SurfaceGateId { get; init; } = string.Empty;
    public string UndergroundGateId { get; init; } = string.Empty;
    public bool Paired { get; init; }
}

public sealed record VisualRegionObjectPlacement
{
    public string ObjectId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string PlacementId { get; init; } = string.Empty;
    public string ObjectKind { get; init; } = string.Empty;
    public string TerrainKind { get; init; } = string.Empty;
    public string RoadNodeId { get; init; } = string.Empty;
    public bool RequiresRoadConnection { get; init; }
    public bool RequiresWaterAdjacency { get; init; }
    public bool RequiresPassableTerrain { get; init; } = true;
}

public sealed record VisualRegionCreaturePlacement
{
    public string CreatureId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string PlacementId { get; init; } = string.Empty;
    public string BodyPlanId { get; init; } = string.Empty;
    public string EquipmentProfileId { get; init; } = string.Empty;
    public string StateMetadataId { get; init; } = string.Empty;
    public bool RatingSafe { get; init; } = true;
}

public sealed record VisualRegionOverlay
{
    public string OverlayId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string PlacementId { get; init; } = string.Empty;
    public string OverlayKind { get; init; } = string.Empty;
    public string DayNightMetadata { get; init; } = string.Empty;
    public string WeatherMetadata { get; init; } = string.Empty;
    public string EffectMetadata { get; init; } = string.Empty;
    public bool AdultMetadataOnly { get; init; }
    public string SafeFallbackRefId { get; init; } = string.Empty;
    public VisualRegionProviderState ProviderState { get; init; } = VisualRegionProviderState.MetadataOnly;
    public bool TreatProviderCandidateAsApprovedOutput { get; init; }
}

public sealed record VisualRegionSourceReference
{
    public string SourceKind { get; init; } = string.Empty;
    public string SourceId { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
}

public sealed record VisualRegionValidationResult
{
    public bool Passed { get; init; }
    public int DiagnosticCount { get; init; }
    public IReadOnlyList<VisualRegionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualRegionDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static VisualRegionDiagnostic Error(string code, string target, string message) =>
        new() { Severity = "error", Code = code, Target = target, Message = message };
}

public sealed record VisualRegionPatchPlacementIndex
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.PatchPlacementIndexSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public int PatchPlacementCount { get; init; }
    public int SurfacePatchPlacementCount { get; init; }
    public int UndergroundPatchPlacementCount { get; init; }
    public int DerivedLogicalCellCount { get; init; }
    public bool AllPatchIdsKnownGoal087 { get; init; }
    public IReadOnlyList<VisualRegionPatchPlacementIndexRow> Placements { get; init; } = [];
}

public sealed record VisualRegionPatchPlacementIndexRow
{
    public string PlacementId { get; init; } = string.Empty;
    public string LayerId { get; init; } = string.Empty;
    public string SourceGoal087PatchId { get; init; } = string.Empty;
    public int GridX { get; init; }
    public int GridY { get; init; }
    public string TransformSummary { get; init; } = string.Empty;
}

public sealed record VisualRegionChunkIndex
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.ChunkIndexSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ChunkCount { get; init; }
    public IReadOnlyList<VisualRegionChunk> Chunks { get; init; } = [];
}

public sealed record VisualRegionBiomeDistributionProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.BiomeDistributionProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool SurfaceCoveragePassed { get; init; }
    public bool UndergroundCoveragePassed { get; init; }
    public IReadOnlyList<VisualRegionBiomeBand> Bands { get; init; } = [];
}

public sealed record VisualRegionWaterNetworkProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.WaterNetworkProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool SeaCovered { get; init; }
    public bool LakeCovered { get; init; }
    public bool CoastCovered { get; init; }
    public bool RiverCovered { get; init; }
    public bool MarshCovered { get; init; }
    public bool BridgeCovered { get; init; }
    public bool DockCovered { get; init; }
    public bool UndergroundWaterCovered { get; init; }
    public bool LavaBoundaryMetadataCovered { get; init; }
    public bool ConnectorMismatchesRejectedByValidator { get; init; }
    public int SegmentCount { get; init; }
}

public sealed record VisualRegionRoadReachabilityProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.RoadReachabilityProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool RoadsConnected { get; init; }
    public bool SettlementCastleGarrisonCaravanAnchorsReachable { get; init; }
    public bool ObjectAnchorsReachable { get; init; }
    public int RoadNodeCount { get; init; }
    public int RoadEdgeCount { get; init; }
    public IReadOnlyList<string> ReachableRequiredAnchorIds { get; init; } = [];
}

public sealed record VisualRegionLayerTransitionProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.LayerTransitionProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public int GatePairCount { get; init; }
    public IReadOnlyList<VisualRegionGateTransition> GateTransitions { get; init; } = [];
}

public sealed record VisualRegionObjectPlacementProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.ObjectPlacementProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public int SettlementCount { get; init; }
    public int ObjectCount { get; init; }
    public int CreatureCount { get; init; }
    public bool CastleSettlementGarrisonCaravanCovered { get; init; }
    public bool MineBridgeDockObjectCreatureCovered { get; init; }
}

public sealed record VisualRegionNegativeProof
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.NegativeProofSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public int ScenarioCount { get; init; }
    public int RejectedCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public IReadOnlyList<VisualRegionNegativeScenario> Scenarios { get; init; } = [];
}

public sealed record VisualRegionNegativeScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string CausalMutation { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<VisualRegionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualRegionSourceLineage
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.SourceLineageSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public bool Passed { get; init; }
    public bool Goal084ArtifactsGreen { get; init; }
    public bool Goal085ArtifactsGreen { get; init; }
    public bool Goal086ArtifactsGreen { get; init; }
    public bool Goal087ArtifactsGreen { get; init; }
    public bool Goal087AcceptedFalseArtifactPreserved { get; init; }
    public bool Goal087CatalogRead { get; init; }
    public int SourceRecordCount { get; init; }
    public IReadOnlyList<VisualRegionSourceLineageRecord> Records { get; init; } = [];
}

public sealed record VisualRegionSourceLineageRecord
{
    public string RelativePath { get; init; } = string.Empty;
    public bool Exists { get; init; }
    public string Sha256 { get; init; } = string.Empty;
    public IReadOnlyList<string> PurposeTags { get; init; } = [];
}

public sealed record VisualRegionQualityGateScan
{
    public string SchemaVersion { get; init; } = DeterministicVisualRegionComposerVocabulary.QualityGateSchemaVersion;
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualRegionComposerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public bool DimensionsPassed { get; init; }
    public bool PatchPlacementCountPassed { get; init; }
    public bool CompactArtifactsPassed { get; init; }
    public bool Goal087ReferencesPassed { get; init; }
    public bool WaterNetworkProofPassed { get; init; }
    public bool RoadReachabilityProofPassed { get; init; }
    public bool LayerTransitionProofPassed { get; init; }
    public bool ObjectPlacementProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool SourceLineagePassed { get; init; }
    public bool SafeSvgOverviewsPassed { get; init; }
    public bool NoForbiddenFilesChanged { get; init; } = true;
    public bool NoRuntimeUnityProviderSchemaProjectDependencyChanges { get; init; } = true;
    public bool NoBinaryOrRasterMediaAdded { get; init; } = true;
    public bool NoPromptDumps { get; init; } = true;
    public bool NoExplicitAdultContent { get; init; } = true;
    public bool ArtifactScopeReady { get; init; }
    public IReadOnlyList<string> ExpectedChangedPathPrefixes { get; init; } = [];
    public IReadOnlyList<VisualRegionDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record VisualRegionReport
{
    public string GoalId { get; init; } = DeterministicVisualRegionComposerVocabulary.GoalId;
    public string ManualGate { get; init; } = DeterministicVisualRegionComposerVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public bool Accepted { get; init; }
    public string RegionId { get; init; } = DeterministicVisualRegionComposerVocabulary.RegionId;
    public int SurfaceWidth { get; init; }
    public int SurfaceHeight { get; init; }
    public int UndergroundWidth { get; init; }
    public int UndergroundHeight { get; init; }
    public int PatchPlacementCount { get; init; }
    public int DerivedLogicalCellCount { get; init; }
    public bool ValidationPassed { get; init; }
    public bool CompactArtifactsPassed { get; init; }
    public bool WaterNetworkProofPassed { get; init; }
    public bool RoadReachabilityProofPassed { get; init; }
    public bool LayerTransitionProofPassed { get; init; }
    public bool ObjectPlacementProofPassed { get; init; }
    public bool NegativeProofPassed { get; init; }
    public bool QualityGatePassed { get; init; }
    public string DefinitionHash { get; init; } = string.Empty;
    public string PatchPlacementIndexHash { get; init; } = string.Empty;
    public string ChunkIndexHash { get; init; } = string.Empty;
    public string BiomeDistributionProofHash { get; init; } = string.Empty;
    public string WaterNetworkProofHash { get; init; } = string.Empty;
    public string RoadReachabilityProofHash { get; init; } = string.Empty;
    public string LayerTransitionProofHash { get; init; } = string.Empty;
    public string ObjectPlacementProofHash { get; init; } = string.Empty;
    public string NegativeProofHash { get; init; } = string.Empty;
    public string SourceLineageHash { get; init; } = string.Empty;
    public string QualityGateHash { get; init; } = string.Empty;
    public string SurfaceOverviewHash { get; init; } = string.Empty;
    public string UndergroundOverviewHash { get; init; } = string.Empty;
    public string CombinedOverviewHash { get; init; } = string.Empty;
    public string DeterministicReportHash { get; init; } = string.Empty;
}

public sealed record VisualRegionEvidenceResult
{
    public VisualRegionDefinition Definition { get; init; } = new();
    public VisualRegionPatchPlacementIndex PatchPlacementIndex { get; init; } = new();
    public VisualRegionChunkIndex ChunkIndex { get; init; } = new();
    public VisualRegionBiomeDistributionProof BiomeDistributionProof { get; init; } = new();
    public VisualRegionWaterNetworkProof WaterNetworkProof { get; init; } = new();
    public VisualRegionRoadReachabilityProof RoadReachabilityProof { get; init; } = new();
    public VisualRegionLayerTransitionProof LayerTransitionProof { get; init; } = new();
    public VisualRegionObjectPlacementProof ObjectPlacementProof { get; init; } = new();
    public VisualRegionNegativeProof NegativeProof { get; init; } = new();
    public VisualRegionSourceLineage SourceLineage { get; init; } = new();
    public VisualRegionQualityGateScan QualityGateScan { get; init; } = new();
    public VisualRegionReport Report { get; init; } = new();
    public string DefinitionJson { get; init; } = string.Empty;
    public string PatchPlacementIndexJson { get; init; } = string.Empty;
    public string ChunkIndexJson { get; init; } = string.Empty;
    public string BiomeDistributionProofJson { get; init; } = string.Empty;
    public string WaterNetworkProofJson { get; init; } = string.Empty;
    public string RoadReachabilityProofJson { get; init; } = string.Empty;
    public string LayerTransitionProofJson { get; init; } = string.Empty;
    public string ObjectPlacementProofJson { get; init; } = string.Empty;
    public string NegativeProofJson { get; init; } = string.Empty;
    public string SourceLineageJson { get; init; } = string.Empty;
    public string QualityGateScanJson { get; init; } = string.Empty;
    public string ReportMarkdown { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> OverviewSvgByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

public sealed record VisualRegionWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public string ReportMarkdownPath { get; init; } = string.Empty;
    public string DefinitionJsonPath { get; init; } = string.Empty;
    public string PatchPlacementIndexJsonPath { get; init; } = string.Empty;
    public string ChunkIndexJsonPath { get; init; } = string.Empty;
    public string BiomeDistributionProofJsonPath { get; init; } = string.Empty;
    public string WaterNetworkProofJsonPath { get; init; } = string.Empty;
    public string RoadReachabilityProofJsonPath { get; init; } = string.Empty;
    public string LayerTransitionProofJsonPath { get; init; } = string.Empty;
    public string ObjectPlacementProofJsonPath { get; init; } = string.Empty;
    public string NegativeProofJsonPath { get; init; } = string.Empty;
    public string SourceLineageJsonPath { get; init; } = string.Empty;
    public string QualityGateScanJsonPath { get; init; } = string.Empty;
    public IReadOnlyList<string> OverviewSvgPaths { get; init; } = [];
}
