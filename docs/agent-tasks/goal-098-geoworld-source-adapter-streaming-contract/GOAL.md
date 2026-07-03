# Goal 098 — Geoworld Source Adapter & Runtime Streaming Contract Foundation

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Create the first LLMGameCreator-native geoworld source adapter and streaming contract foundation, based only on LLMGameCreator documentation extracted from the LFZ pattern study.

Do not read or require the LFZ archive. Do not copy code from LFZ. Do not implement live network fetching. Do not scrape map tiles. This is a BCL-only Application-side contract, validation and evidence goal.

The goal should be a larger composite outcome than recent narrow proof slices:
- source adapter contracts;
- geotile/profile/chunk address contracts;
- cache/provenance/license policy contracts;
- normalized feature taxonomy;
- streaming window request/queue contracts;
- negative proof matrix;
- evidence and product smoke;
- docs/state/debt routing.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes `583880df GREEN Goal 097 final roadmap rebaseline dream scope productivity`.
4. Confirm these docs exist:
   - `docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md`
   - `docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md`
   - `docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md`
   - `docs/proposals/GEOWORLD_INGESTION_FUTURE_GOAL_SEQUENCE.md`
5. Confirm Goal097 artifacts exist and remain `accepted=false`.
6. Inspect dirty state before edits. Do not stage/revert unrelated user work.

## Read first

- `AGENTS.md`
- `docs/ROADMAP_FINAL_REBASELINE.md`
- `docs/context/DREAM_SCOPE_REGISTER.md`
- `docs/context/REALISM_GEOWORLD_SIMULATOR_TRACK.md`
- `docs/RELEASE_RISK_REGISTER.md`
- `docs/MILESTONE_GATES.md`
- `docs/GOAL_PRODUCTIVITY_POLICY.md`
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

## Allowed files / areas

- `src/LLMGameCreator.Application/Design/GeoworldSourceAdapterStreamingContract/`
- `tests/LLMGameCreator.Tests/Application/GeoworldSourceAdapterStreamingContract/`
- `tests/LLMGameCreator.Tests/ProductSmoke/GeoworldSourceAdapterStreamingContractProductSmokeTests.cs`
- `.llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/agent-tasks/goal-098-geoworld-source-adapter-streaming-contract/`

## Forbidden files / areas

Do not change:
- LFZ archive or any copied LFZ source.
- Runtime / Runtime.Abstractions.
- Unity files.
- public GamePackage schema.
- provider / LLM / RAG / media provider code.
- Lua / Scripting.
- generator-library.
- `.sln`, `.csproj`, lock files.
- binary/raster media.
- generated real geodata dumps.
- live network fetch code.
- external dependencies.

No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Create BCL-only geoworld contracts

Recommended types:
- `GeoCoordinate`
- `GeoBounds`
- `GeoTileKey`
- `GeoTileGridRequest`
- `GeoStreamWindowRequest`
- `GeoSourceAdapterSpec`
- `GeoSourceAdapterKind`
- `GeoSourceLicensePolicy`
- `GeoSourceProvenance`
- `GeoTileCachePolicy`
- `GeoFetchPlan`
- `GeoFetchResult`
- `GeoFeatureRawDescriptor`
- `GeoFeatureNormalized`
- `GeoFeatureKind`
- `WorldSourceGraph`
- `WorldSourceGraphChunk`
- `GeoStreamingPolicy`
- `GeoworldContractValidationResult`
- `GeoworldContractEvidenceResult`

### 2. Add contract fixtures

Minimum fixtures:
- `offline_osm_extract_city_radius`
- `user_provided_map_bundle`
- `licensed_vector_tile_adapter_spec`
- `runtime_online_optional_policy_blocked_by_default`
- `ocr_georeference_fallback_future_only`
- `self_generated_realism_world_source`
- `earth_radius_stream_window_boundary_prefetch`

Fixtures are metadata-only. No network. No raw geodata.

### 3. Validator rules

Reject:
- scraping public tile servers;
- bulk/preseed/offline public tile archive mode;
- runtime online mode enabled without explicit policy;
- missing attribution/license/provenance;
- raw OSM tags consumed directly by gameplay;
- absolute paths;
- missing cache policy;
- missing stream radius/boundary prefetch policy;
- full planet raw dump;
- provider/API hardcoded into core;
- prompt text as source of truth;
- copying LFZ source code marker;
- OCR fallback treated as primary path;
- unsupported adult/rating metadata without safe fallback if present.

### 4. Evidence

Create `.llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/` with:
- `geoworld-source-adapter-streaming-contract-report.md`
- `geoworld-source-adapter-catalog.json`
- `geoworld-normalized-feature-taxonomy.json`
- `geoworld-streaming-policy-matrix.json`
- `geoworld-negative-proof.json`
- `geoworld-lfz-pattern-lineage.json`
- `geoworld-quality-gate-scan.json`

Evidence must prove:
- LFZ docs consumed as lineage;
- no LFZ code copied;
- no network/provider implementation;
- no Runtime/Unity/schema changes;
- future runtime streaming is represented as contracts only;
- negative proof passed.

### 5. Tests

Focused tests:
- valid fixtures pass;
- negative matrix rejects all cases;
- normalized feature taxonomy includes buildings, roads, water, landuse, POI, barriers, bridges, vegetation;
- runtime boundary-prefetch contract exists;
- no live adapter performs network I/O.

Product smoke:
- build evidence from repo root;
- read report/catalog/taxonomy/policy/negative proof;
- assert offline/local/licensed/optional-online/future-OCR/self-generated fixtures exist;
- assert no binary media or geodata dumps.

### 6. Docs/state

Update docs quartet and debt register.

Manual gate:
`geoworld_source_adapter_streaming_contract_verification required`

Status:
`accepted=false`.

## Validation

Use Goal089 tiered validation:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter GeoworldSourceAdapterStreamingContract
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter GeoworldSourceAdapterStreamingContractProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-098-geoworld-source-adapter-streaming-contract" -FocusedFilter "GeoworldSourceAdapterStreamingContract" -ProductSmokeFilter "GeoworldSourceAdapterStreamingContractProductSmokeTests"
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-098-geoworld-source-adapter-streaming-contract"
git diff --check
git diff --cached --check
```

Full `check-all.ps1` is not required unless current-goal/spine-fast indicates shared/core risk.

## Quality gate

GREEN only if:
- no forbidden files changed;
- no network/provider implementation;
- no LFZ source copied;
- no Runtime/Unity/schema/project/dependency changes;
- no raw geodata dumps;
- valid fixtures pass;
- negative proof rejects unsafe cases;
- evidence is deterministic;
- current-goal and spine-fast validation pass;
- artifact scope passes;
- final worktree clean.

## Commit/push

Always commit and push to `origin/main`.

Commit message:
- `GREEN Goal 098 geoworld source adapter streaming contract`
- `BLOCKED Goal 098 geoworld source adapter streaming contract`
- `FAILED Goal 098 geoworld source adapter streaming contract`
