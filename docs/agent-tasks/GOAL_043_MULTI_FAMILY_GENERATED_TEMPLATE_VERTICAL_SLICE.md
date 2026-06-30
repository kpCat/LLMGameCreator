# Codex task — Goal 043 Multi-Family Generated Template Vertical Slice

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
goal-043-multi-family-generated-template-vertical-slice
Goal 043: Multi-Family Generated Template Vertical Slice
```

Codex reasoning level:

```text
very high
```

Gate:

```text
multi_family_generated_template_vertical_slice_verification required
```

## Status policy

This is an aggressive composite goal. It intentionally absorbs the next family-track work:

- Goal 043: Family 1 - Map And Panel RPG Template.
- Goal 044: Family 2 - Survival Sandbox Template.
- Goal 045: Family 3 - First-Person Grid Dungeon Template.
- Goal 046: Multi-Family Capability Regression.

Do not create separate goals for 044-046 in this task. Treat them as absorbed into Goal 043 evidence if implemented.

## Mandatory preflight

1. Work from `C:\Users\endim\LLMGameCreator\`.
2. Confirm branch is `main`.
3. Read current state/queue docs.
4. Confirm Goal 040 is present as produced-for-review with `chunked_runtime_preview_export_multifamily_smoke_verification required`.
5. Record acceptance of Goal 040 by user handoff before implementing Goal 043:

```text
chunked_runtime_preview_export_multifamily_smoke_verification passed
```

6. Do not start by creating documentation-only updates; the goal must implement the multi-family generated template vertical slice.

## Read-first list

Read these first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_043_MULTI_FAMILY_GENERATED_TEMPLATE_VERTICAL_SLICE_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_043_MULTI_FAMILY_GENERATED_TEMPLATE_VERTICAL_SLICE.md`
8. Goal 040 artifacts:
   - `.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/`
9. Goal 039 artifacts:
   - `.llmgc/procedural/goal-039-runtime-chunk-delta-traversal-smoke/`
10. Goal 038 artifacts:
    - `.llmgc/procedural/goal-038-world-scale-region-map-foundation/`
11. Goal 037 artifacts:
    - `.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/`
12. Existing Application seams:
    - `src/LLMGameCreator.Application/Design/ChunkedRuntimePreviewExportSmoke/**`
    - `src/LLMGameCreator.Application/Design/RuntimeChunkDeltaTraversal/**`
    - `src/LLMGameCreator.Application/Design/WorldScaleRegionMapFoundation/**`
    - `src/LLMGameCreator.Application/Design/HybridDraftLuaExpansion/**`
13. Existing product smoke patterns under:
    - `tests/LLMGameCreator.Tests/ProductSmoke/`
14. Existing artifact scope policy:
    - `.devflow/artifact-scope/artifact-scope-policy.json`

## Allowed files / areas

You may create or edit only:

```text
docs/GOAL_043_MULTI_FAMILY_GENERATED_TEMPLATE_VERTICAL_SLICE_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_043_MULTI_FAMILY_GENERATED_TEMPLATE_VERTICAL_SLICE.md
docs/agent-tasks/GOAL_043_MULTI_FAMILY_GENERATED_TEMPLATE_VERTICAL_SLICE.md
docs/agent-tasks/GOAL_043_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/MultiFamilyGeneratedTemplateVerticalSlice/**
tests/LLMGameCreator.Tests/Application/MultiFamilyGeneratedTemplateVerticalSlice/**
tests/LLMGameCreator.Tests/ProductSmoke/MultiFamilyGeneratedTemplateVerticalSliceProductSmokeTests.cs
.llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/**
```

If an existing local naming convention suggests a shorter folder/class prefix, use it only if it remains clearly Goal 043-specific.

## Forbidden files / areas

