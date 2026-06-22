using System.Text.Json;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveGameDataPayloadTests
{
    private static readonly string[] RequiredFiles =
    [
        "data/game-package.json",
        "data/generated-content-index.json",
        "data/scenes-index.json",
        "data/npcs-index.json",
        "data/quests-index.json",
        "data/dialogues-index.json",
        "data/items-index.json",
        "data/encounters-index.json"
    ];

    [Fact]
    public async Task UnityArchiveGameDataPayloadWritesRequiredDeterministicSortedFiles()
    {
        using var temp = new TempDirectory();
        var service = new UnityArchiveGameDataPayloadService();
        var request = new UnityArchiveGameDataPayloadRequest
        {
            ProjectRootPath = temp.Path,
            Package = CreatePackage()
        };

        var first = await service.WriteAsync(request);
        var firstContents = ReadContents(first.OutputDirectoryPath);
        var second = await service.WriteAsync(request);
        var secondContents = ReadContents(second.OutputDirectoryPath);

        Assert.Equal("game/payload-smoke", first.SourcePackageId);
        Assert.Equal(RequiredFiles.OrderBy(path => path), first.WrittenFiles.Select(file => file.RelativePath).OrderBy(path => path));
        Assert.Equal(firstContents, secondContents);
        Assert.All(RequiredFiles, relativePath => Assert.True(File.Exists(ProjectPath(temp.Path, relativePath)), relativePath));
        Assert.All(Directory.EnumerateFiles(first.OutputDirectoryPath), path => Assert.False(File.ReadAllBytes(path).Take(3).SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF })));

        using var items = JsonDocument.Parse(await File.ReadAllTextAsync(ProjectPath(temp.Path, "data/items-index.json")));
        var itemEntries = items.RootElement.GetProperty("entries").EnumerateArray().ToList();
        Assert.Equal(["item/apple", "item/generated", "item/zeta"], itemEntries.Select(entry => entry.GetProperty("id").GetString()!).ToArray());
        Assert.Equal(["Food", "quest", "rare"], itemEntries[0].GetProperty("tags").EnumerateArray().Select(value => value.GetString()!).ToArray());

        using var encounters = JsonDocument.Parse(await File.ReadAllTextAsync(ProjectPath(temp.Path, "data/encounters-index.json")));
        var linkedIds = encounters.RootElement.GetProperty("entries")[0].GetProperty("linkedIds").EnumerateArray()
            .Select(value => value.GetString()!).ToArray();
        Assert.Equal(["npc/alpha", "npc/zeta", "region/forest", "scene/clearing"], linkedIds);

        foreach (var indexPath in RequiredFiles.Where(path => path.EndsWith("index.json", StringComparison.Ordinal)))
        {
            var json = await File.ReadAllTextAsync(ProjectPath(temp.Path, indexPath));
            Assert.DoesNotContain("timestamp", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("generatedAt", json, StringComparison.OrdinalIgnoreCase);
            using var document = JsonDocument.Parse(json);
            Assert.Equal("1", document.RootElement.GetProperty("schemaVersion").GetString());
            Assert.True(document.RootElement.TryGetProperty("category", out _));
            Assert.Equal(JsonValueKind.Array, document.RootElement.GetProperty("entries").ValueKind);
        }
    }

    [Fact]
    public async Task UnityArchiveGameDataPayloadWritesValidEmptyCategoryIndexes()
    {
        using var temp = new TempDirectory();
        var package = new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/empty" }
        };

        var result = await new UnityArchiveGameDataPayloadService().WriteAsync(new UnityArchiveGameDataPayloadRequest
        {
            ProjectRootPath = temp.Path,
            Package = package
        });

        foreach (var category in new[] { "scenes", "npcs", "quests", "dialogues", "items", "encounters" })
        {
            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(result.OutputDirectoryPath, $"{category}-index.json")));
            Assert.Equal(category, document.RootElement.GetProperty("category").GetString());
            Assert.Equal("game/empty", document.RootElement.GetProperty("sourcePackageId").GetString());
            Assert.Empty(document.RootElement.GetProperty("entries").EnumerateArray());
        }
    }

    [Fact]
    public async Task UnityArchiveGameDataPayloadRejectsEscapingOutputPath()
    {
        using var temp = new TempDirectory();
        var escaped = Path.GetFullPath(Path.Combine(temp.Path, "..", "escaped-unity-data"));
        if (Directory.Exists(escaped))
        {
            Directory.Delete(escaped, true);
        }

        await Assert.ThrowsAsync<InvalidOperationException>(() => new UnityArchiveGameDataPayloadService().WriteAsync(
            new UnityArchiveGameDataPayloadRequest
            {
                ProjectRootPath = temp.Path,
                RelativeOutputDirectory = "../escaped-unity-data",
                Package = CreatePackage()
            }));

        Assert.False(Directory.Exists(escaped));
    }

    [Fact]
    public async Task UnityArchiveGameDataPayloadIntegratesOnlyWhenPackageIsSupplied()
    {
        using var temp = new TempDirectory();
        var service = CreateMaterializationService();
        var current = CreateMaterializationRequest(temp.Path) with { GamePackage = CreatePackage() };

        var materialized = await service.MaterializeAsync(current);

        Assert.Contains(materialized.MaterializedFiles, file => file.RelativePath == UnityArchiveGameDataPayloadService.GamePackageFilePath);
        Assert.True(File.Exists(ProjectPath(temp.Path, "data/game-package.json")));

        var presets = new UnityTargetContractPresetProvider();
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerMixedViewFuture, out var futureProfile));
        var future = await service.MaterializeAsync(current with
        {
            TargetProfile = futureProfile,
            ArchiveManifest = current.ArchiveManifest with
            {
                TargetProfileId = futureProfile.TargetProfileId,
                RuntimeModuleIds = futureProfile.RequiredRuntimeModuleIds
            },
            GamePackage = null
        });

        Assert.Equal(UnityArchiveMaterializationReadiness.MaterializedMetadataOnly, future.Readiness);
        Assert.DoesNotContain(future.MaterializedFiles, file => file.RelativePath.StartsWith("data/", StringComparison.Ordinal));
        Assert.False(Directory.Exists(Path.Combine(future.OutputDirectoryPath, "data")));
    }

    private static GamePackageDefinition CreatePackage()
    {
        return new GamePackageDefinition
        {
            Manifest = new GameManifest { PackageId = "game/payload-smoke", Title = "Payload Smoke" },
            Game = new GameDefinition
            {
                Maps =
                [
                    new MapDefinition { Id = "map/town", Name = "Town", DefaultTileId = "tile/ground" }
                ],
                Items =
                [
                    new ItemDefinition { Id = "item/zeta", Name = "Zeta", Kind = "tool" },
                    new ItemDefinition { Id = "item/apple", Name = "Apple", Kind = "food", Tags = ["rare", "Food", "quest"] }
                ]
            },
            GeneratedContent = new GeneratedContentDefinition
            {
                Scenes = [new GeneratedSceneDefinition { SourceId = "scene/clearing", PackageMapId = "map/town", Title = "Clearing" }],
                Npcs = [new GeneratedNpcDefinition { SourceId = "npc/alpha", Name = "Alpha", RegionId = "region/forest", SceneId = "scene/clearing" }],
                Quests = [new GeneratedQuestSeedDefinition { SourceId = "quest/trail", PackageQuestId = "quest/trail", Title = "Trail" }],
                Dialogues = [new GeneratedDialogueDefinition { SourceId = "dialogue/guide", Title = "Guide", NpcId = "npc/alpha", SceneId = "scene/clearing" }],
                Items = [new GeneratedItemDefinition { SourceId = "item/generated", Name = "Generated Item" }],
                Encounters =
                [
                    new GeneratedEncounterDefinition
                    {
                        SourceId = "encounter/ambush",
                        Title = "Ambush",
                        RegionId = "region/forest",
                        SceneId = "scene/clearing",
                        NpcIds = ["npc/zeta", "npc/alpha"]
                    }
                ]
            }
        };
    }

    private static UnityArchiveMaterializationRequest CreateMaterializationRequest(string projectRoot)
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

    private static UnityArchiveMaterializationService CreateMaterializationService()
    {
        return new UnityArchiveMaterializationService(new UnityArchiveExportDryRunService(
            new UnityTargetContractValidator(),
            new UnityArchiveExportPlanMarkdownRenderer()));
    }

    private static IReadOnlyList<string> ReadContents(string outputDirectory)
    {
        return Directory.EnumerateFiles(outputDirectory)
            .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
            .ThenBy(path => Path.GetFileName(path), StringComparer.Ordinal)
            .Select(File.ReadAllText)
            .ToList();
    }

    private static string ProjectPath(string projectRoot, string archiveRelativePath)
    {
        return Path.Combine(projectRoot, ".llmgc", "unity-archive", archiveRelativePath.Replace('/', Path.DirectorySeparatorChar));
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
