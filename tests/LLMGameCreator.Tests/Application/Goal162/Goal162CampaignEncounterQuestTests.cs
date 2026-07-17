using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal162;

[Collection(Goal160Collection.Name)]
public sealed class Goal162CampaignEncounterQuestTests
{
    [Fact]
    public void Behavioral_generated_encounter_with_exact_combat_route_is_causally_enabled()
    {
        var state = Goal162EncounterQuestState.Value;
        var action = Assert.Single(state.BeforeFight.Actions,
            item => item.Kind == GeneratedCampaignActionKind.StartEncounter
                    && item.TargetTitle == state.EncounterTitle);

        Assert.True(action.Enabled);
        Assert.True(string.IsNullOrWhiteSpace(action.DisabledReason));
        Assert.NotNull(state.AfterFight.Encounter);
    }

    [Fact]
    public void Behavioral_generated_encounter_dispatches_enemy_ai()
    {
        Assert.Contains(GameRuntimeCommandType.RunCurrentTurnAi,
            Goal162EncounterQuestState.Value.Runtime.GameplayCommands);
    }

    [Fact]
    public void Behavioral_generated_encounter_dispatches_real_resource_damage()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.Contains(GameRuntimeCommandType.BasicAttack, state.Runtime.GameplayCommands);
        Assert.Contains(state.AfterFight.Consequences,
            consequence => consequence.Kind == GeneratedCampaignConsequenceKind.Damage);
    }

    [Fact]
    public void Behavioral_generated_encounter_claims_runtime_victory()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.NotNull(state.AfterFight.Encounter);
        Assert.False(state.AfterFight.Encounter!.Active);
        Assert.Contains(state.AfterFight.Consequences,
            consequence => consequence.Kind == GeneratedCampaignConsequenceKind.EncounterWon);
    }

    [Fact]
    public void Behavioral_generated_encounter_grants_reward_item()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.Contains(state.AfterFight.Inventory, row => row.Title == state.RewardItemTitle);
    }

    [Fact]
    public void Behavioral_generated_encounter_leaves_quest_active_and_ready_for_turn_in()
    {
        var state = Goal162EncounterQuestState.Value;
        var quest = Assert.Single(state.AfterFight.Quests, quest => quest.Title == state.QuestTitle);

        Assert.True(quest.Completable);
        Assert.All(quest.Objectives, objective => Assert.True(objective.Completed));
    }

    [Fact]
    public void Behavioral_ready_quest_exposes_manual_completion_action()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.Contains(state.AfterFight.Actions,
            action => action.Kind == GeneratedCampaignActionKind.CompleteQuest
                      && action.TargetTitle == state.QuestTitle && action.Enabled);
    }

    [Fact]
    public void Behavioral_manual_turn_in_changes_reputation_and_completes_quest()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.NotEqual(state.ReputationBefore, state.ReputationAfter);
        Assert.Contains(state.AfterComplete.Consequences, consequence => consequence.Kind is
            GeneratedCampaignConsequenceKind.QuestCompleted or GeneratedCampaignConsequenceKind.Reputation);
    }

    [Fact]
    public void Behavioral_campaign_uses_complete_quest_without_direct_objective_advance()
    {
        var commands = Goal162EncounterQuestState.Value.Runtime.GameplayCommands;

        Assert.DoesNotContain(GameRuntimeCommandType.AdvanceQuestObjective, commands);
        Assert.DoesNotContain(GameRuntimeCommandType.RefreshQuestObjectives, commands);
        Assert.Equal(1, commands.Count(command => command == GameRuntimeCommandType.CompleteQuest));
    }

    [Fact]
    public void Behavioral_generated_encounter_reward_and_reputation_surface_is_human_only()
    {
        var state = Goal162EncounterQuestState.Value;
        var text = string.Join(Environment.NewLine,
            new[]
            {
                Goal162TestKit.PrimaryText(state.AfterFight),
                Goal162TestKit.PrimaryText(state.AfterComplete)
            });

        Assert.DoesNotContain("generated/", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("encounter/", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("faction/", text, StringComparison.OrdinalIgnoreCase);
    }
}

internal static class Goal162EncounterQuestState
{
    private static readonly Lazy<Goal162EncounterQuestFixture> Fixture = new(Create);
    public static Goal162EncounterQuestFixture Value => Fixture.Value;

    private static Goal162EncounterQuestFixture Create()
    {
        var route = Goal164CampaignState.AllSelectable;
        var quest = route.Build.Package.Game.Quests.Single(candidate => candidate.Title == route.QuestTitle);
        var reputationReward = quest.Rewards.Single(reward => reward.Kind == "reputation");
        var factionTitle = route.Build.Package.Game.Factions
            .Single(faction => faction.Id == reputationReward.Id).Name;
        var reputationBefore = Assert.Single(route.AfterVictory.Factions,
            row => row.Title == factionTitle).Value;
        var reputationAfter = Assert.Single(route.AfterTurnIn.Factions,
            row => row.Title == factionTitle).Value;
        return new Goal162EncounterQuestFixture(route.Runtime, route.AtEncounter, route.AfterVictory,
            route.AfterTurnIn, route.EncounterTitle, route.QuestTitle, route.RewardTitle,
            reputationBefore, reputationAfter);
    }
}

internal sealed record Goal162EncounterQuestFixture(
    Goal162CountingRuntime Runtime,
    GeneratedCampaignSnapshot BeforeFight,
    GeneratedCampaignSnapshot AfterFight,
    GeneratedCampaignSnapshot AfterComplete,
    string EncounterTitle,
    string QuestTitle,
    string RewardItemTitle,
    string ReputationBefore,
    string ReputationAfter);

internal sealed class Goal162CountingRuntime : IUnifiedGameRuntimeService
{
    private readonly IUnifiedGameRuntimeService _inner;

    public Goal162CountingRuntime(IUnifiedGameRuntimeService inner) => _inner = inner;

    public int StartCount { get; private set; }
    public List<PlayerCommandType> PlayerCommands { get; } = [];
    public List<GameRuntimeCommandType> GameplayCommands { get; } = [];
    public List<UnifiedRuntimeResult> Results { get; } = [];

    public UnifiedRuntimeResult Start(GamePackageDefinition package)
    {
        StartCount++;
        var result = _inner.Start(package);
        Results.Add(result);
        return result;
    }

    public UnifiedRuntimeResult ExecutePlayerCommand(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        PlayerCommand command)
    {
        PlayerCommands.Add(command.Type);
        var result = _inner.ExecutePlayerCommand(package, session, command);
        Results.Add(result);
        return result;
    }

    public UnifiedRuntimeResult ExecuteGameplayCommand(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        GameRuntimeCommand command)
    {
        GameplayCommands.Add(command.Type);
        var result = _inner.ExecuteGameplayCommand(package, session, command);
        Results.Add(result);
        return result;
    }

    public UnifiedRuntimeResult ExecuteMany(
        GamePackageDefinition package,
        UnifiedRuntimeSession session,
        IEnumerable<GameRuntimeCommand> commands)
    {
        var materialized = commands.ToList();
        GameplayCommands.AddRange(materialized.Select(command => command.Type));
        var result = _inner.ExecuteMany(package, session, materialized);
        Results.Add(result);
        return result;
    }
}
