# External scouting — Goal 031 Semantic Pack Composition Blueprint

## Decision

Goal 031 should not add external dependencies. The implementation should be BCL-only and should reuse Goal 030's semantic artifact contract registry and compatibility planner.

The next valuable layer is not an RDF/knowledge-graph import system, not an LLM orchestration layer, and not a dialogue/runtime engine. The immediate need is a deterministic semantic pack composition and blueprint planning kernel that can merge selected semantic packs into a coherent generation blueprint plan.

## Scouted options

### dotNetRDF

- Area: RDF/SPARQL, semantic web, ontology-oriented data.
- License: MIT.
- Decision: defer.
- Reason: useful later as an optional import/export adapter for RDF/OWL/SKOS-like semantic packs, but too heavy for the first in-repo pack composition kernel.
- Goal 031 stance: design models so that a later RDF adapter can map into internal semantic facts/relations, but do not add dotNetRDF now.

### Microsoft Semantic Kernel

- Area: LLM/agent orchestration.
- License: MIT.
- Current repository note from scouting: current Semantic Kernel direction is agent/orchestration-oriented and the .NET requirement has moved forward in recent docs/repo metadata.
- Decision: reject for Goal 031.
- Reason: the project policy is that LLM should assist authoring/planning/seed-pack levels and must not become a runtime or core generation dependency. Goal 031 is deterministic programmatic composition.

### QuikGraph

- Area: graph structures and algorithms for .NET.
- License: Microsoft Public License.
- Decision: reject for Goal 031.
- Reason: Goal 031 only needs small deterministic topological/link traversal and conflict checks; BCL in-house code is safer and avoids a hard graph-library dependency.

### GoRogue

- Area: roguelike/2D utility algorithms: FOV, pathfinding, map generation, lines, RNG, messaging.
- License: MIT.
- Decision: defer.
- Reason: valuable later for map/navigation/runtime-like generator utilities, not needed for semantic pack composition.

### DeBroglie

- Area: Wave Function Collapse / tile constraints for procedural generation.
- License: MIT.
- Decision: defer.
- Reason: valuable later for map/tile/layout generation. Goal 031 is higher-level semantic blueprint composition, not spatial materialization.

### Yarn Spinner / Yarn Spinner Unity

- Area: dialogue authoring/runtime integration.
- License: MIT for the core Unity integration.
- Decision: defer.
- Reason: useful later as an optional dialogue export/runtime adapter. Goal 031 should only produce dialogue tone/string-table/quest motive semantic hints, not Yarn scripts.

### Arch / DefaultEcs

- Area: ECS frameworks.
- Licenses: Arch Apache-2.0; DefaultEcs MIT-0.
- Decision: defer.
- Reason: useful later for simulation/runtime experiments, but not needed in a deterministic editor/generator semantic planning layer.

## Architectural conclusion

Implement Goal 031 as internal application-layer code:

- no external NuGet packages;
- no runtime/provider/LLM/RAG/Lua/UI/Unity changes;
- no GamePackage schema mutation;
- deterministic C# models and planners only;
- future adapters can translate from RDF/Yarn/WFC/ECS formats into the internal semantic pack model, but the core does not depend on any of them.

## Goal 031 implication

The new layer should consume Goal 030's registry/planner concepts and produce a stable semantic blueprint plan that future generator modules can use as the "why these modules and artifacts belong together" layer.

This is intentionally broader than a single feature module and narrower than final game package assembly.
