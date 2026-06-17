# M5_007 — Forbidden API golden fixtures

This task spec is locked draft guidance. If implementation conflicts with current source, stop and report.

## Header

Task ID: `M5_007`

Milestone: `M5 Lua Module Executor Integration`

Status: `locked_until_M4_1_gate_passes`

Depends on:

```text
- M5_003 static sandbox policy exists or has been refreshed.
- M5_004 harness exists or is explicitly deferred.
- docs/CURRENT_GENERATOR_STATE.md says M4.1 passed.
- docs/CURRENT_GENERATOR_STATE.json says M4.1 passed.
- User explicitly approves this M5 task in NEXT_TASK.md.
- check-all is green before starting.
```

Unlocks:

```text
- M5_008 no GamePackage mutation guard.
- M5_009 one module family slice after sandbox evidence is stable.
```

Risk level: medium

Expected changed files count: 3-8

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M5_007 after M4.1 gate passed
```

## Source of truth

Source-of-truth docs:

```text
docs/LUA_SCRIPTING.md
docs/LUA_STANDARD_LIBRARY.md
docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
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
src/LLMGameCreator.Scripting/PrototypeLuaStaticAnalyzer.cs
tests/LLMGameCreator.Tests/Scripting/PrototypeLuaExecutorTests.cs
tests/LLMGameCreator.Tests/Scripting/PrototypeLuaDeclarationMapperTests.cs
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Scripting/**  (only analyzer/diagnostic updates if fixture exposes a confirmed bug)
tests/LLMGameCreator.Tests/Scripting/**
tests/fixtures/lua_sandbox/**
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
- Add small golden Lua fixtures for forbidden and safe sandbox cases.
- Analyzer rejects actual forbidden API usage with exact diagnostic codes.
- Analyzer does not reject forbidden words inside comments or string literals.
- No Lua execution is required for static golden tests.
```

Failure behavior:

```text
Return deterministic diagnostics instead of throwing for expected invalid module/manifest/source/output cases.
Never hide sandbox or validation failures.
```

Diagnostic codes:

```text
lua.generator.forbidden.io
lua.generator.forbidden.os
lua.generator.forbidden.debug
lua.generator.forbidden.package
lua.generator.forbidden.require
lua.generator.forbidden.loader
```

## Proof tests

Tests to add before/with implementation:

```text
- io.open fixture -> lua.generator.forbidden.io;
- os.execute fixture -> lua.generator.forbidden.os;
- require fixture -> lua.generator.forbidden.require;
- load/loadfile/dofile fixture -> lua.generator.forbidden.loader;
- package.path fixture -> lua.generator.forbidden.package;
- debug.getinfo fixture -> lua.generator.forbidden.debug;
- safe fixture passes;
- forbidden words in strings/comments pass.
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
Task id: M5_008
Task spec file: docs/agent-tasks/M5/M5_008_NO_GAMEPACKAGE_MUTATION_GUARD.md
Reason: Guard the executor path against GamePackage mutation after sandbox fixtures are stable.
User approval: required
Expected stop after completion: yes
```

On block, write BLOCKERS:

```text
M5_007 blocked: M4.1 gate not passed or current source layout makes this draft stale.
```
