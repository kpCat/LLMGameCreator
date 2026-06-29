namespace LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

public sealed class ChunkedWorldConfigPreludeBuilder
{
    public WorldScaleChunkedWorldConfigPrelude Build(
        IReadOnlyList<WorldScaleRegionGraph> graphs,
        IReadOnlyDictionary<string, WorldScaleFiniteMapPack> mapPacksByFileName)
    {
        var packsByScenario = mapPacksByFileName.Values.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);
        var scenarios = graphs
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(graph => BuildScenario(graph, packsByScenario[graph.ScenarioId]))
            .ToList();

        return new WorldScaleChunkedWorldConfigPrelude
        {
            ScenarioCount = scenarios.Count,
            Scenarios = scenarios
        };
    }

    private static WorldScaleScenarioChunkConfig BuildScenario(WorldScaleRegionGraph graph, WorldScaleFiniteMapPack mapPack)
    {
        var chunkSize = graph.ScenarioId == "metamodule_kingdoms" ? 24 : 16;
        var coverages = mapPack.RegionBindings
            .OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .Select(binding =>
            {
                var primary = ChunkId(graph.ScenarioId, binding.RegionId, "primary");
                var border = ChunkId(graph.ScenarioId, binding.RegionId, "border");
                return new WorldScaleChunkRegionCoverage
                {
                    RegionId = binding.RegionId,
                    ChunkIds = [primary, border]
                };
            })
            .ToList();
        var chunkIds = coverages
            .SelectMany(item => item.ChunkIds)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new WorldScaleScenarioChunkConfig
        {
            ScenarioId = graph.ScenarioId,
            WorldGraphId = graph.WorldGraphId,
            FiniteMapId = mapPack.MapId,
            ChunkSize = chunkSize,
            ChunkIdFormat = $"chunk/{graph.ScenarioId}/<region-stable-suffix>/<primary|border>",
            ScenarioWorldSeed = graph.DeterministicSeed,
            RegionToChunkCoverage = coverages,
            FiniteMapProjection = new WorldScaleFiniteMapChunkProjection
            {
                MapId = mapPack.MapId,
                CoordinateKind = mapPack.CoordinateKind,
                CoveredChunkIds = chunkIds
            },
            FutureGenerationRuleRefs =
            [
                $"future-rule/{graph.ScenarioId}/biome-neighbor-expansion",
                $"future-rule/{graph.ScenarioId}/route-corridor-fill",
                $"future-rule/{graph.ScenarioId}/landmark-encounter-hook-placement"
            ],
            ForbiddenMutationNotes =
            [
                "package definitions are seed/config/source content, not runtime chunk state",
                "runtime discovery and mutation deltas must not be written into finite map packs",
                "Goal038 does not integrate Runtime, Unity, UI, GamePackage schema, providers, LLM/RAG, Lua source or generator-library paths"
            ],
            RuntimeDeltaHandoffNotes =
            [
                "Goal041 should persist discovered and mutated chunk state through runtime/save state",
                "Goal041 should validate deltas against these deterministic chunk ids",
                "Goal041 should keep package/source config immutable after promotion"
            ]
        };
    }

    private static string ChunkId(string scenarioId, string regionId, string role)
    {
        var suffix = regionId.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "unknown";
        return $"chunk/{scenarioId}/{suffix}/{role}";
    }
}
