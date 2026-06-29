using System.Text.Json;
using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;
using Xunit;

namespace LLMGameCreator.Tests.Application.LuaModuleManifestRegistry;

public sealed class LuaModuleManifestEvidenceTests
{
    [Fact]
    public void EvidenceBuildIsDeterministicAndKeepsManualGateRequired()
    {
        var service = new LuaModuleManifestEvidenceService();

        var first = service.Build();
        var second = service.Build();

        Assert.Equal(first.ArtifactJsonByFileName[LuaModuleManifestEvidenceService.RegistrySummaryJsonFileName], second.ArtifactJsonByFileName[LuaModuleManifestEvidenceService.RegistrySummaryJsonFileName]);
        Assert.Equal(first.ArtifactJsonByFileName[LuaModuleManifestEvidenceService.HostApiSurfacePolicyJsonFileName], second.ArtifactJsonByFileName[LuaModuleManifestEvidenceService.HostApiSurfacePolicyJsonFileName]);
        Assert.Equal(first.SelectionJsonByFileName[LuaModuleManifestEvidenceService.MetamoduleSelectionJsonFileName], second.SelectionJsonByFileName[LuaModuleManifestEvidenceService.MetamoduleSelectionJsonFileName]);
        Assert.Equal(first.Report.DeterministicHash, second.Report.DeterministicHash);
        Assert.True(first.Report.ContractProofPassed, string.Join(Environment.NewLine, first.Report.Diagnostics.Select(item => $"{item.Severity}:{item.Code}:{item.Target}")));
        Assert.False(first.Report.Accepted);
        Assert.Equal(LuaModuleManifestEvidenceService.FinalGate, first.Report.ManualGate);
        Assert.True(first.Report.NoLuaExecutionOrParsing);
        Assert.True(first.Report.NoLuaSourceGenerated);
        Assert.True(first.Report.NoProviderLlmRagCallHappened);
        Assert.True(first.Report.NoRuntimeUiUnityGamePackageMutation);
        Assert.DoesNotContain(Environment.NewLine, first.ArtifactJsonByFileName[LuaModuleManifestEvidenceService.RegistrySummaryJsonFileName]);
    }

    [Fact]
    public async Task EvidenceArtifactsAreWrittenAndParse()
    {
        using var temp = new TempDirectory();
        var write = await new LuaModuleManifestEvidenceService().BuildAndWriteAsync(temp.Path);

        var names = write.WrittenFiles.Select(path => Path.GetFileName(path) ?? string.Empty).OrderBy(item => item, StringComparer.Ordinal).ToArray();
        Assert.Equal(
            [
                "invalid-lua-manifest-diagnostics-matrix.json",
                "lua-host-api-surface-policy.json",
                "lua-module-dependency-plan.json",
                "lua-module-manifest-registry-report.md",
                "lua-module-registry-summary.json",
                "lua-module-selection-caravan.json",
                "lua-module-selection-frontier.json",
                "lua-module-selection-gothic.json",
                "lua-module-selection-metamodule-kingdoms.json"
            ],
            names);

        using var summary = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, "lua-module-registry-summary.json")));
        using var metamodule = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, "lua-module-selection-metamodule-kingdoms.json")));
        using var invalid = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, "invalid-lua-manifest-diagnostics-matrix.json")));

        Assert.True(summary.RootElement.GetProperty("metamoduleSpeciesArchetypeSlotManifestCount").GetInt32() >= 100);
        Assert.True(metamodule.RootElement.GetProperty("summary").GetProperty("selectedCount").GetInt32() >= 100);
        Assert.True(invalid.RootElement.GetProperty("passed").GetBoolean());
        var report = await File.ReadAllTextAsync(write.ReportMarkdownPath);
        Assert.Contains("lua_module_manifest_registry_verification required", report);
        Assert.Contains("accepted=false", report);
        Assert.Contains("No Lua execution or parsing happened", report);
        Assert.Contains("No provider/LLM/RAG call happened", report);
        Assert.Contains("No Runtime/UI/Unity/GamePackage schema mutation happened", report);
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
