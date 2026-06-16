# M5_003 — Lua static sandbox policy for generator modules

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M5_003`

Milestone: `M5 Lua Module Registry / Executor Integration`

Status: `locked_by_m4_1_gate_and_M5_001_M5_002`

Depends on:

```text
- M4.1 gate passed;
- M5_001 contracts completed;
- M5_002 manifest validation completed;
- existing PrototypeLuaStaticAnalyzer behavior understood.
```

Unlocks:

```text
- one safe module family execution task.
```

Risk level: high

Expected changed files count: 4-8

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M5_003 after M5_001 and M5_002 completed
```

## Source of truth

Source-of-truth docs:

```text
docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
docs/LUA_SCRIPTING.md
docs/LUA_STANDARD_LIBRARY.md
docs/SCRIPT_MANIFEST_SPEC.md
.devflow/MODELING_STRATEGY.md
```

Context budget:

```text
Read existing PrototypeLuaStaticAnalyzer/Executor/tests and one or two manifest/executor contract tests.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Scripting/PrototypeLuaStaticAnalyzer.cs
src/LLMGameCreator.Scripting/PrototypeLuaExecutor.cs
tests/LLMGameCreator.Tests/Scripting/PrototypeLuaExecutorTests.cs
tests/LLMGameCreator.Tests/Scripting/PrototypeLuaDeclarationMapperTests.cs
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Scripting/**
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
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces: avoid unless required by M5_001 contract.

New classes:

```text
LuaGeneratorModuleStaticAnalyzer or extend existing PrototypeLuaStaticAnalyzer only if ownership remains clear.
```

New methods:

```text
AnalyzeGeneratorModule(source, target/moduleId) -> diagnostics
```

Modified classes:

```text
PrototypeLuaStaticAnalyzer or new sibling analyzer following its pattern.
```

Public contracts changed: small diagnostic/result contracts only if already present.

Schema changed: no.

New dependencies: no.

## Exact behavior

Input contract:

```text
Lua source string + target/module id.
```

Output contract:

```text
Diagnostic list. No execution.
```

Success behavior:

```text
Safe declaration/generator-module source passes static policy.
```

Failure behavior:

```text
Forbidden APIs fail with deterministic diagnostics before execution.
```

Diagnostic codes:

```text
lua.generator.forbidden.io
lua.generator.forbidden.os
lua.generator.forbidden.debug
lua.generator.forbidden.package
lua.generator.forbidden.loader
lua.generator.forbidden.require
lua.generator.forbidden.process
lua.generator.forbidden.network
lua.generator.forbidden.random_unseeded
lua.generator.forbidden.control_flow  (only if loops are forbidden for the selected module family)
```

Validation rules:

```text
- Static analyzer must remove comments/strings before matching forbidden identifiers.
- False positives inside strings/comments should not fail.
- Actual identifiers/tables/functions must fail.
```

Security/sandbox rules:

```text
Forbidden: io, os, debug, package, require, load, loadfile, dofile, filesystem, network, process, unrestricted randomness, direct GameState access.
```

Persistence rules: none.

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- io.open fails;
- os.execute fails;
- debug.getinfo fails;
- package.path fails;
- require('x') fails;
- load('...') fails;
- forbidden word inside string/comment does not fail;
- safe source passes;
- analyzer does not execute Lua.
```

Required pass tests:

```text
safe_generator_module.lua -> no error diagnostics.
forbidden token in string/comment -> no error diagnostics.
```

Required fail/reject tests:

```text
io.open -> lua.generator.forbidden.io
os.execute -> lua.generator.forbidden.os
require -> lua.generator.forbidden.require
load/loadfile/dofile -> lua.generator.forbidden.loader
```

Regression tests: add for any existing analyzer false positive/negative found.

Golden/snapshot fixtures: optional Lua fixture files.

Fake/corpus requirements: use local source strings/fixtures, no execution.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Unit/integration test commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Lua"
```

Manifest integrity commands: not required unless manifest rules changed.

Runtime scenario commands: not applicable.

## Stop conditions

Stop if:

```text
- M4.1 gate not passed;
- M5_001/M5_002 not completed;
- static policy needs a real Lua engine/dependency change;
- analyzer cannot avoid obvious false positives without broad parser work;
- task exceeds 8 files;
- implementing sandbox would require Runtime project changes.
```

## Non-goals

```text
- Do not execute generator modules.
- Do not add a new Lua dependency.
- Do not mutate GamePackage.
- Do not add UI.
- Do not support broad Lua module families yet.
```

## Expected final report

Final report must include:

```text
- forbidden API list;
- diagnostic codes;
- pass/fail fixture list;
- confirmation no execution occurs;
- tests run;
- check-all result.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: phase_plan
Phase plan file: .devflow/phase-plans/30_M5_LUA_MODULE_EXECUTOR.md
Reason: Select one safe module family execution task after contracts/manifest/sandbox are proven.
```

On block, write BLOCKERS:

```text
M5_003 blocked: sandbox policy requires parser/dependency/architecture approval.
```
