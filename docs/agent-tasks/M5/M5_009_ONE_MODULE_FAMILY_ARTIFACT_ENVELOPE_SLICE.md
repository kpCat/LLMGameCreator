# M5_009 — One module family artifact envelope slice

This task spec is locked draft guidance. If implementation conflicts with current source, stop and report.

## Header

Task ID: `M5_009`

Milestone: `M5 Lua Module Executor Integration`

Status: `locked_until_M4_1_gate_passes`

Depends on:

```text
- M5_001 through M5_008 completed or explicitly refreshed and approved.
- M4.1 gate passed.
- User approved first M5 vertical slice.
- docs/CURRENT_GENERATOR_STATE.md says M4.1 passed.
- docs/CURRENT_GENERATOR_STATE.json says M4.1 passed.
- User explicitly approves this M5 task in NEXT_TASK.md.
- check-all is green before starting.
```

Unlocks:

```text
- M6 artifact-to-package mapping contract preparation after review.
```

Risk level: high

Expected changed files count: 5-8

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M5_009 after M4.1 gate passed
```

## Source of truth

Source-of-truth docs:

```text
docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
docs/LUA_GENERATION_PLAN_AND_PROMPTS.md
docs/SCRIPT_MANIFEST_SPEC.md
docs/ASSET_CONTRACT_SPEC.md
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
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Application/Design/**Generator**.cs
tests/LLMGameCreator.Tests/Scripting/**
tests/LLMGameCreator.Tests/Design/**
generator-library/manifests/  (only one representative manifest if already present)
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Application/Design/**
tests/LLMGameCreator.Tests/Scripting/**
tests/LLMGameCreator.Tests/Design/**
tests/fixtures/lua_one_module_family/**
docs/agent-tasks/001_TASK_PACK_LEDGER.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
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
- Execute or simulate exactly one approved Lua generator module family through the M5 request/result boundary.
- Output exactly typed artifact envelope data with module id, capability id, contract id/version, seed, and payload JSON.
- Validate artifact envelope before reporting success.
- Do not assemble or apply to GamePackage.
```

Failure behavior:

```text
Return deterministic diagnostics instead of throwing for expected invalid module/manifest/source/output cases.
Never hide sandbox or validation failures.
```

Diagnostic codes:

```text
lua.execution.artifact.contract_missing
lua.execution.artifact.contract_version_missing
lua.execution.artifact.module_id_missing
lua.execution.artifact.seed_missing
lua.execution.artifact.payload_invalid
lua.execution.capability.mismatch
```

## Proof tests

Tests to add before/with implementation:

```text
- same seed + same config produces identical artifact envelope;
- different seed changes only allowed generated payload fields;
- capability mismatch fails before execution;
- forbidden API fails before execution;
- invalid artifact envelope fails with exact diagnostic;
- artifact envelope is not applied to GamePackage.
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
Task id: M6_001
Task spec file: docs/agent-tasks/M6/M6_001_ARTIFACT_TO_PACKAGE_MAPPING_CONTRACTS.md
Reason: Prepare artifact-to-package mapping contracts after the first M5 artifact envelope slice is reviewed.
User approval: required
Expected stop after completion: yes
```

On block, write BLOCKERS:

```text
M5_009 blocked: M4.1 gate not passed or current source layout makes this draft stale.
```
