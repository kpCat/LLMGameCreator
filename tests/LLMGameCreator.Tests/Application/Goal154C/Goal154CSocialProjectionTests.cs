using System.Text;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal154B;
using Xunit;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Tests.Application.Goal154C;

public sealed class Goal154CSocialProjectionTests
{
    [Fact]
    public void Behavioral_default_claimed_projection_contains_typed_social_facts()
    {
        var social = Project(Goal154BFixture.Create(), "claimed-fields");
        Assert.True(social.Passed, string.Join(";", social.Diagnostics));
        Assert.Equal(0, social.ReputationBefore);
        Assert.Equal(10, social.ReputationAfter);
        Assert.Equal("completed", social.QuestState);
        Assert.Equal(["unavailable", "available", "unavailable"], social.ChoiceVisibilitySequence);
        Assert.Equal(0, social.GoldBefore);
        Assert.Equal(10, social.GoldAfterQuest);
        Assert.Equal(17, social.GoldAfterClaim);
        Assert.Equal(7, social.TrustedRewardDelta);
        Assert.True(social.RewardClaimed);
        Assert.False(social.RepeatRewardAvailable);
        Assert.Equal("claimed", social.SocialOutcome);
    }

    [Fact]
    public void Behavioral_locked_projection_is_green_without_a_claim_resource_transition()
    {
        var social = Project(Goal154BFixture.Create(trustedReputationThreshold: 20), "locked-fields");
        Assert.True(social.Passed, string.Join(";", social.Diagnostics));
        Assert.Equal(["unavailable", "unavailable", "unavailable"], social.ChoiceVisibilitySequence);
        Assert.Equal(10, social.GoldAfterQuest);
        Assert.Equal(10, social.GoldAfterClaim);
        Assert.Equal(0, social.TrustedRewardDelta);
        Assert.False(social.RewardClaimed);
        Assert.Equal("still_locked", social.SocialOutcome);
    }

    [Fact]
    public void Behavioral_custom_trusted_reward_projects_nineteen_gold()
    {
        var social = Project(Goal154BFixture.Create(trustedGoldReward: 9), "custom-nine");
        Assert.True(social.Passed, string.Join(";", social.Diagnostics));
        Assert.Equal(19, social.GoldAfterClaim);
        Assert.Equal(9, social.TrustedRewardDelta);
    }

