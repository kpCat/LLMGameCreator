# External scouting for Goal 030 — semantic artifact contract registry

Status: planning aid for `goal-030-semantic-artifact-contract-registry-v1`.

Date: 2026-06-29.

## Decision summary

Goal 030 should not add a hard dependency on any external library.

Use in-house/BCL-only code for the first semantic artifact contract registry because the immediate need is not heavy graph algorithms, RDF/SPARQL, WFC generation, ECS runtime, or dialogue execution. The immediate need is a deterministic compatibility and routing kernel that can classify artifact contracts, semantic scopes, dependencies, blocked/future capabilities, and module absence behavior.

External libraries should be documented as later optional adapters, not added now:

| Candidate | License / source signal | Useful later for | Goal 030 decision |
| --- | --- | --- | --- |
| QuikGraph | MS-PL; generic graph structures and algorithms for .NET | Larger dependency graphs, shortest paths, graph analysis, visualization/export proof | Do not add now. Goal 030 graphs are small enough for in-house deterministic DFS/topological validation. MS-PL is acceptable only after explicit review and adapter boundary. |
| dotNetRDF | MIT; RDF/SPARQL library for .NET | Future formal ontology import/export, Turtle/RDF/SHACL-like semantic packs | Do not add now. RDF is too heavy for the current deterministic seed-pack contract. Keep a future optional `SemanticOntologyAdapter` seam only in docs if needed. |
| GoRogue | MIT; C# roguelike/2D utility algorithms: FOV, pathfinding, map generation, spatial maps | Future map/grid/pathfinding/world-scale goals | Do not add now. Goal 030 is semantic/contract routing, not map generation. Consider later for Goal 035-039 or as optional map adapter. |
| DeBroglie | MIT; C# WFC tilemap generation with .NET/Unity use | Future tile/chunk/biome/settlement layout generation | Do not add now. WFC should be isolated behind future map-layout adapter, never in contract registry core. |
| ink | MIT; narrative scripting language and Unity integration | Future narrative import/export or authoring bridge | Do not add now. Dialogue contracts should remain internal and JSON/data-driven until explicit narrative tooling goal. |
| Yarn Spinner | MIT for the Unity repository checked; dialogue tooling | Future Unity/dialogue adapter or import/export | Do not add now. Keep future adapter-only option; no Unity/dialogue dependency in Goal 030. |
| Arch ECS | Apache-2.0; high-performance C# ECS with Unity/Godot support | Future runtime simulation/player internals | Do not add now. Goal 030 is editor/generator-side. Runtime/ECS adoption is a separate architecture decision. |
| DefaultEcs | MIT-0; C# ECS framework | Future runtime simulation/player internals | Do not add now. Same reason as Arch. |

## Architectural consequence

Goal 030 should create stable, simple, deterministic in-repo primitives:

- artifact contract descriptors;
- semantic pack descriptors;
- semantic scopes and relation tags;
- dependency and compatibility diagnostics;
- resolver/planner output;
- compact evidence writer.

These primitives must not depend on LLM, RAG, Lua, WinForms, Runtime, Unity, provider SDKs, external packages, or public GamePackage schema changes.

## Future adapter notes

If future goals need one of these libraries:

1. add a dedicated scouting doc update;
2. add an explicit optional adapter module;
3. keep core data contracts BCL-only;
4. prove that disabling the adapter keeps the generator contract registry usable;
5. include license and transitive dependency review.
