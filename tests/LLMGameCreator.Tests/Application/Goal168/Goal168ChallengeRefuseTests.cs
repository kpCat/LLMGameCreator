using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal166;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal168;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal168ChallengeRefuseTests
{
    [Fact]
    public void Behavioral_challenge_flee_has_no_reward_quest_or_reputation()
    {
        var started = Goal168TestKit.Real.Runtime.Start(Goal168TestKit.Package);
        var result = new GeneratedCampaignExactCombatRouteService().Execute(
            new GeneratedCampaignExactCombatRouteRequest
            {
                FinalPackage = Goal168TestKit.Package,
                EncounterId = Goal168TestKit.EncounterId,
                CombatSummary = Goal168TestKit.Combat,
                Runtime = Goal168TestKit.Real.Runtime,
                InitialSession = started.Session,
                Goal = GeneratedCampaignExactCombatRouteGoal.FLEE
            });
        Assert.True(result.Passed,
            string.Join(Environment.NewLine, result.Diagnostics));
        Assert.False(result.RewardObserved);
        Assert.False(result.QuestProgressObserved);
        Assert.False(result.ReputationChanged);
    }

    [Fact]
    public void Behavioral_challenge_victory_uses_exact_catalog()
    {
        Assert.True(Goal168TestKit.Relationships.ChallengeVictoryPassed);
        Assert.True(Goal168TestKit.Relationships.ExactCombatCatalogPassed);
        Assert.Equal(Goal168TestKit.Combat.QualifiedActionsSha256,
            Goal168TestKit.Relationships.QualifiedActionsSha256);
    }

    [Fact]
    public void Behavioral_challenge_defeat_retry_remains_functional()
    {
        var route = Goal166CampaignRoute.Defeat();
        var retried = route.Service.Execute(
            GeneratedCampaignRecoveryService.RetryActionId);
        Assert.Equal(GeneratedCampaignSessionStatus.ACTIVE, retried.Status);
        Assert.Equal(route.EncounterTitle, retried.Encounter!.Title);
    }

    [Fact]
    public void Behavioral_refuse_applies_exact_negative_reputation() =>
        Assert.True(Goal168TestKit.Relationships.RefusePassed);

    [Fact]
    public void Behavioral_refuse_starts_no_quest_or_encounter()
    {
        var frames = Goal168TestKit.Relationships.RuntimeFrames.Where(item =>
            item.Branch == GeneratedCampaignRelationshipBranch.REFUSE);
        Assert.NotEmpty(frames);
        Assert.All(frames, item =>
        {
            Assert.Equal(GameRuntimeCommandType.ChooseDialogueOption.ToString(),
                item.CommandType);
            Assert.True(string.IsNullOrWhiteSpace(item.QuestId));
            Assert.True(string.IsNullOrWhiteSpace(item.EncounterId));
        });
    }

    [Fact]
    public void Behavioral_branches_are_exclusive_after_decision() =>
        Assert.True(Goal168TestKit.Relationships.ExclusiveBranchingPassed);

    [Fact]
    public void Behavioral_challenge_flee_and_victory_are_both_qualified()
    {
        Assert.True(Goal168TestKit.Relationships.ChallengeFleePassed);
        Assert.True(Goal168TestKit.Relationships.ChallengeVictoryPassed);
    }

    [Fact]
    public void Behavioral_changed_descriptor_failure_is_atomic()
    {
        Assert.True(Goal168TestKit.Relationships.AtomicRollbackPassed);
        Assert.True(Goal168TestKit.Relationships.ExactCombatCatalogPassed);
    }
}
