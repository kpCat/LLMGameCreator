using System.Text.Json;
using LLMGameCreator.Application.Design.DeterministicVisualMicrotileMaterializer;
using Xunit;

namespace LLMGameCreator.Tests.Application.DeterministicVisualMicrotileMaterializer;

public sealed class DeterministicVisualMicrotileMaterializerEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsGateExplicit()
    {
        var repoRoot = FindRepoRoot();
        var service = new DeterministicVisualMicrotileMaterializerEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);

        Assert.Equal(first.PreviewCatalogJson, second.PreviewCatalogJson);
        Assert.Equal(first.MaterializationManifestJson, second.MaterializationManifestJson);
        Assert.Equal(first.FileLedgerJson, second.FileLedgerJson);
        Assert.Equal(first.WaterBiomeProofJson, second.WaterBiomeProofJson);
        Assert.Equal(first.LayeringProofJson, second.LayeringProofJson);
        Assert.Equal(first.NegativeProofJson, second.NegativeProofJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.SourceLineageJson, second.SourceLineageJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);
        Assert.Equal(first.SvgByPreviewId, second.SvgByPreviewId);

        Assert.Equal(24, first.PreviewCatalog.PreviewCount);
        Assert.Equal(24, first.MaterializationManifest.PreviewCount);
        Assert.Equal(31, first.FileLedger.FileCount);
        Assert.True(first.FileLedger.Passed);
        Assert.True(first.WaterBiomeProof.Passed);
        Assert.True(first.LayeringProof.Passed);
        Assert.True(first.NegativeProof.Passed);
        Assert.True(first.SourceLineage.Passed);
        Assert.True(first.QualityGateScan.PreviewCountWithinBounds);
        Assert.True(first.QualityGateScan.SvgTextOnlyPreviews);
        Assert.True(first.QualityGateScan.WaterBiomeCoveragePassed);
        Assert.True(first.QualityGateScan.CreatureEquipmentStateCoveragePassed);
        Assert.True(first.QualityGateScan.UiEffectWeatherCoveragePassed);
        Assert.True(first.QualityGateScan.AdultMetadataOnlyFallbackCoveragePassed);
        Assert.True(first.QualityGateScan.NegativeProofPassed);
        Assert.True(first.QualityGateScan.SourceLineagePassed);
        Assert.True(first.QualityGateScan.NoExternalDependenciesAdded);
        Assert.True(first.QualityGateScan.NoBinaryMediaAdded);
        Assert.True(first.QualityGateScan.NoProviderCalls);
        Assert.False(first.Report.Accepted);
        Assert.Equal(DeterministicVisualMicrotileMaterializerVocabulary.FinalGate, first.Report.ManualGate);
        Assert.Contains("deterministic_visual_microtile_materializer_verification required", first.ReportMarkdown);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var service = new DeterministicVisualMicrotileMaterializerEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.PreviewCatalogJsonPath));
        Assert.True(File.Exists(write.MaterializationManifestJsonPath));
        Assert.True(File.Exists(write.FileLedgerJsonPath));
        Assert.True(File.Exists(write.WaterBiomeProofJsonPath));
        Assert.True(File.Exists(write.LayeringProofJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));
        Assert.Equal(24, write.PreviewSvgPaths.Count);
        Assert.All(write.PreviewSvgPaths, path => Assert.True(File.Exists(path), path));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.PreviewCatalogJsonPath));
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(write.MaterializationManifestJsonPath));
        using var ledger = JsonDocument.Parse(await File.ReadAllTextAsync(write.FileLedgerJsonPath));
        using var water = JsonDocument.Parse(await File.ReadAllTextAsync(write.WaterBiomeProofJsonPath));
        using var layering = JsonDocument.Parse(await File.ReadAllTextAsync(write.LayeringProofJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));
        using var lineage = JsonDocument.Parse(await File.ReadAllTextAsync(write.SourceLineageJsonPath));

        Assert.Equal(24, catalog.RootElement.GetProperty("previewCount").GetInt32());
        Assert.Equal(24, manifest.RootElement.GetProperty("previewCount").GetInt32());
        Assert.Equal(31, ledger.RootElement.GetProperty("fileCount").GetInt32());
        Assert.True(water.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(layering.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("svgTextOnlyPreviews").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(24, catalog.RootElement.GetProperty("previews").GetArrayLength());
        Assert.All(write.PreviewSvgPaths, AssertSafeSvg);
    }

    private static void AssertSafeSvg(string path)
    {
        var svg = File.ReadAllText(path);

        Assert.Contains("<svg", svg);
        Assert.Contains("viewBox=\"0 0 64 64\"", svg);
        Assert.DoesNotContain("<script", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("base64", svg, StringComparison.OrdinalIgnoreCase);
        Assert.True(DeterministicVisualMicrotileMaterializerValidator.CountGeneratedShapes(svg) >= 4);
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
