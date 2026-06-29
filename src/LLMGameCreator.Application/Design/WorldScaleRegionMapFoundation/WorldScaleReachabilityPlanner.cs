namespace LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

public sealed class WorldScaleReachabilityPlanner
{
    public WorldScaleReachabilityMatrix BuildMatrix(IReadOnlyList<WorldScaleRegionGraph> graphs)
    {
        var scenarios = graphs
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(Analyze)
            .ToList();

        return new WorldScaleReachabilityMatrix
        {
            ScenarioCount = scenarios.Count,
            RequiredTargetCount = scenarios.Sum(item => item.RequiredTargetRegionIds.Count),
            ReachableRequiredTargetCount = scenarios.Sum(item => item.RequiredTargetRegionIds.Count - item.UnreachableRequiredRegionIds.Count),
            AllRequiredTargetsReachable = scenarios.All(item => item.AllRequiredReachable),
            Scenarios = scenarios
        };
    }

    public WorldScaleReachabilityScenario Analyze(WorldScaleRegionGraph graph)
    {
        var diagnostics = new List<WorldScaleRegionMapDiagnostic>();
        var regionIds = graph.Regions.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
        if (!regionIds.Contains(graph.StartRegionId))
        {
            diagnostics.Add(WorldScaleRegionMapCatalog.Diagnostic("error", "world_scale.start_region.missing", graph.StartRegionId, "Start region is not present in the graph."));
            return new WorldScaleReachabilityScenario
            {
                ScenarioId = graph.ScenarioId,
                StartRegionId = graph.StartRegionId,
                RequiredTargetRegionIds = graph.RequiredTargetRegionIds.Order(StringComparer.Ordinal).ToList(),
                UnreachableRequiredRegionIds = graph.RequiredTargetRegionIds.Order(StringComparer.Ordinal).ToList(),
                Diagnostics = WorldScaleRegionMapCatalog.SortDiagnostics(diagnostics)
            };
        }

        var adjacency = BuildAdjacency(graph);
        var shortest = Dijkstra(graph.StartRegionId, adjacency);
        var reachable = shortest.Keys.Order(StringComparer.Ordinal).ToList();
        var missing = graph.RequiredTargetRegionIds
            .Where(item => !shortest.ContainsKey(item))
            .Order(StringComparer.Ordinal)
            .ToList();
        var itineraries = graph.RequiredTargetRegionIds
            .Where(shortest.ContainsKey)
            .Order(StringComparer.Ordinal)
            .Select(target => BuildItinerary(graph.ScenarioId, target, shortest[target]))
            .ToList();
        var costs = itineraries
            .ToDictionary(item => item.TargetRegionId, item => item.TotalCost, StringComparer.Ordinal);
        var blockedCritical = DetectBlockedCriticalEdges(graph, reachable.ToHashSet(StringComparer.Ordinal), missing);
        var components = BuildDisconnectedComponents(graph, adjacency);

        if (missing.Count > 0)
        {
            diagnostics.Add(WorldScaleRegionMapCatalog.Diagnostic("error", "world_scale.required_target.unreachable", graph.ScenarioId, "One or more required target regions are not reachable from the start."));
        }

        if (blockedCritical.Count > 0)
        {
            diagnostics.Add(WorldScaleRegionMapCatalog.Diagnostic("error", "world_scale.edge.blocked_critical", graph.ScenarioId, "A blocked or future edge separates reachable graph from a required target."));
        }

        return new WorldScaleReachabilityScenario
        {
            ScenarioId = graph.ScenarioId,
            StartRegionId = graph.StartRegionId,
            RequiredTargetRegionIds = graph.RequiredTargetRegionIds.Order(StringComparer.Ordinal).ToList(),
            ReachableRegionIds = reachable,
            UnreachableRequiredRegionIds = missing,
            RouteCostTotalsByTarget = new SortedDictionary<string, int>(costs, StringComparer.Ordinal),
            RequiredTargetItineraries = itineraries,
            DisconnectedComponents = components,
            BlockedCriticalEdgeIds = blockedCritical,
            AllRequiredReachable = missing.Count == 0,
            Diagnostics = WorldScaleRegionMapCatalog.SortDiagnostics(diagnostics)
        };
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

            Add(adjacency, edge.FromRegionId, new AdjacentEdge(edge.EdgeId, edge.FromRegionId, edge.ToRegionId, edge.Cost));
            if (edge.Bidirectional)
            {
                Add(adjacency, edge.ToRegionId, new AdjacentEdge(edge.EdgeId, edge.ToRegionId, edge.FromRegionId, edge.Cost));
            }
        }

