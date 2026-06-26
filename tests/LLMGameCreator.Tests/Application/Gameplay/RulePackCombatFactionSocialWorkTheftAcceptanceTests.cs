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
        Assert.Equal(["S078", "S079", "S080", "S081", "S082", "S083", "S084", "S084A"], first.Report.CompletedSlices);
        Assert.Equal(7, first.Report.ValidScenarioCount);
        Assert.Equal(19, first.Report.InvalidScenarioCount);
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
            Assert.True(scenario.RuntimeEvidence.SaveLoadEvidence.TempSnapshotCleanupSucceeded);
            Assert.Equal(scenario.RuntimeEvidence.SaveLoadEvidence.SerializedStateHash, scenario.RuntimeEvidence.SaveLoadEvidence.RestoredSerializedStateHash);
            Assert.Equal(scenario.RuntimeEvidence.StateEvidence, scenario.RuntimeEvidence.RestoredStateEvidence);
            Assert.True(scenario.RuntimeEvidence.ScenarioIsolationPassed);
            Assert.Empty(scenario.RuntimeEvidence.ScenarioIsolationEvidence.UnexpectedRetainedKeys);
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
        Assert.Contains(faction.RuntimeEvidence.Commands, command => command.CommandType == "faction/change_reputation" && command.FactionDelta.After["faction/settlement_watch"].StartsWith("100|", StringComparison.Ordinal));
        var clamp = faction.RuntimeEvidence.FactionClampEvidence.Single(item => item.CommandId == "cmd/watch_reputation_gain");
        Assert.Equal(0, clamp.Before);
        Assert.Equal(112, clamp.RequestedAmount);
        Assert.Equal(112, clamp.UnclampedCandidate);
        Assert.Equal(-100, clamp.Min);
        Assert.Equal(100, clamp.Max);
        Assert.Equal(100, clamp.ExpectedAfter);
        Assert.Equal(100, clamp.ActualAfter);
        Assert.True(clamp.Clamped);
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
        Assert.Contains(invalid["invalid_same_declaration_wrong_existing_target"].Diagnostics, item => item.Code == "combat_family.audit.command_target_mismatch");
        Assert.Contains(invalid["invalid_same_declaration_wrong_ability_actor_target"].Diagnostics, item => item.Code == "combat_family.audit.command_actor_mismatch");
        Assert.Contains(invalid["invalid_same_declaration_wrong_ability_actor_target"].Diagnostics, item => item.Code == "combat_family.audit.command_secondary_target_mismatch");
        Assert.Contains(invalid["invalid_same_declaration_wrong_amount_flag_value"].Diagnostics, item => item.Code == "combat_family.audit.command_amount_mismatch");
        Assert.Contains(invalid["invalid_same_declaration_wrong_amount_flag_value"].Diagnostics, item => item.Code == "combat_family.audit.command_value_mismatch");
        Assert.Contains(invalid["invalid_missing_participant_ability_resource_cost_reward_binding"].Diagnostics, item => item.Code == "combat_family.audit.participant_ability_mismatch");
        Assert.Contains(invalid["invalid_missing_participant_ability_resource_cost_reward_binding"].Diagnostics, item => item.Code == "combat_family.audit.ability_cost_resource_mismatch");
        Assert.Contains(invalid["invalid_missing_participant_ability_resource_cost_reward_binding"].Diagnostics, item => item.Code == "combat_family.audit.encounter_reward_missing");
        Assert.Contains(invalid["invalid_missing_faction_ref"].Diagnostics, item => item.Code == "combat_family.audit.missing_faction_ref");
        Assert.Contains(invalid["invalid_dialogue_or_choice_ref"].Diagnostics, item => item.Code == "combat_family.audit.missing_dialogue_choice_ref");
        Assert.Contains(invalid["invalid_dialogue_wrong_node_or_output_mismatch"].Diagnostics, item => item.Code == "combat_family.audit.dialogue_choice_node_mismatch");
        Assert.Contains(invalid["invalid_work_requirement_unmet"].RuntimeEvidence.Commands, item => !item.Succeeded && item.RuntimeDiagnosticCodes.Contains("requirement.item_missing"));
        Assert.Contains(invalid["invalid_work_wrong_existing_transaction"].Diagnostics, item => item.Code == "combat_family.audit.work_transaction_mismatch");
        Assert.Contains(invalid["invalid_theft_container_or_item_ref"].Diagnostics, item => item.Code == "combat_family.audit.missing_theft_item_ref");
        Assert.Contains(invalid["invalid_theft_amount_flag_reputation_mismatch"].Diagnostics, item => item.Code == "combat_family.audit.command_amount_mismatch");
        Assert.Contains(invalid["invalid_theft_amount_flag_reputation_mismatch"].Diagnostics, item => item.Code == "combat_family.audit.command_value_mismatch");
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
    public void ValidScenariosExposeSequentialIsolationEvidenceAndInjectedLeakIsConcrete()
    {
        var report = RulePackCombatFactionSocialWorkTheftAcceptanceTestFactory.CreateService().Build().Report;
        var valid = report.Scenarios.Where(item => item.ExpectedValid).OrderBy(item => item.ScenarioId, StringComparer.Ordinal).ToList();
        var withPrevious = valid.Where(item => !string.IsNullOrWhiteSpace(item.RuntimeEvidence.ScenarioIsolationEvidence.PreviousScenarioId)).ToList();

        Assert.NotEmpty(withPrevious);
        Assert.All(valid, scenario =>
        {
            Assert.True(scenario.RuntimeEvidence.ScenarioIsolationPassed);
            Assert.Empty(scenario.RuntimeEvidence.ScenarioIsolationEvidence.CurrentInitialStateSignature);
            Assert.Empty(scenario.RuntimeEvidence.ScenarioIsolationEvidence.UnexpectedRetainedKeys);
        });

        var leaked = report.Scenarios.Single(item => item.ScenarioId == "invalid_cross_scenario_state_leakage");
        Assert.False(leaked.ActualValid);
        Assert.True(leaked.RuntimeEvidence.ScenarioIsolationEvidence.InjectedLeak);
        Assert.NotEmpty(leaked.RuntimeEvidence.ScenarioIsolationEvidence.CurrentInitialStateSignature);
        Assert.NotEmpty(leaked.RuntimeEvidence.ScenarioIsolationEvidence.UnexpectedRetainedKeys);
        Assert.Contains(leaked.Diagnostics, item => item.Code == "combat_family.evidence.cross_scenario_state_leakage");
    }

    [Fact]
    public void SuccessfulRealDeltaForWrongTargetCannotSatisfyRuntimeEvidence()
    {
        var result = new RulePackCombatFactionSocialWorkTheftAcceptanceService(new WrongTargetRuntimeAdapter()).Build();

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.ValidScenariosAccepted);
        Assert.Contains(result.Report.Scenarios.Single(item => item.ScenarioId == "faction_reputation_change").Diagnostics, item => item.Code == "combat_family.evidence.command_correlation_mismatch");
    }

    [Fact]
    public void RemovingInjectedLeakMakesExpectedInvalidMatrixFail()
    {
        var result = new RulePackCombatFactionSocialWorkTheftAcceptanceService(new LeakRemovedRuntimeAdapter()).Build();
        var leakage = result.Report.Scenarios.Single(item => item.ScenarioId == "invalid_cross_scenario_state_leakage");

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.InvalidScenariosRejected);
        Assert.True(leakage.ActualValid);
        Assert.DoesNotContain(leakage.Diagnostics, item => item.Code == "combat_family.evidence.cross_scenario_state_leakage");
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

    private sealed class WrongTargetRuntimeAdapter : IRulePackCombatFactionSocialWorkTheftRuntimeAdapter
    {
        private readonly RealRulePackCombatFactionSocialWorkTheftRuntimeAdapter _inner = new();

        public RulePackCombatFactionSocialWorkTheftRuntimeEvidence Run(RulePackCombatFactionSocialWorkTheftRuntimeRequest request)
        {
            var evidence = _inner.Run(request);
            if (request.ScenarioId != "faction_reputation_change")
            {
                return evidence;
            }

            var commands = evidence.Commands
                .Select(command => command.CommandId == "cmd/watch_reputation_gain" ? command with { TargetId = "faction/merchant_guild" } : command)
                .ToList();
            return evidence with { Commands = commands };
        }
    }

    private sealed class LeakRemovedRuntimeAdapter : IRulePackCombatFactionSocialWorkTheftRuntimeAdapter
    {
        private readonly RealRulePackCombatFactionSocialWorkTheftRuntimeAdapter _inner = new();

        public RulePackCombatFactionSocialWorkTheftRuntimeEvidence Run(RulePackCombatFactionSocialWorkTheftRuntimeRequest request)
        {
            if (request.ScenarioId != "invalid_cross_scenario_state_leakage")
            {
                return _inner.Run(request);
            }

            return _inner.Run(request with { ExpectedScenarioStateMarker = "cross_scenario_state_leakage_removed" });
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
