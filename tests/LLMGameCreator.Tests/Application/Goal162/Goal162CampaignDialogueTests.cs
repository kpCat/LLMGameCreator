using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal162;

[Collection(Goal160Collection.Name)]
public sealed class Goal162CampaignDialogueTests
{
    [Fact]
    public void Behavioral_generated_actor_interaction_opens_real_dialogue()
    {
        var state = Goal162DialogueState.Value;

        Assert.Null(state.Started.Dialogue);
        Assert.True(state.Opened.Dialogue?.Open);
        Assert.NotEqual(state.Started.SessionSha256, state.Opened.SessionSha256);
    }

    [Fact]
    public void Behavioral_dialogue_projection_contains_human_speaker_and_text()
    {
        var dialogue = Assert.IsType<GeneratedCampaignDialogue>(Goal162DialogueState.Value.Opened.Dialogue);

        Assert.False(string.IsNullOrWhiteSpace(dialogue.Title));
        Assert.False(string.IsNullOrWhiteSpace(dialogue.Speaker));
        Assert.False(string.IsNullOrWhiteSpace(dialogue.Text));
        Assert.DoesNotContain("generated/", dialogue.Title + dialogue.Speaker + dialogue.Text,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_open_dialogue_projects_real_choice_actions()
    {
        var state = Goal162DialogueState.Value;
        var choiceActions = state.Opened.Actions
            .Where(action => action.Kind == GeneratedCampaignActionKind.ChooseDialogue).ToList();

        Assert.NotEmpty(choiceActions);
        Assert.Equal(state.Opened.Dialogue?.Choices.Count, choiceActions.Count);
        Assert.Contains(choiceActions, action => action.Enabled);
        Assert.All(choiceActions.Where(action => !action.Enabled), action =>
            Assert.False(string.IsNullOrWhiteSpace(action.DisabledReason)));
    }

    [Fact]
    public void Behavioral_selecting_dialogue_choice_executes_existing_runtime_route()
    {
        var state = Goal162DialogueState.Value;

        Assert.NotEqual(state.Opened.SessionSha256, state.AfterChoice.SessionSha256);
        Assert.Contains(state.AfterChoice.RecentEvents,
            message => message.Contains("Ответ выбран", StringComparison.OrdinalIgnoreCase)
                       || message.Contains("Разговор завершён", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_generated_branch_choice_closes_dialogue_and_routes_runtime_state()
    {
        var after = Goal162DialogueState.Value.AfterChoice;

        Assert.False(after.Dialogue?.Open == true);
        Assert.Contains(after.Actions, action => action.Kind is GeneratedCampaignActionKind.MoveUp
            or GeneratedCampaignActionKind.MoveDown
            or GeneratedCampaignActionKind.MoveLeft
            or GeneratedCampaignActionKind.MoveRight
            or GeneratedCampaignActionKind.RunEncounterAi
            or GeneratedCampaignActionKind.BasicAttack
            or GeneratedCampaignActionKind.UseAbility);
    }

    [Fact]
    public void Behavioral_explicit_close_action_closes_dialogue_without_direct_state_mutation()
    {
        var state = Goal162DialogueState.Value;

        Assert.Contains(state.ExplicitOpened.Actions,
            action => action.Kind == GeneratedCampaignActionKind.CloseDialogue);
        Assert.False(state.ExplicitClosed.Dialogue?.Open == true);
        Assert.Contains(state.ExplicitClosed.RecentEvents,
            message => message.Contains("Разговор завершён", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_dialogue_primary_surface_never_exposes_choice_or_dialogue_ids()
    {
        var state = Goal162DialogueState.Value;
        var primary = Goal162TestKit.PrimaryText(state.Opened) + Goal162TestKit.PrimaryText(state.AfterChoice);

        Assert.DoesNotContain("dialogue/", primary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("choice/", primary, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("generated/", primary, StringComparison.OrdinalIgnoreCase);
    }
}

internal static class Goal162DialogueState
{
    private static readonly Lazy<Goal162DialogueFixture> Fixture = new(Create);
    public static Goal162DialogueFixture Value => Fixture.Value;

    private static Goal162DialogueFixture Create()
    {
        var service = Goal162TestKit.Service();
        var started = service.StartNew();
        var target = Assert.IsType<GeneratedCampaignMapProjection>(started.Map).Entities.First(entity =>
            entity.Interactable && Goal162TestKit.Package.Game.Dialogues.Any(dialogue =>
                dialogue.Id.StartsWith("generated/", StringComparison.Ordinal)
                && dialogue.Nodes.Any(node => node.Choices.Count > 0)
                &&
                string.Equals(dialogue.Title, entity.Title, StringComparison.Ordinal)));
        var opened = Goal162TestKit.Interact(service, target.Title);
        var choice = Assert.Single(opened.Actions.Where(action =>
            action.Kind == GeneratedCampaignActionKind.ChooseDialogue).Take(1));
        var afterChoice = service.Execute(choice.ActionId);

        var explicitService = Goal162TestKit.Service();
        var explicitStart = explicitService.StartNew();
        var explicitTarget = Assert.IsType<GeneratedCampaignMapProjection>(explicitStart.Map).Entities.First(entity =>
            entity.Interactable && Goal162TestKit.Package.Game.Dialogues.Any(dialogue =>
                dialogue.Id.StartsWith("generated/", StringComparison.Ordinal)
                && dialogue.Nodes.Any(node => node.Choices.Count > 0)
                &&
                string.Equals(dialogue.Title, entity.Title, StringComparison.Ordinal)));
        var explicitOpened = Goal162TestKit.Interact(explicitService, explicitTarget.Title);
        var close = Assert.Single(explicitOpened.Actions,
            action => action.Kind == GeneratedCampaignActionKind.CloseDialogue);
        var explicitClosed = explicitService.Execute(close.ActionId);
        return new Goal162DialogueFixture(started, opened, afterChoice, explicitOpened, explicitClosed);
    }
}

internal sealed record Goal162DialogueFixture(
    GeneratedCampaignSnapshot Started,
    GeneratedCampaignSnapshot Opened,
    GeneratedCampaignSnapshot AfterChoice,
    GeneratedCampaignSnapshot ExplicitOpened,
    GeneratedCampaignSnapshot ExplicitClosed);