    [Fact]
    public void Behavioral_projection_fails_causally_when_a_required_contract_is_missing()
    {
        var fixture = Goal154BFixture.Create();
        var modules = fixture.SocialModules.Select(module => module with
        {
            RuntimeEffectContracts = module.RuntimeEffectContracts.Where(contract =>
                contract.MetricKind != FeatureModuleRuntimeEffectMetricKinds.FactionReputationInitialized).ToList()
        }).ToList();
        var social = Project(fixture, "missing-contract", modules);
        Assert.False(social.Passed);
        Assert.Contains(social.Diagnostics, diagnostic => diagnostic.Contains("contract_missing:faction_reputation_initialized", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_projection_fails_causally_when_a_social_contract_is_ambiguous()
    {
        var fixture = Goal154BFixture.Create();
        var modules = fixture.SocialModules.Select(module => module.ModuleId == Goal154BFixture.FactionModuleId
            ? module with { RuntimeEffectContracts = module.RuntimeEffectContracts.Concat(module.RuntimeEffectContracts.Take(1)).ToList() }
            : module).ToList();
        var social = Project(fixture, "ambiguous-contract", modules);
        Assert.False(social.Passed);
        Assert.Contains(social.Diagnostics, diagnostic => diagnostic.Contains("contract_ambiguous:", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_partial_social_dependency_closure_has_no_social_card()
    {
        var fixture = Goal154BFixture.CreateSelected(Goal154BFixture.FactionModuleId, Goal154BFixture.QuestModuleId);
        var qualification = fixture.Qualify("partial-closure");
        var observations = new FeatureModuleRuntimeEffectEvaluator().Evaluate(fixture.SocialModules, qualification.Session,
            new RuntimeInteractiveSession(), fixture.Package);
        var social = new SocialRuntimeReviewProjectionService().Project(fixture.SocialModules, fixture.Package, fixture.Plan,
            qualification.Session, observations, qualification.CheckpointReplay.Passed, qualification.FinalReplay.Passed);
        Assert.True(social.Passed);
        Assert.False(social.Present);
    }

    [Fact]
    public void Behavioral_no_social_contracts_leave_the_historical_result_unchanged()
    {
        var fixture = Goal154BFixture.CreateSelected();
        var qualification = fixture.Qualify("no-social");
        var social = new SocialRuntimeReviewProjectionService().Project([], fixture.Package, fixture.Plan,
            qualification.Session, [], qualification.CheckpointReplay.Passed, qualification.FinalReplay.Passed);
        Assert.True(social.Passed);
        Assert.False(social.Present);
        Assert.Empty(SocialRuntimeReviewProjectionService.HumanSummaryLines(social));
    }

    [Fact]
    public void Behavioral_projection_requires_checkpoint_and_full_replay_evidence_from_real_qualification()
    {
        var social = Project(Goal154BFixture.Create(), "replay-evidence");
        Assert.True(social.CheckpointReplayPassed);
        Assert.True(social.FullReplayEquivalent);
    }

    [Fact]
    public void Behavioral_default_projection_is_deterministic_across_two_real_qualifications()
    {
        var fixture = Goal154BFixture.Create();
        var first = Project(fixture, "repeat-first");
        var second = Project(fixture, "repeat-second");
        Assert.Equal(first.HumanFacts, second.HumanFacts);
        Assert.Equal(first.GoldAfterClaim, second.GoldAfterClaim);
        Assert.Equal(first.SocialOutcome, second.SocialOutcome);
    }

    [Fact]
    public void Behavioral_custom_reward_changes_the_real_final_state_hash()
    {
        var standard = Goal154BFixture.Create().Qualify("hash-standard");
        var custom = Goal154BFixture.Create(trustedGoldReward: 9).Qualify("hash-custom");
        Assert.NotEqual(standard.Session.CurrentStateHash, custom.Session.CurrentStateHash);
    }

    [Fact]
    public void Behavioral_claimed_projection_uses_action_scoped_resource_observation()
    {
        var fixture = Goal154BFixture.Create();
        var qualification = fixture.Qualify("action-scope");
        var claim = qualification.Session.CanonicalSession.Snapshots.Single(snapshot => snapshot.StepId == "capability.claim_trusted_reward");
        claim.RuntimeEvents = claim.RuntimeEvents.Where(runtimeEvent => runtimeEvent.EventType != "ResourceChanged").ToList();
        var observations = new FeatureModuleRuntimeEffectEvaluator().Evaluate(fixture.SocialModules, qualification.Session,
            new RuntimeInteractiveSession(), fixture.Package);
        var social = new SocialRuntimeReviewProjectionService().Project(fixture.SocialModules, fixture.Package, fixture.Plan,
            qualification.Session, observations, qualification.CheckpointReplay.Passed, qualification.FinalReplay.Passed);
        Assert.False(social.Passed);
        Assert.Contains(social.Diagnostics, diagnostic => diagnostic.Contains("effect_failed:resource_transition_truthful", StringComparison.Ordinal));
    }

    [Fact]
    public void Behavioral_human_facts_are_actual_typed_values_not_module_or_hash_text()
    {
        var social = Project(Goal154BFixture.Create(), "human-values");
        var text = string.Join("\n", SocialRuntimeReviewProjectionService.HumanSummaryLines(social));
        Assert.Contains("Репутация: 0 → 10", text, StringComparison.Ordinal);
        Assert.Contains("Золото: 0 → 10 → 17", text, StringComparison.Ordinal);
        Assert.DoesNotContain("feature.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("sha", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Typed_build_result_and_snapshot_can_carry_the_last_successful_social_summary()
    {
        var social = Project(Goal154BFixture.Create(), "typed-result");
        var build = new GameProjectBuildResult { Passed = true, Status = "GREEN", Social = social };
        var snapshot = new UnifiedGameProjectWorkspaceSnapshot { Social = build.Social };
        Assert.Same(social, snapshot.Social);
    }

    [Fact]
    public void Evidence_text_integrity_rejects_nul_and_forbidden_controls()
    {
        Assert.True(IsEvidenceTextValid("Статус: GREEN\nРепутация: 0 → 10"));
        Assert.False(IsEvidenceTextValid("GREEN\0"));
        Assert.False(IsEvidenceTextValid("GREEN\u0001"));
    }

    private static GameProjectSocialSummary Project(
        Goal154BFixture fixture,
        string id,
        IReadOnlyList<FeatureModuleDefinition>? modules = null)
    {
        modules ??= fixture.SocialModules;
        var qualification = fixture.Qualify("goal154c-" + id);
        var observations = new FeatureModuleRuntimeEffectEvaluator().Evaluate(modules, qualification.Session,
            new RuntimeInteractiveSession(), fixture.Package);
        return new SocialRuntimeReviewProjectionService().Project(modules, fixture.Package, fixture.Plan,
            qualification.Session, observations, qualification.CheckpointReplay.Passed,
            qualification.FinalReplay.Passed && qualification.FinalReplay.ActualStateHash == qualification.Session.CurrentStateHash);
    }

    private static bool IsEvidenceTextValid(string value) => value.All(character =>
        character is '\r' or '\n' or '\t' || (character >= ' ' && character != '\u007f'));
}
