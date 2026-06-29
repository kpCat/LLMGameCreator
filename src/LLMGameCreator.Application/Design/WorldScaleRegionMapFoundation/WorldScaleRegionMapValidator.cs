namespace LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

public sealed class WorldScaleRegionMapValidator
{
    private static readonly IReadOnlySet<string> ValidCoordinateKinds = new HashSet<string>(["square", "axial_hex"], StringComparer.Ordinal);

    public IReadOnlyList<WorldScaleRegionMapDiagnostic> ValidateGraph(WorldScaleRegionGraph graph)
    {
        var diagnostics = new List<WorldScaleRegionMapDiagnostic>();
        if (!WorldScaleRegionMapVocabulary.Scenarios.Contains(graph.ScenarioId))
        {
            diagnostics.Add(Error("world_scale.scenario.unknown", graph.ScenarioId, "Scenario id is not part of the Goal038 scenario set."));
        }

        if (!string.Equals(graph.ScenarioId, graph.ProfileId, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("world_scale.scenario_profile.mismatch", graph.ProfileId, "Scenario id and profile id must match for Goal038 evidence."));
        }

        AddDuplicates(diagnostics, graph.Regions.Select(item => item.RegionId), "world_scale.region.duplicate", "Region ids must be unique.");
        AddDuplicates(diagnostics, graph.TravelEdges.Select(item => item.EdgeId), "world_scale.edge.duplicate", "Travel edge ids must be unique.");
        AddDuplicates(diagnostics, graph.Kingdoms.Select(item => item.KingdomId), "world_scale.kingdom.duplicate", "Kingdom ids must be unique.");

        if (!IsSorted(graph.Kingdoms.Select(item => item.KingdomId))
            || !IsSorted(graph.Regions.Select(item => item.RegionId))
            || !IsSorted(graph.TravelEdges.Select(item => item.EdgeId)))
        {
            diagnostics.Add(Error("world_scale.order.nondeterministic", graph.ScenarioId, "Graph collections must be sorted by stable ids."));
        }

        var regionIds = graph.Regions.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
        var kingdomIds = graph.Kingdoms.Select(item => item.KingdomId).ToHashSet(StringComparer.Ordinal);
        if (!regionIds.Contains(graph.StartRegionId))
        {
            diagnostics.Add(Error("world_scale.start_region.missing", graph.StartRegionId, "Start region must exist in the graph."));
        }

        foreach (var required in graph.RequiredTargetRegionIds.Where(item => !regionIds.Contains(item)).Order(StringComparer.Ordinal))
        {
            diagnostics.Add(Error("world_scale.required_target.unknown", required, "Required target region must exist."));
        }

        foreach (var optional in graph.OptionalTargetRegionIds.Where(item => !regionIds.Contains(item)).Order(StringComparer.Ordinal))
        {
            diagnostics.Add(Error("world_scale.optional_target.unknown", optional, "Optional target region must exist."));
        }

        foreach (var region in graph.Regions.OrderBy(item => item.RegionId, StringComparer.Ordinal))
        {
            if (!kingdomIds.Contains(region.KingdomId))
            {
                diagnostics.Add(Error("world_scale.region.kingdom_unknown", region.RegionId, "Region must reference an existing kingdom group."));
            }

            ValidateEvidenceRefs(diagnostics, region.SourceEvidenceRefs, $"region:{region.RegionId}");
        }

        foreach (var kingdom in graph.Kingdoms.OrderBy(item => item.KingdomId, StringComparer.Ordinal))
        {
            foreach (var regionId in kingdom.RegionIds.Where(item => !regionIds.Contains(item)).Order(StringComparer.Ordinal))
            {
                diagnostics.Add(Error("world_scale.kingdom.region_unknown", regionId, "Kingdom group references an unknown region."));
            }
        }

        foreach (var edge in graph.TravelEdges.OrderBy(item => item.EdgeId, StringComparer.Ordinal))
        {
            if (!regionIds.Contains(edge.FromRegionId) || !regionIds.Contains(edge.ToRegionId))
            {
                diagnostics.Add(Error("world_scale.edge.endpoint_unknown", edge.EdgeId, "Travel edge endpoint must reference known regions."));
            }

            if (!WorldScaleRegionMapVocabulary.RequiredRouteKinds.Contains(edge.RouteKind))
            {
                diagnostics.Add(Error("world_scale.edge.route_kind_unknown", edge.EdgeId, "Travel edge route kind is not allowed by Goal038."));
            }

            if (edge.Cost <= 0)
            {
                diagnostics.Add(Error("world_scale.edge.travel_cost.invalid", edge.EdgeId, "Travel cost must be positive."));
            }

            ValidateEvidenceRefs(diagnostics, edge.SourceEvidenceRefs, $"edge:{edge.EdgeId}");
        }

        if (graph.TravelEdges.Count > 0 && graph.TravelEdges.All(item => !item.IsTraversableNow))
        {
            diagnostics.Add(Error("world_scale.routes.all_blocked", graph.ScenarioId, "At least one route must be traversable in valid graph evidence."));
        }

        foreach (var bidirectional in graph.TravelEdges.Where(item => item.Bidirectional).OrderBy(item => item.EdgeId, StringComparer.Ordinal))
        {
            var reverse = graph.TravelEdges.FirstOrDefault(item =>
                item.EdgeId != bidirectional.EdgeId &&
                string.Equals(item.FromRegionId, bidirectional.ToRegionId, StringComparison.Ordinal) &&
                string.Equals(item.ToRegionId, bidirectional.FromRegionId, StringComparison.Ordinal));
            if (reverse != null && (reverse.RouteKind != bidirectional.RouteKind || reverse.Cost != bidirectional.Cost || reverse.Bidirectional != bidirectional.Bidirectional))
            {
                diagnostics.Add(Error("world_scale.edge.bidirectional_contradiction", bidirectional.EdgeId, "Bidirectional edge contradicts a separately declared reverse edge."));
            }
        }

        ValidateEvidenceRefs(diagnostics, graph.SourceEvidenceRefs, $"graph:{graph.ScenarioId}");
        AddBoundaryDiagnostics(diagnostics, graph.BoundaryClaims, graph.ScenarioId);

        return WorldScaleRegionMapCatalog.SortDiagnostics(diagnostics);
    }

