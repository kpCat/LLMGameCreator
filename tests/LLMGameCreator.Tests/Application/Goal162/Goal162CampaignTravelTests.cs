using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal162;

[Collection(Goal160Collection.Name)]
public sealed class Goal162CampaignTravelTests
{
    [Fact]
    public void Behavioral_travel_route_uses_real_projected_movement_commands()
    {
        var runtime = Goal162TravelState.Value.Runtime;

        Assert.Contains(PlayerCommandType.Move, runtime.PlayerCommands);
        Assert.True(runtime.PlayerCommands.Count(command => command == PlayerCommandType.Move) > 0);
        Assert.DoesNotContain(runtime.Results.SelectMany(result => result.MapEvents),
            runtimeEvent => runtimeEvent.Type == RuntimeEventType.Error);
    }

    [Fact]
    public void Behavioral_generated_gate_interaction_emits_real_map_changed_event()
    {
        var state = Goal162TravelState.Value;

        Assert.Contains(state.Runtime.Results.SelectMany(result => result.MapEvents),
            runtimeEvent => runtimeEvent.Type == RuntimeEventType.MapChanged);
        Assert.Contains(state.AfterTravel.RecentEvents,
            message => message.Contains("другой регион", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_gate_route_changes_human_region_and_map_projection()
    {
        var state = Goal162TravelState.Value;

        Assert.NotEqual(state.Started.CurrentMapTitle, state.AfterTravel.CurrentMapTitle);
        Assert.NotEqual(state.Started.CurrentRegionTitle, state.AfterTravel.CurrentRegionTitle);
        Assert.Equal(state.DestinationMapTitle, state.AfterTravel.CurrentMapTitle);
    }

    [Fact]
    public void Behavioral_destination_generated_object_remains_interactable_after_travel()
    {
        var state = Goal162TravelState.Value;

        Assert.Contains(state.AfterTravel.Map!.Entities,
            entity => entity.Title == state.DestinationInteractionTitle && entity.Interactable);
        Assert.Contains(state.BeforeDestinationInteraction.Actions,
            action => action.Kind == GeneratedCampaignActionKind.Interact
                      && action.TargetTitle == state.DestinationInteractionTitle);
    }

    [Fact]
    public void Behavioral_destination_interaction_executes_real_runtime_route()
    {
        var state = Goal162TravelState.Value;

        Assert.NotEqual(state.BeforeDestinationInteraction.SessionSha256,
            state.AfterDestinationInteraction.SessionSha256);
        Assert.Contains(state.Runtime.Results.SelectMany(result => result.MapEvents),
            runtimeEvent => runtimeEvent.Type == RuntimeEventType.InteractionTriggered);
    }

    [Fact]
    public void Behavioral_travel_and_destination_context_are_human_readable()
    {
        var state = Goal162TravelState.Value;
        var text = Goal162TestKit.PrimaryText(state.AfterTravel)
                   + Goal162TestKit.PrimaryText(state.AfterDestinationInteraction);

        Assert.DoesNotContain("generated/", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("map/", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("entity/", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_repeat_player_route_reaches_equivalent_destination_state()
    {
        var state = Goal162TravelState.Value;

        Assert.Equal(state.AfterDestinationInteraction.SessionSha256,
            state.ReplayAfterDestinationInteraction.SessionSha256);
        Assert.Equal(state.AfterDestinationInteraction.CurrentMapTitle,
            state.ReplayAfterDestinationInteraction.CurrentMapTitle);
    }
}

internal static class Goal162TravelState
{
    private static readonly Lazy<Goal162TravelFixture> Fixture = new(Create);
    public static Goal162TravelFixture Value => Fixture.Value;

    private static Goal162TravelFixture Create()
    {
        var package = Goal162TestKit.Package;
        var runtime = new Goal162CountingRuntime(Goal162TestKit.Bundle.Saves.Runtime);
        var service = Goal162TestKit.Service(runtime: runtime);
        var started = service.StartNew();
        var destination = package.Game.Maps.First(map => map.Name != started.CurrentMapTitle
            && package.GeneratedContent.Scenes.Any(scene => scene.PackageMapId == map.Id));
        var afterTravel = Goal162TestKit.TravelTo(service, destination.Name);
        var target = Assert.IsType<GeneratedCampaignMapProjection>(afterTravel.Map).Entities.First(entity =>
            entity.Interactable && !entity.Title.StartsWith("Переход в ", StringComparison.Ordinal));
        var beforeInteraction = Goal162TestKit.MoveAdjacentTo(service, target.Title);
        var action = Assert.Single(beforeInteraction.Actions,
            action => action.Kind == GeneratedCampaignActionKind.Interact
                      && action.TargetTitle == target.Title);
        var afterInteraction = service.Execute(action.ActionId);

        var replayService = Goal162TestKit.Service();
        replayService.StartNew();
        Goal162TestKit.TravelTo(replayService, destination.Name);
        var replayBefore = Goal162TestKit.MoveAdjacentTo(replayService, target.Title);
        var replayAction = Assert.Single(replayBefore.Actions,
            item => item.Kind == GeneratedCampaignActionKind.Interact && item.TargetTitle == target.Title);
        var replayAfter = replayService.Execute(replayAction.ActionId);
        return new Goal162TravelFixture(runtime, started, afterTravel, beforeInteraction, afterInteraction,
            replayAfter, destination.Name, target.Title);
    }
}

internal sealed record Goal162TravelFixture(
    Goal162CountingRuntime Runtime,
    GeneratedCampaignSnapshot Started,
    GeneratedCampaignSnapshot AfterTravel,
    GeneratedCampaignSnapshot BeforeDestinationInteraction,
    GeneratedCampaignSnapshot AfterDestinationInteraction,
    GeneratedCampaignSnapshot ReplayAfterDestinationInteraction,
    string DestinationMapTitle,
    string DestinationInteractionTitle);
