using LLMGameCreator.Application.Design.DeterministicVisualChunkStreamWindow;
using LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;
using Xunit;

namespace LLMGameCreator.Tests.Application.DeterministicVisualChunkStreamWindow;

public sealed class DeterministicVisualChunkStreamWindowValidatorTests
{
    [Fact]
    public void RequiredFixturesMaterializeOnlyRequestedWindows()
    {
        var profiles = ParameterizedVisualWorldProfilesFixtures.BuildProfiles();
        var requests = DeterministicVisualChunkStreamWindowFixtures.BuildRequests();
        var windows = DeterministicVisualChunkStreamWindowMaterializer.MaterializeAll(requests, profiles);

        Assert.Equal(5, windows.Count);
        Assert.All(requests, request =>
        {
            var validation = DeterministicVisualChunkStreamWindowValidator.ValidateRequest(request, profiles);
            Assert.True(validation.Passed, string.Join(Environment.NewLine, validation.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        });
        Assert.All(windows, window =>
        {
            var validation = DeterministicVisualChunkStreamWindowValidator.ValidateWindow(window);
            Assert.True(validation.Passed, string.Join(Environment.NewLine, validation.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        });

        var finite = windows.Single(item => item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.FiniteFixtureId);
        Assert.Equal("finite_custom_sizes_matrix", finite.ProfileId);
        Assert.Equal(255, finite.EffectiveFiniteWidth);
        Assert.Equal(257, finite.EffectiveFiniteHeight);
        Assert.True(finite.ClippedAtFiniteBoundary);
        Assert.Equal(9, finite.ChunkCount);
        Assert.Equal(0, finite.MaterializedMinChunkX);
        Assert.Equal(0, finite.MaterializedMinChunkY);
        Assert.Equal(2, finite.MaterializedMaxChunkX);
        Assert.Equal(2, finite.MaterializedMaxChunkY);

        var huge = windows.Single(item => item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.HugeSparseFixtureId);
        Assert.Equal("huge_sparse_100000x100000_multilayer", huge.ProfileId);
        Assert.Equal(9, huge.ChunkCount);
        Assert.True(huge.EstimatedFullWorldChunkCapacity > huge.ChunkCount);
        Assert.True(huge.NoRawFullWorldDump);
    }

    [Fact]
    public void InfiniteOverlappingWindowsReuseStableChunkKeys()
    {
        var service = new DeterministicVisualChunkStreamWindowEvidenceService();
        var evidence = service.Build(FindRepoRoot());
        var infiniteWindows = evidence.MaterializationManifest.Windows
            .Where(item => item.FixtureId == DeterministicVisualChunkStreamWindowFixtures.InfiniteFixtureId)
            .ToList();

        Assert.Equal(2, infiniteWindows.Count);
        Assert.True(evidence.CacheReuseProof.Passed);
        Assert.Equal(24, evidence.CacheReuseProof.InfiniteOverlapReusedChunkKeyCount);
        Assert.All(evidence.CacheReuseProof.Records.Where(item => item.Reused), item =>
        {
            Assert.Equal(1, item.MaterializationCount);
            Assert.True(item.RequestCount > 1);
        });
    }

    [Fact]
    public void SeamAndLayerTransitionProofsPass()
    {
        var evidence = new DeterministicVisualChunkStreamWindowEvidenceService().Build(FindRepoRoot());

        Assert.True(evidence.SeamProof.Passed);
        Assert.True(evidence.SeamProof.WaterContinuityPassed);
        Assert.True(evidence.SeamProof.RoadContinuityPassed);
        Assert.True(evidence.SeamProof.BiomeContinuityPassed);
        Assert.True(evidence.SeamProof.SeamCount > 0);

        Assert.True(evidence.LayerTransitionProof.Passed);
        Assert.True(evidence.LayerTransitionProof.NotHardcodedSurfaceUndergroundOnly);
        Assert.Contains(evidence.LayerTransitionProof.Rows, row =>
            row.FixtureId == DeterministicVisualChunkStreamWindowFixtures.LayerTransitionFixtureId
            && row.LayerIds.Contains("underwater", StringComparer.Ordinal)
            && row.LayerLinks.Any(link => link.ToLayerId == "underwater" || link.FromLayerId == "underwater"));
    }

    [Fact]
    public void NegativeMatrixRejectsExpectedCases()
    {
        var proof = new DeterministicVisualChunkStreamWindowEvidenceService().Build(FindRepoRoot()).NegativeProof;

        Assert.True(proof.Passed, string.Join(Environment.NewLine, proof.Scenarios.SelectMany(item => item.Diagnostics).Select(item => $"{item.Code}:{item.Target}")));
        Assert.Equal(proof.ScenarioCount, proof.RejectedCount);
        AssertScenarioHasCode(proof, "unknown_profile", "visual_chunk_stream.profile.unknown");
        AssertScenarioHasCode(proof, "unknown_layer", "visual_chunk_stream.layer.unknown");
        AssertScenarioHasCode(proof, "missing_seed", "visual_chunk_stream.seed.missing");
        AssertScenarioHasCode(proof, "missing_generator_version", "visual_chunk_stream.generator_version.missing");
        AssertScenarioHasCode(proof, "invalid_radius", "visual_chunk_stream.radius.invalid");
        AssertScenarioHasCode(proof, "raw_full_world_dump", "visual_chunk_stream.raw_full_world_dump.forbidden");
        AssertScenarioHasCode(proof, "finite_out_of_bounds_without_clipping", "visual_chunk_stream.finite_clipping.required");
        AssertScenarioHasCode(proof, "chunk_key_mismatch", "visual_chunk_stream.chunk_key.mismatch");
        AssertScenarioHasCode(proof, "seam_key_mismatch", "visual_chunk_stream.seam_key.mismatch");
        AssertScenarioHasCode(proof, "water_connector_mismatch", "visual_chunk_stream.water_connector.mismatch");
        AssertScenarioHasCode(proof, "road_connector_mismatch", "visual_chunk_stream.road_connector.mismatch");
        AssertScenarioHasCode(proof, "duplicate_chunk_keys", "visual_chunk_stream.chunk_key.duplicate");
        AssertScenarioHasCode(proof, "prompt_text_source_of_truth", "visual_chunk_stream.prompt.source_of_truth");
        AssertScenarioHasCode(proof, "absolute_path_metadata", "visual_chunk_stream.path.absolute");
        AssertScenarioHasCode(proof, "rating_metadata_without_safe_fallback", "visual_chunk_stream.rating.safe_fallback_missing");
        AssertScenarioHasCode(proof, "delta_overlay_raw_payload", "visual_chunk_stream.delta_overlay.raw_payload");
    }

    private static void AssertScenarioHasCode(VisualChunkStreamNegativeProof proof, string scenarioId, string code)
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
