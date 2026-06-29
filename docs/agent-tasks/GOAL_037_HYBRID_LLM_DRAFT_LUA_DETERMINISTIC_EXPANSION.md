# Codex Task — Goal 037 Hybrid LLM Draft Plus Lua Deterministic Expansion

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
goal_037_hybrid_llm_draft_plus_lua_deterministic_expansion
Goal 037: Hybrid LLM Draft Plus Lua Deterministic Expansion
```

Required goal marker / gate:

```text
hybrid_llm_draft_lua_deterministic_expansion_verification required
```

Codex reasoning level:

```text
very high
```

## Mandatory process change

This task is intentionally aggressive. Work longer rather than stopping for routine handoffs.

You must commit and push the final state to `origin/main` even if the result is `GREEN`, `BLOCKED`, or `FAILED`.

Use an honest commit message:

```text
Goal 037 hybrid LLM draft plus Lua deterministic expansion
BLOCKED Goal 037 hybrid LLM draft plus Lua deterministic expansion
FAILED Goal 037 hybrid LLM draft plus Lua deterministic expansion
```

Do not pretend a blocked/failed result is green. Do not mark the Goal 037 gate passed inside this task.

## Preflight acceptance embedded in this goal

The user reported Goal 036 as GREEN, pushed and verified with `check-all.ps1` 978/978 and artifact scope guard 12/12 allowed. The user explicitly requested fewer acceptance-only tasks and wants the next large goal to proceed.

Therefore, as a preflight doc update inside this Goal 037 task, record Goal 036 manual/user acceptance:

```text
lua_sandbox_execution_gate_verification passed
```

This is not a separate acceptance-only task. It is part of Goal 037 preflight.

If local docs already record Goal 036 as passed, do not duplicate it.

## Read-first list

Read in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_037_HYBRID_LLM_DRAFT_LUA_DETERMINISTIC_EXPANSION_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_037_HYBRID_LLM_DRAFT_LUA_DETERMINISTIC_EXPANSION.md`
8. Goal 034 implementation/evidence:
   - `src/LLMGameCreator.Application/Design/StrictLlmDraftArtifactLoop/**`
   - `tests/LLMGameCreator.Tests/Application/StrictLlmDraftArtifactLoop/**`
   - `.llmgc/procedural/goal-034-strict-llm-draft-artifact-loop/**`
9. Goal 035 implementation/evidence:
   - `src/LLMGameCreator.Application/Design/LuaModuleManifestRegistry/**`
   - `tests/LLMGameCreator.Tests/Application/LuaModuleManifestRegistry/**`
   - `.llmgc/procedural/goal-035-lua-module-manifest-registry/**`
10. Goal 036 implementation/evidence:
    - `src/LLMGameCreator.Application/Design/LuaSandboxExecutionGate/**`
    - `tests/LLMGameCreator.Tests/Application/LuaSandboxExecutionGate/**`
    - `.llmgc/procedural/goal-036-lua-sandbox-execution-gate/**`
11. Existing test patterns for current-state docs, product smoke and artifact scope guard.

Do not read the whole repository unless local search shows a required pattern is elsewhere.

## Allowed files / areas

You may create/edit:

```text
docs/GOAL_037_HYBRID_LLM_DRAFT_LUA_DETERMINISTIC_EXPANSION_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_037_HYBRID_LLM_DRAFT_LUA_DETERMINISTIC_EXPANSION.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
src/LLMGameCreator.Application/Design/HybridDraftLuaExpansion/**
tests/LLMGameCreator.Tests/Application/HybridDraftLuaExpansion/**
tests/LLMGameCreator.Tests/ProductSmoke/HybridDraftLuaExpansionProductSmokeTests.cs
.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/**
```

Dependency adoption is allowed only if required for real bounded Lua execution:

```text
src/LLMGameCreator.Application/LLMGameCreator.Application.csproj
```

Only one new package may be added, preferably `LuaCSharp`, pinned to an exact version that restores/builds locally. Do not add source generator packages unless absolutely required; prefer runtime package only.

## Forbidden files / areas

