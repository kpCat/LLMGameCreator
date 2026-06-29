using LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticAuthoringIntentResolver;

public sealed class SemanticAuthoringIntentValidatorTests
{
    [Fact]
    public void InvalidFakeLeakMatrixProducesCausalDiagnostics()
    {
        var matrix = SemanticAuthoringIntentValidator.BuildInvalidMatrix();

        Assert.True(matrix.Passed, string.Join(Environment.NewLine, matrix.Scenarios.Select(item => $"{item.ScenarioId}:{item.ExpectedValid}:{item.ActualValid}:{string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code))}")));
        Assert.True(matrix.ScenarioCount >= 15);
        Assert.Equal(matrix.ScenarioCount, matrix.MatchedExpectationCount);
        Assert.Contains(matrix.Scenarios, Scenario("duplicate_workspace_field_id", "semantic_authoring.field_id.duplicate"));
        Assert.Contains(matrix.Scenarios, Scenario("unknown_feature_reference", "semantic_authoring.feature_ref.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("unknown_target_domain", "semantic_authoring.domain.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("illegal_feature_domain_applicability", "semantic_authoring.feature_domain.illegal"));
        Assert.Contains(matrix.Scenarios, Scenario("required_manual_field_missing", "semantic_authoring.required_field.missing"));
        Assert.Contains(matrix.Scenarios, item => item.ScenarioId == "optional_absent_field_valid" && item.ExpectedValid && item.ActualValid);
        Assert.Contains(matrix.Scenarios, Scenario("conflicting_provenance_for_same_field", "semantic_authoring.provenance.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("llm_candidate_treated_as_accepted", "semantic_authoring.candidate.not_accepted"));
        Assert.Contains(matrix.Scenarios, Scenario("imported_candidate_treated_as_accepted", "semantic_authoring.candidate.not_accepted"));
        Assert.Contains(matrix.Scenarios, Scenario("final_dialogue_prose_leakage", "semantic_authoring.final_prose.leakage"));
        Assert.Contains(matrix.Scenarios, Scenario("final_gamepackage_materialization_leakage", "semantic_authoring.boundary.leakage"));
        Assert.Contains(matrix.Scenarios, Scenario("runtime_ui_unity_provider_llm_rag_lua_media_boundary_leakage", "semantic_authoring.boundary.leakage"));
        Assert.Contains(matrix.Scenarios, Scenario("fake_intent_target_accepted", "semantic_authoring.intent_target.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("missing_source_feature_trace", "semantic_authoring.intent_trace.missing"));
        Assert.Contains(matrix.Scenarios, Scenario("nondeterministic_ordering_mutation", "semantic_authoring.order.nondeterministic"));
    }

    private static Predicate<SemanticAuthoringIntentInvalidScenario> Scenario(string scenarioId, string code) =>
        scenario => scenario.ScenarioId == scenarioId && scenario.Diagnostics.Any(diagnostic => diagnostic.Code == code);
}
