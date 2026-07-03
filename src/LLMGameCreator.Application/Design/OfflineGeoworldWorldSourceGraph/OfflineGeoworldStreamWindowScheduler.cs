namespace LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

public static class OfflineGeoworldStreamWindowScheduler
{
    public static OfflineGeoworldStreamWindowPlan BuildPlan(
        OfflineGeoworldWorldSourceGraph graph,
        OfflineGeoworldStreamWindowRequest? request = null)
    {
        ArgumentNullException.ThrowIfNull(graph);
        var effectiveRequest = request ?? new OfflineGeoworldStreamWindowRequest
        {
            CenterChunkKey = OfflineGeoworldBundleFixtures.CenterChunkKey,
            RadiusChunks = 1,
            BoundaryPrefetchBandChunks = 1,
            RuntimeTravelModeRequested = true,
            BoundaryPrefetchEnabled = true
        };
        var required = WindowChunkKeys(effectiveRequest.CenterChunkKey, effectiveRequest.RadiusChunks);
        var withPrefetch = WindowChunkKeys(
            effectiveRequest.CenterChunkKey,
            effectiveRequest.RadiusChunks + effectiveRequest.BoundaryPrefetchBandChunks);
        var boundaryPrefetch = withPrefetch
            .Except(required, StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var loaded = graph.Chunks
            .Select(item => item.TileKey.Key)
            .ToHashSet(StringComparer.Ordinal);
        var allWindow = required
            .Concat(boundaryPrefetch)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var states = allWindow
            .Select(chunkKey =>
            {
                var loadedFromBundle = loaded.Contains(chunkKey);
                return new OfflineGeoworldChunkCacheState
                {
                    ChunkKey = chunkKey,
                    State = loadedFromBundle
                        ? "loaded_from_offline_bundle"
                        : "scheduled_for_offline_cache_prefetch",
                    LoadedFromOfflineBundle = loadedFromBundle,
                    ScheduledNoNetwork = !loadedFromBundle
                };
            })
            .ToList();
        var missing = states
            .Where(item => item.ScheduledNoNetwork)
            .Select(item => item.ChunkKey)
            .ToList();

        return new OfflineGeoworldStreamWindowPlan
        {
            Request = effectiveRequest,
            RequiredChunkKeys = required,
            BoundaryPrefetchChunkKeys = boundaryPrefetch,
            CacheStates = states,
            MissingScheduledChunkKeys = missing,
            NetworkFetchAttempted = false,
            BoundaryPrefetchStatus = "scheduled_no_network_cache_first",
            CacheStateSummary = "loaded="
                + states.Count(item => item.LoadedFromOfflineBundle)
                + "; scheduledNoNetwork="
                + states.Count(item => item.ScheduledNoNetwork)
        };
    }

    public static OfflineGeoworldBoundaryPrefetchProof BuildBoundaryPrefetchProof(
        OfflineGeoworldStreamWindowPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var passed = plan.Request.RuntimeTravelModeRequested
            && plan.Request.BoundaryPrefetchEnabled
            && plan.Request.BoundaryPrefetchBandChunks > 0
            && plan.BoundaryPrefetchChunkKeys.Count > 0
            && !plan.NetworkFetchAttempted
            && plan.MissingScheduledChunkKeys.Count > 0;

        return new OfflineGeoworldBoundaryPrefetchProof
        {
            Passed = passed,
            CenterChunkKey = plan.Request.CenterChunkKey,
            RequiredChunkCount = plan.RequiredChunkKeys.Count,
            BoundaryPrefetchChunkCount = plan.BoundaryPrefetchChunkKeys.Count,
            MissingScheduledChunkCount = plan.MissingScheduledChunkKeys.Count,
            BoundaryPrefetchEnabled = plan.Request.BoundaryPrefetchEnabled,
            RuntimeTravelModeRequested = plan.Request.RuntimeTravelModeRequested,
            NoNetworkFetch = !plan.NetworkFetchAttempted,
            DiagnosticSummary = passed
                ? "boundary prefetch schedules neighbor chunks without network fetch"
                : "boundary prefetch contract failed"
        };
    }

    private static IReadOnlyList<string> WindowChunkKeys(string centerChunkKey, int radius)
    {
        var center = OfflineGeoworldWorldSourceGraphBuilder.TileKey(centerChunkKey);
        var keys = new List<string>();
        for (var y = center.Y - radius; y <= center.Y + radius; y++)
        {
            for (var x = center.X - radius; x <= center.X + radius; x++)
            {
                keys.Add(OfflineGeoTileKey.Create(x, y).Key);
            }
        }

        return keys.OrderBy(item => item, StringComparer.Ordinal).ToList();
    }
}
