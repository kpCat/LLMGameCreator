# Goal 100 — Offline Geoworld Visual Cache Unity Handoff

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Objective

Deliver a larger composite milestone after Goal 099.

Goal 099 proved:

`offline bundle -> normalized features -> WorldSourceGraph -> stream window -> visual workspace`.

Goal 100 must connect that geoworld pipeline to the existing visual cache / Unity handoff stack:

1. consume real Goal 099 WorldSourceGraph / stream-window / projection artifacts;
2. project normalized geoworld features into visual chunk cache records;
3. create compact geoworld visual cache export packages;
4. mirror compact payload into Unity StreamingAssets;
5. add a small standalone Unity geoworld handoff probe;
6. integrate the geoworld visual cache and Unity handoff into the existing Visual World Stream Preview Workspace;
7. produce deterministic evidence, negative proof, focused tests and product smoke.

This must be a composite outcome, not a narrow one-service proof.

## Non-goals

Do not add live network fetching.
Do not read/copy LFZ source code.
Do not use real map data.
Do not implement Runtime consumption.
Do not modify `AlphaRuntimeBootstrap.cs`.
Do not change public GamePackage schema.
Do not add external dependencies.

## Preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `c6c2093a GREEN Goal 098 geoworld source adapter streaming contract`
   - `48322dae GREEN Goal 099 offline geoworld WorldSourceGraph streaming`
4. Confirm Goal 099 artifacts exist and remain `accepted=false`.
5. Confirm Goal 099 report proves synthetic offline bundle only, normalized features, WorldSourceGraph chunks, stream-window/boundary-prefetch proof, workspace binding passed, no LFZ code copied and no network/provider implementation.
6. Confirm existing visual cache/handoff artifacts exist from Goals 093-096.
7. Record `AlphaRuntimeBootstrap.cs` hash/line count before work and do not modify it.
8. Inspect dirty state. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/GOAL_PRODUCTIVITY_POLICY.md`
- `docs/VALIDATION_PIPELINE.md`
- `docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md`
- `docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md`
- `docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md`
- `docs/proposals/GEOWORLD_INGESTION_FUTURE_GOAL_SEQUENCE.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/offline-geoworld-worldsourcegraph-report.md`
- `.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/offline-geoworld-normalized-features.json`
- `.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/offline-geoworld-worldsourcegraph.json`
- `.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/offline-geoworld-stream-window-plan.json`
- `.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/offline-geoworld-visual-projection-summary.json`
- `.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/offline-geoworld-negative-proof.json`
- `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/visual-chunk-cache-export-report.md`
- `.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/visual-chunk-cache-unity-handoff-report.md`
- `.llmgc/procedural/goal-096-unity-handoff-inspector-probe-readiness/unity-handoff-inspector-report.md`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs` as read-only baseline.
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`

## Allowed files

- `src/LLMGameCreator.Application/Design/OfflineGeoworldVisualCacheUnityHandoff/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldVisualCacheUnityHandoff/`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldVisualCacheUnityHandoffProductSmokeTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldHandoffProbe.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal100/`
- `.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/`
- docs quartet, debt register, artifact-scope policy
- this task pack

## Forbidden files

