# Apply README — agent task pack 001

This patch adds the first `docs/agent-tasks/` layer.

It is designed for the workflow:

```text
phase plan -> agent task spec -> proof tests -> system gates -> next task pointer
```

## What this changes

Local agents should no longer implement directly from broad phase plans when an `agent_task_spec` exists. They should read:

1. `.devflow/NEXT_TASK.md`;
2. `docs/agent-tasks/000_INDEX.md`;
3. exactly one referenced task spec;
4. only source docs and existing patterns listed by that spec.

## Apply

Extract into the repository root:

```text
C:\Users\endim\LLMGameCreator
```

Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Do not run M5/M6 from this patch yet

This patch includes M5/M6 task specs as future executable contracts. They remain locked by the current M4.1 real-model evaluation gate until the repository current-state docs explicitly say otherwise.
