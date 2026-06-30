# Codex task — GOAL 040 Chunked Runtime Preview/Export Multi-Family Smoke

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
goal-040-chunked-runtime-preview-export-multifamily-smoke
Goal 040: Chunked Runtime Preview/Export Multi-Family Smoke
```

Codex reasoning level:

```text
very high
```

Gate:

```text
chunked_runtime_preview_export_multifamily_smoke_verification required
```

## Process policy

This is an aggressive composite goal. It intentionally combines:

- accepting Goal 039 by user handoff;
- Goal 040 chunked runtime preview/export consumption;
- Goal 041 multi-family world-scale runtime regression;
- Goal 042 infinite/chunked world smoke pre-proof.

Do not split these into separate user-blocking tasks unless the work becomes unsafe.

Final commit/push is mandatory even for BLOCKED/FAILED results.

## Required final commit policy

At the end of the task, commit and push final state to `origin/main` regardless of result.

Commit messages:

```text
GREEN Goal 040 chunked runtime preview export multifamily smoke
BLOCKED Goal 040 chunked runtime preview export multifamily smoke
FAILED Goal 040 chunked runtime preview export multifamily smoke
```

Use `GREEN` only if all required checks are green and the implementation honestly proves the consumer path.

Use `BLOCKED` if the implementation is partly done or the repository state proves a blocker outside allowed safe scope.

Use `FAILED` if the implementation cannot be completed and the changed diagnostic state must still be preserved.

Never pretend a blocked/non-green result is accepted.

## Read-first list

Read these first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_040_CHUNKED_RUNTIME_PREVIEW_EXPORT_MULTIFAMILY_SMOKE_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_040_CHUNKED_RUNTIME_PREVIEW_EXPORT_MULTIFAMILY_SMOKE.md`
8. Goal 038 artifacts: `.llmgc/procedural/goal-038-world-scale-region-map-foundation/**`
9. Goal 039 artifacts: `.llmgc/procedural/goal-039-runtime-chunk-delta-traversal-smoke/**`
10. Existing Application design areas around:
    - `src/LLMGameCreator.Application/Design/WorldScaleRegionMapFoundation/**`
    - `src/LLMGameCreator.Application/Design/RuntimeChunkDeltaTraversal/**`
    - existing preview/export/minimum-playable/Unity-export acceptance services under `src/LLMGameCreator.Application/Design/**`, if present.
11. Existing product smoke tests for Goal 038/039 and any preview/export product smokes.

Use narrow search. Do not scan the whole repository unless needed to identify existing preview/export seams.

## Allowed files / areas

You may create/edit:

```text
docs/GOAL_040_CHUNKED_RUNTIME_PREVIEW_EXPORT_MULTIFAMILY_SMOKE_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_040_CHUNKED_RUNTIME_PREVIEW_EXPORT_MULTIFAMILY_SMOKE.md
docs/agent-tasks/GOAL_040_CHUNKED_RUNTIME_PREVIEW_EXPORT_MULTIFAMILY_SMOKE.md
docs/agent-tasks/GOAL_040_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
src/LLMGameCreator.Application/Design/ChunkedRuntimePreviewExportSmoke/**
tests/LLMGameCreator.Tests/Application/ChunkedRuntimePreviewExportSmoke/**
tests/LLMGameCreator.Tests/ProductSmoke/ChunkedRuntimePreviewExportSmokeProductSmokeTests.cs
.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/**
```

Pre-authorized bounded test guard repairs, only if stale current-state/handoff tests fail due to the new Goal 040 gate:

```text
tests/LLMGameCreator.Tests/Devflow/**
tests/LLMGameCreator.Tests/Application/**AcceptanceTests.cs
```

Only make minimal current-state consistency changes. Do not weaken historical goal assertions.

Pre-authorized artifact scope policy update, only if the existing scope guard requires a new Goal 040 folder entry:

```text
.devflow/artifact-scope/artifact-scope-policy.json
```

## Forbidden files / areas

