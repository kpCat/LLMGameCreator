using System.Text.Json;
using LLMGameCreator.Application.Design.GeoworldSourceAdapterStreamingContract;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class GeoworldSourceAdapterStreamingContractProductSmokeTests
{
    private static readonly HashSet<string> ForbiddenOutputExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".wav",
        ".ogg",
        ".mp3",
        ".mp4",
        ".asset",
        ".bytes",
        ".osm",
        ".pbf",
        ".mbtiles",
        ".gpkg",
        ".geojson",
        ".tif",
        ".tiff"
    };

    [Fact]
    public async Task Goal098GeoworldSourceAdapterStreamingContractProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new GeoworldContractEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);
        var write = await service.BuildAndWriteAsync(repoRoot, projectRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.LfzPatternLineageJson, second.LfzPatternLineageJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.TaxonomyJsonPath));
        Assert.True(File.Exists(write.StreamingPolicyMatrixJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.LfzPatternLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var taxonomy = JsonDocument.Parse(await File.ReadAllTextAsync(write.TaxonomyJsonPath));
        using var policy = JsonDocument.Parse(await File.ReadAllTextAsync(write.StreamingPolicyMatrixJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var lineage = JsonDocument.Parse(await File.ReadAllTextAsync(write.LfzPatternLineageJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        var fixtureIds = catalog.RootElement.GetProperty("fixtureIds").EnumerateArray().Select(item => item.GetString()).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("offline_osm_extract_city_radius", fixtureIds);
        Assert.Contains("user_provided_map_bundle", fixtureIds);
        Assert.Contains("licensed_vector_tile_adapter_spec", fixtureIds);
        Assert.Contains("runtime_online_optional_policy_blocked_by_default", fixtureIds);
        Assert.Contains("ocr_georeference_fallback_future_only", fixtureIds);
        Assert.Contains("self_generated_realism_world_source", fixtureIds);
        Assert.Contains("earth_radius_stream_window_boundary_prefetch", fixtureIds);

        Assert.True(policy.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("lfzDocsConsumedAsLineage").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noLfzCodeCopied").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noNetworkOrProviderImplementation").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRuntimeUnitySchemaChanges").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("futureRuntimeStreamingContractsOnly").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRawGeodataDumps").GetBoolean());

        var taxonomyKinds = taxonomy.RootElement.GetProperty("rows").EnumerateArray()
            .Select(item => item.GetProperty("kind").GetString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("building", taxonomyKinds);
        Assert.Contains("road", taxonomyKinds);
        Assert.Contains("water", taxonomyKinds);
        Assert.Contains("landUse", taxonomyKinds);
        Assert.Contains("poi", taxonomyKinds);
        Assert.Contains("barrier", taxonomyKinds);
        Assert.Contains("bridge", taxonomyKinds);
        Assert.Contains("vegetation", taxonomyKinds);

        AssertNoForbiddenEvidenceFiles(write.OutputDirectoryPath);
        AssertNoNetworkImplementationSource(repoRoot);

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("geoworld_source_adapter_streaming_contract_verification required", report);
        Assert.Contains("noRawGeodataDumps: true", report);
    }

    private static void AssertNoForbiddenEvidenceFiles(string outputDirectory)
    {
        var forbidden = Directory.EnumerateFiles(outputDirectory, "*", SearchOption.AllDirectories)
            .Where(path => ForbiddenOutputExtensions.Contains(Path.GetExtension(path)))
            .ToList();
        Assert.Empty(forbidden);
    }

    private static void AssertNoNetworkImplementationSource(string repoRoot)
    {
        var sourceDirectory = Path.Combine(repoRoot, "src", "LLMGameCreator.Application", "Design", "GeoworldSourceAdapterStreamingContract");
        var source = string.Join(Environment.NewLine, Directory.EnumerateFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));
        Assert.DoesNotContain("HttpClient", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WebRequest", source, StringComparison.Ordinal);
        Assert.DoesNotContain("NetworkStream", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Socket", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DownloadString", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SendAsync", source, StringComparison.Ordinal);
    }

    private static string ResolveProjectFolder(string repoRoot)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? repoRoot : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
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
