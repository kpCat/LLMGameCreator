using LLMGameCreator.Application.Design.GeneratorSpineQualityConsolidation;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class GeneratorSpineQualityConsolidationProductSmokeTests
{
    [Fact]
    public async Task Goal072GeneratorSpineQualityConsolidationEvidenceIsProducedForReview()
    {
        var write = await new GeneratorSpineQualityEvidenceService().BuildAndWriteAsync(ProjectRoot());

        Assert.Contains(write.Result.QualityDashboard.Status, new[] { "GREEN", "BLOCKED" });
        if (write.Result.QualityDashboard.P0Count == 0)
        {
            Assert.Equal("GREEN", write.Result.QualityDashboard.Status);
        }
        else
        {
            Assert.Equal("BLOCKED", write.Result.QualityDashboard.Status);
        }

        Assert.True(write.Result.ProofQualityRiskReport.Goal071ProofIndicators.ProofQualityPassed);
        Assert.True(write.Result.Inventory.SourceFileCount > 0);
        Assert.True(write.Result.Inventory.ArtifactFileCount > 0);
        Assert.True(write.Result.Inventory.ProductSmokeFileCount > 0);
        Assert.True(write.Result.UnityAlphaBootstrapRiskReport.LineCount > 0);

        foreach (var fileName in GeneratorSpineQualityVocabulary.RequiredEvidenceFiles)
        {
            Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, fileName)), "Missing artifact: " + fileName);
        }

        var report = await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, GeneratorSpineQualityEvidenceService.ReportMarkdownFileName));
        Assert.Contains("generator_spine_quality_consolidation_verification required", report);
        Assert.Contains("accepted=false", report);
        Assert.True(File.Exists(write.DebtRegisterMarkdownPath));
    }

    private static string ProjectRoot()
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