        return adjacency.ToDictionary(
            item => item.Key,
            item => (IReadOnlyList<AdjacentEdge>)item.Value.OrderBy(edge => edge.EdgeId, StringComparer.Ordinal).ThenBy(edge => edge.ToRegionId, StringComparer.Ordinal).ToList(),
            StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, PathState> Dijkstra(string startRegionId, IReadOnlyDictionary<string, IReadOnlyList<AdjacentEdge>> adjacency)
    {
        var best = new Dictionary<string, PathState>(StringComparer.Ordinal)
        {
            [startRegionId] = new(startRegionId, 0, [startRegionId], [])
        };
        var open = new SortedSet<QueueEntry>(new QueueEntryComparer())
        {
            new(startRegionId, 0)
        };

        while (open.Count > 0)
        {
            var current = open.Min!;
            open.Remove(current);
            var currentState = best[current.RegionId];
            foreach (var edge in adjacency.GetValueOrDefault(current.RegionId, []))
            {
                var nextCost = currentState.Cost + edge.Cost;
                var nextPath = currentState.RegionPath.Concat([edge.ToRegionId]).ToList();
                var nextEdges = currentState.EdgePath.Concat([edge.EdgeId]).ToList();
                if (!best.TryGetValue(edge.ToRegionId, out var existing)
                    || nextCost < existing.Cost
                    || (nextCost == existing.Cost && ComparePath(nextPath, existing.RegionPath) < 0))
                {
                    if (existing is not null)
                    {
                        open.Remove(new QueueEntry(edge.ToRegionId, existing.Cost));
                    }

                    best[edge.ToRegionId] = new PathState(edge.ToRegionId, nextCost, nextPath, nextEdges);
                    open.Add(new QueueEntry(edge.ToRegionId, nextCost));
                }
            }
        }

        return best;
    }

    private static WorldScaleTraversalItinerary BuildItinerary(string scenarioId, string target, PathState path) =>
        new()
        {
            ScenarioId = scenarioId,
            TargetRegionId = target,
            TotalCost = path.Cost,
            RegionPath = path.RegionPath,
            EdgePath = path.EdgePath
        };

    private static IReadOnlyList<string> DetectBlockedCriticalEdges(
        WorldScaleRegionGraph graph,
        IReadOnlySet<string> reachable,
        IReadOnlyList<string> missingRequired)
    {
        if (missingRequired.Count == 0)
        {
            return [];
        }

        var missing = missingRequired.ToHashSet(StringComparer.Ordinal);
        return graph.TravelEdges
            .Where(item => !item.IsTraversableNow)
            .Where(item =>
                (reachable.Contains(item.FromRegionId) && missing.Contains(item.ToRegionId)) ||
                (item.Bidirectional && reachable.Contains(item.ToRegionId) && missing.Contains(item.FromRegionId)))
            .Select(item => item.EdgeId)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<WorldScaleDisconnectedComponent> BuildDisconnectedComponents(
        WorldScaleRegionGraph graph,
        IReadOnlyDictionary<string, IReadOnlyList<AdjacentEdge>> adjacency)
    {
        var undirected = graph.Regions.ToDictionary(item => item.RegionId, _ => new SortedSet<string>(StringComparer.Ordinal), StringComparer.Ordinal);
        foreach (var edge in adjacency.SelectMany(item => item.Value))
        {
            if (undirected.TryGetValue(edge.FromRegionId, out var from))
            {
                from.Add(edge.ToRegionId);
            }

            if (undirected.TryGetValue(edge.ToRegionId, out var to))
            {
                to.Add(edge.FromRegionId);
            }
        }

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var components = new List<WorldScaleDisconnectedComponent>();
        foreach (var regionId in graph.Regions.Select(item => item.RegionId).Order(StringComparer.Ordinal))
        {
            if (!visited.Add(regionId))
            {
                continue;
            }

            var stack = new Stack<string>();
            var members = new List<string>();
            stack.Push(regionId);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                members.Add(current);
                foreach (var next in undirected[current].Reverse())
                {
                    if (visited.Add(next))
                    {
                        stack.Push(next);
                    }
                }
            }

            components.Add(new WorldScaleDisconnectedComponent
            {
                ComponentId = $"component/{graph.ScenarioId}/{components.Count + 1:000}",
                RegionIds = members.Order(StringComparer.Ordinal).ToList()
            });
        }

        return components.OrderBy(item => item.ComponentId, StringComparer.Ordinal).ToList();
    }

    private static int ComparePath(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        var leftKey = string.Join("|", left);
        var rightKey = string.Join("|", right);
        return string.CompareOrdinal(leftKey, rightKey);
    }

    private static void Add(IDictionary<string, List<AdjacentEdge>> adjacency, string regionId, AdjacentEdge edge)
    {
        if (adjacency.TryGetValue(regionId, out var edges))
        {
            edges.Add(edge);
        }
    }

    private sealed record AdjacentEdge(string EdgeId, string FromRegionId, string ToRegionId, int Cost);

    private sealed record PathState(string RegionId, int Cost, IReadOnlyList<string> RegionPath, IReadOnlyList<string> EdgePath);

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
