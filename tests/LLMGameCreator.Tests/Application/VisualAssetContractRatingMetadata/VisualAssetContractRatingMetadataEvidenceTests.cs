using System.Text.Json;
using LLMGameCreator.Application.Design.VisualAssetContractRatingMetadata;
using Xunit;

namespace LLMGameCreator.Tests.Application.VisualAssetContractRatingMetadata;

public sealed class VisualAssetContractRatingMetadataEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsBoundariesExplicit()
    {
        var repoRoot = FindRepoRoot();
        var service = new VisualAssetContractRatingMetadataEvidenceService();

        var first = service.Build(repoRoot);
        var second = service.Build(repoRoot);

        Assert.Equal(first.CatalogJson, second.CatalogJson);
        Assert.Equal(first.RatingPolicyMatrixJson, second.RatingPolicyMatrixJson);
        Assert.Equal(first.ValidationMatrixJson, second.ValidationMatrixJson);
        Assert.Equal(first.NegativeProofJson, second.NegativeProofJson);
        Assert.Equal(first.SourceDocumentLineageJson, second.SourceDocumentLineageJson);
        Assert.Equal(first.QualityGateScanJson, second.QualityGateScanJson);
        Assert.Equal(first.Report.DeterministicReportHash, second.Report.DeterministicReportHash);
        Assert.True(first.ValidationMatrix.Passed);
        Assert.True(first.NegativeProof.Passed);
        Assert.True(first.SourceDocumentLineage.Passed);
        Assert.False(first.Report.Accepted);
        Assert.Equal(VisualAssetContractRatingMetadataVocabulary.FinalGate, first.Report.ManualGate);
        Assert.True(first.QualityGateScan.NoPublicGamePackageSchemaChanged);
        Assert.True(first.QualityGateScan.NoRuntimeChanged);
        Assert.True(first.QualityGateScan.NoUnityChanged);
        Assert.True(first.QualityGateScan.NoProviderOrLlmOrRagOrMediaExecution);
        Assert.True(first.QualityGateScan.NoProjectFilesChanged);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndInspectable()
    {
        using var temp = new TempDirectory();
        var repoRoot = FindRepoRoot();
        var service = new VisualAssetContractRatingMetadataEvidenceService();
        var result = service.Build(repoRoot);

        var write = await service.WriteAsync(temp.Path, result);

        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.CatalogJsonPath));
        Assert.True(File.Exists(write.RatingPolicyMatrixJsonPath));
        Assert.True(File.Exists(write.ValidationMatrixJsonPath));
        Assert.True(File.Exists(write.NegativeProofJsonPath));
        Assert.True(File.Exists(write.SourceDocumentLineageJsonPath));
        Assert.True(File.Exists(write.QualityGateScanJsonPath));
        Assert.Contains("visual_asset_contract_rating_metadata_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));

        using var catalog = JsonDocument.Parse(await File.ReadAllTextAsync(write.CatalogJsonPath));
        using var validation = JsonDocument.Parse(await File.ReadAllTextAsync(write.ValidationMatrixJsonPath));
        using var negative = JsonDocument.Parse(await File.ReadAllTextAsync(write.NegativeProofJsonPath));
        using var quality = JsonDocument.Parse(await File.ReadAllTextAsync(write.QualityGateScanJsonPath));

        Assert.Equal(6, catalog.RootElement.GetProperty("fixtureCount").GetInt32());
        Assert.True(validation.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(negative.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(quality.RootElement.GetProperty("noGeneratedImageAssetsAdded").GetBoolean());
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
