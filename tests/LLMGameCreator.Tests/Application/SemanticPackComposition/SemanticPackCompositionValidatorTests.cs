using LLMGameCreator.Application.Design.SemanticPackComposition;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticPackComposition;

public sealed class SemanticPackCompositionValidatorTests
{
    [Fact]
    public void InvalidFakeLeakMatrixProducesCausalDiagnostics()
    {
        var matrix = SemanticPackCompositionEvidenceService.BuildInvalidMatrix();

        Assert.True(matrix.Passed);
        Assert.Equal(10, matrix.ScenarioCount);
        Assert.Equal(10, matrix.RejectedCount);
        Assert.Contains(matrix.Scenarios, Scenario("duplicate_pack_id_mutation", "semantic_pack.catalog.pack_id.duplicate"));
        Assert.Contains(matrix.Scenarios, Scenario("unknown_profile_family_mutation", "semantic_pack.request.profile.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("missing_semantic_scope_mutation", "semantic_pack.scope.missing"));
        Assert.Contains(matrix.Scenarios, Scenario("duplicate_fact_id_mutation", "semantic_pack.fact_id.duplicate"));
        Assert.Contains(matrix.Scenarios, Scenario("unknown_fact_relation_mutation", "semantic_pack.relation.fact.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("fake_goal030_contract_mutation", "semantic_pack.expansion_intent.contract.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("fake_goal030_contract_mutation", "semantic_pack.expansion_intent.artifact_kind.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("incompatible_pack_selection_mutation", "semantic_pack.selection.exclusion.incompatible"));
        Assert.Contains(matrix.Scenarios, Scenario("future_only_pack_selected_mutation", "semantic_pack.request.pack.future_only"));
        Assert.Contains(matrix.Scenarios, Scenario("fake_selected_pack_id_mutation", "semantic_pack.request.pack_id.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("leakage_attempt_mutation", "semantic_pack.boundary.leakage"));
    }

    [Fact]
    public void ValidatorReturnsDiagnosticsInsteadOfThrowingForOrdinaryFailures()
    {
        var packs = SemanticPackCompositionCatalog.BuildDefaultPacks()
            .Select(pack => pack.PackId == "semantic_pack/frontier_survival"
                ? pack with
                {
                    PackId = "Invalid Pack Id",
                    SupportedProfileIds = ["unknown"],
                    ProvidedSemanticScopes = [],
                    Facts = [pack.Facts[0] with { Domain = "unknown_domain" }]
                }
                : pack)
            .ToList();

        var diagnostics = SemanticPackCompositionValidator.ValidateCatalog(packs);

        Assert.Contains(diagnostics, item => item.Code == "semantic_pack.pack_id.invalid");
        Assert.Contains(diagnostics, item => item.Code == "semantic_pack.profile.unknown");
        Assert.Contains(diagnostics, item => item.Code == "semantic_pack.scope.missing");
        Assert.Contains(diagnostics, item => item.Code == "semantic_pack.fact.domain.invalid");
    }

    private static Predicate<SemanticPackCompositionInvalidScenario> Scenario(string scenarioId, string code) =>
        scenario => scenario.ScenarioId == scenarioId && scenario.Diagnostics.Any(diagnostic => diagnostic.Code == code);
}
