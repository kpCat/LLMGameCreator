using System.Text.Json;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveProviderJobPlanProductSmokeTests
{
    private static readonly string[] RequiredPlanFiles =
    [
        "production/fulfillment-plan.json",
        "production/readiness-report.json",
        "assets/asset-slots.json",
        "audio/audio-slots.json",
        "lua/module-slots.json",
        "providers/manual-import/jobs.json",
        "providers/comfyui/jobs.json",
        "providers/suno/jobs.json",
        "providers/local-audio/jobs.json",
        "providers/procedural/jobs.json"
    ];

    [Fact]
    public async Task UnityArchiveProviderJobPlanProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var service = CreateService();
        var request = new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            GamePackage = CreatePackage()
        };

        var first = await service.MaterializeAsync(request);
        Assert.All(RequiredPlanFiles, relativePath => Assert.True(File.Exists(ArchivePath(first.OutputDirectoryPath, relativePath)), relativePath));

        var firstContents = RequiredPlanFiles.ToDictionary(
            relativePath => relativePath,
            relativePath => File.ReadAllText(ArchivePath(first.OutputDirectoryPath, relativePath)),
            StringComparer.Ordinal);
        foreach (var content in firstContents.Values)
        {
            using var document = JsonDocument.Parse(content);
            Assert.True(document.RootElement.TryGetProperty("schemaVersion", out _));
        }

        var assetRequestCount = await ReadArrayCount(first.OutputDirectoryPath, "assets/asset-requests.json", "requests");
        var audioRequestCount = await ReadArrayCount(first.OutputDirectoryPath, "audio/audio-requests.json", "requests");
        var luaRequestCount = await ReadArrayCount(first.OutputDirectoryPath, "lua/module-requests.json", "requests");
        Assert.Equal(assetRequestCount, await ReadArrayCount(first.OutputDirectoryPath, "assets/asset-slots.json", "slots"));
        Assert.Equal(audioRequestCount, await ReadArrayCount(first.OutputDirectoryPath, "audio/audio-slots.json", "slots"));
        Assert.Equal(luaRequestCount, await ReadArrayCount(first.OutputDirectoryPath, "lua/module-slots.json", "slots"));

        using (var fulfillment = JsonDocument.Parse(firstContents["production/fulfillment-plan.json"]))
        {
            var slots = fulfillment.RootElement.GetProperty("slots").EnumerateArray().ToList();
            Assert.Equal(assetRequestCount + audioRequestCount + luaRequestCount, slots.Count);
            Assert.All(slots, slot =>
            {
                var expectedPath = slot.GetProperty("expectedOutputRelativePath").GetString()!;
                Assert.True(UnityArchiveProviderJobPlanService.IsSafeExpectedOutputRelativePath(expectedPath));
                Assert.DoesNotContain("..", expectedPath, StringComparison.Ordinal);
                Assert.False(File.Exists(ArchivePath(first.OutputDirectoryPath, expectedPath)));
                Assert.Equal("missing", slot.GetProperty("status").GetString());
            });
        }

        foreach (var providerPath in RequiredPlanFiles.Where(path => path.StartsWith("providers/", StringComparison.Ordinal)))
        {
            using var providerJobs = JsonDocument.Parse(firstContents[providerPath]);
            Assert.False(providerJobs.RootElement.GetProperty("executionEnabled").GetBoolean());
            Assert.All(providerJobs.RootElement.GetProperty("jobs").EnumerateArray(), job =>
            {
                Assert.Equal("planned_not_executed", job.GetProperty("readiness").GetString());
                Assert.False(job.GetProperty("executionEnabled").GetBoolean());
            });
        }

        var second = await service.MaterializeAsync(request);
        Assert.All(RequiredPlanFiles, relativePath =>
            Assert.Equal(firstContents[relativePath], File.ReadAllText(ArchivePath(second.OutputDirectoryPath, relativePath))));
    }

    private static UnityArchiveMaterializationService CreateService()
    {
        return new UnityArchiveMaterializationService(new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer()));
    }

    private static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new LLMGameCreator.Domain.Definitions.GameManifest
            {
                PackageId = "game/provider-job-plan-smoke",
                Title = "Provider Job Plan Smoke"
            },
            Game = new LLMGameCreator.Domain.Definitions.GameDefinition
            {
                TilePrototypes =
                [
                    new LLMGameCreator.Domain.Definitions.TilePrototypeDefinition
                    {
                        Id = "tile/grass",
                        Name = "Grass",
                        Walkable = true,
                        MovementCost = 1.0
                    }
                ],
                Items =
                [
                    new LLMGameCreator.Domain.Definitions.ItemDefinition
                    {
                        Id = "item/key",
                        Name = "Key",
                        Kind = "tool"
                    }
                ]
            },
            GeneratedContent = new GeneratedContentDefinition
            {
                Npcs =
                [
                    new GeneratedNpcDefinition
                    {
                        SourceId = "npc/alpha",
                        Name = "Alpha",
                        SceneId = "scene/start"
                    }
                ]
            }
        };
    }

    private static async Task<int> ReadArrayCount(string outputDirectory, string relativePath, string propertyName)
    {
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(ArchivePath(outputDirectory, relativePath)));
        return document.RootElement.GetProperty(propertyName).GetArrayLength();
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
