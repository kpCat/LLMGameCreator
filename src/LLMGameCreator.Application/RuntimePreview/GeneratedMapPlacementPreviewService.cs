using System.Text;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.RuntimePreview;

public sealed class GeneratedMapPlacementPreviewService
{
    public GeneratedMapPlacementPreviewModel Build(
        GamePackageDefinition package,
        GameState state,
        GeneratedPackageRuntimePreviewModel preview)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(preview);

        var markers = new List<GeneratedRuntimeMapMarker>();
        var occupiedByMap = new Dictionary<string, HashSet<(int X, int Y)>>(StringComparer.OrdinalIgnoreCase);

        for (var index = 0; index < package.GeneratedContent.Npcs.Count; index++)
        {
            var npc = package.GeneratedContent.Npcs[index];
            var sourceId = FirstNonEmpty(npc.SourceId, $"npc/{index + 1}");
            var resolution = ResolveMap(package, preview.CurrentMapId, npc.SceneId, npc.RegionId);
            var linkedDialogues = package.GeneratedContent.Dialogues
                .Where(dialogue => IdEquals(dialogue.NpcId, npc.SourceId))
                .ToList();
            var references = NonEmpty(npc.RegionId, npc.SceneId)
                .Concat(linkedDialogues.Select(dialogue => dialogue.SourceId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            markers.Add(CreateMarker(
                package,
                state,
                occupiedByMap,
                $"generated-marker/npc/{sourceId}",
                GeneratedRuntimeMapMarkerType.Npc,
                sourceId,
                FirstNonEmpty(npc.Name, sourceId),
                npc.Description,
                resolution,
                references,
                ("Region", npc.RegionId),
                ("Scene", npc.SceneId),
                ("Linked dialogues", Join(linkedDialogues.Select(dialogue =>
                    string.IsNullOrWhiteSpace(dialogue.Title)
                        ? dialogue.SourceId
                        : $"{dialogue.SourceId} ({dialogue.Title.Trim()})")))));
        }

        for (var index = 0; index < package.GeneratedContent.Encounters.Count; index++)
        {
            var encounter = package.GeneratedContent.Encounters[index];
            var sourceId = FirstNonEmpty(encounter.SourceId, $"encounter/{index + 1}");
            var resolution = ResolveMap(package, preview.CurrentMapId, encounter.SceneId, encounter.RegionId);
            var references = NonEmpty(encounter.RegionId, encounter.SceneId)
                .Concat(encounter.NpcIds.Where(NotBlank).Select(Trim))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            markers.Add(CreateMarker(
                package,
                state,
                occupiedByMap,
                $"generated-marker/encounter/{sourceId}",
                GeneratedRuntimeMapMarkerType.Encounter,
                sourceId,
                FirstNonEmpty(encounter.Title, sourceId),
                encounter.Description,
                resolution,
                references,
                ("Region", encounter.RegionId),
                ("Scene", encounter.SceneId),
                ("Participants", Join(encounter.NpcIds))));
        }

        return new GeneratedMapPlacementPreviewModel
        {
            Markers = markers,
            Warnings = markers
                .Where(marker => !string.IsNullOrWhiteSpace(marker.Warning))
                .Select(marker => marker.Warning)
                .Distinct(StringComparer.Ordinal)
                .ToList()
        };
    }

    private static GeneratedRuntimeMapMarker CreateMarker(
        GamePackageDefinition package,
        GameState state,
        IDictionary<string, HashSet<(int X, int Y)>> occupiedByMap,
        string markerId,
        GeneratedRuntimeMapMarkerType type,
        string sourceId,
        string title,
        string description,
        MapResolution resolution,
        IReadOnlyList<string> references,
        params (string Label, string Value)[] extraDetails)
    {
        var position = ResolvePosition(package, state, resolution.MapId, markerId, occupiedByMap, out var positionWarning);
        var warning = JoinWarnings(resolution.Warning, positionWarning);
        var builder = new StringBuilder();
        AppendDetail(builder, "Type", type == GeneratedRuntimeMapMarkerType.Npc ? "NPC" : "Encounter");
        AppendDetail(builder, "Source id", sourceId);
        AppendDetail(builder, "Title", title);
        AppendDetail(builder, "Description", description);
        AppendDetail(builder, "Map id", resolution.MapId);
        AppendDetail(builder, "Position", $"{position.X}, {position.Y}");
        foreach (var detail in extraDetails)
        {
            AppendDetail(builder, detail.Label, detail.Value);
        }

        AppendDetail(builder, "References", Join(references));
        AppendDetail(builder, "Warning", warning);

        return new GeneratedRuntimeMapMarker
        {
            MarkerId = markerId,
            Type = type,
            SourceId = sourceId,
            Title = title.Trim(),
            Description = description.Trim(),
            MapId = resolution.MapId,
            Position = position,
            ReferenceIds = references,
            DetailsText = builder.ToString().TrimEnd(),
            Warning = warning
        };
    }

    private static MapResolution ResolveMap(
        GamePackageDefinition package,
        string currentMapId,
        string sceneId,
        string regionId)
    {
        var directScene = package.GeneratedContent.Scenes.FirstOrDefault(scene => IdEquals(scene.SourceId, sceneId));
        var directMapId = ValidMapId(package, directScene?.PackageMapId);
        if (!string.IsNullOrWhiteSpace(directMapId))
        {
            return new MapResolution(directMapId, string.Empty);
        }

        var region = package.GeneratedContent.Regions.FirstOrDefault(candidate => IdEquals(candidate.SourceId, regionId));
        foreach (var linkedSceneId in region?.SceneIds ?? Enumerable.Empty<string>())
        {
            var linkedScene = package.GeneratedContent.Scenes.FirstOrDefault(scene => IdEquals(scene.SourceId, linkedSceneId));
            var linkedMapId = ValidMapId(package, linkedScene?.PackageMapId);
            if (!string.IsNullOrWhiteSpace(linkedMapId))
            {
                return new MapResolution(linkedMapId, $"Scene '{sceneId}' was not mapped; used region scene '{linkedSceneId}'.");
            }
        }

        var fallbackMapId = FirstNonEmpty(
            ValidMapId(package, currentMapId),
            ValidMapId(package, package.Manifest.StartMapId),
            package.Game.Maps.FirstOrDefault()?.Id);
        return new MapResolution(
            fallbackMapId,
            $"Generated references could not resolve a map; used fallback map '{fallbackMapId}'.");
    }

    private static Position2D ResolvePosition(
        GamePackageDefinition package,
        GameState state,
        string mapId,
        string stableId,
        IDictionary<string, HashSet<(int X, int Y)>> occupiedByMap,
        out string warning)
    {
        var map = package.Game.Maps.FirstOrDefault(candidate => IdEquals(candidate.Id, mapId));
        if (map == null || map.Width <= 0 || map.Height <= 0)
        {
            warning = $"Map '{mapId}' has no usable bounds; marker uses 0, 0.";
            return new Position2D();
        }

        if (!occupiedByMap.TryGetValue(map.Id, out var occupied))
        {
            occupied = new HashSet<(int X, int Y)> { (map.StartPosition.X, map.StartPosition.Y) };
            if (IdEquals(state.CurrentMapId, map.Id))
            {
                occupied.Add((state.PlayerPosition.X, state.PlayerPosition.Y));
            }

            occupiedByMap.Add(map.Id, occupied);
        }

        var cellCount = map.Width * map.Height;
        var startIndex = (int)(StableHash(stableId) % (uint)cellCount);
        var passable = FindCandidate(package, map, occupied, startIndex, requirePassable: true);
        var candidate = passable ?? FindCandidate(package, map, occupied, startIndex, requirePassable: false);
        if (candidate == null)
        {
            candidate = new Position2D(startIndex % map.Width, startIndex / map.Width);
            warning = $"No free tile remained on map '{map.Id}'; marker overlap was unavoidable.";
        }
        else
        {
            warning = passable == null
                ? $"No free passable tile was found on map '{map.Id}'; used a free in-bounds tile."
                : string.Empty;
        }

        occupied.Add((candidate.X, candidate.Y));
        return candidate;
    }

    private static Position2D? FindCandidate(
        GamePackageDefinition package,
        MapDefinition map,
        ISet<(int X, int Y)> occupied,
        int startIndex,
        bool requirePassable)
    {
        var cellCount = map.Width * map.Height;
        for (var offset = 0; offset < cellCount; offset++)
        {
            var index = (startIndex + offset) % cellCount;
            var x = index % map.Width;
            var y = index / map.Width;
            if (occupied.Contains((x, y)))
            {
                continue;
            }

            if (!requirePassable || IsPassable(package, map, x, y))
            {
                return new Position2D(x, y);
            }
        }

        return null;
    }

    private static bool IsPassable(GamePackageDefinition package, MapDefinition map, int x, int y)
    {
        var tileId = map.Tiles.LastOrDefault(tile => tile.X == x && tile.Y == y)?.TileId ?? map.DefaultTileId;
        return package.Game.TilePrototypes.FirstOrDefault(tile => IdEquals(tile.Id, tileId))?.Walkable == true;
    }

    private static string ValidMapId(GamePackageDefinition package, string? mapId)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return string.Empty;
        }

