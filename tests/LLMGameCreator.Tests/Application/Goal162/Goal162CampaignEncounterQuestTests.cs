using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal162;

[Collection(Goal160Collection.Name)]
public sealed class Goal162CampaignEncounterQuestTests
{
    [Fact]
    public void Behavioral_generated_encounter_starts_with_real_participants()
    {
        var started = Goal162EncounterQuestState.Value.EncounterStarted;

        Assert.True(started.Encounter?.Active);
        Assert.True(started.Encounter?.Participants.Count >= 2);
        Assert.Contains(started.Encounter!.Participants, participant => participant.TeamTitle == "Игрок");
        Assert.Contains(started.Encounter.Participants, participant => participant.TeamTitle == "Противник");
    }

    [Fact]
    public void Behavioral_bounded_enemy_ai_hands_control_to_player()
    {
        var state = Goal162EncounterQuestState.Value;
        var encounter = Assert.IsType<GeneratedCampaignEncounter>(state.EncounterStarted.Encounter);

        Assert.Contains(encounter.Participants, participant => participant.CurrentTurn
            && participant.TeamTitle == "Игрок");
        var playerTurnCommands = state.Runtime.GameplayCommands.Count(command => command is
            GameRuntimeCommandType.BasicAttack or GameRuntimeCommandType.UseAbility or GameRuntimeCommandType.EndTurn);
        Assert.True(state.Runtime.GameplayCommands.Count(command =>
            command == GameRuntimeCommandType.RunCurrentTurnAi)
                    <= (playerTurnCommands + 1) * encounter.Participants.Count * 2);
    }

    [Fact]
    public void Behavioral_player_combat_actions_change_participant_resources()
    {
        var state = Goal162EncounterQuestState.Value;
        var startedValues = state.EncounterStarted.Encounter!.Participants
            .SelectMany(participant => participant.Resources.Select(resource => resource.Value)).ToList();
        var changed = state.FightSnapshots.Skip(1)
            .SelectMany(snapshot => snapshot.Encounter?.Participants ?? [])
            .SelectMany(participant => participant.Resources.Select(resource => resource.Value)).ToList();

        Assert.NotEmpty(startedValues);
        Assert.Contains(changed, value => !startedValues.Contains(value, StringComparer.Ordinal));
        Assert.Contains(state.Runtime.GameplayCommands, command => command is GameRuntimeCommandType.BasicAttack
            or GameRuntimeCommandType.UseAbility);
    }

