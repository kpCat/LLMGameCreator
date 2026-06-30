namespace LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;

public sealed class ConstrainedSpatialReachabilityPlanner
{
    private readonly IReadOnlyDictionary<string, ConstrainedSpatialTileDefinition> _tileById;

    public ConstrainedSpatialReachabilityPlanner(IReadOnlyDictionary<string, ConstrainedSpatialTileDefinition> tileById)
    {
        _tileById = tileById;
    }

    public ConstrainedSpatialReachabilityProof BuildProof(
        string rowId,
        string familyId,
        IReadOnlyList<ConstrainedSpatialCell> cells,
        IReadOnlyList<ConstrainedSpatialAnchor> anchors,
        IReadOnlyList<string> familyRouteAnchorIds)
    {
        var diagnostics = new List<ConstrainedSpatialDiagnostic>();
        var anchorById = anchors.ToDictionary(item => item.AnchorId, StringComparer.Ordinal);
        if (!anchorById.TryGetValue("entry", out var entry))
        {
            diagnostics.Add(Error("goal062.reachability.entry_missing", rowId, "Entry anchor is required."));
        }

        if (!anchorById.TryGetValue("objective", out var objective))
        {
            diagnostics.Add(Error("goal062.reachability.objective_missing", rowId, "Objective anchor is required."));
        }

        if (!anchorById.TryGetValue("exit", out var exit))
        {
            diagnostics.Add(Error("goal062.reachability.exit_missing", rowId, "Exit anchor is required."));
        }

        var entryToObjective = entry is not null && objective is not null
            ? Route(rowId + "/entry-objective", entry, objective, cells)
            : EmptyRoute(rowId + "/entry-objective", "entry", "objective");
        var objectiveToExit = objective is not null && exit is not null
            ? Route(rowId + "/objective-exit", objective, exit, cells)
            : EmptyRoute(rowId + "/objective-exit", "objective", "exit");
        var familyRoute = BuildFamilyRoute(rowId, familyRouteAnchorIds, anchorById, cells);
        var passableCount = cells.Count(IsPassable);
        var blockedCount = cells.Count(cell => !IsPassable(cell));
        var routeCells = familyRoute.RouteCellIds
            .Concat(entryToObjective.RouteCellIds)
            .Concat(objectiveToExit.RouteCellIds)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        if (!entryToObjective.RouteVerified)
        {
            diagnostics.Add(Error("goal062.reachability.entry_to_objective_unreachable", rowId, "Entry must reach the objective anchor."));
        }

        if (!objectiveToExit.RouteVerified)
        {
            diagnostics.Add(Error("goal062.reachability.objective_to_exit_unreachable", rowId, "Objective must reach the exit anchor."));
        }

        if (!familyRoute.RouteVerified)
        {
            diagnostics.Add(Error("goal062.reachability.family_route_unverified", rowId, "Family-specific route must visit required anchors."));
        }

        if (familyId == "survival_sandbox" && RouteTraversesUnsafeHazard(familyRoute.RouteCellIds, cells))
        {
            diagnostics.Add(Error("goal062.reachability.unsafe_path_traversal", rowId, "Survival route must avoid blocked hazard cells."));
        }

        var reachable = entryToObjective.RouteVerified && objectiveToExit.RouteVerified && familyRoute.RouteVerified && diagnostics.All(item => item.Severity != "error");
        return new ConstrainedSpatialReachabilityProof
        {
            RowId = rowId,
            Reachable = reachable,
            RouteVerified = reachable,
            EntryToObjective = entryToObjective,
            ObjectiveToExit = objectiveToExit,
            FamilySpecificRoute = familyRoute,
            BlockedCellCount = blockedCount,
            PassableCellCount = passableCount,
            SemanticAnchorsFound = anchors.Select(item => item.Semantic).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            RouteCellIds = routeCells,
            Diagnostics = ConstrainedSpatialDetailSourceLoader.SortDiagnostics(diagnostics)
        };
    }

    public ConstrainedSpatialReachabilityProofMatrix BuildMatrix(IReadOnlyList<ConstrainedSpatialDetailRow> rows)
    {
        var proofs = rows
            .OrderBy(row => ConstrainedSpatialDetailVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => ConstrainedSpatialDetailVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row => row.ReachabilityProof)
            .ToList();

        return new ConstrainedSpatialReachabilityProofMatrix
        {
            Passed = proofs.Count == 9
                && proofs.All(item => item.Reachable)
                && proofs.All(item => item.RouteVerified)
                && proofs.All(item => item.EntryToObjective.RouteVerified)
                && proofs.All(item => item.ObjectiveToExit.RouteVerified)
                && proofs.All(item => item.FamilySpecificRoute.RouteVerified),
            RowCount = proofs.Count,
            ReachableRowCount = proofs.Count(item => item.Reachable),
            RouteVerifiedRowCount = proofs.Count(item => item.RouteVerified),
            Rows = proofs
        };
    }