        var map = package.Game.Maps.FirstOrDefault(candidate => IdEquals(candidate.Id, mapId));
        return map?.Id.Trim() ?? string.Empty;
    }

    private static uint StableHash(string value)
    {
        const uint offsetBasis = 2166136261;
        const uint prime = 16777619;
        var hash = offsetBasis;
        foreach (var character in value)
        {
            hash ^= character;
            hash *= prime;
        }

        return hash;
    }

    private static void AppendDetail(StringBuilder builder, string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            builder.Append(label).Append(": ").AppendLine(value.Trim());
        }
    }

    private static string Join(IEnumerable<string> values) =>
        string.Join(", ", values.Where(NotBlank).Select(Trim));

    private static IReadOnlyList<string> NonEmpty(params string[] values) =>
        values.Where(NotBlank).Select(Trim).ToList();

    private static string JoinWarnings(params string[] warnings) =>
        string.Join(" ", warnings.Where(NotBlank).Select(Trim));

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static bool IdEquals(string? left, string? right) =>
        !string.IsNullOrWhiteSpace(left)
        && !string.IsNullOrWhiteSpace(right)
        && string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool NotBlank(string? value) => !string.IsNullOrWhiteSpace(value);

    private static string Trim(string value) => value.Trim();

    private sealed record MapResolution(string MapId, string Warning);
}

public enum GeneratedRuntimeMapMarkerType
{
    Npc,
    Encounter
}

public sealed record GeneratedRuntimeMapMarker
{
    public string MarkerId { get; init; } = string.Empty;
    public GeneratedRuntimeMapMarkerType Type { get; init; }
    public string SourceId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string MapId { get; init; } = string.Empty;
    public Position2D Position { get; init; } = new();
    public IReadOnlyList<string> ReferenceIds { get; init; } = Array.Empty<string>();
    public string DetailsText { get; init; } = string.Empty;
    public string Warning { get; init; } = string.Empty;
}

public sealed record GeneratedMapPlacementPreviewModel
{
    public IReadOnlyList<GeneratedRuntimeMapMarker> Markers { get; init; } = Array.Empty<GeneratedRuntimeMapMarker>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
