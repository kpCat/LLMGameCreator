using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal161;

[Collection(Goal160Collection.Name)]
public sealed class Goal161MigrationRuntimeContinuationTests
{
    [Fact]
    public void Behavioral_DefaultGameRuntime_loaded_migrated_session_executes_real_movement()
    {
        var state = Goal161MigrationState.Value;
        var moves = state.Route.Actions.Select((action, index) => (action, index))
            .Where(item => item.action.Kind == GeneratedWorldTravelPlannedActionKind.Move).ToList();
        Assert.NotEmpty(moves);
        Assert.All(moves, item => Assert.True(state.FirstRouteResults[item.index].Success));
        Assert.Contains(state.FirstRouteResults.SelectMany(item => item.MapEvents),
            item => item.Type == RuntimeEventType.PlayerMoved);
    }

    [Fact]
    public void Behavioral_loaded_migrated_session_crosses_generated_travel_gate()
    {
        var state = Goal161MigrationState.Value;
        var gates = state.Route.Actions.Select((action, index) => (action, index))
            .Where(item => item.action.Kind == GeneratedWorldTravelPlannedActionKind.GateInteraction).ToList();
        Assert.NotEmpty(gates);
        Assert.All(gates, item => Assert.Contains(state.FirstRouteResults[item.index].MapEvents,
            runtimeEvent => runtimeEvent.Type == RuntimeEventType.MapChanged));
        Assert.Equal(state.Route.DestinationMapId, state.FirstRouteResults[^1].Session.MapState.CurrentMapId);
    }

    [Fact]
    public void Behavioral_destination_generated_interaction_succeeds_after_load()
    {
        var state = Goal161MigrationState.Value;
        var destination = state.Route.Actions.Select((action, index) => (action, index)).Single(item =>
            item.action.Kind == GeneratedWorldTravelPlannedActionKind.DestinationInteraction);
        Assert.Contains(state.FirstRouteResults[destination.index].MapEvents,
            item => item.Type == RuntimeEventType.InteractionTriggered
                    && item.TargetId == state.Route.DestinationEntityId);
    }

    [Fact]
    public void Behavioral_repeat_load_and_command_sequence_is_state_equivalent()
    {
        var state = Goal161MigrationState.Value;
        Assert.Equal(state.FirstRouteSessionJson, state.ReplayRouteSessionJson);
    }

    [Fact]
    public void Behavioral_repeat_route_event_sequence_is_equivalent()
    {
        var state = Goal161MigrationState.Value;
        var first = state.FirstRouteResults.SelectMany(item => item.MapEvents)
            .Select(item => item.Type + ":" + item.TargetId + ":" + item.Message).ToList();
        var replay = state.ReplayRouteResults.SelectMany(item => item.MapEvents)
            .Select(item => item.Type + ":" + item.TargetId + ":" + item.Message).ToList();
        Assert.Equal(first, replay);
    }

    [Fact]
    public void Behavioral_route_visits_distinct_origin_and_destination_maps()
    {
        var route = Goal161MigrationState.Value.Route;
        Assert.NotEqual(route.OriginMapId, route.DestinationMapId);
        Assert.True(route.VisitedMapIds.Distinct(StringComparer.Ordinal).Count() >= 2);
        Assert.True(route.VisitedRegionIds.Distinct(StringComparer.Ordinal).Count() >= 2);
    }

    [Fact]
    public void Behavioral_original_historical_world_is_restored_after_migration()
    {
        var state = Goal161MigrationState.Value;
        Assert.Equal(state.Saved.Revision?.WorldId,
            state.Bundle.Controller.ReadGeneratedWorldHistory().CurrentWorldId);
        Assert.Equal("CAMPAIGN_CURRENT", state.Rollback.AuthoritativeSnapshot?.GeneratedWorld?.Status);
    }

    [Fact]
    public void Behavioral_slot_manifest_retains_both_historical_revisions_after_rollback()
    {
        var state = Goal161MigrationState.Value;
        var slot = state.Bundle.Saves.Store.ReadSlot(state.Project.Path, "campaign");
        Assert.Equal(2, slot.Manifest?.RevisionSha256s.Count);
        Assert.Contains(state.Saved.RevisionSha256, slot.Manifest?.RevisionSha256s ?? []);
        Assert.Contains(state.Migrated.MigratedRevisionSha256, slot.Manifest?.RevisionSha256s ?? []);
    }
}
