using LLMGameCreator.Application.Design.ParameterizedVisualWorldProfiles;

namespace LLMGameCreator.Application.Design.DeterministicVisualChunkStreamWindow;

public static class DeterministicVisualChunkStreamWindowMaterializer
{
    public static IReadOnlyList<VisualChunkStreamWindow> MaterializeAll(
        IReadOnlyList<VisualChunkStreamRequest> requests,
        IReadOnlyList<VisualWorldProfile> profiles) =>
        requests
            .OrderBy(item => item.FixtureId, StringComparer.Ordinal)
            .ThenBy(item => item.WindowId, StringComparer.Ordinal)
            .Select(request => Materialize(request, profiles.Single(profile => profile.ProfileId == request.ProfileId)))
            .ToList();

    public static VisualChunkStreamWindow Materialize(VisualChunkStreamRequest request, VisualWorldProfile profile)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(profile);

        var layerIdSet = request.LayerIds.Count == 0
            ? new HashSet<string>([request.LayerId], StringComparer.Ordinal)
            : request.LayerIds.ToHashSet(StringComparer.Ordinal);
        var selectedLayers = profile.Layers
            .Where(item => layerIdSet.Contains(item.LayerId))
            .OrderBy(item => item.Order)
            .ThenBy(item => item.LayerId, StringComparer.Ordinal)
            .ToList();

        var finiteWidth = request.FiniteWidthOverride ?? profile.FiniteWidth;
        var finiteHeight = request.FiniteHeightOverride ?? profile.FiniteHeight;
        var chunkColumns = finiteWidth.HasValue ? CeilingDivide(finiteWidth.Value, profile.ChunkProfile.ChunkWidth) : (long?)null;
        var chunkRows = finiteHeight.HasValue ? CeilingDivide(finiteHeight.Value, profile.ChunkProfile.ChunkHeight) : (long?)null;
        var requestedMinX = request.CenterChunkX - request.RadiusChunks;
        var requestedMinY = request.CenterChunkY - request.RadiusChunks;
        var requestedMaxX = request.CenterChunkX + request.RadiusChunks;
        var requestedMaxY = request.CenterChunkY + request.RadiusChunks;

        var materializedMinX = requestedMinX;
        var materializedMinY = requestedMinY;
        var materializedMaxX = requestedMaxX;
        var materializedMaxY = requestedMaxY;
        var clipped = false;

        if (request.BoundaryPolicy == VisualChunkStreamBoundaryPolicy.ClipToFiniteBounds
            && chunkColumns is > 0
            && chunkRows is > 0)
        {
            materializedMinX = Math.Max(0, requestedMinX);
            materializedMinY = Math.Max(0, requestedMinY);
            materializedMaxX = Math.Min(chunkColumns.Value - 1, requestedMaxX);
            materializedMaxY = Math.Min(chunkRows.Value - 1, requestedMaxY);
            clipped = materializedMinX != requestedMinX
                || materializedMinY != requestedMinY
                || materializedMaxX != requestedMaxX
                || materializedMaxY != requestedMaxY;
        }

