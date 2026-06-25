using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class RuntimeMicrogameStateAcceptanceTests
{
    [Fact]
    public void RuntimeMicrogameStateAcceptanceRecordsRuntimeAndPersistenceEvidence()
    {
        using var temp = new TempDirectory();
        var visibleResult = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());
        var serializer = new RuntimeStateSerializer();
        var service = new RuntimeBackedMicrogameStateAcceptanceService(
            serializer,
            new RuntimeSnapshotStore(serializer));

        var result = service.Build(visibleResult, temp.Path);

        Assert.Equal(result.SnapshotJson, service.Build(visibleResult, temp.Path).SnapshotJson);
        Assert.False(string.IsNullOrWhiteSpace(result.Snapshot.DeterministicHash));
        Assert.True(result.Snapshot.RuntimeStartSucceeded);
        Assert.True(result.Snapshot.RuntimeMoveSucceeded);
        Assert.True(result.Snapshot.RuntimeInteractSucceeded);
        Assert.True(result.Snapshot.ActiveGoalVisible);
        Assert.True(result.Snapshot.ProgressAdvanced);
        Assert.Equal("runtime_state_quests", result.Snapshot.GoalProgressStateSource);
        Assert.False(result.Snapshot.GoalProgressFallbackPreviewJournalUsed);
        Assert.Equal("runtime_state_flags_inventory_encounter", result.Snapshot.ChallengeStateSource);
        Assert.True(result.Snapshot.RuntimeRewardGranted);
        Assert.True(result.Snapshot.RuntimeCompletionBacked);
        Assert.False(result.Snapshot.ChallengeFallbackPreviewProjectionUsed);
        Assert.True(result.Snapshot.Persistence.SerializerAvailable);
        Assert.True(result.Snapshot.Persistence.SerializationRoundtripSucceeded);
        Assert.True(result.Snapshot.Persistence.SnapshotStoreAvailable);
        Assert.True(result.Snapshot.Persistence.SnapshotSaveSucceeded);
        Assert.True(result.Snapshot.Persistence.SnapshotLoadSucceeded);
        Assert.Equal(RuntimeBackedMicrogameStateAcceptanceService.SnapshotSlotName, result.Snapshot.Persistence.SnapshotSlotName);
        Assert.Contains(result.Snapshot.Diagnostics, item => item.Code == "runtime_backed_microgame_state.serialization_roundtrip_passed");
        Assert.Contains(result.Snapshot.Diagnostics, item => item.Code == "runtime_backed_microgame_state.snapshot_store_roundtrip_passed");
        Assert.Contains("Runtime-Backed Microgame State Acceptance", result.ReportMarkdown);
        Assert.Contains("Manual Runtime-Backed Microgame Verification", result.ManualVerificationMarkdown);
    }

    [Fact]
    public async Task RuntimeMicrogameStateAcceptanceWriteCreatesExpectedArtifacts()
    {
        using var temp = new TempDirectory();
        var visibleResult = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());
        var serializer = new RuntimeStateSerializer();
        var service = new RuntimeBackedMicrogameStateAcceptanceService(
            serializer,
            new RuntimeSnapshotStore(serializer));
        var result = service.Build(visibleResult, temp.Path);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.SnapshotJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.ManualVerificationMarkdownPath));
        Assert.Contains("runtime-backed-microgame-state", write.OutputDirectoryPath, StringComparison.OrdinalIgnoreCase);
        var json = File.ReadAllText(write.SnapshotJsonPath);
        Assert.Contains("\"goalProgressStateSource\": \"runtime_state_quests\"", json);
        Assert.Contains("\"challengeStateSource\": \"runtime_state_flags_inventory_encounter\"", json);
        Assert.Contains("\"serializationRoundtripSucceeded\": true", json);
        Assert.Contains("\"snapshotSaveSucceeded\": true", json);
        Assert.Contains("\"snapshotLoadSucceeded\": true", json);
    }

    private static VisibleGeneratedPlayablePreviewRequest CreateRequest() => new()
    {
        Seed = "runtime-microgame-state-acceptance-tests",
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

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
