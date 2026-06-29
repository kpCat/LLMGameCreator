using System.Text.Json;
using LLMGameCreator.Application.Design.LuaModuleManifestRegistry;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class LuaModuleManifestRegistryProductSmokeTests
{
    [Fact]
    public async Task Goal035LuaModuleManifestRegistryProductSmoke()
    {
        var repoRoot = FindRepoRoot();
        var projectRoot = ResolveProjectFolder(repoRoot);
        var write = await new LuaModuleManifestEvidenceService().BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "lua-module-registry-summary.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "lua-host-api-surface-policy.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "lua-module-selection-frontier.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "lua-module-selection-gothic.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "lua-module-selection-caravan.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "lua-module-selection-metamodule-kingdoms.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "lua-module-dependency-plan.json")));
        Assert.True(File.Exists(Path.Combine(write.OutputDirectoryPath, "invalid-lua-manifest-diagnostics-matrix.json")));
        Assert.True(File.Exists(write.ReportMarkdownPath));

        using var summary = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(write.OutputDirectoryPath, "lua-module-registry-summary.json")));
        Assert.Equal(11, summary.RootElement.GetProperty("familyCount").GetInt32());
        Assert.True(summary.RootElement.GetProperty("metamoduleSpeciesArchetypeSlotManifestCount").GetInt32() >= 100);
        Assert.Contains("lua_module_manifest_registry_verification required", await File.ReadAllTextAsync(write.ReportMarkdownPath));
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
