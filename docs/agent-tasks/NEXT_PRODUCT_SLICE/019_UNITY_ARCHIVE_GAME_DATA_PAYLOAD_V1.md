# Product Slice 019 Task: Unity Archive Game Data Payload v1

## Task type

Bounded editor-side archive data-payload slice.

## Goal

Extend the Slice 018 materialized Unity archive with deterministic game-data payload files under:

```text
.llmgc/unity-archive/data/
```

This makes the archive more than only contract/meta files, while still avoiding Unity implementation and Runtime/GamePackage schema changes.

## Recommended Codex reasoning level

High.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SLICE_016_UNITY_TARGET_CONTRACT_FOUNDATION.md
docs/PRODUCT_SLICE_017_UNITY_ARCHIVE_EXPORT_DRY_RUN.md
docs/PRODUCT_SLICE_018_UNITY_ARCHIVE_MATERIALIZATION_V1.md
docs/UNITY_ARCHIVE_GAME_DATA_PAYLOAD_SPEC.md
src/LLMGameCreator.Application/Composition/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Application/Packages/**
src/LLMGameCreator.Application/Projects/**
tests/LLMGameCreator.Tests/Application/UnityArchiveMaterializationTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveMaterializationSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

Read existing assembled/current package services before adding new abstractions. Do not duplicate package schema.

## Allowed files

```text
src/LLMGameCreator.Application/Composition/**
src/LLMGameCreator.Application/Packages/**
tests/LLMGameCreator.Tests/Application/**
tests/LLMGameCreator.Tests/ProductSmoke/**
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Touch `Application/Packages` only if an existing package payload helper belongs there. Prefer `Application/Composition` for Unity archive-specific payload code.

## Forbidden files

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Infrastructure/Generation/**
src/LLMGameCreator.WinForms/**
generator-library/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
```

Do not add NuGet packages.
Do not create Unity project files.
Do not build Unity.
Do not call LLM/provider.
Do not execute generators.
Do not execute Lua.
Do not implement ComfyUI/Suno integration.
Do not change Runtime, WinForms, GamePackage schema or generator-library.

## Required implementation

### 1. Add data payload models

Suggested types:

```text
UnityArchiveGameDataPayloadRequest
UnityArchiveGameDataPayloadResult
UnityArchiveGameDataIndex
UnityArchiveGameDataIndexEntry
UnityArchiveGameDataCategory
UnityArchiveGameDataPayloadDiagnostic
```

Categories:

```text
scenes
npcs
quests
dialogues
items
encounters
```

### 2. Add data payload service

Suggested type:

```text
UnityArchiveGameDataPayloadService
```

It should:
- accept project root;
- accept an existing assembled/current game package object or serialized package data without changing the package schema;
- write deterministic files under `.llmgc/unity-archive/data/`;
- write empty valid indexes for missing categories;
- never write outside project root;
- write UTF-8 without BOM where possible;
- avoid timestamps.

### 3. Integrate with materialization

Update `UnityArchiveMaterializationService` so that current archive materialization can include the data payload files when package data is supplied.

Do not make payload required for metadata-only future targets unless tests explicitly pass a current package.

### 4. Required files

Write:

```text
.llmgc/unity-archive/data/game-package.json
.llmgc/unity-archive/data/generated-content-index.json
.llmgc/unity-archive/data/scenes-index.json
.llmgc/unity-archive/data/npcs-index.json
.llmgc/unity-archive/data/quests-index.json
.llmgc/unity-archive/data/dialogues-index.json
.llmgc/unity-archive/data/items-index.json
.llmgc/unity-archive/data/encounters-index.json
```

If existing package data lacks a category, write a valid empty index with `schemaVersion`, `category`, `sourcePackageId`, `entries: []`.

### 5. Stable extraction

Extract category entries only from existing known package/generated-content structures.

Do not invent new gameplay data.
Do not infer broad semantics.
Do not create fake generated NPCs/quests just to fill files.

Stable ordering:
- entries by id/name/path, case-insensitive then ordinal;
- linkedIds/tags sorted;
- files written in stable order.

### 6. Product smoke

Add scenario:

```text
unity-archive-game-data-payload
```

Smoke verifies:
1. archive materialization writes `data/` folder;
2. all required data files exist;
3. `game-package.json` is valid JSON;
4. every category index is valid JSON with schemaVersion/category/entries;
5. indexes are deterministic across two runs;
6. no timestamp strings in generated data index files;
7. future metadata-only archive does not pretend to have playable data if no package data is supplied;
8. no Unity/provider/generator/Runtime/GamePackage schema/Lua/WinForms execution.

### 7. State/docs

Update:
- `docs/PRODUCT_SMOKE_SCENARIOS.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `.devflow/CURRENT_RUN.md`

Mark Slice 018 accepted/completed and Slice 019 completed only after checks pass.

## Tests

Required tests:
1. payload service writes all required files.
2. empty categories produce valid empty indexes.
3. category indexes are deterministic.
4. tags/linked ids are sorted where present.
5. unsafe paths cannot escape output directory.
6. materialization includes data payload when package data is supplied.
7. future metadata-only target stays metadata-only when package data is absent.
8. product smoke `unity-archive-game-data-payload` passes.
9. existing ProductSmoke tests pass.

## Required checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveGameDataPayload"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveMaterialization"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-target-contract
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-export-dry-run
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-materialization
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-game-data-payload

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

No manual UI verification is required. This slice must not change WinForms UI.

## Stop conditions

Stop and report if:
- accessing package data requires changing GamePackageDefinition;
- Unity implementation becomes necessary;
- Runtime/GamePackage schema/WinForms changes become necessary;
- provider/generator/Lua execution becomes necessary;
- `.sln` or `.csproj` changes are required;
- more than 18 files need changes;
- check-all fails after 2 repair attempts.

## Final report

Russian report with files read/changed, payload service behavior, data files written, package data source used, empty category behavior, deterministic proof, smoke/check results, confirmation that Unity/Runtime/UI/package schema/provider/generator/Lua execution were not implemented, and recommended next slice.
