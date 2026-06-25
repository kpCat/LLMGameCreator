using LLMGameCreator.Application.RuntimePreview;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime;
using LLMGameCreator.Runtime.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimePreview;

public sealed class ExtensionSpineScenarioHarnessTests
{
    [Fact]
    public void HarnessBuildsStableAcceptedBaseAndExtensionScenarios()
    {
        using var temp = new TempDirectory();
        var service = CreateService();

        var first = service.Run(temp.Path);
        var second = service.Run(temp.Path);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.True(first.Report.ExtensionChangedBehavior);
        Assert.True(first.Report.InvalidRulePackRejected);
        Assert.Equal("manual_extension_spine_verification", first.Report.ManualGate);
        Assert.Equal(2, first.Report.Scenarios.Count);
        Assert.All(first.Report.Scenarios, scenario =>
        {
            Assert.True(scenario.Accepted);
            Assert.True(scenario.RuntimeStartSucceeded);
            Assert.True(scenario.RuntimeMoveSucceeded);
            Assert.True(scenario.RuntimeInteractSucceeded);
            Assert.True(scenario.GoalProgressAdvanced);
            Assert.True(scenario.RuntimeRewardGranted);
            Assert.True(scenario.RuntimeCompletionBacked);
            Assert.Equal("runtime_state_quests", scenario.GoalProgressStateSource);
            Assert.Equal("runtime_state_flags_inventory_encounter", scenario.ChallengeStateSource);
        });

        var extension = first.Report.Scenarios.Single(item => item.ScenarioId == "extension_inventory_objective");
        Assert.True(extension.ExtensionEvidence.Consumed);
        Assert.True(extension.ExtensionEvidence.InventoryObjectiveCompleted);
        Assert.True(extension.ExtensionEvidence.AdditionalRewardGranted);
        Assert.Equal("item/extension_spine_badge", extension.ExtensionEvidence.AddedRewardItemId);
        Assert.Equal("validated_rule_pack_existing_runtime_state", extension.ExtensionEvidence.StateSource);
        Assert.Contains("quest objectives", first.Report.WhatIsDataExtensible);
        Assert.Contains("new runtime command families", first.Report.WhatStillRequiresCSharpPrimitive);
    }

    [Fact]
    public void ValidatorRejectsUnsafeUnknownFormulaAndUnsupportedExtensionDeclarations()
    {
        var validator = new ExtensionRulePackValidator();
        var invalid = ExtensionSpineScenarioHarnessService.BuildInvalidProofPack();

        var report = validator.Validate(invalid);

        Assert.True(report.HasErrors);
        Assert.Contains(report.Diagnostics, item => item.Code == "extension_rule_pack.unsafe_id");
        Assert.Contains(report.Diagnostics, item => item.Code == "extension_rule_pack.unsafe_path");
        Assert.Contains(report.Diagnostics, item => item.Code == "extension_rule_pack.unknown_api_call");
        Assert.Contains(report.Diagnostics, item => item.Code == "extension_formula.expression.unsafe");
        Assert.Contains(report.Diagnostics, item => item.Code == "rule_pack.unknown_formula_ref");
        Assert.Contains(report.Diagnostics, item => item.Code == "extension_rule_pack.unsupported_mutation");
    }

    [Fact]
    public async Task HarnessWriteCreatesExpectedExtensionSpineArtifacts()
    {
        using var temp = new TempDirectory();
        var service = CreateService();
        var result = service.Run(temp.Path);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.ProofRulePackJsonPath));
        Assert.True(File.Exists(write.ProofValidationJsonPath));
        Assert.True(File.Exists(write.InvalidValidationJsonPath));
        Assert.True(File.Exists(write.ManualVerificationMarkdownPath));
        Assert.Contains("extension-spine", write.OutputDirectoryPath, StringComparison.OrdinalIgnoreCase);
        var json = File.ReadAllText(write.ReportJsonPath);
        Assert.Contains("\"accepted\": true", json);
        Assert.Contains("\"extensionChangedBehavior\": true", json);
        Assert.Contains("\"manualGate\": \"manual_extension_spine_verification\"", json);
    }

    private static ExtensionSpineScenarioHarnessService CreateService()
    {
        var serializer = new RuntimeStateSerializer();
        return new ExtensionSpineScenarioHarnessService(
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
