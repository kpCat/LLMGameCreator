using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class RuntimeContainerTests
{
    [Fact]
    public void OpenTakeAndDepositPreserveStackMetadata()
    {
        var package = CreatePackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;

        var open = runtime.Execute(package, state, GameRuntimeCommand.OpenContainer("inventory/chest"));
        var take = runtime.Execute(package, state, GameRuntimeCommand.TakeFromContainer("inventory/chest", "item/badge"));
        var deposit = runtime.Execute(package, state, GameRuntimeCommand.DepositToContainer("inventory/chest", "item/badge"));

        Assert.True(open.Success);
        Assert.Contains(open.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.ContainerOpened);
        Assert.True(take.Success);
        Assert.True(deposit.Success);
        Assert.Contains(ContainerInventory(state).Stacks, stack => stack.ItemId == "item/badge" && stack.QuestItem && stack.UniqueInstanceId == "badge-1" && stack.Metadata["origin"] == "chest");
    }

    [Fact]
    public void MissingTakeDoesNotMutate()
    {
        var package = CreatePackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;
        var before = JsonSerializer.Serialize(state);

        var result = runtime.Execute(package, state, GameRuntimeCommand.TakeFromContainer("inventory/chest", "item/missing"));

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
        return new GameRuntimeService(
            new GameRuntimeStateFactory(),
            recipeRuntimeService,
            new LootRuntimeService(requirementEvaluator, outputApplier),
            transactionRuntimeService,
            new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            new UseItemRuntimeService(requirementEvaluator, outputApplier),
            new InteractionRuntimeService(requirementEvaluator, outputApplier, recipeRuntimeService, transactionRuntimeService),
            new EquipmentRuntimeService(requirementEvaluator),
            new ContainerRuntimeService(),
            new HarvestRuntimeService(requirementEvaluator, costConsumer, outputApplier));
    }

    private static InventoryState ContainerInventory(GameRuntimeState state)
    {
        return state.Inventories.Single(inventory => inventory.Id == "inventory/chest");
    }

    private static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/container-test", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                Maps = new List<MapDefinition> { new MapDefinition { Id = "map/start", Name = "Start", Width = 1, Height = 1 } },
                Items = new List<ItemDefinition> { new ItemDefinition { Id = "item/badge", Name = "Badge", QuestItem = true, Unique = true } },
                Inventories = new List<InventoryDefinition>
                {
                    new InventoryDefinition { Id = "inventory/player", OwnerKind = "player" },
                    new InventoryDefinition
                    {
                        Id = "inventory/chest",
                        OwnerKind = "container",
                        Stacks = new List<ItemStackDefinition>
                        {
                            new ItemStackDefinition
                            {
                                ItemId = "item/badge",
                                Amount = 1,
                                QuestItem = true,
                                UniqueInstanceId = "badge-1",
                                Metadata = new Dictionary<string, string> { ["origin"] = "chest" }
                            }
                        }
                    }
                }
            }
        };
    }
}
