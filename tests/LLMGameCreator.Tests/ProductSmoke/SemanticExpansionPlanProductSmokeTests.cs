using System.Text.Json;
using LLMGameCreator.Application.Design.SemanticArtifactContracts;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class SemanticExpansionPlanProductSmokeTests
{
    [Fact]
    public async Task Goal030SemanticArtifactContractRegistryProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = new SemanticArtifactContractEvidenceService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.RegistrySummaryJsonPath));
        Assert.True(File.Exists(write.CompatibilityMatrixJsonPath));
        Assert.True(File.Exists(write.FrontierPlanJsonPath));
        Assert.True(File.Exists(write.GothicPlanJsonPath));
        Assert.True(File.Exists(write.CaravanPlanJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var frontier = JsonDocument.Parse(await File.ReadAllTextAsync(write.FrontierPlanJsonPath));
        using var gothic = JsonDocument.Parse(await File.ReadAllTextAsync(write.GothicPlanJsonPath));
        using var caravan = JsonDocument.Parse(await File.ReadAllTextAsync(write.CaravanPlanJsonPath));

        Assert.Equal("frontier_survival", frontier.RootElement.GetProperty("profileId").GetString());
        Assert.Equal("gothic_intrigue", gothic.RootElement.GetProperty("profileId").GetString());
        Assert.Equal("caravan_trade", caravan.RootElement.GetProperty("profileId").GetString());
        Assert.Contains("semantic_artifact_contract_registry_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));
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
