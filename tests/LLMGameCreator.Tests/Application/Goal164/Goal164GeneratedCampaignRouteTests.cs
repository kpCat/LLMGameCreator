using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal162;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal164;

[Collection(Goal160Collection.Name)]
public sealed class Goal164GeneratedCampaignRouteTests
{
    [Fact]
    public void Behavioral_generated_campaign_starts_only_from_campaign_current_history()
    {
        var route = Goal164CampaignState.AllSelectable;

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, route.Started.Status);
        Assert.Equal("CAMPAIGN_CURRENT", route.Build.Build.GeneratedEncounterCombat?.Status);
    }

    [Fact]
    public void Behavioral_human_generated_encounter_action_is_enabled()
    {
        var route = Goal164CampaignState.AllSelectable;

        Assert.Contains(route.AtEncounter.Actions, item => item.Enabled
            && item.Kind == GeneratedCampaignActionKind.StartEncounter
            && item.TargetTitle == route.EncounterTitle);
    }

    [Fact]
    public void Behavioral_real_generated_route_dispatches_basic_attack()
    {
        Assert.Contains(GameRuntimeCommandType.BasicAttack,
            Goal164CampaignState.AllSelectable.Runtime.GameplayCommands);
    }

    [Fact]
    public void Behavioral_real_generated_route_dispatches_package_ability()
    {
        Assert.Contains(GameRuntimeCommandType.UseAbility,
            Goal164CampaignState.AllSelectable.Runtime.GameplayCommands);
    }

    [Fact]
    public void Behavioral_real_generated_route_dispatches_opponent_ai_and_victory()
    {
        var route = Goal164CampaignState.AllSelectable;

        Assert.Contains(GameRuntimeCommandType.RunCurrentTurnAi, route.Runtime.GameplayCommands);
        Assert.Contains(GameRuntimeCommandType.FleeEncounter, route.Runtime.GameplayCommands);
        Assert.NotNull(route.AfterVictory.Encounter);
        Assert.False(route.AfterVictory.Encounter!.Active);
        Assert.Contains(route.AfterVictory.Consequences,
            item => item.Kind == GeneratedCampaignConsequenceKind.EncounterWon);
    }

    [Fact]
    public void Behavioral_victory_grants_reward_and_leaves_generated_quest_ready_active()
    {
        var route = Goal164CampaignState.AllSelectable;
        var quest = Assert.Single(route.AfterVictory.Quests, item => item.Title == route.QuestTitle);

        Assert.True(quest.Completable);
        Assert.Equal("Готово к завершению", quest.StateTitle);
        Assert.Contains(route.AfterVictory.Inventory, item => item.Title == route.RewardTitle);
    }

    [Fact]
    public void Behavioral_manual_turn_in_dispatches_complete_once_and_reputation_consequence()
    {
        var route = Goal164CampaignState.AllSelectable;

        Assert.Equal(1, route.Runtime.GameplayCommands.Count(item =>
            item == GameRuntimeCommandType.CompleteQuest));
        Assert.DoesNotContain(GameRuntimeCommandType.AdvanceQuestObjective, route.Runtime.GameplayCommands);
        Assert.Contains(route.AfterTurnIn.Consequences, item => item.Kind is
            GeneratedCampaignConsequenceKind.QuestCompleted or GeneratedCampaignConsequenceKind.Reputation);
    }

    [Fact]
    public void Behavioral_core_only_route_preserves_travel_save_and_exact_continue()
    {
        var route = Goal164CampaignState.CoreOnly;

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, route.Continued.Status);
        Assert.Equal(route.Saved.CurrentMapTitle, route.Continued.CurrentMapTitle);
        Assert.Equal(route.Saved.CurrentRegionTitle, route.Continued.CurrentRegionTitle);
        Assert.Equal("CAMPAIGN_CURRENT", route.Build.Snapshot.GeneratedWorld?.Status);
    }
}

internal static class Goal164CampaignState
{
    private static readonly Lazy<Goal164CampaignRoute> AllSelectableRoute =
        new(() => Goal164CampaignRoute.Create(Goal164TestKit.AllSelectable, "goal164-all-route"));
    private static readonly Lazy<Goal164CampaignRoute> CoreOnlyRoute =
        new(() => Goal164CampaignRoute.Create(Goal164TestKit.CoreOnly, "goal164-core-route"));

    public static Goal164CampaignRoute AllSelectable => AllSelectableRoute.Value;
    public static Goal164CampaignRoute CoreOnly => CoreOnlyRoute.Value;
}

