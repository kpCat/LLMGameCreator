namespace LLMGameCreator.Application.Design.GeoworldSourceAdapterStreamingContract;

public static class GeoworldContractFixtures
{
    public static readonly IReadOnlyList<string> RequiredFixtureIds =
    [
        "offline_osm_extract_city_radius",
        "user_provided_map_bundle",
        "licensed_vector_tile_adapter_spec",
        "runtime_online_optional_policy_blocked_by_default",
        "ocr_georeference_fallback_future_only",
        "self_generated_realism_world_source",
        "earth_radius_stream_window_boundary_prefetch"
    ];

    public static IReadOnlyList<GeoSourceAdapterSpec> BuildFixtures()
    {
        var fixtures = new List<GeoSourceAdapterSpec>
        {
            Spec(
                "offline_osm_extract_city_radius",
                "Offline OSM extract city radius",
                GeoSourceAdapterKind.OfflineOsmExtract,
                "odbl-1.0-reviewed-extract",
                "OpenStreetMap contributors; metadata-only fixture, no raw geodata redistributed",
                "docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md",
                GeoTileCacheMode.ImportTimeLocalCache,
                "geoworld/cache/offline-osm-city-radius",
                radius: 2,
                prefetch: 1,
                features:
                [
                    Raw("raw/building_polygon", "osm:building", ["building", "building:levels"], GeoFeatureKind.Building),
                    Raw("raw/road_way", "osm:highway", ["highway", "surface"], GeoFeatureKind.Road),
                    Raw("raw/water_area", "osm:natural_water", ["natural", "water"], GeoFeatureKind.Water),
                    Raw("raw/landuse_area", "osm:landuse", ["landuse"], GeoFeatureKind.LandUse)
                ]),
            Spec(
                "user_provided_map_bundle",
                "User-provided map bundle",
                GeoSourceAdapterKind.UserProvidedMapBundle,
                "user-provided-local-bundle",
                "User-provided local bundle with explicit attribution supplied by project owner",
                "docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md",
                GeoTileCacheMode.UserProvidedBundle,
                "geoworld/cache/user-map-bundle",
                radius: 1,
                prefetch: 1,
                features:
                [
                    Raw("raw/user_poi", "bundle:poi", ["poi_class", "label"], GeoFeatureKind.Poi),
                    Raw("raw/user_barrier", "bundle:barrier", ["barrier_class"], GeoFeatureKind.Barrier),
                    Raw("raw/user_vegetation", "bundle:vegetation", ["vegetation_class"], GeoFeatureKind.Vegetation)
                ]),
            Spec(
                "licensed_vector_tile_adapter_spec",
                "Licensed vector tile adapter spec",
                GeoSourceAdapterKind.LicensedVectorTileAdapterSpec,
                "licensed-vector-policy-reviewed",
                "Licensed vector source attribution placeholder; legal/provider policy required before implementation",
                "docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md",
                GeoTileCacheMode.LicensedVectorCache,
                "geoworld/cache/licensed-vector-adapter",
                radius: 2,
                prefetch: 1,
                features:
                [
                    Raw("raw/licensed_road", "vector:road", ["class", "rank"], GeoFeatureKind.Road),
                    Raw("raw/licensed_bridge", "vector:bridge", ["bridge", "layer"], GeoFeatureKind.Bridge),
                    Raw("raw/licensed_water", "vector:water", ["kind"], GeoFeatureKind.Water)
                ]),
            Spec(
                "runtime_online_optional_policy_blocked_by_default",
                "Runtime online optional policy blocked by default",
                GeoSourceAdapterKind.RuntimeOnlineOptionalPolicy,
                "runtime-online-policy-required",
                "Runtime online mode blocked by default until explicit legal/provider policy exists",
                "docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md",
                GeoTileCacheMode.RuntimeOnlineBlockedByDefault,
                "geoworld/cache/runtime-online-blocked",
                radius: 1,
                prefetch: 1,
                networkMode: GeoNetworkIoMode.RuntimeOptionalBlockedByDefault,
                runtimeOnlineBlocked: true,
                features:
                [
                    Raw("raw/online_contract_placeholder", "policy:runtime_online_future", ["kind"], GeoFeatureKind.TerrainHint)
                ]),
            Spec(
                "ocr_georeference_fallback_future_only",
                "OCR georeference fallback future-only",
                GeoSourceAdapterKind.OcrGeoreferenceFallbackFutureOnly,
                "user-provided-image-future-research",
                "User-provided image/OCR fallback is future research only and never primary path",
                "docs/context/REALISM_GEOWORLD_SIMULATOR_TRACK.md",
                GeoTileCacheMode.UserProvidedBundle,
                "geoworld/cache/ocr-future-only",
                radius: 1,
                prefetch: 1,
                ocrFutureOnly: true,
                features:
                [
                    Raw("raw/ocr_georeference_hint", "ocr:georeference_hint", ["label", "anchor"], GeoFeatureKind.TerrainHint)
                ]),
            Spec(
                "self_generated_realism_world_source",
                "Self-generated realism world source",
                GeoSourceAdapterKind.SelfGeneratedRealismWorldSource,
                "llmgc-self-generated-realism-v1",
                "LLMGameCreator deterministic self-generated realism source; no external geodata",
                "docs/context/REALISM_GEOWORLD_SIMULATOR_TRACK.md",
                GeoTileCacheMode.SelfGeneratedSeedCache,
                "geoworld/cache/self-generated-realism",
                radius: 2,
                prefetch: 1,
                features:
                [
                    Raw("raw/generated_settlement", "generated:settlement", ["district", "density"], GeoFeatureKind.AdministrativeArea),
                    Raw("raw/generated_transit", "generated:transit", ["route_kind"], GeoFeatureKind.Transit),
                    Raw("raw/generated_vegetation", "generated:vegetation", ["biome"], GeoFeatureKind.Vegetation)
                ]),
            Spec(
                "earth_radius_stream_window_boundary_prefetch",
                "Earth radius stream window boundary prefetch",
                GeoSourceAdapterKind.OfflineOsmExtract,
                "odbl-1.0-reviewed-extract",
                "OpenStreetMap contributors; boundary prefetch contract fixture only",
                "docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md",
                GeoTileCacheMode.ImportTimeLocalCache,
                "geoworld/cache/earth-radius-boundary-prefetch",
                radius: 3,
                prefetch: 2,
                features:
                [
                    Raw("raw/boundary_road", "osm:highway", ["highway"], GeoFeatureKind.Road),
                    Raw("raw/boundary_water", "osm:water", ["water"], GeoFeatureKind.Water),
                    Raw("raw/boundary_bridge", "osm:bridge", ["bridge"], GeoFeatureKind.Bridge)
                ])
        };

        return fixtures.OrderBy(item => item.SpecId, StringComparer.Ordinal).ToList();
    }

