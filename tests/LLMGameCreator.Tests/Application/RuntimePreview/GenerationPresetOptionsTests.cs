using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class GenerationPresetOptionsTests
{
    [Fact]
    public void GenerationPresetOptionsDefaultMatchesCurrentWorkflow()
    {
        var service = new GenerationPresetOptionsService();

        var options = service.ResolveDefault();

        Assert.Equal(GenerationPresetOptionsService.DefaultSeed, options.Seed);
        Assert.Equal(ProceduralGameGenerationModes.SemiProceduralRegions, options.Mode);
        Assert.Equal(GenerationPresetOptionsService.DefaultPresetId, options.PresetId);
        Assert.Contains("theme/exploration", options.CompactStyleHintIds);
        Assert.Contains("theme/survival", options.CompactStyleHintIds);
        Assert.Contains("quest_motif/faction_truce", options.CompactStyleHintIds);
        Assert.Contains("world_topology/region_graph", options.SelectedVariantIds);
    }

    [Fact]
    public void GenerationPresetOptionsProducesStableAcceptedPreviewAndVariesBySeedAndPreset()
    {
        var previewService = new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter());
        var defaultRequest = new VisibleGeneratedPlayablePreviewRequest();

        var first = previewService.Generate(defaultRequest);
        var second = previewService.Generate(defaultRequest);
        var alternateSeed = previewService.Generate(defaultRequest with { Seed = "generation-preset-options-alternate-seed" });
        var alternatePreset = previewService.Generate(defaultRequest with { PresetId = "recover_resource" });

        Assert.Equal(first.SnapshotJson, second.SnapshotJson);
        Assert.True(first.Report.RuntimeStartSucceeded);
        Assert.True(first.Report.ActiveGoalSelected);
        Assert.True(first.Report.GoalProgressAdvanced);
        Assert.True(first.Report.ChallengeResolved);
        Assert.True(first.Report.RewardVisible);
        Assert.True(first.Report.CompletionVisible);
        Assert.Equal("runtime_state_quests", first.Snapshot.MicrogameGoal.ProgressStateSource);
        Assert.Equal("runtime_state_flags_inventory_encounter", first.Snapshot.MicrogameChallenge.StateSource);
        Assert.Contains(first.Report.Diagnostics, item => item.Code == "generation_preset_options.selected");
        Assert.Contains(first.Report.Diagnostics, item => item.Code == "visible_generated_playable_preview.no_external_execution");
        Assert.NotEqual(first.Snapshot.PackageId, alternateSeed.Snapshot.PackageId);
        Assert.NotEqual(first.Snapshot.DeterministicHash, alternateSeed.Snapshot.DeterministicHash);
        Assert.NotEqual(first.Snapshot.GenerationOptions.StableSummary, alternatePreset.Snapshot.GenerationOptions.StableSummary);
        Assert.Equal("recover_resource", alternatePreset.Snapshot.GenerationOptions.PresetId);
        Assert.Contains("quest_motif/recover_lost_resource", alternatePreset.Snapshot.GenerationOptions.CompactStyleHintIds);
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
}
