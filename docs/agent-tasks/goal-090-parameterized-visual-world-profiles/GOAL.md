# Goal 090 — Parameterized Visual World Profiles & Infinite Chunk Addressing

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Implement a BCL-only Application-side visual world profile/addressing seam that proves the visual generator is not tied to any fixed map size such as `144x144`, `256x256`, or `100000x100000`.

The model must support:

- arbitrary finite region dimensions within validation bounds;
- arbitrary data-driven layer sets;
- configurable patch/chunk sizes;
- sparse huge finite worlds;
- infinite / unbounded chunk-addressed worlds;
- deterministic chunk keys and stream windows;
- no full raw-cell dump for huge or infinite profiles;
- `144x144` only as one benchmark fixture, not as a domain constant.

This goal must not change Runtime, Unity, public GamePackage schema, providers, Lua, generator-library, `.sln`, `.csproj`, or external dependencies.

## Why this matters

Goal 088 created a Heroes-like 144x144 surface/underground proof. That was useful as a benchmark, but the generator must not become fixed to that size or to the exact `surface + underground` layer model.

The intended architecture is:

`microtile -> patch -> finite region -> arbitrary region profile -> sparse virtual world -> infinite deterministic chunk addressing`

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `d8cd8059 GREEN Goal 087 deterministic visual map patch composer`
   - `114a3280f BLOCKED Goal 088 deterministic visual region composer`
   - `9bfdff86e GREEN Goal 088A check-all hang triage validation repair`
   - `6cfc71379 GREEN Goal 089 tiered validation pipeline`
4. Confirm Goal 088 artifacts exist and are produced-for-review / accepted=false.
5. Confirm Goal 088A proves check-all passed and the previous blocker was timeout/wall-clock.
6. Confirm Goal 089 validation pipeline exists:
   - `.devflow/scripts/check-current-goal.ps1`
   - `.devflow/scripts/check-spine-fast.ps1`
   - `.devflow/scripts/check-all-observed.ps1`
   - `docs/VALIDATION_PIPELINE.md`
   - `.devflow/validation-profiles/validation-tiers.json`
7. Inspect current dirty state before edits. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/VALIDATION_PIPELINE.md`
- `.devflow/validation-profiles/validation-tiers.json`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md`
- `docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md`
- `docs/deepsearch/03_PSEUDO3D_FIRST_PERSON_FROM_2D_ASSETS.md`
- `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-composer-report.md`
- `.llmgc/procedural/goal-088-deterministic-visual-region-composer/visual-region-composer-report.md`
- `.llmgc/procedural/goal-088-deterministic-visual-region-composer/visual-region-quality-gate-scan.json`
- `.llmgc/procedural/goal-088a-check-all-hang-triage-validation-repair/check-all-hang-triage-report.md`
- `.llmgc/procedural/goal-089-tiered-validation-pipeline/tiered-validation-pipeline-report.md`

## Allowed files / areas

- New Application namespace:
  - `src/LLMGameCreator.Application/Design/ParameterizedVisualWorldProfiles/`
- Tests:
  - `tests/LLMGameCreator.Tests/Application/ParameterizedVisualWorldProfiles/`
  - `tests/LLMGameCreator.Tests/ProductSmoke/ParameterizedVisualWorldProfilesProductSmokeTests.cs`
- Evidence:
  - `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/`
