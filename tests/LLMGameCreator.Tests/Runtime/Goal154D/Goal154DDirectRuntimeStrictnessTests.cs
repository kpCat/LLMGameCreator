using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Tests.Application.Goal154B;
using LLMGameCreator.Tests.Application.Goal154D;
using Xunit;

namespace LLMGameCreator.Tests.Runtime.Goal154D;

public sealed class Goal154DDirectRuntimeStrictnessTests
{
    [Fact]
    public void Behavioral_direct_runtime_still_rejects_advance_after_refresh_completed_quest()
    {
        var fixture = Goal154DFixture.Create(startingHerbs: 4);
        var runtime = Goal154BFixture.CreateGameRuntime();
        var state = runtime.CreateInitialState(fixture.Package).State;
        Assert.True(runtime.Execute(fixture.Package, state,
            GameRuntimeCommand.StartQuest(Goal154DFixture.QuestId)).Success);
        var refresh = runtime.Execute(fixture.Package, state,
            new GameRuntimeCommand { Type = GameRuntimeCommandType.RefreshQuestObjectives });
        Assert.True(refresh.Success);
        Assert.Equal("completed", state.Quests.Single(item => item.QuestId == Goal154DFixture.QuestId).State);
        var before = Goal154BFixture.Stable(state);

        var advance = runtime.Execute(fixture.Package, state,
            GameRuntimeCommand.AdvanceQuestObjective(Goal154DFixture.QuestId, Goal154DFixture.ObjectiveId, 10));

        Assert.False(advance.Success);
        Assert.Contains(advance.Diagnostics, item => item.Code == "quest.not_active");
        Assert.Equal(before, Goal154BFixture.Stable(state));
        Assert.DoesNotContain(advance.Events, item => item.Type is GameRuntimeEventType.QuestObjectiveUpdated
            or GameRuntimeEventType.QuestCompleted or GameRuntimeEventType.QuestRewardGranted);
    }

    [Theory]
    [InlineData("missing_runtime_quest")]
    [InlineData("failed_quest")]
    [InlineData("completed_incomplete_objective")]
    [InlineData("completed_without_events")]
    [InlineData("ambiguous_runtime_quest")]
    [InlineData("missing_runtime_objective")]
    public void Behavioral_invalid_quest_states_fail_causally_and_never_skip(string scenario)
    {
        var fixture = Goal154DFixture.Create(startingHerbs: 4);
        var beforeAdvance = fixture.ExecuteBeforeAdvance();
        var state = beforeAdvance.Session.CanonicalSession.RuntimeSession.GameplayState;
        var quest = state.Quests.Single(item => item.QuestId == Goal154DFixture.QuestId);
        switch (scenario)
        {
            case "missing_runtime_quest":
                state.Quests.Clear();
                break;
            case "failed_quest":
                quest.State = "failed";
                break;
            case "completed_incomplete_objective":
                quest.Objectives.Single(item => item.ObjectiveId == Goal154DFixture.ObjectiveId).Completed = false;
                break;
            case "completed_without_events":
                foreach (var snapshot in beforeAdvance.Session.CanonicalSession.Snapshots)
                    snapshot.RuntimeEvents = snapshot.RuntimeEvents.Where(item =>
                        item.EventType is not "QuestCompleted" and not "QuestRewardGranted").ToList();
                break;
            case "ambiguous_runtime_quest":
                state.Quests.Add(quest);
                break;
            case "missing_runtime_objective":
                quest.Objectives.Clear();
                break;
        }

        var result = beforeAdvance.ExecuteAdvance(fixture.Package);

        Assert.Equal("REJECTED", result.Status);
        Assert.NotEqual("SKIPPED", result.Status);
        Assert.Contains(result.Diagnostics, item => item.Contains("quest_advance_state_invalid", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics, item => item.Contains("actionId=", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics, item => item.Contains("questId=", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics, item => item.Contains("objectiveId=", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics, item => item.Contains("observedQuestState=", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics, item => item.Contains("priorCompletionEventCount=", StringComparison.Ordinal));
        Assert.Contains(result.Diagnostics, item => item.Contains("priorRewardEventCount=", StringComparison.Ordinal));
    }
}
