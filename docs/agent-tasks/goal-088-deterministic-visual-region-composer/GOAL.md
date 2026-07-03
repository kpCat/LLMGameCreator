# Goal 088 — Deterministic Visual Region Composer & 144x144 Surface/Underground Preview

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Build the next practical step after Goal 087: a BCL-only Application-side deterministic visual region composer that consumes Goal 084/085/086/087 visual metadata and stitches the Goal 087 24x16 map patches into a Heroes-3-scale logical region: 144x144 surface + 144x144 underground.

This goal should prove fast, chunked, deterministic region assembly without generating a giant image dump and without listing all 41,472 cells as heavy raw artifacts. Use patch placements, chunk index, RLE/summary manifests, compact text SVG overviews, and proofs.

Do not generate raster images. Do not call LLM/media providers. Do not mutate Runtime, Unity, public GamePackage schema, providers, Lua, generator-library, project files or dependencies.

## Why this matters

The long-term target is a generator that can create large Heroes-3-like maps quickly while preserving a path toward 2D/isometric/pseudo-3D/first-person presentation. Goal 086 proved microtiles. Goal 087 proved small 24x16 map patches. Goal 088 must prove a 144x144x2 region planner/composer that scales by reusing patches/rules/seeds instead of storing 10,000+ unique visual pieces.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `49930836 GREEN Goal 084 visual asset contract rating metadata`
   - `26d213b7 GREEN Goal 085 visual part pack rule stack`
   - `1034c3c7 GREEN Goal 086 deterministic visual microtile materializer`
   - `d8cd8059 GREEN Goal 087 deterministic visual map patch composer`
4. Confirm Goal 087 artifacts exist and are GREEN / accepted=false.
5. Confirm Goal 087 is recorded as produced-for-review. If current project state requires a single gate transition, record Goal 087 as accepted by handoff before Goal 088 while preserving Goal 087 artifact `accepted=false` evidence.
6. Inspect current dirty state before edits. Do not stage/revert unrelated user changes.
7. Confirm no P0/P1 source-format or forbidden-zone debt is active.
8. Confirm this goal can be implemented without modifying Goal 087 files; create a new namespace instead of growing `DeterministicVisualMapPatchComposerEvidenceService.cs`.

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
- `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-composer-report.md`
- `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-catalog.json`
- `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-materialization-manifest.json`
- `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-water-flow-proof.json`
- `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-reachability-proof.json`
- `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-negative-proof.json`
- `.llmgc/procedural/goal-087-deterministic-visual-map-patch-composer/visual-map-patch-quality-gate-scan.json`

## Allowed files / areas

- New Application namespace:
  - `src/LLMGameCreator.Application/Design/DeterministicVisualRegionComposer/`
- Tests:
  - `tests/LLMGameCreator.Tests/Application/DeterministicVisualRegionComposer/`
  - `tests/LLMGameCreator.Tests/ProductSmoke/DeterministicVisualRegionComposerProductSmokeTests.cs`
- Evidence:
  - `.llmgc/procedural/goal-088-deterministic-visual-region-composer/`
