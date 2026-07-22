using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Tests.Application.Goal164;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal167;

[Collection(LLMGameCreator.Tests.Application.Goal160.Goal160Collection.Name)]
public sealed class Goal167BranchRuntimeQualificationTests
{
    private static GameProjectGeneratedCampaignChoiceSummary Choices =>
        Assert.IsType<GameProjectGeneratedCampaignChoiceSummary>(Goal164TestKit.AllSelectable.Build.GeneratedCampaignChoices);

    [Fact]
    public void Behavioral_v5_choice_summary_is_current() =>
        Assert.True(Choices is { Present: true, Passed: true, Status: "CHOICE_CURRENT" },
            string.Join(",", Choices.Diagnostics));

    [Fact]
    public void Behavioral_v5_primary_route_belongs_to_choices() =>
        Assert.Equal("generated-campaign-choice-v1", Goal164TestKit.AllSelectable.Build.RuntimePlaythroughPlanId);

    [Fact]
    public void Behavioral_v5_primary_final_state_is_choice_final_state() =>
        Assert.Equal(Choices.FinalStateHash, Goal164TestKit.AllSelectable.Build.FinalStateHash);

    [Fact]
    public void Behavioral_combat_summary_remains_separately_valid()
    {
        var build = Goal164TestKit.AllSelectable.Build;
        Assert.True(build.GeneratedEncounterCombat is { Passed: true, Status: "CAMPAIGN_CURRENT" });
        Assert.NotEqual(build.GeneratedEncounterCombat!.FinalStateHash, Choices.FinalStateHash);
    }

    [Fact]
    public void Behavioral_combat_and_choices_reference_exact_final_package()
    {
        var build = Goal164TestKit.AllSelectable.Build;
        Assert.Equal(build.PackageSha256, Choices.FinalPackageSha256);
        Assert.Equal(build.PackageSha256, build.GeneratedEncounterCombat!.ExactPackageSha256);
    }

    [Fact]
    public void Behavioral_every_branch_has_two_independent_replays() =>
        Assert.All(Choices.RuntimeFrames.GroupBy(item => (item.DialogueId, item.BranchKind)), group =>
            Assert.Equal(new[] { 1, 2 }, group.Select(item => item.ReplayIndex).OrderBy(item => item)));

    [Fact]
    public void Behavioral_independent_replays_have_equal_before_and_after_states() =>
        Assert.All(Choices.RuntimeFrames.GroupBy(item => (item.DialogueId, item.BranchKind)), group =>
        {
            Assert.Single(group.Select(item => item.BeforeStateHash).Distinct(StringComparer.Ordinal));
            Assert.Single(group.Select(item => item.StateHash).Distinct(StringComparer.Ordinal));
        });

    [Fact]
    public void Behavioral_replay_commands_are_ordered_and_hashed() =>
        Assert.All(Choices.RuntimeFrames, frame =>
        {
            Assert.Equal(new[] { "OpenDialogue", "ChooseDialogueOption" }, frame.Commands);
            Assert.False(string.IsNullOrWhiteSpace(frame.CommandSha256));
        });

    [Fact]
    public void Behavioral_replay_events_are_ordered_and_hashed() =>
        Assert.All(Choices.RuntimeFrames, frame =>
        {
            Assert.Contains("DialogueChoiceSelected", frame.Events);
            Assert.Contains("DialogueClosed", frame.Events);
            Assert.False(string.IsNullOrWhiteSpace(frame.EventSha256));
        });

    [Fact]
    public void Behavioral_branch_flag_truth_matches_branch_kind() =>
        Assert.All(Choices.RuntimeFrames, frame => Assert.Equal(frame.BranchKind.ToString(), frame.FlagValue));

    [Fact]
    public void Behavioral_alternatives_lock_is_proven_from_flag_truth() =>
        Assert.All(Choices.RuntimeFrames, frame => Assert.True(frame.AlternativesLocked));