- Docs/state:
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
  - `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- Artifact scope:
  - `.devflow/artifact-scope/artifact-scope-policy.json`
- Task pack:
  - `docs/agent-tasks/goal-090-parameterized-visual-world-profiles/`

## Forbidden files / areas

Do not change:

- public GamePackage schema;
- Runtime / Runtime.Abstractions;
- Unity files, including `AlphaRuntimeBootstrap.cs`;
- Infrastructure provider / LLM / RAG / media provider code;
- Lua / Scripting;
- generator-library;
- `.sln`;
- `.csproj`;
- package lock files;
- binary media assets;
- generated raster assets;
- real NSFW assets;
- explicit prompt dumps or provider-output fixtures;
- external dependencies.

Do not branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Create parameterized visual world profile models

Create a BCL-only namespace for arbitrary finite profiles, sparse huge profiles and infinite chunk addressing.

Recommended types:

- `VisualWorldProfile`
- `VisualWorldLayerProfile`
- `VisualRegionSize`
- `VisualDimensionRange`
- `VisualChunkProfile`
- `VisualPatchProfile`
- `VisualVirtualWorldBounds`
- `VisualSparseRegionIndex`
- `VisualChunkAddress`
- `VisualChunkKey`
- `VisualStreamWindow`
- `VisualLayerLink`
- `VisualWorldProfileValidationResult`
- `VisualWorldProfileEvidenceResult`

Required concepts:

- `profileId`
- `worldSeed`
- `generatorVersion`
- `coordinateOrigin`
- `finiteWidth`
- `finiteHeight`
- `virtualBounds`
- `isInfinite`
- `layerId`
- `layerKind`
- `chunkWidth`
- `chunkHeight`
- `patchWidth`
- `patchHeight`
- `logicalCellCount` as computed summary only
- `rawCellDumpAllowed=false` for huge/infinite profiles
- deterministic chunk key formula
- stream window radius/size
- source lineage to Goal087/088

### 2. Explicitly avoid fixed-size architecture

Domain code must not treat `144`, `256`, or `100000` as architectural constants.

Allowed:
- named test/fixture values;
- benchmark profile metadata;
- evidence fixtures.

Forbidden:
- fixed-size-only validators;
- hardcoded branch logic that only accepts 144/256/100000;
- domain names implying world generation only supports those sizes;
- code paths that require exactly two layers named `surface` and `underground`.

### 3. Add profile fixtures and size matrix

Create deterministic metadata-only profile fixtures.

Minimum named fixtures:

1. `benchmark_heroes_144x144_surface_underground`
   - finite;
   - 144x144;
   - layers: surface + underground;
   - explicitly marked as benchmark, not architectural limit.

2. `finite_custom_sizes_matrix`
   - finite arbitrary dimension samples, not just 144/256.
   - include at least: `1x1`, `17x31`, `64x96`, `144x144`, `255x257`, `512x384`.
   - all must use the same validator/model path.

3. `huge_sparse_100000x100000_multilayer`
   - finite huge virtual bounds;
   - sparse chunk index;
   - layers at least surface, underground, underwater/interior or another data-driven layer;
   - prove only sampled chunks/anchors are materialized.

4. `infinite_streaming_world_multilayer`
   - infinite coordinate mode;
   - deterministic chunk key proof;
   - stream window around one or more player positions;
   - no finite raw cell count required beyond sampled chunks.

### 4. Validation rules

Reject at least:

- fixed-size-only profile pretending to be generic;
- finite profile with invalid dimensions;
- huge profile that attempts raw cell dump;
- infinite profile that declares finite-only materialization;
- invalid layer ids;
- duplicate layer ids;
- hardcoded-only `surface + underground` layer requirement;
- chunk size <= 0;
- patch size <= 0;
- patch/chunk incompatibility where required;
- missing world seed;
- missing generator version;
- absolute output paths;
- non-deterministic chunk key;
- layer link to unknown layer;
- stream window without center or radius;
- adult/rating metadata without safe fallback when present;
- prompt text as source of truth.

### 5. Generate deterministic evidence

Create `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/`.

Recommended artifacts:

- `visual-world-profile-report.md`
- `visual-world-profile-catalog.json`
- `visual-world-profile-size-matrix.json`
- `visual-world-profile-validation-matrix.json`
- `visual-world-profile-negative-proof.json`
- `visual-world-profile-chunk-address-proof.json`
- `visual-world-profile-sparse-world-proof.json`
- `visual-world-profile-layer-model-proof.json`
- `visual-world-profile-source-lineage.json`
- `visual-world-profile-quality-gate-scan.json`
- `profile-overviews/benchmark_heroes_144x144_surface_underground.svg`
- `profile-overviews/finite_custom_sizes_matrix.svg`
- `profile-overviews/huge_sparse_100000x100000_multilayer.svg`
- `profile-overviews/infinite_streaming_world_multilayer.svg`

SVGs must be compact text diagrams, not final art and not raw cell dumps.

### 6. Tests

Focused tests must prove:

- arbitrary finite size matrix validates through one generic path;
- 144x144 is only one benchmark profile;
- a non-power-of-two / non-Heroes size such as 255x257 validates;
- huge 100000x100000 profile validates as sparse and rejects raw dump;
- infinite profile validates chunk addressing and stream windows;
- deterministic chunk keys are stable across reruns and differ when seed/layer/chunk/version differs;
- layer sets are data-driven and not restricted to surface/underground;
- invalid matrix rejects all expected cases;
- evidence is deterministic.

Product smoke must build evidence from repo root, read back report/catalog/proofs, and assert:

- arbitrary finite profile mode is present;
- benchmark 144x144 is present but marked as benchmark;
- huge sparse mode is present;
- infinite mode is present;
- no raw heavy cell dump;
- no binary/raster media;
- no Runtime/Unity/provider/schema/dependency changes.

### 7. Docs/state

Update docs quartet and debt register.

Goal 090 manual gate:
`parameterized_visual_world_profiles_verification required`

Goal 090 `accepted=false`.

Record that Goal 088 fixed-size concern is now handled at the profile/addressing layer if validation passes.

## Validation policy

Use the Goal 089 tiered validation policy.

Required:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter ParameterizedVisualWorldProfiles
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter ParameterizedVisualWorldProfilesProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-090-parameterized-visual-world-profiles" -FocusedFilter "ParameterizedVisualWorldProfiles" -ProductSmokeFilter "ParameterizedVisualWorldProfilesProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-090-parameterized-visual-world-profiles"
git diff --check
git diff --cached --check
```

