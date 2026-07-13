using System.Text.Json;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal154B;
using Xunit;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Tests.Application.Goal154B1;

public sealed class Goal154B1QuestRewardPreservationTests
{
    [Fact]
    public void Behavioral_baseline_healer_quest_keeps_existing_ten_gold_reward()
    {
        var package = Goal154BFixture.Deserialize(Goal154BFixture.ReadBasePackage());

        Assert.Equal(10, GoldReward(package));
    }

    [Fact]
    public void Behavioral_faction_module_alone_preserves_quest_and_dialogue_bytes()
    {
        var fixture = Goal154BFixture.CreateSelected(Goal154BFixture.FactionModuleId);
        var baseline = Goal154BFixture.Deserialize(fixture.BasePackageJson);

        Assert.Equal(JsonSerializer.Serialize(baseline.Game.Quests), JsonSerializer.Serialize(fixture.Package.Game.Quests));
        Assert.Equal(JsonSerializer.Serialize(baseline.Game.Dialogues), JsonSerializer.Serialize(fixture.Package.Game.Dialogues));
        Assert.Equal(0, fixture.Package.Game.Factions.Single(item => item.Id == "faction/village").DefaultReputation);
    }

    [Fact]
    public void Behavioral_quest_reputation_module_without_dialogue_preserves_gold_and_changes_reputation()
    {
        var fixture = Goal154BFixture.CreateSelected(Goal154BFixture.FactionModuleId, Goal154BFixture.QuestModuleId);
        var result = fixture.Qualify("goal154b1-quest-without-dialogue");
        var state = result.Session.CanonicalSession.RuntimeSession.GameplayState;

        Assert.Equal(10, GoldReward(fixture.Package));
        Assert.Equal(10, state.Factions.Single(item => item.FactionId == "faction/village").Reputation);
        Assert.DoesNotContain(fixture.Plan.OrderedActions, item => item.ActionId == "claim_trusted_reward");
        Assert.DoesNotContain(state.Flags, item => item.Id == "flag/village_trusted_reward_claimed");
    }

    [Fact]
    public void Behavioral_default_social_lifecycle_is_zero_to_ten_to_seventeen()
    {
        var fixture = Goal154BFixture.Create();
        var result = fixture.Qualify("goal154b1-default");
        var state = result.Session.CanonicalSession.RuntimeSession.GameplayState;
        var questSnapshot = Snapshot(result.Session, "advance_healer_objective");
        var claimSnapshot = Snapshot(result.Session, "claim_trusted_reward");

        Assert.Equal("10", GoldEvent(questSnapshot).Args["after"]);
        Assert.Equal("10", GoldEvent(claimSnapshot).Args["before"]);
        Assert.Equal(17, Goal154BFixture.Gold(state));
        Assert.Equal("true", state.Flags.Single(item => item.Id == "flag/village_trusted_reward_claimed").Value);
    }

    [Fact]
    public void Behavioral_locked_threshold_preserves_ten_gold_and_emits_no_claim_resource_event()
    {
        var fixture = Goal154BFixture.Create(trustedReputationThreshold: 20);
        var result = fixture.Qualify("goal154b1-locked");
        var state = result.Session.CanonicalSession.RuntimeSession.GameplayState;
        var claimSnapshot = Snapshot(result.Session, "claim_trusted_reward");

        Assert.Equal("SKIPPED", result.Session.ActionJournal.Single(item => item.ActionId == "claim_trusted_reward").Status);
        Assert.Equal(10, Goal154BFixture.Gold(state));
        Assert.Empty(claimSnapshot.RuntimeEvents.Where(item => item.EventType == "ResourceChanged"));
        Assert.DoesNotContain(state.Flags, item => item.Id == "flag/village_trusted_reward_claimed"
                                                 && item.Value.Equals("true", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Behavioral_zero_trusted_reward_keeps_ten_gold_and_sets_claim_flag()
    {
        var fixture = Goal154BFixture.Create(trustedGoldReward: 0);
        var result = fixture.Qualify("goal154b1-zero");
        var state = result.Session.CanonicalSession.RuntimeSession.GameplayState;
        var reward = GoldEvent(Snapshot(result.Session, "claim_trusted_reward"));

        Assert.Equal(10, Goal154BFixture.Gold(state));
        Assert.Equal("0", reward.Args["requestedDelta"]);
        Assert.Equal("0", reward.Args["actualDelta"]);
        Assert.Equal("true", state.Flags.Single(item => item.Id == "flag/village_trusted_reward_claimed").Value);
    }

    [Fact]
    public void Behavioral_custom_trusted_reward_nine_finishes_with_nineteen_gold()
    {
        var fixture = Goal154BFixture.Create(trustedGoldReward: 9);
        var result = fixture.Qualify("goal154b1-custom-nine");
        var reward = GoldEvent(Snapshot(result.Session, "claim_trusted_reward"));

        Assert.Equal(19, Goal154BFixture.Gold(result.Session.CanonicalSession.RuntimeSession.GameplayState));
        Assert.Equal("9", reward.Args["actualDelta"]);
    }

    [Fact]
    public void Behavioral_social_runtime_effects_remain_green_for_the_corrected_default_lifecycle()
    {
        var fixture = Goal154BFixture.Create();
        var result = fixture.Qualify("goal154b1-social-effects");
        var observations = new FeatureModuleRuntimeEffectEvaluator().Evaluate(fixture.SocialModules,
            result.Session, new RuntimeInteractiveSession(), fixture.Package);

        Assert.All(observations, observation => Assert.True(observation.Passed,
            observation.EffectId + ":" + observation.ActualValue + ":" + string.Join(";", observation.Diagnostics)));
    }

    private static double GoldReward(LLMGameCreator.GamePackage.GamePackageDefinition package) => package.Game.Quests
        .Single(item => item.Id == "quest/help_healer").Rewards
        .Single(item => item.Kind == "resource" && item.Id == "resource/gold").Amount;

    internal static CanonicalRuntimePlayerCommandLoopSnapshot Snapshot(RuntimeInteractiveSession session, string actionId) =>
        session.CanonicalSession.Snapshots.Single(item => item.StepId == "capability." + actionId);

    internal static CanonicalRuntimePlayerCommandLoopRuntimeEvent GoldEvent(CanonicalRuntimePlayerCommandLoopSnapshot snapshot) =>
        snapshot.RuntimeEvents.Single(item => item.EventType == "ResourceChanged" && item.TargetId == "resource/gold");
}
