# M9_001 — Template family contracts

This task spec is locked planning guidance. If implementation conflicts with this file after unlock, stop and report.

## Header

Task ID: `M9_001`

Milestone: `M9 Templates and Balancing`

Status: ``locked_until_generation_and_package_validation_paths_are_stable``

Depends on:

```text
- M4.1 gate passed;
- M5/M6 generated artifact/package assembly path stable;
- M8 or equivalent validation smoke exists;
- user explicitly unlocks M9.
```

Unlocks:

```text
Task source: agent_task_spec
Task id: M9_002
Task spec file: docs/agent-tasks/M9/M9_002_RANGES.md
Reason: Add numeric range constraints after template contracts exist.
```

Risk level: medium/high

Expected changed files count: 4-8

## Gate status

Allowed before current gate review: no

Requires user approval: yes

Approval text required in NEXT_TASK.md:

```text
User approval: approved to start M9_001 after M9 gate conditions passed
```

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
docs/agent-tasks/M9/000_M9_SEQUENCE.md
docs/VALIDATION_STRATEGY.md
docs/CONTEXT_INDEX.md
```

Context budget:

```text
Read this task spec, M9 sequence, relevant contract/validator docs, existing generator/template/balance tests if present, and 2-3 adjacent local patterns only.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Application/Validation/**
src/LLMGameCreator.Application/Design/**
tests/LLMGameCreator.Tests/Design/**
tests/LLMGameCreator.Tests/Validation/**
samples/minimal-map-game/package.json
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Application/Design/**
src/LLMGameCreator.Application/Validation/**
tests/LLMGameCreator.Tests/Design/**
tests/fixtures/templates/**
docs/agent-tasks/001_TASK_PACK_LEDGER.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.GamePackage/** unless schema approval is explicit
src/LLMGameCreator.Scripting/** unless template expansion is explicitly Lua-backed
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.WinForms/**
LLMGameCreator.sln
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces:

```text
Avoid new interfaces unless the source-refreshed executable spec identifies a missing seam.
```

New classes:

```text
Prefer small data contracts/validators over broad services.
```

Public contracts changed:

```text
Allowed only after source refresh and explicit approval. GamePackage schema changes are not allowed by default.
```

Schema changed: no by default.

New dependencies: no.

## Exact behavior

Purpose:

```text
Define small template family contracts that can expand into data envelopes without producing C# code or bypassing review/validation.
```

Success behavior:

```text
- valid template family definition is representable as data;
- template instance expansion is deterministic under seed/input;
- expanded output carries template family id/version/source id;
- invalid/missing family id is rejected.
```

Failure behavior:

```text
Invalid inputs fail with stable diagnostic codes and do not produce partial accepted output.
```

Diagnostic codes:

```text
template.family.id.missing
template.family.version.missing
template.instance.invalid
template.expansion.failed
template.output.invalid
```

Validation rules:

```text
- behavior is deterministic under seed/input;
- output remains data/contracts, not C# code;
- no runtime/provider/LLM calls;
- exact assertions must pin ids, counts, order, ranges, and diagnostic codes where applicable.
```

Security/sandbox rules: no LLM/provider/runtime execution.

Persistence rules: use existing fixture/sample patterns only unless explicit task approval says otherwise.

GamePackage mutation rule: forbidden unless a later M6/M8 validated boundary explicitly owns package mutation.

## Proof tests

Tests to add before/with implementation:

```text
- valid template family fixture expands to exact expected envelope id/count/order;
- missing family id -> template.family.id.missing;
- same seed/input -> byte-stable or structurally equal output;
- generated output contains data only and no C# fragments.
```

Required pass tests:

```text
At least one valid fixture proves exact expected output, not just non-empty success.
```

Required fail/reject tests:

```text
At least one invalid fixture asserts exact diagnostic code.
```

Regression tests: add if existing fixtures expose a known contract drift.

Golden/snapshot fixtures: recommended when output ordering/shape is contractual.

Fake/corpus requirements: use local fixtures only; do not call real LLM/provider.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Unit/integration test commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Template|FullyQualifiedName~Balance|FullyQualifiedName~Formula"
```

Package validator commands: only if package input/sample is introduced.

Runtime scenario commands: not applicable unless M8 has unlocked runtime preview.

## Stop conditions

Stop if:

```text
- M9 gate is not open;
- package generation/validation paths are not stable;
- task needs broad schema changes;
- task requires generated C# code;
- proof tests cannot pin exact deterministic behavior;
- changed files exceed 8;
- implementation would bypass review/apply or validation boundaries.
```

## Non-goals

```text
- Do not implement broad generator orchestration.
- Do not call real LLM/provider.
- Do not change runtime.
- Do not change GamePackage schema unless explicitly approved later.
- Do not claim subjective balance quality without exact assertions.
```

## Expected final report

Final report must include:

```text
- contracts/validators added or changed;
- diagnostic codes;
- fixture/golden list;
- exact proof assertions;
- confirmation no generated C# path was added;
- confirmation runtime/provider/UI untouched;
- check-all result.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M9_002
Task spec file: docs/agent-tasks/M9/M9_002_RANGES.md
Reason: Add numeric range constraints after template contracts exist.
```

On block, write BLOCKERS:

```text
M9_001 blocked: M9 gate not open or exact deterministic proof tests cannot be defined.
```
