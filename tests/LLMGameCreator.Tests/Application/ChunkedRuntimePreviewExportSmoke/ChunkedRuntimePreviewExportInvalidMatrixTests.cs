using LLMGameCreator.Application.Design.ChunkedRuntimePreviewExportSmoke;
using Xunit;

namespace LLMGameCreator.Tests.Application.ChunkedRuntimePreviewExportSmoke;

public sealed class ChunkedRuntimePreviewExportInvalidMatrixTests
{
    [Fact]
    public async Task InvalidFakeAndLeakMatrixHasStableCausalDiagnostics()
    {
        using var temp = await ChunkedRuntimePreviewExportTestFactory.CreateProjectWithGoal039SourceAsync();

        var matrix = ChunkedRuntimePreviewExportTestFactory.CreateService().Build(temp.Path).InvalidMatrix;
        var byId = matrix.Scenarios.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        Assert.Equal(16, matrix.ScenarioCount);
        AssertCase(byId, "missing_goal039_source_evidence", "chunked_consumer.source.goal039_missing", "rejected");
        AssertCase(byId, "fake_scenario_id", "chunked_consumer.scenario.fake", "rejected");
        AssertCase(byId, "fake_chunk_id", "chunked_consumer.chunk.fake", "rejected");
        AssertCase(byId, "static_map_without_runtime_delta", "chunked_consumer.source.goal039_runtime_delta_missing", "rejected");
        AssertCase(byId, "family_lens_forks_core_schema", "chunked_consumer.family.core_schema_fork", "rejected");
        AssertCase(byId, "family_lens_missing_required_consumer_needs", "chunked_consumer.family.needs_missing", "rejected");
        AssertCase(byId, "infinite_window_nondeterministic_seed", "chunked_consumer.infinite.seed_nondeterministic", "rejected");
        AssertCase(byId, "boundary_overflow_invalid_window", "chunked_consumer.infinite.window_invalid", "rejected");
        AssertCase(byId, "package_mutation_attempt", "chunked_consumer.boundary.gamepackage.forbidden", "blocked");
        AssertCase(byId, "runtime_ui_unity_source_mutation_claim", "chunked_consumer.boundary.runtime_ui_unity.forbidden", "blocked");
        AssertCase(byId, "provider_llm_rag_claim", "chunked_consumer.boundary.provider_llm_rag.forbidden", "blocked");
        AssertCase(byId, "lua_execution_claim", "chunked_consumer.boundary.lua.forbidden", "blocked");
        AssertCase(byId, "filesystem_network_process_reflection_thread_time_random_native_interop_claim", "chunked_consumer.boundary.filesystem_network_process_reflection_thread_time_random_native_interop.forbidden", "blocked");
        AssertCase(byId, "final_prose_only_payload", "chunked_consumer.payload.final_prose_only", "rejected");
        AssertCase(byId, "missing_save_load_replay_correlation", "chunked_consumer.persistence.correlation_missing", "rejected");
        AssertCase(byId, "nondeterministic_ordering", "chunked_consumer.order.nondeterministic", "rejected");
    }

    private static void AssertCase(
        IReadOnlyDictionary<string, InvalidChunkedConsumerScenario> byId,
        string scenarioId,
        string expectedCode,
        string expectedStatus)
    {
        Assert.True(byId.TryGetValue(scenarioId, out var scenario), "Missing invalid scenario: " + scenarioId);
        Assert.Equal(expectedStatus, scenario.ActualStatus);
        Assert.False(scenario.ActualValid);
        Assert.Contains(scenario.Diagnostics, item => item.Code == expectedCode);
    }
}
