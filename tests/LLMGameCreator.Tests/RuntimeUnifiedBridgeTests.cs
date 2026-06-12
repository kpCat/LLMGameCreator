using System.Text.Json;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests;

public sealed class RuntimeUnifiedBridgeTests
{
    [Fact]
    public void UnifiedStart_CreatesMapAndGameplayState()
    {
        var package = CreatePackage();
        var bridge = CreateBridge();

        var result = bridge.Start(package);

        Assert.True(result.Success);
        Assert.Equal("map/start", result.Session.MapState.CurrentMapId);
        Assert.Equal("map/start", result.Session.GameplayState.CurrentMapId);
        Assert.Contains(result.MapEvents, runtimeEvent => runtimeEvent.Type == RuntimeEventType.Message);
        Assert.Contains(result.GameplayEvents, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.GameStarted);
    }

    [Fact]
    public void UnifiedMove_RoutesThroughLegacyRuntimeAndKeepsGameplayState()
    {
        var package = CreatePackage();
        var bridge = CreateBridge();
        var session = bridge.Start(package).Session;
        var gameplayBefore = JsonSerializer.Serialize(session.GameplayState);

        var result = bridge.ExecutePlayerCommand(package, session, PlayerCommand.Move(Direction2D.Right));

        Assert.True(result.Success);
        Assert.Equal(2, session.MapState.PlayerPosition.X);
        Assert.Equal(gameplayBefore, JsonSerializer.Serialize(session.GameplayState));
    }

    [Fact]
    public void UnifiedGameplayCommand_RoutesThroughGameplayRuntimeAndKeepsMapState()
    {
        var package = CreatePackage();
        var bridge = CreateBridge();
        var session = bridge.Start(package).Session;
        var mapBefore = JsonSerializer.Serialize(session.MapState);

        var result = bridge.ExecuteGameplayCommand(package, session, GameRuntimeCommand.UseItem("item/healing_potion"));

        Assert.True(result.Success);
        Assert.Equal(mapBefore, JsonSerializer.Serialize(session.MapState));
        Assert.Contains(session.GameplayState.Statuses, status => status.StatusId == "status/healed");
        Assert.DoesNotContain(PlayerInventory(session.GameplayState).Stacks, stack => stack.ItemId == "item/healing_potion");
    }

    [Fact]
    public void UnifiedWait_TicksGameplayRuntime()
    {
        var package = CreatePackage();
        var bridge = CreateBridge();
        var session = bridge.Start(package).Session;

        var result = bridge.ExecuteGameplayCommand(package, session, new GameRuntimeCommand { Type = GameRuntimeCommandType.Wait, Ticks = 2 });

        Assert.True(result.Success);
        Assert.Equal(2, session.GameplayState.Tick);
        Assert.Contains(result.GameplayEvents, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.ResourceChanged);
    }

    [Fact]
    public void UseItem_FailsWithoutItemAndDoesNotMutate()
    {
        var package = CreatePackage();
        var runtime = CreateGameplayRuntime();
        var state = runtime.CreateInitialState(package).State;
        var before = JsonSerializer.Serialize(state);

        var result = runtime.Execute(package, state, GameRuntimeCommand.UseItem("item/missing"));

        Assert.False(result.Success);
        Assert.Equal(before, JsonSerializer.Serialize(state));
    }

    [Fact]
    public void UseItem_AppliesResourceStatusFlagAndNonConsumableIsKept()
    {
        var package = CreatePackage();
        var runtime = CreateGameplayRuntime();
        var state = runtime.CreateInitialState(package).State;

        var result = runtime.Execute(package, state, GameRuntimeCommand.UseItem("item/charm"));

        Assert.True(result.Success);
        Assert.Contains(PlayerInventory(state).Stacks, stack => stack.ItemId == "item/charm");
        Assert.Contains(state.Resources, resource => resource.ResourceId == "resource/mana" && resource.Amount == 8);
        Assert.Contains(state.Statuses, status => status.StatusId == "status/focused");
        Assert.Contains(state.Flags, flag => flag.Id == "flag/charm_used" && flag.Value == "true");
    }

