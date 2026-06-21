using LLMGameCreator.Application.Projects;

namespace LLMGameCreator.Application.Composition;

public sealed class UnityTargetContractPresetProvider
{
    public const string GenericUnityPlayerTwoPointFiveD = "generic_unity_player_2_5d";
    public const string GenericUnityPlayerTopDown = "generic_unity_player_topdown";
    public const string GenericUnityPlayerMixedViewFuture = "generic_unity_player_mixed_view_future";

    private static readonly IReadOnlyList<UnityRuntimeModuleContract> RuntimeModules =
    [
        Current("unity.core.archive_loader", "Archive loader"),
        Current("unity.core.save_load", "Save and load"),
        Current("unity.core.input_settings", "Input settings"),
        Current("unity.core.asset_loader", "Asset loader"),
        Current("unity.ui.dynamic_layout", "Dynamic UI layout"),
        Current("unity.ui.data_binding", "UI data binding"),
        Current("unity.audio.short_sfx", "Short sound effects"),
        Current("unity.audio.music_themes", "Music themes"),
        Current("unity.world.topdown_map", "Top-down map"),
        Current("unity.world.streaming", "World streaming", UnityRuntimePerformanceClass.Medium),
        Current("unity.gameplay.stats", "Stats"),
        Current("unity.gameplay.inventory", "Inventory"),
        Current("unity.gameplay.dialogue", "Dialogue"),
        Current("unity.gameplay.quest_journal", "Quest journal"),
        Current("unity.gameplay.personal_combat", "Personal combat", UnityRuntimePerformanceClass.Medium),
        Current("unity.gameplay.crafting", "Crafting"),
        Future("unity.transport.vehicle_future", "Vehicle transport"),
        Future("unity.transport.public_transport_future", "Public transport"),
        Future("unity.society.npc_schedule_future", "NPC schedules", UnityRuntimePerformanceClass.Medium),
        Future("unity.crime.police_future", "Crime and police response", UnityRuntimePerformanceClass.Medium),
        Future("unity.combat.army_battle_future", "Army battles", UnityRuntimePerformanceClass.High),
        Future("unity.world.imported_real_map_future", "Imported real map", UnityRuntimePerformanceClass.High)
    ];

    private static readonly IReadOnlyList<UnityTargetProfile> TargetProfiles =
    [
        new()
        {
            TargetProfileId = GenericUnityPlayerTwoPointFiveD,
            Title = "Generic Unity player 2.5D",
            Maturity = UnityContractMaturity.Current,
            RenderingModes = [UnityRenderingMode.TwoDimensional, UnityRenderingMode.TwoPointFiveDimensional],
            ViewModes = [UnityViewMode.TopDown, UnityViewMode.Isometric],
            InputProfile = "keyboard_mouse_gamepad",
            PerformanceBudget = "desktop_medium",
            RequiredRuntimeModuleIds = CurrentPlayerModules(),
            OptionalRuntimeModuleIds = ["unity.world.streaming", "unity.audio.music_themes"],
            AssetPipelineProfile = "manual_or_future_comfyui_metadata",
            AudioPipelineProfile = "manual_or_future_suno_like_metadata"
        },
        new()
        {
            TargetProfileId = GenericUnityPlayerTopDown,
            Title = "Generic Unity player top-down",
            Maturity = UnityContractMaturity.Current,
            RenderingModes = [UnityRenderingMode.TwoDimensional],
            ViewModes = [UnityViewMode.TopDown, UnityViewMode.WorldMap],
            InputProfile = "keyboard_mouse_gamepad",
            PerformanceBudget = "desktop_low",
            RequiredRuntimeModuleIds = CurrentPlayerModules(),
            OptionalRuntimeModuleIds = ["unity.world.streaming"],
            AssetPipelineProfile = "manual_or_future_comfyui_metadata",
            AudioPipelineProfile = "manual_or_future_suno_like_metadata"
        },
        new()
        {
            TargetProfileId = GenericUnityPlayerMixedViewFuture,
            Title = "Generic Unity player mixed-view future",
            Maturity = UnityContractMaturity.PlannedFuture,
            RenderingModes = [UnityRenderingMode.TwoDimensional, UnityRenderingMode.TwoPointFiveDimensional, UnityRenderingMode.ThreeDimensional],
            ViewModes = [UnityViewMode.TopDown, UnityViewMode.FirstPerson, UnityViewMode.ThirdPerson, UnityViewMode.WorldMap],
            InputProfile = "keyboard_mouse_gamepad_vehicle",
            PerformanceBudget = "desktop_high_future",
            RequiredRuntimeModuleIds = CurrentPlayerModules()
                .Concat(
                [
                    "unity.transport.vehicle_future",
                    "unity.transport.public_transport_future",
                    "unity.society.npc_schedule_future",
                    "unity.crime.police_future",
                    "unity.combat.army_battle_future",
                    "unity.world.imported_real_map_future"
                ])
                .ToList(),
            OptionalRuntimeModuleIds = ["unity.world.streaming"],
            AssetPipelineProfile = "future_generated_mixed_view_assets",
            AudioPipelineProfile = "future_generated_dynamic_audio"
        }
    ];

    public IReadOnlyList<UnityTargetProfile> ListTargetProfiles()
    {
        return TargetProfiles;
    }

    public IReadOnlyList<UnityRuntimeModuleContract> ListRuntimeModules()
    {
        return RuntimeModules;
    }

