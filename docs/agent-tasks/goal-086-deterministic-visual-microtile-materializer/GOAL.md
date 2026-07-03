# Goal 086 — Deterministic Visual Microtile Materializer & Biome/Water Preview

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Implement the first real deterministic procedural visual materialization seam for LLMGameCreator: a BCL-only editor-side microtile/part-preview materializer that consumes the Goal 084 visual asset contract and Goal 085 visual part-pack rule stack, then produces compact text-based SVG preview artifacts and machine-readable manifests for terrain, biome, water, structure, creature/paperdoll, UI and effects fixtures.

This goal must prove that the project can generate visual pieces from metadata/rules/seeds without LLM/provider/runtime calls and without a giant pre-generated asset dictionary.

This is not the final renderer. It is a deterministic proof foundation for the future visual compiler.

## Current context

Important recent commits/goals:
- Goal 083 integrated visual/adult layer context and media policy gates.
- Goal 084 added visual asset contract and rating metadata validation.
- Goal 085 added deepsearch-backed visual part-pack rule stack with fixtures for overworld tiles, water/coast/river/lake/marsh, settlements/buildings, creature bodyplans/equipment, UI/effects, and adult/rating-gated metadata-only extension.
- The user's target is a Heroes III-like fast map-generation capability, eventually feeding 2D/isometric/pseudo-3D/first-person views, with water/biomes/settlements/creatures/effects handled as first-class concerns.
- Runtime/Unity Player must not call LLM/RAG/media providers. Visual generation is editor/offline only.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current `HEAD` includes:
   - `1ee359f3 GREEN Goal 083 visual adult layer context integration`
   - `49930836 GREEN Goal 084 visual asset contract rating metadata`
   - `e5b9b12 deepsearch docs`
   - `26d213b7 GREEN Goal 085 visual part pack rule stack`
4. Confirm Goal 084 and Goal 085 artifacts exist, are GREEN, `accepted=false`, and manual gates remain required.
5. Confirm all 8 `docs/deepsearch/*.md` files exist.
6. Confirm `docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md` exists.
7. Inspect current dirty state. Do not stage/revert unrelated user files.
8. Confirm the goal can be implemented without public GamePackage schema, Runtime, Unity, providers, Lua/Scripting, generator-library, project-file, dependency or binary-media changes.

## Read first

- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md`
- `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md`
- all files in `docs/deepsearch/`
- `.llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-rating-metadata-report.md`
- `.llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/visual-asset-contract-catalog.json`
- `.llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-rule-stack-report.md`
- `.llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-catalog.json`
- `.llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/water-biome-coverage-matrix.json`
- `.llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-validation-matrix.json`
- `.llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/visual-part-pack-negative-proof.json`
- `src/LLMGameCreator.Application/Design/VisualAssetContractRatingMetadata/**`
- `src/LLMGameCreator.Application/Design/VisualPartPackRuleStack/**`
- related Goal 084/085 tests and product smoke tests.

## Allowed files / areas

You may change only:

- `src/LLMGameCreator.Application/Design/DeterministicVisualMicrotileMaterializer/`
- `tests/LLMGameCreator.Tests/Application/DeterministicVisualMicrotileMaterializer/`
- `tests/LLMGameCreator.Tests/ProductSmoke/DeterministicVisualMicrotileMaterializerProductSmokeTests.cs`
- `.llmgc/procedural/goal-086-deterministic-visual-microtile-materializer/`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- optionally `docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md` only to route Goal 086 output.
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/agent-tasks/goal-086-deterministic-visual-microtile-materializer/`

## Forbidden files / areas

Do not change:
- public GamePackage schema;
- Runtime / Runtime.Abstractions;
- Unity files, including `AlphaRuntimeBootstrap.cs`;
- Infrastructure provider / LLM / RAG / media provider code;
- Lua / Scripting;
- generator-library;
- `.sln`, `.csproj`, lock files;
- external dependencies;
- binary media assets;
- real NSFW assets;
- explicit prompt dumps or provider-output fixtures.

Do not branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Create BCL-only visual microtile materializer

Create `LLMGameCreator.Application.Design.DeterministicVisualMicrotileMaterializer`.

Use only BCL APIs. Do not add SkiaSharp/ImageSharp/DeBroglie/etc. Those can be optional adapters later.

The materializer should consume Goal 084/085 artifacts from `.llmgc/procedural/...` and build a compact deterministic visual preview catalog.

Output text SVG preview files plus JSON manifests. SVG is allowed because it is text evidence; do not add binary images.

### 2. Required generated preview coverage

Generate a bounded set of preview variants, approximately 18–36 total, not thousands.

Must include at least:

Terrain / biome:
- grass/overworld;
- snow;
- desert/dry;
- lava/ash;
- forest/decor overlay;
- mountain/rock overlay.

Water:
- water base;
- coast transition;
- river segment;
- lake edge;
- marsh/swamp;
- bridge/dock/water-object anchor metadata.

Settlement / structure:
- small dwelling/module;
- wall/gate/module;
- mine/production object;
- caravan/camp marker.

Creature / NPC:
- simple body-plan silhouette;
- equipment/clothing overlay;
- damaged/dirty/worn state overlay metadata.
Use neutral safe metadata only.

UI / effects:
- UI frame/panel motif;
- status/effect aura;
- day/night/weather palette overlay.

Adult/rating:
- metadata-only adult-capable slot proof with safe fallback; do not generate explicit image/content.

### 3. SVG materialization constraints

Each SVG preview must be deterministic and derived from:
- part pack id;
- asset slot id;
- palette profile;
- layer stack;
- seed;
- mask/socket/anchor metadata;
- optional biome/water rule.

The SVG should include:
- viewBox;
- background layer;
- 2–5 generated shapes/patterns;
- palette-driven colors;
- deterministic jitter/noise based on seed;
- layer ordering;
- no embedded external resources;
- no scripts;
- no base64;
- no prompt text.

### 4. Manifests and proofs

Create evidence artifacts:

- `visual-microtile-materializer-report.md`
- `visual-microtile-preview-catalog.json`
- `visual-microtile-materialization-manifest.json`
- `visual-microtile-file-ledger.json`
- `visual-microtile-water-biome-proof.json`
- `visual-microtile-layering-proof.json`
- `visual-microtile-negative-proof.json`
- `visual-microtile-quality-gate-scan.json`
- `visual-microtile-source-lineage.json`
- `previews/*.svg`

### 5. Negative proof

Reject and prove at least these invalid cases:

- absolute output path;
- prompt text as source of truth;
- missing palette;
- missing layer stack;
- coast tile without water/land adjacency metadata;
- river tile without flow connectors;
- adult-capable slot without safe fallback;
- provider candidate treated as approved output;
- non-deterministic seed / missing seed;
- SVG with script/external resource/base64;
- duplicate preview id;
- missing source Goal084/085 lineage.

### 6. Product smoke

Add product smoke that:
- builds evidence from repo root;
- reads every generated SVG and JSON manifest;
- validates all previews referenced by the catalog exist;
- verifies hashes in file ledger;
- verifies deterministic rerun hash stability;
- verifies water/coast/river/marsh coverage;
- verifies creature/equipment/state coverage;
- verifies UI/effect/weather coverage;
- verifies adult metadata-only fallback coverage;
- verifies negative proof rejects invalid cases;
- verifies no binary media files were created.

### 7. Docs/state update

Update current-state docs, context index, queue, debt register and artifact-scope policy.

Goal 086 manual gate:
`deterministic_visual_microtile_materializer_verification required`

Record remaining P2/P3 debt:
- SVG preview is a proof materializer, not final renderer;
- no external renderer adapter yet;
- no atlas/texture packing yet;
- no full map composition yet;
- no WinForms visual review workspace yet;
- no Unity consumption of generated previews yet.

## Quality gate

GREEN only if:
- no forbidden files changed;
- no external dependencies added;
- no public GamePackage schema changes;
- no Runtime/Unity/provider/Lua/generator-library changes;
- no binary media added;
- generated previews are text SVG only;
- no prompt dumps;
- no provider calls;
- no explicit adult content;
- deterministic rerun stable;
- water/biome coverage present;
- negative proof is real;
- artifact scope passes;
- build/tests/check-all pass;
- source formatting remains clean.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualMicrotileMaterializer
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualMicrotileMaterializerProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-086-deterministic-visual-microtile-materializer"
git diff --check
git diff --cached --check
```

Also scan changed files for mojibake, absolute local paths in evidence, timestamps/heavy logs, prompt dumps, binary/media additions.

## Stop / block conditions

Return BLOCKED if:
- producing text SVG previews requires external dependencies;
- materialization requires public GamePackage schema changes;
- materialization requires Runtime/Unity/provider/Lua/generator-library changes;
- adult/rating metadata cannot be kept non-explicit and fallback-bound;
- artifact scope cannot be satisfied without broadening into forbidden zones.

Return FAILED if build/tests regress due to this goal and cannot be repaired inside allowed files.

## Final report format

Report:
- Final status.
- Latest commit before/after.
- Push status.
- Preflight summary.
- Files changed.
- Generated preview count and categories.
- Water/biome coverage.
- Creature/equipment/state coverage.
- UI/effect/weather coverage.
- Adult metadata-only fallback coverage.
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
- `GREEN Goal 086 deterministic visual microtile materializer`
- `BLOCKED Goal 086 deterministic visual microtile materializer`
- `FAILED Goal 086 deterministic visual microtile materializer`
