using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal165;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal166;

public sealed class Goal166TacticalCombatUiTests
{
    [Fact] public void Behavioral_tactical_basic_action_has_human_summary() => Assert.False(EncounterActions().First(x => x.Action.Kind == GeneratedCampaignActionKind.BasicAttack).Action.Tactical!.EffectSummary.Contains("/"));
    [Fact] public void Behavioral_tactical_basic_action_is_primary() => Assert.True(EncounterActions().First(x => x.Action.Kind == GeneratedCampaignActionKind.BasicAttack).Action.Tactical!.Primary);
    [Fact] public void Behavioral_tactical_ability_has_cost_and_effect_summary() { var action = EncounterActions().First(x => x.Action.Kind == GeneratedCampaignActionKind.UseAbility); Assert.False(string.IsNullOrWhiteSpace(action.Action.Tactical!.CostSummary)); Assert.False(string.IsNullOrWhiteSpace(action.Action.Tactical.EffectSummary)); }
    [Fact] public void Behavioral_tactical_action_has_human_target() => Assert.False(string.IsNullOrWhiteSpace(EncounterActions().First(x => x.Action.Tactical is not null).Action.Tactical!.TargetTitle));
    [Fact] public void Behavioral_qualified_tactical_action_progresses_encounter() => Assert.Contains(EncounterActions(), x => x.Action.Tactical is { ProgressesEncounter: true });
    [Fact] public void Behavioral_support_action_is_not_primary_when_not_qualified() => Assert.DoesNotContain(EncounterActions().Where(x => x.Action.Tactical is { ProgressesEncounter: false }), x => x.Action.Tactical!.Primary);

    private static IReadOnlyList<GeneratedCampaignPlannedAction> EncounterActions()
    {
        var build = Goal164TestKit.AllSelectable;
        var start = build.Runtime.Start(build.Package).Session;
        var encounter = build.Package.Game.Encounters.First();
        var active = build.Runtime.ExecuteGameplayCommand(build.Package, start, GameRuntimeCommand.StartEncounter(encounter.Id)).Session;
        return new GeneratedCampaignActionPlanner().Plan(build.Package, active,
            Goal165RouteFixtures.Resolve(Goal165RouteFixtureKind.Both).Contract!.QualifiedActions);
    }
}
