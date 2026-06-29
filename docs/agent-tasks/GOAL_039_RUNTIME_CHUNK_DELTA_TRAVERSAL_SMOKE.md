# Codex task — GOAL 039 Runtime Chunk Delta Traversal Smoke

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
goal_039_runtime_chunk_delta_traversal_smoke
Goal 039: Runtime Chunk Delta And Traversal Smoke
```

Required gate marker:

```text
runtime_chunk_delta_traversal_smoke_verification
```

Codex reasoning level:

```text
very high
```

## Process policy

This is an aggressive composite goal. It intentionally combines the next practical chunk/runtime proof work instead of making several small handoff goals.

You must commit and push the final state to `origin/main` even if the result is GREEN, BLOCKED or FAILED.

Commit message policy:

```text
GREEN Goal 039 runtime chunk delta traversal smoke
BLOCKED Goal 039 runtime chunk delta traversal smoke
FAILED Goal 039 runtime chunk delta traversal smoke
```

Do not pretend a non-green result is accepted. Do not mark the manual gate passed.

## Required preflight

1. Work in `C:\Users\endim\LLMGameCreator\`.
2. Confirm branch is `main`.
3. Read the current state/queue docs.
4. Record user handoff acceptance of Goal 038 before implementing Goal 039:

```text
world_scale_region_map_foundation_verification passed
```

Source: user report after Goal 038 green checks and push.

5. Do not create a separate acceptance-only commit. Fold the Goal 038 acceptance docs update into this Goal 039 commit.

## Read-first list

Read these first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_039_RUNTIME_CHUNK_DELTA_TRAVERSAL_SMOKE_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_039_RUNTIME_CHUNK_DELTA_TRAVERSAL_SMOKE.md`
8. Goal 038 implementation/evidence:
   - `src/LLMGameCreator.Application/Design/WorldScaleRegionMapFoundation/**`
   - `tests/LLMGameCreator.Tests/Application/WorldScaleRegionMapFoundation/**`
   - `.llmgc/procedural/goal-038-world-scale-region-map-foundation/**`
9. Existing runtime state/save/load/serializer/snapshot code. Search narrowly for:
   - `GameRuntimeState`
   - `RuntimeStateSerializer`
   - `RuntimeSnapshotStore`
   - `QuestRuntimeState`
   - `Inventory`
   - `Flags`
   - previous product smoke tests that prove runtime save/load/state deltas
10. Goal 037 evidence only as upstream provenance if useful:
    - `.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/**`

Do not read the whole repo unless narrow search shows the relevant code lives elsewhere.

## Allowed files / areas

You may create/edit:

```text
docs/GOAL_039_RUNTIME_CHUNK_DELTA_TRAVERSAL_SMOKE_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_039_RUNTIME_CHUNK_DELTA_TRAVERSAL_SMOKE.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md

src/LLMGameCreator.Application/Design/RuntimeChunkDeltaTraversal/**
tests/LLMGameCreator.Tests/Application/RuntimeChunkDeltaTraversal/**
tests/LLMGameCreator.Tests/ProductSmoke/RuntimeChunkDeltaTraversalProductSmokeTests.cs

.llmgc/procedural/goal-039-runtime-chunk-delta-traversal-smoke/**
```