    private ConstrainedSpatialRoute BuildFamilyRoute(
        string rowId,
        IReadOnlyList<string> routeAnchorIds,
        IReadOnlyDictionary<string, ConstrainedSpatialAnchor> anchorById,
        IReadOnlyList<ConstrainedSpatialCell> cells)
    {
        var segments = new List<string>();
        var verified = routeAnchorIds.Count >= 3;
        for (var index = 0; index < routeAnchorIds.Count - 1; index++)
        {
            if (!anchorById.TryGetValue(routeAnchorIds[index], out var from)
                || !anchorById.TryGetValue(routeAnchorIds[index + 1], out var to))
            {
                verified = false;
                continue;
            }

            var segment = FindPath(from.CellId, to.CellId, cells);
            if (segment.Count == 0)
            {
                verified = false;
                continue;
            }

            if (segments.Count > 0 && segment.Count > 0)
            {
                segment = segment.Skip(1).ToList();
            }

            segments.AddRange(segment);
        }

        return new ConstrainedSpatialRoute
        {
            RouteId = rowId + "/family-route",
            FromAnchorId = routeAnchorIds.FirstOrDefault() ?? string.Empty,
            ToAnchorId = routeAnchorIds.LastOrDefault() ?? string.Empty,
            RouteVerified = verified && segments.Count > 0,
            RouteCellIds = segments
        };
    }

    private ConstrainedSpatialRoute Route(
        string routeId,
        ConstrainedSpatialAnchor from,
        ConstrainedSpatialAnchor to,
        IReadOnlyList<ConstrainedSpatialCell> cells)
    {
        var path = FindPath(from.CellId, to.CellId, cells);
        return new ConstrainedSpatialRoute
        {
            RouteId = routeId,
            FromAnchorId = from.AnchorId,
            ToAnchorId = to.AnchorId,
            RouteVerified = path.Count > 0,
            RouteCellIds = path
        };
    }

    private IReadOnlyList<string> FindPath(string startCellId, string targetCellId, IReadOnlyList<ConstrainedSpatialCell> cells)
    {
        var cellById = cells.ToDictionary(item => item.CellId, StringComparer.Ordinal);
        if (!cellById.ContainsKey(startCellId) || !cellById.ContainsKey(targetCellId))
        {
            return [];
        }

        var queue = new Queue<string>();
        var previous = new Dictionary<string, string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal) { startCellId };
        queue.Enqueue(startCellId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == targetCellId)
            {
                return ReconstructPath(startCellId, targetCellId, previous);
            }

            foreach (var neighbor in Neighbors(cellById[current], cells).Where(IsPassable).OrderBy(item => item.CellId, StringComparer.Ordinal))
            {
                if (visited.Add(neighbor.CellId))
                {
                    previous[neighbor.CellId] = current;
                    queue.Enqueue(neighbor.CellId);
                }
            }
        }

        return [];
    }

    private static IReadOnlyList<string> ReconstructPath(string startCellId, string targetCellId, IReadOnlyDictionary<string, string> previous)
    {
        var result = new List<string>();
        var current = targetCellId;
        result.Add(current);
        while (current != startCellId)
        {
            if (!previous.TryGetValue(current, out var next))
            {
                return [];
            }

            current = next;
            result.Add(current);
        }

        result.Reverse();
        return result;
    }

    private static IEnumerable<ConstrainedSpatialCell> Neighbors(ConstrainedSpatialCell cell, IReadOnlyList<ConstrainedSpatialCell> cells)
    {
        foreach (var candidate in cells)
        {
            var manhattan = Math.Abs(candidate.X - cell.X) + Math.Abs(candidate.Y - cell.Y);
            if (manhattan == 1)
            {
                yield return candidate;
            }
        }
    }

    private bool IsPassable(ConstrainedSpatialCell cell) =>
        _tileById.TryGetValue(cell.TileId, out var tile) && tile.Passable;

    private bool RouteTraversesUnsafeHazard(IReadOnlyList<string> routeCellIds, IReadOnlyList<ConstrainedSpatialCell> cells)
    {
        var route = routeCellIds.ToHashSet(StringComparer.Ordinal);
        return cells.Any(cell => route.Contains(cell.CellId)
            && _tileById.TryGetValue(cell.TileId, out var tile)
            && tile.Hazard
            && !tile.Passable);
    }

    private static ConstrainedSpatialRoute EmptyRoute(string routeId, string from, string to) =>
        new()
        {
            RouteId = routeId,
            FromAnchorId = from,
            ToAnchorId = to,
            RouteVerified = false,
            RouteCellIds = []
        };

    private static ConstrainedSpatialDiagnostic Error(string code, string target, string message) =>
        ConstrainedSpatialDiagnostic.Error(code, target, message);
}
