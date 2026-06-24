using LLMGameCreator.Application.Design.Semantics;
using LLMGameCreator.Application.Generation.Procedural;
using Xunit;

namespace LLMGameCreator.Tests.Application.Procedural;

public sealed class ProceduralGameKernelServiceTests
{
    [Fact]
    public void SameSeedAndProfileProduceByteIdenticalJsonAndMarkdown()
    {
        var service = new ProceduralGameKernelService();
        var request = CreateRequest("route-17");

        var first = service.Generate(request);
        var second = service.Generate(request);

        Assert.Equal(first.Json, second.Json);
        Assert.Equal(first.Markdown, second.Markdown);
        Assert.Equal(first.Plan.Metadata.DeterministicHash, second.Plan.Metadata.DeterministicHash);
        Assert.Equal("route-17", first.Plan.Metadata.Seed);
        Assert.Equal(3, first.Plan.Factions.Count);
        Assert.True(first.Plan.ActorSeeds.Count >= 2);
        Assert.True(first.Plan.ItemResourceSeeds.Count >= 2);
        Assert.True(first.Plan.EncounterSeeds.Count >= 1);
        Assert.True(first.Plan.QuestEventSeeds.Count >= 1);
    }

    [Fact]
    public void DifferentSeedChangesGeneratedValuesButPreservesStructure()
    {
        var service = new ProceduralGameKernelService();

        var first = service.Generate(CreateRequest("route-17"));
        var second = service.Generate(CreateRequest("route-18"));

        Assert.NotEqual(first.Json, second.Json);
        Assert.NotEqual(first.Plan.Metadata.DeterministicHash, second.Plan.Metadata.DeterministicHash);
        Assert.Equal(first.Plan.World.Regions.Count, second.Plan.World.Regions.Count);
        Assert.Equal(first.Plan.Factions.Count, second.Plan.Factions.Count);
        Assert.Equal(first.Plan.ActorSeeds.Count, second.Plan.ActorSeeds.Count);
        Assert.Equal(first.Plan.ItemResourceSeeds.Count, second.Plan.ItemResourceSeeds.Count);
        Assert.Equal(first.Plan.EncounterSeeds.Count, second.Plan.EncounterSeeds.Count);
        Assert.Equal(first.Plan.QuestEventSeeds.Count, second.Plan.QuestEventSeeds.Count);
    }

    [Fact]
    public void InvalidSeedModeAndUnsafeIdsReturnDiagnosticsInsteadOfThrowing()
    {
        var service = new ProceduralGameKernelService();

        var result = service.Generate(new ProceduralGameKernelRequest
        {
            Seed = " ",
            Mode = "not a supported mode",
            CompactStyleHintIds = ["theme:unsafe", "Tone/Mysterious"],
            SelectedVariantIds = ["../bad", "Combat Model/Turn Based", "world_topology/region_graph"]
        });

        Assert.Equal("default_seed", result.Plan.Metadata.Seed);
        Assert.Equal(ProceduralGameGenerationModes.AuthoredSmallWorld, result.Plan.Metadata.Mode);
        Assert.Contains(result.Diagnostics, item => item.Code == "procedural_kernel.invalid_seed");
        Assert.Contains(result.Diagnostics, item => item.Code == "procedural_kernel.invalid_mode");
        Assert.Contains(result.Diagnostics, item => item.Code == "procedural_kernel.invalid_style_hint_id");
        Assert.Contains(result.Diagnostics, item => item.Code == "procedural_kernel.invalid_variant_id");
        Assert.Contains("tone/mysterious", result.Plan.Profile.StyleHintIds);
        Assert.Contains("combat_model/turn_based", result.Plan.Profile.VariantIds);
        Assert.DoesNotContain(result.Plan.Profile.VariantIds, id => id.Contains("..", StringComparison.Ordinal));
    }

