using System.Text;

namespace LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

public static class OfflineGeoworldVisualProjectionBuilder
{
    public static OfflineGeoworldVisualProjectionSummary BuildProjection(
        OfflineGeoworldWorldSourceGraph graph,
        OfflineGeoworldNormalizedFeatureSet normalized,
        OfflineGeoworldStreamWindowPlan streamWindow)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(normalized);
        ArgumentNullException.ThrowIfNull(streamWindow);

        var featuresById = normalized.Features.ToDictionary(item => item.FeatureId, StringComparer.Ordinal);
        var chunks = graph.Chunks
            .OrderBy(item => item.TileKey.Key, StringComparer.Ordinal)
            .Select(chunk =>
            {
                var features = chunk.FeatureIds
                    .Where(featuresById.ContainsKey)
                    .Select(item => featuresById[item])
                    .ToArray();
                return new OfflineGeoworldChunkProjectionSummary
                {
                    ChunkKey = chunk.TileKey.Key,
                    FeatureCount = features.Length,
                    HasBuildings = features.Any(item => item.Kind == OfflineGeoFeatureKind.Building),
                    HasRoads = features.Any(item => item.Kind == OfflineGeoFeatureKind.Road),
                    HasWater = features.Any(item => item.Kind == OfflineGeoFeatureKind.Water),
                    HasPoi = features.Any(item => item.Kind == OfflineGeoFeatureKind.Poi),
                    HasBridge = features.Any(item => item.Kind == OfflineGeoFeatureKind.Bridge),
                    HasBarrier = features.Any(item => item.Kind == OfflineGeoFeatureKind.Barrier),
                    HasVegetation = features.Any(item => item.Kind == OfflineGeoFeatureKind.Vegetation)
                };
            })
            .ToList();

        var passed = chunks.Count > 0
            && chunks.Any(item => item.HasBuildings)
            && chunks.Any(item => item.HasRoads)
            && chunks.Any(item => item.HasWater)
            && streamWindow.RequiredChunkKeys.Count > 0
            && !streamWindow.NetworkFetchAttempted;

        return new OfflineGeoworldVisualProjectionSummary
        {
            Passed = passed,
            NoRasterImages = true,
            NoUnityOutput = true,
            OverviewSvgRelativePath = "overviews/synthetic_city_radius_stream_window.svg",
            CompactOverviewEntry = "bundle="
                + normalized.BundleId
                + "; features="
                + normalized.FeatureCount
                + "; graphChunks="
                + graph.Chunks.Count
                + "; streamChunks="
                + streamWindow.RequiredChunkKeys.Count,
            Chunks = chunks
        };
    }

    public static string RenderSvg(
        OfflineGeoworldVisualProjectionSummary projection,
        OfflineGeoworldStreamWindowPlan streamWindow)
    {
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(streamWindow);

        var builder = new StringBuilder();
        builder.AppendLine("<svg viewBox=\"0 0 640 420\" role=\"img\" aria-label=\"Synthetic geoworld stream window overview\">");
        builder.AppendLine("<rect x=\"0\" y=\"0\" width=\"640\" height=\"420\" fill=\"#f7f7f2\"/>");
        builder.AppendLine("<text x=\"24\" y=\"34\" font-family=\"Consolas, monospace\" font-size=\"18\" fill=\"#1f2933\">Goal099 offline geoworld stream window</text>");
        builder.AppendLine("<text x=\"24\" y=\"58\" font-family=\"Consolas, monospace\" font-size=\"12\" fill=\"#425466\">offline synthetic bundle, no network, no raster, no Unity output</text>");

        var center = OfflineGeoworldWorldSourceGraphBuilder.TileKey(streamWindow.Request.CenterChunkKey);
        var cell = 48;
        var originX = 172;
        var originY = 98;
        for (var y = center.Y - 2; y <= center.Y + 2; y++)
        {
            for (var x = center.X - 2; x <= center.X + 2; x++)
            {
                var key = OfflineGeoTileKey.Create(x, y).Key;
                var required = streamWindow.RequiredChunkKeys.Contains(key, StringComparer.Ordinal);
                var prefetch = streamWindow.BoundaryPrefetchChunkKeys.Contains(key, StringComparer.Ordinal);
                var chunk = projection.Chunks.FirstOrDefault(item => item.ChunkKey == key);
                var px = originX + (x - center.X + 2) * cell;
                var py = originY + (y - center.Y + 2) * cell;
                var fill = required ? "#d8f0ff" : prefetch ? "#fff2bd" : "#ececec";
                var stroke = key == streamWindow.Request.CenterChunkKey ? "#0f5c8c" : "#778899";
                builder.AppendLine($"<rect x=\"{px}\" y=\"{py}\" width=\"44\" height=\"44\" fill=\"{fill}\" stroke=\"{stroke}\" stroke-width=\"2\"/>");
                builder.AppendLine($"<text x=\"{px + 5}\" y=\"{py + 17}\" font-family=\"Consolas, monospace\" font-size=\"9\" fill=\"#1f2933\">x{x}</text>");
                builder.AppendLine($"<text x=\"{px + 5}\" y=\"{py + 30}\" font-family=\"Consolas, monospace\" font-size=\"9\" fill=\"#1f2933\">y{y}</text>");
                if (chunk is not null)
                {
                    builder.AppendLine($"<text x=\"{px + 28}\" y=\"{py + 37}\" font-family=\"Consolas, monospace\" font-size=\"11\" fill=\"#0f5c8c\">{chunk.FeatureCount}</text>");
                }
            }
        }

        builder.AppendLine("<text x=\"24\" y=\"360\" font-family=\"Consolas, monospace\" font-size=\"12\" fill=\"#1f2933\">Legend: blue=required window, yellow=boundary prefetch, number=normalized features in graph chunk</text>");
        builder.AppendLine($"<text x=\"24\" y=\"384\" font-family=\"Consolas, monospace\" font-size=\"12\" fill=\"#1f2933\">{Escape(projection.CompactOverviewEntry)}</text>");
        builder.AppendLine("</svg>");
        return builder.ToString();
    }

    private static string Escape(string value) =>
        value.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
}