    public static GeoworldNormalizedFeatureTaxonomy BuildTaxonomy() =>
        new()
        {
            Rows =
            [
                Taxonomy(GeoFeatureKind.Building, "BuildingFootprintFeature"),
                Taxonomy(GeoFeatureKind.Road, "RoadSegmentFeature"),
                Taxonomy(GeoFeatureKind.Water, "WaterBodyFeature"),
                Taxonomy(GeoFeatureKind.LandUse, "LandUseAreaFeature"),
                Taxonomy(GeoFeatureKind.Poi, "PoiFeature"),
                Taxonomy(GeoFeatureKind.Barrier, "BarrierFeature"),
                Taxonomy(GeoFeatureKind.Bridge, "BridgeFeature"),
                Taxonomy(GeoFeatureKind.Vegetation, "VegetationFeature"),
                Taxonomy(GeoFeatureKind.TerrainHint, "TerrainHintFeature"),
                Taxonomy(GeoFeatureKind.Transit, "TransitFeature"),
                Taxonomy(GeoFeatureKind.AdministrativeArea, "AdministrativeAreaFeature")
            ]
        };

    private static GeoworldFeatureTaxonomyRow Taxonomy(GeoFeatureKind kind, string contract) =>
        new()
        {
            Kind = kind,
            NeutralFeatureContract = contract,
            RawSourcePolicy = "raw source tags are normalized before gameplay or visual projection"
        };

    private static GeoFeatureRawDescriptor Raw(
        string id,
        string family,
        IReadOnlyList<string> tags,
        GeoFeatureKind kind) =>
        new()
        {
            RawDescriptorId = id,
            SourceTagFamily = family,
            RawTagKeys = tags,
            NormalizedKind = kind,
            ConsumedDirectlyByGameplay = false,
            PreservedAsRawPayload = false
        };

