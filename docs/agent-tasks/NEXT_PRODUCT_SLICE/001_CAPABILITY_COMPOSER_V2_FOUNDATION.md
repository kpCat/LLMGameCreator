# Product Slice 001 Task: Capability Composer v2 Foundation

## Task type

Large product implementation slice.

This is not documentation-only.

## Goal

Implement a non-breaking Capability Composer v2 foundation:

1. Russian-readable option/bundle help metadata.
2. Help/details panel in Capability Picker.
3. Better diagnostic categories.
4. Optional composable fields in capability selection:
   - `selected_module_ids`
   - `selected_modifier_ids`
   - `selected_constraint_ids`
   - `runtime_requirement_ids`
5. Existing M4.1 Capability Picker -> LLM Artifacts -> LLM Evaluation flow must remain working.

## Source-of-truth docs to read first

Read only these first:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CAPABILITY_COMPOSER_V2_SPEC.md
docs/CAPABILITY_COMPOSER_V2_RU_GLOSSARY.md
docs/PRODUCT_SLICE_001_CAPABILITY_COMPOSER_V2_FOUNDATION.md
docs/GENERATOR_PLAN_CAPABILITY_SELECTION_PICKER.md
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs
src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPresenter.cs
```

Then search narrowly for exact model/service/test files.

## Allowed files

Expected areas:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/**Capability*
src/LLMGameCreator.WinForms/Pages/CapabilityPicker/**
tests/LLMGameCreator.Tests/**/Capability*Tests.cs
tests/LLMGameCreator.Tests/**/GeneratorPlan*Capability*Tests.cs
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Optional if already existing and directly relevant:

```text
docs/CONTEXT_INDEX.md
docs/ROADMAP_TO_FULL_GENERATOR.md
```

## Forbidden files

```text
src/LLMGameCreator.Runtime*/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/**
src/LLMGameCreator.WinForms/Pages/StrictLlmEvaluation/**
generator-library/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
.devflow/scripts/**
docs/agent-tasks/M5/**
docs/agent-tasks/M6/**
docs/agent-tasks/M8/**
docs/agent-tasks/M9/**
docs/agent-tasks/M10/**
```

## Required behavior

### 1. Help metadata catalog

Add a small help metadata catalog for current visible axes and key bundles.

It may be in Application layer or CapabilityPicker-specific application service.

Must include at least:

Core axes:
- current presentation mode options visible in picker;
- current world topology options visible in picker;
- current actor model options visible in picker;
- current inventory model options visible in picker;
- current combat model options visible in picker;
- current progression model options visible in picker;
- current pathfinding profile options visible in picker;
- current NPC behavior model options visible in picker;
- current runtime target options visible in picker.

Feature bundles:
- core atlas planning;
- dialogue choice graph;
- faction reputation;
- world region/chunk generation;
- city builder production/conquest;
- inventory panel grid;
- optional horror content overlay;
- party roster/progression;
- combat realtime/turn hybrid;
- survival sandbox.

Do not try to write perfect descriptions for every atlas entry if there are many. Cover enough visible/high-impact options to prove the pattern.

### 2. Russian user-facing fields

Each metadata entry should support:

```text
DisplayNameRu
ShortDescriptionRu
DetailsRu
ExamplesRu
BestForRu
WarningsRu
ImplementationStatus
```

Machine ids must still be visible somewhere.

### 3. Help/details panel in Capability Picker

Add a panel/section to Capability Picker UI that updates when:

- user changes a dropdown selection;
- user selects/checks a feature bundle;
- user selects a diagnostic row if practical.

Panel should show:

```text
Russian display name
machine id
short description
details/examples
best-for
status/warnings
```

Do not redesign the whole UI.

### 4. Diagnostic category mapping

Add mapping for capability diagnostics:

```text
impossible
unsupported_yet
risky
info
```

At minimum map current known patterns:

- incompatible presentation/world topology -> `impossible`
- missing artifact contract -> `unsupported_yet`
- capability gap / future module unresolved -> `unsupported_yet`
- variant not recommended -> `risky`
- loaded/built info -> `info`

UI should make this understandable to the user.

### 5. Non-breaking composable selection fields

Extend capability selection model to optionally carry:

```text
selected_module_ids
selected_modifier_ids
selected_constraint_ids
runtime_requirement_ids
```

Rules:

- Defaults to empty arrays.
- Old saved selections still load.
- Existing strict artifact generation flow remains valid.
- Prompt context may include these fields only when non-empty.
- Build selection/save latest selection still works if all new arrays are empty.

### 6. Minimal module/modifier seed catalog

Add a minimal catalog/list for future modules/modifiers/constraints, but do not require UI editing of all of them yet.

Include at least ids/concepts for:

Progression modules:
- perk_tree
- level_up_stat_allocation
- skill_xp
- class_tree
- faction_rank
- metamodule_growth

Combat modules/modifiers:
- realtime
- turn_based
- hybrid_realtime_turn_toggle
- dialogue_combat
- party_commands

World modules:
- region_graph
- chunk_generation
- biomes
- weather
- time_of_day
- procedural_events
- settlements

Economy/balance modules:
- economy
- trading
- price_policy
- supply_demand
- power_budget
- encounter_tiers

This can be internal metadata only in this slice.

### 7. Preserve existing flow

Existing flow must still work:

```text
Capability Picker
-> Build selection
-> Save latest selection
-> LLM Artifacts Load
-> Generate game_profile_v1
-> LLM Evaluation latest/batch
```

Do not break M4.1 gate path.

## Tests

Add/adjust tests for:

1. Old selection JSON with only `selected_variant_ids` deserializes and has empty module/modifier/constraint/runtime requirement arrays.
2. New selection with optional arrays serializes/deserializes.
3. Metadata lookup returns Russian label/help for a known option.
4. Unknown metadata id returns safe fallback.
5. Diagnostic category mapping:
   - incompatible -> impossible
   - missing artifact contract -> unsupported_yet
   - variant not recommended -> risky
   - loaded/info -> info
6. Prompt context includes optional arrays when non-empty and omits or leaves empty when empty.
7. Existing capability picker build smoke still passes if such test exists.

## Commands

Run focused tests first:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Capability"
```

Then run:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

After tests:

1. Start WinForms app.
2. Open/create a test game.
3. Open Capability Picker.
4. Load atlas.
5. Select `Map And Panel RPG` + `Region Graph`.
6. Build selection.
7. Confirm details/help panel updates.
8. Confirm diagnostics are understandable.
9. Save latest selection.
10. Open LLM Artifacts and confirm selection loads.

## Stop conditions

Stop and report instead of continuing if:

- more than 14 files need changes;
- `.sln` or `.csproj` changes are required;
- runtime/package/Lua changes become necessary;
- existing M4.1 LLM flow breaks;
- check-all fails after 2 repair attempts;
- model changes require destructive saved selection migration.

## Expected final report

Report in Russian:

- files read;
- files changed;
- summary of new metadata model;
- UI changes;
- compatibility diagnostic categories;
- optional composable fields added;
- tests added/changed;
- focused test result;
- check-devflow-state result;
- check-all result;
- manual verification status;
- remaining gaps and recommended next slice.
