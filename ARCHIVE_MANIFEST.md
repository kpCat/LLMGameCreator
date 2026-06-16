# ARCHIVE_MANIFEST.md — llmgc_agent_task_pack_003

Archive: `llmgc_agent_task_pack_003.zip`

Purpose:

```text
Add the third agent-task pack: M4.1 gate/automation executable task specs and cursor-ready guidance, without changing production source, tests, solution files, project files, or current M5/M6 lock status.
```

This archive is docs/task-spec guidance only. It does not modify `src/`, `tests/`, `.sln`, `.csproj`, `.devflow/scripts/`, package schema, runtime behavior, Lua execution, or provider integration.

## Files included

```text
README_APPLY_AGENT_TASK_PACK_003.md
ARCHIVE_MANIFEST.md

docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md

docs/agent-tasks/M4_1/000_M4_1_SEQUENCE.md
docs/agent-tasks/M4_1/M4_1_009_DEVFLOW_NAMED_GATES_CHECK_ALL.md
docs/agent-tasks/M4_1/M4_1_010_REAL_EVALUATION_ARTIFACT_DISCOVERY.md
docs/agent-tasks/M4_1/M4_1_011_CURRENT_STATE_GATE_REVIEW_UPDATE.md
docs/agent-tasks/M4_1/M4_1_012_OVERNIGHT_RUN_REPORT_REVIEW_GATE.md
```

## Repository-state assumptions used

```text
Branch reviewed: kilo-night-001
Compared to main: ahead by 2 commits, docs/devflow/task-spec changes only.
Current phase remains M4.1 real-model evaluation gate.
M5/M6/M8 production work remains locked until current-state docs explicitly unlock it.
No GitHub workflow runs were found for the latest pushed commit, so local check-all remains the required verification.
```

## Safety

```text
- No src/ changes.
- No tests/ changes.
- No .sln changes.
- No .csproj changes.
- No .devflow/scripts changes.
- No M5/M6 unlock.
- No forced NEXT_TASK.md cursor change.
```
