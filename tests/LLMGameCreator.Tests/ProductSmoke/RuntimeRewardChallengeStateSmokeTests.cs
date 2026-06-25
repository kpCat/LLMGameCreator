using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class RuntimeRewardChallengeStateSmokeTests
{
    [Fact]
    public async Task RuntimeRewardChallengeStateProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var current = new FakeCurrentGamePackageService(projectRoot);
        var service = new OneClickGeneratedPreviewWorkflowService(
            visiblePreviewService: new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()),
            currentGamePackageService: current);

        var result = await service.ExecuteAsync(new OneClickGeneratedPreviewWorkflowRequest
        {
            ProjectRootPath = projectRoot,
            Seed = "product-smoke-runtime-reward-challenge-state",
            Mode = "semi_procedural_regions",
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
        });

        var challenge = result.VisiblePreviewResult.Snapshot.MicrogameChallenge;
        Assert.True(result.Ok);
        Assert.True(result.VisiblePreviewResult.Report.RuntimeStartSucceeded);
        Assert.True(challenge.Resolved);
        Assert.True(challenge.RewardVisible);
        Assert.True(challenge.CompletionVisible);
        Assert.True(challenge.RuntimeChallengeResolved);
        Assert.True(challenge.RuntimeRewardGranted);
        Assert.True(challenge.RuntimeCompletionBacked);
        Assert.Equal("runtime_state_flags_inventory_encounter", challenge.StateSource);
        Assert.Contains(result.Diagnostics, item => item.Code == "generated_microgame_challenge.runtime_state_evidence");
        Assert.True(File.Exists(result.Paths.MicrogameAcceptanceSnapshotJsonPath));
        var acceptanceJson = File.ReadAllText(result.Paths.MicrogameAcceptanceSnapshotJsonPath);
        Assert.Contains("\"challengeStateSource\": \"runtime_state_flags_inventory_encounter\"", acceptanceJson);
        Assert.Contains("\"runtimeRewardGranted\": true", acceptanceJson);
        Assert.Contains("\"runtimeCompletionBacked\": true", acceptanceJson);
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
            EventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EventTargets = result.Events.Select(item => item.TargetId ?? string.Empty).Where(value => !string.IsNullOrWhiteSpace(value)).OrderBy(item => item, StringComparer.Ordinal).ToList(),
            EventMessages = result.Events.Select(item => item.Message).Where(value => !string.IsNullOrWhiteSpace(value)).OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }

    private sealed class FakeCurrentGamePackageService : ICurrentGamePackageService
    {
        public FakeCurrentGamePackageService(string currentFolder)
        {
            CurrentFolder = currentFolder;
        }

        public string? CurrentFolder { get; }
        public GamePackageDefinition? CurrentPackage { get; private set; }
        public event EventHandler? CurrentChanged;
        public Task LoadAsync(string projectFolder, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void ReplaceCurrent(GamePackageDefinition package)
        {
            CurrentPackage = package;
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }
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
