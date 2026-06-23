using System.Text.Json;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.ProductSmoke;

public sealed class UnityArchiveRequestPipelineProductSmokeTests
{
    [Fact]
    public async Task UnityArchiveRequestPipelineProductSmoke()
    {
        using var temp = new TempDirectory();
        var projectRoot = ResolveProjectFolder(temp.Path);
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var currentProfile));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerMixedViewFuture, out var futureProfile));
        var service = CreateService();

        var package = CreatePackage();

        var first = await service.MaterializeAsync(new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = currentProfile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            GamePackage = package
        });

        Assert.Equal(UnityArchiveMaterializationReadiness.MaterializedWithWarnings, first.Readiness);
        Assert.All(new[]
        {
            "assets/asset-requests.json",
            "assets/asset-request-index.json",
            "audio/audio-requests.json",
            "audio/audio-request-index.json",
            "lua/module-requests.json",
            "lua/modules-index.json"
        }, relativePath => Assert.True(File.Exists(ArchivePath(first.OutputDirectoryPath, relativePath)), relativePath));

        foreach (var requestPath in new[]
        {
            "assets/asset-requests.json",
            "assets/asset-request-index.json",
            "audio/audio-requests.json",
            "audio/audio-request-index.json",
            "lua/module-requests.json",
            "lua/modules-index.json"
        })
        {
            var json = await File.ReadAllTextAsync(ArchivePath(first.OutputDirectoryPath, requestPath));
            using var document = JsonDocument.Parse(json);
            Assert.True(document.RootElement.TryGetProperty("schemaVersion", out _));
        }

        var firstAssetRequests = await File.ReadAllTextAsync(ArchivePath(first.OutputDirectoryPath, "assets/asset-requests.json"));
        var firstAudioRequests = await File.ReadAllTextAsync(ArchivePath(first.OutputDirectoryPath, "audio/audio-requests.json"));
        var firstLuaRequests = await File.ReadAllTextAsync(ArchivePath(first.OutputDirectoryPath, "lua/module-requests.json"));

        var second = await service.MaterializeAsync(new UnityArchiveMaterializationRequest
        {
            ProjectRootPath = projectRoot,
            DesignBrief = brief,
            TargetProfile = currentProfile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            GamePackage = package
        });

        Assert.Equal(firstAssetRequests, await File.ReadAllTextAsync(ArchivePath(second.OutputDirectoryPath, "assets/asset-requests.json")));
        Assert.Equal(firstAudioRequests, await File.ReadAllTextAsync(ArchivePath(second.OutputDirectoryPath, "audio/audio-requests.json")));
        Assert.Equal(firstLuaRequests, await File.ReadAllTextAsync(ArchivePath(second.OutputDirectoryPath, "lua/module-requests.json")));

        using (var assetRequests = JsonDocument.Parse(firstAssetRequests))
        {
            var requestIds = assetRequests.RootElement.GetProperty("requests").EnumerateArray()
                .Select(r => r.GetProperty("requestId").GetString()!).ToList();
            Assert.Contains(requestIds, id => id.StartsWith("asset-request.ui_widget.", StringComparison.Ordinal));
            Assert.Contains(requestIds, id => id.StartsWith("asset-request.tile_texture.", StringComparison.Ordinal));
            Assert.Contains(requestIds, id => id.StartsWith("asset-request.scene_illustration.", StringComparison.Ordinal));
            Assert.Contains(requestIds, id => id.StartsWith("asset-request.portrait.", StringComparison.Ordinal));
            Assert.Contains(requestIds, id => id.StartsWith("asset-request.icon.", StringComparison.Ordinal));
        }

        using (var audioRequests = JsonDocument.Parse(firstAudioRequests))
        {
            var audioKinds = audioRequests.RootElement.GetProperty("requests").EnumerateArray()
                .Select(r => r.GetProperty("audioKind").GetString()!).ToList();
            Assert.Contains(audioKinds, k => k == "ui_sfx");
            Assert.Contains(audioKinds, k => k == "footstep");
            Assert.Contains(audioKinds, k => k == "ability");
            Assert.Contains(audioKinds, k => k == "scene_ambience");
            Assert.Contains(audioKinds, k => k == "music");
        }

        using (var luaRequests = JsonDocument.Parse(firstLuaRequests))
        {
            var moduleKinds = luaRequests.RootElement.GetProperty("requests").EnumerateArray()
                .Select(r => r.GetProperty("moduleKind").GetString()!).ToList();
            Assert.Contains(moduleKinds, k => k == "inventory");
            Assert.Contains(moduleKinds, k => k == "quest_journal");
            Assert.Contains(moduleKinds, k => k == "dialogue");
            Assert.Contains(moduleKinds, k => k == "combat");
            Assert.Contains(moduleKinds, k => k == "crafting");
            Assert.Contains(moduleKinds, k => k == "world_map");
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
            RuntimeModules = presets.ListRuntimeModules(),
            GamePackage = package
        });

        Assert.Equal(UnityArchiveMaterializationReadiness.MaterializedMetadataOnly, future.Readiness);
        Assert.All(future.MaterializedFiles, file =>
        {
            Assert.False(Path.IsPathRooted(file.RelativePath));
            Assert.DoesNotContain("..", file.RelativePath, StringComparison.Ordinal);
        });
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
                PackageId = "game/request-pipeline-smoke",
                Title = "Request Pipeline Smoke"
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
                Maps =
                [
                    new LLMGameCreator.Domain.Definitions.MapDefinition
                    {
                        Id = "map/town",
                        Name = "Town",
                        DefaultTileId = "tile/grass",
                        Width = 10,
                        Height = 10
                    }
                ],
                Items =
                [
                    new LLMGameCreator.Domain.Definitions.ItemDefinition
                    {
                        Id = "item/key",
                        Name = "Key",
                        Kind = "tool"
                    },
                    new LLMGameCreator.Domain.Definitions.ItemDefinition
                    {
                        Id = "item/apple",
                        Name = "Apple",
                        Kind = "food",
                        Tags = ["Food", "quest"]
                    }
                ],
                Abilities =
                [
                    new LLMGameCreator.Domain.Definitions.AbilityDefinition
                    {
                        Id = "ability/smash",
                        Name = "Smash",
                        Kind = "active"
                    }
                ],
                Quests =
                [
                    new LLMGameCreator.Domain.Definitions.QuestDefinition
                    {
                        Id = "quest/start",
                        Title = "Start Quest"
                    }
                ],
                Dialogues =
                [
                    new LLMGameCreator.Domain.Definitions.DialogueDefinition
                    {
                        Id = "dialogue/guide",
                        Title = "Guide"
                    }
                ],
                Factions =
                [
                    new LLMGameCreator.Domain.Definitions.FactionDefinition
                    {
                        Id = "faction/town",
                        Name = "Town"
                    }
                ]
            },
            GeneratedContent = new LLMGameCreator.GamePackage.GeneratedContentDefinition
            {
                Scenes =
                [
                    new LLMGameCreator.GamePackage.GeneratedSceneDefinition
                    {
                        SourceId = "scene/clearing",
                        PackageMapId = "map/town",
                        Title = "Clearing"
                    },
                    new LLMGameCreator.GamePackage.GeneratedSceneDefinition
                    {
                        SourceId = "scene/start",
                        PackageMapId = "map/town",
                        Title = "Start"
                    }
                ],
                Npcs =
                [
                    new LLMGameCreator.GamePackage.GeneratedNpcDefinition
                    {
                        SourceId = "npc/alpha",
                        Name = "Alpha",
                        SceneId = "scene/clearing"
                    },
                    new LLMGameCreator.GamePackage.GeneratedNpcDefinition
                    {
                        SourceId = "npc/guide",
                        Name = "Guide",
                        SceneId = "scene/start"
                    }
                ],
                Mechanics =
                [
                    new LLMGameCreator.GamePackage.GeneratedMechanicDefinition
                    {
                        SourceId = "mechanic/combat",
                        Name = "Combat",
                        Tags = ["combat"]
                    }
                ],
                Items =
                [
                    new LLMGameCreator.GamePackage.GeneratedItemDefinition
                    {
                        SourceId = "item/gem",
                        Name = "Gem"
                    }
                ]
            }
        };
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
