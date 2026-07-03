using LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;
using Xunit;

namespace LLMGameCreator.Tests.Application.ParameterizedVisualWorldProfiles;

public sealed class ParameterizedVisualWorldProfilesValidatorTests
{
    [Fact]
    public void ArbitraryFiniteSizeMatrixValidatesThroughGenericProfilePath()
    {
        var profiles = ParameterizedVisualWorldProfilesFixtures.BuildProfiles();
        var matrixProfile = profiles.Single(item => item.ProfileId == "finite_custom_sizes_matrix");
        var sizeMatrix = ParameterizedVisualWorldProfilesEvidenceService.BuildSizeMatrix(profiles);

        Assert.True(sizeMatrix.Passed, string.Join(Environment.NewLine, sizeMatrix.Rows.SelectMany(item => item.Diagnostics).Select(item => $"{item.Code}:{item.Target}")));
        Assert.Equal(6, sizeMatrix.Rows.Count);
        Assert.Contains(sizeMatrix.Rows, item => item.Width == 1 && item.Height == 1);
        Assert.Contains(sizeMatrix.Rows, item => item.Width == 17 && item.Height == 31);
        Assert.Contains(sizeMatrix.Rows, item => item.Width == 64 && item.Height == 96);
        Assert.Contains(sizeMatrix.Rows, item => item.Width == 144 && item.Height == 144);
        Assert.Contains(sizeMatrix.Rows, item => item.Width == 255 && item.Height == 257);
        Assert.Contains(sizeMatrix.Rows, item => item.Width == 512 && item.Height == 384);
        Assert.All(sizeMatrix.Rows, row => Assert.True(row.ValidatorPassed));

        Assert.Equal(
            new[] { "interior", "terrain", "weather_overlay" },
            matrixProfile.Layers.Select(item => item.LayerId).OrderBy(item => item, StringComparer.Ordinal).ToArray());
        Assert.False(matrixProfile.RequiresSurfaceUndergroundOnly);
    }

