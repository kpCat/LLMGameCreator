using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;
using Xunit;

namespace LLMGameCreator.Tests.Application.WorldScaleRegionMapFoundation;

public sealed class WorldScaleRegionGraphTests
{
    [Fact]
    public void DefaultGraphsCoverFourScenariosAndRequiredRouteKinds()
    {
        var graphs = WorldScaleRegionMapCatalog.BuildDefaultGraphs();
        var validator = new WorldScaleRegionMapValidator();
        var routeKinds = graphs
            .SelectMany(item => item.TravelEdges)
            .Select(item => item.RouteKind)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(["caravan_trade", "frontier_survival", "gothic_intrigue", "metamodule_kingdoms"], graphs.Select(item => item.ScenarioId).Order(StringComparer.Ordinal));
        Assert.True(WorldScaleRegionMapVocabulary.RequiredRouteKinds.All(routeKinds.Contains));
        Assert.All(graphs, graph => Assert.DoesNotContain(validator.ValidateGraph(graph), item => item.Severity == "error"));
        Assert.Equal(4, graphs.Select(item => item.WorldGraphId).Distinct(StringComparer.Ordinal).Count());
        Assert.True(graphs.Select(item => item.Regions.Count).Distinct().Count() > 1);
    }

    [Fact]
    public void MetamoduleGraphPreservesSevenKingdomGroupsAndCanonicalSlotRefs()
    {
        var graph = WorldScaleRegionMapCatalog.BuildDefaultGraphs().Single(item => item.ScenarioId == "metamodule_kingdoms");
        var slots = graph.Kingdoms.SelectMany(item => item.SpeciesArchetypeSlotRefs).ToArray();

        Assert.Equal(7, graph.Kingdoms.Count);
        Assert.Equal(14, graph.Regions.Count);
        Assert.Equal(112, slots.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("slot/metamodule/species-archetype/001", slots);
        Assert.Contains("slot/metamodule/species-archetype/112", slots);
        Assert.Contains(graph.TravelEdges, edge => edge.RouteKind == "magical_gate");
        Assert.Contains(graph.TravelEdges, edge => edge.RouteKind == "sea_lane");
    }
}
