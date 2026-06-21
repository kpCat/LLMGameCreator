using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveExportDryRunSmokeTests
{
    [Fact]
    public async Task UnityArchiveExportDryRunProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var currentProfile));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerMixedViewFuture, out var futureProfile));
        var service = new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer());

        var current = await service.ExportAsync(new UnityArchiveExportDryRunRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = currentProfile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules()
        });
        var currentPlanJson = await File.ReadAllTextAsync(current.PlanJsonPath);
        var currentMarkdown = await File.ReadAllTextAsync(current.PlanMarkdownPath);
        var second = await service.ExportAsync(new UnityArchiveExportDryRunRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = currentProfile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules()
        });

        Assert.True(Directory.Exists(current.OutputDirectoryPath));
        Assert.All(
            new[] { current.PlanJsonPath, current.PlanMarkdownPath, current.ArchiveManifestJsonPath, current.ValidationReportJsonPath },
            path => Assert.True(File.Exists(path), path));
        Assert.Contains(current.Plan.Readiness, new[]
        {
            UnityArchiveExportReadiness.ExportableNow,
            UnityArchiveExportReadiness.ExportableWithWarnings
        });
        Assert.Equal(currentPlanJson, await File.ReadAllTextAsync(second.PlanJsonPath));
        Assert.Equal(currentMarkdown, await File.ReadAllTextAsync(second.PlanMarkdownPath));

        var futureArchive = presets.CreateTopDownGeneratedRpgArchive() with
        {
            TargetProfileId = futureProfile.TargetProfileId,
            RuntimeModuleIds = futureProfile.RequiredRuntimeModuleIds
        };
        var future = await service.ExportAsync(new UnityArchiveExportDryRunRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = futureProfile,
            ArchiveManifest = futureArchive,
            RuntimeModules = presets.ListRuntimeModules()
        });

        Assert.Equal(UnityArchiveExportReadiness.BlockedByFutureModules, future.Plan.Readiness);
        Assert.Contains(future.Plan.Diagnostics, diagnostic =>
            diagnostic.Code == UnityArchiveExportDiagnosticCodes.FutureRuntimeModule);
        Assert.All(future.Plan.PlannedFiles, file => Assert.DoesNotContain("..", file.RelativePath, StringComparison.Ordinal));
        Assert.All(presets.ListRuntimeModules(), module => Assert.DoesNotContain("provider", module.ModuleId, StringComparison.OrdinalIgnoreCase));
        Assert.All(presets.ListRuntimeModules(), module => Assert.DoesNotContain("generator", module.ModuleId, StringComparison.OrdinalIgnoreCase));
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
