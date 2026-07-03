# Goal 099 — Offline Geoworld Bundle → WorldSourceGraph → Stream Window → Visual Workspace

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Primary objective

Implement a larger composite geoworld milestone after Goal 098.

Do not add live network fetching, do not read/copy LFZ source code, and do not use real map data. Build a BCL-only deterministic offline geoworld bundle pipeline that proves the future real-world/geodata track can work end-to-end at contract level:

1. metadata-only offline geoworld bundle fixtures;
2. normalization into typed geofeatures;
3. WorldSourceGraph construction;
4. stream-window/boundary-prefetch scheduling;
5. compact visual projection summary;
6. integration into the existing Visual World Stream Preview Workspace;
7. deterministic evidence, negative proof, focused tests and product smoke.

This goal must be materially larger than a narrow one-service proof slice. It should connect several parts of the stack in one outcome while staying inside allowed boundaries.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `583880df GREEN Goal 097 final roadmap rebaseline dream scope productivity`
   - `c6c2093a GREEN Goal 098 geoworld source adapter streaming contract`
4. Confirm LFZ/geoworld docs exist:
   - `docs/context/LFZ_ARCHIVE_ANALYSIS_MANIFEST.md`
   - `docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md`
   - `docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md`
   - `docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md`
   - `docs/proposals/GEOWORLD_INGESTION_FUTURE_GOAL_SEQUENCE.md`
5. Confirm Goal098 artifacts exist and remain `accepted=false`.
6. Confirm Goal098 evidence proves no LFZ code copied, no network implementation, no Runtime/provider/schema changes, normalized feature taxonomy exists.
7. Inspect dirty state. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/GOAL_PRODUCTIVITY_POLICY.md`
- `docs/ROADMAP_FINAL_REBASELINE.md`
- `docs/context/DREAM_SCOPE_REGISTER.md`
- `docs/context/REALISM_GEOWORLD_SIMULATOR_TRACK.md`
- `docs/context/LFZ_ARCHIVE_ANALYSIS_MANIFEST.md`
- `docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md`
- `docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md`
- `docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md`
- `docs/proposals/GEOWORLD_INGESTION_FUTURE_GOAL_SEQUENCE.md`
- `docs/VALIDATION_PIPELINE.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-source-adapter-streaming-contract-report.md`
- `.llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-source-adapter-catalog.json`
- `.llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-normalized-feature-taxonomy.json`
- `.llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-streaming-policy-matrix.json`
- `.llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/geoworld-negative-proof.json`
- existing Visual World Stream Preview Workspace files, because this goal must add a geoworld group to the workspace.

## Allowed files / areas

- `src/LLMGameCreator.Application/Design/OfflineGeoworldWorldSourceGraph/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldWorldSourceGraph/`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldWorldSourceGraphProductSmokeTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`
- `.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/`
- docs quartet, debt register, artifact-scope policy
- `docs/agent-tasks/goal-099-offline-geoworld-worldsourcegraph-streaming/`

## Forbidden files / areas

Do not change LFZ archive or copy LFZ source. Do not change Runtime, Unity, public GamePackage schema, providers, Lua/Scripting, generator-library, `.sln`, `.csproj`, lock files, binary/raster media, generated real geodata dumps, live network fetch code, or external dependencies. No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Offline geoworld bundle fixtures

Create deterministic metadata-only offline bundle fixtures. Minimum bundle: `synthetic_city_radius_offline_bundle`.

Required raw descriptors:
- building footprints;
- road segments;
- water body;
- land use area;
- POI;
- bridge;
- barrier;
- vegetation area;
- terrain hint;
- administrative area.

Use synthetic data only. No real map data and no raw OSM dump.

### 2. Normalize into geofeatures

Implement:

`RawGeoFeatureDescriptor -> NormalizedGeoFeature`

Required output:
- stable feature ids;
- feature kind;
- normalized geometry summary;
- source lineage;
- license/provenance summary;
- gameplaySafe only after normalization;
- raw tags summarized/mapped, not passed directly to gameplay.

### 3. Build WorldSourceGraph

Build deterministic `WorldSourceGraph` over chunks:
- chunk ids / geo tile keys;
- features indexed by chunk;
- cross-chunk references for roads/water/bridges;
- base data immutable;
- deltas separate and empty;
- source provenance attached;
- no raw full-area dump.

