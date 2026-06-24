using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class GeneratedMicrogameChallengePreviewServiceTests
{
    [Fact]
    public void GeneratedMicrogameChallengeIsDeterministicAndLinkedToActiveGoal()
    {
        var service = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter());
        var first = service.Generate(CreateRequest());
        var second = service.Generate(CreateRequest());

        Assert.Equal(first.SnapshotJson, second.SnapshotJson);
        Assert.True(first.Snapshot.MicrogameChallenge.ChallengeSelected);
        Assert.Equal(first.Snapshot.MicrogameGoal.ActiveQuestId, first.Snapshot.MicrogameChallenge.QuestId);
        Assert.Equal(first.Snapshot.MicrogameGoal.Related.EncounterId, first.Snapshot.MicrogameChallenge.EncounterId);
        Assert.Equal(first.Snapshot.MicrogameGoal.Related.ItemId, first.Snapshot.MicrogameChallenge.RewardItemId);
        Assert.False(string.IsNullOrWhiteSpace(first.Snapshot.MicrogameChallenge.RelatedNpcId));
        Assert.Contains(first.Report.Diagnostics, item => item.Code == "generated_microgame_challenge.no_external_execution");
    }

    [Fact]
    public void GeneratedMicrogameChallengeResolveShowsRewardAndCompletionEvidence()
    {
        var result = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());
        var challenge = result.Snapshot.MicrogameChallenge;

        Assert.True(challenge.Resolved);
        Assert.True(challenge.RewardVisible);
        Assert.True(challenge.CompletionVisible);
        Assert.Equal("completed", challenge.CompletionStatus);
        Assert.Equal(challenge.StepCount, challenge.CompletedStepCount);
        Assert.Contains("interact", challenge.ResolveAction, StringComparison.OrdinalIgnoreCase);
        Assert.True(result.Report.ChallengeResolved);
        Assert.True(result.Report.RewardVisible);
        Assert.True(result.Report.CompletionVisible);
        Assert.Equal(1, result.Snapshot.Counts.ResolvedChallenges);
        Assert.Equal(1, result.Snapshot.Counts.VisibleRewards);
        Assert.Equal(1, result.Snapshot.Counts.VisibleCompletions);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "generated_microgame_challenge.preview_level_resolution");
    }

    private static VisibleGeneratedPlayablePreviewRequest CreateRequest() => new()
    {
        Seed = "generated-microgame-challenge-tests",
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