Do not modify:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Generation/** provider/LLM/RAG paths
unity/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
*.Designer.cs
```

Do not add external dependencies.

Do not call LLM/provider/RAG/media tools.

Do not execute arbitrary Lua or generate Lua source. You may only consume existing Goal 037 evidence/output references as data.

Do not mutate public GamePackage schema.

## Exact behavior

### 1. Create Application seam

Create a new Application-layer seam under:

```text
src/LLMGameCreator.Application/Design/MultiFamilyGeneratedTemplateVerticalSlice/
```

Recommended small components:

- `MultiFamilyGeneratedTemplateModels`
- `MultiFamilyGeneratedTemplateCatalog`
- `MultiFamilyLifecycleBuilder`
- `FamilySimulatableLoopRunner`
- `MultiFamilyGeneratedTemplateValidator`
- `MultiFamilyGeneratedTemplateEvidenceService`

Do not create a single huge monolithic class.

### 2. Consume previous goal artifacts as evidence inputs

Load or model source references to Goal 037-040 artifacts. Prefer deterministic compact input readers or stable in-memory fixtures derived from artifacts. Avoid heavy logs.

The implementation must prove that Goal 043 is downstream of:

- Goal 037 hybrid Lua deterministic expansion;
- Goal 038 world/region/map foundation;
- Goal 039 runtime chunk delta traversal;
- Goal 040 preview/export multi-family consumer payload.

### 3. Generate three family lifecycle plans

Create deterministic family lifecycle plans for:

```text
map_panel_rpg
survival_sandbox
first_person_grid_dungeon
```

Each plan must include:

- family id;
- scenario/profile id;
- selected feature/intention references;
- region/chunk/traversal source refs;
- preview/export consumer refs;
- family-specific extension section;
- loop commands/events;
- validation trace;
- deterministic ordering key.

### 4. Prove simulatable loop per family

Implement a lightweight Application-owned simulatable loop runner, not a Runtime source change.

For each family, prove a minimal but real state transition loop:

- initial state;
- ordered commands/events;
- after state;
- changed markers;
- replay determinism hash;
- blocked invalid action if applicable.

Family-specific minimums:

#### map_panel_rpg

- movement/traversal marker;
- focused NPC/encounter/item/quest target;
- quest/event progress marker.

#### survival_sandbox

- hazard/resource observation;
- collect/consume/craft/survival transition;
- state changes due to chunk context.

#### first_person_grid_dungeon

- orientation or corridor/room marker;
- encounter/progression/locked-route pressure;
- party/blob-like traversal marker.

### 5. Prove shared lifecycle contract

Write a shared lifecycle contract model proving that all families use the same phases and only family-scoped extension sections differ.

Reject architecture forks.

### 6. Evidence writer

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/
```

Required files:

```text
family-template-catalog.json
shared-lifecycle-contract.json
family-loop-plan-map-panel-rpg.json
family-loop-plan-survival-sandbox.json
family-loop-plan-first-person-grid-dungeon.json
family-simulatable-loop-proof-map-panel-rpg.json
family-simulatable-loop-proof-survival-sandbox.json
family-simulatable-loop-proof-first-person-grid-dungeon.json
multi-family-regression-matrix.json
preview-export-consumption-matrix.json
invalid-family-diagnostics-matrix.json
multi-family-generated-template-vertical-slice-report.md
```

Report must include:

```text
implementationStatus=GREEN|BLOCKED|FAILED
accepted=false
manualGate=multi_family_generated_template_vertical_slice_verification
familyCount=3
simulatableLoopProofCount=3
sourceGoal040PreviewExportConsumed=true
sharedLifecycleContractPassed=true
invalidMatrixPassed=true
```

### 7. Update artifact scope policy

Add the Goal 043 artifact path to existing artifact scope policy using the local pattern. Do not loosen unrelated paths.

### 8. Update current-state docs

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Expected state:

- Goal 040 recorded as accepted by user handoff:
  - `chunked_runtime_preview_export_multifamily_smoke_verification passed`
- Goal 043 produced for review:
  - `multi_family_generated_template_vertical_slice_verification required`
- Goal 044/045/046 intent recorded as absorbed into aggressive Goal 043 if fully proven.
- Goal 047 or next appropriate family/runtime integration task recommended but not started.
- Preserve Goal 031/032 produced-for-review/not passed if current docs still keep that model.

## Tests

Add focused tests covering:

- family template catalog validity;
- three family lifecycle plans;
- three simulatable loop proofs;
- shared lifecycle contract;
- preview/export consumption matrix;
- invalid/fake/leak matrix;
- evidence artifact writing and deterministic parse;
- product smoke route.

Suggested test classes:

```text
MultiFamilyTemplateCatalogTests
MultiFamilyLifecycleBuilderTests
FamilySimulatableLoopRunnerTests
MultiFamilyRegressionTests
MultiFamilyGeneratedTemplateEvidenceTests
MultiFamilyGeneratedTemplateInvalidMatrixTests
MultiFamilyGeneratedTemplateVerticalSliceProductSmokeTests
```

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~MultiFamilyGeneratedTemplate|FullyQualifiedName~Goal043"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~MultiFamilyGeneratedTemplateVerticalSliceProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal043|FullyQualifiedName~MultiFamily"

.\.devflow\scripts\check-all.ps1
```

Run the existing artifact scope guard for Goal 043 using the repository's established pattern. Do not invent a new unrelated guard.

Also inspect the evidence directory and parse JSON artifacts if local tests do not already prove that.

## Pre-authorized bounded repairs

To reduce unnecessary user handoffs, you may perform these bounded repairs if needed:

1. Update stale current-state/handoff tests that hardcode the previous latest gate, but only to preserve historical assertions and check current-state consistency.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates unrelated tracked artifacts.
3. Add the Goal 043 artifact path to artifact scope policy.
4. If focused tests reveal a small missing current-state assertion under existing test files, update only the narrow affected test.

Every bounded repair must be reported.

## Stop / BLOCKED conditions

Commit and push even if blocked, but report BLOCKED if:

- You must change public GamePackage schema.
- You must change Runtime/Runtime.Abstractions/WinForms/Unity/provider/LLM/RAG/media/Lua executor/generator-library.
- You need an external dependency.
- You cannot prove three family simulatable loops.
- Evidence is only reports without state transitions.
- `check-all.ps1` fails and cannot be repaired within allowed bounded repair scope.
- Artifact scope guard fails and cannot be repaired within allowed bounded repair scope.

## Git policy

Codex must commit and push final state to `origin/main` regardless of GREEN/BLOCKED/FAILED result.

Allowed git inspection commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit changed paths>
git diff --stat --cached
```

Allowed final commands:

```text
git add <explicit Goal 043 allowed paths and bounded repair paths>
git commit -m "<status> Goal 043 multi-family generated template vertical slice"
git push origin main
```

Commit messages:

```text
GREEN Goal 043 multi-family generated template vertical slice
BLOCKED Goal 043 multi-family generated template vertical slice
FAILED Goal 043 multi-family generated template vertical slice
```

Forbidden:

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

## Final report format

Report in Russian:

```text
Goal 043 выполнен / заблокирован / провален

Status:
GREEN / BLOCKED / FAILED

Gate:
multi_family_generated_template_vertical_slice_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<catalog/lifecycle/simulatable loops/regression/evidence>

Family proofs:
map_panel_rpg: <summary>
survival_sandbox: <summary>
first_person_grid_dungeon: <summary>

Evidence artifacts:
<список>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<summary>

Bounded repairs:
<none or list>

Git:
<commit hash and push result>

Ограничения:
<forbidden areas not touched>

Следующий разумный шаг:
<next goal recommendation>
```
