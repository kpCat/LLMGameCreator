namespace LLMGameCreator.Application.Design.CandidateNavigationPathfinding;

public sealed class CandidateNavigationRouteGraphPlanner
{
    public CandidateNavigationRouteGraphResult Plan(CandidateNavigationRouteGraphRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Graph);
        ArgumentNullException.ThrowIfNull(request.MovementProfile);

        var validation = ValidateGraph(request.Graph, request.StartNodeId, request.GoalNodeId);
        if (validation.Status == CandidateNavigationRouteGraphStatus.InvalidGraph)
        {
            return validation;
        }

        var nodesById = request.Graph.Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        if (!nodesById.TryGetValue(request.StartNodeId, out var startNode))
        {
            return Failure(CandidateNavigationRouteGraphStatus.StartMissing, "candidate_navigation.route_graph.start_missing", request.StartNodeId);
        }

        if (!nodesById.TryGetValue(request.GoalNodeId, out var goalNode))
        {
            return Failure(CandidateNavigationRouteGraphStatus.GoalMissing, "candidate_navigation.route_graph.goal_missing", request.GoalNodeId);
        }

        if (IsNodeBlocked(startNode, request.MovementProfile))
        {
            return Failure(CandidateNavigationRouteGraphStatus.StartBlocked, "candidate_navigation.route_graph.start_blocked", startNode.Id);
        }

        if (IsNodeBlocked(goalNode, request.MovementProfile))
        {
            return Failure(CandidateNavigationRouteGraphStatus.GoalBlocked, "candidate_navigation.route_graph.goal_blocked", goalNode.Id);
        }

        if (request.StartNodeId == request.GoalNodeId)
        {
            return new CandidateNavigationRouteGraphResult
            {
                Status = CandidateNavigationRouteGraphStatus.Success,
                TotalCost = 0,
                NodeSteps = [NodeStep(startNode, 0)],
                ExpandedNodeCount = 0,
                VisitedNodeCount = 1,
                Diagnostics =
                [
                    CandidateNavigationRouteDiagnostic.Info(
                        "candidate_navigation.route_graph.start_equals_goal",
                        startNode.Id,
                        "Start and goal are the same traversable route graph node.")
                ]
            };
        }

        var adjacency = BuildAdjacency(request.Graph);
        var open = new List<CandidateNavigationRouteGraphOpenNode>();
        var searchNodes = new Dictionary<string, CandidateNavigationRouteGraphSearchNode>(StringComparer.Ordinal);
        var closed = new HashSet<string>(StringComparer.Ordinal);
        var insertionOrdinal = 0;

        searchNodes[startNode.Id] = new CandidateNavigationRouteGraphSearchNode
        {
            NodeId = startNode.Id,
            GCost = 0,
            InsertionOrdinal = insertionOrdinal
        };
        open.Add(CandidateNavigationRouteGraphOpenNode.From(searchNodes[startNode.Id]));

