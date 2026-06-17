# README_APPLY_AGENT_TASK_PACK_013.md

Pack id: `agent-task-pack-013-start-m4-1-005`

## What this pack does

This pack prepares the repository for the first coding-agent run after the roadmap freeze.

It does not add more speculative future roadmap docs. It activates `M4_1_005` by setting `.devflow/NEXT_TASK.md` to approved for execution on a dedicated branch.

## Apply

From repository root:

```powershell
cd C:\Users\endim\LLMGameCreator
# unzip this archive into the repository root with overwrite enabled

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## After apply

Create a dedicated branch from `main`, then run Kilo/local agent on exactly one task:

```text
M4_1_005
```

Use:

```text
docs/agent-tasks/007_START_RUN.md
docs/agent-tasks/M4_1/021_START_005.md
docs/agent-tasks/M4_1/022_REPORT.md
```

## Boundaries

This pack does not touch:

```text
src/**
tests/**
LLMGameCreator.sln
*.csproj
.devflow/scripts/**
GamePackage schema
runtime implementation
Lua implementation
provider layer
```

M5/M6/M8/M9/M10 remain locked.
