using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.DeterministicVisualRegionComposer;

public static class DeterministicVisualRegionComposerValidator
{
    private static readonly Regex StableIdRegex = new("^[a-z0-9][a-z0-9_.-]*$", RegexOptions.Compiled);

    public static VisualRegionValidationResult Validate(
        VisualRegionDefinition definition,
        IReadOnlySet<string>? knownGoal087PatchIds = null,
        IReadOnlyDictionary<string, string>? overviewSvgByFileName = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        knownGoal087PatchIds ??= DeterministicVisualRegionComposerFixtures.KnownGoal087PatchIds;
        var diagnostics = new List<VisualRegionDiagnostic>();

        ValidateId(definition.RegionId, "visual_region.region_id.invalid", definition.RegionId, "Region id must be stable and lowercase.", diagnostics);
        ValidateRelativePath(definition.OutputRelativeDirectory, "visual_region.path.absolute", definition.RegionId, "Output directory must be a safe relative path.", diagnostics);
        if (definition.Width != DeterministicVisualRegionComposerVocabulary.RegionWidth
            || definition.Height != DeterministicVisualRegionComposerVocabulary.RegionHeight
            || definition.DerivedLogicalCellCount != DeterministicVisualRegionComposerVocabulary.DerivedLogicalCellCount)
        {
            diagnostics.Add(Error("visual_region.dimensions.invalid", definition.RegionId, "Region must prove 144x144x2 logical dimensions and 41,472 derived cells."));
        }

        if (definition.LayerCount != DeterministicVisualRegionComposerVocabulary.LayerCount
            || definition.Layers.Count != DeterministicVisualRegionComposerVocabulary.LayerCount
            || !definition.Layers.Any(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.SurfaceLayerId)
            || !definition.Layers.Any(item => item.LayerId == DeterministicVisualRegionComposerVocabulary.UndergroundLayerId))
        {
            diagnostics.Add(Error("visual_region.layer_count.invalid", definition.RegionId, "Region must contain exactly surface and underground layers."));
        }

        if (definition.PatchWidth != DeterministicVisualRegionComposerVocabulary.PatchWidth
            || definition.PatchHeight != DeterministicVisualRegionComposerVocabulary.PatchHeight
            || definition.PatchGridColumns != DeterministicVisualRegionComposerVocabulary.PatchGridColumns
            || definition.PatchGridRows != DeterministicVisualRegionComposerVocabulary.PatchGridRows)
        {
            diagnostics.Add(Error("visual_region.patch_grid.invalid", definition.RegionId, "Region patch grid must be 6x9 placements of 24x16 patches."));
        }

        if (definition.HeavyRawCellMode || definition.ExplicitRawCellRecordCount >= DeterministicVisualRegionComposerVocabulary.DerivedLogicalCellCount)
        {
            diagnostics.Add(Error("visual_region.heavy_raw_cells.forbidden", definition.RegionId, "Goal 088 must not emit heavy explicit 41,472 raw cell artifacts."));
        }

        if (definition.PromptTextIsSourceOfTruth
            || string.Equals(definition.SourceOfTruthKind, "provider_prompt_text", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("visual_region.prompt.source_of_truth", definition.RegionId, "Prompt text must not be visual region source of truth."));
        }

        if (definition.TreatProviderCandidateAsApprovedOutput)
        {
            diagnostics.Add(Error("visual_region.provider_candidate.treated_as_approved", definition.RegionId, "Provider candidates must not be treated as approved output."));
        }

        ValidateLayers(definition, knownGoal087PatchIds, diagnostics);
        ValidateWaterConnectors(definition, diagnostics);
        ValidateWaterNetwork(definition, diagnostics);
        ValidateRoadReachability(definition, diagnostics);
        ValidateGateTransitions(definition, diagnostics);
        ValidateSettlements(definition, diagnostics);
        ValidateObjects(definition, diagnostics);
        ValidateCreatures(definition, diagnostics);
        ValidateOverlays(definition, diagnostics);
        ValidateSourceLineage(definition, diagnostics);

        if (overviewSvgByFileName != null)
        {
            ValidateOverviewSvg("region-overview-surface.svg", overviewSvgByFileName, 54, diagnostics);
            ValidateOverviewSvg("region-overview-underground.svg", overviewSvgByFileName, 54, diagnostics);
            ValidateOverviewSvg("region-overview-combined.svg", overviewSvgByFileName, 108, diagnostics);
        }

        return new VisualRegionValidationResult
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            DiagnosticCount = diagnostics.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<VisualRegionDiagnostic> SortDiagnostics(IEnumerable<VisualRegionDiagnostic> diagnostics) =>
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

    private static void ValidateLayers(
        VisualRegionDefinition definition,
        IReadOnlySet<string> knownGoal087PatchIds,
        List<VisualRegionDiagnostic> diagnostics)
    {
        foreach (var layer in definition.Layers.OrderBy(item => item.LayerId, StringComparer.Ordinal))
        {
            ValidateId(layer.LayerId, "visual_region.layer_id.invalid", layer.LayerId, "Layer id must be stable and lowercase.", diagnostics);
            if (layer.Width != definition.Width
                || layer.Height != definition.Height
                || layer.PatchGridColumns != DeterministicVisualRegionComposerVocabulary.PatchGridColumns
                || layer.PatchGridRows != DeterministicVisualRegionComposerVocabulary.PatchGridRows)
            {
                diagnostics.Add(Error("visual_region.patch_grid.invalid", layer.LayerId, "Each layer must be 144x144 with a 6x9 patch grid."));
            }

            if (layer.PatchPlacements.Count != DeterministicVisualRegionComposerVocabulary.PatchPlacementsPerLayer)
            {
                diagnostics.Add(Error("visual_region.patch_placement_count.invalid", layer.LayerId, "Each layer must contain exactly 54 patch placements."));
            }

            if (layer.Chunks.Count != layer.PatchPlacements.Count)
            {
                diagnostics.Add(Error("visual_region.chunk_index.invalid", layer.LayerId, "Chunk index must mirror patch placements."));
            }

            ValidateDuplicatePlacementCoordinates(layer, diagnostics);
            foreach (var placement in layer.PatchPlacements.OrderBy(item => item.GridY).ThenBy(item => item.GridX))
            {
                ValidatePlacement(definition, placement, knownGoal087PatchIds, diagnostics);
            }
        }
    }

    private static void ValidateDuplicatePlacementCoordinates(
        VisualRegionLayer layer,
        List<VisualRegionDiagnostic> diagnostics)
    {
        foreach (var duplicate in layer.PatchPlacements
            .GroupBy(item => (item.LayerId, item.GridX, item.GridY))
            .Where(item => item.Count() > 1))
        {
            diagnostics.Add(Error("visual_region.placement.coordinate.duplicate", $"{duplicate.Key.LayerId}/{duplicate.Key.GridX}/{duplicate.Key.GridY}", "Patch coordinates must be unique per layer."));
        }
    }

    private static void ValidatePlacement(
        VisualRegionDefinition definition,
        VisualRegionPatchPlacement placement,
        IReadOnlySet<string> knownGoal087PatchIds,
        List<VisualRegionDiagnostic> diagnostics)
    {
        ValidateId(placement.PlacementId, "visual_region.placement_id.invalid", placement.PlacementId, "Placement id must be stable and lowercase.", diagnostics);
        if (!knownGoal087PatchIds.Contains(placement.SourceGoal087PatchId))
        {
            diagnostics.Add(Error("visual_region.patch_id.unknown", placement.PlacementId, "Every region placement must reference a known Goal087 patch id."));
        }

        if (placement.Width != DeterministicVisualRegionComposerVocabulary.PatchWidth
            || placement.Height != DeterministicVisualRegionComposerVocabulary.PatchHeight
            || placement.GridX < 0
            || placement.GridY < 0
            || placement.GridX >= DeterministicVisualRegionComposerVocabulary.PatchGridColumns
            || placement.GridY >= DeterministicVisualRegionComposerVocabulary.PatchGridRows
            || placement.X != placement.GridX * DeterministicVisualRegionComposerVocabulary.PatchWidth
            || placement.Y != placement.GridY * DeterministicVisualRegionComposerVocabulary.PatchHeight
            || placement.X + placement.Width > definition.Width
            || placement.Y + placement.Height > definition.Height)
        {
            diagnostics.Add(Error("visual_region.placement.bounds.invalid", placement.PlacementId, "Patch placement must stay inside 144x144 layer bounds and align to 24x16 grid."));
        }

        if (placement.Transform.RotationDegrees % 90 != 0
            || placement.Transform.RotationDegrees < 0
            || placement.Transform.RotationDegrees > 270
            || string.IsNullOrWhiteSpace(placement.Transform.RepaletteProfileId))
        {
            diagnostics.Add(Error("visual_region.placement.transform.invalid", placement.PlacementId, "Patch transform must be deterministic rotate/mirror/repalette metadata only."));
        }
    }

    private static void ValidateWaterConnectors(VisualRegionDefinition definition, List<VisualRegionDiagnostic> diagnostics)
    {
        foreach (var layer in definition.Layers)
        {
            var map = layer.PatchPlacements
                .GroupBy(item => (item.GridX, item.GridY))
                .ToDictionary(item => item.Key, item => item.First());
            foreach (var placement in layer.PatchPlacements)
            {
                if (map.TryGetValue((placement.GridX + 1, placement.GridY), out var east))
                {
                    ValidateConnectorPair(placement.PlacementId, east.PlacementId, placement.WaterConnectors.East, east.WaterConnectors.West, diagnostics);
                }

                if (map.TryGetValue((placement.GridX, placement.GridY + 1), out var south))
                {
                    ValidateConnectorPair(placement.PlacementId, south.PlacementId, placement.WaterConnectors.South, south.WaterConnectors.North, diagnostics);
                }
            }
        }
    }

    private static void ValidateConnectorPair(
        string firstPlacementId,
        string secondPlacementId,
        string firstConnector,
        string secondConnector,
        List<VisualRegionDiagnostic> diagnostics)
    {
        var first = NormalizeConnector(firstConnector);
        var second = NormalizeConnector(secondConnector);
        if (first == "none" && second == "none")
        {
            return;
        }

        if (!string.Equals(first, second, StringComparison.Ordinal))
        {
            diagnostics.Add(Error("visual_region.water.connector_mismatch", $"{firstPlacementId}->{secondPlacementId}", "Water/coast/river/lake/marsh connector metadata must match across patch boundaries."));
        }
    }

    private static void ValidateWaterNetwork(VisualRegionDefinition definition, List<VisualRegionDiagnostic> diagnostics)
    {
        var declaredWaterKinds = definition.Layers
            .SelectMany(item => item.PatchPlacements)
            .SelectMany(item => item.DeclaredWaterKinds)
            .Where(item => !string.Equals(item, "none", StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.Ordinal);
        if (declaredWaterKinds.Count > 0 && (!definition.WaterNetwork.DeclaresWater || definition.WaterNetwork.Segments.Count == 0))
        {
            diagnostics.Add(Error("visual_region.water_network.missing", definition.RegionId, "Declared water, coast, river, marsh or lava boundary metadata requires a water network proof."));
        }

        foreach (var segment in definition.WaterNetwork.Segments)
        {
            if (segment.ConnectedPlacementIds.Count == 0 || !segment.BoundaryConnectorsValid)
            {
                diagnostics.Add(Error("visual_region.water_network.segment_invalid", segment.SegmentId, "Water network segments require connected placements and valid boundary connectors."));
            }
        }

        if (declaredWaterKinds.Contains("lava_boundary") && !definition.WaterNetwork.DeclaresLavaBoundaryMetadata)
        {
            diagnostics.Add(Error("visual_region.water_network.lava_boundary_missing", definition.RegionId, "Underground lava boundary metadata must be declared explicitly."));
        }
    }

    private static void ValidateRoadReachability(VisualRegionDefinition definition, List<VisualRegionDiagnostic> diagnostics)
    {
        var nodeIds = definition.RoadNetwork.Nodes.Select(item => item.NodeId).ToHashSet(StringComparer.Ordinal);
        foreach (var edge in definition.RoadNetwork.Edges)
        {
            if (!nodeIds.Contains(edge.FromNodeId) || !nodeIds.Contains(edge.ToNodeId))
            {
                diagnostics.Add(Error("visual_region.road.edge_unknown_node", edge.EdgeId, "Road network edges must reference known nodes."));
            }
        }

        var required = definition.RoadNetwork.Nodes.Where(item => item.RequiredAnchor).Select(item => item.NodeId).ToHashSet(StringComparer.Ordinal);
        foreach (var settlement in definition.Settlements)
        {
            if (string.IsNullOrWhiteSpace(settlement.RoadNodeId) || !nodeIds.Contains(settlement.RoadNodeId))
            {
                diagnostics.Add(Error("visual_region.road.anchor_missing", settlement.SettlementId, "Settlement/castle/garrison/caravan anchors require a road node."));
            }
            else
            {
                required.Add(settlement.RoadNodeId);
            }
        }

        foreach (var placement in definition.ObjectPlacements.Where(item => item.RequiresRoadConnection))
        {
            if (string.IsNullOrWhiteSpace(placement.RoadNodeId) || !nodeIds.Contains(placement.RoadNodeId))
            {
                diagnostics.Add(Error("visual_region.road.anchor_missing", placement.ObjectId, "Object anchors that require roads must reference a road node."));
            }
            else
            {
                required.Add(placement.RoadNodeId);
            }
        }

        var reachable = ReachableRoadNodes(definition.RoadNetwork);
        foreach (var requiredNode in required)
        {
            if (!reachable.Contains(requiredNode))
            {
                diagnostics.Add(Error("visual_region.road.reachability.disconnected", requiredNode, "Road network must connect settlement, castle, garrison, caravan and object anchors."));
            }
        }
    }

    private static void ValidateGateTransitions(VisualRegionDefinition definition, List<VisualRegionDiagnostic> diagnostics)
    {
        var nodeIds = definition.RoadNetwork.Nodes.Select(item => item.NodeId).ToHashSet(StringComparer.Ordinal);
        foreach (var transition in definition.GateTransitions)
        {
            if (!transition.Paired
                || string.IsNullOrWhiteSpace(transition.SurfaceGateId)
                || string.IsNullOrWhiteSpace(transition.UndergroundGateId)
                || !nodeIds.Contains(transition.SurfaceGateId)
                || !nodeIds.Contains(transition.UndergroundGateId))
            {
                diagnostics.Add(Error("visual_region.gate_transition.pair_missing", transition.TransitionId, "Surface-underground transitions require paired surface and underground gates."));
            }
        }
    }

    private static void ValidateSettlements(VisualRegionDefinition definition, List<VisualRegionDiagnostic> diagnostics)
    {
        var placementIds = PlacementIds(definition);
        foreach (var settlement in definition.Settlements)
        {
            if (!placementIds.Contains(settlement.PlacementId))
            {
                diagnostics.Add(Error("visual_region.settlement.placement_missing", settlement.SettlementId, "Settlement placement must reference a known patch placement."));
            }

            if (IsInvalidSettlementTerrain(settlement.TerrainKind))
            {
                diagnostics.Add(Error("visual_region.settlement.terrain.invalid", settlement.SettlementId, "Settlement and castle anchors must not be placed on invalid water, lava or impassable terrain."));
            }
        }
    }

    private static void ValidateObjects(VisualRegionDefinition definition, List<VisualRegionDiagnostic> diagnostics)
    {
        var placementIds = PlacementIds(definition);
        foreach (var placement in definition.ObjectPlacements)
        {
            if (!placementIds.Contains(placement.PlacementId))
            {
                diagnostics.Add(Error("visual_region.object.placement_missing", placement.ObjectId, "Object placement must reference a known patch placement."));
            }

            if (placement.RequiresPassableTerrain && IsInvalidSettlementTerrain(placement.TerrainKind))
            {
                diagnostics.Add(Error("visual_region.object.terrain.invalid", placement.ObjectId, "Object placement requires valid terrain unless it is a declared bridge/dock crossing."));
            }
        }
    }

    private static void ValidateCreatures(VisualRegionDefinition definition, List<VisualRegionDiagnostic> diagnostics)
    {
        var placementIds = PlacementIds(definition);
        foreach (var creature in definition.CreaturePlacements)
        {
            if (!placementIds.Contains(creature.PlacementId))
            {
                diagnostics.Add(Error("visual_region.creature.placement_missing", creature.CreatureId, "Creature placement must reference a known patch placement."));
            }

            if (string.IsNullOrWhiteSpace(creature.BodyPlanId)
                || string.IsNullOrWhiteSpace(creature.EquipmentProfileId)
                || string.IsNullOrWhiteSpace(creature.StateMetadataId)
                || !creature.RatingSafe)
            {
                diagnostics.Add(Error("visual_region.creature.metadata.missing", creature.CreatureId, "Creature placement requires safe bodyplan, equipment and state metadata."));
            }
        }
    }

    private static void ValidateOverlays(VisualRegionDefinition definition, List<VisualRegionDiagnostic> diagnostics)
    {
        foreach (var overlay in definition.Overlays)
        {
            if (overlay.AdultMetadataOnly)
            {
                if (string.IsNullOrWhiteSpace(overlay.SafeFallbackRefId))
                {
                    diagnostics.Add(Error("visual_region.adult.safe_fallback_missing", overlay.OverlayId, "Adult/rating metadata-only overlays require a safe fallback."));
                }

                if (overlay.ProviderState != VisualRegionProviderState.CandidateQuarantine)
                {
                    diagnostics.Add(Error("visual_region.adult.boundary_invalid", overlay.OverlayId, "Adult-capable metadata must remain safe-fallback-bound and quarantined."));
                }
            }

            if (overlay.ProviderState == VisualRegionProviderState.CandidateQuarantine
                && overlay.TreatProviderCandidateAsApprovedOutput)
            {
                diagnostics.Add(Error("visual_region.provider_candidate.treated_as_approved", overlay.OverlayId, "Provider candidates must not be treated as approved output."));
            }
        }
    }

    private static void ValidateSourceLineage(VisualRegionDefinition definition, List<VisualRegionDiagnostic> diagnostics)
    {
        if (!definition.SourceGoal084085086087LineageRequired)
        {
            return;
        }

        var sourceKinds = definition.SourceReferences.Select(item => item.SourceKind).ToHashSet(StringComparer.Ordinal);
        foreach (var required in new[] { "goal084", "goal085", "goal086", "goal087" })
        {
            if (!sourceKinds.Contains(required))
            {
                diagnostics.Add(Error("visual_region.source_lineage.missing", definition.RegionId, "Region must trace to Goal084, Goal085, Goal086 and Goal087 evidence."));
            }
        }

        foreach (var source in definition.SourceReferences)
        {
            ValidateRelativePath(source.RelativePath, "visual_region.path.absolute", source.SourceId, "Source lineage path must be relative and safe.", diagnostics);
        }
    }

    private static void ValidateOverviewSvg(
        string fileName,
        IReadOnlyDictionary<string, string> overviewSvgByFileName,
        int minRectCount,
        List<VisualRegionDiagnostic> diagnostics)
    {
        if (!overviewSvgByFileName.TryGetValue(fileName, out var svg))
        {
            diagnostics.Add(Error("visual_region.svg.missing", fileName, "Region overview SVG is required."));
            return;
        }

        if (!IsSvgSafe(svg) || CountSvgRects(svg) < minRectCount)
        {
            diagnostics.Add(Error("visual_region.svg.unsafe", fileName, "Overview SVG must be text-only, script-free, external-resource-free, base64-free and include patch blocks."));
        }
    }

    private static HashSet<string> ReachableRoadNodes(VisualRegionRoadNetwork roadNetwork)
    {
        var requiredStart = roadNetwork.Nodes.FirstOrDefault(item => item.RequiredAnchor)?.NodeId
            ?? roadNetwork.Nodes.FirstOrDefault()?.NodeId
            ?? string.Empty;
        var reachable = new HashSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(requiredStart))
        {
            return reachable;
        }

        var adjacency = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var node in roadNetwork.Nodes)
        {
            adjacency[node.NodeId] = [];
        }

        foreach (var edge in roadNetwork.Edges)
        {
            if (!adjacency.ContainsKey(edge.FromNodeId) || !adjacency.ContainsKey(edge.ToNodeId))
            {
                continue;
            }

            adjacency[edge.FromNodeId].Add(edge.ToNodeId);
            adjacency[edge.ToNodeId].Add(edge.FromNodeId);
        }

        var queue = new Queue<string>();
        queue.Enqueue(requiredStart);
        reachable.Add(requiredStart);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in adjacency[current].OrderBy(item => item, StringComparer.Ordinal))
            {
                if (reachable.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return reachable;
    }

    private static HashSet<string> PlacementIds(VisualRegionDefinition definition) =>
        definition.Layers.SelectMany(item => item.PatchPlacements).Select(item => item.PlacementId).ToHashSet(StringComparer.Ordinal);

    private static bool IsInvalidSettlementTerrain(string terrain) =>
        terrain.Contains("impassable", StringComparison.OrdinalIgnoreCase)
        || terrain.Contains("deep_water", StringComparison.OrdinalIgnoreCase)
        || terrain.Equals("water", StringComparison.OrdinalIgnoreCase)
        || terrain.Equals("lava", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeConnector(string connector) =>
        string.IsNullOrWhiteSpace(connector) ? "none" : connector.Trim().ToLowerInvariant();

    private static void ValidateId(
        string id,
        string code,
        string target,
        string message,
        List<VisualRegionDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id) || !StableIdRegex.IsMatch(id))
        {
            diagnostics.Add(Error(code, string.IsNullOrWhiteSpace(target) ? "<empty>" : target, message));
        }
    }

    private static void ValidateRelativePath(
        string relativePath,
        string code,
        string target,
        string message,
        List<VisualRegionDiagnostic> diagnostics)
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

    private static VisualRegionDiagnostic Error(string code, string target, string message) =>
        VisualRegionDiagnostic.Error(code, target, message);
}