    [Fact]
    public void GeneratedReferencesAndMarkdownSummaryAreDeterministicAndRuntimeFacing()
    {
        var service = new ProceduralGameKernelService();

        var result = service.Generate(CreateRequest("runtime-facing-01"));
        var regionIds = result.Plan.World.Regions.Select(item => item.RegionId).ToHashSet(StringComparer.Ordinal);
        var factionIds = result.Plan.Factions.Select(item => item.FactionId).ToHashSet(StringComparer.Ordinal);
        var actorIds = result.Plan.ActorSeeds.Select(item => item.ActorSeedId).ToHashSet(StringComparer.Ordinal);
        var itemIds = result.Plan.ItemResourceSeeds.Select(item => item.ItemSeedId).ToHashSet(StringComparer.Ordinal);
        var encounterIds = result.Plan.EncounterSeeds.Select(item => item.EncounterSeedId).ToHashSet(StringComparer.Ordinal);
        var placeholderIds = result.Plan.FormulaEffectActionPlaceholders.Select(item => item.PlaceholderId).ToHashSet(StringComparer.Ordinal);

        Assert.All(result.Plan.World.Connections, item =>
        {
            Assert.Contains(item.FromRegionId, regionIds);
            Assert.Contains(item.ToRegionId, regionIds);
            Assert.Contains(item.GateRequirementPlaceholderId, placeholderIds);
        });
        Assert.All(result.Plan.Factions, item => Assert.Contains(item.HomeRegionId, regionIds));
        Assert.All(result.Plan.ActorSeeds, item =>
        {
            Assert.Contains(item.RegionId, regionIds);
            Assert.Contains(item.FactionId, factionIds);
        });
        Assert.All(result.Plan.ItemResourceSeeds, item => Assert.Contains(item.RegionId, regionIds));
        Assert.All(result.Plan.EncounterSeeds, item =>
        {
            Assert.Contains(item.RegionId, regionIds);
            Assert.All(item.FactionIds, id => Assert.Contains(id, factionIds));
            Assert.All(item.ActorSeedIds, id => Assert.Contains(id, actorIds));
            Assert.All(item.RewardItemSeedIds, id => Assert.Contains(id, itemIds));
            Assert.Contains(item.ActionPlaceholderId, placeholderIds);
        });
        Assert.All(result.Plan.QuestEventSeeds, item =>
        {
            Assert.Contains(item.RegionId, regionIds);
            Assert.Contains(item.SourceFactionId, factionIds);
            Assert.Contains(item.TargetEncounterSeedId, encounterIds);
            Assert.Contains(item.RequiredItemSeedId, itemIds);
            Assert.Contains(item.RewardPlaceholderId, placeholderIds);
        });
        Assert.Contains("- Regions: `", result.Markdown, StringComparison.Ordinal);
        Assert.Contains("- Factions: `3`", result.Markdown, StringComparison.Ordinal);
        Assert.Contains("Formula/effect/action placeholders", result.Markdown, StringComparison.Ordinal);
        Assert.Contains("does not call an LLM, provider, Lua, Unity, media generator, or runtime execution", result.Markdown, StringComparison.Ordinal);
    }

    private static ProceduralGameKernelRequest CreateRequest(string seed) => new()
    {
        Seed = seed,
        Mode = ProceduralGameGenerationModes.SemiProceduralRegions,
        CompactStyleHintIds =
        [
            "theme/survival",
            "tone/mysterious",
            "quest_motif/faction_truce",
            "item_affordance/tradable"
        ],
        SelectedVariantIds =
        [
            "world_topology/region_graph",
            "actor_model/single_player_character",
            "combat_model/turn_based",
            "inventory_model/list_inventory"
        ],
        SemanticCatalog = new SemanticCatalog
        {
            Terms =
            [
                new SemanticCatalogTerm
                {
                    TermId = "location_mood/ruined",
                    Kind = SemanticTermKinds.LocationMood,
                    Status = SemanticTermStatuses.Known,
                    Label = "Ruined"
                },
                new SemanticCatalogTerm
                {
                    TermId = "npc_archetype/cartographer",
                    Kind = SemanticTermKinds.NpcArchetype,
                    Status = SemanticTermStatuses.Candidate,
                    Label = "Cartographer"
                }
            ]
        }
    };
}