        var expanded = 0;
        while (open.Count > 0)
        {
            open.Sort(CandidateNavigationRouteGraphOpenNodeComparer.Instance);
            var currentOpen = open[0];
            open.RemoveAt(0);

            if (closed.Contains(currentOpen.NodeId))
            {
                continue;
            }

            if (expanded >= request.MaxExpandedNodes)
            {
                return new CandidateNavigationRouteGraphResult
                {
                    Status = CandidateNavigationRouteGraphStatus.SearchLimitReached,
                    ExpandedNodeCount = expanded,
                    VisitedNodeCount = searchNodes.Count,
                    Diagnostics =
                    [
                        CandidateNavigationRouteDiagnostic.Error(
                            "candidate_navigation.route_graph.search_limit_reached",
                            request.MaxExpandedNodes.ToString(),
                            "Route graph search budget was exhausted before a route was found.")
                    ]
                };
            }

            var current = searchNodes[currentOpen.NodeId];
            closed.Add(current.NodeId);
            expanded++;

            if (current.NodeId == request.GoalNodeId)
            {
                return BuildSuccessResult(nodesById, searchNodes, current, expanded);
            }

            if (!adjacency.TryGetValue(current.NodeId, out var outgoingEdges))
            {
                continue;
            }

            foreach (var edge in outgoingEdges)
            {
                if (closed.Contains(edge.ToNodeId) ||
                    !nodesById.TryGetValue(edge.ToNodeId, out var targetNode) ||
                    !TryResolveEdgeCost(edge, targetNode, request.MovementProfile, out var edgeCost))
                {
                    continue;
                }

                var nextGCost = checked(current.GCost + edgeCost);
                if (!searchNodes.TryGetValue(edge.ToNodeId, out var existing) ||
                    IsBetter(nextGCost, current.NodeId, edge.Edge.Id, existing))
                {
                    insertionOrdinal++;
                    var next = new CandidateNavigationRouteGraphSearchNode
                    {
                        NodeId = edge.ToNodeId,
                        GCost = nextGCost,
                        PreviousNodeId = current.NodeId,
                        IncomingEdgeId = edge.Edge.Id,
                        IncomingRouteKind = edge.Edge.RouteKind,
                        IncomingEdgeCost = edgeCost,
                        InsertionOrdinal = insertionOrdinal
                    };
                    searchNodes[edge.ToNodeId] = next;
                    open.Add(CandidateNavigationRouteGraphOpenNode.From(next));
                }
            }
        }

