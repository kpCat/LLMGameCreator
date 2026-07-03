# Final Roadmap Rebaseline

Status: Goal 097 strategic control document
Manual gate: `final_roadmap_rebaseline_dream_scope_productivity_verification required`
Accepted: false

## Current Position After Goal 096

Goal 096 leaves the project in a strong evidence-and-handoff state, not in a final playable visual streaming state. The current visual/world/Unity chain has:

- deterministic visual metadata contracts and rating metadata from Goals 084-085;
- text-SVG microtile, map patch and region proofs from Goals 086-088;
- Goal 089 tiered validation policy;
- Goal 090 arbitrary finite, huge sparse and infinite visual world profiles;
- Goal 091 deterministic requested chunk stream windows;
- Goal 092/092A editor review workspace plus source-health repair;
- Goal 093 visual chunk cache/export contract;
- Goal 094 cache export inspection;
- Goal 095 compact Unity StreamingAssets handoff/probe;
- Goal 096 editor/readiness inspection over the Unity handoff.

That chain proves a usable planning, validation and handoff spine. It does not yet prove live Unity visual rendering, runtime streaming, approved atlas consumption, final renderer quality, save/load deltas for streamed visual worlds, clean-machine release export or a player-facing game loop built on these visual artifacts.

## What Exists Now

- A validated generated-game foundation with deterministic package, runtime preview and Unity Alpha evidence from earlier goals.
- A visual stack that can describe, compose, inspect, cache and hand off compact visual world metadata.
- Infinite and huge sparse world planning at the profile/window level without raw full-world dumps.
- A Unity Alpha project that can receive compact StreamingAssets payloads and probe them.
- A tiered validation model that makes ordinary future goals faster while preserving full validation for milestone and release-like work.
- A source-health debt register with explicit known large-file and validation-duration risks.

## What Is Not Yet Real Or Playable

- No final visual renderer, raster atlas, sprite sheet, tile sheet or production visual asset pipeline is approved.
- No Runtime or Unity player consumes Goal 093/095 cache exports as live streamed gameplay visuals.
- No save/load system owns infinite world visual deltas or player-discovered chunk state for the visual stream.
- No clean installer/export path proves the product works on a clean machine.
- No release-grade adult/rating export filter is proven end to end.
- No legal/licensing/provider policy exists for real-world map/geodata ingestion.
- No geospatial ingestion, OCR/georeferencing, map adapter, 2D-to-3D reconstruction or online travel mode is implemented.
- No "Dream Full Final" simulator track has implementation permission from this document.

Proof/evidence layers are not enough. Future work must increasingly produce editor-visible, Unity-visible, playable or exportable progress. A goal can still create contracts and validators, but the next few goals should turn those contracts into visible product outcomes instead of only adding another isolated proof service.

## Milestone Ladder

### Vertical Slice Final

Purpose: a user can generate or select a coherent game direction, inspect the generated package, run a short player-facing loop, and export or hand off a package with visible assets/fallbacks.

Required shape:

- one coherent game loop with movement/exploration, interaction, quest/event, reward/cost and save/load proof;
- editor-visible review or inspector path for the generated package;
- Unity-visible or player-visible proof where applicable;
- package export/import proof and validation tier evidence;
- no runtime dependency on LLM, providers, WinForms or arbitrary Lua.

Estimated remaining aggressive goals: 4-7.

### Strong Alpha

Purpose: the tool can repeatedly create distinct playable packages across several families with useful authoring controls and stable validation.

Required shape:

- at least three distinct game families or presentation profiles using the same lifecycle;
- visible editor workflow for profile/options, review, validation and export;
- Unity/player path that consumes approved package/asset/handoff data;
- save/load and runtime state deltas for the selected world scale;
- adult/rating safe-public build behavior if adult metadata is enabled in source data;
- release-risk review after each 5-8 goals.

Estimated remaining aggressive goals: 10-16 after Vertical Slice Final.

### v1 Full Final

Purpose: local-first product loop from idea/profile to validated playable/exportable package for selected supported game families.

Required shape:

- clean-machine install/export proof;
- stable user workflows for generation, review, validation, package export and player launch;
- release-grade diagnostics, crash recovery and sample packages;
- approved visual/audio fallback policy and provenance/rating enforcement;
- performance budgets for selected Unity/player targets;
- documentation for supported and unsupported modes.

Estimated remaining aggressive goals: 18-28 after Strong Alpha.

### Dream Full Final

Purpose: long-term platform ambition beyond v1, covering large/infinite living worlds, richer visual/media compilation, multiple genres and optional simulator tracks.

Dream scope includes:

- fantasy exploration and Heroes-like map/panel adventure;
- sci-fi and ultra-modern future games;
- Space Rangers-like hybrid map, economy, text adventure and tactical events;
- procedural visual/media compiler with provider quarantine and approved asset consumption;
- adult/rating-gated extension path with fail-closed safe exports;
- realism/geospatial simulator track with licensed/open data and optional adapters;
- fully self-generated realism simulator track with living-world causal systems;
- release/packaging/Steam/export track.

Estimated remaining aggressive goals after v1: 30-60+, depending on which dream tracks are selected. These are not active implementation promises.

## Mandatory End-To-End Progress Rule

- Every 3-5 feature goals must produce user-visible, editor-visible, Unity-visible, playable or exportable progress.
- Every 5-8 goals must include a quality consolidation or release-risk pass.
- Ordinary future feature goals should combine contract, Application seam, evidence, tests and UI/export/readback when that produces a real outcome.
- Line count is not the target. Outcome size is the target.
- Avoid repeated isolated proof-only services when an existing seam can carry the next product-visible outcome.

## What To Defer

- Real provider/media execution until quarantine, provenance, licensing, rating and safe fallback gates are in place.
- Public GamePackage schema expansion until a consumer proof and migration plan exist.
- Runtime/Unity visual streaming consumption until approved renderer/cache/atlas contracts are validated.
- Geospatial ingestion, online maps, OCR/georeferencing and 2D-to-3D reconstruction until research and legal/provider policy exist.
- Full dream-scope simulator work until the v1 loop is real enough to justify it.

## What Should Be Killed If Scope Explodes

- Any proof layer that cannot name a near-term editor/player/export outcome.
- Any new abstraction whose only value is making future tasks feel cleaner.
- Any dream track that requires provider/network/runtime LLM dependency in the core.
- Any real-world map ingestion path without explicit licensing and ToS policy.
- Any adult/rating feature that cannot fail closed for safe/public export.
- Any Unity/player feature that embeds game-specific logic instead of consuming package data.
