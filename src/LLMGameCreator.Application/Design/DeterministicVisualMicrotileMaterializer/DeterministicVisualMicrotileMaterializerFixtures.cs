namespace LLMGameCreator.Application.Design.DeterministicVisualMicrotileMaterializer;

public static class DeterministicVisualMicrotileMaterializerFixtures
{
    public static readonly IReadOnlyList<string> RequiredPreviewIds =
    [
        "terrain_grass_overworld",
        "terrain_snow_tundra",
        "terrain_desert_dry",
        "terrain_lava_ash",
        "terrain_forest_overlay",
        "terrain_mountain_rock",
        "water_base",
        "water_coast_transition",
        "water_river_segment",
        "water_lake_edge",
        "water_marsh_swamp",
        "water_bridge_dock_anchor",
        "settlement_small_dwelling",
        "settlement_wall_gate",
        "settlement_mine_production",
        "settlement_caravan_camp",
        "creature_bodyplan_silhouette",
        "creature_equipment_clothing_overlay",
        "creature_damaged_dirty_worn_state",
        "creature_paperdoll_neutral_slot",
        "ui_frame_panel_motif",
        "effect_status_aura",
        "atmosphere_day_night_weather_overlay",
        "adult_metadata_only_safe_fallback_slot"
    ];

    public static VisualMicrotileMaterializationRequest BuildDefaultRequest() =>
        new()
        {
            RequestId = "visual_microtile_materializer_goal086",
            GeneratorVersion = "goal086-microtile-materializer-v1",
            OutputRelativeDirectory = DeterministicVisualMicrotileMaterializerVocabulary.RelativeOutputDirectory,
            SourceOfTruthKind = "metadata_contract",
            PromptTextIsSourceOfTruth = false,
            SourceGoal084And085LineageRequired = true,
            Previews =
            [
                Preview(
                    "terrain_grass_overworld",
                    VisualMicrotileCategory.TerrainBiome,
                    "fantasy_overworld_tile_part_pack",
                    "fantasy_overworld_tile_safe",
                    "palette/fantasy_overworld/readable_v1",
                    860101,
                    Palette("#274b2d", "#5f9f47", "#9bcf6f", "#1f3428"),
                    biomeRuleId: "biome/fantasy_overworld/grass_overworld"),
                Preview(
                    "terrain_snow_tundra",
                    VisualMicrotileCategory.TerrainBiome,
                    "fantasy_overworld_tile_part_pack",
                    "fantasy_overworld_tile_safe",
                    "palette/fantasy_overworld/readable_v1",
                    860102,
                    Palette("#dfeef2", "#a6c6d8", "#f8fbff", "#6e8790"),
                    biomeRuleId: "biome/fantasy_overworld/snow"),
                Preview(
                    "terrain_desert_dry",
                    VisualMicrotileCategory.TerrainBiome,
                    "fantasy_overworld_tile_part_pack",
                    "fantasy_overworld_tile_safe",
                    "palette/fantasy_overworld/readable_v1",
                    860103,
                    Palette("#a16d35", "#d7aa62", "#f0d08a", "#704522"),
                    biomeRuleId: "biome/fantasy_overworld/desert_dry"),
                Preview(
                    "terrain_lava_ash",
                    VisualMicrotileCategory.TerrainBiome,
                    "fantasy_overworld_tile_part_pack",
                    "fantasy_overworld_tile_safe",
                    "palette/fantasy_overworld/readable_v1",
                    860104,
                    Palette("#252324", "#6b3530", "#e25b2d", "#f4b55c"),
                    biomeRuleId: "biome/fantasy_overworld/lava_ash"),
                Preview(
                    "terrain_forest_overlay",
                    VisualMicrotileCategory.TerrainBiome,
                    "fantasy_overworld_tile_part_pack",
                    "fantasy_overworld_tile_safe",
                    "palette/fantasy_overworld/readable_v1",
                    860105,
                    Palette("#1f3a2b", "#346d3d", "#6a9f58", "#15251d"),
                    biomeRuleId: "biome/fantasy_overworld/forest_decor_overlay"),
                Preview(
                    "terrain_mountain_rock",
                    VisualMicrotileCategory.TerrainBiome,
                    "fantasy_overworld_tile_part_pack",
                    "fantasy_overworld_tile_safe",
                    "palette/fantasy_overworld/readable_v1",
                    860106,
                    Palette("#3a3d41", "#747a7c", "#b6b2a5", "#232629"),
                    biomeRuleId: "biome/fantasy_overworld/mountain_rock_overlay"),
                Preview(
                    "water_base",
                    VisualMicrotileCategory.Water,
                    "water_coast_river_marsh_part_pack",
                    "water_coast_biome_safe",
                    "palette/water_biome/readable_v1",
                    860201,
                    Palette("#123e62", "#216c91", "#65b9c6", "#e2f2ed"),
                    waterRuleId: "water_profile/base_water",
                    adjacency: Water(["north", "south", "east", "west"], [])),
                Preview(
                    "water_coast_transition",
                    VisualMicrotileCategory.Water,
                    "water_coast_river_marsh_part_pack",
                    "water_coast_biome_safe",
                    "palette/water_biome/readable_v1",
                    860202,
                    Palette("#1a5f81", "#d4b56a", "#71c3d0", "#7a5734"),
                    waterRuleId: "water_profile/coast_transition",
                    adjacency: Water(["north", "west"], ["south", "east"])),
                Preview(
                    "water_river_segment",
                    VisualMicrotileCategory.Water,
                    "water_coast_river_marsh_part_pack",
                    "water_coast_biome_safe",
                    "palette/water_biome/readable_v1",
                    860203,
                    Palette("#195478", "#3aa2ba", "#c7ede9", "#24533b"),
                    waterRuleId: "water_profile/river_segment",
                    adjacency: Water(["north", "south"], ["east", "west"]),
                    flowConnectors: ["north", "south"]),
                Preview(
                    "water_lake_edge",
                    VisualMicrotileCategory.Water,
                    "water_coast_river_marsh_part_pack",
                    "water_coast_biome_safe",
                    "palette/water_biome/readable_v1",
                    860204,
                    Palette("#174665", "#438ba0", "#c8ded3", "#355837"),
                    waterRuleId: "water_profile/lake_edge",
                    adjacency: Water(["north", "east"], ["south", "west"])),
                Preview(
                    "water_marsh_swamp",
                    VisualMicrotileCategory.Water,
                    "water_coast_river_marsh_part_pack",
                    "water_coast_biome_safe",
                    "palette/water_biome/readable_v1",
                    860205,
                    Palette("#263f36", "#3f6f52", "#78a96a", "#1f2c28"),
                    waterRuleId: "water_profile/marsh_swamp",
                    adjacency: Water(["north", "south", "west"], ["east"])),
                Preview(
                    "water_bridge_dock_anchor",
                    VisualMicrotileCategory.Water,
                    "water_coast_river_marsh_part_pack",
                    "water_coast_biome_safe",
                    "palette/water_biome/readable_v1",
                    860206,
                    Palette("#144964", "#2e7890", "#8a613c", "#d3b579"),
                    waterRuleId: "placement/water/bridge_dock_anchor",
                    adjacency: Water(["north", "south"], ["east", "west"]),
                    flowConnectors: ["north", "south"]),
                Preview(
                    "settlement_small_dwelling",
                    VisualMicrotileCategory.SettlementStructure,
                    "settlement_building_facade_part_pack",
                    "settlement_building_safe",
                    "palette/settlement_facade/readable_v1",
                    860301,
                    Palette("#6d4b32", "#b58456", "#d8c2a0", "#3d2f27"),
                    biomeRuleId: "placement/settlement/small_dwelling"),
                Preview(
                    "settlement_wall_gate",
                    VisualMicrotileCategory.SettlementStructure,
                    "settlement_building_facade_part_pack",
                    "settlement_building_safe",
                    "palette/settlement_facade/readable_v1",
                    860302,
                    Palette("#444b50", "#858b8c", "#c8c4b8", "#262b30"),
                    biomeRuleId: "placement/settlement/wall_gate"),
                Preview(
                    "settlement_mine_production",
                    VisualMicrotileCategory.SettlementStructure,
                    "settlement_building_facade_part_pack",
                    "settlement_building_safe",
                    "palette/settlement_facade/readable_v1",
                    860303,
                    Palette("#38363a", "#7b6449", "#c49a55", "#1f1f22"),
                    biomeRuleId: "placement/settlement/mine_production"),
                Preview(
                    "settlement_caravan_camp",
                    VisualMicrotileCategory.SettlementStructure,
                    "settlement_building_facade_part_pack",
                    "settlement_building_safe",
                    "palette/settlement_facade/readable_v1",
                    860304,
                    Palette("#5d4532", "#a07448", "#d7b173", "#314a3a"),
                    biomeRuleId: "placement/settlement/caravan_camp"),
                Preview(
                    "creature_bodyplan_silhouette",
                    VisualMicrotileCategory.CreatureNpc,
                    "creature_bodyplan_equipment_part_pack",
                    "creature_bodyplan_safe",
                    "palette/creature_bodyplan/readable_v1",
                    860401,
                    Palette("#2e3038", "#6f7685", "#aeb8c2", "#16191f")),
                Preview(
                    "creature_equipment_clothing_overlay",
                    VisualMicrotileCategory.CreatureNpc,
                    "creature_bodyplan_equipment_part_pack",
                    "creature_bodyplan_safe",
                    "palette/creature_bodyplan/readable_v1",
                    860402,
                    Palette("#3b2f2b", "#7b4f39", "#b98652", "#d8c096")),
                Preview(
                    "creature_damaged_dirty_worn_state",
                    VisualMicrotileCategory.CreatureNpc,
                    "creature_bodyplan_equipment_part_pack",
                    "creature_bodyplan_safe",
                    "palette/creature_bodyplan/readable_v1",
                    860403,
                    Palette("#262a2e", "#695846", "#9c7f60", "#c9b18a")),
                Preview(
                    "creature_paperdoll_neutral_slot",
                    VisualMicrotileCategory.CreatureNpc,
                    "creature_bodyplan_equipment_part_pack",
                    "creature_bodyplan_safe",
                    "palette/creature_bodyplan/readable_v1",
                    860404,
                    Palette("#33414d", "#66798a", "#b7c2cd", "#202933")),
                Preview(
                    "ui_frame_panel_motif",
                    VisualMicrotileCategory.UiEffect,
                    "ui_theme_icon_effect_part_pack",
                    "tech_future_ui_panel_safe",
                    "palette/ui_theme_effect/readable_v1",
                    860501,
                    Palette("#17242d", "#2e5968", "#81b4bd", "#d8edf0")),
                Preview(
                    "effect_status_aura",
                    VisualMicrotileCategory.UiEffect,
                    "ui_theme_icon_effect_part_pack",
                    "tech_future_ui_panel_safe",
                    "palette/ui_theme_effect/readable_v1",
                    860502,
                    Palette("#231f3a", "#5750a8", "#96a7ff", "#d9e0ff")),
                Preview(
                    "atmosphere_day_night_weather_overlay",
                    VisualMicrotileCategory.UiEffect,
                    "ui_theme_icon_effect_part_pack",
                    "tech_future_ui_panel_safe",
                    "palette/ui_theme_effect/readable_v1",
                    860503,
                    Palette("#111927", "#355275", "#c2d1df", "#f2c66d")),
                Preview(
                    "adult_metadata_only_safe_fallback_slot",
                    VisualMicrotileCategory.AdultRating,
                    "adult_rating_gated_extension_metadata_only",
                    "humanoid_paperdoll_adult_capable_metadata_only",
                    "palette/adult_rating_metadata/readable_v1",
                    860601,
                    Palette("#243136", "#60787f", "#c6d6d9", "#f1f5f5"),
                    adultMetadataOnly: true,
                    safeFallbackPreviewId: "creature_paperdoll_neutral_slot")
            ]
        };

