# 00_DEVFLOW_BASELINE.md — baseline and local-agent bootstrap

Read only when NEXT_TASK points to baseline/devflow setup.

## Goal

Make sure the repository and `.devflow` are operational before asking a local agent to touch production code.

## TASK BASELINE-001 — Run baseline check-all

Status: ready if baseline not recorded.

Objective: run `.devflow/scripts/check-all.ps1`, record result path and warnings.

Allowed before M4.1 gate: yes.
Requires approval: no.

Source docs:

```text
.devflow/AUTONOMOUS_RUNBOOK.md
.devflow/VERIFICATION_MATRIX.md
.devflow/DEFINITION_OF_DONE.md
```

Target areas:

```text
.devflow/CURRENT_RUN.md
.devflow/BLOCKERS.md
.devflow/runs/
```

Non-goals:

```text
- no production code changes;
- no solution/csproj changes;
- no warning suppression;
- no git commands.
```

Required checks:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Next candidate: DEVFLOW-001 if script/devflow issues exist; otherwise M4EVAL-001 if a real evaluation report exists, or SIM-001/OBS-001 only after user approval.

## TASK DEVFLOW-001 — Harden devflow scripts/docs only

Status: ready only if baseline shows devflow script/documentation issue.

Objective: adjust only `.devflow` scripts/docs to make baseline checks reliable.

Allowed before M4.1 gate: yes.
Requires approval: no, if only `.devflow` files change.

Target areas:

```text
.devflow/scripts/
.devflow/*.md
.devflow/*.json
```

Non-goals:

```text
- no src/ changes;
- no tests/ changes;
- no solution/csproj changes;
- no hiding real build/test failures.
```

Required checks:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Stop on:

```text
need_to_change_production_code
need_to_weaken_checks
need_to_change_solution_or_projects
```
