using LLMGameCreator.Application.Play.GeneratedCampaign;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal165;

public sealed class Goal165SaveNewGameRecoveryTests
{
    [Fact]
    public void Behavioral_defeated_current_save_can_restore_exact_checkpoint_state()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        var restored = harness.Recovery.Restore(harness.Truth, harness.Package);

        Assert.True(restored.Passed);
        Assert.Equal(Goal165RecoveryHarness.SessionHash(harness.PreEncounter),
            Goal165RecoveryHarness.SessionHash(restored.Session!));
    }

    [Fact]
    public void Behavioral_migration_required_save_remains_explicit()
    {
        var projection = Goal165RecoveryHarness.WithCheckpoint().Recovery.Project(
            canContinue: false, continueReason: "Сохранение требует явного переноса в текущий мир.");

        Assert.False(projection.ContinueEnabled);
        Assert.Contains("переноса", projection.DisabledReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_successful_continue_clears_checkpoint()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        harness.Recovery.Clear();

        Assert.Null(harness.Recovery.Checkpoint);
    }

    [Fact]
    public void Behavioral_new_game_recovery_action_is_enabled()
    {
        var projection = Goal165RecoveryHarness.WithCheckpoint().Recovery.Project(false,
            "Нет совместимого сохранения для продолжения.");

        Assert.True(projection.NewGameEnabled);
    }

    [Fact]
    public void Behavioral_new_game_clears_checkpoint()
    {
        var harness = Goal165RecoveryHarness.WithCheckpoint();
        harness.Recovery.Clear();

        Assert.Null(harness.Recovery.Checkpoint);
    }

    [Fact]
    public void Behavioral_no_save_continue_is_disabled_with_human_reason()
    {
        var projection = Goal165RecoveryHarness.WithCheckpoint().Recovery.Project(false,
            "Нет совместимого сохранения для продолжения.");

        Assert.False(projection.ContinueEnabled);
        Assert.DoesNotContain("campaign.", projection.DisabledReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Behavioral_recovery_actions_have_truthful_distinct_kinds()
    {
        var actions = Goal165RecoveryHarness.WithCheckpoint().Recovery.RecoveryActions(
            Goal165RecoveryHarness.WithCheckpoint().Recovery.Project(true, string.Empty));

        Assert.Equal(
        [
            GeneratedCampaignActionKind.RetryEncounter,
            GeneratedCampaignActionKind.RecoveryLoad,
            GeneratedCampaignActionKind.NewGame
        ], actions.Select(action => action.Kind));
    }

    [Fact]
    public void Contract_failed_recovery_has_no_positive_projection()
    {
        var projection = new GeneratedCampaignRecoveryService().Project(false,
            "Нет сохранённой точки перед встречей.");

        Assert.False(projection.Available);
        Assert.False(projection.NewGameEnabled);
    }

    [Fact]
    public void Behavioral_checkpoint_does_not_survive_new_service_instance()
    {
        var previous = Goal165RecoveryHarness.WithCheckpoint();
        var restarted = new GeneratedCampaignRecoveryService();

        Assert.NotNull(previous.Recovery.Checkpoint);
        Assert.Null(restarted.Checkpoint);
    }
}
