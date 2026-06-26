using LLMGameCreator.Application.Design.Gameplay;
using Xunit;

namespace LLMGameCreator.Tests.Application.Gameplay;

public sealed class RulePackCombatFactionSocialWorkTheftAcceptanceTests
{
    [Fact]
    public async Task BuildsStableAcceptedRulePackCombatFactionSocialWorkTheftArtifacts()
    {
        using var temp = new TempDirectory();
        var service = RulePackCombatFactionSocialWorkTheftAcceptanceTestFactory.CreateService();

        var first = service.Build(temp.Path);
        var second = service.Build(temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.Equal("rule_pack_combat_faction_social_work_theft_artifact_verification", first.Report.ManualGate);
        Assert.True(first.Report.Goal008GateRecorded);
        Assert.Equal(["S078", "S079", "S080", "S081", "S082", "S083", "S084"], first.Report.CompletedSlices);
        Assert.Equal(7, first.Report.ValidScenarioCount);
        Assert.Equal(12, first.Report.InvalidScenarioCount);
        Assert.True(first.Report.ValidScenariosAccepted);
        Assert.True(first.Report.InvalidScenariosRejected);
        Assert.True(first.Report.PackageRuleBindingAuditPassed);
        Assert.True(first.Report.CombatFactionSocialWorkTheftRuntimeExecutionPassed);
        Assert.True(first.Report.SaveLoadRoundtripPassed);
        Assert.True(first.Report.DeterministicReplayPassed);
        Assert.True(first.Report.ScenarioIsolationPassed);
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
    public void ValidScenariosProveExactBindingsRealRuntimeCommandsAndFullStateSaveLoad()
    {
        var report = RulePackCombatFactionSocialWorkTheftAcceptanceTestFactory.CreateService().Build().Report;
        var valid = report.Scenarios.Where(item => item.ExpectedValid).ToList();

        Assert.All(valid, scenario =>
        {
            Assert.True(scenario.ActualValid);
            Assert.True(scenario.PackageBindingAudit.Passed);
            Assert.NotEmpty(scenario.PackageBindingAudit.AuditedDeclarationIds);
            Assert.All(scenario.PackageRuntimeIds, id => Assert.Contains(id, scenario.PackageBindingAudit.AuditedPackageRuntimeIds));
            Assert.True(scenario.RuntimeEvidence.RuntimeAttempted);
            Assert.Equal("GameRuntimeState", scenario.RuntimeEvidence.RuntimeStateOwner);
            Assert.True(scenario.RuntimeEvidence.RuntimeBoundary.UsedGameRuntimeService);
            Assert.True(scenario.RuntimeEvidence.RuntimeBoundary.UsedRuntimeStateFactory);
            Assert.True(scenario.RuntimeEvidence.RuntimeBoundary.UsedEncounterRuntimeService);
            Assert.True(scenario.RuntimeEvidence.RuntimeBoundary.UsedEncounterAiService);
            Assert.True(scenario.RuntimeEvidence.RuntimeBoundary.UsedFactionRuntimeService);
            Assert.True(scenario.RuntimeEvidence.RuntimeBoundary.UsedDialogueRuntimeService);
            Assert.True(scenario.RuntimeEvidence.RuntimeBoundary.UsedInteractionRuntimeService);
            Assert.True(scenario.RuntimeEvidence.RuntimeBoundary.UsedContainerRuntimeService);
            Assert.EndsWith("GameRuntimeService", scenario.RuntimeEvidence.RuntimeBoundary.RuntimeServiceType, StringComparison.Ordinal);
            Assert.EndsWith("GameRuntimeStateFactory", scenario.RuntimeEvidence.RuntimeBoundary.StateFactoryType, StringComparison.Ordinal);
            Assert.EndsWith("RuntimeStateSerializer", scenario.RuntimeEvidence.RuntimeBoundary.SerializerType, StringComparison.Ordinal);
            Assert.EndsWith("RuntimeSnapshotStore", scenario.RuntimeEvidence.RuntimeBoundary.SnapshotStoreType, StringComparison.Ordinal);
            Assert.NotEmpty(scenario.RuntimeEvidence.Commands);
            Assert.All(scenario.RuntimeEvidence.Commands, command =>
            {
                Assert.True(command.Succeeded);
                Assert.NotEmpty(command.RuntimeEventTypes);
            });
            Assert.True(scenario.RuntimeEvidence.SaveLoadEvidence.UsedRuntimeStateSerializer);
            Assert.True(scenario.RuntimeEvidence.SaveLoadEvidence.UsedRuntimeSnapshotStore);
            Assert.True(scenario.RuntimeEvidence.SaveLoadEvidence.SerializedFullState);
            Assert.Equal(scenario.RuntimeEvidence.SaveLoadEvidence.SerializedStateHash, scenario.RuntimeEvidence.SaveLoadEvidence.RestoredSerializedStateHash);
            Assert.Equal(scenario.RuntimeEvidence.StateEvidence, scenario.RuntimeEvidence.RestoredStateEvidence);
        });
    }

    [Fact]
    public void ValidScenarioFamiliesCoverCombatFactionSocialWorkTheftAndCombinedLoop()
    {
        var report = RulePackCombatFactionSocialWorkTheftAcceptanceTestFactory.CreateService().Build().Report;
        var combat = report.Scenarios.Single(item => item.ScenarioId == "combat_turn_based_encounter");
        var resolution = report.Scenarios.Single(item => item.ScenarioId == "combat_resolution_reward");
        var faction = report.Scenarios.Single(item => item.ScenarioId == "faction_reputation_change");
        var social = report.Scenarios.Single(item => item.ScenarioId == "social_dialogue_reputation_consequence");
        var work = report.Scenarios.Single(item => item.ScenarioId == "work_contract_reward");
        var theft = report.Scenarios.Single(item => item.ScenarioId == "theft_container_reputation_consequence");
        var combined = report.Scenarios.Single(item => item.ScenarioId == "combined_combat_social_work_theft_loop");

        Assert.Contains(combat.RuntimeEvidence.Commands, command => command.CommandType == "combat/use_ability" && command.RuntimeEventTypes.Contains("DamageApplied"));
        Assert.Contains(combat.RuntimeEvidence.Commands, command => command.CommandType == "combat/run_ai" && command.RuntimeEventTypes.Contains("AiActionChosen"));
        Assert.False(resolution.RuntimeEvidence.EncounterAfter.Active);
        Assert.Contains(resolution.RuntimeEvidence.Commands, command => command.RuntimeEventTypes.Contains("EncounterWon") && command.InventoryDelta.After["item/victory_token"] == "1");
        Assert.Contains(faction.RuntimeEvidence.Commands, command => command.CommandType == "faction/change_reputation" && command.FactionDelta.After["faction/settlement_watch"].StartsWith("12|", StringComparison.Ordinal));
        Assert.Contains(social.RuntimeEvidence.Commands, command => command.CommandType == "social/choose_dialogue" && command.RuntimeEventTypes.Contains("DialogueChoiceSelected") && command.FactionDelta.Changed && command.FlagDelta.Changed);
        Assert.Contains(work.RuntimeEvidence.Commands, command => command.CommandType == "work/execute_contract" && command.WorkDelta.Changed && command.InventoryDelta.After["item/wage_scrip"] == "3");
        Assert.Equal("completed", work.RuntimeEvidence.WorkEvidence.CompletionFlagAfter);
        Assert.Contains(theft.RuntimeEvidence.Commands, command => command.CommandType == "theft/take_from_container" && command.ContainerDelta.After["item/stolen_gem"] == "1" && command.InventoryDelta.After["item/stolen_gem"] == "1");
        Assert.Equal("true", theft.RuntimeEvidence.TheftEvidence.TheftFlagAfter);
        Assert.Equal(
            [
                "social/open_dialogue",
                "social/choose_dialogue",
                "work/execute_contract",
                "combat/start_encounter",
                "combat/use_ability",
                "gameplay/set_flag",
                "theft/open_container",
                "theft/take_from_container",
                "gameplay/set_flag",
                "faction/change_reputation"
            ],
            combined.RuntimeEvidence.Commands.Select(item => item.CommandType).ToList());
        Assert.Equal("true", combined.RuntimeEvidence.TheftEvidence.TheftFlagAfter);
        Assert.Equal("completed", combined.RuntimeEvidence.WorkEvidence.CompletionFlagAfter);
        Assert.False(combined.RuntimeEvidence.EncounterAfter.Active);
    }

    [Fact]
    public void InvalidScenariosAreRejectedByStableCausalDiagnosticsOrRuntimeFailures()
    {
        var report = RulePackCombatFactionSocialWorkTheftAcceptanceTestFactory.CreateService().Build().Report;
        var invalid = report.Scenarios.Where(item => !item.ExpectedValid).ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.Contains(invalid["invalid_missing_encounter_or_participant_ref"].Diagnostics, item => item.Code == "combat_family.audit.missing_encounter_ref");
        Assert.Contains(invalid["invalid_missing_ability_or_resource_ref"].Diagnostics, item => item.Code == "combat_family.audit.missing_ability_ref");
        Assert.Contains(invalid["invalid_combat_wrong_turn_or_target"].RuntimeEvidence.Commands, item => !item.Succeeded && item.DiagnosticCode == "encounter.turn.invalid");
        Assert.Contains(invalid["invalid_missing_faction_ref"].Diagnostics, item => item.Code == "combat_family.audit.missing_faction_ref");
        Assert.Contains(invalid["invalid_dialogue_or_choice_ref"].Diagnostics, item => item.Code == "combat_family.audit.missing_dialogue_choice_ref");
        Assert.Contains(invalid["invalid_work_requirement_unmet"].RuntimeEvidence.Commands, item => !item.Succeeded && item.RuntimeDiagnosticCodes.Contains("requirement.item_missing"));
        Assert.Contains(invalid["invalid_theft_container_or_item_ref"].Diagnostics, item => item.Code == "combat_family.audit.missing_theft_item_ref");
        Assert.Contains(invalid["invalid_theft_nonpositive_amount"].Diagnostics, item => item.Code == "combat_family.audit.theft_amount_invalid");
        Assert.Contains(invalid["invalid_command_not_covered_by_declaration"].Diagnostics, item => item.Code == "combat_family.audit.command_not_covered_by_declaration");
        Assert.Contains(invalid["invalid_fake_runtime_success"].Diagnostics, item => item.Code == "combat_family.evidence.required_command_missing");
        Assert.Contains(invalid["invalid_save_load_mismatch"].Diagnostics, item => item.Code == "combat_family.evidence.save_load_mismatch");
        Assert.Contains(invalid["invalid_cross_scenario_state_leakage"].Diagnostics, item => item.Code == "combat_family.evidence.cross_scenario_state_leakage");
        Assert.All(invalid.Values, scenario =>
        {
            Assert.False(scenario.ActualValid);
            Assert.Contains(scenario.Diagnostics, item => item.Severity == "error");
        });
    }

    [Fact]
    public void DefaultUnavailableAdapterCannotSatisfyAcceptance()
    {
        var result = new RulePackCombatFactionSocialWorkTheftAcceptanceService().Build();

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.ValidScenariosAccepted);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "combat_family.runtime_adapter_unavailable");
    }

    [Fact]
    public void ExternalFakeAdapterCannotSatisfyAcceptance()
    {
        var result = new RulePackCombatFactionSocialWorkTheftAcceptanceService(new FakeSuccessRuntimeAdapter()).Build();

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.ValidScenariosAccepted);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "combat_family.evidence.required_command_missing");
    }

    private sealed class FakeSuccessRuntimeAdapter : IRulePackCombatFactionSocialWorkTheftRuntimeAdapter
    {
        public RulePackCombatFactionSocialWorkTheftRuntimeEvidence Run(RulePackCombatFactionSocialWorkTheftRuntimeRequest request) => new()
        {
            RuntimeAttempted = true,
            RuntimeStartSucceeded = true,
            RuntimeStateOwner = "GameRuntimeState",
            PackageId = request.Package.Manifest.PackageId,
            RuntimeStateHash = "copied",
            RestoredRuntimeStateHash = "copied",
            SaveLoadRoundtripPassed = true,
            ScenarioIsolationPassed = true,
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
