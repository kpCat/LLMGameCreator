using LLMGameCreator.Application.Design.CandidateNavigationPathfinding;
using Xunit;

namespace LLMGameCreator.Tests.Application.CandidateNavigationPathfinding;

public sealed class CandidateNavigationRouteGraphPlannerTests
{
    [Fact]
    public void SimpleRoadGraphReturnsDeterministicRouteAndCost()
    {
        var graph = Graph(
            [Node("settlement/a"), Node("road/junction"), Node("settlement/b")],
            [Edge("road/a-junction", "settlement/a", "road/junction", "road", 2), Edge("road/junction-b", "road/junction", "settlement/b", "road", 3)]);

        var result = Plan(graph, "settlement/a", "settlement/b");

        Assert.Equal(CandidateNavigationRouteGraphStatus.Success, result.Status);
        Assert.Equal(5, result.TotalCost);
        Assert.Equal(["settlement/a", "road/junction", "settlement/b"], result.NodeSteps.Select(step => step.NodeId).ToList());
        Assert.Equal(["road/a-junction", "road/junction-b"], result.EdgeSteps.Select(step => step.EdgeId).ToList());
    }

    [Fact]
    public void ChoosesCheaperRoadRouteOverShorterExpensiveTrail()
    {
        var graph = Graph(
            [Node("start"), Node("road/mid"), Node("goal")],
            [
                Edge("trail/direct", "start", "goal", "trail", 10),
                Edge("road/first", "start", "road/mid", "road", 2),
                Edge("road/second", "road/mid", "goal", "road", 2)
            ]);

        var result = Plan(graph, "start", "goal");

        Assert.Equal(CandidateNavigationRouteGraphStatus.Success, result.Status);
        Assert.Equal(4, result.TotalCost);
        Assert.Equal(["road/first", "road/second"], result.EdgeSteps.Select(step => step.EdgeId).ToList());
    }

    [Fact]
    public void DirectedEdgeCannotBeUsedBackward()
    {
        var graph = Graph([Node("a"), Node("b")], [Edge("road/a-b", "a", "b", "road", 1)]);

        var result = Plan(graph, "b", "a");

        Assert.Equal(CandidateNavigationRouteGraphStatus.NoPath, result.Status);
    }

    [Fact]
    public void BidirectionalEdgeCanBeUsedBackward()
    {
        var graph = Graph([Node("a"), Node("b")], [Edge("road/a-b", "a", "b", "road", 1, bidirectional: true)]);

        var result = Plan(graph, "b", "a");

        Assert.Equal(CandidateNavigationRouteGraphStatus.Success, result.Status);
        Assert.Equal(1, result.TotalCost);
        Assert.Equal(["b", "a"], result.NodeSteps.Select(step => step.NodeId).ToList());
        Assert.Equal("road/a-b", Assert.Single(result.EdgeSteps).EdgeId);
    }

    [Fact]
    public void BlockedRouteKindForcesDetour()
    {
        var graph = Graph(
            [Node("start"), Node("trail/mid"), Node("road/mid"), Node("goal")],
            [
                Edge("trail/first", "start", "trail/mid", "trail", 1),
                Edge("trail/second", "trail/mid", "goal", "trail", 1),
                Edge("road/first", "start", "road/mid", "road", 3),
                Edge("road/second", "road/mid", "goal", "road", 3)
            ]);
        var profile = CandidateNavigationRouteGraphMovementProfile.Default with
        {
            Id = "movement/no_trails",
            BlockedRouteKinds = ["trail"]
        };

        var result = Plan(graph, "start", "goal", profile);

        Assert.Equal(CandidateNavigationRouteGraphStatus.Success, result.Status);
        Assert.Equal(6, result.TotalCost);
        Assert.Equal(["road/first", "road/second"], result.EdgeSteps.Select(step => step.EdgeId).ToList());
    }

    [Fact]
    public void BlockedNodeAndOverlayPreventTraversal()
    {
        var nodeBlockedGraph = Graph(
            [Node("start"), Node("blocked", blocked: true), Node("goal")],
            [Edge("road/blocked", "start", "blocked", "road", 1), Edge("road/goal", "blocked", "goal", "road", 1)]);
        var overlayBlockedGraph = Graph(
            [Node("start"), Node("closed", overlays: ["closed"]), Node("goal")],
            [Edge("road/closed", "start", "closed", "road", 1), Edge("road/goal", "closed", "goal", "road", 1)]);
        var profile = CandidateNavigationRouteGraphMovementProfile.Default with
        {
            Id = "movement/avoid_closed",
            BlockedOverlayIds = ["closed"]
        };

        Assert.Equal(CandidateNavigationRouteGraphStatus.NoPath, Plan(nodeBlockedGraph, "start", "goal").Status);
        Assert.Equal(CandidateNavigationRouteGraphStatus.NoPath, Plan(overlayBlockedGraph, "start", "goal", profile).Status);
    }

