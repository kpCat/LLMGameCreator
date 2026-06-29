using LLMGameCreator.Application.Design.DynamicSemanticFeatures;
using Xunit;

namespace LLMGameCreator.Tests.Application.DynamicSemanticFeatures;

public sealed class DynamicSemanticFeatureCatalogTests
{
    [Fact]
    public void SeedCatalogValidatesAndCoversRequiredScopesAndValueKinds()
    {
        var definitions = DynamicSemanticFeatureCatalog.BuildDefaultFeatureDefinitions();
        var rules = DynamicSemanticFeatureCatalog.BuildDefaultInfluenceRules();
        var diagnostics = DynamicSemanticFeatureValidator.ValidateCatalog(definitions, rules);

        Assert.DoesNotContain(diagnostics, item => item.Severity == "error");
        foreach (var scope in DynamicSemanticFeatureVocabulary.ValidScopes)
        {
            Assert.Contains(definitions, item => item.TargetScope == scope);
        }

        foreach (var valueKind in DynamicSemanticFeatureVocabulary.ValidValueKinds)
        {
            Assert.Contains(definitions, item => item.ValueKind == valueKind);
        }
    }

    [Fact]
    public void SeedCatalogContainsFourRequiredProofScenarios()
    {
        var scenarios = DynamicSemanticFeatureCatalog.BuildDefaultScenarios();

        Assert.Contains(scenarios, item => item.ScenarioId == "frontier_survival");
        Assert.Contains(scenarios, item => item.ScenarioId == "gothic_intrigue");
        Assert.Contains(scenarios, item => item.ScenarioId == "caravan_trade");
        Assert.Contains(scenarios, item => item.ScenarioId == "metamodule_kingdoms");

        var metamodule = Assert.Single(scenarios, item => item.ScenarioId == "metamodule_kingdoms");
        Assert.Contains(metamodule.Targets, item => item.TargetScope == "kingdom" && item.TargetId == "kingdom/auric");
        Assert.Contains(metamodule.Targets, item => item.TargetScope == "kingdom" && item.TargetId == "kingdom/umbra");
        Assert.Contains(metamodule.Targets, item => item.TargetScope == "species" && item.TargetId == "species/metamodule_bearer");
        Assert.Contains(metamodule.Targets, item => item.TargetScope == "archetype" && item.TargetId == "archetype/module_scout");
    }
}
