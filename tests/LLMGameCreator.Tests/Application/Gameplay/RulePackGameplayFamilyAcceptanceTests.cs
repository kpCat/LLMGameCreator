using LLMGameCreator.Application.Design.Gameplay;
using Xunit;

namespace LLMGameCreator.Tests.Application.Gameplay;

public sealed class RulePackGameplayFamilyAcceptanceTests
{
    [Fact]
    public async Task BuildsStableAcceptedRulePackGameplayFamilyArtifacts()
    {
        using var temp = new TempDirectory();
        var service = new RulePackGameplayFamilyAcceptanceService();

        var first = service.Build(temp.Path);
        var second = service.Build(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.Equal("rule_pack_gameplay_family_artifact_verification", first.Report.ManualGate);
        Assert.True(first.Report.Goal007GateRecorded);
        Assert.Equal(["S071", "S072", "S073", "S074", "S075", "S076", "S077"], first.Report.CompletedSlices);
        Assert.Equal(6, first.Report.ValidScenarioCount);
        Assert.Equal(6, first.Report.InvalidScenarioCount);
        Assert.True(first.Report.ValidScenariosAccepted);
        Assert.True(first.Report.InvalidScenariosRejected);
        Assert.True(first.Report.PackageRuleBindingAuditPassed);
        Assert.True(first.Report.GameplayRuntimeExecutionPassed);
        Assert.True(first.Report.SaveLoadRoundtripPassed);
        Assert.True(first.Report.DeterministicReplayPassed);
        Assert.True(first.Report.FakeRuntimeSuccessRejected);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ExternalExecution.LlmExecuted);
        Assert.False(first.Report.ExternalExecution.RagExecuted);
        Assert.False(first.Report.ExternalExecution.ProviderExecuted);
        Assert.False(first.Report.ExternalExecution.LuaExecuted);
        Assert.False(first.Report.ExternalExecution.UnityExecuted);
        Assert.False(first.Report.ExternalExecution.MediaExecuted);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
    }

    [Fact]
    public void ValidScenariosProveExactBindingsCommandsAndStateDeltas()
    {
        var report = new RulePackGameplayFamilyAcceptanceService().Build().Report;
        var valid = report.Scenarios.Where(item => item.ExpectedValid).ToList();

        Assert.All(valid, scenario =>
        {
            Assert.True(scenario.ActualValid);
            Assert.True(scenario.PackageBindingAudit.Passed);
            Assert.NotEmpty(scenario.PackageBindingAudit.AuditedDeclarationIds);
            Assert.All(scenario.PackageRuntimeIds, id => Assert.Contains(id, scenario.PackageBindingAudit.AuditedPackageRuntimeIds));
            Assert.True(scenario.RuntimeEvidence.RuntimeAttempted);
            Assert.Equal("GameRuntimeState", scenario.RuntimeEvidence.RuntimeStateOwner);
            Assert.NotEmpty(scenario.RuntimeEvidence.Commands);
            Assert.All(scenario.RuntimeEvidence.Commands, command =>
            {
                Assert.True(command.Succeeded);
                Assert.True(
                    command.InventoryDelta.Changed ||
                    command.EquipmentDelta.Changed ||
                    command.CraftingDelta.Changed ||
                    command.TradeDelta.Changed ||
                    command.StatusDelta.Changed ||
                    command.CompletionDelta.Changed);
            });
            Assert.Equal(scenario.RuntimeEvidence.StateEvidence, scenario.RuntimeEvidence.RestoredStateEvidence);
        });
    }

