using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class RuntimeHarvestTests
{
    [Fact]
    public void HarvestAppliesOutputsLootAndConsumesEquippedToolDurability()
    {
        var package = CreatePackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;

        Assert.True(runtime.Execute(package, state, GameRuntimeCommand.EquipItem("item/axe", "slot/tool")).Success);
        var result = runtime.Execute(package, state, GameRuntimeCommand.HarvestResourceNode("node/apple_tree", seed: 7));

        Assert.True(result.Success);
        Assert.Contains(result.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.ResourceHarvested);
        Assert.Contains(PlayerInventory(state).Stacks, stack => stack.ItemId == "item/log");
        Assert.Contains(PlayerInventory(state).Stacks, stack => stack.ItemId == "item/apple");
        Assert.Equal(1, state.Equipment.Single().Slots.Single().Durability);
    }

    [Fact]
    public void HarvestMissingToolDoesNotMutate()
    {
        var package = CreatePackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;
        PlayerInventory(state).Stacks.Clear();
        var before = JsonSerializer.Serialize(state);

        var result = runtime.Execute(package, state, GameRuntimeCommand.HarvestResourceNode("node/apple_tree", seed: 7));

        Assert.False(result.Success);
        Assert.Equal(before, JsonSerializer.Serialize(state));
    }

    private static IGameRuntimeService CreateRuntime()
    {
        var requirementEvaluator = new RequirementEvaluator();
        var costConsumer = new CostConsumer();
        var outputApplier = new OutputApplier();
        var recipeRuntimeService = new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var transactionRuntimeService = new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var harvestRuntimeService = new HarvestRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        return new GameRuntimeService(
            new GameRuntimeStateFactory(),
            recipeRuntimeService,
            new LootRuntimeService(requirementEvaluator, outputApplier),
            transactionRuntimeService,
            new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            new UseItemRuntimeService(requirementEvaluator, outputApplier),
            new InteractionRuntimeService(requirementEvaluator, outputApplier, recipeRuntimeService, transactionRuntimeService, harvestRuntimeService: harvestRuntimeService),
            new EquipmentRuntimeService(requirementEvaluator),
            new ContainerRuntimeService(),
            harvestRuntimeService);
    }

    private static InventoryState PlayerInventory(GameRuntimeState state)
    {
        return state.Inventories.Single(inventory => inventory.OwnerKind == "player");
    }

    private static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/harvest-test", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                Maps = new List<MapDefinition> { new MapDefinition { Id = "map/start", Name = "Start", Width = 1, Height = 1 } },
                EquipmentSlots = new List<EquipmentSlotDefinition>
                {
                    new EquipmentSlotDefinition { Id = "slot/tool", Name = "Tool", AllowedTags = new List<string> { "tool" } }
                },
                Items = new List<ItemDefinition>
                {
                    new ItemDefinition { Id = "item/axe", Name = "Axe", Kind = "tool", Tags = new List<string> { "tool", "axe" } },
                    new ItemDefinition { Id = "item/log", Name = "Log" },
                    new ItemDefinition { Id = "item/apple", Name = "Apple" }
                },
                ResourceNodes = new List<ResourceNodeDefinition>
                {
                    new ResourceNodeDefinition
                    {
                        Id = "node/apple_tree",
                        Name = "Apple Tree",
                        Production = new List<OutputDefinition> { new OutputDefinition { Kind = "item", Id = "item/log", Amount = 1 } },
                        Metadata = new Dictionary<string, string>
                        {
                            ["required_tool_tag"] = "axe",
                            ["tool_slot_id"] = "slot/tool",
                            ["durability_cost"] = "1",
                            ["harvest_loot_table_id"] = "loot/apple_tree"
                        }
                    }
                },
                LootTables = new List<LootTableDefinition>
                {
                    new LootTableDefinition
                    {
                        Id = "loot/apple_tree",
                        Name = "Apple Tree",
                        Entries = new List<LootEntryDefinition>
                        {
                            new LootEntryDefinition { Id = "entry/apple", Weight = 1, Output = new OutputDefinition { Kind = "item", Id = "item/apple", Amount = 1 } }
                        }
                    }
                },
                Inventories = new List<InventoryDefinition>
                {
                    new InventoryDefinition
                    {
                        Id = "inventory/player",
                        OwnerKind = "player",
                        Stacks = new List<ItemStackDefinition> { new ItemStackDefinition { ItemId = "item/axe", Amount = 1, Durability = 2 } }
                    }
                }
            }
        };
    }
}
