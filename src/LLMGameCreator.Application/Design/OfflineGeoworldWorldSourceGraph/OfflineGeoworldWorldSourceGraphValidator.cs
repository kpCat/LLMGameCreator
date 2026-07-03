namespace LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

public static class OfflineGeoworldWorldSourceGraphValidator
{
    public static OfflineGeoworldValidationResult Validate(
        OfflineGeoworldBundle bundle,
        OfflineGeoworldNormalizedFeatureSet normalized,
        OfflineGeoworldWorldSourceGraph graph,
        OfflineGeoworldStreamWindowPlan streamWindow,
        OfflineGeoworldVisualProjectionSummary projection)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentNullException.ThrowIfNull(normalized);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(streamWindow);
        ArgumentNullException.ThrowIfNull(projection);

        var diagnostics = new List<OfflineGeoworldDiagnostic>();
        ValidateBundle(bundle, diagnostics);
        ValidateNormalizedFeatures(bundle, normalized, diagnostics);
        ValidateGraph(normalized, graph, diagnostics);
        ValidateStreamWindow(streamWindow, diagnostics);
        ValidateProjection(projection, diagnostics);
        return Result(diagnostics);
    }

    public static IReadOnlyList<OfflineGeoworldDiagnostic> SortDiagnostics(
        IEnumerable<OfflineGeoworldDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathFullyQualified(path)
        && !path.StartsWith("/", StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static void ValidateBundle(
        OfflineGeoworldBundle bundle,
        List<OfflineGeoworldDiagnostic> diagnostics)
    {
        Required(bundle.BundleId, "goal099.bundle.id.missing", "bundle", diagnostics);
        if (!bundle.MetadataOnly || !bundle.SyntheticOnly || bundle.ContainsRealMapData)
        {
            diagnostics.Add(Error(
                "goal099.bundle.synthetic_metadata_only",
                bundle.BundleId,
                "Offline geoworld bundle must be synthetic metadata only."));
        }

        if (bundle.ContainsRawOsmDump || bundle.ContainsRawFullAreaDump || bundle.RealGeodataDumpMarkerPresent)
        {
            diagnostics.Add(Error(
                "goal099.raw_geodata_dump.forbidden",
                bundle.BundleId,
                "Raw OSM/full-area/real geodata dumps are forbidden."));
        }

        if (bundle.PublicTileScrapingAttempted)
        {
            diagnostics.Add(Error(
                "goal099.public_tile_scraping.forbidden",
                bundle.BundleId,
                "Public tile scraping is forbidden."));
        }

        if (bundle.RuntimeOnlineFetchAttempted)
        {
            diagnostics.Add(Error(
                "goal099.runtime_online_fetch.forbidden",
                bundle.BundleId,
                "Runtime online fetching is forbidden in Goal099."));
        }

        if (bundle.ContainsLfzCopiedCodeMarker)
        {
            diagnostics.Add(Error(
                "goal099.lfz_copied_code.marker",
                bundle.BundleId,
                "LFZ copied-code markers are forbidden."));
        }

        if (bundle.PromptTextIsSourceOfTruth)
        {
            diagnostics.Add(Error(
                "goal099.prompt.source_of_truth",
                bundle.BundleId,
                "Prompt text must not be treated as source of truth."));
        }

        if (string.IsNullOrWhiteSpace(bundle.SourceLineage)
            || string.IsNullOrWhiteSpace(bundle.LicenseProvenanceSummary))
        {
            diagnostics.Add(Error(
                "goal099.license_or_provenance.missing",
                bundle.BundleId,
                "License and provenance summary are required."));
        }

        if (!IsSafeRelativePath(bundle.SourceLineage))
        {
            diagnostics.Add(Error(
                "goal099.path.absolute_or_unsafe",
                bundle.BundleId,
                "Source lineage paths must be repository-relative."));
        }

        if (bundle.ContainsAdultOrRatingMetadata && string.IsNullOrWhiteSpace(bundle.SafeFallbackPolicyId))
        {
            diagnostics.Add(Error(
                "goal099.rating.safe_fallback_missing",
                bundle.BundleId,
                "Adult/rating metadata requires a safe fallback."));
        }

        foreach (var raw in bundle.RawDescriptors)
        {
            if (raw.NormalizedKind == OfflineGeoFeatureKind.Unknown)
            {
                diagnostics.Add(Error(
                    "goal099.feature_kind.unknown",
                    raw.RawDescriptorId,
                    "Raw descriptor must map to a known feature kind."));
            }

            if (raw.ConsumedDirectlyByGameplay || raw.PreservedAsRawPayload)
            {
                diagnostics.Add(Error(
                    "goal099.raw_tags.gameplay_leak",
                    raw.RawDescriptorId,
                    "Raw tags must be summarized and must not be consumed by gameplay."));
            }
        }
    }

    private static void ValidateNormalizedFeatures(
        OfflineGeoworldBundle bundle,
        OfflineGeoworldNormalizedFeatureSet normalized,
        List<OfflineGeoworldDiagnostic> diagnostics)
    {
        var rawIds = bundle.RawDescriptors
            .Select(item => item.RawDescriptorId)
            .ToHashSet(StringComparer.Ordinal);
        var kinds = normalized.Features
            .Select(item => item.Kind)
            .ToHashSet();
        foreach (var kind in OfflineGeoworldBundleFixtures.RequiredFeatureKinds)
        {
            if (!kinds.Contains(kind))
            {
                diagnostics.Add(Error(
                    "goal099.taxonomy.kind_missing",
                    OfflineGeoworldNormalizer.KindName(kind),
                    "Required normalized feature kind is missing."));
            }
        }

        foreach (var feature in normalized.Features)
        {
            Required(feature.FeatureId, "goal099.normalized_feature.id.missing", "normalized", diagnostics);
            if (feature.Kind == OfflineGeoFeatureKind.Unknown)
            {
                diagnostics.Add(Error(
                    "goal099.feature_kind.unknown",
                    feature.FeatureId,
                    "Normalized feature kind must be known."));
            }

            if (!rawIds.Contains(feature.SourceRawDescriptorId))
            {
                diagnostics.Add(Error(
                    "goal099.normalized_feature.raw_ref.unknown",
                    feature.FeatureId,
                    "Normalized feature must reference a raw descriptor."));
            }

            if (!feature.GameplaySafe || feature.ContainsRawSourceTags)
            {
                diagnostics.Add(Error(
                    "goal099.normalized_feature.raw_tag_leak",
                    feature.FeatureId,
                    "Normalized features must be gameplay-safe and raw-tag free."));
            }

            if (string.IsNullOrWhiteSpace(feature.LicenseProvenanceSummary)
                || string.IsNullOrWhiteSpace(feature.SourceLineage))
            {
                diagnostics.Add(Error(
                    "goal099.license_or_provenance.missing",
                    feature.FeatureId,
                    "Normalized feature license and provenance are required."));
            }
        }
    }

    private static void ValidateGraph(
        OfflineGeoworldNormalizedFeatureSet normalized,
        OfflineGeoworldWorldSourceGraph graph,
        List<OfflineGeoworldDiagnostic> diagnostics)
    {
        if (!graph.BaseDataImmutable
            || !graph.GameplayDeltasSeparate
            || graph.DeltaCount != 0
            || !graph.NoRawFullAreaDump)
        {
            diagnostics.Add(Error(
                "goal099.worldsourcegraph.contract_boundary",
                graph.GraphId,
                "Graph must keep immutable base data, empty separate deltas and no raw dump."));
        }

        if (graph.Chunks.Count == 0)
        {
            diagnostics.Add(Error(
                "goal099.worldsourcegraph.chunks_missing",
                graph.GraphId,
                "WorldSourceGraph must contain chunks."));
        }

        var references = graph.CrossChunkReferences
            .Select(item => item.FeatureId)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var feature in normalized.Features.Where(RequiresBoundaryReference))
        {
            if (!references.Contains(feature.FeatureId))
            {
                diagnostics.Add(Error(
                    "goal099.cross_chunk_reference.missing",
                    feature.FeatureId,
                    "Road/water/bridge boundary-crossing feature requires cross-chunk reference."));
            }
        }
    }

    private static void ValidateStreamWindow(
        OfflineGeoworldStreamWindowPlan streamWindow,
        List<OfflineGeoworldDiagnostic> diagnostics)
    {
        if (streamWindow.NetworkFetchAttempted)
        {
            diagnostics.Add(Error(
                "goal099.runtime_online_fetch.forbidden",
                streamWindow.Request.RequestId,
                "Stream window must not perform online fetch."));
        }

        if (streamWindow.Request.RuntimeTravelModeRequested
            && (!streamWindow.Request.BoundaryPrefetchEnabled
                || streamWindow.Request.BoundaryPrefetchBandChunks < 1
                || streamWindow.BoundaryPrefetchChunkKeys.Count == 0))
        {
            diagnostics.Add(Error(
                "goal099.boundary_prefetch.disabled",
                streamWindow.Request.RequestId,
                "Runtime travel mode requires boundary prefetch scheduling."));
        }

        if (streamWindow.RequiredChunkKeys.Count == 0)
        {
            diagnostics.Add(Error(
                "goal099.stream_window.required_chunks_missing",
                streamWindow.Request.RequestId,
                "Stream window must include required chunks."));
        }
    }

    private static void ValidateProjection(
        OfflineGeoworldVisualProjectionSummary projection,
        List<OfflineGeoworldDiagnostic> diagnostics)
    {
        if (!projection.NoRasterImages || !projection.NoUnityOutput)
        {
            diagnostics.Add(Error(
                "goal099.visual_projection.output_boundary",
                "projection",
                "Projection must remain compact text/SVG summary only."));
        }

        if (!IsSafeRelativePath(projection.OverviewSvgRelativePath)
            || !projection.OverviewSvgRelativePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error(
                "goal099.path.absolute_or_unsafe",
                projection.OverviewSvgRelativePath,
                "Overview SVG path must be repository-relative."));
        }

        if (projection.Chunks.Count == 0)
        {
            diagnostics.Add(Error(
                "goal099.visual_projection.chunks_missing",
                "projection",
                "Projection must include chunk summaries."));
        }
    }

    private static bool RequiresBoundaryReference(NormalizedGeoFeature feature) =>
        feature.CrossesChunkBoundary
        && (feature.Kind == OfflineGeoFeatureKind.Road
            || feature.Kind == OfflineGeoFeatureKind.Water
            || feature.Kind == OfflineGeoFeatureKind.Bridge);

    private static void Required(
        string value,
        string code,
        string target,
        List<OfflineGeoworldDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add(Error(code, target, "Required value is missing."));
        }
    }

    private static OfflineGeoworldValidationResult Result(
        List<OfflineGeoworldDiagnostic> diagnostics) =>
        new()
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            DiagnosticCount = diagnostics.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static OfflineGeoworldDiagnostic Error(string code, string target, string message) =>
        OfflineGeoworldDiagnostic.Error(code, target, message);
}
