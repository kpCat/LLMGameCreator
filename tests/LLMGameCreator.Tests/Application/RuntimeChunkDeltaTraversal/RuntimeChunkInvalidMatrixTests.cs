using Xunit;

namespace LLMGameCreator.Tests.Application.RuntimeChunkDeltaTraversal;

public sealed class RuntimeChunkInvalidMatrixTests
{
    [Fact]
    public void InvalidFakeAndLeakMatrixGivesCausalDiagnostics()
    {
        var matrix = RuntimeChunkDeltaTraversalTestFactory.CreateService().Build().InvalidMatrix;
        var byId = matrix.Scenarios.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        Assert.Equal(13, matrix.ScenarioCount);
        AssertCase(byId, "fake_goal038_scenario_id", "runtime_chunk.goal038_scenario.fake", "rejected");
        AssertCase(byId, "fake_region_id", "runtime_chunk.region.unknown", "rejected");
        AssertCase(byId, "fake_chunk_id", "runtime_chunk.chunk.unknown", "rejected");
        AssertCase(byId, "route_edge_not_in_reachability_plan", "runtime_chunk.route.edge_unreachable", "rejected");
        AssertCase(byId, "chunk_coordinate_outside_bounds", "runtime_chunk.coordinate.out_of_bounds", "rejected");
        AssertCase(byId, "duplicate_delta_id", "runtime_chunk.delta.duplicate", "rejected");
        AssertCase(byId, "conflicting_delta_mutation", "runtime_chunk.delta.conflict", "rejected");
        AssertCase(byId, "replay_seed_mismatch", "runtime_chunk.replay.seed_mismatch", "rejected");
        AssertCase(byId, "mutation_tries_to_edit_gamepackage_definitions", "runtime_chunk.boundary.gamepackage.forbidden", "blocked");
        AssertCase(byId, "runtime_ui_unity_provider_llm_rag_lua_generator_library_leakage", "runtime_chunk.boundary.ui.forbidden", "blocked");
        AssertCase(byId, "filesystem_network_process_reflection_thread_time_random_native_interop_leakage", "runtime_chunk.boundary.network.forbidden", "blocked");
        AssertCase(byId, "missing_save_load_proof", "runtime_chunk.persistence.missing", "rejected");
        AssertCase(byId, "nondeterministic_ordering", "runtime_chunk.order.nondeterministic", "rejected");
    }

    private static void AssertCase(
        IReadOnlyDictionary<string, LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal.RuntimeChunkInvalidScenario> byId,
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