    private static VisualMicrotilePreviewSpec Preview(
        string previewId,
        VisualMicrotileCategory category,
        string partPackId,
        string assetSlotId,
        string paletteProfileId,
        int seed,
        IReadOnlyList<VisualMicrotilePaletteSwatch> palette,
        string biomeRuleId = "",
        string waterRuleId = "",
        VisualMicrotileWaterAdjacency? adjacency = null,
        IReadOnlyList<string>? flowConnectors = null,
        bool adultMetadataOnly = false,
        string safeFallbackPreviewId = "") =>
        new()
        {
            PreviewId = previewId,
            Category = category,
            PartPackId = partPackId,
            AssetSlotId = assetSlotId,
            PaletteProfileId = paletteProfileId,
            Seed = seed,
            PreviewRelativePath = $"{DeterministicVisualMicrotileMaterializerVocabulary.PreviewRelativeDirectory}/{previewId}.svg",
            LayerStack = LayersFor(category),
            Palette = palette,
            MaskIds = MasksFor(category),
            SocketIds = SocketsFor(category),
            AnchorIds = AnchorsFor(category),
            SourceGoal084SlotId = assetSlotId,
            SourceGoal085PackId = partPackId,
            BiomeRuleId = biomeRuleId,
            WaterRuleId = waterRuleId,
            WaterLandAdjacency = adjacency,
            FlowConnectors = flowConnectors ?? [],
            AdultMetadataOnly = adultMetadataOnly,
            SafeFallbackPreviewId = safeFallbackPreviewId,
            ProviderState = adultMetadataOnly
                ? VisualMicrotileProviderState.CandidateQuarantine
                : VisualMicrotileProviderState.MetadataOnly,
            TreatProviderCandidateAsApprovedOutput = false
        };

