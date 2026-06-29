using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;
using Xunit;

namespace LLMGameCreator.Tests.Application.LuaModuleManifestRegistry;

public sealed class LuaModuleManifestPlannerTests
{
    [Fact]
    public void ScenarioSelectionsAreDeterministicAndMeaningfullyDifferent()
    {
        var planner = new LuaModuleManifestPlanner();
        var first = planner.PlanDefaultScenarios();
        var second = planner.PlanDefaultScenarios();

        Assert.Equal(
            first.Select(item => item.Summary.StableSummary).Order(StringComparer.Ordinal),
            second.Select(item => item.Summary.StableSummary).Order(StringComparer.Ordinal));
        Assert.All(first, plan => Assert.DoesNotContain(plan.CompatibilityDiagnostics, item => item.Severity == "error"));
        Assert.All(first, plan => Assert.DoesNotContain(plan.DeniedApiDiagnostics, item => item.Severity == "error"));

        var selectedShapes = first
            .Select(item => string.Join("|", item.SelectedManifests.Select(manifest => manifest.FamilyId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        Assert.Equal(4, selectedShapes.Count);
        Assert.True(first.Single(item => item.ScenarioId == "metamodule_kingdoms").SelectedManifests.Count >= 100);
        Assert.NotEqual(
            first.Single(item => item.ScenarioId == "frontier_survival").SelectedManifests.Select(item => item.ModuleId),
            first.Single(item => item.ScenarioId == "gothic_intrigue").SelectedManifests.Select(item => item.ModuleId));
    }

    [Fact]
    public void DependencyOrderIsStableAndPlacesDependenciesBeforeDependents()
    {
        var plan = new LuaModuleManifestPlanner().PlanDefaultScenarios().Single(item => item.ScenarioId == "frontier_survival");
        var order = plan.DependencyOrder.ToList();

        Assert.Empty(plan.MissingDependencies);
        Assert.Equal(order, order.Distinct(StringComparer.Ordinal).ToList());
        Assert.True(order.IndexOf("lua-module/frontier/world-generation-hints") < order.IndexOf("lua-module/frontier/quest-objective-reward-rules"));
        Assert.True(order.IndexOf("lua-module/frontier/item-resource-economy-rules") < order.IndexOf("lua-module/frontier/quest-objective-reward-rules"));
    }
}
