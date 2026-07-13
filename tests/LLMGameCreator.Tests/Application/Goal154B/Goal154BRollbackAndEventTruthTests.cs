using System.Globalization;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal154B;

public sealed class Goal154BRollbackAndEventTruthTests
{
    [Fact]
    public void Behavioral_quest_completion_rolls_back_reputation_when_later_resource_output_fails()
    {
        var fixture = Goal154BFixture.Create();
        var package = Goal154BFixture.ClonePackage(fixture.Package);
        var quest = package.Game.Quests.Single(item => item.Id == "quest/help_healer");
        quest.Rewards =
        [
            new OutputDefinition { Kind = "reputation", Id = "faction/village", Amount = 10 },
            new OutputDefinition { Kind = "resource", Id = "resource/missing", Amount = 1 }
        ];
        var service = Quest();
        var state = new GameRuntimeStateFactory().CreateInitialState(package).State;
        Assert.True(service.StartQuest(package, state, quest.Id).Success);
        var before = Goal154BFixture.Stable(state);

        var result = service.CompleteQuest(package, state, quest.Id);

        AssertRolledBack(before, state, result);
    }

    [Fact]
    public void Behavioral_quest_failure_rolls_back_negative_reputation_when_later_output_fails()
    {
        var fixture = Goal154BFixture.Create();
        var package = Goal154BFixture.ClonePackage(fixture.Package);
        var quest = package.Game.Quests.Single(item => item.Id == "quest/help_healer");
        quest.FailureEffects =
        [
            new OutputDefinition { Kind = "reputation", Id = "faction/village", Amount = -5 },
            new OutputDefinition { Kind = "resource", Id = "resource/missing", Amount = 1 }
        ];
        var service = Quest();
        var state = new GameRuntimeStateFactory().CreateInitialState(package).State;
        Assert.True(service.StartQuest(package, state, quest.Id).Success);
        var before = Goal154BFixture.Stable(state);

        var result = service.FailQuest(package, state, quest.Id);

        AssertRolledBack(before, state, result);
    }

    [Fact]
    public void Behavioral_dialogue_outputs_roll_back_flag_when_later_resource_output_fails()
    {
        var fixture = Goal154BFixture.Create();
        var package = Goal154BFixture.ClonePackage(fixture.Package);
        var choice = TrustedChoice(package);
        choice.Rewards = [new OutputDefinition { Kind = "resource", Id = "resource/missing", Amount = 1 }];
        var service = Dialogue();
        var state = new GameRuntimeStateFactory().CreateInitialState(package).State;
        state.Factions.Single(item => item.FactionId == "faction/village").Reputation = 10;
        Assert.True(service.OpenDialogue(package, state, "dialogue/healer").Success);
        var before = Goal154BFixture.Stable(state);

        var result = service.ChooseDialogueOption(package, state, choice.Id, "inventory/player_start");

        AssertRolledBack(before, state, result);
    }

    [Fact]
    public void Behavioral_nested_dialogue_action_failure_rolls_back_earlier_flag_output()
    {
        var fixture = Goal154BFixture.Create();
        var package = Goal154BFixture.ClonePackage(fixture.Package);
        var choice = TrustedChoice(package);
        choice.Rewards.Clear();
        choice.StartQuestId = "quest/missing";
        var service = Dialogue();
        var state = new GameRuntimeStateFactory().CreateInitialState(package).State;
        state.Factions.Single(item => item.FactionId == "faction/village").Reputation = 10;
        Assert.True(service.OpenDialogue(package, state, "dialogue/healer").Success);
        var before = Goal154BFixture.Stable(state);

        var result = service.ChooseDialogueOption(package, state, choice.Id, "inventory/player_start");

        AssertRolledBack(before, state, result);
    }

    [Fact]
    public void Behavioral_social_event_numeric_arguments_are_invariant_under_comma_decimal_culture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("uk-UA");
            CultureInfo.CurrentUICulture = new CultureInfo("uk-UA");
            var execution = Goal154BFixture.Create(startingReputation: 95).ExecuteActionByAction("culture");
            var numericKeys = new HashSet<string>(StringComparer.Ordinal)
            {
                "before", "requested", "after", "delta", "requestedDelta", "actualDelta"
            };
            var social = Goal154BClaimedAndLockedLifecycleTests.Events(execution.Session).Where(item =>
                item.EventType == "FactionReputationChanged"
                || item.EventType == "ResourceChanged" && item.StepId == "capability.claim_trusted_reward").ToList();
            Assert.NotEmpty(social);
            foreach (var runtimeEvent in social)
            foreach (var pair in runtimeEvent.Args.Where(pair => numericKeys.Contains(pair.Key)))
            {
                Assert.DoesNotContain(',', pair.Value);
                Assert.True(decimal.TryParse(pair.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out _),
                    runtimeEvent.EventType + ":" + pair.Key + "=" + pair.Value);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    private static QuestRuntimeService Quest() => new(new RequirementEvaluator(), new OutputApplier());

    private static DialogueRuntimeService Dialogue()
    {
        var requirements = new RequirementEvaluator();
        var outputs = new OutputApplier();
        var costs = new CostConsumer();
        var quest = new QuestRuntimeService(requirements, outputs);
        var transaction = new TransactionRuntimeService(requirements, costs, outputs);
        var encounter = new EncounterRuntimeService(requirements, outputs);
        return new DialogueRuntimeService(requirements, costs, outputs, quest, transaction, encounter);
    }

    private static DialogueChoiceDefinition TrustedChoice(LLMGameCreator.GamePackage.GamePackageDefinition package) =>
        package.Game.Dialogues.Single(item => item.Id == "dialogue/healer").Nodes
            .Single(item => item.Id == "start").Choices.Single(item => item.Id == "trusted_village_reward");

    private static void AssertRolledBack(string before, GameRuntimeState state, GameRuntimeResult result)
    {
        Assert.False(result.Success);
        Assert.Equal(before, Goal154BFixture.Stable(state));
        Assert.Single(result.Events);
        Assert.Equal(GameRuntimeEventType.ValidationFailed, result.Events[0].Type);
        Assert.DoesNotContain(result.Events, item => item.Type is GameRuntimeEventType.FactionReputationChanged
            or GameRuntimeEventType.ResourceChanged or GameRuntimeEventType.OutputApplied
            or GameRuntimeEventType.QuestCompleted or GameRuntimeEventType.QuestFailed
            or GameRuntimeEventType.DialogueChoiceSelected or GameRuntimeEventType.DialogueEffectApplied);
    }
}
