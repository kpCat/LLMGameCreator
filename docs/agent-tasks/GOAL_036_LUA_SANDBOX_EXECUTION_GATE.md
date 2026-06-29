# Codex task — GOAL 036 Lua Sandbox Execution Gate

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
goal-036-lua-sandbox-execution-gate
Goal 036: Lua Sandbox Execution Gate
```

Required goal marker / manual gate:

```text
lua_sandbox_execution_gate_verification
```

Codex reasoning level:

```text
very high
```

## Critical workflow rule: commit/push final state

At the end of this task, commit and push the final state to `origin/main` regardless of result.

Use honest status:

```text
GREEN Goal 036 lua sandbox execution gate
BLOCKED Goal 036 lua sandbox execution gate
FAILED Goal 036 lua sandbox execution gate
```

Commit message rules:

- If implementation and checks are green: `Goal 036 lua sandbox execution gate`
- If useful work exists but final gate is blocked: `BLOCKED Goal 036 lua sandbox execution gate`
- If implementation cannot be completed but diagnostics/evidence changed files: `FAILED Goal 036 lua sandbox execution gate`

Do not pretend non-green work is accepted. Do not mark `lua_sandbox_execution_gate_verification` passed. The final state should be reviewable from GitHub.

If no implementation files can be changed before a blocker, create a compact blocked report under `.llmgc/procedural/goal-036-lua-sandbox-execution-gate/blocked-report.md`, update state docs honestly if appropriate, then commit/push the BLOCKED state.

## Purpose

Add a deterministic Application-layer Lua sandbox execution gate.

Goal 035 created a Lua module manifest registry, host API policy and dependency planning. Goal 036 must add the next safety layer: execution request records, sandbox budget policy, host binding matrix, dry-run/probe plan records, deny-first decision engine, repair plans and evidence.

This goal is an execution **gate**, not a Lua executor.

## Hard boundary

Do not execute Lua.
Do not parse Lua.
Do not generate Lua source.
Do not add interpreter dependencies.
Do not touch Runtime, UI, Unity, GamePackage schema, provider/LLM/RAG, media, Lua generator-library or project files.

The gate may say a manifest is `ready_for_future_executor`, `dry_run_only`, `needs_repair`, `blocked_no_executor`, or `rejected`, but it must not claim real Lua execution happened.

## Read-first list

Read in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_036_LUA_SANDBOX_EXECUTION_GATE_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_036_LUA_SANDBOX_EXECUTION_GATE.md`
8. `docs/GOAL_035_LUA_MODULE_MANIFEST_REGISTRY_SPEC.md` if present
9. `docs/EXTERNAL_SCOUTING_GOAL_035_LUA_MODULE_MANIFEST_REGISTRY.md` if present
10. `src/LLMGameCreator.Application/Design/LuaModuleManifestRegistry/**`
11. `tests/LLMGameCreator.Tests/Application/LuaModuleManifestRegistry/**`
12. `tests/LLMGameCreator.Tests/ProductSmoke/LuaModuleManifestRegistryProductSmokeTests.cs`
13. Existing current-state guard tests that were modernized around Goals 033-035 if they fail.

Do not broad-scan the whole repo unless narrow search proves required code is elsewhere.

## Allowed files / areas

You may create/edit:

```text
docs/GOAL_036_LUA_SANDBOX_EXECUTION_GATE_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_036_LUA_SANDBOX_EXECUTION_GATE.md
docs/agent-tasks/GOAL_036_LUA_SANDBOX_EXECUTION_GATE.md
docs/agent-tasks/GOAL_036_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
src/LLMGameCreator.Application/Design/LuaSandboxExecutionGate/**
tests/LLMGameCreator.Tests/Application/LuaSandboxExecutionGate/**
tests/LLMGameCreator.Tests/ProductSmoke/LuaSandboxExecutionGateProductSmokeTests.cs
.llmgc/procedural/goal-036-lua-sandbox-execution-gate/**
```

You may read from but should not modify Goal 035 registry files unless there is a tiny compile-time integration issue that is impossible to solve otherwise. If that happens, keep the edit narrow and explain it.

## Forbidden files / areas

