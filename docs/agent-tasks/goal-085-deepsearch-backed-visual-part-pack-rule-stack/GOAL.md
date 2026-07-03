# Goal 085 — Deepsearch-Backed Visual Part-Pack Contract & Rule Stack Foundation

## Repo / working copy / branch

Repo: https://github.com/kpCat/LLMGameCreator
Working copy: `C:\Users\endim\LLMGameCreator\`
Branch: `main`
Codex reasoning: very high

## Primary objective

Consume the newly pushed `docs/deepsearch/*` research results and implement the next real foundation for the procedural visual stack: a BCL-only Application-side Visual Part-Pack Contract and Rule Stack validator/evidence seam.

This goal bridges:
- Goal 083 visual/adult context integration;
- Goal 084 visual asset contract/rating metadata;
- `e5b9b12 deepsearch docs`;
- the target of fast Heroes-III-like large maps, 2D/isometric transition targets, pseudo-3D/first-person presentation, water-aware biomes, settlements, creatures/NPCs, UI themes/effects, and rating-gated adult metadata.

Do not generate images, call media providers, add dependencies, mutate Runtime, mutate Unity, mutate public GamePackage schema, or create explicit adult content.

## Required preflight

1. Confirm branch is `main` and fetch `origin/main`.
2. Confirm current HEAD includes:
   - `1ee359f3` Goal 083;
   - `49930836` Goal 084;
   - `e5b9b12` deepsearch docs.
3. Confirm Goal 084 artifacts exist and are GREEN / accepted=false.
4. Confirm all 8 deepsearch docs exist under `docs/deepsearch/`.
5. Confirm `CONTEXT_INDEX.md` and `FULL_GENERATOR_GOAL_QUEUE.md` currently do or do not route deepsearch docs; record result in final report.
6. Confirm no active P0/P1 source-format issue.
7. Inspect dirty state. Do not stage/revert unrelated user changes.
8. Confirm no external dependencies are needed. Do not add SkiaSharp, DeBroglie, FastNoise, Clipper2, Rectpack, Unity packages or any other library in this goal.

## Read first

- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- Goal083 docs/evidence:
  - `docs/context/VISUAL_ADULT_LAYER_CONTEXT_INDEX.md`
  - `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md`
  - `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-layer-context-integration-report.md`
  - `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-doc-inventory.json`
  - `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-policy-routing-matrix.json`
- Goal084 docs/evidence/code/tests:
  - `.llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md`
  - `.llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-catalog.json`
  - `.llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-contract-negative-proof.json`
  - `.llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/source-document-lineage.json`
  - `src/LLMGameCreator.Application/Design/VisualAssetContractRatingMetadata/**`
  - `tests/LLMGameCreator.Tests/Application/VisualAssetContractRatingMetadata/**`
- Deepsearch:
  - all 8 `docs/deepsearch/*.md`
- Existing visual proposals:
  - `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md`
  - `docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md`
  - `docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md`
  - `docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md`

## Allowed files / areas

Application:
- `src/LLMGameCreator.Application/Design/VisualPartPackRuleStack/`

Tests:
- `tests/LLMGameCreator.Tests/Application/VisualPartPackRuleStack/`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualPartPackRuleStackProductSmokeTests.cs`

Evidence:
- `.llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/`

Docs/state:
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`

Optional docs:
- `docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md`
- `docs/proposals/VISUAL_PART_PACK_RULE_STACK_CONTRACT_V1.md`

Artifact scope:
- `.devflow/artifact-scope/artifact-scope-policy.json`

Task pack:
- `docs/agent-tasks/goal-085-deepsearch-backed-visual-part-pack-rule-stack/`

## Forbidden files / areas

Do not change:
- public GamePackage schema;
- Runtime / Runtime.Abstractions;
- Unity files, including `AlphaRuntimeBootstrap.cs`;
- Infrastructure provider / LLM / RAG / media provider code;
- Lua / Scripting;
- generator-library;
- `.sln`, `.csproj`, lock files;
- binary media assets;
- real NSFW assets;
- explicit adult content fixtures;
- prompt dumps or provider-output fixtures;
- external dependencies.

Do not branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Index and synthesize deepsearch docs

Update `CONTEXT_INDEX.md` and `FULL_GENERATOR_GOAL_QUEUE.md` to route the 8 deepsearch docs.

Create or update `docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md` with concise implementation-oriented synthesis:
- immediate future optional adapters: DeBroglie, SkiaSharp, RectpackSharp, FastNoise Lite, Clipper2, Tiled/LDtk as external importers, Unity BillboardAsset/LODGroup/SpriteAtlas as presentation-side consumers;
- prototyping candidates: MarkovJunior, mxgmn/WaveFunctionCollapse as reference, ConvChain, Unity Sprite Shape, SuperTiled2Unity/LDtkToUnity, OR-Tools/graph/constraint options;
- rejected/deferred candidates and reasons: ImageSharp default due licensing, Triangle.NET license ambiguity, unclear-license city generators, GPL/complex-provenance paperdoll tools as direct integration;
- design requirements: water/coast/river/lake/marsh is first-class; logical map and visual map remain separate; visual compiler is editor-time/offline; Runtime/Unity Player consumes approved refs/staged payloads only; adult/rating layer is metadata/review/export policy.

Do not copy whole deepsearch docs into the synthesis.

### 2. Implement VisualPartPackRuleStack Application seam

Create BCL-only models, validator, fixtures, evidence service, hash helper and quality gate scanner.

Recommended files:
- `VisualPartPackRuleStackModels.cs`
- `VisualPartPackRuleStackValidator.cs`
- `VisualPartPackRuleStackFixtures.cs`
- `VisualPartPackRuleStackEvidenceService.cs`
- `VisualPartPackRuleStackHash.cs`
- `VisualPartPackRuleStackQualityGateScanner.cs`

Recommended model concepts:
- `VisualPartPackManifest`, `VisualPartDefinition`, `VisualPartLayer`, `VisualMaskDefinition`, `VisualSocketDefinition`, `VisualAnchorDefinition`
- `VisualPaletteProfile`, `VisualPaletteSwapRule`, `VisualOverlayRule`
- `VisualBiomeProfile`, `VisualWaterProfile`, `VisualTerrainTransitionRule`, `VisualAutoTileRule`, `VisualObjectPlacementRule`
- `VisualCreatureBodyPlanProfile`, `VisualEquipmentOverlayProfile`, `VisualUiThemeProfile`, `VisualEffectProfile`
- `VisualPartPackRecipe`, `VisualRuleStackValidationResult`, `VisualRuleStackEvidenceResult`

Use metadata-only paths/refs/hashes. No images.

### 3. Create deterministic metadata-only fixture packs

At least six fixture packs:
1. `fantasy_overworld_tile_part_pack` — grass/dirt/snow/lava/rough/forest transitions.
2. `water_coast_river_marsh_part_pack` — sea/lake/river/coast/marsh/bridge/dock/water-object coverage.
3. `settlement_building_facade_part_pack` — house/castle/wall/gate/market/farm/mine/district concepts.
4. `creature_bodyplan_equipment_part_pack` — humanoid/beast/reptilian/insectoid/undead/mechanical profiles, equipment sockets, clothing/state overlays.
5. `ui_theme_icon_effect_part_pack` — panels/buttons/icons/status/weather/day-night/effect overlays.
6. `adult_rating_gated_extension_metadata_only` — neutral metadata extension only, safe fallback required, no explicit content.

### 4. Validate rule stack constraints

Validator must reject at least:
- duplicate ids;
- absolute paths;
- missing masks/sockets/anchors for layered parts;
- unknown palette/recipe refs;
- missing safe fallback for rating-gated adult extension;
- adult extension without eligible body-plan metadata;
- water/coast pack without coast/river/lake coverage;
- tile pack without transition/autotile rules;
- creature pack without body-plan compatibility rules;
- equipment overlay without socket compatibility;
- UI/effect pack without safe fallback;
- prompt text as source of truth;
- provider candidate treated as approved;
- cyclic recipe dependencies;
- unsafe export policy contradictions.

### 5. Link to Goal084 visual asset contract

Consume Goal084 evidence as lineage and map fixture packs to Goal084 slots:
- `fantasy_overworld_tile_safe`
- `water_coast_biome_safe`
- `settlement_building_safe`
- `creature_bodyplan_safe`
- `tech_future_ui_panel_safe`
- `humanoid_paperdoll_adult_capable_metadata_only`

Do not mutate Goal084 artifacts.

### 6. Generate evidence

Create `.llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/`.

Required artifacts:
- `visual-part-pack-rule-stack-report.md`
- `visual-part-pack-catalog.json`
- `visual-part-pack-validation-matrix.json`
- `visual-part-pack-negative-proof.json`
- `deepsearch-lineage-inventory.json`
- `goal084-contract-binding-matrix.json`
- `water-biome-coverage-matrix.json`
- `quality-gate-scan.json`

Evidence must prove:
- all 8 deepsearch docs consumed as lineage;
- Goal084 contract lineage passed;
- all six fixture packs validate;
- negative proof scenarios reject as expected;
- water/coast/river/marsh coverage exists;
- creature body-plan/equipment layering metadata exists;
- UI/effect/weather/day-night metadata exists;
- adult/rating extension is metadata-only and safe-fallback-bound;
- no media/provider/runtime/unity/schema/project/dependency changes;
- deterministic report hash.

### 7. Product smoke

Add product smoke that:
- builds Goal085 evidence from repo root;
- reads catalog, validation matrix, negative proof, deepsearch lineage and Goal084 binding matrix;
- proves all six fixture packs exist;
- proves water coverage is first-class;
- proves 100+ species scalability is represented as body-plan grammar capacity metadata, not individual hand-authored species assets;
- proves invalid scenarios are rejected;
- proves no generated media assets exist.

### 8. Docs/state update

Update current-state docs, context index, full queue and debt register.

Goal085 manual gate:
`visual_part_pack_rule_stack_verification required`

Status:
`implementationStatus=GREEN`, `accepted=false` only if all gates pass.

Record Goal084 as accepted by handoff before Goal085 if the current process requires a single active gate transition, but do not rewrite Goal084 artifacts.

## Quality gate

GREEN only if:
- no forbidden files changed;
- no external dependencies added;
- no images/media/binary assets added;
- no provider integration;
- no public GamePackage schema changes;
- no Runtime/Unity changes;
- all deepsearch docs are indexed/routed;
- all fixtures validate;
- invalid matrix rejects fake/unsafe cases;
- water/biome coverage exists;
- body-plan/equipment layering coverage exists;
- adult/rating extension remains metadata-only;
- evidence is deterministic;
- build/tests/check-all/artifact-scope pass;
- source formatting guard remains clean.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualPartPackRuleStack
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualPartPackRuleStackProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-085-deepsearch-backed-visual-part-pack-rule-stack"
git diff --check
git diff --cached --check
```

Also scan changed files for mojibake, absolute local paths in evidence, timestamps/heavy logs, prompt dumps, binary/media additions and one-line/minified C#.

## Stop / block conditions

Return BLOCKED if:
- the rule stack requires public GamePackage schema changes;
- it requires Runtime/Unity/provider/Lua/generator-library changes;
- it requires external dependencies;
- deepsearch docs conflict so strongly that a safe stage-1 contract cannot be created;
- adult/rating metadata cannot be represented safely without explicit content;
- artifact scope cannot be satisfied without forbidden areas.

Return FAILED if build/tests regress due to this goal and cannot be repaired inside allowed files.

## Final report format

Report:
- Final status.
- Latest commit before/after.
- Push status.
- Preflight summary.
- Deepsearch docs consumed and routed.
- Files changed.
- Contract/model types added.
- Fixture packs added.
- Water/biome coverage.
- Creature/body-plan/equipment coverage.
- Adult/rating metadata boundary.
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
- `GREEN Goal 085 visual part pack rule stack`
- `BLOCKED Goal 085 visual part pack rule stack`
- `FAILED Goal 085 visual part pack rule stack`

Do not rewrite history.