Do not require full `check-all.ps1` for this ordinary feature goal unless the new scripts indicate a shared/core risk. Do not ask the user to run check-all manually.

## Quality gate

GREEN only if:

- no forbidden files changed;
- no Unity/Runtime/provider/schema/project/dependency changes;
- no binary/raster media added;
- no prompt dumps;
- 144x144 is not hardcoded as the only size;
- arbitrary finite sizes validate through a generic path;
- non-standard sizes such as 17x31 and 255x257 pass;
- 100000x100000 huge sparse profile works without raw cell dump;
- infinite profile has deterministic chunk addressing and stream windows;
- layer sets are data-driven;
- source lineage to Goal087/088 exists;
- evidence is deterministic;
- current-goal and spine-fast validation pass;
- artifact scope passes;
- source formatting guard remains clean.

## Stop / block conditions

Return BLOCKED if:

- parameterized profiles require public GamePackage schema changes;
- chunk addressing requires Runtime/Unity/provider/Lua/generator-library changes;
- huge/infinite proof cannot be represented without raw heavy dumps;
- artifact scope cannot be satisfied.

Return FAILED if:

- build/tests regress due to this goal and cannot be fixed inside allowed files.

## Final report format

Report:

- Final status.
- Latest commit before/after.
- Push status.
- Files changed.
- Profile fixtures added.
- Proof that 144x144 is only a benchmark.
- Arbitrary finite size matrix proof summary.
- Huge sparse / infinite proof summary.
- Negative proof summary.
- Validation tier commands and results.
- Artifact scope result.
- Evidence hygiene.
- Remaining P2/P3 debt.
- Final git status.
- Git commands used and why.

## Mandatory commit/push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED`.

Commit message must honestly reflect status:

- `GREEN Goal 090 parameterized visual world profiles`
- `BLOCKED Goal 090 parameterized visual world profiles`
- `FAILED Goal 090 parameterized visual world profiles`