Do not change LFZ archive/source, Runtime, public schema, providers, Lua, generator-library, `.sln`, `.csproj`, lock files, `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/asmdefs/settings/packages/build settings, binary/raster media, real geodata dumps, live network fetch code, external dependencies, prompt dumps. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Required behavior

### 1. Geoworld visual cache export

Create a BCL-only Application service that reads Goal 099 artifacts and maps normalized geoworld features to compact visual cache records.

Required mapping:
- building footprint;
- road segment;
- water body;
- land use;
- POI;
- bridge;
- barrier;
- vegetation;
- terrain hint;
- administrative hint.

Each record must include source feature id, feature kind, source chunk, visual chunk key, visual layer id, projection status, cache record hash, Goal098/099 lineage, safe/rating metadata status and no raw geodata.

### 2. Cache packages

Create at least three metadata-only packages:

- `geoworld_editor_review_package`
- `geoworld_unity_handoff_package`
- `geoworld_stream_window_runtime_preview_package`

No raw full-area dumps.

### 3. Unity StreamingAssets payload

Write compact payload under:

`unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal100/`

Required files:
- `offline-geoworld-unity-handoff-manifest.json`
- `offline-geoworld-package-index.json`
- `offline-geoworld-feature-chunk-ledger.json`
- `offline-geoworld-stream-window-index.json`
- `offline-geoworld-runtime-readme.json`

Use relative paths only.

### 4. Unity probe

Add `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldHandoffProbe.cs`.

It must read from `Application.streamingAssetsPath`, validate expected files/counts/markers, expose simple Inspector-readable status fields, make no provider/LLM/network calls, use no external packages, and not hardcode GREEN.

### 5. Simulated Unity read proof

.NET tests must read the mirrored payload and verify package/feature/chunk/window counts, manifest hashes, no raw full-world dump, no absolute paths, no binary/raster media and no provider/network markers.

### 6. Workspace integration

Extend Visual World Stream Preview Workspace with `offline_geoworld_handoff` group showing package count, feature count by kind, chunk count, stream window summary, Unity payload count, simulated read proof, negative proof and AlphaRuntimeBootstrap unchanged status.

### 7. Evidence

Create `.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/`.

Required artifacts:
- `offline-geoworld-visual-cache-unity-handoff-report.md`
- `offline-geoworld-visual-cache-catalog.json`
- `offline-geoworld-visual-cache-package-index.json`
- `offline-geoworld-feature-chunk-ledger.json`
- `offline-geoworld-unity-handoff-manifest.json`
- `offline-geoworld-unity-streamingassets-ledger.json`
- `offline-geoworld-unity-probe-source-inventory.json`
- `offline-geoworld-unity-simulated-read-proof.json`
- `offline-geoworld-negative-proof.json`
- `offline-geoworld-workspace-binding-inventory.json`
- `offline-geoworld-source-lineage.json`
- `offline-geoworld-quality-gate-scan.json`

### 8. Negative proof

Reject missing Goal099 world graph, unmapped feature kind, raw geodata leak, missing license/provenance, absolute path, live network fetch, public tile scraping marker, LFZ copied-code marker, raw full-area/planet dump marker, fake Unity success without file read, missing StreamingAssets manifest, tampered manifest hash, Unity probe provider/network marker and adult/rating metadata without safe fallback if present.

## Tests

Focused tests:
- service builds from repo root;
- visual cache packages created;
- all feature kinds mapped;
- Unity payload created;
- simulated Unity read proof passes;
- workspace `offline_geoworld_handoff` group exists;
- negative proof rejects expected scenarios;
- evidence deterministic;
- all changed C# files below 1000 lines and new files below 700 lines.

Product smoke:
- read `.llmgc` evidence and Unity StreamingAssets payload;
- verify package/feature/chunk/window counts;
- verify Unity probe inventory;
- verify no forbidden areas, no binary/raster media and no raw geodata dump.

## Docs/state

Update docs quartet and debt register.

Manual gate:
`offline_geoworld_visual_cache_unity_handoff_verification required`

Status: `accepted=false`.

Record that this remains offline/synthetic and does not implement real online geodata fetching or live Unity gameplay rendering.

## Validation

Use Goal089 tiered validation:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldVisualCacheUnityHandoff
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldVisualCacheUnityHandoffProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspace
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspaceProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-100-offline-geoworld-visual-cache-unity-handoff" -FocusedFilter "OfflineGeoworldVisualCacheUnityHandoff|VisualWorldStreamPreviewWorkspace" -ProductSmokeFilter "OfflineGeoworldVisualCacheUnityHandoffProductSmokeTests|VisualWorldStreamPreviewWorkspaceProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-100-offline-geoworld-visual-cache-unity-handoff"
git diff --check
git diff --cached --check
```

## Quality gate

GREEN only if no forbidden files changed; no LFZ code copied; no network/provider implementation; no Runtime/public schema/project/dependency changes; AlphaRuntimeBootstrap unchanged by hash; Unity probe and StreamingAssets payload exist; no raw geodata dump; all feature kinds map into visual cache records; simulated Unity read proof passes; workspace integration is real; negative proof passes; source-health limits pass; current-goal and spine-fast pass; artifact scope passes; worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
- `GREEN Goal 100 offline geoworld visual cache Unity handoff`
- `BLOCKED Goal 100 offline geoworld visual cache Unity handoff`
- `FAILED Goal 100 offline geoworld visual cache Unity handoff`
