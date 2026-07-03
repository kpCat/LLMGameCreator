using LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;
using Xunit;

namespace LLMGameCreator.Tests.Application.OfflineGeoworldWorldSourceGraph;

public sealed class OfflineGeoworldWorldSourceGraphTests
{
    [Fact]
    public void SyntheticBundleNormalizesIntoGameplaySafeTaxonomyAndGraph()
    {
        var bundle = OfflineGeoworldBundleFixtures.BuildSyntheticCityRadiusBundle();
        var normalized = OfflineGeoworldNormalizer.Normalize(bundle);
        var graph = OfflineGeoworldWorldSourceGraphBuilder.Build(bundle, normalized);
        var stream = OfflineGeoworldStreamWindowScheduler.BuildPlan(graph);
        var proof = OfflineGeoworldStreamWindowScheduler.BuildBoundaryPrefetchProof(stream);
        var projection = OfflineGeoworldVisualProjectionBuilder.BuildProjection(graph, normalized, stream);
        var validation = OfflineGeoworldWorldSourceGraphValidator.Validate(
            bundle,
            normalized,
            graph,
            stream,
            projection);

        Assert.True(validation.Passed);
        Assert.Equal(10, bundle.RawDescriptors.Count);
        Assert.Equal(10, normalized.FeatureCount);
        Assert.Equal(10, normalized.FeatureKindsCovered.Count);
        Assert.True(normalized.GameplaySafeOnlyAfterNormalization);
        Assert.True(normalized.RawTagsMappedNotPassedDirectly);
        Assert.All(normalized.Features, feature =>
        {
            Assert.True(feature.GameplaySafe, feature.FeatureId);
            Assert.False(feature.ContainsRawSourceTags, feature.FeatureId);
            Assert.False(string.IsNullOrWhiteSpace(feature.RawTagSummary), feature.FeatureId);
        });

        Assert.True(graph.BaseDataImmutable);
        Assert.True(graph.GameplayDeltasSeparate);
        Assert.Equal(0, graph.DeltaCount);
        Assert.True(graph.NoRawFullAreaDump);
        Assert.Contains(graph.CrossChunkReferences, reference => reference.FeatureKind == OfflineGeoFeatureKind.Road);
        Assert.Contains(graph.CrossChunkReferences, reference => reference.FeatureKind == OfflineGeoFeatureKind.Water);
        Assert.Contains(graph.CrossChunkReferences, reference => reference.FeatureKind == OfflineGeoFeatureKind.Bridge);
        Assert.True(stream.RequiredChunkKeys.Count >= 9);
        Assert.True(stream.BoundaryPrefetchChunkKeys.Count >= 16);
        Assert.False(stream.NetworkFetchAttempted);
        Assert.Equal("scheduled_no_network_cache_first", stream.BoundaryPrefetchStatus);
        Assert.True(proof.Passed);
        Assert.True(projection.Passed);
        Assert.True(projection.NoRasterImages);
        Assert.True(projection.NoUnityOutput);
    }

    [Fact]
    public void NegativeProofRejectsForbiddenGeoworldBoundaryScenarios()
    {
        var result = new OfflineGeoworldWorldSourceGraphEvidenceService().Build(ProjectRoot());
        var scenarioIds = result.NegativeProof.Scenarios
            .Select(scenario => scenario.ScenarioId)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(result.QualityGateScan.Passed);
        Assert.True(result.NegativeProof.Passed);
        Assert.Equal(14, result.NegativeProof.ScenarioCount);
        Assert.Equal(14, result.NegativeProof.RejectedCount);
        Assert.All(result.NegativeProof.Scenarios, scenario =>
        {
            Assert.False(scenario.ActualValid, scenario.ScenarioId);
            Assert.Contains(scenario.Diagnostics, diagnostic => diagnostic.Severity == "error");
        });
        Assert.Contains("raw_osm_tags_direct_to_gameplay", scenarioIds);
        Assert.Contains("runtime_online_fetch_attempted", scenarioIds);
        Assert.Contains("public_tile_scraping", scenarioIds);
        Assert.Contains("full_area_raw_dump", scenarioIds);
        Assert.Contains("boundary_crossing_without_reference", scenarioIds);
        Assert.Contains("boundary_prefetch_disabled_runtime_travel", scenarioIds);
        Assert.Contains("real_geodata_dump_marker", scenarioIds);
        Assert.Contains("raster_or_unity_projection_output", scenarioIds);
        Assert.True(result.SourceLineage.Goal098AcceptedFalsePreserved);
        Assert.True(result.SourceLineage.Goal098NoLfzCodeCopiedProven);
        Assert.True(result.SourceLineage.Goal098NoNetworkImplementationProven);
        Assert.True(result.WorkspaceBindingInventory.Passed);
    }

    private static string ProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