    [Fact]
    public void Behavioral_support_has_exact_positive_reputation()
    {
        var binding = Choices.Overlay!.Bindings.SelectMany(item => item.Branches)
            .Single(item => item.Kind == GeneratedCampaignBranchKind.SUPPORT);
        Assert.All(Choices.RuntimeFrames.Where(item => item.BranchKind == GeneratedCampaignBranchKind.SUPPORT),
            frame =>
            {
                Assert.True(frame.ReputationDelta > 0);
                Assert.Equal(binding.ReputationAmount, frame.ReputationDelta);
                Assert.Equal(frame.ReputationBefore + binding.ReputationAmount, frame.ReputationAfter);
            });
    }

    [Fact]
    public void Behavioral_support_activates_exact_quest() =>
        Assert.All(Choices.RuntimeFrames.Where(item => item.BranchKind == GeneratedCampaignBranchKind.SUPPORT),
            frame =>
            {
                Assert.Equal("not_started", frame.QuestStateBefore);
                Assert.Equal("active", frame.QuestState);
            });

    [Fact]
    public void Behavioral_support_proves_active_and_completed_followups_after_combat_turn_in() =>
        Assert.Equal("True", Choices.TechnicalDetails["supportRuntimePassed"]);

    [Fact]
    public void Behavioral_challenge_starts_an_exact_active_encounter() =>
        Assert.All(Choices.RuntimeFrames.Where(item => item.BranchKind == GeneratedCampaignBranchKind.CHALLENGE),
            frame => Assert.NotEqual(frame.EncounterStateBefore, frame.EncounterState));

    [Fact]
    public void Behavioral_challenge_followup_is_checked_after_flee() =>
        Assert.True(Choices.ChallengeFleeFollowUpPassed);

    [Fact]
    public void Behavioral_challenge_followup_is_checked_after_victory() =>
        Assert.True(Choices.ChallengeVictoryFollowUpPassed);

    [Fact]
    public void Behavioral_refuse_has_exact_negative_reputation()
    {
        var binding = Choices.Overlay!.Bindings.SelectMany(item => item.Branches)
            .Single(item => item.Kind == GeneratedCampaignBranchKind.REFUSE);
        Assert.All(Choices.RuntimeFrames.Where(item => item.BranchKind == GeneratedCampaignBranchKind.REFUSE),
            frame =>
            {
                Assert.True(frame.ReputationDelta < 0);
                Assert.Equal(binding.ReputationAmount, frame.ReputationDelta);
            });
    }

    [Fact]
    public void Behavioral_refuse_does_not_mutate_quest_state() =>
        Assert.All(Choices.RuntimeFrames.Where(item => item.BranchKind == GeneratedCampaignBranchKind.REFUSE),
            frame => Assert.Equal(frame.QuestStateBefore, frame.QuestState));

    [Fact]
    public void Behavioral_refuse_does_not_mutate_encounter_state() =>
        Assert.All(Choices.RuntimeFrames.Where(item => item.BranchKind == GeneratedCampaignBranchKind.REFUSE),
            frame => Assert.Equal(frame.EncounterStateBefore, frame.EncounterState));

    [Fact]
    public void Behavioral_failing_choice_rolls_back_gameplay_state()
    {
        Assert.True(Choices.AtomicRollbackPassed);
        Assert.Equal(Choices.RollbackBeforeStateHash, Choices.RollbackAfterStateHash);
    }

    [Fact]
    public void Behavioral_failing_choice_does_not_mutate_package() =>
        Assert.Equal(Choices.RollbackPackageBeforeSha256, Choices.RollbackPackageAfterSha256);

    [Fact]
    public void Behavioral_failing_choice_exposes_only_validation_failure_event() =>
        Assert.Equal(new[] { "ValidationFailed" }, Choices.RollbackEventTypes);

    [Fact]
    public void Behavioral_all_runtime_frames_pass() =>
        Assert.All(Choices.RuntimeFrames, frame => Assert.True(frame.Passed));
}