- Docs/state:
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
  - `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- Artifact scope:
  - `.devflow/artifact-scope/artifact-scope-policy.json`
- Task pack:
  - `docs/agent-tasks/goal-088-deterministic-visual-region-composer/`

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
- binary/raster media assets;
- generated PNG/JPG/WebP;
- real NSFW assets;
- explicit prompt dumps or provider-output fixtures;
- external dependencies;
- existing Goal 087 Application namespace unless absolutely necessary for a compile-only reference issue; prefer no edits there.

Do not branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Create BCL-only region composer models

Create models for a deterministic 144x144x2 visual region.

Recommended types:

- `VisualRegionDefinition`
- `VisualRegionLayer`
- `VisualRegionPatchPlacement`
- `VisualRegionChunk`
- `VisualRegionBiomeBand`
- `VisualRegionWaterNetwork`
- `VisualRegionRoadNetwork`
- `VisualRegionSettlementPlacement`
- `VisualRegionGateTransition`
- `VisualRegionObjectPlacement`
- `VisualRegionCreaturePlacement`
- `VisualRegionOverlay`
- `VisualRegionValidationResult`
- `VisualRegionEvidenceResult`

Required metadata concepts:

- region id;
- seed;
- dimensions: 144x144;
- layer ids: `surface`, `underground`;
- patch size: 24x16;
- patch grid: 6 columns x 9 rows per layer;
- 54 patch placements per layer;
- 108 patch placements total;
- derived logical cell count: 144 * 144 * 2 = 41,472;
- source Goal087 patch ids;
- patch transforms: rotate/mirror/repalette metadata only, no raster generation;
- biome distribution;
- water network;
- road network;
- settlement/castle/garrison/caravan/object/creature placements;
- surface-underground transitions;
- weather/day-night/effect overlay metadata;
- adult/rating route metadata only, safe fallback bound;
- source lineage to Goal084/085/086/087.

### 2. Compose deterministic fixture region

Create one deterministic region fixture:

`heroes_scale_surface_underground_144x144`

It must include:

Surface:
- mixed grass/forest/mountain/snow/desert/lava/ash bands;
- sea/lake/coast/river/marsh/water crossings;
- castle/settlement/garrison/mine/caravan/object anchors;
- road network with reachable important anchors;
- creature markers.

Underground:
- cave/rock/lava/underground water/mushroom/ruin-like regions via metadata;
- cave roads/tunnels;
- surface-to-underground gates;
- water/lava boundary metadata;
- settlement/outpost/object/creature anchors.

Do not create 41,472 explicit per-cell JSON rows unless compacted or summarized. Prefer:
- patch placement records;
- RLE bands;
- summary metrics;
- compact proof rows;
- overview SVG at patch/chunk scale.

### 3. Generate deterministic text SVG overviews

Generate compact text SVG artifacts:

- `region-overview-surface.svg`
- `region-overview-underground.svg`
- `region-overview-combined.svg`

These are overview SVGs, not full final art. They may represent each 24x16 patch placement as a block or mini-grid. They must not contain scripts, external resources, base64, or binary embeds.

### 4. Validate region composition

Validation must reject at least:

- wrong dimensions;
- wrong layer count;
- wrong patch grid;
- missing or unknown Goal087 patch id;
- patch placement outside bounds;
- duplicate patch coordinates in same layer;
- missing water network when water is declared;
- river/coast/water connector mismatch across patch boundaries;
- road network not connected to declared settlement/castle/garrison/caravan anchors;
- surface-underground transition without paired gate;
- settlement/castle placed on invalid water/impassable placement;
- creature placement missing bodyplan/equipment metadata;
- adult/rating marker without safe fallback;
- prompt text as source of truth;
- provider candidate treated as approved;
- absolute paths;
- unsafe SVG script/external/base64 content;
- heavy artifact mode with explicit 41,472 raw cells.

### 5. Generate evidence

Create `.llmgc/procedural/goal-088-deterministic-visual-region-composer/`.

Recommended artifacts:

- `visual-region-composer-report.md`
- `visual-region-definition.json`
- `visual-region-patch-placement-index.json`
- `visual-region-chunk-index.json`
- `visual-region-biome-distribution-proof.json`
- `visual-region-water-network-proof.json`
- `visual-region-road-reachability-proof.json`
- `visual-region-layer-transition-proof.json`
- `visual-region-object-placement-proof.json`
- `visual-region-negative-proof.json`
- `visual-region-source-lineage.json`
- `visual-region-quality-gate-scan.json`
- `region-overview-surface.svg`
- `region-overview-underground.svg`
- `region-overview-combined.svg`

### 6. Tests

Add focused tests proving:

- the 144x144 surface and 144x144 underground dimensions are present;
- patch grid is 6x9 per layer;
- patch placements total 108;
- derived logical cell count is 41,472;
- no heavy explicit raw cell dump is emitted;
- all placements reference known Goal087 patch ids;
- water/coast/river/lake/marsh/bridge/dock coverage persists at region level;
- road reachability connects settlement/castle/garrison/caravan/object anchors;
- surface-underground gate pairs exist;
- overview SVG files are deterministic and safe;
- negative proof rejects all expected invalid scenarios.

Add product smoke proving:

- evidence builds from repo root;
- report/definition/indexes/proofs/overviews can be read;
- surface and underground overview SVGs exist;
- quality gate is GREEN;
- no binary/raster media was created;
- no Runtime/Unity/provider/schema files changed.

### 7. Docs/state

Update docs quartet and debt register.

Goal 088 manual gate:

`deterministic_visual_region_composer_verification required`

Do not mark future final renderer / atlas generation / Unity consumption / provider work complete.

## Quality gate

GREEN only if:

- no forbidden files changed;
- no Unity/Runtime/provider/schema/project/dependency changes;
- no binary/raster media added;
- no prompt dumps;
- no explicit adult content;
- region is represented by compact patch placement / chunk/index artifacts, not heavy raw cell dumps;
- 144x144x2 logical dimensions are proven;
- Goal087 patch references are proven;
- water/road/reachability/layer transition proofs pass;
- negative proof rejects unsafe/fake cases;
- evidence is deterministic;
- build/tests/check-all/artifact-scope pass;
- source formatting guard remains clean.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualRegionComposer
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualRegionComposerProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-088-deterministic-visual-region-composer"
git diff --check
git diff --cached --check
```

Also scan changed files for:
- mojibake;
- absolute local paths in evidence;
- timestamps/heavy logs;
- prompt dumps;
- binary/media additions;
- raw huge cell dump artifacts.

## Stop / block conditions

Return BLOCKED if:

- region composition requires GamePackage schema changes;
- region composition requires Runtime/Unity/provider/Lua/generator-library changes;
- evidence cannot prove 144x144x2 region composition without heavy raw cell artifacts;
- artifact scope cannot be satisfied without forbidden areas.

Return FAILED if:

- build/tests regress due to this goal and cannot be repaired inside allowed files.

## Final report format

Report:

- Final status.
- Latest commit before/after.
- Push status.
- Preflight summary.
- Files changed.
- Region fixture added.
- Surface/underground dimensions.
- Patch placement counts.
- Water/road/reachability/layer transition proof summary.
- Negative proof summary.
- Evidence artifacts.
- Validation results.
- Artifact scope result.
- Evidence hygiene.
- Remaining P2/P3 debt.
- Final git status.
- Git commands used and why.

## Mandatory commit/push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:
- `GREEN Goal 088 deterministic visual region composer`
- `BLOCKED Goal 088 deterministic visual region composer`
- `FAILED Goal 088 deterministic visual region composer`
