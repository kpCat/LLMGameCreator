using System.Text.Json;
using LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class SemanticAuthoringIntentResolverProductSmokeTests
{
    [Fact]
    public async Task Goal033SemanticAuthoringIntentResolverProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new SemanticAuthoringIntentEvidenceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.WorkspaceSchemaSummaryJsonPath));
        Assert.True(File.Exists(write.MetamoduleLoreSkeletonJsonPath));
        Assert.True(File.Exists(write.ManualVsAutoAuthoringMatrixJsonPath));
        Assert.True(File.Exists(write.FrontierResolutionJsonPath));
        Assert.True(File.Exists(write.GothicResolutionJsonPath));
        Assert.True(File.Exists(write.CaravanResolutionJsonPath));
        Assert.True(File.Exists(write.MetamoduleKingdomsResolutionJsonPath));
        Assert.True(File.Exists(write.InvalidMatrixJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(write.MetamoduleKingdomsResolutionJsonPath));
        Assert.Equal("metamodule_kingdoms", metamodule.RootElement.GetProperty("scenarioId").GetString());
        Assert.Contains("semantic_authoring_intent_resolver_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));
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
