# README_APPLY_PACK_008.md

## Pack

`agent-task-pack-008-locked-m6-assembly-draft-specs-shortpaths`

## What this pack does

Adds locked M6 draft specs for future GamePackage assembly work:

```text
M6_002_BASE_MAPPING
M6_003_ITEMS_MAPPING
M6_004_SCENE_MAPPING
M6_005_QUEST_MAPPING
M6_006_VALIDATION
M6_007_REVIEW_APPLY
M6_008_SAMPLE_SLICE
```

It also updates:

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/M6/000_M6_SEQUENCE.md
```

## What this pack does not do

```text
- does not unlock M6;
- does not create executable M6 production tasks;
- does not touch src/;
- does not touch tests/;
- does not change GamePackage schema;
- does not change .devflow/scripts;
- does not change .sln/.csproj.
```

## Apply

Unzip into the repository root with overwrite.

Then run:

```powershell
cd C:\Users\endim\LLMGameCreator
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Expected result

Docs-only diff. M4.1 remains the active gate. M5/M6/M8/M9/M10 implementation remains locked until current-state docs explicitly unlock the relevant phase.