    [Fact]
    public void Behavioral_real_encounter_reaches_runtime_victory_without_direct_state_mutation()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.False(state.AfterFight.Encounter?.Active);
        Assert.Contains(state.FightSnapshots.SelectMany(snapshot => snapshot.RecentEvents),
            message => message.Contains("побед", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_encounter_reward_item_appears_in_projected_inventory()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.DoesNotContain(state.BeforeFight.Inventory, row => row.Title == state.RewardItemTitle);
        Assert.Contains(state.AfterFight.Inventory, row => row.Title == state.RewardItemTitle);
    }

    [Fact]
    public void Behavioral_causal_encounter_and_item_complete_real_quest_objectives()
    {
        var state = Goal162EncounterQuestState.Value;
        var quest = Assert.Single(state.AfterFight.Quests, quest => quest.Title == state.QuestTitle);

        Assert.Equal("Завершено", quest.StateTitle);
        Assert.False(quest.Completable);
        Assert.All(quest.Objectives, objective => Assert.True(objective.Completed));
        Assert.All(quest.Objectives, objective => Assert.Equal("1 / 1", objective.Progress));
    }

    [Fact]
    public void Behavioral_completed_quest_exposes_no_duplicate_completion_action()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.DoesNotContain(state.AfterFight.Actions,
            action => action.Kind == GeneratedCampaignActionKind.CompleteQuest
                      && action.TargetTitle == state.QuestTitle);
        Assert.Contains(state.AfterFight.RecentEvents,
            message => message.Contains("Задание завершено", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_quest_completion_changes_reputation_and_marks_quest_complete()
    {
        var state = Goal162EncounterQuestState.Value;
        var quest = Assert.Single(state.AfterComplete.Quests, quest => quest.Title == state.QuestTitle);

        Assert.Equal("Завершено", quest.StateTitle);
        Assert.NotEqual(state.ReputationBefore, state.ReputationAfter);
        Assert.Contains(state.AfterComplete.RecentEvents,
            message => message.Contains("Задание завершено", StringComparison.OrdinalIgnoreCase)
                       || message.Contains("репутац", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_campaign_never_uses_direct_advance_quest_objective_command()
    {
        var commands = Goal162EncounterQuestState.Value.Runtime.GameplayCommands;

        Assert.DoesNotContain(GameRuntimeCommandType.AdvanceQuestObjective, commands);
        Assert.Contains(GameRuntimeCommandType.RefreshQuestObjectives, commands);
        Assert.DoesNotContain(GameRuntimeCommandType.CompleteQuest, commands);
    }

    [Fact]
    public void Behavioral_encounter_quest_reward_and_reputation_surface_is_human_only()
    {
        var state = Goal162EncounterQuestState.Value;
        var text = string.Join(Environment.NewLine,
            state.FightSnapshots.Select(Goal162TestKit.PrimaryText).Append(
                Goal162TestKit.PrimaryText(state.AfterComplete)));

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
        var package = Goal162TestKit.Package;
        var quest = package.Game.Quests.First(candidate => candidate.AutoStart
            && candidate.Objectives.Any(objective => objective.Kind == "complete_encounter")
            && candidate.Objectives.Any(objective => objective.Kind == "has_item"));
        var encounterId = quest.Objectives.Single(objective => objective.Kind == "complete_encounter").TargetId!;
        var itemId = quest.Objectives.Single(objective => objective.Kind == "has_item").TargetId!;
        var encounter = package.Game.Encounters.Single(candidate => candidate.Id == encounterId);
        Assert.Contains(encounter.Rewards, reward => reward.Kind == "item" && reward.Id == itemId);
        var generated = package.GeneratedContent.Encounters.Single(candidate => candidate.Title == encounter.Name);
        var region = package.GeneratedContent.Regions.Single(candidate => candidate.SourceId == generated.RegionId);
        var scene = package.GeneratedContent.Scenes.First(candidate => region.SceneIds.Contains(candidate.SourceId)
            || region.SceneIds.Contains(candidate.PackageMapId));
        var destination = package.Game.Maps.Single(map => map.Id == scene.PackageMapId);
        var itemTitle = package.Game.Items.Single(item => item.Id == itemId).Name;
        var reputationReward = quest.Rewards.Single(reward => reward.Kind == "reputation");
        var factionTitle = package.Game.Factions.Single(faction => faction.Id == reputationReward.Id).Name;

        var runtime = new Goal162CountingRuntime(Goal162TestKit.Bundle.Saves.Runtime);
        var service = Goal162TestKit.Service(runtime: runtime);
        var started = service.StartNew();
        var atRegion = started.CurrentMapTitle == destination.Name
            ? started
            : Goal162TestKit.TravelTo(service, destination.Name);
        var beforeFight = atRegion;
        var reputationBefore = Assert.Single(beforeFight.Factions, row => row.Title == factionTitle).Value;
        var fight = Goal162TestKit.Fight(service, encounter.Name);
        var afterFight = fight[^1];
        var afterComplete = afterFight;
        var reputationAfter = Assert.Single(afterComplete.Factions, row => row.Title == factionTitle).Value;
        return new Goal162EncounterQuestFixture(runtime, beforeFight, fight[0], fight, afterFight,
            afterComplete, quest.Title, itemTitle, reputationBefore, reputationAfter);
    }
}

internal sealed record Goal162EncounterQuestFixture(
    Goal162CountingRuntime Runtime,
    GeneratedCampaignSnapshot BeforeFight,
    GeneratedCampaignSnapshot EncounterStarted,
    IReadOnlyList<GeneratedCampaignSnapshot> FightSnapshots,
    GeneratedCampaignSnapshot AfterFight,
    GeneratedCampaignSnapshot AfterComplete,
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
