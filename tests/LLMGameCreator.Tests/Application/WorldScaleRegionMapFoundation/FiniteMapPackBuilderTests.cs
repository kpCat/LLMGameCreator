using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;
using Xunit;

namespace LLMGameCreator.Tests.Application.WorldScaleRegionMapFoundation;

public sealed class FiniteMapPackBuilderTests
{
    [Fact]
    public void FiniteMapPacksBindRegionsLandmarksRoutesAndHooks()
    {
        var graphs = WorldScaleRegionMapCatalog.BuildDefaultGraphs();
        var packs = new FiniteMapPackBuilder().BuildMapPacksByFileName(graphs);
        var validator = new WorldScaleRegionMapValidator();

        Assert.Equal(4, packs.Count);
        Assert.Contains(packs.Values, item => item.CoordinateKind == "axial_hex");
        foreach (var graph in graphs)
        {
            var pack = packs[FiniteMapPackBuilder.FileName(graph.ScenarioId)];
            Assert.Equal(graph.Regions.Count, pack.RegionBindings.Count);
            Assert.True(pack.LandmarkPlacements.Count >= graph.Regions.Count);
            Assert.Equal(graph.TravelEdges.Count, pack.RouteSummaries.Count);
            Assert.NotEmpty(pack.HookPlacements);
            Assert.True(pack.PreviewCells.Count <= 12);
            Assert.Equal(0, pack.AttemptedTileArrayCellCount);
            Assert.DoesNotContain(validator.ValidateMapPack(pack, graph), item => item.Severity == "error");
        }
    }

    [Fact]
    public void RouteSummariesCarryEndpointRegionBindings()
    {
        var graph = WorldScaleRegionMapCatalog.BuildDefaultGraphs().Single(item => item.ScenarioId == "caravan_trade");
        var pack = new FiniteMapPackBuilder().Build(graph);

        foreach (var route in pack.RouteSummaries)
        {
            Assert.Contains(route.FromRegionId, route.RouteRegionBindingIds);
            Assert.Contains(route.ToRegionId, route.RouteRegionBindingIds);
            Assert.NotEmpty(route.RouteCellAnchors);
        }
    }
}
