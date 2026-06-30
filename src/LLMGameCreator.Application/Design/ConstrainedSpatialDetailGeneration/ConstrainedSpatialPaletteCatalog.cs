namespace LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;

public sealed class ConstrainedSpatialPaletteCatalogBuilder
{
    public ConstrainedSpatialPaletteCatalog Build()
    {
        var tiles = new List<ConstrainedSpatialTileDefinition>();
        tiles.AddRange(MapPanelRpgTiles());
        tiles.AddRange(SurvivalSandboxTiles());
        tiles.AddRange(FirstPersonGridDungeonTiles());

        var requiredFamiliesCovered = ConstrainedSpatialDetailVocabulary.FamilyIds
            .All(familyId => tiles.Any(tile => tile.FamilyApplicability.Contains(familyId, StringComparer.Ordinal)));
        var requiredSemanticsCovered = RequiredSemantics()
            .All(required => tiles.Any(tile =>
                tile.FamilyApplicability.Contains(required.FamilyId, StringComparer.Ordinal)
                && tile.SemanticTags.Contains(required.Tag, StringComparer.Ordinal)));

        return new ConstrainedSpatialPaletteCatalog
        {
            Passed = tiles.Count >= 22
                && requiredFamiliesCovered
                && requiredSemanticsCovered
                && tiles.All(tile => tile.Provenance == "in_house_fixture"),
            TileCount = tiles.Count,
            FamilyIds = ConstrainedSpatialDetailVocabulary.FamilyIds,
            Tiles = tiles.OrderBy(tile => ConstrainedSpatialDetailVocabulary.FamilyOrderingKey(tile.FamilyApplicability.First()), StringComparer.Ordinal)
                .ThenBy(tile => tile.TileId, StringComparer.Ordinal)
                .ToList()
        };
    }

    public static IReadOnlyDictionary<string, ConstrainedSpatialTileDefinition> TileById(ConstrainedSpatialPaletteCatalog catalog) =>
        catalog.Tiles.ToDictionary(item => item.TileId, StringComparer.Ordinal);

    private static IReadOnlyList<ConstrainedSpatialTileDefinition> MapPanelRpgTiles() =>
    [
        Tile("tile/map_panel_rpg/road", ["road", "corridor", "safe_path"], ["map_panel_rpg"], true, corridor: true, marker: "R", color: "#8c7650", adjacency: ["passable", "road"]),
        Tile("tile/map_panel_rpg/field", ["field", "biome"], ["map_panel_rpg"], true, biome: true, marker: "F", color: "#6da65a", adjacency: ["passable", "field"]),
        Tile("tile/map_panel_rpg/forest", ["forest", "biome"], ["map_panel_rpg"], false, biome: true, marker: "T", color: "#2d6b3b", adjacency: ["blocked", "forest"]),
        Tile("tile/map_panel_rpg/settlement", ["settlement", "landmark"], ["map_panel_rpg"], true, settlement: true, marker: "S", color: "#c49a62", adjacency: ["passable", "settlement"]),
        Tile("tile/map_panel_rpg/quest_marker", ["quest", "objective"], ["map_panel_rpg"], true, objective: true, marker: "Q", color: "#ffd166", adjacency: ["passable", "objective"]),
        Tile("tile/map_panel_rpg/npc_marker", ["npc", "landmark"], ["map_panel_rpg"], true, marker: "N", color: "#83c5be", adjacency: ["passable", "npc"]),
        Tile("tile/map_panel_rpg/item_marker", ["item", "resource"], ["map_panel_rpg"], true, resource: true, marker: "I", color: "#f28482", adjacency: ["passable", "item"]),
        Tile("tile/map_panel_rpg/exit", ["exit"], ["map_panel_rpg"], true, marker: "X", color: "#ffffff", adjacency: ["passable", "exit"]),
        Tile("tile/map_panel_rpg/entry", ["entry"], ["map_panel_rpg"], true, marker: "E", color: "#d9ed92", adjacency: ["passable", "entry"])
    ];

