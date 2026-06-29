namespace LLMGameCreator.Application.Design.WorldScaleRegionMapFoundation;

public sealed class FiniteMapPackBuilder
{
    public IReadOnlyDictionary<string, WorldScaleFiniteMapPack> BuildMapPacksByFileName(IReadOnlyList<WorldScaleRegionGraph> graphs) =>
        graphs
            .OrderBy(item => item.ScenarioId, StringComparer.Ordinal)
            .Select(Build)
            .ToDictionary(item => FileName(item.ScenarioId), item => item, StringComparer.Ordinal);

    public WorldScaleFiniteMapPack Build(WorldScaleRegionGraph graph)
    {
        var coordinateKind = graph.ScenarioId is "gothic_intrigue" or "metamodule_kingdoms" ? "axial_hex" : "square";
        var mapId = $"finite-map/{graph.ScenarioId}/world-scale-preview";
        var anchors = BuildAnchors(graph, coordinateKind);
        var terrainPatches = graph.Regions
            .OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .Select((region, index) => new WorldScaleTerrainPatchSummary
            {
                PatchId = $"patch/{graph.ScenarioId}/{index + 1:000}",
                RegionId = region.RegionId,
                TerrainTags = region.TerrainTags,
                AnchorCell = anchors[region.RegionId],
                SummaryCellCount = coordinateKind == "axial_hex" ? 7 : 9
            })
            .ToList();
        var patchByRegion = terrainPatches.ToDictionary(item => item.RegionId, item => item.PatchId, StringComparer.Ordinal);
        var bindings = graph.Regions
            .OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .Select(region => new WorldScaleRegionMapBinding
            {
                RegionId = region.RegionId,
                MapId = mapId,
                PatchId = patchByRegion[region.RegionId],
                AnchorCell = anchors[region.RegionId]
            })
            .ToList();

        var routeSummaries = graph.TravelEdges
            .OrderBy(item => item.EdgeId, StringComparer.Ordinal)
            .Select(edge => new WorldScaleRouteCellSummary
            {
                EdgeId = edge.EdgeId,
                RouteKind = edge.RouteKind,
                FromRegionId = edge.FromRegionId,
                ToRegionId = edge.ToRegionId,
                RouteCellAnchors = [anchors.GetValueOrDefault(edge.FromRegionId, "missing"), MidCell(edge.EdgeId, coordinateKind), anchors.GetValueOrDefault(edge.ToRegionId, "missing")],
                RouteRegionBindingIds = [edge.FromRegionId, edge.ToRegionId]
            })
            .ToList();
        var landmarks = graph.Regions
            .SelectMany(region => region.LandmarkIds.Select((landmark, index) => new WorldScaleLandmarkPlacement
            {
                LandmarkId = landmark,
                RegionId = region.RegionId,
                Cell = index == 0 ? anchors[region.RegionId] : $"{anchors[region.RegionId]}+{index}",
                PlacementTags = region.RequiredGameplayTarget ? ["required_target", "landmark"] : ["landmark"]
            }))
            .OrderBy(item => item.LandmarkId, StringComparer.Ordinal)
            .ToList();
        var hooks = graph.Regions
            .Where(item => item.RequiredGameplayTarget || item.OptionalTarget)
            .OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .SelectMany(region => new[]
            {
                new WorldScaleHookPlacementSummary
                {
                    HookId = $"hook/{graph.ScenarioId}/quest/{StableSuffix(region.RegionId)}",
                    HookKind = region.RequiredGameplayTarget ? "required_quest_target" : "optional_quest_target",
                    RegionId = region.RegionId,
                    Cell = anchors[region.RegionId]
                },
                new WorldScaleHookPlacementSummary
                {
                    HookId = $"hook/{graph.ScenarioId}/encounter/{StableSuffix(region.RegionId)}",
                    HookKind = region.RequiredGameplayTarget ? "required_encounter_target" : "optional_encounter_target",
                    RegionId = region.RegionId,
                    Cell = anchors[region.RegionId]
                }
            })
            .OrderBy(item => item.HookId, StringComparer.Ordinal)
            .ToList();
        var traversableRoutes = graph.TravelEdges.Where(item => item.IsTraversableNow).Select(item => item.EdgeId).Order(StringComparer.Ordinal).ToList();
        var blockedRoutes = graph.TravelEdges.Where(item => !item.IsTraversableNow).Select(item => item.EdgeId).Order(StringComparer.Ordinal).ToList();

        return new WorldScaleFiniteMapPack
        {
            ScenarioId = graph.ScenarioId,
            ProfileId = graph.ProfileId,
            WorldGraphId = graph.WorldGraphId,
            MapId = mapId,
            CoordinateKind = coordinateKind,
            Width = coordinateKind == "square" ? Math.Max(12, graph.Regions.Count * 3) : 0,
            Height = coordinateKind == "square" ? Math.Max(10, graph.Regions.Count * 2) : 0,
            Radius = coordinateKind == "axial_hex" ? Math.Max(4, graph.Regions.Count / 2) : 0,
            MinQ = coordinateKind == "axial_hex" ? -Math.Max(4, graph.Regions.Count / 2) : 0,
            MaxQ = coordinateKind == "axial_hex" ? Math.Max(4, graph.Regions.Count / 2) : 0,
            MinR = coordinateKind == "axial_hex" ? -Math.Max(4, graph.Regions.Count / 2) : 0,
            MaxR = coordinateKind == "axial_hex" ? Math.Max(4, graph.Regions.Count / 2) : 0,
            DeterministicSeed = $"{graph.DeterministicSeed}/finite-map-pack",
            TerrainPatches = terrainPatches,
            PassabilitySummary = new WorldScalePassabilitySummary
            {
                PassablePatchCount = terrainPatches.Count,
                HazardPatchCount = graph.Regions.Count(item => item.HazardTags.Count > 0),
                BlockedRouteIds = blockedRoutes,
                TraversableRouteIds = traversableRoutes
            },
            LandmarkPlacements = landmarks,
            RegionBindings = bindings,
            RouteSummaries = routeSummaries,
            HookPlacements = hooks,
            ValidationTrace =
            [
                $"regions_bound={bindings.Count}",
                $"landmarks_placed={landmarks.Count}",
                $"routes_summarized={routeSummaries.Count}",
                $"required_targets={graph.RequiredTargetRegionIds.Count}",
                "tile_array_dump=false"
            ],
            PreviewCells = BuildPreviewCells(graph, anchors),
            AttemptedTileArrayCellCount = 0
        };
    }

