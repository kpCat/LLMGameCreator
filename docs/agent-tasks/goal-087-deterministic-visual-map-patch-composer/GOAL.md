# Goal 087 — Deterministic Visual Map Patch Composer & Biome/Water Layout Preview

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Build the next practical step after Goal 086: a BCL-only Application-side deterministic visual map patch composer that consumes Goal 084 visual asset contracts, Goal 085 part-pack rule stack metadata, and Goal 086 text SVG microtile previews, then assembles a small but meaningful map-patch preview with biome transitions, water/coast/river/lake/marsh coverage, road/path connectors, settlement/object anchors, creature/NPC markers, UI/effect/weather overlay metadata, and rating-safe/adult metadata fallback routing.

This goal must not generate raster images, call providers, mutate Runtime/Unity/public GamePackage schema, add external dependencies, or create real adult content. It should produce deterministic text SVG and JSON evidence only.

## Why this matters

The long-term direction is Heroes-3-like fast world/map generation scaled toward 2D/isometric/pseudo-3D/first-person presentation. Goal 086 proved microtiles. Goal 087 must prove that these pieces can be arranged into a deterministic, rule-validated map patch rather than remaining isolated previews.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `1ee359f3 GREEN Goal 083 visual adult layer context integration`
   - `49930836 GREEN Goal 084 visual asset contract rating metadata`
   - `e5b9b12 deepsearch docs`
   - `26d213b7 GREEN Goal 085 visual part pack rule stack`
   - `1034c3c7 GREEN Goal 086 deterministic visual microtile materializer`
4. Confirm Goal 086 artifacts exist, are GREEN, and remain `accepted=false`.
5. Confirm `deterministic_visual_microtile_materializer_verification` is recorded as produced-for-review. If the repo’s current handoff rules require moving to the next gate, record Goal 086 as accepted by handoff before Goal 087 without rewriting Goal 086 artifact `accepted=false` evidence.
6. Inspect current dirty state before edits. Do not stage/revert unrelated user changes.
7. Confirm no P0/P1 source-format or forbidden-zone debt is active.

## Read first

- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md`
- `docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md`
- `docs/deepsearch/03_PSEUDO3D_FIRST_PERSON_FROM_2D_ASSETS.md`
- `docs/deepsearch/05_SETTLEMENTS_CITIES_CARAVANS_LIVING_WORLD_VISUALS.md`
- `.llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md`
- `.llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-rule-stack-report.md`
- `.llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-materializer-report.md`
- `.llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-preview-catalog.json`
- `.llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-materialization-manifest.json`
- `.llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-water-biome-proof.json`
- `.llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-negative-proof.json`
- `.llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/visual-microtile-quality-gate-scan.json`

## Allowed files / areas

- New Application namespace: `src/LLMGameCreator.Application/Design/DeterministicVisualMapPatchComposer/`
- Tests: `tests/LLMGameCreator.Tests/Application/DeterministicVisualMapPatchComposer/`
- Product smoke: `tests/LLMGameCreator.Tests/ProductSmoke/DeterministicVisualMapPatchComposerProductSmokeTests.cs`
- Evidence: `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/`
- Docs/state: `docs/CURRENT_GENERATOR_STATE.md`, `docs/CURRENT_GENERATOR_STATE.json`, `docs/CONTEXT_INDEX.md`, `docs/FULL_GENERATOR_GOAL_QUEUE.md`, `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- Artifact scope: `.devflow/artifact-scope/artifact-scope-policy.json`
- Task pack: `docs/agent-tasks/goal-087-deterministic-visual-map-patch-composer/`

## Forbidden files / areas

Do not change public GamePackage schema, Runtime / Runtime.Abstractions, Unity files including `AlphaRuntimeBootstrap.cs`, Infrastructure provider / LLM / RAG / media provider code, Lua / Scripting, generator-library, `.sln`, `.csproj`, lock files, binary media assets, generated raster assets, real NSFW assets, explicit prompt dumps/provider-output fixtures, or external dependencies.

Do not branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Create BCL-only map-patch composer models

Create models for deterministic visual map patches.

Recommended types:
- `VisualMapPatchDefinition`
- `VisualMapPatchCell`
- `VisualMapPatchLayer`
- `VisualMapPatchTileRef`
- `VisualMapPatchObjectAnchor`
- `VisualMapPatchRoadPath`
- `VisualMapPatchWaterFlow`
- `VisualMapPatchBiomeTransition`
- `VisualMapPatchSettlementAnchor`
- `VisualMapPatchCreatureMarker`
- `VisualMapPatchOverlay`
- `VisualMapPatchValidationResult`
- `VisualMapPatchEvidenceResult`

Required metadata concepts: patch id, dimensions, seed, grid coordinates, terrain biome, water kind, transition kind, source microtile preview id, adjacency/connectors, road/river path connectors, object/settlement anchors, creature/bodyplan/equipment marker, day/night/weather/effect overlay metadata, adult metadata fallback marker only, and source lineage to Goal084/085/086.

