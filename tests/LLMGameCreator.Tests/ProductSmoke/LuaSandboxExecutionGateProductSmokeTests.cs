using System.Text.Json;
using LLMGameCreator.Application.Design.LuaSandboxExecutionGate;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class LuaSandboxExecutionGateProductSmokeTests
{
    [Fact]
    public async Task Goal036LuaSandboxExecutionGateProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var write = await new LuaSandboxExecutionGateEvidenceService().BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.PolicySummaryJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.HostBindingMatrixJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.ExecutionRequestsJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.FrontierDecisionJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.GothicDecisionJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.CaravanDecisionJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.MetamoduleDecisionJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.DryRunTraceMatrixJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.RepairPlanMatrixJsonFileName)));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.InvalidMatrixJsonFileName)));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var requests = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.ExecutionRequestsJsonFileName)));
        using var trace = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.DryRunTraceMatrixJsonFileName)));
        Assert.Equal(4, requests.RootElement.GetProperty("requestCount").GetInt32());
        Assert.True(requests.RootElement.GetProperty("metamoduleSpeciesArchetypeSlotManifestCount").GetInt32() >= 100);
        Assert.False(trace.RootElement.GetProperty("luaExecuted").GetBoolean());
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("lua_sandbox_execution_gate_verification required", report);
        Assert.Contains("luaExecuted=false", report);
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
