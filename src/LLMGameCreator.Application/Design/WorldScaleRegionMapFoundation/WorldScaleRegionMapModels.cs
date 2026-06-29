using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

public static class WorldScaleRegionMapVocabulary
{
    public const string SchemaVersion = "world_scale_region_map_foundation_v1";
    public const string GoalId = "goal_038_world_scale_region_map_foundation";
    public const string FinalGate = "world_scale_region_map_foundation_verification";
    public const string ProductSmokeRoute = "goal-038-world-scale-region-map-foundation";

    public static readonly IReadOnlySet<string> Scenarios = new HashSet<string>(
        ["frontier_survival", "gothic_intrigue", "caravan_trade", "metamodule_kingdoms"],
        StringComparer.Ordinal);

    public static readonly IReadOnlySet<string> RequiredRouteKinds = new HashSet<string>(
        ["road", "trail", "river", "mountain_pass", "sea_lane", "caravan_route", "dungeon_descent", "magical_gate"],
        StringComparer.Ordinal);
}

public sealed record WorldScaleBoundaryClaims
{
    public bool RuntimeMutation { get; init; }
    public bool UiWinForms { get; init; }
    public bool Unity { get; init; }
    public bool GamePackageSchema { get; init; }
    public bool ProviderLlmRag { get; init; }
    public bool LuaSourceOrExecution { get; init; }
    public bool GeneratorLibrary { get; init; }
    public bool ExternalDependency { get; init; }

    [JsonIgnore]
    public bool AllFalse =>
        !RuntimeMutation &&
        !UiWinForms &&
        !Unity &&
        !GamePackageSchema &&
        !ProviderLlmRag &&
        !LuaSourceOrExecution &&
        !GeneratorLibrary &&
        !ExternalDependency;
}

public sealed record WorldScaleSourceEvidenceRef
{
    public string SourceGoal { get; init; } = string.Empty;
    public string EvidenceId { get; init; } = string.Empty;
    public string ArtifactFamily { get; init; } = string.Empty;
}

public sealed record WorldScaleKingdomGroup
{
    public string KingdomId { get; init; } = string.Empty;
    public string RegionGroupId { get; init; } = string.Empty;
    public IReadOnlyList<string> RegionIds { get; init; } = [];
    public IReadOnlyList<string> SpeciesArchetypeSlotRefs { get; init; } = [];
    public IReadOnlyList<string> SemanticTags { get; init; } = [];
}

public sealed record WorldScaleRegionNode
{
    public string RegionId { get; init; } = string.Empty;
    public string KingdomId { get; init; } = string.Empty;
    public IReadOnlyList<string> BiomeTags { get; init; } = [];
    public IReadOnlyList<string> TerrainTags { get; init; } = [];
    public IReadOnlyList<string> HazardTags { get; init; } = [];
    public IReadOnlyList<string> WeatherTags { get; init; } = [];
    public IReadOnlyList<string> EventTags { get; init; } = [];
    public IReadOnlyList<string> SettlementIds { get; init; } = [];
    public IReadOnlyList<string> LandmarkIds { get; init; } = [];
    public bool RequiredGameplayTarget { get; init; }
    public bool OptionalTarget { get; init; }
    public IReadOnlyList<WorldScaleSourceEvidenceRef> SourceEvidenceRefs { get; init; } = [];
}

public sealed record WorldScaleTravelEdge
{
    public string EdgeId { get; init; } = string.Empty;
    public string FromRegionId { get; init; } = string.Empty;
    public string ToRegionId { get; init; } = string.Empty;
    public string RouteKind { get; init; } = string.Empty;
    public int Cost { get; init; }
    public bool Bidirectional { get; init; }
    public bool IsBlocked { get; init; }
    public bool IsConditional { get; init; }
    public bool FutureRequired { get; init; }
    public IReadOnlyList<string> Constraints { get; init; } = [];
    public IReadOnlyList<string> SemanticTags { get; init; } = [];
    public IReadOnlyList<WorldScaleSourceEvidenceRef> SourceEvidenceRefs { get; init; } = [];

    [JsonIgnore]
    public bool IsTraversableNow => !IsBlocked && !IsConditional && !FutureRequired;
}

