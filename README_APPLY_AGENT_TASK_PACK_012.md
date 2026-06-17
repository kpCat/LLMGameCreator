# README_APPLY_AGENT_TASK_PACK_012.md

Pack id: `agent-task-pack-012-roadmap-freeze-m4-execution-support`

## Purpose

This pack freezes the documentation-only roadmap after M10 locked drafts and adds M4.1 execution-branch support docs.

It is intended to be applied to `main` before creating separate Kilo/local-agent execution branches.

## Files added

```text
docs/agent-tasks/005_ROADMAP_FREEZE.md
docs/agent-tasks/006_BRANCH_RUNBOOK.md
docs/agent-tasks/M4_1/018_EXEC_QUEUE.md
docs/agent-tasks/M4_1/019_KILO_PROMPTS.md
docs/agent-tasks/M4_1/020_REVIEW_GATE.md
```

## Files updated

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
```

## Apply

From repository root:

```powershell
cd C:\Users\endim\LLMGameCreator
# unzip this archive into the repository root with overwrite
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Explicit non-goals

This pack does not touch:

```text
src/**
tests/**
*.sln
*.csproj
.devflow/scripts/**
GamePackage schema
Lua implementation
runtime implementation
provider layer
Unity project files
```

## Recommended next action after push

Create a dedicated branch and run exactly one Kilo task:

```text
M4_1_005
```

Use:

```text
docs/agent-tasks/006_BRANCH_RUNBOOK.md
docs/agent-tasks/M4_1/019_KILO_PROMPTS.md
```