    private static IReadOnlyList<VisualMicrotileLayerSpec> LayersFor(VisualMicrotileCategory category) =>
        category switch
        {
            VisualMicrotileCategory.SettlementStructure =>
            [
                Layer("layer/base", 0, "facade_mass"),
                Layer("layer/detail", 10, "door_window_trim"),
                Layer("layer/overlay", 20, "district_marker")
            ],
            VisualMicrotileCategory.CreatureNpc or VisualMicrotileCategory.AdultRating =>
            [
                Layer("layer/base", 0, "silhouette"),
                Layer("layer/detail", 10, "surface_or_clothing"),
                Layer("layer/overlay", 20, "equipment_or_state")
            ],
            VisualMicrotileCategory.UiEffect =>
            [
                Layer("layer/base", 0, "frame_or_aura_base"),
                Layer("layer/detail", 10, "icon_or_status_detail"),
                Layer("layer/overlay", 20, "weather_or_effect_overlay")
            ],
            _ =>
            [
                Layer("layer/base", 0, "base_surface"),
                Layer("layer/detail", 10, "detail_pattern"),
                Layer("layer/overlay", 20, "transition_or_decor_overlay")
            ]
        };

    private static IReadOnlyList<string> MasksFor(VisualMicrotileCategory category) =>
        category switch
        {
            VisualMicrotileCategory.SettlementStructure => ["mask/facade/base", "mask/facade/openings", "mask/facade/detail"],
            VisualMicrotileCategory.CreatureNpc or VisualMicrotileCategory.AdultRating => ["mask/creature/body", "mask/creature/clothing", "mask/creature/equipment"],
            VisualMicrotileCategory.UiEffect => ["mask/ui/frame", "mask/ui/icon", "mask/ui/effect"],
            _ => ["mask/tile/base", "mask/tile/edge", "mask/tile/detail"]
        };

