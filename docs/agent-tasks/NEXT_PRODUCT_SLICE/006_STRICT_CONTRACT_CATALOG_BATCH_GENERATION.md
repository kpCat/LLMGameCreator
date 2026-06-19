# Product Slice 006 Task: Strict Contract Catalog + Batch Generation

## Goal

Expand LLM Artifacts beyond the initial baseline contracts and add the first controlled batch-generation flow.

Current contracts:

```text
game_profile_v1
scene_pack_v1
quest_pack_v1
mechanics_pack_v1
```

Add:

```text
region_pack_v1
npc_pack_v1
item_pack_v1
dialogue_pack_v1
encounter_pack_v1
```

Add batch presets so the user can generate meaningful content groups instead of checking contracts one by one.

## Recommended Codex reasoning level

High.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/PRODUCT_SLICE_006_STRICT_CONTRACT_CATALOG_BATCH_GENERATION.md
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactContractCatalog.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactPromptBuilder.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactValidator.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
src/LLMGameCreator.Application/RuntimePreview/GeneratedPackageRuntimePreviewService.cs
src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsPageControl.cs
tests/LLMGameCreator.Tests/ProductSmoke/BaselineStrictArtifactsPackageAssemblySmokeTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/GeneratedPackageRuntimePreviewSmokeTests.cs
.devflow/scripts/run-product-smoke.ps1
```

## Allowed files

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactContractCatalog.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactPromptBuilder.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactValidator.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyValidator.cs
src/LLMGameCreator.Application/RuntimePreview/GeneratedPackageRuntimePreviewService.cs
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/**
src/LLMGameCreator.WinForms/Pages/RuntimePreview/**
src/LLMGameCreator.WinForms/CompositionRoot.cs
tests/LLMGameCreator.Tests/Design/**
tests/LLMGameCreator.Tests/ProductSmoke/**
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

## Forbidden files

```text
src/LLMGameCreator.Scripting/**
generator-library/**
src/LLMGameCreator.Infrastructure/Generation/**
src/LLMGameCreator.Runtime*/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
```

Do not add packages. Do not call LLM in tests. Do not execute generated effects.

## Required behavior

### 1. Contract catalog

Add strict contracts:

```text
region_pack_v1
npc_pack_v1
item_pack_v1
dialogue_pack_v1
encounter_pack_v1
```

Each contract needs id, label, purpose, expected artifact kind, prompt instruction, minimal JSON shape and validator coverage.

### 2. Prompt builder

Each new prompt must:
- forbid markdown/code fences/comments;
- preserve exact ids/enums;
- generate small bounded JSON;
- avoid C#/Lua/code;
- produce id references.

### 3. Validators

Minimum:
- artifact_kind matches;
- schema_version exists;
- root arrays exist;
- ids non-empty;
- ids unique within artifact;
- referenced scene/region/npc ids are strings when present.

### 4. Batch presets

Add preset definitions:

```text
baseline_game_seed
world_content_expansion
character_content_expansion
encounter_item_expansion
full_small_rpg_seed
```

If low-risk, expose preset dropdown in LLM Artifacts that checks contracts. If UI is risky, implement service/tests/docs first and report UI as gap.

### 5. Package assembly

Approved new artifacts must not be lost. Map or preserve:
- region_pack_v1 -> generatedContent.regions or preserved equivalent;
- npc_pack_v1 -> generatedContent.npcs or package entities/factions where safe;
- item_pack_v1 -> package items and/or generatedContent.items;
- dialogue_pack_v1 -> generatedContent.dialogues or package dialogues where safe;
- encounter_pack_v1 -> generatedContent.encounters or package encounters where safe.

Prefer non-breaking generatedContent sections if exact runtime structures are not ready.

### 6. Runtime Preview

Show counts/summaries for:

```text
regions
npcs
items
dialogues
encounters
```

Keep existing scene/profile/quest/mechanics display.

### 7. Product smoke

Add scenario:

```text
expanded-contract-batch-smoke
```

It must use fixture approved artifacts for at least:

```text
game_profile_v1
region_pack_v1
scene_pack_v1
npc_pack_v1
quest_pack_v1
dialogue_pack_v1
mechanics_pack_v1
encounter_pack_v1
item_pack_v1
```

Assertions:
- assembly succeeds;
- package JSON exists;
- all new contract ids are in provenance;
- generatedContent contains non-empty new sections or preserved equivalents;
- runtime preview projection includes new sections/counts;
- no LLM/provider dependency.

### 8. Script

Extend:

```text
.devflow/scripts/run-product-smoke.ps1
```

with:

```powershell
-Scenario expanded-contract-batch-smoke
```

## Tests

Required:
1. catalog exposes all five new contracts.
2. validator accepts valid minimal artifact for each new contract.
3. validator rejects wrong artifact_kind.
4. batch preset resolves expected ids.
5. package assembly maps/preserves all five new types.
6. runtime preview projection shows new sections/counts.
7. expanded batch smoke passes without LLM/provider.

## Checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlm"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GamePackageAssembly"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~expanded"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Stop conditions

Stop if more than 24 files need changes, csproj/sln changes are required, runtime/Unity/Lua implementation becomes necessary, generated effect execution is required, package schema needs destructive migration, or check-all fails after 2 repair attempts.

## Final report

Russian report with files read/changed, contracts added, batch presets, validators, package assembly strategy, Runtime Preview visibility, product smoke scenarios and all checks.
