# Codex task — GOAL 062 Constrained Spatial Detail Generation

## Assignment metadata

Repository:

```text
https://github.com/kpCat/LLMGameCreator
```

Working copy:

```text
C:\Users\endim\LLMGameCreator\
```

Branch:

```text
main
```

Composite goal id/name:

```text
goal-062-constrained-spatial-detail-generation
Goal 062: Constrained Spatial Detail Generation
```

Required goal marker / manual gate:

```text
constrained_spatial_detail_generation_verification
```

Codex reasoning level:

```text
very high
```

## Process update

This task follows the user's current LLMGameCreator workflow:

- aggressive composite goals are preferred;
- Codex must commit/push final state even when GREEN/BLOCKED/FAILED;
- small acceptance/hotfix work should be embedded in the next large goal when safe;
- do not create a separate acceptance-only goal unless explicitly requested.

## Preflight acceptance to record

Before implementing Goal 062, record the user's handoff acceptance of Goal 061:

```text
full_campaign_playable_review_package_rc_verification passed before Goal 062
```

Goal 061 implementation evidence from user handoff:
- status GREEN;
- commit `65689794 GREEN Goal 061 full campaign playable review package RC`;
- check-all: 1071/1071 ordinary tests;
- Unity proof: unityExitCode=0, playerExitCode=0, provenRowCount=9, missing markers 0;
- gate was left required until this handoff.

Do not start Goal 062 until the docs/state update clearly records this acceptance.

## Purpose

Consume the full campaign playable review package RC and materialized package matrix, then generate deterministic spatial detail for all 9 family/seed rows.

This goal should prove that the playable package rows are not only runnable, but also have validated local map/chunk/detail layouts:

- family-specific tile/detail palettes;
- MarkovJunior-style in-house rewrite/repair rule records;
- WFC-inspired adjacency/constraint records;
- deterministic row planner;
- reachability proof;
- repair/fallback diagnostics;
- Unity Alpha player markers.

Use mxgmn/WFC/MarkovJunior/TextureSynthesis as design references only. Do not add external dependencies or copy external assets/source.

## Read-first list