    public bool TryGetTargetProfile(string? targetProfileId, out UnityTargetProfile profile)
    {
        profile = TargetProfiles.FirstOrDefault(item =>
            string.Equals(item.TargetProfileId, targetProfileId?.Trim(), StringComparison.OrdinalIgnoreCase))!;
        return profile is not null;
    }

    public UnityGameArchiveManifest CreateTopDownGeneratedRpgArchive()
    {
        return new UnityGameArchiveManifest
        {
            GameId = "topdown-generated-rpg",
            Title = "Top-down generated RPG",
            ContentLanguage = ContentLanguageCodes.Russian,
            TargetProfileId = GenericUnityPlayerTopDown,
            DesignBriefId = GameDesignBriefPresetProvider.TopDownGeneratedRpg,
            DataPackages = ["data/package.json"],
            RuntimeModuleIds = CurrentPlayerModules().Concat(["unity.world.streaming"]).ToList(),
            UiLayouts =
            [
                new UnityUiLayoutContract
                {
                    LayoutId = "layout.gameplay.topdown",
                    ThemeId = "theme.frontier",
                    Panels =
                    [
                        new UnityUiPanelContract
                        {
                            PanelId = "panel.status",
                            PanelKind = "status",
                            Dock = "top",
                            Widgets =
                            [
                                new UnityUiWidgetContract
                                {
                                    WidgetId = "widget.health",
                                    WidgetKind = "health_bar",
                                    BindingId = "binding.player_health"
                                }
                            ]
                        }
                    ],
                    Bindings =
                    [
                        new UnityUiBindingContract
                        {
                            BindingId = "binding.player_health",
                            SourcePath = "player.stats.health",
                            ValueType = "number"
                        }
                    ],
                    InputActions = ["move", "interact", "open_inventory"]
                }
            ],
            AssetRequests =
            [
                new UnityAssetGenerationRequest
                {
                    RequestId = "asset-request.player-portrait",
                    AssetId = "portrait.player",
                    AssetKind = "portrait",
                    Source = UnityAssetRequestSource.Manual,
                    PromptOrInstruction = "Approved player portrait metadata."
                }
            ],
            AudioRequests =
            [
                new UnityAudioGenerationRequest
                {
                    RequestId = "audio-request.ui-confirm",
                    AudioId = "sfx.ui.confirm",
                    AudioKind = "short_sfx",
                    Source = UnityAudioRequestSource.Manual,
                    PromptOrInstruction = "Short confirmation sound metadata."
                }
            ],
            LocalizationFiles = ["localization/ru.json"],
            WorldStreamingPolicy = new UnityWorldStreamingPolicy
            {
                WorldScale = UnityWorldScale.Large,
                ChunkSize = 64,
                ActiveRadius = 1,
                BackgroundSimulationMode = UnityBackgroundSimulationMode.AbstractRegions,
                PersistentEntityPolicy = UnityMaterializationPolicy.AuthoredImportantAndLazyGenerated,
                GeneratedEntityBudget = 2000,
                ActiveNpcBudget = 64,
                NpcMaterializationPolicy = UnityMaterializationPolicy.AuthoredImportantAndLazyGenerated,
                QuestMaterializationPolicy = UnityMaterializationPolicy.LazyOnDemand,
                StoreSeedRulesAndTemplates = true,
                MaterializeActiveChunksOnly = true,
                PersistDirtyDeltas = true,
                GenerateNpcsLazily = true,
                GenerateQuestsLazily = true,
                SeparateAuthoredAndGeneratedPopulation = true
            },
            SavePolicy = "seed_templates_and_dirty_deltas",
            BuildExportPolicy = "archive_first_future_standalone_build"
        };
    }

    private static IReadOnlyList<string> CurrentPlayerModules()
    {
        return
        [
            "unity.core.archive_loader",
            "unity.core.save_load",
            "unity.core.input_settings",
            "unity.core.asset_loader",
            "unity.ui.dynamic_layout",
            "unity.ui.data_binding",
            "unity.audio.short_sfx",
            "unity.world.topdown_map",
            "unity.gameplay.stats",
            "unity.gameplay.inventory",
            "unity.gameplay.dialogue",
            "unity.gameplay.quest_journal",
            "unity.gameplay.personal_combat",
            "unity.gameplay.crafting"
        ];
    }

    private static UnityRuntimeModuleContract Current(
        string id,
        string title,
        UnityRuntimePerformanceClass performanceClass = UnityRuntimePerformanceClass.Low)
    {
        return Module(id, title, UnityContractMaturity.Current, performanceClass, "contract_current_implementation_future");
    }

    private static UnityRuntimeModuleContract Future(
        string id,
        string title,
        UnityRuntimePerformanceClass performanceClass = UnityRuntimePerformanceClass.Low)
    {
        return Module(id, title, UnityContractMaturity.PlannedFuture, performanceClass, "planned_future_contract_only");
    }

    private static UnityRuntimeModuleContract Module(
        string id,
        string title,
        UnityContractMaturity maturity,
        UnityRuntimePerformanceClass performanceClass,
        string implementationStatus)
    {
        return new UnityRuntimeModuleContract
        {
            ModuleId = id,
            Title = title,
            Description = $"Archive-driven contract for {title.ToLowerInvariant()}.",
            Maturity = maturity,
            ProvidesCapabilities = [id],
            InputDataContracts = ["unity_game_archive_manifest_v1"],
            CanRunOffline = true,
            CanRunAtRuntime = true,
            PerformanceClass = performanceClass,
            ImplementationStatus = implementationStatus
        };
    }
}
