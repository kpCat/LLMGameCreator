# RECURSIVE_TASK_SELECTION_PROTOCOL.md — cursor-driven task selection without context explosion

Термин “recursive” в этом проекте означает не бесконечную рекурсию, а безопасный cursor-driven loop:

```text
read cursor -> pick one task -> read one phase plan -> execute one task -> verify -> update cursor -> stop or continue if explicitly allowed
```

## Required cursor files

```text
.devflow/NEXT_TASK.md      human-readable next task cursor
.devflow/CURRENT_RUN.md    current task plan/report scratchpad
.devflow/BLOCKERS.md       stop conditions and user decisions needed
.devflow/TASK_GRAPH.json   current gate-level task graph
.devflow/PHASE_PLAN_INDEX.md phase routing
```

## Selection algorithm

1. Read Tier 0 docs from `CONTEXT_BUDGET_POLICY.md`.
2. Read `.devflow/NEXT_TASK.md`.
3. If NEXT_TASK names a task in `.devflow/TASK_GRAPH.json`, use that task.
4. If NEXT_TASK names a phase or phase task card, read `.devflow/PHASE_PLAN_INDEX.md` and then exactly one phase plan file.
5. Choose the first task card in that phase where:

```text
status is ready/proposed and allowed by current gate
blocked_by is empty or already satisfied
requires_approval is false, or user explicitly approved it
```

6. Copy the selected task card into CURRENT_RUN.md as the working task.
7. Execute by AUTONOMOUS_RUNBOOK.md.
8. After successful checks, update NEXT_TASK.md with the next task card id or stop reason.
9. If blocked, update BLOCKERS.md and stop.

## Agent must not do this

```text
- read all phase plans;
- invent a new milestone;
- skip M4.1 gate;
- continue after approval-required task without user approval;
- silently mark a blocked task complete;
- execute more than 1 task unless the prompt explicitly allows up to 3 low-risk tasks.
```

## NEXT_TASK.md recommended shape

```text
# NEXT_TASK

Mode: single-task
Task source: task_graph | phase_plan
Task id: BASELINE-001
Phase plan file: .devflow/phase-plans/00_DEVFLOW_BASELINE.md
Reason:
User approval:
Expected stop after completion: yes
```

## Phase task card shape

```text
## TASK-ID — Title

Status:
Objective:
Allowed before M4.1 gate:
Requires approval:
Source docs:
Target areas:
Non-goals:
Implementation notes:
Required checks:
Stop on:
Next candidate:
```