    private static IReadOnlyList<ConstrainedSpatialTileDefinition> SurvivalSandboxTiles() =>
    [
        Tile("tile/survival_sandbox/safe_path", ["safe_path", "corridor"], ["survival_sandbox"], true, corridor: true, marker: "P", color: "#d6d3a5", adjacency: ["passable", "safe_path"]),
        Tile("tile/survival_sandbox/shelter", ["shelter", "settlement"], ["survival_sandbox"], true, settlement: true, marker: "S", color: "#b08968", adjacency: ["passable", "shelter"]),
        Tile("tile/survival_sandbox/water", ["water", "resource"], ["survival_sandbox"], true, resource: true, marker: "W", color: "#4cc9f0", adjacency: ["passable", "water"]),
        Tile("tile/survival_sandbox/resource", ["resource"], ["survival_sandbox"], true, resource: true, marker: "R", color: "#95d5b2", adjacency: ["passable", "resource"]),
        Tile("tile/survival_sandbox/hazard", ["hazard", "unsafe"], ["survival_sandbox"], false, hazard: true, marker: "H", color: "#d00000", adjacency: ["blocked", "hazard"]),
        Tile("tile/survival_sandbox/weather_marker", ["weather", "biome"], ["survival_sandbox"], true, biome: true, marker: "M", color: "#adb5bd", adjacency: ["passable", "weather"]),
        Tile("tile/survival_sandbox/exit", ["exit"], ["survival_sandbox"], true, marker: "X", color: "#ffffff", adjacency: ["passable", "exit"]),
        Tile("tile/survival_sandbox/entry", ["entry"], ["survival_sandbox"], true, marker: "E", color: "#d9ed92", adjacency: ["passable", "entry"])
    ];

    private static IReadOnlyList<ConstrainedSpatialTileDefinition> FirstPersonGridDungeonTiles() =>
    [
        Tile("tile/first_person_grid_dungeon/wall", ["wall", "blocked"], ["first_person_grid_dungeon"], false, marker: "#", color: "#1f1f1f", adjacency: ["blocked", "wall"]),
        Tile("tile/first_person_grid_dungeon/floor", ["floor"], ["first_person_grid_dungeon"], true, marker: ".", color: "#6c757d", adjacency: ["passable", "floor"]),
        Tile("tile/first_person_grid_dungeon/corridor", ["corridor", "safe_path"], ["first_person_grid_dungeon"], true, corridor: true, marker: "C", color: "#8d99ae", adjacency: ["passable", "corridor"]),
        Tile("tile/first_person_grid_dungeon/door", ["door"], ["first_person_grid_dungeon"], true, door: true, marker: "D", color: "#bc6c25", adjacency: ["passable", "door"]),
        Tile("tile/first_person_grid_dungeon/encounter", ["encounter", "hazard"], ["first_person_grid_dungeon"], true, hazard: true, marker: "K", color: "#e63946", adjacency: ["passable", "encounter"]),
        Tile("tile/first_person_grid_dungeon/objective", ["objective"], ["first_person_grid_dungeon"], true, objective: true, marker: "O", color: "#ffd166", adjacency: ["passable", "objective"]),
        Tile("tile/first_person_grid_dungeon/exit", ["exit"], ["first_person_grid_dungeon"], true, marker: "X", color: "#ffffff", adjacency: ["passable", "exit"]),
        Tile("tile/first_person_grid_dungeon/entry", ["entry"], ["first_person_grid_dungeon"], true, marker: "E", color: "#d9ed92", adjacency: ["passable", "entry"])
    ];

    private static ConstrainedSpatialTileDefinition Tile(
        string id,
        IReadOnlyList<string> tags,
        IReadOnlyList<string> families,
        bool passable,
        bool hazard = false,
        bool resource = false,
        bool objective = false,
        bool door = false,
        bool corridor = false,
        bool settlement = false,
        bool biome = false,
        string marker = ".",
        string color = "#808080",
        IReadOnlyList<string>? adjacency = null) =>
        new()
        {
            TileId = id,
            SemanticTags = tags,
            FamilyApplicability = families,
            Passable = passable,
            Hazard = hazard,
            Resource = resource,
            Objective = objective,
            Door = door,
            Corridor = corridor,
            Settlement = settlement,
            Biome = biome,
            AdjacencyTags = adjacency ?? [],
            RenderMarker = marker,
            ThumbnailColor = color,
            Provenance = "in_house_fixture"
        };

    private static IReadOnlyList<(string FamilyId, string Tag)> RequiredSemantics() =>
    [
        ("map_panel_rpg", "road"),
        ("map_panel_rpg", "field"),
        ("map_panel_rpg", "forest"),
        ("map_panel_rpg", "settlement"),
        ("map_panel_rpg", "quest"),
        ("map_panel_rpg", "npc"),
        ("map_panel_rpg", "item"),
        ("map_panel_rpg", "exit"),
        ("survival_sandbox", "shelter"),
        ("survival_sandbox", "water"),
        ("survival_sandbox", "resource"),
        ("survival_sandbox", "hazard"),
        ("survival_sandbox", "safe_path"),
        ("survival_sandbox", "weather"),
        ("survival_sandbox", "exit"),
        ("first_person_grid_dungeon", "wall"),
        ("first_person_grid_dungeon", "floor"),
        ("first_person_grid_dungeon", "corridor"),
        ("first_person_grid_dungeon", "door"),
        ("first_person_grid_dungeon", "encounter"),
        ("first_person_grid_dungeon", "objective"),
        ("first_person_grid_dungeon", "exit")
    ];
}
