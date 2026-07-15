using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal158;

public sealed class Goal158RuntimeMapTransitionTests
{
    [Fact]
    public void Behavioral_valid_transition_changes_map_and_coordinates()
    {
        var (package, state) = Fixture();

        var result = new DefaultGameRuntime().Execute(package, state, PlayerCommand.Interact());

        Assert.True(result.Success);
        Assert.Equal("map/destination", result.State.CurrentMapId);
        Assert.Equal(2, result.State.PlayerPosition.X);
        Assert.Equal(2, result.State.PlayerPosition.Y);
        Assert.Collection(result.Events,
            interaction => Assert.Equal(RuntimeEventType.InteractionTriggered, interaction.Type),
            changed => Assert.Equal(RuntimeEventType.MapChanged, changed.Type));
    }

    [Fact]
    public void Behavioral_map_changed_event_has_exact_data_derived_args()
    {
        var (package, state) = Fixture();

        var changed = Assert.Single(new DefaultGameRuntime()
            .Execute(package, state, PlayerCommand.Interact()).Events,
            item => item.Type == RuntimeEventType.MapChanged);

        Assert.Equal("map/destination", changed.TargetId);
        Assert.Equal(new Dictionary<string, string>
        {
            [MapTransitionInteractionContract.ConnectionIdKey] = "connection/origin__destination",
            [MapTransitionInteractionContract.SourceMapIdKey] = "map/origin",
            [MapTransitionInteractionContract.DestinationMapIdKey] = "map/destination",
            [MapTransitionInteractionContract.FromRegionIdKey] = "region/origin",
            [MapTransitionInteractionContract.ToRegionIdKey] = "region/destination",
            [MapTransitionInteractionContract.DestinationXKey] = "2",
            [MapTransitionInteractionContract.DestinationYKey] = "2"
        }, changed.Args);
    }

    [Fact]
    public void Behavioral_wrong_source_map_fails_atomically()
    {
        var (package, state) = Fixture(args =>
            args[MapTransitionInteractionContract.SourceMapIdKey] = "map/other");

        AssertAtomicFailure(package, state, "map_transition.source_map_mismatch");
    }

    [Fact]
    public void Behavioral_missing_destination_map_fails_atomically()
    {
        var (package, state) = Fixture(args =>
            args[MapTransitionInteractionContract.DestinationMapIdKey] = "map/missing");

        AssertAtomicFailure(package, state, "map_transition.destination_map_missing");
    }

    [Fact]
    public void Behavioral_malformed_coordinates_fail_atomically()
    {
        var (package, state) = Fixture(args =>
            args[MapTransitionInteractionContract.DestinationXKey] = "два");

        AssertAtomicFailure(package, state, "map_transition.destination_position_invalid");
    }

    [Fact]
    public void Behavioral_blocked_destination_fails_atomically()
    {
        var (package, state) = Fixture();
        package.Game.Maps.Single(map => map.Id == "map/destination").Tiles.Add(
            new TileOverrideDefinition { X = 2, Y = 2, TileId = "tile/wall" });

        AssertAtomicFailure(package, state, "map_transition.destination_tile_blocked");
    }

    [Fact]
    public void Behavioral_incomplete_transition_contract_fails_atomically()
    {
        var (package, state) = Fixture(args =>
            args.Remove(MapTransitionInteractionContract.ToRegionIdKey));

        AssertAtomicFailure(package, state, "map_transition.contract_incomplete");
    }

    [Fact]
    public void Behavioral_legacy_text_interaction_is_unchanged()
    {
        var (package, state) = Fixture(transition: false);
        var component = package.Game.Maps[0].Entities[0].Components[0];
        component.Args["text"] = "Старое текстовое взаимодействие.";

        var result = new DefaultGameRuntime().Execute(package, state, PlayerCommand.Interact());

        Assert.True(result.Success);
        Assert.Equal("map/origin", result.State.CurrentMapId);
        Assert.Equal([RuntimeEventType.InteractionTriggered, RuntimeEventType.Message],
            result.Events.Select(item => item.Type));
        Assert.Equal("Старое текстовое взаимодействие.", result.Events[1].Message);
    }

    [Fact]
    public void Behavioral_legacy_dialogue_interaction_is_unchanged()
    {
        var (package, state) = Fixture(transition: false);
        package.Game.Maps[0].Entities[0].Components[0].Args["dialogueId"] = "dialogue/legacy";

        var result = new DefaultGameRuntime().Execute(package, state, PlayerCommand.Interact());

        Assert.True(result.Success);
        Assert.Equal([RuntimeEventType.InteractionTriggered, RuntimeEventType.DialogueRequested],
            result.Events.Select(item => item.Type));
        Assert.Equal("dialogue/legacy", result.Events[1].TargetId);
    }

