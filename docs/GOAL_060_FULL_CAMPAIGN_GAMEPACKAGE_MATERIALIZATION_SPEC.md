# Goal 060 — Full Campaign GamePackage Materialization Matrix

## Goal

Turn the Goal 059 full generator variability matrix into real, validator-clean GamePackage materialization artifacts and prove runtime/Unity consumption through the existing proof chain.

This goal is intentionally more concrete than previous proof-only seams:

```text
Goal 059 full generator variability rows
 -> package materialization plans
 -> validator-clean GamePackage JSON artifacts
 -> runtime/state smoke plans
 -> preview/export payloads
 -> Unity Alpha package-consumption markers
 -> compact review evidence
```

## Non-goals

- Do not change public GamePackage schema.
- Do not introduce a new runtime architecture.
- Do not add external dependencies.
- Do not generate media or call providers.
- Do not call LLM/RAG.
- Do not execute arbitrary Lua beyond already accepted bounded Goal 037 fixture path.
- Do not broaden Unity beyond a narrow Alpha proof extension if needed.
- Do not create a fake materialization proof that only copies prior JSON evidence.

## Required proof

A GREEN result requires at minimum:

1. Goal 059 accepted in state docs by user handoff before Goal 060.
2. At least nine package materialization rows corresponding to the 3 family x 3 seed matrix.
3. At least one real GamePackage JSON artifact per family, and preferably one per matrix row if existing schema supports it.
4. Every materialized package must pass existing GamePackage validation path or an existing accepted package validation/assembly seam.
5. Package ids, source matrix row ids, family ids, seed ids and hashes must be traceable.
6. Runtime/state smoke must prove at least one state-changing loop per family through existing runtime-owned state seams.
7. Unity Alpha proof must load/consume the materialized package-selection manifest and emit deterministic markers for all three families.
8. Invalid/fake/leak matrix must reject fake row ids, fake package ids, stale hashes, schema mutation claims, Runtime/UI/Unity/provider/LLM/RAG/Lua leakage and nondeterministic order.

If exact 9 package JSON artifacts are impossible without public schema changes, the task may still be GREEN only if it produces:

- a causal proof explaining which rows are package-materializable under current schema;
- at least one validator-clean materialized package per family;
- explicit blocked/future-required gaps for the remaining rows;
- runtime/Unity proof for materialized rows;
- no fake success for blocked rows.

Otherwise commit/push BLOCKED.

## Expected artifact folder

```text
.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/
```

Suggested artifacts:

```text
source-campaign-matrix-manifest.json
package-materialization-plan.json
materialized-package-inventory.json
package-validation-matrix.json
runtime-consumption-matrix.json
unity-package-consumption-command-plan.json
unity-package-consumption-proof.json
preview-export-package-payloads.json
invalid-package-materialization-diagnostics-matrix.json
full-campaign-gamepackage-materialization-report.md
artifact-scope-report.json
```

If packages are emitted as physical JSON files, place them under a deterministic subfolder such as:

```text
packages/<family>/<row-id>/game-package.json
```

Keep paths relative, compact, deterministic and reviewable.
