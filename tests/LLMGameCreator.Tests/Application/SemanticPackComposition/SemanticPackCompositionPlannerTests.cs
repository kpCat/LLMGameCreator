using LLMGameCreator.Application.Design.SemanticPackComposition;
using Xunit;

namespace LLMGameCreator.Tests.Application.SemanticPackComposition;

public sealed class SemanticPackCompositionPlannerTests
{
    [Fact]
    public void ThreeScenarioBlueprintsAreDifferentButUseSameComposerAndGoal030PlannerPath()
    {
        var planner = new SemanticPackCompositionPlanner();

        var frontier = planner.BuildBlueprint(SemanticPackCompositionCatalog.FrontierRequest());
        var gothic = planner.BuildBlueprint(SemanticPackCompositionCatalog.GothicRequest());
        var caravan = planner.BuildBlueprint(SemanticPackCompositionCatalog.CaravanRequest());

        Assert.DoesNotContain(frontier.Diagnostics, item => item.Severity == "error");
        Assert.DoesNotContain(gothic.Diagnostics, item => item.Severity == "error");
        Assert.DoesNotContain(caravan.Diagnostics, item => item.Severity == "error");
        Assert.Equal("frontier_survival", frontier.ProfileId);
        Assert.Equal("gothic_intrigue", gothic.ProfileId);
        Assert.Equal("caravan_trade", caravan.ProfileId);
        Assert.Contains("semantic_pack/frontier_survival", frontier.SelectedPackIds);
        Assert.Contains("semantic_pack/gothic_intrigue", gothic.SelectedPackIds);
        Assert.Contains("semantic_pack/caravan_trade", caravan.SelectedPackIds);
        Assert.Contains("semantic_pack_v1", frontier.Goal030CoverageContractIds);
        Assert.Contains("semantic_pack_v1", gothic.Goal030CoverageContractIds);
        Assert.Contains("semantic_pack_v1", caravan.Goal030CoverageContractIds);
        Assert.NotEqual(frontier.StableSummary, gothic.StableSummary);
        Assert.NotEqual(gothic.StableSummary, caravan.StableSummary);
    }

    [Fact]
    public void BlueprintContainsRequiredSectionsAndCrossArtifactLinks()
    {
        var plan = new SemanticPackCompositionPlanner().BuildBlueprint(SemanticPackCompositionCatalog.CaravanRequest());

        Assert.Equal(11, plan.Sections.Count);
        Assert.Contains(plan.Sections, section => section.SectionId == "world_route_pressure");
        Assert.Contains(plan.Sections, section => section.SectionId == "biome_weather_hazard_event_pressure");
        Assert.Contains(plan.Sections, section => section.SectionId == "factions_reputation_social_relations");
        Assert.Contains(plan.Sections, section => section.SectionId == "npc_archetype_variation");
        Assert.Contains(plan.Sections, section => section.SectionId == "quest_motive_objective_reward_patterns");
        Assert.Contains(plan.Sections, section => section.SectionId == "dialogue_localization_hints");
        Assert.Contains(plan.Sections, section => section.SectionId == "economy_resource_recipe_loot_chains");
        Assert.Contains(plan.Sections, section => section.SectionId == "combat_progression_ability_pressure");
        Assert.Contains(plan.Sections, section => section.SectionId == "settlement_landmark_anchors");
        Assert.Contains(plan.Sections, section => section.SectionId == "global_events");
        Assert.Contains(plan.Sections, section => section.SectionId == "coverage_gaps_future_required");
        Assert.Contains(plan.CrossArtifactLinks, link => link.LinkId == "faction_npc_quest_dialogue" && link.FactPath.Count >= 4);
        Assert.Contains(plan.CrossArtifactLinks, link => link.LinkId == "biome_resource_economy_loot" && link.FactPath.Count >= 3);
        Assert.Contains(plan.CrossArtifactLinks, link => link.LinkId == "settlement_landmark_route_event" && link.FactPath.Count >= 3);
        Assert.Contains(plan.CrossArtifactLinks, link => link.LinkId == "combat_progression_reward_pattern" && link.FactPath.Count >= 3);
    }

    [Fact]
    public void SameInputProducesStructurallyEquivalentBlueprint()
    {
        var planner = new SemanticPackCompositionPlanner();
        var request = SemanticPackCompositionCatalog.FrontierRequest();

        var first = planner.BuildBlueprint(request);
        var second = planner.BuildBlueprint(request);

        Assert.Equal(first.SelectedPackIds, second.SelectedPackIds);
        Assert.Equal(first.MergedSemanticFacts.Select(fact => fact.FactId), second.MergedSemanticFacts.Select(fact => fact.FactId));
        Assert.Equal(first.RelationGraph.Select(relation => relation.RelationId), second.RelationGraph.Select(relation => relation.RelationId));
        Assert.Equal(first.ResolvedExpansionIntents.Select(intent => intent.IntentId), second.ResolvedExpansionIntents.Select(intent => intent.IntentId));
        Assert.Equal(first.CrossArtifactLinks.Select(link => link.LinkId), second.CrossArtifactLinks.Select(link => link.LinkId));
        Assert.Equal(first.StableSummary, second.StableSummary);
    }
}
