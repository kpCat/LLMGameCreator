using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class RuntimeEquipmentTests
{
    [Fact]
    public void EquipReplaceAndUnequipMoveItemsAtomically()
    {
        var package = CreatePackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;

        var axe = runtime.Execute(package, state, GameRuntimeCommand.EquipItem("item/axe", "slot/tool"));
        var hammer = runtime.Execute(package, state, GameRuntimeCommand.EquipItem("item/hammer", "slot/tool"));
        var unequip = runtime.Execute(package, state, GameRuntimeCommand.UnequipItem("slot/tool"));

        Assert.True(axe.Success);
        Assert.True(hammer.Success);
        Assert.True(unequip.Success);
        Assert.Contains(PlayerInventory(state).Stacks, stack => stack.ItemId == "item/axe");
        Assert.Contains(PlayerInventory(state).Stacks, stack => stack.ItemId == "item/hammer");
        Assert.DoesNotContain(state.Equipment.Single().Slots, slot => !string.IsNullOrWhiteSpace(slot.ItemId));
    }

    [Fact]
    public void EquipFailureDoesNotMutate()
    {
        var package = CreatePackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;
        var before = JsonSerializer.Serialize(state);

        var missing = runtime.Execute(package, state, GameRuntimeCommand.EquipItem("item/missing", "slot/tool"));
        var wrongKind = runtime.Execute(package, state, GameRuntimeCommand.EquipItem("item/apple", "slot/tool"));

        Assert.False(missing.Success);
        Assert.False(wrongKind.Success);
        Assert.Equal(before, JsonSerializer.Serialize(state));
    }

    private static IGameRuntimeService CreateRuntime()
    {
        var requirementEvaluator = new RequirementEvaluator();
        var costConsumer = new CostConsumer();
        var outputApplier = new OutputApplier();
        var recipeRuntimeService = new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var transactionRuntimeService = new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var equipmentRuntimeService = new EquipmentRuntimeService(requirementEvaluator);
        var containerRuntimeService = new ContainerRuntimeService();
        var harvestRuntimeService = new HarvestRuntimeService(requirementEvaluator, costConsumer, outputApplier);
        var useItemRuntimeService = new UseItemRuntimeService(requirementEvaluator, outputApplier);
        return new GameRuntimeService(
            new GameRuntimeStateFactory(),
            recipeRuntimeService,
            new LootRuntimeService(requirementEvaluator, outputApplier),
            transactionRuntimeService,
            new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            useItemRuntimeService,
            new InteractionRuntimeService(requirementEvaluator, outputApplier, recipeRuntimeService, transactionRuntimeService, containerRuntimeService, harvestRuntimeService, useItemRuntimeService),
            equipmentRuntimeService,
            containerRuntimeService,
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
            Manifest = new GameManifest { PackageId = "game/equipment-test", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                Maps = new List<MapDefinition> { new MapDefinition { Id = "map/start", Name = "Start", Width = 1, Height = 1 } },
                EquipmentSlots = new List<EquipmentSlotDefinition>
                {
                    new EquipmentSlotDefinition { Id = "slot/tool", Name = "Tool", AllowedTags = new List<string> { "tool" }, AllowedKinds = new List<string> { "tool" } }
                },
                Items = new List<ItemDefinition>
                {
                    new ItemDefinition { Id = "item/axe", Name = "Axe", Kind = "tool", Tags = new List<string> { "tool" } },
                    new ItemDefinition { Id = "item/hammer", Name = "Hammer", Kind = "tool", Tags = new List<string> { "tool" } },
                    new ItemDefinition { Id = "item/apple", Name = "Apple", Kind = "consumable" }
                },
                Inventories = new List<InventoryDefinition>
                {
                    new InventoryDefinition
                    {
                        Id = "inventory/player",
                        OwnerKind = "player",
                        Stacks = new List<ItemStackDefinition>
                        {
                            new ItemStackDefinition { ItemId = "item/axe", Amount = 1 },
                            new ItemStackDefinition { ItemId = "item/hammer", Amount = 1 },
                            new ItemStackDefinition { ItemId = "item/apple", Amount = 1 }
                        }
                    }
                }
            }
        };
    }
}
