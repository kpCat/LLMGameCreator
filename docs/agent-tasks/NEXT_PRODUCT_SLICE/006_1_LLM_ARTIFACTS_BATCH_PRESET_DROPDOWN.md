# Product Slice 006.1 Task: LLM Artifacts Batch Preset Dropdown

## Task type

Narrow UI wiring/productivity slice.

## Goal

Expose existing batch presets from `GeneratorPlanStrictLlmArtifactContractCatalog` in the LLM Artifacts page.

Selecting a batch preset should update the contract checkbox list to match the preset's `ContractIds`.

## Recommended Codex reasoning level

Medium.

Do not use Max/Ultra.
Use High only if the page has unexpected structure and Medium cannot safely reason about it.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/PRODUCT_SMOKE_SCENARIOS.md
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactContractCatalog.cs
src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsPageControl.cs
src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsPresenter.cs
src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/StrictLlmArtifactsViewModels.cs
tests/LLMGameCreator.Tests/WinForms/StrictLlmArtifactsPresenterTests.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmArtifactValidatorTests.cs
```

Then search narrowly for existing StrictLlmArtifacts tests and local UI patterns.

## Allowed files

```text
src/LLMGameCreator.WinForms/Pages/StrictLlmArtifacts/**
src/LLMGameCreator.WinForms/CompositionRoot.cs
tests/LLMGameCreator.Tests/WinForms/StrictLlmArtifactsPresenterTests.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmArtifactValidatorTests.cs
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Only touch catalog/application files if absolutely necessary to expose already existing preset list. Prefer using existing `ListBatchPresets()` and `TryGetBatchPreset()`.

## Forbidden files

```text
src/LLMGameCreator.Runtime*/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactValidator.cs
generator-library/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
.devflow/scripts/**
```

Do not add NuGet packages.
Do not call LLM/provider in tests.
Do not change contract definitions.

## Required behavior

### 1. Preset dropdown

Add a ComboBox or equivalent to LLM Artifacts page:

```text
Batch preset:
[manual/custom]
baseline_game_seed
world_content_expansion
character_content_expansion
encounter_item_expansion
full_small_rpg_seed
```

Default should be manual/custom or no preset, preserving current behavior.

### 2. Selecting preset updates contract checkboxes

When user selects a preset:
- check every contract whose id is in preset.ContractIds;
- uncheck contracts not in the preset;
- keep the existing contract list order;
- do not remove manual ability to check/uncheck after preset selection.

If the user manually changes checkboxes after selecting a preset, it is acceptable to:
- keep preset dropdown value as-is; or
- switch to manual/custom.
Prefer whichever is simplest and stable.

### 3. Existing Preview/Generate behavior remains unchanged

`Preview` and `Generate` must use the currently checked contract ids exactly as before.

### 4. Unknown preset handling

If preset lookup fails:
- do not crash;
- show status message;
- keep current checkbox state.

### 5. UI layout safety

Do not introduce hard early SplitterDistance or layout crash patterns.
If adding controls to an existing page, keep layout stable.

### 6. No broad rewrite

Do not split/rewrite the entire page unless already trivial. This is a small UI wiring task.

## Tests

Add/adjust tests:

1. Presenter/model exposes batch presets from catalog.
2. Selecting `baseline_game_seed` selects exactly:
   - `game_profile_v1`
   - `scene_pack_v1`
   - `quest_pack_v1`
   - `mechanics_pack_v1`
3. Selecting `full_small_rpg_seed` selects all 9 expected contracts.
4. Selecting unknown preset does not throw and preserves current contract selection.
5. Manual contract selection still affects Preview/Generate request as before.
6. Existing StrictLlm tests still pass.

Prefer presenter tests over fragile UI automation.

## Focused commands

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlmArtifacts"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlm"
```

## Required checks

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

Manual UI verification is expected.

Steps:

```text
1. Open app.
2. Open Ash Beacon or any test project.
3. Open LLM Artifacts.
4. Press Load.
5. Confirm new preset dropdown is visible.
6. Choose baseline_game_seed.
7. Confirm exactly four baseline contracts are checked.
8. Choose full_small_rpg_seed.
9. Confirm all nine contracts are checked.
10. Manually uncheck one contract.
11. Press Preview and confirm preview still uses currently checked contracts.
```

## Stop conditions

Stop and report if:
- `.sln` or `.csproj` change is required;
- contract catalog schema needs rewrite;
- LLM/provider changes become necessary;
- package assembly/runtime changes become necessary;
- WinForms page requires broad rewrite beyond ~8 files;
- check-all fails after 2 repair attempts.

## Final report

Russian report with:
- files read;
- files changed;
- how preset list is loaded;
- how checkbox selection is updated;
- manual override behavior;
- tests;
- smoke/check results;
- manual verification status.
