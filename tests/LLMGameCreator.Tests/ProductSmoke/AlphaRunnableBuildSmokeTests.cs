using System.Text.Json;
using LLMGameCreator.Application.Design.AlphaBuild;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class AlphaRunnableBuildSmokeTests
{
    [Fact]
    public async Task AlphaRunnableBuildProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), projectRoot);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(projectRoot, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new AlphaRunnableBuildAcceptanceService();

        var write = await service.BuildAndWriteAsync(projectRoot, content, assets);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(Directory.Exists(write.StagingDirectoryPath));
        Assert.True(Directory.Exists(write.BuildDirectoryPath));

        var report = JsonSerializer.Deserialize<AlphaRunnableBuildReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.True(report.BlockerReached, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
        Assert.Equal("alpha_unity_build_environment_blocker", report.FinalStatus);
        Assert.Equal("alpha-runnable-build", report.ProductSmokeRoute);
        Assert.Equal(3, report.StyleCandidates.Count);
        Assert.True(report.StyleCandidates.All(candidate => candidate.Accepted));
        Assert.Equal("frontier_survival", report.PrimaryBuildCandidate.StyleId);
        Assert.True(report.Staging.Passed);
        Assert.True(report.InvalidMatrix.Passed);
        Assert.False(report.WindowsExecutableProduced);
        Assert.False(report.UnityEditorExecuted);
        Assert.False(report.UnityBuildProduced);
        Assert.False(report.LaunchVerified);
        Assert.False(report.PlayLoopVerified);
        Assert.False(report.RuntimePreviewDependency);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.False(report.ExternalExecution.AnyExecuted());
        Assert.Contains(report.BuildOutput.Diagnostics, item => item.Code == "alpha_build.output.no_supported_repo_build_path");
        Assert.Contains(report.BuildEnvironment.Diagnostics, item => item.Code == "alpha_build.environment.no_repo_unity_project");
        Assert.Contains(report.BuildEnvironment.Diagnostics, item => item.Code == "alpha_build.environment.no_repo_build_script");
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