Narrow runtime state allowance:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
```

Only if strictly required for a backward-compatible runtime chunk delta state record or serializer/save-load support. If existing runtime state can store the deltas without runtime code changes, prefer not touching Runtime.

You may update existing current-state/handoff guard tests only if they become stale because the current gate moved from Goal 038 to Goal 039.

## Forbidden files / areas unless stop condition is triggered and final state is committed as BLOCKED

```text
src/LLMGameCreator.GamePackage/**
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

Forbidden always:

- external dependencies;
- GamePackage public schema change;
- real Lua execution changes;
- generated Lua source;
- provider/LLM/RAG calls;
- WinForms/UI;
- Unity build/player work;
- broad Runtime refactor;
- weakening tests/evidence;
- fake save/load proof.

## Exact behavior

### 1. Goal 038 acceptance preflight

Update state docs so Goal 038 is accepted by user handoff:

```text
world_scale_region_map_foundation_verification passed
```

Do not mark Goal 039 passed.

### 2. Runtime chunk delta traversal seam

Create a narrow Application-layer seam for runtime chunk delta traversal.

Suggested folder:

```text
src/LLMGameCreator.Application/Design/RuntimeChunkDeltaTraversal/
```

Required behavior:

- load/consume Goal 038 scenario ids and map/chunk evidence as source facts, or reproduce the cataloged deterministic scenario facts through the same in-memory catalog pattern used by nearby goals;
- build traversal itineraries for 4 scenarios;
- project traversal events into runtime chunk delta commands;
- apply those commands to runtime-owned state or a narrow runtime-facing state record;
- produce before/after state deltas;
- prove save/load or serializer roundtrip;
- prove replay determinism.

### 3. Runtime-owned state proof

Do not create a paper-only model detached from runtime.

Use existing runtime serializer/snapshot/save-load mechanisms where possible. If a small runtime chunk delta state type is strictly required, keep it:

- backward-compatible;
- serializable;
- deterministic;
- isolated from GamePackage definitions;
- tested through the existing serializer/save-load path.

Minimum state facts:

- scenario id;
- region id;
- chunk id;
- visited/discovered marker;
- route checkpoint marker;
- landmark discovery;
- at least one mutation;
- deterministic replay marker.

### 4. Scenarios

Support at least:

```text
frontier_survival
gothic_intrigue
caravan_trade
metamodule_kingdoms
```

`metamodule_kingdoms` must remain large enough to prove the 7 kingdom / 112 species-archetype slot context is not lost, even if not every slot is physically traversed.

### 5. Evidence writer

Write compact deterministic evidence to:

```text
.llmgc/procedural/goal-039-runtime-chunk-delta-traversal-smoke/
```

Required files:

```text
chunk-traversal-plan-frontier.json
chunk-traversal-plan-gothic.json
chunk-traversal-plan-caravan.json
chunk-traversal-plan-metamodule.json
runtime-chunk-delta-state-frontier.json
runtime-chunk-delta-state-metamodule.json
runtime-save-load-roundtrip-proof.json
chunk-replay-determinism-proof.json
invalid-chunk-diagnostics-matrix.json
runtime-chunk-delta-traversal-smoke-report.md
```

The markdown report must contain:

```text
runtime_chunk_delta_traversal_smoke_verification required
accepted=false
```

### 6. Invalid/fake/leak matrix

Implement causal diagnostics for:

- fake Goal038 scenario id;
- fake region id;
- fake chunk id;
- route edge not in reachability plan;
- chunk coordinate outside finite/chunk config bounds;
- duplicate delta id;
- conflicting delta mutation;
- replay seed mismatch;
- mutation tries to edit GamePackage/package definitions;
- Runtime/UI/Unity/provider/LLM/RAG/Lua source/generator-library leakage;
- filesystem/network/process/reflection/thread/time/random/native interop leakage;
- missing save/load proof;
- nondeterministic ordering.

### 7. Tests

Add focused tests proving:

- traversal plans are derived from known scenario/map/chunk facts;
- reachability/itinerary commands are stable;
- runtime chunk delta state changes after traversal;
- save/load or serializer roundtrip preserves chunk deltas;
- replay with the same seed is deterministic;
- invalid/fake/leak matrix gives causal diagnostics;
- evidence artifacts exist and parse;
- no GamePackage definitions are mutated;
- no Runtime/UI/Unity/provider/LLM/RAG/Lua source/generator-library leakage.

Suggested test classes:

```text
RuntimeChunkTraversalPlannerTests
RuntimeChunkDeltaStateTests
RuntimeChunkSaveLoadRoundtripTests
RuntimeChunkInvalidMatrixTests
RuntimeChunkDeltaEvidenceTests
RuntimeChunkDeltaTraversalProductSmokeTests
```

### 8. Validation commands

Run focused checks first, full check at the end:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~RuntimeChunkDeltaTraversal|FullyQualifiedName~RuntimeChunk|FullyQualifiedName~Goal039"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~RuntimeChunkDeltaTraversalProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal039|FullyQualifiedName~RuntimeChunk"

.\.devflow\scripts\check-all.ps1
```

Then run the existing artifact scope guard for Goal 039 if the repository has the usual command/policy pattern. Do not invent a new guard framework.

### 9. Pre-authorized bounded repairs

To avoid unnecessary handoffs, you may perform these bounded repairs if they are needed and directly caused by this goal:

1. Update stale current-state/handoff guard tests when they hardcode the previous latest gate.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates them.
3. Repair product-smoke route metadata if the new smoke route is missing only manifest/registry wiring.
4. If the runtime serializer has a narrow missing case for the newly introduced chunk delta state, add minimal backward-compatible serialization support inside the allowed Runtime scope.

Every bounded repair must be listed in the final report.

## Stop conditions

Commit/push as BLOCKED if any occurs:

- public GamePackage schema change is required;
- WinForms/UI or Unity changes are required;
- provider/LLM/RAG calls are required;
- generated Lua source or new Lua executor changes are required;
- external dependency is required;
- runtime changes would become broad/refactor-level;
- save/load proof cannot be made real and would be paper-only;
- check-all fails after bounded repair attempts;
- evidence is nondeterministic or contains absolute paths/heavy logs.

## Git policy

Allowed:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <specific changed files>
git add <specific changed files>
git commit -m "<GREEN/BLOCKED/FAILED message>"
git push origin main
```

Forbidden:

```text
git checkout
git switch
git merge
git rebase
git cherry-pick
git reset
git stash
git clean
git push --force
```

You must commit and push final state to origin/main regardless of GREEN/BLOCKED/FAILED result.

## Final report format

Report in Russian:

```text
Goal 039 выполнен / заблокирован / failed

Status:
GREEN / BLOCKED / FAILED

Gate:
runtime_chunk_delta_traversal_smoke_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Runtime proof:
<what real runtime state/save-load/replay path was proven>

Evidence artifacts:
<список>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<covered cases>

Bounded repairs:
<none or list>

Git:
<commit hash and push result>

Committed despite non-green result:
yes/no

Ограничения:
<GamePackage/UI/Unity/provider/LLM/RAG/Lua source/generator-library/external deps not touched>

Следующий разумный шаг:
<next large goal, likely chunked runtime preview/export or multi-family generator loop>
```
