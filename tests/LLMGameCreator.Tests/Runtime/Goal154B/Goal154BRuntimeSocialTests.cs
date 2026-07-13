using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal154B;
using Xunit;

namespace LLMGameCreator.Tests.Runtime.Goal154B;

public sealed class Goal154BRuntimeSocialTests
{
    [Fact]
    public void Behavioral_direct_second_claim_is_atomic_and_emits_no_success_event()
    {
        var fixture = Goal154BFixture.Create();
        var runtime = Goal154BFixture.CreateGameRuntime();
        var state = runtime.CreateInitialState(fixture.Package).State;
        Assert.True(runtime.Execute(fixture.Package, state, GameRuntimeCommand.ChangeReputation("faction/village", 10)).Success);
        Assert.True(runtime.Execute(fixture.Package, state, GameRuntimeCommand.OpenDialogue("dialogue/healer")).Success);
        var first = runtime.Execute(fixture.Package, state, GameRuntimeCommand.ChooseDialogueOption("trusted_village_reward"));
        Assert.True(first.Success);
        Assert.True(runtime.Execute(fixture.Package, state, GameRuntimeCommand.OpenDialogue("dialogue/healer")).Success);
        var before = Goal154BFixture.Stable(state);

        var second = runtime.Execute(fixture.Package, state, GameRuntimeCommand.ChooseDialogueOption("trusted_village_reward"));

        Assert.False(second.Success);
        Assert.Equal(before, Goal154BFixture.Stable(state));
        Assert.Contains(second.Diagnostics, diagnostic => diagnostic.Code == "requirement.flag_match");
        Assert.DoesNotContain(second.Events, item => item.Type is GameRuntimeEventType.ResourceChanged
            or GameRuntimeEventType.OutputApplied or GameRuntimeEventType.DialogueChoiceSelected
            or GameRuntimeEventType.DialogueEffectApplied or GameRuntimeEventType.DialogueClosed);
    }

    [Fact]
    public void Behavioral_already_claimed_effect_observation_is_derived_from_contract_fields()
    {
        var fixture = Goal154BFixture.Create();
        var session = new SelectedRuntimeVariantInteractiveSession
        {
            CapabilityPlan = fixture.Plan,
            CanonicalSession = new CanonicalRuntimePlayerCommandLoopSession
            {
                RuntimeSession = new UnifiedRuntimeSession
                {
                    GameplayState = new GameRuntimeState
                    {
                        Flags = [new RuntimeFlagState { Id = "flag/village_trusted_reward_claimed", Value = "true" }]
                    }
                }
            }
        };
        var dialogueModule = fixture.SocialModules.Single(module => module.ModuleId == Goal154BFixture.DialogueModuleId);
        var observation = new FeatureModuleRuntimeEffectEvaluator().Evaluate([dialogueModule], session,
                new SelectedRuntimeVariantInteractiveSession(), fixture.Package)
            .Single(item => item.MetricKind == FeatureModuleRuntimeEffectMetricKinds.TrustedRewardSocialOutcome);

        Assert.Equal("already_claimed", observation.ActualValue);
        Assert.False(observation.Passed);
    }

    [Fact]
    public void Behavioral_positive_reputation_clamp_reports_requested_10_actual_5()
    {
        var fixture = Goal154BFixture.Create(startingReputation: 95, questReputationReward: 10);
        var runtime = Goal154BFixture.CreateGameRuntime();
        var state = runtime.CreateInitialState(fixture.Package).State;
        Assert.True(runtime.Execute(fixture.Package, state, GameRuntimeCommand.StartQuest("quest/help_healer")).Success);

        var result = runtime.Execute(fixture.Package, state,
            GameRuntimeCommand.AdvanceQuestObjective("quest/help_healer", "objective/collect_red_herbs", 10));
        var reputation = result.Events.Single(item => item.Type == GameRuntimeEventType.FactionReputationChanged);

        Assert.True(result.Success, string.Join("; ", result.Diagnostics.Select(item => item.Message)));
        Assert.Equal(100, state.Factions.Single(item => item.FactionId == "faction/village").Reputation);
        Assert.Equal("95", reputation.Args["before"]);
        Assert.Equal("10", reputation.Args["requested"]);
        Assert.Equal("100", reputation.Args["after"]);
        Assert.Equal("5", reputation.Args["delta"]);
        Assert.Equal("true", reputation.Args["clamped"]);
    }

    [Fact]
    public void Behavioral_negative_reputation_clamp_reports_requested_minus_10_actual_minus_5_and_failed_quest()
    {
        var fixture = Goal154BFixture.Create(startingReputation: -95, questFailurePenalty: 10);
        var runtime = Goal154BFixture.CreateGameRuntime();
        var state = runtime.CreateInitialState(fixture.Package).State;
        Assert.True(runtime.Execute(fixture.Package, state, GameRuntimeCommand.StartQuest("quest/help_healer")).Success);

        var result = runtime.Execute(fixture.Package, state,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.FailQuest, Id = "quest/help_healer" });
        var reputation = result.Events.Single(item => item.Type == GameRuntimeEventType.FactionReputationChanged);

        Assert.True(result.Success, string.Join("; ", result.Diagnostics.Select(item => item.Message)));
        Assert.Equal(-100, state.Factions.Single(item => item.FactionId == "faction/village").Reputation);
        Assert.Equal("failed", state.Quests.Single(item => item.QuestId == "quest/help_healer").State);
        Assert.Equal("-95", reputation.Args["before"]);
        Assert.Equal("-10", reputation.Args["requested"]);
        Assert.Equal("-100", reputation.Args["after"]);
        Assert.Equal("-5", reputation.Args["delta"]);
        Assert.Equal("true", reputation.Args["clamped"]);
    }

    [Fact]
    public void Behavioral_presentation_primitives_create_truthful_non_mutating_snapshots()
    {
        var execution = Goal154BFixture.Create().ExecuteActionByAction("presentation-snapshots");
        var presentation = execution.Session.ActionJournal.Where(item => item.Route == "presentation_only").ToList();

        Assert.NotEmpty(presentation);
        Assert.All(presentation, entry =>
        {
            Assert.False(entry.RuntimeExecuted);
            Assert.False(entry.RuntimeMutation);
            Assert.Equal(entry.StateHashBefore, entry.StateHashAfter);
            Assert.Equal(0, entry.RuntimeEventCount);
        });
        Assert.Contains(execution.Session.CanonicalSession.Snapshots,
            item => item.StepId == "presentation.inspect_initial_faction_reputation"
                    && item.FactionSummary.Contains("faction/village=0", StringComparison.Ordinal));
        Assert.Contains(execution.Session.CanonicalSession.Snapshots,
            item => item.StepId == "presentation.inspect_social_summary"
                    && item.SocialSummary.Contains("socialOutcome=claimed", StringComparison.Ordinal));
    }
}
