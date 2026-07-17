using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal165;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal166;

public sealed class Goal166DefeatWithoutCheckpointTests
{
    [Fact] public void Behavioral_defeat_without_checkpoint_keeps_recovery_available() => Assert.True(new GeneratedCampaignRecoveryService().Project(GeneratedCampaignSessionStatus.DEFEATED, true, string.Empty).Available);
    [Fact] public void Behavioral_defeat_without_checkpoint_disables_retry() => Assert.False(new GeneratedCampaignRecoveryService().Project(GeneratedCampaignSessionStatus.DEFEATED, true, string.Empty).RetryEnabled);
    [Fact] public void Behavioral_defeat_without_checkpoint_keeps_continue() => Assert.True(new GeneratedCampaignRecoveryService().Project(GeneratedCampaignSessionStatus.DEFEATED, true, string.Empty).ContinueEnabled);
    [Fact] public void Behavioral_defeat_without_checkpoint_keeps_new_game() => Assert.True(new GeneratedCampaignRecoveryService().Project(GeneratedCampaignSessionStatus.DEFEATED, false, "Нет сохранения").NewGameEnabled);
    [Fact] public void Behavioral_defeat_without_checkpoint_has_human_retry_reason() => Assert.DoesNotContain("campaign.", new GeneratedCampaignRecoveryService().Project(GeneratedCampaignSessionStatus.DEFEATED, true, string.Empty).DisabledReason, StringComparison.OrdinalIgnoreCase);
}
