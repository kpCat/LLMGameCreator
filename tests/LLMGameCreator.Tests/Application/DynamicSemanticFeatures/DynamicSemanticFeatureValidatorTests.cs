using LLMGameCreator.Application.Design.DynamicSemanticFeatures;
using Xunit;

namespace LLMGameCreator.Tests.Application.DynamicSemanticFeatures;

public sealed class DynamicSemanticFeatureValidatorTests
{
    [Fact]
    public void InvalidFakeLeakMatrixProducesCausalDiagnostics()
    {
        var matrix = DynamicSemanticFeatureEvidenceService.BuildInvalidMatrix();

        Assert.True(matrix.Passed, string.Join(Environment.NewLine, matrix.Scenarios.Select(item => $"{item.ScenarioId}:{item.ExpectedValid}:{item.ActualValid}:{string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code))}")));
        Assert.True(matrix.ScenarioCount >= 16);
        Assert.Equal(matrix.ScenarioCount, matrix.MatchedExpectationCount);
        Assert.Contains(matrix.Scenarios, Scenario("duplicate_feature_id", "dynamic_semantic.feature_id.duplicate"));
        Assert.Contains(matrix.Scenarios, Scenario("invalid_empty_id", "dynamic_semantic.feature_id.invalid"));
        Assert.Contains(matrix.Scenarios, Scenario("unknown_feature_reference", "dynamic_semantic.feature_ref.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("unknown_target_scope", "dynamic_semantic.scope.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("invalid_value_shape", "dynamic_semantic.value_shape.invalid"));
        Assert.Contains(matrix.Scenarios, Scenario("illegal_assignment_scope", "dynamic_semantic.assignment.scope_illegal"));
        Assert.Contains(matrix.Scenarios, Scenario("required_feature_missing", "dynamic_semantic.required_feature.missing"));
        Assert.Contains(matrix.Scenarios, item => item.ScenarioId == "optional_feature_missing_is_traceable" && item.ExpectedValid && item.ActualValid);
        Assert.Contains(matrix.Scenarios, Scenario("feature_conflict", "dynamic_semantic.feature.conflict"));
        Assert.Contains(matrix.Scenarios, Scenario("unknown_inheritance_source", "dynamic_semantic.inheritance.source.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("circular_inheritance", "dynamic_semantic.inheritance.circular"));
        Assert.Contains(matrix.Scenarios, Scenario("unknown_influence_target", "dynamic_semantic.influence.target.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("circular_influence", "dynamic_semantic.influence.circular"));
        Assert.Contains(matrix.Scenarios, Scenario("self_feeding_influence", "dynamic_semantic.influence.self_feeding"));
        Assert.Contains(matrix.Scenarios, Scenario("overconstrained_output", "dynamic_semantic.output.overconstrained"));
        Assert.Contains(matrix.Scenarios, Scenario("fake_selected_feature_id", "dynamic_semantic.target.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("forbidden_leakage_terms", "dynamic_semantic.boundary.leakage"));
    }

    private static Predicate<DynamicSemanticInvalidScenario> Scenario(string scenarioId, string code) =>
        scenario => scenario.ScenarioId == scenarioId && scenario.Diagnostics.Any(diagnostic => diagnostic.Code == code);
}
