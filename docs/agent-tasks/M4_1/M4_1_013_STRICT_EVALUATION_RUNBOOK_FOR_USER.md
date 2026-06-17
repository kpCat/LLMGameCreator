# M4_1_013 — Strict evaluation runbook for user

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_013`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_with_user_approval`

Depends on:

```text
- Pack 004 shared quality docs are present.
- check-all is green or baseline warnings are known.
- User wants a manual/user-facing runbook for real strict evaluation.
```

Unlocks:

```text
- M4_1_014 real evaluation evidence manifest.
```

Risk level: low

Expected changed files count: 2-4

## Gate status

Allowed before current gate review: yes

Requires user approval: yes, because this creates/updates repository documentation that guides manual evaluation.

Approval text required in NEXT_TASK.md:

```text
User approval: approved
```

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
docs/agent-tasks/_DIFF_HYGIENE_RULES.md
```

Context budget:

```text
Read this task spec, the strict evaluation/generation docs, and only source files needed to identify actual UI/service entry points. Do not read all source.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/*Evaluation*.cs
src/LLMGameCreator.WinForms/Pages/Generation/*Strict*  (only if needed to identify user workflow)
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
```

## File boundaries

Allowed files:

```text
docs/M4_1_REAL_EVALUATION_RUNBOOK.md
docs/agent-tasks/M4_1/M4_1_013_STRICT_EVALUATION_RUNBOOK_FOR_USER.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Allowed source-code reads only, no edits:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/**
src/LLMGameCreator.WinForms/Pages/Generation/**
```

Forbidden files:

```text
src/**
tests/**
*.sln
*.csproj
.devflow/scripts/**
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
```

Deleted files: none.

## API / implementation contract

New interfaces: none.

New classes: none.

New methods: none.

Public contracts changed: no.

Schema changed: no.

New dependencies: no.

## Exact behavior

Create or update a user-facing runbook that explains how the user should manually run/collect M4.1 real strict evaluation evidence.

The runbook must include:

```text
- purpose of the M4.1 real evaluation gate;
- prerequisites;
- exact files/locations where outputs should be saved;
- what evidence is required for pass/needs_repair/blocked;
- what must not be done automatically by agents;
- how to request the next task after evidence exists;
- how to keep real model calls outside automated tests;
- how to redact/minimize evidence before committing fixtures.
```

Failure behavior:

```text
If actual evaluation entry point cannot be identified from docs/source within context budget, write a runbook section named "Unknown/needs user confirmation" instead of inventing commands.
```

Diagnostic codes: none.

Validation rules:

```text
- The runbook must not claim M4.1 passed.
- The runbook must not unlock M5/M6.
- The runbook must not instruct tests to call real LLM/provider.
- The runbook must distinguish manual real evaluation from fake/corpus automated tests.
```

Security/sandbox rules:

```text
No real LLM/provider calls during this task.
```

Persistence rules:

```text
Docs only.
```

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
No code tests. This is a docs-only task.
```

Required proof checks:

```text
- docs/M4_1_REAL_EVALUATION_RUNBOOK.md exists.
- It includes sections: Purpose, Prerequisites, Manual run, Evidence files, Gate decision, Redaction, Non-goals, Next task.
- It states M5/M6 remain locked until current-state docs explicitly pass M4.1.
- check-all passes.
```

Required pass tests: not applicable.

Required fail/reject tests: not applicable.

Regression tests: not applicable.

Golden/snapshot fixtures: not applicable.

Fake/corpus requirements: not applicable.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Docs consistency commands: `check-all.ps1` unless dedicated docs gate exists.

## Stop conditions

Stop if:

```text
- source/docs do not reveal enough to write a truthful runbook;
- task would require source/test/project/script changes;
- user expects agent to run real LLM evaluation automatically;
- check-all fails after 2 repair attempts.
```

## Non-goals

```text
- Do not run real LLM evaluation.
- Do not import/analyze real reports.
- Do not update current state gate result.
- Do not unlock M5/M6.
- Do not change production code.
```

## Expected final report

Final report must include:

```text
- runbook file path;
- actual source/docs inspected;
- sections created;
- unknowns needing user confirmation;
- check-all result;
- next task pointer.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_014
Task spec file: docs/agent-tasks/M4_1/M4_1_014_REAL_EVALUATION_EVIDENCE_MANIFEST.md
Reason: Record real/manual strict evaluation evidence in a bounded manifest before report import or gate closure.
User approval: required
```

On block, write BLOCKERS.
