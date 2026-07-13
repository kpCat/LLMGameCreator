using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154A;

public sealed class Goal154ASavedProjectStandaloneTests
{
    [Fact] public void Social_modules_remain_default_off_for_existing_projects() => Goal154ATestFiles.AssertContains("faction-reputation-standing.featuremodule.json", "\"defaultSelected\": false");
    [Fact] public void Social_modules_use_existing_product_ids_not_dummy_content() => Goal154ATestFiles.AssertContains("dialogue-reputation-gated-reward.featuremodule.json", "dialogue/healer", "resource/gold", "flag/village_trusted_reward_claimed");
}
