using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;
using Xunit;

namespace LLMGameCreator.Tests.Application.WorldScaleRegionMapFoundation;

public sealed class WorldScaleRegionMapValidatorTests
{
    [Fact]
    public void InvalidFakeLeakMatrixRejectsRequiredCasesWithCausalDiagnostics()
    {
        var graphs = WorldScaleRegionMapCatalog.BuildDefaultGraphs();
        var maps = new FiniteMapPackBuilder().BuildMapPacksByFileName(graphs);
        var chunks = new ChunkedWorldConfigPreludeBuilder().Build(graphs, maps);
        var matrix = new WorldScaleRegionMapValidator().BuildInvalidMatrix(graphs, maps, chunks);
        var codes = matrix.Scenarios
            .SelectMany(item => item.Diagnostics)
            .Select(item => item.Code)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed, string.Join(Environment.NewLine, matrix.Scenarios.Select(item => item.ScenarioId + ":" + string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code)))));
        Assert.Equal(17, matrix.ScenarioCount);
        Assert.Contains(matrix.Scenarios, item => item.ScenarioId == "duplicate_region_id" && item.ActualStatus == "rejected");
        Assert.Contains(matrix.Scenarios, item => item.ScenarioId == "forbidden_runtime_ui_unity_gamepackage_provider_llm_rag_lua_generator_library_leakage" && item.ActualStatus == "blocked");
        Assert.Contains("world_scale.region.duplicate", codes);
        Assert.Contains("world_scale.edge.duplicate", codes);
        Assert.Contains("world_scale.edge.endpoint_unknown", codes);
        Assert.Contains("world_scale.start_region.missing", codes);
        Assert.Contains("world_scale.required_target.unreachable", codes);
        Assert.Contains("world_scale.routes.all_blocked", codes);
        Assert.Contains("world_scale.edge.bidirectional_contradiction", codes);
        Assert.Contains("world_scale.edge.travel_cost.invalid", codes);
        Assert.Contains("world_scale.landmark.region_unknown", codes);
        Assert.Contains("world_scale.map.coordinate_invalid", codes);
        Assert.Contains("world_scale.map.size_invalid", codes);
        Assert.Contains("world_scale.route_polyline.region_binding_missing", codes);
        Assert.Contains("world_scale.chunk.coverage_missing", codes);
        Assert.Contains("world_scale.goal037_output.fake", codes);
        Assert.Contains("world_scale.scenario_profile.mismatch", codes);
        Assert.Contains("world_scale.order.nondeterministic", codes);
        Assert.Contains("world_scale.map.tile_dump.forbidden", codes);
        Assert.Contains("world_scale.boundary.runtime.forbidden", codes);
        Assert.Contains("world_scale.boundary.generator_library.forbidden", codes);
    }
}
