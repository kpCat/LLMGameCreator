# Geoworld Ingestion Future Goal Sequence

## Purpose

Plan future geoworld work without starting implementation prematurely.

## Rule

Do not give Codex the LFZ archive. Future Codex goals must consume LLMGameCreator docs only.

## Proposed future goals

### Goal GEO-001 — LFZ Pattern Study Integration

Docs/evidence-only. Index this study into context/queue/state and generate policy proof.

### Goal GEO-002 — Geo Source Adapter Contract

BCL-only Application contracts for adapters, cache policy, provenance, licensing and geotile addressing. No network.

### Goal GEO-003 — Geo Feature Normalization Matrix

Metadata-only fixtures for buildings, roads, water, landuse, POI, barriers, bridges and vegetation. Negative proof for raw tags leaking to gameplay.

### Goal GEO-004 — WorldSourceGraph Foundation

Neutral graph that can represent imported geodata or self-generated realism data. No Unity/runtime.

### Goal GEO-005 — Geo Stream Window Scheduler

Editor-side proof for radius-based loading around player/camera positions, chunk overlap, boundary prefetch and cache reuse.

### Goal GEO-006 — Offline Bundle Import Prototype

Offline-only adapter over deterministic test bundles. No public API and no runtime network.

### Goal GEO-007 — GeoWorld Visual Projection

Project normalized geodata into existing visual world profiles/chunk windows/cache export.

### Goal GEO-008 — Unity Handoff for GeoWorld Preview

StreamingAssets handoff for a small offline geoworld bundle. No live online mode.

### Goal GEO-009 — Legal/License/Attribution Export Gate

Attribution, share-alike/export policy, provider terms, source provenance and release constraints.

### Goal GEO-010 — Optional Runtime Online Mode Design

Design only unless legal/provider policy is resolved. Explicit opt-in, cache-limited, no scraping.

## Not now

- No live map scraping.
- No public tile bulk download.
- No direct OSM/Overpass runtime dependency.
- No copying LFZ source.
- No full Earth raw data dump.