    [Fact]
    public void BenchmarkHeroesSizeIsFixtureMetadataAndNotArchitectureLimit()
    {
        var profiles = ParameterizedVisualWorldProfilesFixtures.BuildProfiles();
        var benchmark = profiles.Single(item => item.ProfileId == "benchmark_heroes_144x144_surface_underground");
        var matrixProfile = profiles.Single(item => item.ProfileId == "finite_custom_sizes_matrix");
        var validation = ParameterizedVisualWorldProfilesValidator.Validate(benchmark);

        Assert.True(validation.Passed, string.Join(Environment.NewLine, validation.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(benchmark.IsBenchmarkProfile);
        Assert.Equal(144, benchmark.FiniteWidth);
        Assert.Equal(144, benchmark.FiniteHeight);
        Assert.Contains("not an architectural size limit", benchmark.BenchmarkNote);
        Assert.Contains(matrixProfile.FiniteSizeSamples, item => item.Width == 255 && item.Height == 257);
        Assert.DoesNotContain("144x144", benchmark.FixedSizeAllowlist);
    }

    [Fact]
    public void HugeSparseAndInfiniteProfilesValidateWithoutRawCellDumps()
    {
        var profiles = ParameterizedVisualWorldProfilesFixtures.BuildProfiles();
        var huge = profiles.Single(item => item.ProfileId == "huge_sparse_100000x100000_multilayer");
        var infinite = profiles.Single(item => item.ProfileId == "infinite_streaming_world_multilayer");
        var sparseProof = ParameterizedVisualWorldProfilesEvidenceService.BuildSparseWorldProof(profiles);

        Assert.True(ParameterizedVisualWorldProfilesValidator.Validate(huge).Passed);
        Assert.True(ParameterizedVisualWorldProfilesValidator.Validate(infinite).Passed);
        Assert.True(sparseProof.Passed);
        Assert.Equal(30_000_000_000L, huge.LogicalCellCount);
        Assert.False(huge.RawCellDumpAllowed);
        Assert.True(huge.SparseRegionIndex.SparseOnly);
        Assert.Equal(4, huge.SparseRegionIndex.MaterializedChunks.Count);
        Assert.True(ParameterizedVisualWorldProfilesValidator.EstimateFiniteChunkCapacity(huge) > huge.SparseRegionIndex.MaterializedChunks.Count);
        Assert.True(infinite.IsInfinite);
        Assert.Null(infinite.LogicalCellCount);
        Assert.False(infinite.RawCellDumpAllowed);
        Assert.True(infinite.SparseRegionIndex.SparseOnly);
        Assert.NotEmpty(infinite.StreamWindows);
    }

    [Fact]
    public void DeterministicChunkKeysAreStableAndVariantSensitive()
    {
        var profiles = ParameterizedVisualWorldProfilesFixtures.BuildProfiles();
        var proof = ParameterizedVisualWorldProfilesEvidenceService.BuildChunkAddressProof(profiles);

        Assert.True(proof.Passed);
        Assert.True(proof.StableAcrossReruns);
        Assert.True(proof.DiffersBySeedLayerChunkAndVersion);
        Assert.All(proof.Rows, row =>
        {
            Assert.Equal(row.FirstKey, row.SecondKey);
            Assert.NotEqual(row.FirstKey, row.VariantSeedKey);
            Assert.NotEqual(row.FirstKey, row.VariantLayerKey);
            Assert.NotEqual(row.FirstKey, row.VariantChunkKey);
            Assert.NotEqual(row.FirstKey, row.VariantVersionKey);
        });
    }

    [Fact]
    public void LayerSetsAreDataDrivenAndNegativeMatrixRejectsExpectedCases()
    {
        var profiles = ParameterizedVisualWorldProfilesFixtures.BuildProfiles();
        var layerProof = ParameterizedVisualWorldProfilesEvidenceService.BuildLayerModelProof(profiles);
        var negativeProof = ParameterizedVisualWorldProfilesEvidenceService.BuildNegativeProof();

        Assert.True(layerProof.Passed);
        Assert.True(layerProof.NotRestrictedToSurfaceUnderground);
        Assert.Contains(layerProof.Rows, row => row.LayerIds.Contains("underwater", StringComparer.Ordinal));
        Assert.Contains(layerProof.Rows, row => row.LayerIds.Contains("interior", StringComparer.Ordinal));
        Assert.Contains(layerProof.Rows, row => row.LayerIds.Contains("weather_overlay", StringComparer.Ordinal));

        Assert.True(negativeProof.Passed, string.Join(Environment.NewLine, negativeProof.Scenarios.SelectMany(item => item.Diagnostics).Select(item => $"{item.Code}:{item.Target}")));
        Assert.Equal(negativeProof.ScenarioCount, negativeProof.RejectedCount);
        AssertScenarioHasCode(negativeProof, "fixed_size_only_profile_claims_generic", "visual_world.fixed_size_only.forbidden");
        AssertScenarioHasCode(negativeProof, "finite_invalid_dimensions", "visual_world.dimension.invalid");
        AssertScenarioHasCode(negativeProof, "huge_attempts_raw_cell_dump", "visual_world.raw_cell_dump.forbidden");
        AssertScenarioHasCode(negativeProof, "infinite_declares_finite_only_materialization", "visual_world.infinite.finite_materialization");
        AssertScenarioHasCode(negativeProof, "invalid_layer_id", "visual_world.layer_id.invalid");
        AssertScenarioHasCode(negativeProof, "duplicate_layer_ids", "visual_world.layer_id.duplicate");
        AssertScenarioHasCode(negativeProof, "hardcoded_surface_underground_only_requirement", "visual_world.surface_underground_only.forbidden");
        AssertScenarioHasCode(negativeProof, "chunk_size_zero", "visual_world.chunk_size.invalid");
        AssertScenarioHasCode(negativeProof, "patch_size_zero", "visual_world.patch_size.invalid");
        AssertScenarioHasCode(negativeProof, "patch_chunk_incompatibility", "visual_world.patch_chunk.incompatible");
        AssertScenarioHasCode(negativeProof, "missing_world_seed", "visual_world.seed.missing");
        AssertScenarioHasCode(negativeProof, "missing_generator_version", "visual_world.generator_version.missing");
        AssertScenarioHasCode(negativeProof, "absolute_output_path", "visual_world.path.absolute");
        AssertScenarioHasCode(negativeProof, "non_deterministic_chunk_key", "visual_world.chunk_key.nondeterministic");
        AssertScenarioHasCode(negativeProof, "layer_link_unknown_layer", "visual_world.layer_link.unknown_layer");
        AssertScenarioHasCode(negativeProof, "stream_window_without_center", "visual_world.stream_window.invalid");
        AssertScenarioHasCode(negativeProof, "rating_metadata_without_safe_fallback", "visual_world.rating.safe_fallback_missing");
        AssertScenarioHasCode(negativeProof, "prompt_text_source_of_truth", "visual_world.prompt.source_of_truth");
    }

    private static void AssertScenarioHasCode(VisualWorldProfileNegativeProof proof, string scenarioId, string code)
    {
        var scenario = proof.Scenarios.Single(item => item.ScenarioId == scenarioId);
        Assert.Contains(scenario.Diagnostics, diagnostic => diagnostic.Code == code);
    }
}
