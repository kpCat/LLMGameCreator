using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimeChunkDeltaTraversal;

public sealed class RuntimeChunkTraversalPlannerTests
{
    [Fact]
    public void PlansAreDerivedFromGoal038ScenarioMapAndChunkFacts()
    {
        var plans = new RuntimeChunkTraversalPlanner().BuildPlans();

        Assert.Equal(
            ["caravan_trade", "frontier_survival", "gothic_intrigue", "metamodule_kingdoms"],
            plans.Select(item => item.ScenarioId).Order(StringComparer.Ordinal).ToArray());
        Assert.All(plans, plan =>
        {
            Assert.StartsWith("world-graph/", plan.WorldGraphId, StringComparison.Ordinal);
            Assert.StartsWith("finite-map/", plan.FiniteMapId, StringComparison.Ordinal);
            Assert.NotEmpty(plan.RequiredTargetRegionIds);
            Assert.NotEmpty(plan.Steps);
            Assert.NotEmpty(plan.Commands);
            Assert.All(plan.Steps, step =>
            {
                Assert.StartsWith("region/", step.RegionId, StringComparison.Ordinal);
                Assert.StartsWith("chunk/" + plan.ScenarioId + "/", step.ChunkId, StringComparison.Ordinal);
                Assert.False(string.IsNullOrWhiteSpace(step.Coordinate));
            });
        });

        var metamodule = plans.Single(item => item.ScenarioId == "metamodule_kingdoms");
        Assert.Equal(7, metamodule.SourceFacts.KingdomGroupCount);
        Assert.Equal(112, metamodule.SourceFacts.SpeciesArchetypeSlotRefCount);
        Assert.True(metamodule.RequiredTargetRegionIds.Count >= 7);
    }

    [Fact]
    public void ItineraryCommandsAreStableAndCoverRequiredDeltaKinds()
    {
        var first = new RuntimeChunkTraversalPlanner().BuildPlans();
        var second = new RuntimeChunkTraversalPlanner().BuildPlans();

        Assert.Equal(
            first.SelectMany(item => item.Commands).Select(item => item.DeltaId),
            second.SelectMany(item => item.Commands).Select(item => item.DeltaId));

        foreach (var plan in first)
        {
            var kinds = plan.Commands.Select(item => item.DeltaKind).ToHashSet(StringComparer.Ordinal);
            Assert.Contains("region_entered", kinds);
            Assert.Contains("chunk_discovered", kinds);
            Assert.Contains("landmark_discovered", kinds);
            Assert.Contains("route_checkpoint", kinds);
            Assert.Contains("local_mutation", kinds);
            Assert.Contains("deterministic_replay_marker", kinds);
            Assert.Equal(plan.Commands.Select(item => item.Order).Order().ToArray(), plan.Commands.Select(item => item.Order).ToArray());
        }
    }
}
