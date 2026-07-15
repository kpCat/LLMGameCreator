using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;

namespace LLMGameCreator.Application.Generation.Procedural;

public enum GeneratedWorldTravelPlannedActionKind
{
    Move = 0,
    OriginInteraction = 1,
    GateInteraction = 2,
    DestinationInteraction = 3
}

public sealed record GeneratedWorldTravelPlannedAction
{
    public GeneratedWorldTravelPlannedActionKind Kind { get; init; }
    public PlayerCommand Command { get; init; } = new();
    public string ExpectedMapId { get; init; } = string.Empty;
    public int ExpectedX { get; init; }
    public int ExpectedY { get; init; }
    public string TargetEntityId { get; init; } = string.Empty;
    public string ConnectionId { get; init; } = string.Empty;
    public string SourceMapId { get; init; } = string.Empty;
    public string FromRegionId { get; init; } = string.Empty;
    public string ToRegionId { get; init; } = string.Empty;
    public string DestinationMapId { get; init; } = string.Empty;
    public int DestinationX { get; init; }
    public int DestinationY { get; init; }
}

public sealed record GeneratedWorldTravelRoutePlan
{
    public bool Passed { get; init; }
    public string OriginRegionId { get; init; } = string.Empty;
    public string OriginRegionTitle { get; init; } = string.Empty;
    public string OriginMapId { get; init; } = string.Empty;
    public string OriginMapTitle { get; init; } = string.Empty;
    public string OriginEntityId { get; init; } = string.Empty;
    public string DestinationRegionId { get; init; } = string.Empty;
    public string DestinationRegionTitle { get; init; } = string.Empty;
    public string DestinationMapId { get; init; } = string.Empty;
    public string DestinationMapTitle { get; init; } = string.Empty;
    public string DestinationEntityId { get; init; } = string.Empty;
    public IReadOnlyList<string> ConnectionIds { get; init; } = [];
    public IReadOnlyList<string> VisitedRegionIds { get; init; } = [];
    public IReadOnlyList<string> VisitedMapIds { get; init; } = [];
    public IReadOnlyList<GeneratedWorldTravelPlannedAction> Actions { get; init; } = [];
    public int MovementCommandCount { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

public sealed class GeneratedWorldTravelRoutePlanner
{
    // This order is the public deterministic tie-break for equal grid paths.
    private static readonly (Direction2D Direction, int X, int Y)[] DirectionOrder =
    [
        (Direction2D.Up, 0, -1),
        (Direction2D.Left, -1, 0),
        (Direction2D.Right, 1, 0),
        (Direction2D.Down, 0, 1)
    ];

    private readonly GeneratedWorldRegionMapBindingService _bindingService;

    public GeneratedWorldTravelRoutePlanner(GeneratedWorldRegionMapBindingService? bindingService = null)
    {
        _bindingService = bindingService ?? new GeneratedWorldRegionMapBindingService();
    }

    public GeneratedWorldTravelRoutePlan Plan(
        SeededGeneratedProjectSourceValidationResult source,
        GamePackageDefinition travelPackage)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(travelPackage);
        if (!source.Present || !source.Passed || source.Source is null || source.RegeneratedPlan is null
            || source.GeneratedMvpPackage is null)
            return Failed("generated_travel.source_not_current");

        var binding = _bindingService.Bind(source, travelPackage);
        if (!binding.Passed)
            return Failed(binding.Diagnostics.Count > 0 ? binding.Diagnostics : ["generated_travel.binding_failed"]);

        var origins = binding.RegionBindings
            .Where(item => string.Equals(item.MapId, source.Source.GeneratedStartMapId, StringComparison.Ordinal))
            .ToList();
        if (origins.Count != 1)
            return Failed("generated_travel.region_binding_missing:" + source.Source.GeneratedStartMapId);
        var origin = origins[0];
        var originMap = travelPackage.Game.Maps.Single(item => item.Id == origin.MapId);

        var generatedEntityIdsByMap = source.GeneratedMvpPackage.Game.Maps.ToDictionary(
            map => map.Id,
            map => map.Entities.Select(entity => entity.Id).ToHashSet(StringComparer.Ordinal),
            StringComparer.Ordinal);
        var routes = ReachableRoutes(origin.RegionId, binding.ConnectionBindings);
        var candidates = routes
            .Where(route => route.Count > 0)
            .Select(route => new
            {
                Route = route,
                Destination = binding.RegionBindings.Single(item => item.RegionId == route[^1].ToRegionId)
            })
            .Where(item => GeneratedInteractables(travelPackage,
                item.Destination.MapId, generatedEntityIdsByMap).Count > 0)
            .OrderBy(item => item.Route.Count)
            .ThenBy(item => item.Destination.RegionId, StringComparer.Ordinal)
            .ThenBy(item => string.Join("\u001f", item.Route.Select(connection => connection.ConnectionId)),
                StringComparer.Ordinal)
            .ToList();
        if (candidates.Count == 0)
            return Failed("generated_travel.destination_interactable_missing");

        var selected = candidates[0];
        var destination = selected.Destination;
        var destinationMap = travelPackage.Game.Maps.Single(item => item.Id == destination.MapId);
        var actions = new List<GeneratedWorldTravelPlannedAction>();
        var current = new Cell(originMap.StartPosition.X, originMap.StartPosition.Y);

        var originTarget = SelectReachableInteractable(
            travelPackage,
            originMap,
            current,
            GeneratedInteractables(travelPackage, origin.MapId, generatedEntityIdsByMap));
        if (originTarget is null)
            return Failed("generated_travel.target_unreachable:" + origin.MapId);
        AddMoves(actions, origin.MapId, ref current, originTarget.Path);
        actions.Add(Interaction(
            GeneratedWorldTravelPlannedActionKind.OriginInteraction,
            origin.MapId,
            current,
            originTarget.Entity.Id));

        var visitedRegions = new List<string> { origin.RegionId };
        var visitedMaps = new List<string> { origin.MapId };
        foreach (var connection in selected.Route)
        {
            var map = travelPackage.Game.Maps.Single(item => item.Id == connection.SourceMapId);
            var gateId = GeneratedWorldTravelOverlayService.GateEntityId(connection.ConnectionId);
            var gates = map.Entities.Where(entity => string.Equals(entity.Id, gateId, StringComparison.Ordinal)).ToList();
            if (gates.Count != 1 || !GateMatches(gates[0], connection))
                return Failed("generated_travel.gate_missing:" + connection.ConnectionId);
            var gateTarget = SelectReachableInteractable(travelPackage, map, current, gates);
            if (gateTarget is null)
                return Failed("generated_travel.gate_unreachable:" + connection.ConnectionId);
            AddMoves(actions, connection.SourceMapId, ref current, gateTarget.Path);

            var destinationForHop = travelPackage.Game.Maps.SingleOrDefault(item => item.Id == connection.DestinationMapId);
            if (destinationForHop is null)
                return Failed("generated_travel.destination_map_missing:" + connection.DestinationMapId);
            var component = Interactable(travelPackage, gates[0]);
            if (component is null
                || !TryCoordinate(component, MapTransitionInteractionContract.DestinationXKey, out var destinationX)
                || !TryCoordinate(component, MapTransitionInteractionContract.DestinationYKey, out var destinationY))
                return Failed("generated_travel.gate_missing:" + connection.ConnectionId);
            actions.Add(new GeneratedWorldTravelPlannedAction
            {
                Kind = GeneratedWorldTravelPlannedActionKind.GateInteraction,
                Command = PlayerCommand.Interact(),
                ExpectedMapId = connection.DestinationMapId,
                ExpectedX = destinationX,
                ExpectedY = destinationY,
                TargetEntityId = gates[0].Id,
                ConnectionId = connection.ConnectionId,
                SourceMapId = connection.SourceMapId,
                FromRegionId = connection.FromRegionId,
                ToRegionId = connection.ToRegionId,
                DestinationMapId = connection.DestinationMapId,
                DestinationX = destinationX,
                DestinationY = destinationY
            });
            current = new Cell(destinationX, destinationY);
            visitedRegions.Add(connection.ToRegionId);
            visitedMaps.Add(connection.DestinationMapId);
        }

        var destinationTarget = SelectReachableInteractable(
            travelPackage,
            destinationMap,
            current,
            GeneratedInteractables(travelPackage, destination.MapId, generatedEntityIdsByMap));
        if (destinationTarget is null)
            return Failed("generated_travel.target_unreachable:" + destination.MapId);
        AddMoves(actions, destination.MapId, ref current, destinationTarget.Path);
        actions.Add(Interaction(
            GeneratedWorldTravelPlannedActionKind.DestinationInteraction,
            destination.MapId,
            current,
            destinationTarget.Entity.Id));

        return new GeneratedWorldTravelRoutePlan
        {
            Passed = true,
            OriginRegionId = origin.RegionId,
            OriginRegionTitle = origin.RegionTitle,
            OriginMapId = origin.MapId,
            OriginMapTitle = origin.MapTitle,
            OriginEntityId = originTarget.Entity.Id,
            DestinationRegionId = destination.RegionId,
            DestinationRegionTitle = destination.RegionTitle,
            DestinationMapId = destination.MapId,
            DestinationMapTitle = destination.MapTitle,
            DestinationEntityId = destinationTarget.Entity.Id,
            ConnectionIds = selected.Route.Select(item => item.ConnectionId).ToList(),
            VisitedRegionIds = visitedRegions,
            VisitedMapIds = visitedMaps,
            Actions = actions,
            MovementCommandCount = actions.Count(item => item.Kind == GeneratedWorldTravelPlannedActionKind.Move)
        };
    }

    private static IReadOnlyList<IReadOnlyList<GeneratedWorldTravelConnectionBinding>> ReachableRoutes(
        string startRegionId,
        IReadOnlyList<GeneratedWorldTravelConnectionBinding> connections)
    {
        var result = new List<IReadOnlyList<GeneratedWorldTravelConnectionBinding>>();
        var queue = new Queue<(string RegionId, IReadOnlyList<GeneratedWorldTravelConnectionBinding> Route,
            IReadOnlySet<string> Visited)>();
        queue.Enqueue((startRegionId, [], new HashSet<string>(StringComparer.Ordinal) { startRegionId }));
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var connection in connections
                         .Where(item => item.FromRegionId == current.RegionId)
                         .OrderBy(item => item.ConnectionId, StringComparer.Ordinal))
            {
                if (current.Visited.Contains(connection.ToRegionId)) continue;
                var route = current.Route.Concat([connection]).ToList();
                result.Add(route);
                var visited = current.Visited.ToHashSet(StringComparer.Ordinal);
                visited.Add(connection.ToRegionId);
                queue.Enqueue((connection.ToRegionId, route, visited));
            }
        }
        return result;
    }

    private static IReadOnlyList<EntityInstanceDefinition> GeneratedInteractables(
        GamePackageDefinition package,
        string mapId,
        IReadOnlyDictionary<string, HashSet<string>> generatedEntityIdsByMap)
    {
        if (!generatedEntityIdsByMap.TryGetValue(mapId, out var generatedIds)) return [];
        var map = package.Game.Maps.Single(item => item.Id == mapId);
        return map.Entities
            .Where(entity => generatedIds.Contains(entity.Id)
                             && !entity.Id.StartsWith(GeneratedWorldTravelOverlayService.TravelEntityIdPrefix,
                                 StringComparison.Ordinal)
                             && Interactable(package, entity) is not null)
            .OrderBy(entity => entity.Id, StringComparer.Ordinal)
            .ToList();
    }

    private static ReachableTarget? SelectReachableInteractable(
        GamePackageDefinition package,
        MapDefinition map,
        Cell start,
        IReadOnlyList<EntityInstanceDefinition> entities)
    {
        var reachable = new List<ReachableTarget>();
        foreach (var entity in entities)
        {
            var path = FindPathAdjacent(package, map, start, entity);
            if (path is not null) reachable.Add(new ReachableTarget(entity, path));
        }
        return reachable.OrderBy(item => item.Path.Count)
            .ThenBy(item => item.Entity.Id, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static IReadOnlyList<PlannedMove>? FindPathAdjacent(
        GamePackageDefinition package,
        MapDefinition map,
        Cell start,
        EntityInstanceDefinition target)
    {
        var blocked = map.Entities.Where(entity => HasComponent(package, entity, "collidable"))
            .Select(entity => new Cell(entity.Position.X, entity.Position.Y))
            .ToHashSet();
        var targetCell = new Cell(target.Position.X, target.Position.Y);
        var queue = new Queue<Cell>();
        var visited = new HashSet<Cell> { start };
        var previous = new Dictionary<Cell, (Cell Cell, Direction2D Direction)>();
        queue.Enqueue(start);
        Cell? found = null;
        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();
            if (Distance(cell, targetCell) == 1)
            {
                found = cell;
                break;
            }
            foreach (var direction in DirectionOrder)
            {
                var next = new Cell(cell.X + direction.X, cell.Y + direction.Y);
                if (!visited.Add(next) || !Walkable(package, map, next) || blocked.Contains(next)) continue;
                previous[next] = (cell, direction.Direction);
                queue.Enqueue(next);
            }
        }
        if (found is null) return null;
        var reversed = new List<PlannedMove>();
        var cursor = found.Value;
        while (!cursor.Equals(start))
        {
            var predecessor = previous[cursor];
            reversed.Add(new PlannedMove(predecessor.Direction, cursor));
            cursor = predecessor.Cell;
        }
        reversed.Reverse();
        return reversed;
    }

    private static void AddMoves(
        ICollection<GeneratedWorldTravelPlannedAction> actions,
        string mapId,
        ref Cell current,
        IReadOnlyList<PlannedMove> path)
    {
        foreach (var move in path)
        {
            actions.Add(new GeneratedWorldTravelPlannedAction
            {
                Kind = GeneratedWorldTravelPlannedActionKind.Move,
                Command = PlayerCommand.Move(move.Direction),
                ExpectedMapId = mapId,
                ExpectedX = move.Destination.X,
                ExpectedY = move.Destination.Y
            });
            current = move.Destination;
        }
    }

    private static GeneratedWorldTravelPlannedAction Interaction(
        GeneratedWorldTravelPlannedActionKind kind,
        string mapId,
        Cell current,
        string entityId) => new()
        {
            Kind = kind,
            Command = PlayerCommand.Interact(),
            ExpectedMapId = mapId,
            ExpectedX = current.X,
            ExpectedY = current.Y,
            TargetEntityId = entityId
        };

    private static bool GateMatches(
        EntityInstanceDefinition gate,
        GeneratedWorldTravelConnectionBinding connection)
    {
        var component = gate.Components.SingleOrDefault(item =>
            string.Equals(item.Type, MapTransitionInteractionContract.ComponentType, StringComparison.Ordinal));
        return component is not null
               && Value(component, MapTransitionInteractionContract.TransitionKindKey)
               == MapTransitionInteractionContract.TransitionKindMap
               && Value(component, MapTransitionInteractionContract.ConnectionIdKey) == connection.ConnectionId
               && Value(component, MapTransitionInteractionContract.SourceMapIdKey) == connection.SourceMapId
               && Value(component, MapTransitionInteractionContract.DestinationMapIdKey) == connection.DestinationMapId
               && Value(component, MapTransitionInteractionContract.FromRegionIdKey) == connection.FromRegionId
               && Value(component, MapTransitionInteractionContract.ToRegionIdKey) == connection.ToRegionId;
    }

    private static ComponentDefinition? Interactable(
        GamePackageDefinition package,
        EntityInstanceDefinition entity) =>
        entity.Components.FirstOrDefault(component => component.Type == MapTransitionInteractionContract.ComponentType)
        ?? package.Game.EntityPrototypes.FirstOrDefault(prototype => prototype.Id == entity.PrototypeId)
            ?.Components.FirstOrDefault(component => component.Type == MapTransitionInteractionContract.ComponentType);

    private static bool HasComponent(
        GamePackageDefinition package,
        EntityInstanceDefinition entity,
        string componentType) =>
        entity.Components.Any(component => component.Type == componentType)
        || package.Game.EntityPrototypes.Any(prototype => prototype.Id == entity.PrototypeId
            && prototype.Components.Any(component => component.Type == componentType));

    private static bool Walkable(GamePackageDefinition package, MapDefinition map, Cell cell)
    {
        if (cell.X < 0 || cell.Y < 0 || cell.X >= map.Width || cell.Y >= map.Height) return false;
        var tileId = map.Tiles.FirstOrDefault(tile => tile.X == cell.X && tile.Y == cell.Y)?.TileId
                     ?? map.DefaultTileId;
        return package.Game.TilePrototypes.Any(tile => tile.Id == tileId && tile.Walkable);
    }

    private static bool TryCoordinate(ComponentDefinition component, string key, out int coordinate) =>
        int.TryParse(Value(component, key), System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out coordinate);

    private static string Value(ComponentDefinition component, string key) =>
        component.Args.TryGetValue(key, out var value) ? value : string.Empty;

    private static int Distance(Cell left, Cell right) =>
        Math.Abs(left.X - right.X) + Math.Abs(left.Y - right.Y);

    private static GeneratedWorldTravelRoutePlan Failed(string diagnostic) => new()
    {
        Passed = false,
        Diagnostics = [diagnostic]
    };

    private static GeneratedWorldTravelRoutePlan Failed(IReadOnlyList<string> diagnostics) => new()
    {
        Passed = false,
        Diagnostics = diagnostics
    };

    private readonly record struct Cell(int X, int Y);
    private readonly record struct PlannedMove(Direction2D Direction, Cell Destination);
    private sealed record ReachableTarget(EntityInstanceDefinition Entity, IReadOnlyList<PlannedMove> Path);
}
