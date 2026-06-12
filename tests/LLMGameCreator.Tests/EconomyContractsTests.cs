using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class EconomyContractsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    [Fact]
    public void OlderMinimalPackageDeserializesWithDefaultEmptyEconomyLists()
    {
        var package = JsonSerializer.Deserialize<GamePackageDefinition>("""
        {
          "manifest": { "packageId": "game/old", "title": "Old", "version": "0.1.0", "formatVersion": "0.1", "startMapId": "map/start" },
          "game": {
            "tilePrototypes": [],
            "entityPrototypes": [],
            "maps": [],
            "items": []
          },
          "assetCatalog": { "contracts": [], "assets": [], "generationRequests": [] },
          "scriptCatalog": { "scripts": [], "generators": [] }
        }
        """, JsonOptions);

        Assert.NotNull(package);
        Assert.Empty(package!.Game.Resources);
        Assert.Empty(package.Game.Recipes);
        Assert.Empty(package.Game.LootTables);
        Assert.Empty(package.Game.Transactions);
        Assert.Empty(package.Game.ResourceNetworks);
        Assert.Empty(package.Game.ResourceNodes);
        Assert.Empty(package.Game.Inventories);
    }

    [Fact]
    public void EconomyFieldsRoundTripThroughJson()
    {
        var package = new GamePackageDefinition
        {
            Game = new GameDefinition
            {
                Items = new List<ItemDefinition>
                {
                    new ItemDefinition
                    {
                        Id = "item/healing_potion",
                        Name = "Healing Potion",
                        Kind = "consumable",
                        Rarity = "common",
                        MaxStack = 5,
                        Value = 25,
                        MaxDurability = 10,
                        MaxCharge = 3
                    }
                },
                Resources = new List<ResourceDefinition>
                {
                    new ResourceDefinition
                    {
                        Id = "resource/mana",
                        Name = "Mana",
                        Kind = "magic",
                        MinValue = 0,
                        MaxValue = 100,
                        RegenPerTick = 1
                    }
                },
                Recipes = new List<RecipeDefinition>
                {
                    new RecipeDefinition
                    {
                        Id = "recipe/healing_potion",
                        Name = "Healing Potion",
                        Inputs = new List<CostDefinition> { new CostDefinition { Kind = "item", Id = "item/healing_potion", Amount = 1 } },
                        Outputs = new List<OutputDefinition> { new OutputDefinition { Kind = "resource", Id = "resource/mana", Amount = 5 } }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(package, JsonOptions);
        var roundTrip = JsonSerializer.Deserialize<GamePackageDefinition>(json, JsonOptions);

        Assert.NotNull(roundTrip);
        Assert.Equal("common", roundTrip!.Game.Items[0].Rarity);
        Assert.Equal(100, roundTrip.Game.Resources[0].MaxValue);
        Assert.Equal("recipe/healing_potion", roundTrip.Game.Recipes[0].Id);
    }
}
