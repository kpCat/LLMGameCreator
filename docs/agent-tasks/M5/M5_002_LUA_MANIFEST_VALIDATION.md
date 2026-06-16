# M5_002 — Lua generator manifest validation before execution

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M5_002`

Milestone: `M5 Lua Module Registry / Executor Integration`

Status: `locked_by_m4_1_gate_and_M5_001`

Depends on:

```text
- M4.1 gate passed;
- M5_001 executor/request/result contracts exist;
- generator library registry manifest import path is green.
```

Unlocks:

```text
- M5_003 Lua static sandbox policy;
- one-family execution task.
```

Risk level: medium/high

Expected changed files count: 5-8

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M5_002 after M5_001 completed
```

## Source of truth

Source-of-truth docs:

```text
docs/SCRIPT_MANIFEST_SPEC.md
docs/DESIGN_DB_AND_GENERATOR_REGISTRY.md
docs/CONTEXT_INDEX.md
docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
```

Context budget:

```text
Read manifest spec, registry import tests, M5_001 contracts, and only sample manifests needed for fixtures.
```

Existing patterns to inspect:

```text
tests/LLMGameCreator.Tests/Design/GeneratorLibraryRegistryTests.cs
src/LLMGameCreator.Application/Design/**GeneratorLibrary**.cs
src/LLMGameCreator.Infrastructure/Storage/SqliteDesignDatabase.cs  (only if registry persistence already owns manifest diagnostics)
generator-library/manifests/*.manifest.json  (only 1-3 representative fixtures, not all)
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Application/Design/**
tests/LLMGameCreator.Tests/Design/**
tests/fixtures/lua_manifests/**
docs/agent-tasks/001_TASK_PACK_LEDGER.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.WinForms/** unless existing registry diagnostics page mapping needs a tiny update
src/LLMGameCreator.GamePackage/**
LLMGameCreator.sln
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces: avoid unless a manifest validator abstraction already exists nearby.

New classes:

```text
LuaGeneratorModuleManifestValidator or equivalent only if no existing validator owns this.
```

New methods:

```text
ValidateForExecution(manifest, selectedCapabilities/config) -> validation report/diagnostics
```

Modified classes:

```text
Existing generator library integrity validator or registry validator if it already owns canonical manifest checks.
```

Public contracts changed: avoid.

Schema changed: no.

New dependencies: no.

## Exact behavior

Input contract:

```text
Generator module manifest + selected capability/config context.
```

Output contract:

```text
Validation report/diagnostics. No execution. No GamePackage mutation.
```

Success behavior:

```text
Canonical manifest fields validate.
Declared module path is inside generator-library allowed root.
Declared capability matches selected context.
Duplicate module ids are rejected.
Unknown noncanonical fields are metadata only unless execution-critical.
```

Failure behavior:

```text
Invalid manifest blocks execution and returns diagnostics.
```

Diagnostic codes:

```text
lua.manifest.id.missing
lua.manifest.path.missing
lua.manifest.path.outside_root
lua.manifest.capability.mismatch
lua.manifest.duplicate_id
lua.manifest.contract.missing
lua.manifest.execution.not_approved
```

Validation rules:

```text
- Manifest validation does not execute Lua.
- Manifest validation does not read arbitrary module source unless path existence/content check is explicitly required.
- Canonical fields must be preferred over legacy aliases.
```

Security/sandbox rules: no execution.

Persistence rules: use existing registry diagnostics style.

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- valid canonical manifest passes execution-readiness validation;
- missing id fails;
- missing path fails;
- outside-root path fails;
- capability mismatch fails;
- duplicate id fails;
- validation does not call executor.
```

Required pass tests:

```text
valid_manifest.execution_ready.json -> no error diagnostics.
```

Required fail/reject tests:

```text
missing_id.json -> lua.manifest.id.missing
outside_root_path.json -> lua.manifest.path.outside_root
capability_mismatch.json -> lua.manifest.capability.mismatch
```

Regression tests: add if previous manifest contract drift appears in fixtures.

Golden/snapshot fixtures: small JSON fixtures under tests/fixtures/lua_manifests/.

Fake/corpus requirements: use fixture manifests.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Unit/integration test commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GeneratorLibraryRegistry|FullyQualifiedName~Manifest"
```

Manifest integrity commands: manifest integrity test suite.

Artifact schema commands: not applicable.

Package validator commands: not applicable.

Runtime scenario commands: not applicable.

## Stop conditions

Stop if:

```text
- M4.1 gate not passed;
- M5_001 not completed;
- validation requires DB schema change;
- validation requires executing Lua;
- task exceeds 8 files;
- canonical manifest rules are ambiguous.
```

## Non-goals

```text
- Do not execute modules.
- Do not mutate GamePackage.
- Do not add UI unless explicitly scoped.
- Do not support legacy manifest aliases as canonical output.
```

## Expected final report

Final report must include:

```text
- manifest rules added;
- diagnostic codes;
- fixture list;
- tests run;
- confirmation executor was not called.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M5_003
Task spec file: docs/agent-tasks/M5/M5_003_LUA_STATIC_SANDBOX_POLICY.md
Reason: Validate static sandbox policy before any module execution.
```

On block, write BLOCKERS:

```text
M5_002 blocked: manifest validation requires architecture/schema approval.
```
