using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal156;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal158;

[Collection(Goal156Collection.Name)]
public sealed class Goal158TravelRouteRuntimeTests
{
    [Fact]
    public void Behavioral_destination_selection_is_repeatable_and_reachable()
    {
        var (source, overlay) = Goal158TestKit.Overlay();
        var planner = new GeneratedWorldTravelRoutePlanner();

        var first = planner.Plan(source, overlay.PlayerCompositionPackage);
        var second = planner.Plan(source, overlay.PlayerCompositionPackage);

        Assert.True(first.Passed, string.Join(Environment.NewLine, first.Diagnostics));
        Assert.Equal(first.DestinationRegionId, second.DestinationRegionId);
        Assert.Equal(first.ConnectionIds, second.ConnectionIds);
        Assert.NotEqual(first.OriginRegionId, first.DestinationRegionId);
    }

    [Fact]
    public void Behavioral_route_uses_exact_plan_connection_ids()
    {
        var (source, overlay) = Goal158TestKit.Overlay();

        var route = new GeneratedWorldTravelRoutePlanner().Plan(source, overlay.PlayerCompositionPackage);

        var planIds = source.RegeneratedPlan!.World.Connections.Select(item => item.ConnectionId).ToHashSet();
        Assert.All(route.ConnectionIds, connectionId => Assert.Contains(connectionId, planIds));
        Assert.All(route.Actions.Where(item => item.Kind == GeneratedWorldTravelPlannedActionKind.GateInteraction),
            action => Assert.Contains(action.ConnectionId, planIds));
    }

    [Fact]
    public void Behavioral_origin_generated_interaction_is_runtime_observed()
    {
        var result = Goal158TestKit.Activate();

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.True(result.Summary.OriginInteractionObserved);
        Assert.Equal("generated_origin_interaction",
            result.Summary.RuntimeFrames.First(item => item.Category == "generated_origin_interaction").Category);
    }

    [Fact]
    public void Behavioral_grid_path_is_shortest_tie_order_deterministic()
    {
        var (source, overlay) = Goal158TestKit.Overlay();
        var planner = new GeneratedWorldTravelRoutePlanner();

        var first = planner.Plan(source, overlay.PlayerCompositionPackage);
        var second = planner.Plan(source, overlay.PlayerCompositionPackage);
        var firstMoves = first.Actions.Where(item => item.Kind == GeneratedWorldTravelPlannedActionKind.Move)
            .Select(item => (item.Command.Direction, item.ExpectedMapId, item.ExpectedX, item.ExpectedY)).ToList();
        var secondMoves = second.Actions.Where(item => item.Kind == GeneratedWorldTravelPlannedActionKind.Move)
            .Select(item => (item.Command.Direction, item.ExpectedMapId, item.ExpectedX, item.ExpectedY)).ToList();

        Assert.Equal(first.MovementCommandCount, firstMoves.Count);
        Assert.Equal(firstMoves, secondMoves);
        Assert.DoesNotContain(firstMoves, item => item.Direction == Direction2D.None);
    }

