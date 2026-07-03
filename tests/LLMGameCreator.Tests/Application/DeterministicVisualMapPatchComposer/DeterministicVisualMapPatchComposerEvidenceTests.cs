using System.Text.Json;
using LLMGameCreator.Application.Design.DeterministicVisualMapPatchComposer;
using Xunit;

namespace LLMGameCreator.Tests.Application.DeterministicVisualMapPatchComposer;

public sealed class DeterministicVisualMapPatchComposerEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsGateExplicit()
    {
        var repoRoot = FindRepoRoot();
        var service = new DeterministicVisualMapPatchComposerEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.MaterializationManifestJson, second.MaterializationManifestJson);
        Assert.Equal(first.FileLedgerJson, second.FileLedgerJson);
        Assert.Equal(first.WaterFlowProofJson, second.WaterFlowProofJson);
        Assert.Equal(first.ReachabilityProofJson, second.ReachabilityProofJson);
        Assert.Equal(first.LayeringProofJson, second.LayeringProofJson);
        Assert.Equal(first.NegativeProofJson, second.NegativeProofJson);
        Assert.Equal(first.SourceLineageJson, second.SourceLineageJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);
        Assert.Equal(first.SvgByPatchId, second.SvgByPatchId);

        Assert.Equal(3, first.Catalog.PatchCount);
        Assert.Equal(1152, first.Catalog.TotalCellCount);
        Assert.Equal(3, first.MaterializationManifest.PatchCount);
        Assert.Equal(11, first.FileLedger.FileCount);
        Assert.True(first.FileLedger.Passed);
        Assert.True(first.WaterFlowProof.Passed);
        Assert.True(first.ReachabilityProof.Passed);
        Assert.True(first.LayeringProof.Passed);
        Assert.True(first.NegativeProof.Passed);
        Assert.True(first.SourceLineage.Passed);
        Assert.True(first.QualityGateScan.PatchCountPassed);
        Assert.True(first.QualityGateScan.SvgTextOnlyPreviews);
        Assert.True(first.QualityGateScan.AllReferencesKnownGoal086Microtiles);
        Assert.True(first.QualityGateScan.WaterFlowProofPassed);
        Assert.True(first.QualityGateScan.ReachabilityProofPassed);
        Assert.True(first.QualityGateScan.LayeringProofPassed);
        Assert.True(first.QualityGateScan.NegativeProofPassed);
        Assert.True(first.QualityGateScan.SourceLineagePassed);
        Assert.True(first.QualityGateScan.NoExternalDependenciesAdded);
        Assert.True(first.QualityGateScan.NoBinaryOrRasterMediaAdded);
        Assert.True(first.QualityGateScan.NoProviderCalls);
        Assert.False(first.Report.Accepted);
        Assert.Equal(DeterministicVisualMapPatchComposerVocabulary.FinalGate, first.Report.ManualGate);
        Assert.Contains("deterministic_visual_map_patch_composer_verification required", first.ReportMarkdown);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var service = new DeterministicVisualMapPatchComposerEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.MaterializationManifestJsonPath));
        Assert.True(File.Exists(write.FileLedgerJsonPath));
        Assert.True(File.Exists(write.WaterFlowProofJsonPath));
        Assert.True(File.Exists(write.ReachabilityProofJsonPath));
        Assert.True(File.Exists(write.LayeringProofJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));
        Assert.Equal(3, write.PatchSvgPaths.Count);
        Assert.All(write.PatchSvgPaths, path => Assert.True(File.Exists(path), path));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(write.MaterializationManifestJsonPath));
        using var ledger = JsonDocument.Parse(await File.ReadAllTextAsync(write.FileLedgerJsonPath));
        using var water = JsonDocument.Parse(await File.ReadAllTextAsync(write.WaterFlowProofJsonPath));
        using var reachability = JsonDocument.Parse(await File.ReadAllTextAsync(write.ReachabilityProofJsonPath));
        using var layering = JsonDocument.Parse(await File.ReadAllTextAsync(write.LayeringProofJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var lineage = JsonDocument.Parse(await File.ReadAllTextAsync(write.SourceLineageJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        Assert.Equal(3, catalog.RootElement.GetProperty("patchCount").GetInt32());
        Assert.Equal(1152, catalog.RootElement.GetProperty("totalCellCount").GetInt32());
        Assert.Equal(3, manifest.RootElement.GetProperty("patchCount").GetInt32());
        Assert.Equal(11, ledger.RootElement.GetProperty("fileCount").GetInt32());
        Assert.True(water.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(reachability.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(layering.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("svgTextOnlyPreviews").GetBoolean());
        Assert.Equal(3, catalog.RootElement.GetProperty("patches").GetArrayLength());
        Assert.All(write.PatchSvgPaths, AssertSafeSvg);
    }

    private static void AssertSafeSvg(string path)
    {
        var svg = File.ReadAllText(path);

        Assert.Contains("<svg", svg);
        Assert.Contains("viewBox=\"0 0 288 192\"", svg);
        Assert.DoesNotContain("<script", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", svg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("base64", svg, StringComparison.OrdinalIgnoreCase);
        Assert.True(DeterministicVisualMapPatchComposerValidator.CountSvgRects(svg) >= 24 * 16);
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