    private static IReadOnlyList<string> SocketsFor(VisualMicrotileCategory category) =>
        category switch
        {
            VisualMicrotileCategory.SettlementStructure => ["socket/facade/base", "socket/facade/opening", "socket/facade/roof"],
            VisualMicrotileCategory.CreatureNpc or VisualMicrotileCategory.AdultRating => ["socket/creature/head", "socket/creature/torso", "socket/creature/hand_main"],
            VisualMicrotileCategory.UiEffect => ["socket/ui/frame", "socket/ui/icon", "socket/ui/effect"],
            _ => ["socket/tile/center", "socket/tile/edge", "socket/tile/object"]
        };

    private static IReadOnlyList<string> AnchorsFor(VisualMicrotileCategory category) =>
        category switch
        {
            VisualMicrotileCategory.SettlementStructure => ["anchor/facade/bottom", "anchor/facade/door"],
            VisualMicrotileCategory.CreatureNpc or VisualMicrotileCategory.AdultRating => ["anchor/creature/head", "anchor/creature/torso", "anchor/creature/feet"],
            VisualMicrotileCategory.UiEffect => ["anchor/ui/center", "anchor/ui/status"],
            _ => ["anchor/tile/center", "anchor/tile/edge"]
        };

    private static IReadOnlyList<VisualMicrotilePaletteSwatch> Palette(
        string background,
        string primary,
        string secondary,
        string accent) =>
    [
        new VisualMicrotilePaletteSwatch { SlotId = "background", HexColor = background },
        new VisualMicrotilePaletteSwatch { SlotId = "primary", HexColor = primary },
        new VisualMicrotilePaletteSwatch { SlotId = "secondary", HexColor = secondary },
        new VisualMicrotilePaletteSwatch { SlotId = "accent", HexColor = accent }
    ];

    private static VisualMicrotileWaterAdjacency Water(
        IReadOnlyList<string> waterEdges,
        IReadOnlyList<string> landEdges) =>
        new() { WaterEdges = waterEdges, LandEdges = landEdges };

    private static VisualMicrotileLayerSpec Layer(string id, int order, string role) =>
        new() { LayerId = id, Order = order, Role = role };
}