Do not modify unless explicitly required by the bounded dependency adoption above:

```text
*.sln
other *.csproj
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG paths
src/LLMGameCreator.Scripting/** unless it already owns a strict existing Lua seam and you stop/report first
generator-library/**
unity/**
samples/**
templates/**
public GamePackage schema/model files
```

Do not call provider/LLM/RAG. Do not generate final prose. Do not execute arbitrary user Lua. Do not expose filesystem/network/process/reflection/threading/wall-clock/random/native interop.

## Exact behavior

### 1. Preflight state update

Record Goal 036 accepted/passed in current-state docs based on the user handoff. Keep Goal 031/032 produced-for-review/not passed if current docs say so. Do not mark Goal 037 passed.

### 2. Dependency selection and executor adapter

Create a small Application-layer executor abstraction under `HybridDraftLuaExpansion`.

Required concepts:

- executor adapter selection record;
- selected package id/version/license/risk notes;
- adapter capability flags;
- execution request id;
- sandbox decision id from Goal 036;
- source category: repo-owned fixture only;
- deterministic output contract;
- output validation trace;
- failure/blocker reason.

If adopting `LuaCSharp` succeeds, implement a real bounded executor adapter that runs only repo-owned deterministic expansion fixtures. If adopting a dependency fails or safe API isolation cannot be proven, produce a `BLOCKED` state with evidence and do not fake execution.

### 3. Hybrid pipeline model

Model the pipeline:

```text
Goal034 draft request/candidate
 -> Goal035 Lua manifest selection
 -> Goal036 sandbox gate decision
 -> bounded Lua expansion request
 -> executor adapter result
 -> C# output validator
 -> promotion decision
```

No live LLM call. Simulate draft input only through existing Goal 034 artifacts/catalogs or deterministic fixtures derived from them.

### 4. Expansion output model

Output must be structured IR, not final prose:

- stable id;
- scenario id;
- source draft request id;
- source manifest id;
- sandbox decision id;
- produced artifact family;
- generated deterministic slots/tags/weights/relations;
- diagnostics;
- promotion status: accepted/rejected/repair_required/blocked;
- trace hash or structural trace summary.

Required families:

- NPC/species/archetype expansion hints;
- region/faction/kingdom expansion hints;
- quest/event intent expansion hints;
- economy/combat/settlement expansion hints;
- metamodule species/archetype slot expansion.

### 5. Determinism and budget enforcement

The implementation must prove:

- same input produces structurally identical output;
- ordered output is stable;
- output count budgets are enforced;
- invalid/fake/leak attempts are rejected causally;
- Lua execution, if implemented, is constrained by the Goal036 gate and this goal’s adapter.

If true instruction-count/time cancellation is not supported by the chosen interpreter, restrict accepted scripts to declarative/deterministic fixtures and record this as a limitation in the adapter selection evidence. Do not allow arbitrary loops.

### 6. Evidence writer

Write compact evidence under:

```text
.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/
```

Required files:

```text
executor-adapter-selection.json
hybrid-pipeline-summary.json
draft-to-lua-request-map.json
sandbox-approved-expansion-matrix.json
lua-expansion-output-frontier.json
lua-expansion-output-gothic.json
lua-expansion-output-caravan.json
lua-expansion-output-metamodule-kingdoms.json
promotion-decision-matrix.json
invalid-hybrid-expansion-diagnostics-matrix.json
hybrid-llm-draft-lua-deterministic-expansion-report.md
```

Report must contain:

```text
hybrid_llm_draft_lua_deterministic_expansion_verification required
```

If status is GREEN, report must prove at least one real bounded executor path, not just contracts. If not possible, status must be BLOCKED/FAILED.

### 7. Product smoke

Add a product smoke test route/class:

```text
tests/LLMGameCreator.Tests/ProductSmoke/HybridDraftLuaExpansionProductSmokeTests.cs
```

It must write/inspect all required Goal 037 artifacts and prove at least the four scenario outputs.

### 8. Invalid/fake/leak matrix

Cover at minimum:

