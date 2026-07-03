# Goal 093 — Visual Chunk Cache Export Contract & Runtime Handoff Sidecar

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Create the next bridge after Goal 091/092: a BCL-only Application-side visual chunk cache export contract and runtime-handoff sidecar that packages deterministic visual stream-window outputs into a compact, read-back-validatable cache/export manifest for future Runtime/Unity consumption.

This goal must remain editor/Application-side only. It must not change Runtime, Unity, public GamePackage schema, providers, Lua, generator-library, project files or dependencies.

The purpose is to prove that generated visual chunks can be exported as a deterministic cache package with relative paths, chunk keys, profile/layer lineage, stream-window membership, invalidation metadata, safe/rating metadata, and read-back/negative proof.

## Current context

Recent visual stack:

- Goal 086: deterministic microtile materializer.
- Goal 087: deterministic visual map patch composer.
- Goal 088/088A: region composer and check-all unblock.
- Goal 089: tiered validation pipeline.
- Goal 090: arbitrary finite / huge sparse / infinite world profiles.
- Goal 091: deterministic chunk stream windows.
- Goal 092: WinForms visual stream preview workspace.
- Goal 092A: split oversized preview service and strengthened source-health guard.

Goal 093 must not create a renderer or runtime implementation. It creates the export/cache contract that a renderer/runtime can consume later.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `871741346 GREEN Goal 091 deterministic visual chunk stream window`
   - `18d98f381 GREEN Goal 092 visual world stream preview workspace`
   - `bf286b608 GREEN Goal 092A visual world preview service split source health`
4. Confirm Goal 091, 092 and 092A artifacts exist and remain `accepted=false`.
5. Confirm Goal 092A source-health evidence shows no Goal092 namespace file over 1000 lines and `VisualWorldStreamPreviewWorkspaceService.cs` below 700 lines.
6. Confirm Goal 089 validation pipeline exists and use it.
7. Inspect dirty state before edits. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/VALIDATION_PIPELINE.md`
- `.devflow/validation-profiles/validation-tiers.json`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-090-parameterized-visual-world-profiles/visual-world-profile-report.md`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-window-report.md`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-window-catalog.json`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-materialization-manifest.json`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-file-ledger.json`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-determinism-proof.json`
- `.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/visual-chunk-stream-cache-reuse-proof.json`
- `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/visual-world-stream-preview-workspace-report.md`
- `.llmgc/procedural/goal-092a-visual-world-preview-service-split-source-health/source-health-before-after.json`
- `.llmgc/procedural/goal-092a-visual-world-preview-service-split-source-health/quality-gate-scan.json`

## Allowed files / areas

- New Application namespace:
  - `src/LLMGameCreator.Application/Design/VisualChunkCacheExportContract/`
- Tests:
  - `tests/LLMGameCreator.Tests/Application/VisualChunkCacheExportContract/`
  - `tests/LLMGameCreator.Tests/ProductSmoke/VisualChunkCacheExportContractProductSmokeTests.cs`
- Evidence:
  - `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/`
- Docs/state:
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
  - `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- Artifact scope:
  - `.devflow/artifact-scope/artifact-scope-policy.json`
- Task pack:
  - `docs/agent-tasks/goal-093-visual-chunk-cache-export-contract/`

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

### 1. Create visual chunk cache/export models

Create a BCL-only namespace for export/cache contracts.

Recommended types:

- `VisualChunkCacheExportPackage`
- `VisualChunkCacheExportManifest`
- `VisualChunkCacheRecord`
- `VisualChunkCacheKey`
- `VisualChunkCacheArtifactRef`
- `VisualChunkCacheStreamWindowRef`
- `VisualChunkCacheLayerRef`
- `VisualChunkCacheInvalidationRule`
- `VisualChunkCacheDeltaOverlayRef`
- `VisualChunkCacheRuntimeHandoffSidecar`
- `VisualChunkCacheValidationResult`
- `VisualChunkCacheEvidenceResult`

Required metadata concepts:

- package id;
- source goals and hashes;
- profile id;
- world seed;
- generator version;
- layer id;
- chunk address;
- chunk key;
- chunk hash;
- stream window id membership;
- relative artifact paths;
- preview SVG/text artifact refs;
- cache policy;
- invalidation keys;
- delta overlay marker;
- safe/rating metadata summary;
- export target kind: `editorReview`, `runtimeHandoff`, `unityStreamingAssetsCandidate`;
- no absolute paths;
- no raw full-world cell dump.

### 2. Build deterministic export fixtures

Create deterministic metadata/text-only fixtures from real Goal091 artifacts.

Minimum export packages:

