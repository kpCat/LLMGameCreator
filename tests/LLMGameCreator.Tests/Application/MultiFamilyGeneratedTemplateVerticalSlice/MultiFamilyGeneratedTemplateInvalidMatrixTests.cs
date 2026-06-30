using Xunit;

namespace LLMGameCreator.Tests.Application.MultiFamilyGeneratedTemplateVerticalSlice;

public sealed class MultiFamilyGeneratedTemplateInvalidMatrixTests
{
    [Fact]
    public async Task InvalidFakeAndLeakMatrixHasStableCausalDiagnostics()
    {
        using var temp = await MultiFamilyGeneratedTemplateTestFactory.CreateProjectWithGoal037To040SourceAsync();

        var matrix = MultiFamilyGeneratedTemplateTestFactory.CreateService().Build(temp.Path).InvalidMatrix;
        var byId = matrix.Scenarios.ToDictionary(item => item.ScenarioId, StringComparer.Ordinal);

        Assert.True(matrix.Passed);
        Assert.True(matrix.ScenarioCount >= 19);
        AssertCase(byId, "duplicate_family_id", "goal043.catalog.duplicate_family_id", "rejected");
        AssertCase(byId, "unknown_family_id", "goal043.family.unknown", "rejected");
        AssertCase(byId, "unknown_scenario_id", "goal043.scenario.unknown_or_mismatch", "rejected");
        AssertCase(byId, "missing_required_lifecycle_section", "goal043.lifecycle.section_missing", "rejected");
        AssertCase(byId, "missing_preview_export_source_ref", "goal043.source.preview_export_missing", "rejected");
        AssertCase(byId, "missing_chunk_traversal_source_ref", "goal043.source.chunk_traversal_missing", "rejected");
        AssertCase(byId, "fake_goal034_reference", "goal043.source.fake_reference", "rejected");
        AssertCase(byId, "fake_goal035_reference", "goal043.source.fake_reference", "rejected");
        AssertCase(byId, "fake_goal036_reference", "goal043.source.fake_reference", "rejected");
        AssertCase(byId, "fake_goal037_reference", "goal043.source.fake_reference", "rejected");
        AssertCase(byId, "fake_goal038_reference", "goal043.source.fake_reference", "rejected");
        AssertCase(byId, "fake_goal039_reference", "goal043.source.fake_reference", "rejected");
        AssertCase(byId, "fake_goal040_reference", "goal043.source.fake_reference", "rejected");
        AssertCase(byId, "family_specific_field_outside_extension_scope", "goal043.family.extension_scope", "rejected");
        AssertCase(byId, "architecture_fork_attempt", "goal043.architecture_fork.blocked", "blocked");
        AssertCase(byId, "gamepackage_schema_mutation_claim", "goal043.boundary.gamepackage_schema.forbidden", "blocked");
        AssertCase(byId, "runtime_ui_unity_provider_llm_rag_media_lua_source_leakage", "goal043.boundary.runtime_ui_unity.forbidden", "blocked");
        AssertCase(byId, "final_prose_promoted_as_playable_content", "goal043.final_prose.forbidden", "rejected");
        AssertCase(byId, "nondeterministic_ordering", "goal043.order.nondeterministic", "rejected");
        AssertCase(byId, "cross_family_id_collision", "goal043.catalog.cross_family_id_collision", "rejected");
        AssertCase(byId, "scenario_profile_mismatch", "goal043.scenario.profile_mismatch", "rejected");
        AssertCase(byId, "simulatable_loop_proof_without_state_transition", "goal043.loop.state_transition_missing", "rejected");
        AssertCase(byId, "preview_export_payload_copied_without_transformation", "goal043.preview_export.payload_copy", "rejected");
        AssertCase(byId, "missing_validation_trace", "goal043.validation_trace.missing", "rejected");
    }

    private static void AssertCase(
        IReadOnlyDictionary<string, LLMGameCreator.Application.Design.MultiFamilyGeneratedTemplateVerticalSlice.InvalidFamilyDiagnosticsScenario> byId,
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
