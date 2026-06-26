using System.Text.Json;
using LLMGameCreator.Application.Design.UnityGeneratedScene;
using LLMGameCreator.Tests.Application.Assets;
using LLMGameCreator.Tests.Application.ContentGeneration;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

[Collection("UnityAlphaProductSmoke")]
public sealed class UnityGeneratedSceneProjectionSmokeTests
{
    [Fact]
    public async Task UnityGeneratedSceneProjectionProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var repoRoot = FindRepoRoot();
        var content = ContentGenerationScaleAcceptanceTestFactory.CreateService()
            .BuildFromReferencePackDirectory(Path.Combine(repoRoot, "samples", "content-generation-packs"), projectRoot);
        var assets = MinimumAssetPipelineAcceptanceTestFactory.CreateService()
            .BuildFromContentGeneration(projectRoot, Path.Combine(repoRoot, "samples", "minimum-asset-pipeline"), content);
        var service = new UnityGeneratedSceneProjectionAcceptanceService();

        var result = service.BuildFromAcceptedEvidence(
            projectRoot,
            content,
            assets,
            new UnityGeneratedSceneProjectionOptions
            {
                RepositoryRootPath = repoRoot,
                ExecuteUnityBuild = true,
                LaunchBuiltPlayer = true
            });
        var write = await service.WriteAsync(projectRoot, result);

        Assert.True(File.Exists(write.ProjectionJsonPath));
        Assert.True(File.Exists(write.ReportJsonPath));
        Assert.True(File.Exists(write.ReportMarkdownPath));
        Assert.True(File.Exists(write.VerificationMarkdownPath));

        var report = JsonSerializer.Deserialize<UnityGeneratedSceneProjectionReport>(
            await File.ReadAllTextAsync(write.ReportJsonPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.False(report.Accepted);
        Assert.Equal(UnityGeneratedSceneProjectionAcceptanceService.FinalGate, report.FinalStatus);
        Assert.Equal("unity_playable_presentation_firewall_safe_build_verification passed", report.PreviousAcceptedGate);
        Assert.Equal("unity-generated-scene-projection", report.ProductSmokeRoute);
        Assert.Equal("frontier_survival", report.SelectedStyleId);
        Assert.True(report.SceneProjectionVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(report.SceneNodesResolved);
        Assert.True(report.GeneratedIdBindingVerified);
        Assert.True(report.AssetBindingVerified);
        Assert.True(report.FirewallSafeBuildVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.True(report.InvalidMatrix.Passed, string.Join(Environment.NewLine, report.InvalidMatrix.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        Assert.False(report.RuntimePreviewDependency);
        Assert.False(report.PublicGamePackageSchemaChanged);
        Assert.False(report.ProjectFilesChanged);
        Assert.False(report.GeneratorLibraryChanged);

        if (report.AlphaBuild.WindowsExecutableProduced)
        {
            Assert.True(report.AlphaBuild.UnityBuildProduced);
            Assert.True(report.AlphaBuild.LaunchVerified, string.Join(Environment.NewLine, report.AlphaBuild.LaunchVerification.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
            Assert.True(report.MovementVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
            Assert.True(report.InteractionVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
            Assert.True(report.PlayLoopVerified, string.Join(Environment.NewLine, report.Diagnostics.Select(item => $"{item.Code}:{item.Target}")));
        }
        else
        {
            Assert.False(report.AlphaBuild.LaunchVerified);
            Assert.Contains(report.AlphaBuild.Diagnostics, item => item.Code.StartsWith("alpha_build.unity_build.", StringComparison.Ordinal) || item.Code == "alpha_build.environment.unity_not_found");
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