- fake Goal034 draft id;
- fake Goal035 manifest id;
- fake Goal036 sandbox decision id;
- sandbox denied but executor attempted;
- wrong scenario/profile;
- final prose payload;
- GamePackage mutation claim;
- Runtime/UI/Unity/provider/LLM/RAG/Lua source generation leakage;
- filesystem/network/process/reflection/thread/time/random/native interop request;
- over-budget output;
- nondeterministic output order;
- missing trace;
- self-promotion;
- dependency unavailable / unsafe adapter blocker path;
- malformed executor output.

## Bounded repairs pre-authorized

To reduce handoff stalls, you may do these bounded repairs if they are directly caused by this task:

1. Update stale current-state/handoff guard tests that hardcode the previous latest gate, preserving strict historical assertions and replacing only latest-gate brittleness with current-state consistency.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates them outside Goal 037 scope.
3. Copy exact historical runtime/Unity logs only from an existing real generated cache to an exact expected historical evidence path if check-all requires them and the content contains expected markers. Report source -> target.
4. Refresh artifact scope policy only if the standard final guard rejects Goal 037 files solely because the new goal folder/test file is missing from the allowlist. Keep the allowlist narrow.

Do not use broad repairs. Do not hide failures.

## Tests

Suggested focused tests:

```text
HybridDraftLuaExpansionCatalogTests
HybridDraftLuaExecutorAdapterTests
HybridDraftLuaPipelinePlannerTests
HybridDraftLuaOutputValidatorTests
HybridDraftLuaPromotionDecisionTests
HybridDraftLuaEvidenceTests
HybridDraftLuaInvalidMatrixTests
HybridDraftLuaExpansionProductSmokeTests
```

Tests must prove GREEN/BLOCKED honestly. If real executor path is not implemented, tests must not pretend it is.

## Validation commands

Run focused checks first:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~HybridDraftLuaExpansion|FullyQualifiedName~Goal037"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~HybridDraftLuaExpansionProductSmokeTests"
```

Then final:

```powershell
.\.devflow\scripts\check-all.ps1
```

Run the existing final artifact scope guard if the repo has a standard command/policy for it. Do not invent a new unrelated guard.

Also inspect artifacts directly:

```powershell
Get-ChildItem .\.llmgc\procedural\goal-037-hybrid-llm-draft-lua-deterministic-expansion -File | Sort-Object Name | Select-Object Name,Length
Get-Content .\.llmgc\procedural\goal-037-hybrid-llm-draft-lua-deterministic-expansion\hybrid-llm-draft-lua-deterministic-expansion-report.md -TotalCount 100
```

## Git policy

Allowed inspection:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <changed files>
git diff --stat --cached
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

Mandatory final commit/push:

If GREEN:

```powershell
git add <changed files>
git commit -m "Goal 037 hybrid LLM draft plus Lua deterministic expansion"
git push origin main
```

If BLOCKED:

```powershell
git add <changed files>
git commit -m "BLOCKED Goal 037 hybrid LLM draft plus Lua deterministic expansion"
git push origin main
```

If FAILED:

```powershell
git add <changed files>
git commit -m "FAILED Goal 037 hybrid LLM draft plus Lua deterministic expansion"
git push origin main
```

If there are genuinely no file changes, report that explicitly, but this goal should normally leave state/evidence even when blocked.

## Final report format

Report in Russian:

```text
Goal 037 выполнен / заблокирован / провален
Status: GREEN / BLOCKED / FAILED
Gate: hybrid_llm_draft_lua_deterministic_expansion_verification required

Что стало реальнее:
<1-3 предложения>

Dependency/adaptor decision:
<selected package/version/license or no-dependency/blocker reason>

Изменённые файлы:
<list>

Реализовано:
<catalog/adapter/pipeline/output validator/promotion/evidence>

Evidence artifacts:
<list>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<covered cases>

Bounded repairs:
<none or exact repairs>

Git:
<commit hash and push result>

Ограничения:
<no GamePackage/UI/Runtime/Unity/provider/LLM/RAG/generator-library etc.>

Следующий разумный шаг:
<Goal 038 or hotfix>
```
