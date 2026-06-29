using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

namespace LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;

public sealed class RuntimeChunkTraversalPlanner
{
    public IReadOnlyList<RuntimeChunkTraversalPlan> BuildPlans()
    {
        var graphs = WorldScaleRegionMapCatalog.BuildDefaultGraphs();
        var mapPacks = new FiniteMapPackBuilder().BuildMapPacksByFileName(graphs);
        var chunkConfig = new ChunkedWorldConfigPreludeBuilder().Build(graphs, mapPacks);

        return graphs
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(graph => BuildPlan(
                graph,
                mapPacks[FiniteMapPackBuilder.FileName(graph.ScenarioId)],
                chunkConfig.Scenarios.Single(item => item.ScenarioId == graph.ScenarioId)))
            .ToList();
    }

    private RuntimeChunkTraversalPlan BuildPlan(
        WorldScaleRegionGraph graph,
        WorldScaleFiniteMapPack mapPack,
        WorldScaleScenarioChunkConfig chunkConfig)
    {
        var bindingsByRegion = mapPack.RegionBindings.ToDictionary(item => item.RegionId, StringComparer.Ordinal);
        var coverageByRegion = chunkConfig.RegionToChunkCoverage.ToDictionary(item => item.RegionId, StringComparer.Ordinal);
        var regionById = graph.Regions.ToDictionary(item => item.RegionId, StringComparer.Ordinal);
        var orderedRegionPath = BuildRequiredTargetRoute(graph);
        var steps = new List<RuntimeChunkTraversalStep>();

        for (var index = 0; index < orderedRegionPath.Count; index++)
        {
            var node = orderedRegionPath[index];
            var region = regionById[node.RegionId];
            var chunkId = coverageByRegion[node.RegionId].ChunkIds
                .Order(StringComparer.Ordinal)
                .First(item => item.EndsWith("/primary", StringComparison.Ordinal));
            var localMutation = BuildLocalMutation(graph.ScenarioId, region.RegionId, index, region.RequiredGameplayTarget);
            steps.Add(new RuntimeChunkTraversalStep
            {
                StepIndex = index,
                RegionId = node.RegionId,
                ArrivedByEdgeId = node.ArrivedByEdgeId,
                ChunkId = chunkId,
                Coordinate = bindingsByRegion[node.RegionId].AnchorCell,
                LandmarkId = region.LandmarkIds.Order(StringComparer.Ordinal).FirstOrDefault() ?? string.Empty,
                RouteCheckpointMarkerId = string.IsNullOrWhiteSpace(node.ArrivedByEdgeId)
                    ? string.Empty
                    : $"checkpoint/{graph.ScenarioId}/{StableSuffix(node.ArrivedByEdgeId)}",
                LocalMutationId = localMutation.MutationId,
                LocalMutationKind = localMutation.MutationKind
            });
        }

        var sourceFacts = new RuntimeChunkGoal038SourceFacts
        {
            ScenarioId = graph.ScenarioId,
            WorldGraphId = graph.WorldGraphId,
            FiniteMapId = mapPack.MapId,
            CoordinateKind = mapPack.CoordinateKind,
            ScenarioWorldSeed = chunkConfig.ScenarioWorldSeed,
            ChunkSize = chunkConfig.ChunkSize,
            RegionCount = graph.Regions.Count,
            TravelEdgeCount = graph.TravelEdges.Count,
            KingdomGroupCount = graph.Kingdoms.Count,
            SpeciesArchetypeSlotRefCount = graph.Kingdoms
                .SelectMany(item => item.SpeciesArchetypeSlotRefs)
                .Distinct(StringComparer.Ordinal)
                .Count(),
            Goal038EvidenceRefs = graph.SourceEvidenceRefs
                .Select(item => item.EvidenceId)
                .Order(StringComparer.Ordinal)
                .ToList()
        };
        var planWithoutCommands = new RuntimeChunkTraversalPlan
        {
            ScenarioId = graph.ScenarioId,
            ProfileId = graph.ProfileId,
            WorldGraphId = graph.WorldGraphId,
            FiniteMapId = mapPack.MapId,
            CoordinateKind = mapPack.CoordinateKind,
            ReplaySeed = $"{chunkConfig.ScenarioWorldSeed}/goal039-runtime-replay",
            StartRegionId = graph.StartRegionId,
            RequiredTargetRegionIds = graph.RequiredTargetRegionIds.Order(StringComparer.Ordinal).ToList(),
            SourceFacts = sourceFacts,
            Steps = steps
        };

        return planWithoutCommands with
        {
            Commands = RuntimeChunkDeltaProjector.BuildCommands(planWithoutCommands)
        };
    }

    private static IReadOnlyList<RouteNode> BuildRequiredTargetRoute(WorldScaleRegionGraph graph)
    {
        var current = graph.StartRegionId;
        var route = new List<RouteNode> { new(current, null) };

        foreach (var target in graph.RequiredTargetRegionIds.Order(StringComparer.Ordinal))
        {
            if (string.Equals(current, target, StringComparison.Ordinal))
            {
                continue;
            }

            var path = FindShortestPath(graph, current, target);
            foreach (var node in path.Skip(1))
            {
                route.Add(node);
            }

            current = target;
        }

        return route;
    }

