using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public sealed record GeneratedWorldTravelRecordFingerprint
{
    public string CollectionPath { get; init; } = string.Empty;
    public string RecordId { get; init; } = string.Empty;
    public string CanonicalSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedWorldTravelGateFingerprint
{
    public string ConnectionId { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string SourceMapId { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
    public int ApproachX { get; init; }
    public int ApproachY { get; init; }
    public string CanonicalSha256 { get; init; } = string.Empty;
}

public sealed record GeneratedWorldTravelOverlayDocument
{
    public string SchemaVersion { get; init; } = "generated_world_travel_overlay_v1";
    public string SourceRequestSha256 { get; init; } = string.Empty;
    public string PlanSha256 { get; init; } = string.Empty;
    public string CompatibilityPackageSha256 { get; init; } = string.Empty;
    public string TravelOverlaySha256 { get; init; } = string.Empty;
    public string PlayerCompositionPackageSha256 { get; init; } = string.Empty;
    public int RegionBindingCount { get; init; }
    public int ConnectionCount { get; init; }
    public int GateCount { get; init; }
    public bool GatePlacementPassed { get; init; }
    public bool ControlledDeltaPassed { get; init; }
    public GeneratedWorldTravelRecordFingerprint PrototypeFingerprint { get; init; } = new();
    public IReadOnlyList<GeneratedWorldTravelGateFingerprint> GateFingerprints { get; init; } = [];
    public IReadOnlyList<GeneratedWorldTravelRecordFingerprint> PreTravelRecords { get; init; } = [];
    public IReadOnlyList<GeneratedWorldTravelRecordFingerprint> MapFingerprintsBefore { get; init; } = [];
    public IReadOnlyList<GeneratedWorldTravelRecordFingerprint> MapFingerprintsAfter { get; init; } = [];
}

public sealed record GeneratedWorldTravelOverlayResult
{
    public bool Passed { get; init; }
    public GeneratedWorldTravelOverlayDocument Document { get; init; } = new();
    public GeneratedWorldRegionMapBindingResult Binding { get; init; } = new();
    public GamePackageDefinition TravelOverlayPackage { get; init; } = new();
    public GamePackageDefinition PlayerCompositionPackage { get; init; } = new();
    public string TravelOverlayPackageJson { get; init; } = string.Empty;
    public string PlayerCompositionPackageJson { get; init; } = string.Empty;
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class GeneratedWorldTravelOverlayService
{
    public const string TravelPrototypeId = "entity_prototype/generated_region_travel_gate";
    public const string TravelEntityIdPrefix = "entity/generated_travel_gate/";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly (int X, int Y)[] AdjacentDirections =
    [
        (0, -1),
        (-1, 0),
        (1, 0),
        (0, 1)
    ];

    private readonly GeneratedWorldRegionMapBindingService _bindingService;

    public GeneratedWorldTravelOverlayService(GeneratedWorldRegionMapBindingService? bindingService = null)
    {
        _bindingService = bindingService ?? new GeneratedWorldRegionMapBindingService();
    }

    public GeneratedWorldTravelOverlayResult Build(
        SeededGeneratedProjectSourceValidationResult source,
        GamePackageDefinition compatibilityPackage)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(compatibilityPackage);
        var binding = _bindingService.Bind(source, compatibilityPackage);
        if (!binding.Passed || source.Source is null)
            return Failed(binding, binding.Diagnostics.Count > 0
                ? binding.Diagnostics
                : ["generated_travel.binding_failed"]);

        var diagnostics = new List<string>();
        var before = Clone(compatibilityPackage);
        var travel = Clone(compatibilityPackage);
        var expectedPrototype = new EntityPrototypeDefinition
        {
            Id = TravelPrototypeId,
            Name = "Generated Region Travel Gate"
        };
        var existingPrototypes = travel.Game.EntityPrototypes
            .Where(item => string.Equals(item.Id, TravelPrototypeId, StringComparison.Ordinal)).ToList();
        if (existingPrototypes.Count > 1
            || existingPrototypes.Count == 1 && !CanonicalEqual(existingPrototypes[0], expectedPrototype))
            diagnostics.Add("generated_travel.id_collision:" + TravelPrototypeId);
        else if (existingPrototypes.Count == 0)
            travel.Game.EntityPrototypes.Add(expectedPrototype);

        var gateFingerprints = new List<GeneratedWorldTravelGateFingerprint>();
        foreach (var sourceMapGroup in binding.ConnectionBindings
                     .GroupBy(item => item.SourceMapId, StringComparer.Ordinal)
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var map = travel.Game.Maps.Single(item => item.Id == sourceMapGroup.Key);
            var connections = sourceMapGroup.OrderBy(item => item.ConnectionId, StringComparer.Ordinal).ToList();
            var placements = PlaceGates(travel, map, connections.Count);
            if (placements.Count != connections.Count)
            {
                diagnostics.Add("generated_travel.gate_placement_insufficient:" + map.Id);
                continue;
            }

            for (var index = 0; index < connections.Count; index++)
            {
                var connection = connections[index];
                var destination = binding.RegionBindings.Single(item => item.RegionId == connection.ToRegionId);
                var destinationMap = travel.Game.Maps.Single(item => item.Id == connection.DestinationMapId);
                var entity = GateEntity(connection, destination, destinationMap, placements[index].Gate);
                var collisions = travel.Game.Maps.SelectMany(item => item.Entities)
                    .Where(item => string.Equals(item.Id, entity.Id, StringComparison.Ordinal)).ToList();
                if (collisions.Count > 0)
                {
                    if (collisions.Count != 1 || !CanonicalEqual(collisions[0], entity)
                                              || !map.Entities.Contains(collisions[0]))
                        diagnostics.Add("generated_travel.id_collision:" + entity.Id);
                }
                else
                {
                    map.Entities.Add(entity);
                }

                gateFingerprints.Add(new GeneratedWorldTravelGateFingerprint
                {
                    ConnectionId = connection.ConnectionId,
                    EntityId = entity.Id,
                    SourceMapId = connection.SourceMapId,
                    X = placements[index].Gate.X,
                    Y = placements[index].Gate.Y,
                    ApproachX = placements[index].Approach.X,
                    ApproachY = placements[index].Approach.Y,
                    CanonicalSha256 = Hash(Canonical(entity))
                });
            }
        }
        var player = Clone(travel);
        player.Manifest.StartMapId = source.Source.GeneratedStartMapId;
        var stripped = Clone(player);
        stripped.Manifest.StartMapId = before.Manifest.StartMapId;
        stripped.Game.EntityPrototypes.RemoveAll(item => item.Id == TravelPrototypeId);
        foreach (var map in stripped.Game.Maps)
            map.Entities.RemoveAll(item => item.Id.StartsWith(TravelEntityIdPrefix, StringComparison.Ordinal));
        var controlledDelta = CanonicalEqual(before, stripped);
        if (!controlledDelta) diagnostics.Add("generated_travel.unexpected_package_delta");
        if (gateFingerprints.Count != binding.ConnectionBindings.Count)
            diagnostics.Add("generated_travel.gate_count_mismatch");

        diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal).ToList();
        var travelJson = Canonical(travel) + Environment.NewLine;
        var playerJson = Canonical(player) + Environment.NewLine;
        var prototypeFingerprint = new GeneratedWorldTravelRecordFingerprint
        {
            CollectionPath = "game.entityPrototypes",
            RecordId = TravelPrototypeId,
            CanonicalSha256 = Hash(Canonical(expectedPrototype))
        };
        var mapsBefore = FingerprintMaps(before.Game.Maps);
        var mapsAfter = FingerprintMaps(travel.Game.Maps);
        var overlayHash = Hash(Canonical(new
        {
            source.Source.PlanSha256,
            compatibilityPackageSha256 = Hash(Canonical(before) + Environment.NewLine),
            prototypeFingerprint,
            gateFingerprints,
            mapsBefore,
            mapsAfter
        }));
        var document = new GeneratedWorldTravelOverlayDocument
        {
            SourceRequestSha256 = Hash(string.Join("\n", new[]
            {
                source.Source.Seed,
                source.Source.Mode,
                source.Source.PresetId,
                string.Join("|", source.Source.StyleHintIds.OrderBy(value => value, StringComparer.Ordinal)),
                string.Join("|", source.Source.VariantIds.OrderBy(value => value, StringComparer.Ordinal))
            })),
            PlanSha256 = source.Source.PlanSha256,
            CompatibilityPackageSha256 = Hash(Canonical(before) + Environment.NewLine),
            TravelOverlaySha256 = overlayHash,
            PlayerCompositionPackageSha256 = Hash(playerJson),
            RegionBindingCount = binding.RegionBindings.Count,
            ConnectionCount = binding.ConnectionBindings.Count,
            GateCount = gateFingerprints.Count,
            GatePlacementPassed = gateFingerprints.Count == binding.ConnectionBindings.Count,
            ControlledDeltaPassed = controlledDelta,
            PrototypeFingerprint = prototypeFingerprint,
            GateFingerprints = gateFingerprints.OrderBy(item => item.ConnectionId, StringComparer.Ordinal).ToList(),
            PreTravelRecords = FingerprintRecords(before),
            MapFingerprintsBefore = mapsBefore,
            MapFingerprintsAfter = mapsAfter
        };
        return new GeneratedWorldTravelOverlayResult
        {
            Passed = diagnostics.Count == 0,
            Document = document,
            Binding = binding,
            TravelOverlayPackage = travel,
            PlayerCompositionPackage = player,
            TravelOverlayPackageJson = travelJson,
            PlayerCompositionPackageJson = playerJson,
            Diagnostics = diagnostics
        };
    }

    public static string GateEntityId(string connectionId) =>
        TravelEntityIdPrefix + Hash(connectionId)[..16];

    private static EntityInstanceDefinition GateEntity(
        GeneratedWorldTravelConnectionBinding connection,
        GeneratedWorldRegionMapBinding destination,
        MapDefinition destinationMap,
        Cell gate) => new()
    {
        Id = GateEntityId(connection.ConnectionId),
        PrototypeId = TravelPrototypeId,
        Position = new Position2D(gate.X, gate.Y),
        Components =
        [
            new ComponentDefinition
            {
                Type = MapTransitionInteractionContract.ComponentType,
                Args = new SortedDictionary<string, string>(StringComparer.Ordinal)
                {
                    [MapTransitionInteractionContract.TransitionKindKey] = MapTransitionInteractionContract.TransitionKindMap,
                    [MapTransitionInteractionContract.ConnectionIdKey] = connection.ConnectionId,
                    [MapTransitionInteractionContract.SourceMapIdKey] = connection.SourceMapId,
                    [MapTransitionInteractionContract.DestinationMapIdKey] = connection.DestinationMapId,
                    [MapTransitionInteractionContract.DestinationXKey] = destinationMap.StartPosition.X.ToString(CultureInfo.InvariantCulture),
                    [MapTransitionInteractionContract.DestinationYKey] = destinationMap.StartPosition.Y.ToString(CultureInfo.InvariantCulture),
                    [MapTransitionInteractionContract.FromRegionIdKey] = connection.FromRegionId,
                    [MapTransitionInteractionContract.ToRegionIdKey] = connection.ToRegionId,
                    ["text"] = "Переход: " + destination.RegionTitle
                }.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal)
            }
        ]
    };

    private static IReadOnlyList<Placement> PlaceGates(
        GamePackageDefinition package,
        MapDefinition map,
        int count)
    {
        if (count == 0) return [];
        var existingPositions = map.Entities.Select(item => new Cell(item.Position.X, item.Position.Y))
            .ToHashSet();
        var existingInteractables = map.Entities.Where(item => IsInteractable(package, item))
            .Select(item => new Cell(item.Position.X, item.Position.Y)).ToHashSet();
        var collidable = map.Entities.Where(item => HasComponent(package, item, "collidable"))
            .Select(item => new Cell(item.Position.X, item.Position.Y)).ToHashSet();
        var options = new List<Placement>();
        for (var y = 0; y < map.Height; y++)
        for (var x = 0; x < map.Width; x++)
        {
            var gate = new Cell(x, y);
            if (gate == new Cell(map.StartPosition.X, map.StartPosition.Y)
                || existingPositions.Contains(gate) || !Walkable(package, map, gate)) continue;
            foreach (var direction in AdjacentDirections)
            {
                var approach = new Cell(x + direction.X, y + direction.Y);
                if (!Inside(map, approach) || !Walkable(package, map, approach)
                    || existingPositions.Contains(approach) || collidable.Contains(approach)
                    || existingInteractables.Any(position => Adjacent(position, approach))
                    || !Reachable(package, map, new Cell(map.StartPosition.X, map.StartPosition.Y), approach, collidable))
                    continue;
                options.Add(new Placement(gate, approach));
            }
        }

        var selected = new List<Placement>();
        return Select(0) ? selected.ToList() : [];

        bool Select(int index)
        {
            if (index == count) return true;
            foreach (var option in options)
            {
                if (selected.Any(item => item.Gate == option.Gate
                                         || item.Approach == option.Approach
                                         || item.Gate == option.Approach
                                         || item.Approach == option.Gate
                                         || Adjacent(item.Gate, option.Approach)
                                         || Adjacent(option.Gate, item.Approach)))
                    continue;
                selected.Add(option);
                if (Select(index + 1)) return true;
                selected.RemoveAt(selected.Count - 1);
            }
            return false;
        }
    }

    private static bool Reachable(
        GamePackageDefinition package,
        MapDefinition map,
        Cell start,
        Cell destination,
        IReadOnlySet<Cell> blocked)
    {
        var visited = new HashSet<Cell> { start };
        var queue = new Queue<Cell>();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == destination) return true;
            foreach (var direction in AdjacentDirections)
            {
                var next = new Cell(current.X + direction.X, current.Y + direction.Y);
                if (!Inside(map, next) || !Walkable(package, map, next)
                    || blocked.Contains(next) || !visited.Add(next)) continue;
                queue.Enqueue(next);
            }
        }
        return false;
    }

    private static bool Walkable(GamePackageDefinition package, MapDefinition map, Cell cell)
    {
        var tileId = map.Tiles.FirstOrDefault(item => item.X == cell.X && item.Y == cell.Y)?.TileId
                     ?? map.DefaultTileId;
        return package.Game.TilePrototypes.Any(item => item.Id == tileId && item.Walkable);
    }

    private static bool IsInteractable(GamePackageDefinition package, EntityInstanceDefinition entity) =>
        HasComponent(package, entity, MapTransitionInteractionContract.ComponentType);

    private static bool HasComponent(
        GamePackageDefinition package,
        EntityInstanceDefinition entity,
        string type) => entity.Components.Any(item => item.Type == type)
                        || package.Game.EntityPrototypes.Any(prototype => prototype.Id == entity.PrototypeId
                            && prototype.Components.Any(item => item.Type == type));

    private static bool Inside(MapDefinition map, Cell cell) =>
        cell.X >= 0 && cell.Y >= 0 && cell.X < map.Width && cell.Y < map.Height;

    private static bool Adjacent(Cell left, Cell right) =>
        Math.Abs(left.X - right.X) + Math.Abs(left.Y - right.Y) == 1;

    private static IReadOnlyList<GeneratedWorldTravelRecordFingerprint> FingerprintMaps(
        IEnumerable<MapDefinition> maps) => maps.OrderBy(item => item.Id, StringComparer.Ordinal)
        .Select(item => Fingerprint("game.maps", item.Id, item)).ToList();

    private static IReadOnlyList<GeneratedWorldTravelRecordFingerprint> FingerprintRecords(
        GamePackageDefinition package)
    {
        using var document = JsonDocument.Parse(Canonical(package));
        var result = new List<GeneratedWorldTravelRecordFingerprint>();
        foreach (var parentName in new[] { "game", "assetCatalog", "scriptCatalog", "generatedContent" })
        {
            if (!document.RootElement.TryGetProperty(parentName, out var parent)) continue;
            foreach (var collection in parent.EnumerateObject().Where(item => item.Value.ValueKind == JsonValueKind.Array))
            foreach (var record in collection.Value.EnumerateArray())
            {
                var id = RecordId(record);
                if (string.IsNullOrWhiteSpace(id)) continue;
                result.Add(new GeneratedWorldTravelRecordFingerprint
                {
                    CollectionPath = parentName + "." + collection.Name,
                    RecordId = id,
                    CanonicalSha256 = Hash(record.GetRawText())
                });
            }
        }
        return result.OrderBy(item => item.CollectionPath, StringComparer.Ordinal)
            .ThenBy(item => item.RecordId, StringComparer.Ordinal).ToList();
    }

    private static string RecordId(JsonElement record)
    {
        foreach (var name in new[] { "id", "sourceId", "artifactId" })
            if (record.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                return value.GetString() ?? string.Empty;
        return string.Empty;
    }

    private static GeneratedWorldTravelRecordFingerprint Fingerprint(string path, string id, object value) => new()
    {
        CollectionPath = path,
        RecordId = id,
        CanonicalSha256 = Hash(Canonical(value))
    };

    private static T Clone<T>(T value) =>
        JsonSerializer.Deserialize<T>(Canonical(value), JsonOptions)
        ?? throw new InvalidOperationException("generated_travel.clone_failed");

    private static bool CanonicalEqual<T>(T left, T right) =>
        string.Equals(Canonical(left), Canonical(right), StringComparison.Ordinal);

    private static string Canonical<T>(T value) => JsonSerializer.Serialize(value, JsonOptions);

    internal static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static GeneratedWorldTravelOverlayResult Failed(
        GeneratedWorldRegionMapBindingResult binding,
        IReadOnlyList<string> diagnostics) => new()
        {
            Passed = false,
            Binding = binding,
            Diagnostics = diagnostics.Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToList()
        };

    private readonly record struct Cell(int X, int Y);
    private readonly record struct Placement(Cell Gate, Cell Approach);
}