        return new CandidateNavigationRouteGraphResult
        {
            Status = CandidateNavigationRouteGraphStatus.NoPath,
            ExpandedNodeCount = expanded,
            VisitedNodeCount = searchNodes.Count,
            Diagnostics =
            [
                CandidateNavigationRouteDiagnostic.Error(
                    "candidate_navigation.route_graph.no_path",
                    request.GoalNodeId,
                    "No traversable route graph path reaches the requested goal.")
            ]
        };
    }

    private static CandidateNavigationRouteGraphResult ValidateGraph(
        CandidateNavigationRouteGraph graph,
        string startNodeId,
        string goalNodeId)
    {
        if (string.IsNullOrWhiteSpace(startNodeId))
        {
            return Invalid("candidate_navigation.route_graph.invalid_start", "startNodeId", "Route graph start node id is required.");
        }

        if (string.IsNullOrWhiteSpace(goalNodeId))
        {
            return Invalid("candidate_navigation.route_graph.invalid_goal", "goalNodeId", "Route graph goal node id is required.");
        }

        var nodeIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var node in graph.Nodes.OrderBy(node => node.Id, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(node.Id))
            {
                return Invalid("candidate_navigation.route_graph.invalid_node_id", "node", "Route graph node id is required.");
            }

            if (!nodeIds.Add(node.Id))
            {
                return Invalid("candidate_navigation.route_graph.duplicate_node", node.Id, "Route graph node ids must be unique.");
            }
        }

        var edgeIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var edge in graph.Edges.OrderBy(edge => edge.Id, StringComparer.Ordinal))
        {
            if (string.IsNullOrWhiteSpace(edge.Id))
            {
                return Invalid("candidate_navigation.route_graph.invalid_edge_id", "edge", "Route graph edge id is required.");
            }

            if (!edgeIds.Add(edge.Id))
            {
                return Invalid("candidate_navigation.route_graph.duplicate_edge", edge.Id, "Route graph edge ids must be unique.");
            }

            if (edge.BaseCost <= 0)
            {
                return Invalid("candidate_navigation.route_graph.invalid_edge_cost", edge.Id, "Route graph edge base cost must be greater than zero.");
            }

            if (!nodeIds.Contains(edge.FromNodeId))
            {
                return Invalid("candidate_navigation.route_graph.missing_from_node", edge.Id, "Route graph edge references a missing fromNodeId.");
            }

            if (!nodeIds.Contains(edge.ToNodeId))
            {
                return Invalid("candidate_navigation.route_graph.missing_to_node", edge.Id, "Route graph edge references a missing toNodeId.");
            }
        }

        return new CandidateNavigationRouteGraphResult { Status = CandidateNavigationRouteGraphStatus.Success };
    }

    private static Dictionary<string, List<CandidateNavigationRouteGraphTraversalEdge>> BuildAdjacency(
        CandidateNavigationRouteGraph graph)
    {
        var adjacency = new Dictionary<string, List<CandidateNavigationRouteGraphTraversalEdge>>(StringComparer.Ordinal);
        foreach (var edge in graph.Edges.OrderBy(edge => edge.Id, StringComparer.Ordinal))
        {
            AddTraversalEdge(adjacency, new CandidateNavigationRouteGraphTraversalEdge(edge, edge.FromNodeId, edge.ToNodeId, 0));
            if (edge.IsBidirectional)
            {
                AddTraversalEdge(adjacency, new CandidateNavigationRouteGraphTraversalEdge(edge, edge.ToNodeId, edge.FromNodeId, 1));
            }
        }

        foreach (var entry in adjacency.Values)
        {
            entry.Sort(CandidateNavigationRouteGraphTraversalEdgeComparer.Instance);
        }

        return adjacency;
    }

    private static void AddTraversalEdge(
        IDictionary<string, List<CandidateNavigationRouteGraphTraversalEdge>> adjacency,
        CandidateNavigationRouteGraphTraversalEdge edge)
    {
        if (!adjacency.TryGetValue(edge.FromNodeId, out var edges))
        {
            edges = [];
            adjacency[edge.FromNodeId] = edges;
        }

        edges.Add(edge);
    }

    private static bool IsNodeBlocked(
        CandidateNavigationRouteGraphNode node,
        CandidateNavigationRouteGraphMovementProfile movementProfile)
    {
        var blockedNodeKinds = movementProfile.BlockedNodeKinds.ToHashSet(StringComparer.Ordinal);
        var blockedOverlayIds = movementProfile.BlockedOverlayIds.ToHashSet(StringComparer.Ordinal);
        var overlayIds = NormalizeIds(node.OverlayIds);

        return node.IsBlocked ||
               blockedNodeKinds.Contains(node.Kind) ||
               overlayIds.Any(blockedOverlayIds.Contains);
    }

    private static bool TryResolveEdgeCost(
        CandidateNavigationRouteGraphTraversalEdge traversalEdge,
        CandidateNavigationRouteGraphNode targetNode,
        CandidateNavigationRouteGraphMovementProfile movementProfile,
        out int edgeCost)
    {
        edgeCost = 0;
        var edge = traversalEdge.Edge;
        var blockedRouteKinds = movementProfile.BlockedRouteKinds.ToHashSet(StringComparer.Ordinal);
        var blockedOverlayIds = movementProfile.BlockedOverlayIds.ToHashSet(StringComparer.Ordinal);
        var edgeOverlayIds = NormalizeIds(edge.OverlayIds);
        var targetOverlayIds = NormalizeIds(targetNode.OverlayIds);

        if (edge.IsBlocked ||
            IsNodeBlocked(targetNode, movementProfile) ||
            blockedRouteKinds.Contains(edge.RouteKind) ||
            edgeOverlayIds.Any(blockedOverlayIds.Contains) ||
            targetOverlayIds.Any(blockedOverlayIds.Contains))
        {
            return false;
        }

        edgeCost = edge.BaseCost;
        if (movementProfile.RouteKindCostOverrides.TryGetValue(edge.RouteKind, out var routeOverride) &&
            routeOverride > 0)
        {
            edgeCost = routeOverride;
        }

        if (movementProfile.RouteKindAdditionalCosts.TryGetValue(edge.RouteKind, out var routeAdditional) &&
            routeAdditional >= 0)
        {
            edgeCost = checked(edgeCost + routeAdditional);
        }

        var overlayOverride = edgeOverlayIds
            .Select(overlayId => movementProfile.OverlayEnterCostOverrides.TryGetValue(overlayId, out var value) ? value : 0)
            .Where(value => value > 0)
            .DefaultIfEmpty(0)
            .Min();
        if (overlayOverride > 0)
        {
            edgeCost = overlayOverride;
        }

        foreach (var overlayId in edgeOverlayIds)
        {
            if (movementProfile.OverlayAdditionalCosts.TryGetValue(overlayId, out var overlayAdditional))
            {
                edgeCost = checked(edgeCost + overlayAdditional);
            }
        }

        edgeCost = Math.Max(1, edgeCost);
        return true;
    }

    private static CandidateNavigationRouteGraphResult BuildSuccessResult(
        IReadOnlyDictionary<string, CandidateNavigationRouteGraphNode> nodesById,
        IReadOnlyDictionary<string, CandidateNavigationRouteGraphSearchNode> searchNodes,
        CandidateNavigationRouteGraphSearchNode goal,
        int expanded)
    {
        var reversed = new List<CandidateNavigationRouteGraphSearchNode>();
        var current = goal;
        while (true)
        {
            reversed.Add(current);
            if (current.PreviousNodeId is not { } previousNodeId)
            {
                break;
            }

            current = searchNodes[previousNodeId];
        }

        reversed.Reverse();
        var nodeSteps = reversed
            .Select((node, index) => NodeStep(nodesById[node.NodeId], index))
            .ToList();
        var edgeSteps = reversed
            .Skip(1)
            .Select((node, index) => new CandidateNavigationRouteGraphEdgeStep
            {
                StepIndex = index,
                EdgeId = node.IncomingEdgeId,
                FromNodeId = node.PreviousNodeId ?? string.Empty,
                ToNodeId = node.NodeId,
                RouteKind = node.IncomingRouteKind,
                Cost = node.IncomingEdgeCost
            })
            .ToList();

        return new CandidateNavigationRouteGraphResult
        {
            Status = CandidateNavigationRouteGraphStatus.Success,
            TotalCost = goal.GCost,
            NodeSteps = nodeSteps,
            EdgeSteps = edgeSteps,
            ExpandedNodeCount = expanded,
            VisitedNodeCount = searchNodes.Count,
            Diagnostics =
            [
                CandidateNavigationRouteDiagnostic.Info(
                    "candidate_navigation.route_graph.route_found",
                    goal.NodeId,
                    "Deterministic route graph path was found.")
            ]
        };
    }

    private static CandidateNavigationRouteGraphNodeStep NodeStep(
        CandidateNavigationRouteGraphNode node,
        int stepIndex)
    {
        return new CandidateNavigationRouteGraphNodeStep
        {
            StepIndex = stepIndex,
            NodeId = node.Id,
            Kind = node.Kind,
            X = node.X,
            Y = node.Y
        };
    }

    private static bool IsBetter(
        int nextGCost,
        string previousNodeId,
        string edgeId,
        CandidateNavigationRouteGraphSearchNode existing)
    {
        if (nextGCost != existing.GCost)
        {
            return nextGCost < existing.GCost;
        }

        var compare = string.CompareOrdinal(previousNodeId, existing.PreviousNodeId);
        if (compare != 0)
        {
            return compare < 0;
        }

        return string.CompareOrdinal(edgeId, existing.IncomingEdgeId) < 0;
    }

    private static IReadOnlyList<string> NormalizeIds(IEnumerable<string> ids)
    {
        return ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
    }

    private static CandidateNavigationRouteGraphResult Failure(
        CandidateNavigationRouteGraphStatus status,
        string code,
        string target)
    {
        return new CandidateNavigationRouteGraphResult
        {
            Status = status,
            Diagnostics =
            [
                CandidateNavigationRouteDiagnostic.Error(
                    code,
                    target,
                    "Route graph request could not be planned.")
            ]
        };
    }

    private static CandidateNavigationRouteGraphResult Invalid(string code, string target, string message)
    {
        return new CandidateNavigationRouteGraphResult
        {
            Status = CandidateNavigationRouteGraphStatus.InvalidGraph,
            Diagnostics =
            [
                CandidateNavigationRouteDiagnostic.Error(code, target, message)
            ]
        };
    }

    private sealed record CandidateNavigationRouteGraphSearchNode
    {
        public string NodeId { get; init; } = string.Empty;
        public int GCost { get; init; }
        public string? PreviousNodeId { get; init; }
        public string IncomingEdgeId { get; init; } = string.Empty;
        public string IncomingRouteKind { get; init; } = string.Empty;
        public int IncomingEdgeCost { get; init; }
        public int InsertionOrdinal { get; init; }
    }

    private sealed record CandidateNavigationRouteGraphOpenNode
    {
        public string NodeId { get; init; } = string.Empty;
        public int GCost { get; init; }
        public string IncomingEdgeId { get; init; } = string.Empty;
        public int InsertionOrdinal { get; init; }

        public static CandidateNavigationRouteGraphOpenNode From(CandidateNavigationRouteGraphSearchNode node) => new()
        {
            NodeId = node.NodeId,
            GCost = node.GCost,
            IncomingEdgeId = node.IncomingEdgeId,
            InsertionOrdinal = node.InsertionOrdinal
        };
    }

    private sealed record CandidateNavigationRouteGraphTraversalEdge(
        CandidateNavigationRouteGraphEdge Edge,
        string FromNodeId,
        string ToNodeId,
        int DirectionOrdinal);

    private sealed class CandidateNavigationRouteGraphOpenNodeComparer : IComparer<CandidateNavigationRouteGraphOpenNode>
    {
        public static readonly CandidateNavigationRouteGraphOpenNodeComparer Instance = new();

        public int Compare(CandidateNavigationRouteGraphOpenNode? x, CandidateNavigationRouteGraphOpenNode? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            var compare = x.GCost.CompareTo(y.GCost);
            if (compare != 0)
            {
                return compare;
            }

            compare = string.CompareOrdinal(x.NodeId, y.NodeId);
            if (compare != 0)
            {
                return compare;
            }

            compare = string.CompareOrdinal(x.IncomingEdgeId, y.IncomingEdgeId);
            if (compare != 0)
            {
                return compare;
            }

            return x.InsertionOrdinal.CompareTo(y.InsertionOrdinal);
        }
    }

    private sealed class CandidateNavigationRouteGraphTraversalEdgeComparer : IComparer<CandidateNavigationRouteGraphTraversalEdge>
    {
        public static readonly CandidateNavigationRouteGraphTraversalEdgeComparer Instance = new();

        public int Compare(CandidateNavigationRouteGraphTraversalEdge? x, CandidateNavigationRouteGraphTraversalEdge? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            var compare = string.CompareOrdinal(x.ToNodeId, y.ToNodeId);
            if (compare != 0)
            {
                return compare;
            }

            compare = string.CompareOrdinal(x.Edge.Id, y.Edge.Id);
            if (compare != 0)
            {
                return compare;
            }

            compare = x.DirectionOrdinal.CompareTo(y.DirectionOrdinal);
            if (compare != 0)
            {
                return compare;
            }

            return string.CompareOrdinal(x.Edge.RouteKind, y.Edge.RouteKind);
        }
    }
}