    private static IReadOnlyList<RouteNode> FindShortestPath(WorldScaleRegionGraph graph, string startRegionId, string targetRegionId)
    {
        var adjacency = BuildAdjacency(graph);
        var best = new Dictionary<string, PathState>(StringComparer.Ordinal)
        {
            [startRegionId] = new(0, [new RouteNode(startRegionId, null)])
        };
        var open = new SortedSet<QueueEntry>(new QueueEntryComparer())
        {
            new(startRegionId, 0)
        };

        while (open.Count > 0)
        {
            var current = open.Min!;
            open.Remove(current);
            if (string.Equals(current.RegionId, targetRegionId, StringComparison.Ordinal))
            {
                return best[current.RegionId].Nodes;
            }

            foreach (var edge in adjacency.GetValueOrDefault(current.RegionId, []))
            {
                var state = best[current.RegionId];
                var nextCost = state.Cost + edge.Cost;
                var nextNodes = state.Nodes.Concat([new RouteNode(edge.ToRegionId, edge.EdgeId)]).ToList();
                if (!best.TryGetValue(edge.ToRegionId, out var existing)
                    || nextCost < existing.Cost
                    || (nextCost == existing.Cost && CompareRoute(nextNodes, existing.Nodes) < 0))
                {
                    if (existing is not null)
                    {
                        open.Remove(new QueueEntry(edge.ToRegionId, existing.Cost));
                    }

                    best[edge.ToRegionId] = new PathState(nextCost, nextNodes);
                    open.Add(new QueueEntry(edge.ToRegionId, nextCost));
                }
            }
        }

        return [new RouteNode(startRegionId, null)];
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<AdjacentEdge>> BuildAdjacency(WorldScaleRegionGraph graph)
    {
        var adjacency = graph.Regions.ToDictionary(
            item => item.RegionId,
            _ => new List<AdjacentEdge>(),
            StringComparer.Ordinal);

        foreach (var edge in graph.TravelEdges.OrderBy(item => item.EdgeId, StringComparer.Ordinal))
        {
            if (!edge.IsTraversableNow || edge.Cost <= 0)
            {
                continue;
            }

            Add(adjacency, edge.FromRegionId, new AdjacentEdge(edge.EdgeId, edge.ToRegionId, edge.Cost));
            if (edge.Bidirectional)
            {
                Add(adjacency, edge.ToRegionId, new AdjacentEdge(edge.EdgeId, edge.FromRegionId, edge.Cost));
            }
        }

        return adjacency.ToDictionary(
            item => item.Key,
            item => (IReadOnlyList<AdjacentEdge>)item.Value
                .OrderBy(edge => edge.EdgeId, StringComparer.Ordinal)
                .ThenBy(edge => edge.ToRegionId, StringComparer.Ordinal)
                .ToList(),
            StringComparer.Ordinal);
    }

    private static void Add(IDictionary<string, List<AdjacentEdge>> adjacency, string regionId, AdjacentEdge edge)
    {
        if (adjacency.TryGetValue(regionId, out var edges))
        {
            edges.Add(edge);
        }
    }

    private static (string MutationId, string MutationKind) BuildLocalMutation(
        string scenarioId,
        string regionId,
        int stepIndex,
        bool requiredTarget)
    {
        if (!requiredTarget)
        {
            return (string.Empty, string.Empty);
        }

        var suffix = StableSuffix(regionId);
        return scenarioId switch
        {
            "frontier_survival" => ($"mutation/{scenarioId}/{suffix}/resource-depleted", "resource_depleted"),
            "gothic_intrigue" => ($"mutation/{scenarioId}/{suffix}/gate-opened", "gate_opened"),
            "caravan_trade" => ($"mutation/{scenarioId}/{suffix}/tariff-cleared", "tariff_cleared"),
            "metamodule_kingdoms" when stepIndex % 2 == 0 => ($"mutation/{scenarioId}/{suffix}/kingdom-beacon-synced", "kingdom_beacon_synced"),
            "metamodule_kingdoms" => ($"mutation/{scenarioId}/{suffix}/court-route-attuned", "court_route_attuned"),
            _ => ($"mutation/{scenarioId}/{suffix}/local-state", "local_state")
        };
    }

    private static int CompareRoute(IReadOnlyList<RouteNode> left, IReadOnlyList<RouteNode> right) =>
        string.CompareOrdinal(
            string.Join("|", left.Select(item => item.RegionId + "@" + item.ArrivedByEdgeId)),
            string.Join("|", right.Select(item => item.RegionId + "@" + item.ArrivedByEdgeId)));

    internal static string StableSuffix(string value)
    {
        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? "unknown" : parts[^1];
    }

    private sealed record RouteNode(string RegionId, string? ArrivedByEdgeId);

    private sealed record AdjacentEdge(string EdgeId, string ToRegionId, int Cost);

    private sealed record PathState(int Cost, IReadOnlyList<RouteNode> Nodes);

    private sealed record QueueEntry(string RegionId, int Cost);

    private sealed class QueueEntryComparer : IComparer<QueueEntry>
    {
        public int Compare(QueueEntry? x, QueueEntry? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            var cost = x.Cost.CompareTo(y.Cost);
            return cost != 0 ? cost : string.CompareOrdinal(x.RegionId, y.RegionId);
        }
    }
}
