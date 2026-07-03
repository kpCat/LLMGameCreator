using System.Text.Json;
using LLMGameCreator.Application.Design.DeterministicVisualChunkStreamWindow;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class DeterministicVisualChunkStreamWindowProductSmokeTests
{
    private static readonly HashSet<string> BinaryOrRasterMediaExtensions = new(StringComparer.OrdinalIgnoreCase)
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
        ".bytes"
    };

    [Fact]
    public async Task Goal091DeterministicVisualChunkStreamWindowProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new DeterministicVisualChunkStreamWindowEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);
        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.MaterializationManifestJson, second.MaterializationManifestJson);
        Assert.Equal(first.DeterminismProofJson, second.DeterminismProofJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.MaterializationManifestJsonPath));
        Assert.True(File.Exists(write.FileLedgerJsonPath));
        Assert.True(File.Exists(write.DeterminismProofJsonPath));
        Assert.True(File.Exists(write.SeamProofJsonPath));
        Assert.True(File.Exists(write.CacheReuseProofJsonPath));
        Assert.True(File.Exists(write.LayerTransitionProofJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(write.MaterializationManifestJsonPath));
        using var determinism = JsonDocument.Parse(await File.ReadAllTextAsync(write.DeterminismProofJsonPath));
        using var seam = JsonDocument.Parse(await File.ReadAllTextAsync(write.SeamProofJsonPath));
        using var cache = JsonDocument.Parse(await File.ReadAllTextAsync(write.CacheReuseProofJsonPath));
        using var layers = JsonDocument.Parse(await File.ReadAllTextAsync(write.LayerTransitionProofJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        var fixtures = catalog.RootElement.GetProperty("fixtures").EnumerateArray().ToArray();
        Assert.Contains(fixtures, item => item.GetProperty("fixtureId").GetString() == DeterministicVisualChunkStreamWindowFixtures.FiniteFixtureId);
        Assert.Contains(fixtures, item => item.GetProperty("fixtureId").GetString() == DeterministicVisualChunkStreamWindowFixtures.HugeSparseFixtureId);
        Assert.Contains(fixtures, item => item.GetProperty("fixtureId").GetString() == DeterministicVisualChunkStreamWindowFixtures.InfiniteFixtureId);
        Assert.Contains(fixtures, item => item.GetProperty("fixtureId").GetString() == DeterministicVisualChunkStreamWindowFixtures.LayerTransitionFixtureId);

        var windows = manifest.RootElement.GetProperty("windows").EnumerateArray().ToArray();
        Assert.Contains(windows, item =>
            item.GetProperty("fixtureId").GetString() == DeterministicVisualChunkStreamWindowFixtures.FiniteFixtureId
            && item.GetProperty("effectiveFiniteWidth").GetInt32() == 255
            && item.GetProperty("effectiveFiniteHeight").GetInt32() == 257
            && item.GetProperty("clippedAtFiniteBoundary").GetBoolean());
        Assert.Contains(windows, item =>
            item.GetProperty("fixtureId").GetString() == DeterministicVisualChunkStreamWindowFixtures.HugeSparseFixtureId
            && item.GetProperty("estimatedFullWorldChunkCapacity").GetInt64() > item.GetProperty("chunkCount").GetInt32());
        Assert.True(manifest.RootElement.GetProperty("noRawFullWorldDump").GetBoolean());
        Assert.True(determinism.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(seam.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(cache.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(cache.RootElement.GetProperty("infiniteOverlapReusedChunkKeyCount").GetInt32() > 0);
        Assert.True(layers.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(layers.RootElement.GetProperty("notHardcodedSurfaceUndergroundOnly").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRuntimeUnityProviderSchemaProjectDependencyChanges").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noBinaryOrRasterMediaAdded").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noPromptDumps").GetBoolean());

        Assert.Equal(4, write.OverviewSvgPaths.Count);
        Assert.All(write.OverviewSvgPaths, AssertSafeSvg);

        var mediaFiles = Directory.EnumerateFiles(write.OutputDirectoryPath, "*", SearchOption.AllDirectories)
            .Where(path => BinaryOrRasterMediaExtensions.Contains(Path.GetExtension(path)))
            .ToList();
        Assert.Empty(mediaFiles);

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("deterministic_visual_chunk_stream_window_verification required", report);
        Assert.Contains(DeterministicVisualChunkStreamWindowFixtures.FiniteFixtureId, report);
        Assert.Contains(DeterministicVisualChunkStreamWindowFixtures.HugeSparseFixtureId, report);
        Assert.Contains(DeterministicVisualChunkStreamWindowFixtures.InfiniteFixtureId, report);
        Assert.Contains(DeterministicVisualChunkStreamWindowFixtures.LayerTransitionFixtureId, report);
    }

    private static void AssertSafeSvg(string path)
    {
        Assert.True(File.Exists(path), path);
        var svg = File.ReadAllText(path);

        Assert.Contains("<svg", svg);
        Assert.Contains("viewBox=", svg);
        Assert.DoesNotContain("<script", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("base64", svg, StringComparison.OrdinalIgnoreCase);
        Assert.True(DeterministicVisualChunkStreamWindowValidator.CountSvgRects(svg) >= 4);
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
