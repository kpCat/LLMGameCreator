# README_APPLY_NEXT_TASK_008.md

Purpose: switch `.devflow/NEXT_TASK.md` to the next M4.1 task after M4_1_006 has passed.

## Apply

```powershell
cd C:\Users\endim\LLMGameCreator
# unzip this archive into the repository root with overwrite

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Next Kilo run

Start a fresh Kilo session. It must execute exactly one task from `.devflow/NEXT_TASK.md` and stop.

Use this only after `M4_1_006` focused tests and `check-all.ps1` are green.