        var layerRefs = BuildLayerRefs(selectedLayers, profile.LayerLinks);
        var layerLinks = BuildLayerLinks(profile, layerIdSet);
        var chunks = BuildChunks(request, profile, selectedLayers, materializedMinX, materializedMinY, materializedMaxX, materializedMaxY);
        var seams = BuildSeams(chunks);
        var overlays = request.DeltaOverlay == null ? [] : new[] { request.DeltaOverlay };
        var windowWithoutHash = new VisualChunkStreamWindow
        {
            FixtureId = request.FixtureId,
            WindowId = request.WindowId,
            ProfileId = request.ProfileId,
            Mode = request.Mode,
            LayerId = request.LayerId,
            LayerIds = selectedLayers.Select(item => item.LayerId).ToList(),
            WorldSeed = request.WorldSeed,
            GeneratorVersion = request.GeneratorVersion,
            CenterChunkX = request.CenterChunkX,
            CenterChunkY = request.CenterChunkY,
            RadiusChunks = request.RadiusChunks,
            BoundaryPolicy = request.BoundaryPolicy,
            RequestedMinChunkX = requestedMinX,
            RequestedMinChunkY = requestedMinY,
            RequestedMaxChunkX = requestedMaxX,
            RequestedMaxChunkY = requestedMaxY,
            MaterializedMinChunkX = materializedMinX,
            MaterializedMinChunkY = materializedMinY,
            MaterializedMaxChunkX = materializedMaxX,
            MaterializedMaxChunkY = materializedMaxY,
            ClippedAtFiniteBoundary = clipped,
            EffectiveFiniteWidth = finiteWidth,
            EffectiveFiniteHeight = finiteHeight,
            EffectiveChunkColumns = chunkColumns,
            EffectiveChunkRows = chunkRows,
            EstimatedFullWorldChunkCapacity = EstimateFullWorldChunkCapacity(profile, finiteWidth, finiteHeight),
            ChunkCount = chunks.Count,
            NoRawFullWorldDump = !request.AttemptsRawFullWorldDump && chunks.Count <= DeterministicVisualChunkStreamWindowVocabulary.RawDumpChunkThreshold,
            AttemptsRawFullWorldDump = request.AttemptsRawFullWorldDump,
            PromptTextIsSourceOfTruth = request.PromptTextIsSourceOfTruth,
            SourceOfTruthKind = request.SourceOfTruthKind,
            ContainsAbsolutePath = request.ContainsAbsolutePath,
            ContainsAdultRatingMetadata = request.ContainsAdultRatingMetadata,
            SafeFallbackRefId = request.SafeFallbackRefId,
            Layers = layerRefs,
            Chunks = chunks,
            Seams = seams,
            LayerLinks = layerLinks,
            DeltaOverlays = overlays
        };

