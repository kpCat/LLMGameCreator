using System.Text.Json;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveGameDataPayloadSmokeTests
{
    [Fact]
    public async Task UnityArchiveGameDataPayloadProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var service = CreateService();
        var request = CreateRequest(projectRoot) with { GamePackage = CreatePackage() };

        var first = await service.MaterializeAsync(request);
        var firstData = ReadDataFiles(first.OutputDirectoryPath);
        var second = await service.MaterializeAsync(request);
        var secondData = ReadDataFiles(second.OutputDirectoryPath);

        Assert.Equal(UnityArchiveMaterializationReadiness.MaterializedPlayableContract, first.Readiness);
        Assert.Equal(firstData, secondData);
        Assert.All(new[]
        {
            "data/game-package.json",
            "data/generated-content-index.json",
            "data/scenes-index.json",
            "data/npcs-index.json",
            "data/quests-index.json",
            "data/dialogues-index.json",
            "data/items-index.json",
            "data/encounters-index.json"
        }, relativePath => Assert.True(File.Exists(ArchivePath(first.OutputDirectoryPath, relativePath)), relativePath));

        using (var packageJson = JsonDocument.Parse(await File.ReadAllTextAsync(ArchivePath(first.OutputDirectoryPath, "data/game-package.json"))))
        {
            Assert.Equal("game/product-smoke", packageJson.RootElement.GetProperty("manifest").GetProperty("packageId").GetString());
        }

        foreach (var indexPath in Directory.EnumerateFiles(Path.Combine(first.OutputDirectoryPath, "data"), "*-index.json"))
        {
            var json = await File.ReadAllTextAsync(indexPath);
            using var index = JsonDocument.Parse(json);
            Assert.Equal("1", index.RootElement.GetProperty("schemaVersion").GetString());
            Assert.True(index.RootElement.TryGetProperty("category", out _));
            Assert.Equal(JsonValueKind.Array, index.RootElement.GetProperty("entries").ValueKind);
            Assert.DoesNotContain("timestamp", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("generatedAt", json, StringComparison.OrdinalIgnoreCase);
        }

        var presets = new UnityTargetContractPresetProvider();
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerMixedViewFuture, out var futureProfile));
        var future = await service.MaterializeAsync(request with
        {
            TargetProfile = futureProfile,
            ArchiveManifest = request.ArchiveManifest with
            {
                TargetProfileId = futureProfile.TargetProfileId,
                RuntimeModuleIds = futureProfile.RequiredRuntimeModuleIds
            },
            GamePackage = null
        });

        Assert.Equal(UnityArchiveMaterializationReadiness.MaterializedMetadataOnly, future.Readiness);
        Assert.DoesNotContain(future.MaterializedFiles, file => file.RelativePath.StartsWith("data/", StringComparison.Ordinal));
        Assert.False(Directory.Exists(Path.Combine(future.OutputDirectoryPath, "data")));
        Assert.All(future.MaterializedFiles, file =>
        {
            Assert.False(Path.IsPathRooted(file.RelativePath));
            Assert.DoesNotContain("..", file.RelativePath, StringComparison.Ordinal);
        });
    }

    private static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new LLMGameCreator.Domain.Definitions.GameManifest
            {
                PackageId = "game/product-smoke",
                Title = "Product Smoke"
            },
            GeneratedContent = new GeneratedContentDefinition
            {
                Scenes = [new GeneratedSceneDefinition { SourceId = "scene/start", PackageMapId = "map/start", Title = "Start" }],
                Npcs = [new GeneratedNpcDefinition { SourceId = "npc/guide", Name = "Guide", SceneId = "scene/start" }],
                Quests = [new GeneratedQuestSeedDefinition { SourceId = "quest/start", PackageQuestId = "quest/start", Title = "Start Quest" }],
                Dialogues = [new GeneratedDialogueDefinition { SourceId = "dialogue/guide", Title = "Guide", NpcId = "npc/guide" }],
                Items = [new GeneratedItemDefinition { SourceId = "item/key", Name = "Key" }],
                Encounters = [new GeneratedEncounterDefinition { SourceId = "encounter/start", Title = "Start Encounter", NpcIds = ["npc/guide"] }]
            }
        };
    }

    private static UnityArchiveMaterializationRequest CreateRequest(string projectRoot)
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        return new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules()
        };
    }

    private static UnityArchiveMaterializationService CreateService()
    {
        return new UnityArchiveMaterializationService(new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer()));
    }

    private static IReadOnlyList<string> ReadDataFiles(string outputDirectory)
    {
        return Directory.EnumerateFiles(Path.Combine(outputDirectory, "data"))
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ThenBy(path => Path.GetFileName(path), StringComparer.Ordinal)
            .Select(File.ReadAllText)
            .ToList();
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
