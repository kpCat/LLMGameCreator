using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Tests.Application.Goal154B;

public sealed class Goal154BClaimedAndLockedLifecycleTests
{
    [Fact]
    public void Behavioral_default_0_10_5_10_7_composition_executes_claimed_lifecycle()
    {
        var fixture = Goal154BFixture.Create(0, 10, 5, 10, 7);
        var result = fixture.Qualify("claimed-default");
        var state = result.Session.CanonicalSession.RuntimeSession.GameplayState;

        Assert.Equal(10, state.Factions.Single(item => item.FactionId == "faction/village").Reputation);
        Assert.Equal("completed", state.Quests.Single(item => item.QuestId == "quest/help_healer").State);
        Assert.Equal(17, Goal154BFixture.Gold(state));
        Assert.Equal("true", state.Flags.Single(item => item.Id == "flag/village_trusted_reward_claimed").Value);
        Assert.Single(Events(result.Session), item => item.EventType == "DialogueChoiceSelected"
                                                     && item.TargetId == "trusted_village_reward");
        Assert.Equal("claimed", result.Session.LatestSnapshot.SocialSummary.Split(';')
            .Single(item => item.StartsWith("socialOutcome=", StringComparison.Ordinal)).Split('=')[1]);

        var observations = new FeatureModuleRuntimeEffectEvaluator().Evaluate(fixture.SocialModules,
            result.Session, new RuntimeInteractiveSession(), fixture.Package);
        Assert.All(observations, observation => Assert.True(observation.Passed,
            observation.EffectId + ":" + observation.ActualValue + ":" + string.Join(";", observation.Diagnostics)));
    }

    [Fact]
    public void Behavioral_interactive_actions_open_healer_and_observe_unavailable_available_unavailable()
    {
        var execution = Goal154BFixture.Create().ExecuteActionByAction("visibility");
        var actionIds = execution.Session.ActionJournal.Select(item => item.ActionId).ToList();
        Assert.True(actionIds.IndexOf("open_healer_before_quest") < actionIds.IndexOf("advance_healer_objective"));
        Assert.True(actionIds.IndexOf("open_healer_after_quest") > actionIds.IndexOf("advance_healer_objective"));
        Assert.Equal(["unavailable", "available", "unavailable"], Visibility(execution.Session));
        Assert.Contains(execution.Session.CanonicalSession.Snapshots,
            snapshot => snapshot.StepId == "presentation.inspect_trusted_choice_before_quest"
                        && snapshot.StateHashBefore == snapshot.StateHashAfter
                        && snapshot.RuntimeEvents.Count == 0);
        Assert.Single(Events(execution.Session), item => item.EventType == "DialogueChoiceSelected");
    }

    [Fact]
    public void Behavioral_claimed_checkpoint_reload_and_full_replay_match_hashes_and_social_events()
    {
        var fixture = Goal154BFixture.Create();
        var result = fixture.Qualify("claimed-replay");

        Assert.True(result.CheckpointReplay.Passed, string.Join("; ", result.CheckpointReplay.Diagnostics));
        Assert.True(result.FinalReplay.Passed, string.Join("; ", result.FinalReplay.Diagnostics));
        Assert.Equal(result.CheckpointReplay.ExpectedStateHash, result.CheckpointReplay.ActualStateHash);
        Assert.Equal(result.FinalReplay.ExpectedStateHash, result.FinalReplay.ActualStateHash);
        Assert.Equal("advance_healer_objective",
            fixture.Plan.OrderedActions.Take(result.CheckpointActionCount).Last().ActionId);

        var uninterrupted = fixture.ExecuteActionByAction("uninterrupted-events");
        var final = uninterrupted.Service.SaveCheckpoint(uninterrupted.Session, "goal154b-final-events", "2026-07-13T00:00:00Z");
        var replay = uninterrupted.Service.ReloadCheckpoint(fixture.Package, uninterrupted.Start, final);
        Assert.True(replay.Passed, string.Join("; ", replay.Diagnostics));
        Assert.Equal(uninterrupted.Session.CurrentStateHash, replay.Session.CurrentStateHash);
        Assert.Equal(SocialEvents(uninterrupted.Session), SocialEvents(replay.Session));
    }

