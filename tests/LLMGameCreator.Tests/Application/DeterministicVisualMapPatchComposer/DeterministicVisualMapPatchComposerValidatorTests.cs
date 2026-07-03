using System.Text.Json;
using LLMGameCreator.Application.Design.DeterministicVisualMapPatchComposer;
using Xunit;

namespace LLMGameCreator.Tests.Application.DeterministicVisualMapPatchComposer;

public sealed class DeterministicVisualMapPatchComposerValidatorTests
{
    [Fact]
    public void DefaultMapPatchRequestPassesRequiredFixtureCoverage()
    {
        var request = DeterministicVisualMapPatchComposerFixtures.BuildDefaultRequest();

        var result = DeterministicVisualMapPatchComposerValidator.Validate(request);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.Equal(3, request.Patches.Count);
        Assert.All(DeterministicVisualMapPatchComposerFixtures.RequiredPatchIds, id => Assert.Contains(request.Patches, patch => patch.PatchId == id));
        Assert.All(request.Patches, patch =>
        {
            Assert.Equal(24, patch.Width);
            Assert.Equal(16, patch.Height);
            Assert.Equal(384, patch.Cells.Count);
            Assert.NotEmpty(patch.Layers);
            Assert.NotEmpty(patch.SourceReferences);
        });

        var waterPatch = request.Patches.Single(item => item.PatchId == "water_coast_river_lake_marsh_24x16");
        Assert.Contains(waterPatch.Cells, cell => cell.WaterKind == VisualMapPatchWaterKind.Sea);
        Assert.Contains(waterPatch.Cells, cell => cell.WaterKind == VisualMapPatchWaterKind.Coast);
        Assert.Contains(waterPatch.Cells, cell => cell.WaterKind == VisualMapPatchWaterKind.River);
        Assert.Contains(waterPatch.Cells, cell => cell.WaterKind == VisualMapPatchWaterKind.Lake);
        Assert.Contains(waterPatch.Cells, cell => cell.WaterKind == VisualMapPatchWaterKind.Marsh);
        Assert.Contains(waterPatch.ObjectAnchors, anchor => anchor.ObjectKind == "bridge" && anchor.RequiresWaterAdjacency);
        Assert.Contains(waterPatch.ObjectAnchors, anchor => anchor.ObjectKind == "dock" && anchor.RequiresWaterAdjacency);

        var mixedPatch = request.Patches.Single(item => item.PatchId == "mixed_biome_settlement_creature_24x16");
        Assert.Contains(mixedPatch.SettlementAnchors, anchor => anchor.SettlementId == "riverbend_market");
        Assert.Contains(mixedPatch.CreatureMarkers, marker => marker.BodyPlanId == "bodyplan/humanoid" && marker.EquipmentProfileId == "equipment/caravan_guard");
        Assert.Contains(mixedPatch.Overlays, overlay => overlay.AdultMetadataOnly && overlay.SafeFallbackMicrotilePreviewId == "creature_paperdoll_neutral_slot");
    }

    [Fact]
    public void ReferencedMicrotilesAllExistInGoal086Catalog()
    {
        var repoRoot = FindRepoRoot();
        var catalogPath = Path.Combine(
            repoRoot,
            ".llmgc",
            "procedural",
            "goal-086-deterministic-visual-microtile-materializer",
            "visual-microtile-preview-catalog.json");
        using var catalog = JsonDocument.Parse(File.ReadAllText(catalogPath));
        var knownPreviewIds = catalog.RootElement.GetProperty("previews")
            .EnumerateArray()
            .Select(item => item.GetProperty("previewId").GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        var request = DeterministicVisualMapPatchComposerFixtures.BuildDefaultRequest();

        var referenced = request.Patches
            .SelectMany(DeterministicVisualMapPatchComposerFixtures.ReferencedMicrotilePreviewIds)
            .ToList();

        Assert.NotEmpty(referenced);
        Assert.All(referenced, previewId => Assert.Contains(previewId, knownPreviewIds));
        Assert.Contains("water_coast_transition", referenced);
        Assert.Contains("water_river_segment", referenced);
        Assert.Contains("settlement_wall_gate", referenced);
        Assert.Contains("creature_equipment_clothing_overlay", referenced);
        Assert.Contains("adult_metadata_only_safe_fallback_slot", referenced);
    }

    [Fact]
    public void NegativeProofRejectsUnsafeFakeAndIncompatibleCases()
    {
        var request = DeterministicVisualMapPatchComposerFixtures.BuildDefaultRequest();
        var proof = DeterministicVisualMapPatchComposerEvidenceService.BuildNegativeProof(request);

        Assert.True(proof.Passed, string.Join(Environment.NewLine, proof.Scenarios.SelectMany(item => item.Diagnostics).Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(proof.ScenarioCount >= 12);
        Assert.Equal(proof.ScenarioCount, proof.RejectedCount);
        Assert.Equal(proof.ScenarioCount, proof.MatchedExpectationCount);
        Assert.All(proof.Scenarios, scenario => Assert.False(scenario.ActualValid));

        AssertScenarioHasCode(proof, "absolute_output_path", "visual_map_patch.output_path.invalid");
        AssertScenarioHasCode(proof, "absolute_patch_svg_path", "visual_map_patch.svg_path.invalid");
        AssertScenarioHasCode(proof, "prompt_text_as_source_of_truth", "visual_map_patch.prompt.source_of_truth");
        AssertScenarioHasCode(proof, "unknown_microtile_preview_ref", "visual_map_patch.microtile_ref.unknown");
        AssertScenarioHasCode(proof, "coast_without_water_land_adjacency", "visual_map_patch.water.coast_adjacency_missing");
        AssertScenarioHasCode(proof, "river_without_flow_connectors", "visual_map_patch.water.river_flow_missing");
        AssertScenarioHasCode(proof, "bridge_without_water_adjacency", "visual_map_patch.object.water_adjacency_missing");
        AssertScenarioHasCode(proof, "road_connector_gap", "visual_map_patch.road.connector_gap");
        AssertScenarioHasCode(proof, "settlement_on_water_without_path", "visual_map_patch.settlement.land_invalid");
        AssertScenarioHasCode(proof, "creature_unsafe_missing_bodyplan_equipment", "visual_map_patch.creature.safe_metadata_invalid");
        AssertScenarioHasCode(proof, "adult_metadata_without_safe_fallback", "visual_map_patch.adult.safe_fallback_missing");
        AssertScenarioHasCode(proof, "provider_candidate_treated_as_approved", "visual_map_patch.provider_candidate.treated_as_approved");
        AssertScenarioHasCode(proof, "duplicate_patch_id", "visual_map_patch.patch_id.duplicate");
        AssertScenarioHasCode(proof, "missing_source_lineage", "visual_map_patch.source_lineage.missing");
        AssertScenarioHasCode(proof, "svg_with_script_external_resource_base64", "visual_map_patch.svg.unsafe");
    }

    private static void AssertScenarioHasCode(VisualMapPatchNegativeProof proof, string scenarioId, string code)
    {
        var scenario = proof.Scenarios.Single(item => item.ScenarioId == scenarioId);
        Assert.Contains(scenario.Diagnostics, diagnostic => diagnostic.Code == code);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null && !File.Exists(Path.Combine(current.FullName, "LLMGameCreator.sln")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Repository root could not be resolved.");
        }

        return current.FullName;
    }
}
