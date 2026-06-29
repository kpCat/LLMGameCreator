using System.Text.Json;
using LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class WorldScaleRegionMapFoundationProductSmokeTests
{
    [Fact]
    public async Task Goal038WorldScaleRegionMapFoundationProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var write = await new WorldScaleRegionMapEvidenceService().BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.RegionGraphSummaryJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.ReachabilityMatrixJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, FiniteMapPackBuilder.FileName("frontier_survival"))));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, FiniteMapPackBuilder.FileName("gothic_intrigue"))));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, FiniteMapPackBuilder.FileName("caravan_trade"))));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, FiniteMapPackBuilder.FileName("metamodule_kingdoms"))));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.ChunkedWorldConfigPreludeJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.TraversalItineraryMatrixJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.InvalidMatrixJsonFileName)));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var graph = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.RegionGraphSummaryJsonFileName)));
        using var reachability = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.ReachabilityMatrixJsonFileName)));
        using var frontier = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, FiniteMapPackBuilder.FileName("frontier_survival"))));
        using var gothic = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, FiniteMapPackBuilder.FileName("gothic_intrigue"))));
        using var caravan = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, FiniteMapPackBuilder.FileName("caravan_trade"))));
        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, FiniteMapPackBuilder.FileName("metamodule_kingdoms"))));
        using var chunks = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.ChunkedWorldConfigPreludeJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, WorldScaleRegionMapEvidenceService.InvalidMatrixJsonFileName)));
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.Equal(4, graph.RootElement.GetProperty("scenarioCount").GetInt32());
        Assert.Equal(4, reachability.RootElement.GetProperty("scenarioCount").GetInt32());
        Assert.True(reachability.RootElement.GetProperty("allRequiredTargetsReachable").GetBoolean());
        Assert.Equal("frontier_survival", frontier.RootElement.GetProperty("scenarioId").GetString());
        Assert.Equal("gothic_intrigue", gothic.RootElement.GetProperty("scenarioId").GetString());
        Assert.Equal("caravan_trade", caravan.RootElement.GetProperty("scenarioId").GetString());
        Assert.Equal("metamodule_kingdoms", metamodule.RootElement.GetProperty("scenarioId").GetString());
        Assert.Equal("axial_hex", gothic.RootElement.GetProperty("coordinateKind").GetString());
        Assert.True(metamodule.RootElement.GetProperty("regionBindings").GetArrayLength() >= 14);
        Assert.Equal(4, chunks.RootElement.GetProperty("scenarioCount").GetInt32());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("goal037AcceptedByUserHandoff: true", report);
        Assert.Contains("world_scale_region_map_foundation_verification required", report);
        Assert.Contains("No Runtime, UI, Unity, GamePackage schema, provider, LLM/RAG, Lua source/execution, generator-library or external dependency change", report);
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