    [Fact]
    public void UseItem_ConditionsFailWithoutMutation()
    {
        var package = CreatePackage();
        var runtime = CreateGameplayRuntime();
        var state = runtime.CreateInitialState(package).State;
        var before = JsonSerializer.Serialize(state);

        var result = runtime.Execute(package, state, GameRuntimeCommand.UseItem("item/locked_scroll"));

        Assert.False(result.Success);
        Assert.Equal(before, JsonSerializer.Serialize(state));
    }

    [Fact]
    public void Interaction_AppliesEffectsAndUnknownFailsCleanly()
    {
        var package = CreatePackage();
        var runtime = CreateGameplayRuntime();
        var state = runtime.CreateInitialState(package).State;

        var result = runtime.Execute(package, state, GameRuntimeCommand.ExecuteInteraction("interaction/sign"));
        var unknown = runtime.Execute(package, state, GameRuntimeCommand.ExecuteInteraction("interaction/missing"));

        Assert.True(result.Success);
        Assert.Contains(state.Flags, flag => flag.Id == "flag/sign_read" && flag.Value == "true");
        Assert.Contains(result.Events, runtimeEvent => runtimeEvent.Type == GameRuntimeEventType.InteractionTriggered);
        Assert.False(unknown.Success);
    }

    [Fact]
    public void InteractCommand_CanTriggerInteractionRouteFromMapComponent()
    {
        var package = CreatePackage();
        var bridge = CreateBridge();
        var session = bridge.Start(package).Session;

        var result = bridge.ExecutePlayerCommand(package, session, PlayerCommand.Interact());

        Assert.True(result.Success);
        Assert.Contains(session.GameplayState.Flags, flag => flag.Id == "flag/sign_read");
    }

    [Fact]
    public void RuntimeStateSerializer_RoundtripsStateAndUnifiedSessionWithoutPackageDefinitions()
    {
        var package = CreatePackage();
        var bridge = CreateBridge();
        var session = bridge.Start(package).Session;
        var serializer = new RuntimeStateSerializer();

        var stateJson = serializer.Serialize(session.GameplayState);
        var stateRoundtrip = serializer.DeserializeGameRuntimeState(stateJson);
        var sessionJson = serializer.Serialize(session);
        var sessionRoundtrip = serializer.DeserializeUnifiedSession(sessionJson);

        Assert.Equal(session.GameplayState.PackageId, stateRoundtrip.PackageId);
        Assert.Equal(session.MapState.CurrentMapId, sessionRoundtrip.MapState.CurrentMapId);
        Assert.DoesNotContain("\"game\"", stateJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tilePrototypes", sessionJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Runtime_DoesNotMutateGamePackageDefinitions()
    {
        var package = CreatePackage();
        var before = JsonSerializer.Serialize(package);
        var bridge = CreateBridge();
        var session = bridge.Start(package).Session;

        bridge.ExecutePlayerCommand(package, session, PlayerCommand.Move(Direction2D.Right));
        bridge.ExecuteGameplayCommand(package, session, GameRuntimeCommand.UseItem("item/healing_potion"));
        bridge.ExecuteGameplayCommand(package, session, GameRuntimeCommand.ExecuteInteraction("interaction/sign"));

        Assert.Equal(before, JsonSerializer.Serialize(package));
    }

    private static IUnifiedGameRuntimeService CreateBridge()
    {
        return new UnifiedGameRuntimeService(new DefaultGameRuntime(), CreateGameplayRuntime());
    }

    private static IGameRuntimeService CreateGameplayRuntime()
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
            new InteractionRuntimeService(requirementEvaluator, outputApplier, recipeRuntimeService, transactionRuntimeService));
    }

    private static InventoryState PlayerInventory(GameRuntimeState state)
    {
        return state.Inventories.Single(inventory => inventory.OwnerKind == "player");
    }

