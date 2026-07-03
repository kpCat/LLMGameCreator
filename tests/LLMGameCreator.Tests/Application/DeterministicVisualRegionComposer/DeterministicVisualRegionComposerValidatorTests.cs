using System.Text.Json;
using LLMGameCreator.Application.Design.DeterministicVisualRegionComposer;
using Xunit;

namespace LLMGameCreator.Tests.Application.DeterministicVisualRegionComposer;

public sealed class DeterministicVisualRegionComposerValidatorTests
{
    [Fact]
    public void DefaultRegionDefinitionPassesRequiredFixtureCoverage()
    {
        var definition = DeterministicVisualRegionComposerFixtures.BuildDefaultDefinition();
        var result = DeterministicVisualRegionComposerValidator.Validate(definition);

        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.Equal("heroes_scale_surface_underground_144x144", definition.RegionId);
        Assert.Equal(144, definition.Width);
        Assert.Equal(144, definition.Height);
        Assert.Equal(2, definition.LayerCount);
        Assert.Equal(24, definition.PatchWidth);
        Assert.Equal(16, definition.PatchHeight);
        Assert.Equal(6, definition.PatchGridColumns);
        Assert.Equal(9, definition.PatchGridRows);
        Assert.Equal(41472, definition.DerivedLogicalCellCount);
        Assert.False(definition.HeavyRawCellMode);
        Assert.Equal(0, definition.ExplicitRawCellRecordCount);

        var surface = definition.Layers.Single(item => item.LayerId == "surface");
        var underground = definition.Layers.Single(item => item.LayerId == "underground");
        Assert.Equal(54, surface.PatchPlacements.Count);
        Assert.Equal(54, underground.PatchPlacements.Count);
        Assert.Equal(108, definition.Layers.SelectMany(item => item.PatchPlacements).Count());
        Assert.Equal(54, surface.Chunks.Count);
        Assert.Equal(54, underground.Chunks.Count);

        Assert.Contains(definition.BiomeBands, item => item.LayerId == "surface" && item.BiomeId == "grass" && item.EstimatedCellCount > 0);
        Assert.Contains(definition.BiomeBands, item => item.LayerId == "surface" && item.BiomeId == "lava_ash" && item.EstimatedCellCount > 0);
        Assert.Contains(definition.BiomeBands, item => item.LayerId == "underground" && item.BiomeId == "underground_water" && item.EstimatedCellCount > 0);
        Assert.Contains(definition.WaterNetwork.Segments, item => item.WaterKind == "river");
        Assert.Contains(definition.WaterNetwork.Segments, item => item.WaterKind == "lava_boundary");
        Assert.Contains(definition.Settlements, item => item.Role == "castle");
        Assert.Contains(definition.Settlements, item => item.Role == "garrison");
        Assert.Contains(definition.Settlements, item => item.Role == "caravan");
        Assert.Contains(definition.ObjectPlacements, item => item.ObjectKind == "bridge");
        Assert.Contains(definition.ObjectPlacements, item => item.ObjectKind == "dock");
        Assert.Contains(definition.CreaturePlacements, item => item.BodyPlanId == "bodyplan/humanoid");
        Assert.Contains(definition.Overlays, item => item.AdultMetadataOnly && item.SafeFallbackRefId == "visual_safe_fallback/public_paperdoll_neutral");
    }

    [Fact]
    public void AllPlacementsReferenceKnownGoal087PatchIdsFromArtifactCatalog()
    {
        var repoRoot = FindRepoRoot();
        var catalogPath = Path.Combine(
            repoRoot,
            ".llmgc",
            "procedural",
            "goal-087-deterministic-visual-map-patch-composer",
            "visual-map-patch-catalog.json");
        using var catalog = JsonDocument.Parse(File.ReadAllText(catalogPath));
        var knownPatchIds = catalog.RootElement.GetProperty("patches")
            .EnumerateArray()
            .Select(item => item.GetProperty("patchId").GetString()!)
            .ToHashSet(StringComparer.Ordinal);
        var definition = DeterministicVisualRegionComposerFixtures.BuildDefaultDefinition();
        var referenced = DeterministicVisualRegionComposerFixtures.ReferencedGoal087PatchIds(definition);

        Assert.Equal(3, knownPatchIds.Count);
        Assert.Equal(3, referenced.Count);
        Assert.All(referenced, patchId => Assert.Contains(patchId, knownPatchIds));
        Assert.Contains("heroes_like_overworld_24x16", referenced);
        Assert.Contains("water_coast_river_lake_marsh_24x16", referenced);
        Assert.Contains("mixed_biome_settlement_creature_24x16", referenced);
    }

    [Fact]
    public void NegativeProofRejectsUnsafeFakeAndIncompatibleCases()
    {
        var definition = DeterministicVisualRegionComposerFixtures.BuildDefaultDefinition();
        var proof = DeterministicVisualRegionComposerEvidenceService.BuildNegativeProof(definition);

        Assert.True(proof.Passed, string.Join(Environment.NewLine, proof.Scenarios.SelectMany(item => item.Diagnostics).Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(proof.ScenarioCount >= 18);
        Assert.Equal(proof.ScenarioCount, proof.RejectedCount);
        Assert.Equal(proof.ScenarioCount, proof.MatchedExpectationCount);
        Assert.All(proof.Scenarios, scenario => Assert.False(scenario.ActualValid));

        AssertScenarioHasCode(proof, "wrong_dimensions", "visual_region.dimensions.invalid");
        AssertScenarioHasCode(proof, "wrong_layer_count", "visual_region.layer_count.invalid");
        AssertScenarioHasCode(proof, "wrong_patch_grid", "visual_region.patch_grid.invalid");
        AssertScenarioHasCode(proof, "unknown_goal087_patch_id", "visual_region.patch_id.unknown");
        AssertScenarioHasCode(proof, "placement_outside_bounds", "visual_region.placement.bounds.invalid");
        AssertScenarioHasCode(proof, "duplicate_patch_coordinate", "visual_region.placement.coordinate.duplicate");
        AssertScenarioHasCode(proof, "missing_water_network", "visual_region.water_network.missing");
        AssertScenarioHasCode(proof, "connector_mismatch", "visual_region.water.connector_mismatch");
        AssertScenarioHasCode(proof, "road_not_connected", "visual_region.road.reachability.disconnected");
        AssertScenarioHasCode(proof, "transition_without_pair", "visual_region.gate_transition.pair_missing");
        AssertScenarioHasCode(proof, "settlement_on_invalid_water", "visual_region.settlement.terrain.invalid");
        AssertScenarioHasCode(proof, "creature_missing_bodyplan_equipment", "visual_region.creature.metadata.missing");
        AssertScenarioHasCode(proof, "adult_rating_without_safe_fallback", "visual_region.adult.safe_fallback_missing");
        AssertScenarioHasCode(proof, "prompt_text_as_source_of_truth", "visual_region.prompt.source_of_truth");
        AssertScenarioHasCode(proof, "provider_candidate_treated_as_approved", "visual_region.provider_candidate.treated_as_approved");
        AssertScenarioHasCode(proof, "absolute_source_path", "visual_region.path.absolute");
        AssertScenarioHasCode(proof, "unsafe_svg_script_external_base64", "visual_region.svg.unsafe");
        AssertScenarioHasCode(proof, "heavy_raw_cell_dump", "visual_region.heavy_raw_cells.forbidden");
    }

    private static void AssertScenarioHasCode(VisualRegionNegativeProof proof, string scenarioId, string code)
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
