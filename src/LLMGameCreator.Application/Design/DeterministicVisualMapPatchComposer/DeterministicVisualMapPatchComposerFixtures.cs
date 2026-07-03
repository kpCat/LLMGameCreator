namespace LLMGameCreator.Application.Design.DeterministicVisualMapPatchComposer;

public static class DeterministicVisualMapPatchComposerFixtures
{
    public static readonly IReadOnlyList<string> RequiredPatchIds =
    [
        "heroes_like_overworld_24x16",
        "water_coast_river_lake_marsh_24x16",
        "mixed_biome_settlement_creature_24x16"
    ];

    public static readonly IReadOnlySet<string> KnownGoal086MicrotilePreviewIds =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "adult_metadata_only_safe_fallback_slot",
            "atmosphere_day_night_weather_overlay",
            "creature_bodyplan_silhouette",
            "creature_damaged_dirty_worn_state",
            "creature_equipment_clothing_overlay",
            "creature_paperdoll_neutral_slot",
            "effect_status_aura",
            "settlement_caravan_camp",
            "settlement_mine_production",
            "settlement_small_dwelling",
            "settlement_wall_gate",
            "terrain_desert_dry",
            "terrain_forest_overlay",
            "terrain_grass_overworld",
            "terrain_lava_ash",
            "terrain_mountain_rock",
            "terrain_snow_tundra",
            "ui_frame_panel_motif",
            "water_base",
            "water_bridge_dock_anchor",
            "water_coast_transition",
            "water_lake_edge",
            "water_marsh_swamp",
            "water_river_segment"
        };

    public static VisualMapPatchComposerRequest BuildDefaultRequest() =>
        new()
        {
            RequestId = "visual_map_patch_composer_goal087",
            GeneratorVersion = "goal087-map-patch-composer-v1",
            OutputRelativeDirectory = DeterministicVisualMapPatchComposerVocabulary.RelativeOutputDirectory,
            SourceOfTruthKind = "metadata_contract",
            PromptTextIsSourceOfTruth = false,
            SourceGoal084085086LineageRequired = true,
            Patches =
            [
                BuildHeroesLikeOverworld(),
                BuildWaterCoastRiverLakeMarsh(),
                BuildMixedBiomeSettlementCreature()
            ]
        };

    public static IReadOnlyList<string> ReferencedMicrotilePreviewIds(VisualMapPatchDefinition patch)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var cell in patch.Cells)
        {
            Add(ids, cell.SourceMicrotilePreviewId);
            foreach (var tileRef in cell.TileRefs)
            {
                Add(ids, tileRef.PreviewId);
            }
        }

        foreach (var anchor in patch.ObjectAnchors)
        {
            Add(ids, anchor.SourceMicrotilePreviewId);
        }

        foreach (var settlement in patch.SettlementAnchors)
        {
            Add(ids, settlement.SourceMicrotilePreviewId);
        }

        foreach (var marker in patch.CreatureMarkers)
        {
            Add(ids, marker.SourceMicrotilePreviewId);
        }

        foreach (var overlay in patch.Overlays)
        {
            Add(ids, overlay.SourceMicrotilePreviewId);
            Add(ids, overlay.SafeFallbackMicrotilePreviewId);
        }

        return ids.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    private static VisualMapPatchDefinition BuildHeroesLikeOverworld()
    {
        var cells = BuildGrid((x, y) =>
        {
            if (x >= 20 && y >= 10)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.LavaAsh, VisualMapPatchWaterKind.None, passable: false);
            }

            if (x >= 17 && y <= 5)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Snow, VisualMapPatchWaterKind.None, passable: true);
            }

            if (x >= 15 && y >= 6)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Desert, VisualMapPatchWaterKind.None, passable: true);
            }

            if (y >= 10 && x <= 7)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Mountain, VisualMapPatchWaterKind.None, passable: false);
            }

            if (x is >= 5 and <= 12 && y is >= 3 and <= 11)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Forest, VisualMapPatchWaterKind.None, passable: true);
            }

            return Cell(x, y, VisualMapPatchTerrainBiome.Grass, VisualMapPatchWaterKind.None, passable: true);
        });
        var mainRoad = Road(
            "road_main_gate_to_desert",
            Points((1, 7), (2, 7), (3, 7), (4, 7), (5, 7), (6, 7), (7, 7), (8, 7), (9, 7), (10, 7), (11, 7), (12, 7), (13, 7), (14, 7), (15, 7), (16, 7), (17, 7), (18, 7), (19, 7), (20, 7), (21, 7), (22, 7)));
        var northRoad = Road(
            "path_forest_to_snow",
            Points((9, 7), (9, 6), (9, 5), (10, 5), (11, 5), (12, 5), (13, 5), (14, 5), (15, 5), (16, 5), (17, 5), (18, 5)));
        return Patch(
            "heroes_like_overworld_24x16",
            870101,
            cells,
            [
                Anchor("mine_iron_northwest", "mine", 4, 8, "settlement_mine_production", road: true),
                Anchor("wall_gate_central_pass", "wall_gate", 10, 7, "settlement_wall_gate", road: true),
                Anchor("caravan_camp_desert_edge", "caravan_camp", 18, 8, "settlement_caravan_camp", road: true),
                Anchor("treasure_forest_glade", "treasure", 7, 4, "effect_status_aura", road: false)
            ],
            [mainRoad, northRoad],
            [],
            [
                Transition("grass_forest_edge", VisualMapPatchTerrainBiome.Grass, VisualMapPatchTerrainBiome.Forest, Points((5, 3), (5, 4), (5, 5), (5, 6), (5, 7), (5, 8), (5, 9))),
                Transition("desert_lava_edge", VisualMapPatchTerrainBiome.Desert, VisualMapPatchTerrainBiome.LavaAsh, Points((20, 10), (20, 11), (20, 12), (20, 13))),
                Transition("snow_desert_edge", VisualMapPatchTerrainBiome.Snow, VisualMapPatchTerrainBiome.Desert, Points((17, 5), (18, 5), (19, 5)))
            ],
            [
                Settlement("greenhill_keep", "walled_keep", 10, 7, "settlement_wall_gate", "road_main_gate_to_desert", "mine_iron_northwest"),
                Settlement("red_dune_camp", "caravan_outpost", 18, 8, "settlement_caravan_camp", "road_main_gate_to_desert", "")
            ],
            [
                Creature("wolf_pack_forest_marker", 8, 5, "creature_bodyplan_silhouette", "bodyplan/quadruped_small", "equipment/none", "state/patrolling"),
                Creature("ash_guard_marker", 21, 9, "creature_equipment_clothing_overlay", "bodyplan/humanoid", "equipment/guard_basic", "state/alert")
            ],
            [
                Overlay("status_aura_treasure_hint", "effect", "effect_status_aura", 7, 4, 2, 2, effect: "effect/status_aura/treasure_hint")
            ]);
    }

    private static VisualMapPatchDefinition BuildWaterCoastRiverLakeMarsh()
    {
        var cells = BuildGrid((x, y) =>
        {
            if (x <= 4)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Water, VisualMapPatchWaterKind.Sea, passable: false);
            }

            if (y == 7 && x is >= 5 and <= 20)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Water, VisualMapPatchWaterKind.River, passable: false);
            }

            if (x == 5)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Grass, VisualMapPatchWaterKind.Coast, passable: true);
            }

            if (x is >= 17 and <= 21 && y is >= 2 and <= 5)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Water, VisualMapPatchWaterKind.Lake, passable: false);
            }

            if (x >= 18 && y >= 11)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Marsh, VisualMapPatchWaterKind.Marsh, passable: true);
            }

            return Cell(x, y, VisualMapPatchTerrainBiome.Grass, VisualMapPatchWaterKind.None, passable: true);
        });
        var road = Road(
            "road_bridge_to_dock",
            Points((6, 10), (7, 10), (8, 10), (9, 10), (10, 10), (11, 10), (12, 10), (12, 9), (12, 8), (12, 7), (12, 6), (13, 6), (14, 6), (15, 6), (16, 6), (17, 6), (18, 6)));
        var flow = WaterFlow(
            "river_west_to_east",
            VisualMapPatchWaterKind.River,
            Points((5, 7), (6, 7), (7, 7), (8, 7), (9, 7), (10, 7), (11, 7), (12, 7), (13, 7), (14, 7), (15, 7), (16, 7), (17, 7), (18, 7), (19, 7), (20, 7)));
        return Patch(
            "water_coast_river_lake_marsh_24x16",
            870202,
            cells,
            [
                Anchor("bridge_river_crossing", "bridge", 12, 7, "water_bridge_dock_anchor", water: true, road: true, land: false),
                Anchor("dock_west_coast", "dock", 5, 10, "water_bridge_dock_anchor", water: true, road: true, land: false)
            ],
            [road],
            [flow],
            [
                Transition("sea_to_grass_coast", VisualMapPatchTerrainBiome.Water, VisualMapPatchTerrainBiome.Grass, Points((5, 4), (5, 5), (5, 6), (5, 8), (5, 9), (5, 10))),
                Transition("lake_grass_edge", VisualMapPatchTerrainBiome.Water, VisualMapPatchTerrainBiome.Grass, Points((17, 2), (17, 3), (17, 4), (21, 5))),
                Transition("marsh_grass_edge", VisualMapPatchTerrainBiome.Marsh, VisualMapPatchTerrainBiome.Grass, Points((18, 11), (18, 12), (18, 13)))
            ],
            [Settlement("coast_watch_post", "dock_watch", 6, 10, "settlement_small_dwelling", "road_bridge_to_dock", "")],
            [Creature("marsh_stalker_marker", 19, 12, "creature_bodyplan_silhouette", "bodyplan/amphibious_small", "equipment/none", "state/hidden")],
            [Overlay("river_mist_overlay", "weather", "atmosphere_day_night_weather_overlay", 9, 6, 8, 3, weather: "weather/river_mist")]);
    }

    private static VisualMapPatchDefinition BuildMixedBiomeSettlementCreature()
    {
        var cells = BuildGrid((x, y) =>
        {
            if (x <= 2 && y >= 4)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Water, VisualMapPatchWaterKind.Sea, passable: false);
            }

            if (x == 3 && y >= 4)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Water, VisualMapPatchWaterKind.Coast, passable: true);
            }

            if (x is >= 4 and <= 10 && y is >= 3 and <= 10)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Grass, VisualMapPatchWaterKind.None, passable: true);
            }

            if (x is >= 11 and <= 16 && y <= 6)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Forest, VisualMapPatchWaterKind.None, passable: true);
            }

            if (x >= 17 && y <= 7)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Mountain, VisualMapPatchWaterKind.None, passable: false);
            }

            if (x >= 14 && y >= 11)
            {
                return Cell(x, y, VisualMapPatchTerrainBiome.Desert, VisualMapPatchWaterKind.None, passable: true);
            }

            return Cell(x, y, VisualMapPatchTerrainBiome.Grass, VisualMapPatchWaterKind.None, passable: true);
        });
        var road = Road(
            "road_settlement_to_harbor_and_forest",
            Points((4, 8), (5, 8), (6, 8), (7, 8), (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (13, 8), (13, 7), (13, 6), (14, 6), (14, 5)));
        var harbor = Road("path_to_harbor", Points((6, 8), (5, 8), (4, 8), (3, 8), (3, 9), (3, 10)));
        return Patch(
            "mixed_biome_settlement_creature_24x16",
            870303,
            cells,
            [
                Anchor("harbor_dock_anchor", "dock", 3, 10, "water_bridge_dock_anchor", water: true, road: true, land: false),
                Anchor("market_treasure_anchor", "treasure", 8, 8, "effect_status_aura", road: true),
                Anchor("gatehouse_anchor", "wall_gate", 7, 8, "settlement_wall_gate", road: true)
            ],
            [road, harbor],
            [],
            [
                Transition("grass_forest_contact", VisualMapPatchTerrainBiome.Grass, VisualMapPatchTerrainBiome.Forest, Points((11, 6), (12, 6), (13, 6))),
                Transition("grass_mountain_contact", VisualMapPatchTerrainBiome.Grass, VisualMapPatchTerrainBiome.Mountain, Points((17, 7), (18, 7), (19, 7))),
                Transition("grass_desert_contact", VisualMapPatchTerrainBiome.Grass, VisualMapPatchTerrainBiome.Desert, Points((14, 11), (15, 11), (16, 11)))
            ],
            [
                Settlement("riverbend_market", "market_town", 7, 8, "settlement_small_dwelling", "road_settlement_to_harbor_and_forest", "market_treasure_anchor")
            ],
            [
                Creature("caravan_guard_marker", 10, 8, "creature_equipment_clothing_overlay", "bodyplan/humanoid", "equipment/caravan_guard", "state/escort"),
                Creature("forest_beast_marker", 13, 5, "creature_damaged_dirty_worn_state", "bodyplan/quadruped_large", "equipment/natural_hide", "state/wounded")
            ],
            [
                Overlay("settlement_weather_daynight_overlay", "day_night_weather", "atmosphere_day_night_weather_overlay", 4, 5, 8, 5, dayNight: "daynight/dusk", weather: "weather/light_rain"),
                Overlay(
                    "adult_rating_metadata_only_safe_fallback_route",
                    "rating_fallback",
                    "adult_metadata_only_safe_fallback_slot",
                    12,
                    9,
                    2,
                    2,
                    adultMetadataOnly: true,
                    safeFallback: "creature_paperdoll_neutral_slot",
                    providerState: VisualMapPatchProviderState.CandidateQuarantine)
            ]);
    }

    private static VisualMapPatchDefinition Patch(
        string patchId,
        int seed,
        IReadOnlyList<VisualMapPatchCell> cells,
        IReadOnlyList<VisualMapPatchObjectAnchor> anchors,
        IReadOnlyList<VisualMapPatchRoadPath> roads,
        IReadOnlyList<VisualMapPatchWaterFlow> waterFlows,
        IReadOnlyList<VisualMapPatchBiomeTransition> transitions,
        IReadOnlyList<VisualMapPatchSettlementAnchor> settlements,
        IReadOnlyList<VisualMapPatchCreatureMarker> creatures,
        IReadOnlyList<VisualMapPatchOverlay> overlays) =>
        new()
        {
            PatchId = patchId,
            Width = 24,
            Height = 16,
            Seed = seed,
            PatchSvgRelativePath = $"{DeterministicVisualMapPatchComposerVocabulary.PatchRelativeDirectory}/{patchId}.svg",
            Layers = DefaultLayers(),
            Cells = cells,
            ObjectAnchors = anchors,
            RoadPaths = roads,
            WaterFlows = waterFlows,
            BiomeTransitions = transitions,
            SettlementAnchors = settlements,
            CreatureMarkers = creatures,
            Overlays = overlays,
            SourceReferences = SourceReferences()
        };

    private static IReadOnlyList<VisualMapPatchCell> BuildGrid(Func<int, int, VisualMapPatchCell> factory)
    {
        var cells = new List<VisualMapPatchCell>(24 * 16);
        for (var y = 0; y < 16; y++)
        {
            for (var x = 0; x < 24; x++)
            {
                cells.Add(factory(x, y));
            }
        }

        return cells;
    }

    private static VisualMapPatchCell Cell(
        int x,
        int y,
        VisualMapPatchTerrainBiome biome,
        VisualMapPatchWaterKind waterKind,
        bool passable)
    {
        var previewId = MicrotileFor(biome, waterKind);
        var transition = waterKind switch
        {
            VisualMapPatchWaterKind.Coast => VisualMapPatchTransitionKind.Coast,
            VisualMapPatchWaterKind.River => VisualMapPatchTransitionKind.River,
            VisualMapPatchWaterKind.Lake => VisualMapPatchTransitionKind.LakeEdge,
            VisualMapPatchWaterKind.Marsh => VisualMapPatchTransitionKind.MarshEdge,
            _ => VisualMapPatchTransitionKind.None
        };

        return new VisualMapPatchCell
        {
            CellId = $"cell_{x:00}_{y:00}",
            X = x,
            Y = y,
            TerrainBiome = biome,
            WaterKind = waterKind,
            TransitionKind = transition,
            IsPassable = passable,
            SourceMicrotilePreviewId = previewId,
            TileRefs =
            [
                new VisualMapPatchTileRef { PreviewId = previewId, LayerKind = waterKind == VisualMapPatchWaterKind.None ? VisualMapPatchLayerKind.Terrain : VisualMapPatchLayerKind.Water, Order = 0 }
            ],
            Connectors = [],
            Tags = TagsFor(biome, waterKind)
        };
    }

    private static string MicrotileFor(VisualMapPatchTerrainBiome biome, VisualMapPatchWaterKind waterKind) =>
        waterKind switch
        {
            VisualMapPatchWaterKind.Sea => "water_base",
            VisualMapPatchWaterKind.Coast => "water_coast_transition",
            VisualMapPatchWaterKind.River => "water_river_segment",
            VisualMapPatchWaterKind.Lake => "water_lake_edge",
            VisualMapPatchWaterKind.Marsh => "water_marsh_swamp",
            _ => biome switch
            {
                VisualMapPatchTerrainBiome.Forest => "terrain_forest_overlay",
                VisualMapPatchTerrainBiome.Mountain => "terrain_mountain_rock",
                VisualMapPatchTerrainBiome.Snow => "terrain_snow_tundra",
                VisualMapPatchTerrainBiome.Desert => "terrain_desert_dry",
                VisualMapPatchTerrainBiome.LavaAsh => "terrain_lava_ash",
                _ => "terrain_grass_overworld"
            }
        };

    private static IReadOnlyList<string> TagsFor(VisualMapPatchTerrainBiome biome, VisualMapPatchWaterKind waterKind)
    {
        var tags = new List<string> { $"biome/{biome.ToString().ToLowerInvariant()}" };
        if (waterKind != VisualMapPatchWaterKind.None)
        {
            tags.Add($"water/{waterKind.ToString().ToLowerInvariant()}");
        }

        return tags;
    }

    private static IReadOnlyList<VisualMapPatchLayer> DefaultLayers() =>
    [
        Layer("layer/terrain", VisualMapPatchLayerKind.Terrain, 0, "terrain and biome base"),
        Layer("layer/water", VisualMapPatchLayerKind.Water, 10, "water, coast, river, lake and marsh cells"),
        Layer("layer/road", VisualMapPatchLayerKind.Road, 20, "road and path connectors"),
        Layer("layer/object", VisualMapPatchLayerKind.Object, 30, "treasure, mines, bridge, dock and props"),
        Layer("layer/settlement", VisualMapPatchLayerKind.Settlement, 40, "settlement anchors"),
        Layer("layer/creature", VisualMapPatchLayerKind.Creature, 50, "creature and NPC markers"),
        Layer("layer/overlay", VisualMapPatchLayerKind.Overlay, 60, "day night weather effect overlays"),
        Layer("layer/rating_fallback", VisualMapPatchLayerKind.RatingFallback, 70, "adult/rating metadata-only fallback route")
    ];

    private static VisualMapPatchLayer Layer(string id, VisualMapPatchLayerKind kind, int order, string description) =>
        new() { LayerId = id, Kind = kind, Order = order, Description = description };

    private static VisualMapPatchObjectAnchor Anchor(
        string id,
        string kind,
        int x,
        int y,
        string previewId,
        bool water = false,
        bool road = false,
        bool land = true) =>
        new()
        {
            AnchorId = id,
            ObjectKind = kind,
            X = x,
            Y = y,
            SourceMicrotilePreviewId = previewId,
            RequiresWaterAdjacency = water,
            RequiresRoadAdjacency = road,
            RequiresLandCell = land,
            MetadataTags = [$"object/{kind}"]
        };

    private static VisualMapPatchSettlementAnchor Settlement(
        string id,
        string role,
        int x,
        int y,
        string previewId,
        string nearPathId,
        string nearResourceAnchorId) =>
        new()
        {
            SettlementId = id,
            SettlementRole = role,
            X = x,
            Y = y,
            SourceMicrotilePreviewId = previewId,
            NearPathId = nearPathId,
            NearResourceAnchorId = nearResourceAnchorId
        };

    private static VisualMapPatchCreatureMarker Creature(
        string id,
        int x,
        int y,
        string previewId,
        string bodyPlan,
        string equipment,
        string state) =>
        new()
        {
            MarkerId = id,
            X = x,
            Y = y,
            SourceMicrotilePreviewId = previewId,
            BodyPlanId = bodyPlan,
            EquipmentProfileId = equipment,
            StateMetadataId = state,
            RatingSafe = true
        };

    private static VisualMapPatchOverlay Overlay(
        string id,
        string kind,
        string previewId,
        int x,
        int y,
        int width,
        int height,
        string dayNight = "",
        string weather = "",
        string effect = "",
        bool adultMetadataOnly = false,
        string safeFallback = "",
        VisualMapPatchProviderState providerState = VisualMapPatchProviderState.MetadataOnly) =>
        new()
        {
            OverlayId = id,
            OverlayKind = kind,
            SourceMicrotilePreviewId = previewId,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            DayNightMetadata = dayNight,
            WeatherMetadata = weather,
            EffectMetadata = effect,
            AdultMetadataOnly = adultMetadataOnly,
            SafeFallbackMicrotilePreviewId = safeFallback,
            ProviderState = providerState,
            TreatProviderCandidateAsApprovedOutput = false
        };

    private static VisualMapPatchRoadPath Road(string id, IReadOnlyList<VisualMapPatchCoordinate> points) =>
        new()
        {
            PathId = id,
            PathKind = "road",
            Nodes = Nodes(points)
        };

    private static VisualMapPatchWaterFlow WaterFlow(
        string id,
        VisualMapPatchWaterKind kind,
        IReadOnlyList<VisualMapPatchCoordinate> points) =>
        new()
        {
            FlowId = id,
            WaterKind = kind,
            Nodes = Nodes(points)
        };

    private static VisualMapPatchBiomeTransition Transition(
        string id,
        VisualMapPatchTerrainBiome from,
        VisualMapPatchTerrainBiome to,
        IReadOnlyList<VisualMapPatchCoordinate> cells) =>
        new() { TransitionId = id, FromBiome = from, ToBiome = to, Cells = cells };

    private static IReadOnlyList<VisualMapPatchPathNode> Nodes(IReadOnlyList<VisualMapPatchCoordinate> points)
    {
        var nodes = new List<VisualMapPatchPathNode>(points.Count);
        for (var index = 0; index < points.Count; index++)
        {
            var connectors = new HashSet<VisualMapPatchConnector>();
            if (index > 0)
            {
                connectors.Add(ConnectorTo(points[index], points[index - 1]));
            }

            if (index + 1 < points.Count)
            {
                connectors.Add(ConnectorTo(points[index], points[index + 1]));
            }

            nodes.Add(new VisualMapPatchPathNode
            {
                X = points[index].X,
                Y = points[index].Y,
                Connectors = connectors.OrderBy(item => item).ToList()
            });
        }

        return nodes;
    }

    private static VisualMapPatchConnector ConnectorTo(VisualMapPatchCoordinate from, VisualMapPatchCoordinate to)
    {
        if (to.X > from.X)
        {
            return VisualMapPatchConnector.East;
        }

        if (to.X < from.X)
        {
            return VisualMapPatchConnector.West;
        }

        if (to.Y > from.Y)
        {
            return VisualMapPatchConnector.South;
        }

        return VisualMapPatchConnector.North;
    }

    private static IReadOnlyList<VisualMapPatchCoordinate> Points(params (int X, int Y)[] points) =>
        points.Select(item => new VisualMapPatchCoordinate { X = item.X, Y = item.Y }).ToList();

    private static IReadOnlyList<VisualMapPatchSourceReference> SourceReferences() =>
    [
        Source("goal084", "visual_asset_contract_rating_metadata", ".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md"),
        Source("goal085", "visual_part_pack_rule_stack", ".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-rule-stack-report.md"),
        Source("goal086", "deterministic_visual_microtile_materializer", ".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-materializer-report.md"),
        Source("goal086", "visual_microtile_preview_catalog", ".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-preview-catalog.json")
    ];

    private static VisualMapPatchSourceReference Source(string kind, string id, string relativePath) =>
        new() { SourceKind = kind, SourceId = id, RelativePath = relativePath };

    private static void Add(HashSet<string> ids, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            ids.Add(value);
        }
    }
}
