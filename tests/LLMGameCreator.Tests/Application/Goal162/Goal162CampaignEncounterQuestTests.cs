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
    public void Behavioral_generated_encounter_without_exact_combat_route_is_causally_disabled()
    {
        var state = Goal162EncounterQuestState.Value;
        var action = Assert.Single(state.BeforeFight.Actions,
            item => item.Kind == GeneratedCampaignActionKind.StartEncounter
                    && item.TargetTitle == state.EncounterTitle);

        Assert.False(action.Enabled);
        Assert.False(string.IsNullOrWhiteSpace(action.DisabledReason));
        Assert.Null(state.EncounterStarted.Encounter);
    }

    [Fact]
    public void Behavioral_disabled_generated_encounter_dispatches_no_enemy_ai()
    {
        var state = Goal162EncounterQuestState.Value;
        Assert.DoesNotContain(GameRuntimeCommandType.RunCurrentTurnAi, state.Runtime.GameplayCommands);
    }

    [Fact]
    public void Behavioral_disabled_generated_encounter_does_not_fake_resource_damage()
    {
        var state = Goal162EncounterQuestState.Value;
        Assert.DoesNotContain(GameRuntimeCommandType.BasicAttack, state.Runtime.GameplayCommands);
        Assert.DoesNotContain(state.AfterFight.Consequences,
            consequence => consequence.Kind == GeneratedCampaignConsequenceKind.Damage);
    }

    [Fact]
    public void Behavioral_disabled_generated_encounter_does_not_claim_runtime_victory()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.Null(state.AfterFight.Encounter);
        Assert.DoesNotContain(state.FightSnapshots.SelectMany(snapshot => snapshot.RecentEvents),
            message => message.Contains("побед", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_disabled_generated_encounter_grants_no_reward_item()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.DoesNotContain(state.BeforeFight.Inventory, row => row.Title == state.RewardItemTitle);
        Assert.DoesNotContain(state.AfterFight.Inventory, row => row.Title == state.RewardItemTitle);
    }

    [Fact]
    public void Behavioral_disabled_generated_encounter_leaves_quest_active_and_not_ready()
    {
        var state = Goal162EncounterQuestState.Value;
        var quest = Assert.Single(state.AfterFight.Quests, quest => quest.Title == state.QuestTitle);

        Assert.Equal("Активно", quest.StateTitle);
        Assert.False(quest.Completable);
        Assert.Contains(quest.Objectives, objective => !objective.Completed);
    }

    [Fact]
    public void Behavioral_not_ready_quest_exposes_no_manual_completion_action()
    {
        var state = Goal162EncounterQuestState.Value;

        Assert.DoesNotContain(state.AfterFight.Actions,
            action => action.Kind == GeneratedCampaignActionKind.CompleteQuest
                      && action.TargetTitle == state.QuestTitle);
        Assert.DoesNotContain(state.AfterFight.RecentEvents,
            message => message.Contains("Задание завершено", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_no_turn_in_leaves_reputation_and_quest_unchanged()
    {
        var state = Goal162EncounterQuestState.Value;
        var quest = Assert.Single(state.AfterComplete.Quests, quest => quest.Title == state.QuestTitle);

        Assert.Equal("Активно", quest.StateTitle);
        Assert.Equal(state.ReputationBefore, state.ReputationAfter);
        Assert.DoesNotContain(state.AfterComplete.Actions,
            action => action.Kind == GeneratedCampaignActionKind.CompleteQuest
                      && action.TargetTitle == state.QuestTitle);
        Assert.DoesNotContain(state.AfterComplete.RecentEvents,
            message => message.Contains("Задание завершено", StringComparison.OrdinalIgnoreCase)
                       || message.Contains("репутац", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_campaign_never_uses_direct_advance_quest_objective_command()
    {
        var commands = Goal162EncounterQuestState.Value.Runtime.GameplayCommands;

        Assert.DoesNotContain(GameRuntimeCommandType.AdvanceQuestObjective, commands);
        Assert.DoesNotContain(GameRuntimeCommandType.RefreshQuestObjectives, commands);
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
        var startAction = Assert.Single(beforeFight.Actions,
            action => action.Kind == GeneratedCampaignActionKind.StartEncounter
                      && action.TargetTitle == encounter.Name);
        Assert.False(startAction.Enabled);
        var fight = new[] { beforeFight };
        var afterFight = beforeFight;
        var afterComplete = beforeFight;
        var reputationAfter = Assert.Single(afterComplete.Factions, row => row.Title == factionTitle).Value;
        return new Goal162EncounterQuestFixture(runtime, beforeFight, fight[0], fight, afterFight,
            afterComplete, encounter.Name, quest.Title, itemTitle, reputationBefore, reputationAfter);
    }
}

internal sealed record Goal162EncounterQuestFixture(
    Goal162CountingRuntime Runtime,
    GeneratedCampaignSnapshot BeforeFight,
    GeneratedCampaignSnapshot EncounterStarted,
    IReadOnlyList<GeneratedCampaignSnapshot> FightSnapshots,
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