    [Fact]
    public void Behavioral_gate_interaction_emits_correlated_map_changed()
    {
        var result = Goal158TestKit.Activate();

        Assert.True(result.Summary.TravelGateInteractionsPassed,
            string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Equal(result.RoutePlan.ConnectionIds.Count, result.Summary.TransitionCount);
        Assert.Contains(result.Summary.RuntimeFrames, item => item.Category == "generated_travel");
    }

    [Fact]
    public void Behavioral_actual_state_enters_planned_destination_map()
    {
        var (source, overlay) = Goal158TestKit.Overlay();
        var route = new GeneratedWorldTravelRoutePlanner().Plan(source, overlay.PlayerCompositionPackage);

        var state = Execute(overlay.PlayerCompositionPackage, route);

        Assert.Equal(route.DestinationMapId, state.CurrentMapId);
    }

    [Fact]
    public void Behavioral_destination_generated_interaction_is_runtime_observed()
    {
        var result = Goal158TestKit.Activate();

        Assert.True(result.Summary.DestinationInteractionObserved,
            string.Join(Environment.NewLine, result.Diagnostics));
        Assert.Contains(result.Summary.RuntimeFrames,
            item => item.Category == "generated_destination_interaction");
    }

    [Fact]
    public void Behavioral_visited_maps_and_regions_contain_two_distinct_values()
    {
        var result = Goal158TestKit.Activate();

        Assert.True(result.Summary.VisitedMapIds.Distinct(StringComparer.Ordinal).Count() >= 2);
        Assert.True(result.Summary.VisitedRegionIds.Distinct(StringComparer.Ordinal).Count() >= 2);
        Assert.Equal(result.Summary.VisitedMapIds.Count, result.Summary.VisitedRegionIds.Count);
    }

    [Fact]
    public void Behavioral_multi_hop_route_works_when_intermediate_region_has_no_generated_interaction()
    {
        var (source, baseOverlay) = Goal158TestKit.Overlay();
        var package = Goal158TestKit.Clone(baseOverlay.TravelOverlayPackage);
        package.Game.EntityPrototypes.RemoveAll(item =>
            item.Id == GeneratedWorldTravelOverlayService.TravelPrototypeId);
        foreach (var map in package.Game.Maps)
            map.Entities.RemoveAll(item => item.Id.StartsWith(
                GeneratedWorldTravelOverlayService.TravelEntityIdPrefix, StringComparison.Ordinal));
        var binding = new GeneratedWorldRegionMapBindingService().Bind(source, package);
        var origin = binding.RegionBindings.Single(item => item.MapId == source.Source!.GeneratedStartMapId);
        var others = binding.RegionBindings.Where(item => item.RegionId != origin.RegionId)
            .OrderBy(item => item.RegionId, StringComparer.Ordinal).Take(2).ToList();
        var intermediateIds = source.GeneratedMvpPackage!.Game.Maps.Single(item => item.Id == others[0].MapId)
            .Entities.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        package.Game.Maps.Single(item => item.Id == others[0].MapId).Entities
            .RemoveAll(item => intermediateIds.Contains(item.Id));
        var connections = new List<ProceduralRegionConnection>
        {
            new() { ConnectionId = "connection/test-hop-a", FromRegionId = origin.RegionId, ToRegionId = others[0].RegionId },
            new() { ConnectionId = "connection/test-hop-b", FromRegionId = others[0].RegionId, ToRegionId = others[1].RegionId }
        };
        var mutatedSource = source with
        {
            RegeneratedPlan = source.RegeneratedPlan! with
            {
                World = source.RegeneratedPlan.World with { Connections = connections }
            }
        };
        var overlay = new GeneratedWorldTravelOverlayService().Build(mutatedSource, package);

        var route = new GeneratedWorldTravelRoutePlanner().Plan(mutatedSource, overlay.PlayerCompositionPackage);

        Assert.True(route.Passed, string.Join(Environment.NewLine, route.Diagnostics));
        Assert.Equal(2, route.ConnectionIds.Count);
        Assert.Equal(3, route.VisitedRegionIds.Count);
    }

    [Fact]
    public void Behavioral_unreachable_gate_rejects_route()
    {
        var (source, original) = Goal158TestKit.Overlay();
        var package = Goal158TestKit.Clone(original.PlayerCompositionPackage);
        var connection = original.Binding.ConnectionBindings.First();
        var map = package.Game.Maps.Single(item => item.Id == connection.SourceMapId);
        var gate = map.Entities.Single(item =>
            item.Id == GeneratedWorldTravelOverlayService.GateEntityId(connection.ConnectionId));
        var candidate = Enumerable.Range(0, map.Width * map.Height)
            .Select(index => new Position2D(index % map.Width, index / map.Width))
            .Where(position => Math.Abs(position.X - map.StartPosition.X)
                               + Math.Abs(position.Y - map.StartPosition.Y) > 1)
            .OrderByDescending(position => position.X + position.Y).First();
        gate.Position = candidate;
        foreach (var (x, y) in new[] { (0, -1), (-1, 0), (1, 0), (0, 1) })
        {
            var px = candidate.X + x;
            var py = candidate.Y + y;
            if (px < 0 || py < 0 || px >= map.Width || py >= map.Height) continue;
            map.Entities.Add(new EntityInstanceDefinition
            {
                Id = $"entity/test-blocker/{px}/{py}",
                Position = new Position2D(px, py),
                Components = [new ComponentDefinition { Type = "collidable" }]
            });
        }

        var route = new GeneratedWorldTravelRoutePlanner().Plan(source, package);

        Assert.False(route.Passed);
        Assert.Contains(route.Diagnostics, item => item.StartsWith(
            "generated_travel.gate_unreachable:", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_missing_destination_interactable_rejects_route()
    {
        var (source, original) = Goal158TestKit.Overlay();
        var package = Goal158TestKit.Clone(original.PlayerCompositionPackage);
        foreach (var map in package.Game.Maps.Where(item => item.Id != source.Source!.GeneratedStartMapId))
        {
            var generated = source.GeneratedMvpPackage!.Game.Maps.SingleOrDefault(item => item.Id == map.Id)
                ?.Entities.Select(item => item.Id).ToHashSet(StringComparer.Ordinal) ?? [];
            map.Entities.RemoveAll(item => generated.Contains(item.Id));
        }

        var route = new GeneratedWorldTravelRoutePlanner().Plan(source, package);

        Assert.False(route.Passed);
        Assert.Contains("generated_travel.destination_interactable_missing", route.Diagnostics);
    }

    [Fact]
    public void Behavioral_wrong_runtime_map_changed_args_fail_correlation()
    {
        var result = Goal158TestKit.Activate(new Goal158FaultRuntime(corruptMapChanged: true));

        Assert.False(result.Passed);
        Assert.Contains(result.Diagnostics, item => item.StartsWith(
            "generated_travel.map_change_correlation_failed:", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_suppressed_map_changed_fails_route()
    {
        var result = Goal158TestKit.Activate(new Goal158FaultRuntime(suppressMapChanged: true));

        Assert.False(result.Passed);
        Assert.False(result.Summary.TravelGateInteractionsPassed);
    }

    [Fact]
    public void Behavioral_full_route_replay_divergence_is_rejected()
    {
        var result = Goal158TestKit.Activate(new Goal158FaultRuntime(divergeReplay: true));

        Assert.False(result.Passed);
        Assert.False(result.Summary.ReplayEquivalent);
        Assert.Contains("generated_travel.replay_mismatch", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_full_route_state_roundtrip_divergence_is_rejected()
    {
        var result = Goal158TestKit.Activate(serializer: new Goal158CorruptingStateSerializer());

        Assert.False(result.Passed);
        Assert.False(result.Summary.StateRoundtripPassed);
        Assert.Contains("generated_travel.state_roundtrip_mismatch", result.Diagnostics);
    }

    private static GameState Execute(GamePackageDefinition package, GeneratedWorldTravelRoutePlan route)
    {
        var runtime = new DefaultGameRuntime();
        var state = runtime.Start(package).State;
        foreach (var action in route.Actions)
        {
            var result = runtime.Execute(package, state, action.Command);
            Assert.True(result.Success);
            state = result.State;
        }
        return state;
    }
}

internal sealed class Goal158FaultRuntime : IGameRuntime
{
    private readonly DefaultGameRuntime _inner = new();
    private readonly bool _suppressMapChanged;
    private readonly bool _corruptMapChanged;
    private readonly bool _divergeReplay;
    private int _run;

    public Goal158FaultRuntime(
        bool suppressMapChanged = false,
        bool corruptMapChanged = false,
        bool divergeReplay = false)
    {
        _suppressMapChanged = suppressMapChanged;
        _corruptMapChanged = corruptMapChanged;
        _divergeReplay = divergeReplay;
    }

    public CommandResult Start(GamePackageDefinition package)
    {
        _run++;
        return _inner.Start(package);
    }

    public CommandResult Execute(GamePackageDefinition package, GameState state, PlayerCommand command)
    {
        var result = _inner.Execute(package, state, command);
        foreach (var changed in result.Events.Where(item => item.Type == RuntimeEventType.MapChanged))
        {
            if (_corruptMapChanged)
                changed.Args[MapTransitionInteractionContract.ConnectionIdKey] = "connection/corrupt";
        }
        if (_suppressMapChanged)
            result.Events.RemoveAll(item => item.Type == RuntimeEventType.MapChanged);
        if (_divergeReplay && _run >= 2 && command.Type == PlayerCommandType.Interact)
            result.State.Flags["goal158.replay.divergence"] = "true";
        return result;
    }
}

internal sealed class Goal158CorruptingStateSerializer : IRuntimeStateSerializer
{
    private readonly RuntimeStateSerializer _inner = new();

    public string Serialize(GameRuntimeState state) => _inner.Serialize(state);
    public GameRuntimeState DeserializeGameRuntimeState(string json) => _inner.DeserializeGameRuntimeState(json);
    public string Serialize(UnifiedRuntimeSession session) => _inner.Serialize(session);

    public UnifiedRuntimeSession DeserializeUnifiedSession(string json)
    {
        var session = _inner.DeserializeUnifiedSession(json);
        session.MapState.CurrentMapId = "generated/map/corrupt-roundtrip";
        return session;
    }
}
