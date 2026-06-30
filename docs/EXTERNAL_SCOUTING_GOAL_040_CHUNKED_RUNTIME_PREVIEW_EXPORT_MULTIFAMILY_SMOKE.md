# External scouting — Goal 040 Chunked Runtime Preview/Export Multi-Family Smoke

## Decision

Do not add external dependencies in Goal 040.

Goal 040 is a consumer/proof slice over existing repository-generated evidence and existing Application/Runtime serializer seams. The useful next step is not graph/pathfinding/GIS/library adoption; it is proving that Goal 039 runtime chunk deltas can be consumed by a bounded preview/export/generated-loop artifact route and reused across multiple generated families/scenarios.

## Considered options

### QuikGraph

Potential use:
- general graph algorithms.

Decision:
- Do not add.
- The repository already has in-house reachability and traversal evidence from Goals 038/039.
- Adding a graph library here would increase dependency/license review surface without moving the preview/export/generated-loop proof.

### GoRogue

Potential use:
- grid, map, roguelike, FOV/pathing foundations.

Decision:
- Defer.
- It may become useful later for richer tile/grid gameplay families, but Goal 040 should prove current chunk traversal artifacts can be consumed.

### MessagePack-CSharp

Potential use:
- binary serialization of runtime/chunk state.

Decision:
- Defer.
- Goal 039 already proved `RuntimeStateSerializer`/snapshot style save-load. Goal 040 evidence should remain compact deterministic JSON and human-reviewable.

### NetTopologySuite

Potential use:
- GIS geometry/topology.

Decision:
- Do not add.
- Overkill for generated game chunks/regions at this stage.

## Current implementation rule

Goal 040 must use BCL and existing repo seams. It must not start broad streaming architecture or Unity runtime refactors. It should prove an actual consumer path:
Goal 038 world facts -> Goal 039 runtime chunk delta traversal -> Goal 040 chunked preview/export payload + multi-family smoke evidence.

If a real existing preview/export seam cannot be consumed without touching forbidden UI/Unity/Runtime source, Codex must commit/push BLOCKED with precise evidence instead of faking GREEN.
