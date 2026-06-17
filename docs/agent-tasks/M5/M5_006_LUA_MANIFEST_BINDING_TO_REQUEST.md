# M5_006 — Lua manifest binding to request

This task spec is locked draft guidance. If implementation conflicts with current source, stop and report.

## Header

Task ID: `M5_006`

Milestone: `M5 Lua Module Executor Integration`

Status: `locked_until_M4_1_gate_passes`

Depends on:

```text
- M5_002 manifest validation behavior exists or has been refreshed.
- M5_005 request/result DTOs exist.
- docs/CURRENT_GENERATOR_STATE.md says M4.1 passed.
- docs/CURRENT_GENERATOR_STATE.json says M4.1 passed.
- User explicitly approves this M5 task in NEXT_TASK.md.
- check-all is green before starting.
```

Unlocks:

```text
- M5_009 one module family artifact envelope slice.
```

Risk level: medium/high

Expected changed files count: 4-8

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M5_006 after M4.1 gate passed
```

## Source of truth

Source-of-truth docs:

```text
docs/SCRIPT_MANIFEST_SPEC.md
docs/DESIGN_DB_AND_GENERATOR_REGISTRY.md
docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
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
tests/LLMGameCreator.Tests/Design/GeneratorLibraryRegistryTests.cs
src/LLMGameCreator.Application/Design/**GeneratorLibrary**.cs
src/LLMGameCreator.Scripting/**
tests/LLMGameCreator.Tests/Scripting/**
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Application/Design/**
src/LLMGameCreator.Scripting/**
tests/LLMGameCreator.Tests/Design/**
tests/LLMGameCreator.Tests/Scripting/**
tests/fixtures/lua_manifests/**
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
- Convert an approved canonical Lua generator manifest plus selected capability/config into a Lua execution request.
- Reject missing id/path/capability/contract fields before request creation.
- Reject manifest paths outside the allowed generator root.
- Reject capability mismatch with exact diagnostic.
- Do not execute Lua in this task.
```

Failure behavior:

```text
Return deterministic diagnostics instead of throwing for expected invalid module/manifest/source/output cases.
Never hide sandbox or validation failures.
```

Diagnostic codes:

```text
lua.manifest.id.missing
lua.manifest.path.missing
lua.manifest.path.outside_root
lua.manifest.capability.mismatch
lua.manifest.contract.missing
lua.manifest.execution.not_approved
```

## Proof tests

Tests to add before/with implementation:

```text
- valid manifest + selected capability produces request with module id/path/capability/contract id/seed;
- missing id returns lua.manifest.id.missing;
- outside-root path returns lua.manifest.path.outside_root;
- capability mismatch returns lua.manifest.capability.mismatch;
- binding does not call executor and does not mutate GamePackage.
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
Task id: M5_007
Task spec file: docs/agent-tasks/M5/M5_007_FORBIDDEN_API_GOLDEN_FIXTURES.md
Reason: Add forbidden API golden fixtures after manifest-to-request binding is deterministic.
User approval: required
Expected stop after completion: yes
```

On block, write BLOCKERS:

```text
M5_006 blocked: M4.1 gate not passed or current source layout makes this draft stale.
```
