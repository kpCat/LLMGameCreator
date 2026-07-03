using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.DeterministicVisualMapPatchComposer;

public static class DeterministicVisualMapPatchComposerValidator
{
    private static readonly Regex StableIdRegex = new("^[a-z0-9][a-z0-9_.-]*$", RegexOptions.Compiled);

    public static VisualMapPatchValidationResult Validate(
        VisualMapPatchComposerRequest request,
        IReadOnlySet<string>? knownGoal086MicrotilePreviewIds = null,
        IReadOnlyDictionary<string, string>? svgByPatchId = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        knownGoal086MicrotilePreviewIds ??= DeterministicVisualMapPatchComposerFixtures.KnownGoal086MicrotilePreviewIds;
        var diagnostics = new List<VisualMapPatchDiagnostic>();

        ValidateId(request.RequestId, "visual_map_patch.request_id.invalid", "request", "Request id must be stable and lowercase.", diagnostics);
        ValidateRequiredText(request.GeneratorVersion, "visual_map_patch.generator_version.missing", request.RequestId, "Generator version is required.", diagnostics);
        ValidateRelativePath(request.OutputRelativeDirectory, "visual_map_patch.output_path.invalid", request.RequestId, "Output directory must be a safe relative path.", diagnostics);
        if (request.PromptTextIsSourceOfTruth
            || string.Equals(request.SourceOfTruthKind, "provider_prompt_text", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("visual_map_patch.prompt.source_of_truth", request.RequestId, "Prompt text must not be visual map patch source of truth."));
        }

        ValidateDuplicates(request.Patches, item => item.PatchId, "visual_map_patch.patch_id.duplicate", "Patch ids must be unique.", diagnostics);
        foreach (var patch in request.Patches.OrderBy(item => item.PatchId, StringComparer.Ordinal))
        {
            ValidatePatch(patch, request.SourceGoal084085086LineageRequired, knownGoal086MicrotilePreviewIds, diagnostics);
        }

        if (svgByPatchId != null)
        {
            foreach (var patch in request.Patches.OrderBy(item => item.PatchId, StringComparer.Ordinal))
            {
                if (!svgByPatchId.TryGetValue(patch.PatchId, out var svg))
                {
                    diagnostics.Add(Error("visual_map_patch.svg.missing", patch.PatchId, "Every patch must have a rendered SVG preview."));
                    continue;
                }

                ValidateSvg(patch.PatchId, svg, diagnostics);
            }
        }

        return new VisualMapPatchValidationResult
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            DiagnosticCount = diagnostics.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<VisualMapPatchDiagnostic> SortDiagnostics(IEnumerable<VisualMapPatchDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    public static bool IsSvgSafe(string svg) =>
        !string.IsNullOrWhiteSpace(svg)
        && svg.Contains("<svg", StringComparison.Ordinal)
        && svg.Contains("viewBox=", StringComparison.Ordinal)
        && !svg.Contains("<script", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("http://", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("https://", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("xlink:href", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains(" href=", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("data:", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("base64", StringComparison.OrdinalIgnoreCase);

    public static int CountSvgRects(string svg) => Count(svg, "<rect ");

    private static void ValidatePatch(
        VisualMapPatchDefinition patch,
        bool sourceLineageRequired,
        IReadOnlySet<string> knownGoal086MicrotilePreviewIds,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        ValidateId(patch.PatchId, "visual_map_patch.patch_id.invalid", patch.PatchId, "Patch id must be stable and lowercase.", diagnostics);
        ValidateRelativePath(patch.PatchSvgRelativePath, "visual_map_patch.svg_path.invalid", patch.PatchId, "Patch SVG path must be relative and safe.", diagnostics);
        if (!patch.PatchSvgRelativePath.StartsWith(DeterministicVisualMapPatchComposerVocabulary.PatchRelativeDirectory + "/", StringComparison.Ordinal)
            || !patch.PatchSvgRelativePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("visual_map_patch.svg_path.not_patch_svg", patch.PatchId, "Patch output must be patches/*.svg."));
        }

        if (patch.Width is < 4 or > 64 || patch.Height is < 4 or > 64)
        {
            diagnostics.Add(Error("visual_map_patch.dimensions.out_of_bounds", patch.PatchId, "Patch dimensions must be bounded between 4 and 64 cells."));
        }

        if (patch.Seed <= 0)
        {
            diagnostics.Add(Error("visual_map_patch.seed.missing_or_nondeterministic", patch.PatchId, "A deterministic positive seed is required."));
        }

        if (patch.Cells.Count != patch.Width * patch.Height)
        {
            diagnostics.Add(Error("visual_map_patch.cell_count.invalid", patch.PatchId, "Patch cell count must match width multiplied by height."));
        }

        ValidateDuplicates(patch.Cells, item => item.CellId, "visual_map_patch.cell_id.duplicate", "Cell ids must be unique.", diagnostics);
        ValidateDuplicates(patch.ObjectAnchors, item => item.AnchorId, "visual_map_patch.object_id.duplicate", "Object anchor ids must be unique.", diagnostics);
        ValidateDuplicates(patch.SettlementAnchors, item => item.SettlementId, "visual_map_patch.settlement_id.duplicate", "Settlement anchor ids must be unique.", diagnostics);
        ValidateDuplicates(patch.CreatureMarkers, item => item.MarkerId, "visual_map_patch.creature_id.duplicate", "Creature marker ids must be unique.", diagnostics);
        ValidateDuplicates(patch.Overlays, item => item.OverlayId, "visual_map_patch.overlay_id.duplicate", "Overlay ids must be unique.", diagnostics);

        var cellMap = patch.Cells.ToDictionary(item => (item.X, item.Y), item => item);
        foreach (var cell in patch.Cells.OrderBy(item => item.CellId, StringComparer.Ordinal))
        {
            ValidateCell(patch, cell, knownGoal086MicrotilePreviewIds, diagnostics);
        }

        ValidateLayering(patch, diagnostics);
        ValidateSourceLineage(patch, sourceLineageRequired, diagnostics);
        ValidateWaterAndTransitions(patch, cellMap, diagnostics);
        ValidateRoads(patch, cellMap, diagnostics);
        ValidateObjects(patch, cellMap, diagnostics);
        ValidateSettlements(patch, cellMap, diagnostics);
        ValidateCreatures(patch, diagnostics);
        ValidateOverlays(patch, knownGoal086MicrotilePreviewIds, diagnostics);
    }

    private static void ValidateCell(
        VisualMapPatchDefinition patch,
        VisualMapPatchCell cell,
        IReadOnlySet<string> knownGoal086MicrotilePreviewIds,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        ValidateId(cell.CellId, "visual_map_patch.cell_id.invalid", $"{patch.PatchId}/{cell.CellId}", "Cell id must be stable and lowercase.", diagnostics);
        if (cell.X < 0 || cell.Y < 0 || cell.X >= patch.Width || cell.Y >= patch.Height)
        {
            diagnostics.Add(Error("visual_map_patch.cell_coordinate.out_of_bounds", $"{patch.PatchId}/{cell.CellId}", "Cell coordinates must stay inside patch dimensions."));
        }

        ValidateKnownMicrotile(cell.SourceMicrotilePreviewId, $"{patch.PatchId}/{cell.CellId}", knownGoal086MicrotilePreviewIds, diagnostics);
        foreach (var tileRef in cell.TileRefs)
        {
            ValidateKnownMicrotile(tileRef.PreviewId, $"{patch.PatchId}/{cell.CellId}/{tileRef.LayerKind}", knownGoal086MicrotilePreviewIds, diagnostics);
        }
    }

    private static void ValidateLayering(VisualMapPatchDefinition patch, List<VisualMapPatchDiagnostic> diagnostics)
    {
        if (patch.Layers.Count == 0)
        {
            diagnostics.Add(Error("visual_map_patch.layers.missing", patch.PatchId, "Patch layers are required."));
            return;
        }

        if (patch.Layers.Select(item => item.Order).Distinct().Count() != patch.Layers.Count)
        {
            diagnostics.Add(Error("visual_map_patch.layers.order_duplicate", patch.PatchId, "Layer orders must be deterministic and unique."));
        }

        var required = new[]
        {
            VisualMapPatchLayerKind.Terrain,
            VisualMapPatchLayerKind.Water,
            VisualMapPatchLayerKind.Road,
            VisualMapPatchLayerKind.Object,
            VisualMapPatchLayerKind.Settlement,
            VisualMapPatchLayerKind.Creature,
            VisualMapPatchLayerKind.Overlay,
            VisualMapPatchLayerKind.RatingFallback
        };
        foreach (var kind in required)
        {
            if (!patch.Layers.Any(item => item.Kind == kind))
            {
                diagnostics.Add(Error("visual_map_patch.layers.kind_missing", patch.PatchId, $"Layer kind {kind} is required."));
            }
        }
    }

    private static void ValidateSourceLineage(
        VisualMapPatchDefinition patch,
        bool sourceLineageRequired,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        if (!sourceLineageRequired)
        {
            return;
        }

        var sourceKinds = patch.SourceReferences.Select(item => item.SourceKind).ToHashSet(StringComparer.Ordinal);
        foreach (var required in new[] { "goal084", "goal085", "goal086" })
        {
            if (!sourceKinds.Contains(required))
            {
                diagnostics.Add(Error("visual_map_patch.source_lineage.missing", patch.PatchId, "Every patch must trace to Goal084, Goal085 and Goal086 lineage."));
            }
        }

        foreach (var source in patch.SourceReferences)
        {
            ValidateRelativePath(source.RelativePath, "visual_map_patch.source_lineage.path_invalid", $"{patch.PatchId}/{source.SourceId}", "Source lineage paths must be relative and safe.", diagnostics);
        }
    }

    private static void ValidateWaterAndTransitions(
        VisualMapPatchDefinition patch,
        IReadOnlyDictionary<(int X, int Y), VisualMapPatchCell> cellMap,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        foreach (var coast in patch.Cells.Where(item => item.WaterKind == VisualMapPatchWaterKind.Coast))
        {
            var neighbors = Neighbors(coast, cellMap).ToList();
            if (!neighbors.Any(IsWaterLike) || !neighbors.Any(item => item.WaterKind == VisualMapPatchWaterKind.None))
            {
                diagnostics.Add(Error("visual_map_patch.water.coast_adjacency_missing", $"{patch.PatchId}/{coast.CellId}", "Coast cells require both water and land adjacency."));
            }
        }

        var riverCells = patch.Cells
            .Where(item => item.WaterKind == VisualMapPatchWaterKind.River)
            .Select(item => (item.X, item.Y))
            .ToHashSet();
        var flowCells = patch.WaterFlows
            .Where(item => item.WaterKind == VisualMapPatchWaterKind.River)
            .SelectMany(item => item.Nodes.Select(node => (node.X, node.Y)))
            .ToHashSet();
        foreach (var river in riverCells)
        {
            if (!flowCells.Contains(river))
            {
                diagnostics.Add(Error("visual_map_patch.water.river_flow_missing", $"{patch.PatchId}/cell_{river.X:00}_{river.Y:00}", "River cells require deterministic water-flow connectors."));
            }
        }

        foreach (var flow in patch.WaterFlows)
        {
            if (flow.Nodes.Count < 2 || flow.Nodes.Any(item => item.Connectors.Count == 0))
            {
                diagnostics.Add(Error("visual_map_patch.water.flow_connectors_missing", $"{patch.PatchId}/{flow.FlowId}", "Water flows require at least two nodes and deterministic connectors."));
            }

            ValidateAdjacentPath(flow.Nodes, $"{patch.PatchId}/{flow.FlowId}", "visual_map_patch.water.flow_gap", "Water flow nodes must be adjacent.", diagnostics);
        }

        foreach (var transition in patch.BiomeTransitions)
        {
            if (transition.FromBiome == transition.ToBiome || transition.Cells.Count == 0)
            {
                diagnostics.Add(Error("visual_map_patch.biome_transition.invalid", $"{patch.PatchId}/{transition.TransitionId}", "Biome transitions require two distinct biomes and cells."));
                continue;
            }

            var compatible = transition.Cells.Any(point =>
                cellMap.TryGetValue((point.X, point.Y), out var cell)
                && Neighbors(cell, cellMap).Any(neighbor =>
                    neighbor.TerrainBiome == transition.FromBiome || neighbor.TerrainBiome == transition.ToBiome));
            if (!compatible)
            {
                diagnostics.Add(Error("visual_map_patch.biome_transition.neighbor_missing", $"{patch.PatchId}/{transition.TransitionId}", "Biome transition cells must touch compatible neighboring biomes."));
            }
        }
    }

    private static void ValidateRoads(
        VisualMapPatchDefinition patch,
        IReadOnlyDictionary<(int X, int Y), VisualMapPatchCell> cellMap,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        var bridgeAnchors = patch.ObjectAnchors
            .Where(item => string.Equals(item.ObjectKind, "bridge", StringComparison.Ordinal))
            .Select(item => (item.X, item.Y))
            .ToList();
        foreach (var road in patch.RoadPaths)
        {
            if (road.Nodes.Count < 2)
            {
                diagnostics.Add(Error("visual_map_patch.road.nodes_missing", $"{patch.PatchId}/{road.PathId}", "Roads require at least two nodes."));
            }

            ValidateAdjacentPath(road.Nodes, $"{patch.PatchId}/{road.PathId}", "visual_map_patch.road.connector_gap", "Road/path nodes must connect to adjacent cells.", diagnostics);
            foreach (var node in road.Nodes)
            {
                if (!cellMap.TryGetValue((node.X, node.Y), out var cell))
                {
                    diagnostics.Add(Error("visual_map_patch.road.node_out_of_bounds", $"{patch.PatchId}/{road.PathId}", "Road nodes must stay inside the patch."));
                    continue;
                }

                if (!cell.IsPassable && !bridgeAnchors.Any(anchor => Distance(anchor.X, anchor.Y, node.X, node.Y) <= 1))
                {
                    diagnostics.Add(Error("visual_map_patch.road.impassable_cell", $"{patch.PatchId}/{road.PathId}", "Road nodes must use passable cells or declared bridge crossings."));
                }
            }
        }
    }

    private static void ValidateObjects(
        VisualMapPatchDefinition patch,
        IReadOnlyDictionary<(int X, int Y), VisualMapPatchCell> cellMap,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        var roadNodes = patch.RoadPaths.SelectMany(item => item.Nodes).ToList();
        foreach (var anchor in patch.ObjectAnchors)
        {
            ValidateKnownMicrotile(anchor.SourceMicrotilePreviewId, $"{patch.PatchId}/{anchor.AnchorId}", DeterministicVisualMapPatchComposerFixtures.KnownGoal086MicrotilePreviewIds, diagnostics);
            if (!cellMap.TryGetValue((anchor.X, anchor.Y), out var cell))
            {
                diagnostics.Add(Error("visual_map_patch.object.out_of_bounds", $"{patch.PatchId}/{anchor.AnchorId}", "Object anchors must stay inside the patch."));
                continue;
            }

            if (anchor.RequiresLandCell && !cell.IsPassable)
            {
                diagnostics.Add(Error("visual_map_patch.object.land_invalid", $"{patch.PatchId}/{anchor.AnchorId}", "Object anchor requires a valid passable land cell."));
            }

            if (anchor.RequiresWaterAdjacency
                && !IsWaterLike(cell)
                && !Neighbors(cell, cellMap).Any(IsWaterLike))
            {
                diagnostics.Add(Error("visual_map_patch.object.water_adjacency_missing", $"{patch.PatchId}/{anchor.AnchorId}", "Bridge/dock/water object anchors require valid water adjacency."));
            }

            if (anchor.RequiresRoadAdjacency
                && !roadNodes.Any(node => Distance(node.X, node.Y, anchor.X, anchor.Y) <= 1))
            {
                diagnostics.Add(Error("visual_map_patch.object.road_adjacency_missing", $"{patch.PatchId}/{anchor.AnchorId}", "Object anchors that declare road adjacency must be near a road/path node."));
            }
        }
    }

    private static void ValidateSettlements(
        VisualMapPatchDefinition patch,
        IReadOnlyDictionary<(int X, int Y), VisualMapPatchCell> cellMap,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        foreach (var settlement in patch.SettlementAnchors)
        {
            ValidateKnownMicrotile(settlement.SourceMicrotilePreviewId, $"{patch.PatchId}/{settlement.SettlementId}", DeterministicVisualMapPatchComposerFixtures.KnownGoal086MicrotilePreviewIds, diagnostics);
            if (!cellMap.TryGetValue((settlement.X, settlement.Y), out var cell)
                || !cell.IsPassable
                || cell.WaterKind is VisualMapPatchWaterKind.Sea or VisualMapPatchWaterKind.River or VisualMapPatchWaterKind.Lake)
            {
                diagnostics.Add(Error("visual_map_patch.settlement.land_invalid", $"{patch.PatchId}/{settlement.SettlementId}", "Settlement anchors must be on valid passable land/coast cells."));
            }

            var road = patch.RoadPaths.FirstOrDefault(item => item.PathId == settlement.NearPathId);
            if (road == null || !road.Nodes.Any(node => Distance(node.X, node.Y, settlement.X, settlement.Y) <= 2))
            {
                diagnostics.Add(Error("visual_map_patch.settlement.path_adjacency_missing", $"{patch.PatchId}/{settlement.SettlementId}", "Settlement anchors must be near the declared road/path."));
            }

            if (!string.IsNullOrWhiteSpace(settlement.NearResourceAnchorId)
                && !patch.ObjectAnchors.Any(item => item.AnchorId == settlement.NearResourceAnchorId))
            {
                diagnostics.Add(Error("visual_map_patch.settlement.resource_missing", $"{patch.PatchId}/{settlement.SettlementId}", "Declared settlement resource anchor must exist."));
            }
        }
    }

    private static void ValidateCreatures(VisualMapPatchDefinition patch, List<VisualMapPatchDiagnostic> diagnostics)
    {
        foreach (var marker in patch.CreatureMarkers)
        {
            ValidateKnownMicrotile(marker.SourceMicrotilePreviewId, $"{patch.PatchId}/{marker.MarkerId}", DeterministicVisualMapPatchComposerFixtures.KnownGoal086MicrotilePreviewIds, diagnostics);
            if (string.IsNullOrWhiteSpace(marker.BodyPlanId)
                || string.IsNullOrWhiteSpace(marker.EquipmentProfileId)
                || string.IsNullOrWhiteSpace(marker.StateMetadataId)
                || !marker.RatingSafe)
            {
                diagnostics.Add(Error("visual_map_patch.creature.safe_metadata_invalid", $"{patch.PatchId}/{marker.MarkerId}", "Creature markers require safe bodyplan, equipment and state metadata."));
            }
        }
    }

    private static void ValidateOverlays(
        VisualMapPatchDefinition patch,
        IReadOnlySet<string> knownGoal086MicrotilePreviewIds,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        foreach (var overlay in patch.Overlays)
        {
            ValidateKnownMicrotile(overlay.SourceMicrotilePreviewId, $"{patch.PatchId}/{overlay.OverlayId}", knownGoal086MicrotilePreviewIds, diagnostics);
            if (overlay.AdultMetadataOnly)
            {
                if (string.IsNullOrWhiteSpace(overlay.SafeFallbackMicrotilePreviewId)
                    || !knownGoal086MicrotilePreviewIds.Contains(overlay.SafeFallbackMicrotilePreviewId))
                {
                    diagnostics.Add(Error("visual_map_patch.adult.safe_fallback_missing", $"{patch.PatchId}/{overlay.OverlayId}", "Adult-capable metadata-only overlays require a known safe fallback microtile."));
                }

                if (overlay.ProviderState != VisualMapPatchProviderState.CandidateQuarantine)
                {
                    diagnostics.Add(Error("visual_map_patch.adult.boundary_invalid", $"{patch.PatchId}/{overlay.OverlayId}", "Adult/rating metadata fallback routes must remain metadata-only and quarantined."));
                }
            }

            if (overlay.ProviderState == VisualMapPatchProviderState.CandidateQuarantine
                && overlay.TreatProviderCandidateAsApprovedOutput)
            {
                diagnostics.Add(Error("visual_map_patch.provider_candidate.treated_as_approved", $"{patch.PatchId}/{overlay.OverlayId}", "Provider candidates must not be treated as approved output."));
            }
        }
    }

    private static IEnumerable<VisualMapPatchCell> Neighbors(
        VisualMapPatchCell cell,
        IReadOnlyDictionary<(int X, int Y), VisualMapPatchCell> cellMap)
    {
        foreach (var point in new[] { (cell.X, cell.Y - 1), (cell.X + 1, cell.Y), (cell.X, cell.Y + 1), (cell.X - 1, cell.Y) })
        {
            if (cellMap.TryGetValue(point, out var neighbor))
            {
                yield return neighbor;
            }
        }
    }

    private static bool IsWaterLike(VisualMapPatchCell cell) =>
        cell.WaterKind != VisualMapPatchWaterKind.None || cell.TerrainBiome == VisualMapPatchTerrainBiome.Water;

    private static void ValidateAdjacentPath(
        IReadOnlyList<VisualMapPatchPathNode> nodes,
        string target,
        string code,
        string message,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        for (var index = 1; index < nodes.Count; index++)
        {
            if (Distance(nodes[index - 1].X, nodes[index - 1].Y, nodes[index].X, nodes[index].Y) != 1)
            {
                diagnostics.Add(Error(code, target, message));
                return;
            }
        }
    }

    private static int Distance(int ax, int ay, int bx, int by) =>
        Math.Abs(ax - bx) + Math.Abs(ay - by);

    private static void ValidateKnownMicrotile(
        string previewId,
        string target,
        IReadOnlySet<string> knownGoal086MicrotilePreviewIds,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(previewId) || !knownGoal086MicrotilePreviewIds.Contains(previewId))
        {
            diagnostics.Add(Error("visual_map_patch.microtile_ref.unknown", target, "Every map patch reference must point to a known Goal086 microtile preview id."));
        }
    }

    private static void ValidateSvg(string patchId, string svg, List<VisualMapPatchDiagnostic> diagnostics)
    {
        if (!IsSvgSafe(svg))
        {
            diagnostics.Add(Error("visual_map_patch.svg.unsafe", patchId, "SVG must be script-free, external-resource-free and base64-free."));
        }

        if (CountSvgRects(svg) < 24 * 16)
        {
            diagnostics.Add(Error("visual_map_patch.svg.cell_rects_missing", patchId, "SVG patch previews must include one text rect per patch cell."));
        }
    }

    private static void ValidateDuplicates<T>(
        IEnumerable<T> items,
        Func<T, string> idSelector,
        string code,
        string message,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        foreach (var duplicate in items
            .Where(item => !string.IsNullOrWhiteSpace(idSelector(item)))
            .GroupBy(idSelector, StringComparer.Ordinal)
            .Where(item => item.Count() > 1))
        {
            diagnostics.Add(Error(code, duplicate.Key, message));
        }
    }

    private static void ValidateId(
        string id,
        string code,
        string target,
        string message,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id) || !StableIdRegex.IsMatch(id))
        {
            diagnostics.Add(Error(code, string.IsNullOrWhiteSpace(target) ? "<empty>" : target, message));
        }
    }

    private static void ValidateRequiredText(
        string text,
        string code,
        string target,
        string message,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static void ValidateRelativePath(
        string relativePath,
        string code,
        string target,
        string message,
        List<VisualMapPatchDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(relativePath)
            || Path.IsPathRooted(relativePath)
            || relativePath.Contains(':', StringComparison.Ordinal)
            || relativePath.Contains('\\', StringComparison.Ordinal)
            || relativePath.Split('/').Any(segment => segment == ".." || string.IsNullOrWhiteSpace(segment)))
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static int Count(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }

    private static VisualMapPatchDiagnostic Error(string code, string target, string message) =>
        VisualMapPatchDiagnostic.Error(code, target, message);
}