    [Fact]
    public void HazardOverlayPenaltyAvoidsHazardousRouteWhenAlternativeIsCheaper()
    {
        var graph = Graph(
            [Node("start"), Node("hazard/mid"), Node("safe/mid"), Node("goal")],
            [
                Edge("trail/hazard-1", "start", "hazard/mid", "trail", 1, overlays: ["hazard"]),
                Edge("trail/hazard-2", "hazard/mid", "goal", "trail", 1, overlays: ["hazard"]),
                Edge("road/safe-1", "start", "safe/mid", "road", 3),
                Edge("road/safe-2", "safe/mid", "goal", "road", 3)
            ]);
        var profile = CandidateNavigationRouteGraphMovementProfile.Default with
        {
            Id = "movement/cautious",
            OverlayAdditionalCosts = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["hazard"] = 5
            }
        };

        var result = Plan(graph, "start", "goal", profile);

        Assert.Equal(CandidateNavigationRouteGraphStatus.Success, result.Status);
        Assert.Equal(6, result.TotalCost);
        Assert.Equal(["road/safe-1", "road/safe-2"], result.EdgeSteps.Select(step => step.EdgeId).ToList());
    }

    [Fact]
    public void UnknownRouteKindAndOverlayUseBaseBehavior()
    {
        var graph = Graph(
            [Node("start"), Node("goal")],
            [Edge("unknown/direct", "start", "goal", "unknown_route", 4, overlays: ["unknown_overlay"])]);

        var result = Plan(graph, "start", "goal");

        Assert.Equal(CandidateNavigationRouteGraphStatus.Success, result.Status);
        Assert.Equal(4, result.TotalCost);
    }

    [Theory]
    [MemberData(nameof(InvalidGraphs))]
    public void InvalidGraphCasesReturnInvalidGraph(CandidateNavigationRouteGraph graph)
    {
        var result = Plan(graph, "start", "goal");

        Assert.Equal(CandidateNavigationRouteGraphStatus.InvalidGraph, result.Status);
        Assert.NotEmpty(result.Diagnostics);
    }

    [Fact]
    public void MissingStartAndGoalReturnSpecificStatuses()
    {
        var graph = Graph([Node("present")], []);

        Assert.Equal(CandidateNavigationRouteGraphStatus.StartMissing, Plan(graph, "missing", "present").Status);
        Assert.Equal(CandidateNavigationRouteGraphStatus.GoalMissing, Plan(graph, "present", "missing").Status);
    }

    [Fact]
    public void BlockedStartAndGoalReturnSpecificStatuses()
    {
        var blockedStartGraph = Graph([Node("start", blocked: true), Node("goal")], []);
        var blockedGoalGraph = Graph([Node("start"), Node("goal", blocked: true)], []);

        Assert.Equal(CandidateNavigationRouteGraphStatus.StartBlocked, Plan(blockedStartGraph, "start", "goal").Status);
        Assert.Equal(CandidateNavigationRouteGraphStatus.GoalBlocked, Plan(blockedGoalGraph, "start", "goal").Status);
    }

    [Fact]
    public void DisconnectedGraphReturnsNoPath()
    {
        var graph = Graph([Node("start"), Node("goal")], []);

        var result = Plan(graph, "start", "goal");

        Assert.Equal(CandidateNavigationRouteGraphStatus.NoPath, result.Status);
    }

    [Fact]
    public void MaxExpandedNodesReturnsSearchLimitReached()
    {
        var graph = Graph(
            [Node("start"), Node("mid"), Node("goal")],
            [Edge("road/first", "start", "mid", "road", 1), Edge("road/second", "mid", "goal", "road", 1)]);

        var result = Plan(graph, "start", "goal", maxExpandedNodes: 1);

        Assert.Equal(CandidateNavigationRouteGraphStatus.SearchLimitReached, result.Status);
        Assert.Equal(1, result.ExpandedNodeCount);
        Assert.Empty(result.NodeSteps);
    }

    [Fact]
    public void SourceNodeAndEdgeOrderDoesNotAffectResult()
    {
        var nodes = new[] { Node("goal"), Node("a"), Node("b"), Node("start") };
        var edges = new[]
        {
            Edge("road/start-b", "start", "b", "road", 2),
            Edge("road/b-goal", "b", "goal", "road", 2),
            Edge("road/start-a", "start", "a", "road", 2),
            Edge("road/a-goal", "a", "goal", "road", 2)
        };

        var first = Plan(Graph(nodes, edges), "start", "goal");
        var second = Plan(Graph(nodes.AsEnumerable().Reverse(), edges.AsEnumerable().Reverse()), "start", "goal");

        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.TotalCost, second.TotalCost);
        Assert.Equal(first.NodeSteps.Select(step => step.NodeId), second.NodeSteps.Select(step => step.NodeId));
        Assert.Equal(first.EdgeSteps.Select(step => step.EdgeId), second.EdgeSteps.Select(step => step.EdgeId));
        Assert.Equal(["start", "a", "goal"], first.NodeSteps.Select(step => step.NodeId).ToList());
    }

    [Fact]
    public void RepeatedIdenticalRequestReturnsIdenticalStatusCostAndSteps()
    {
        var graph = Graph(
            [Node("start"), Node("mid"), Node("goal")],
            [Edge("road/first", "start", "mid", "road", 2), Edge("road/second", "mid", "goal", "road", 3)]);
        var request = new CandidateNavigationRouteGraphRequest
        {
            Graph = graph,
            StartNodeId = "start",
            GoalNodeId = "goal"
        };
        var planner = new CandidateNavigationRouteGraphPlanner();

        var first = planner.Plan(request);
        var second = planner.Plan(request);

        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.TotalCost, second.TotalCost);
        Assert.Equal(first.NodeSteps.Select(step => step.NodeId), second.NodeSteps.Select(step => step.NodeId));
        Assert.Equal(first.EdgeSteps.Select(step => step.EdgeId), second.EdgeSteps.Select(step => step.EdgeId));
    }

    [Fact]
    public void RouteKindOverrideAndOverlayOverrideResolveEffectiveCost()
    {
        var graph = Graph(
            [Node("start"), Node("bridge"), Node("goal")],
            [
                Edge("river/bridge", "start", "bridge", "river", 20, overlays: ["bridge", "maintained"]),
                Edge("road/goal", "bridge", "goal", "road", 3)
            ]);
        var profile = CandidateNavigationRouteGraphMovementProfile.Default with
        {
            Id = "movement/bridge",
            RouteKindCostOverrides = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["river"] = 9
            },
            RouteKindAdditionalCosts = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["river"] = 1
            },
            OverlayEnterCostOverrides = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["bridge"] = 4,
                ["maintained"] = 2
            },
            OverlayAdditionalCosts = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["bridge"] = -5,
                ["maintained"] = 1
            }
        };

        var result = Plan(graph, "start", "goal", profile);

        Assert.Equal(CandidateNavigationRouteGraphStatus.Success, result.Status);
        Assert.Equal(4, result.TotalCost);
        Assert.Equal([1, 3], result.EdgeSteps.Select(step => step.Cost).ToList());
    }

    public static IEnumerable<object[]> InvalidGraphs()
    {
        yield return
        [
            Graph(
                [Node("start"), Node("start"), Node("goal")],
                [Edge("road/direct", "start", "goal", "road", 1)])
        ];
        yield return
        [
            Graph(
                [Node("start"), Node("goal")],
                [Edge("road/direct", "start", "goal", "road", 1), Edge("road/direct", "goal", "start", "road", 1)])
        ];
        yield return
        [
            Graph(
                [Node("start"), Node("goal")],
                [Edge("road/missing", "start", "missing", "road", 1)])
        ];
        yield return
        [
            Graph(
                [Node("start"), Node("goal")],
                [Edge("road/invalid", "start", "goal", "road", 0)])
        ];
    }

    private static CandidateNavigationRouteGraphResult Plan(
        CandidateNavigationRouteGraph graph,
        string startNodeId,
        string goalNodeId,
        CandidateNavigationRouteGraphMovementProfile? profile = null,
        int maxExpandedNodes = 10_000)
    {
        return new CandidateNavigationRouteGraphPlanner().Plan(new CandidateNavigationRouteGraphRequest
        {
            Graph = graph,
            StartNodeId = startNodeId,
            GoalNodeId = goalNodeId,
            MovementProfile = profile ?? CandidateNavigationRouteGraphMovementProfile.Default,
            MaxExpandedNodes = maxExpandedNodes
        });
    }

    private static CandidateNavigationRouteGraph Graph(
        IEnumerable<CandidateNavigationRouteGraphNode> nodes,
        IEnumerable<CandidateNavigationRouteGraphEdge> edges)
    {
        return new CandidateNavigationRouteGraph
        {
            Nodes = nodes.ToList(),
            Edges = edges.ToList()
        };
    }

    private static CandidateNavigationRouteGraphNode Node(
        string id,
        string kind = "settlement",
        bool blocked = false,
        IReadOnlyList<string>? overlays = null)
    {
        return new CandidateNavigationRouteGraphNode
        {
            Id = id,
            Kind = kind,
            IsBlocked = blocked,
            OverlayIds = overlays ?? []
        };
    }

    private static CandidateNavigationRouteGraphEdge Edge(
        string id,
        string from,
        string to,
        string kind,
        int cost,
        bool bidirectional = false,
        IReadOnlyList<string>? overlays = null)
    {
        return new CandidateNavigationRouteGraphEdge
        {
            Id = id,
            FromNodeId = from,
            ToNodeId = to,
            RouteKind = kind,
            BaseCost = cost,
            IsBidirectional = bidirectional,
            OverlayIds = overlays ?? []
        };
    }
}