    public static string FileName(string scenarioId) =>
        scenarioId switch
        {
            "frontier_survival" => "finite-map-pack-frontier.json",
            "gothic_intrigue" => "finite-map-pack-gothic.json",
            "caravan_trade" => "finite-map-pack-caravan.json",
            "metamodule_kingdoms" => "finite-map-pack-metamodule-kingdoms.json",
            _ => $"finite-map-pack-{scenarioId}.json"
        };

    private static IReadOnlyDictionary<string, string> BuildAnchors(WorldScaleRegionGraph graph, string coordinateKind)
    {
        var anchors = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var ordered = graph.Regions.OrderBy(item => item.RegionId, StringComparer.Ordinal).ToList();
        for (var index = 0; index < ordered.Count; index++)
        {
            var region = ordered[index];
            anchors[region.RegionId] = coordinateKind == "axial_hex"
                ? $"q{index - ordered.Count / 2}:r{(index * 2 % Math.Max(3, ordered.Count)) - ordered.Count / 3}"
                : $"x{2 + index * 3}:y{2 + index * 2}";
        }

        return anchors;
    }

    private static IReadOnlyList<string> BuildPreviewCells(WorldScaleRegionGraph graph, IReadOnlyDictionary<string, string> anchors) =>
        graph.Regions
            .OrderBy(item => item.RegionId, StringComparer.Ordinal)
            .Take(12)
            .Select(region => $"{anchors[region.RegionId]}={StableSuffix(region.RegionId)}")
            .ToList();

    private static string MidCell(string edgeId, string coordinateKind)
    {
        var hash = WorldScaleRegionMapCatalog.ComputeHash(edgeId);
        var first = Convert.ToInt32(hash[..2], 16) % 9;
        var second = Convert.ToInt32(hash[2..4], 16) % 9;
        return coordinateKind == "axial_hex" ? $"q{first - 4}:r{second - 4}" : $"x{first + 1}:y{second + 1}";
    }

    private static string StableSuffix(string value)
    {
        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? "unknown" : parts[^1];
    }
}