    public IReadOnlyList<WorldScaleRegionMapDiagnostic> ValidateReachability(WorldScaleReachabilityScenario reachability)
    {
        if (reachability.AllRequiredReachable)
        {
            return reachability.Diagnostics;
        }

        return WorldScaleRegionMapCatalog.SortDiagnostics(reachability.Diagnostics.Concat([
            Error("world_scale.required_target.unreachable", reachability.ScenarioId, "Required target coverage failed.")
        ]));
    }

    public IReadOnlyList<WorldScaleRegionMapDiagnostic> ValidateMapPack(WorldScaleFiniteMapPack mapPack, WorldScaleRegionGraph graph)
    {
        var diagnostics = new List<WorldScaleRegionMapDiagnostic>();
        var regionIds = graph.Regions.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
        if (!string.Equals(mapPack.ScenarioId, graph.ScenarioId, StringComparison.Ordinal)
            || !string.Equals(mapPack.ProfileId, graph.ProfileId, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("world_scale.map.scenario_profile_mismatch", mapPack.MapId, "Map pack must match graph scenario/profile."));
        }

        if (!ValidCoordinateKinds.Contains(mapPack.CoordinateKind))
        {
            diagnostics.Add(Error("world_scale.map.coordinate_invalid", mapPack.MapId, "Map coordinate kind must be square or axial_hex."));
            if (mapPack.Width <= 0 && mapPack.Height <= 0 && mapPack.Radius <= 0)
            {
                diagnostics.Add(Error("world_scale.map.size_invalid", mapPack.MapId, "Map coordinate bounds must be positive for the selected coordinate kind."));
            }
        }

        if (mapPack.CoordinateKind == "square" && (mapPack.Width <= 0 || mapPack.Height <= 0))
        {
            diagnostics.Add(Error("world_scale.map.size_invalid", mapPack.MapId, "Square map must have positive width and height."));
        }

        if (mapPack.CoordinateKind == "axial_hex" && mapPack.Radius <= 0)
        {
            diagnostics.Add(Error("world_scale.map.size_invalid", mapPack.MapId, "Axial hex map must have a positive radius."));
        }

        if (mapPack.AttemptedTileArrayCellCount > 256)
        {
            diagnostics.Add(Error("world_scale.map.tile_dump.forbidden", mapPack.MapId, "Goal038 evidence must stay compact and must not dump huge tile arrays."));
        }

        var boundRegions = mapPack.RegionBindings.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
        foreach (var regionId in regionIds.Where(item => !boundRegions.Contains(item)).Order(StringComparer.Ordinal))
        {
            diagnostics.Add(Error("world_scale.map.region_binding_missing", regionId, "Every graph region must have a finite map binding."));
        }

        foreach (var placement in mapPack.LandmarkPlacements.OrderBy(item => item.LandmarkId, StringComparer.Ordinal))
        {
            if (!regionIds.Contains(placement.RegionId))
            {
                diagnostics.Add(Error("world_scale.landmark.region_unknown", placement.LandmarkId, "Landmark placement must reference a known region."));
            }
        }

        var edgeById = graph.TravelEdges.ToDictionary(item => item.EdgeId, StringComparer.Ordinal);
        foreach (var route in mapPack.RouteSummaries.OrderBy(item => item.EdgeId, StringComparer.Ordinal))
        {
            if (!edgeById.TryGetValue(route.EdgeId, out var edge))
            {
                diagnostics.Add(Error("world_scale.route.edge_unknown", route.EdgeId, "Route summary must reference a graph edge."));
                continue;
            }

            if (!route.RouteRegionBindingIds.Contains(edge.FromRegionId, StringComparer.Ordinal)
                || !route.RouteRegionBindingIds.Contains(edge.ToRegionId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("world_scale.route_polyline.region_binding_missing", route.EdgeId, "Route summary must bind both route endpoint regions."));
            }
        }

        return WorldScaleRegionMapCatalog.SortDiagnostics(diagnostics);
    }

