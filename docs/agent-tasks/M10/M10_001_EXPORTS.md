# M10_001 — Export profile contracts

This task spec is a locked draft planning contract. It is not executable until current-state docs explicitly unlock M10.

## Header

Task ID: `M10_001`

Milestone: `M10 Export Profiles / Unity IR`

Status: `locked_by_m10_gate`

Depends on:

```text
- M4.1 passed;
- package generation/validation/runtime preview are stable;
- current-state docs explicitly unlock M10 export profile work.
```

Risk level: high

Expected changed files count: 4-8 after unlock

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M10_001 after M10 gate is unlocked
```

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
docs/agent-tasks/M10/000_M10_SEQUENCE.md
docs/GAME_PACKAGE_FORMAT.md
docs/ASSET_CONTRACT_SPEC.md
docs/CONTEXT_INDEX.md
```

Context budget:

```text
Read this task spec, M10 sequence, current-state docs, package/export-related docs, and 2-3 local analogs. Do not read all M5/M6/M8/M9 specs.
```

Existing patterns to inspect after unlock:

```text
- existing package validation/export/storage services if present;
- existing artifact/asset contract docs and tests;
- runtime/player boundary docs and tests if present.
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Application/Export/**
tests/LLMGameCreator.Tests/Export/**
tests/fixtures/export_profiles/**
docs/agent-tasks/001_TASK_PACK_LEDGER.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/** unless a boundary test is explicitly scoped
src/LLMGameCreator.WinForms/**
LLMGameCreator.sln
*.csproj
Unity project files
provider/LLM integration files
```

Deleted files: none

## API / implementation contract

New interfaces/classes/methods:

```text
Define the smallest data contracts/services needed for this task only.
Avoid UI wiring and player/runtime coupling unless this task explicitly owns a boundary test.
```

Public contracts changed: only if current-state docs and task-specific approval allow it.

Schema changed: no GamePackage schema changes unless explicitly approved.

New dependencies: no.

## Exact behavior

Purpose:

```text
Define data-only export profile contracts that describe target, format, asset policy, and validation mode without invoking export implementation.
```

Expected behavior after unlock:

```text
- valid profile carries target format, package id, asset policy, deterministic ordering policy, and validation mode;
- invalid profile is rejected before export;
- no file/provider/network export happens in this contract-only task.
```

Failure behavior / diagnostics:

```text
export.profile.id.missing
export.profile.target.unsupported
export.profile.asset_policy.invalid
export.profile.validation_mode.invalid
```

Validation rules:

```text
- output must be deterministic;
- failure must produce stable machine-readable diagnostics;
- no LLM/provider/editor workflow may move into runtime/player;
- data contracts must remain reviewable and testable.
```

Security/boundary rules:

```text
Unity/player/export consumer receives data only. No editor generator service, LLM client, ComfyUI/Fooocus provider, or asset generation provider is allowed in player/runtime.
```

GamePackage mutation rule:

```text
Export reads validated package/artifact data. It does not mutate the source GamePackage.
```

## Proof tests

Tests to add before/with implementation after unlock:

```text
- valid profile fixture passes;
- missing id -> export.profile.id.missing;
- unsupported target -> export.profile.target.unsupported;
- invalid asset policy -> export.profile.asset_policy.invalid;
- contract test proves no export side effects.
```

Required pass tests:

```text
A small valid fixture produces deterministic accepted output for this task.
```

Required fail/reject tests:

```text
An invalid fixture fails with exact diagnostic code.
```

Golden/snapshot fixtures: small deterministic fixtures only.

Fake/corpus requirements: no real LLM/provider/network calls.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Unit/integration test commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Export|FullyQualifiedName~Unity|FullyQualifiedName~Asset|FullyQualifiedName~Boundary"
```

Runtime scenario commands: only if a stable runtime/player boundary test harness exists.

## Stop conditions

Stop if:

```text
- current-state docs do not unlock M10;
- contract needs GamePackage schema change;
- implementation tries to create Unity project files;
- task needs provider/LLM dependency;
- proof tests cannot pin exact diagnostics.
```

## Non-goals

```text
- Do not generate C# from LLM output.
- Do not create Unity project/runtime implementation in this task unless current-state docs explicitly unlock it.
- Do not add provider dependencies.
- Do not mutate GamePackage during export.
- Do not broaden into unrelated M5/M6/M8/M9 work.
```

## Expected final report

Final report must include:

```text
- contracts/classes/methods added;
- exact diagnostics;
- proof tests and fixtures;
- confirmation no provider/editor dependency entered runtime/player;
- check-all result.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M10_002
Task spec file: docs/agent-tasks/M10/M10_002_UNITY_IR.md
Reason: Define Unity-oriented IR only after export profile contracts exist.
```

On block, write BLOCKERS:

```text
M10_001 blocked: M10 gate is not unlocked or source assumptions require architecture approval.
```
