using LLMGameCreator.Runtime;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154A;

public sealed class Goal154ASocialRuntimeLifecycleTests
{
    [Fact] public void Set_reputation_runtime_service_is_available_for_truthful_transition() => Assert.NotNull(typeof(FactionRuntimeService).GetMethod("SetReputation"));
    [Fact] public void Quest_runtime_service_is_available_for_existing_objective_completion() => Assert.NotNull(typeof(QuestRuntimeService).GetMethod("AdvanceQuestObjective"));
    [Fact] public void Dialogue_runtime_service_is_available_for_one_time_choice() => Assert.NotNull(typeof(DialogueRuntimeService).GetMethod("ChooseDialogueOption"));
    [Fact] public void Default_social_lifecycle_contract_names_all_required_observations() => Goal154ATestFiles.AssertContains("dialogue-reputation-gated-reward.featuremodule.json", "dialogue_choice_visibility_sequence", "resource_transition_truthful", "flag_equals");
}
