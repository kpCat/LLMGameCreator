# 007_START_RUN.md — M4.1 execution branch start

This file is operator guidance for starting the first coding-agent run after the documentation roadmap freeze.

## Purpose

The roadmap through M10 is now documented as locked planning. The next productive step is not another speculative documentation pack. The next productive step is to run a single M4.1 task on a separate execution branch and review the result.

## Branch policy

Use a dedicated branch created from current `main`.

Recommended branch name:

```text
kilo-m4-1-005
```

Do not run the coding agent directly on `main`.

## Start task

The active task pointer is:

```text
Task source: agent_task_spec
Task id: M4_1_005
Task spec file: docs/agent-tasks/M4_1/M4_1_005_EVALUATION_MARKDOWN_GOLDEN_RECOMMENDATIONS.md
```

`M4_1_005` is selected because the roadmap freeze is complete and the M4.1 deterministic sequence should continue with markdown/golden recommendation coverage before repair-prompt and doc-consistency hardening.

## Required pre-run commands

Run these before starting Kilo/local agent:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Expected state before agent work:

```text
CHECK-ALL PASSED
Devflow state check passed
NEXT_TASK points to M4_1_005 with user approval
```

## Agent scope

The agent must execute exactly one task. It must not continue into `M4_1_006` in the same run.

The task's own allowed/forbidden files are authoritative.

## After the agent stops

Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
```

Then review using:

```text
docs/agent-tasks/M4_1/022_REPORT.md
```

## Stop instead of continuing if

```text
- check-all fails after the agent run;
- agent changed files outside M4_1_005 boundaries;
- agent rewrote broad code unrelated to markdown rendering/tests;
- agent updated CURRENT_GENERATOR_STATE or unlocked M5/M6/M8/M9/M10;
- agent attempted provider/LLM/runtime/GamePackage changes;
- report wording implies automatic M5/M6 unlock.
```
