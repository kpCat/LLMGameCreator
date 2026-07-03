# LFZ Geoworld Pattern Study

## Executive summary

The LFZ archive shows a practical real-world game-map pipeline built around geospatial tiles, cache/server providers, OSM/Overpass-like conversion, typed map features and Unity generation stages.

The valuable idea for LLMGameCreator is not the code. The valuable idea is the pipeline architecture:

`planet position -> WebMercator tile grid -> data tile cache/source -> normalized features -> generator chain -> playable map`.

LLMGameCreator should adapt this into an Application-first, data-driven, provider-optional architecture.

## Observed architecture

### 1. Planet/geographic addressing

Relevant LFZ areas:

- `MercatorGrid`
- `GeoCoordinate`
- `GeoMetersCoordinates`
- `GeoPixelCoordinate`
- `GoogleGeoTileCoordinate`
- `TmsGeoTileCoordinate`
- `GoogleTile`
- `TileGrid`
- `WebMercatorMapper`

Observed responsibilities:

- latitude/longitude to projected meters;
- projected meters to pixels;
- pixels/meters to tile coordinates;
- TMS/Google tile coordinate conversion;
- tile bounding boxes;
- tile grids with border tiles;
- conversion from geospatial coordinates to Unity-space positions.

LLMGameCreator adaptation:

- `GeoCoordinate`
- `GeoTileKey`
- `GeoBounds`
- `GeoProjectionProfile`
- `GeoTileGrid`
- `GeoWorldChunkAddress`
- `GeoToVisualProjection`
- `GeoToRuntimeProjection`

These must live as BCL/Application contracts first, not Unity-first types.

### 2. Tile grid and border loading

LFZ uses a tile grid around a starting tile and can include border tiles. Border tiles are important because roads, buildings, land polygons and water features often cross tile boundaries.

LLMGameCreator should model:

- requested center coordinate / chunk key;
- stream radius;
- required core tiles;
- required border/neighbor tiles;
- loaded/cache/pending/failed state;
- transition rules at tile boundaries.

This maps well to current `DeterministicVisualChunkStreamWindow`.

### 3. Provider/cache/source split

Relevant LFZ areas:

- `Provider<T>`
- `FromCacheProvider<T>`
- `FromServerProvider<T>`
- `RequestsDispatcher`
- `DataTileProvider`
- `CacheParameters`
- `DataTileProviderConfig`
- `OverpassTileProviderConfig`

Observed pattern:

1. Try cache.
2. Validate cached result.
3. If invalid/missing, request from source/server.
4. Validate server result.
5. Save result to cache.
6. Merge multiple data tiles into one map dataset.

LLMGameCreator adaptation:

- `IGeoDataSourceAdapter`
- `IGeoTileCache`
- `GeoTileCachePolicy`
- `GeoDataProvenance`
- `GeoSourceLicensePolicy`
- `GeoFetchPlan`
- `GeoFetchResult`
- `GeoTileReadbackProof`

The default architecture should prefer offline/imported/cached data. Runtime online mode must be optional and policy-gated.

### 4. OSM/Overpass-like data loading

LFZ has an Overpass-like provider that computes a tile bounding box and sends it to a query endpoint. The useful idea is bbox-based data extraction per tile, not any particular endpoint or query string.

LLMGameCreator adaptation:

- `GeoBoundingBoxRequest`
- `GeoFeatureQuery`
- `GeoSourceAdapterKind`
  - offline extract;
  - local file;
  - official API;
  - user-provided bundle;
  - optional online adapter;
  - OCR/georeferencing fallback as future research only.

### 5. Raw geodata to typed data

Relevant LFZ converters:

- `OverpassToDataTileConverter`
- `OverpassGeometryConventer`
- `OsmDataMapping`
- `TypeMapping`
- `OsmTaggedGeometryToBuildingConverter`
- `OsmRoadConverter`
- `OsmLandusageToLandConverter`
- `OsmAmenityToPoiConverter`
- `OsmAreaToBridgeConverter`
- `OsmBarrierWayToBarrierConverter`

Observed typed outputs include:

- buildings;
- roads;
- land/landuse;
- water-like land areas;
- POI;
- barriers;
- bridges.

LLMGameCreator adaptation:

`RawGeoFeature -> NormalizedGeoFeature -> WorldSourceGraphFeature -> GameSemanticFeature`

Recommended normalized feature types:

- `BuildingFootprintFeature`
- `RoadSegmentFeature`
- `LandUseAreaFeature`
- `WaterBodyFeature`
- `BridgeFeature`
- `BarrierFeature`
- `PoiFeature`
- `VegetationFeature`
- `TerrainHintFeature`
- `TransitFeature`
- `AdministrativeAreaFeature`

Raw OSM tags must not be consumed directly by gameplay or visual generation.

### 6. Geometry processing

LFZ uses polygon/geometry processing for areas, roads, land and water-related generation. The important pattern is geometry normalization before rendering.

LLMGameCreator adaptation:

- polygon simplification;
- polygon clipping;
- area classification;
- line buffering for roads/rivers;
- bridge and water adjacency;
- building footprint cleanup;
- feature-to-chunk indexing;
- seam handling between geotiles/chunks.

External library choices must be optional adapters, not core dependencies.

### 7. Generator chain

LFZ uses a generator chain with dependencies and progress. Generator stages include buildings, roads, terrain, water, trees, navigation, weather, objects and more.

LLMGameCreator adaptation:

- `GeoWorldGenerationStage`
- `StageInputContract`
- `StageOutputContract`
- `StageDependency`
- `StageDiagnostics`
- `StageEvidenceHash`
- `StageProgress`
- deterministic rerun proof.

The current LLMGameCreator goal/evidence pattern should remain the controlling spine, but future geoworld generation needs a runtime/editor staging model.

## Runtime streaming idea

The target user scenario:

1. Player starts at a real-world coordinate.
2. The system loads a radius around that coordinate.
3. As the player approaches boundary chunks, the system schedules neighboring chunks.
4. Source adapter/cache provides missing geodata.
5. Normalizer converts raw features.
6. WorldSourceGraph updates.
7. Visual/runtime chunk projections materialize.
8. Saved deltas overlay the deterministic base world.

Important: this must not require runtime LLM or runtime media-provider calls.

## What not to copy

Do not copy:

- Unity-first domain model;
- direct coupling of core data to `UnityEngine` types;
- direct use of ScriptableObject configs as source of truth;
- server/download logic as core behavior;
- public tile-server scraping assumptions;
- monolithic MapOperator architecture.

Use the architecture idea, not the implementation.

## Immediate LLMGameCreator value

The LFZ pattern should feed future work:

1. Geoworld source adapter contracts.
2. Geo tile cache/provenance policy.
3. Normalized geofeature taxonomy.
4. WorldSourceGraph.
5. Geo stream window scheduler.
6. Geo-to-visual projection.
7. Optional geodata provider adapters after legal/licensing research.
