# M4_1_011 — Current-state update after explicit M4.1 gate review

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_011`

Milestone: `M4.1 real-model evaluation gate`

Status: `requires_explicit_user_decision`

Depends on:

```text
- User has explicitly reviewed M4.1 evidence.
- User states one of: gate_passed, gate_needs_repair, gate_blocked.
- check-all is green before docs update.
```

Unlocks:

```text
- If gate_passed: next pack may generate M5 entry execution specs.
- If gate_needs_repair: next cursor remains M4.1 hardening.
- If gate_blocked: no M5/M6 unlock.
```

Risk level: high for project direction, low for docs-only patch.

Expected changed files count: 2-5

## Gate status

Allowed before current gate review: no. This task is the gate review update.

Requires user approval: yes.

Approval text required in NEXT_TASK.md:

```text
User approval: approved; M4.1 decision = gate_passed | gate_needs_repair | gate_blocked
```

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
```

Context budget:

```text
Read only current-state docs, roadmap M4.1/M5 sections, ledger, and the final evaluation/run report being used for the decision.
```

Existing patterns to inspect:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/ROADMAP_TO_FULL_GENERATOR.md
```

## File boundaries

Allowed files:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
.devflow/NEXT_TASK.md
.devflow/CURRENT_RUN.md
```

Forbidden files:

```text
src/**
tests/**
LLMGameCreator.sln
*.csproj
.devflow/scripts/**
```

Deleted files: none

## API / implementation contract

New interfaces: none.

New classes: none.

New methods: none.

This is a docs/state task only.

## Exact behavior

If user decision is `gate_passed`:

```text
- Update CURRENT_GENERATOR_STATE.md/.json active manual gate to completed/pass state.
- Remove M5/M6 from blocked_next_milestones_until_gate_passes if and only if user explicitly approved pass.
- Set recommended next decision/task to M5 entry planning/executable specs.
- Update roadmap notes only if roadmap currently marks M5/M6 blocked solely by M4.1.
- Update NEXT_TASK to stop or to a user-approved M5 entry task spec if it exists.
```

If user decision is `gate_needs_repair`:

```text
- Keep M5/M6 blocked.
- Set recommended next task to M4_1_003 or specific repair/hardening spec.
- Record reason/evidence from report.
```

If user decision is `gate_blocked`:

```text
- Keep M5/M6 blocked.
- Record blocker and required user/manual action.
```

Failure behavior:

```text
- Missing explicit user decision -> stop.
- Mismatch between .md and .json update -> stop/fix before final.
```

Validation rules:

```text
- CURRENT_GENERATOR_STATE.md and .json must agree on current phase/gate/blocked milestones.
- Do not unlock future phases from task-pack docs alone.
- Do not claim real evaluation passed unless user explicitly said so.
```

## Proof tests

Proof checks:

```text
- Manual diff shows CURRENT_GENERATOR_STATE.md and .json updated together.
- check-all passes after docs update.
- NEXT_TASK points to an allowed next task or explicit stop reason.
- Ledger records the gate decision source.
```

If docs consistency test exists, run it. If not, `check-all.ps1` is required.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Docs consistency commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
```

## Stop conditions

Stop if:

```text
- user decision is missing or ambiguous;
- evaluation evidence is missing;
- .md and .json would disagree;
- change would unlock M5/M6 without explicit user pass decision;
- more than 5 files need updates;
- production code changes appear necessary.
```

## Non-goals

```text
- Do not implement M5/M6.
- Do not change source code.
- Do not fabricate evaluation results.
- Do not run real LLM evaluation.
```

## Expected final report

Final report must include:

```text
- user decision used;
- evidence file/report used;
- state files changed;
- blocked/unblocked milestones;
- next cursor;
- checks run.
```

## Next task pointer

If gate passed:

```text
Stop: request next generated pack for M5 entry execution specs.
```

If gate needs repair:

```text
Task source: agent_task_spec
Task id: M4_1_003
Task spec file: docs/agent-tasks/M4_1/M4_1_003_REPAIR_POLICY_HARDENING.md
Reason: M4.1 gate needs repair/hardening before M5.
```

If blocked:

```text
Stop: M4.1 gate blocked pending user/manual action.
```
