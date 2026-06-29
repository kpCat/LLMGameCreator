using System.Text.Json;
using LLMGameCreator.Application.Design.HybridDraftLuaExpansion;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class HybridDraftLuaExpansionProductSmokeTests
{
    [Fact]
    public async Task Goal037HybridDraftLuaExpansionProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var write = await new HybridDraftLuaExpansionEvidenceService().BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.AdapterSelectionJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.PipelineSummaryJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.DraftToLuaRequestMapJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.SandboxApprovedMatrixJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.FrontierOutputJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.GothicOutputJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.CaravanOutputJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.MetamoduleOutputJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.PromotionDecisionMatrixJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.InvalidMatrixJsonFileName)));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var pipeline = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.PipelineSummaryJsonFileName)));
        using var frontier = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.FrontierOutputJsonFileName)));
        using var gothic = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.GothicOutputJsonFileName)));
        using var caravan = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.CaravanOutputJsonFileName)));
        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.MetamoduleOutputJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, HybridDraftLuaExpansionEvidenceService.InvalidMatrixJsonFileName)));
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.Equal(4, pipeline.RootElement.GetProperty("scenarioCount").GetInt32());
        Assert.Equal(8, pipeline.RootElement.GetProperty("outputCount").GetInt32());
        Assert.True(pipeline.RootElement.GetProperty("realBoundedExecutorPathProven").GetBoolean());
        Assert.Equal("frontier_survival", frontier.RootElement.GetProperty("scenarioId").GetString());
        Assert.Equal("gothic_intrigue", gothic.RootElement.GetProperty("scenarioId").GetString());
        Assert.Equal("caravan_trade", caravan.RootElement.GetProperty("scenarioId").GetString());
        Assert.Equal("metamodule_kingdoms", metamodule.RootElement.GetProperty("scenarioId").GetString());
        Assert.True(metamodule.RootElement.GetProperty("slotCount").GetInt32() >= 100);
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("hybrid_llm_draft_lua_deterministic_expansion_verification required", report);
        Assert.Contains("LuaCSharp 0.5.5", report);
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
