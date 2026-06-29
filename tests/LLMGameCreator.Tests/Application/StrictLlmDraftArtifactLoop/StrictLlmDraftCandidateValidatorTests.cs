using LLMGameCreator.Application.Design.StrictLlmDraftArtifactLoop;
using Xunit;

namespace LLMGameCreator.Tests.Application.StrictLlmDraftArtifactLoop;

public sealed class StrictLlmDraftCandidateValidatorTests
{
    [Fact]
    public void CandidateQuarantineDefaultsAreSafeAndValidateCleanly()
    {
        var requestSets = StrictLlmDraftArtifactLoopCatalog.BuildDefaultRequestSets();
        var requests = requestSets.SelectMany(item => item.Requests).OrderBy(item => item.RequestId, StringComparer.Ordinal).ToList();
        var candidates = StrictLlmDraftArtifactLoopCatalog.BuildProgrammaticFixtureCandidates(requestSets);

        Assert.NotEmpty(candidates);
        Assert.All(candidates, candidate =>
        {
            Assert.Equal("quarantined", candidate.Status);
            Assert.Equal("programmatic_fixture", candidate.SourceKind);
            Assert.DoesNotContain(candidate.PayloadFields, field => field.FinalProse);
            Assert.DoesNotContain(candidate.PayloadFields, field => field.Value.Contains("GamePackage", StringComparison.OrdinalIgnoreCase));
        });
        Assert.DoesNotContain(StrictLlmDraftArtifactLoopValidator.ValidateCandidates(requests, candidates), item => item.Severity == "error");
    }

    [Fact]
    public void InvalidFakeLeakMatrixProducesCausalDiagnostics()
    {
        var matrix = StrictLlmDraftArtifactLoopValidator.BuildInvalidMatrix();

        Assert.True(matrix.Passed, string.Join(Environment.NewLine, matrix.Scenarios.Select(item => $"{item.ScenarioId}:{string.Join(",", item.Diagnostics.Select(diagnostic => diagnostic.Code))}")));
        Assert.True(matrix.ScenarioCount >= 18);
        Assert.Contains(matrix.Scenarios, Scenario("duplicate_request_id", "strict_draft.request_id.duplicate"));
        Assert.Contains(matrix.Scenarios, Scenario("duplicate_candidate_id", "strict_draft.candidate_id.duplicate"));
        Assert.Contains(matrix.Scenarios, Scenario("unknown_request", "strict_draft.request.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("wrong_family", "strict_draft.family.wrong"));
        Assert.Contains(matrix.Scenarios, Scenario("missing_required_field", "strict_draft.required_field.missing"));
        Assert.Contains(matrix.Scenarios, Scenario("forbidden_final_prose_field", "strict_draft.final_prose.forbidden"));
        Assert.Contains(matrix.Scenarios, Scenario("provider_runtime_ui_unity_lua_gamepackage_code_leakage", "strict_draft.boundary.leakage"));
        Assert.Contains(matrix.Scenarios, Scenario("candidate_self_marked_promoted", "strict_draft.candidate.self_promoted"));
        Assert.Contains(matrix.Scenarios, Scenario("source_provenance_mismatch", "strict_draft.source_kind.mismatch"));
        Assert.Contains(matrix.Scenarios, Scenario("missing_intent_trace", "strict_draft.intent_trace.missing"));
        Assert.Contains(matrix.Scenarios, Scenario("missing_contract_trace", "strict_draft.contract_trace.missing"));
        Assert.Contains(matrix.Scenarios, Scenario("fake_target_contract", "strict_draft.contract.fake"));
        Assert.Contains(matrix.Scenarios, Scenario("fake_semantic_scope", "strict_draft.semantic_scope.fake"));
        Assert.Contains(matrix.Scenarios, Scenario("incompatible_scenario_profile", "strict_draft.scenario.incompatible"));
        Assert.Contains(matrix.Scenarios, Scenario("over_budget_candidate_count", "strict_draft.candidate_count.over_budget"));
        Assert.Contains(matrix.Scenarios, Scenario("invalid_repair_target", "strict_draft.repair_target.invalid"));
        Assert.Contains(matrix.Scenarios, Scenario("repair_attempts_immutable_mutation", "strict_draft.repair.immutable_field_mutation"));
        Assert.Contains(matrix.Scenarios, Scenario("nondeterministic_ordering_mutation", "strict_draft.order.nondeterministic"));
    }

    private static Predicate<StrictLlmDraftInvalidScenario> Scenario(string scenarioId, string code) =>
        scenario => scenario.ScenarioId == scenarioId && scenario.Diagnostics.Any(diagnostic => diagnostic.Code == code);
}