public sealed record CandidateNavigationRouteGraphRequest
{
    public CandidateNavigationRouteGraph Graph { get; init; } = CandidateNavigationRouteGraph.Empty;
    public string StartNodeId { get; init; } = string.Empty;
    public string GoalNodeId { get; init; } = string.Empty;
    public CandidateNavigationRouteGraphMovementProfile MovementProfile { get; init; } = CandidateNavigationRouteGraphMovementProfile.Default;
    public int MaxExpandedNodes { get; init; } = 10_000;
}

public sealed record CandidateNavigationRouteGraph
{
    public static readonly CandidateNavigationRouteGraph Empty = new();

    public IReadOnlyList<CandidateNavigationRouteGraphNode> Nodes { get; init; } = [];
    public IReadOnlyList<CandidateNavigationRouteGraphEdge> Edges { get; init; } = [];
}

public sealed record CandidateNavigationRouteGraphNode
{
    public string Id { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public int? X { get; init; }
    public int? Y { get; init; }
    public bool IsBlocked { get; init; }
    public IReadOnlyList<string> OverlayIds { get; init; } = [];
    public IReadOnlyList<string> TagIds { get; init; } = [];
}

public sealed record CandidateNavigationRouteGraphEdge
{
    public string Id { get; init; } = string.Empty;
    public string FromNodeId { get; init; } = string.Empty;
    public string ToNodeId { get; init; } = string.Empty;
    public string RouteKind { get; init; } = string.Empty;
    public int BaseCost { get; init; } = 1;
    public bool IsBlocked { get; init; }
    public bool IsBidirectional { get; init; }
    public IReadOnlyList<string> OverlayIds { get; init; } = [];
    public IReadOnlyList<string> TagIds { get; init; } = [];
}

public sealed record CandidateNavigationRouteGraphMovementProfile
{
    public static readonly CandidateNavigationRouteGraphMovementProfile Default = new()
    {
        Id = "route_graph/default"
    };

