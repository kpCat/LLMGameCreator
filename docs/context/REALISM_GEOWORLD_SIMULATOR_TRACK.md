# Realism / Geoworld Simulator Track

Status: future planning only
Manual gate: `final_roadmap_rebaseline_dream_scope_productivity_verification required`
Accepted: false

This document records a future realism/simulator ambition. It does not authorize geodata ingestion, OCR, scraping, network access, provider integration, public schema changes, Runtime changes, Unity changes or external dependencies.

## Mode A: Real-World / Geospatial Ingestion

Purpose: optionally build game or simulator worlds from licensed/open real-world map and geospatial sources while preserving LLMGameCreator's package/runtime boundaries.

Neutral future vocabulary:

- geodata ingestion;
- vector/raster tile ingestion;
- map source adapters;
- official API adapters;
- offline import/cache boundary;
- optional OCR/georeferencing for screenshots or non-structured maps;
- 2D-to-3D or first-person reconstruction;
- living-world simulation overlay;
- causal world state and deltas.

Required future architecture:

- External data adapters stay optional and outside the core generator/runtime.
- Imported data becomes reviewed, validated, relative-path package/editor artifacts before runtime use.
- Runtime consumes compiled package data, approved refs, cached data and save-compatible deltas only.
- Online runtime travel mode, if ever selected, is an optional adapter mode, not a core dependency.
- The project must keep a clear offline import/cache boundary so a generated package remains reviewable and reproducible.
- 2D-to-3D reconstruction must be a compiled presentation output, not runtime provider generation.
- Living-world simulation overlays must own causal deltas in deterministic runtime/save state.

Required warnings:

- Do not scrape map tiles by default.
- Do not violate provider Terms of Service.
- Prefer licensed/open data and official APIs.
- Keep provider/API credentials, quotas and cache rules outside core package/runtime logic.
- No runtime LLM/provider dependency.
- No implementation until deep research and legal/licensing policy exists.

Future gates before any implementation:

1. Legal/licensing/provider policy gate.
2. Data-source adapter contract gate.
3. Offline import/cache proof gate.
4. Geodata validation/provenance gate.
5. 2D-to-3D reconstruction sidecar proof gate.
6. Runtime/player consumption gate for compiled data only.

Key risks:

- provider ToS and licensing violations;
- paid API cost and quota instability;
- stale, incomplete or privacy-sensitive map data;
- georeferencing inaccuracies;
- terrain/building reconstruction quality;
- storage/performance of cached map data;
- legal obligations for redistribution.

## Mode B: Self-Generated Realism

Purpose: generate believable finite or infinite realistic worlds without external geodata.

Future scope:

- finite and infinite generated realistic worlds;
- procedural roads, settlements, districts and points of interest;
- traffic, ecology, economy and faction simulation;
- first-person, pseudo-3D or 3D presentation;
- near/far simulation tiers;
- deterministic seed/config generation;
- causal world state and save-compatible deltas.

Required future architecture:

- Store seed, rules, sparse overrides and chunk/region configs rather than raw full-world dumps.
- Use deterministic generation for world layout and near/far simulation setup.
- Runtime owns discovered/mutated world deltas.
- Presentation consumes approved compiled world data and visual sidecars.
- LLM may draft compact rules/archetypes offline; it must not generate every tile, building, NPC or runtime event.

Key risks:

- unrealistic or repetitive generated worlds;
- simulation complexity outgrowing validation;
- save/load deltas becoming unbounded;
- Unity/player performance for large worlds;
- content balancing and player guidance.

## Non-Goals For Goal 097

- no geodata ingestion implementation;
- no OCR implementation;
- no online map/API integration;
- no scraping;
- no Runtime or Unity changes;
- no new dependencies;
- no provider calls;
- no public GamePackage schema changes.
