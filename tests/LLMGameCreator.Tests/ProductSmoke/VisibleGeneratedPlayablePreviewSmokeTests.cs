using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class VisibleGeneratedPlayablePreviewSmokeTests
{
    [Fact]
    public async Task VisibleGeneratedPlayablePreviewProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var service = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter());

        var first = service.Generate(new VisibleGeneratedPlayablePreviewRequest
        {
            Seed = "product-smoke-visible-generated-playable-preview",
            Mode = "fully_seeded_world",
            CompactStyleHintIds =
            [
                "theme/exploration",
                "theme/survival",
                "tone/mysterious",
                "quest_motif/recover_lost_resource",
                "item_affordance/quest_item"
            ],
            SelectedVariantIds =
            [
                "world_topology/infinite_chunks",
                "chunk_streaming/generated_on_demand",
                "actor_model/single_player_character",
                "combat_model/turn_based",
                "inventory_model/list_inventory"
            ]
        });
        var second = service.Generate(new VisibleGeneratedPlayablePreviewRequest
        {
            Seed = "product-smoke-visible-generated-playable-preview",
            Mode = "fully_seeded_world",
            CompactStyleHintIds =
            [
                "theme/exploration",
                "theme/survival",
                "tone/mysterious",
                "quest_motif/recover_lost_resource",
                "item_affordance/quest_item"
            ],
            SelectedVariantIds =
            [
                "world_topology/infinite_chunks",
                "chunk_streaming/generated_on_demand",
                "actor_model/single_player_character",
                "combat_model/turn_based",
                "inventory_model/list_inventory"
            ]
        });

        await new ProceduralGameKernelService().WriteAsync(projectRoot, first.PlanResult);
        await new FormulaEffectActionRegistryService().WriteAsync(projectRoot, first.RulePackResult);
        await new TinyGeneratedRuntimeLoopService().WriteAsync(projectRoot, first.TinyLoopResult);
        await new GeneratedPackageMvpService().WriteAsync(projectRoot, first.PackageMvpResult);
        var write = await service.WriteAsync(projectRoot, first);

        Assert.Equal(first.SnapshotJson, second.SnapshotJson);
        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.Equal(first.ReportMarkdown, second.ReportMarkdown);
        Assert.True(File.Exists(write.SnapshotJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.ManualVerificationMarkdownPath));
        Assert.Equal(first.SnapshotJson, await File.ReadAllTextAsync(write.SnapshotJsonPath));
        Assert.Equal(first.ReportJson, await File.ReadAllTextAsync(write.ReportJsonPath));
        Assert.True(first.Report.RuntimeStartSucceeded);
        Assert.True(first.Report.RuntimeCommandAttempted);
        Assert.True(first.Report.RuntimeCommandSucceeded);
        Assert.True(first.Snapshot.Counts.Regions >= 2);
        Assert.NotEmpty(first.Snapshot.Projection.Quests);
        Assert.NotEmpty(first.Snapshot.Projection.Mechanics);
        Assert.Contains("visible-generated-playable-preview", write.SnapshotJsonPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

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
            EventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList()
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