Do not modify unless the task becomes BLOCKED and you report why:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/**
unity/**
generator-library/**
templates/**
samples/**
*.sln
*.csproj
```

Also forbidden:

- broad Runtime streaming architecture;
- WinForms UI changes;
- Unity script/entrypoint changes;
- GamePackage schema changes;
- provider/LLM/RAG calls;
- Lua executor changes;
- new external dependencies;
- media generation;
- weakening tests to get green.

## Exact behavior

### 1. Preflight

- Confirm branch `main`.
- Confirm current state contains Goal 039 produced-for-review with `runtime_chunk_delta_traversal_smoke_verification required`.
- Record user handoff acceptance of Goal 039:

```text
runtime_chunk_delta_traversal_smoke_verification passed
```

- Do not mark Goal 040 passed.

### 2. Build a real consumer seam

Create a new Application-layer seam under:

```text
src/LLMGameCreator.Application/Design/ChunkedRuntimePreviewExportSmoke/
```

The seam must consume Goal 039 runtime chunk delta traversal artifacts and produce deterministic preview/export-compatible payloads.

Minimum components, names may follow local style:

- catalog / source loader or source facts model;
- consumer payload builder;
- export manifest builder;
- multi-family regression planner;
- infinite/chunked smoke proof builder;
- validator;
- evidence writer.

The consumer payload must preserve at least:

- scenario id;
- source Goal 038/039 evidence refs;
- chunk ids;
- traversal route/checkpoints;
- visited/discovered markers;
- landmark discovery markers;
- mutation markers;
- replay/save-load correlation;
- family lens id;
- preview/export readiness flags;
- blocked/future-required gaps.

### 3. Multi-family regression

Without implementing Goals 043-045, prove that at least three family lenses can reuse the same world/chunk traversal payload format:

```text
map_panel_rpg
survival_sandbox
first_person_grid_dungeon
```

Each family lens must have distinct expected consumer needs, for example:

- map/panel RPG: region panels, travel log, landmark focus;
- survival sandbox: hazard/resources/return-to-camp traversal hints;
- first-person grid dungeon: corridor/room/route orientation hints.

Do not fork core traversal logic per family.

### 4. Infinite/chunked smoke pre-proof

Create deterministic smoke evidence that proves the chunked path can extend beyond finite maps without requiring infinite streaming implementation yet.

This may be a bounded infinite-window proof:

- seed id;
- window origin;
- fixed radius/window dimensions;
- deterministic chunk id derivation;
- boundary handoff placeholders;
- repeatable hash;
- invalid/fake/leak rejection.

Do not implement real infinite streaming or Runtime chunk loading.

### 5. Preview/export consumption proof

Produce a bounded consumer manifest that a future Runtime Preview/Unity/export route can use.

If existing Application preview/export acceptance services can be consumed without touching forbidden source, reuse their patterns. If not, create a contract-bound preview/export payload and mark concrete integration as future-required, with a causal blocked/future-required diagnostic.

A GREEN result must still prove a real consumer payload path from Goal 039 deltas. It must not merely copy Goal 039 JSON.

### 6. Package immutability audit

Prove that consumer payload generation did not mutate:

- GamePackage definitions;
- public package schema;
- Runtime state source contracts;
- Unity entrypoints;
- WinForms UI.

### 7. Invalid/fake/leak matrix

Cover at least these cases:

- missing Goal 039 source evidence;
- fake scenario id;
- fake chunk id;
- traversal references Goal 038 static map but no Goal 039 runtime delta;
- family lens forks core schema;
- family lens missing required consumer needs;
- infinite window nondeterministic seed;
- boundary overflow/invalid window;
- package mutation attempt;
- Runtime/UI/Unity source mutation claim;
- provider/LLM/RAG claim;
- Lua execution claim;
- filesystem/network/process/reflection/thread/time/random/native interop claim;
- final prose-only payload;
- missing save-load/replay correlation;
- nondeterministic ordering.

Each case must produce stable causal diagnostic codes.

### 8. Evidence artifacts

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/
```

Required files:

```text
chunked-consumer-catalog-summary.json
chunked-preview-payload-frontier.json
chunked-preview-payload-gothic.json
chunked-preview-payload-caravan.json
chunked-preview-payload-metamodule.json
chunked-export-manifest.json
multi-family-world-scale-regression-matrix.json
infinite-chunked-world-smoke-proof.json
runtime-preview-consumption-proof.json
package-immutability-audit.json
invalid-chunked-consumer-diagnostics-matrix.json
chunked-runtime-preview-export-multifamily-smoke-report.md
```

Evidence requirements:

- deterministic ordering;
- no timestamps unless repository has deterministic convention;
- no absolute machine paths;
- no heavy logs/build output;
- JSON parseable;
- report contains exact gate:

```text
chunked_runtime_preview_export_multifamily_smoke_verification required
```

### 9. Docs/state updates

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Expected final state:

- Goal 039 accepted by user handoff with `runtime_chunk_delta_traversal_smoke_verification passed`;
- Goal 040 produced for review with `chunked_runtime_preview_export_multifamily_smoke_verification required`;
- Goal 041/042 intent may be recorded as absorbed into aggressive Goal 040 if implemented;
- Goal 043 not started;
- Goal 031/032 remain produced-for-review/not passed unless current docs already say otherwise.

## Tests

Add focused tests proving:

- source Goal 039 facts are consumed;
- consumer payloads are not copies of source JSON;
- four scenario payloads are generated;
- three family lenses reuse same core payload schema;
- infinite/chunk window proof is deterministic;
- preview/export manifest is stable and references scenario payloads;
- package immutability audit passes;
- invalid/fake/leak matrix is causal;
- evidence writer creates all required files;
- product smoke writes artifacts and report.

Suggested classes:

```text
ChunkedRuntimePreviewPayloadTests
ChunkedRuntimeExportManifestTests
MultiFamilyWorldScaleRegressionTests
InfiniteChunkedWorldSmokeTests
ChunkedRuntimePreviewExportEvidenceTests
ChunkedRuntimePreviewExportInvalidMatrixTests
```

Product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/ChunkedRuntimePreviewExportSmokeProductSmokeTests.cs
```

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~ChunkedRuntimePreviewExport|FullyQualifiedName~ChunkedRuntime|FullyQualifiedName~Goal040"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~ChunkedRuntimePreviewExportSmokeProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal040|FullyQualifiedName~ChunkedRuntime"

.\.devflow\scripts\check-all.ps1
```

Then run existing artifact scope guard if available in current flow. Use the existing command/pattern. Do not invent a new scope system.

Also run direct artifact inspection:

```powershell
Get-ChildItem .\.llmgc\procedural\goal-040-chunked-runtime-preview-export-multifamily-smoke -File | Sort-Object Name | Select-Object Name,Length
Get-Content .\.llmgc\procedural\goal-040-chunked-runtime-preview-export-multifamily-smoke\chunked-runtime-preview-export-multifamily-smoke-report.md -TotalCount 120
```

## Bounded repairs

Pre-authorized, only if needed:

1. Update stale current-state/handoff guard tests so they read current gate from `CURRENT_GENERATOR_STATE.json` instead of hardcoding previous latest goal.
2. Restore exact accidental historical artifacts mutated by `check-all.ps1` using:

```powershell
git restore --source=HEAD -- <exact accidental historical artifact paths>
```

3. If historical ignored logs are required by unrelated tests and a real existing cache contains them, copy exact source -> target with report. Do not fabricate logs.
4. Update artifact scope policy only to include Goal 040 artifact folder if required.

If any bounded repair is used, report exact files and reason.

## Git policy

Allowed inspection/status commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit files>
git diff --stat --cached
```

Allowed final commands:

```text
git add <explicit changed files>
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

Final commit/push is mandatory even for BLOCKED/FAILED, unless there are literally no file changes. If no changes, report why no commit was possible.

## Final report format

Report in Russian:

```text
Goal 040 выполнен / заблокирован / провален

Status:
GREEN / BLOCKED / FAILED

Gate:
chunked_runtime_preview_export_multifamily_smoke_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<consumer seam / preview payload / export manifest / multi-family regression / infinite smoke / immutability audit / invalid matrix>

Evidence artifacts:
<список>

Проверки:
<commands and results>

Bounded repairs:
<none or exact list>

Git:
<commit hash and push result>

Ограничения:
<forbidden areas not touched>

Следующий разумный шаг:
<Goal 043 or repair recommendation>
```
