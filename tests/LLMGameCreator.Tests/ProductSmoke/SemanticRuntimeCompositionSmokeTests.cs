using LLMGameCreator.Tests.Application.Semantics;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class SemanticRuntimeCompositionSmokeTests
{
    [Fact]
    public async Task SemanticRuntimeCompositionProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var service = SemanticRuntimeCompositionAcceptanceTests.CreateService();

        var write = await service.BuildAndWriteAsync(projectRoot);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        var json = await File.ReadAllTextAsync(write.ReportJsonPath);
        Assert.Contains("\"accepted\": true", json);
        Assert.Contains("\"manualGate\": \"semantic_selected_runtime_composition_artifact_verification\"", json);
        Assert.Contains("\"semanticSelectedIdsExecutedInRuntime\": true", json);
        Assert.Contains("\"invalidScenarioRejected\": true", json);
        Assert.Contains("\"llmExecuted\": false", json);
        Assert.Contains("\"ragExecuted\": false", json);
        Assert.Contains("\"providerExecuted\": false", json);
        Assert.Contains("\"luaExecuted\": false", json);
        Assert.Contains("\"unityExecuted\": false", json);
        Assert.Contains("\"mediaExecuted\": false", json);
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
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
                Directory.Delete(Path, true);
            }
        }
    }
}
