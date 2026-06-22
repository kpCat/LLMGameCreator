using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.GamePackage;
using Xunit;

namespace LLMGameCreator.Tests.Application;

public sealed class UnityArchiveRequestPipelineTests
{
    [Fact]
    public void BuildRequestsWithEmptyPackageReturnsEmptyRequestsAndReady()
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var archive = presets.CreateTopDownGeneratedRpgArchive();
        var service = new UnityArchiveAssetAudioLuaRequestService();

        var result = service.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = ".",
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = archive,
            RuntimeModules = presets.ListRuntimeModules(),
            Package = null
        });

        Assert.NotEmpty(archive.UiLayouts);
        Assert.NotEmpty(brief.AudioStyleWishes);
        Assert.Equal(UnityArchiveRequestReadiness.ReadyWithWarnings, result.Readiness);
        Assert.Contains(result.Diagnostics, d => d.Code == "request.diagnostic.future_provider_kind.asset.comfyui_future");
        Assert.Contains(result.Diagnostics, d => d.Code == "request.diagnostic.future_provider_kind.audio.local_audio_future");
        Assert.NotEmpty(result.LuaModuleRequests);
    }

    [Fact]
    public async Task BuildRequestsWithSamplePackageCreatesExpectedAssetRequests()
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var service = new UnityArchiveAssetAudioLuaRequestService();

        var result = service.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = ".",
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            Package = CreatePackage()
        });

        Assert.Equal(UnityArchiveRequestReadiness.ReadyWithWarnings, result.Readiness);
        Assert.NotEmpty(result.AssetRequests);

        var assetIds = result.AssetRequests.Select(r => r.AssetId).ToList();
        Assert.Contains("portrait.npc.npc-alpha", assetIds);
        Assert.Contains("portrait.npc.npc-guide", assetIds);
        Assert.Contains("icon.item.item-key", assetIds);
        Assert.Contains("icon.item.item-apple", assetIds);
        Assert.Contains("icon.ability.ability-smash", assetIds);
        Assert.Contains("icon.mechanic.mechanic-combat", assetIds);
        Assert.Contains("tile.tile-grass", assetIds);
        Assert.Contains("illustration.scene.scene-clearing", assetIds);
        Assert.Contains("illustration.scene.scene-start", assetIds);
        Assert.Contains("background.map.map-town", assetIds);
        Assert.Contains("ui.theme.generic_unity_player_topdown", assetIds);
        Assert.Contains("ui.widget.widget-health", assetIds);

        foreach (var request in result.AssetRequests)
        {
            Assert.False(string.IsNullOrWhiteSpace(request.RequestId));
        }
    }

    [Fact]
    public async Task BuildRequestsWithSamplePackageCreatesExpectedAudioRequests()
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var service = new UnityArchiveAssetAudioLuaRequestService();

        var result = service.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = ".",
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            Package = CreatePackage()
        });

        Assert.Equal(UnityArchiveRequestReadiness.ReadyWithWarnings, result.Readiness);
        Assert.NotEmpty(result.AudioRequests);

        var audioIds = result.AudioRequests.Select(r => r.AudioId).ToList();
        Assert.Contains("sfx.ui.confirm", audioIds);
        Assert.Contains("sfx.ui.cancel", audioIds);
        Assert.Contains("sfx.ui.click", audioIds);
        Assert.Contains("sfx.footstep.tile-grass", audioIds);
        Assert.Contains("sfx.ability.ability-smash", audioIds);
        Assert.Contains("ambience.scene.scene-clearing", audioIds);
        Assert.Contains("ambience.scene.scene-start", audioIds);
        Assert.Contains("ambience.map.map-town", audioIds);
        Assert.Contains("music.theme.short_sfx", audioIds);
    }

    [Fact]
    public void BuildRequestsWithTopDownModulesCreatesLuaModuleRequests()
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var service = new UnityArchiveAssetAudioLuaRequestService();

        var result = service.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = ".",
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            Package = CreatePackage()
        });

        Assert.Equal(UnityArchiveRequestReadiness.ReadyWithWarnings, result.Readiness);
        Assert.NotEmpty(result.LuaModuleRequests);

        var moduleIds = result.LuaModuleRequests.Select(r => r.ModuleId).ToList();
        Assert.Contains("lua-request.inventory", moduleIds);
        Assert.Contains("lua-request.quest_journal", moduleIds);
        Assert.Contains("lua-request.dialogue", moduleIds);
        Assert.Contains("lua-request.combat", moduleIds);
        Assert.Contains("lua-request.crafting", moduleIds);
        Assert.Contains("lua-request.stats", moduleIds);
        Assert.Contains("lua-request.world_map", moduleIds);

        Assert.DoesNotContain("lua-request.army_battle_future", moduleIds);
        Assert.All(result.LuaModuleRequests, r => Assert.Equal(UnityArchiveRequestProviderKind.none, r.ProviderKind));
    }

    [Fact]
    public void BuildRequestsWithFutureMixedViewModulesProducesLuaWarningsNotErrors()
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerMixedViewFuture, out var profile));
        var service = new UnityArchiveAssetAudioLuaRequestService();

        var result = service.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = ".",
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive() with
            {
                TargetProfileId = profile.TargetProfileId,
                RuntimeModuleIds = profile.RequiredRuntimeModuleIds
            },
            RuntimeModules = presets.ListRuntimeModules(),
            Package = CreatePackage()
        });

        Assert.Equal(UnityArchiveRequestReadiness.ReadyWithWarnings, result.Readiness);
        Assert.All(result.Diagnostics, d => Assert.NotEqual(UnityArchiveExportDiagnosticSeverity.Error, d.Severity));
        Assert.Contains(result.Diagnostics, d => d.Code == "request.diagnostic.future_lua_module");
        Assert.Contains(result.LuaModuleRequests, r => r.ProviderKind == UnityArchiveRequestProviderKind.procedural_future);
    }

    [Fact]
    public void BuildRequestsProducesDeterministicOutputAcrossRuns()
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var service = new UnityArchiveAssetAudioLuaRequestService();
        var package = CreatePackage();

        var first = service.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = ".",
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            Package = package
        });
        var second = service.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = ".",
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            Package = package
        });

        var firstJson = JsonSerializer.Serialize(first, DefaultJsonOptions);
        var secondJson = JsonSerializer.Serialize(second, DefaultJsonOptions);
        Assert.Equal(firstJson, secondJson);
    }

    [Fact]
    public void BuildRequestsAggregatesFutureProviderWarningsByProviderKind()
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var service = new UnityArchiveAssetAudioLuaRequestService();

        var result = service.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = ".",
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            Package = null
        });

        Assert.Equal(UnityArchiveRequestReadiness.ReadyWithWarnings, result.Readiness);

        var assetComfyuiWarnings = result.Diagnostics
            .Where(d => d.Code == "request.diagnostic.future_provider_kind.asset.comfyui_future")
            .ToList();
        Assert.Single(assetComfyuiWarnings);
        Assert.Contains("1 request(s)", assetComfyuiWarnings[0].Message);
    }

    [Fact]
    public void BuildRequestsReportsDuplicateAssetRequestIdsAndBlocksByErrors()
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var service = new UnityArchiveAssetAudioLuaRequestService();

        var package = new GamePackageDefinition
        {
            Manifest = new LLMGameCreator.Domain.Definitions.GameManifest
            {
                PackageId = "game/duplicate-ids",
                Title = "Duplicate IDs"
            },
            Game = new LLMGameCreator.Domain.Definitions.GameDefinition
            {
                Items =
                [
                    new LLMGameCreator.Domain.Definitions.ItemDefinition { Id = "item/alpha", Name = "Alpha", Kind = "tool" },
                    new LLMGameCreator.Domain.Definitions.ItemDefinition { Id = "item.alpha", Name = "Alpha Dot", Kind = "tool" }
                ]
            },
            GeneratedContent = new LLMGameCreator.GamePackage.GeneratedContentDefinition()
        };

        var result = service.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = ".",
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            Package = package
        });

        Assert.Equal(UnityArchiveRequestReadiness.BlockedByErrors, result.Readiness);
        Assert.Contains(result.Diagnostics, d => d.Code == "request.diagnostic.duplicate_asset_request_id");
        Assert.Contains(result.Diagnostics, d => d.TargetId.Contains("item-alpha"));
    }

    [Fact]
    public void BuildRequestsNormalizesBlankIdsToUnknownAndReportsDuplicates()
    {
        var presets = new UnityTargetContractPresetProvider();
        var briefPresets = new GameDesignBriefPresetProvider();
        Assert.True(briefPresets.TryGet(GameDesignBriefPresetProvider.TopDownGeneratedRpg, out var brief));
        Assert.True(presets.TryGetTargetProfile(UnityTargetContractPresetProvider.GenericUnityPlayerTopDown, out var profile));
        var service = new UnityArchiveAssetAudioLuaRequestService();

        var package = new GamePackageDefinition
        {
            Manifest = new LLMGameCreator.Domain.Definitions.GameManifest
            {
                PackageId = "game/blank-ids",
                Title = "Blank IDs"
            },
            Game = new LLMGameCreator.Domain.Definitions.GameDefinition
            {
                Items =
                [
                    new LLMGameCreator.Domain.Definitions.ItemDefinition { Id = "", Name = "Empty", Kind = "tool" },
                    new LLMGameCreator.Domain.Definitions.ItemDefinition { Id = " ", Name = "Space", Kind = "tool" }
                ]
            },
            GeneratedContent = new LLMGameCreator.GamePackage.GeneratedContentDefinition()
        };

        var result = service.BuildRequests(new UnityArchiveRequestPipelineRequest
        {
            ProjectRootPath = ".",
            DesignBrief = brief,
            TargetProfile = profile,
            ArchiveManifest = presets.CreateTopDownGeneratedRpgArchive(),
            RuntimeModules = presets.ListRuntimeModules(),
            Package = package
        });

        Assert.Equal(UnityArchiveRequestReadiness.BlockedByErrors, result.Readiness);
        Assert.Contains(result.AssetRequests, r => r.AssetId == "icon.item.unknown");
        Assert.Contains(result.Diagnostics, d => d.Code == "request.diagnostic.duplicate_asset_request_id");
    }

    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

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
}
