# External scouting — Goal 066 Settlement Construction/Destruction/Production Matrix

## Decision

Do not add external dependencies in Goal 066.

Goal 066 should be a BCL-only Application seam with a narrow Unity Alpha marker/proof extension. The purpose is to prove deterministic settlement/building/construction/destruction/production/defense state changes across the existing 9 family/seed rows, not to import a city-builder toolkit or destructible-terrain package.

## Sources considered

### Syomus/ProceduralToolkit

- URL: https://github.com/Syomus/ProceduralToolkit
- License: MIT according to repository metadata.
- Useful later for Unity-side procedural helpers, cellular automata, geometry, sampling and future visual generation utilities.
- Not adopted now: Goal 066 needs domain-state proof, not Unity geometry dependency.

### Unity grid/building-system prototypes

- Example: https://github.com/Mansitos/Unity-Grid-Based-Terrain-Building-System
- Useful as concept reference for grid placement, footprint validation and RTS/city-builder style placement.
- Not adopted now: prototype-level Unity project, unclear dependency/packaging fit, and the current goal should keep the settlement state model Application-owned.

### Ideefixze/DTerrain

- URL: https://github.com/Ideefixze/DTerrain
- License: MIT according to repository metadata.
- Useful later for Unity-side destructible terrain/visible destruction prototypes.
- Not adopted now: Goal 066 should prove deterministic building/settlement destruction as data/state deltas first, not physics/pixel terrain destruction.

### mxgmn/WFC, MarkovJunior, TextureSynthesis, ConvChain

- Useful for later constrained local tile/settlement/detail generation, especially after settlement/building semantics are stable.
- Not adopted now: Goal 062 already introduced constrained spatial detail generation; Goal 066 should consume that output and prove settlement/gameplay systems depth.

## Recommendation

Implement Goal 066 as an in-house BCL-only settlement/construction/destruction/production matrix:

- settlement sites and building slots derived from Goal 060/061/062/063/064/065 evidence;
- building footprint/placement rules;
- construction cost/resource deltas;
- production output/resource conversion;
- damage/destruction/repair state;
- defense/threat/raid pressure;
- NPC/faction/living-world consequences;
- save/load/replay and Unity marker proof;
- no new public GamePackage schema changes.

Future optional adapter task can revisit ProceduralToolkit/DTerrain/WFC/MarkovJunior after the domain model proves useful.
