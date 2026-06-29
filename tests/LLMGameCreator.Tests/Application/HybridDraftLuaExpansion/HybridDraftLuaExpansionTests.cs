using System.Text.Json;
using LLMGameCreator.Application.Design.HybridDraftLuaExpansion;
using Xunit;

namespace LLMGameCreator.Tests.Application.HybridDraftLuaExpansion;

public sealed class HybridDraftLuaExpansionCatalogTests
{
    [Fact]
    public void DefaultRequestsCoverRequiredScenariosFamiliesAndMetamoduleSlots()
    {
        var requests = HybridDraftLuaExpansionCatalog.BuildDefaultRequests();
        var fixtures = HybridDraftLuaExpansionCatalog.BuildFixtures(requests);
        var sandboxMatrix = HybridDraftLuaExpansionCatalog.BuildSandboxApprovedExpansionMatrix(requests);
        var families = requests.Select(item => item.ProducedArtifactFamily).Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(8, requests.Count);
        Assert.Equal(4, requests.Select(item => item.ScenarioId).Distinct(StringComparer.Ordinal).Count());
        Assert.True(HybridDraftLuaExpansionVocabulary.ArtifactFamilies.All(families.Contains));
        Assert.All(requests, request => Assert.True(request.SandboxApprovedForGoal037Executor));
        Assert.Equal(4, sandboxMatrix.RowCount);
        Assert.Equal(4, sandboxMatrix.ApprovedCount);
        Assert.Contains(requests, item => item.ScenarioId == "metamodule_kingdoms" && item.OutputBudget >= 100);
        Assert.All(fixtures.Values, fixture => Assert.DoesNotContain(HybridDraftLuaExpansionCatalog.ValidateFixture(fixture), item => item.Severity == "error"));
    }
}

public sealed class HybridDraftLuaExecutorAdapterTests
{
    [Fact]
    public async Task AdapterExecutesRepoOwnedFixtureDeterministically()
    {
        var requests = HybridDraftLuaExpansionCatalog.BuildDefaultRequests();
        var request = requests.First(item => item.ScenarioId == "frontier_survival");
        var fixture = HybridDraftLuaExpansionCatalog.BuildFixtures(requests)[request.FixtureId];
        var adapter = new HybridDraftLuaExecutorAdapter();

        var first = await adapter.ExecuteAsync(request, fixture);
        var second = await adapter.ExecuteAsync(request, fixture);

        Assert.Equal("accepted", first.Status);
        Assert.True(first.LuaExecuted);
        Assert.NotNull(first.Output);
        Assert.Equal(first.Output!.TraceHash, second.Output!.TraceHash);
        Assert.Equal(first.Output.StructuralTraceSummary, second.Output.StructuralTraceSummary);
        Assert.Equal(first.Output.Slots.Select(item => item.SlotId), second.Output.Slots.Select(item => item.SlotId));
        Assert.All(first.Diagnostics, item => Assert.NotEqual("error", item.Severity));
    }

    [Fact]
    public async Task MetamoduleFixtureProducesCanonicalSlotFrontier()
    {
        var requests = HybridDraftLuaExpansionCatalog.BuildDefaultRequests();
        var request = requests.Single(item => item.ScenarioId == "metamodule_kingdoms" && item.ProducedArtifactFamily == "metamodule_species_archetype_slot_expansion");
        var fixture = HybridDraftLuaExpansionCatalog.BuildFixtures(requests)[request.FixtureId];
        var result = await new HybridDraftLuaExecutorAdapter().ExecuteAsync(request, fixture);

        Assert.Equal("accepted", result.Status);
        Assert.NotNull(result.Output);
        Assert.True(result.Output!.Slots.Count >= 100);
        Assert.Equal(result.Output.Slots.Select(item => item.SlotId).Order(StringComparer.Ordinal), result.Output.Slots.Select(item => item.SlotId));
    }
}

