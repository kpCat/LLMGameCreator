using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Tests.Application.Goal154D;

public sealed class Goal154DEffectProjectionCorrelationTests
{
    [Fact]
    public void Behavioral_explicit_path_correlates_reputation_and_gold_to_advance_completion_snapshot()
    {
        var fixture = Goal154DFixture.Create(startingHerbs: 2);
        var result = fixture.Qualify("explicit-correlation");
        var completion = CompletionSnapshot(result.Session);
        var projection = Project(fixture, result.Session, result.CheckpointReplay.Passed, result.FinalReplay.Passed);

        Assert.Equal("capability." + Goal154DFixture.AdvanceActionId, completion.StepId);
        Assert.Single(completion.RuntimeEvents, item => item.EventType == "FactionReputationChanged");
        Assert.Single(completion.RuntimeEvents, item => item.EventType == "ResourceChanged" && item.TargetId == "resource/gold");
        Assert.Equal("EXECUTED", result.Session.ActionJournal.Single(item => item.ActionId == Goal154DFixture.AdvanceActionId).Status);
        Assert.True(projection.Passed, string.Join("; ", projection.Diagnostics) + " | "
            + string.Join("; ", Observations(fixture, result.Session).Select(item =>
                item.MetricKind + "=" + item.ActualValue + ":" + item.Passed)));
        Assert.Equal(0, projection.ReputationBefore);
        Assert.Equal(10, projection.ReputationAfter);
        Assert.Equal(0, projection.GoldBefore);
        Assert.Equal(10, projection.GoldAfterQuest);
        Assert.Equal(17, projection.GoldAfterClaim);
    }

    [Fact]
    public void Behavioral_already_completed_path_correlates_reputation_and_gold_to_start_snapshot()
    {
        var fixture = Goal154DFixture.Create(startingHerbs: 4);
        var result = fixture.Qualify("already-correlation");
        var completion = CompletionSnapshot(result.Session);
        var projection = Project(fixture, result.Session, result.CheckpointReplay.Passed, result.FinalReplay.Passed);
        var advance = result.Session.ActionJournal.Single(item => item.ActionId == Goal154DFixture.AdvanceActionId);

        Assert.Equal("capability." + Goal154DFixture.StartActionId, completion.StepId);
        Assert.Equal("SKIPPED", advance.Status);
        Assert.False(advance.RuntimeExecuted);
        Assert.False(advance.RuntimeMutation);
        Assert.Equal(0, advance.RuntimeEventCount);
        Assert.Contains(advance.Diagnostics, item => item == "completedDuringAction=" + Goal154DFixture.StartActionId);
        Assert.True(projection.Passed, string.Join("; ", projection.Diagnostics) + " | "
            + string.Join("; ", Observations(fixture, result.Session).Select(item =>
                item.MetricKind + "=" + item.ActualValue + ":" + item.Passed)));
        Assert.Equal(10, projection.GoldAfterQuest);
        Assert.Equal(17, projection.GoldAfterClaim);
    }

