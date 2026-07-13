using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154A;

public sealed class Goal154AParameterDomainTests
{
    [Fact] public void Starting_reputation_is_a_bounded_integer() => Goal154ATestFiles.AssertContains("faction-reputation-standing.featuremodule.json", "\"minimum\":-100", "\"maximum\":100", "\"step\":1");
    [Fact] public void Quest_reward_and_failure_penalty_are_bounded_integers() => Goal154ATestFiles.AssertContains("quest-faction-reputation-consequences.featuremodule.json", "questReputationReward", "questFailurePenalty", "\"step\":1");
    [Fact] public void Trusted_threshold_is_not_cross_parameter_rejected() => Goal154ATestFiles.AssertContains("dialogue-reputation-gated-reward.featuremodule.json", "trustedReputationThreshold", "\"parameterConstraints\": []");
    [Fact] public void Gold_reward_allows_zero_and_has_upper_bound() => Goal154ATestFiles.AssertContains("dialogue-reputation-gated-reward.featuremodule.json", "trustedGoldReward", "\"minimum\":0", "\"maximum\":1000");
}
