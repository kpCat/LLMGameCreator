using System.Text.Json;
using LLMGameCreator.Application.Design.DeterministicVisualChunkStreamWindow;
using Xunit;

namespace LLMGameCreator.Tests.Application.DeterministicVisualChunkStreamWindow;

public sealed class DeterministicVisualChunkStreamWindowEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsManualGateExplicit()
    {
        var repoRoot = FindRepoRoot();
        var service = new DeterministicVisualChunkStreamWindowEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.MaterializationManifestJson, second.MaterializationManifestJson);
        Assert.Equal(first.FileLedgerJson, second.FileLedgerJson);
        Assert.Equal(first.DeterminismProofJson, second.DeterminismProofJson);
        Assert.Equal(first.SeamProofJson, second.SeamProofJson);
        Assert.Equal(first.CacheReuseProofJson, second.CacheReuseProofJson);
        Assert.Equal(first.LayerTransitionProofJson, second.LayerTransitionProofJson);
        Assert.Equal(first.NegativeProofJson, second.NegativeProofJson);
        Assert.Equal(first.SourceLineageJson, second.SourceLineageJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);
        Assert.Equal(first.OverviewSvgByFixtureId, second.OverviewSvgByFixtureId);

        Assert.Equal(4, first.Catalog.FixtureCount);
        Assert.Equal(5, first.Catalog.WindowCount);
        Assert.True(first.DeterminismProof.Passed);
        Assert.True(first.SeamProof.Passed);
        Assert.True(first.CacheReuseProof.Passed);
        Assert.True(first.LayerTransitionProof.Passed);
        Assert.True(first.NegativeProof.Passed);
        Assert.True(first.SourceLineage.Passed);
        Assert.True(first.QualityGateScan.BoundaryClippingExplicit);
        Assert.True(first.QualityGateScan.HugeSparseNoRawDump);
        Assert.True(first.QualityGateScan.InfiniteOverlapReuseProven);
        Assert.True(first.QualityGateScan.NoRuntimeUnityProviderSchemaProjectDependencyChanges);
        Assert.True(first.QualityGateScan.NoBinaryOrRasterMediaAdded);
        Assert.False(first.Report.Accepted);
        Assert.Equal(DeterministicVisualChunkStreamWindowVocabulary.FinalGate, first.Report.ManualGate);
        Assert.Contains("deterministic_visual_chunk_stream_window_verification required", first.ReportMarkdown);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var service = new DeterministicVisualChunkStreamWindowEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(temp.Path, result);

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
        Assert.Equal(4, write.OverviewSvgPaths.Count);
        Assert.All(write.OverviewSvgPaths, path => Assert.True(File.Exists(path), path));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(write.MaterializationManifestJsonPath));
        using var ledger = JsonDocument.Parse(await File.ReadAllTextAsync(write.FileLedgerJsonPath));
        using var determinism = JsonDocument.Parse(await File.ReadAllTextAsync(write.DeterminismProofJsonPath));
        using var seam = JsonDocument.Parse(await File.ReadAllTextAsync(write.SeamProofJsonPath));
        using var cache = JsonDocument.Parse(await File.ReadAllTextAsync(write.CacheReuseProofJsonPath));
        using var layers = JsonDocument.Parse(await File.ReadAllTextAsync(write.LayerTransitionProofJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        Assert.Equal(4, catalog.RootElement.GetProperty("fixtureCount").GetInt32());
        Assert.Equal(5, manifest.RootElement.GetProperty("windowCount").GetInt32());
        Assert.True(manifest.RootElement.GetProperty("noRawFullWorldDump").GetBoolean());
        Assert.True(ledger.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(determinism.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(seam.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(cache.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(layers.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("boundaryClippingExplicit").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("infiniteOverlapReuseProven").GetBoolean());

        Assert.All(write.OverviewSvgPaths, AssertSafeSvg);
    }

    private static void AssertSafeSvg(string path)
    {
        var svg = File.ReadAllText(path);

        Assert.Contains("<svg", svg);
        Assert.Contains("viewBox=", svg);
        Assert.DoesNotContain("<script", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("base64", svg, StringComparison.OrdinalIgnoreCase);
        Assert.True(DeterministicVisualChunkStreamWindowValidator.CountSvgRects(svg) >= 4);
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

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