1. `finite_custom_255x257_window_cache_export`
   - source Goal091 finite custom stream window.

2. `huge_sparse_100000x100000_window_cache_export`
   - source Goal091 huge sparse stream window.
   - prove only materialized chunks are exported.

3. `infinite_streaming_overlap_cache_export`
   - source Goal091 infinite stream windows.
   - prove overlapping chunks share identical cache keys.

4. `layer_transition_runtime_handoff_sidecar`
   - source Goal091 layer transition window.
   - produce runtime-handoff sidecar metadata only.

No binary/raster output. Compact JSON and text manifests only.

### 3. Validation rules

Reject at least:

- unknown source chunk key;
- absolute artifact path;
- missing chunk hash;
- duplicate chunk key with conflicting hash;
- stream window membership mismatch;
- huge/infinite export that tries full-world raw dump;
- missing source lineage to Goal090/091;
- stale generator version mismatch;
- cache invalidation rule with unknown key;
- runtime handoff sidecar with provider call instructions;
- prompt text as source of truth;
- adult/rating metadata without safe fallback when present;
- binary/raster artifact ref in this goal.

### 4. Generate evidence

Create `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/`.

Recommended artifacts:

- `visual-chunk-cache-export-report.md`
- `visual-chunk-cache-export-manifest.json`
- `visual-chunk-cache-file-ledger.json`
- `visual-chunk-cache-runtime-handoff-sidecar.json`
- `visual-chunk-cache-invalidation-matrix.json`
- `visual-chunk-cache-readback-proof.json`
- `visual-chunk-cache-overlap-reuse-proof.json`
- `visual-chunk-cache-negative-proof.json`
- `visual-chunk-cache-source-lineage.json`
- `visual-chunk-cache-quality-gate-scan.json`

Evidence must prove:

- Goal091 stream-window chunks were consumed by relative paths;
- finite / huge sparse / infinite / layer transition exports exist;
- overlapping infinite chunks share stable cache keys;
- no full-world raw cell dump;
- read-back validation succeeds;
- negative proof rejects expected failures;
- no Runtime/Unity/provider/schema/project/dependency changes.

### 5. Tests

Focused tests must prove:

- export manifest builds deterministically from repo root;
- all four export fixtures exist;
- no absolute paths;
- all cache records have stable chunk keys and hashes;
- overlap reuse proof passes;
- huge sparse export has no raw world dump;
- runtime handoff sidecar is metadata-only and contains no provider/runtime execution instructions;
- invalid matrix rejects all expected failures;
- evidence is deterministic.

Product smoke must build evidence, read back manifest/sidecar/readback/negative proof and assert:

- finite / huge / infinite / layer-transition export entries exist;
- no binary/raster media;
- no forbidden area changes;
- no prompt dumps.

### 6. Docs/state

Update docs quartet and debt register.

Goal 093 manual gate:
`visual_chunk_cache_export_contract_verification required`

Goal 093 `accepted=false`.

Record that this prepares runtime/Unity consumption but does not implement runtime or Unity consumption.

## Validation policy

Use Goal 089 tiered validation.

Required:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualChunkCacheExportContract
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualChunkCacheExportContractProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-093-visual-chunk-cache-export-contract" -FocusedFilter "VisualChunkCacheExportContract" -ProductSmokeFilter "VisualChunkCacheExportContractProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-093-visual-chunk-cache-export-contract"
git diff --check
git diff --cached --check
```

Full `check-all.ps1` is not required for this ordinary feature goal unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if:

- no forbidden files changed;
- no Runtime/Unity/provider/schema/project/dependency changes;
- no binary/raster media added;
- no prompt dumps;
- finite/huge/infinite/layer-transition export packages exist;
- read-back proof passes;
- overlap reuse proof passes;
- huge/infinite export avoids raw full-world dump;
- runtime handoff sidecar is metadata-only and has no execution/provider calls;
- evidence is deterministic;
- current-goal and spine-fast validation pass;
- artifact scope passes;
- source formatting guard remains clean.

## Stop / block conditions

Return BLOCKED if:

- export contract requires public GamePackage schema changes;
- runtime handoff sidecar requires Runtime/Unity/provider/Lua/generator-library changes;
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
- Export fixtures added.
- Runtime handoff sidecar summary.
- Readback/overlap/negative proof summary.
- Validation tier commands and results.
- Artifact scope result.
- Evidence hygiene.
- Remaining P2/P3 debt.
- Final git status.
- Git commands used and why.

## Mandatory commit/push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:

- `GREEN Goal 093 visual chunk cache export contract`
- `BLOCKED Goal 093 visual chunk cache export contract`
- `FAILED Goal 093 visual chunk cache export contract`
