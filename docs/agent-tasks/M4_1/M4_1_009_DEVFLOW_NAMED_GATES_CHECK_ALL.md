# M4_1_009 — Optional named devflow gates for check-all

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_009`

Milestone: `M4.1 real-model evaluation gate`

Status: `proposal_requires_user_approval`

Depends on:

```text
- check-all currently passes without this change.
- User explicitly approves editing .devflow scripts.
```

Unlocks:

```text
- More granular local-agent verification runs without weakening default check-all behavior.
```

Risk level: medium

Expected changed files count: 2-5

## Gate status

Allowed before current gate review: yes, because this is devflow-only automation.

Requires user approval: yes.

Approval text required in NEXT_TASK.md:

```text
User approval: approved for devflow script changes only
```

## Source of truth

Source-of-truth docs:

```text
.devflow/scripts/README.md
.devflow/scripts/check-all.ps1
.devflow/scripts/check-devflow-state.ps1
.devflow/VERIFICATION_MATRIX.md
docs/agent-tasks/_SYSTEM_GATES.md
```

Context budget:

```text
Read only this task spec, the listed scripts/docs, and at most the latest check-all run log if needed.
```

Do not read:

```text
- src/**
- tests/**
- all docs/**
- phase plans unrelated to M4.1
```

Existing patterns to inspect:

```text
.devflow/scripts/check-all.ps1
.devflow/scripts/_common.ps1
.devflow/scripts/check-devflow-state.ps1
.devflow/scripts/README.md
```

## File boundaries

Allowed files:

```text
.devflow/scripts/check-all.ps1
.devflow/scripts/check-devflow-state.ps1
.devflow/scripts/README.md
.devflow/VERIFICATION_MATRIX.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/**
tests/**
LLMGameCreator.sln
*.csproj
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
```

Deleted files: none

## API / implementation contract

New interfaces: none.

New classes: none.

New script parameters, only if compatible with existing PowerShell style:

```text
-GateProfile baseline | m4_1 | full
-SkipOptionalGates
```

Do not remove existing parameters.

Do not change default behavior of `check-all.ps1`.

## Exact behavior

Default behavior:

```text
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

must remain equivalent to the current baseline:

```text
- environment info;
- devflow state check;
- restore unless skipped;
- build;
- known-warning analysis;
- tests unless skipped;
- summary/run directory.
```

`-GateProfile baseline`:

```text
Equivalent to default required baseline checks. Must not weaken default behavior.
```

`-GateProfile m4_1`:

```text
Runs baseline plus any available M4.1 focused checks that already exist. If no dedicated M4.1 scripts exist, report that optional gates were not available; do not fail only because optional gates are absent.
```

`-GateProfile full`:

```text
Runs baseline plus all available named optional gates. Missing optional gate scripts are reported as skipped, not as pass.
```

Failure behavior:

```text
- Required baseline failure -> check-all fails.
- Optional gate script exists and fails -> check-all fails for m4_1/full profile.
- Optional gate script missing -> check-all reports skipped optional gate.
```

Diagnostic/report labels:

```text
devflow.gate.required_failed
devflow.gate.optional_failed
devflow.gate.optional_skipped
devflow.gate.profile_unknown
```

## Proof tests

Tests to add before/with implementation:

```text
- Manual script proof: default check-all still passes.
- Manual script proof: check-all -GateProfile baseline passes with same required checks.
- Manual script proof: check-all -GateProfile m4_1 reports optional gate availability/skips without hiding baseline failures.
- Negative proof: unknown GateProfile returns non-zero and prints devflow.gate.profile_unknown.
```

No C# tests are required for this devflow-script task unless a script-test harness already exists.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1 -GateProfile baseline
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1 -GateProfile m4_1
```

Docs consistency commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
```

## Stop conditions

Stop if:

```text
- default check-all behavior must be weakened;
- more than 5 files are needed;
- implementation requires src/ or tests/ changes;
- scripts become unable to run on Windows PowerShell;
- the task needs CI/workflow configuration changes;
- GateProfile design becomes ambiguous.
```

## Non-goals

```text
- Do not add GitHub Actions workflows.
- Do not add new scripts for every future gate.
- Do not implement docs/manifests/runtime/snapshot gates here unless they already exist.
- Do not alter known-warning policy.
```

## Expected final report

Final report must include:

```text
- changed script/docs files;
- exact commands run;
- default check-all result;
- GateProfile baseline result;
- GateProfile m4_1 result;
- skipped optional gates if any;
- next recommended cursor.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_012
Task spec file: docs/agent-tasks/M4_1/M4_1_012_OVERNIGHT_RUN_REPORT_REVIEW_GATE.md
Reason: Review local-agent run results through a formal gate before continuing.
```
