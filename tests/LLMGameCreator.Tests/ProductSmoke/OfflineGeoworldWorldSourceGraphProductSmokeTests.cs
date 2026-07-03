using System.Text.Json;
using LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;
using LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class OfflineGeoworldWorldSourceGraphProductSmokeTests
{
    private static readonly HashSet<string> ForbiddenOutputExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".osm",
        ".pbf",
        ".mbtiles",
        ".gpkg",
        ".geojson",
        ".png",
        ".jpg",
        ".jpeg",
        ".webp",
        ".gif",
        ".bmp",
        ".asset",
        ".bytes"
    };

    [Fact]
    public async Task Goal099OfflineGeoworldWorldSourceGraphStreamingProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new OfflineGeoworldWorldSourceGraphEvidenceService();
        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);
        var write = await service.BuildAndWriteAsync(repoRoot, projectRoot);

        Assert.Equal(first.BundleCatalogJson, second.BundleCatalogJson);
        Assert.Equal(first.NormalizedFeaturesJson, second.NormalizedFeaturesJson);
        Assert.Equal(first.WorldSourceGraphJson, second.WorldSourceGraphJson);
        Assert.Equal(first.StreamWindowPlanJson, second.StreamWindowPlanJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.BundleCatalogJsonPath));
        Assert.True(File.Exists(write.NormalizedFeaturesJsonPath));
        Assert.True(File.Exists(write.WorldSourceGraphJsonPath));
        Assert.True(File.Exists(write.StreamWindowPlanJsonPath));
        Assert.True(File.Exists(write.BoundaryPrefetchProofJsonPath));
        Assert.True(File.Exists(write.VisualProjectionSummaryJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.WorkspaceBindingInventoryJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));
        Assert.True(File.Exists(write.OverviewSvgPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.BundleCatalogJsonPath));
        using var normalized = JsonDocument.Parse(await File.ReadAllTextAsync(write.NormalizedFeaturesJsonPath));
        using var graph = JsonDocument.Parse(await File.ReadAllTextAsync(write.WorldSourceGraphJsonPath));
        using var stream = JsonDocument.Parse(await File.ReadAllTextAsync(write.StreamWindowPlanJsonPath));
        using var boundary = JsonDocument.Parse(await File.ReadAllTextAsync(write.BoundaryPrefetchProofJsonPath));
        using var projection = JsonDocument.Parse(await File.ReadAllTextAsync(write.VisualProjectionSummaryJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var binding = JsonDocument.Parse(await File.ReadAllTextAsync(write.WorkspaceBindingInventoryJsonPath));
        using var lineage = JsonDocument.Parse(await File.ReadAllTextAsync(write.SourceLineageJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        Assert.Equal("GREEN", catalog.RootElement.GetProperty("implementationStatus").GetString());
        Assert.False(catalog.RootElement.GetProperty("accepted").GetBoolean());
        Assert.Equal(
            OfflineGeoworldBundleFixtures.SyntheticCityRadiusBundleId,
            catalog.RootElement.GetProperty("bundleIds")[0].GetString());
        Assert.Equal(10, normalized.RootElement.GetProperty("featureCount").GetInt32());
        Assert.True(normalized.RootElement.GetProperty("gameplaySafeOnlyAfterNormalization").GetBoolean());
        Assert.Equal(10, normalized.RootElement.GetProperty("featureKindsCovered").GetArrayLength());
        Assert.True(graph.RootElement.GetProperty("baseDataImmutable").GetBoolean());
        Assert.True(graph.RootElement.GetProperty("gameplayDeltasSeparate").GetBoolean());
        Assert.Equal(0, graph.RootElement.GetProperty("deltaCount").GetInt32());
        Assert.True(graph.RootElement.GetProperty("crossChunkReferences").GetArrayLength() >= 3);
        Assert.Equal(9, stream.RootElement.GetProperty("requiredChunkKeys").GetArrayLength());
        Assert.True(stream.RootElement.GetProperty("boundaryPrefetchChunkKeys").GetArrayLength() >= 16);
        Assert.False(stream.RootElement.GetProperty("networkFetchAttempted").GetBoolean());
        Assert.True(boundary.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(projection.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(projection.RootElement.GetProperty("noRasterImages").GetBoolean());
        Assert.True(projection.RootElement.GetProperty("noUnityOutput").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(14, negative.RootElement.GetProperty("scenarioCount").GetInt32());
        Assert.True(binding.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(binding.RootElement.GetProperty("workspaceCatalogIncludesGeoworldGroup").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("goal098AcceptedFalsePreserved").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("offlineSyntheticBundleOnly").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noNetworkOrProviderImplementation").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noLfzCodeCopied").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRawGeodataDump").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noBinaryOrRasterMedia").GetBoolean());

        var forbiddenOutputs = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(path => ForbiddenOutputExtensions.Contains(Path.GetExtension(path)))
            .ToArray();
        Assert.Empty(forbiddenOutputs);
        Assert.All(Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories), path =>
        {
            var relative = Path.GetRelativePath(projectRoot, path);
            Assert.False(Path.IsPathFullyQualified(relative), relative);
        });

        var overview = await File.ReadAllTextAsync(write.OverviewSvgPath);
        Assert.Contains("<svg", overview, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<script", overview, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", overview, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", overview, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("base64", overview, StringComparison.OrdinalIgnoreCase);

        var workspace = new VisualWorldStreamPreviewWorkspaceService().Build(repoRoot);
        var geoworld = Assert.Single(workspace.Catalog.Groups, group => group.GroupId == "geoworld");
        Assert.True(workspace.QualityGateScan.Passed);
        Assert.True(workspace.QualityGateScan.GeoworldGroupPresent);
        Assert.Equal(OfflineGeoworldBundleFixtures.SyntheticCityRadiusBundleId, workspace.QualityGateScan.GeoworldOfflineBundleId);
        Assert.True(workspace.QualityGateScan.GeoworldBoundaryPrefetchPassed);
        Assert.True(workspace.QualityGateScan.GeoworldTaxonomyCoveragePassed);
        Assert.True(workspace.QualityGateScan.Goal099FilesDiscoveredByRelativePaths);
        Assert.Contains(geoworld.Entries, entry => entry.ArtifactKind == "text_svg_geoworld_stream_window_overview");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal099.boundary_prefetch");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal099.negative");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal099.visual_projection");
        AssertProofPassed(workspace.ProofStatus.Proofs, "goal099.quality_gate");

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("offline_geoworld_worldsourcegraph_streaming_verification required", report);
        Assert.Contains("offlineBundleId: " + OfflineGeoworldBundleFixtures.SyntheticCityRadiusBundleId, report);
        Assert.Contains("noNetworkOrProviderImplementation: true", report);
        Assert.Contains("noRawGeodataDump: true", report);
        Assert.DoesNotContain(repoRoot, report, StringComparison.OrdinalIgnoreCase);

        AssertNoForbiddenSourceApi(repoRoot);
    }

    private static void AssertProofPassed(
        IReadOnlyList<VisualWorldPreviewProofStatus> proofs,
        string proofId)
    {
        var proof = Assert.Single(proofs, item => item.ProofId == proofId);
        Assert.True(proof.Passed, proof.DiagnosticSummary);
        Assert.False(Path.IsPathFullyQualified(proof.RelativePath), proof.RelativePath);
        Assert.False(string.IsNullOrWhiteSpace(proof.Sha256), proof.ProofId);
    }

    private static void AssertNoForbiddenSourceApi(string repoRoot)
    {
        var sourceRoot = Path.Combine(
            repoRoot,
            "src",
            "LLMGameCreator.Application",
            "Design",
            "OfflineGeoworldWorldSourceGraph");
        var forbiddenTokens = new[]
        {
            "HttpClient",
            "WebRequest",
            "NetworkStream",
            "TcpClient",
            "Socket(",
            "DownloadString",
            "SendAsync("
        };

        foreach (var path in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.TopDirectoryOnly))
        {
            var text = File.ReadAllText(path);
            foreach (var token in forbiddenTokens)
            {
                Assert.DoesNotContain(token, text, StringComparison.Ordinal);
            }
        }
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
