using LLMGameCreator.Application.Design.SemanticArtifactContracts;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticArtifactContracts;

public sealed class SemanticArtifactCompatibilityPlannerTests
{
    [Fact]
    public void ThreeScenarioPlansAreDifferentButUseSamePlannerAndRegistry()
    {
        var contracts = SemanticArtifactContractRegistry.BuildDefaultContracts();
        var packs = SemanticArtifactContractRegistry.BuildDefaultSemanticPacks();
        var planner = new SemanticArtifactCompatibilityPlanner(contracts);

        var frontier = planner.BuildPlan(Request("frontier_survival", packs, "semantic_pack/frontier_survival"));
        var gothic = planner.BuildPlan(Request("gothic_intrigue", packs, "semantic_pack/gothic_intrigue"));
        var caravan = planner.BuildPlan(Request("caravan_trade", packs, "semantic_pack/caravan_trade"));

        Assert.DoesNotContain(frontier.Diagnostics, item => item.Severity == "error");
        Assert.DoesNotContain(gothic.Diagnostics, item => item.Severity == "error");
        Assert.DoesNotContain(caravan.Diagnostics, item => item.Severity == "error");
        Assert.Equal("frontier_survival", frontier.ProfileId);
        Assert.Contains("semantic_pack/frontier_survival", frontier.SelectedSemanticPackIds);
        Assert.Contains("semantic_pack/gothic_intrigue", gothic.SelectedSemanticPackIds);
        Assert.Contains("semantic_pack/caravan_trade", caravan.SelectedSemanticPackIds);
        Assert.NotEqual(frontier.StableSummary, gothic.StableSummary);
        Assert.NotEqual(gothic.StableSummary, caravan.StableSummary);
        Assert.Contains(frontier.SemanticExpansionSlots, slot => slot.SlotFamily == "biome_weather_hazard_event_hint");
        Assert.Contains(gothic.SemanticExpansionSlots, slot => slot.SlotFamily == "dialogue_tone_localization_string_table_hint");
        Assert.Contains(caravan.SemanticExpansionSlots, slot => slot.SlotFamily == "item_resource_recipe_loot_hint");
    }

    [Fact]
    public void PlannerKeepsFutureRequiredAndModuleAbsenceExplicit()
    {
        var contracts = SemanticArtifactContractRegistry.BuildDefaultContracts();
        var packs = SemanticArtifactContractRegistry.BuildDefaultSemanticPacks();
        var plan = new SemanticArtifactCompatibilityPlanner(contracts).BuildPlan(Request("frontier_survival", packs, "semantic_pack/frontier_survival"));

        Assert.Contains(plan.BlockedOrFutureRequiredItems, item => item.ContractId == "settlement_building_landmark_v1" && item.Status == "future_required");
        Assert.DoesNotContain("settlement_building_landmark_v1", plan.SelectedContractIds);
        Assert.Contains(plan.SemanticExpansionSlots, slot => slot.TargetArtifactContractId == "settlement_building_landmark_v1" && slot.Status == "future_required");

        var absent = new SemanticArtifactCompatibilityPlanner(contracts).BuildPlan(new SemanticCompatibilityRequest
        {
            ProfileId = "frontier_survival",
            SelectedSemanticPacks = packs.Where(pack => pack.PackId is "semantic_pack/core_generator_spine" or "semantic_pack/frontier_survival").ToList(),
            AvailableModuleIds = new HashSet<string>(SemanticArtifactContractRegistry.DefaultAvailableModuleIds.Where(id => id != "package_assembly_dialogue_quests"), StringComparer.Ordinal)
        });
        Assert.Contains(absent.Diagnostics, item => item.Code == "semantic_plan.module_absent.required");
    }

    [Fact]
    public void SameInputProducesStructurallyEquivalentOutput()
    {
        var packs = SemanticArtifactContractRegistry.BuildDefaultSemanticPacks();
        var planner = new SemanticArtifactCompatibilityPlanner();
        var request = Request("caravan_trade", packs, "semantic_pack/caravan_trade");

        var first = planner.BuildPlan(request);
        var second = planner.BuildPlan(request);

        Assert.Equal(first.SelectedContractIds, second.SelectedContractIds);
        Assert.Equal(first.DependencyOrder, second.DependencyOrder);
        Assert.Equal(first.SemanticExpansionSlots.Select(slot => slot.SlotId), second.SemanticExpansionSlots.Select(slot => slot.SlotId));
        Assert.Equal(first.StableSummary, second.StableSummary);
    }

    private static SemanticCompatibilityRequest Request(string profileId, IReadOnlyList<SemanticPackDescriptor> packs, string profilePackId) =>
        new()
        {
            ProfileId = profileId,
            SelectedSemanticPacks = packs.Where(pack => pack.PackId is "semantic_pack/core_generator_spine" || pack.PackId == profilePackId).ToList()
        };
}
