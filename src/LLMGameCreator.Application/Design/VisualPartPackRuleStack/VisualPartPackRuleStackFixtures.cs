namespace LLMGameCreator.Application.Design.VisualPartPackRuleStack;

public static class VisualPartPackRuleStackFixtures
{
    public static readonly IReadOnlyList<string> RequiredFixturePackIds =
    [
        "fantasy_overworld_tile_part_pack",
        "water_coast_river_marsh_part_pack",
        "settlement_building_facade_part_pack",
        "creature_bodyplan_equipment_part_pack",
        "ui_theme_icon_effect_part_pack",
        "adult_rating_gated_extension_metadata_only"
    ];

    public static readonly IReadOnlyDictionary<string, string> Goal084SlotBindings =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["fantasy_overworld_tile_part_pack"] = "fantasy_overworld_tile_safe",
            ["water_coast_river_marsh_part_pack"] = "water_coast_biome_safe",
            ["settlement_building_facade_part_pack"] = "settlement_building_safe",
            ["creature_bodyplan_equipment_part_pack"] = "creature_bodyplan_safe",
            ["ui_theme_icon_effect_part_pack"] = "tech_future_ui_panel_safe",
            ["adult_rating_gated_extension_metadata_only"] = "humanoid_paperdoll_adult_capable_metadata_only"
        };

    public static VisualPartPackManifest BuildDefaultManifest()
    {
        var packs = new List<VisualPartPackDefinition>
        {
            FantasyOverworldTilePack(),
            WaterCoastRiverMarshPack(),
            SettlementBuildingFacadePack(),
            CreatureBodyplanEquipmentPack(),
            UiThemeIconEffectPack(),
            AdultRatingGatedExtensionPack()
        };

        return new VisualPartPackManifest
        {
            ManifestId = "visual_part_pack_rule_stack_goal085",
            GeneratorVersion = "goal085-rule-stack-v1",
            StrictReferenceValidation = true,
            SourceOfTruthKind = "metadata_contract",
            PromptTextIsSourceOfTruth = false,
            PartPacks = packs,
            Recipes = packs.Select(RecipeForPack).ToList()
        };
    }

    private static VisualPartPackDefinition FantasyOverworldTilePack()
    {
        var palette = Palette("palette/fantasy_overworld/readable_v1", ["grass", "dirt", "snow", "lava", "rough", "forest", "edge_shadow"]);
        var masks = TileMasks();
        var sockets = TileSockets();
        var anchors = TileAnchors();
        return new VisualPartPackDefinition
        {
            PackId = "fantasy_overworld_tile_part_pack",
            Kind = VisualPartPackKind.TileTerrain,
            Rating = VisualContentRating.Safe,
            ExportPolicy = VisualPartExportPolicy.PublicSafe,
            SafeFallbackPackId = "fantasy_overworld_tile_part_pack",
            MetadataRelativePath = "visual/part-packs/fantasy_overworld_tile_part_pack/manifest.metadata.json",
            Sha256 = StableHash("pack/fantasy_overworld_tile_part_pack"),
            ProvenanceRef = "docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md",
            FeatureTags = ["tile", "terrain", "grass", "dirt", "snow", "lava", "rough", "forest"],
            Layers = TileLayers(),
            Masks = masks,
            Sockets = sockets,
            Anchors = anchors,
            PaletteProfiles = [palette],
            Parts =
            [
                Part("fantasy_overworld_tile_part_pack", "grass_base", "terrain_base", palette.PaletteProfileId, masks, sockets, anchors, ["grass"]),
                Part("fantasy_overworld_tile_part_pack", "dirt_transition", "terrain_transition", palette.PaletteProfileId, masks, sockets, anchors, ["dirt"]),
                Part("fantasy_overworld_tile_part_pack", "snow_cap", "weather_overlay", palette.PaletteProfileId, masks, sockets, anchors, ["snow"]),
                Part("fantasy_overworld_tile_part_pack", "lava_edge", "hazard_transition", palette.PaletteProfileId, masks, sockets, anchors, ["lava"]),
                Part("fantasy_overworld_tile_part_pack", "rough_rock", "rough_ground", palette.PaletteProfileId, masks, sockets, anchors, ["rough"]),
                Part("fantasy_overworld_tile_part_pack", "forest_canopy", "forest_overlay", palette.PaletteProfileId, masks, sockets, anchors, ["forest"])
            ],
            BiomeProfiles =
            [
                new VisualBiomeProfile
                {
                    BiomeProfileId = "biome/fantasy_overworld/core",
                    BiomeKinds = ["grass", "dirt", "snow", "lava", "rough", "forest"]
                }
            ],
            TerrainTransitionRules =
            [
                Transition("transition/grass_dirt", "grass", "dirt"),
                Transition("transition/grass_snow", "grass", "snow"),
                Transition("transition/rough_lava", "rough", "lava"),
                Transition("transition/forest_grass", "forest", "grass")
            ],
            AutoTileRules =
            [
                new VisualAutoTileRule
                {
                    RuleId = "autotile/fantasy_overworld/terrain_edges",
                    TerrainKinds = ["grass", "dirt", "snow", "lava", "rough", "forest"],
                    EdgeMaskId = "mask/tile/edge"
                }
            ],
            PaletteSwapRules = [PaletteSwap("palette_swap/fantasy_overworld/seasonal", palette.PaletteProfileId, ["grass", "snow", "forest"])],
            OverlayRules = [Overlay("overlay/fantasy_overworld/damage_dirt", "damage_dirt", ["layer/detail", "layer/overlay"])]
        };
    }

    private static VisualPartPackDefinition WaterCoastRiverMarshPack()
    {
        var palette = Palette("palette/water_biome/readable_v1", ["sea", "lake", "river", "coast", "marsh", "foam", "wood"]);
        var masks = TileMasks();
        var sockets = TileSockets();
        var anchors = TileAnchors();
        return new VisualPartPackDefinition
        {
            PackId = "water_coast_river_marsh_part_pack",
            Kind = VisualPartPackKind.WaterBiome,
            Rating = VisualContentRating.Safe,
            ExportPolicy = VisualPartExportPolicy.PublicSafe,
            SafeFallbackPackId = "water_coast_river_marsh_part_pack",
            MetadataRelativePath = "visual/part-packs/water_coast_river_marsh_part_pack/manifest.metadata.json",
            Sha256 = StableHash("pack/water_coast_river_marsh_part_pack"),
            ProvenanceRef = "docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md",
            FeatureTags = ["water", "coast", "river", "lake", "marsh", "bridge", "dock", "water_object"],
            Layers = TileLayers(),
            Masks = masks,
            Sockets = sockets,
            Anchors = anchors,
            PaletteProfiles = [palette],
            Parts =
            [
                Part("water_coast_river_marsh_part_pack", "sea_surface", "water_surface", palette.PaletteProfileId, masks, sockets, anchors, ["sea"]),
                Part("water_coast_river_marsh_part_pack", "lake_surface", "water_surface", palette.PaletteProfileId, masks, sockets, anchors, ["lake"]),
                Part("water_coast_river_marsh_part_pack", "river_channel", "river", palette.PaletteProfileId, masks, sockets, anchors, ["river"]),
                Part("water_coast_river_marsh_part_pack", "coast_edge", "coast_transition", palette.PaletteProfileId, masks, sockets, anchors, ["coast"]),
                Part("water_coast_river_marsh_part_pack", "marsh_patch", "marsh_transition", palette.PaletteProfileId, masks, sockets, anchors, ["marsh"]),
                Part("water_coast_river_marsh_part_pack", "bridge_dock_marker", "water_object", palette.PaletteProfileId, masks, sockets, anchors, ["bridge", "dock"])
            ],
            WaterProfiles =
            [
                new VisualWaterProfile
                {
                    WaterProfileId = "water_profile/sea_lake_river_coast_marsh",
                    WaterKinds = ["sea", "lake", "river", "coast", "marsh"],
                    CoastAware = true,
                    RiverAware = true,
                    LakeAware = true,
                    MarshAware = true
                }
            ],
            ObjectPlacementRules =
            [
                Placement("placement/water/bridge", "bridge", ["river", "coast"]),
                Placement("placement/water/dock", "dock", ["lake", "coast"]),
                Placement("placement/water/water_object", "water_object", ["sea", "lake", "river"])
            ],
            TerrainTransitionRules = [Transition("transition/water_coast", "water", "coast")],
            AutoTileRules =
            [
                new VisualAutoTileRule
                {
                    RuleId = "autotile/water/coast_river_marsh_edges",
                    TerrainKinds = ["sea", "lake", "river", "coast", "marsh"],
                    EdgeMaskId = "mask/tile/edge"
                }
            ]
        };
    }

    private static VisualPartPackDefinition SettlementBuildingFacadePack()
    {
        var palette = Palette("palette/settlement_facade/readable_v1", ["stone", "wood", "cloth", "roof", "gate", "market", "field"]);
        var masks = FacadeMasks();
        var sockets = FacadeSockets();
        var anchors = FacadeAnchors();
        return new VisualPartPackDefinition
        {
            PackId = "settlement_building_facade_part_pack",
            Kind = VisualPartPackKind.SettlementFacade,
            Rating = VisualContentRating.Safe,
            ExportPolicy = VisualPartExportPolicy.PublicSafe,
            SafeFallbackPackId = "settlement_building_facade_part_pack",
            MetadataRelativePath = "visual/part-packs/settlement_building_facade_part_pack/manifest.metadata.json",
            Sha256 = StableHash("pack/settlement_building_facade_part_pack"),
            ProvenanceRef = "docs/deepsearch/05_SETTLEMENTS_CITIES_CARAVANS_LIVING_WORLD_VISUALS.md",
            FeatureTags = ["house", "castle", "wall", "gate", "market", "farm", "mine", "district"],
            Layers = FacadeLayers(),
            Masks = masks,
            Sockets = sockets,
            Anchors = anchors,
            PaletteProfiles = [palette],
            Parts =
            [
                Part("settlement_building_facade_part_pack", "house_front", "house_facade", palette.PaletteProfileId, masks, sockets, anchors, ["house"]),
                Part("settlement_building_facade_part_pack", "castle_tower", "castle_facade", palette.PaletteProfileId, masks, sockets, anchors, ["castle"]),
                Part("settlement_building_facade_part_pack", "wall_segment", "wall", palette.PaletteProfileId, masks, sockets, anchors, ["wall"]),
                Part("settlement_building_facade_part_pack", "gate_house", "gate", palette.PaletteProfileId, masks, sockets, anchors, ["gate"]),
                Part("settlement_building_facade_part_pack", "market_awning", "market", palette.PaletteProfileId, masks, sockets, anchors, ["market"]),
                Part("settlement_building_facade_part_pack", "farm_mine_marker", "production_site", palette.PaletteProfileId, masks, sockets, anchors, ["farm", "mine", "district"])
            ],
            ObjectPlacementRules =
            [
                Placement("placement/settlement/house", "house", ["district", "road"]),
                Placement("placement/settlement/castle_wall_gate", "castle_wall_gate", ["defense", "district"]),
                Placement("placement/settlement/market_farm_mine", "market_farm_mine", ["economy", "district"])
            ]
        };
    }

    private static VisualPartPackDefinition CreatureBodyplanEquipmentPack()
    {
        var palette = Palette("palette/creature_bodyplan/readable_v1", ["skin", "fur", "scale", "chitin", "cloth", "metal", "bone", "glow"]);
        var masks = CreatureMasks();
        var sockets = CreatureSockets();
        var anchors = CreatureAnchors();
        return new VisualPartPackDefinition
        {
            PackId = "creature_bodyplan_equipment_part_pack",
            Kind = VisualPartPackKind.CreatureBodyPlanEquipment,
            Rating = VisualContentRating.Safe,
            ExportPolicy = VisualPartExportPolicy.PublicSafe,
            SafeFallbackPackId = "creature_bodyplan_equipment_part_pack",
            MetadataRelativePath = "visual/part-packs/creature_bodyplan_equipment_part_pack/manifest.metadata.json",
            Sha256 = StableHash("pack/creature_bodyplan_equipment_part_pack"),
            ProvenanceRef = "docs/deepsearch/04_CREATURE_NPC_APPEARANCE_BODYPLAN_PAPERDOLL.md",
            FeatureTags = ["humanoid", "beast", "reptilian", "insectoid", "undead", "mechanical", "equipment", "clothing", "state_overlay"],
            Layers = CreatureLayers(),
            Masks = masks,
            Sockets = sockets,
            Anchors = anchors,
            PaletteProfiles = [palette],
            BodyPlanGrammarCapacity = 128,
            HandAuthoredSpeciesAssetCount = 0,
            Parts =
            [
                Part("creature_bodyplan_equipment_part_pack", "humanoid_silhouette", "body_plan", palette.PaletteProfileId, masks, sockets, anchors, ["humanoid"]),
                Part("creature_bodyplan_equipment_part_pack", "beast_silhouette", "body_plan", palette.PaletteProfileId, masks, sockets, anchors, ["beast"]),
                Part("creature_bodyplan_equipment_part_pack", "reptilian_scale_surface", "surface_plan", palette.PaletteProfileId, masks, sockets, anchors, ["reptilian"]),
                Part("creature_bodyplan_equipment_part_pack", "insectoid_chitin_surface", "surface_plan", palette.PaletteProfileId, masks, sockets, anchors, ["insectoid"]),
                Part("creature_bodyplan_equipment_part_pack", "undead_state_overlay", "state_overlay", palette.PaletteProfileId, masks, sockets, anchors, ["undead"]),
                Part("creature_bodyplan_equipment_part_pack", "mechanical_plate_overlay", "equipment_overlay", palette.PaletteProfileId, masks, sockets, anchors, ["mechanical"])
            ],
            CreatureBodyPlanProfiles =
            [
                BodyPlan("bodyplan/humanoid", "humanoid", adultEligible: true),
                BodyPlan("bodyplan/beast_safe", "beast", adultEligible: false),
                BodyPlan("bodyplan/reptilian", "reptilian", adultEligible: false),
                BodyPlan("bodyplan/insectoid", "insectoid", adultEligible: false),
                BodyPlan("bodyplan/undead", "undead", adultEligible: false),
                BodyPlan("bodyplan/mechanical", "mechanical", adultEligible: false)
            ],
            EquipmentOverlayProfiles =
            [
                Equipment("equipment_overlay/headgear", "headgear", ["socket/creature/head"], ["bodyplan/humanoid", "bodyplan/reptilian"]),
                Equipment("equipment_overlay/torso_clothing", "torso_clothing", ["socket/creature/torso"], ["bodyplan/humanoid"]),
                Equipment("equipment_overlay/weapon_main", "weapon", ["socket/creature/hand_main"], ["bodyplan/humanoid", "bodyplan/mechanical"]),
                Equipment("equipment_overlay/back_item", "back_item", ["socket/creature/back"], ["bodyplan/humanoid", "bodyplan/beast_safe"])
            ]
        };
    }

    private static VisualPartPackDefinition UiThemeIconEffectPack()
    {
        var palette = Palette("palette/ui_theme_effect/readable_v1", ["panel", "button", "icon", "status", "weather", "day", "night", "effect"]);
        var masks = UiMasks();
        var sockets = UiSockets();
        var anchors = UiAnchors();
        return new VisualPartPackDefinition
        {
            PackId = "ui_theme_icon_effect_part_pack",
            Kind = VisualPartPackKind.UiThemeEffect,
            Rating = VisualContentRating.Safe,
            ExportPolicy = VisualPartExportPolicy.PublicSafe,
            SafeFallbackPackId = "ui_theme_icon_effect_part_pack",
            MetadataRelativePath = "visual/part-packs/ui_theme_icon_effect_part_pack/manifest.metadata.json",
            Sha256 = StableHash("pack/ui_theme_icon_effect_part_pack"),
            ProvenanceRef = "docs/deepsearch/06_UI_THEMES_EFFECTS_WEATHER_DAYNIGHT_VFX.md",
            FeatureTags = ["panel", "button", "icon", "status", "weather", "day_night", "effect_overlay"],
            Layers = UiLayers(),
            Masks = masks,
            Sockets = sockets,
            Anchors = anchors,
            PaletteProfiles = [palette],
            Parts =
            [
                Part("ui_theme_icon_effect_part_pack", "panel_frame", "ui_panel", palette.PaletteProfileId, masks, sockets, anchors, ["panel"]),
                Part("ui_theme_icon_effect_part_pack", "button_frame", "ui_button", palette.PaletteProfileId, masks, sockets, anchors, ["button"]),
                Part("ui_theme_icon_effect_part_pack", "resource_icon", "icon", palette.PaletteProfileId, masks, sockets, anchors, ["icon"]),
                Part("ui_theme_icon_effect_part_pack", "status_badge", "status", palette.PaletteProfileId, masks, sockets, anchors, ["status"]),
                Part("ui_theme_icon_effect_part_pack", "weather_overlay", "weather", palette.PaletteProfileId, masks, sockets, anchors, ["weather"]),
                Part("ui_theme_icon_effect_part_pack", "day_night_effect", "day_night_effect", palette.PaletteProfileId, masks, sockets, anchors, ["day_night", "effect_overlay"])
            ],
            UiThemeProfiles =
            [
                new VisualUiThemeProfile
                {
                    UiThemeProfileId = "ui_theme/tech_future_safe",
                    UiElementKinds = ["panel", "button", "icon", "status"],
                    SafeFallbackThemeId = "ui_theme/generic_safe"
                }
            ],
            EffectProfiles =
            [
                new VisualEffectProfile { EffectProfileId = "effect/status_safe", EffectKind = "status", HasSafeFallback = true },
                new VisualEffectProfile { EffectProfileId = "effect/weather_safe", EffectKind = "weather", HasSafeFallback = true },
                new VisualEffectProfile { EffectProfileId = "effect/day_night_safe", EffectKind = "day_night", HasSafeFallback = true },
                new VisualEffectProfile { EffectProfileId = "effect/vfx_overlay_safe", EffectKind = "effect_overlay", HasSafeFallback = true }
            ]
        };
    }

    private static VisualPartPackDefinition AdultRatingGatedExtensionPack()
    {
        var palette = Palette("palette/adult_rating_metadata/readable_v1", ["safe_fallback", "policy_marker", "review_state"]);
        var masks = CreatureMasks();
        var sockets = CreatureSockets();
        var anchors = CreatureAnchors();
        return new VisualPartPackDefinition
        {
            PackId = "adult_rating_gated_extension_metadata_only",
            Kind = VisualPartPackKind.AdultRatingExtension,
            Rating = VisualContentRating.SuggestiveMetadata,
            ExportPolicy = VisualPartExportPolicy.MatureOptional,
            ReviewStatus = VisualPartReviewStatus.CandidateQuarantined,
            ProviderState = VisualPartProviderState.CandidateQuarantine,
            IsAdultRatingExtension = true,
            SafeFallbackPackId = "creature_bodyplan_equipment_part_pack",
            MetadataRelativePath = "visual/part-packs/adult_rating_gated_extension_metadata_only/manifest.metadata.json",
            Sha256 = StableHash("pack/adult_rating_gated_extension_metadata_only"),
            ProvenanceRef = "docs/deepsearch/07_MEDIA_PIPELINE_PROVIDER_QUARANTINE_PROVENANCE_RATING_ADULT.md",
            FeatureTags = ["rating_metadata", "adult_policy", "safe_fallback", "candidate_quarantine", "metadata_only"],
            Layers = CreatureLayers(),
            Masks = masks,
            Sockets = sockets,
            Anchors = anchors,
            PaletteProfiles = [palette],
            Parts =
            [
                Part("adult_rating_gated_extension_metadata_only", "neutral_policy_marker", "rating_policy_metadata", palette.PaletteProfileId, masks, sockets, anchors, ["metadata_only"]),
                Part("adult_rating_gated_extension_metadata_only", "safe_fallback_binding", "safe_fallback_metadata", palette.PaletteProfileId, masks, sockets, anchors, ["safe_fallback"])
            ],
            CreatureBodyPlanProfiles =
            [
                new VisualCreatureBodyPlanProfile
                {
                    BodyPlanProfileId = "bodyplan/adult_eligible_humanoid_metadata",
                    BodyPlanKind = "humanoid_metadata_only",
                    AdultEligible = true,
                    AgeKnownAdult = true,
                    Sapient = true,
                    HumanoidCompatible = true,
                    CompatibleSocketIds = ["socket/creature/head", "socket/creature/torso"]
                }
            ],
            OverlayRules = [Overlay("overlay/adult_metadata/safe_fallback_required", "metadata_only_safe_fallback", ["layer/body", "layer/clothing"])]
        };
    }

    private static VisualPartPackRecipe RecipeForPack(VisualPartPackDefinition pack)
    {
        var goal084Slot = Goal084SlotBindings[pack.PackId];
        var recipeId = $"recipe/{pack.PackId}/v1";
        var dependencies = pack.PackId == "adult_rating_gated_extension_metadata_only"
            ? new[] { "recipe/creature_bodyplan_equipment_part_pack/v1" }
            : [];

        return new VisualPartPackRecipe
        {
            RecipeId = recipeId,
            PackId = pack.PackId,
            PaletteProfileId = pack.PaletteProfiles[0].PaletteProfileId,
            PartIds = pack.Parts.Select(item => item.PartId).ToList(),
            DependsOnRecipeIds = dependencies,
            SafeFallbackRecipeId = pack.PackId == "adult_rating_gated_extension_metadata_only"
                ? "recipe/creature_bodyplan_equipment_part_pack/v1"
                : recipeId,
            Goal084SlotId = goal084Slot
        };
    }

    private static VisualPartDefinition Part(
        string packId,
        string suffix,
        string role,
        string paletteId,
        IReadOnlyList<VisualMaskDefinition> masks,
        IReadOnlyList<VisualSocketDefinition> sockets,
        IReadOnlyList<VisualAnchorDefinition> anchors,
        IReadOnlyList<string> tags) =>
        new()
        {
            PartId = $"part/{packId}/{suffix}",
            Role = role,
            RelativePath = $"visual/part-packs/{packId}/parts/{suffix}.metadata.json",
            RequiresLayeredComposition = true,
            PaletteProfileId = paletteId,
            LayerIds = ["layer/base", "layer/detail", "layer/overlay"],
            MaskIds = masks.Take(2).Select(item => item.MaskId).ToList(),
            SocketIds = sockets.Take(2).Select(item => item.SocketId).ToList(),
            AnchorIds = anchors.Take(2).Select(item => item.AnchorId).ToList(),
            CompatibleTags = tags
        };

    private static VisualPaletteProfile Palette(string id, IReadOnlyList<string> slots) =>
        new() { PaletteProfileId = id, ColorSlots = slots };

    private static VisualPaletteSwapRule PaletteSwap(string id, string paletteId, IReadOnlyList<string> tags) =>
        new() { RuleId = id, PaletteProfileId = paletteId, AllowedTargetTags = tags };

    private static VisualOverlayRule Overlay(string id, string kind, IReadOnlyList<string> layerIds) =>
        new() { RuleId = id, OverlayKind = kind, CompatibleLayerIds = layerIds };

    private static VisualTerrainTransitionRule Transition(string id, string from, string to) =>
        new() { RuleId = id, FromTerrain = from, ToTerrain = to, MaskId = "mask/tile/edge" };

    private static VisualObjectPlacementRule Placement(string id, string kind, IReadOnlyList<string> tags) =>
        new() { RuleId = id, ObjectKind = kind, RequiredTags = tags };

    private static VisualCreatureBodyPlanProfile BodyPlan(string id, string kind, bool adultEligible) =>
        new()
        {
            BodyPlanProfileId = id,
            BodyPlanKind = kind,
            AdultEligible = adultEligible,
            AgeKnownAdult = adultEligible,
            Sapient = adultEligible,
            HumanoidCompatible = adultEligible,
            CompatibleSocketIds = ["socket/creature/head", "socket/creature/torso", "socket/creature/hand_main", "socket/creature/back"]
        };

    private static VisualEquipmentOverlayProfile Equipment(
        string id,
        string kind,
        IReadOnlyList<string> socketIds,
        IReadOnlyList<string> bodyPlanIds) =>
        new()
        {
            EquipmentOverlayProfileId = id,
            OverlayKind = kind,
            CompatibleSocketIds = socketIds,
            CompatibleBodyPlanProfileIds = bodyPlanIds
        };

    private static IReadOnlyList<VisualPartLayer> TileLayers() =>
    [
        new VisualPartLayer { LayerId = "layer/base", Order = 0, Role = "base_surface" },
        new VisualPartLayer { LayerId = "layer/detail", Order = 10, Role = "terrain_detail" },
        new VisualPartLayer { LayerId = "layer/overlay", Order = 20, Role = "transition_overlay" }
    ];

    private static IReadOnlyList<VisualPartLayer> FacadeLayers() =>
    [
        new VisualPartLayer { LayerId = "layer/base", Order = 0, Role = "facade_mass" },
        new VisualPartLayer { LayerId = "layer/detail", Order = 10, Role = "door_window_trim" },
        new VisualPartLayer { LayerId = "layer/overlay", Order = 20, Role = "district_marker" }
    ];

    private static IReadOnlyList<VisualPartLayer> CreatureLayers() =>
    [
        new VisualPartLayer { LayerId = "layer/base", Order = 0, Role = "silhouette" },
        new VisualPartLayer { LayerId = "layer/detail", Order = 10, Role = "surface_or_clothing" },
        new VisualPartLayer { LayerId = "layer/overlay", Order = 20, Role = "equipment_or_state" },
        new VisualPartLayer { LayerId = "layer/body", Order = 1, Role = "body_plan" },
        new VisualPartLayer { LayerId = "layer/clothing", Order = 11, Role = "clothing" }
    ];

    private static IReadOnlyList<VisualPartLayer> UiLayers() =>
    [
        new VisualPartLayer { LayerId = "layer/base", Order = 0, Role = "frame" },
        new VisualPartLayer { LayerId = "layer/detail", Order = 10, Role = "icon_detail" },
        new VisualPartLayer { LayerId = "layer/overlay", Order = 20, Role = "effect_overlay" }
    ];

    private static IReadOnlyList<VisualMaskDefinition> TileMasks() =>
    [
        Mask("mask/tile/base", "base_fill"),
        Mask("mask/tile/edge", "edge_transition"),
        Mask("mask/tile/detail", "detail_noise")
    ];

    private static IReadOnlyList<VisualMaskDefinition> FacadeMasks() =>
    [
        Mask("mask/facade/base", "base_shape"),
        Mask("mask/facade/openings", "door_window"),
        Mask("mask/facade/detail", "trim_detail")
    ];

    private static IReadOnlyList<VisualMaskDefinition> CreatureMasks() =>
    [
        Mask("mask/creature/body", "body_silhouette"),
        Mask("mask/creature/clothing", "clothing"),
        Mask("mask/creature/equipment", "equipment")
    ];

    private static IReadOnlyList<VisualMaskDefinition> UiMasks() =>
    [
        Mask("mask/ui/frame", "frame"),
        Mask("mask/ui/icon", "icon"),
        Mask("mask/ui/effect", "effect")
    ];

    private static VisualMaskDefinition Mask(string id, string kind) =>
        new() { MaskId = id, MaskKind = kind, RelativePath = $"visual/masks/{id.Replace('/', '_')}.metadata.json" };

    private static IReadOnlyList<VisualSocketDefinition> TileSockets() =>
    [
        Socket("socket/tile/center", "tile_center", ["terrain_base", "water_surface"]),
        Socket("socket/tile/edge", "tile_edge", ["terrain_transition", "coast_transition"]),
        Socket("socket/tile/object", "tile_object", ["water_object"])
    ];

    private static IReadOnlyList<VisualSocketDefinition> FacadeSockets() =>
    [
        Socket("socket/facade/base", "facade_base", ["house_facade", "castle_facade"]),
        Socket("socket/facade/opening", "facade_opening", ["gate", "market"]),
        Socket("socket/facade/roof", "facade_roof", ["wall", "production_site"])
    ];

    private static IReadOnlyList<VisualSocketDefinition> CreatureSockets() =>
    [
        Socket("socket/creature/head", "head", ["headgear", "body_plan"]),
        Socket("socket/creature/torso", "torso", ["torso_clothing", "surface_plan"]),
        Socket("socket/creature/hand_main", "hand_main", ["weapon"]),
        Socket("socket/creature/hand_off", "hand_off", ["shield"]),
        Socket("socket/creature/back", "back", ["back_item"])
    ];

    private static IReadOnlyList<VisualSocketDefinition> UiSockets() =>
    [
        Socket("socket/ui/frame", "frame", ["ui_panel", "ui_button"]),
        Socket("socket/ui/icon", "icon", ["icon", "status"]),
        Socket("socket/ui/effect", "effect", ["weather", "day_night_effect"])
    ];

    private static VisualSocketDefinition Socket(string id, string kind, IReadOnlyList<string> roles) =>
        new() { SocketId = id, SocketKind = kind, CompatibleRoles = roles };

    private static IReadOnlyList<VisualAnchorDefinition> TileAnchors() =>
    [
        Anchor("anchor/tile/center", "center", 0.5, 0.5),
        Anchor("anchor/tile/edge", "edge", 0.5, 0.0)
    ];

    private static IReadOnlyList<VisualAnchorDefinition> FacadeAnchors() =>
    [
        Anchor("anchor/facade/bottom", "bottom_center", 0.5, 1.0),
        Anchor("anchor/facade/door", "door_center", 0.5, 0.72)
    ];

    private static IReadOnlyList<VisualAnchorDefinition> CreatureAnchors() =>
    [
        Anchor("anchor/creature/head", "head", 0.5, 0.16),
        Anchor("anchor/creature/torso", "torso", 0.5, 0.45),
        Anchor("anchor/creature/feet", "feet", 0.5, 0.96)
    ];

    private static IReadOnlyList<VisualAnchorDefinition> UiAnchors() =>
    [
        Anchor("anchor/ui/center", "center", 0.5, 0.5),
        Anchor("anchor/ui/status", "status_corner", 0.92, 0.12)
    ];

    private static VisualAnchorDefinition Anchor(string id, string kind, double x, double y) =>
        new() { AnchorId = id, AnchorKind = kind, NormalizedX = x, NormalizedY = y };

    internal static string StableHash(string value) => VisualPartPackRuleStackHash.StableHash(value);
}
