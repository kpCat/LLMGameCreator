using System.Text.Json;
using LLMGameCreator.Application.Design.LuaSandboxExecutionGate;
using Xunit;

namespace LLMGameCreator.Tests.Application.LuaSandboxExecutionGate;

public sealed class LuaSandboxEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsManualGateRequired()
    {
        var service = new LuaSandboxExecutionGateEvidenceService();

        var first = service.Build();
        var second = service.Build();

        Assert.Equal(first.ArtifactJsonByFileName[LuaSandboxExecutionGateEvidenceService.PolicySummaryJsonFileName], second.ArtifactJsonByFileName[LuaSandboxExecutionGateEvidenceService.PolicySummaryJsonFileName]);
        Assert.Equal(first.ArtifactJsonByFileName[LuaSandboxExecutionGateEvidenceService.HostBindingMatrixJsonFileName], second.ArtifactJsonByFileName[LuaSandboxExecutionGateEvidenceService.HostBindingMatrixJsonFileName]);
        Assert.Equal(first.DecisionJsonByFileName[LuaSandboxExecutionGateEvidenceService.MetamoduleDecisionJsonFileName], second.DecisionJsonByFileName[LuaSandboxExecutionGateEvidenceService.MetamoduleDecisionJsonFileName]);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(first.Report.Accepted);
        Assert.Equal(LuaSandboxExecutionGateEvidenceService.FinalGate, first.Report.ManualGate);
        Assert.False(first.Report.LuaExecuted);
        Assert.False(first.Report.LuaParserUsed);
        Assert.False(first.Report.LuaSourceGenerated);
        Assert.False(first.Report.ExternalDependencyAdded);
        Assert.False(first.Report.RuntimeUiUnityGamePackageProviderLlmRagTouched);
        Assert.DoesNotContain(Environment.NewLine, first.ArtifactJsonByFileName[LuaSandboxExecutionGateEvidenceService.PolicySummaryJsonFileName]);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndParse()
    {
        using var temp = new TempDirectory();
        var write = await new LuaSandboxExecutionGateEvidenceService().BuildAndWriteAsync(temp.Path);

        var names = write.WrittenFiles.Select(path => Path.GetFileName(path) ?? string.Empty).OrderBy(item => item, StringComparer.Ordinal).ToArray();
        Assert.Equal(
            [
                "invalid-lua-sandbox-diagnostics-matrix.json",
                "lua-host-binding-matrix.json",
                "lua-sandbox-decision-caravan.json",
                "lua-sandbox-decision-frontier.json",
                "lua-sandbox-decision-gothic.json",
                "lua-sandbox-decision-metamodule.json",
                "lua-sandbox-dry-run-trace-matrix.json",
                "lua-sandbox-execution-gate-report.md",
                "lua-sandbox-execution-requests.json",
                "lua-sandbox-policy-summary.json",
                "lua-sandbox-repair-plan-matrix.json"
            ],
            names);

        using var requests = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.ExecutionRequestsJsonFileName)));
        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.MetamoduleDecisionJsonFileName)));
        using var trace = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.DryRunTraceMatrixJsonFileName)));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, LuaSandboxExecutionGateEvidenceService.InvalidMatrixJsonFileName)));

        Assert.True(requests.RootElement.GetProperty("metamoduleSpeciesArchetypeSlotManifestCount").GetInt32() >= 100);
        Assert.True(metamodule.RootElement.GetProperty("metamoduleSpeciesArchetypeSlotManifestCount").GetInt32() >= 100);
        Assert.False(trace.RootElement.GetProperty("luaExecuted").GetBoolean());
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("lua_sandbox_execution_gate_verification required", report);
        Assert.Contains("luaExecuted=false", report);
        Assert.Contains("No real Lua execution happened", report);
        Assert.Contains("No Runtime/UI/Unity/GamePackage/provider/LLM/RAG path was touched", report);
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
