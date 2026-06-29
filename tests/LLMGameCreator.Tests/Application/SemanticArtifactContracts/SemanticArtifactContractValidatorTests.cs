using LLMGameCreator.Application.Design.SemanticArtifactContracts;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticArtifactContracts;

public sealed class SemanticArtifactContractValidatorTests
{
    [Fact]
    public void InvalidFakeLeakMatrixProducesCausalDiagnostics()
    {
        var matrix = SemanticArtifactContractEvidenceService.BuildInvalidMatrix();

        Assert.True(matrix.Passed);
        Assert.Equal(9, matrix.ScenarioCount);
        Assert.Equal(9, matrix.RejectedCount);
        Assert.Contains(matrix.Scenarios, Scenario("duplicate_contract_id", "semantic_registry.contract_id.duplicate"));
        Assert.Contains(matrix.Scenarios, Scenario("unknown_dependency", "semantic_registry.dependency.unknown"));
        Assert.Contains(matrix.Scenarios, Scenario("dependency_cycle", "semantic_registry.dependency.cycle"));
        Assert.Contains(matrix.Scenarios, Scenario("missing_semantic_scope", "semantic_registry.semantic_scope.missing"));
        Assert.Contains(matrix.Scenarios, Scenario("incompatible_tag_declaration", "semantic_registry.tags.incompatible"));
        Assert.Contains(matrix.Scenarios, Scenario("future_required_marked_ready", "semantic_registry.lifecycle.future_required_marked_ready"));
        Assert.Contains(matrix.Scenarios, Scenario("leakage_attempt", "semantic_registry.boundary.leakage"));
        Assert.Contains(matrix.Scenarios, Scenario("module_absent_mutation", "semantic_plan.module_absent.required"));
        Assert.Contains(matrix.Scenarios, Scenario("fake_contract_id", "semantic_plan.contract.unknown"));
    }

    [Fact]
    public void ValidatorReturnsDiagnosticsInsteadOfThrowingForOrdinaryFailures()
    {
        var contracts = SemanticArtifactContractRegistry.BuildDefaultContracts();
        var mutated = contracts.Select(contract => contract.ContractId == "semantic_pack_v1"
            ? contract with { Version = "not-a-version", ProducedArtifactTypes = [], LifecycleStatus = "maybe" }
            : contract).ToList();

        var diagnostics = SemanticArtifactContractValidator.ValidateContracts(mutated);

        Assert.Contains(diagnostics, item => item.Code == "semantic_registry.version.invalid");
        Assert.Contains(diagnostics, item => item.Code == "semantic_registry.produced_artifact.missing");
        Assert.Contains(diagnostics, item => item.Code == "semantic_registry.lifecycle.unknown");
    }

    private static Predicate<SemanticArtifactInvalidScenario> Scenario(string scenarioId, string code) =>
        scenario => scenario.ScenarioId == scenarioId && scenario.Diagnostics.Any(diagnostic => diagnostic.Code == code);
}
