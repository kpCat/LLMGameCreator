using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal160;
using LLMGameCreator.Tests.Application.Goal162;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal163;

[Collection(Goal160Collection.Name)]
public sealed class Goal163RegressionImmutabilityTests
{
    [Fact]
    public void Behavioral_real_truth_exposes_actual_final_state_hash_separately_from_history_sha()
    {
        var result = Goal162TestKit.TruthService().Capture();

        Assert.Equal(GeneratedCampaignSessionStatus.READY, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.Truth!.FinalStateHash));
        Assert.False(string.IsNullOrWhiteSpace(result.Truth.SelectedBuildHistorySha256));
        Assert.NotEqual(result.Truth.FinalStateHash, result.Truth.SelectedBuildHistorySha256);
    }

    [Fact]
    public void Behavioral_projection_keeps_hashes_in_technical_details_only()
    {
        var service = Goal162TestKit.Service();
        var snapshot = service.StartNew();
        var primary = Goal162TestKit.PrimaryText(snapshot);

        Assert.Equal(snapshot.TechnicalDetails["finalStateHash"], Goal162TestKit.TruthService().Capture().Truth!.FinalStateHash);
        Assert.Equal(snapshot.TechnicalDetails["selectedBuildHistorySha256"], Goal162TestKit.TruthService().Capture().Truth!.SelectedBuildHistorySha256);
        Assert.DoesNotContain(snapshot.TechnicalDetails["finalStateHash"], primary);
        Assert.DoesNotContain(snapshot.TechnicalDetails["selectedBuildHistorySha256"], primary);
    }

    [Fact]
    public void Behavioral_real_all_selectable_package_never_offers_synthetic_attack()
    {
        var package = Goal162TestKit.Package;
        var service = Goal162TestKit.Service();
        var snapshot = service.StartNew();

        Assert.DoesNotContain(package.Game.Abilities, item => item.Id == "campaign/session-compatible-attack");
        Assert.DoesNotContain(snapshot.Actions, item => item.Description.Contains("campaign/session-compatible-attack", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_basic_attack_plans_exact_command_type_without_use_ability_substitution()
    {
        var package = Goal163TestKit.CombatPackage();
        var session = Goal163TestKit.CombatSession();

        var action = Assert.Single(new GeneratedCampaignActionPlanner().Plan(package, session),
            item => item.Action.Kind == GeneratedCampaignActionKind.BasicAttack);

        Assert.Equal(GameRuntimeCommandType.BasicAttack, action.RuntimeCommand!.Type);
        Assert.NotEqual(GameRuntimeCommandType.UseAbility, action.RuntimeCommand.Type);
        Assert.True(string.IsNullOrWhiteSpace(action.RuntimeCommand.Id));
    }

    [Fact]
    public void Behavioral_core_only_route_is_real_and_truthfully_classified()
    {
        var snapshot = Goal164CampaignState.CoreOnly.Started;

        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, snapshot.Status);
        Assert.NotNull(snapshot.Map);
        Assert.DoesNotContain(snapshot.Diagnostics, item => item.Contains("synthetic", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_non_generated_quest_refresh_compatibility_is_retained()
    {
        var package = Goal163TestKit.FullPackage();
        package.Game.Quests[0].Kind = "custom";
        package.Game.Quests[0].Tags.Clear();
        package.GeneratedContent.Quests.Clear();
        var session = Goal163TestKit.ReadyQuestSession();
        session.GameplayState.Quests[0].Objectives.ForEach(item => item.Completed = true);

        var actions = new GeneratedCampaignActionPlanner().Plan(package, session);

        Assert.Contains(actions, item => item.Action.Kind == GeneratedCampaignActionKind.CompleteQuest);
        Assert.DoesNotContain(new GeneratedCampaignQuestReadinessService().EvaluateAll(package, session), item => item.Generated);
    }
}
