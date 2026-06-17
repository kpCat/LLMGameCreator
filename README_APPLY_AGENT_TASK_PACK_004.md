# README_APPLY_AGENT_TASK_PACK_004.md

Apply from repository root:

```powershell
cd C:\Users\endim\LLMGameCreator
# unzip llmgc_agent_task_pack_004.zip here, replacing existing files

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

This pack is **documentation/devflow guidance only**. It does not modify production source, tests, scripts, solution files, project files, package schema, Lua execution, or M5/M6 state.

## Purpose

Pack 004 hardens the shared quality rules that every local agent reaches through the normal read chain.

The goal is to avoid repeating the same quality requirements in every task prompt. A local agent should naturally reach these rules by reading:

```text
.devflow/AUTONOMOUS_RUNBOOK.md
.devflow/CODE_QUALITY_AND_STYLE.md
.devflow/DEFINITION_OF_DONE.md
.devflow/LOCAL_AGENT_REVIEW_CHECKLIST.md
docs/agent-tasks/000_INDEX.md
one referenced task spec
```

## Added shared rules

```text
docs/agent-tasks/_TEST_QUALITY_RULES.md
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
docs/agent-tasks/_DIFF_HYGIENE_RULES.md
docs/agent-tasks/_AGENT_EXECUTION_QUALITY_RULES.md
```

## Updated routing docs

```text
.devflow/AUTONOMOUS_RUNBOOK.md
.devflow/CODE_QUALITY_AND_STYLE.md
.devflow/DEFINITION_OF_DONE.md
.devflow/LOCAL_AGENT_REVIEW_CHECKLIST.md
.devflow/prompts/local_agent_start_prompt.md
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/_TASK_TEMPLATE.md
docs/agent-tasks/_TASK_READINESS_CHECKLIST.md
```
