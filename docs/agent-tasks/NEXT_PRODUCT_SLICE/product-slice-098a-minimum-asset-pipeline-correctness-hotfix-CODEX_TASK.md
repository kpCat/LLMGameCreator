# Codex Task - Product Slice 098A Minimum Asset Pipeline Correctness Hotfix

## Purpose

This is a bounded correctness hotfix for Goal 011 after external artifact review.

Keep the existing final gate:

```text
minimum_asset_pipeline_artifact_verification
```

Do not mark it passed. Do not create or start S099, Goal 012, Unity/export work, provider/media generation work or post-Goal-011 planning.

## Starting Review Findings To Fix

The current Goal 011 artifact must not be accepted yet. The pushed implementation proves the valid fixture/fallback path, but several required invalid/fake/leak scenarios are accepted by manually created diagnostics instead of actual mutations flowing through the same validation and binding logic.

Concrete gaps found in pushed code:

1. `tampered_package_content_hash`, `cross_pack_asset_leakage`, `duplicate_slot_ids`, `over_budget_request` and `unavailable_default_resolver` in `BuildInvalidMatrix` are partly or fully synthetic diagnostics rather than causal validation results.
2. `ValidateAssets` does not actually compare the supplied package/content hash to the package/content used for binding. The current package-hash check is effectively a no-op when `package.Manifest.PackageId` is present.
3. Cross-pack leakage is not structurally validated. A resolved asset from another source pack/path can be represented as long as the content id exists and the file path remains under the artifact root.
4. Package/content binding audit mostly counts `AssetCatalog.LinkedEntityIds`; it does not prove category-specific attachment for the fields/seams the report claims, such as tile/entity/item/icon/dialogue/interaction metadata where those seams exist.
5. Product smoke asserts report booleans and file existence, but does not prove selected attached asset refs can be resolved from the package/content structures.

Fix these defects without redesigning architecture.

## Read First

