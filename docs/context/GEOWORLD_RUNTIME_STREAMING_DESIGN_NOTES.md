# Geoworld Runtime Streaming Design Notes

## Purpose

Record the desired future mechanic:

A player can choose or arrive at any point on Earth-like space. The game loads only the nearby geospatial chunks, then streams neighboring chunks as the player approaches boundaries.

This document is planning only. It does not authorize immediate network implementation.

## Target flow

```text
player geo position
  ↓
geo chunk address
  ↓
stream window radius
  ↓
required chunk set
  ↓
cache lookup
  ↓
source adapter fetch only for missing allowed chunks
  ↓
provenance/license validation
  ↓
raw geodata normalization
  ↓
WorldSourceGraph update
  ↓
visual/runtime chunk projection
  ↓
delta overlay
```

## Chunk lifecycle

Recommended chunk states:

- `unknown`
- `scheduled`
- `loadingFromCache`
- `fetchingFromSource`
- `normalizing`
- `readyBase`
- `compiledVisual`
- `compiledRuntime`
- `active`
- `evictable`
- `failed`
- `blockedByPolicy`

## Boundary loading

When the player enters a boundary band:

1. Compute new stream window.
2. Diff old and new required chunk sets.
3. Keep overlapping active chunks.
4. Queue missing chunks.
5. Use cache first.
6. Fetch from source only if policy allows.
7. Materialize only requested chunks.
8. Never materialize a full planet or large raw cell dump.

## Delta overlay

Real-world base data must not be treated as mutable source. Gameplay changes are deltas:

- destroyed building;
- repaired bridge;
- looted location;
- road blocked;
- faction control;
- NPC death/state;
- player-built object;
- simulation change.

Base geodata plus deltas produces current world state.

## Online mode

Runtime online fetching must be optional. Preferred modes:

1. Offline imported bundles.
2. Local cache from editor/import stage.
3. Licensed provider adapter.
4. Online runtime mode only when explicitly enabled and policy-compliant.

## Failure handling

The player should not hard-crash if a chunk cannot be loaded.

Fallbacks:

- use cached older chunk;
- use procedural placeholder terrain;
- mark region as unavailable;
- postpone detailed features;
- show diagnostics in editor/developer mode.

## Relationship to existing LLMGameCreator stack

Existing stack:

- Parameterized Visual World Profiles.
- Deterministic Visual Chunk Stream Window.
- Visual Chunk Cache Export Contract.
- Unity StreamingAssets handoff.

Future geoworld streaming should plug into this by adding geodata as another world source, not by replacing the current generator architecture.
