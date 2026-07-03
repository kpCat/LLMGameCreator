namespace LLMGameCreator.Application.Design.OfflineGeoworldWorldSourceGraph;

public static class OfflineGeoworldBundleFixtures
{
    public const string SyntheticCityRadiusBundleId = "synthetic_city_radius_offline_bundle";
    public const string CenterChunkKey = "z14/x4210/y6142";

    public static readonly IReadOnlyList<OfflineGeoFeatureKind> RequiredFeatureKinds =
    [
        OfflineGeoFeatureKind.Building,
        OfflineGeoFeatureKind.Road,
        OfflineGeoFeatureKind.Water,
        OfflineGeoFeatureKind.LandUse,
        OfflineGeoFeatureKind.Poi,
        OfflineGeoFeatureKind.Bridge,
        OfflineGeoFeatureKind.Barrier,
        OfflineGeoFeatureKind.Vegetation,
        OfflineGeoFeatureKind.TerrainHint,
        OfflineGeoFeatureKind.AdministrativeArea
    ];

    public static OfflineGeoworldBundle BuildSyntheticCityRadiusBundle() =>
        new()
        {
            BundleId = SyntheticCityRadiusBundleId,
            DisplayName = "Synthetic city radius offline bundle",
            MetadataOnly = true,
            SyntheticOnly = true,
            ContainsRealMapData = false,
            ContainsRawOsmDump = false,
            ContainsRawFullAreaDump = false,
            PublicTileScrapingAttempted = false,
            RuntimeOnlineFetchAttempted = false,
            ContainsLfzCopiedCodeMarker = false,
            PromptTextIsSourceOfTruth = false,
            RealGeodataDumpMarkerPresent = false,
            ContainsAdultOrRatingMetadata = false,
            SafeFallbackPolicyId = "safe_public_geoworld_fallback",
            SourceLineage = "docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md",
            LicenseProvenanceSummary =
                "synthetic offline fixture; no real map data; Goal098 policy lineage only",
            RawDescriptors =
            [
                Raw(
                    "raw/building_footprint_residential_block",
                    OfflineGeoFeatureKind.Building,
                    OfflineGeoGeometryKind.Polygon,
                    "closed synthetic footprint polygon with four corners inside center chunk",
                    "synthetic_osm:building",
                    ["building", "levels"],
                    [CenterChunkKey]),
                Raw(
                    "raw/road_segment_market_axis",
                    OfflineGeoFeatureKind.Road,
                    OfflineGeoGeometryKind.LineString,
                    "east-west synthetic road segment crossing the center chunk boundary",
                    "synthetic_osm:highway",
                    ["highway", "surface"],
                    [CenterChunkKey, "z14/x4211/y6142"],
                    crossesBoundary: true),
                Raw(
                    "raw/water_body_linear_canal",
                    OfflineGeoFeatureKind.Water,
                    OfflineGeoGeometryKind.Polygon,
                    "synthetic narrow canal area crossing north neighbor seam",
                    "synthetic_osm:water",
                    ["natural", "water"],
                    [CenterChunkKey, "z14/x4210/y6141"],
                    crossesBoundary: true),
                Raw(
                    "raw/landuse_market_square",
                    OfflineGeoFeatureKind.LandUse,
                    OfflineGeoGeometryKind.Polygon,
                    "synthetic civic land-use area around market square",
                    "synthetic_osm:landuse",
                    ["landuse"],
                    [CenterChunkKey]),
                Raw(
                    "raw/poi_clinic_marker",
                    OfflineGeoFeatureKind.Poi,
                    OfflineGeoGeometryKind.Point,
                    "synthetic point of interest marker for clinic",
                    "synthetic_osm:amenity",
                    ["amenity", "name_class"],
                    [CenterChunkKey]),
                Raw(
                    "raw/bridge_canal_crossing",
                    OfflineGeoFeatureKind.Bridge,
                    OfflineGeoGeometryKind.LineString,
                    "synthetic bridge segment connecting road over canal seam",
                    "synthetic_osm:bridge",
                    ["bridge", "layer"],
                    [CenterChunkKey, "z14/x4211/y6142"],
                    crossesBoundary: true),
                Raw(
                    "raw/barrier_service_gate",
                    OfflineGeoFeatureKind.Barrier,
                    OfflineGeoGeometryKind.LineString,
                    "synthetic service gate barrier near industrial edge",
                    "synthetic_osm:barrier",
                    ["barrier"],
                    ["z14/x4209/y6142", CenterChunkKey],
                    crossesBoundary: true),
                Raw(
                    "raw/vegetation_pocket_park",
                    OfflineGeoFeatureKind.Vegetation,
                    OfflineGeoGeometryKind.Polygon,
                    "synthetic pocket park vegetation area",
                    "synthetic_osm:vegetation",
                    ["vegetation", "leaf_type"],
                    [CenterChunkKey]),
                Raw(
                    "raw/terrain_hint_embankment",
                    OfflineGeoFeatureKind.TerrainHint,
                    OfflineGeoGeometryKind.AreaHint,
                    "synthetic embankment slope hint beside canal",
                    "synthetic:terrain_hint",
                    ["slope_class"],
                    ["z14/x4210/y6141", CenterChunkKey],
                    crossesBoundary: true),
                Raw(
                    "raw/administrative_area_demo_district",
                    OfflineGeoFeatureKind.AdministrativeArea,
                    OfflineGeoGeometryKind.Polygon,
                    "synthetic administrative district boundary summary",
                    "synthetic_osm:boundary",
                    ["boundary", "admin_level"],
                    ["z14/x4209/y6141", "z14/x4210/y6141", "z14/x4209/y6142", CenterChunkKey],
                    crossesBoundary: true)
            ]
        };

    private static RawGeoFeatureDescriptor Raw(
        string id,
        OfflineGeoFeatureKind kind,
        OfflineGeoGeometryKind geometryKind,
        string geometrySummary,
        string sourceTagFamily,
        IReadOnlyList<string> rawTagKeys,
        IReadOnlyList<string> chunkKeys,
        bool crossesBoundary = false) =>
        new()
        {
            RawDescriptorId = id,
            NormalizedKind = kind,
            GeometryKind = geometryKind,
            GeometrySummary = geometrySummary,
            SourceTagFamily = sourceTagFamily,
            RawTagKeys = rawTagKeys,
            IntersectingChunkKeys = chunkKeys,
            CrossesChunkBoundary = crossesBoundary,
            ConsumedDirectlyByGameplay = false,
            PreservedAsRawPayload = false,
            SourceLineage = "docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md",
            LicenseProvenanceSummary = "synthetic offline descriptor; no real geodata"
        };
}
