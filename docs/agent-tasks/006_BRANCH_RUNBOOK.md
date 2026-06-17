# 006_BRANCH_RUNBOOK.md — execution branch runbook for Kilo/local agents

This runbook is for the human operator before running Kilo/local-agent implementation tasks.

## Baseline branch model

Use `main` as the documentation baseline.

Create a separate execution branch for coding-agent work.

Recommended pattern:

```powershell
cd C:\Users\endim\LLMGameCreator
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
git checkout -b kilo-m4-1-005
```

The coding agent must not use git commands. The human controls commit/push/revert.

## Before each Kilo task

1. Confirm `check-all.ps1` passes.
2. Confirm `.devflow/NEXT_TASK.md` points to exactly one task.
3. Paste the corresponding prompt from `docs/agent-tasks/M4_1/019_KILO_PROMPTS.md`.
4. Tell Kilo to stop after exactly one task.
5. Forbid broad repo read, git commands, dependency installs, and VS Designer edits.

## After each Kilo task

Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
```

Then review:

```text
- changed files match Allowed files in the task spec;
- proof tests assert exact diagnostic codes / exact golden output;
- no generated .devflow/runs/** artifacts are committed;
- no M5/M6/M8/M9/M10 production work was touched;
- CURRENT_RUN has a useful report;
- NEXT_TASK suggests only the next allowed task.
```

## Branch outcome policy

If the task is clean:

```text
push the execution branch and ask for review.
```

If the task is messy but repairable:

```text
stop feature progression and request a focused repair/hardening pack.
```

If the task violates boundaries:

```text
do not merge; either revert branch or create a new smaller task branch.
```

## Batch size rule

Do not run the entire roadmap in one unattended session at first.

Preferred first execution wave:

```text
M4_1_005 only
```

If clean, next wave may be:

```text
M4_1_006 only
```

Then:

```text
M4_1_008 only
```

Only after several clean single-task runs should overnight multi-task execution be considered.
