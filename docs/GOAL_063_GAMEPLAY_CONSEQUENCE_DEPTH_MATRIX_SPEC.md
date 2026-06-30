# Goal 063 Spec — Gameplay Consequence Depth Matrix

## Purpose

Turn the existing 9-row full campaign review package into a deeper gameplay-consequence matrix.

The project already proves that generated content can travel through semantic/draft/Lua/world/chunk/media/GamePackage/Unity paths. Goal 063 must prove that those rows can also produce non-trivial gameplay consequences:

- inventory changes;
- quest/progress changes;
- spatial/travel consequences;
- survival/resource/crafting consequences;
- combat/encounter/progression pressure;
- faction/reputation/social consequence;
- save/load/replay stability;
- Unity Alpha markers for each family row.

## Non-goals

Do not implement a full RPG combat engine, economy engine, AI planner, UI editor, or runtime rewrite.

Do not change public GamePackage schema.

Do not call LLM/provider/RAG/media tools.

Do not generate arbitrary Lua source or expand the Lua executor surface.

Do not modify Runtime/Runtime.Abstractions unless the task stops BLOCKED and explains why.

## Intended scope

Create an Application-layer seam:

```text
src/LLMGameCreator.Application/Design/GameplayConsequenceDepthMatrix/**
```

Suggested components:

- `GameplayConsequenceDepthMatrixModels`
- `GameplayConsequenceDepthMatrixSourceLoader`
- `GameplayConsequenceDepthMatrixCatalog`
- `GameplayConsequenceCommandPlanBuilder`
- `GameplayConsequenceRuntimeProjector`
- `GameplayConsequenceReplayAuditor`
- `GameplayConsequenceUnityProofRunner`
- `GameplayConsequenceDepthMatrixValidator`
- `GameplayConsequenceDepthMatrixEvidenceService`
- `GameplayConsequenceDepthMatrixHash`

Naming may follow local style.

## Required scenario rows

Use the current family/seed matrix:

```text
map_panel_rpg / seed_alpha
map_panel_rpg / seed_beta
map_panel_rpg / seed_gamma
survival_sandbox / seed_alpha
survival_sandbox / seed_beta
survival_sandbox / seed_gamma
first_person_grid_dungeon / seed_alpha
first_person_grid_dungeon / seed_beta
first_person_grid_dungeon / seed_gamma
```

## Required gameplay consequence proof

Each row must produce a deterministic gameplay command plan with at least three state-changing steps.

Minimum per-family shape:

### map_panel_rpg

Must include at least:

- spatial travel or region/detail step;
- NPC/quest/event interaction;
- inventory or reward delta;
- faction/reputation or social consequence where available.

### survival_sandbox

Must include at least:

- hazard or need pressure;
- resource collection;
- consume/craft/recover or state mitigation;
- inventory/resource delta.

### first_person_grid_dungeon

Must include at least:

- grid orientation/traversal step;
- encounter/pressure step;
- loot/progression or unlock step;
- blocked/valid movement distinction.

## Runtime proof

Goal 063 must prove state transitions, not only write plans.

Preferred proof:

- use existing runtime serializer/snapshot/state patterns if available;
- or create an Application-level projection that explicitly consumes existing GamePackage/runtime state artifacts and produces deterministic before/after state records.

Required evidence:

- before/after deltas;
- row-specific command ids;
- causal expected outcomes;
- save/load or serializer roundtrip;
- same-seed replay determinism;
- distinct rows are not merely different ids.

## Unity proof

Narrowly extend the existing Unity Alpha diagnostic route if needed.

Required Unity/player markers:

```text
gameplay_consequence_goal=goal063
gameplay_consequence_row=<family>/<seed>
gameplay_consequence_step=<step id>
gameplay_consequence_delta=<delta id>
gameplay_consequence_completed=<family>/<seed>
gameplay_consequence_matrix_completed=true
```

Do not invent a broad Unity UI. IMGUI/diagnostic markers are sufficient.

## Required artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-063-gameplay-consequence-depth-matrix/
```

Required files:

```text
source-manifest.json
gameplay-consequence-catalog.json
gameplay-command-plan-matrix.json
runtime-state-delta-matrix.json
save-load-replay-audit.json
family-consequence-summary.json
unity-command-plan.json
unity-player-proof-summary.json
preview-export-gameplay-payload.json
invalid-diagnostics-matrix.json
artifact-scope-report.json
gameplay-consequence-depth-matrix-report.md
```

Also write one compact per-row proof JSON file if this is consistent with local artifact style:

```text
rows/<family>-<seed>-gameplay-proof.json
```

No timestamps, absolute paths, heavy logs, or nondeterministic ordering.

## Invalid/fake/leak matrix

Cover at least:

- missing Goal 060 package row;
- missing Goal 061 review package row;
- missing Goal 062 spatial-detail row;
- fake family;
- fake seed;
- fake package id;
- fake command id;
- duplicate command id;
- command without state delta;
- delta without before/after values;
- replay mismatch;
- save/load mismatch;
- row hash collision;
- no meaningful variance;
- unsafe path;
- final prose treated as gameplay consequence;
- provider/LLM/RAG/media generation claim;
- Runtime/UI/Unity broad mutation claim;
- GamePackage schema mutation claim;
- Lua arbitrary execution/source claim;
- nondeterministic ordering.

## Expected final status

If successful:

```text
implementationStatus=GREEN
accepted=false
manualGate=gameplay_consequence_depth_matrix_verification
```

The gate must remain required, not passed.
