using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal165;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal166;

public sealed class Goal166SaveNewGameRecoveryTests
{
    [Fact] public void Behavioral_checkpoint_restore_keeps_exact_session_hash() { var harness = Goal165RecoveryHarness.WithCheckpoint(); Assert.Equal(Goal165RecoveryHarness.SessionHash(harness.PreEncounter), Goal165RecoveryHarness.SessionHash(harness.Recovery.Restore(harness.Truth, harness.Package).Session!)); }
    [Fact] public void Behavioral_recovery_load_action_remains_distinct() => Assert.Equal(GeneratedCampaignActionKind.RecoveryLoad, Goal165RecoveryHarness.WithCheckpoint().Recovery.RecoveryActions(Goal165RecoveryHarness.WithCheckpoint().Recovery.Project(true, string.Empty))[1].Kind);
    [Fact] public void Behavioral_new_game_action_remains_available_after_defeat() => Assert.True(new GeneratedCampaignRecoveryService().Project(GeneratedCampaignSessionStatus.DEFEATED, false, "Нет сохранения").NewGameEnabled);
    [Fact] public void Behavioral_new_game_recovery_does_not_require_checkpoint() => Assert.True(new GeneratedCampaignRecoveryService().Project(GeneratedCampaignSessionStatus.DEFEATED, false, "Нет сохранения").Available);
    [Fact] public void Behavioral_recovery_checkpoint_is_in_memory_only() { var harness = Goal165RecoveryHarness.WithCheckpoint(); Assert.NotNull(harness.Recovery.Checkpoint); Assert.Null(new GeneratedCampaignRecoveryService().Checkpoint); }
}
