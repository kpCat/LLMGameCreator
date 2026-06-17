# M4_1_016 — M4 gate closure decision

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_016`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_after_user_gate_decision`

Depends on:

```text
- User explicitly chooses one gate status: passed, needs_repair, or blocked.
- Evidence manifest or gate decision report exists, or user explicitly accepts that evidence is manual/out-of-band.
- check-all is green.
```

Unlocks:

```text
- If passed: Pack generation may move to M5 entry executable specs.
- If needs_repair: next M4.1 repair task/spec.
- If blocked: stop until missing evidence or blocker is resolved.
```

Risk level: high, because this may unlock or block the next phase.

Expected changed files count: 3-6

## Gate status

Allowed before current gate review: yes

Requires user approval: yes, explicit status is mandatory.

Approval text required in NEXT_TASK.md:

```text
User approval: approved
Gate decision: passed | needs_repair | blocked
```

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/M4_1_REAL_EVALUATION_EVIDENCE_MANIFEST.md
docs/M4_1_REAL_EVALUATION_RUNBOOK.md
docs/agent-tasks/003_DEVELOPMENT_ROADMAP.md
```

Context budget:

```text
Read only current state docs, evidence/decision docs, this task spec, and the next-pack policy docs.
```

Existing patterns to inspect:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
```

## File boundaries

Allowed files:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/M4_1_GATE_CLOSURE_DECISION.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
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
docs/agent-tasks/M5/**
docs/agent-tasks/M6/**
```

Deleted files: none.

## API / implementation contract

New interfaces: none.

New classes: none.

New methods: none.

Public contracts changed: no.

Schema changed: current-state JSON content may change, but no schema format change.

New dependencies: no.

## Exact behavior

Update current-state docs only according to the explicit user gate decision.

Allowed decisions:

```text
passed
needs_repair
blocked
```

For `passed`:

```text
- mark M4.1 as passed in current state;
- allow next pack generation for M5 entry specs;
- do not implement M5 in this task.
```

For `needs_repair`:

```text
- keep M5/M6 locked;
- document repair reason and suggested next M4.1 task;
- do not pretend gate passed.
```

For `blocked`:

```text
- keep M5/M6 locked;
- record blocker and missing evidence;
- route next work to blocker resolution.
```

Failure behavior:

```text
If user decision is missing or ambiguous, stop and write BLOCKERS.md. Do not infer pass/fail from partial evidence.
```

Diagnostic codes: none.

Validation rules:

```text
- markdown and JSON state must agree.
- task pack docs cannot unlock phases by themselves.
- if M4.1 passes, NEXT_PACK_REQUEST may request M5 entry executable specs.
- if not passed, NEXT_PACK_REQUEST must keep M5/M6 locked.
```

Security/sandbox rules: no provider calls.

Persistence rules: docs/state only.

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
No code tests; docs/state consistency task.
```

Required proof checks:

```text
- docs/CURRENT_GENERATOR_STATE.md and .json agree on current_phase/gate status.
- docs/M4_1_GATE_CLOSURE_DECISION.md records user decision and evidence summary.
- docs/agent-tasks/002_NEXT_PACK_REQUEST.md points to the correct next pack type for the decision.
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
- explicit user gate decision is missing;
- evidence summary conflicts with user decision and needs clarification;
- current-state JSON format is unclear;
- task would require source/test/project/script changes;
- check-all fails after 2 repair attempts.
```

## Non-goals

```text
- Do not implement M5/M6.
- Do not edit M5/M6 task specs.
- Do not run real evaluation.
- Do not decide the gate without user decision.
```

## Expected final report

Final report must include:

```text
- user gate decision recorded;
- changed current-state files;
- whether M5 remains locked or next pack may generate M5 entry specs;
- check-all result;
- next task/pack recommendation.
```

## Next task pointer

If decision is `passed`, suggest next pack rather than executing a task:

```text
Next pack: agent-task-pack-007-m5-entry-executable-specs
Reason: M4.1 passed; generate M5 entry executable specs from current source.
```

If decision is `needs_repair`, suggest:

```text
Task source: agent_task_spec
Task id: M4_1_006 or M4_1_003
Reason: Repair strict generation/evaluation weakness identified by M4.1 gate evidence.
User approval: required
```

If decision is `blocked`, write BLOCKERS.