    public IReadOnlyList<WorldScaleRegionMapDiagnostic> ValidateChunkConfig(
        WorldScaleChunkedWorldConfigPrelude prelude,
        IReadOnlyList<WorldScaleRegionGraph> graphs,
        IReadOnlyDictionary<string, WorldScaleFiniteMapPack> mapPacksByFileName)
    {
        var diagnostics = new List<WorldScaleRegionMapDiagnostic>();
        var graphByScenario = graphs.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);
        var mapByScenario = mapPacksByFileName.Values.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        foreach (var scenario in prelude.Scenarios.OrderBy(item => item.ScenarioId, StringComparer.Ordinal))
        {
            if (scenario.ChunkSize <= 0)
            {
                diagnostics.Add(Error("world_scale.chunk.size_invalid", scenario.ScenarioId, "Chunk size must be positive."));
            }

            if (!graphByScenario.TryGetValue(scenario.ScenarioId, out var graph)
                || !mapByScenario.TryGetValue(scenario.ScenarioId, out var mapPack))
            {
                diagnostics.Add(Error("world_scale.chunk.scenario_unknown", scenario.ScenarioId, "Chunk config references an unknown scenario."));
                continue;
            }

            var covered = scenario.RegionToChunkCoverage.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
            foreach (var binding in mapPack.RegionBindings.Where(item => !covered.Contains(item.RegionId)).OrderBy(item => item.RegionId, StringComparer.Ordinal))
            {
                diagnostics.Add(Error("world_scale.chunk.coverage_missing", binding.RegionId, "Chunk config must cover every finite-map region binding."));
            }

            if (!string.Equals(scenario.WorldGraphId, graph.WorldGraphId, StringComparison.Ordinal)
                || !string.Equals(scenario.FiniteMapId, mapPack.MapId, StringComparison.Ordinal))
            {
                diagnostics.Add(Error("world_scale.chunk.source_ref_mismatch", scenario.ScenarioId, "Chunk config must reference the matching graph and finite map pack."));
            }
        }

