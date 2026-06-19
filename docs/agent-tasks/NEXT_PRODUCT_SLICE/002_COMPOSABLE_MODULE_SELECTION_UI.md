# Product Slice 002 Task: Composable Module Selection UI

## Task type

Large product implementation slice.

## Goal

Wire the existing non-breaking composable capability fields into the Capability Picker UI and save/load/generation prompt flow.

Fields already introduced in Product Slice 001:

```text
selected_module_ids
selected_modifier_ids
selected_constraint_ids
runtime_requirement_ids
```

This task makes them actually selectable and persistent.

## Recommended Codex reasoning level

High.

Do not use Max/Ultra for the first run unless High fails.
Do not use Low/Medium.

## Source-of-truth docs to read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CAPABILITY_COMPOSER_V2_SPEC.md
docs/CAPABILITY_COMPOSER_V2_RU_GLOSSARY.md
docs/PRODUCT_SLICE_001_CAPABILITY_COMPOSER_V2_FOUNDATION.md
docs/PRODUCT_SLICE_002_COMPOSABLE_MODULE_SELECTION_UI.md
docs/agent-tasks/NEXT_PRODUCT_SLICE/002_COMPOSABLE_MODULE_SELECTION_UI.md
src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs
src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPresenter.cs
src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerViewModels.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionModels.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilityHelpCatalog.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionService.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactPromptBuilder.cs
```

Then search narrowly for existing capability tests and saved-selection tests.

## Allowed files

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilityHelpCatalog.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionModels.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionService.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionArtifactService.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactPromptBuilder.cs
src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPageControl.cs
src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerPresenter.cs
src/LLMGameCreator.WinForms/Pages/CapabilityPicker/CapabilityPickerViewModels.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanCapabilitySelectionServiceTests.cs
tests/LLMGameCreator.Tests/WinForms/CapabilityPickerPresenterTests.cs
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Optional, only if needed for a small new focused test:

```text
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmArtifactPromptBuilderTests.cs
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

### 1. Add selectable composable groups to Capability Picker UI

Add multi-select UI sections for:

```text
Modules
Modifiers
Constraints
Runtime requirements
```

Acceptable UI implementation:

- tab control in the left lower area;
- or grouped CheckedListBoxes;
- or a single CheckedListBox with group prefixes if this is much simpler.

Preferred:
Use a TabControl with tabs:

```text
Фичи
Модули
Модификаторы
Ограничения
Требования рантайма
Справка
```

But do not overbuild. Keep WinForms stable.

### 2. Use existing seed catalog

Use `GeneratorPlanCapabilityHelpCatalog` seed entries where possible.

If the current catalog only has one combined seed catalog, split/filter it into UI groups by id prefix or metadata category.

At minimum expose these concepts:

Progression:
```text
perk tree
level-up stat allocation
skill XP
class tree
faction rank
metamodule growth
```

Combat:
```text
realtime
turn-based
hybrid realtime/turn toggle
dialogue combat
party commands
```

World:
```text
region graph
chunk generation
biomes
weather
time of day
procedural events
settlements
```

Economy/balance:
```text
economy
trading
price policy
supply/demand
power budget
encounter tiers
```

### 3. ViewState wiring

Extend `CapabilityPickerViewState` with:

```text
SelectedModuleIds
SelectedModifierIds
SelectedConstraintIds
RuntimeRequirementIds
AvailableModules
AvailableModifiers
AvailableConstraints
AvailableRuntimeRequirements
```

Use appropriate view-model records/classes.

### 4. Presenter wiring

Update `CapabilityPickerPresenter` so:

- `FromAtlas(...)` populates available module/modifier/constraint/runtime requirement view models;
- `BuildRequest(...)` passes selected ids into `GeneratorPlanCapabilitySelectionRequest`;
- `FromSelectionResult(...)` and `FromLatestSelection(...)` restore selected arrays;
- old saved selections with empty/missing arrays still work.

### 5. UI read/apply state wiring

Update `CapabilityPickerPageControl` so:

- `ReadControlsToState()` reads checked composable lists;
- `ApplyViewState()` populates composable lists and checks saved selections;
- help panel updates when user selects a module/modifier/constraint/runtime requirement;
- no ItemCheck/BeginInvoke loop is introduced;
- required core atlas planning remains selected;
- startup does not throw SplitterDistance exceptions.

### 6. Selection service behavior

Ensure `BuildSelectionAsync` output selection includes selected arrays.

Do not require selected modules to be fully implemented.
Unsupported/future modules may create warnings but not errors unless truly impossible.

### 7. Diagnostic behavior

Add warnings where practical:

- economy/balance/chunk generation modules can be `unsupported_yet`;
- hybrid combat can be `risky` or `info`, not invalid;
- multiple progression modules are allowed.

Do not create a huge compatibility engine in this task.

### 8. Prompt builder behavior

Confirm strict prompt context includes selected arrays when non-empty.

If already implemented, add/keep tests. Do not duplicate logic unnecessarily.

### 9. List rendering

The visible list item text should be readable:

```text
Дерево перков
Распределение характеристик при уровне
Опыт навыков
Гибрид: реалтайм + пошаговый режим
Погода
Время суток
Экономика
Торговля
Баланс: бюджет силы
```

Machine ids should be visible in help/details, not as the dominant list text.

### 10. Preserve old flow

Existing flow must still work:

```text
Load atlas
Build selection
Save latest selection
LLM Artifacts -> Load
Preview prompt
```

## Tests

Add/adjust tests:

1. Old selection JSON without selected arrays deserializes and has empty arrays.
2. New selection JSON with selected arrays deserializes.
3. Presenter `BuildRequest` includes checked module/modifier/constraint/runtime requirement ids.
4. Presenter `FromLatestSelection` restores selected arrays.
5. Selection service result preserves selected arrays.
6. Prompt builder includes selected arrays when non-empty.
7. Prompt builder omits/keeps compact behavior when arrays empty.
8. Multiple progression modules are allowed.
9. Hybrid combat module does not cause invalid status by itself.
10. Unknown/future module gets warning, not fatal error, when appropriate.

## Focused commands

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Capability"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~CapabilityPicker"
```

## Required checks

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

Report whether manual verification was done. If not, say so clearly.

Manual steps:

```text
1. Start WinForms app.
2. Open Capability Picker.
3. Load atlas.
4. Select base axes.
5. Select several progression modules at the same time.
6. Select hybrid combat module.
7. Select weather/time/biomes modules.
8. Build selection.
9. Save latest selection.
10. Open LLM Artifacts.
11. Load selection.
12. Preview prompt.
13. Confirm selected arrays appear in prompt.
```

## Stop conditions

Stop and report if:

- more than 16 files need changes;
- `.sln` or `.csproj` changes are required;
- runtime/package/Lua changes become necessary;
- selected array compatibility would require destructive migration;
- check-all fails after 2 repair attempts;
- startup UI exception appears again.

## Expected final report in Russian

Include:

- files read;
- files changed;
- UI groups added;
- selected arrays wired;
- save/load behavior;
- prompt context behavior;
- diagnostics behavior;
- focused test results;
- check-devflow-state result;
- check-all result;
- manual verification status;
- remaining gaps and recommended next slice.
