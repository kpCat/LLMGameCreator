# M4_1_012 — Overnight/local-agent run report review gate

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_012`

Milestone: `M4.1 local-agent execution quality gate`

Status: `ready_when_local_agent_run_exists`

Depends on:

```text
- A local-agent/Kilo run changed files or produced .devflow/OVERNIGHT_RUN_REPORT.md / CURRENT_RUN.md.
- User wants a structured review before continuing.
```

Unlocks:

```text
- Continue to next M4.1 task if run quality is acceptable.
- Stop/revert/refine specs if run quality is poor.
```

Risk level: low

Expected changed files count: 1-3

## Gate status

Allowed before current gate review: yes.

Requires user approval: no, if limited to reporting and cursor update.

## Source of truth

Source-of-truth docs:

```text
.devflow/RUN_REPORT_TEMPLATE.md
.devflow/DEFINITION_OF_DONE.md
.devflow/LOCAL_AGENT_REVIEW_CHECKLIST.md
.devflow/STOP_CONDITIONS.md
docs/agent-tasks/000_INDEX.md
```

Context budget:

```text
Read only this task spec, latest CURRENT_RUN/BLOCKERS/NEXT_TASK/OVERNIGHT_RUN_REPORT, changed file list, and the latest check-all run directory summary.
```

Do not read:

```text
- full src tree;
- full tests tree;
- all run logs;
- unrelated phase plans;
- M5/M6 specs unless the run touched them.
```

Existing patterns to inspect:

```text
.devflow/CURRENT_RUN.md
.devflow/BLOCKERS.md
.devflow/NEXT_TASK.md
.devflow/RUN_REPORT_TEMPLATE.md
```

## File boundaries

Allowed files:

```text
.devflow/CURRENT_RUN.md
.devflow/BLOCKERS.md
.devflow/NEXT_TASK.md
.devflow/OVERNIGHT_RUN_REPORT.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
```

Forbidden files:

```text
src/**
tests/**
LLMGameCreator.sln
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces: none.

New classes: none.

This is a review/report/cursor task only.

## Exact behavior

Review the local-agent run against these gates:

```text
1. check-all passed after the final task.
2. Changed files stayed inside allowed task-spec boundaries.
3. No forbidden files were modified.
4. Each executed task has a proof test or an explicit blocked reason.
5. No schema/dependency/project/runtime boundary was changed without approval.
6. NEXT_TASK points to an allowed next task or stop reason.
7. BLOCKERS.md is empty or clear.
8. Report includes repair attempts and risks.
```

Quality classification:

```text
pass: small bounded diff, check-all green, cursor/report sane.
repair: minor documentation/cursor/report issue, no production risk.
fail: forbidden files touched, check-all red, scope expanded, or hidden failure.
```

Failure behavior:

```text
- fail -> write BLOCKERS.md and recommend revert/review.
- repair -> update cursor/report and stop.
- pass -> set next cursor to the next allowed M4.1 spec, or stop if user review needed.
```

Diagnostic/report labels:

```text
local_agent.run.pass
local_agent.run.needs_repair
local_agent.run.fail
local_agent.run.forbidden_files
local_agent.run.check_all_missing
local_agent.run.cursor_invalid
```

## Proof tests

Proof checks:

```text
- latest check-all run directory exists or final report states why not.
- changed file list is recorded in OVERNIGHT_RUN_REPORT.md or CURRENT_RUN.md.
- forbidden file check result is recorded.
- NEXT_TASK validity is recorded.
```

No production tests are required because this task does not change production code.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Review commands suggested for user/operator:

```powershell
git status
git diff --stat
```

Agent must not run git commands unless the user explicitly permits it. If git commands are forbidden, user/operator provides changed file list.

## Stop conditions

Stop if:

```text
- changed file list is unavailable and git commands are not allowed;
- final check-all was not run and cannot be run;
- forbidden files were modified;
- local-agent run changed more than 24 files;
- report/cursor is inconsistent;
- M5/M6 were touched while M4.1 is locked.
```

## Non-goals

```text
- Do not fix the local-agent implementation diff here.
- Do not merge/revert branches.
- Do not unlock M5/M6.
- Do not rewrite task specs broadly.
```

## Expected final report

Final report must include:

```text
- pass/repair/fail classification;
- changed files summary;
- check-all result/run directory;
- forbidden file result;
- cursor validity;
- blockers;
- next recommended action.
```

## Next task pointer

If pass and M4.1 still active:

```text
Task source: agent_task_spec
Task id: next allowed M4_1 task from docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
Task spec file: matching spec path
Reason: Continue bounded M4.1 hardening after successful local-agent run review.
```

If fail:

```text
Stop: local-agent run failed quality gate; user review/revert needed.
```
