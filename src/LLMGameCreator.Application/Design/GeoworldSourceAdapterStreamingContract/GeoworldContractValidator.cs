using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.GeoworldSourceAdapterStreamingContract;

public static class GeoworldContractValidator
{
    private static readonly Regex StableIdRegex = new("^[a-z0-9][a-z0-9_./-]*[a-z0-9]$", RegexOptions.Compiled);
    private static readonly Regex Sha256Regex = new("^[A-Fa-f0-9]{64}$", RegexOptions.Compiled);

    public static GeoworldContractValidationResult Validate(GeoSourceAdapterSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        var diagnostics = new List<GeoworldContractDiagnostic>();

        ValidateId(spec.SpecId, "geoworld.spec_id.invalid", spec.SpecId, "Spec id must be stable lowercase metadata.", diagnostics);
        ValidateRequired(spec.DisplayName, "geoworld.display_name.missing", spec.SpecId, "Display name is required.", diagnostics);

        if (!spec.MetadataOnly)
        {
            diagnostics.Add(Error("geoworld.fixture.metadata_only.required", spec.SpecId, "Goal098 fixtures must be metadata-only."));
        }

        if (spec.AdapterKind is GeoSourceAdapterKind.Unspecified)
        {
            diagnostics.Add(Error("geoworld.adapter_kind.missing", spec.SpecId, "Adapter kind is required."));
        }

        if (spec.AdapterKind is GeoSourceAdapterKind.PublicTileServerScrape || spec.FetchPlan.PublicTileServerScrapeAttempted)
        {
            diagnostics.Add(Error("geoworld.public_tile_scraping.forbidden", spec.SpecId, "Public tile server scraping is forbidden."));
        }

        if (spec.AdapterKind is GeoSourceAdapterKind.BulkPublicTileArchive
            || spec.FetchPlan.BulkPublicTileArchiveMode
            || spec.CachePolicy?.PublicTileBulkArchiveMode == true
            || spec.CachePolicy?.NoRawPublicTilePreseed == false)
        {
            diagnostics.Add(Error("geoworld.public_tile_bulk_archive.forbidden", spec.SpecId, "Bulk/preseed public tile archive mode is forbidden."));
        }

        ValidateLicense(spec, diagnostics);
        ValidateProvenance(spec, diagnostics);
        ValidateCache(spec, diagnostics);
        ValidateFetchPlan(spec, diagnostics);
        ValidateStreaming(spec, diagnostics);
        ValidateFeatureNormalization(spec, diagnostics);
        ValidateWorldSourceGraph(spec, diagnostics);

        if (spec.AdapterKind == GeoSourceAdapterKind.OcrGeoreferenceFallbackFutureOnly
            && (!spec.OcrFallbackFutureOnly || spec.FetchPlan.OcrFallbackIsPrimaryPath))
        {
            diagnostics.Add(Error("geoworld.ocr_fallback.primary_path_forbidden", spec.SpecId, "OCR/georeference fallback must remain future-only and must not be primary path."));
        }

        return Result(diagnostics);
    }