public sealed record WorldScaleRegionGraph
{
    public string SchemaVersion { get; init; } = "world_scale_region_graph_v1";
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string WorldGraphId { get; init; } = string.Empty;
    public string DeterministicSeed { get; init; } = string.Empty;
    public string StartRegionId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredTargetRegionIds { get; init; } = [];
    public IReadOnlyList<string> OptionalTargetRegionIds { get; init; } = [];
    public IReadOnlyList<WorldScaleKingdomGroup> Kingdoms { get; init; } = [];
    public IReadOnlyList<WorldScaleRegionNode> Regions { get; init; } = [];
    public IReadOnlyList<WorldScaleTravelEdge> TravelEdges { get; init; } = [];
    public IReadOnlyList<WorldScaleSourceEvidenceRef> SourceEvidenceRefs { get; init; } = [];
    public WorldScaleBoundaryClaims BoundaryClaims { get; init; } = new();
}

public sealed record WorldScaleRegionGraphSummary
{
    public string SchemaVersion { get; init; } = "world_scale_region_graph_summary_v1";
    public string GoalId { get; init; } = WorldScaleRegionMapVocabulary.GoalId;
    public string FinalGate { get; init; } = WorldScaleRegionMapVocabulary.FinalGate;
    public bool Accepted { get; init; }
    public int ScenarioCount { get; init; }
    public int TotalKingdomCount { get; init; }
    public int TotalRegionCount { get; init; }
    public int TotalTravelEdgeCount { get; init; }
    public IReadOnlyList<string> RouteKindsCovered { get; init; } = [];
    public IReadOnlyList<WorldScaleRegionGraph> Graphs { get; init; } = [];
}

public sealed record WorldScaleReachabilityMatrix
{
    public string SchemaVersion { get; init; } = "world_scale_reachability_matrix_v1";
    public int ScenarioCount { get; init; }
    public int RequiredTargetCount { get; init; }
    public int ReachableRequiredTargetCount { get; init; }
    public bool AllRequiredTargetsReachable { get; init; }
    public IReadOnlyList<WorldScaleReachabilityScenario> Scenarios { get; init; } = [];
}

public sealed record WorldScaleReachabilityScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string StartRegionId { get; init; } = string.Empty;
    public IReadOnlyList<string> RequiredTargetRegionIds { get; init; } = [];
    public IReadOnlyList<string> ReachableRegionIds { get; init; } = [];
    public IReadOnlyList<string> UnreachableRequiredRegionIds { get; init; } = [];
    public IReadOnlyDictionary<string, int> RouteCostTotalsByTarget { get; init; } = new SortedDictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyList<WorldScaleTraversalItinerary> RequiredTargetItineraries { get; init; } = [];
    public IReadOnlyList<WorldScaleDisconnectedComponent> DisconnectedComponents { get; init; } = [];
    public IReadOnlyList<string> BlockedCriticalEdgeIds { get; init; } = [];
    public bool AllRequiredReachable { get; init; }
    public IReadOnlyList<WorldScaleRegionMapDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldScaleTraversalItinerary
{
    public string ScenarioId { get; init; } = string.Empty;
    public string TargetRegionId { get; init; } = string.Empty;
    public int TotalCost { get; init; }
    public IReadOnlyList<string> RegionPath { get; init; } = [];
    public IReadOnlyList<string> EdgePath { get; init; } = [];
}

public sealed record WorldScaleDisconnectedComponent
{
    public string ComponentId { get; init; } = string.Empty;
    public IReadOnlyList<string> RegionIds { get; init; } = [];
}

public sealed record WorldScaleTerrainPatchSummary
{
    public string PatchId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public IReadOnlyList<string> TerrainTags { get; init; } = [];
    public string AnchorCell { get; init; } = string.Empty;
    public int SummaryCellCount { get; init; }
}

public sealed record WorldScalePassabilitySummary
{
    public int PassablePatchCount { get; init; }
    public int HazardPatchCount { get; init; }
    public IReadOnlyList<string> BlockedRouteIds { get; init; } = [];
    public IReadOnlyList<string> TraversableRouteIds { get; init; } = [];
}