        return WorldScaleRegionMapCatalog.SortDiagnostics(diagnostics);
    }

    public WorldScaleInvalidMatrix BuildInvalidMatrix(
        IReadOnlyList<WorldScaleRegionGraph> validGraphs,
        IReadOnlyDictionary<string, WorldScaleFiniteMapPack> validMapPacks,
        WorldScaleChunkedWorldConfigPrelude validChunkConfig)
    {
        var graph = validGraphs.Single(item => item.ScenarioId == "frontier_survival");
        var map = validMapPacks[FiniteMapPackBuilder.FileName("frontier_survival")];
        var bidirectionalEdge = graph.TravelEdges.First(item => item.Bidirectional);
        var invalid = new List<WorldScaleInvalidScenario>
        {
            GraphInvalid("duplicate_region_id", "duplicate region id", graph with { Regions = graph.Regions.Concat([graph.Regions[0]]).ToList() }),
            GraphInvalid("duplicate_edge_id", "duplicate edge id", graph with { TravelEdges = graph.TravelEdges.Concat([graph.TravelEdges[0]]).ToList() }),
            GraphInvalid("unknown_edge_endpoint", "unknown edge endpoint", graph with { TravelEdges = graph.TravelEdges.Select((edge, index) => index == 0 ? edge with { ToRegionId = "region/frontier/missing" } : edge).ToList() }),
            GraphInvalid("missing_start_region", "missing start region", graph with { StartRegionId = "region/frontier/missing-start" }),
            GraphReachabilityInvalid("required_target_unreachable", "required target unreachable", graph with { TravelEdges = graph.TravelEdges.Select(edge => edge.EdgeId is "edge/frontier/pine-river" or "edge/frontier/river-pass" ? edge with { IsBlocked = true } : edge).ToList() }),
            GraphInvalid("all_routes_blocked", "all routes blocked", graph with { TravelEdges = graph.TravelEdges.Select(edge => edge with { IsBlocked = true }).ToList() }),
            GraphInvalid("contradictory_bidirectional_edge_declaration", "contradictory bidirectional edge declaration", graph with { TravelEdges = graph.TravelEdges.Concat([bidirectionalEdge with { EdgeId = "edge/frontier/pine-homestead-conflict", FromRegionId = bidirectionalEdge.ToRegionId, ToRegionId = bidirectionalEdge.FromRegionId, Cost = bidirectionalEdge.Cost + 9 }]).OrderBy(item => item.EdgeId, StringComparer.Ordinal).ToList() }),
            GraphInvalid("negative_zero_invalid_travel_cost", "negative/zero invalid travel cost", graph with { TravelEdges = graph.TravelEdges.Select((edge, index) => index == 0 ? edge with { Cost = 0 } : edge).ToList() }),
            MapInvalid("unknown_landmark_region", "unknown landmark region", map with { LandmarkPlacements = map.LandmarkPlacements.Select((item, index) => index == 0 ? item with { RegionId = "region/frontier/missing" } : item).ToList() }, graph),
            MapInvalid("invalid_map_coordinate_size", "invalid map coordinate/size", map with { CoordinateKind = "triangle", Width = 0, Height = 0 }, graph),
            MapInvalid("route_polyline_missing_required_region_binding", "route polyline missing required region binding", map with { RouteSummaries = map.RouteSummaries.Select((item, index) => index == 0 ? item with { RouteRegionBindingIds = [item.FromRegionId] } : item).ToList() }, graph),
            ChunkInvalid("chunk_config_missing_required_map_region", "chunk config does not cover required map region", validChunkConfig with { Scenarios = validChunkConfig.Scenarios.Select(item => item.ScenarioId == "frontier_survival" ? item with { RegionToChunkCoverage = item.RegionToChunkCoverage.Skip(1).ToList() } : item).ToList() }, validGraphs, validMapPacks),
            GraphInvalid("fake_goal037_expansion_output_id", "fake Goal037 expansion output id", graph with { SourceEvidenceRefs = [new WorldScaleSourceEvidenceRef { SourceGoal = "Goal037", EvidenceId = "hybrid-expansion/fake/missing", ArtifactFamily = "fake" }] }),
            GraphInvalid("scenario_profile_mismatch", "scenario/profile mismatch", graph with { ProfileId = "gothic_intrigue" }),
            GraphInvalid("nondeterministic_ordering_mutation", "nondeterministic ordering mutation", graph with { Regions = graph.Regions.Reverse().ToList() }),
            MapInvalid("huge_tile_array_dump_attempt", "huge tile-array dump attempt", map with { AttemptedTileArrayCellCount = 10000 }, graph),
            GraphInvalid("forbidden_runtime_ui_unity_gamepackage_provider_llm_rag_lua_generator_library_leakage", "forbidden Runtime/UI/Unity/GamePackage/provider/LLM/RAG/Lua source/generator-library leakage", graph with { BoundaryClaims = new WorldScaleBoundaryClaims { RuntimeMutation = true, UiWinForms = true, Unity = true, GamePackageSchema = true, ProviderLlmRag = true, LuaSourceOrExecution = true, GeneratorLibrary = true } }, expectedStatus: "blocked")
        };

        return new WorldScaleInvalidMatrix
        {
            ScenarioCount = invalid.Count,
            MatchedExpectationCount = invalid.Count(item => item.ExpectedStatus == item.ActualStatus),
            RejectedCount = invalid.Count(item => item.ActualStatus == "rejected"),
            BlockedCount = invalid.Count(item => item.ActualStatus == "blocked"),
            Passed = invalid.All(item => item.ExpectedStatus == item.ActualStatus && item.Diagnostics.Count > 0),
            Scenarios = invalid.OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList()
        };
    }

    private WorldScaleInvalidScenario GraphInvalid(string scenarioId, string kind, WorldScaleRegionGraph graph, string expectedStatus = "rejected")
    {
        var diagnostics = ValidateGraph(graph);
        return Invalid(scenarioId, kind, expectedStatus, diagnostics);
    }

    private WorldScaleInvalidScenario GraphReachabilityInvalid(string scenarioId, string kind, WorldScaleRegionGraph graph)
    {
        var diagnostics = ValidateGraph(graph)
            .Concat(ValidateReachability(new WorldScaleReachabilityPlanner().Analyze(graph)))
            .ToList();
        return Invalid(scenarioId, kind, "rejected", diagnostics);
    }

    private WorldScaleInvalidScenario MapInvalid(string scenarioId, string kind, WorldScaleFiniteMapPack mapPack, WorldScaleRegionGraph graph)
    {
        var diagnostics = ValidateMapPack(mapPack, graph);
        return Invalid(scenarioId, kind, "rejected", diagnostics);
    }

    private WorldScaleInvalidScenario ChunkInvalid(
        string scenarioId,
        string kind,
        WorldScaleChunkedWorldConfigPrelude prelude,
        IReadOnlyList<WorldScaleRegionGraph> graphs,
        IReadOnlyDictionary<string, WorldScaleFiniteMapPack> mapPacks)
    {
        var diagnostics = ValidateChunkConfig(prelude, graphs, mapPacks);
        return Invalid(scenarioId, kind, "rejected", diagnostics);
    }

    private static WorldScaleInvalidScenario Invalid(
        string scenarioId,
        string kind,
        string expectedStatus,
        IReadOnlyList<WorldScaleRegionMapDiagnostic> diagnostics)
    {
        var errors = diagnostics.Where(item => item.Severity == "error").ToList();
        var actualStatus = errors.Any(item => item.Code.Contains(".boundary.", StringComparison.Ordinal))
            ? "blocked"
            : errors.Count > 0
                ? "rejected"
                : "accepted";

        return new WorldScaleInvalidScenario
        {
            ScenarioId = scenarioId,
            MutatedEvidenceKind = kind,
            ExpectedStatus = expectedStatus,
            ActualStatus = actualStatus,
            ExpectedValid = false,
            ActualValid = actualStatus == "accepted",
            Diagnostics = WorldScaleRegionMapCatalog.SortDiagnostics(errors)
        };
    }

    private static void ValidateEvidenceRefs(List<WorldScaleRegionMapDiagnostic> diagnostics, IEnumerable<WorldScaleSourceEvidenceRef> refs, string target)
    {
        foreach (var evidenceRef in refs.OrderBy(item => item.EvidenceId, StringComparer.Ordinal))
        {
            if (evidenceRef.SourceGoal == "Goal037" && !WorldScaleRegionMapCatalog.AcceptedGoal037EvidenceIds.Contains(evidenceRef.EvidenceId))
            {
                diagnostics.Add(Error("world_scale.goal037_output.fake", target, "Goal038 source evidence must reference accepted Goal037 expansion output ids."));
            }
        }
    }

    private static void AddBoundaryDiagnostics(List<WorldScaleRegionMapDiagnostic> diagnostics, WorldScaleBoundaryClaims claims, string target)
    {
        if (claims.RuntimeMutation) diagnostics.Add(Error("world_scale.boundary.runtime.forbidden", target, "Runtime mutation is forbidden in Goal038."));
        if (claims.UiWinForms) diagnostics.Add(Error("world_scale.boundary.ui.forbidden", target, "WinForms/UI changes are forbidden in Goal038."));
        if (claims.Unity) diagnostics.Add(Error("world_scale.boundary.unity.forbidden", target, "Unity changes are forbidden in Goal038."));
        if (claims.GamePackageSchema) diagnostics.Add(Error("world_scale.boundary.gamepackage.forbidden", target, "GamePackage schema changes are forbidden in Goal038."));
        if (claims.ProviderLlmRag) diagnostics.Add(Error("world_scale.boundary.provider_llm_rag.forbidden", target, "Provider/LLM/RAG calls are forbidden in Goal038."));
        if (claims.LuaSourceOrExecution) diagnostics.Add(Error("world_scale.boundary.lua.forbidden", target, "New Lua source/execution work is forbidden in Goal038."));
        if (claims.GeneratorLibrary) diagnostics.Add(Error("world_scale.boundary.generator_library.forbidden", target, "Generator-library changes are forbidden in Goal038."));
        if (claims.ExternalDependency) diagnostics.Add(Error("world_scale.boundary.external_dependency.forbidden", target, "External dependency additions are forbidden in Goal038."));
    }

    private static void AddDuplicates(
        List<WorldScaleRegionMapDiagnostic> diagnostics,
        IEnumerable<string> ids,
        string code,
        string message)
    {
        foreach (var duplicate in ids.GroupBy(item => item, StringComparer.Ordinal).Where(group => group.Count() > 1).Select(group => group.Key).Order(StringComparer.Ordinal))
        {
            diagnostics.Add(Error(code, duplicate, message));
        }
    }

    private static bool IsSorted(IEnumerable<string> ids)
    {
        var list = ids.ToList();
        return list.SequenceEqual(list.Order(StringComparer.Ordinal), StringComparer.Ordinal);
    }

    private static WorldScaleRegionMapDiagnostic Error(string code, string target, string message) =>
        WorldScaleRegionMapCatalog.Diagnostic("error", code, target, message);
}
