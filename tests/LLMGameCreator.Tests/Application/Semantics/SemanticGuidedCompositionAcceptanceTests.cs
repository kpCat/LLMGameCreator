using LLMGameCreator.Application.Design.Semantics;
using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.Semantics;

public sealed class SemanticGuidedCompositionAcceptanceTests
{
    [Fact]
    public async Task BuildsStableAcceptedSemanticGuidedCompositionArtifacts()
    {
        using var temp = new TempDirectory();
        var service = CreateService();

        var first = service.Build(temp.Path);
        var second = service.Build(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.Equal("semantic_guided_composition_artifact_verification", first.Report.ManualGate);
        Assert.True(first.Report.Goal004RuntimeEvidencePreserved);
        Assert.True(first.Report.RepeatedRunStable);
        Assert.True(first.Report.MultiSeedNoDanglingReferences);
        Assert.True(first.Report.MeaningfulValidVariantCount >= 3);
        Assert.False(first.Report.ExternalExecution.LlmExecuted);
        Assert.False(first.Report.ExternalExecution.RagExecuted);
        Assert.False(first.Report.ExternalExecution.ProviderExecuted);
        Assert.False(first.Report.ExternalExecution.LuaExecuted);
        Assert.False(first.Report.ExternalExecution.UnityExecuted);
        Assert.False(first.Report.ExternalExecution.MediaExecuted);

        var validSelections = first.Report.Scenarios
            .Where(item => item.ExpectedValid)
            .Select(item => (item.SelectedQuestPatternId, item.SelectedDialogueIntentId, item.SelectedInteractionPatternId))
            .Distinct()
            .ToList();
        Assert.True(validSelections.Count >= 3);

        var overlay = first.Report.Scenarios.Single(item => item.ScenarioId == "core_genre_project_overlay");
        Assert.Equal("quest_pattern/two_step_sequence", overlay.SelectedQuestPatternId);
        Assert.Equal("dialogue/completion_response/default", overlay.SelectedDialogueIntentId);
        Assert.Equal("interaction/use_reward_on_contact", overlay.SelectedInteractionPatternId);

        var candidate = first.Report.Scenarios.Single(item => item.ScenarioId == "candidate_quarantine");
        Assert.True(candidate.Accepted);
        Assert.False(candidate.CandidateLeakageDetected);
        Assert.True(candidate.QuarantinedTermCount >= 1);

        var invalid = first.Report.Scenarios.Single(item => item.ScenarioId == "invalid_conflict_rejection");
        Assert.False(invalid.Accepted);
        Assert.Contains(invalid.Diagnostics, item => item.Code == "semantic_guided.excludes_conflict");

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.Contains("\"accepted\": true", await File.ReadAllTextAsync(write.ReportJsonPath));
    }

    private static SemanticGuidedCompositionAcceptanceService CreateService()
    {
        var serializer = new RuntimeStateSerializer();
        var goal004 = new QuestDialogInteractionFamilyAcceptanceService(
            visiblePreviewService: new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()),
            runtimeBackedStateAcceptanceService: new RuntimeBackedMicrogameStateAcceptanceService(
                serializer,
                new RuntimeSnapshotStore(serializer)));

        return new SemanticGuidedCompositionAcceptanceService(goal004Service: goal004);
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