    [Fact]
    public void ValidScenarioFamiliesCoverInventoryEquipmentCraftingTradingStatusAndCombinedLoop()
    {
        var report = new RulePackGameplayFamilyAcceptanceService().Build().Report;
        var inventory = report.Scenarios.Single(item => item.ScenarioId == "gameplay_inventory_item_use");
        var equipment = report.Scenarios.Single(item => item.ScenarioId == "gameplay_equipment_loadout");
        var crafting = report.Scenarios.Single(item => item.ScenarioId == "gameplay_crafting_recipe");
        var trading = report.Scenarios.Single(item => item.ScenarioId == "gameplay_trading_transaction");
        var status = report.Scenarios.Single(item => item.ScenarioId == "gameplay_status_effect_chain");
        var combined = report.Scenarios.Single(item => item.ScenarioId == "gameplay_combined_loop");

        Assert.Contains(inventory.RuntimeEvidence.Commands, command => command.CommandType == "gameplay/use_item" && command.InventoryDelta.Changed && command.StatusDelta.Changed);
        Assert.Contains(equipment.RuntimeEvidence.Commands, command => command.CommandType == "gameplay/equip_item" && command.EquipmentDelta.After["slot/tool"] == "item/scavenger_tool");
        Assert.Contains(crafting.RuntimeEvidence.Commands, command =>
            command.CommandType == "gameplay/craft_recipe" &&
            command.CraftingDelta.Inputs.Any(delta => delta.ItemId == "item/scrap") &&
            command.CraftingDelta.Outputs.Any(delta => delta.ItemId == "item/repair_wrap"));
        Assert.Contains(trading.RuntimeEvidence.Commands, command =>
            command.CommandType == "gameplay/execute_transaction" &&
            command.TradeDelta.Costs.Any(delta => delta.ItemId == "item/trade_token") &&
            command.TradeDelta.Outputs.Any(delta => delta.ItemId == "item/signal_charm"));
        Assert.Contains(status.RuntimeEvidence.StatusAfter, item => item.Key == "status/focused@player" && item.Value.StartsWith("3|", StringComparison.Ordinal));
        Assert.Equal(
            ["gameplay/equip_item", "gameplay/craft_recipe", "gameplay/execute_transaction", "gameplay/use_item", "gameplay/set_flag"],
            combined.RuntimeEvidence.Commands.Select(item => item.CommandType).ToList());
        Assert.Equal("completed", combined.RuntimeEvidence.CompletionRewardEvidence.CompletionFlagAfter);
    }

    [Fact]
    public void InvalidScenariosAreRejectedByStableDiagnosticsOrFailedRuntimeEvidence()
    {
        var report = new RulePackGameplayFamilyAcceptanceService().Build().Report;
        var invalid = report.Scenarios.Where(item => !item.ExpectedValid).ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.Contains(invalid["invalid_missing_item_or_recipe_ref"].Diagnostics, item => item.Code == "gameplay_family.audit.missing_item_ref");
        Assert.Contains(invalid["invalid_equipment_slot_mismatch"].RuntimeEvidence.Commands, item => !item.Succeeded && item.DiagnosticCode == "equipment.slot_mismatch");
        Assert.Contains(invalid["invalid_crafting_missing_inputs"].RuntimeEvidence.Commands, item => !item.Succeeded && item.DiagnosticCode == "crafting.missing_inputs");
        Assert.Contains(invalid["invalid_trade_insufficient_cost"].RuntimeEvidence.Commands, item => !item.Succeeded && item.DiagnosticCode == "trade.insufficient_cost");
        Assert.Contains(invalid["invalid_status_or_effect_binding"].Diagnostics, item => item.Code == "gameplay_family.audit.invalid_status_effect_binding");
        Assert.Contains(invalid["invalid_fake_runtime_success"].Diagnostics, item => item.Code == "gameplay_family.evidence.required_command_missing");
        Assert.All(invalid.Values, scenario =>
        {
            Assert.False(scenario.ActualValid);
            Assert.Contains(scenario.Diagnostics, item => item.Severity == "error");
        });
    }

    [Fact]
    public void ExternalFakeAdapterCannotSatisfyAcceptance()
    {
        var result = new RulePackGameplayFamilyAcceptanceService(new FakeSuccessRuntimeAdapter()).Build();

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.ValidScenariosAccepted);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "gameplay_family.evidence.required_command_missing");
    }

    private sealed class FakeSuccessRuntimeAdapter : IRulePackGameplayFamilyRuntimeAdapter
    {
        public RulePackGameplayFamilyRuntimeEvidence Run(RulePackGameplayFamilyRuntimeRequest request) => new()
        {
            RuntimeAttempted = true,
            RuntimeStartSucceeded = true,
            RuntimeStateOwner = "GameRuntimeState",
            PackageId = request.Package.Manifest.PackageId,
            RuntimeStateHash = "copied",
            RestoredRuntimeStateHash = "copied",
            SaveLoadRoundtripPassed = true,
            StateEvidence = new Dictionary<string, string>(StringComparer.Ordinal) { ["scenarioId"] = request.ScenarioId },
            RestoredStateEvidence = new Dictionary<string, string>(StringComparer.Ordinal) { ["scenarioId"] = request.ScenarioId }
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