internal sealed record Goal164CampaignRoute(
    Goal164BuildFixture Build,
    Goal162CountingRuntime Runtime,
    GeneratedCampaignSnapshot Started,
    GeneratedCampaignSnapshot AtEncounter,
    GeneratedCampaignSnapshot AfterVictory,
    GeneratedCampaignSnapshot AfterTurnIn,
    GeneratedCampaignSnapshot Saved,
    GeneratedCampaignSnapshot Continued,
    string EncounterTitle,
    string QuestTitle,
    string RewardTitle)
{
    public static Goal164CampaignRoute Create(Goal164BuildFixture build, string slot)
    {
        var runtime = new Goal162CountingRuntime(build.Runtime);
        var service = new GeneratedCampaignSessionService(
            build.Current,
            new GeneratedCampaignSessionTruthService(
                build.Current, build.Saves.Validator, build.Saves.Coordinator),
            runtime,
            build.Saves.Save,
            build.Saves.Migration,
            new GeneratedCampaignActionPlanner(),
            new GeneratedCampaignProjectionService(),
            new GeneratedCampaignEventPresenter());
        var started = service.StartNew();
        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, started.Status);
        var sourceId = build.Build.GeneratedEncounterCombat!.TechnicalDetails["representativeEncounterSource"];
        var bindingSourceId = sourceId.StartsWith("generated/", StringComparison.Ordinal)
            ? sourceId : "generated/" + sourceId;
        var generated = build.Package.GeneratedContent.Encounters.Single(item => item.SourceId == bindingSourceId);
        var encounter = build.Package.Game.Encounters.Single(item =>
            item.Metadata.TryGetValue("sourceEncounterSeedId", out var value) && value == generated.SourceId);
        var questId = build.Build.GeneratedEncounterCombat.TechnicalDetails["representativeQuestId"];
        var quest = build.Package.Game.Quests.Single(item => item.Id == questId);
        var rewardId = quest.Objectives.Single(item => item.Kind == "has_item").TargetId;
        var rewardTitle = build.Package.Game.Items.Single(item => item.Id == rewardId).Name;
        var current = started;
        var preparationIds = build.Build.GeneratedEncounterCombat.TechnicalDetails[
                "campaignPreparationEncounterIds"]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var reserved = preparationIds.Append(encounter.Id).ToHashSet(StringComparer.Ordinal);
        var currentRegion = RegionForMap(build, current.CurrentMapTitle);
        var nextCampaignRegion = RegionForEncounter(build,
            preparationIds.FirstOrDefault() ?? encounter.Id);
        var warmup = build.Package.GeneratedContent.Encounters
            .Select(item => build.Package.Game.Encounters.Single(definition =>
                definition.Metadata.TryGetValue("sourceEncounterSeedId", out var value)
                && value == item.SourceId))
            .Where(item => !reserved.Contains(item.Id))
            .Where(item => RegionPath(build, currentRegion, RegionForEncounter(build, item.Id)).Count > 0)
            .Where(item => RegionPath(build, RegionForEncounter(build, item.Id), nextCampaignRegion).Count > 0)
            .OrderBy(item => item.Id, StringComparer.Ordinal)
            .First();
        current = TravelToEncounter(service, build, current, warmup.Id);
        var warmupStart = Assert.Single(current.Actions, item => item.Enabled
            && item.Kind == GeneratedCampaignActionKind.StartEncounter
            && item.TargetTitle == warmup.Name);
        var warmupFight = service.Execute(warmupStart.ActionId);
        var ability = warmupFight.Actions.First(item => item.Enabled
            && item.Kind == GeneratedCampaignActionKind.UseAbility);
        warmupFight = service.Execute(ability.ActionId);
        var flee = Assert.Single(warmupFight.Actions, item => item.Enabled
            && item.Kind == GeneratedCampaignActionKind.FleeEncounter);
        current = service.Execute(flee.ActionId);

        foreach (var preparationId in preparationIds)
        {
            current = TravelToEncounter(service, build, current, preparationId);
            current = WinEncounter(service, current,
                build.Package.Game.Encounters.Single(item => item.Id == preparationId).Name);
        }

        current = TravelToEncounter(service, build, current, encounter.Id);
        var atEncounter = current;
        var fight = WinEncounter(service, atEncounter, encounter.Name);
        Assert.NotNull(fight.Encounter);
        Assert.False(fight.Encounter!.Active);
        var completeActions = fight.Actions.Where(item => item.Enabled
            && item.Kind == GeneratedCampaignActionKind.CompleteQuest
            && item.TargetTitle == quest.Title).ToList();
        Assert.True(completeActions.Count == 1,
            Goal164TestKit.Canonical(new { fight.Quests, fight.Inventory, fight.Consequences }));
        var complete = completeActions[0];
        var afterTurnIn = service.Execute(complete.ActionId);
        var saved = service.Save(slot);
        var continued = service.Continue(slot);
        return new Goal164CampaignRoute(build, runtime, started, atEncounter, fight, afterTurnIn,
            saved, continued, encounter.Name, quest.Title, rewardTitle);
    }

    private static GeneratedCampaignSnapshot WinEncounter(
        GeneratedCampaignSessionService service,
        GeneratedCampaignSnapshot current,
        string encounterTitle)
    {
        var start = Assert.Single(current.Actions, item => item.Enabled
            && item.Kind == GeneratedCampaignActionKind.StartEncounter
            && item.TargetTitle == encounterTitle);
        var fight = service.Execute(start.ActionId);
        var commandBound = fight.Encounter!.Participants.SelectMany(item => item.Resources)
            .Select(row => double.TryParse(row.Value.Split('/')[0].Trim(), out var value) ? value : 0)
            .Sum() + fight.Encounter.Participants.Count;
        for (var index = 0; index < commandBound && fight.Encounter is { Active: true }; index++)
        {
            var action = fight.Actions.FirstOrDefault(item => item.Enabled
                && item.Kind == GeneratedCampaignActionKind.BasicAttack)
                         ?? fight.Actions.FirstOrDefault(item => item.Enabled
                             && item.Kind == GeneratedCampaignActionKind.RunEncounterAi);
            Assert.NotNull(action);
            fight = service.Execute(action!.ActionId);
        }
        Assert.NotNull(fight.Encounter);
        Assert.False(fight.Encounter!.Active);
        return fight;
    }

    private static GeneratedCampaignSnapshot TravelToEncounter(
        GeneratedCampaignSessionService service,
        Goal164BuildFixture build,
        GeneratedCampaignSnapshot current,
        string encounterId)
    {
        var targetRegion = RegionForEncounter(build, encounterId);
        foreach (var region in RegionPath(build,
                     RegionForMap(build, current.CurrentMapTitle), targetRegion).Skip(1))
            current = Goal162TestKit.TravelTo(service, MapForRegion(build, region).Name);
        return current;
    }

    private static string RegionForEncounter(Goal164BuildFixture build, string encounterId)
    {
        var definition = build.Package.Game.Encounters.Single(item => item.Id == encounterId);
        var source = definition.Metadata["sourceEncounterSeedId"];
        return build.Source.RegeneratedPlan!.EncounterSeeds.Single(item =>
            Canonical(item.EncounterSeedId) == source).RegionId;
    }

    private static string RegionForMap(Goal164BuildFixture build, string mapTitle)
    {
        var map = build.Package.Game.Maps.Single(item => item.Name == mapTitle);
        var scene = build.Package.GeneratedContent.Scenes.Single(item => item.PackageMapId == map.Id);
        return build.Source.RegeneratedPlan!.World.Regions.Single(region =>
            build.Package.GeneratedContent.Regions.Any(row => row.SourceId == Canonical(region.RegionId)
                && row.SceneIds.Any(id => id == scene.SourceId || id == map.Id))).RegionId;
    }

    private static LLMGameCreator.Domain.Definitions.MapDefinition MapForRegion(
        Goal164BuildFixture build,
        string regionId)
    {
        var region = build.Package.GeneratedContent.Regions.Single(item => item.SourceId == Canonical(regionId));
        var scene = build.Package.GeneratedContent.Scenes.Single(item =>
            region.SceneIds.Any(id => id == item.SourceId || id == item.PackageMapId));
        return build.Package.Game.Maps.Single(item => item.Id == scene.PackageMapId);
    }

    private static IReadOnlyList<string> RegionPath(
        Goal164BuildFixture build,
        string start,
        string target)
    {
        var previous = new Dictionary<string, string>(StringComparer.Ordinal) { [start] = start };
        var queue = new Queue<string>();
        queue.Enqueue(start);
        while (queue.Count > 0 && !previous.ContainsKey(target))
        {
            var current = queue.Dequeue();
            foreach (var next in build.Source.RegeneratedPlan!.World.Connections
                         .Where(item => item.FromRegionId == current)
                         .Select(item => item.ToRegionId))
            {
                if (!previous.TryAdd(next, current)) continue;
                queue.Enqueue(next);
            }
        }
        if (!previous.ContainsKey(target)) return [];
        var path = new List<string>();
        for (var cursor = target;; cursor = previous[cursor])
        {
            path.Add(cursor);
            if (cursor == start) break;
        }
        path.Reverse();
        return path;
    }

    private static string Canonical(string value) => value.StartsWith("generated/", StringComparison.Ordinal)
        ? value : "generated/" + value;
}
