# M6_001 — Artifact-to-GamePackage mapping contracts

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M6_001`

Milestone: `M6 Rich GamePackage Assembly`

Status: `locked_by_m4_1_gate_and_artifact_stability`

Depends on:

```text
- M4.1 gate passed;
- strict artifact contracts selected and stable;
- approved artifact set pipeline works;
- package patch/dry-run pipeline is green.
```

Unlocks:

```text
- M6_002 items/economy assembly;
- M6_003 dialogue/quest assembly.
```

Risk level: high

Expected changed files count: 5-8

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M6_001 after M4.1 gate passed and artifact contracts are stable
```

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/GAME_PACKAGE_FORMAT.md
docs/DESIGN_DB_AND_GENERATOR_REGISTRY.md
docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md
docs/VALIDATION_STRATEGY.md
docs/CONTEXT_INDEX.md
```

Context budget:

```text
Read package format, existing patch service, existing artifact review/approved set services, validator, one sample package, and adjacent tests.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
src/LLMGameCreator.Application/Design/GamePackagePatchService.cs
src/LLMGameCreator.Application/Validation/GamePackageValidator.cs
tests/LLMGameCreator.Tests/Design/*Patch*.cs
tests/LLMGameCreator.Tests/SmokeTests.cs
samples/minimal-map-game/package.json
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Application/Design/**
src/LLMGameCreator.Application/Validation/**
tests/LLMGameCreator.Tests/Design/**
tests/fixtures/artifact_mapping/**
samples/minimal-map-game/** only if adding non-breaking fixture data is explicitly approved
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/** unless runtime smoke reveals a validator-only issue and user approves
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.GamePackage/** unless schema approval is explicitly granted
LLMGameCreator.sln
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces:

```text
IArtifactToPackageMappingService or equivalent only if existing patch pipeline lacks this seam.
```

New classes:

```text
ArtifactToPackageMappingService
ArtifactToPackageMappingResult
ArtifactToPackageMappingDiagnostic
```

New methods:

```text
MapApprovedArtifactsToPatch(approvedArtifacts, targetPackage) -> game_package_patch_v1 artifact or typed dry-run result
```

Modified classes:

```text
Prefer adding mapping service over changing GamePackageDefinition.
```

Public contracts changed: no package schema change unless separately approved.

Schema changed: no.

New dependencies: no.

## Exact behavior

Input contract:

```text
Approved artifact set + target GamePackageDefinition/current package snapshot.
```

Output contract:

```text
game_package_patch_v1 operations or typed dry-run result that can be validated before apply.
```

Success behavior:

```text
- known artifact contract maps to allowlisted patch operations;
- invalid refs are rejected before apply;
- package validator passes after dry-run/apply in test fixture;
- no UI/provider/runtime call.
```

Failure behavior:

```text
- unknown artifact contract -> diagnostic;
- invalid target ref -> diagnostic;
- unsupported schema requirement -> diagnostic and stop, not silent schema change.
```

Diagnostic codes:

```text
artifact_mapping.contract.unsupported
artifact_mapping.ref.invalid
artifact_mapping.patch.invalid
artifact_mapping.schema.required
artifact_mapping.package.validation_failed
```

Validation rules:

```text
- mapping must create allowlisted data-only operations;
- mapping must not write arbitrary package JSON directly;
- dry-run must validate before apply;
- invalid refs fail deterministically.
```

Security/sandbox rules: no LLM/provider/Lua/runtime execution.

Persistence rules: use existing Design DB/artifact/patch pipeline if present; avoid new persistence format.

GamePackage mutation rule:

```text
No direct mutation from mapping. Only dry-run/apply service may mutate package through existing validated boundary when explicitly invoked by tests or approved workflow.
```

## Proof tests

Tests to add before/with implementation:

```text
- supported fixture artifact maps to expected patch operation;
- unsupported contract id fails;
- invalid ref fails;
- dry-run package validation passes for valid fixture;
- GamePackage schema is unchanged;
- mapping service does not call LLM/provider/Lua/runtime.
```

Required pass tests:

```text
valid approved artifact fixture -> patch/dry-run result -> package validator pass.
```

Required fail/reject tests:

```text
unknown contract -> artifact_mapping.contract.unsupported
invalid ref -> artifact_mapping.ref.invalid
schema-required field -> artifact_mapping.schema.required
```

Regression tests: add if existing package patch service has a known edge case.

Golden/snapshot fixtures: recommended for expected patch JSON.

Fake/corpus requirements: use fixture artifacts only.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Unit/integration test commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Patch|FullyQualifiedName~Package|FullyQualifiedName~Artifact"
```

Artifact schema commands: artifact fixture validation tests.

Package validator commands: package validator fixture tests.

Runtime scenario commands: not for M6_001 unless a stable runtime smoke runner already exists.

Snapshot/golden commands: expected patch JSON snapshot if introduced.

## Stop conditions

Stop if:

```text
- M4.1 gate not passed;
- artifact contract family is not stable;
- task requires GamePackage schema change;
- task requires touching Runtime project;
- mapping needs more than 8 files;
- existing patch pipeline cannot support dry-run without architecture decision.
```

## Non-goals

```text
- Do not implement items/economy/dialogue/quest mapping in this task.
- Do not change GamePackage schema.
- Do not add UI.
- Do not run Lua/LLM/provider.
- Do not auto-apply artifacts without validation/review boundary.
```

## Expected final report

Final report must include:

```text
- mapping contracts supported;
- patch operations emitted;
- validation diagnostics;
- tests/fixtures added;
- confirmation schema unchanged;
- check-all result;
- next recommended M6 task.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M6_002
Task spec file: docs/agent-tasks/M6/M6_002_ITEMS_ECONOMY_ASSEMBLY.md
Reason: Implement one selected artifact family mapping after mapping contracts are proven.
```

On block, write BLOCKERS:

```text
M6_001 blocked: M4.1 gate not passed or mapping requires GamePackage schema/architecture approval.
```
