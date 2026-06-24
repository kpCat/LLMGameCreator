using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class VisibleGeneratedPlayablePreviewServiceTests
{
    [Fact]
    public void SamePipelineInputProducesByteIdenticalPreviewOutputs()
    {
        var service = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter());
        var request = CreateRequest();

        var first = service.Generate(request);
        var second = service.Generate(request);

        Assert.Equal(first.SnapshotJson, second.SnapshotJson);
        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.Equal(first.ReportMarkdown, second.ReportMarkdown);
        Assert.Equal(first.ManualVerificationMarkdown, second.ManualVerificationMarkdown);
        Assert.True(first.Report.RuntimeStartSucceeded);
        Assert.True(first.Report.RuntimeCommandAttempted);
        Assert.True(first.Report.RuntimeCommandSucceeded);
        Assert.Contains(first.Report.Diagnostics, item => item.Code == "visible_generated_playable_preview.no_external_execution");
    }

    [Fact]
    public void ProjectionContainsGeneratedPackageContentAndRuntimeCommandEvidence()
    {
        var result = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()).Generate(CreateRequest());

        Assert.StartsWith("Generated MVP", result.Snapshot.PackageTitle, StringComparison.Ordinal);
        Assert.NotEmpty(result.Snapshot.CurrentMapId);
        Assert.NotNull(result.Snapshot.Projection.CurrentScene);
        Assert.NotEmpty(result.Snapshot.Projection.Profile.Title);
        Assert.True(result.Snapshot.Counts.Regions >= 2);
        Assert.NotEmpty(result.Snapshot.Projection.Quests);
        Assert.NotEmpty(result.Snapshot.Projection.Mechanics);
        Assert.NotEmpty(result.Snapshot.Projection.Provenance);
        Assert.Contains(result.Snapshot.RuntimeAttempt.CommandAttempts, item => item.CommandType == "move/right" && item.Succeeded);
        Assert.Contains(result.Snapshot.RuntimeAttempt.CommandAttempts, item => item.CommandType == "interact" && item.EventTypes.Contains("InteractionTriggered"));
        Assert.False(string.IsNullOrWhiteSpace(result.Snapshot.SourceHashes.PlanHash));
        Assert.False(string.IsNullOrWhiteSpace(result.Snapshot.SourceHashes.RulePackHash));
        Assert.False(string.IsNullOrWhiteSpace(result.Snapshot.SourceHashes.TinyLoopStateHash));
        Assert.False(string.IsNullOrWhiteSpace(result.Snapshot.SourceHashes.GeneratedPackageFinalHash));
    }

    [Fact]
    public void MissingRuntimeAdapterProducesExplicitDeterministicBlocker()
    {
        var result = new VisibleGeneratedPlayablePreviewService().Generate(CreateRequest());

        Assert.False(result.Report.RuntimeStartSucceeded);
        Assert.False(result.Report.RuntimeCommandAttempted);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "visible_generated_playable_preview.runtime_adapter_unavailable");
        Assert.NotEmpty(result.Snapshot.Projection.Profile.Title);
        Assert.True(result.Snapshot.Counts.Regions >= 2);
    }

    private static VisibleGeneratedPlayablePreviewRequest CreateRequest() => new()
    {
        Seed = "visible-generated-playable-preview-tests",
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
            EventTypes = result.Events.Select(item => item.Type.ToString()).OrderBy(item => item, StringComparer.Ordinal).ToList()
        };
    }
}
