# Goal 091 — Deterministic Visual Chunk Stream Window & Infinite World Preview

## Repo
https://github.com/kpCat/LLMGameCreator

## Working copy
`C:\Users\endim\LLMGameCreator\`

## Branch
`main`

## Codex reasoning
very high

## Primary objective
Build the next feature step after Goal 090: a BCL-only Application-side deterministic visual chunk stream window materializer that consumes Goal 090 parameterized visual world profiles and proves that finite, huge sparse and infinite worlds can materialize only the requested chunk windows around player/camera positions.

This remains editor/Application-side proof only: no Runtime changes, no Unity changes, no public GamePackage schema changes, no provider/LLM/media calls, no external dependencies, no binary/raster assets, and no raw full-world cell dump.

## Current context
Goal 090 proved arbitrary finite dimensions, `144x144` as benchmark-only, huge sparse `100000x100000`, infinite chunk-addressed profiles, and data-driven layer sets. Goal 091 must prove stream-window materialization: given `profileId`, `layerId`, center chunk, radius, world seed and generator version, materialize a deterministic visible chunk window with stable chunk keys, seam metadata, water/road/biome continuity, layer portals, cache/reuse proof, and compact SVG/JSON evidence.

## Required preflight
1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes `6cfc71379 GREEN Goal 089 tiered validation pipeline` and `40cd1db08 GREEN Goal 090 parameterized visual world profiles`.
4. Confirm Goal 090 artifacts exist, are GREEN, and remain `accepted=false`.
5. Confirm Goal 090 report proves arbitrary finite matrix, benchmark-only `144x144`, huge sparse no-raw-dump, and infinite chunk addressing.
6. Inspect dirty state before edits. Do not stage/revert unrelated user work.

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
- `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-report.md`
- `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-catalog.json`
- `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-size-matrix.json`
- `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-chunk-address-proof.json`
- `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-sparse-world-proof.json`
- `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-layer-model-proof.json`
- `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-negative-proof.json`
- `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-quality-gate-scan.json`

## Allowed files / areas
- `src/LLMGameCreator.Application/Design/DeterministicVisualChunkStreamWindow/`
- `tests/LLMGameCreator.Tests/Application/DeterministicVisualChunkStreamWindow/`
- `tests/LLMGameCreator.Tests/ProductSmoke/DeterministicVisualChunkStreamWindowProductSmokeTests.cs`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/agent-tasks/goal-091-deterministic-visual-chunk-stream-window/`

## Forbidden files / areas
Do not change public GamePackage schema, Runtime/Runtime.Abstractions, Unity files including `AlphaRuntimeBootstrap.cs`, provider/LLM/RAG/media code, Lua/Scripting, generator-library, `.sln`, `.csproj`, lock files, binary/raster media, real NSFW assets, prompt dumps, provider-output fixtures, or external dependencies. Do not branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Create chunk stream window models
Create a BCL-only namespace with models such as `VisualChunkStreamRequest`, `VisualChunkStreamWindow`, `VisualChunkStreamChunkRef`, `VisualChunkStreamLayerRef`, `VisualChunkStreamSeam`, `VisualChunkStreamCacheRecord`, `VisualChunkStreamDeltaOverlay`, `VisualChunkStreamMaterializationManifest`, `VisualChunkStreamValidationResult`, and `VisualChunkStreamEvidenceResult`.

Required concepts: profile id, world seed, generator version, layer id, center chunk x/y, radius, finite/huge/infinite mode, chunk key, deterministic chunk hash, neighbor seam keys, water/road/biome continuity summaries, layer portal/link references, cache policy metadata, optional delta overlay summary, and no raw heavy cell dump.