    [Fact]
    public void Behavioral_unrelated_reputation_event_cannot_replace_causal_completion_transition()
    {
        var fixture = Goal154DFixture.Create(startingHerbs: 4);
        var result = fixture.Qualify("unrelated-reputation");
        var completion = CompletionSnapshot(result.Session);
        var transition = completion.RuntimeEvents.Single(item => item.EventType == "FactionReputationChanged");
        completion.RuntimeEvents = completion.RuntimeEvents.Where(item => item != transition).ToList();
        result.Session.CanonicalSession.Snapshots.Add(new CanonicalRuntimePlayerCommandLoopSnapshot
        {
            StepId = "capability.unrelated_reputation",
            RuntimeEvents = [transition]
        });

        var observation = Observations(fixture, result.Session).Single(item =>
            item.MetricKind == FeatureModuleRuntimeEffectMetricKinds.FactionReputationTransitionTruthful);

        Assert.False(observation.Passed);
        Assert.Contains(observation.Diagnostics, item => item.Contains("causal_event_counts", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_unrelated_resource_event_cannot_replace_quest_completion_gold()
    {
        var fixture = Goal154DFixture.Create(startingHerbs: 4);
        var result = fixture.Qualify("unrelated-resource");
        var completion = CompletionSnapshot(result.Session);
        var gold = completion.RuntimeEvents.Single(item => item.EventType == "ResourceChanged" && item.TargetId == "resource/gold");
        completion.RuntimeEvents = completion.RuntimeEvents.Where(item => item != gold).ToList();
        result.Session.CanonicalSession.Snapshots.Add(new CanonicalRuntimePlayerCommandLoopSnapshot
        {
            StepId = "capability.unrelated_resource",
            RuntimeEvents = [gold]
        });

        var projection = Project(fixture, result.Session, true, true);

        Assert.False(projection.Passed);
        Assert.Contains(projection.Diagnostics, item => item == "social.projection.quest_resource_transition_missing");
    }

    [Fact]
    public void Behavioral_missing_completion_event_fails_effect_truth_causally()
    {
        var fixture = Goal154DFixture.Create(startingHerbs: 4);
        var result = fixture.Qualify("missing-completion");
        var completion = CompletionSnapshot(result.Session);
        completion.RuntimeEvents = completion.RuntimeEvents.Where(item => item.EventType != "QuestCompleted").ToList();

        var observations = Observations(fixture, result.Session);

        Assert.False(observations.Single(item =>
            item.MetricKind == FeatureModuleRuntimeEffectMetricKinds.QuestStateEquals).Passed);
        Assert.False(observations.Single(item =>
            item.MetricKind == FeatureModuleRuntimeEffectMetricKinds.FactionReputationTransitionTruthful).Passed);
        Assert.Contains(observations.SelectMany(item => item.Diagnostics), item =>
            item.Contains("completion_snapshot_count=0", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("duplicate_completion_snapshot")]
    [InlineData("duplicate_quest_gold")]
    public void Behavioral_duplicate_causal_evidence_is_rejected(string scenario)
    {
        var fixture = Goal154DFixture.Create(startingHerbs: 4);
        var result = fixture.Qualify("duplicate-" + scenario);
        var completion = CompletionSnapshot(result.Session);
        if (scenario == "duplicate_completion_snapshot")
        {
            result.Session.CanonicalSession.Snapshots.Add(new CanonicalRuntimePlayerCommandLoopSnapshot
            {
                StepId = "capability.duplicate_completion",
                RuntimeEvents = completion.RuntimeEvents.Where(item =>
                    item.EventType is "QuestCompleted" or "QuestRewardGranted").ToList()
            });
            var observations = Observations(fixture, result.Session);
            Assert.Contains(observations.SelectMany(item => item.Diagnostics), item =>
                item.Contains("completion_snapshot_count=2", StringComparison.Ordinal));
        }
        else
        {
            var gold = completion.RuntimeEvents.Single(item =>
                item.EventType == "ResourceChanged" && item.TargetId == "resource/gold");
            completion.RuntimeEvents = [.. completion.RuntimeEvents, gold];
        }

        Assert.False(Project(fixture, result.Session, true, true).Passed);
    }

    private static CanonicalRuntimePlayerCommandLoopSnapshot CompletionSnapshot(RuntimeInteractiveSession session) =>
        Assert.Single(session.CanonicalSession.Snapshots.Where(snapshot => snapshot.RuntimeEvents.Any(item =>
            item.EventType == "QuestCompleted" && item.TargetId == Goal154DFixture.QuestId)));

    private static IReadOnlyList<FeatureModuleRuntimeEffectObservation> Observations(
        Goal154DFixture fixture,
        RuntimeInteractiveSession session) => new FeatureModuleRuntimeEffectEvaluator().Evaluate(
        fixture.Modules, session, new RuntimeInteractiveSession(), fixture.Package);

    private static GameProjectSocialSummary Project(
        Goal154DFixture fixture,
        RuntimeInteractiveSession session,
        bool checkpoint,
        bool replay) => new SocialRuntimeReviewProjectionService().Project(
        fixture.Modules, fixture.Package, fixture.Plan, session, Observations(fixture, session), checkpoint, replay);
}
