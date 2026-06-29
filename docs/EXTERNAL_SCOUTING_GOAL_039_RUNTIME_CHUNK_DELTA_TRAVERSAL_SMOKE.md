# Goal 039 External Scouting — Runtime Chunk Delta And Traversal Smoke

## Decision

Do not add external dependencies in Goal 039.

Goal 038 already introduced an in-house world-scale region graph, reachability planner, finite map packs and chunk-config prelude. Goal 039 must move that evidence into a real runtime-facing chunk delta/traversal smoke without expanding dependency surface.

## Checked options

### QuikGraph

Potential value:
- generic graph structures and algorithms;
- useful later if region/network algorithms become large.

Decision:
- Do not add now.
- Current in-house reachability and chunk traversal needs are small and deterministic.
- License is MS-PL, which is permissive but not necessary here.
- Pulling it now would make a simple runtime state proof depend on a broad graph package.

### GoRogue / TheSadRogue primitives

Potential value:
- grid, map, roguelike utility types and algorithms;
- likely useful later for deeper grid gameplay, FOV, pathfinding, map layers.

Decision:
- Defer as optional adapter/reference.
- Goal 039 should prove the project's own chunk-state seam and runtime persistence first.
- Adding a roguelike toolkit now risks adapting the project around library abstractions before runtime needs are proven.

### RoyT.AStar

Potential value:
- small A* pathfinding library.

Decision:
- Defer.
- Goal 039 needs chunk traversal/state delta proof, not a mature pathfinding implementation.
- In-house simple route replay from Goal 038 reachability evidence is sufficient.

### NetTopologySuite

Potential value:
- serious GIS/topology library.

Decision:
- Do not add.
- Overkill for game chunk traversal and deterministic evidence.
- Useful only if future world generation needs real geometric GIS-like operations.

### MessagePack-CSharp

Potential value:
- compact/high-performance binary serialization.

Decision:
- Defer.
- Current evidence must remain inspectable and deterministic through compact JSON.
- Binary chunk persistence can be a later optional storage/export adapter after the runtime state shape is stable.

## Current recommendation

Use BCL/System.Text.Json and existing runtime serializer/snapshot mechanisms where available. Goal 039 should not invent a new persistence technology. It must prove:

1. chunk traversal commands derived from Goal 038 map/chunk evidence;
2. runtime-owned chunk delta state;
3. discovered/visited/mutated chunk facts survive save/load;
4. invalid/fake/leak scenarios are rejected causally;
5. no GamePackage schema mutation is needed.
