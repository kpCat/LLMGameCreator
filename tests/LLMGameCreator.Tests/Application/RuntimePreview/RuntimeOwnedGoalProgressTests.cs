using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class RuntimeOwnedGoalProgressTests
{
    [Fact]
    public void RuntimeOwnedGoalProgressInitializesAndAdvancesFromInteraction()
    {
        var result = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());
        var goal = result.Snapshot.MicrogameGoal;

        Assert.True(goal.ActiveGoalSelected);
        Assert.True(goal.ProgressAdvancedByInteraction);
        Assert.Equal("runtime_state_quests", goal.ProgressStateSource);
        Assert.False(goal.FallbackPreviewJournalUsed);
        Assert.False(string.IsNullOrWhiteSpace(goal.RuntimeQuestId));
        Assert.False(string.IsNullOrWhiteSpace(goal.RuntimeObjectiveId));
        Assert.Equal(1, goal.RuntimeObjectiveCurrentAmount);
        Assert.True(goal.RuntimeObjectiveRequiredAmount >= 2);
        Assert.Equal(goal.RuntimeQuestId, goal.RuntimeState.Quests.Single().QuestId);
        Assert.Equal(goal.RuntimeObjectiveId, goal.RuntimeState.Quests.Single().Objectives.Single().ObjectiveId);
        Assert.Equal("runtime_state_quests", goal.RuntimeState.Metadata["generated_microgame_goal.progress_source"]);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "generated_microgame_goal.runtime_state_progress");
    }

    [Fact]
    public void RuntimeOwnedGoalProgressSurvivesRuntimeStateSerialization()
    {
        var result = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());
        var goal = result.Snapshot.MicrogameGoal;
        var serializer = new RuntimeStateSerializer();

        var json = serializer.Serialize(goal.RuntimeState);
        var restored = serializer.DeserializeGameRuntimeState(json);
        var restoredQuest = restored.Quests.Single();
        var restoredObjective = restoredQuest.Objectives.Single();

        Assert.Equal(goal.RuntimeQuestId, restoredQuest.QuestId);
        Assert.Equal(goal.RuntimeObjectiveId, restoredObjective.ObjectiveId);
        Assert.Equal(goal.RuntimeObjectiveCurrentAmount, restoredObjective.CurrentAmount);
        Assert.Equal(goal.RuntimeObjectiveRequiredAmount, restoredObjective.RequiredAmount);
        Assert.Equal("runtime_state_quests", restored.Metadata["generated_microgame_goal.progress_source"]);
    }

    [Fact]
    public void RuntimeOwnedGoalProgressDoesNotSilentlyUsePreviewFallback()
    {
        var result = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());
        var goal = result.Snapshot.MicrogameGoal;

        Assert.True(result.Report.GoalProgressAdvanced);
        Assert.Equal("runtime_state_quests", goal.ProgressStateSource);
        Assert.False(goal.FallbackPreviewJournalUsed);
        Assert.DoesNotContain(goal.Diagnostics, item => item.Code == "generated_microgame_goal.preview_journal_fallback");
    }

    private static VisibleGeneratedPlayablePreviewRequest CreateRequest() => new()
    {
        Seed = "runtime-owned-goal-progress-tests",
        Mode = ProceduralGameGenerationModes.SemiProceduralRegions,
        CompactStyleHintIds =
        [
            "theme/exploration",
            "theme/survival",
            "tone/mysterious",
            "quest_motif/faction_truce",
            "item_affordance/quest_item"
        ],
        SelectedVariantIds =
        [
            "world_topology/region_graph",
            "actor_model/single_player_character",
            "combat_model/turn_based",
            "inventory_model/list_inventory"
        ]
    };

    private sealed class DefaultRuntimeAdapter : IVisibleGeneratedPlayableRuntimeAdapter
    {
        public VisibleGeneratedPlayableRuntimeAttempt Run(GamePackageDefinition package)
        {
            var runtime = new DefaultGameRuntime();
            var start = runtime.Start(package);
            var startPosition = new VisibleGeneratedPlayablePosition
            {
                X = start.State.PlayerPosition.X,
                Y = start.State.PlayerPosition.Y
            };
            var eventTypes = new SortedSet<string>(start.Events.Select(item => item.Type.ToString()), StringComparer.Ordinal);
            var commandAttempts = new List<VisibleGeneratedPlayableRuntimeCommandAttempt>();
            var currentState = start.State;

            if (start.Success)
            {
                var move = runtime.Execute(package, currentState, PlayerCommand.Move(Direction2D.Right));
                currentState = move.State;
                commandAttempts.Add(ToAttempt("01_move_right", "move/right", move));
                foreach (var eventType in move.Events.Select(item => item.Type.ToString()))
                {
                    eventTypes.Add(eventType);
                }

                var interact = runtime.Execute(package, currentState, PlayerCommand.Interact());
                currentState = interact.State;
                commandAttempts.Add(ToAttempt("02_interact", "interact", interact));
                foreach (var eventType in interact.Events.Select(item => item.Type.ToString()))
                {
                    eventTypes.Add(eventType);
                }
            }

            return new VisibleGeneratedPlayableRuntimeAttempt
            {
                RuntimeStartAttempted = true,
                RuntimeStartSucceeded = start.Success,
                StartMapId = package.Manifest.StartMapId,
                CurrentMapId = currentState.CurrentMapId,
                PlayerStartPosition = startPosition,
                PlayerCurrentPosition = new VisibleGeneratedPlayablePosition
                {
                    X = currentState.PlayerPosition.X,
                    Y = currentState.PlayerPosition.Y
                },
                CommandAttempts = commandAttempts,
                EventTypes = eventTypes.ToList()
            };
        }

        private static VisibleGeneratedPlayableRuntimeCommandAttempt ToAttempt(
            string commandId,
            string commandType,
            CommandResult result) => new()
        {
            CommandId = commandId,
            CommandType = commandType,
            Succeeded = result.Success,
            CurrentMapId = result.State.CurrentMapId,
            PlayerPosition = new VisibleGeneratedPlayablePosition
            {
                X = result.State.PlayerPosition.X,
                Y = result.State.PlayerPosition.Y
            },
            EventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EventTargets = result.Events.Select(item => item.TargetId ?? string.Empty).Where(value => !string.IsNullOrWhiteSpace(value)).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EventMessages = result.Events.Select(item => item.Message).Where(value => !string.IsNullOrWhiteSpace(value)).OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }
}