Forbidden unless the final report is BLOCKED and asks the user for a separate explicit decision:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG/media paths
src/LLMGameCreator.Scripting/** unless already Application-only and read-only inspection proves it is only metadata
unity/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
*.Designer.cs
```

Also forbidden:

- external NuGet/package dependency;
- real Lua source parsing;
- real Lua execution;
- generated Lua source;
- weakening existing tests/evidence tests;
- changing public GamePackage schema;
- broad refactors.

## Exact behavior

### 1. Preflight

- Confirm branch `main`.
- Confirm current state has Goal 035 accepted and Goal 036 recommended/not started. If docs are slightly stale but GitHub/user handoff clearly accepted Goal 035, fix docs only inside this goal as bounded preflight and report it.
- Confirm no unexpected dirty worktree except task input files from this archive.

### 2. Sandbox policy model

Create Application-layer BCL-only models under `Design/LuaSandboxExecutionGate`.

Represent at least:

- execution request id;
- scenario id;
- selected manifest ids from Goal 035;
- requested host API groups;
- denied host API groups;
- budget: instruction limit, memory budget, output/event budget, deterministic step budget;
- determinism flags: no time, no random, no network, no filesystem, no reflection, no threads;
- provenance: manual/import/llm-draft/promoted-from-Goal034;
- dry-run/probe plan records;
- expected trace event families;
- decision status;
- diagnostics.

### 3. Host binding matrix

Build a deterministic host binding matrix that maps Goal 035 host API groups to sandbox binding decisions:

- allowed in dry-run;
- allowed only for future executor;
- denied;
- needs explicit adapter;
- blocked by boundary.

At minimum deny:

```text
file_system
network
process
reflection
threading
time
random
ui
unity
runtime_mutation
gamepackage_schema_mutation
provider_llm
rag
media_generation
native_interop
```

### 4. Decision engine

Implement a deny-first engine that takes Goal 035 selected manifests and a sandbox request and returns deterministic decisions.

Required statuses:

```text
ready_for_future_executor
dry_run_only
needs_repair
blocked_no_executor
rejected
```

Rules:

- A valid manifest with allowed host API groups and no executor dependency may be `dry_run_only` or `ready_for_future_executor`, but not executed.
- If a manifest requires denied host APIs, reject or needs-repair with causal diagnostics.
- If a request includes source text, parser claims, runtime claims or final prose, reject.
- If a request tries to promote itself without Goal 034 promotion trace, reject.
- If budgets are missing or over limits, needs-repair/reject according to severity.
- If dependency order is unstable or references fake manifests, reject.

### 5. Dry-run/probe plan

Create a deterministic dry-run/probe plan model. It may simulate only predefined probe records such as:

```text
validate_manifest_selection
validate_host_bindings
validate_budget
validate_dependency_order
validate_expected_outputs
```

These are not Lua statements and not source execution.

The trace must explicitly state `lua_executed=false` or equivalent.

### 6. Repair planner

Create a repair planner for invalid sandbox requests:

- remove denied host API group;
- reduce budget;
- add missing Goal 034 promotion trace;
- replace fake manifest id;
- split overlarge request;
- mark future executor adapter required.

Repair plan must not mutate immutable accepted manifests.

### 7. Evidence writer

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-036-lua-sandbox-execution-gate/
```

Required files:

```text
lua-sandbox-policy-summary.json
lua-host-binding-matrix.json
lua-sandbox-execution-requests.json
lua-sandbox-decision-frontier.json
lua-sandbox-decision-gothic.json
lua-sandbox-decision-caravan.json
lua-sandbox-decision-metamodule.json
lua-sandbox-dry-run-trace-matrix.json
lua-sandbox-repair-plan-matrix.json
invalid-lua-sandbox-diagnostics-matrix.json
lua-sandbox-execution-gate-report.md
```

Report must contain:

```text
lua_sandbox_execution_gate_verification required
luaExecuted=false
```

### 8. Scenario coverage

Cover:

- `frontier_survival`
- `gothic_intrigue`
- `caravan_trade`
- `metamodule_kingdoms`

Metamodule scenario must show scale from Goal 035, ideally 112 species/archetype slot manifest selections or a directly derived count.

### 9. Invalid/fake/leak matrix

Include deterministic negative cases for:

- fake manifest id;
- unknown host API group;
- denied host API group;
- missing budget;
- over budget;
- unstable dependency order;
- source text included;
- parser claim included;
- lua execution claim included;
- final prose included;
- self promotion;
- missing Goal 034 promotion trace;
- provider/LLM/RAG leak;
- Runtime/UI/Unity/GamePackage schema mutation leak;
- filesystem/network/process/reflection/thread/time/random/native interop leak;
- immutable repair mutation;
- nondeterministic ordering.

Every case must have causal diagnostic code or deterministic rejected/needs-repair status.

### 10. Docs/current-state update

Update docs consistently:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Expected final docs:

- Goal 035 accepted/passed;
- Goal 036 produced for review;
- active/manual gate `lua_sandbox_execution_gate_verification required`;
- Goal 037 only recommended/not started;
- Goal 031/032 preserved as produced-for-review/not passed if current docs do so.

Do not mark Goal 036 passed.

## Tests

Add focused tests following local style. Suggested names:

```text
LuaSandboxExecutionPolicyTests
LuaSandboxHostBindingTests
LuaSandboxDecisionEngineTests
LuaSandboxRepairPlannerTests
LuaSandboxEvidenceTests
LuaSandboxExecutionGateProductSmokeTests
```

Must prove:

- policy seed validates cleanly;
- host denied groups are rejected;
- Goal 035 manifest selections integrate deterministically;
- four scenarios produce meaningfully different decisions;
- metamodule scale remains visible;
- dry-run traces have `luaExecuted=false`;
- repair plans are deterministic and do not mutate accepted manifests;
- invalid/fake/leak matrix returns causal diagnostics;
- evidence artifacts exist, parse and contain required markers;
- no external dependency/runtime/UI/GamePackage/provider/LLM/RAG/Lua-source behavior is required.

## Validation commands

Run focused checks first:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~LuaSandboxExecutionGate|FullyQualifiedName~LuaSandbox|FullyQualifiedName~Goal036"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~LuaSandboxExecutionGateProductSmokeTests"
```

Direct artifact inspection:

```powershell
Get-ChildItem .\.llmgc\procedural\goal-036-lua-sandbox-execution-gate -File | Sort-Object Name | Select-Object Name,Length
Get-Content .\.llmgc\procedural\goal-036-lua-sandbox-execution-gate\lua-sandbox-execution-gate-report.md -TotalCount 120
```

Then full gate:

```powershell
.\.devflow\scripts\check-all.ps1
```

Run the existing final artifact scope guard pattern used by Goal 035. Do not invent a new policy/script. If the exact invocation is not obvious, inspect recent docs/reports/scripts and use the same existing mechanism.

## Pre-authorized bounded repairs

These are allowed to reduce manual blocking:

1. Update stale current-state/handoff guard tests only if they hardcode the previous latest gate and fail because Goal 036 docs are correct. Keep historical assertions strict and make only current-state consistency dynamic.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates unrelated tracked evidence.
3. Copy exact historical runtime/Unity logs only from existing real generated cache to exact expected historical log paths if check-all requires them and they are missing; do not fabricate content; report source -> target.
4. Refresh generated artifact inventory only if the existing artifact scope policy requires it and the change is scoped to Goal 036 evidence.
5. Include these repairs in the same final commit with clear report, instead of asking the user for a separate unblock, unless the repair would touch forbidden code areas or change semantics.

Still forbidden:

```text
git checkout
git reset
git clean
git stash
git merge
git rebase
git cherry-pick
git push --force
```

`git restore --source=HEAD -- <exact accidental historical artifact paths>` is allowed only for bounded historical artifact cleanup, not for Goal 036 implementation/evidence.

## Git policy

Allowed inspection:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <exact changed files>
git diff --stat --cached
```

Final commit/push is mandatory.

GREEN:

```powershell
git add <all Goal036 code/tests/docs/evidence plus allowed bounded repairs>
git commit -m "Goal 036 lua sandbox execution gate"
git push origin main
```

BLOCKED:

```powershell
git add <all changed files that document/implement the blocked state>
git commit -m "BLOCKED Goal 036 lua sandbox execution gate"
git push origin main
```

FAILED:

```powershell
git add <diagnostic evidence / partial files worth preserving>
git commit -m "FAILED Goal 036 lua sandbox execution gate"
git push origin main
```

## Final report format

Report in Russian:

```text
Goal 036 выполнен / заблокирован / провален

Status:
GREEN / BLOCKED / FAILED

Gate:
lua_sandbox_execution_gate_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<policy / host binding / decision engine / dry-run trace / repair planner / evidence>

Evidence artifacts:
<список files>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<covered cases>

Bounded repairs:
<none or exact details>

Git:
<commit hash and push result>
<Committed despite non-green result: yes/no>

Ограничения:
No real Lua execution/parser/source generation, no external dependencies, no GamePackage/UI/Runtime/Unity/provider/LLM/RAG/generator-library changes.

Следующий разумный шаг:
<Goal 037 recommendation; no implementation started>
```
