# M10_004 — Player boundary tests

This task spec is a locked draft planning contract. It is not executable until current-state docs explicitly unlock M10.

## Header

Task ID: `M10_004`

Milestone: `M10 Export Profiles / Unity IR`

Status: `locked_by_m10_gate`

Depends on:

```text
- deterministic export package exists;
- runtime/player boundary assumptions are reviewed;
- current-state docs explicitly unlock player boundary proof work.
```

Risk level: high

Expected changed files count: 4-8 after unlock

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M10_004 after M10 gate is unlocked
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
tests/LLMGameCreator.Tests/Export/**
tests/LLMGameCreator.Tests/Runtime/** only if existing boundary test patterns exist
src/LLMGameCreator.Application/Export/** only for testable metadata helpers
docs/agent-tasks/001_TASK_PACK_LEDGER.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.WinForms/**
provider/LLM implementation files
Unity project implementation files
GamePackage schema files
*.csproj unless explicitly approved
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
Add boundary tests proving the player/export consumer path does not reference editor/generator/provider/LLM workflows.
```

Expected behavior after unlock:

```text
- boundary scan/test rejects editor/generator/provider dependencies in player-facing artifacts;
- export artifacts remain data-only;
- runtime/player-facing layer can consume only data contracts.
```

Failure behavior / diagnostics:

```text
player_boundary.editor_dependency.forbidden
player_boundary.provider_dependency.forbidden
player_boundary.generated_code.forbidden
player_boundary.asset_provider_call.forbidden
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
- fixture/export artifact passes dependency boundary scan;
- injected provider reference fails with player_boundary.provider_dependency.forbidden;
- injected generated C# marker fails with player_boundary.generated_code.forbidden;
- test confirms no WinForms/editor assembly dependency in player-facing contract.
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
- no stable player/export boundary exists;
- boundary test would require broad project restructuring;
- dependency scan cannot be deterministic;
- task would need UI/provider changes.
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
Task id: M10_005
Task spec file: docs/agent-tasks/M10/M10_005_ASSETS.md
Reason: Map asset references after export/player boundary is proven.
```

On block, write BLOCKERS:

```text
M10_004 blocked: M10 gate is not unlocked or source assumptions require architecture approval.
```
