# Geoworld Source Adapter Architecture

## Purpose

Define the future source-adapter layer inspired by LFZ-style map loading, adapted to LLMGameCreator.

## Adapter categories

Potential future adapters:

- `OfflineOsmExtractAdapter`
- `UserMapBundleAdapter`
- `LicensedVectorTileAdapter`
- `OfficialApiAdapter`
- `LocalGeoPackageAdapter`
- `RasterMapGeoreferenceAdapter`
- `OcrMapFallbackAdapter`
- `RuntimeOnlineAdapter`

Only the first offline/local adapters should be considered early. Runtime online adapters require policy and legal gates.

## Core contracts

Recommended contracts:

- `GeoSourceAdapterSpec`
- `GeoSourceLicensePolicy`
- `GeoSourceProvenance`
- `GeoTileKey`
- `GeoBounds`
- `GeoFetchRequest`
- `GeoFetchResult`
- `GeoTileCacheRecord`
- `GeoFeatureRaw`
- `GeoFeatureNormalized`
- `GeoFeatureNormalizationReport`
- `WorldSourceGraph`
- `WorldSourceGraphChunk`
- `GeoStreamingPolicy`

## Provenance fields

Every fetched/imported tile or feature bundle should track:

- source id;
- source kind;
- license id;
- attribution text;
- retrieval/import time as optional untracked runtime metadata, not deterministic evidence unless normalized;
- source URL or file ref if allowed;
- content hash;
- adapter version;
- normalization version;
- cache policy;
- allowed export modes.

## Policy gates

Required before implementation:

- no scraping by default;
- no bulk tile downloading from public tile servers;
- official API/provider policy respected;
- ODbL/share-alike implications reviewed for OSM-derived datasets;
- attribution/export policy defined;
- user-provided data policy defined;
- runtime online mode opt-in only;
- no runtime LLM/media provider dependency.

## Normalization target

Raw source data must be converted to neutral features:

- buildings;
- roads;
- water;
- land use;
- barriers;
- bridges;
- POIs;
- vegetation;
- terrain hints;
- administrative/region hints.

The neutral graph can then feed fantasy, realism, zombie, sci-fi or other game profiles.
