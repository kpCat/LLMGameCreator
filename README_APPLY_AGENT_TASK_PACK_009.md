# README_APPLY_AGENT_TASK_PACK_009.md

Pack id: `agent-task-pack-009-locked-m8-runtime-preview-drafts`

## Purpose

This pack adds locked M8 runtime-preview draft task specs.

It is documentation-only planning. It does **not** unlock runtime preview work, package repair loops, M5, M6, M9, or M10.

## Apply

Unzip this archive into the repository root with replacement:

```powershell
cd C:\Users\endim\LLMGameCreator
# unzip into repo root with overwrite

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Files changed

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/M8/000_M8_SEQUENCE.md

docs/agent-tasks/M8/M8_001_LOAD_SMOKE.md
docs/agent-tasks/M8/M8_002_COMMAND_SCENARIO.md
docs/agent-tasks/M8/M8_003_SNAPSHOT_GUARD.md
docs/agent-tasks/M8/M8_004_NO_MUTATION.md
docs/agent-tasks/M8/M8_005_DIAGNOSTIC_REPORT.md
```

## Explicit non-goals

```text
- No src/ changes.
- No tests/ changes.
- No .sln/.csproj changes.
- No .devflow/scripts changes.
- No Runtime implementation changes.
- No GamePackage schema changes.
- No M8 unlock.
- No M5/M6/M9/M10 unlock.
```

## Gate status

M8 remains locked until:

```text
- a validated assembled sample GamePackage exists;
- package validation is green;
- runtime boundaries remain clean;
- current-state docs or explicit gate decision select runtime preview validation as next work.
```