Read in this order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/GOAL_011_MINIMUM_ASSET_PIPELINE.md`
6. current `src/LLMGameCreator.Application/Design/Assets/MinimumAssetPipelineAcceptanceService.cs`
7. current Goal 011 tests/smoke/artifacts directly touched by this hotfix
8. existing GamePackage asset/content definitions needed for honest binding audit

Do not read historical apply packs or old task prompts unless a concrete blocker requires it.

## Allowed Files

Allowed:

- `src/LLMGameCreator.Application/Design/Assets/MinimumAssetPipelineAcceptanceService.cs`
- `tests/LLMGameCreator.Tests/Application/Assets/MinimumAssetPipelineAcceptanceTests.cs`
- `tests/LLMGameCreator.Tests/Application/Assets/MinimumAssetPipelineTestResolver.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/MinimumAssetPipelineSmokeTests.cs`
- `samples/minimum-asset-pipeline/*.json` only if a fixture/source-pack mutation is needed for the hotfix
- `samples/minimum-asset-pipeline/fixtures/*` only if a tiny deterministic fixture mutation is needed for the hotfix
- `.llmgc/procedural/minimum-asset-pipeline/minimum-asset-pipeline-report.json`
- `.llmgc/procedural/minimum-asset-pipeline/minimum-asset-pipeline-report.md`
- `.llmgc/procedural/minimum-asset-pipeline/minimum-asset-pipeline-verification.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md` only if routing text must mention S098A

Conditionally allowed only if a focused failing test proves a narrow existing defect:

- the smallest existing GamePackage/Application asset metadata seam required to honestly audit an already-existing field;
- its focused regression test.

Forbidden:

- `.sln` or `.csproj` edits;
- public GamePackage/runtime schema redesign;
- WinForms/UI, Unity/export, external asset/media generation, provider, RAG, LLM or Lua execution;
- `generator-library`;
- broad refactors outside Goal 011 minimum asset pipeline acceptance;
- S099, Goal 012 or next-goal task files.

Do not use git commands.

## Required Fixes

### 1. Remove Synthetic Invalid Acceptance

Update `BuildInvalidMatrix` so every required invalid/fake/leak scenario is produced by an actual mutation that runs through shared validation/binding/resolution code.

At minimum, make these causal:

- `duplicate_slot_ids`: mutate expanded requests or resolved assets and run a shared request/manifest validation path that detects the duplicate.
- `tampered_package_content_hash`: mutate the package/content hash or bound package after manifest creation and run a shared hash-integrity validation path that detects the mismatch.
- `over_budget_request`: lower source/category budgets and use actual `ExpandRequests` diagnostics, not a hand-authored diagnostic.
- `cross_pack_asset_leakage`: inject a resolved asset/path/source from one valid run into another pack/source context and reject it through shared validation.
- `unavailable_default_resolver`: actually invoke the default unavailable resolver/service path or a shared resolver evidence path, not a hand-authored diagnostic.

Keep existing causal cases for source kind, media type, fixture missing/corrupt, unsafe paths, executable payloads, unresolved content id, mismatched file hash and expectation-only copied report evidence.

The expectation-only invalid fixture must still fail when its mutation is removed.

### 2. Hash And Manifest Integrity

Strengthen validation so acceptance proves:

- source pack hash in the manifest matches the loaded source pack bytes;
- package hash in the manifest matches the pre-asset package used for binding;
- generated content/package-content hash in the manifest matches the content pack used for requests;
- package hash with assets changes when asset bindings change and is stable on replay;
- tampered package/content hash fails via the same validation used by valid runs.

Do not rely on a no-op comparison or report-only strings.

### 3. Cross-Pack Isolation And Leakage

Add structural checks that reject asset leakage across generated packs/source packs.

Validation must verify:

- every resolved asset source id exists in the current source pack;
- every resolved asset source kind/category/media type matches the current source declaration;
- every resolved asset relative path is under the expected artifact subfolder for the current source pack or an explicitly declared safe import/fallback location;
- every resolved asset content id belongs to the current generated/package content graph;
- a concrete injected foreign asset/path/source/content mismatch is rejected causally.

Run valid packs sequentially through the same resolver/service path and prove no prior pack assets satisfy a later pack.

### 4. Category-Specific Binding Audit

Package binding audit must prove more than `AssetCatalog` row count.

For every resolved asset, verify the strongest existing seam available without public schema changes:

- `AssetCatalog.Assets` contains exact asset id, media type/category, relative path and linked content id.
- `tile_region_graphic`: tile prototype asset id is set when the content id is a tile prototype; map/region ids are at least linked through `AssetCatalog` and any existing metadata seam if available.
- `npc_portrait`: entity/entity-prototype asset id is set when the content id maps to an entity; generated NPC ids are linked through `AssetCatalog` and any existing metadata seam if available.
- `item_icon_ui_graphic`: item `IconAssetId` is exact when the content id is an item.
- `sound_effect`: dialogue/interaction metadata is exact when the content id is a dialogue or interaction.
- `music_ambience`: map/interaction/ambience content ids are linked through `AssetCatalog` and any existing metadata seam if available.

Report category-specific binding counts and failures. Do not claim fields/seams are bound unless the audit verifies them.

### 5. Product Smoke And Tests

Strengthen focused tests and product smoke to assert:

- invalid matrix scenarios above have diagnostics generated by the actual mutation path;
- default unavailable resolver fails through a real service path;
- tampered package/content hash fails without synthetic diagnostics;
- cross-pack injected leakage fails without synthetic diagnostics;
- category-specific bindings can be resolved from the package/content structures for selected assets;
- all valid resolved files still exist, hashes/byte counts/media headers match, and import/fallback counts remain non-zero.

## Artifacts And State

Regenerate exactly the existing Goal 011 artifact files:

```text
.llmgc/procedural/minimum-asset-pipeline/minimum-asset-pipeline-report.json
.llmgc/procedural/minimum-asset-pipeline/minimum-asset-pipeline-report.md
.llmgc/procedural/minimum-asset-pipeline/minimum-asset-pipeline-verification.md
```

The report must include:

- completed slices including `S098A`;
- unchanged manual gate `minimum_asset_pipeline_artifact_verification`;
- valid counts for the same required asset categories;
- import/fallback distribution;
- source/package/content/manifest hash-integrity evidence;
- category-specific binding audit evidence;
- causal invalid/fake/leak diagnostics, including the fixed scenarios;
- all external execution flags false;
- no absolute paths, timestamps, GUIDs or machine-specific content in deterministic artifacts.

Update current-state docs to record S098A as a correctness hotfix under Goal 011, but leave:

```text
minimum_asset_pipeline_artifact_verification: required
```

Do not recommend or create Goal 012.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~MinimumAssetPipeline|FullyQualifiedName~ContentGenerationScale|FullyQualifiedName~RulePackCombatFactionSocialWorkTheft|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario minimum-asset-pipeline
.\.devflow\scripts\check-all.ps1
```

Also scan changed/generated files for:

- mojibake markers;
- absolute local paths;
- nondeterministic timestamps or GUIDs in deterministic artifacts;
- `S099|Goal 012|goal_012` outside this prohibition text.

## Stop Conditions

Stop with a blocker report instead of weakening acceptance if:

- causal invalid matrix fixes require public GamePackage/runtime schema redesign;
- category-specific binding cannot be honestly proven through existing fields/metadata/AssetCatalog seams;
- hash-integrity validation would require changing content generation public contracts;
- real file/hash validation would require external provider/media execution;
- `.sln` or `.csproj` edits are required;
- full verification exposes an unrelated pre-existing failure.

## Final Report

Report:

- root cause fixed for synthetic invalid/fake/leak acceptance;
- changed files;
- hash-integrity rules;
- category-specific binding audit results;
- invalid/fake/leak diagnostics and which mutation path produced them;
- asset category counts and import/fallback distribution;
- artifact folder and deterministic hash;
- focused/smoke/full verification totals;
- confirmation that the gate remains `minimum_asset_pipeline_artifact_verification` required;
- confirmation that S099/Goal 012, public schemas, UI, Unity/export, Lua/provider/media/LLM/RAG, generator-library and project files were untouched.
