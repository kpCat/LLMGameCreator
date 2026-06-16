# RECURSIVE_TASK_SELECTION_PROTOCOL.md — cursor-driven task selection without context explosion

Термин “recursive” в этом проекте означает не бесконечную рекурсию, а безопасный cursor-driven loop:

```text
read cursor -> pick one task -> read one task source -> execute one task -> verify -> update cursor -> stop or continue if explicitly allowed
```

## Required cursor files

```text
.devflow/NEXT_TASK.md                    human-readable next task cursor
.devflow/CURRENT_RUN.md                  current task plan/report scratchpad
.devflow/BLOCKERS.md                     stop conditions and user decisions needed
.devflow/TASK_GRAPH.json                 current gate-level task graph
.devflow/PHASE_PLAN_INDEX.md             phase routing
docs/agent-tasks/000_INDEX.md            executable agent task spec routing
```

## Selection algorithm

1. Read Tier 0 docs from `CONTEXT_BUDGET_POLICY.md`.
2. Read `.devflow/NEXT_TASK.md`.
3. Determine `Task source`.

### If Task source is `task_graph`

1. Find `Task id` in `.devflow/TASK_GRAPH.json`.
2. Check status, blocked_by, requires_approval, current gate and required checks.
3. Copy the selected task into `.devflow/CURRENT_RUN.md` as the working task.
4. Execute by `.devflow/AUTONOMOUS_RUNBOOK.md`.

### If Task source is `phase_plan`

1. Read `.devflow/PHASE_PLAN_INDEX.md`.
2. Read exactly one phase plan file.
3. Choose the first task card in that phase where:

```text
status is ready/proposed and allowed by current gate
blocked_by is empty or already satisfied
requires_approval is false, or user explicitly approved it
```

4. Copy the selected task card into `.devflow/CURRENT_RUN.md` as the working task.
5. Execute by `.devflow/AUTONOMOUS_RUNBOOK.md`.

### If Task source is `agent_task_spec`

1. Read `docs/agent-tasks/000_INDEX.md`.
2. Read exactly one task spec file named by `.devflow/NEXT_TASK.md`.
3. Do not read sibling task specs.
4. Check:

```text
Status
Depends on
Allowed before current gate review
Requires user approval
Allowed files
Forbidden files
Proof tests
System gates
Stop conditions
```

5. If the task spec is locked, blocked, missing proof tests, or requires approval not granted by the user, stop and update `.devflow/BLOCKERS.md`.
6. Read only source docs and existing patterns listed by the task spec.
7. Copy the task spec summary into `.devflow/CURRENT_RUN.md` as the working task.
8. Execute by `.devflow/AUTONOMOUS_RUNBOOK.md`, `.devflow/DEFINITION_OF_DONE.md`, and the task spec itself.
9. After successful checks, update `.devflow/NEXT_TASK.md` with the task spec `Next task pointer`.

## After execution

After successful checks:

```text
- update .devflow/CURRENT_RUN.md;
- update .devflow/NEXT_TASK.md;
- write final report by .devflow/RUN_REPORT_TEMPLATE.md;
- stop unless prompt explicitly allows continuing.
```

If blocked:

```text
- update .devflow/BLOCKERS.md;
- do not continue to another task;
- do not invent a workaround.
```

## Agent must not do this

```text
- read all phase plans;
- read all docs/agent-tasks specs;
- invent a new milestone;
- skip M4.1 gate;
- continue after approval-required task without user approval;
- silently mark a blocked task complete;
- execute more than 1 task unless the prompt explicitly allows up to 3 low-risk tasks;
- execute future locked task specs because they exist as files.
```

## NEXT_TASK.md recommended shape

```text
# NEXT_TASK

Mode: single-task
Task source: task_graph | phase_plan | agent_task_spec
Task id: BASELINE-001
Task spec file:
Phase plan file: .devflow/phase-plans/00_DEVFLOW_BASELINE.md
Reason:
User approval:
Expected stop after completion: yes
```

## Agent task spec pointer shape

```text
# NEXT_TASK

Mode: single-task
Task source: agent_task_spec
Task id: M4_1_002
Task spec file: docs/agent-tasks/M4_1/M4_1_002_STRICT_OUTPUT_CORPUS_FIXTURES.md
Phase plan file:
Reason: Add proof-test corpus coverage for strict LLM output handling.
User approval: approved for M4_1_002 corpus fixture task
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
