# README_APPLY_AGENT_TASK_PACK_002.md

Apply from repository root:

```powershell
cd C:\Users\endim\LLMGameCreator
# unzip llmgc_agent_task_pack_002.zip here, replacing existing files

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

This pack only updates `docs/agent-tasks` guidance files. It does not change production source, tests, solution, or project files.

## What to do after applying

Review:

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
```

The pack does not forcibly change `.devflow/NEXT_TASK.md`. To let Kilo execute one of these specs, point `NEXT_TASK.md` to a specific `agent_task_spec`, for example:

```text
# NEXT_TASK

Mode: single-task
Task source: agent_task_spec
Task id: M4_1_004
Task spec file: docs/agent-tasks/M4_1/M4_1_004_STRICT_JSON_PARSER_CORPUS_GUARD.md
Reason: Add fixture-driven proof coverage for strict JSON parser behavior before changing prompts/repair.
User approval: approved
Expected stop after completion: yes
```

Recommended first executable task from this pack: `M4_1_004`.
