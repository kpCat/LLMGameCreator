using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Application.Design.FeatureModuleComposition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal154B;
using LLMGameCreator.WinForms.Pages;
using Xunit;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Tests.Application.Goal154C1;

public sealed class Goal154C1PersistedLastSuccessTests
{
    [Fact]
    public void Behavioral_history_reader_restores_newest_matching_green_social_summary()
    {
        using var temp = new TempDirectory();
        Write(temp.Path, "a.json", Entry("2026-07-14T10:00:00Z"));
        Write(temp.Path, "z.json", Entry("2026-07-14T11:00:00Z", 19));
        var result = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, Document());
        Assert.Equal(19, result.LastSuccessfulBuild?.Social?.GoldAfterClaim);
    }

    [Fact]
    public void Behavioral_history_reader_uses_filename_ordinal_order_for_equal_timestamps()
    {
        using var temp = new TempDirectory();
        Write(temp.Path, "a.json", Entry("2026-07-14T10:00:00Z", 17));
        Write(temp.Path, "b.json", Entry("2026-07-14T10:00:00Z", 19));
        var result = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, Document());
        Assert.Equal(19, result.LastSuccessfulBuild?.Social?.GoldAfterClaim);
    }

    [Fact]
    public void Behavioral_history_reader_skips_failed_malformed_stale_and_failed_social_entries()
    {
        using var temp = new TempDirectory();
        Directory.CreateDirectory(History(temp.Path));
        File.WriteAllText(Path.Combine(History(temp.Path), "bad.json"), "{");
        Write(temp.Path, "failed.json", Entry("2026-07-14T12:00:00Z") with { Status = "FAILED", AttemptStatus = "FAILED" });
        Write(temp.Path, "stale.json", Entry("2026-07-14T12:01:00Z") with { PackageSha256 = "stale" });
        Write(temp.Path, "social-failed.json", Entry("2026-07-14T12:02:00Z") with { Social = Social() with { Passed = false } });
        var result = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, Document());
        Assert.Null(result.LastSuccessfulBuild);
        Assert.Contains(result.Diagnostics, item => item.StartsWith("social.history.invalid_json:", StringComparison.Ordinal));
        Assert.Contains("social.history.no_matching_green_social_success", result.Diagnostics);
    }

    [Fact]
    public void Behavioral_history_reader_handles_missing_history_without_crash()
    {
        using var temp = new TempDirectory();
        var result = new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, Document());
        Assert.Null(result.LastSuccessfulBuild);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Behavioral_projection_populates_choice_text_from_package_data()
    {
        var fixture = Goal154BFixture.Create();
        var social = Project(fixture, "choice-text");
        var expected = fixture.Package.Game.Dialogues.Single(dialogue => dialogue.Id == "dialogue/healer").Nodes
            .Single(node => node.Id == "start").Choices.Single(choice => choice.Id == "trusted_village_reward").Text;
        Assert.Equal(expected, social.ChoiceText);
        Assert.False(string.IsNullOrWhiteSpace(social.ChoiceText));
    }

    [Fact]
    public void Behavioral_projection_requires_checkpoint_replay_for_complete_social_success()
    {
        var social = Project(Goal154BFixture.Create(), "checkpoint", checkpoint: false);
        Assert.False(social.Passed);
        Assert.Contains("social.projection.checkpoint_replay_failed", social.Diagnostics);
    }

    [Fact]
    public void Behavioral_projection_requires_full_replay_for_complete_social_success()
    {
        var social = Project(Goal154BFixture.Create(), "full-replay", fullReplay: false);
        Assert.False(social.Passed);
        Assert.Contains("social.projection.full_replay_not_equivalent", social.Diagnostics);
    }

    [Fact]
    public void Behavioral_claimed_human_facts_show_reward_and_repeat_row()
    {
        var facts = Project(Goal154BFixture.Create(), "claimed-facts").HumanFacts;
        Assert.Contains(facts, fact => fact.Label == "Награда за доверие" && fact.Value == "+7");
        Assert.Contains(facts, fact => fact.Label == "Повторная награда" && fact.Value == "недоступна");
    }

    [Fact]
    public void Behavioral_locked_human_facts_omit_repeat_reward_row()
    {
        var facts = Project(Goal154BFixture.Create(trustedReputationThreshold: 20), "locked-facts").HumanFacts;
        Assert.Contains(facts, fact => fact.Label == "Награда за доверие" && fact.Value == "пока недоступна");
        Assert.DoesNotContain(facts, fact => fact.Label == "Повторная награда");
    }

    [Fact]
    public void Behavioral_custom_reward_projects_nineteen_gold_and_preserves_other_social_facts()
    {
        var standard = Project(Goal154BFixture.Create(), "standard");
        var custom = Project(Goal154BFixture.Create(trustedGoldReward: 9), "custom");
        Assert.Equal(19, custom.GoldAfterClaim);
        Assert.Equal(9, custom.TrustedRewardDelta);
        Assert.Equal(standard.ReputationAfter, custom.ReputationAfter);
        Assert.Equal(standard.ChoiceVisibilitySequence, custom.ChoiceVisibilitySequence);
    }

    [Fact]
    public void Behavioral_locked_projection_retains_baseline_quest_gold()
    {
        var social = Project(Goal154BFixture.Create(trustedReputationThreshold: 20), "locked-gold");
        Assert.True(social.Passed, string.Join(";", social.Diagnostics));
        Assert.Equal(10, social.GoldAfterQuest);
        Assert.Equal(10, social.GoldAfterClaim);
    }

    [Fact]
    public void Behavioral_partial_social_closure_remains_absent_not_failed()
    {
        var fixture = Goal154BFixture.CreateSelected(Goal154BFixture.FactionModuleId, Goal154BFixture.QuestModuleId);
        var qualification = fixture.Qualify("partial");
        var observations = new FeatureModuleRuntimeEffectEvaluator().Evaluate(fixture.SocialModules, qualification.Session,
            new RuntimeInteractiveSession(), fixture.Package);
        var social = new SocialRuntimeReviewProjectionService().Project(fixture.SocialModules, fixture.Package, fixture.Plan,
            qualification.Session, observations, qualification.CheckpointReplay.Passed, qualification.FinalReplay.Passed);
        Assert.True(social.Passed);
        Assert.False(social.Present);
    }

    [Fact]
    public void Behavioral_default_off_social_result_remains_absent()
    {
        var fixture = Goal154BFixture.CreateSelected();
        var qualification = fixture.Qualify("default-off");
        var social = new SocialRuntimeReviewProjectionService().Project([], fixture.Package, fixture.Plan,
            qualification.Session, [], qualification.CheckpointReplay.Passed, qualification.FinalReplay.Passed);
        Assert.True(social.Passed);
        Assert.False(social.Present);
    }

    [Fact]
    public void Behavioral_reader_never_accepts_missing_replay_binding_guards()
    {
        using var temp = new TempDirectory();
        Write(temp.Path, "checkpoint.json", Entry("2026-07-14T10:00:00Z") with { CheckpointReloadPassed = false });
        Write(temp.Path, "replay.json", Entry("2026-07-14T10:01:00Z") with { FullReplayEquivalent = false });
        Write(temp.Path, "binding.json", Entry("2026-07-14T10:02:00Z") with { ActionBindingPassed = false });
        Assert.Null(new GameProjectBuildHistoryReader().ReadLatestMatchingSocialSuccess(temp.Path, Document()).LastSuccessfulBuild);
    }

    [Fact]
    public void Behavioral_winforms_claimed_card_binds_actual_typed_facts_without_ids_or_hashes()
    {
        RunSta(() =>
        {
            using var page = new ProjectsPageControl();
            Bind(page, new UnifiedGameProjectWorkspaceSnapshot { Social = Social(), Dirty = false });
            var label = Field<Label>(page, "_socialCardLabel");
            Assert.Contains("Репутация", label.Text, StringComparison.Ordinal);
            Assert.Contains("0 → 10", label.Text, StringComparison.Ordinal);
            Assert.DoesNotContain("feature.", label.Text, StringComparison.Ordinal);
            Assert.DoesNotContain("sha", label.Text, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void Behavioral_winforms_dirty_card_is_explicitly_last_successful()
    {
        RunSta(() =>
        {
            using var page = new ProjectsPageControl();
            Bind(page, new UnifiedGameProjectWorkspaceSnapshot { Social = Social(), Dirty = false, SocialConfigurationStatus = "LAST_SUCCESS" });
            Assert.Contains("последняя успешная проверка", Field<Label>(page, "_socialCardLabel").Text, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Behavioral_new_evidence_text_rejects_nul_and_forbidden_controls()
    {
        Assert.True(IsEvidenceTextValid("Статус: GREEN\nРепутация: 0 → 10"));
        Assert.False(IsEvidenceTextValid("GREEN\0"));
        Assert.False(IsEvidenceTextValid("GREEN\u0001"));
    }

    private static GameProjectSocialSummary Social() => Project(Goal154BFixture.Create(), "history-social");

    private static GameProjectSocialSummary Project(Goal154BFixture fixture, string id, bool checkpoint = true, bool fullReplay = true)
    {
        var qualification = fixture.Qualify("goal154c1-" + id);
        var observations = new FeatureModuleRuntimeEffectEvaluator().Evaluate(fixture.SocialModules, qualification.Session,
            new RuntimeInteractiveSession(), fixture.Package);
        return new SocialRuntimeReviewProjectionService().Project(fixture.SocialModules, fixture.Package, fixture.Plan,
            qualification.Session, observations, checkpoint && qualification.CheckpointReplay.Passed,
            fullReplay && qualification.FinalReplay.Passed && qualification.FinalReplay.ActualStateHash == qualification.Session.CurrentStateHash);
    }

    private static FeatureModuleCompositionDocument Document() => new()
    {
        LastActivatedProjectPackageSha256 = "package", LastCompositionPackageSha256 = "composition",
        LastQualifiedFinalStateHash = "final", LastQualificationStatus = "GREEN"
    };

    private static GameProjectBuildHistoryEntry Entry(string completedAt, decimal gold = 17) => new()
    {
        CompletedAtUtc = DateTimeOffset.Parse(completedAt), Status = "GREEN", AttemptStatus = "GREEN",
        PackageSha256 = "package", ActivatedProjectPackageSha256 = "package", CompositionPackageSha256 = "composition",
        FinalStateHash = "final", CheckpointReloadPassed = true, FullReplayEquivalent = true, ActionBindingPassed = true,
        Social = Social() with { GoldAfterClaim = gold }
    };

    private static string History(string project) => Path.Combine(project, ".llmgc", "build-history");
    private static void Write(string project, string name, GameProjectBuildHistoryEntry entry)
    {
        Directory.CreateDirectory(History(project));
        File.WriteAllText(Path.Combine(History(project), name), JsonSerializer.Serialize(entry));
    }
    private static bool IsEvidenceTextValid(string value) => value.All(character => character is '\r' or '\n' or '\t' || (character >= ' ' && character != '\u007f'));
    private static void Bind(ProjectsPageControl page, UnifiedGameProjectWorkspaceSnapshot snapshot) =>
        page.GetType().GetMethod("BindWorkspace", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, [snapshot]);
    private static T Field<T>(object target, string name) where T : class =>
        (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;
    private static void RunSta(Action action)
    {
        Exception? captured = null;
        var thread = new Thread(() => { try { action(); } catch (Exception exception) { captured = exception; } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); thread.Join();
        if (captured is not null) throw new Xunit.Sdk.XunitException(captured.ToString());
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path); }
        public string Path { get; }
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
    }
}
