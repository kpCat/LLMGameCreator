using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class QuestDialogInteractionFamilyAcceptanceTests
{
    [Fact]
    public void HarnessBuildsStableAcceptedQuestDialogInteractionFamilies()
    {
        using var temp = new TempDirectory();
        var service = CreateService();

        var first = service.Build(temp.Path);
        var second = service.Build(temp.Path);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.Equal("manual_quest_dialog_interaction_family_verification", first.Report.ManualGate);
        Assert.True(first.Report.InvalidRulePackRejected);
        Assert.True(first.Report.QuestStructureVariantCount >= 2);
        Assert.True(first.Report.DialogueEvidenceGenerated);
        Assert.True(first.Report.InteractionFamilyInvocationCount >= 2);
        Assert.Equal(4, first.Report.Scenarios.Count);
        Assert.All(first.Report.Scenarios, scenario =>
        {
            Assert.True(scenario.Accepted);
            Assert.True(scenario.RuntimeStartSucceeded);
            Assert.True(scenario.RuntimeInteractSucceeded);
            Assert.True(scenario.GoalProgressAdvanced);
            Assert.True(scenario.RuntimeRewardGranted);
            Assert.True(scenario.RuntimeCompletionBacked);
            Assert.Equal("runtime_state_quests", scenario.GoalProgressStateSource);
            Assert.Equal("runtime_state_flags_inventory_encounter", scenario.ChallengeStateSource);
        });

        var questVariant = first.Report.Scenarios.Single(item => item.ScenarioId == "quest_pattern_variant");
        Assert.True(questVariant.QuestEvidence.Generated);
        Assert.True(questVariant.QuestEvidence.ObjectiveCount >= 2);

        var dialogueVariant = first.Report.Scenarios.Single(item => item.ScenarioId == "dialogue_intent_variant");
        Assert.True(dialogueVariant.DialogueEvidence.Generated);
        Assert.NotEmpty(dialogueVariant.DialogueEvidence.Lines);

        var interactionVariant = first.Report.Scenarios.Single(item => item.ScenarioId == "interaction_pattern_variant");
        Assert.True(interactionVariant.InteractionEvidence.Invoked);
        Assert.True(interactionVariant.InteractionEvidence.ChangedGeneratedReportEvidence);
        Assert.True(interactionVariant.InteractionEvidence.InvokedFamilies.Count >= 2);
        Assert.Contains("dialogue intent templates with semantic slots", first.Report.WhatIsDataExtensible);
        Assert.Contains("new runtime command families", first.Report.WhatStillRequiresCSharpPrimitive);
    }

    [Fact]
    public void ValidatorRejectsUnsafeUnknownQuestDialogueInteractionDeclarations()
    {
        var validator = new QuestDialogInteractionRulePackValidator();
        var invalid = QuestDialogInteractionFamilyAcceptanceService.BuildInvalidProofPack();

        var report = validator.Validate(invalid);

        Assert.True(report.HasErrors);
        Assert.Contains(report.Diagnostics, item => item.Code == "quest_dialog_interaction.unsafe_id");
        Assert.Contains(report.Diagnostics, item => item.Code == "quest_dialog_interaction.unsafe_target_ref");
        Assert.Contains(report.Diagnostics, item => item.Code == "quest_pattern.unknown_interaction_ref");
        Assert.Contains(report.Diagnostics, item => item.Code == "dialogue_intent.unsupported_type");
        Assert.Contains(report.Diagnostics, item => item.Code == "interaction_pattern.unsupported_result_action");
    }

    [Fact]
    public async Task HarnessWriteCreatesExpectedFamilyArtifacts()
    {
        using var temp = new TempDirectory();
        var service = CreateService();
        var result = service.Build(temp.Path);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.ManualVerificationMarkdownPath));
        Assert.Contains("quest-dialog-interaction-families", write.OutputDirectoryPath, StringComparison.OrdinalIgnoreCase);
        var json = File.ReadAllText(write.ReportJsonPath);
        Assert.Contains("\"accepted\": true", json);
        Assert.Contains("\"manualGate\": \"manual_quest_dialog_interaction_family_verification\"", json);
        Assert.Contains("\"dialogueEvidenceGenerated\": true", json);
        Assert.Contains("\"invalidRulePackRejected\": true", json);
    }

    private static QuestDialogInteractionFamilyAcceptanceService CreateService()
    {
        var serializer = new RuntimeStateSerializer();
        return new QuestDialogInteractionFamilyAcceptanceService(
            visiblePreviewService: new VisibleGeneratedPlayablePreviewService(runtimeAdapter: new DefaultRuntimeAdapter()),
            runtimeBackedStateAcceptanceService: new RuntimeBackedMicrogameStateAcceptanceService(
                serializer,
                new RuntimeSnapshotStore(serializer)));
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
