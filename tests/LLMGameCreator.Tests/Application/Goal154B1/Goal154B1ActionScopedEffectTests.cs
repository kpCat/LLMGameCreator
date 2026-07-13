using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal154B;
using Xunit;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Tests.Application.Goal154B1;

public sealed class Goal154B1ActionScopedEffectTests
{
    [Fact]
    public void Behavioral_locked_claim_ignores_the_quest_gold_event_and_is_not_applicable()
    {
        var fixture = Goal154BFixture.Create(trustedReputationThreshold: 20);
        var result = fixture.Qualify("goal154b1-action-locked");
        var observation = TrustedResourceObservation(fixture, result.Session);

        Assert.Equal("not_applicable", observation.ActualValue);
        Assert.True(observation.Passed, string.Join(";", observation.Diagnostics));
    }

    [Fact]
    public void Behavioral_unrelated_later_gold_event_cannot_satisfy_missing_claim_resource_evidence()
    {
        var fixture = Goal154BFixture.Create();
        var result = fixture.Qualify("goal154b1-unrelated-later");
        var claim = Goal154B1QuestRewardPreservationTests.Snapshot(result.Session, "claim_trusted_reward");
        claim.RuntimeEvents = claim.RuntimeEvents.Where(item => item.EventType != "ResourceChanged").ToList();
        result.Session.CanonicalSession.Snapshots.Add(new CanonicalRuntimePlayerCommandLoopSnapshot
        {
            StepId = "capability.unrelated_gold_transaction",
            RuntimeEvents = [ResourceEvent("10", "7", "17", "7")]
        });

        var observation = TrustedResourceObservation(fixture, result.Session);

        Assert.False(observation.Passed);
        Assert.Equal(string.Empty, observation.ActualValue);
        Assert.Contains(observation.Diagnostics, item => item.Contains("missing runtime effect metric", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_multiple_claim_resource_events_are_rejected_as_ambiguous()
    {
        var fixture = Goal154BFixture.Create();
        var result = fixture.Qualify("goal154b1-ambiguous");
        var claim = Goal154B1QuestRewardPreservationTests.Snapshot(result.Session, "claim_trusted_reward");
        var reward = Goal154B1QuestRewardPreservationTests.GoldEvent(claim);
        claim.RuntimeEvents = claim.RuntimeEvents.Concat([reward]).ToList();

        var observation = TrustedResourceObservation(fixture, result.Session);

        Assert.False(observation.Passed);
        Assert.Equal("false", observation.ActualValue);
    }

    [Fact]
    public void Behavioral_action_scoped_resource_event_must_match_final_resource_state()
    {
        var fixture = Goal154BFixture.Create();
        var result = fixture.Qualify("goal154b1-final-state");
        var claim = Goal154B1QuestRewardPreservationTests.Snapshot(result.Session, "claim_trusted_reward");
        var reward = Goal154B1QuestRewardPreservationTests.GoldEvent(claim);
        reward.Args = new Dictionary<string, string>(reward.Args, StringComparer.Ordinal) { ["after"] = "18" };

        var observation = TrustedResourceObservation(fixture, result.Session);

        Assert.False(observation.Passed);
        Assert.Equal("false", observation.ActualValue);
    }

    private static FeatureModuleRuntimeEffectObservation TrustedResourceObservation(
        Goal154BFixture fixture,
        RuntimeInteractiveSession session) => new FeatureModuleRuntimeEffectEvaluator().Evaluate(fixture.SocialModules,
        session, new RuntimeInteractiveSession(), fixture.Package).Single(item =>
        item.MetricKind == FeatureModuleRuntimeEffectMetricKinds.ResourceTransitionTruthful);

    private static CanonicalRuntimePlayerCommandLoopRuntimeEvent ResourceEvent(
        string before,
        string requested,
        string after,
        string actual) => new()
    {
        EventType = "ResourceChanged",
        TargetId = "resource/gold",
        Args = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["resourceId"] = "resource/gold",
            ["before"] = before,
            ["requestedDelta"] = requested,
            ["after"] = after,
            ["actualDelta"] = actual,
            ["clamped"] = "false",
            ["scope"] = "global"
        }
    };
}