public sealed class HybridDraftLuaOutputValidatorTests
{
    [Fact]
    public async Task ValidatorRejectsCausalFakeLeakCases()
    {
        var evidence = await new HybridDraftLuaExpansionEvidenceService().BuildAsync();
        var matrix = evidence.InvalidMatrix;
        var codes = matrix.Scenarios
            .SelectMany(item => item.Diagnostics)
            .Select(item => item.Code)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        Assert.Contains(matrix.Scenarios, item => item.ScenarioId == "fake_goal034_draft_id" && item.ActualStatus == "rejected");
        Assert.Contains(matrix.Scenarios, item => item.ScenarioId == "fake_goal035_manifest_id" && item.ActualStatus == "rejected");
        Assert.Contains(matrix.Scenarios, item => item.ScenarioId == "fake_goal036_sandbox_decision_id" && item.ActualStatus == "rejected");
        Assert.Contains(matrix.Scenarios, item => item.ScenarioId == "dependency_unavailable_unsafe_adapter_blocker_path" && item.ActualStatus == "blocked");
        Assert.Contains("hybrid.goal034_draft.fake", codes);
        Assert.Contains("hybrid.goal035_manifest.fake", codes);
        Assert.Contains("hybrid.goal036_sandbox_decision.fake", codes);
        Assert.Contains("hybrid.sandbox_denied.executor_attempted", codes);
        Assert.Contains("hybrid.profile.wrong_scenario", codes);
        Assert.Contains("hybrid.final_prose.forbidden", codes);
        Assert.Contains("hybrid.gamepackage_mutation.forbidden", codes);
        Assert.Contains("hybrid.boundary.runtime_mutation.forbidden", codes);
        Assert.Contains("hybrid.boundary.file_system.forbidden", codes);
        Assert.Contains("hybrid.output.budget.exceeded", codes);
        Assert.Contains("hybrid.output.order.nondeterministic", codes);
        Assert.Contains("hybrid.output.trace.missing", codes);
        Assert.Contains("hybrid.self_promotion.forbidden", codes);
        Assert.Contains("hybrid.adapter.blocked", codes);
        Assert.Contains("hybrid.output.malformed", codes);
    }
}

public sealed class HybridDraftLuaEvidenceTests
{
    [Fact]
    public async Task EvidenceBuildIsDeterministicAndKeepsManualGateRequired()
    {
        var service = new HybridDraftLuaExpansionEvidenceService();

        var first = await service.BuildAsync();
        var second = await service.BuildAsync();

        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(first.Report.Accepted);
        Assert.Equal(HybridDraftLuaExpansionVocabulary.FinalGate, first.Report.ManualGate);
        Assert.True(first.Report.RealBoundedExecutorPathProven);
        Assert.Equal(4, first.Report.ScenarioCount);
        Assert.Equal(8, first.Report.OutputCount);
        Assert.True(first.Report.MetamoduleSpeciesArchetypeSlotCount >= 100);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.Equal(first.ArtifactJsonByFileName[HybridDraftLuaExpansionEvidenceService.PipelineSummaryJsonFileName], second.ArtifactJsonByFileName[HybridDraftLuaExpansionEvidenceService.PipelineSummaryJsonFileName]);
        Assert.DoesNotContain(Environment.NewLine, first.ArtifactJsonByFileName[HybridDraftLuaExpansionEvidenceService.PipelineSummaryJsonFileName]);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndParse()
    {
        using var temp = new TempDirectory();
        var write = await new HybridDraftLuaExpansionEvidenceService().BuildAndWriteAsync(temp.Path);
        var names = write.WrittenFiles.Select(path => Path.GetFileName(path) ?? string.Empty).OrderBy(item => item, StringComparer.Ordinal).ToArray();

        Assert.Equal(
            [
                "draft-to-lua-request-map.json",
                "executor-adapter-selection.json",
                "hybrid-llm-draft-lua-deterministic-expansion-report.md",
                "hybrid-pipeline-summary.json",
                "invalid-hybrid-expansion-diagnostics-matrix.json",
                "lua-expansion-output-caravan.json",
                "lua-expansion-output-frontier.json",
                "lua-expansion-output-gothic.json",
                "lua-expansion-output-metamodule-kingdoms.json",
                "promotion-decision-matrix.json",
                "sandbox-approved-expansion-matrix.json"
            ],
            names);

        using var pipeline = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.PipelineSummaryJsonFileName)));
        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.MetamoduleOutputJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.InvalidMatrixJsonFileName)));
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.True(pipeline.RootElement.GetProperty("realBoundedExecutorPathProven").GetBoolean());
        Assert.True(pipeline.RootElement.GetProperty("metamoduleSlotCount").GetInt32() >= 100);
        Assert.Equal("metamodule_kingdoms", metamodule.RootElement.GetProperty("scenarioId").GetString());
        Assert.True(metamodule.RootElement.GetProperty("slotCount").GetInt32() >= 100);
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("hybrid_llm_draft_lua_deterministic_expansion_verification required", report);
        Assert.Contains("realBoundedExecutorPathProven: true", report);
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
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