    [Fact]
    public void Behavioral_no_nearby_interaction_is_unchanged()
    {
        var (package, state) = Fixture();
        package.Game.Maps[0].Entities.Clear();

        var result = new DefaultGameRuntime().Execute(package, state, PlayerCommand.Interact());

        Assert.True(result.Success);
        Assert.Single(result.Events);
        Assert.Equal(RuntimeEventType.Message, result.Events[0].Type);
        Assert.Equal("Рядом нет объекта для взаимодействия.", result.Events[0].Message);
    }

    [Fact]
    public void Contract_runtime_event_values_remain_stable_and_map_changed_is_additive()
    {
        Assert.Equal(0, (int)RuntimeEventType.Message);
        Assert.Equal(1, (int)RuntimeEventType.PlayerMoved);
        Assert.Equal(2, (int)RuntimeEventType.MovementBlocked);
        Assert.Equal(3, (int)RuntimeEventType.InteractionTriggered);
        Assert.Equal(4, (int)RuntimeEventType.DialogueRequested);
        Assert.Equal(5, (int)RuntimeEventType.SoundRequested);
        Assert.Equal(6, (int)RuntimeEventType.MusicRequested);
        Assert.Equal(7, (int)RuntimeEventType.Error);
        Assert.Equal(8, (int)RuntimeEventType.MapChanged);
    }

    private static void AssertAtomicFailure(
        GamePackageDefinition package,
        GameState state,
        string expectedCode)
    {
        var originalFlags = state.Flags.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
        var result = new DefaultGameRuntime().Execute(package, state, PlayerCommand.Interact());

        Assert.False(result.Success);
        Assert.Equal("map/origin", result.State.CurrentMapId);
        Assert.Equal(1, result.State.PlayerPosition.X);
        Assert.Equal(1, result.State.PlayerPosition.Y);
        Assert.Equal(originalFlags, result.State.Flags);
        var error = Assert.Single(result.Events);
        Assert.Equal(RuntimeEventType.Error, error.Type);
        Assert.Equal(expectedCode, error.Args["code"]);
        Assert.DoesNotContain(result.Events, item => item.Type == RuntimeEventType.InteractionTriggered);
        Assert.DoesNotContain(result.Events, item => item.Type == RuntimeEventType.MapChanged);
    }

    private static (GamePackageDefinition Package, GameState State) Fixture(
        Action<Dictionary<string, string>>? edit = null,
        bool transition = true)
    {
        var args = new Dictionary<string, string>(StringComparer.Ordinal);
        if (transition)
        {
            args[MapTransitionInteractionContract.TransitionKindKey] =
                MapTransitionInteractionContract.TransitionKindMap;
            args[MapTransitionInteractionContract.ConnectionIdKey] = "connection/origin__destination";
            args[MapTransitionInteractionContract.SourceMapIdKey] = "map/origin";
            args[MapTransitionInteractionContract.DestinationMapIdKey] = "map/destination";
            args[MapTransitionInteractionContract.DestinationXKey] = "2";
            args[MapTransitionInteractionContract.DestinationYKey] = "2";
            args[MapTransitionInteractionContract.FromRegionIdKey] = "region/origin";
            args[MapTransitionInteractionContract.ToRegionIdKey] = "region/destination";
        }
        edit?.Invoke(args);
        var package = new GamePackageDefinition
        {
            Manifest = new GameManifest
            {
                PackageId = "game.goal158.runtime",
                Title = "Goal158 Runtime",
                Version = "1.0.0",
                FormatVersion = "0.1",
                StartMapId = "map/origin"
            },
            Game = new GameDefinition
            {
                TilePrototypes =
                [
                    new TilePrototypeDefinition { Id = "tile/floor", Name = "Floor", Walkable = true },
                    new TilePrototypeDefinition { Id = "tile/wall", Name = "Wall", Walkable = false }
                ],
                Maps =
                [
                    new MapDefinition
                    {
                        Id = "map/origin", Name = "Origin", Width = 4, Height = 4,
                        DefaultTileId = "tile/floor", StartPosition = new Position2D(1, 1),
                        Entities =
                        [
                            new EntityInstanceDefinition
                            {
                                Id = "entity/gate", Position = new Position2D(2, 1),
                                Components = [new ComponentDefinition { Type = "interactable", Args = args }]
                            }
                        ]
                    },
                    new MapDefinition
                    {
                        Id = "map/destination", Name = "Destination", Width = 4, Height = 4,
                        DefaultTileId = "tile/floor", StartPosition = new Position2D(2, 2)
                    }
                ]
            }
        };
        var state = new GameState
        {
            CurrentMapId = "map/origin",
            PlayerPosition = new Position2D(1, 1),
            Flags = new Dictionary<string, string> { ["keep"] = "same" }
        };
        return (package, state);
    }
}