public sealed record WorldScaleLandmarkPlacement
{
    public string LandmarkId { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string Cell { get; init; } = string.Empty;
    public IReadOnlyList<string> PlacementTags { get; init; } = [];
}

public sealed record WorldScaleRegionMapBinding
{
    public string RegionId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public string PatchId { get; init; } = string.Empty;
    public string AnchorCell { get; init; } = string.Empty;
}

public sealed record WorldScaleRouteCellSummary
{
    public string EdgeId { get; init; } = string.Empty;
    public string RouteKind { get; init; } = string.Empty;
    public string FromRegionId { get; init; } = string.Empty;
    public string ToRegionId { get; init; } = string.Empty;
    public IReadOnlyList<string> RouteCellAnchors { get; init; } = [];
    public IReadOnlyList<string> RouteRegionBindingIds { get; init; } = [];
}

public sealed record WorldScaleHookPlacementSummary
{
    public string HookId { get; init; } = string.Empty;
    public string HookKind { get; init; } = string.Empty;
    public string RegionId { get; init; } = string.Empty;
    public string Cell { get; init; } = string.Empty;
}

public sealed record WorldScaleFiniteMapPack
{
    public string SchemaVersion { get; init; } = "world_scale_finite_map_pack_v1";
    public string ScenarioId { get; init; } = string.Empty;
    public string ProfileId { get; init; } = string.Empty;
    public string WorldGraphId { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public string CoordinateKind { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public int Radius { get; init; }
    public int MinQ { get; init; }
    public int MaxQ { get; init; }
    public int MinR { get; init; }
    public int MaxR { get; init; }
    public string DeterministicSeed { get; init; } = string.Empty;
    public IReadOnlyList<WorldScaleTerrainPatchSummary> TerrainPatches { get; init; } = [];
    public WorldScalePassabilitySummary PassabilitySummary { get; init; } = new();
    public IReadOnlyList<WorldScaleLandmarkPlacement> LandmarkPlacements { get; init; } = [];
    public IReadOnlyList<WorldScaleRegionMapBinding> RegionBindings { get; init; } = [];
    public IReadOnlyList<WorldScaleRouteCellSummary> RouteSummaries { get; init; } = [];
    public IReadOnlyList<WorldScaleHookPlacementSummary> HookPlacements { get; init; } = [];
    public IReadOnlyList<string> ValidationTrace { get; init; } = [];
    public IReadOnlyList<string> PreviewCells { get; init; } = [];
    public int AttemptedTileArrayCellCount { get; init; }
}

public sealed record WorldScaleChunkRegionCoverage
{
    public string RegionId { get; init; } = string.Empty;
    public IReadOnlyList<string> ChunkIds { get; init; } = [];
}

public sealed record WorldScaleFiniteMapChunkProjection
{
    public string MapId { get; init; } = string.Empty;
    public string CoordinateKind { get; init; } = string.Empty;
    public IReadOnlyList<string> CoveredChunkIds { get; init; } = [];
}

public sealed record WorldScaleScenarioChunkConfig
{
    public string ScenarioId { get; init; } = string.Empty;
    public string WorldGraphId { get; init; } = string.Empty;
    public string FiniteMapId { get; init; } = string.Empty;
    public int ChunkSize { get; init; }
    public string ChunkIdFormat { get; init; } = string.Empty;
    public string ScenarioWorldSeed { get; init; } = string.Empty;
    public IReadOnlyList<WorldScaleChunkRegionCoverage> RegionToChunkCoverage { get; init; } = [];
    public WorldScaleFiniteMapChunkProjection FiniteMapProjection { get; init; } = new();
    public IReadOnlyList<string> FutureGenerationRuleRefs { get; init; } = [];
    public IReadOnlyList<string> ForbiddenMutationNotes { get; init; } = [];
    public IReadOnlyList<string> RuntimeDeltaHandoffNotes { get; init; } = [];
}

public sealed record WorldScaleChunkedWorldConfigPrelude
{
    public string SchemaVersion { get; init; } = "world_scale_chunked_world_config_prelude_v1";
    public string GoalId { get; init; } = WorldScaleRegionMapVocabulary.GoalId;
    public int ScenarioCount { get; init; }
    public IReadOnlyList<WorldScaleScenarioChunkConfig> Scenarios { get; init; } = [];
}

public sealed record WorldScaleTraversalItineraryMatrix
{
    public string SchemaVersion { get; init; } = "world_scale_traversal_itinerary_matrix_v1";
    public int ItineraryCount { get; init; }
    public IReadOnlyList<WorldScaleTraversalItinerary> Itineraries { get; init; } = [];
}

public sealed record WorldScaleInvalidScenario
{
    public string ScenarioId { get; init; } = string.Empty;
    public string MutatedEvidenceKind { get; init; } = string.Empty;
    public string ExpectedStatus { get; init; } = "rejected";
    public string ActualStatus { get; init; } = string.Empty;
    public bool ExpectedValid { get; init; }
    public bool ActualValid { get; init; }
    public IReadOnlyList<WorldScaleRegionMapDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldScaleInvalidMatrix
{
    public string SchemaVersion { get; init; } = "invalid_world_scale_region_map_diagnostics_matrix_v1";
    public int ScenarioCount { get; init; }
    public int MatchedExpectationCount { get; init; }
    public int RejectedCount { get; init; }
    public int BlockedCount { get; init; }
    public bool Passed { get; init; }
    public IReadOnlyList<WorldScaleInvalidScenario> Scenarios { get; init; } = [];
}

public sealed record WorldScaleRegionMapFoundationReport
{
    public bool Accepted { get; init; }
    public string FinalStatus { get; init; } = WorldScaleRegionMapVocabulary.FinalGate;
    public string ManualGate { get; init; } = WorldScaleRegionMapVocabulary.FinalGate;
    public string ImplementationStatus { get; init; } = "GREEN";
    public string ProductSmokeRoute { get; init; } = WorldScaleRegionMapVocabulary.ProductSmokeRoute;
    public bool Goal037AcceptedByUserHandoff { get; init; }
    public bool ContractProofPassed { get; init; }
    public int ScenarioCount { get; init; }
    public int RegionGraphCount { get; init; }
    public int TotalRegionCount { get; init; }
    public int TotalTravelEdgeCount { get; init; }
    public int RequiredReachabilityCount { get; init; }
    public int ReachableRequiredTargetCount { get; init; }
    public bool RequiredReachabilityPassed { get; init; }
    public int FiniteMapPackCount { get; init; }
    public int ChunkConfigScenarioCount { get; init; }
    public int MetamoduleKingdomGroupCount { get; init; }
    public int MetamoduleSpeciesArchetypeSlotRefCount { get; init; }
    public int InvalidScenarioCount { get; init; }
    public bool InvalidMatrixPassed { get; init; }
    public bool NoRuntimeUiUnityGamePackageProviderLlmRagLuaGeneratorLibraryChanges { get; init; } = true;
    public string RegionGraphSummaryHash { get; init; } = string.Empty;
    public string ReachabilityMatrixHash { get; init; } = string.Empty;
    public string ChunkConfigPreludeHash { get; init; } = string.Empty;
    public string TraversalItineraryMatrixHash { get; init; } = string.Empty;
    public string InvalidMatrixHash { get; init; } = string.Empty;
    public IReadOnlyList<string> FiniteMapPackHashes { get; init; } = [];
    public string DeterministicHash { get; init; } = string.Empty;
    public IReadOnlyList<WorldScaleRegionMapDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record WorldScaleEvidenceResult
{
    public WorldScaleRegionGraphSummary RegionGraphSummary { get; init; } = new();
    public WorldScaleReachabilityMatrix ReachabilityMatrix { get; init; } = new();
    public IReadOnlyDictionary<string, WorldScaleFiniteMapPack> FiniteMapPacksByFileName { get; init; } = new Dictionary<string, WorldScaleFiniteMapPack>(StringComparer.Ordinal);
    public WorldScaleChunkedWorldConfigPrelude ChunkConfigPrelude { get; init; } = new();
    public WorldScaleTraversalItineraryMatrix TraversalItineraryMatrix { get; init; } = new();
    public WorldScaleInvalidMatrix InvalidMatrix { get; init; } = new();
    public WorldScaleRegionMapFoundationReport Report { get; init; } = new();
    public IReadOnlyDictionary<string, string> ArtifactJsonByFileName { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public string ReportMarkdown { get; init; } = string.Empty;
}

public sealed record WorldScaleEvidenceWriteResult
{
    public string OutputDirectoryPath { get; init; } = string.Empty;
    public IReadOnlyList<string> WrittenFiles { get; init; } = [];
    public string ReportMarkdownPath { get; init; } = string.Empty;
}

public sealed record WorldScaleRegionMapDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
