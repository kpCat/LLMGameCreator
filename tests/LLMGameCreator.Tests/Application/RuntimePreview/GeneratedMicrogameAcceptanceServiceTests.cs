using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class GeneratedMicrogameAcceptanceServiceTests
{
    [Fact]
    public void GeneratedMicrogameAcceptanceReportIsDeterministicAndContainsLoopEvidence()
    {
        var visibleService = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter());
        var acceptanceService = new GeneratedMicrogameAcceptanceService();
        var first = acceptanceService.Build(visibleService.Generate(CreateRequest()));
        var second = acceptanceService.Build(visibleService.Generate(CreateRequest()));

        Assert.Equal(first.SnapshotJson, second.SnapshotJson);
        Assert.Equal(first.ReportMarkdown, second.ReportMarkdown);
        Assert.Equal(first.ManualVerificationMarkdown, second.ManualVerificationMarkdown);
        Assert.False(string.IsNullOrWhiteSpace(first.Snapshot.DeterministicHash));
        Assert.False(string.IsNullOrWhiteSpace(first.Snapshot.PackageId));
        Assert.False(string.IsNullOrWhiteSpace(first.Snapshot.ActiveGoalId));
        Assert.False(string.IsNullOrWhiteSpace(first.Snapshot.ActiveGoalTitle));
        Assert.False(string.IsNullOrWhiteSpace(first.Snapshot.ObjectiveText));
        Assert.False(string.IsNullOrWhiteSpace(first.Snapshot.ChallengeId));
        Assert.False(string.IsNullOrWhiteSpace(first.Snapshot.ChallengeTitle));
        Assert.False(string.IsNullOrWhiteSpace(first.Snapshot.RewardTitle));
        Assert.True(first.Snapshot.RuntimeStartSucceeded);
        Assert.True(first.Snapshot.RuntimeMoveSucceeded);
        Assert.True(first.Snapshot.RuntimeInteractSucceeded);
        Assert.True(first.Snapshot.ActiveGoalVisible);
        Assert.True(first.Snapshot.ProgressAdvanced);
        Assert.True(first.Snapshot.ChallengeResolved);
        Assert.True(first.Snapshot.RewardVisible);
        Assert.True(first.Snapshot.CompletionVisible);
        Assert.Equal("completed", first.Snapshot.CompletionStatus);
        Assert.Contains(first.Snapshot.Diagnostics, item => item.Code == "generated_microgame_acceptance.no_external_execution");
        Assert.Contains(first.Snapshot.Diagnostics, item => item.Code == "generated_microgame_acceptance.manual_verification_required");
    }

    [Fact]
    public async Task GeneratedMicrogameAcceptanceWriteCreatesSnapshotReportAndManualVerificationDoc()
    {
        using var temp = new TempDirectory();
        var visibleService = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter());
        var acceptanceService = new GeneratedMicrogameAcceptanceService();
        var result = acceptanceService.Build(visibleService.Generate(CreateRequest()));

        var write = await acceptanceService.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.SnapshotJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.ManualVerificationMarkdownPath));
        Assert.Contains("generated-microgame-loop", write.OutputDirectoryPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("generated-microgame-loop", write.SnapshotJsonPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("generated_microgame_acceptance.no_external_execution", File.ReadAllText(write.SnapshotJsonPath));
        Assert.Contains("Generated Microgame Loop Acceptance", File.ReadAllText(write.ReportMarkdownPath));
        var manual = File.ReadAllText(write.ManualVerificationMarkdownPath);
        Assert.Contains("Manual Microgame Loop Verification", manual);
        Assert.Contains("manual UI check", manual);
        Assert.Contains(result.Snapshot.ActiveGoalTitle, manual);
        Assert.Contains(result.Snapshot.ChallengeTitle, manual);
        Assert.Contains(result.Snapshot.RewardTitle, manual);
    }

    private static VisibleGeneratedPlayablePreviewRequest CreateRequest() => new()
    {
        Seed = "generated-microgame-acceptance-tests",
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
