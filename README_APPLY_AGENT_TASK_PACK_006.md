# README_APPLY_AGENT_TASK_PACK_006.md

Pack id: `agent-task-pack-006-future-phase-sequence-skeletons`

Purpose: add locked sequence skeletons for M5/M6/M8/M9/M10 so future development can be planned from repository docs without generating stale executable specs too early.

## What this pack does

```text
- Adds phase sequence skeletons for M5, M6, M8, M9, and M10.
- Updates docs/agent-tasks/000_INDEX.md so local agents can see future phase routes without executing them.
- Updates docs/agent-tasks/001_TASK_PACK_LEDGER.md with Pack 006 state.
- Updates docs/agent-tasks/002_NEXT_PACK_REQUEST.md with the post-Pack-006 decision policy.
```

## What this pack does not do

```text
- Does not unlock M5/M6/M8/M9/M10.
- Does not create executable production-code tasks for future phases.
- Does not touch src/, tests/, .sln, .csproj, .devflow/scripts/, GamePackage schema, runtime, Lua executor implementation, or provider code.
- Does not replace existing M4.1 executable specs.
```

## Apply

From repository root:

```powershell
cd C:\Users\endim\LLMGameCreator
# unzip llmgc_agent_task_pack_006.zip here, replacing existing docs files

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## After applying

Suggested next action:

```text
Commit/push this documentation pack to main.
Then either:
1. run remaining M4.1 executable tasks on a separate agent branch; or
2. request another documentation-only pack for locked future task skeletons, knowing that those future specs must be refreshed before execution.
```

Current gate remains:

```text
M4.1 real-model evaluation gate
```

M5/M6/M8/M9/M10 remain locked until `docs/CURRENT_GENERATOR_STATE.md` and `docs/CURRENT_GENERATOR_STATE.json` explicitly unlock them.