Read first, in order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_062_CONSTRAINED_SPATIAL_DETAIL_GENERATION_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_062_CONSTRAINED_SPATIAL_DETAIL_GENERATION.md`
8. Goal 061 artifacts:
   - `.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/`
9. Goal 060 artifacts:
   - `.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/`
10. Goal 059 artifacts:
   - `.llmgc/procedural/goal-059-full-generator-variability-regression-matrix/`
11. Existing local analogs:
   - `src/LLMGameCreator.Application/Design/FullCampaignPlayableReviewPackageRc/**`
   - `src/LLMGameCreator.Application/Design/FullCampaignGamePackageMaterialization/**`
   - `src/LLMGameCreator.Application/Design/FullGeneratorVariabilityRegressionMatrix/**`
   - `src/LLMGameCreator.Application/Design/WorldScaleRegionMapFoundation/**`
   - `src/LLMGameCreator.Application/Design/RuntimeChunkDeltaTraversal/**`
   - current Unity Alpha bootstrap:
     `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
12. Existing artifact scope policy and scenario patterns:
   - `.devflow/artifact-scope/artifact-scope-policy.json`
   - `.devflow/scripts/check-artifact-scope.ps1`

Do not read the entire repository unless local search shows the required pattern is elsewhere.

## Allowed files / areas

You may create or edit only these areas:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/GOAL_062_CONSTRAINED_SPATIAL_DETAIL_GENERATION_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_062_CONSTRAINED_SPATIAL_DETAIL_GENERATION.md
docs/agent-tasks/GOAL_062_CONSTRAINED_SPATIAL_DETAIL_GENERATION.md
docs/agent-tasks/GOAL_062_LAUNCHER.txt
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/ConstrainedSpatialDetailGeneration/**
tests/LLMGameCreator.Tests/Application/ConstrainedSpatialDetailGeneration/**
tests/LLMGameCreator.Tests/ProductSmoke/ConstrainedSpatialDetailGenerationProductSmokeTests.cs
.llmgc/procedural/goal-062-constrained-spatial-detail-generation/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Unity source change must be narrow:
- load a spatial-detail command plan or manifest from the Goal 062 staged/review package route;
- emit deterministic proof markers;
- do not refactor the whole bootstrap;
- do not add Unity packages;
- do not change scenes/project settings unless unavoidable, and stop as BLOCKED if unavoidable.

## Forbidden files / areas

Do not modify:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/**
src/LLMGameCreator.Scripting/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Also forbidden:
- public GamePackage schema changes;
- Runtime source changes;
- WinForms/UI changes;
- provider/LLM/RAG calls;
- media generation/provider calls;
- arbitrary Lua execution;
- adding NuGet dependencies;
- importing/copying mxgmn/DeBroglie source code;
- importing external sample assets/tiles/images;
- pretending Unity proof is real if Unity/player did not execute.

## Exact behavior

### 1. Goal 061 acceptance docs update

Update current-state docs to record:

```text
full_campaign_playable_review_package_rc_verification passed before Goal 062
```

Do not mark Goal 062 passed.

Preserve Goal 031/032 as produced-for-review/not passed if that remains the current docs model.

### 2. Application seam

Create:

```text
src/LLMGameCreator.Application/Design/ConstrainedSpatialDetailGeneration/
```

Suggested components, adapt to local style:

```text
ConstrainedSpatialDetailModels.cs
ConstrainedSpatialDetailSourceLoader.cs
ConstrainedSpatialPaletteCatalog.cs
ConstrainedSpatialRewriteRuleCatalog.cs
ConstrainedSpatialConstraintPlanner.cs
ConstrainedSpatialReachabilityPlanner.cs
ConstrainedSpatialRepairPlanner.cs
ConstrainedSpatialDetailEvidenceService.cs
ConstrainedSpatialDetailHash.cs
ConstrainedSpatialDetailValidator.cs
```

Keep classes reasonably split. Do not create a giant monolith.

### 3. Source facts

Load or read source facts from Goal 061/060/059 artifacts.

Minimum source facts:
- 9 package rows;
- 3 family ids;
- 3 seed ids;
- package row ids;
- review package RC manifest;
- family command plan or equivalent;
- media/review package binding ids if available.

Do not invent a new unrelated campaign. Consume the prior evidence chain.

### 4. Spatial palette model

Represent at least:
- tile id;
- semantic tags;
- family applicability;
- passability;
- hazard/resource/objective/door/corridor/settlement/biome flags;
- adjacency tags;
- render/thumbnail color or symbolic marker;
- provenance: `in_house_fixture`.

Families need different palettes:
- `map_panel_rpg`: roads, field, forest, settlement, quest marker, npc marker, item marker, exit;
- `survival_sandbox`: shelter, water, resource, hazard, safe path, weather marker, exit;
- `first_person_grid_dungeon`: wall, floor, corridor, door, encounter, objective, exit.

### 5. Rewrite/repair rules

Implement a tiny in-house deterministic rewrite rule record model inspired by MarkovJunior, not a MarkovJunior interpreter.

Rules should cover:
- ensure entry exists;
- ensure exit exists;
- ensure one objective anchor exists;
- connect critical anchors;
- repair isolated passable pockets;
- insert family-specific landmark/resource/encounter;
- mark blocked/unsafe cells where necessary.

Rule records must have:
- id;
- family applicability;
- priority;
- match/effect description fields;
- deterministic application order;
- diagnostics.

### 6. Constraint planning / WFC-inspired adjacency

Implement a bounded in-house planner, not external WFC code.

Minimum behavior:
- deterministic seed;
- grid dimensions per family/row;
- adjacency constraints by tile class;
- family-specific constraints;
- contradiction detection;
- retry or fallback budget;
- output row layout;
- trace of applied rules;
- trace of contradictions/fallback if any.

This can be a small domain-specific solver/rewrite planner, not a general WFC algorithm. The goal is validated spatial detail, not algorithm purity.

### 7. Reachability proof

For each of 9 rows prove family-specific routes.

Required:
- BFS or equivalent in-house path check;
- entry -> objective path;
- objective -> exit path;
- optional family-specific path:
  - map_panel_rpg: NPC/item/quest route;
  - survival_sandbox: shelter/resource/water/hazard-safe route;
  - first_person_grid_dungeon: corridor/door/encounter/objective route.

Record:
- reachable=true;
- route cell ids;
- blocked cell count;
- passable cell count;
- semantic anchors found.

### 8. Meaningful variance proof

Rows must not differ only by id/hash.

Compute compact variance metrics:
- tile histogram;
- anchor positions;
- path length;
- hazard/resource/encounter counts;
- family-specific semantic counts.

Prove:
- 9 distinct row hashes;
- rows in same family differ by at least 2 meaningful metrics;
- families differ by palette/rule set.

### 9. Preview/export payload

Produce a compact payload that future preview/export consumers can read:
- row id;
- family id;
- seed id;
- grid dimensions;
- tile data compact form;
- anchors;
- paths;
- thumbnail refs if generated;
- package row ref;
- review package ref;
- provenance and hashes.

### 10. Optional BCL PNG thumbnails

If practical within scope, generate small deterministic PNG thumbnails using BCL-only code or an existing in-repo helper.

If PNG writing is too risky or no local helper exists, skip thumbnails and record why.

Do not add ImageSharp/SkiaSharp/other dependency.

### 11. Unity Alpha proof

Narrowly extend `AlphaRuntimeBootstrap.cs` to load a Goal 062 command plan/manifest and emit proof markers.

Required markers should include:
- `spatial_detail_loaded=true`
- `spatial_detail_family=<family>`
- `spatial_detail_seed=<seed>`
- `spatial_detail_row=<row>`
- `spatial_detail_reachable=true`
- `spatial_detail_route_verified=true`
- `spatial_detail_variance_marker=<...>`
- `review_package_proof=goal062`

Preferred: prove all 9 rows.
Minimum acceptable GREEN: all 9 rows if current Unity route can do it.
If Unity route cannot execute honestly, status must be BLOCKED.

### 12. Evidence artifacts

Write under:

```text
.llmgc/procedural/goal-062-constrained-spatial-detail-generation/
```

Required artifacts:
- `source-manifest.json`
- `spatial-palette-catalog.json`
- `rewrite-rule-catalog.json`
- `constraint-rule-catalog.json`
- `spatial-detail-matrix.json`
- 9 row files named:
  - `spatial-detail-row-map_panel_rpg-seed_alpha.json`
  - `spatial-detail-row-map_panel_rpg-seed_beta.json`
  - `spatial-detail-row-map_panel_rpg-seed_gamma.json`
  - `spatial-detail-row-survival_sandbox-seed_alpha.json`
  - `spatial-detail-row-survival_sandbox-seed_beta.json`
  - `spatial-detail-row-survival_sandbox-seed_gamma.json`
  - `spatial-detail-row-first_person_grid_dungeon-seed_alpha.json`
  - `spatial-detail-row-first_person_grid_dungeon-seed_beta.json`
  - `spatial-detail-row-first_person_grid_dungeon-seed_gamma.json`
- `reachability-proof-matrix.json`
- `spatial-repair-fallback-matrix.json`
- `unity-spatial-detail-command-plan.json`
- `unity-spatial-detail-proof-summary.json`
- `preview-export-spatial-payload.json`
- `invalid-spatial-detail-diagnostics-matrix.json`
- `artifact-scope-report.json`
- `constrained-spatial-detail-generation-report.md`

Report must include:
- `implementationStatus=GREEN|BLOCKED|FAILED`
- `accepted=false`
- `manualGate=constrained_spatial_detail_generation_verification`
- `goal061AcceptedByUserHandoff=true`
- row counts;
- family counts;
- reachability proof summary;
- variance proof summary;
- Unity proof summary;
- no external dependency/source/assets imported.

### 13. Invalid/fake/leak matrix

Cover at minimum:
- missing Goal061 source;
- fake package row id;
- fake family;
- fake seed;
- invalid tile id;
- missing entry;
- missing exit;
- unreachable objective;
- contradiction/no tile candidate;
- unsafe path traversal;
- external asset provenance leak;
- copied mxgmn sample asset claim;
- provider/network/LLM/RAG claim;
- Lua execution claim;
- public GamePackage mutation claim;
- Runtime/UI broad mutation claim;
- nondeterministic ordering;
- missing Unity proof trace.

Each invalid case must have causal diagnostic code.

## Tests

Add focused tests in local style:

```text
tests/LLMGameCreator.Tests/Application/ConstrainedSpatialDetailGeneration/**
tests/LLMGameCreator.Tests/ProductSmoke/ConstrainedSpatialDetailGenerationProductSmokeTests.cs
```

Minimum test coverage:
- source loading;
- palette/rule catalog;
- deterministic planner;
- reachability proof;
- variance proof;
- invalid matrix;
- evidence writer;
- Unity command plan/proof summary;
- product smoke writes artifacts.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~ConstrainedSpatialDetailGeneration|FullyQualifiedName~Goal062"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~ConstrainedSpatialDetailGenerationProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal062|FullyQualifiedName~ConstrainedSpatial"

.\.devflow\scripts\check-all.ps1
```

Run the artifact scope guard using the existing repo pattern. Do not invent a new guard if the script already supports scenarios.

Also run a mojibake scan over changed text files/artifacts for common mojibake markers.

## Bounded repairs pre-authorized

You may perform bounded repairs if needed:
- stale current-state guard tests expecting a previous latest gate, if they are clearly current-state consistency guards;
- artifact-scope allowlist entry for Goal 062;
- exact historical artifact cleanup if `check-all.ps1` mutates unrelated historical artifacts, using only exact path restore from HEAD;
- Unity proof route marker extension in `AlphaRuntimeBootstrap.cs` only.

Do not broaden scope.

## Git policy

Codex must commit and push final state to `origin/main` regardless of GREEN/BLOCKED/FAILED.

Allowed inspection/staging commands:
- `git branch --show-current`
- `git status -sb`
- `git status --short --untracked-files=all`
- `git diff --stat`
- `git diff -- <explicit files>`
- `git diff --cached --check`
- `git diff --cached --stat`
- `git add -- <explicit allowed paths>`
- `git commit -m "..."`
- `git rev-parse HEAD`
- `git push origin main`

Forbidden:
- branch/switch/checkout;
- merge/rebase/cherry-pick;
- reset/stash/clean;
- force-push.

Commit message:
- GREEN: `GREEN Goal 062 constrained spatial detail generation`
- BLOCKED: `BLOCKED Goal 062 constrained spatial detail generation`
- FAILED: `FAILED Goal 062 constrained spatial detail generation`

## Final report format

Report in Russian:

```text
Goal 062 выполнен / заблокирован / провален

Status:
GREEN / BLOCKED / FAILED

Gate:
constrained_spatial_detail_generation_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<palette / rewrite rules / constraint planner / reachability / variance / Unity proof>

Evidence artifacts:
<список ключевых artifacts>

Проверки:
<команды и результаты>

Invalid/fake/leak matrix:
<covered scenarios>

Unity proof:
<unity/player exit codes and markers, or blocker>

Git:
<commit hash and push result>

Ограничения:
<external deps/source/assets/GamePackage/Runtime/UI/LLM/Lua/etc. not touched>

Следующий разумный шаг:
<one paragraph>
```