### 2. Add deterministic stream fixtures
Create metadata/text-SVG-only fixtures:
1. `finite_custom_255x257_surface_window`: finite non-standard size from Goal090, window radius 1 or 2, explicit boundary clipping proof.
2. `huge_sparse_100000x100000_surface_window`: huge sparse profile, far coordinate, no full world expansion.
3. `infinite_streaming_multilayer_window`: infinite profile, at least two player/camera centers, stable chunks reused where windows overlap.
4. `layer_transition_window_surface_underground_water`: data-driven layers with portal/link summary, not hardcoded surface+underground only.

### 3. Validation rules
Reject unknown profile/layer, missing seed/version, invalid radius, raw full-world dump, finite out-of-bounds window without clipping policy, chunk key mismatch, non-deterministic rerun, seam mismatch, road/water connector mismatch across chunk boundary, duplicate chunk keys, prompt text as source of truth, absolute paths, and adult/rating metadata without safe fallback when present.

### 4. Generate evidence
Create `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/` with:
- `visual-chunk-stream-window-report.md`
- `visual-chunk-stream-window-catalog.json`
- `visual-chunk-stream-materialization-manifest.json`
- `visual-chunk-stream-file-ledger.json`
- `visual-chunk-stream-determinism-proof.json`
- `visual-chunk-stream-seam-proof.json`
- `visual-chunk-stream-cache-reuse-proof.json`
- `visual-chunk-stream-layer-transition-proof.json`
- `visual-chunk-stream-negative-proof.json`
- `visual-chunk-stream-source-lineage.json`
- `visual-chunk-stream-quality-gate-scan.json`
- compact text SVGs under `stream-overviews/` for all four fixtures.

SVGs must be compact text diagrams, not final art.

### 5. Tests
Focused tests must prove all four fixtures materialize, deterministic chunk keys are stable, overlapping infinite windows reuse stable chunk keys, boundary clipping is explicit, huge sparse windows do not create raw full-world dumps, seam/layer-transition proofs pass, invalid matrix rejects expected cases, and evidence is deterministic.

Product smoke must build evidence from repo root and assert finite/huge/infinite/layer-transition fixtures exist, at least one arbitrary non-standard finite size is used, and no raw heavy dump/binary media/Runtime/Unity/provider/schema/dependency changes are introduced.

### 6. Docs/state
Update docs quartet and debt register. Manual gate: `deterministic_visual_chunk_stream_window_verification required`. Goal 091 remains `accepted=false`.

## Validation policy
Use Goal 089 tiered validation:
```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualChunkStreamWindow
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualChunkStreamWindowProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-091-deterministic-visual-chunk-stream-window" -FocusedFilter "DeterministicVisualChunkStreamWindow" -ProductSmokeFilter "DeterministicVisualChunkStreamWindowProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-091-deterministic-visual-chunk-stream-window"
git diff --check
git diff --cached --check
```
Do not require full `check-all.ps1` unless current-goal/spine-fast indicates shared/core risk.

## Quality gate
GREEN only if no forbidden files changed, no binary/raster media/prompt dumps, arbitrary finite + huge sparse + infinite stream windows materialize, deterministic chunk keys and overlapping-window cache reuse are proven, no raw full-world dump is created, seam/layer-transition/negative proofs pass, evidence is deterministic, current-goal and spine-fast validation pass, artifact scope passes, and source formatting remains clean.

## Stop / block conditions
Return BLOCKED if stream window materialization requires public schema, Runtime, Unity, provider, Lua, generator-library changes, if huge/infinite proof cannot be represented without raw heavy dumps, or if artifact scope cannot be satisfied. Return FAILED if build/tests regress due to this goal and cannot be fixed inside allowed files.

## Final report format
Report final status, latest commit before/after, push status, files changed, stream fixtures, finite/huge/infinite proof, seam/cache/layer-transition proof, negative proof, validation tier results, artifact scope, evidence hygiene, remaining debt, final git status and git commands used.

## Mandatory commit/push policy
Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED. Commit message must be one of:
- `GREEN Goal 091 deterministic visual chunk stream window`
- `BLOCKED Goal 091 deterministic visual chunk stream window`
- `FAILED Goal 091 deterministic visual chunk stream window`
