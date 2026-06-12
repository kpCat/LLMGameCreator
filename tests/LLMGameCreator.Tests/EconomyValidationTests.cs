using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class EconomyValidationTests
{
    [Fact]
    public void ValidEconomyDefinitionsPassWithDataOnlyProgressionWarning()
    {
        var package = CreateEconomyPackage();
        package.Game.Transactions[0].Outputs.Add(new OutputDefinition { Kind = "progression", Id = "progression/fire_magic", Amount = 1 });

        var report = new GamePackageValidator().Validate(package);

        Assert.DoesNotContain(report.Issues, issue => issue.Severity == Domain.Validation.ValidationSeverity.Error);
        Assert.Contains(report.Issues, issue => issue.Code == "economy.runtime.not_implemented");
    }

    [Fact]
    public void DuplicateIdsAndMissingRecipeReferencesProduceErrors()
    {
        var package = CreateEconomyPackage();
        package.Game.Resources.Add(new ResourceDefinition { Id = "resource/mana", Name = "Mana Copy" });
        package.Game.Recipes.Add(new RecipeDefinition
        {
            Id = "recipe/healing_potion",
            Name = "Broken Potion",
            Inputs = new List<CostDefinition> { new CostDefinition { Kind = "item", Id = "item/missing", Amount = 1 } },
            Outputs = new List<OutputDefinition> { new OutputDefinition { Kind = "resource", Id = "resource/missing", Amount = 1 } }
        });

        var report = new GamePackageValidator().Validate(package);

        Assert.Contains(report.Issues, issue => issue.Code == "resource.id.duplicate");
        Assert.Contains(report.Issues, issue => issue.Code == "recipe.id.duplicate");
        Assert.Contains(report.Issues, issue => issue.Code == "recipe.input.item_missing");
        Assert.Contains(report.Issues, issue => issue.Code == "recipe.output.resource_missing");
    }

    [Fact]
    public void LootTransactionNetworkNodeAndInventoryReferencesAreValidated()
    {
        var package = CreateEconomyPackage();
        package.Game.LootTables[0].Entries.Add(new LootEntryDefinition
        {
            Id = "entry/missing",
            Output = new OutputDefinition { Kind = "item", Id = "item/missing", Amount = 1 }
        });
        package.Game.Transactions[0].Costs.Add(new CostDefinition { Kind = "resource", Id = "resource/missing", Amount = 1 });
        package.Game.ResourceNetworks[0].ResourceId = "resource/missing";
        package.Game.ResourceNodes[0].NetworkId = "network/missing";
        package.Game.ResourceNodes[0].Production.Add(new OutputDefinition { Kind = "resource", Id = "resource/missing", Amount = 1 });
        package.Game.Inventories[0].Stacks.Add(new ItemStackDefinition { ItemId = "item/missing", Amount = 1 });

        var report = new GamePackageValidator().Validate(package);

        Assert.Contains(report.Issues, issue => issue.Code == "loot.output.item_missing");
        Assert.Contains(report.Issues, issue => issue.Code == "transaction.cost.unknown_reference");
        Assert.Contains(report.Issues, issue => issue.Code == "resource_network.resource_missing");
        Assert.Contains(report.Issues, issue => issue.Code == "resource_node.network_missing");
        Assert.Contains(report.Issues, issue => issue.Code == "resource_node.production.resource_missing");
        Assert.Contains(report.Issues, issue => issue.Code == "inventory.item_missing");
    }

    [Fact]
    public void UniqueQuestLootWithSingleGlobalCountIsValid()
    {
        var package = CreateEconomyPackage();
        package.Game.Items.Add(new ItemDefinition { Id = "item/guard_badge", Name = "Guard Badge", QuestItem = true, Unique = true });
        package.Game.LootTables[0].Entries.Add(new LootEntryDefinition
        {
            Id = "entry/guard_badge",
            Unique = true,
            QuestItem = true,
            MaxGlobalCount = 1,
            Output = new OutputDefinition { Kind = "item", Id = "item/guard_badge", Amount = 1 }
        });

        var report = new GamePackageValidator().Validate(package);

        Assert.DoesNotContain(report.Issues, issue => issue.Code == "unique_loot.invalid_count");
        Assert.DoesNotContain(report.Issues, issue => issue.Code == "loot.output.item_missing");
    }

    private static GamePackageDefinition CreateEconomyPackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/test", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                TilePrototypes = new List<TilePrototypeDefinition> { new TilePrototypeDefinition { Id = "tile/grass", Name = "Grass" } },
                Maps = new List<MapDefinition> { new MapDefinition { Id = "map/start", Name = "Start", Width = 1, Height = 1, DefaultTileId = "tile/grass" } },
                Items = new List<ItemDefinition>
                {
                    new ItemDefinition { Id = "item/red_herb", Name = "Red Herb" },
                    new ItemDefinition { Id = "item/healing_potion", Name = "Healing Potion" },
                    new ItemDefinition { Id = "item/fuel_can", Name = "Fuel Can" }
                },
                Resources = new List<ResourceDefinition>
                {
                    new ResourceDefinition { Id = "resource/mana", Name = "Mana", Kind = "magic", MinValue = 0, MaxValue = 100 },
                    new ResourceDefinition { Id = "resource/gold", Name = "Gold", Kind = "currency", MinValue = 0 },
                    new ResourceDefinition { Id = "resource/electricity", Name = "Electricity", Kind = "network_resource", MinValue = 0 }
                },
                Recipes = new List<RecipeDefinition>
                {
                    new RecipeDefinition
                    {
                        Id = "recipe/healing_potion",
                        Name = "Healing Potion",
                        Inputs = new List<CostDefinition> { new CostDefinition { Kind = "item", Id = "item/red_herb", Amount = 2 } },
                        Costs = new List<CostDefinition> { new CostDefinition { Kind = "resource", Id = "resource/mana", Amount = 5 } },
                        Outputs = new List<OutputDefinition> { new OutputDefinition { Kind = "item", Id = "item/healing_potion", Amount = 1 } }
                    }
                },
                LootTables = new List<LootTableDefinition>
                {
                    new LootTableDefinition
                    {
                        Id = "loot/goblin_common",
                        Name = "Goblin Common Loot",
                        Entries = new List<LootEntryDefinition>
                        {
                            new LootEntryDefinition
                            {
                                Id = "entry/gold",
                                Output = new OutputDefinition { Kind = "resource", Id = "resource/gold", Amount = 3 }
                            }
                        }
                    }
                },
                Transactions = new List<TransactionDefinition>
                {
                    new TransactionDefinition
                    {
                        Id = "transaction/buy_healing_potion",
                        Name = "Buy Healing Potion",
                        Costs = new List<CostDefinition> { new CostDefinition { Kind = "resource", Id = "resource/gold", Amount = 25 } },
                        Outputs = new List<OutputDefinition> { new OutputDefinition { Kind = "item", Id = "item/healing_potion", Amount = 1 } }
                    }
                },
                ResourceNetworks = new List<ResourceNetworkDefinition>
                {
                    new ResourceNetworkDefinition { Id = "network/base_power", Name = "Base Power Grid", ResourceId = "resource/electricity" }
                },
                ResourceNodes = new List<ResourceNodeDefinition>
                {
                    new ResourceNodeDefinition
                    {
                        Id = "node/diesel_generator",
                        Name = "Diesel Generator",
                        NetworkId = "network/base_power",
                        Production = new List<OutputDefinition> { new OutputDefinition { Kind = "resource", Id = "resource/electricity", Amount = 20 } },
                        ConversionInputs = new List<CostDefinition> { new CostDefinition { Kind = "item", Id = "item/fuel_can", Amount = 1 } }
                    }
                },
                Inventories = new List<InventoryDefinition>
                {
                    new InventoryDefinition
                    {
                        Id = "inventory/player_start",
                        OwnerKind = "player",
                        Stacks = new List<ItemStackDefinition> { new ItemStackDefinition { ItemId = "item/red_herb", Amount = 2 } }
                    }
                }
            }
        };
    }
}
