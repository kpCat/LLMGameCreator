# Codex task — Goal 038 World-scale region graph, finite map pack and chunk-config foundation

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
goal_038_world_scale_region_map_foundation
Goal 038: World-scale region graph, finite map pack and chunk-config foundation
```

Required manual gate marker:

```text
world_scale_region_map_foundation_verification required
```

Codex reasoning level:

```text
very high
```

## Strategic intent

This is an aggressive composite goal. Do not implement only a small region registry. Move the generator toward a real generated world-scale playable/simulatable loop:

- first record Goal 037 acceptance from user handoff;
- then create deterministic region graphs;
- prove reachability;
- produce compact finite map packs;
- produce a chunked-world config prelude;
- write product-smoke evidence across four scenarios.

This goal must not be paper-only. It must produce deterministic generated world/map artifacts that a later runtime/export goal can consume.

## Read-first list

Read these first, in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_038_WORLD_SCALE_REGION_MAP_FOUNDATION_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_038_WORLD_SCALE_REGION_MAP_FOUNDATION.md`
8. Goal 037 docs/evidence:
   - `docs/GOAL_037_HYBRID_LLM_DRAFT_LUA_DETERMINISTIC_EXPANSION_SPEC.md` if present
   - `.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/hybrid-llm-draft-lua-deterministic-expansion-report.md`
   - `.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/hybrid-pipeline-summary.json`
   - `.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/lua-expansion-output-frontier.json`
   - `.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/lua-expansion-output-gothic.json`
   - `.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/lua-expansion-output-caravan.json`
   - `.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/lua-expansion-output-metamodule-kingdoms.json`
9. Relevant existing Application design patterns under `src/LLMGameCreator.Application/Design/` for recent goals 033–037.
10. Relevant product-smoke/evidence tests for recent goals 033–037.

Do not read the entire repository unless a narrow search shows the exact relevant files are elsewhere.

## Allowed files / areas

You may create or edit only:

```text
docs/GOAL_038_WORLD_SCALE_REGION_MAP_FOUNDATION_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_038_WORLD_SCALE_REGION_MAP_FOUNDATION.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
src/LLMGameCreator.Application/Design/WorldScaleRegionMapFoundation/**
tests/LLMGameCreator.Tests/Application/WorldScaleRegionMapFoundation/**
tests/LLMGameCreator.Tests/ProductSmoke/WorldScaleRegionMapFoundationProductSmokeTests.cs
.llmgc/procedural/goal-038-world-scale-region-map-foundation/**
```

Additionally, for pre-authorized bounded repairs only, you may edit stale current-state/handoff guard tests if and only if they fail because they hard-code an older current gate and the change preserves historical assertions while making current-state consistency dynamic:

```text
tests/LLMGameCreator.Tests/Application/**/**AcceptanceTests.cs
tests/LLMGameCreator.Tests/Devflow/**Tests.cs
```

Bounded guard repair must be minimal, causal, and reported separately.

## Forbidden files / areas

