# M4_1_017 — M4.1 completion checklist

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_017`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_near_m4_1_completion`

Depends on:

```text
- At least one M4.1 execution path has been completed.
- User wants a final checklist before moving to the next phase or stopping M4.1.
```

Unlocks:

```text
- Safer next pack generation.
```

Risk level: low

Expected changed files count: 2-4

## Gate status

Allowed before current gate review: yes

Requires user approval: yes, because it writes final checklist docs.

Approval text required in NEXT_TASK.md:

```text
User approval: approved
```

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
docs/agent-tasks/004_PACK_GENERATION_POLICY.md
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
```

Context budget:

```text
Read current state, M4.1 sequence, roadmap/policy, and any existing M4.1 evidence/closure docs. Do not read all task specs.
```

Existing patterns to inspect:

```text
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
```

## File boundaries

Allowed files:

```text
docs/M4_1_COMPLETION_CHECKLIST.md
docs/agent-tasks/M4_1/M4_1_017_M4_1_COMPLETION_CHECKLIST.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
.devflow/BLOCKERS.md
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

Create a checklist that verifies M4.1 readiness to stop, repair, or proceed.

Checklist must include:

```text
- current-state agreement check;
- check-all green check;
- proof-test coverage summary;
- real evaluation evidence status;
- local-agent execution quality status;
- remaining blockers;
- M5/M6 lock/unlock status;
- next pack recommendation.
```

Failure behavior:

```text
If required evidence is missing, checklist should mark it missing and route to a blocker/next task. Do not mark completion by assumption.
```

Diagnostic codes: none.

Validation rules:

```text
- Checklist must not unlock phases.
- Checklist must distinguish pass/repair/block states.
- Checklist must not replace CURRENT_GENERATOR_STATE as source of truth.
```

Security/sandbox rules: no provider calls.

Persistence rules: docs only.

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
No code tests; checklist documentation task.
```

Required proof checks:

```text
- docs/M4_1_COMPLETION_CHECKLIST.md exists.
- It includes sections: Current state, Check-all, Proof tests, Real evidence, Local-agent quality, Blockers, Phase lock status, Next pack.
- It does not claim pass unless current-state docs already say pass.
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

## Stop conditions

Stop if:

```text
- task would require source/test/project/script changes;
- current-state docs are missing or inconsistent;
- user expects gate pass to be inferred without decision;
- check-all fails after 2 repair attempts.
```

## Non-goals

```text
- Do not update current state.
- Do not unlock M5/M6.
- Do not implement production code.
- Do not generate next phase specs.
```

## Expected final report

Final report must include:

```text
- checklist file path;
- missing evidence/blockers;
- phase lock status observed;
- check-all result;
- next pack recommendation.
```

## Next task pointer

If M4.1 is passed in current-state docs:

```text
Next pack: agent-task-pack-007-m5-entry-executable-specs
```

If M4.1 is not passed:

```text
Task source: agent_task_spec
Task id: M4_1_013 or M4_1_014 or M4_1_016
Reason: Complete missing M4.1 runbook/evidence/decision path.
User approval: required
```

On block, write BLOCKERS.
