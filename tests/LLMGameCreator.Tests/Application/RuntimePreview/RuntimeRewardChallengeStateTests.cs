using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class RuntimeRewardChallengeStateTests
{
    [Fact]
    public void RuntimeRewardChallengeStateRecordsResolutionRewardAndCompletionEvidence()
    {
        var result = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());
        var challenge = result.Snapshot.MicrogameChallenge;

        Assert.True(challenge.Resolved);
        Assert.True(challenge.RewardVisible);
        Assert.True(challenge.CompletionVisible);
        Assert.Equal("runtime_state_flags_inventory_encounter", challenge.StateSource);
        Assert.True(challenge.RuntimeChallengeResolved);
        Assert.True(challenge.RuntimeRewardGranted);
        Assert.True(challenge.RuntimeCompletionBacked);
        Assert.False(challenge.FallbackPreviewProjectionUsed);
        Assert.Equal(challenge.EncounterId, challenge.RuntimeEncounterId);
        Assert.Equal(challenge.RewardItemId, challenge.RuntimeRewardItemId);
        Assert.Contains(challenge.RuntimeState.Flags, flag => flag.Id == challenge.RuntimeChallengeFlagId && flag.Value == "true");
        Assert.Contains(challenge.RuntimeState.Inventories.SelectMany(inventory => inventory.Stacks), stack => stack.ItemId == challenge.RuntimeRewardItemId && stack.Amount == 1);
        Assert.NotNull(challenge.RuntimeState.ActiveEncounter);
        Assert.False(challenge.RuntimeState.ActiveEncounter!.Active);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "generated_microgame_challenge.runtime_state_evidence");
    }

    [Fact]
    public void RuntimeRewardChallengeStateSurvivesRuntimeStateSerialization()
    {
        var result = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());
        var challenge = result.Snapshot.MicrogameChallenge;
        var serializer = new RuntimeStateSerializer();

        var json = serializer.Serialize(challenge.RuntimeState);
        var restored = serializer.DeserializeGameRuntimeState(json);

        Assert.Contains(restored.Flags, flag => flag.Id == challenge.RuntimeChallengeFlagId && flag.Value == "true");
        Assert.Contains(restored.Inventories.SelectMany(inventory => inventory.Stacks), stack => stack.ItemId == challenge.RuntimeRewardItemId && stack.Amount == 1);
        Assert.NotNull(restored.ActiveEncounter);
        Assert.Equal(challenge.RuntimeEncounterId, restored.ActiveEncounter!.EncounterId);
        Assert.False(restored.ActiveEncounter.Active);
        Assert.Equal("runtime_state_flags_inventory_encounter", restored.Metadata["generated_microgame_challenge.state_source"]);
    }

    [Fact]
    public void RuntimeRewardChallengeStateDoesNotSilentlyUseProjectionFallback()
    {
        var result = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());
        var challenge = result.Snapshot.MicrogameChallenge;

        Assert.Equal("completed", challenge.CompletionStatus);
        Assert.Equal(challenge.StepCount, challenge.CompletedStepCount);
        Assert.Equal("runtime_state_flags_inventory_encounter", challenge.StateSource);
        Assert.False(challenge.FallbackPreviewProjectionUsed);
        Assert.DoesNotContain(challenge.Diagnostics, item => item.Code == "generated_microgame_challenge.preview_level_resolution");
    }

    private static VisibleGeneratedPlayablePreviewRequest CreateRequest() => new()
    {
        Seed = "runtime-reward-challenge-state-tests",
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
