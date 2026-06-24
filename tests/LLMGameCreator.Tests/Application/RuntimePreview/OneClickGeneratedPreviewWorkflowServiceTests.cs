using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using LLMGameCreator.Application.Projects;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class OneClickGeneratedPreviewWorkflowServiceTests
{
    [Fact]
    public async Task OneClickGeneratedPreviewWorkflowProducesArtifactsAndLoadsCurrentPackage()
    {
        using var temp = new TempDirectory();
        var current = new FakeCurrentGamePackageService(temp.Path);
        var service = CreateService(current);

        var first = await service.ExecuteAsync(new OneClickGeneratedPreviewWorkflowRequest { ProjectRootPath = temp.Path });
        var second = await service.ExecuteAsync(new OneClickGeneratedPreviewWorkflowRequest { ProjectRootPath = temp.Path });

        Assert.True(first.Ok);
        Assert.Equal("generated_preview_ready", first.Status);
        Assert.True(first.CurrentPackageReplaced);
        Assert.Equal(first.PackageId, first.GeneratedPackage.Manifest.PackageId);
        Assert.Equal(second.PackageId, current.CurrentPackage?.Manifest.PackageId);
        Assert.StartsWith("Generated MVP", first.PackageTitle, StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(first.PackageId));
        Assert.True(File.Exists(first.Paths.PlanJsonPath));
        Assert.True(File.Exists(first.Paths.RulePackJsonPath));
        Assert.True(File.Exists(first.Paths.TinyRuntimeLoopStateJsonPath));
        Assert.True(File.Exists(first.Paths.GeneratedPackageJsonPath));
        Assert.True(File.Exists(first.Paths.VisiblePreviewSnapshotJsonPath));
        Assert.True(File.Exists(first.Paths.VisiblePreviewReportJsonPath));
        Assert.True(File.Exists(first.Paths.ManualVerificationMarkdownPath));
        Assert.Equal(first.StableSummary, second.StableSummary);
        Assert.Equal(first.VisiblePreviewResult.SnapshotJson, second.VisiblePreviewResult.SnapshotJson);
        Assert.True(first.VisiblePreviewResult.Report.RuntimeStartSucceeded);
        Assert.True(first.VisiblePreviewResult.Report.RuntimeCommandSucceeded);
        Assert.Contains(first.Diagnostics, item => item.Code == "one_click_generated_preview.no_external_execution");
        Assert.Contains(first.Diagnostics, item => item.Code == "one_click_generated_preview.current_package_replaced");
    }

    [Fact]
    public async Task OneClickGeneratedPreviewWorkflowRejectsConcurrentRun()
    {
        using var temp = new TempDirectory();
        var service = CreateService(new FakeCurrentGamePackageService(temp.Path));
        var request = new OneClickGeneratedPreviewWorkflowRequest { ProjectRootPath = temp.Path };

        var running = service.ExecuteAsync(request);
        var concurrent = await service.ExecuteAsync(request);
        var completed = await running;

        Assert.True(completed.Ok);
        Assert.False(concurrent.Ok);
        Assert.Equal("already_running", concurrent.Status);
        Assert.Contains(concurrent.Diagnostics, item => item.Code == "one_click_generated_preview.already_running");
    }

    [Fact]
    public async Task OneClickGeneratedPreviewWorkflowCanDeferCurrentPackageReplacementToCaller()
    {
        using var temp = new TempDirectory();
        var current = new FakeCurrentGamePackageService(temp.Path);
        var service = CreateService(current);

        var result = await service.ExecuteAsync(new OneClickGeneratedPreviewWorkflowRequest
        {
            ProjectRootPath = temp.Path,
            ReplaceCurrentPackage = false
        });

        Assert.True(result.Ok);
        Assert.False(result.CurrentPackageReplaced);
        Assert.Null(current.CurrentPackage);
        Assert.False(string.IsNullOrWhiteSpace(result.GeneratedPackage.Manifest.PackageId));
        Assert.True(File.Exists(result.Paths.GeneratedPackageJsonPath));
        Assert.Contains(result.Diagnostics, item => item.Code == "one_click_generated_preview.current_package_replacement_deferred");
        Assert.DoesNotContain(result.Diagnostics, item => item.Code == "one_click_generated_preview.current_package_replaced");
    }

    private static OneClickGeneratedPreviewWorkflowService CreateService(FakeCurrentGamePackageService current) =>
        new(
            visiblePreviewService: new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()),
            currentGamePackageService: current);

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
