# Apply NEXT_TASK repair for M4_1_005

This archive switches the next Kilo run to a repair-only task.

## Files

```text
.devflow/NEXT_TASK.md
docs/agent-tasks/M4_1/023_REPAIR_005.md
```

## Apply

From repository root:

```powershell
# unzip this archive into the repository root with overwrite enabled

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Then start a new Kilo session using the normal one-task prompt.
