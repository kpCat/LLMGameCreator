using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal163;

[Collection(Goal160Collection.Name)]
public sealed class Goal163QuestTurnInTests
{
    [Fact]
    public void Behavioral_generated_quest_classification_requires_marker_and_exact_mapping()
    {
        var result = Ready();

        Assert.True(result.Generated);
        Assert.True(result.MappingExact);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Behavioral_generated_quest_classification_does_not_use_substring_ids()
    {
        var package = Goal163TestKit.FullPackage();
        package.Game.Quests[0].Kind = "quest";
        package.Game.Quests[0].Tags.Clear();
        package.Game.Quests[0].Id = "custom/generated-looking";
        package.GeneratedContent.Quests[0].PackageQuestId = "custom/generated-looking";
        var session = Goal163TestKit.ReadyQuestSession();
        session.GameplayState.Quests[0].QuestId = "custom/generated-looking";

        var result = new GeneratedCampaignQuestReadinessService().Evaluate(
            package, session, "custom/generated-looking");

        Assert.False(result.Generated);
    }

    [Fact]
    public void Behavioral_complete_encounter_readiness_is_true_only_after_victory()
    {
        var result = Ready();

        Assert.True(result.Objectives.Single(item => item.Kind == "complete_encounter").Satisfied);
    }

    [Fact]
    public void Behavioral_flee_does_not_satisfy_complete_encounter_objective()
    {
        var result = Ready(Goal163TestKit.ReadyQuestSession(fled: true));

        Assert.False(result.Ready);
        Assert.False(result.Objectives.Single(item => item.Kind == "complete_encounter").Satisfied);
    }

    [Fact]
    public void Behavioral_has_item_readiness_uses_exact_player_owned_amount()
    {
        var notReady = Ready(Goal163TestKit.ReadyQuestSession(itemAmount: 0.5));
        var ready = Ready(Goal163TestKit.ReadyQuestSession(itemAmount: 1));

        Assert.Equal(0.5, notReady.Objectives.Single(item => item.Kind == "has_item").CurrentAmount);
        Assert.False(notReady.Ready);
        Assert.True(ready.Ready);
    }

    [Fact]
    public void Behavioral_unsupported_required_objective_kind_blocks_readiness()
    {
        var package = Goal163TestKit.FullPackage();
        package.Game.Quests[0].Objectives[1].Kind = "visit_unknown_place";
        var session = Goal163TestKit.ReadyQuestSession();
        session.GameplayState.Quests[0].Objectives[1].Kind = "visit_unknown_place";

        var result = new GeneratedCampaignQuestReadinessService().Evaluate(
            package, session, "quest/generated");

        Assert.False(result.Ready);
        Assert.Contains("campaign.quest_objective_kind_unsupported:visit_unknown_place",
            result.Diagnostics);
    }

    [Fact]
    public void Behavioral_generated_readiness_does_not_mutate_runtime_objective_state()
    {
        var package = Goal163TestKit.FullPackage();
        var session = Goal163TestKit.ReadyQuestSession();
        var before = Goal163TestKit.Copy(session);

        var result = new GeneratedCampaignQuestReadinessService().Evaluate(
            package, session, "quest/generated");

        Assert.True(result.Ready);
        Assert.Equal(before.GameplayState.Quests[0].Objectives.Select(item => item.Completed),
            session.GameplayState.Quests[0].Objectives.Select(item => item.Completed));
        Assert.All(session.GameplayState.Quests[0].Objectives, item => Assert.False(item.Completed));
    }

    [Fact]
    public void Behavioral_generated_quest_remains_active_when_computed_ready()
    {
        var session = Goal163TestKit.ReadyQuestSession();

        var result = Ready(session);

        Assert.True(result.Ready);
        Assert.True(result.Active);
        Assert.Equal("active", session.GameplayState.Quests[0].State);
    }

    [Fact]
    public void Behavioral_manual_turn_in_action_appears_only_when_ready()
    {
        var package = Goal163TestKit.FullPackage();
        var session = Goal163TestKit.ReadyQuestSession();

        var actions = new GeneratedCampaignActionPlanner().Plan(package, session);

        var action = Assert.Single(actions, item => item.Action.Kind == GeneratedCampaignActionKind.CompleteQuest);
        Assert.True(action.Action.Enabled);
        Assert.StartsWith("Завершить задание: ", action.Action.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void Behavioral_readiness_is_rechecked_and_stale_turn_in_dispatches_zero_commands()
    {
        var package = Goal163TestKit.FullPackage();
        var session = Goal163TestKit.ReadyQuestSession();
        var first = Ready(session);
        session.GameplayState.Inventories[0].Stacks.Clear();
        var second = Ready(session);
        var runtime = new Goal163SpyRuntime(
            LLMGameCreator.Tests.Application.Goal162.Goal162TestKit.Bundle.Saves.Runtime);

        Assert.True(first.Ready);
        Assert.False(second.Ready);
        Assert.Empty(runtime.GameplayCommands);
    }

    [Fact]
    public void Behavioral_complete_quest_dispatch_executes_exactly_once()
    {
        var fixture = CompleteQuest();

        Assert.True(fixture.Result.Passed, string.Join(",", fixture.Result.Diagnostics));
        Assert.Equal(1, fixture.Runtime.GameplayCommands.Count(item =>
            item.Type == GameRuntimeCommandType.CompleteQuest));
    }

    [Fact]
    public void Behavioral_manual_turn_in_never_dispatches_advance_objective()
    {
        var fixture = CompleteQuest();

        Assert.DoesNotContain(fixture.Runtime.GameplayCommands,
            item => item.Type == GameRuntimeCommandType.AdvanceQuestObjective);
    }

    [Fact]
    public void Behavioral_manual_turn_in_applies_reward_and_reputation()
    {
        var fixture = CompleteQuest();
        var state = fixture.Result.UnifiedRuntimeResult.Session.GameplayState;

        Assert.Contains(state.Inventories.SelectMany(item => item.Stacks),
            item => item.ItemId == "item/quest-reward" && item.Amount == 1);
        Assert.Equal(5, state.Factions.Single(item => item.FactionId == "faction/keepers").Reputation);
        Assert.Equal("completed", state.Quests.Single(item => item.QuestId == "quest/generated").State);
    }

    [Fact]
    public void Behavioral_turn_in_action_disappears_after_completion_and_non_generated_compatibility_remains()
    {
        var fixture = CompleteQuest();
        var generatedActions = new GeneratedCampaignActionPlanner().Plan(
            fixture.Package, fixture.Result.UnifiedRuntimeResult.Session);
        var customPackage = Goal163TestKit.FullPackage();
        customPackage.Game.Quests[0].Kind = "custom";
        customPackage.Game.Quests[0].Tags.Clear();
        customPackage.GeneratedContent.Quests.Clear();
        var customSession = Goal163TestKit.ReadyQuestSession();
        customSession.GameplayState.Quests[0].Objectives.ForEach(item => item.Completed = true);
        var customActions = new GeneratedCampaignActionPlanner().Plan(customPackage, customSession);

        Assert.DoesNotContain(generatedActions,
            item => item.Action.Kind == GeneratedCampaignActionKind.CompleteQuest);
        Assert.Contains(customActions,
            item => item.Action.Kind == GeneratedCampaignActionKind.CompleteQuest);
    }

    private static GeneratedCampaignQuestReadiness Ready(UnifiedRuntimeSession? session = null) =>
        new GeneratedCampaignQuestReadinessService().Evaluate(
            Goal163TestKit.FullPackage(), session ?? Goal163TestKit.ReadyQuestSession(), "quest/generated");

    private static Goal163DispatchFixture CompleteQuest()
    {
        var package = Goal163TestKit.FullPackage();
        var session = Goal163TestKit.ReadyQuestSession();
        Assert.True(new GeneratedCampaignQuestReadinessService()
            .Evaluate(package, session, "quest/generated").Ready);
        return Goal163TestKit.Dispatch(
            new GameRuntimeCommand { Type = GameRuntimeCommandType.CompleteQuest, Id = "quest/generated" },
            package,
            session);
    }
}
