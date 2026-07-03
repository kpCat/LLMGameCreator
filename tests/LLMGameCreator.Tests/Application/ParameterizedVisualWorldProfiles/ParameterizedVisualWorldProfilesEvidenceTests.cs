using System.Text.Json;
using LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;
using Xunit;

namespace LLMGameCreator.Tests.Application.ParameterizedVisualWorldProfiles;

public sealed class ParameterizedVisualWorldProfilesEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsManualGateExplicit()
    {
        var repoRoot = FindRepoRoot();
        var service = new ParameterizedVisualWorldProfilesEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.SizeMatrixJson, second.SizeMatrixJson);
        Assert.Equal(first.ValidationMatrixJson, second.ValidationMatrixJson);
        Assert.Equal(first.NegativeProofJson, second.NegativeProofJson);
        Assert.Equal(first.ChunkAddressProofJson, second.ChunkAddressProofJson);
        Assert.Equal(first.SparseWorldProofJson, second.SparseWorldProofJson);
        Assert.Equal(first.LayerModelProofJson, second.LayerModelProofJson);
        Assert.Equal(first.SourceLineageJson, second.SourceLineageJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);
        Assert.Equal(first.OverviewSvgByRelativePath, second.OverviewSvgByRelativePath);

        Assert.Equal(4, first.Catalog.Profiles.Count);
        Assert.True(first.SizeMatrix.Passed);
        Assert.True(first.ValidationMatrix.Passed);
        Assert.True(first.NegativeProof.Passed);
        Assert.True(first.ChunkAddressProof.Passed);
        Assert.True(first.SparseWorldProof.Passed);
        Assert.True(first.LayerModelProof.Passed);
        Assert.True(first.SourceLineage.Passed);
        Assert.True(first.QualityGateScan.NoRawHeavyCellDump);
        Assert.True(first.QualityGateScan.Benchmark144OnlyFixturePassed);
        Assert.True(first.QualityGateScan.NoRuntimeUnityProviderSchemaProjectDependencyChanges);
        Assert.True(first.QualityGateScan.NoBinaryOrRasterMediaAdded);
        Assert.False(first.Report.Accepted);
        Assert.Equal(ParameterizedVisualWorldProfilesVocabulary.FinalGate, first.Report.ManualGate);
        Assert.Contains("parameterized_visual_world_profiles_verification required", first.ReportMarkdown);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var service = new ParameterizedVisualWorldProfilesEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.SizeMatrixJsonPath));
        Assert.True(File.Exists(write.ValidationMatrixJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.ChunkAddressProofJsonPath));
        Assert.True(File.Exists(write.SparseWorldProofJsonPath));
        Assert.True(File.Exists(write.LayerModelProofJsonPath));
        Assert.True(File.Exists(write.SourceLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));
        Assert.Equal(4, write.OverviewSvgPaths.Count);
        Assert.All(write.OverviewSvgPaths, path => Assert.True(File.Exists(path), path));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var sizeMatrix = JsonDocument.Parse(await File.ReadAllTextAsync(write.SizeMatrixJsonPath));
        using var validation = JsonDocument.Parse(await File.ReadAllTextAsync(write.ValidationMatrixJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var chunks = JsonDocument.Parse(await File.ReadAllTextAsync(write.ChunkAddressProofJsonPath));
        using var sparse = JsonDocument.Parse(await File.ReadAllTextAsync(write.SparseWorldProofJsonPath));
        using var layers = JsonDocument.Parse(await File.ReadAllTextAsync(write.LayerModelProofJsonPath));
        using var lineage = JsonDocument.Parse(await File.ReadAllTextAsync(write.SourceLineageJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        Assert.Equal(4, catalog.RootElement.GetProperty("profiles").GetArrayLength());
        Assert.True(sizeMatrix.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(6, sizeMatrix.RootElement.GetProperty("rows").GetArrayLength());
        Assert.True(validation.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(chunks.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(sparse.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(layers.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(lineage.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noRawHeavyCellDump").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("benchmark144OnlyFixturePassed").GetBoolean());

        foreach (var path in write.OverviewSvgPaths)
        {
            AssertSafeSvg(path);
        }
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
        Assert.True(ParameterizedVisualWorldProfilesValidator.CountSvgRects(svg) >= 4);
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
