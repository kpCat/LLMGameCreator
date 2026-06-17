# README_APPLY_AGENT_TASK_PACK_010.md

Pack id: `agent-task-pack-010-locked-m9-template-balancing-drafts`

## Purpose

This pack converts M9 template/balancing placeholders into locked draft task specs with short Windows-safe filenames.

It does not unlock M9 implementation.

## Apply

From repository root:

```powershell
cd C:\Users\endim\LLMGameCreator

# unzip this archive into the repository root with overwrite enabled

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Files added

```text
docs/agent-tasks/M9/M9_001_TEMPLATES.md
docs/agent-tasks/M9/M9_002_RANGES.md
docs/agent-tasks/M9/M9_003_PROGRESSION.md
docs/agent-tasks/M9/M9_004_FORMULAS.md
docs/agent-tasks/M9/M9_005_SAMPLE_PACKS.md
```

## Files updated

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/M9/000_M9_SEQUENCE.md
```

## Explicit non-goals

```text
- no src changes;
- no tests changes;
- no .sln/.csproj changes;
- no .devflow/scripts changes;
- no runtime implementation;
- no Lua/provider implementation;
- no GamePackage schema changes;
- no M9 unlock.
```

## Gate note

M9 remains locked until generated/assembled package validation is stable and current-state docs explicitly allow template/balancing expansion.
