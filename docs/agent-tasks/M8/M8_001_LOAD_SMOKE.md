# M8_001 — Package load smoke

This task spec is locked draft guidance. If implementation conflicts with current source when the phase is unlocked, refresh this spec before execution.

## Header

Task ID: `M8_001`

Milestone: `M8 Runtime Preview Validation Loop`

Status: `locked_by_package_assembly_gate`

Depends on:

```text
- M4.1 gate passed;
- M6 has produced a small validated assembled package;
- check-all is green.
```

Unlocks:

```text
- M8_002 deterministic command scenario.
```

Risk level: medium/high

Expected changed files count: 4-8 after unlock

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M8_001 after package assembly path exists
```

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/GAME_PACKAGE_FORMAT.md
docs/VALIDATION_STRATEGY.md
docs/CONTEXT_INDEX.md
docs/agent-tasks/M8/000_M8_SEQUENCE.md
```

Context budget:

```text
Read this task spec, M8 sequence, package format, validator docs, and only the runtime/package files named by local analog inspection. Do not read all Runtime or all samples unless blocked by missing local pattern.
```

Existing patterns to inspect after unlock:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Application/Validation/GamePackageValidator.cs
tests/LLMGameCreator.Tests/*Runtime*.cs
tests/LLMGameCreator.Tests/SmokeTests.cs
samples/minimal-map-game/package.json
```

## File boundaries

Allowed files after unlock:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
tests/LLMGameCreator.Tests/Runtime/**
tests/LLMGameCreator.Tests/**Runtime*.cs
tests/fixtures/runtime_preview/**
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
LLMGameCreator.sln
*.csproj
docs/CURRENT_GENERATOR_STATE.md unless the task is explicitly a gate-update task
```

Deleted files: none

## API / implementation contract

New interfaces:

```text
Avoid unless an existing runtime preview seam is missing and the task is amended with user approval.
```

New classes:

```text
Use existing runtime runner/diagnostic/result patterns when present. Add one small task-specific helper only if needed.
```

Public contracts changed: avoid.

Schema changed: no GamePackage schema changes.

New dependencies: no.

## Exact behavior

Intent:

```text
Load one validated sample package through the headless runtime/package load path and return a deterministic success result.
```

Input contract:

```text
Validated GamePackage or validated package fixture + deterministic runtime scenario input.
```

Output contract:

```text
Typed runtime result/diagnostics/events/state snapshot. No package mutation.
```

Success behavior:

```text
- validated sample package loads without runtime diagnostics;
- invalid package fixture fails before runtime scenario execution;
- runtime remains headless and provider-free.
```

Failure behavior:

```text
- invalid package/scenario produces deterministic diagnostics;
- runtime dependency boundary violation is a blocker, not a workaround;
- no generator/LLM/provider repair path is invoked.
```

Diagnostic codes:

```text
runtime.preview.package.invalid
runtime.preview.load.failed
runtime.preview.command.invalid
runtime.preview.scenario.failed
runtime.preview.snapshot.mismatch
runtime.preview.boundary.violation
runtime.preview.package.mutated
```

Validation rules:

```text
- package is validated before scenario execution;
- runtime output is deterministic for the same input;
- runtime does not mutate package definition;
- no LLM/provider/UI calls are introduced.
```

Security/sandbox rules:

```text
Runtime preview is headless and provider-free.
```

Persistence rules:

```text
No persistent package writes unless an existing approved test fixture path explicitly owns snapshot/golden output.
```

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- valid assembled sample package loads successfully;
- invalid package fixture returns runtime.preview.package.invalid;
- load path does not call LLM/provider/UI;
- same package load produces stable result shape.
```

Required pass tests:

```text
Valid fixture/scenario -> deterministic success result.
```

Required fail/reject tests:

```text
Invalid fixture/scenario -> exact diagnostic code.
```

Golden/snapshot fixtures:

```text
Use small human-readable fixtures only. Snapshot/golden output must be deterministic and reviewable.
```

Fake/corpus requirements:

```text
Use local package/scenario fixtures. Do not call real LLM/provider/network.
```

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Unit/integration test commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Runtime|FullyQualifiedName~Package"
```

Package validator commands: package validator fixture tests when applicable.

Runtime scenario commands: deterministic runtime scenario test when applicable.

Snapshot/golden commands: golden comparison test when applicable.

## Stop conditions

Stop if:

```text
- no assembled validated package fixture exists;
- load requires UI/provider/generation dependency;
- package schema change is needed;
- runtime loader behavior is ambiguous and needs architecture decision.
```

## Non-goals

```text
- Do not call LLM/provider/UI.
- Do not mutate GamePackage schema.
- Do not add package repair generation loop.
- Do not add Unity/export behavior.
- Do not implement unrelated runtime mechanics.
```

## Expected final report

Final report must include:

```text
- runtime/package files changed;
- diagnostic codes asserted;
- proof tests and fixtures;
- confirmation runtime is headless/provider-free;
- confirmation GamePackage definition was not mutated;
- check-all result.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M8_002
Task spec file: docs/agent-tasks/M8/M8_002_COMMAND_SCENARIO.md
Reason: Add deterministic runtime command scenario after package load smoke is proven.
```

On block, write BLOCKERS:

```text
M8_001 blocked: package assembly path is missing, current gate is not open, or runtime boundary assumptions need architecture approval.
```
