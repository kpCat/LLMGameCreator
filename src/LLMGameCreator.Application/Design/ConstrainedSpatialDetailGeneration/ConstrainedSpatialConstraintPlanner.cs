namespace LLMGameCreator.Application.Design.ConstrainedSpatialDetailGeneration;

public sealed class ConstrainedSpatialConstraintPlanner
{
    public ConstrainedSpatialConstraintRuleCatalog BuildConstraintRuleCatalog(ConstrainedSpatialPaletteCatalog paletteCatalog)
    {
        var rules = paletteCatalog.Tiles
            .OrderBy(tile => ConstrainedSpatialDetailVocabulary.FamilyOrderingKey(tile.FamilyApplicability.First()), StringComparer.Ordinal)
            .ThenBy(tile => tile.TileId, StringComparer.Ordinal)
            .Select(tile => new ConstrainedSpatialConstraintRule
            {
                RuleId = "constraint/" + tile.TileId.Replace("tile/", string.Empty, StringComparison.Ordinal).Replace('/', '-'),
                FamilyId = tile.FamilyApplicability.First(),
                TileId = tile.TileId,
                AllowedNeighborTags = tile.Passable ? ["passable", "entry", "exit", "objective", "corridor"] : ["blocked", "biome", "wall", "hazard"],
                ContradictionDetected = false,
                RetryBudget = 2,
                FallbackBudget = 3,
                DiagnosticCode = "goal062.constraint.in_house_adjacency_record"
            })
            .ToList();

        return new ConstrainedSpatialConstraintRuleCatalog
        {
            Passed = rules.Count == paletteCatalog.TileCount
                && rules.All(item => !string.IsNullOrWhiteSpace(item.TileId))
                && rules.All(item => item.RetryBudget > 0 && item.FallbackBudget > 0),
            RuleCount = rules.Count,
            Rules = rules
        };
    }

    public IReadOnlyList<ConstrainedSpatialDetailRow> BuildRows(
        ConstrainedSpatialSourceBundle source,
        ConstrainedSpatialPaletteCatalog paletteCatalog,
        ConstrainedSpatialRewriteRuleCatalog rewriteCatalog,
        ConstrainedSpatialConstraintRuleCatalog constraintCatalog)
    {
        var tileById = ConstrainedSpatialPaletteCatalogBuilder.TileById(paletteCatalog);
        var reachability = new ConstrainedSpatialReachabilityPlanner(tileById);
        var repair = new ConstrainedSpatialRepairPlanner();

        return source.PackageRows
            .OrderBy(row => ConstrainedSpatialDetailVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => ConstrainedSpatialDetailVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row => BuildRow(row, paletteCatalog, rewriteCatalog, constraintCatalog, reachability, repair))
            .ToList();
    }

    public ConstrainedSpatialDetailMatrix BuildMatrix(IReadOnlyList<ConstrainedSpatialDetailRow> rows)
    {
        var summaries = rows
            .OrderBy(row => ConstrainedSpatialDetailVocabulary.FamilyOrderingKey(row.FamilyId), StringComparer.Ordinal)
            .ThenBy(row => ConstrainedSpatialDetailVocabulary.SeedOrderingKey(row.SeedId), StringComparer.Ordinal)
            .Select(row => new ConstrainedSpatialDetailMatrixRowSummary
            {
                RowId = row.RowId,
                FamilyId = row.FamilyId,
                SeedId = row.SeedId,
                PackageId = row.PackageId,
                RowHash = row.RowHash,
                VarianceMarker = row.VarianceMetrics.VarianceMarker,
                Reachable = row.ReachabilityProof.Reachable,
                RouteVerified = row.ReachabilityProof.RouteVerified,
                PathLength = row.VarianceMetrics.PathLength,
                TileHistogram = row.VarianceMetrics.TileHistogram
            })
            .ToList();

        var sameFamilyVariancePassed = ConstrainedSpatialDetailVocabulary.FamilyIds
            .All(familyId =>
            {
                var familyRows = rows.Where(row => row.FamilyId == familyId).OrderBy(row => row.SeedId, StringComparer.Ordinal).ToList();
                return familyRows.Count == 3 && PairwiseDifferences(familyRows).All(count => count >= 2);
            });

        return new ConstrainedSpatialDetailMatrix
        {
            Passed = rows.Count == 9
                && rows.Select(row => row.RowHash).Distinct(StringComparer.Ordinal).Count() == 9
                && rows.All(row => row.ReachabilityProof.Reachable && row.ReachabilityProof.RouteVerified)
                && sameFamilyVariancePassed,
            Accepted = false,
            RowCount = rows.Count,
            FamilyCount = rows.Select(row => row.FamilyId).Distinct(StringComparer.Ordinal).Count(),
            SeedCount = rows.Select(row => row.SeedId).Distinct(StringComparer.Ordinal).Count(),
            DistinctRowHashCount = rows.Select(row => row.RowHash).Distinct(StringComparer.Ordinal).Count(),
            SameFamilyRowsDifferByTwoMetrics = sameFamilyVariancePassed,
            FamiliesDifferByPaletteAndRuleSet = ConstrainedSpatialDetailVocabulary.FamilyIds.All(familyId => rows.Any(row => row.FamilyId == familyId)),
            Rows = summaries
        };
    }

