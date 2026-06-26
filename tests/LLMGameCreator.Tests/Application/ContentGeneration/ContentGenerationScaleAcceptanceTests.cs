using System.Text.Json;
using LLMGameCreator.Application.Design.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.Application.ContentGeneration;

public sealed class ContentGenerationScaleAcceptanceTests
{
    [Fact]
    public async Task BuildsStableAcceptedContentGenerationScaleArtifacts()
    {
        using var temp = new TempDirectory();
        var service = ContentGenerationScaleAcceptanceTestFactory.CreateService();
        var packDirectory = ResolvePackDirectory();

        var first = service.BuildFromReferencePackDirectory(packDirectory, temp.Path);
        var second = service.BuildFromReferencePackDirectory(packDirectory, temp.Path);
        var write = await service.WriteAsync(temp.Path, first);

        Assert.Equal(first.ReportJson, second.ReportJson);
        Assert.True(first.Report.Accepted);
        Assert.Equal("content_generation_at_scale_artifact_verification", first.Report.ManualGate);
        Assert.True(first.Report.Goal009GateRecorded);
        Assert.Equal(["S085", "S086", "S087", "S088", "S089", "S090", "S091", "S091A"], first.Report.CompletedSlices);
        Assert.Equal(3, first.Report.PackCount);
        Assert.Equal(3, first.Report.ValidPackCount);
        Assert.True(first.Report.RuntimeThreadCount >= 6);
        Assert.Equal(first.Report.RuntimeThreadCount, first.Report.RuntimeThreadsAccepted);
        Assert.True(first.Report.ObjectiveKindDistribution.Count >= 3);
        Assert.True(first.Report.EventActionDistribution.Count >= 3);
        Assert.True(first.Report.ValidMatrixPassed);
        Assert.True(first.Report.InvalidMatrixPassed);
        Assert.True(first.Report.PackageRuntimePassed);
        Assert.True(first.Report.RepetitionPassed);
        Assert.False(first.Report.PublicGamePackageSchemaChanged);
        Assert.False(first.Report.ProjectFilesChanged);
        Assert.False(first.Report.ExternalExecution.LlmExecuted);
        Assert.False(first.Report.ExternalExecution.RagExecuted);
        Assert.False(first.Report.ExternalExecution.ProviderExecuted);
        Assert.False(first.Report.ExternalExecution.LuaExecuted);
        Assert.False(first.Report.ExternalExecution.UnityExecuted);
        Assert.False(first.Report.ExternalExecution.MediaExecuted);
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var roundTrip = JsonSerializer.Deserialize<ContentGenerationScaleReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(roundTrip);
        Assert.True(roundTrip!.Accepted);
    }

    [Fact]
    public void ReferencePacksExpandAtScaleWithStableProvenanceAndRepetitionMetrics()
    {
        var report = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(ResolvePackDirectory(), FindRepoRoot())
            .Report;

        Assert.All(report.Packs, pack =>
        {
            Assert.True(pack.Accepted);
            Assert.Equal(204, pack.Counts.TotalInstances);
            Assert.True(pack.Counts.Npcs >= 24);
            Assert.True(pack.Counts.Quests >= 24);
            Assert.True(pack.Counts.Events >= 24);
            Assert.True(pack.Counts.DialogueLines >= 48);
            Assert.True(pack.Counts.ItemLootSpawnEntries >= 48);
            Assert.True(pack.AuthoredExpandedCounts.ExpandedShare >= 0.9);
            Assert.NotEmpty(pack.SourceHash);
            Assert.NotEmpty(pack.Catalog.CatalogHash);
            Assert.All(pack.Catalog.Npcs, npc =>
            {
                Assert.NotEmpty(npc.Provenance.SourcePackId);
                Assert.NotEmpty(npc.Provenance.SourceId);
                Assert.NotEmpty(npc.RegionId);
                Assert.NotEmpty(npc.FactionId);
            });
            Assert.Equal(0, pack.RepetitionMetrics.DuplicateNpcDisplayNames);
            Assert.Equal(0, pack.RepetitionMetrics.DuplicateQuestSignatures);
            Assert.Equal(0, pack.RepetitionMetrics.DuplicateDialogueLines);
            Assert.Equal(0, pack.RepetitionMetrics.DuplicateEventSignatures);
            Assert.True(pack.RepetitionMetrics.MaxSharePassed);
            Assert.DoesNotContain(pack.Catalog.Dialogues, dialogue => dialogue.Line.Contains('{') || dialogue.Line.Contains('}'));
        });
        Assert.Equal(3, report.IsolationEvidence.DistinctCatalogHashes);
        Assert.True(report.ReplayEvidence.Passed);
        Assert.True(report.VariationEvidence.Passed);
    }

