using System.Text.Json;
using LLMGameCreator.Application.Design.Assets;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class MinimumAssetPipelineSmokeTests
{
    [Fact]
    public async Task MinimumAssetPipelineProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(FindRepoRoot(), "samples", "content-generation-packs"), projectRoot);
        var service = MinimumAssetPipelineAcceptanceTestFactory.CreateService();

        var write = await service.BuildAndWriteAsync(projectRoot, Path.Combine(FindRepoRoot(), "samples", "minimum-asset-pipeline"), content);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var report = JsonSerializer.Deserialize<MinimumAssetPipelineReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.True(report.Accepted);
        Assert.Equal("minimum_asset_pipeline_artifact_verification", report.ManualGate);
        Assert.Equal(3, report.GeneratedInputCount);
        Assert.True(report.TotalResolvedAssetSlots >= 90);
        Assert.True(report.ValidMatrixPassed);
        Assert.True(report.InvalidMatrix.Passed);
        Assert.True(report.PackageContentBindingPassed);
        Assert.True(report.AssetValidationPassed);
        Assert.True(report.ReplayEvidence.Passed);
        Assert.True(report.VariationEvidence.Passed);
        Assert.All(report.Runs, run =>
        {
            Assert.True(run.PackageBindingAudit.Passed);
            Assert.True(run.AssetValidation.Passed);
            Assert.All(run.ResolvedAssets, asset =>
            {
                Assert.False(Path.IsPathRooted(asset.RelativePath));
                Assert.True(File.Exists(Path.Combine(projectRoot, asset.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
            });
        });
        Assert.False(report.ExternalExecution.LlmExecuted);
        Assert.False(report.ExternalExecution.RagExecuted);
        Assert.False(report.ExternalExecution.ProviderExecuted);
        Assert.False(report.ExternalExecution.LuaExecuted);
        Assert.False(report.ExternalExecution.UnityExecuted);
        Assert.False(report.ExternalExecution.MediaExecuted);
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
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
