using System.Text.Json;
using LLMGameCreator.Application.Design.Gameplay;
using LLMGameCreator.Tests.Application.Gameplay;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class RulePackCombatFactionSocialWorkTheftSmokeTests
{
    [Fact]
    public async Task RulePackCombatFactionSocialWorkTheftProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var service = RulePackCombatFactionSocialWorkTheftAcceptanceTestFactory.CreateService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        var json = await File.ReadAllTextAsync(write.ReportJsonPath);
        var report = JsonSerializer.Deserialize<RulePackCombatFactionSocialWorkTheftReport>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.True(report.Accepted);
        Assert.Equal("rule_pack_combat_faction_social_work_theft_artifact_verification", report.ManualGate);
        Assert.True(report.Goal008GateRecorded);
        Assert.Equal(7, report.ValidScenarioCount);
        Assert.Equal(12, report.InvalidScenarioCount);
        Assert.True(report.PackageRuleBindingAuditPassed);
        Assert.True(report.CombatFactionSocialWorkTheftRuntimeExecutionPassed);
        Assert.True(report.SaveLoadRoundtripPassed);
        Assert.True(report.DeterministicReplayPassed);
        Assert.True(report.ScenarioIsolationPassed);
        Assert.True(report.FakeRuntimeSuccessRejected);
        Assert.False(report.ExternalExecution.LlmExecuted);
        Assert.False(report.ExternalExecution.RagExecuted);
        Assert.False(report.ExternalExecution.ProviderExecuted);
        Assert.False(report.ExternalExecution.LuaExecuted);
        Assert.False(report.ExternalExecution.UnityExecuted);
        Assert.False(report.ExternalExecution.MediaExecuted);

        var combined = report.Scenarios.Single(item => item.ScenarioId == "combined_combat_social_work_theft_loop");
        Assert.True(combined.RuntimeEvidence.RuntimeBoundary.UsedGameRuntimeService);
        Assert.True(combined.RuntimeEvidence.RuntimeBoundary.UsedRuntimeStateFactory);
        Assert.True(combined.RuntimeEvidence.RuntimeBoundary.UsedEncounterRuntimeService);
        Assert.True(combined.RuntimeEvidence.RuntimeBoundary.UsedFactionRuntimeService);
        Assert.True(combined.RuntimeEvidence.RuntimeBoundary.UsedDialogueRuntimeService);
        Assert.True(combined.RuntimeEvidence.RuntimeBoundary.UsedInteractionRuntimeService);
        Assert.True(combined.RuntimeEvidence.RuntimeBoundary.UsedContainerRuntimeService);
        Assert.True(combined.RuntimeEvidence.SaveLoadEvidence.UsedRuntimeStateSerializer);
        Assert.True(combined.RuntimeEvidence.SaveLoadEvidence.UsedRuntimeSnapshotStore);
        Assert.Equal(combined.RuntimeEvidence.SaveLoadEvidence.SerializedStateHash, combined.RuntimeEvidence.SaveLoadEvidence.RestoredSerializedStateHash);
        Assert.False(combined.RuntimeEvidence.EncounterAfter.Active);
        Assert.Equal("completed", combined.RuntimeEvidence.WorkEvidence.CompletionFlagAfter);
        Assert.Equal("true", combined.RuntimeEvidence.TheftEvidence.TheftFlagAfter);
        Assert.Contains(combined.RuntimeEvidence.Commands, command => command.CommandType == "combat/use_ability" && command.RuntimeEventTypes.Contains("EncounterWon"));
        Assert.Contains(combined.RuntimeEvidence.Commands, command => command.CommandType == "social/choose_dialogue" && command.FactionDelta.Changed);
        Assert.Contains(combined.RuntimeEvidence.Commands, command => command.CommandType == "work/execute_contract" && command.WorkDelta.Changed);
        Assert.Contains(combined.RuntimeEvidence.Commands, command => command.CommandType == "theft/take_from_container" && command.ContainerDelta.Changed && command.InventoryDelta.Changed);
        Assert.Contains(combined.RuntimeEvidence.Commands, command => command.CommandType == "faction/change_reputation" && command.FactionDelta.Changed);

        var invalid = report.Scenarios.Where(item => !item.ExpectedValid).ToList();
        Assert.All(invalid, scenario =>
        {
            Assert.False(scenario.ActualValid);
            Assert.Contains(scenario.Diagnostics, item => item.Severity == "error");
        });
        Assert.Contains(invalid.Single(item => item.ScenarioId == "invalid_fake_runtime_success").Diagnostics, item => item.Code == "combat_family.evidence.required_command_missing");
        Assert.Contains(invalid.Single(item => item.ScenarioId == "invalid_save_load_mismatch").Diagnostics, item => item.Code == "combat_family.evidence.save_load_mismatch");
        Assert.Contains(invalid.Single(item => item.ScenarioId == "invalid_cross_scenario_state_leakage").Diagnostics, item => item.Code == "combat_family.evidence.cross_scenario_state_leakage");
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
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