### 4. Stream window and boundary prefetch

Apply stream-window request:
- center chunk;
- radius;
- boundary prefetch band;
- required chunks;
- cache state summary;
- missing/scheduled chunks summary;
- no network fetch.

This models the future mechanic: approaching a boundary schedules neighbor chunks.

### 5. Compact visual projection summary

Create compact projection summary:
- feature counts by chunk;
- roads/water/buildings/POI/bridge/barrier/vegetation presence;
- stream window SVG/text overview;
- no raster images;
- no Unity output.

### 6. Workspace integration

Extend Visual World Stream Preview Workspace with a geoworld group showing:
- offline bundle id;
- normalized feature count;
- WorldSourceGraph chunk count;
- stream window chunk count;
- boundary prefetch status;
- feature taxonomy coverage;
- negative proof status;
- compact overview entry.

The workspace must read real Goal099 evidence and not hardcode success.

### 7. Evidence

Create `.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/`.

Recommended artifacts:
- `offline-geoworld-worldsourcegraph-report.md`
- `offline-geoworld-bundle-catalog.json`
- `offline-geoworld-normalized-features.json`
- `offline-geoworld-worldsourcegraph.json`
- `offline-geoworld-stream-window-plan.json`
- `offline-geoworld-boundary-prefetch-proof.json`
- `offline-geoworld-visual-projection-summary.json`
- `offline-geoworld-negative-proof.json`
- `offline-geoworld-workspace-binding-inventory.json`
- `offline-geoworld-source-lineage.json`
- `offline-geoworld-quality-gate-scan.json`
- `overviews/synthetic_city_radius_stream_window.svg`

### 8. Negative proof

Reject:
- raw OSM-like tags directly consumed by gameplay;
- missing license/provenance;
- runtime online fetch attempted;
- public tile scraping;
- full area/planet raw dump;
- absolute paths;
- LFZ copied-code marker;
- unknown feature kind;
- road/water crossing chunk boundary without cross-chunk reference;
- boundary prefetch disabled while runtime travel mode requested;
- prompt text as source of truth;
- real geodata dump marker;
- adult/rating metadata without safe fallback if present.

### 9. Tests

Focused tests:
- valid offline bundle normalizes;
- WorldSourceGraph builds;
- taxonomy includes buildings, roads, water, landuse, POI, bridges, barriers, vegetation;
- stream window and boundary prefetch proof pass;
- workspace catalog includes geoworld group;
- negative proof rejects all cases;
- evidence deterministic.

Product smoke:
- build evidence from repo root;
- read report/catalog/normalized/worldgraph/stream-window/projection/negative proof;
- verify workspace binding inventory;
- verify no network, no LFZ copied code, no binary/raster media, no forbidden areas.

### 10. Docs/state

Update docs quartet and debt register.

Manual gate: `offline_geoworld_worldsourcegraph_streaming_verification required`
Status: `accepted=false`.

Record that this is an offline/synthetic bundle proof. It does not implement real online geodata fetching.

## Validation

Use Goal089 tiered validation:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldWorldSourceGraph
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter OfflineGeoworldWorldSourceGraphProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspace
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualWorldStreamPreviewWorkspaceProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-099-offline-geoworld-worldsourcegraph-streaming" -FocusedFilter "OfflineGeoworldWorldSourceGraph|VisualWorldStreamPreviewWorkspace" -ProductSmokeFilter "OfflineGeoworldWorldSourceGraphProductSmokeTests|VisualWorldStreamPreviewWorkspaceProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-099-offline-geoworld-worldsourcegraph-streaming"
git diff --check
git diff --cached --check
```

Full `check-all.ps1` is not required unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if no forbidden files changed; no LFZ code copied; no network/provider implementation; no Runtime/Unity/schema/project/dependency changes; no raw geodata dump; valid bundle normalizes and builds WorldSourceGraph; stream window and boundary prefetch proof pass; workspace geoworld group is real; negative proof passes; all changed C# files below 1000 lines and touched workspace files below 700 lines; current-goal and spine-fast validation pass; artifact scope passes; final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
- `GREEN Goal 099 offline geoworld WorldSourceGraph streaming`
- `BLOCKED Goal 099 offline geoworld WorldSourceGraph streaming`
- `FAILED Goal 099 offline geoworld WorldSourceGraph streaming`
