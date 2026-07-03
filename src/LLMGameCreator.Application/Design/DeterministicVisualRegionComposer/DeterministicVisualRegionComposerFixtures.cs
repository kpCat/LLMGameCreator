namespace LLMGameCreator.Application.Design.DeterministicVisualRegionComposer;

public static class DeterministicVisualRegionComposerFixtures
{
    public static readonly IReadOnlySet<string> KnownGoal087PatchIds =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "heroes_like_overworld_24x16",
            "mixed_biome_settlement_creature_24x16",
            "water_coast_river_lake_marsh_24x16"
        };

    public static VisualRegionDefinition BuildDefaultDefinition()
    {
        var surfacePlacements = BuildSurfacePlacements();
        var undergroundPlacements = BuildUndergroundPlacements();
        var surfaceChunks = BuildChunks(surfacePlacements);
        var undergroundChunks = BuildChunks(undergroundPlacements);

        return new VisualRegionDefinition
        {
            Seed = 880144,
            Layers =
            [
                new VisualRegionLayer
                {
                    LayerId = DeterministicVisualRegionComposerVocabulary.SurfaceLayerId,
                    PatchPlacements = surfacePlacements,
                    Chunks = surfaceChunks
                },
                new VisualRegionLayer
                {
                    LayerId = DeterministicVisualRegionComposerVocabulary.UndergroundLayerId,
                    PatchPlacements = undergroundPlacements,
                    Chunks = undergroundChunks
                }
            ],
            BiomeBands = BuildBiomeBands(surfacePlacements, undergroundPlacements),
            WaterNetwork = BuildWaterNetwork(surfacePlacements, undergroundPlacements),
            RoadNetwork = BuildRoadNetwork(),
            Settlements = BuildSettlements(),
            GateTransitions = BuildGateTransitions(),
            ObjectPlacements = BuildObjects(),
            CreaturePlacements = BuildCreatures(),
            Overlays = BuildOverlays(),
            SourceReferences = SourceReferences()
        };
    }

    public static IReadOnlyList<string> ReferencedGoal087PatchIds(VisualRegionDefinition definition) =>
        definition.Layers
            .SelectMany(item => item.PatchPlacements)
            .Select(item => item.SourceGoal087PatchId)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

    private static IReadOnlyList<VisualRegionPatchPlacement> BuildSurfacePlacements()
    {
        var placements = new List<VisualRegionPatchPlacement>(DeterministicVisualRegionComposerVocabulary.PatchPlacementsPerLayer);
        for (var gridY = 0; gridY < DeterministicVisualRegionComposerVocabulary.PatchGridRows; gridY++)
        {
            for (var gridX = 0; gridX < DeterministicVisualRegionComposerVocabulary.PatchGridColumns; gridX++)
            {
                placements.Add(BuildPlacement(
                    DeterministicVisualRegionComposerVocabulary.SurfaceLayerId,
                    gridX,
                    gridY,
                    SurfacePatchId(gridX, gridY),
                    SurfaceBiomes(gridX, gridY),
                    SurfaceWaterKinds(gridX, gridY),
                    SurfaceWaterConnectors(gridX, gridY),
                    SurfaceRoadConnectors(gridX, gridY),
                    SurfacePalette(gridX, gridY)));
            }
        }

        return placements;
    }

    private static IReadOnlyList<VisualRegionPatchPlacement> BuildUndergroundPlacements()
    {
        var placements = new List<VisualRegionPatchPlacement>(DeterministicVisualRegionComposerVocabulary.PatchPlacementsPerLayer);
        for (var gridY = 0; gridY < DeterministicVisualRegionComposerVocabulary.PatchGridRows; gridY++)
        {
            for (var gridX = 0; gridX < DeterministicVisualRegionComposerVocabulary.PatchGridColumns; gridX++)
            {
                placements.Add(BuildPlacement(
                    DeterministicVisualRegionComposerVocabulary.UndergroundLayerId,
                    gridX,
                    gridY,
                    UndergroundPatchId(gridX, gridY),
                    UndergroundBiomes(gridX, gridY),
                    UndergroundWaterKinds(gridX, gridY),
                    UndergroundWaterConnectors(gridX, gridY),
                    UndergroundRoadConnectors(gridX, gridY),
                    UndergroundPalette(gridX, gridY)));
            }
        }

        return placements;
    }

    private static VisualRegionPatchPlacement BuildPlacement(
        string layerId,
        int gridX,
        int gridY,
        string patchId,
        IReadOnlyList<string> biomes,
        IReadOnlyList<string> waterKinds,
        VisualRegionEdgeConnectors waterConnectors,
        VisualRegionEdgeConnectors roadConnectors,
        string palette) =>
        new()
        {
            PlacementId = $"{layerId}_p{gridX:00}_{gridY:00}",
            LayerId = layerId,
            SourceGoal087PatchId = patchId,
            GridX = gridX,
            GridY = gridY,
            X = gridX * DeterministicVisualRegionComposerVocabulary.PatchWidth,
            Y = gridY * DeterministicVisualRegionComposerVocabulary.PatchHeight,
            Transform = new VisualRegionPatchTransform
            {
                RotationDegrees = ((gridX + gridY) % 4) * 90,
                MirrorX = (gridX + gridY) % 2 == 0,
                MirrorY = gridY % 3 == 0,
                RepaletteProfileId = palette
            },
            DeclaredBiomes = biomes,
            DeclaredWaterKinds = waterKinds,
            WaterConnectors = waterConnectors,
            RoadConnectors = roadConnectors,
            MetadataTags =
            [
                $"layer/{layerId}",
                $"patch/{patchId}",
                $"palette/{palette}"
            ]
        };

    private static string SurfacePatchId(int gridX, int gridY)
    {
        if (gridX == 0 || gridY == 4 || (gridX is 3 or 4 && gridY == 2) || (gridX >= 4 && gridY == 7))
        {
            return "water_coast_river_lake_marsh_24x16";
        }

        if ((gridX is 1 or 2 && gridY is >= 3 and <= 5) || (gridX >= 3 && gridY >= 5))
        {
            return "mixed_biome_settlement_creature_24x16";
        }

        return "heroes_like_overworld_24x16";
    }

    private static string UndergroundPatchId(int gridX, int gridY)
    {
        if (gridY == 5 || gridY == 7)
        {
            return "water_coast_river_lake_marsh_24x16";
        }

        if (gridX >= 3 || gridY >= 6)
        {
            return "mixed_biome_settlement_creature_24x16";
        }

        return "heroes_like_overworld_24x16";
    }

    private static IReadOnlyList<string> SurfaceBiomes(int gridX, int gridY)
    {
        if (gridX == 0)
        {
            return ["sea", "coast", "grass"];
        }

        if (gridY == 4)
        {
            return ["river", "grass", "forest"];
        }

        if (gridY <= 1)
        {
            return gridX >= 3 ? ["snow", "mountain"] : ["grass", "forest"];
        }

        if (gridX >= 4 && gridY >= 6)
        {
            return gridY == 8 ? ["lava_ash", "desert"] : ["desert", "lava_ash"];
        }

        if (gridX >= 3 && gridY is >= 2 and <= 4)
        {
            return ["mountain", "snow", "forest"];
        }

        if (gridX >= 4 && gridY == 7)
        {
            return ["marsh", "grass"];
        }

        return gridY >= 6 ? ["forest", "grass"] : ["grass", "forest"];
    }

    private static IReadOnlyList<string> UndergroundBiomes(int gridX, int gridY)
    {
        if (gridY == 5)
        {
            return ["underground_water", "cave"];
        }

        if (gridY == 7)
        {
            return ["lava", "rock"];
        }

        if (gridX >= 4 && gridY <= 2)
        {
            return ["ruin", "rock"];
        }

        if (gridX <= 1 && gridY >= 6)
        {
            return ["mushroom", "cave"];
        }

        if (gridY >= 6)
        {
            return ["rock", "lava"];
        }

        return ["cave", "rock"];
    }

    private static IReadOnlyList<string> SurfaceWaterKinds(int gridX, int gridY)
    {
        var kinds = new SortedSet<string>(StringComparer.Ordinal);
        if (gridX == 0)
        {
            kinds.Add("sea");
            kinds.Add("coast");
        }

        if (gridY == 4)
        {
            kinds.Add("river");
        }

        if (gridX is 3 or 4 && gridY == 2)
        {
            kinds.Add("lake");
        }

        if (gridX >= 4 && gridY == 7)
        {
            kinds.Add("marsh");
        }

        return kinds.ToList();
    }

    private static IReadOnlyList<string> UndergroundWaterKinds(int gridX, int gridY)
    {
        var kinds = new SortedSet<string>(StringComparer.Ordinal);
        if (gridY == 5 && gridX <= 2)
        {
            kinds.Add("underground_water");
        }

        if (gridY == 7 && gridX >= 3)
        {
            kinds.Add("lava_boundary");
        }

        return kinds.ToList();
    }

    private static VisualRegionEdgeConnectors SurfaceWaterConnectors(int gridX, int gridY)
    {
        if (gridY == 4)
        {
            return new VisualRegionEdgeConnectors { East = "river", West = "river" };
        }

        if (gridX == 0)
        {
            return new VisualRegionEdgeConnectors { West = "sea", East = "coast" };
        }

        if (gridX == 1)
        {
            return new VisualRegionEdgeConnectors { West = "coast" };
        }

        if (gridY == 2 && gridX == 3)
        {
            return new VisualRegionEdgeConnectors { East = "lake" };
        }

        if (gridY == 2 && gridX == 4)
        {
            return new VisualRegionEdgeConnectors { West = "lake" };
        }

        if (gridY == 7 && gridX == 4)
        {
            return new VisualRegionEdgeConnectors { East = "marsh" };
        }

        if (gridY == 7 && gridX == 5)
        {
            return new VisualRegionEdgeConnectors { West = "marsh" };
        }

        return new VisualRegionEdgeConnectors();
    }

    private static VisualRegionEdgeConnectors UndergroundWaterConnectors(int gridX, int gridY)
    {
        if (gridY == 5 && gridX <= 2)
        {
            return new VisualRegionEdgeConnectors
            {
                East = gridX < 2 ? "underground_water" : "none",
                West = gridX > 0 ? "underground_water" : "underground_water"
            };
        }

        if (gridY == 7 && gridX >= 3)
        {
            return new VisualRegionEdgeConnectors
            {
                East = gridX < 5 ? "lava_flow" : "none",
                West = gridX > 3 ? "lava_flow" : "none"
            };
        }

        return new VisualRegionEdgeConnectors();
    }

    private static VisualRegionEdgeConnectors SurfaceRoadConnectors(int gridX, int gridY)
    {
        if (gridY is 3 or 4 or 5)
        {
            return new VisualRegionEdgeConnectors { East = "main_road", West = "main_road" };
        }

        if (gridX == 2)
        {
            return new VisualRegionEdgeConnectors { North = "north_south_road", South = "north_south_road" };
        }

        return new VisualRegionEdgeConnectors();
    }

    private static VisualRegionEdgeConnectors UndergroundRoadConnectors(int gridX, int gridY)
    {
        if (gridY is >= 4 and <= 7)
        {
            return new VisualRegionEdgeConnectors { East = "tunnel_road", West = "tunnel_road" };
        }

        if (gridX == 1)
        {
            return new VisualRegionEdgeConnectors { North = "shaft_path", South = "shaft_path" };
        }

        return new VisualRegionEdgeConnectors();
    }

    private static string SurfacePalette(int gridX, int gridY)
    {
        if (gridX == 0)
        {
            return "palette/surface_coast";
        }

        if (gridY <= 1)
        {
            return "palette/surface_snowline";
        }

        if (gridX >= 4 && gridY >= 6)
        {
            return "palette/surface_desert_lava";
        }

        if (gridY == 4)
        {
            return "palette/surface_river";
        }

        return "palette/surface_greenlands";
    }

    private static string UndergroundPalette(int gridX, int gridY)
    {
        if (gridY == 5)
        {
            return "palette/underground_water";
        }

        if (gridY == 7)
        {
            return "palette/underground_lava";
        }

        if (gridX >= 4)
        {
            return "palette/underground_ruins";
        }

        return "palette/underground_cave";
    }

    private static IReadOnlyList<VisualRegionChunk> BuildChunks(IReadOnlyList<VisualRegionPatchPlacement> placements) =>
        placements
            .OrderBy(item => item.LayerId, StringComparer.Ordinal)
            .ThenBy(item => item.GridY)
            .ThenBy(item => item.GridX)
            .Select(item =>
            {
                var dominantBiome = item.DeclaredBiomes.FirstOrDefault() ?? "unknown";
                var dominantWater = item.DeclaredWaterKinds.FirstOrDefault() ?? "none";
                return new VisualRegionChunk
                {
                    ChunkId = $"{item.LayerId}_chunk_{item.GridX:00}_{item.GridY:00}",
                    LayerId = item.LayerId,
                    PlacementId = item.PlacementId,
                    GridX = item.GridX,
                    GridY = item.GridY,
                    X = item.X,
                    Y = item.Y,
                    DominantBiome = dominantBiome,
                    DominantWaterKind = dominantWater,
                    CompactRleRows =
                    [
                        $"dominant:{dominantBiome}:384",
                        $"water:{dominantWater}:{(dominantWater == "none" ? 0 : 96)}"
                    ],
                    SummaryTags = item.MetadataTags
                };
            })
            .ToList();

    private static IReadOnlyList<VisualRegionBiomeBand> BuildBiomeBands(
        IReadOnlyList<VisualRegionPatchPlacement> surfacePlacements,
        IReadOnlyList<VisualRegionPatchPlacement> undergroundPlacements)
    {
        var bands = new List<VisualRegionBiomeBand>();
        bands.AddRange(BuildBandsForLayer(DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, surfacePlacements, ["grass", "forest", "mountain", "snow", "desert", "lava_ash", "sea", "coast", "river", "lake", "marsh"]));
        bands.AddRange(BuildBandsForLayer(DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, undergroundPlacements, ["cave", "rock", "lava", "underground_water", "mushroom", "ruin"]));
        return bands;
    }

    private static IEnumerable<VisualRegionBiomeBand> BuildBandsForLayer(
        string layerId,
        IReadOnlyList<VisualRegionPatchPlacement> placements,
        IReadOnlyList<string> biomes)
    {
        foreach (var biome in biomes)
        {
            var matching = placements.Where(item => item.DeclaredBiomes.Contains(biome, StringComparer.Ordinal)).ToList();
            yield return new VisualRegionBiomeBand
            {
                BandId = $"{layerId}_{biome}_band",
                LayerId = layerId,
                BiomeId = biome,
                EstimatedCellCount = matching.Count * DeterministicVisualRegionComposerVocabulary.PatchWidth * DeterministicVisualRegionComposerVocabulary.PatchHeight,
                CompactRleRows = matching
                    .OrderBy(item => item.GridY)
                    .ThenBy(item => item.GridX)
                    .Select(item => $"p{item.GridX:00}_{item.GridY:00}:{biome}:384")
                    .ToList()
            };
        }
    }

    private static VisualRegionWaterNetwork BuildWaterNetwork(
        IReadOnlyList<VisualRegionPatchPlacement> surfacePlacements,
        IReadOnlyList<VisualRegionPatchPlacement> undergroundPlacements) =>
        new()
        {
            DeclaresWater = true,
            DeclaresLavaBoundaryMetadata = true,
            Segments =
            [
                Segment("surface_western_sea_coast", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "sea", surfacePlacements.Where(item => item.GridX is 0 or 1).Select(item => item.PlacementId), ["surface_dock_west"]),
                Segment("surface_river_crossing", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "river", surfacePlacements.Where(item => item.GridY == 4).Select(item => item.PlacementId), ["surface_bridge_river"]),
                Segment("surface_north_lake", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "lake", surfacePlacements.Where(item => item.GridY == 2 && item.GridX is 3 or 4).Select(item => item.PlacementId), []),
                Segment("surface_southern_marsh", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "marsh", surfacePlacements.Where(item => item.GridY == 7 && item.GridX >= 4).Select(item => item.PlacementId), []),
                Segment("underground_cistern", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_water", undergroundPlacements.Where(item => item.GridY == 5 && item.GridX <= 2).Select(item => item.PlacementId), []),
                Segment("underground_lava_boundary", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "lava_boundary", undergroundPlacements.Where(item => item.GridY == 7 && item.GridX >= 3).Select(item => item.PlacementId), [])
            ]
        };

    private static VisualRegionWaterSegment Segment(
        string segmentId,
        string layerId,
        string waterKind,
        IEnumerable<string> placements,
        IReadOnlyList<string> crossings) =>
        new()
        {
            SegmentId = segmentId,
            LayerId = layerId,
            WaterKind = waterKind,
            ConnectedPlacementIds = placements.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            CrossingObjectIds = crossings,
            BoundaryConnectorsValid = true
        };

    private static VisualRegionRoadNetwork BuildRoadNetwork() =>
        new()
        {
            Nodes =
            [
                Node("surface_gate_north", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p02_01", "surface_gate", true),
                Node("surface_castle_greenhill", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p02_03", "castle", true),
                Node("surface_settlement_riverbend", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p02_04", "settlement", true),
                Node("surface_bridge_river", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p03_04", "bridge", true),
                Node("surface_dock_west", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p00_04", "dock", true),
                Node("surface_mine_northwest", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p01_02", "mine", true),
                Node("surface_garrison_mountain_pass", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p04_03", "garrison", true),
                Node("surface_caravan_desert", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p05_06", "caravan", true),
                Node("surface_object_artifact", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p03_06", "object", true),
                Node("surface_gate_south", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p04_08", "surface_gate", true),
                Node("underground_gate_north", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p01_02", "underground_gate", true),
                Node("underground_outpost", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p02_04", "outpost", true),
                Node("underground_ruin_market", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p04_02", "ruin", true),
                Node("underground_mushroom_grove", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p00_06", "mushroom_grove", true),
                Node("underground_lava_forge", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p04_07", "lava_forge", true),
                Node("underground_gate_south", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p04_08", "underground_gate", true)
            ],
            Edges =
            [
                Edge("surface_gate_to_castle", "surface_gate_north", "surface_castle_greenhill"),
                Edge("surface_castle_to_settlement", "surface_castle_greenhill", "surface_settlement_riverbend"),
                Edge("surface_settlement_to_bridge", "surface_settlement_riverbend", "surface_bridge_river"),
                Edge("surface_bridge_to_garrison", "surface_bridge_river", "surface_garrison_mountain_pass"),
                Edge("surface_settlement_to_dock", "surface_settlement_riverbend", "surface_dock_west"),
                Edge("surface_settlement_to_mine", "surface_settlement_riverbend", "surface_mine_northwest"),
                Edge("surface_garrison_to_caravan", "surface_garrison_mountain_pass", "surface_caravan_desert"),
                Edge("surface_caravan_to_artifact", "surface_caravan_desert", "surface_object_artifact"),
                Edge("surface_caravan_to_south_gate", "surface_caravan_desert", "surface_gate_south"),
                Edge("north_gate_transition", "surface_gate_north", "underground_gate_north", "gate_transition"),
                Edge("underground_gate_to_outpost", "underground_gate_north", "underground_outpost", "cave_road"),
                Edge("underground_outpost_to_ruin", "underground_outpost", "underground_ruin_market", "cave_road"),
                Edge("underground_outpost_to_mushroom", "underground_outpost", "underground_mushroom_grove", "cave_road"),
                Edge("underground_ruin_to_lava", "underground_ruin_market", "underground_lava_forge", "lava_tunnel"),
                Edge("underground_lava_to_south_gate", "underground_lava_forge", "underground_gate_south", "lava_tunnel"),
                Edge("south_gate_transition", "underground_gate_south", "surface_gate_south", "gate_transition")
            ]
        };

    private static IReadOnlyList<VisualRegionSettlementPlacement> BuildSettlements() =>
    [
        Settlement("greenhill_keep", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p02_03", "castle", "grass_passable", "surface_castle_greenhill"),
        Settlement("riverbend_market", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p02_04", "settlement", "river_crossing_land", "surface_settlement_riverbend"),
        Settlement("frost_gate_garrison", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p04_03", "garrison", "mountain_pass", "surface_garrison_mountain_pass"),
        Settlement("red_dune_caravanserai", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p05_06", "caravan", "desert_passable", "surface_caravan_desert"),
        Settlement("deep_cistern_outpost", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p02_04", "outpost", "cave_passable", "underground_outpost")
    ];

    private static IReadOnlyList<VisualRegionGateTransition> BuildGateTransitions() =>
    [
        new VisualRegionGateTransition
        {
            TransitionId = "north_cave_gate_pair",
            SurfacePlacementId = "surface_p02_01",
            UndergroundPlacementId = "underground_p01_02",
            SurfaceGateId = "surface_gate_north",
            UndergroundGateId = "underground_gate_north",
            Paired = true
        },
        new VisualRegionGateTransition
        {
            TransitionId = "southern_lava_gate_pair",
            SurfacePlacementId = "surface_p04_08",
            UndergroundPlacementId = "underground_p04_08",
            SurfaceGateId = "surface_gate_south",
            UndergroundGateId = "underground_gate_south",
            Paired = true
        }
    ];

    private static IReadOnlyList<VisualRegionObjectPlacement> BuildObjects() =>
    [
        ObjectPlacement("surface_mine_iron", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p01_02", "mine", "grass_passable", "surface_mine_northwest", true),
        ObjectPlacement("surface_bridge_river", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p03_04", "bridge", "river_crossing", "surface_bridge_river", true, water: true, passable: false),
        ObjectPlacement("surface_dock_west", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p00_04", "dock", "coast", "surface_dock_west", true, water: true, passable: false),
        ObjectPlacement("surface_artifact_obelisk", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p03_06", "object", "desert_passable", "surface_object_artifact", true),
        ObjectPlacement("underground_ruin_cache", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p04_02", "ruin_object", "ruin_passable", "underground_ruin_market", true),
        ObjectPlacement("underground_lava_forge", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p04_07", "object", "lava_boundary_passable", "underground_lava_forge", true)
    ];

    private static IReadOnlyList<VisualRegionCreaturePlacement> BuildCreatures() =>
    [
        Creature("surface_forest_wolf_pack", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p02_03", "bodyplan/quadruped_small", "equipment/none", "state/patrolling"),
        Creature("surface_caravan_guard", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p05_06", "bodyplan/humanoid", "equipment/caravan_guard", "state/escort"),
        Creature("underground_cave_spider", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p00_06", "bodyplan/arachnid", "equipment/natural_hide", "state/ambush"),
        Creature("underground_lava_imp", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p04_07", "bodyplan/humanoid_small", "equipment/heat_charm", "state/guarding")
    ];

    private static IReadOnlyList<VisualRegionOverlay> BuildOverlays() =>
    [
        Overlay("surface_day_night_weather_sweep", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p02_04", "day_night_weather", "daynight/dusk", "weather/light_rain", "effect/road_lanterns"),
        Overlay("surface_ash_storm_effect", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p05_08", "weather", "daynight/noon", "weather/ash_storm", "effect/heat_haze"),
        Overlay("underground_glow_mushroom_effect", DeterministicVisualRegionComposerVocabulary.UndergroundLayerId, "underground_p00_06", "effect", "daynight/none", "weather/cave_humidity", "effect/bioluminescent_spores"),
        Overlay("adult_rating_metadata_only_safe_fallback_route", DeterministicVisualRegionComposerVocabulary.SurfaceLayerId, "surface_p02_03", "rating_fallback", "daynight/none", "weather/none", "effect/none", adult: true, fallback: "visual_safe_fallback/public_paperdoll_neutral", providerState: VisualRegionProviderState.CandidateQuarantine)
    ];

    private static VisualRegionRoadNode Node(string id, string layerId, string placementId, string role, bool required) =>
        new() { NodeId = id, LayerId = layerId, PlacementId = placementId, Role = role, RequiredAnchor = required };

    private static VisualRegionRoadEdge Edge(string id, string from, string to, string kind = "road") =>
        new() { EdgeId = id, FromNodeId = from, ToNodeId = to, EdgeKind = kind };

    private static VisualRegionSettlementPlacement Settlement(
        string id,
        string layerId,
        string placementId,
        string role,
        string terrain,
        string roadNodeId) =>
        new() { SettlementId = id, LayerId = layerId, PlacementId = placementId, Role = role, TerrainKind = terrain, RoadNodeId = roadNodeId };

    private static VisualRegionObjectPlacement ObjectPlacement(
        string id,
        string layerId,
        string placementId,
        string kind,
        string terrain,
        string roadNodeId,
        bool road,
        bool water = false,
        bool passable = true) =>
        new()
        {
            ObjectId = id,
            LayerId = layerId,
            PlacementId = placementId,
            ObjectKind = kind,
            TerrainKind = terrain,
            RoadNodeId = roadNodeId,
            RequiresRoadConnection = road,
            RequiresWaterAdjacency = water,
            RequiresPassableTerrain = passable
        };

    private static VisualRegionCreaturePlacement Creature(
        string id,
        string layerId,
        string placementId,
        string bodyPlan,
        string equipment,
        string state) =>
        new()
        {
            CreatureId = id,
            LayerId = layerId,
            PlacementId = placementId,
            BodyPlanId = bodyPlan,
            EquipmentProfileId = equipment,
            StateMetadataId = state,
            RatingSafe = true
        };

    private static VisualRegionOverlay Overlay(
        string id,
        string layerId,
        string placementId,
        string kind,
        string dayNight,
        string weather,
        string effect,
        bool adult = false,
        string fallback = "",
        VisualRegionProviderState providerState = VisualRegionProviderState.MetadataOnly) =>
        new()
        {
            OverlayId = id,
            LayerId = layerId,
            PlacementId = placementId,
            OverlayKind = kind,
            DayNightMetadata = dayNight,
            WeatherMetadata = weather,
            EffectMetadata = effect,
            AdultMetadataOnly = adult,
            SafeFallbackRefId = fallback,
            ProviderState = providerState,
            TreatProviderCandidateAsApprovedOutput = false
        };

    private static IReadOnlyList<VisualRegionSourceReference> SourceReferences() =>
    [
        Source("goal084", "visual_asset_contract_rating_metadata_report", ".llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md"),
        Source("goal085", "visual_part_pack_rule_stack_report", ".llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-rule-stack-report.md"),
        Source("goal086", "deterministic_visual_microtile_materializer_report", ".llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-materializer-report.md"),
        Source("goal087", "deterministic_visual_map_patch_composer_report", ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-composer-report.md"),
        Source("goal087", "visual_map_patch_catalog", ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-catalog.json"),
        Source("goal087", "visual_map_patch_materialization_manifest", ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-materialization-manifest.json"),
        Source("goal087", "visual_map_patch_water_flow_proof", ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-water-flow-proof.json"),
        Source("goal087", "visual_map_patch_reachability_proof", ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-reachability-proof.json"),
        Source("goal087", "visual_map_patch_negative_proof", ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-negative-proof.json"),
        Source("goal087", "visual_map_patch_quality_gate_scan", ".llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-quality-gate-scan.json"),
        Source("deepsearch", "visual_stack_synthesis", "docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md"),
        Source("deepsearch", "tile_biome_water_world_map_generation", "docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md"),
        Source("deepsearch", "pseudo3d_first_person_from_2d_assets", "docs/deepsearch/03_PSEUDO3D_FIRST_PERSON_FROM_2D_ASSETS.md"),
        Source("deepsearch", "settlements_cities_caravans_living_world_visuals", "docs/deepsearch/05_SETTLEMENTS_CITIES_CARAVANS_LIVING_WORLD_VISUALS.md")
    ];

    private static VisualRegionSourceReference Source(string kind, string id, string relativePath) =>
        new() { SourceKind = kind, SourceId = id, RelativePath = relativePath };
}