        return windowWithoutHash with { WindowHash = ComputeWindowHash(windowWithoutHash) };
    }

    public static string ComputeWindowHash(VisualChunkStreamWindow window)
    {
        var chunkPayload = string.Join(
            "|",
            window.Chunks
                .OrderBy(item => item.LayerId, StringComparer.Ordinal)
                .ThenBy(item => item.ChunkY)
                .ThenBy(item => item.ChunkX)
                .Select(item => $"{item.LayerId}:{item.ChunkX}:{item.ChunkY}:{item.ChunkKey}:{item.NeighborSeamKeys.North}:{item.NeighborSeamKeys.South}:{item.NeighborSeamKeys.East}:{item.NeighborSeamKeys.West}"));
        var seamPayload = string.Join(
            "|",
            window.Seams
                .OrderBy(item => item.LayerId, StringComparer.Ordinal)
                .ThenBy(item => item.FromChunkY)
                .ThenBy(item => item.FromChunkX)
                .ThenBy(item => item.Direction, StringComparer.Ordinal)
                .Select(item => $"{item.LayerId}:{item.FromChunkX}:{item.FromChunkY}:{item.Direction}:{item.SeamKey}:{item.WaterConnector}:{item.RoadConnector}:{item.BiomeBand}"));
        var linkPayload = string.Join(
            "|",
            window.LayerLinks
                .OrderBy(item => item.LinkId, StringComparer.Ordinal)
                .Select(item => $"{item.LinkId}:{item.FromLayerId}:{item.ToLayerId}:{item.LinkKind}"));
        return DeterministicVisualChunkStreamWindowHash.Compute($"{window.WindowId}|{window.ProfileId}|{window.WorldSeed}|{window.GeneratorVersion}|{chunkPayload}|{seamPayload}|{linkPayload}");
    }

    public static string EdgeSeamKey(
        string profileId,
        string worldSeed,
        string generatorVersion,
        string layerId,
        string axis,
        long boundary,
        long lane) =>
        DeterministicVisualChunkStreamWindowHash.Compute($"{profileId}|{worldSeed}|{generatorVersion}|{layerId}|seam|{axis}|{boundary}|{lane}");

    public static string ConnectorFromSeam(string seamKey, string family)
    {
        var bucket = Convert.ToInt32(seamKey[..2], 16) % 4;
        return family switch
        {
            "water" => bucket switch
            {
                0 => "water_closed",
                1 => "water_stream",
                2 => "water_shore",
                _ => "water_marsh"
            },
            "road" => bucket switch
            {
                0 => "road_none",
                1 => "road_trail",
                2 => "road_bridge",
                _ => "road_gate"
            },
            _ => $"biome_band_{bucket}"
        };
    }

    private static IReadOnlyList<VisualChunkStreamChunkRef> BuildChunks(
        VisualChunkStreamRequest request,
        VisualWorldProfile profile,
        IReadOnlyList<VisualWorldLayerProfile> selectedLayers,
        long minX,
        long minY,
        long maxX,
        long maxY)
    {
        var chunks = new List<VisualChunkStreamChunkRef>();
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                foreach (var layer in selectedLayers)
                {
                    var chunkKey = ParameterizedVisualWorldProfilesValidator.CreateChunkKey(
                        request.ProfileId,
                        request.WorldSeed,
                        request.GeneratorVersion,
                        layer.LayerId,
                        x,
                        y);
                    var seamKeys = new VisualChunkStreamNeighborSeamKeys
                    {
                        North = EdgeSeamKey(request.ProfileId, request.WorldSeed, request.GeneratorVersion, layer.LayerId, "y", y, x),
                        South = EdgeSeamKey(request.ProfileId, request.WorldSeed, request.GeneratorVersion, layer.LayerId, "y", y + 1, x),
                        West = EdgeSeamKey(request.ProfileId, request.WorldSeed, request.GeneratorVersion, layer.LayerId, "x", x, y),
                        East = EdgeSeamKey(request.ProfileId, request.WorldSeed, request.GeneratorVersion, layer.LayerId, "x", x + 1, y)
                    };
                    var hashPayload = $"{chunkKey.Key}|{seamKeys.North}|{seamKeys.South}|{seamKeys.East}|{seamKeys.West}";
                    chunks.Add(new VisualChunkStreamChunkRef
                    {
                        WindowId = request.WindowId,
                        ProfileId = profile.ProfileId,
                        LayerId = layer.LayerId,
                        ChunkX = x,
                        ChunkY = y,
                        ChunkKey = chunkKey.Key,
                        DeterministicChunkHash = DeterministicVisualChunkStreamWindowHash.Compute(hashPayload),
                        NeighborSeamKeys = seamKeys,
                        WaterContinuitySummary = ConnectorFromSeam(seamKeys.East, "water"),
                        RoadContinuitySummary = ConnectorFromSeam(seamKeys.South, "road"),
                        BiomeContinuitySummary = ConnectorFromSeam(seamKeys.North, "biome")
                    });
                }
            }
        }

        return chunks
            .OrderBy(item => item.WindowId, StringComparer.Ordinal)
            .ThenBy(item => item.LayerId, StringComparer.Ordinal)
            .ThenBy(item => item.ChunkY)
            .ThenBy(item => item.ChunkX)
            .ToList();
    }

    private static IReadOnlyList<VisualChunkStreamSeam> BuildSeams(IReadOnlyList<VisualChunkStreamChunkRef> chunks)
    {
        var byAddress = chunks.ToDictionary(item => (item.LayerId, item.ChunkX, item.ChunkY), item => item);
        var seams = new List<VisualChunkStreamSeam>();
        foreach (var chunk in chunks.OrderBy(item => item.LayerId, StringComparer.Ordinal).ThenBy(item => item.ChunkY).ThenBy(item => item.ChunkX))
        {
            if (byAddress.TryGetValue((chunk.LayerId, chunk.ChunkX + 1, chunk.ChunkY), out var east))
            {
                seams.Add(BuildSeam(chunk, east, "east", chunk.NeighborSeamKeys.East));
            }

            if (byAddress.TryGetValue((chunk.LayerId, chunk.ChunkX, chunk.ChunkY + 1), out var south))
            {
                seams.Add(BuildSeam(chunk, south, "south", chunk.NeighborSeamKeys.South));
            }
        }

        return seams;
    }

    private static VisualChunkStreamSeam BuildSeam(
        VisualChunkStreamChunkRef from,
        VisualChunkStreamChunkRef to,
        string direction,
        string seamKey)
    {
        var water = ConnectorFromSeam(seamKey, "water");
        var road = ConnectorFromSeam(seamKey, "road");
        var biome = ConnectorFromSeam(seamKey, "biome");
        var reverseKey = direction == "east" ? to.NeighborSeamKeys.West : to.NeighborSeamKeys.North;

        return new VisualChunkStreamSeam
        {
            WindowId = from.WindowId,
            LayerId = from.LayerId,
            Direction = direction,
            FromChunkKey = from.ChunkKey,
            ToChunkKey = to.ChunkKey,
            FromChunkX = from.ChunkX,
            FromChunkY = from.ChunkY,
            ToChunkX = to.ChunkX,
            ToChunkY = to.ChunkY,
            SeamKey = seamKey,
            WaterConnector = water,
            RoadConnector = road,
            BiomeBand = biome,
            WaterContinuityPassed = string.Equals(seamKey, reverseKey, StringComparison.Ordinal),
            RoadContinuityPassed = string.Equals(seamKey, reverseKey, StringComparison.Ordinal),
            BiomeContinuityPassed = string.Equals(seamKey, reverseKey, StringComparison.Ordinal)
        };
    }

    private static IReadOnlyList<VisualChunkStreamLayerRef> BuildLayerRefs(
        IReadOnlyList<VisualWorldLayerProfile> selectedLayers,
        IReadOnlyList<VisualLayerLink> links)
    {
        var selectedIds = selectedLayers.Select(item => item.LayerId).ToHashSet(StringComparer.Ordinal);
        return selectedLayers
            .Select(layer => new VisualChunkStreamLayerRef
            {
                LayerId = layer.LayerId,
                LayerKind = layer.LayerKind,
                Order = layer.Order,
                MaterializationRole = layer.MaterializationRole,
                SafeFallbackRefId = layer.SafeFallbackRefId,
                LinkedLayerIds = links
                    .Where(link => selectedIds.Contains(link.FromLayerId) && selectedIds.Contains(link.ToLayerId)
                        && (link.FromLayerId == layer.LayerId || link.ToLayerId == layer.LayerId))
                    .Select(link => link.FromLayerId == layer.LayerId ? link.ToLayerId : link.FromLayerId)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToList()
            })
            .ToList();
    }

    private static IReadOnlyList<VisualChunkStreamLayerPortalRef> BuildLayerLinks(
        VisualWorldProfile profile,
        HashSet<string> layerIds) =>
        profile.LayerLinks
            .Where(item => layerIds.Contains(item.FromLayerId) && layerIds.Contains(item.ToLayerId))
            .OrderBy(item => item.LinkId, StringComparer.Ordinal)
            .Select(item => new VisualChunkStreamLayerPortalRef
            {
                LinkId = item.LinkId,
                FromLayerId = item.FromLayerId,
                ToLayerId = item.ToLayerId,
                LinkKind = item.LinkKind.ToString(),
                Summary = $"{item.LinkKind} link from {item.FromLayerId} to {item.ToLayerId}"
            })
            .ToList();

    private static long? EstimateFullWorldChunkCapacity(VisualWorldProfile profile, int? finiteWidth, int? finiteHeight)
    {
        if (profile.IsInfinite || !finiteWidth.HasValue || !finiteHeight.HasValue)
        {
            return null;
        }

        if (profile.ChunkProfile.ChunkWidth <= 0 || profile.ChunkProfile.ChunkHeight <= 0)
        {
            return null;
        }

        return CeilingDivide(finiteWidth.Value, profile.ChunkProfile.ChunkWidth)
            * CeilingDivide(finiteHeight.Value, profile.ChunkProfile.ChunkHeight)
            * Math.Max(1, profile.Layers.Count);
    }

    private static long CeilingDivide(long value, long divisor) => (value + divisor - 1) / divisor;
}
