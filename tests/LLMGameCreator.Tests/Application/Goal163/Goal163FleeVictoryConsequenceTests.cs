using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal163;

[Collection(Goal160Collection.Name)]
public sealed class Goal163FleeVictoryConsequenceTests
{
    private readonly GeneratedCampaignConsequenceProjector _projector = new();

    [Fact]
    public void Behavioral_damage_is_projected_only_from_exact_state_delta()
    {
        var before = Goal163TestKit.CombatSession();
        var after = Goal163TestKit.Copy(before);
        after.GameplayState.ActiveEncounter!.Participants[1].Resources[0].Amount = 1;

        var outcome = Project(before, after);

        var damage = Assert.Single(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Damage);
        Assert.Equal("3", damage.BeforeValue);
        Assert.Equal("1", damage.AfterValue);
        Assert.Equal("-2", damage.Delta);
    }

    [Fact]
    public void Behavioral_unchanged_state_does_not_invent_damage()
    {
        var session = Goal163TestKit.CombatSession();

        var outcome = Project(session, Goal163TestKit.Copy(session));

        Assert.DoesNotContain(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Damage);
        Assert.Equal(outcome.BeforeSessionSha256, outcome.AfterSessionSha256);
    }

    [Fact]
    public void Behavioral_victory_requires_exact_runtime_event()
    {
        var before = Goal163TestKit.CombatSession(1);
        var after = Goal163TestKit.CombatSession(0, false);

        var withoutEvent = Project(before, after);
        var withEvent = Project(before, after,
            gameplayEvents: [new GameRuntimeEvent { Type = GameRuntimeEventType.EncounterWon }]);

        Assert.DoesNotContain(withoutEvent.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.EncounterWon);
        Assert.Contains(withEvent.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.EncounterWon);
    }

    [Fact]
    public void Behavioral_flee_is_distinct_from_victory_and_reward()
    {
        var before = Goal163TestKit.CombatSession();
        var after = Goal163TestKit.CombatSession(active: false);
        var outcome = Project(before, after,
            new GeneratedCampaignAction { Kind = GeneratedCampaignActionKind.FleeEncounter, Title = "Покинуть встречу" },
            gameplayEvents: [new GameRuntimeEvent { Type = GameRuntimeEventType.EncounterEnded }]);

        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.EncounterFled);
        Assert.DoesNotContain(outcome.Consequences, item => item.Kind is GeneratedCampaignConsequenceKind.EncounterWon or GeneratedCampaignConsequenceKind.Reward);
    }

    [Fact]
    public void Behavioral_reward_row_requires_runtime_reward_event_or_inventory_delta()
    {
        var before = Goal163TestKit.ReadyQuestSession(itemAmount: 0);
        var after = Goal163TestKit.Copy(before);

        var noReward = Project(before, after);
        var exactReward = Project(before, after,
            gameplayEvents: [new GameRuntimeEvent { Type = GameRuntimeEventType.RewardGranted }]);

        Assert.DoesNotContain(noReward.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Reward);
        Assert.Contains(exactReward.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Reward);
    }

    [Fact]
    public void Behavioral_generated_quest_ready_transition_is_human_visible()
    {
        var session = Goal163TestKit.ReadyQuestSession();
        var before = new GeneratedCampaignQuestReadiness { QuestId = "quest/generated", Generated = true };
        var after = new GeneratedCampaignQuestReadiness { QuestId = "quest/generated", Generated = true, Ready = true };

        var outcome = Project(session, Goal163TestKit.Copy(session), beforeReadiness: [before], afterReadiness: [after]);

        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.QuestReady);
        Assert.DoesNotContain("quest/generated", string.Join("\n", outcome.Consequences.Select(item => item.Title)));
    }

    [Fact]
    public void Behavioral_manual_completion_projects_quest_inventory_and_reputation_deltas()
    {
        var fixture = Goal163TestKit.Dispatch(
            new GameRuntimeCommand { Type = GameRuntimeCommandType.CompleteQuest, Id = "quest/generated" },
            Goal163TestKit.FullPackage(), Goal163TestKit.ReadyQuestSession());

        var outcome = Project(fixture.InitialSession, fixture.Result.UnifiedRuntimeResult.Session,
            gameplayEvents: fixture.Result.UnifiedRuntimeResult.GameplayEvents);

        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.QuestCompleted);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Inventory);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Reputation);
    }

    [Fact]
    public void Behavioral_map_travel_requires_state_delta_and_map_changed_event()
    {
        var before = Goal163TestKit.CombatSession();
        var after = Goal163TestKit.Copy(before);
        before.MapState.CurrentMapId = "map/start";
        after.MapState.CurrentMapId = "map/next";

        var noEvent = Project(before, after);
        var exactEvent = Project(before, after,
            mapEvents: [new RuntimeEvent { Type = RuntimeEventType.MapChanged }]);

        Assert.DoesNotContain(noEvent.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.MapTravel);
        Assert.Contains(exactEvent.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.MapTravel);
    }

    [Fact]
    public void Behavioral_failed_action_has_stable_hash_and_failure_row()
    {
        var session = Goal163TestKit.CombatSession();

        var outcome = _projector.ProjectFailure("Недоступное действие", session, ["campaign.not_ready"]);

        Assert.False(outcome.Success);
        Assert.Equal(outcome.BeforeSessionSha256, outcome.AfterSessionSha256);
        Assert.Contains(outcome.Consequences, item => item.Kind == GeneratedCampaignConsequenceKind.Failure);
    }

    private GeneratedCampaignActionOutcome Project(
        UnifiedRuntimeSession before,
        UnifiedRuntimeSession after,
        GeneratedCampaignAction? action = null,
        IReadOnlyList<RuntimeEvent>? mapEvents = null,
        IReadOnlyList<GameRuntimeEvent>? gameplayEvents = null,
        IReadOnlyList<GeneratedCampaignQuestReadiness>? beforeReadiness = null,
        IReadOnlyList<GeneratedCampaignQuestReadiness>? afterReadiness = null) => _projector.ProjectAction(
            Goal163TestKit.FullPackage(), before, after, mapEvents ?? [], gameplayEvents ?? [],
            action ?? new GeneratedCampaignAction { Kind = GeneratedCampaignActionKind.BasicAttack, Title = "Обычная атака" },
            beforeReadiness ?? [], afterReadiness ?? [], true, []);
}
