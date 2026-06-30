# External scouting — Goal 043 multi-family generated template vertical slice

## Decision

No new external dependency is required for Goal 043.

The goal must prove a real generated/simulatable multi-family lifecycle with BCL/in-house code first. Dependencies may be revisited later as optional adapters after the generated lifecycle and output contracts stabilize.

## Reviewed options

### Scriban

- Useful later for deterministic text/template rendering, localization text and export scaffold generation.
- Risk now: Goal 043 should not become a text-template renderer or final prose generator.
- Decision: defer. Keep family outputs as structured records, IDs, slots, conditions and validation evidence, not final rendered prose.
- Source: https://github.com/scriban/scriban

### Tiled / TMX / JSON map ecosystem

- Useful later for editor/export compatibility and manual level/map editing workflows.
- Risk now: Tiled formats would pull the goal toward external map-file compatibility before the generated lifecycle is proven.
- Decision: defer to an optional export adapter. Goal 043 should keep compact deterministic JSON evidence and internal map/preview payloads.
- Sources:
  - https://github.com/mapeditor/tiled
  - https://github.com/mapeditor/tiled/wiki/JSON-Map-Format
  - https://github.com/mapeditor/tiled/wiki/TMX-Map-Format

### GoRogue

- Useful later for grid/FOV/pathfinding/roguelike algorithms.
- Risk now: it would hide whether the repository’s own region/chunk/traversal seams are sufficient.
- Decision: defer. Use existing Goal 038/039/040 in-house traversal and map/chunk contracts.
- Source: https://github.com/Chris3606/GoRogue

### DefaultEcs / ECS libraries

- Useful later for high-performance simulation/runtime architecture.
- Risk now: introducing ECS before runtime ownership is settled would fork architecture and expand scope heavily.
- Decision: defer. Goal 043 remains Application-layer and does not introduce ECS.
- Source: https://github.com/Doraku/DefaultEcs

## Goal 043 dependency policy

Do not add NuGet packages.

Do not add Scriban, Tiled parsers/exporters, GoRogue, ECS libraries, pathfinding libraries, JSON schema libraries, media libraries or provider/LLM packages.

The correct Goal 043 result is a deterministic generated multi-family template lifecycle and smoke proof, not an external-format integration.
