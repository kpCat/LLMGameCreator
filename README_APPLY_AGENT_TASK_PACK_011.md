# README_APPLY_AGENT_TASK_PACK_011.md

Pack id: `agent-task-pack-011-locked-m10-export-unity-ir-drafts`

This pack adds locked M10 draft task specs with short filenames.

## Scope

This is documentation-only planning. It does not unlock M10 implementation.

M10 remains locked until current-state docs explicitly allow export profile / Unity IR work.

## Files

```text
docs/agent-tasks/M10/M10_001_EXPORTS.md
docs/agent-tasks/M10/M10_002_UNITY_IR.md
docs/agent-tasks/M10/M10_003_PACKAGE.md
docs/agent-tasks/M10/M10_004_BOUNDARY.md
docs/agent-tasks/M10/M10_005_ASSETS.md
docs/agent-tasks/M10/000_M10_SEQUENCE.md
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
```

## Apply

Unzip this archive into the repository root with overwrite.

Then run:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Non-goals

```text
- no src/ changes;
- no tests/ changes;
- no .sln/.csproj changes;
- no .devflow/scripts changes;
- no Runtime/GamePackage schema changes;
- no M10 unlock;
- no Unity project generation.
```

## Recommended next action

After Pack 011, the far-future locked roadmap is complete enough. The next practical step should be M4.1 execution support / roadmap freeze, or running the already existing M4.1 tasks in Kilo on a separate branch.