    [Fact]
    public void Behavioral_threshold_20_truthfully_skips_claim_without_gold_or_flag_mutation()
    {
        var fixture = Goal154BFixture.Create(trustedReputationThreshold: 20);
        var execution = fixture.ExecuteActionByAction("still-locked");
        var state = execution.Session.CanonicalSession.RuntimeSession.GameplayState;
        var claim = execution.Results.Single(item => item.ActionId == "claim_trusted_reward");

        Assert.Equal("completed", state.Quests.Single(item => item.QuestId == "quest/help_healer").State);
        Assert.Equal(10, state.Factions.Single(item => item.FactionId == "faction/village").Reputation);
        Assert.Equal("SKIPPED", claim.Status);
        Assert.False(claim.RuntimeExecuted);
        Assert.False(claim.RuntimeMutation);
        Assert.Equal(0, claim.RuntimeEventCount);
        Assert.Equal(claim.StateHashBefore, claim.StateHashAfter);
        Assert.Contains(claim.Diagnostics, item => item.Contains("requirement.reputation_too_low", StringComparison.Ordinal));
        Assert.Equal(10, Goal154BFixture.Gold(state));
        Assert.DoesNotContain(state.Flags, item => item.Id == "flag/village_trusted_reward_claimed"
                                                  && item.Value.Equals("true", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(["unavailable", "unavailable", "unavailable"], Visibility(execution.Session));
        Assert.Contains("socialOutcome=still_locked", execution.Session.LatestSnapshot.SocialSummary, StringComparison.Ordinal);
        Assert.DoesNotContain(Events(execution.Session), item => item.EventType is "DialogueChoiceSelected" or "ResourceChanged"
                                                               && item.StepId == "capability.claim_trusted_reward");
    }

    [Fact]
    public void Behavioral_still_locked_checkpoint_and_full_replay_repeat_the_same_skip_decision()
    {
        var fixture = Goal154BFixture.Create(trustedReputationThreshold: 20);
        var result = fixture.Qualify("still-locked-replay");

        Assert.True(result.CheckpointReplay.Passed, string.Join("; ", result.CheckpointReplay.Diagnostics));
        Assert.True(result.FinalReplay.Passed, string.Join("; ", result.FinalReplay.Diagnostics));
        Assert.Equal(result.FinalReplay.ExpectedStateHash, result.FinalReplay.ActualStateHash);
        var claim = result.Session.ActionJournal.Single(item => item.ActionId == "claim_trusted_reward");
        Assert.Equal("SKIPPED", claim.Status);
        Assert.Equal(claim.StateHashBefore, claim.StateHashAfter);
        var observations = new FeatureModuleRuntimeEffectEvaluator().Evaluate(fixture.SocialModules,
            result.Session, new RuntimeInteractiveSession(), fixture.Package);
        Assert.All(observations, observation => Assert.True(observation.Passed,
            observation.EffectId + ":" + observation.ActualValue + ":" + string.Join(";", observation.Diagnostics)));
        Assert.Equal("still_locked", observations.Single(item =>
            item.MetricKind == FeatureModuleRuntimeEffectMetricKinds.TrustedRewardSocialOutcome).ActualValue);
    }

    [Fact]
    public void Behavioral_zero_gold_reward_claims_once_sets_flag_and_keeps_gold_zero()
    {
        var fixture = Goal154BFixture.Create(trustedGoldReward: 0);
        var execution = fixture.ExecuteActionByAction("zero-gold");
        var state = execution.Session.CanonicalSession.RuntimeSession.GameplayState;
        var rewardEvent = Events(execution.Session).Last(item => item.EventType == "ResourceChanged"
                                                               && item.TargetId == "resource/gold");

        Assert.Equal(10, Goal154BFixture.Gold(state));
        Assert.Equal("true", state.Flags.Single(item => item.Id == "flag/village_trusted_reward_claimed").Value);
        Assert.Equal("0", rewardEvent.Args["requestedDelta"]);
        Assert.Equal("0", rewardEvent.Args["actualDelta"]);
        Assert.Single(Events(execution.Session), item => item.EventType == "DialogueChoiceSelected");
    }

    [Fact]
    public void Behavioral_evidence_projection_is_derived_from_claimed_and_locked_runtime_sessions()
    {
        var claimedFixture = Goal154BFixture.Create();
        var claimed = claimedFixture.Qualify("evidence-claimed");
        var lockedFixture = Goal154BFixture.Create(trustedReputationThreshold: 20);
        var locked = lockedFixture.Qualify("evidence-locked");
        var claimedState = claimed.Session.CanonicalSession.RuntimeSession.GameplayState;
        var lockedState = locked.Session.CanonicalSession.RuntimeSession.GameplayState;

        Assert.True(claimed.CheckpointReplay.Passed && claimed.FinalReplay.Passed);
        Assert.True(locked.CheckpointReplay.Passed && locked.FinalReplay.Passed);
        Assert.Equal(17, Goal154BFixture.Gold(claimedState));
        Assert.Equal(10, Goal154BFixture.Gold(lockedState));
        Assert.Equal("SKIPPED", locked.Session.ActionJournal.Single(item =>
            item.ActionId == "claim_trusted_reward").Status);

        var evidencePath = Environment.GetEnvironmentVariable("LLMGC_GOAL154B_RUNTIME_PROOF_PATH");
        if (string.IsNullOrWhiteSpace(evidencePath)) return;
        var proof = new
        {
            status = "GREEN",
            capabilityPlanSignature = claimed.CanonicalActionPlanSignature,
            plannedActionCount = claimed.PlannedActionCount,
            checkpointActionCount = claimed.CheckpointActionCount,
            checkpointActionId = claimedFixture.Plan.OrderedActions.Take(claimed.CheckpointActionCount).Last().ActionId,
            basePackageSha256 = Goal154BFixture.Hash(claimedFixture.BasePackageJson),
            activatedPackageSha256 = Goal154BFixture.Hash(claimedFixture.PackageJson),
            activatedMutationOperationCount = claimedFixture.Binding.EffectiveMutationOperations.Count,
            claimed = new
            {
                finalStateHash = claimed.Session.CurrentStateHash,
                checkpointExpectedStateHash = claimed.CheckpointReplay.ExpectedStateHash,
                checkpointActualStateHash = claimed.CheckpointReplay.ActualStateHash,
                finalReplayExpectedStateHash = claimed.FinalReplay.ExpectedStateHash,
                finalReplayActualStateHash = claimed.FinalReplay.ActualStateHash,
                checkpointReloadPassed = claimed.CheckpointReplay.Passed,
                fullReplayEquivalent = claimed.FinalReplay.Passed,
                reputation = claimedState.Factions.Single(item => item.FactionId == "faction/village").Reputation,
                questState = claimedState.Quests.Single(item => item.QuestId == "quest/help_healer").State,
                gold = Goal154BFixture.Gold(claimedState),
                flag = claimedState.Flags.Single(item => item.Id == "flag/village_trusted_reward_claimed").Value,
                socialEventSignature = SocialEvents(claimed.Session)
            },
            stillLocked = new
            {
                finalStateHash = locked.Session.CurrentStateHash,
                checkpointExpectedStateHash = locked.CheckpointReplay.ExpectedStateHash,
                checkpointActualStateHash = locked.CheckpointReplay.ActualStateHash,
                finalReplayExpectedStateHash = locked.FinalReplay.ExpectedStateHash,
                finalReplayActualStateHash = locked.FinalReplay.ActualStateHash,
                checkpointReloadPassed = locked.CheckpointReplay.Passed,
                fullReplayEquivalent = locked.FinalReplay.Passed,
                reputation = lockedState.Factions.Single(item => item.FactionId == "faction/village").Reputation,
                questState = lockedState.Quests.Single(item => item.QuestId == "quest/help_healer").State,
                gold = Goal154BFixture.Gold(lockedState),
                claimFlagPresent = lockedState.Flags.Any(item => item.Id == "flag/village_trusted_reward_claimed"
                    && item.Value.Equals("true", StringComparison.OrdinalIgnoreCase)),
                socialEventSignature = SocialEvents(locked.Session)
            }
        };
        var parent = Path.GetDirectoryName(evidencePath);
        if (!string.IsNullOrWhiteSpace(parent)) Directory.CreateDirectory(parent);
        File.WriteAllText(evidencePath, JsonSerializer.Serialize(proof, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    internal static IReadOnlyList<string> Visibility(RuntimeInteractiveSession session) =>
        session.CanonicalSession.Snapshots.Where(snapshot =>
                snapshot.StepId.StartsWith("presentation.", StringComparison.Ordinal)
                && Regex.IsMatch(snapshot.DialogueChoicesSummary,
                    @"(?:^|;\s*)trusted_village_reward=(available|unavailable)"))
            .Select(snapshot => Regex.Match(snapshot.DialogueChoicesSummary,
                @"(?:^|;\s*)trusted_village_reward=(?<value>available|unavailable)").Groups["value"].Value)
            .ToList();

    internal static IReadOnlyList<CanonicalRuntimePlayerCommandLoopRuntimeEvent> Events(
        RuntimeInteractiveSession session) =>
        session.CanonicalSession.Snapshots.SelectMany(snapshot => snapshot.RuntimeEvents).ToList();

    private static IReadOnlyList<string> SocialEvents(RuntimeInteractiveSession session) =>
        Events(session).Where(item => item.EventType is "FactionReputationChanged" or "QuestCompleted"
                or "DialogueChoiceSelected" or "ResourceChanged" or "OutputApplied")
            .Select(item => item.EventType + "|" + item.TargetId + "|"
                            + string.Join(",", item.Args.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                                .Select(pair => pair.Key + "=" + pair.Value)))
            .ToList();
}
