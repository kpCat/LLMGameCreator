# M5_001 — Lua generator module executor contracts

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M5_001`

Milestone: `M5 Lua Module Registry / Executor Integration`

Status: `locked_by_m4_1_gate`

Depends on:

```text
- docs/CURRENT_GENERATOR_STATE.md and .json explicitly unlock M5;
- real M4.1 evaluation report has been reviewed;
- check-all is green.
```

Unlocks:

```text
- M5_002 Lua manifest validation;
- M5_003 Lua sandbox policy;
```

Risk level: high

Expected changed files count: 5-8

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M5_001 after M4.1 gate passed
```

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
docs/LUA_GENERATION_PLAN_AND_PROMPTS.md
docs/SCRIPT_MANIFEST_SPEC.md
docs/CONTEXT_INDEX.md
.devflow/MODELING_STRATEGY.md
```

Context budget:

```text
Read this task spec, M5 source docs, existing Prototype Lua executor/analyzer/contracts/tests, and generator library registry manifest import tests. Do not read all Lua templates/manifests.
```

Read only these docs:

```text
docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
docs/SCRIPT_MANIFEST_SPEC.md
docs/CONTEXT_INDEX.md
```

Do not read:

```text
- all generator-library/;
- all templates/;
- M6/M8/M9 plans;
- unrelated WinForms pages.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Scripting/IPrototypeLuaExecutor.cs
src/LLMGameCreator.Scripting/PrototypeLuaExecutor.cs
src/LLMGameCreator.Scripting/PrototypeLuaStaticAnalyzer.cs
tests/LLMGameCreator.Tests/Scripting/PrototypeLuaExecutorTests.cs
tests/LLMGameCreator.Tests/Scripting/PrototypeLuaDeclarationMapperTests.cs
tests/LLMGameCreator.Tests/Design/GeneratorLibraryRegistryTests.cs
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Application/Design/**  (only request/result service contracts if Application owns orchestration)
tests/LLMGameCreator.Tests/Scripting/**
tests/LLMGameCreator.Tests/Design/**  (only manifest/executor contract tests)
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
*.csproj unless task is explicitly updated with approval
```

Deleted files: none

## API / implementation contract

New interfaces:

```text
ILuaGeneratorModuleExecutor
```

Suggested owner:

```text
Scripting owns raw sandboxed Lua execution primitives.
Application owns approved-module orchestration if persisted registry/artifacts are involved.
```

New request/result DTOs:

```text
LuaGeneratorModuleExecutionRequest
- ModuleId
- ModulePath
- ManifestPath or manifest snapshot
- ConfigArtifactJson
- Seed
- MaxExecutionMs
- MaxOutputArtifacts
- WorkingRoot/project root if needed only for path validation, not arbitrary IO

LuaGeneratorModuleExecutionResult
- Success
- IReadOnlyList<GeneratedArtifactEnvelope> Artifacts or neutral artifact DTO if existing envelope exists elsewhere
- IReadOnlyList<Diagnostic> Diagnostics
- ElapsedMs
- AuditJson optional if existing audit pattern exists
```

New methods:

```text
Task<LuaGeneratorModuleExecutionResult> ExecuteAsync(LuaGeneratorModuleExecutionRequest request, CancellationToken cancellationToken)
```

Modified classes:

```text
Prefer new contract files and tests only. Do not wire UI or registry execution in this task.
```

Public contracts changed: yes, new internal/application/scripting contracts only.

Schema changed: no GamePackage schema changes.

New dependencies: no.

## Exact behavior

Input contract:

```text
Approved module manifest + config artifact + deterministic seed.
```

Output contract:

```text
Generated artifact envelope(s) and diagnostics. No GamePackage mutation.
```

Success behavior:

```text
- A valid approved module request can be represented by typed request/result contracts.
- Result can represent one or more generated artifacts without applying them.
- Deterministic seed is part of the request contract.
```

Failure behavior:

```text
- Missing module/manifest/config -> diagnostic result.
- Capability mismatch -> diagnostic result.
- Forbidden API -> diagnostic result.
- Execution error -> diagnostic result.
```

Diagnostic codes:

```text
lua.generator.module.missing
lua.generator.manifest.missing
lua.generator.manifest.invalid
lua.generator.capability.mismatch
lua.generator.config.invalid
lua.generator.forbidden_api
lua.generator.execution_failed
lua.generator.timeout
lua.generator.output.invalid
```

Validation rules:

```text
- Only approved module manifests may be executed.
- Manifest capability must match selected capability/config artifact.
- Output artifact envelope must carry contract id/version/source module id/seed.
- Execution result never mutates GamePackage.
```

Security/sandbox rules:

```text
- Lua must not access io/os/debug/package/load/loadfile/dofile/require/network/process/filesystem.
- No unrestricted C# GameState access.
- No runtime dependency on this executor.
```

Persistence rules:

```text
This task defines contracts only. Persisting audit/artifacts is a later task unless existing service boundary makes it trivial and within file limit.
```

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- request/result contracts can represent approved module execution without GamePackage mutation;
- missing manifest returns lua.generator.manifest.missing;
- capability mismatch returns lua.generator.capability.mismatch;
- forbidden API is represented as lua.generator.forbidden_api;
- same seed must be part of request/result determinism contract.
```

Required pass tests:

```text
Valid contract-only request -> result model can carry generated artifact envelope metadata and diagnostics are empty.
```

Required fail/reject tests:

```text
Missing manifest -> diagnostic, no artifacts.
Capability mismatch -> diagnostic, no artifacts.
Forbidden API -> diagnostic, no artifacts.
```

Regression tests: none unless existing prototype Lua tests expose a conflict.

Golden/snapshot fixtures: optional small JSON fixture for request/result shape.

Fake/corpus requirements:

```text
Use fake module source/manifest/config. Do not execute real generator-library modules in this task unless the task is amended.
```

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Unit/integration test commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Lua"
```

Docs consistency commands: update agent task ledger only; no current-state unlock unless user approved.

Manifest integrity commands: contract tests only; detailed validation in M5_002.

Artifact schema commands: contract fixture if artifact envelope type exists.

Package validator commands: not applicable.

Runtime scenario commands: not applicable.

Snapshot/golden commands: optional request/result fixture.

## Stop conditions

Stop if:

```text
- M4.1 gate is still active;
- task requires new Lua NuGet/runtime dependency;
- task requires GamePackage schema change;
- task requires Runtime project changes;
- task requires UI wiring;
- task exceeds 8 files;
- existing PrototypeLuaExecutor architecture conflicts with the proposed contract and needs refactor.
```

## Non-goals

```text
- Do not execute real generator module family.
- Do not wire WinForms UI.
- Do not mutate GamePackage.
- Do not broaden Lua sandbox.
- Do not add new dependencies.
- Do not implement M6 assembly.
```

## Expected final report

Final report must include:

```text
- exact new interfaces/classes/methods;
- diagnostic codes added;
- proof tests;
- confirmation no GamePackage mutation path was added;
- confirmation Runtime project untouched;
- check-all result.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M5_002
Task spec file: docs/agent-tasks/M5/M5_002_LUA_MANIFEST_VALIDATION.md
Reason: Validate approved module manifests before any execution path.
```

On block, write BLOCKERS:

```text
M5_001 blocked: M4.1 gate not passed or executor contract requires architecture approval.
```