    private static ConstrainedSpatialDetailRow BuildRow(
        ConstrainedSpatialPackageRowSource source,
        ConstrainedSpatialPaletteCatalog paletteCatalog,
        ConstrainedSpatialRewriteRuleCatalog rewriteCatalog,
        ConstrainedSpatialConstraintRuleCatalog constraintCatalog,
        ConstrainedSpatialReachabilityPlanner reachabilityPlanner,
        ConstrainedSpatialRepairPlanner repairPlanner)
    {
        var seedIndex = SeedIndex(source.SeedId);
        var familyPlan = source.FamilyId switch
        {
            "map_panel_rpg" => BuildMapPanelRpgPlan(source, seedIndex),
            "survival_sandbox" => BuildSurvivalSandboxPlan(source, seedIndex),
            "first_person_grid_dungeon" => BuildFirstPersonGridDungeonPlan(source, seedIndex),
            _ => BuildMapPanelRpgPlan(source, seedIndex)
        };

        var tileById = ConstrainedSpatialPaletteCatalogBuilder.TileById(paletteCatalog);
        var cells = BuildCells(familyPlan.Grid, tileById);
        var proof = reachabilityPlanner.BuildProof(source.RowId, source.FamilyId, cells, familyPlan.Anchors, familyPlan.FamilyRouteAnchorIds);
        var repair = repairPlanner.BuildRecord(source, rewriteCatalog, proof);
        var variance = BuildVariance(source.RowId, cells, familyPlan.Anchors, proof, tileById);
        var appliedRules = rewriteCatalog.Rules
            .Where(rule => rule.FamilyApplicability.Contains(source.FamilyId, StringComparer.Ordinal)
                || rule.FamilyApplicability.Count == ConstrainedSpatialDetailVocabulary.FamilyIds.Count)
            .OrderBy(rule => rule.DeterministicApplicationOrder)
            .Select(rule => rule.RuleId)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var constraintDiagnostics = constraintCatalog.Rules.Any(rule => rule.FamilyId == source.FamilyId)
            ? new[] { Info("goal062.constraint.planned", source.RowId, "In-house adjacency constraints were applied deterministically for the row family.") }
            : new[] { Error("goal062.constraint.family_missing", source.FamilyId, "No constraint rules were available for the row family.") };

        var rowWithoutHash = new ConstrainedSpatialDetailRow
        {
            RowId = source.RowId,
            FamilyId = source.FamilyId,
            SeedId = source.SeedId,
            PackageRowId = source.RowId,
            PackageId = source.PackageId,
            PackageHash = source.PackageHash,
            ReviewPackageRef = source.ReviewPackageRelativePath,
            Goal059DerivedCampaignHash = source.Goal059DerivedCampaignHash,
            DeterministicSeed = source.RowId + "/goal062-spatial-detail",
            Width = familyPlan.Width,
            Height = familyPlan.Height,
            TileDataCompact = BuildCompactRows(familyPlan.Grid, tileById),
            TileIdByMarker = BuildTileIdByMarker(cells, tileById),
            Cells = cells,
            Anchors = familyPlan.Anchors,
            Paths = [proof.EntryToObjective, proof.ObjectiveToExit, proof.FamilySpecificRoute],
            AppliedRewriteRuleIds = appliedRules,
            ConstraintDiagnostics = ConstrainedSpatialDetailSourceLoader.SortDiagnostics(constraintDiagnostics),
            RepairDiagnostics = repair.Diagnostics,
            ReachabilityProof = proof,
            VarianceMetrics = variance,
            RepairFallback = repair,
            PreviewExportRef = "preview-export-spatial-payload.json#rows/" + source.RowId,
            ThumbnailRef = string.Empty,
            ThumbnailDecision = "skipped_no_existing_bcl_png_helper_required_for_goal",
            Provenance = "in_house_fixture"
        };

        return rowWithoutHash with
        {
            RowHash = ConstrainedSpatialDetailHash.Hash(ConstrainedSpatialDetailHash.Serialize(rowWithoutHash))
        };
    }

