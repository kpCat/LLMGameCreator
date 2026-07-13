using LLMGameCreator.Application.Design.CapabilityDrivenRuntimePlaythrough;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.ProductLineRuntimeQualification;
using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal153C;

public sealed class Goal153COutcomeAwareQualificationTests
{
    [Fact]
    public void Default_values_tick_real_hostile_five_times_and_expire()
    {
        var fixture = Goal153CFixture.Create();
        var result = Qualify(fixture, "default-expiry");
        Assert.True(result.CheckpointReplay.Passed, string.Join("; ", result.CheckpointReplay.Diagnostics));
        Assert.True(result.FinalReplay.Passed, string.Join("; ", result.FinalReplay.Diagnostics));
        var events = result.Session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();
        Assert.Equal(5, events.Count(item => item.EventType == "StatusTicked"
                                             && item.TargetId == "goblin"
                                             && item.Args.GetValueOrDefault("statusId") == "status/arcane_burn"));
        Assert.Contains(events, item => item.EventType == "StatusRemoved" && item.TargetId == "goblin"
                                       && item.Message.Contains("expired", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("expired", TerminalObservation(fixture, result));
    }

    [Fact]
    public void Lethal_direct_damage_uses_real_terminal_outcome_and_skips_remaining_turns_replay_stably()
    {
        var fixture = Goal153CFixture.Create(abilityDamage: 1000);
        var result = Qualify(fixture, "lethal-direct");
        AssertLethalAndSkipped(fixture, result);
    }

    [Fact]
    public void Lethal_tick_damage_ends_encounter_without_post_end_turn_mutation()
    {
        var fixture = Goal153CFixture.Create(tickDamage: 1000);
        var result = Qualify(fixture, "lethal-tick");
        AssertLethalAndSkipped(fixture, result);
        var events = result.Session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();
        Assert.Single(events.Where(item => item.EventType == "StatusTicked" && item.TargetId == "goblin"));
    }

    private static void AssertLethalAndSkipped(Goal153CFixture fixture, ProductLineRuntimeQualificationResult result)
    {
        Assert.True(result.CheckpointReplay.Passed, string.Join("; ", result.CheckpointReplay.Diagnostics));
        Assert.True(result.FinalReplay.Passed, string.Join("; ", result.FinalReplay.Diagnostics));
        Assert.Equal("target_defeated", TerminalObservation(fixture, result));
        var events = result.Session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();
        Assert.Contains(events, item => item.EventType == "ParticipantDefeated" && item.TargetId == "goblin");
        Assert.Contains(events, item => item.EventType == "EncounterEnded");
        var skipped = result.Session.ActionJournal.Where(entry => entry.Route == "conditional_skip").ToList();
        Assert.NotEmpty(skipped);
        Assert.All(skipped, entry =>
        {
            Assert.False(entry.RuntimeExecuted);
            Assert.False(entry.RuntimeMutation);
            Assert.Equal(0, entry.RuntimeEventCount);
            Assert.Equal(entry.StateHashBefore, entry.StateHashAfter);
            Assert.Contains("terminal_outcome=", entry.CommandKind, StringComparison.Ordinal);
        });
        Assert.DoesNotContain(result.Session.CanonicalSession.Snapshots.Where(snapshot =>
            snapshot.DiagnosticSummary.StartsWith("conditional action skipped", StringComparison.Ordinal))
            .SelectMany(snapshot => snapshot.RuntimeEvents), _ => true);
    }

    private static string TerminalObservation(
        Goal153CFixture fixture,
        ProductLineRuntimeQualificationResult result)
    {
        var baseline = Goal153CFixture.Create();
        return new FeatureModuleRuntimeEffectEvaluator().Evaluate(fixture.Modules, result.Session,
                Qualify(baseline, "baseline-for-observation").Session)
            .Single(observation => observation.MetricKind == FeatureModuleRuntimeEffectMetricKinds.StatusTerminalOutcome)
            .ActualValue;
    }

    private static ProductLineRuntimeQualificationResult Qualify(Goal153CFixture fixture, string id)
    {
        var plan = new CapabilityDrivenRuntimePlaythroughPlanner().Plan(fixture.Modules, fixture.Package);
        return new ProductLineRuntimeQualifier(SelectedRuntimeVariantInteractiveSessionService.CreateDefault()).Qualify(
            fixture.Package,
            new ProductLineRuntimeQualificationRequest
            {
                SessionId = "goal153c-" + id,
                CandidateId = "goal153c",
                VariantKind = id,
                PackagePath = "in-memory/package.json",
                PackageSha256 = new string('c', 64),
                CheckpointId = "goal153c-checkpoint-" + id,
                FinalCheckpointId = "goal153c-final-" + id,
                CapabilityPlan = plan
            });
    }
}
