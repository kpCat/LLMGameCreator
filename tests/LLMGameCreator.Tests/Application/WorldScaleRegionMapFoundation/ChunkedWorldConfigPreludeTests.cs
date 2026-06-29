using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;
using Xunit;

namespace LLMGameCreator.Tests.Application.WorldScaleRegionMapFoundation;

public sealed class ChunkedWorldConfigPreludeTests
{
    [Fact]
    public void ChunkConfigCoversEveryFiniteMapRegionBinding()
    {
        var graphs = WorldScaleRegionMapCatalog.BuildDefaultGraphs();
        var packs = new FiniteMapPackBuilder().BuildMapPacksByFileName(graphs);
        var prelude = new ChunkedWorldConfigPreludeBuilder().Build(graphs, packs);
        var validator = new WorldScaleRegionMapValidator();

        Assert.Equal(4, prelude.ScenarioCount);
        Assert.DoesNotContain(validator.ValidateChunkConfig(prelude, graphs, packs), item => item.Severity == "error");
        foreach (var scenario in prelude.Scenarios)
        {
            var pack = packs.Values.Single(item => item.ScenarioId == scenario.ScenarioId);
            var covered = scenario.RegionToChunkCoverage.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);

            Assert.True(scenario.ChunkSize > 0);
            Assert.Equal(pack.RegionBindings.Count, scenario.RegionToChunkCoverage.Count);
            Assert.All(pack.RegionBindings, binding => Assert.Contains(binding.RegionId, covered));
            Assert.All(scenario.RegionToChunkCoverage, coverage => Assert.All(coverage.ChunkIds, chunkId => Assert.StartsWith($"chunk/{scenario.ScenarioId}/", chunkId, StringComparison.Ordinal)));
            Assert.Contains("Goal041", string.Join("|", scenario.RuntimeDeltaHandoffNotes));
        }
    }
}