    [Fact]
    public void MaterializedPackagesAreCleanAndRuntimeThreadsUseRealRuntimeBoundary()
    {
        var report = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(ResolvePackDirectory(), FindRepoRoot())
            .Report;

        Assert.All(report.Packs, pack =>
        {
            Assert.True(pack.PackageAudit.ValidatorClean);
            Assert.True(pack.PackageAudit.StructuralAuditPassed);
            Assert.True(pack.PackageAudit.GeneratedContentHashMatchesCatalog);
            Assert.NotEmpty(pack.PackageAudit.PackageHash);
            Assert.True(pack.PackageAudit.ObjectiveKindDistribution.Count >= 3);
            Assert.True(pack.PackageAudit.EventActionDistribution.Count >= 3);
            Assert.All(pack.RuntimeThreads, thread =>
            {
                Assert.True(thread.ActualValid);
                Assert.Equal(pack.PackageAudit.PackageHash, thread.PackageHash);
                Assert.True(thread.RuntimeEvidence.RuntimeAttempted);
                Assert.True(thread.RuntimeEvidence.RuntimeStartSucceeded);
                Assert.True(thread.RuntimeEvidence.RuntimeBoundary.UsedGameRuntimeService);
                Assert.True(thread.RuntimeEvidence.RuntimeBoundary.UsedRuntimeStateFactory);
                Assert.EndsWith("GameRuntimeService", thread.RuntimeEvidence.RuntimeBoundary.RuntimeServiceType, StringComparison.Ordinal);
                Assert.EndsWith("RuntimeStateSerializer", thread.RuntimeEvidence.RuntimeBoundary.SerializerType, StringComparison.Ordinal);
                Assert.EndsWith("RuntimeSnapshotStore", thread.RuntimeEvidence.RuntimeBoundary.SnapshotStoreType, StringComparison.Ordinal);
                Assert.True(thread.RuntimeEvidence.StateDelta.QuestProgressChanged);
                Assert.True(thread.RuntimeEvidence.StateDelta.RewardItemChanged);
                Assert.All(thread.Commands.Where(command => !string.IsNullOrWhiteSpace(command.ExpectedChangedQuestId)), command =>
                    Assert.Contains(command.ExpectedChangedQuestId, thread.RuntimeEvidence.StateDelta.ChangedQuestIds));
                Assert.All(thread.Commands.Where(command => !string.IsNullOrWhiteSpace(command.ExpectedChangedItemId)), command =>
                    Assert.Contains(command.ExpectedChangedItemId, thread.RuntimeEvidence.StateDelta.ChangedItemIds));
                Assert.All(thread.Commands.Where(command => !string.IsNullOrWhiteSpace(command.ExpectedChangedFlagId)), command =>
                    Assert.Contains(command.ExpectedChangedFlagId, thread.RuntimeEvidence.StateDelta.ChangedFlagIds));
                Assert.All(thread.Commands.Where(command => !string.IsNullOrWhiteSpace(command.ExpectedChangedFactionId)), command =>
                    Assert.Contains(command.ExpectedChangedFactionId, thread.RuntimeEvidence.StateDelta.ChangedFactionIds));
                Assert.True(thread.RuntimeEvidence.SaveLoadRoundtripPassed);
                Assert.True(thread.RuntimeEvidence.SaveLoadEvidence.UsedRuntimeStateSerializer);
                Assert.True(thread.RuntimeEvidence.SaveLoadEvidence.UsedRuntimeSnapshotStore);
                Assert.True(thread.RuntimeEvidence.SaveLoadEvidence.SerializedFullState);
                Assert.True(thread.RuntimeEvidence.SaveLoadEvidence.TempSnapshotCleanupSucceeded);
                Assert.Equal(thread.RuntimeEvidence.SaveLoadEvidence.SerializedStateHash, thread.RuntimeEvidence.SaveLoadEvidence.RestoredSerializedStateHash);
                Assert.Equal(thread.RuntimeEvidence.StateEvidence, thread.RuntimeEvidence.RestoredStateEvidence);
                Assert.All(thread.Commands, command => Assert.Contains(thread.RuntimeEvidence.Commands, evidence =>
                    evidence.CommandId == command.CommandId &&
                    evidence.CommandType == command.CommandType &&
                    evidence.TargetId == command.TargetId &&
                    evidence.SecondaryTargetId == command.SecondaryTargetId &&
                    evidence.Value == command.Value &&
                    evidence.InventoryId == command.InventoryId &&
                    Math.Abs(evidence.Amount - command.Amount) < 0.0001 &&
                    evidence.Succeeded));
            });
        });
    }

