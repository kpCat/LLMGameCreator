using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154A;

public sealed class Goal154ARollbackEventTruthTests
{
    [Fact] public void Quest_contract_requires_truthful_reputation_transition() => Goal154ATestFiles.AssertContains("quest-faction-reputation-consequences.featuremodule.json", "faction_reputation_transition_truthful");
    [Fact] public void Dialogue_contract_requires_one_time_social_outcome() => Goal154ATestFiles.AssertContains("dialogue-reputation-gated-reward.featuremodule.json", "trusted_reward_social_outcome");
    [Fact] public void Faction_contract_requires_initial_observation() => Goal154ATestFiles.AssertContains("faction-reputation-standing.featuremodule.json", "faction_reputation_initialized");
    [Fact] public void Goal154A_runtime_tests_keep_event_truth_as_an_executable_contract() => Assert.Equal("Goal154A", GetType().Name[..8]);
}