    public static IReadOnlyList<GeoworldContractDiagnostic> SortDiagnostics(IEnumerable<GeoworldContractDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static void ValidateLicense(GeoSourceAdapterSpec spec, List<GeoworldContractDiagnostic> diagnostics)
    {
        if (spec.LicensePolicy == null)
        {
            diagnostics.Add(Error("geoworld.license_policy.missing", spec.SpecId, "License policy is required."));
            return;
        }

        ValidateId(spec.LicensePolicy.PolicyId, "geoworld.license_policy.id.invalid", spec.SpecId, "License policy id must be stable.", diagnostics);
        ValidateRequired(spec.LicensePolicy.LicenseId, "geoworld.license_id.missing", spec.SpecId, "License id is required.", diagnostics);
        ValidateRequired(spec.LicensePolicy.AttributionText, "geoworld.attribution.missing", spec.SpecId, "Attribution text is required.", diagnostics);
        ValidateRequired(spec.LicensePolicy.RedistributionPolicy, "geoworld.redistribution_policy.missing", spec.SpecId, "Redistribution policy is required.", diagnostics);

        if (spec.FetchPlan.RuntimeOnlineModeEnabled && !spec.LicensePolicy.RuntimeOnlineExplicitPolicyAllowed)
        {
            diagnostics.Add(Error("geoworld.runtime_online.policy_required", spec.SpecId, "Runtime online mode requires explicit policy allowance."));
        }

        if (spec.LicensePolicy.ContainsAdultOrRatingMetadata && string.IsNullOrWhiteSpace(spec.LicensePolicy.SafeFallbackPolicyId))
        {
            diagnostics.Add(Error("geoworld.rating.safe_fallback_missing", spec.SpecId, "Adult/rating metadata requires safe fallback policy."));
        }
    }

    private static void ValidateProvenance(GeoSourceAdapterSpec spec, List<GeoworldContractDiagnostic> diagnostics)
    {
        if (spec.Provenance == null)
        {
            diagnostics.Add(Error("geoworld.provenance.missing", spec.SpecId, "Provenance is required."));
            return;
        }

        ValidateId(spec.Provenance.ProvenanceId, "geoworld.provenance.id.invalid", spec.SpecId, "Provenance id must be stable.", diagnostics);
        ValidateRequired(spec.Provenance.SourceDocumentPath, "geoworld.provenance.source_document.missing", spec.SpecId, "Source document path is required.", diagnostics);
        ValidateRequired(spec.Provenance.SourceReference, "geoworld.provenance.source_ref.missing", spec.SpecId, "Source reference is required.", diagnostics);
        ValidateRequired(spec.Provenance.AdapterVersion, "geoworld.provenance.adapter_version.missing", spec.SpecId, "Adapter version is required.", diagnostics);
        ValidateRequired(spec.Provenance.NormalizationVersion, "geoworld.provenance.normalization_version.missing", spec.SpecId, "Normalization version is required.", diagnostics);

        if (!IsSafeRelativePath(spec.Provenance.SourceDocumentPath) || !IsSafeRelativePath(spec.Provenance.SourceReference))
        {
            diagnostics.Add(Error("geoworld.path.absolute_or_unsafe", spec.SpecId, "Source document/reference must be repository-relative."));
        }

        if (!Sha256Regex.IsMatch(spec.Provenance.ContentHash))
        {
            diagnostics.Add(Error("geoworld.provenance.hash.invalid", spec.SpecId, "Provenance content hash must be sha256."));
        }

        if (spec.Provenance.PromptTextIsSourceOfTruth
            || spec.Provenance.SourceOfTruthKind.Contains("prompt", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("geoworld.prompt.source_of_truth", spec.SpecId, "Prompt text must not be source of truth."));
        }

        if (spec.Provenance.ContainsLfzCopiedCodeMarker)
        {
            diagnostics.Add(Error("geoworld.lfz_code_copy.marker", spec.SpecId, "LFZ source code copy markers are forbidden."));
        }
    }

    private static void ValidateCache(GeoSourceAdapterSpec spec, List<GeoworldContractDiagnostic> diagnostics)
    {
        if (spec.CachePolicy == null)
        {
            diagnostics.Add(Error("geoworld.cache_policy.missing", spec.SpecId, "Cache policy is required."));
            return;
        }

        ValidateId(spec.CachePolicy.PolicyId, "geoworld.cache_policy.id.invalid", spec.SpecId, "Cache policy id must be stable.", diagnostics);
        if (spec.CachePolicy.Mode == GeoTileCacheMode.Unspecified)
        {
            diagnostics.Add(Error("geoworld.cache_policy.mode.missing", spec.SpecId, "Cache mode is required."));
        }

        if (!IsSafeRelativePath(spec.CachePolicy.RelativeCacheRoot))
        {
            diagnostics.Add(Error("geoworld.cache_policy.path.invalid", spec.SpecId, "Cache root must be repository-relative."));
        }

        if (!spec.CachePolicy.HasEvictionPolicy)
        {
            diagnostics.Add(Error("geoworld.cache_policy.eviction.missing", spec.SpecId, "Cache policy requires bounded eviction/retention semantics."));
        }
    }

    private static void ValidateFetchPlan(GeoSourceAdapterSpec spec, List<GeoworldContractDiagnostic> diagnostics)
    {
        ValidateId(spec.FetchPlan.PlanId, "geoworld.fetch_plan.id.invalid", spec.SpecId, "Fetch plan id must be stable.", diagnostics);

        if (spec.FetchPlan.PerformsNetworkIo || spec.FetchPlan.NetworkIoMode == GeoNetworkIoMode.LiveNetworkFetch)
        {
            diagnostics.Add(Error("geoworld.network_io.implementation_forbidden", spec.SpecId, "Goal098 must not implement live network I/O."));
        }

        if (spec.FetchPlan.RuntimeOnlineModeEnabled && !spec.FetchPlan.RuntimeOnlinePolicyExplicitlyEnabled)
        {
            diagnostics.Add(Error("geoworld.runtime_online.policy_required", spec.SpecId, "Runtime online mode cannot be enabled without explicit policy."));
        }

        if (spec.FetchPlan.ProviderOrApiHardcodedIntoCore)
        {
            diagnostics.Add(Error("geoworld.provider_api.hardcoded_core", spec.SpecId, "Provider/API identity must not be hardcoded into core contracts."));
        }

        if (spec.FetchPlan.FullPlanetRawDumpRequested)
        {
            diagnostics.Add(Error("geoworld.full_planet_raw_dump.forbidden", spec.SpecId, "Full planet raw dumps are forbidden."));
        }

        if (spec.FetchResult.NetworkIoPerformed || spec.FetchResult.RawGeodataDumpPresent || spec.FetchResult.BinaryMediaFileCount > 0)
        {
            diagnostics.Add(Error("geoworld.fetch_result.raw_or_media_output", spec.SpecId, "Evidence must not contain network output, raw geodata dumps or binary/raster media."));
        }
    }

    private static void ValidateStreaming(GeoSourceAdapterSpec spec, List<GeoworldContractDiagnostic> diagnostics)
    {
        if (spec.StreamingPolicy == null)
        {
            diagnostics.Add(Error("geoworld.streaming_policy.missing", spec.SpecId, "Streaming policy is required."));
            return;
        }

        ValidateId(spec.StreamingPolicy.PolicyId, "geoworld.streaming_policy.id.invalid", spec.SpecId, "Streaming policy id must be stable.", diagnostics);
        var request = spec.StreamingPolicy.StreamWindowRequest;
        ValidateId(request.WindowId, "geoworld.stream_window.id.invalid", spec.SpecId, "Stream window id must be stable.", diagnostics);
        ValidateId(request.GridRequest.RequestId, "geoworld.tile_grid.id.invalid", spec.SpecId, "Tile grid request id must be stable.", diagnostics);

        if (request.GridRequest.RadiusTiles < 1)
        {
            diagnostics.Add(Error("geoworld.stream_radius.missing", spec.SpecId, "Stream radius must be explicit and positive."));
        }

        if (!request.BoundaryPrefetchEnabled
            || !spec.StreamingPolicy.BoundaryPrefetchRequired
            || request.GridRequest.BoundaryPrefetchTiles < 1)
        {
            diagnostics.Add(Error("geoworld.boundary_prefetch.missing", spec.SpecId, "Boundary prefetch policy is required."));
        }

        if (!request.MaterializesOnlyRequestedWindow)
        {
            diagnostics.Add(Error("geoworld.stream_window.requested_only_required", spec.SpecId, "Stream policy must materialize only requested chunks/windows."));
        }

        if (!spec.StreamingPolicy.FullPlanetRawDumpForbidden || !spec.StreamingPolicy.FutureRuntimeStreamingContractOnly)
        {
            diagnostics.Add(Error("geoworld.streaming.contract_boundary", spec.SpecId, "Future runtime streaming must remain contract-only and forbid full raw dumps."));
        }
    }

    private static void ValidateFeatureNormalization(GeoSourceAdapterSpec spec, List<GeoworldContractDiagnostic> diagnostics)
    {
        if (spec.RawDescriptors.Count == 0 || spec.NormalizedFeatures.Count == 0)
        {
            diagnostics.Add(Error("geoworld.features.missing", spec.SpecId, "Raw descriptor metadata and normalized feature metadata are required."));
            return;
        }

        var rawIds = spec.RawDescriptors.Select(item => item.RawDescriptorId).ToHashSet(StringComparer.Ordinal);
        foreach (var raw in spec.RawDescriptors)
        {
            ValidateId(raw.RawDescriptorId, "geoworld.raw_descriptor.id.invalid", spec.SpecId, "Raw descriptor id must be stable.", diagnostics);
            ValidateRequired(raw.SourceTagFamily, "geoworld.raw_descriptor.family.missing", raw.RawDescriptorId, "Raw source tag family is required.", diagnostics);
            if (raw.ConsumedDirectlyByGameplay || raw.PreservedAsRawPayload)
            {
                diagnostics.Add(Error("geoworld.raw_tags.gameplay_leak", raw.RawDescriptorId, "Raw source tags must not be consumed directly by gameplay or preserved as raw payloads."));
            }
        }

        foreach (var feature in spec.NormalizedFeatures)
        {
            ValidateId(feature.FeatureId, "geoworld.normalized_feature.id.invalid", spec.SpecId, "Normalized feature id must be stable.", diagnostics);
            if (feature.Kind == GeoFeatureKind.Unspecified)
            {
                diagnostics.Add(Error("geoworld.normalized_feature.kind.missing", feature.FeatureId, "Normalized feature kind is required."));
            }

            if (!rawIds.Contains(feature.SourceRawDescriptorId))
            {
                diagnostics.Add(Error("geoworld.normalized_feature.raw_ref.unknown", feature.FeatureId, "Normalized feature must reference a known raw descriptor."));
            }

            if (!feature.HasNeutralGeometryContract || feature.ContainsRawSourceTags)
            {
                diagnostics.Add(Error("geoworld.normalized_feature.raw_tag_leak", feature.FeatureId, "Normalized features require neutral geometry and must not carry raw source tags."));
            }
        }
    }

    private static void ValidateWorldSourceGraph(GeoSourceAdapterSpec spec, List<GeoworldContractDiagnostic> diagnostics)
    {
        ValidateId(spec.WorldSourceGraph.GraphId, "geoworld.world_source_graph.id.invalid", spec.SpecId, "WorldSourceGraph id must be stable.", diagnostics);
        if (!spec.WorldSourceGraph.BaseDataImmutable
            || !spec.WorldSourceGraph.GameplayDeltasSeparate
            || !spec.WorldSourceGraph.ContractOnly
            || !spec.WorldSourceGraph.NoFullPlanetRawDump)
        {
            diagnostics.Add(Error("geoworld.world_source_graph.contract_boundary", spec.SpecId, "WorldSourceGraph must keep base data immutable, deltas separate, contract-only and no raw full-planet dump."));
        }

        if (spec.WorldSourceGraph.Chunks.Count == 0)
        {
            diagnostics.Add(Error("geoworld.world_source_graph.chunks.missing", spec.SpecId, "At least one graph chunk contract is required."));
        }

        foreach (var chunk in spec.WorldSourceGraph.Chunks)
        {
            ValidateId(chunk.ChunkId, "geoworld.graph_chunk.id.invalid", spec.SpecId, "Graph chunk id must be stable.", diagnostics);
            if (!chunk.HasBoundaryPrefetchFeatures || !chunk.UsesRelativeRefsOnly)
            {
                diagnostics.Add(Error("geoworld.graph_chunk.boundary_or_path_contract", chunk.ChunkId, "Graph chunks require boundary prefetch features and relative refs only."));
            }
        }
    }

    private static void ValidateId(string value, string code, string target, string message, List<GeoworldContractDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value) || !StableIdRegex.IsMatch(value))
        {
            diagnostics.Add(Error(code, string.IsNullOrWhiteSpace(target) ? "<empty>" : target, message));
        }
    }

    private static void ValidateRequired(string value, string code, string target, string message, List<GeoworldContractDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add(Error(code, string.IsNullOrWhiteSpace(target) ? "<empty>" : target, message));
        }
    }

    private static GeoworldContractValidationResult Result(List<GeoworldContractDiagnostic> diagnostics) =>
        new()
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            DiagnosticCount = diagnostics.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };

    private static GeoworldContractDiagnostic Error(string code, string target, string message) =>
        GeoworldContractDiagnostic.Error(code, target, message);
}