    public string Id { get; init; } = "route_graph/default";
    public IReadOnlyCollection<string> BlockedNodeKinds { get; init; } = [];
    public IReadOnlyCollection<string> BlockedRouteKinds { get; init; } = [];
    public IReadOnlyCollection<string> BlockedOverlayIds { get; init; } = [];
    public IReadOnlyDictionary<string, int> RouteKindCostOverrides { get; init; } =
        new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> RouteKindAdditionalCosts { get; init; } =
        new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> OverlayEnterCostOverrides { get; init; } =
        new Dictionary<string, int>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> OverlayAdditionalCosts { get; init; } =
        new Dictionary<string, int>(StringComparer.Ordinal);
}

public sealed record CandidateNavigationRouteGraphResult
{
    public CandidateNavigationRouteGraphStatus Status { get; init; }
    public int TotalCost { get; init; }
    public IReadOnlyList<CandidateNavigationRouteGraphNodeStep> NodeSteps { get; init; } = [];
    public IReadOnlyList<CandidateNavigationRouteGraphEdgeStep> EdgeSteps { get; init; } = [];
    public int ExpandedNodeCount { get; init; }
    public int VisitedNodeCount { get; init; }
    public IReadOnlyList<CandidateNavigationRouteDiagnostic> Diagnostics { get; init; } = [];
}

public sealed record CandidateNavigationRouteGraphNodeStep
{
    public int StepIndex { get; init; }
    public string NodeId { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public int? X { get; init; }
    public int? Y { get; init; }
}

public sealed record CandidateNavigationRouteGraphEdgeStep
{
    public int StepIndex { get; init; }
    public string EdgeId { get; init; } = string.Empty;
    public string FromNodeId { get; init; } = string.Empty;
    public string ToNodeId { get; init; } = string.Empty;
    public string RouteKind { get; init; } = string.Empty;
    public int Cost { get; init; }
}

public sealed record CandidateNavigationRouteDiagnostic
{
    public string Severity { get; init; } = "info";
    public string Code { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static CandidateNavigationRouteDiagnostic Info(string code, string target, string message) =>
        new()
        {
            Severity = "info",
            Code = code,
            Target = target,
            Message = message
        };

    public static CandidateNavigationRouteDiagnostic Error(string code, string target, string message) =>
        new()
        {
            Severity = "error",
            Code = code,
            Target = target,
            Message = message
        };
}

public enum CandidateNavigationRouteGraphStatus
{
    Success,
    StartMissing,
    GoalMissing,
    StartBlocked,
    GoalBlocked,
    NoPath,
    SearchLimitReached,
    InvalidGraph
}