    [Fact]
    public void InvalidFakeLeakAndExpectationOnlyScenariosAreRejectedCausally()
    {
        var report = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(ResolvePackDirectory(), FindRepoRoot())
            .Report;
        var invalid = report.InvalidMatrix.Scenarios.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.True(report.InvalidMatrix.ScenarioCount >= 23);
        Assert.True(report.InvalidMatrix.Passed);
        Assert.Contains(invalid["wrong_schema_version"].Diagnostics, item => item.Code == "content_generation.pack.schema_version");
        Assert.Contains(invalid["malformed_json"].Diagnostics, item => item.Code == "content_generation.pack.malformed_json");
        Assert.Contains(invalid["duplicate_source_ids"].Diagnostics, item => item.Code == "content_generation.pack.duplicate_npc_archetype");
        Assert.Contains(invalid["missing_archetype_motif_voice_loot_reference"].Diagnostics, item => item.Code == "content_generation.pack.reward_ref_missing");
        Assert.Contains(invalid["cyclic_quest_event_dependency"].Diagnostics, item => item.Code == "content_generation.pack.quest_dependency_cycle");
        Assert.Contains(invalid["semantic_required_excluded_conflict"].Diagnostics, item => item.Code == "content_generation.pack.semantic_conflict");
        Assert.Contains(invalid["unresolved_dialogue_slot"].Diagnostics, item => item.Code == "content_generation.pack.dialogue_slot_missing");
        Assert.Contains(invalid["nonpositive_nan_infinite_loot_weight_or_amount"].Diagnostics, item => item.Code == "content_generation.pack.loot_weight_amount_invalid");
        Assert.Contains(invalid["impossible_dangling_reward_or_requirement"].Diagnostics, item => item.Code == "content_generation.pack.requirement_ref_missing");
        Assert.Contains(invalid["unsupported_trigger_action_runtime_binding"].Diagnostics, item => item.Code == "content_generation.pack.unsupported_action");
        Assert.Contains(invalid["generation_budget_above_safe_cap"].Diagnostics, item => item.Code == "content_generation.pack.budget.total");
        Assert.Contains(invalid["exhausted_combination_pool_without_fallback"].Diagnostics, item => item.Code == "content_generation.repetition.duplicate_names");
        Assert.Contains(invalid["repetition_limit_breach"].Diagnostics, item => item.Code == "content_generation.repetition.share_cap_breached");
        Assert.Contains(invalid["command_not_covered_by_selected_generated_declaration"].Diagnostics, item => item.Code == "content_generation.audit.command_not_covered");
        Assert.Contains(invalid["package_objective_kind_coerced_to_choose_dialogue"].Diagnostics, item => item.Code == "content_generation.audit.objective_kind_mismatch");
        Assert.Contains(invalid["event_action_kind_coerced_to_set_flag"].Diagnostics, item => item.Code == "content_generation.audit.event_action_kind_mismatch" || item.Code == "content_generation.audit.event_action_target_mismatch");
        Assert.Contains(invalid["runtime_command_type_mismatch"].Diagnostics, item => item.Code == "content_generation.evidence.command_type_mismatch");
        Assert.Contains(invalid["runtime_command_value_mismatch"].Diagnostics, item => item.Code == "content_generation.evidence.command_value_mismatch");
        Assert.Contains(invalid["runtime_command_inventory_secondary_target_mismatch"].Diagnostics, item => item.Code == "content_generation.evidence.command_inventory_mismatch" || item.Code == "content_generation.evidence.command_secondary_target_mismatch");
        Assert.Contains(invalid["fake_runtime_success"].Diagnostics, item => item.Code == "content_generation.evidence.runtime_boundary_missing");
        Assert.Contains(invalid["save_load_mismatch"].Diagnostics, item => item.Code == "content_generation.evidence.save_load_mismatch");
        Assert.Contains(invalid["cross_pack_catalog_runtime_leakage"].Diagnostics, item => item.Code == "content_generation.evidence.cross_pack_runtime_leakage");
        Assert.Contains(invalid["expectation_only_invalid_fixture"].Diagnostics, item => item.Code == "content_generation.invalid.expectation_only_mutation_present");
        Assert.All(invalid.Values, scenario =>
        {
            Assert.False(scenario.ActualValid);
            Assert.Contains(scenario.Diagnostics, item => item.Severity == "error");
        });
    }

    [Fact]
    public void DefaultUnavailableAdapterCannotSatisfyAcceptance()
    {
        var result = new ContentGenerationScaleAcceptanceService()
            .BuildFromReferencePackDirectory(ResolvePackDirectory(), FindRepoRoot());

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.PackageRuntimePassed);
        Assert.Contains(result.Report.Diagnostics, item => item.Code == "content_generation.runtime_adapter_unavailable");
    }

    [Fact]
    public void RemovingExpectationOnlyMutationMakesExpectedInvalidMatrixFail()
    {
        var result = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(
                ResolvePackDirectory(),
                FindRepoRoot(),
                new ContentGenerationScaleAcceptanceOptions { IncludeExpectationOnlyInvalidMutation = false });

        Assert.False(result.Report.Accepted);
        Assert.False(result.Report.InvalidMatrixPassed);
        var scenario = result.Report.InvalidMatrix.Scenarios.Single(item => item.ScenarioId == "expectation_only_invalid_fixture");
        Assert.True(scenario.ActualValid);
        Assert.DoesNotContain(scenario.Diagnostics, item => item.Severity == "error");
    }

    private static string ResolvePackDirectory() =>
        Path.Combine(FindRepoRoot(), "samples", "content-generation-packs");

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
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
