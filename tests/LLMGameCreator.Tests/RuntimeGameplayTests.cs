using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class RuntimeGameplayTests
{
    [Fact]
    public void InitialState_LoadsInventoriesResourcesAndDefaultPlayerInventory()
    {
        var package = CreateGameplayPackage();
        package.Game.Inventories.Clear();

        var result = CreateRuntime().CreateInitialState(package);

        Assert.True(result.Success);
        Assert.Contains(result.State.Inventories, inventory => inventory.OwnerKind == "player");
        Assert.Contains(result.State.Resources, resource => resource.ResourceId == "resource/mana" && resource.Amount == 10 && resource.Capacity == 100);
    }

    [Fact]
    public void InitialState_OlderMinimalPackageCanInitialize()
    {
        var package = new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/minimal", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                Maps = new List<MapDefinition> { new MapDefinition { Id = "map/start", Name = "Start", Width = 1, Height = 1 } }
            }
        };

        var result = CreateRuntime().CreateInitialState(package);

        Assert.True(result.Success);
        Assert.Equal("map/start", result.State.CurrentMapId);
        Assert.Contains(result.State.Inventories, inventory => inventory.OwnerKind == "player");
    }

    [Fact]
    public void RequirementEvaluator_PassesFailsAndUnknownKindFails()
    {
        var package = CreateGameplayPackage();
        var state = CreateRuntime().CreateInitialState(package).State;
        state.Flags.Add(new RuntimeFlagState { Id = "flag/tutorial", Value = "done" });
        var evaluator = new RequirementEvaluator();

        var pass = evaluator.Evaluate(package, state, new[]
        {
            new RequirementDefinition { Kind = "has_item", Id = "item/red_herb", Amount = 2 },
            new RequirementDefinition { Kind = "resource_at_least", Id = "resource/mana", Amount = 10 },
            new RequirementDefinition { Kind = "flag_equals", Id = "flag/tutorial", Value = "done" }
        });
        var fail = evaluator.Evaluate(package, state, new[]
        {
            new RequirementDefinition { Kind = "has_item", Id = "item/red_herb", Amount = 99 },
            new RequirementDefinition { Kind = "unknown_gate", Id = "x" }
        });

        Assert.True(pass.Success);
        Assert.False(fail.Success);
        Assert.Contains(fail.Failures, failure => failure.Code == "requirement.kind.unknown");
    }

    [Fact]
    public void CostsAndOutputs_AreAppliedAndFailedCostsDoNotMutate()
    {
        var package = CreateGameplayPackage();
        var state = CreateRuntime().CreateInitialState(package).State;
        var consumer = new CostConsumer();
        var applier = new OutputApplier();

        var failed = consumer.Consume(package, state, new[] { new CostDefinition { Kind = "item", Id = "item/red_herb", Amount = 99 } });
        var herbsAfterFailedCost = PlayerInventory(state).Stacks.Single(s => s.ItemId == "item/red_herb").Amount;
        var consumed = consumer.Consume(package, state, new[]
        {
            new CostDefinition { Kind = "item", Id = "item/red_herb", Amount = 1 },
            new CostDefinition { Kind = "resource", Id = "resource/mana", Amount = 5 }
        });
        var output = applier.Apply(package, state, new[]
        {
            new OutputDefinition { Kind = "item", Id = "item/healing_potion", Amount = 1 },
            new OutputDefinition { Kind = "resource", Id = "resource/gold", Amount = 3 },
            new OutputDefinition { Kind = "flag", Id = "flag/crafted", Amount = 1 },
            new OutputDefinition { Kind = "status", Id = "status/blessed", Amount = 2 }
        });

        Assert.False(failed.Success);
        Assert.Equal(2, herbsAfterFailedCost);
        Assert.True(consumed.Success);
        Assert.True(output.Success);
        Assert.Equal(1, PlayerInventory(state).Stacks.Single(s => s.ItemId == "item/red_herb").Amount);
        Assert.Contains(PlayerInventory(state).Stacks, stack => stack.ItemId == "item/healing_potion");
        Assert.Contains(state.Resources, resource => resource.ResourceId == "resource/gold" && resource.Amount == 3);
        Assert.Contains(state.Flags, flag => flag.Id == "flag/crafted");
        Assert.Contains(state.Statuses, status => status.StatusId == "status/blessed");
    }

    [Fact]
    public void CraftRecipe_ConsumesInputsCostsAndAddsOutputsOrFailsWithoutMutation()
    {
        var package = CreateGameplayPackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;

        var crafted = runtime.Execute(package, state, GameRuntimeCommand.CraftRecipe("recipe/healing_potion"));
        var failed = runtime.Execute(package, state, GameRuntimeCommand.CraftRecipe("recipe/healing_potion"));

        Assert.True(crafted.Success);
        Assert.Contains(crafted.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.RecipeCrafted);
        Assert.Contains(PlayerInventory(state).Stacks, stack => stack.ItemId == "item/healing_potion" && stack.Amount == 1);
        Assert.False(failed.Success);
        Assert.Equal(1, PlayerInventory(state).Stacks.Single(stack => stack.ItemId == "item/healing_potion").Amount);
    }

    [Fact]
    public void LootRoll_IsDeterministicAndRespectsUniqueGlobalCount()
    {
        var package = CreateGameplayPackage();
        var firstState = CreateRuntime().CreateInitialState(package).State;
        var secondState = CreateRuntime().CreateInitialState(package).State;
        var runtime = CreateRuntime();

        var first = runtime.Execute(package, firstState, GameRuntimeCommand.RollLootTable("loot/reward", seed: 10));
        var second = runtime.Execute(package, secondState, GameRuntimeCommand.RollLootTable("loot/reward", seed: 10));

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.Equal(SerializeInventory(firstState), SerializeInventory(secondState));

        var uniqueAgain = runtime.Execute(package, firstState, GameRuntimeCommand.RollLootTable("loot/unique_badge", seed: 1));
        var uniqueThird = runtime.Execute(package, firstState, GameRuntimeCommand.RollLootTable("loot/unique_badge", seed: 1));

        Assert.True(uniqueAgain.Success);
        Assert.False(uniqueThird.Success);
    }

    [Fact]
    public void Transaction_ConsumesCostsAndAppliesOutputsAtomically()
    {
        var package = CreateGameplayPackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;
        runtime.Execute(package, state, new GameRuntimeCommand { Type = GameRuntimeCommandType.ChangeResource, Id = "resource/gold", Amount = 25 });

        var result = runtime.Execute(package, state, GameRuntimeCommand.ExecuteTransaction("transaction/buy_potion"));
        var failed = runtime.Execute(package, state, GameRuntimeCommand.ExecuteTransaction("transaction/buy_potion"));

        Assert.True(result.Success);
        Assert.Contains(result.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.TransactionExecuted);
        Assert.Equal(0, state.Resources.Single(resource => resource.ResourceId == "resource/gold").Amount);
        Assert.False(failed.Success);
        Assert.Equal(0, state.Resources.Single(resource => resource.ResourceId == "resource/gold").Amount);
    }

    [Fact]
    public void ResourceNodeTick_ProducesConvertsClampsAndIncrementsTick()
    {
        var package = CreateGameplayPackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;

        var result = runtime.Execute(package, state, GameRuntimeCommand.TickResourceNodes());

        Assert.True(result.Success);
        Assert.Equal(1, state.Tick);
        Assert.Equal(20, state.Resources.Single(resource => resource.ResourceId == "resource/electricity").Amount);
        Assert.Contains(result.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.ResourceNodeTicked);

        var failed = runtime.Execute(package, state, GameRuntimeCommand.TickResourceNodes());

        Assert.True(failed.Success);
        Assert.Contains(failed.Diagnostics, diagnostic => diagnostic.Severity == "warning");
        Assert.Equal(2, state.Tick);
    }

    [Fact]
    public void Dispatcher_RoutesCommandsAndUnknownCommandFailsCleanly()
    {
        var package = CreateGameplayPackage();
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;

        var crafted = runtime.Execute(package, state, GameRuntimeCommand.CraftRecipe("recipe/healing_potion"));
        var unknown = runtime.Execute(package, state, new GameRuntimeCommand { Type = (GameRuntimeCommandType)999 });

        Assert.True(crafted.Success);
        Assert.False(unknown.Success);
        Assert.Contains(unknown.Diagnostics, diagnostic => diagnostic.Code == "runtime.command.unknown");
    }

    [Fact]
    public void Runtime_DoesNotMutateGamePackageDefinitions()
    {
        var package = CreateGameplayPackage();
        var before = JsonSerializer.Serialize(package);
        var runtime = CreateRuntime();
        var state = runtime.CreateInitialState(package).State;

        runtime.Execute(package, state, GameRuntimeCommand.CraftRecipe("recipe/healing_potion"));
        runtime.Execute(package, state, GameRuntimeCommand.RollLootTable("loot/reward", seed: 5));
        runtime.Execute(package, state, GameRuntimeCommand.TickResourceNodes());

        Assert.Equal(before, JsonSerializer.Serialize(package));
    }

    private static IGameRuntimeService CreateRuntime()
    {
        var requirementEvaluator = new RequirementEvaluator();
        var costConsumer = new CostConsumer();
        var outputApplier = new OutputApplier();
        return new GameRuntimeService(
            new GameRuntimeStateFactory(),
            new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            new LootRuntimeService(requirementEvaluator, outputApplier),
            new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            new ResourceNetworkRuntimeService(requirementEvaluator, costConsumer, outputApplier),
            new UseItemRuntimeService(requirementEvaluator, outputApplier),
            new InteractionRuntimeService(
                requirementEvaluator,
                outputApplier,
                new RecipeRuntimeService(requirementEvaluator, costConsumer, outputApplier),
                new TransactionRuntimeService(requirementEvaluator, costConsumer, outputApplier)));
    }

    private static InventoryState PlayerInventory(GameRuntimeState state)
    {
        return state.Inventories.Single(inventory => inventory.OwnerKind == "player");
    }

    private static string SerializeInventory(GameRuntimeState state)
    {
        return JsonSerializer.Serialize(PlayerInventory(state).Stacks.OrderBy(stack => stack.ItemId).Select(stack => new { stack.ItemId, stack.Amount }));
    }

    private static GamePackageDefinition CreateGameplayPackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/runtime-test", Title = "Runtime Test", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                Maps = new List<MapDefinition> { new MapDefinition { Id = "map/start", Name = "Start", Width = 1, Height = 1 } },
                Items = new List<ItemDefinition>
                {
                    new ItemDefinition { Id = "item/red_herb", Name = "Red Herb" },
                    new ItemDefinition { Id = "item/water_flask", Name = "Water Flask" },
                    new ItemDefinition { Id = "item/healing_potion", Name = "Healing Potion" },
                    new ItemDefinition { Id = "item/fuel_can", Name = "Fuel Can" },
                    new ItemDefinition { Id = "item/badge", Name = "Badge", QuestItem = true, Unique = true }
                },
                Resources = new List<ResourceDefinition>
                {
                    new ResourceDefinition { Id = "resource/mana", Name = "Mana", DefaultValue = 10, MinValue = 0, MaxValue = 100 },
                    new ResourceDefinition { Id = "resource/gold", Name = "Gold", MinValue = 0 },
                    new ResourceDefinition { Id = "resource/electricity", Name = "Electricity", MinValue = 0, MaxValue = 30 }
                },
                Statuses = new List<StatusDefinition> { new StatusDefinition { Id = "status/blessed", Name = "Blessed" } },
                Recipes = new List<RecipeDefinition>
                {
                    new RecipeDefinition
                    {
                        Id = "recipe/healing_potion",
                        Name = "Healing Potion",
                        Inputs = new List<CostDefinition>
                        {
                            new CostDefinition { Kind = "item", Id = "item/red_herb", Amount = 2 },
                            new CostDefinition { Kind = "item", Id = "item/water_flask", Amount = 1 }
                        },
                        Costs = new List<CostDefinition> { new CostDefinition { Kind = "resource", Id = "resource/mana", Amount = 5 } },
                        Outputs = new List<OutputDefinition> { new OutputDefinition { Kind = "item", Id = "item/healing_potion", Amount = 1 } }
                    }
                },
                LootTables = new List<LootTableDefinition>
                {
                    new LootTableDefinition
                    {
                        Id = "loot/reward",
                        Name = "Reward",
                        Entries = new List<LootEntryDefinition>
                        {
                            new LootEntryDefinition { Id = "entry/gold", Weight = 1, Output = new OutputDefinition { Kind = "resource", Id = "resource/gold", Amount = 3 } },
                            new LootEntryDefinition { Id = "entry/potion", Weight = 1, Output = new OutputDefinition { Kind = "item", Id = "item/healing_potion", Amount = 1 } }
                        }
                    },
                    new LootTableDefinition
                    {
                        Id = "loot/unique_badge",
                        Name = "Unique Badge",
                        Entries = new List<LootEntryDefinition>
                        {
                            new LootEntryDefinition
                            {
                                Id = "entry/badge",
                                Weight = 1,
                                Unique = true,
                                QuestItem = true,
                                MaxGlobalCount = 1,
                                Output = new OutputDefinition { Kind = "item", Id = "item/badge", Amount = 1 }
                            }
                        }
                    }
                },
                Transactions = new List<TransactionDefinition>
                {
                    new TransactionDefinition
                    {
                        Id = "transaction/buy_potion",
                        Name = "Buy Potion",
                        Costs = new List<CostDefinition> { new CostDefinition { Kind = "resource", Id = "resource/gold", Amount = 25 } },
                        Outputs = new List<OutputDefinition> { new OutputDefinition { Kind = "item", Id = "item/healing_potion", Amount = 1 } }
                    }
                },
                ResourceNodes = new List<ResourceNodeDefinition>
                {
                    new ResourceNodeDefinition
                    {
                        Id = "node/generator",
                        Name = "Generator",
                        ConversionInputs = new List<CostDefinition> { new CostDefinition { Kind = "item", Id = "item/fuel_can", Amount = 1 } },
                        ConversionOutputs = new List<OutputDefinition> { new OutputDefinition { Kind = "resource", Id = "resource/electricity", Amount = 20 } }
                    }
                },
                Inventories = new List<InventoryDefinition>
                {
                    new InventoryDefinition
                    {
                        Id = "inventory/player_start",
                        OwnerKind = "player",
                        Stacks = new List<ItemStackDefinition>
                        {
                            new ItemStackDefinition { ItemId = "item/red_herb", Amount = 2 },
                            new ItemStackDefinition { ItemId = "item/water_flask", Amount = 1 },
                            new ItemStackDefinition { ItemId = "item/fuel_can", Amount = 1 }
                        }
                    }
                }
            }
        };
    }
}