### 2. Compose deterministic fixture patches

Create deterministic metadata-only/text-SVG map patches.

Minimum fixtures:

1. `heroes_like_overworld_24x16`
   - grass, forest, mountain, snow, desert, lava/ash;
   - roads/path connectors;
   - settlements, mine, wall/gate, caravan camp;
   - creature markers and treasure/object anchors.

2. `water_coast_river_lake_marsh_24x16`
   - water base;
   - coast transition;
   - river flow with connectors;
   - lake edge;
   - marsh/swamp;
   - bridge/dock anchor.

3. `mixed_biome_settlement_creature_24x16`
   - settlement near roads/water;
   - creature marker with bodyplan/equipment/state metadata;
   - weather/day-night overlay.

The output must remain compact text SVG previews plus JSON manifests, not binary images.

### 3. Create validator

Validate at least:

- dimensions are bounded and deterministic;
- every cell references a known Goal086 microtile preview id;
- terrain/water transitions have compatible neighbors;
- coast requires water and land adjacency;
- river requires deterministic flow connectors;
- bridge/dock requires valid water adjacency;
- roads/path connectors connect to compatible neighbors;
- settlement anchors must be on valid land/passable cells and near path or resource where declared;
- creature markers must reference known safe bodyplan/equipment metadata;
- adult/rating marker must be metadata-only and safe fallback bound;
- no prompt text as source of truth;
- no provider candidate treated as approved;
- no absolute output paths;
- no SVG script/external-resource/base64 content;
- no duplicate patch/cell/object ids;
- source lineage to Goal084/085/086 exists.

### 4. Generate evidence

Create `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/`.

Recommended artifacts:
- `visual-map-patch-composer-report.md`
- `visual-map-patch-catalog.json`
- `visual-map-patch-materialization-manifest.json`
- `visual-map-patch-file-ledger.json`
- `visual-map-patch-water-flow-proof.json`
- `visual-map-patch-reachability-proof.json`
- `visual-map-patch-layering-proof.json`
- `visual-map-patch-negative-proof.json`
- `visual-map-patch-source-lineage.json`
- `visual-map-patch-quality-gate-scan.json`
- `patches/heroes_like_overworld_24x16.svg`
- `patches/water_coast_river_lake_marsh_24x16.svg`
- `patches/mixed_biome_settlement_creature_24x16.svg`

The SVG previews should be readable and deterministic, but modest. Do not try to produce final game art.

### 5. Tests

Add focused tests proving:
- all three patches materialize;
- output is deterministic across reruns;
- all referenced microtiles exist in Goal086 catalog;
- water/coast/river/lake/marsh/bridge coverage is present;
- road/path/reachability proof is present;
- settlement/object/creature overlays are represented;
- invalid matrix rejects unsafe/fake cases.

Add product smoke proving evidence builds from repo root, report/catalog/manifest/ledger/proofs can be read, at least 3 SVG patch previews exist, negative proof rejects expected invalid scenarios, and no binary/raster media are created.

### 6. Docs/state

Update docs quartet and debt register.

Goal 087 manual gate:
`deterministic_visual_map_patch_composer_verification required`

Do not mark future procedural renderer / Unity consumption / provider work complete.

## Quality gate

GREEN only if no forbidden files changed; no Unity/Runtime/provider/schema/project/dependency changes; no binary/raster media or prompt dumps; no explicit adult content; map patch previews are deterministic text SVG; all patch references trace to Goal086 microtiles and Goal084/085 lineage; water/biome/path/reachability proofs pass; negative proof rejects unsafe/fake cases; evidence is deterministic; build/tests/check-all/artifact-scope pass; and source formatting guard remains clean.

## Validation commands

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualMapPatchComposer
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualMapPatchComposerProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-087-deterministic-visual-map-patch-composer"
git diff --check
git diff --cached --check
```

Also scan changed files for mojibake, absolute local paths in evidence, timestamps/heavy logs, prompt dumps, and binary/media additions.

## Stop / block conditions

Return BLOCKED if map patch composition requires GamePackage schema changes, Runtime/Unity/provider/Lua/generator-library changes, cannot prove water/biome/path adjacency without broad scope, or artifact scope cannot be satisfied without forbidden areas.

Return FAILED if build/tests regress due to this goal and cannot be repaired inside allowed files.

## Final report format

Report final status, latest commit before/after, push status, preflight summary, files changed, patch fixtures added, water/biome/path proof summary, negative proof summary, evidence artifacts, validation results, artifact scope result, evidence hygiene, remaining P2/P3 debt, final git status, and git commands used and why.

## Mandatory commit/push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:
- `GREEN Goal 087 deterministic visual map patch composer`
- `BLOCKED Goal 087 deterministic visual map patch composer`
- `FAILED Goal 087 deterministic visual map patch composer`