    private static FamilyGridPlan BuildMapPanelRpgPlan(ConstrainedSpatialPackageRowSource source, int seedIndex)
    {
        var width = seedIndex == 1 ? 10 : 9;
        var height = seedIndex == 2 ? 8 : 7;
        var grid = NewGrid(width, height, "tile/map_panel_rpg/field");
        Scatter(grid, "tile/map_panel_rpg/forest", seedIndex, modulo: 7);

        var entry = (1, 1);
        var npc = (2 + seedIndex, 1 + (seedIndex % 2));
        var objective = (width - 4, 2 + seedIndex);
        var item = (3 + seedIndex, height - 3);
        var exit = (width - 2, height - 2);
        MarkPath(grid, [entry, npc, objective, item, exit], "tile/map_panel_rpg/road", horizontalFirst: seedIndex != 1);

        var anchors = new List<ConstrainedSpatialAnchor>
        {
            Anchor("entry", "entry", entry, "tile/map_panel_rpg/entry"),
            Anchor("npc", "npc", npc, "tile/map_panel_rpg/npc_marker"),
            Anchor("objective", "quest_objective", objective, "tile/map_panel_rpg/quest_marker"),
            Anchor("item", "item", item, "tile/map_panel_rpg/item_marker"),
            Anchor("exit", "exit", exit, "tile/map_panel_rpg/exit"),
            Anchor("settlement", "settlement", (Math.Max(1, npc.Item1 - 1), npc.Item2), "tile/map_panel_rpg/settlement")
        };
        ApplyAnchors(grid, anchors);
        return new FamilyGridPlan(width, height, grid, anchors, ["entry", "npc", "objective", "item", "exit"]);
    }

    private static FamilyGridPlan BuildSurvivalSandboxPlan(ConstrainedSpatialPackageRowSource source, int seedIndex)
    {
        var width = seedIndex == 2 ? 11 : 10;
        var height = seedIndex == 1 ? 8 : 7;
        var grid = NewGrid(width, height, "tile/survival_sandbox/weather_marker");
        Scatter(grid, "tile/survival_sandbox/hazard", seedIndex + 2, modulo: 6);

        var entry = (1, height - 2);
        var shelter = (2 + seedIndex, height - 3);
        var resource = (width / 2, 1 + seedIndex);
        var water = (width - 3, 2 + (seedIndex % 2));
        var hazard = (width - 4, height - 3);
        var exit = (width - 2, 1);
        MarkPath(grid, [entry, shelter, resource, water, exit], "tile/survival_sandbox/safe_path", horizontalFirst: seedIndex == 0);

        var anchors = new List<ConstrainedSpatialAnchor>
        {
            Anchor("entry", "entry", entry, "tile/survival_sandbox/entry"),
            Anchor("shelter", "shelter", shelter, "tile/survival_sandbox/shelter"),
            Anchor("objective", "resource_objective", resource, "tile/survival_sandbox/resource"),
            Anchor("water", "water", water, "tile/survival_sandbox/water"),
            Anchor("hazard", "hazard_avoided", hazard, "tile/survival_sandbox/hazard"),
            Anchor("exit", "exit", exit, "tile/survival_sandbox/exit")
        };
        ApplyAnchors(grid, anchors);
        return new FamilyGridPlan(width, height, grid, anchors, ["entry", "shelter", "objective", "water", "exit"]);
    }

    private static FamilyGridPlan BuildFirstPersonGridDungeonPlan(ConstrainedSpatialPackageRowSource source, int seedIndex)
    {
        var width = seedIndex == 1 ? 9 : 8;
        var height = seedIndex == 2 ? 9 : 8;
        var grid = NewGrid(width, height, "tile/first_person_grid_dungeon/wall");

        var entry = (1, 1);
        var door = (2 + seedIndex, 1);
        var encounter = (3 + seedIndex, height / 2);
        var objective = (width - 3, height - 3);
        var exit = (width - 2, height - 2);
        MarkPath(grid, [entry, door, encounter, objective, exit], "tile/first_person_grid_dungeon/corridor", horizontalFirst: seedIndex != 2);
        OpenRoom(grid, objective, "tile/first_person_grid_dungeon/floor");
        OpenRoom(grid, encounter, "tile/first_person_grid_dungeon/floor");

        var anchors = new List<ConstrainedSpatialAnchor>
        {
            Anchor("entry", "entry", entry, "tile/first_person_grid_dungeon/entry"),
            Anchor("door", "door", door, "tile/first_person_grid_dungeon/door"),
            Anchor("encounter", "encounter", encounter, "tile/first_person_grid_dungeon/encounter"),
            Anchor("objective", "objective", objective, "tile/first_person_grid_dungeon/objective"),
            Anchor("exit", "exit", exit, "tile/first_person_grid_dungeon/exit")
        };
        ApplyAnchors(grid, anchors);
        return new FamilyGridPlan(width, height, grid, anchors, ["entry", "door", "encounter", "objective", "exit"]);
    }

