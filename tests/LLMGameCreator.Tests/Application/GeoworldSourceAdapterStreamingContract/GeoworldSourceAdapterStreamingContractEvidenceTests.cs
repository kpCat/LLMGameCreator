using LLMGameCreator.Application.Design.GeoworldSourceAdapterStreamingContract;
using Xunit;

namespace LLMGameCreator.Tests.Application.GeoworldSourceAdapterStreamingContract;

public sealed class GeoworldSourceAdapterStreamingContractEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndLineageBacked()
    {
        var repoRoot = FindRepoRoot();
        var service = new GeoworldContractEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.TaxonomyJson, second.TaxonomyJson);
        Assert.Equal(first.StreamingPolicyMatrixJson, second.StreamingPolicyMatrixJson);
        Assert.Equal(first.NegativeProofJson, second.NegativeProofJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);

        Assert.True(first.LfzPatternLineage.Passed);
        Assert.True(first.LfzPatternLineage.LfzDocsConsumedAsLineage);
        Assert.True(first.LfzPatternLineage.LfzArchiveNotRequired);
        Assert.True(first.LfzPatternLineage.LfzSourceCodeNotCopied);
        Assert.True(first.QualityGateScan.ValidFixturesPassed);
        Assert.True(first.QualityGateScan.NegativeProofPassed);
        Assert.True(first.QualityGateScan.NoNetworkOrProviderImplementation);
        Assert.True(first.QualityGateScan.FutureRuntimeStreamingContractsOnly);
        Assert.DoesNotContain(first.QualityGateScan.Diagnostics, item => item.Severity == "error");
    }

    [Fact]
    public async Task EvidenceWritesRequiredGoal098Files()
    {
        var repoRoot = FindRepoRoot();
        var service = new GeoworldContractEvidenceService();

        var write = await service.BuildAndWriteAsync(repoRoot);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.TaxonomyJsonPath));
        Assert.True(File.Exists(write.StreamingPolicyMatrixJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.LfzPatternLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));

        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("geoworld_source_adapter_streaming_contract_verification required", report);
        Assert.Contains("noNetworkOrProviderImplementation: true", report);
        Assert.Contains("futureRuntimeStreamingContractsOnly: true", report);
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