    private static GeoSourceAdapterSpec Spec(
        string id,
        string displayName,
        GeoSourceAdapterKind kind,
        string licenseId,
        string attribution,
        string sourceDocumentPath,
        GeoTileCacheMode cacheMode,
        string cacheRoot,
        int radius,
        int prefetch,
        IReadOnlyList<GeoFeatureRawDescriptor> features,
        GeoNetworkIoMode networkMode = GeoNetworkIoMode.None,
        bool runtimeOnlineBlocked = false,
        bool ocrFutureOnly = false)
    {
        var normalized = features
            .Select(raw => new GeoFeatureNormalized
            {
                FeatureId = $"normalized/{id}/{raw.NormalizedKind.ToString().ToLowerInvariant()}",
                Kind = raw.NormalizedKind,
                SourceRawDescriptorId = raw.RawDescriptorId,
                GameSemanticFeature = $"game_semantic/{raw.NormalizedKind.ToString().ToLowerInvariant()}",
                HasNeutralGeometryContract = true,
                ContainsRawSourceTags = false
            })
            .ToList();
        var centerTile = new GeoTileKey { Zoom = 14, X = StableCoordinate(id, 10000), Y = StableCoordinate(id, 20000) };
        var window = new GeoStreamWindowRequest
        {
            WindowId = $"stream_window/{id}",
            BoundaryPrefetchEnabled = true,
            QueuePolicyId = $"queue/{id}/cache_first",
            MaterializesOnlyRequestedWindow = true,
            GridRequest = new GeoTileGridRequest
            {
                RequestId = $"tile_grid/{id}",
                CenterTile = centerTile,
                RadiusTiles = radius,
                BoundaryPrefetchTiles = prefetch,
                Bounds = new GeoBounds
                {
                    SouthWest = new GeoCoordinate { Latitude = 48.40, Longitude = 30.40 },
                    NorthEast = new GeoCoordinate { Latitude = 48.60, Longitude = 30.60 }
                }
            }
        };

        return new GeoSourceAdapterSpec
        {
            SpecId = id,
            DisplayName = displayName,
            AdapterKind = kind,
            MetadataOnly = true,
            OcrFallbackFutureOnly = ocrFutureOnly,
            LicensePolicy = new GeoSourceLicensePolicy
            {
                PolicyId = $"license/{id}",
                LicenseId = licenseId,
                AttributionText = attribution,
                RedistributionPolicy = "metadata_only_review_fixture_no_raw_geodata_redistribution",
                AttributionRequired = true,
                RuntimeOnlineExplicitPolicyAllowed = false,
                ContainsAdultOrRatingMetadata = false,
                SafeFallbackPolicyId = "safe_public_geoworld_fallback"
            },
            Provenance = new GeoSourceProvenance
            {
                ProvenanceId = $"provenance/{id}",
                SourceDocumentPath = sourceDocumentPath,
                SourceReference = sourceDocumentPath,
                ContentHash = GeoworldContractHash.Compute($"{id}|{sourceDocumentPath}|{kind}"),
                AdapterVersion = "goal098-contract-v1",
                NormalizationVersion = "goal098-normalization-v1",
                SourceOfTruthKind = "documented_metadata_contract",
                PromptTextIsSourceOfTruth = false,
                ContainsLfzCopiedCodeMarker = false
            },
            CachePolicy = new GeoTileCachePolicy
            {
                PolicyId = $"cache/{id}",
                Mode = cacheMode,
                RelativeCacheRoot = cacheRoot,
                CacheFirst = true,
                HasEvictionPolicy = true,
                PublicTileBulkArchiveMode = false,
                NoRawPublicTilePreseed = true
            },
            FetchPlan = new GeoFetchPlan
            {
                PlanId = $"fetch_plan/{id}",
                NetworkIoMode = networkMode,
                PerformsNetworkIo = false,
                RuntimeOnlineModeEnabled = false,
                RuntimeOnlinePolicyExplicitlyEnabled = false,
                PublicTileServerScrapeAttempted = false,
                BulkPublicTileArchiveMode = false,
                ProviderOrApiHardcodedIntoCore = false,
                FullPlanetRawDumpRequested = false,
                OcrFallbackIsPrimaryPath = false
            },
            FetchResult = new GeoFetchResult
            {
                FixtureId = id,
                MetadataOnly = true,
                NetworkIoPerformed = false,
                RawGeodataDumpPresent = false,
                BinaryMediaFileCount = 0,
                ResultHash = GeoworldContractHash.Compute($"fetch_result|{id}|metadata_only")
            },
            StreamingPolicy = new GeoStreamingPolicy
            {
                PolicyId = $"stream_policy/{id}",
                StreamWindowRequest = window,
                BoundaryPrefetchRequired = true,
                RuntimeOnlineBlockedByDefault = runtimeOnlineBlocked || kind == GeoSourceAdapterKind.RuntimeOnlineOptionalPolicy,
                FullPlanetRawDumpForbidden = true,
                FutureRuntimeStreamingContractOnly = true
            },
            RawDescriptors = features,
            NormalizedFeatures = normalized,
            WorldSourceGraph = new WorldSourceGraph
            {
                GraphId = $"world_source_graph/{id}",
                BaseDataImmutable = true,
                GameplayDeltasSeparate = true,
                ContractOnly = true,
                NoFullPlanetRawDump = true,
                Chunks =
                [
                    new WorldSourceGraphChunk
                    {
                        ChunkId = $"geo_chunk/{id}/center",
                        TileKey = centerTile,
                        FeatureIds = normalized.Select(item => item.FeatureId).ToList(),
                        HasBoundaryPrefetchFeatures = prefetch > 0,
                        UsesRelativeRefsOnly = true
                    }
                ]
            }
        };
    }

    private static long StableCoordinate(string id, int modulo)
    {
        var hash = GeoworldContractHash.Compute(id);
        return Convert.ToInt64(hash[..8], 16) % modulo;
    }
}
