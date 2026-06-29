using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;
using Xunit;

namespace LLMGameCreator.Tests.Application.WorldScaleRegionMapFoundation;

public sealed class WorldScaleReachabilityPlannerTests
{
    [Fact]
    public void RequiredTargetsAreReachableWithStableCosts()
    {
        var graphs = WorldScaleRegionMapCatalog.BuildDefaultGraphs();
        var matrix = new WorldScaleReachabilityPlanner().BuildMatrix(graphs);

        Assert.Equal(4, matrix.ScenarioCount);
        Assert.True(matrix.AllRequiredTargetsReachable);
        Assert.Equal(matrix.RequiredTargetCount, matrix.ReachableRequiredTargetCount);
        Assert.All(matrix.Scenarios, scenario =>
        {
            Assert.True(scenario.AllRequiredReachable);
            Assert.Empty(scenario.UnreachableRequiredRegionIds);
            Assert.NotEmpty(scenario.RequiredTargetItineraries);
            Assert.All(scenario.RequiredTargetItineraries, itinerary =>
            {
                Assert.True(itinerary.TotalCost >= 0);
                Assert.Equal(scenario.StartRegionId, itinerary.RegionPath.First());
                Assert.Equal(itinerary.TargetRegionId, itinerary.RegionPath.Last());
            });
        });
    }

    [Fact]
    public void PlannerReportsUnreachableRequiredTargetsAndBlockedCriticalEdges()
    {
        var graph = WorldScaleRegionMapCatalog.BuildDefaultGraphs()
            .Single(item => item.ScenarioId == "frontier_survival");
        var mutated = graph with
        {
            TravelEdges = graph.TravelEdges
                .Select(edge => edge.EdgeId is "edge/frontier/pine-river" or "edge/frontier/river-pass"
                    ? edge with { IsBlocked = true }
                    : edge)
                .ToList()
        };

        var scenario = new WorldScaleReachabilityPlanner().Analyze(mutated);

        Assert.False(scenario.AllRequiredReachable);
        Assert.Contains("region/frontier/river-ford", scenario.UnreachableRequiredRegionIds);
        Assert.Contains("world_scale.required_target.unreachable", scenario.Diagnostics.Select(item => item.Code));
    }
}
