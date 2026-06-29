using System.Text.Json;
using LLMGameCreator.Application.Design.RuntimeChunkDeltaTraversal;
using LLMGameCreator.Tests.Application.RuntimeChunkDeltaTraversal;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class RuntimeChunkDeltaTraversalProductSmokeTests
{
    [Fact]
    public async Task RuntimeChunkDeltaTraversalProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var service = RuntimeChunkDeltaTraversalTestFactory.CreateService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.FrontierPlanJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.GothicPlanJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.CaravanPlanJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.MetamodulePlanJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.FrontierStateJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.MetamoduleStateJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.InvalidMatrixJsonFileName)));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var frontier = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.FrontierPlanJsonFileName)));
        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.MetamodulePlanJsonFileName)));
        using var frontierState = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.FrontierStateJsonFileName)));
        using var roundtrip = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.SaveLoadRoundtripProofJsonFileName)));
        using var replay = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.ReplayDeterminismProofJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, RuntimeChunkDeltaEvidenceService.InvalidMatrixJsonFileName)));
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);

        Assert.Equal("frontier_survival", frontier.RootElement.GetProperty("scenarioId").GetString());
        Assert.Equal("metamodule_kingdoms", metamodule.RootElement.GetProperty("scenarioId").GetString());
        Assert.True(metamodule.RootElement.GetProperty("sourceFacts").GetProperty("kingdomGroupCount").GetInt32() >= 7);
        Assert.True(metamodule.RootElement.GetProperty("sourceFacts").GetProperty("speciesArchetypeSlotRefCount").GetInt32() >= 112);
        Assert.NotEmpty(frontierState.RootElement.GetProperty("runtimeDeltas").EnumerateArray());
        Assert.True(roundtrip.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(roundtrip.RootElement.GetProperty("scenarios").EnumerateArray().All(item =>
            item.GetProperty("usedRuntimeStateSerializer").GetBoolean() &&
            item.GetProperty("usedRuntimeSnapshotStore").GetBoolean()));
        Assert.True(replay.RootElement.GetProperty("passed").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        Assert.Contains("runtime_chunk_delta_traversal_smoke_verification required", report);
        Assert.Contains("accepted=false", report);
        Assert.Contains("No GamePackage schema/source definition, WinForms/UI, Unity, provider, LLM/RAG, Lua source/execution, generator-library or external dependency change", report);
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
