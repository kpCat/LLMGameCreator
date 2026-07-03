namespace LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

public static class OfflineGeoworldWorldSourceGraphBuilder
{
    public static OfflineGeoworldWorldSourceGraph Build(
        OfflineGeoworldBundle bundle,
        OfflineGeoworldNormalizedFeatureSet normalized)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentNullException.ThrowIfNull(normalized);

        var references = BuildCrossChunkReferences(normalized.Features);
        var referenceLookup = references
            .GroupBy(item => item.FromChunkId, StringComparer.Ordinal)
            .ToDictionary(
                item => item.Key,
                item => item.Select(reference => reference.ReferenceId).ToArray(),
                StringComparer.Ordinal);
        var chunks = normalized.Features
            .SelectMany(item => item.ChunkKeys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .Select(chunkKey => new WorldSourceGraphChunk
            {
                ChunkId = ChunkId(chunkKey),
                TileKey = TileKey(chunkKey),
                FeatureIds = normalized.Features
                    .Where(feature => feature.ChunkKeys.Contains(chunkKey, StringComparer.Ordinal))
                    .Select(feature => feature.FeatureId)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToList(),
                CrossChunkReferenceIds = referenceLookup.TryGetValue(ChunkId(chunkKey), out var refs)
                    ? refs.OrderBy(item => item, StringComparer.Ordinal).ToList()
                    : [],
                SourceProvenance = bundle.SourceLineage
            })
            .ToList();

        return new OfflineGeoworldWorldSourceGraph
        {
            GraphId = "world_source_graph/" + bundle.BundleId,
            BundleId = bundle.BundleId,
            BaseDataImmutable = true,
            GameplayDeltasSeparate = true,
            DeltaCount = 0,
            NoRawFullAreaDump = true,
            SourceProvenance = bundle.SourceLineage,
            Chunks = chunks,
            CrossChunkReferences = references
        };
    }

    private static IReadOnlyList<WorldSourceGraphCrossChunkReference> BuildCrossChunkReferences(
        IReadOnlyList<NormalizedGeoFeature> features)
    {
        var references = new List<WorldSourceGraphCrossChunkReference>();
        foreach (var feature in features
                     .Where(item => item.CrossesChunkBoundary)
                     .OrderBy(item => item.FeatureId, StringComparer.Ordinal))
        {
            var chunks = feature.ChunkKeys
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToArray();
            if (chunks.Length < 2)
            {
                continue;
            }

            var first = chunks[0];
            foreach (var next in chunks.Skip(1))
            {
                var referenceId = "cross_ref/"
                    + OfflineGeoworldNormalizer.Slug(feature.FeatureId)
                    + "/"
                    + OfflineGeoworldNormalizer.Slug(first)
                    + "_to_"
                    + OfflineGeoworldNormalizer.Slug(next);
                references.Add(new WorldSourceGraphCrossChunkReference
                {
                    ReferenceId = referenceId,
                    FeatureId = feature.FeatureId,
                    FeatureKind = feature.Kind,
                    FromChunkId = ChunkId(first),
                    ToChunkId = ChunkId(next),
                    Reason = OfflineGeoworldNormalizer.KindName(feature.Kind)
                        + " crosses a synthetic geoworld chunk boundary"
                });
            }
        }

        return references.OrderBy(item => item.ReferenceId, StringComparer.Ordinal).ToList();
    }

    internal static string ChunkId(string chunkKey) =>
        "geo_chunk/" + chunkKey.Replace('/', '_');

    internal static OfflineGeoTileKey TileKey(string chunkKey)
    {
        var parts = chunkKey.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var zoom = parts.Length > 0 && parts[0].StartsWith('z')
            ? int.Parse(parts[0][1..], System.Globalization.CultureInfo.InvariantCulture)
            : 14;
        var x = parts.Length > 1 && parts[1].StartsWith('x')
            ? int.Parse(parts[1][1..], System.Globalization.CultureInfo.InvariantCulture)
            : 0;
        var y = parts.Length > 2 && parts[2].StartsWith('y')
            ? int.Parse(parts[2][1..], System.Globalization.CultureInfo.InvariantCulture)
            : 0;

        return new OfflineGeoTileKey
        {
            Zoom = zoom,
            X = x,
            Y = y,
            Key = chunkKey
        };
    }
}
