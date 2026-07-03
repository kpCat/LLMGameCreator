namespace LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

public static class OfflineGeoworldNormalizer
{
    public static OfflineGeoworldNormalizedFeatureSet Normalize(OfflineGeoworldBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        var features = bundle.RawDescriptors
            .OrderBy(item => item.RawDescriptorId, StringComparer.Ordinal)
            .Select(raw => NormalizeFeature(bundle, raw))
            .ToList();

        return new OfflineGeoworldNormalizedFeatureSet
        {
            BundleId = bundle.BundleId,
            FeatureCount = features.Count,
            GameplaySafeOnlyAfterNormalization = features.All(item => item.GameplaySafe),
            RawTagsMappedNotPassedDirectly = features.All(item => !item.ContainsRawSourceTags)
                && bundle.RawDescriptors.All(item => !item.ConsumedDirectlyByGameplay && !item.PreservedAsRawPayload),
            FeatureKindsCovered = features
                .Select(item => KindName(item.Kind))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToList(),
            Features = features
        };
    }

    private static NormalizedGeoFeature NormalizeFeature(
        OfflineGeoworldBundle bundle,
        RawGeoFeatureDescriptor raw)
    {
        var featureId = "normalized/"
            + bundle.BundleId
            + "/"
            + KindName(raw.NormalizedKind)
            + "/"
            + Slug(raw.RawDescriptorId);
        return new NormalizedGeoFeature
        {
            FeatureId = featureId,
            Kind = raw.NormalizedKind,
            SourceRawDescriptorId = raw.RawDescriptorId,
            NormalizedGeometrySummary = raw.GeometryKind + ": " + raw.GeometrySummary,
            SourceLineage = raw.SourceLineage,
            LicenseProvenanceSummary = raw.LicenseProvenanceSummary,
            GameplaySafe = raw.NormalizedKind != OfflineGeoFeatureKind.Unknown
                && !raw.ConsumedDirectlyByGameplay
                && !raw.PreservedAsRawPayload,
            ContainsRawSourceTags = false,
            RawTagSummary = "mapped "
                + raw.RawTagKeys.Count
                + " raw tag keys into "
                + KindName(raw.NormalizedKind)
                + " feature metadata",
            ChunkKeys = raw.IntersectingChunkKeys.OrderBy(item => item, StringComparer.Ordinal).ToList(),
            CrossesChunkBoundary = raw.CrossesChunkBoundary,
            CrossChunkReferenceIds = BuildExpectedReferenceIds(featureId, raw.IntersectingChunkKeys)
        };
    }

    internal static IReadOnlyList<string> BuildExpectedReferenceIds(
        string featureId,
        IReadOnlyList<string> chunkKeys)
    {
        var ordered = chunkKeys
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        if (ordered.Length < 2)
        {
            return [];
        }

        var first = ordered[0];
        return ordered
            .Skip(1)
            .Select(next => "cross_ref/" + Slug(featureId) + "/" + Slug(first) + "_to_" + Slug(next))
            .ToList();
    }

    internal static string KindName(OfflineGeoFeatureKind kind) =>
        kind switch
        {
            OfflineGeoFeatureKind.Building => "building",
            OfflineGeoFeatureKind.Road => "road",
            OfflineGeoFeatureKind.Water => "water",
            OfflineGeoFeatureKind.LandUse => "landUse",
            OfflineGeoFeatureKind.Poi => "poi",
            OfflineGeoFeatureKind.Bridge => "bridge",
            OfflineGeoFeatureKind.Barrier => "barrier",
            OfflineGeoFeatureKind.Vegetation => "vegetation",
            OfflineGeoFeatureKind.TerrainHint => "terrainHint",
            OfflineGeoFeatureKind.AdministrativeArea => "administrativeArea",
            _ => "unknown"
        };

    internal static string Slug(string value)
    {
        var chars = value
            .Replace("raw/", string.Empty, StringComparison.Ordinal)
            .Replace("normalized/", string.Empty, StringComparison.Ordinal)
            .Replace("z14/", string.Empty, StringComparison.Ordinal)
            .Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : '_')
            .ToArray();
        return new string(chars).Trim('_');
    }
}
