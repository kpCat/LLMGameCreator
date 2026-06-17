# README_APPLY_AGENT_TASK_PACK_003.md

Apply from repository root:

```powershell
cd C:\Users\endim\LLMGameCreator
# unzip llmgc_agent_task_pack_003.zip here, replacing existing files

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

This pack only updates `docs/agent-tasks` guidance files and top-level archive notes. It does not change production source, tests, solution, project files, or scripts.

## What to review after applying

```text
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
```

## Recommended next execution cursor

This pack does not forcibly change `.devflow/NEXT_TASK.md`. If you want Kilo to start executing the deterministic M4.1 specs, point `.devflow/NEXT_TASK.md` to `M4_1_004` first:

```text
# NEXT_TASK

Mode: single-task
Task source: agent_task_spec
Task id: M4_1_004
Task spec file: docs/agent-tasks/M4_1/M4_1_004_STRICT_JSON_PARSER_CORPUS_GUARD.md
Reason: Add fixture-driven proof coverage for strict JSON parser behavior before further prompt/repair changes.
User approval: approved
Expected stop after completion: yes
```

Recommended sequence for Kilo after baseline is green:

```text
M4_1_004 -> M4_1_005 -> M4_1_006 -> M4_1_008
```

The new Pack 003 tasks are for gate automation and post-run review. They are useful after Kilo has executed at least one M4.1 implementation spec or after a real evaluation report exists.
