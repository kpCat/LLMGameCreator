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

        var result = service.BuildFromAcceptedEvidence(
            projectRoot,
            content,
            assets,
            new AlphaRunnableBuildOptions
            {
                RepositoryRootPath = repoRoot,
                ExecuteUnityBuild = true,
                LaunchBuiltPlayer = true
            });
        var write = await service.WriteAsync(projectRoot, result);

        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));
        Assert.True(Directory.Exists(write.StagingDirectoryPath));
        Assert.True(Directory.Exists(write.BuildDirectoryPath));

        var report = JsonSerializer.Deserialize<AlphaRunnableBuildReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal("alpha-runnable-build", report.ProductSmokeRoute);
        Assert.Equal(3, report.StyleCandidates.Count);
        Assert.True(report.StyleCandidates.All(candidate => candidate.Accepted));
        Assert.Equal("frontier_survival", report.PrimaryBuildCandidate.StyleId);
        Assert.True(report.Staging.Passed);
        Assert.True(report.InvalidMatrix.Passed);
        Assert.True(report.BuildEnvironment.RepoUnityProjectFound);
        Assert.True(report.BuildEnvironment.RepoBuildScriptFound);
        Assert.False(report.RuntimePreviewDependency);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);
        Assert.True(report.UnityEditorExecuted || report.BuildEnvironment.Diagnostics.Any(item => item.Code == "alpha_build.environment.unity_not_found"));

        if (report.WindowsExecutableProduced)
        {
            Assert.Equal(AlphaRunnableBuildAcceptanceService.FinalGate, report.FinalStatus);
            Assert.False(report.BlockerReached);
            Assert.True(report.UnityBuildProduced);
            Assert.True(report.BuildOutput.Passed, string.Join(Environment.NewLine, report.BuildOutput.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
            Assert.NotEmpty(report.BuildOutput.ExecutableRelativePath);
            Assert.Contains(report.BuildOutput.Files, item => item.Kind == "windows_executable");
            Assert.Contains(report.BuildOutput.Files, item => item.RelativePath == "LLMGameCreatorAlpha_Data/StreamingAssets/LLMGameCreatorAlpha/game-data/game-package.json");
            Assert.True(report.LaunchVerified, string.Join(Environment.NewLine, report.LaunchVerification.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
            Assert.True(report.PlayLoopVerified, string.Join(Environment.NewLine, report.LaunchVerification.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
            Assert.False(string.IsNullOrWhiteSpace(report.LaunchVerification.LogRelativePath));
            Assert.False(string.IsNullOrWhiteSpace(report.LaunchVerification.PlayLoopLogRelativePath));
        }
        else
        {
            Assert.True(report.BlockerReached, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}:{item.Message}")));
            Assert.Equal(AlphaRunnableBuildAcceptanceService.BlockerGate, report.FinalStatus);
            Assert.Contains(report.BuildOutput.Diagnostics, item => item.Code == "alpha_build.output.missing_executable");
            Assert.Contains(report.Diagnostics, item => item.Code.StartsWith("alpha_build.unity_build.", StringComparison.Ordinal) || item.Code == "alpha_build.environment.unity_not_found");
        }

        if (report.LaunchVerified)
        {
            Assert.Contains(report.LaunchVerification.Diagnostics, item => item.Code == "alpha_build.launch.executed");
        }
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