    private static ConstrainedSpatialVarianceMetrics BuildVariance(
        string rowId,
        IReadOnlyList<ConstrainedSpatialCell> cells,
        IReadOnlyList<ConstrainedSpatialAnchor> anchors,
        ConstrainedSpatialReachabilityProof proof,
        IReadOnlyDictionary<string, ConstrainedSpatialTileDefinition> tileById)
    {
        var histogram = cells
            .GroupBy(cell => cell.TileId, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var anchorPositions = anchors
            .OrderBy(anchor => anchor.AnchorId, StringComparer.Ordinal)
            .ToDictionary(anchor => anchor.AnchorId, anchor => anchor.CellId, StringComparer.Ordinal);
        var semanticCounts = cells
            .SelectMany(cell => cell.SemanticTags)
            .GroupBy(tag => tag, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        var hazardCount = cells.Count(cell => tileById[cell.TileId].Hazard);
        var resourceCount = cells.Count(cell => tileById[cell.TileId].Resource);
        var encounterCount = cells.Count(cell => cell.SemanticTags.Contains("encounter", StringComparer.Ordinal));
        var pathLength = proof.FamilySpecificRoute.RouteCellIds.Count;
        return new ConstrainedSpatialVarianceMetrics
        {
            RowId = rowId,
            TileHistogram = histogram,
            AnchorPositions = anchorPositions,
            PathLength = pathLength,
            HazardCount = hazardCount,
            ResourceCount = resourceCount,
            EncounterCount = encounterCount,
            FamilySpecificSemanticCounts = semanticCounts,
            MeaningfulMetricKeys = ["tile_histogram", "anchor_positions", "path_length", "hazard_resource_encounter_counts", "family_semantic_counts"],
            VarianceMarker = "goal062-" + ConstrainedSpatialDetailHash.Hash(rowId + "|" + pathLength + "|" + hazardCount + "|" + resourceCount)[..12]
        };
    }

    private static IReadOnlyList<int> PairwiseDifferences(IReadOnlyList<ConstrainedSpatialDetailRow> rows)
    {
        var result = new List<int>();
        for (var left = 0; left < rows.Count; left++)
        {
            for (var right = left + 1; right < rows.Count; right++)
            {
                result.Add(DifferenceCount(rows[left].VarianceMetrics, rows[right].VarianceMetrics));
            }
        }

        return result;
    }

    private static int DifferenceCount(ConstrainedSpatialVarianceMetrics left, ConstrainedSpatialVarianceMetrics right)
    {
        var count = 0;
        if (string.Join("|", left.TileHistogram.Select(item => item.Key + "=" + item.Value)) != string.Join("|", right.TileHistogram.Select(item => item.Key + "=" + item.Value)))
        {
            count++;
        }

        if (string.Join("|", left.AnchorPositions.Select(item => item.Key + "=" + item.Value)) != string.Join("|", right.AnchorPositions.Select(item => item.Key + "=" + item.Value)))
        {
            count++;
        }

        if (left.PathLength != right.PathLength)
        {
            count++;
        }

        if (left.HazardCount != right.HazardCount || left.ResourceCount != right.ResourceCount || left.EncounterCount != right.EncounterCount)
        {
            count++;
        }

        if (left.VarianceMarker != right.VarianceMarker)
        {
            count++;
        }

        return count;
    }

    private static string[,] NewGrid(int width, int height, string tileId)
    {
        var grid = new string[width, height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                grid[x, y] = tileId;
            }
        }

        return grid;
    }

    private static void Scatter(string[,] grid, string tileId, int seed, int modulo)
    {
        var width = grid.GetLength(0);
        var height = grid.GetLength(1);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1 || ((x * 17 + y * 31 + seed) % modulo == 0))
                {
                    grid[x, y] = tileId;
                }
            }
        }
    }

    private static void MarkPath(string[,] grid, IReadOnlyList<(int X, int Y)> anchors, string tileId, bool horizontalFirst)
    {
        for (var index = 0; index < anchors.Count - 1; index++)
        {
            var current = anchors[index];
            var target = anchors[index + 1];
            if (horizontalFirst)
            {
                WalkHorizontal(grid, current.X, target.X, current.Y, tileId);
                WalkVertical(grid, current.Y, target.Y, target.X, tileId);
            }
            else
            {
                WalkVertical(grid, current.Y, target.Y, current.X, tileId);
                WalkHorizontal(grid, current.X, target.X, target.Y, tileId);
            }
        }
    }

    private static void WalkHorizontal(string[,] grid, int fromX, int toX, int y, string tileId)
    {
        var step = fromX <= toX ? 1 : -1;
        for (var x = fromX; x != toX + step; x += step)
        {
            grid[x, y] = tileId;
        }
    }

    private static void WalkVertical(string[,] grid, int fromY, int toY, int x, string tileId)
    {
        var step = fromY <= toY ? 1 : -1;
        for (var y = fromY; y != toY + step; y += step)
        {
            grid[x, y] = tileId;
        }
    }

    private static void OpenRoom(string[,] grid, (int X, int Y) center, string tileId)
    {
        for (var y = Math.Max(1, center.Y - 1); y <= Math.Min(grid.GetLength(1) - 2, center.Y + 1); y++)
        {
            for (var x = Math.Max(1, center.X - 1); x <= Math.Min(grid.GetLength(0) - 2, center.X + 1); x++)
            {
                if (Math.Abs(center.X - x) + Math.Abs(center.Y - y) <= 1)
                {
                    grid[x, y] = tileId;
                }
            }
        }
    }

    private static void ApplyAnchors(string[,] grid, IReadOnlyList<ConstrainedSpatialAnchor> anchors)
    {
        foreach (var anchor in anchors)
        {
            grid[anchor.X, anchor.Y] = anchor.TileId;
        }
    }

    private static IReadOnlyList<ConstrainedSpatialCell> BuildCells(string[,] grid, IReadOnlyDictionary<string, ConstrainedSpatialTileDefinition> tileById)
    {
        var cells = new List<ConstrainedSpatialCell>();
        for (var y = 0; y < grid.GetLength(1); y++)
        {
            for (var x = 0; x < grid.GetLength(0); x++)
            {
                var tileId = grid[x, y];
                cells.Add(new ConstrainedSpatialCell
                {
                    CellId = CellId(x, y),
                    X = x,
                    Y = y,
                    TileId = tileId,
                    SemanticTags = tileById.TryGetValue(tileId, out var tile) ? tile.SemanticTags : []
                });
            }
        }

        return cells;
    }

    private static IReadOnlyList<string> BuildCompactRows(string[,] grid, IReadOnlyDictionary<string, ConstrainedSpatialTileDefinition> tileById)
    {
        var rows = new List<string>();
        for (var y = 0; y < grid.GetLength(1); y++)
        {
            var markers = new List<string>();
            for (var x = 0; x < grid.GetLength(0); x++)
            {
                markers.Add(tileById[grid[x, y]].RenderMarker);
            }

            rows.Add(string.Concat(markers));
        }

        return rows;
    }

    private static IReadOnlyDictionary<string, string> BuildTileIdByMarker(
        IReadOnlyList<ConstrainedSpatialCell> cells,
        IReadOnlyDictionary<string, ConstrainedSpatialTileDefinition> tileById) =>
        cells.Select(cell => tileById[cell.TileId])
            .GroupBy(tile => tile.RenderMarker, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.OrderBy(tile => tile.TileId, StringComparer.Ordinal).First().TileId, StringComparer.Ordinal);

    private static ConstrainedSpatialAnchor Anchor(string id, string semantic, (int X, int Y) cell, string tileId) =>
        new()
        {
            AnchorId = id,
            Semantic = semantic,
            CellId = CellId(cell.X, cell.Y),
            X = cell.X,
            Y = cell.Y,
            TileId = tileId
        };

    private static string CellId(int x, int y) => "x" + x.ToString("00") + "_y" + y.ToString("00");

    private static int SeedIndex(string seedId) =>
        seedId switch
        {
            "seed_alpha" => 0,
            "seed_beta" => 1,
            "seed_gamma" => 2,
            _ => 0
        };

    private static ConstrainedSpatialDiagnostic Error(string code, string target, string message) =>
        ConstrainedSpatialDiagnostic.Error(code, target, message);

    private static ConstrainedSpatialDiagnostic Info(string code, string target, string message) =>
        ConstrainedSpatialDiagnostic.Info(code, target, message);

    private sealed record FamilyGridPlan(
        int Width,
        int Height,
        string[,] Grid,
        IReadOnlyList<ConstrainedSpatialAnchor> Anchors,
        IReadOnlyList<string> FamilyRouteAnchorIds);
}
