# External scouting — Goal 063 Gameplay Consequence Depth Matrix

## Goal

Goal 063 should not add another registry-only layer. It should consume the accepted full campaign / package / Unity / spatial-detail proof chain and deepen actual gameplay consequences across all existing family/seed rows.

## Current repo context

Use the current `main` state as source of truth:

- Goal 061 was accepted by Goal 062 handoff.
- Goal 062 produced constrained spatial detail generation and remains produced-for-review until this task records the user handoff acceptance.
- Existing proof chain includes:
  - strict LLM draft loop;
  - Lua manifest/sandbox/bounded expansion;
  - world/chunk/runtime traversal;
  - multi-family template loops;
  - full generator without media;
  - media campaign/materialization/binding;
  - Unity Alpha multi-family playable loop;
  - full media-bound campaign;
  - 9-row variability matrix;
  - 9 validator-clean GamePackage JSON artifacts;
  - full campaign playable review package RC;
  - constrained spatial-detail rows with Unity markers.

## Dependency decision

No new dependencies for Goal 063.

Reasoning:

- The next risk is not lack of libraries; it is shallow gameplay semantics.
- Existing `GamePackage`, runtime state serializer/snapshot proof, package rows and Unity Alpha markers are enough for a bounded gameplay consequence matrix.
- External behavior-tree/GOAP/rules engines would increase dependency surface before the project has a stable gameplay-consequence contract.
- This goal must be BCL-only and existing-runtime-compatible.

## Deliberately not added now

- Behavior tree libraries.
- GOAP planners.
- ECS frameworks.
- Economy simulation packages.
- Dialogue engines.
- New Lua scripts.
- Any LLM/provider/RAG/media generation integration.

## Future references

Useful later, not now:

- Behavior trees / GOAP for richer NPC decision-making after the current consequence matrix exists.
- `mxgmn/MarkovJunior`, WFC and texture synthesis ideas for spatial detail, not gameplay consequence logic.
- Economy simulation / market balancing libraries only after the in-house economy consequence contract is proven.

## Architecture decision

Goal 063 must be an in-house Application-layer gameplay consequence depth matrix:

```text
Goal 060 package rows
+ Goal 061 playable review package RC
+ Goal 062 spatial detail rows
-> family/seed gameplay command plans
-> runtime-owned state deltas
-> save/load/replay proof
-> Unity Alpha gameplay consequence markers
```

This is intentionally not a UI goal, not a schema-migration goal, and not a Runtime rewrite.
