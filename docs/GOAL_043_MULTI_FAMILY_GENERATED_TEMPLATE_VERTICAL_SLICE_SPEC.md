# Goal 043 — Multi-Family Generated Template Vertical Slice Specification

## Purpose

Goal 043 is the first aggressive multi-family product slice after the world/chunk preview/export track.

It accepts Goal 040 by user handoff and then proves that three distinct game-family templates can be generated through the same lifecycle without forking architecture:

1. `map_panel_rpg`
2. `survival_sandbox`
3. `first_person_grid_dungeon`

The result must not be paper-only. The goal must produce deterministic generated family lifecycle artifacts and prove at least one simulatable loop per family through Application-owned validation/smoke evidence.

## Gate

```text
multi_family_generated_template_vertical_slice_verification required
```

Do not mark this gate passed inside Goal 043.

## Architecture position

Goal 043 consumes existing seams rather than replacing them:

- Goal 030 semantic artifact registry.
- Goal 031 semantic pack composition blueprint.
- Goal 032 dynamic semantic feature system.
- Goal 033 semantic authoring intent resolver.
- Goal 034 strict LLM draft artifact loop.
- Goal 035 Lua module manifest registry.
- Goal 036 Lua sandbox execution gate.
- Goal 037 hybrid LLM draft + bounded Lua deterministic expansion.
- Goal 038 world-scale region/map foundation.
- Goal 039 runtime chunk delta traversal smoke.
- Goal 040 chunked runtime preview/export multi-family smoke.

## Required lifecycle

For each family, generate a compact deterministic lifecycle record:

```text
family profile
-> selected semantic scopes/features/intents
-> draft/request references
-> Lua expansion request/output references where available
-> world/map/chunk source references
-> preview/export consumer payload references
-> family-specific generated loop plan
-> validation diagnostics
-> simulatable loop proof
```

## Required family behavior

### map_panel_rpg

Must prove:

- region/map-panel style traversal;
- at least one NPC or encounter target;
- at least one quest/event intent;
- at least one item/reward/progress marker;
- preview/export payload compatibility;
- deterministic loop proof.

### survival_sandbox

Must prove:

- resource/hazard loop;
- at least one craft/consume/collect or survival-state transition;
- chunk traversal affects available actions or observations;
- preview/export payload compatibility;
- deterministic loop proof.

### first_person_grid_dungeon

Must prove:

- grid/dungeon traversal lens over the same chunked/region source family;
- encounter/combat/progression or locked route pressure;
- party/blob orientation or corridor/room style pathing as structured data only;
- preview/export payload compatibility;
- deterministic loop proof.

## Multi-family regression

Goal 043 must prove:

- the three families use one shared lifecycle contract;
- family-specific fields are schema-scoped extensions, not architecture forks;
- IDs are stable and deterministic;
- invalid/fake/leak scenarios are causally rejected;
- package/runtime/UI/Unity/provider/LLM/RAG/media are not required.

## Evidence artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/
```

Required files:

```text
family-template-catalog.json
shared-lifecycle-contract.json
family-loop-plan-map-panel-rpg.json
family-loop-plan-survival-sandbox.json
family-loop-plan-first-person-grid-dungeon.json
family-simulatable-loop-proof-map-panel-rpg.json
family-simulatable-loop-proof-survival-sandbox.json
family-simulatable-loop-proof-first-person-grid-dungeon.json
multi-family-regression-matrix.json
preview-export-consumption-matrix.json
invalid-family-diagnostics-matrix.json
multi-family-generated-template-vertical-slice-report.md
```

Evidence must be deterministic:

- no wall-clock timestamps unless existing deterministic convention requires them;
- no absolute local paths;
- stable ordering;
- compact JSON;
- no heavy logs;
- no generated media;
- no final LLM prose.

## Invalid/fake/leak matrix

Minimum required cases:

- duplicate family id;
- unknown family id;
- unknown scenario id;
- missing required lifecycle section;
- missing preview/export source ref;
- missing chunk traversal source ref;
- fake Goal 034/035/036/037/038/039/040 reference;
- family-specific field placed outside family extension scope;
- architecture fork attempt;
- GamePackage schema mutation claim;
- Runtime/UI/Unity/provider/LLM/RAG/media/Lua-source leakage;
- final prose promoted as playable content;
- nondeterministic ordering;
- cross-family ID collision;
- scenario profile mismatch;
- simulatable loop proof without state transition;
- preview/export payload copied without transformation;
- missing validation trace.

Each case must produce stable diagnostics or blocked status.

## Non-goals

- No WinForms/UI.
- No Runtime or Runtime.Abstractions source edits.
- No Unity source edits.
- No GamePackage schema edits.
- No provider/LLM/RAG/media integration.
- No external dependencies.
- No final generated prose.
- No manual playability acceptance.
