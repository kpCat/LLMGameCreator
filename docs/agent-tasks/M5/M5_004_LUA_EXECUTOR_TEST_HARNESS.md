# M5_004 — Lua executor test harness

This task spec is locked draft guidance. If implementation conflicts with current source, stop and report.

## Header

Task ID: `M5_004`

Milestone: `M5 Lua Module Executor Integration`

Status: `locked_until_M4_1_gate_passes`

Depends on:

```text
- M5_001 contract boundary exists or has been refreshed.
- M5_002/M5_003 assumptions are reviewed.
- docs/CURRENT_GENERATOR_STATE.md says M4.1 passed.
- docs/CURRENT_GENERATOR_STATE.json says M4.1 passed.
- User explicitly approves this M5 task in NEXT_TASK.md.
- check-all is green before starting.
```

Unlocks:

```text
- M5_005 request/result DTO alignment.
- M5_007 forbidden API golden fixtures.
```

Risk level: medium

Expected changed files count: 4-8

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M5_004 after M4.1 gate passed
```

## Source of truth

Source-of-truth docs:

```text
docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
docs/LUA_SCRIPTING.md
docs/SCRIPT_MANIFEST_SPEC.md
.devflow/MODELING_STRATEGY.md
docs/agent-tasks/M5/000_M5_SEQUENCE.md
docs/agent-tasks/_TEST_QUALITY_RULES.md
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
docs/agent-tasks/_DIFF_HYGIENE_RULES.md
```

Context budget:

```text
Read this spec, M5 sequence, the listed source docs, and only the existing patterns named below. Do not read all generator-library modules or unrelated phase specs.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Scripting/IPrototypeLuaExecutor.cs
src/LLMGameCreator.Scripting/PrototypeLuaExecutor.cs
src/LLMGameCreator.Scripting/PrototypeLuaStaticAnalyzer.cs
tests/LLMGameCreator.Tests/Scripting/PrototypeLuaExecutorTests.cs
tests/LLMGameCreator.Tests/Scripting/PrototypeLuaDeclarationMapperTests.cs
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Scripting/**
tests/LLMGameCreator.Tests/Scripting/**
tests/fixtures/lua_executor_harness/**
docs/agent-tasks/001_TASK_PACK_LEDGER.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.GamePackage/**
LLMGameCreator.sln
*.csproj unless the task is amended with explicit user approval
```

Deleted files: none.

## API / implementation contract

New dependencies: no.

Schema changed: no GamePackage schema changes.

Public contracts changed: only the small M5 contracts explicitly named by this task after M4.1 unlock.

GamePackage mutation rule: forbidden.

Runtime dependency rule: Runtime must not depend on Lua generator executor, manifests, or editor generation providers.

## Exact behavior

```text
- Provide a minimal test harness for invoking the existing prototype Lua execution path or the M5 executor contract.
- Harness uses fake/minimal source strings or fixtures only.
- Harness can represent safe success, static rejection, execution failure, and timeout/cancellation shape when supported.
- Harness never mutates GamePackage and never touches Runtime.
```

Failure behavior:

```text
Return deterministic diagnostics instead of throwing for expected invalid module/manifest/source/output cases.
Never hide sandbox or validation failures.
```



## Proof tests

Tests to add before/with implementation:

```text
- safe minimal Lua fixture passes through harness or contract fake with no diagnostics;
- forbidden source is rejected before execution with exact diagnostic code;
- invalid execution result is represented as diagnostic, not exception;
- cancellation/timeout is represented deterministically if current executor supports it;
- no GamePackage instance/reference is required by the harness.
```

Proof-test quality:

```text
- Assert exact diagnostic codes, not only Ok=false.
- Assert no GamePackage mutation when relevant.
- Assert Runtime project remains untouched conceptually in final report.
- Use fake/minimal Lua source and small fixtures; do not call real LLM/provider/network.
```

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Focused test command:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Lua"
```

Runtime scenario commands: not applicable unless this specific task explicitly adds a headless scenario after M8 unlock.

## Stop conditions

Stop if:

```text
- M4.1 gate is still active.
- task requires new NuGet/runtime dependency.
- task requires GamePackage schema change.
- task requires Runtime project changes.
- task requires WinForms UI wiring.
- task exceeds 8 changed files.
- proof tests cannot pin exact behavior.
- current source layout makes this draft stale.
```

## Non-goals

```text
- Do not implement M6 package assembly.
- Do not apply generated artifacts to GamePackage.
- Do not wire UI.
- Do not broaden Lua sandbox.
- Do not add new dependencies.
- Do not execute real generator-library modules unless this task is refreshed and explicitly allows it.
```

## Expected final report

Final report must include:

```text
- task id;
- changed files;
- contracts/classes/methods added or changed;
- exact diagnostic codes asserted by tests;
- proof tests added;
- confirmation no GamePackage mutation path was added;
- confirmation Runtime project was untouched;
- check-all result;
- next task suggestion.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M5_005
Task spec file: docs/agent-tasks/M5/M5_005_LUA_EXECUTION_REQUEST_RESULT_CONTRACTS.md
Reason: Align request/result DTOs and artifact envelope metadata before module binding.
User approval: required
Expected stop after completion: yes
```

On block, write BLOCKERS:

```text
M5_004 blocked: M4.1 gate not passed or current source layout makes this draft stale.
```
