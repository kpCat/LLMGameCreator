using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class MicrogameVariationAcceptanceTests
{
    [Fact]
    public void MicrogameVariationAcceptanceBuildsStableAcceptedVariantMatrix()
    {
        using var temp = new TempDirectory();
        var serializer = new RuntimeStateSerializer();
        var service = new MicrogameVariationAcceptanceService(
            visiblePreviewService: new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()),
            runtimeBackedStateAcceptanceService: new RuntimeBackedMicrogameStateAcceptanceService(
                serializer,
                new RuntimeSnapshotStore(serializer)));

        var first = service.Build(temp.Path);
        var second = service.Build(temp.Path);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.Equal(3, first.Report.VariantCount);
        Assert.True(first.Report.DifferenceSummary.UniqueSeedCount >= 3);
        Assert.True(first.Report.DifferenceSummary.UniquePresetCount >= 2);
        Assert.True(first.Report.DifferenceSummary.UniquePackageIdCount >= 3);
        Assert.All(first.Report.Variants, variant =>
        {
            Assert.True(variant.Accepted);
            Assert.True(variant.RuntimeStartSucceeded);
            Assert.True(variant.ProgressAdvanced);
            Assert.True(variant.ChallengeResolved);
            Assert.True(variant.RewardVisible);
            Assert.True(variant.RuntimeRewardGranted);
            Assert.True(variant.CompletionVisible);
            Assert.True(variant.RuntimeCompletionBacked);
            Assert.Equal("runtime_state_quests", variant.GoalProgressStateSource);
            Assert.Equal("runtime_state_flags_inventory_encounter", variant.ChallengeStateSource);
            Assert.False(variant.GoalProgressFallbackPreviewJournalUsed);
            Assert.False(variant.ChallengeFallbackPreviewProjectionUsed);
        });
        Assert.Contains(first.Report.Diagnostics, item => item.Code == "generated_microgame_variation.no_external_execution");
        Assert.Contains("manual_configurable_microgame_verification", first.ReportJson);
        Assert.Contains("Manual Configurable Microgame Verification", first.ManualVerificationMarkdown);
    }

    [Fact]
    public async Task MicrogameVariationAcceptanceWriteCreatesExpectedArtifacts()
    {
        using var temp = new TempDirectory();
        var serializer = new RuntimeStateSerializer();
        var service = new MicrogameVariationAcceptanceService(
            visiblePreviewService: new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()),
            runtimeBackedStateAcceptanceService: new RuntimeBackedMicrogameStateAcceptanceService(
                serializer,
                new RuntimeSnapshotStore(serializer)));
        var result = service.Build(temp.Path);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.ManualVerificationMarkdownPath));
        Assert.Contains("generated-microgame-variation", write.OutputDirectoryPath, StringComparison.OrdinalIgnoreCase);
        var json = File.ReadAllText(write.ReportJsonPath);
        Assert.Contains("\"accepted\": true", json);
        Assert.Contains("\"variantCount\": 3", json);
        Assert.Contains("\"manualGate\": \"manual_configurable_microgame_verification\"", json);
    }

    private sealed class DefaultRuntimeAdapter : IVisibleGeneratedPlayableRuntimeAdapter
    {
        public VisibleGeneratedPlayableRuntimeAttempt Run(GamePackageDefinition package)
        {
            var runtime = new DefaultGameRuntime();
            var start = runtime.Start(package);
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
                PlayerStartPosition = new VisibleGeneratedPlayablePosition
                {
                    X = start.State.PlayerPosition.X,
                    Y = start.State.PlayerPosition.Y
                },
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