    private static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/unified-test", Title = "Unified Test", StartMapId = "map/start" },
            Game = new GameDefinition
            {
                TilePrototypes = new List<TilePrototypeDefinition>
                {
                    new TilePrototypeDefinition { Id = "tile/floor", Name = "Floor", Walkable = true }
                },
                EntityPrototypes = new List<EntityPrototypeDefinition>
                {
                    new EntityPrototypeDefinition
                    {
                        Id = "prototype/sign",
                        Name = "Sign",
                        Components = new List<ComponentDefinition>
                        {
                            new ComponentDefinition
                            {
                                Type = "interactable",
                                Args = new Dictionary<string, string> { ["interactionId"] = "interaction/sign" }
                            }
                        }
                    }
                },
                Maps = new List<MapDefinition>
                {
                    new MapDefinition
                    {
                        Id = "map/start",
                        Name = "Start",
                        Width = 4,
                        Height = 3,
                        DefaultTileId = "tile/floor",
                        StartPosition = new Position2D(1, 1),
                        Entities = new List<EntityInstanceDefinition>
                        {
                            new EntityInstanceDefinition
                            {
                                Id = "entity/sign",
                                PrototypeId = "prototype/sign",
                                Position = new Position2D(1, 2)
                            }
                        }
                    }
                },
                Items = new List<ItemDefinition>
                {
                    new ItemDefinition
                    {
                        Id = "item/healing_potion",
                        Name = "Healing Potion",
                        Kind = "consumable",
                        UseEffects = new List<EffectDefinition>
                        {
                            new EffectDefinition { Type = "add_status", Args = new Dictionary<string, string> { ["id"] = "status/healed", ["amount"] = "3" } }
                        }
                    },
                    new ItemDefinition
                    {
                        Id = "item/charm",
                        Name = "Charm",
                        Kind = "tool",
                        UseEffects = new List<EffectDefinition>
                        {
                            new EffectDefinition { Type = "change_resource", Args = new Dictionary<string, string> { ["id"] = "resource/mana", ["amount"] = "-2" } },
                            new EffectDefinition { Type = "add_status", Args = new Dictionary<string, string> { ["id"] = "status/focused", ["amount"] = "2" } },
                            new EffectDefinition { Type = "set_flag", Args = new Dictionary<string, string> { ["id"] = "flag/charm_used", ["value"] = "true" } }
                        }
                    },
                    new ItemDefinition
                    {
                        Id = "item/locked_scroll",
                        Name = "Locked Scroll",
                        Kind = "consumable",
                        UseConditions = new List<ConditionDefinition>
                        {
                            new ConditionDefinition { Type = "flag_equals", Args = new Dictionary<string, string> { ["id"] = "flag/can_read_scroll", ["value"] = "true" } }
                        },
                        UseEffects = new List<EffectDefinition>
                        {
                            new EffectDefinition { Type = "set_flag", Args = new Dictionary<string, string> { ["id"] = "flag/scroll_read", ["value"] = "true" } }
                        }
                    }
                },
                Resources = new List<ResourceDefinition>
                {
                    new ResourceDefinition { Id = "resource/mana", Name = "Mana", DefaultValue = 10, MinValue = 0, MaxValue = 10, RegenPerTick = 1 }
                },
                Statuses = new List<StatusDefinition>
                {
                    new StatusDefinition { Id = "status/healed", Name = "Healed" },
                    new StatusDefinition { Id = "status/focused", Name = "Focused" }
                },
                Interactions = new List<InteractionDefinition>
                {
                    new InteractionDefinition
                    {
                        Id = "interaction/sign",
                        Kind = "inspect",
                        Effects = new List<EffectDefinition>
                        {
                            new EffectDefinition { Type = "set_flag", Args = new Dictionary<string, string> { ["id"] = "flag/sign_read", ["value"] = "true" } },
                            new EffectDefinition { Type = "log", Args = new Dictionary<string, string> { ["id"] = "log/sign", ["message"] = "The sign was read." } }
                        }
                    }
                },
                Inventories = new List<InventoryDefinition>
                {
                    new InventoryDefinition
                    {
                        Id = "inventory/player",
                        OwnerKind = "player",
                        Stacks = new List<ItemStackDefinition>
                        {
                            new ItemStackDefinition { ItemId = "item/healing_potion", Amount = 1 },
                            new ItemStackDefinition { ItemId = "item/charm", Amount = 1 },
                            new ItemStackDefinition { ItemId = "item/locked_scroll", Amount = 1 }
                        }
                    }
                }
            }
        };
    }
}
