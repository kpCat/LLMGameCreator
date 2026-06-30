# Goal 059 Spec — Full Generator Variability Regression Matrix

## Goal id

`goal_059_full_generator_variability_matrix`

## Gate

`full_generator_variability_regression_matrix_verification required`

## Purpose

Consume the accepted/proven Goal 034-058 chain and prove that the full media-bound generator campaign is not a single overfit happy path.

Goal 059 must create a deterministic variation matrix across multiple seeds and families, prove replayability, preserve causal variance metrics, stage Unity Alpha matrix command plans, and execute the existing player proof route with matrix-specific markers.

## What should become more real

The generator can produce and replay multiple distinct media-bound campaigns across the supported family set while preserving deterministic hashes, review/package/media/provenance integrity, and Unity Alpha player proof.

## Required scenarios

Minimum matrix:

- Families:
  - `map_panel_rpg`
  - `survival_sandbox`
  - `first_person_grid_dungeon`
- Seeds:
  - `seed_alpha`
  - `seed_beta`
  - `seed_gamma`

Minimum matrix rows: 9.

Optional stretch:

- add one metamodule/kingdom stress row per family if the source evidence can support it without fake data.

## Required proof chain

Goal 059 must consume the existing Goal 058 campaign evidence and prove a matrix chain:

```text
Goal 058 full media-bound campaign
 -> deterministic seed/profile matrix
 -> per-row campaign facts
 -> variance metrics
 -> replay proof
 -> review package matrix manifest
 -> preview/export matrix payload
 -> Unity Alpha matrix command plan
 -> Unity/player matrix markers
```

## Required evidence artifacts

Write compact deterministic artifacts under:

`.llmgc/procedural/goal-059-full-generator-variability-regression-matrix/`

Required files:

- `matrix-source-manifest.json`
- `seed-profile-matrix.json`
- `matrix-row-map-panel-rpg-seed-alpha.json`
- `matrix-row-map-panel-rpg-seed-beta.json`
- `matrix-row-map-panel-rpg-seed-gamma.json`
- `matrix-row-survival-sandbox-seed-alpha.json`
- `matrix-row-survival-sandbox-seed-beta.json`
- `matrix-row-survival-sandbox-seed-gamma.json`
- `matrix-row-first-person-grid-dungeon-seed-alpha.json`
- `matrix-row-first-person-grid-dungeon-seed-beta.json`
- `matrix-row-first-person-grid-dungeon-seed-gamma.json`
- `variance-metrics.json`
- `replay-determinism-proof.json`
- `review-package-matrix-manifest.json`
- `preview-export-matrix-payload.json`
- `unity-alpha-matrix-command-plan.json`
- `unity-alpha-matrix-player-proof.json`
- `invalid-matrix-diagnostics.json`
- `full-generator-variability-regression-matrix-report.md`
- artifact-scope reports if the existing flow produces them.

Do not include nondeterministic timestamps, absolute paths, huge logs, or build outputs.

## Required implementation shape

Add an Application-only seam under:

`src/LLMGameCreator.Application/Design/FullGeneratorVariabilityRegressionMatrix/`

Suggested small components:

- `FullGeneratorVariabilityMatrixModels.cs`
- `FullGeneratorVariabilityMatrixSourceLoader.cs`
- `FullGeneratorVariabilityMatrixBuilder.cs`
- `FullGeneratorVariabilityMatrixValidator.cs`
- `FullGeneratorVariabilityMatrixEvidenceService.cs`
- `FullGeneratorVariabilityMatrixHash.cs`
- `FullGeneratorVariabilityUnityProofRunner.cs` if Unity/player proof orchestration needs a small Application-side runner.

Avoid monolithic classes.

## Required behavior

### 1. Accept Goal 058 by handoff

Record in state docs that:

`full_media_bound_generator_campaign_verification passed before Goal 059`

Do not mark Goal 059 passed.

### 2. Source loading

Load and validate Goal 058 evidence from:

`.llmgc/procedural/goal-058-full-media-bound-generator-campaign/`

Required consumed facts:

- campaign source manifest;
- campaign plan;
- unified review package manifest;
- preview/export campaign payload;
- Unity Alpha campaign command plan;
- Unity Alpha campaign player proof;
- family run proofs for all three families;
- media-bound manifests/fixtures if required for matrix row references.

### 3. Deterministic seed/profile matrix

Create at least 9 matrix rows from family x seed.

Each row must include:

- row id;
- family id;
- seed id;
- source campaign ids/hashes;
- selected world/map/chunk refs;
- selected family-loop refs;
- selected media refs;
- selected review package refs;
- deterministic derived campaign hash;
- expected Unity markers;
- variance explanation.

### 4. Variance proof

Do not merely change a hash. Prove meaningful row differences.

Metrics must include at least:

- distinct campaign row count;
- per-family row count;
- per-seed row count;
- distinct derived campaign hashes;
- family marker coverage;
- media binding coverage;
- route/chunk/family-loop coverage if available;
- minimum difference dimensions per pair or per family/seed group;
- overfit warning count.

Fail/diagnose if all rows are effectively identical except row id.

### 5. Replay determinism proof

For every matrix row:

- build row once;
- build row again from the same source facts;
- compare stable JSON/hash;
- record match/mismatch.

### 6. Review package matrix manifest

Produce a review package matrix manifest that references the existing media-bound review package and matrix rows without copying heavy build outputs.

### 7. Preview/export matrix payload

Produce a preview/export payload that a future UI/export consumer can use to show/select matrix rows.

### 8. Unity Alpha matrix command plan and proof

Prefer one Unity/player execution with a matrix command plan that emits markers for all 9 rows.

Required markers conceptually:

- `full_generator_matrix_loaded=true`
- `matrix_row_started=<row id>`
- `matrix_row_family=<family id>`
- `matrix_row_seed=<seed id>`
- `matrix_row_hash=<hash>`
- `matrix_row_completed=<row id>`
- `full_generator_matrix_completed=true`

If one player execution cannot honestly cover all 9 rows, execute a smaller bounded player proof for at least one row per family and record the unexecuted rows as Application-level replay proof only. In that case the status may still be GREEN only if the task report clearly explains the bounded Unity coverage and all required Application-level matrix/replay proofs pass. If the existing Unity route cannot be extended safely, commit/push BLOCKED.

### 9. Invalid/fake/leak matrix

Cover at least:

- missing Goal 058 source;
- stale/mismatched source hash;
- duplicate row id;
- fake family;
- fake seed;
- missing matrix row;
- identical-row overfit;
- nondeterministic replay;
- missing Unity marker;
- malformed preview/export payload;
- unsafe relative path;
- provider/network/LLM/RAG claim;
- GamePackage schema mutation claim;
- Runtime broad mutation claim;
- UI/WinForms mutation claim;
- Unity broad mutation claim outside allowed bootstrap route;
- media generation/import claim;
- Lua arbitrary execution claim.

## Out of scope

- No provider/LLM/RAG calls.
- No real media generation or network import.
- No new external dependencies.
- No GamePackage schema changes.
- No Runtime/Runtime.Abstractions broad changes.
- No WinForms/UI changes.
- No generator-library changes.
- No `.sln`/`.csproj` changes.
- No arbitrary Lua execution.
- No heavy Unity build/log output in Git.

## Allowed narrow Unity change

If necessary, edit only:

`unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`

for deterministic matrix command-plan markers.

Do not introduce new Unity packages or scenes.
