using LLMGameCreator.Application.Design.VisualPartPackRuleStack;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualPartPackRuleStack;

public sealed class VisualPartPackRuleStackValidatorTests
{
    [Fact]
    public void DefaultRuleStackManifestPassesRequiredFixtureCoverage()
    {
        var manifest = VisualPartPackRuleStackFixtures.BuildDefaultManifest();

        var result = VisualPartPackRuleStackValidator.Validate(manifest);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.Equal(VisualPartPackRuleStackFixtures.RequiredFixturePackIds.Count, manifest.PartPacks.Count);
        Assert.All(VisualPartPackRuleStackFixtures.RequiredFixturePackIds, id => Assert.Contains(manifest.PartPacks, pack => pack.PackId == id));

        var water = manifest.PartPacks.Single(pack => pack.PackId == "water_coast_river_marsh_part_pack");
        var waterKinds = water.WaterProfiles.SelectMany(profile => profile.WaterKinds).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("coast", waterKinds);
        Assert.Contains("river", waterKinds);
        Assert.Contains("lake", waterKinds);
        Assert.Contains("marsh", waterKinds);

        var creature = manifest.PartPacks.Single(pack => pack.PackId == "creature_bodyplan_equipment_part_pack");
        Assert.True(creature.BodyPlanGrammarCapacity >= 100);
        Assert.Equal(0, creature.HandAuthoredSpeciesAssetCount);
        Assert.NotEmpty(creature.EquipmentOverlayProfiles);

        var adult = manifest.PartPacks.Single(pack => pack.PackId == "adult_rating_gated_extension_metadata_only");
        Assert.True(adult.IsAdultRatingExtension);
        Assert.Equal(VisualPartProviderState.CandidateQuarantine, adult.ProviderState);
        Assert.Equal("creature_bodyplan_equipment_part_pack", adult.SafeFallbackPackId);
    }

    [Fact]
    public void NegativeProofRejectsUnsafeFakeAndMissingRuleStackCases()
    {
        var manifest = VisualPartPackRuleStackFixtures.BuildDefaultManifest();
        var proof = VisualPartPackRuleStackEvidenceService.BuildNegativeProof(manifest);

        Assert.True(proof.Passed, string.Join(Environment.NewLine, proof.Scenarios.SelectMany(item => item.Diagnostics).Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(proof.ScenarioCount >= 16);
        Assert.Equal(proof.ScenarioCount, proof.RejectedCount);
        Assert.All(proof.Scenarios, scenario => Assert.False(scenario.ActualValid));

        AssertScenarioHasCode(proof, "duplicate_ids", "visual_part_pack.pack_id.duplicate");
        AssertScenarioHasCode(proof, "absolute_path_rejected", "visual_part_pack.path.invalid");
        AssertScenarioHasCode(proof, "missing_layered_masks_sockets_anchors", "visual_part_pack.layered_part.binding_missing");
        AssertScenarioHasCode(proof, "unknown_palette_ref", "visual_part_pack.recipe.palette_ref.unknown");
        AssertScenarioHasCode(proof, "missing_adult_safe_fallback", "visual_part_pack.adult.fallback_missing");
        AssertScenarioHasCode(proof, "adult_without_eligible_body_plan", "visual_part_pack.adult.body_plan.ineligible");
        AssertScenarioHasCode(proof, "water_without_coast_river_lake", "visual_part_pack.water.coverage_missing");
        AssertScenarioHasCode(proof, "tile_without_transition_autotile", "visual_part_pack.tile.transition_autotile_missing");
        AssertScenarioHasCode(proof, "creature_without_body_plan_rules", "visual_part_pack.creature.body_plan_rules_missing");
        AssertScenarioHasCode(proof, "equipment_overlay_without_socket", "visual_part_pack.equipment.socket_compatibility_missing");
        AssertScenarioHasCode(proof, "ui_effect_without_safe_fallback", "visual_part_pack.ui_effect.fallback_missing");
        AssertScenarioHasCode(proof, "prompt_text_as_source_of_truth", "visual_part_pack.prompt.source_of_truth");
        AssertScenarioHasCode(proof, "provider_candidate_treated_as_approved", "visual_part_pack.provider_candidate.treated_as_approved");
        AssertScenarioHasCode(proof, "cyclic_recipe_dependencies", "visual_part_pack.recipe_dependency.cycle");
        AssertScenarioHasCode(proof, "unsafe_export_policy_contradiction", "visual_part_pack.export_policy.contradiction");
        AssertScenarioHasCode(proof, "unknown_recipe_ref", "visual_part_pack.recipe_ref.unknown");
    }

    private static void AssertScenarioHasCode(VisualPartPackNegativeProof proof, string scenarioId, string code)
    {
        var scenario = proof.Scenarios.Single(item => item.ScenarioId == scenarioId);
        Assert.Contains(scenario.Diagnostics, diagnostic => diagnostic.Code == code);
    }
}
