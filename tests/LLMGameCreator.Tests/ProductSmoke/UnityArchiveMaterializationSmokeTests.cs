using System.Text.Json;
using LLMGameCreator.Application.Composition;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveMaterializationSmokeTests
{
    [Fact]
    public async Task UnityArchiveMaterializationProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var currentProfile));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerMixedViewFuture, out var futureProfile));
        var service = CreateService();

        var current = await service.MaterializeAsync(new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = currentProfile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules()
        });

        Assert.True(Directory.Exists(current.OutputDirectoryPath));
        Assert.Equal(UnityArchiveMaterializationReadiness.MaterializedPlayableContract, current.Readiness);
        Assert.All(new[]
        {
            "manifest/unity-game-archive.json",
            "composition/game-design-brief.json",
            "composition/unity-target-profile.json",
            "composition/runtime-modules-index.json",
            "ui/layouts-index.json",
            "assets/asset-requests.json",
            "audio/audio-requests.json",
            "localization/index.json",
            "lua/modules-index.json",
            "export-report.md",
            "export-validation.json"
        }, relativePath => Assert.True(File.Exists(ArchivePath(current.OutputDirectoryPath, relativePath)), relativePath));

        using (var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(ArchivePath(current.OutputDirectoryPath, "manifest/unity-game-archive.json"))))
        {
            Assert.Equal("topdown-generated-rpg", manifest.RootElement.GetProperty("gameId").GetString());
        }

        using (var modules = JsonDocument.Parse(await File.ReadAllTextAsync(ArchivePath(current.OutputDirectoryPath, "composition/runtime-modules-index.json"))))
        {
            var moduleIds = modules.RootElement.GetProperty("modules").EnumerateArray()
                .Select(module => module.GetProperty("moduleId").GetString())
                .ToList();
            Assert.All(currentProfile.RequiredRuntimeModuleIds, moduleId => Assert.Contains(moduleId, moduleIds));
        }

        var futureArchive = presets.CreateTopDownGeneratedRpgArchive() with
        {
            TargetProfileId = futureProfile.TargetProfileId,
            RuntimeModuleIds = futureProfile.RequiredRuntimeModuleIds
        };
        var future = await service.MaterializeAsync(new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = futureProfile,
            ArchiveManifest = futureArchive,
            RuntimeModules = presets.ListRuntimeModules()
        });

        Assert.Equal(UnityArchiveMaterializationReadiness.MaterializedMetadataOnly, future.Readiness);
        Assert.Equal(UnityArchiveExportReadiness.BlockedByFutureModules, future.DryRunResult.Plan.Readiness);
        Assert.Null(future.ZipFilePath);
        Assert.All(future.MaterializedFiles, file =>
        {
            Assert.False(Path.IsPathRooted(file.RelativePath));
            Assert.DoesNotContain("..", file.RelativePath, StringComparison.Ordinal);
        });
        Assert.All(presets.ListRuntimeModules(), module => Assert.DoesNotContain("provider", module.ModuleId, StringComparison.OrdinalIgnoreCase));
        Assert.All(presets.ListRuntimeModules(), module => Assert.DoesNotContain("generator", module.ModuleId, StringComparison.OrdinalIgnoreCase));
    }

    private static UnityArchiveMaterializationService CreateService()
    {
        return new UnityArchiveMaterializationService(new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer()));
    }

    private static string ResolveProjectFolder(string tempPath)
    {
        var configured = Environment.GetEnvironmentVariable("LLMGC_PRODUCT_SMOKE_PROJECT_DIR");
        var projectFolder = string.IsNullOrWhiteSpace(configured) ? tempPath : configured;
        Directory.CreateDirectory(projectFolder);
        return projectFolder;
    }

    private static string ArchivePath(string outputDirectory, string relativePath)
    {
        return Path.Combine(outputDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
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