Do not modify unless the task becomes impossible and you record a BLOCKED commit:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG paths
src/LLMGameCreator.Scripting/**
unity/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Also forbidden:

- adding external dependencies;
- changing public GamePackage schema;
- real Runtime integration;
- real Unity integration;
- WinForms/UI work;
- provider/LLM/RAG calls;
- arbitrary Lua source generation or new Lua execution work;
- huge tile-array dumps;
- weakening acceptance/evidence tests for green output.

## Preflight: accept Goal 037 by user handoff

The user reported Goal 037 GREEN with:

- commit `878934a3` pushed to `origin/main`;
- `check-all.ps1` passed, 984/984, 0 warnings/errors;
- artifact scope guard passed, 13/13 allowed, 0 violations;
- `hybrid_llm_draft_lua_deterministic_expansion_verification required` left for manual review.

Record the user handoff acceptance before starting Goal 038 implementation:

```text
hybrid_llm_draft_lua_deterministic_expansion_verification passed
```

Update only state/context/queue docs as part of Goal 038. Do not mark Goal 038 passed.

Preserve these facts:

- Goal 031 remains produced-for-review/not passed unless current docs already say otherwise.
- Goal 032 remains produced-for-review/not passed unless current docs already say otherwise.
- Goal 033/034/035/036 are accepted according to current docs/user handoffs.
- Goal 038 is produced for review and stops at `world_scale_region_map_foundation_verification required`.

## Exact behavior

### 1. Create Application-layer world-scale region/map foundation

Create a new small set of Application-layer components under:

```text
src/LLMGameCreator.Application/Design/WorldScaleRegionMapFoundation/
```

Use local naming style. Keep files reasonably split, for example:

- models;
- catalog/seed builder;
- reachability validator/planner;
- finite map pack builder;
- chunk config prelude builder;
- evidence service.

Do not create one huge monolithic class.

### 2. Region graph model

Represent at least:

- scenario id/profile id;
- world graph id;
- kingdom ids;
- region ids;
- biome/terrain tags;
- hazard/weather/event tags;
- settlement/landmark ids;
- start region;
- required gameplay target regions;
- optional target regions;
- travel edges with stable id, from/to, route kind, cost, bidirectional flag, constraints, semantic tags;
- blocked/conditional/future-required edges;
- source evidence refs to Goal 033–037 concepts where useful.

Required route kinds include at least:

```text
road
trail
river
mountain_pass
sea_lane
caravan_route
dungeon_descent
magical_gate
```

### 3. Reachability and route validation

Implement deterministic in-house graph algorithms using BCL only:

- stable adjacency construction;
- reachability from start;
- required target coverage;
- route cost totals;
- disconnected component detection;
- blocked critical edge detection;
- duplicate edge/node detection;
- unknown ref detection;
- unstable ordering detection;
- scenario mismatch detection.

A small BFS/Dijkstra-style algorithm is enough. Do not add QuikGraph/GoRogue/RoyT.AStar.

### 4. Finite map pack generation

For each required scenario, build a compact finite map pack record with:

- map id;
- coordinate kind: `square` or `axial_hex`;
- width/height or radius/bounds;
- deterministic seed;
- tile/terrain patch summaries;
- passability summary;
- landmark placements;
- region-to-map bindings;
- route polylines or route cell summaries;
- entity/encounter/quest hook placement summaries;
- validation trace proving required landmarks/routes are placed and reachable.

Do not dump giant arrays. Small preview grids or compact patch/cell summaries are acceptable.

Use at least one hex/axial map example if it fits the current code naturally. Red Blob Games may be used as a design reference only; do not copy external code.

### 5. Chunked-world config prelude

Create chunked-world config records without runtime deltas:

- chunk size;
- deterministic chunk id format;
- scenario/world seed;
- region-to-chunk coverage;
- finite-map-to-chunk projection;
- future generation rule refs;
- forbidden mutation notes: package definitions are not runtime chunk state;
- future Goal 041 runtime-delta handoff notes.

This is a compact contract prelude for future chunk goals, not runtime integration.

### 6. Required scenarios

Produce meaningfully different outputs for:

- `frontier_survival`
- `gothic_intrigue`
- `caravan_trade`
- `metamodule_kingdoms`

`metamodule_kingdoms` must include:

- seven kingdom/region groups;
- at least 112 species/archetype slot references as compact metadata;
- a graph that is not just a copy of the other scenarios.

### 7. Invalid/fake/leak matrix

Cover at least these negative cases:

- duplicate region id;
- duplicate edge id;
- unknown edge endpoint;
- missing start region;
- required target unreachable;
- all routes blocked;
- contradictory bidirectional edge declaration;
- negative/zero invalid travel cost;
- unknown landmark region;
- invalid map coordinate/size;
- route polyline missing required region binding;
- chunk config does not cover required map region;
- fake Goal 037 expansion output id;
- scenario/profile mismatch;
- nondeterministic ordering mutation;
- huge tile-array dump attempt;
- forbidden Runtime/UI/Unity/GamePackage/provider/LLM/RAG/Lua source/generator-library leakage.

Each case must produce a specific diagnostic code or a specific blocked status. Do not throw for ordinary validation failures.

### 8. Evidence artifacts

Write deterministic evidence under:

```text
.llmgc/procedural/goal-038-world-scale-region-map-foundation/
```

Required files:

```text
region-graph-summary.json
reachability-matrix.json
finite-map-pack-frontier.json
finite-map-pack-gothic.json
finite-map-pack-caravan.json
finite-map-pack-metamodule-kingdoms.json
chunked-world-config-prelude.json
traversal-itinerary-matrix.json
invalid-world-scale-diagnostics-matrix.json
world-scale-region-map-foundation-report.md
```

Evidence must be compact, deterministic, sorted by stable ids, parseable JSON where applicable, free of absolute paths and free of nondeterministic timestamps.

### 9. Product smoke

Add a product smoke test route/class that proves the complete Goal 038 path:

```text
Goal037 evidence -> region graph -> reachability -> finite map pack -> chunk config prelude -> report
```

The smoke must run without Runtime, Unity, UI, provider, LLM/RAG or generator-library changes.

### 10. State/docs update

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Expected final state:

- Goal 037 accepted by user handoff: `hybrid_llm_draft_lua_deterministic_expansion_verification passed`.
- Goal 038 produced for review: `world_scale_region_map_foundation_verification required`.
- Goal 039+ remain future/recommended, not started.
- Preserve Goal 031/032 not passed unless already accepted in current docs.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/WorldScaleRegionMapFoundation/
```

Suggested classes, adjusted to local style:

- `WorldScaleRegionGraphTests`
- `WorldScaleReachabilityPlannerTests`
- `FiniteMapPackBuilderTests`
- `ChunkedWorldConfigPreludeTests`
- `WorldScaleRegionMapEvidenceTests`
- `WorldScaleRegionMapValidatorTests`

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/WorldScaleRegionMapFoundationProductSmokeTests.cs
```

Tests must prove:

- four scenario graphs are deterministic and meaningfully different;
- required targets are reachable in valid scenarios;
- map packs bind regions/landmarks/routes;
- chunk config covers finite-map extents;
- metamodule scenario includes seven kingdoms/regions and 112+ species/archetype slots;
- invalid/fake/leak matrix produces causal diagnostics;
- evidence files are generated and parseable;
- no Runtime/UI/Unity/GamePackage/provider/LLM/RAG/generator-library path is required.

## Validation commands

Run focused checks first, then full gate at the end:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~WorldScaleRegionMapFoundation|FullyQualifiedName~WorldScaleRegionGraph|FullyQualifiedName~WorldScaleReachability|FullyQualifiedName~FiniteMapPack|FullyQualifiedName~ChunkedWorldConfig|FullyQualifiedName~Goal038"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~WorldScaleRegionMapFoundationProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal038|FullyQualifiedName~WorldScaleRegionMap"

.\.devflow\scripts\check-all.ps1
```

After green `check-all.ps1`, run the existing artifact scope guard using the repo's standard command/pattern if present. Do not invent a new guard. Label/scope should be `goal-038-final` if the existing script supports labels.

## Pre-authorized bounded repairs

To reduce manual handoff blockers, these bounded repairs are pre-authorized:

1. Stale current-state/handoff guard tests may be updated if they hard-code an older current gate. Historical goal-specific assertions must remain strict. Current gate consistency should be dynamic from `CURRENT_GENERATOR_STATE.json` and docs.
2. Exact accidental historical `.llmgc/procedural/**` artifacts mutated by `check-all.ps1` may be restored from `HEAD` using:

```powershell
git restore --source=HEAD -- <exact accidental historical artifact paths>
```

Do not restore Goal 038 source/docs/evidence.

3. If existing historical smoke logs are required by check-all and are missing from root ignored paths, copying from an existing real generated cache is allowed only for exact expected paths and only with source->target report. Do not fabricate logs.
4. These repairs must be reported separately and included in final commit/push with honest GREEN/BLOCKED/FAILED status.

## Git policy — final commit/push required

Codex must commit and push the final state to `origin/main` regardless of GREEN/BLOCKED/FAILED result. GitHub must remain the source of truth for follow-up review.

Allowed git commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <specific changed files>
git add <specific changed files>
git commit -m <message>
git push origin main
git restore --source=HEAD -- <exact accidental historical artifact paths>   # only bounded repair above
```

Forbidden always:

```text
git checkout
git switch
git reset
git clean
git stash
git merge
git rebase
git cherry-pick
git push --force
```

Commit messages:

- GREEN: `Goal 038 world-scale region map foundation`
- BLOCKED: `BLOCKED Goal 038 world-scale region map foundation`
- FAILED: `FAILED Goal 038 world-scale region map foundation`

Do not mark `world_scale_region_map_foundation_verification` passed. Leave it required for review.

## Final report format

Report in Russian:

```text
Goal 038 выполнен / заблокирован / провален

Status:
GREEN / BLOCKED / FAILED

Gate:
world_scale_region_map_foundation_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<region graph / reachability / finite map packs / chunk config prelude / evidence>

Evidence artifacts:
<список файлов>

Сценарии:
<counts for frontier/gothic/caravan/metamodule>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<covered cases>

Bounded repairs:
<none or exact repairs>

Git:
<commit hash and push result>

Ограничения:
<what was not touched>

Следующий разумный шаг:
<Goal 039/040/041 combined recommendation or hotfix>
```
