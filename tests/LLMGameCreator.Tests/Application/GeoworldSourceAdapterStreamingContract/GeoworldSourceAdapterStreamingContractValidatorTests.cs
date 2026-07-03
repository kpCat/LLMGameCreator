using LLMGameCreator.Application.Design.GeoworldSourceAdapterStreamingContract;
using Xunit;

namespace LLMGameCreator.Tests.Application.GeoworldSourceAdapterStreamingContract;

public sealed class GeoworldSourceAdapterStreamingContractValidatorTests
{
    [Fact]
    public void ValidFixturesPassAndStayMetadataOnly()
    {
        var fixtures = GeoworldContractFixtures.BuildFixtures();

        Assert.Equal(GeoworldContractFixtures.RequiredFixtureIds.Count, fixtures.Count);
        Assert.All(GeoworldContractFixtures.RequiredFixtureIds, id => Assert.Contains(fixtures, item => item.SpecId == id));
        Assert.All(fixtures, fixture =>
        {
            var result = GeoworldContractValidator.Validate(fixture);
            Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
            Assert.True(fixture.MetadataOnly);
            Assert.True(fixture.FetchResult.MetadataOnly);
            Assert.False(fixture.FetchPlan.PerformsNetworkIo);
            Assert.False(fixture.FetchResult.NetworkIoPerformed);
            Assert.False(fixture.FetchResult.RawGeodataDumpPresent);
        });
    }

    [Fact]
    public void NegativeMatrixRejectsAllForbiddenCases()
    {
        var proof = GeoworldContractEvidenceService.BuildNegativeProof();

        Assert.True(proof.Passed, string.Join(Environment.NewLine, proof.Scenarios.SelectMany(item => item.Diagnostics).Select(item => $"{item.Code}:{item.Target}")));
        Assert.Equal(proof.ScenarioCount, proof.RejectedCount);
        Assert.All(proof.Scenarios, scenario => Assert.False(scenario.ActualValid));

        AssertScenarioHasCode(proof, "public_tile_scraping", "geoworld.public_tile_scraping.forbidden");
        AssertScenarioHasCode(proof, "bulk_public_tile_archive", "geoworld.public_tile_bulk_archive.forbidden");
        AssertScenarioHasCode(proof, "runtime_online_without_explicit_policy", "geoworld.runtime_online.policy_required");
        AssertScenarioHasCode(proof, "missing_license_policy", "geoworld.license_policy.missing");
        AssertScenarioHasCode(proof, "missing_attribution", "geoworld.attribution.missing");
        AssertScenarioHasCode(proof, "missing_provenance", "geoworld.provenance.missing");
        AssertScenarioHasCode(proof, "raw_osm_tags_direct_to_gameplay", "geoworld.raw_tags.gameplay_leak");
        AssertScenarioHasCode(proof, "absolute_source_path", "geoworld.path.absolute_or_unsafe");
        AssertScenarioHasCode(proof, "missing_cache_policy", "geoworld.cache_policy.missing");
        AssertScenarioHasCode(proof, "missing_stream_radius_boundary_prefetch", "geoworld.stream_radius.missing");
        AssertScenarioHasCode(proof, "full_planet_raw_dump", "geoworld.full_planet_raw_dump.forbidden");
        AssertScenarioHasCode(proof, "hardcoded_provider_api_in_core", "geoworld.provider_api.hardcoded_core");
        AssertScenarioHasCode(proof, "prompt_text_source_of_truth", "geoworld.prompt.source_of_truth");
        AssertScenarioHasCode(proof, "lfz_copied_code_marker", "geoworld.lfz_code_copy.marker");
        AssertScenarioHasCode(proof, "ocr_fallback_primary_path", "geoworld.ocr_fallback.primary_path_forbidden");
        AssertScenarioHasCode(proof, "rating_metadata_without_safe_fallback", "geoworld.rating.safe_fallback_missing");
    }

    [Fact]
    public void NormalizedTaxonomyAndBoundaryPrefetchContractArePresent()
    {
        var taxonomy = GeoworldContractFixtures.BuildTaxonomy();
        var kinds = taxonomy.Rows.Select(item => item.Kind).ToHashSet();

        Assert.Contains(GeoFeatureKind.Building, kinds);
        Assert.Contains(GeoFeatureKind.Road, kinds);
        Assert.Contains(GeoFeatureKind.Water, kinds);
        Assert.Contains(GeoFeatureKind.LandUse, kinds);
        Assert.Contains(GeoFeatureKind.Poi, kinds);
        Assert.Contains(GeoFeatureKind.Barrier, kinds);
        Assert.Contains(GeoFeatureKind.Bridge, kinds);
        Assert.Contains(GeoFeatureKind.Vegetation, kinds);
        Assert.All(taxonomy.Rows, row => Assert.True(row.GameplayConsumesNormalizedFeatureOnly));

        var boundary = GeoworldContractFixtures.BuildFixtures().Single(item => item.SpecId == "earth_radius_stream_window_boundary_prefetch");
        Assert.True(boundary.StreamingPolicy!.StreamWindowRequest.BoundaryPrefetchEnabled);
        Assert.True(boundary.StreamingPolicy.StreamWindowRequest.GridRequest.BoundaryPrefetchTiles >= 2);
        Assert.True(boundary.StreamingPolicy.FutureRuntimeStreamingContractOnly);
    }

    [Fact]
    public void NoLiveAdapterPerformsNetworkIo()
    {
        var fixtures = GeoworldContractFixtures.BuildFixtures();

        Assert.All(fixtures, fixture =>
        {
            Assert.NotEqual(GeoNetworkIoMode.LiveNetworkFetch, fixture.FetchPlan.NetworkIoMode);
            Assert.False(fixture.FetchPlan.PerformsNetworkIo);
            Assert.False(fixture.FetchPlan.ProviderOrApiHardcodedIntoCore);
            Assert.False(fixture.FetchResult.NetworkIoPerformed);
        });
    }

    private static void AssertScenarioHasCode(GeoworldNegativeProof proof, string scenarioId, string code)
    {
        var scenario = proof.Scenarios.Single(item => item.ScenarioId == scenarioId);
        Assert.Contains(scenario.Diagnostics, diagnostic => diagnostic.Code == code);
    }
}
