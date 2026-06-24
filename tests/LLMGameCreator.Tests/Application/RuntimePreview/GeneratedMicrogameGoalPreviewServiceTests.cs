using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class GeneratedMicrogameGoalPreviewServiceTests
{
    [Fact]
    public void GeneratedMicrogameGoalSelectsActiveQuestAndAdvancesOnInteraction()
    {
        var result = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());

        var goal = result.Snapshot.MicrogameGoal;
        Assert.True(goal.ActiveGoalSelected);
        Assert.False(string.IsNullOrWhiteSpace(goal.ActiveQuestId));
        Assert.False(string.IsNullOrWhiteSpace(goal.ActiveQuestTitle));
        Assert.False(string.IsNullOrWhiteSpace(goal.CurrentObjectiveText));
        Assert.True(goal.ProgressAdvancedByInteraction);
        Assert.Equal(1, goal.CompletedStepCount);
        Assert.True(goal.StepCount >= 2);
        Assert.False(string.IsNullOrWhiteSpace(goal.Related.ItemId));
        Assert.False(string.IsNullOrWhiteSpace(goal.Related.EncounterId));
        Assert.True(result.Report.ActiveGoalSelected);
        Assert.True(result.Report.GoalProgressAdvanced);
        Assert.Equal(1, result.Snapshot.Counts.ActiveGoals);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "generated_microgame_goal.preview_level_progress");
    }

    [Fact]
    public void GeneratedMicrogameGoalUsesExistingPreviewQuestJournalTracker()
    {
        var visible = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());
        var package = visible.PackageMvpResult.Package;
        var preview = visible.Snapshot.Projection;
        var tracker = new GeneratedQuestDialoguePreviewService();
        tracker.StartSession(package);
        var service = new GeneratedMicrogameGoalPreviewService();

        var started = service.EnsureActiveGoal(package, preview, tracker);
        var advanced = service.AdvanceAfterInteraction(package, preview, tracker, visible.Snapshot.RuntimeAttempt.CommandAttempts.Single(item => item.CommandType == "interact"));
        var journal = tracker.BuildJournal();

        Assert.True(started.ActiveGoalSelected);
        Assert.Equal(1, started.ActiveQuestCount);
        Assert.True(advanced.ProgressAdvancedByInteraction);
        Assert.Equal(1, advanced.CompletedStepCount);
        Assert.Equal(1, journal.ActiveCount);
        Assert.Contains("interact", advanced.LastProgressAction, StringComparison.OrdinalIgnoreCase);
    }

    private static VisibleGeneratedPlayablePreviewRequest CreateRequest() => new()
    {
        Seed = "generated-microgame-goal-tests",
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
