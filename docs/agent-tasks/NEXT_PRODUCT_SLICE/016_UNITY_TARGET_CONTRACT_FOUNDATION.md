# Product Slice 016 Task: Unity Target Contract + Game Design Brief Foundation

## Task type

Bounded architecture/contract foundation slice.

## Goal

Add the first machine-readable contracts that point the whole project toward the real final target:

```text
LLMGameCreator produces game archives
→ flexible Unity runtime/player loads archives
→ later standalone Unity builds can be produced from the same archive/contracts
```

This slice must not implement Unity. It must define and validate contracts connecting current game-composition system to the future Unity player/exporter.

## Recommended Codex reasoning level

High.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/LLMGAMECREATOR_OFFICIAL_PRODUCT_PLAN.md
docs/GAME_ASSEMBLY_WORKBENCH_ARCHITECTURE.md
docs/CAPABILITY_GRAPH_AND_GENERATOR_CATALOG_PLAN.md
docs/PRODUCT_SLICE_016_UNITY_TARGET_CONTRACT_FOUNDATION.md
docs/GAME_DESIGN_BRIEF_AND_LORE_CONTRACT_SPEC.md
docs/UNITY_TARGET_RUNTIME_CONTRACT_SPEC.md
docs/UNITY_TARGET_CONTRACT_FOUNDATION_RATIONALE.md
src/LLMGameCreator.Application/Composition/**
tests/LLMGameCreator.Tests/Application/**
tests/LLMGameCreator.Tests/ProductSmoke/**
.devflow/scripts/run-product-smoke.ps1
```

## Allowed files

```text
src/LLMGameCreator.Application/Composition/**
tests/LLMGameCreator.Tests/Application/**
tests/LLMGameCreator.Tests/ProductSmoke/**
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

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

Do not add NuGet packages. Do not implement Unity project/runtime. Do not change GamePackageDefinition or package schema. Do not call LLM/provider. Do not execute generators. Do not implement ComfyUI/Suno integration. Do not implement semantic world model, imported maps, lazy world generation, NPC schedules, police/crime, vehicles or army battles yet.

## Required implementation

### 1. Game design brief models

Add models under `Application/Composition`, suggested names:

```text
GameDesignBrief
GameDesignBriefPresetProvider
GameRealismMode
GameLoreMode
GameDesignWish
GameViewModeWish
GameInteractionWish
GameGenerationPolicy
GameScalePolicy
GamePerformancePolicy
```

The brief should store title/pitch, content language, lore facts, world rules, gameplay/view/interaction/UI/asset/audio wishes, generation policy, scale policy and performance policy.

### 2. Unity target/archive models

Add:

```text
UnityTargetProfile
UnityGameArchiveManifest
UnityRuntimeModuleContract
UnityUiLayoutContract
UnityUiPanelContract
UnityUiWidgetContract
UnityUiBindingContract
UnityAssetGenerationRequest
UnityAudioGenerationRequest
UnityWorldStreamingPolicy
UnityTargetContractValidationResult
UnityTargetContractDiagnostic
UnityTargetContractValidator
```

### 3. Built-in Unity target profile/provider

Add `UnityTargetContractPresetProvider`.

Minimum target profiles:

```text
generic_unity_player_2_5d
generic_unity_player_topdown
generic_unity_player_mixed_view_future
```

Minimum runtime modules:

```text
unity.core.archive_loader
unity.core.save_load
unity.core.input_settings
unity.core.asset_loader
unity.ui.dynamic_layout
unity.ui.data_binding
unity.audio.short_sfx
unity.audio.music_themes
unity.world.topdown_map
unity.world.streaming
unity.gameplay.stats
unity.gameplay.inventory
unity.gameplay.dialogue
unity.gameplay.quest_journal
unity.gameplay.personal_combat
unity.gameplay.crafting
unity.transport.vehicle_future
unity.transport.public_transport_future
unity.society.npc_schedule_future
unity.crime.police_future
unity.combat.army_battle_future
unity.world.imported_real_map_future
```

### 4. Validation

Validator should detect blank ids, duplicate runtime module ids, unknown runtime module references in target profile/archive manifest, blank UI binding paths, duplicated asset/audio request ids, unsafe archive ids, future modules requested by current target as warnings and inconsistent large-world streaming policy.

### 5. Large world policy

Make the model express:

```text
store seed/rules/templates
materialize only active chunks
persist dirty deltas
generate NPCs/quests lazily
limit active NPC budget
separate authored important NPCs from generated population
```

### 6. Product smoke

Add scenario:

```text
unity-target-contract
```

Smoke should verify:
1. built-in Unity target profile ids are unique;
2. runtime module ids are unique;
3. generic 2.5D target validates;
4. topdown generated RPG archive manifest validates;
5. mixed/future target reports planned/future modules without crashing;
6. large-world policy can represent lazy NPC/quest generation without materializing thousands of records;
7. no Unity, provider, generator execution, Runtime or GamePackage schema calls.

### 7. State/docs

Update `PRODUCT_SMOKE_SCENARIOS.md`, `CURRENT_GENERATOR_STATE.md`, `CURRENT_GENERATOR_STATE.json`, `.devflow/CURRENT_RUN.md`. Mark Slice 015 accepted/completed and Slice 016 completed only after checks pass.

## Tests

Required tests:
1. game design brief can express lore/rules/view/interactions/generation policy.
2. built-in Unity target profiles are unique.
3. runtime module ids are unique.
4. current generic Unity target validates.
5. future mixed-view/real-map/social target reports warnings, not crash.
6. UI layout contract supports panels/widgets/bindings.
7. asset/audio request contracts support ComfyUI/manual/Suno-like future sources as metadata only.
8. large world streaming policy supports lazy generation/delta persistence.
9. product smoke `unity-target-contract` passes.
10. existing ProductSmoke tests pass.

## Required checks

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityTarget"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GameDesignBrief"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-content-interaction-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario active-package-quest-dialogue-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-map-placement-preview
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario content-language-policy
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario game-blueprint-capability-compatibility
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generator-catalog-contract
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario composition-diagnostics-report
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario composition-report-export
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario composition-workbench-readonly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-target-contract

powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

No manual UI verification is required. This slice must not change WinForms UI.

## Stop conditions

Stop and report if Unity implementation, Runtime/GamePackage schema/WinForms changes, ComfyUI/Suno/provider implementation, `.sln`/`.csproj` changes, or more than 18 files become necessary.

## Final report

Russian report with files read/changed, GameDesignBrief behavior, Unity target/archive/module/UI/asset/audio/streaming contracts, validation rules, smoke/check results, confirmation that Unity/Runtime/UI/package schema/provider execution were not implemented, and recommended next slice.
