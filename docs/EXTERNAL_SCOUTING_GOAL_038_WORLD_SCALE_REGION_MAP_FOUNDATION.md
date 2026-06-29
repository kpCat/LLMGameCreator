# External scouting — Goal 038 World-scale region graph, finite map pack and chunk-config foundation

## Decision

Do not add external dependencies for Goal 038.

Goal 038 needs deterministic region graph construction, reachability validation, compact finite map pack generation and a chunk-config prelude. These are domain-contract problems, not library-selection problems. BCL-only in-house algorithms are sufficient for the first version and keep the future Unity/runtime/chunk system unconstrained.

## Reviewed options

### QuikGraph

- URL: https://github.com/KeRNeLith/QuikGraph
- Summary: generic directed/undirected graph data structures and algorithms for .NET; includes DFS, BFS, A* search, shortest path, k-shortest path, maximum flow and minimum spanning tree.
- License observed: MS-PL.
- Decision: do not add now. Useful algorithms, but license/dependency surface is unnecessary for current deterministic BFS/Dijkstra-style reachability checks.

### GoRogue

- URL: https://github.com/Chris3606/GoRogue
- Summary: .NET Standard roguelike/2D game utility library with FOV, pathfinding, map generation, spatial maps, goal maps and other roguelike systems.
- License observed: MIT.
- Decision: defer as optional future adapter. It is relevant for later roguelike/grid gameplay, but Goal 038 should not import a broad game utility dependency before the project has its own compact world/map contracts.

### RoyT.AStar

- URL: https://github.com/roy-t/AStar
- Summary: fast 2D pathfinding for grids/graphs, .NET Standard 2.0+, no external dependencies.
- License observed: MIT.
- Decision: defer. It is a plausible future pathfinding adapter, but Goal 038 can implement small deterministic graph and finite-grid path checks in-house.

### Red Blob Games hex-grid guide

- URL: https://www.redblobgames.com/grids/hexagons/
- Summary: practical reference for hexagonal grid coordinate systems, neighbors, distances, ranges and pathfinding concepts.
- Decision: use as design reference only. Do not copy code blindly. For Goal 038, prefer simple repo-owned axial/cube/square coordinate records and deterministic tests.

## Architecture conclusion

Implement Goal 038 BCL-only:

- repo-owned region graph records;
- repo-owned reachability/path diagnostics;
- compact finite map pack records;
- chunked-world config prelude;
- product smoke evidence across the existing four scenarios.

No QuikGraph/GoRogue/RoyT.AStar package should be added in Goal 038.
