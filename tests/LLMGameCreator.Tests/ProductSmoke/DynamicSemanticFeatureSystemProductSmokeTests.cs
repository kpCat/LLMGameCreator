using System.Text.Json;
using LLMGameCreator.Application.Design.DynamicSemanticFeatures;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class DynamicSemanticFeatureSystemProductSmokeTests
{
    [Fact]
    public async Task Goal032DynamicSemanticFeatureSystemProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new DynamicSemanticFeatureEvidenceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.FeatureCatalogSummaryJsonPath));
        Assert.True(File.Exists(write.InfluenceRuleSummaryJsonPath));
        Assert.True(File.Exists(write.AuthoringSchemaMatrixJsonPath));
        Assert.True(File.Exists(write.FrontierStateJsonPath));
        Assert.True(File.Exists(write.GothicStateJsonPath));
        Assert.True(File.Exists(write.CaravanStateJsonPath));
        Assert.True(File.Exists(write.MetamoduleKingdomsStateJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(write.MetamoduleKingdomsStateJsonPath));
        Assert.Equal("metamodule_kingdoms", metamodule.RootElement.GetProperty("scenarioId").GetString());
        Assert.Contains("dynamic_semantic_feature_system_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));
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
