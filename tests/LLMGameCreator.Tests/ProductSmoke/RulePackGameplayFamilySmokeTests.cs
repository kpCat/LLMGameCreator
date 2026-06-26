using System.Text.Json;
using LLMGameCreator.Application.Design.Gameplay;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class RulePackGameplayFamilySmokeTests
{
    [Fact]
    public async Task RulePackGameplayFamilyFoundationsProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var service = new RulePackGameplayFamilyAcceptanceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        var json = await File.ReadAllTextAsync(write.ReportJsonPath);
        var report = JsonSerializer.Deserialize<RulePackGameplayFamilyReport>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.True(report.Accepted);
        Assert.Equal("rule_pack_gameplay_family_artifact_verification", report.ManualGate);
        Assert.True(report.Goal007GateRecorded);
        Assert.Equal(6, report.ValidScenarioCount);
        Assert.Equal(6, report.InvalidScenarioCount);
        Assert.True(report.PackageRuleBindingAuditPassed);
        Assert.True(report.GameplayRuntimeExecutionPassed);
        Assert.True(report.SaveLoadRoundtripPassed);
        Assert.True(report.DeterministicReplayPassed);
        Assert.True(report.FakeRuntimeSuccessRejected);
        Assert.False(report.ExternalExecution.LlmExecuted);
        Assert.False(report.ExternalExecution.RagExecuted);
        Assert.False(report.ExternalExecution.ProviderExecuted);
        Assert.False(report.ExternalExecution.LuaExecuted);
        Assert.False(report.ExternalExecution.UnityExecuted);
        Assert.False(report.ExternalExecution.MediaExecuted);

        var combined = report.Scenarios.Single(item => item.ScenarioId == "gameplay_combined_loop");
        Assert.Equal(
            ["gameplay/equip_item", "gameplay/craft_recipe", "gameplay/execute_transaction", "gameplay/use_item", "gameplay/set_flag"],
            combined.RuntimeEvidence.Commands.Select(item => item.CommandType).ToList());
        Assert.Equal("completed", combined.RuntimeEvidence.CompletionRewardEvidence.CompletionFlagAfter);
        Assert.Contains(combined.RuntimeEvidence.Commands, command => command.EquipmentDelta.After["slot/tool"] == "item/scavenger_tool");
        Assert.Contains(combined.RuntimeEvidence.Commands, command => command.CraftingDelta.Outputs.Any(output => output.ItemId == "item/repair_wrap"));
        Assert.Contains(combined.RuntimeEvidence.Commands, command => command.TradeDelta.Outputs.Any(output => output.ItemId == "item/signal_charm"));
        Assert.Contains(combined.RuntimeEvidence.StatusAfter, item => item.Key == "status/focused@player");

        var invalid = report.Scenarios.Where(item => !item.ExpectedValid).ToList();
        Assert.All(invalid, scenario =>
        {
            Assert.False(scenario.ActualValid);
            Assert.Contains(scenario.Diagnostics, item => item.Severity == "error");
        });
        Assert.Contains(invalid.Single(item => item.ScenarioId == "invalid_fake_runtime_success").Diagnostics, item => item.Code == "gameplay_family.evidence.required_command_missing");
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
