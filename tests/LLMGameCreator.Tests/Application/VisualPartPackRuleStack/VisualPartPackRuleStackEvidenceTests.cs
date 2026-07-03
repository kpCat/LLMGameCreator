using System.Text.Json;
using LLMGameCreator.Application.Design.VisualPartPackRuleStack;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualPartPackRuleStack;

public sealed class VisualPartPackRuleStackEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsBoundariesExplicit()
    {
        var repoRoot = FindRepoRoot();
        var service = new VisualPartPackRuleStackEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.ValidationMatrixJson, second.ValidationMatrixJson);
        Assert.Equal(first.NegativeProofJson, second.NegativeProofJson);
        Assert.Equal(first.DeepsearchLineageJson, second.DeepsearchLineageJson);
        Assert.Equal(first.Goal084BindingMatrixJson, second.Goal084BindingMatrixJson);
        Assert.Equal(first.WaterBiomeCoverageMatrixJson, second.WaterBiomeCoverageMatrixJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.True(first.ValidationMatrix.Passed);
        Assert.True(first.NegativeProof.Passed);
        Assert.True(first.DeepsearchLineageInventory.Passed);
        Assert.True(first.Goal084ContractBindingMatrix.Passed);
        Assert.True(first.WaterBiomeCoverageMatrix.Passed);
        Assert.False(first.Report.Accepted);
        Assert.Equal(VisualPartPackRuleStackVocabulary.FinalGate, first.Report.ManualGate);
        Assert.True(first.QualityGateScan.NoExternalDependenciesAdded);
        Assert.True(first.QualityGateScan.NoRuntimeOrUnityChanged);
        Assert.True(first.QualityGateScan.NoProviderIntegrationAdded);
        Assert.True(first.QualityGateScan.NoPublicGamePackageSchemaChanged);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var service = new VisualPartPackRuleStackEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.ValidationMatrixJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.DeepsearchLineageJsonPath));
        Assert.True(File.Exists(write.Goal084BindingMatrixJsonPath));
        Assert.True(File.Exists(write.WaterBiomeCoverageMatrixJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));
        Assert.Contains("visual_part_pack_rule_stack_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var validation = JsonDocument.Parse(await File.ReadAllTextAsync(write.ValidationMatrixJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var deepsearch = JsonDocument.Parse(await File.ReadAllTextAsync(write.DeepsearchLineageJsonPath));
        using var water = JsonDocument.Parse(await File.ReadAllTextAsync(write.WaterBiomeCoverageMatrixJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        Assert.Equal(6, catalog.RootElement.GetProperty("fixturePackCount").GetInt32());
        Assert.True(validation.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.Equal(8, deepsearch.RootElement.GetProperty("documentCount").GetInt32());
        Assert.True(water.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("adultMetadataOnlyFallbackBound").GetBoolean());
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
