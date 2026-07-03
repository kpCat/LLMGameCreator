using LLMGameCreator.Application.Design.DeterministicVisualMicrotileMaterializer;
using Xunit;

namespace LLMGameCreator.Tests.Application.DeterministicVisualMicrotileMaterializer;

public sealed class DeterministicVisualMicrotileMaterializerValidatorTests
{
    [Fact]
    public void DefaultMaterializationRequestPassesRequiredFixtureCoverage()
    {
        var request = DeterministicVisualMicrotileMaterializerFixtures.BuildDefaultRequest();

        var result = DeterministicVisualMicrotileMaterializerValidator.Validate(request);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.Equal(DeterministicVisualMicrotileMaterializerFixtures.RequiredPreviewIds.Count, request.Previews.Count);
        Assert.All(DeterministicVisualMicrotileMaterializerFixtures.RequiredPreviewIds, id => Assert.Contains(request.Previews, preview => preview.PreviewId == id));

        var water = request.Previews.Where(preview => preview.Category == VisualMicrotileCategory.Water).ToList();
        Assert.Contains(water, preview => preview.PreviewId == "water_coast_transition" && preview.WaterLandAdjacency is { WaterEdges.Count: > 0, LandEdges.Count: > 0 });
        Assert.Contains(water, preview => preview.PreviewId == "water_river_segment" && preview.FlowConnectors.Count >= 2);
        Assert.Contains(water, preview => preview.PreviewId == "water_marsh_swamp");
        Assert.Contains(water, preview => preview.PreviewId == "water_bridge_dock_anchor");

        var creature = request.Previews.Where(preview => preview.Category == VisualMicrotileCategory.CreatureNpc).ToList();
        Assert.Contains(creature, preview => preview.PreviewId == "creature_bodyplan_silhouette");
        Assert.Contains(creature, preview => preview.PreviewId == "creature_equipment_clothing_overlay");
        Assert.Contains(creature, preview => preview.PreviewId == "creature_damaged_dirty_worn_state");
        Assert.Contains(creature, preview => preview.PreviewId == "creature_paperdoll_neutral_slot");

        var adult = request.Previews.Single(preview => preview.PreviewId == "adult_metadata_only_safe_fallback_slot");
        Assert.True(adult.AdultMetadataOnly);
        Assert.Equal(VisualMicrotileProviderState.CandidateQuarantine, adult.ProviderState);
        Assert.Equal("creature_paperdoll_neutral_slot", adult.SafeFallbackPreviewId);
        Assert.False(adult.TreatProviderCandidateAsApprovedOutput);
    }

    [Fact]
    public void NegativeProofRejectsUnsafeFakeAndMissingMaterializerCases()
    {
        var request = DeterministicVisualMicrotileMaterializerFixtures.BuildDefaultRequest();
        var proof = DeterministicVisualMicrotileMaterializerEvidenceService.BuildNegativeProof(request);

        Assert.True(proof.Passed, string.Join(Environment.NewLine, proof.Scenarios.SelectMany(item => item.Diagnostics).Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(proof.ScenarioCount >= 12);
        Assert.Equal(proof.ScenarioCount, proof.RejectedCount);
        Assert.Equal(proof.ScenarioCount, proof.MatchedExpectationCount);
        Assert.All(proof.Scenarios, scenario => Assert.False(scenario.ActualValid));

        AssertScenarioHasCode(proof, "absolute_output_path", "visual_microtile.output_path.invalid");
        AssertScenarioHasCode(proof, "prompt_text_as_source_of_truth", "visual_microtile.prompt.source_of_truth");
        AssertScenarioHasCode(proof, "missing_palette", "visual_microtile.palette.missing");
        AssertScenarioHasCode(proof, "missing_layer_stack", "visual_microtile.layer_stack.missing");
        AssertScenarioHasCode(proof, "coast_without_water_land_adjacency", "visual_microtile.water.coast_adjacency_missing");
        AssertScenarioHasCode(proof, "river_without_flow_connectors", "visual_microtile.water.river_flow_missing");
        AssertScenarioHasCode(proof, "adult_capable_without_safe_fallback", "visual_microtile.adult.safe_fallback_missing");
        AssertScenarioHasCode(proof, "provider_candidate_treated_as_approved_output", "visual_microtile.provider_candidate.treated_as_approved");
        AssertScenarioHasCode(proof, "missing_seed", "visual_microtile.seed.missing_or_nondeterministic");
        AssertScenarioHasCode(proof, "svg_with_script_external_resource_base64", "visual_microtile.svg.unsafe");
        AssertScenarioHasCode(proof, "duplicate_preview_id", "visual_microtile.preview_id.duplicate");
        AssertScenarioHasCode(proof, "missing_goal084_085_lineage", "visual_microtile.source_lineage.missing");
    }

    private static void AssertScenarioHasCode(VisualMicrotileNegativeProof proof, string scenarioId, string code)
    {
        var scenario = proof.Scenarios.Single(item => item.ScenarioId == scenarioId);
        Assert.Contains(scenario.Diagnostics, diagnostic => diagnostic.Code == code);
    }
}
