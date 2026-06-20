# Product Smoke Scenarios

This document tracks headless product smoke scenarios that verify product flows without UI automation, LLM providers, LM Studio or Lua execution.

## baseline-strict-package-assembly

Validates the sampled M4.1 baseline approved-artifact flow:

```text
fixture approved artifacts
-> GamePackage assembly service
-> package.json export
-> package validation assertions
-> product smoke report
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
```

The scenario uses deterministic in-test approved artifacts for:

```text
game_profile_v1
scene_pack_v1
quest_pack_v1
mechanics_pack_v1
```

Expected run output:

```text
.devflow/runs/<timestamp>-product-smoke/product-smoke-summary.md
.devflow/runs/<timestamp>-product-smoke/product-smoke-summary.json
.devflow/runs/<timestamp>-product-smoke/test-results/
.devflow/runs/<timestamp>-product-smoke/package-output/package.json
```

Expected assertions:

- `package.json` exists.
- manifest title and description are populated from `game_profile_v1`.
- `generatedContent.profile.title` is not empty.
- `generatedContent.scenes`, `generatedContent.quests` and `generatedContent.mechanics` contain baseline content.
- `generatedContent.appliedArtifacts` includes provenance and content hashes for all four baseline contracts.
- assembly diagnostics and package validation have no package-blocking errors.

No-LLM guarantee:

- The smoke test constructs fixture approved artifacts directly.
- It calls `GeneratorPlanGamePackageAssemblyService` with `JsonGamePackageRepository`.
- It does not call provider APIs, LM Studio, repair prompts, Lua, runtime preview or WinForms UI.

Relationship to manual UI tests:

This smoke proves the baseline approved-artifact package assembly/export path headlessly. Manual UI checks remain useful for Artifact Review button flow, status text, layout and other UI-specific behavior, but they are not required for this headless package assembly smoke.

## generated-package-runtime-preview

Validates the assembled generated package runtime-preview bridge:

```text
fixture approved artifacts
-> GamePackage assembly service
-> package.json export
-> DefaultGameRuntime start
-> generated package runtime-preview projection
-> simple movement command
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-package-runtime-preview
```

Expected assertions:

- Runtime starts on the assembled baseline package.
- Current scene is resolved from `generatedContent.scenes` by current map id.
- Generated profile title, description and core loop are visible.
- Generated quests and mechanics are visible.
- Applied artifact provenance includes the sampled baseline contracts.
- A simple movement command still succeeds after runtime start.
- The fixture does not require LLM/provider metadata.

Expected run output:

```text
.devflow/runs/<timestamp>-product-smoke/product-smoke-summary.md
.devflow/runs/<timestamp>-product-smoke/product-smoke-summary.json
.devflow/runs/<timestamp>-product-smoke/test-results/
.devflow/runs/<timestamp>-product-smoke/package-output/package.json
```

No-LLM guarantee:

- The smoke test constructs fixture approved artifacts directly.
- It calls `GeneratorPlanGamePackageAssemblyService`, `DefaultGameRuntime` and `GeneratedPackageRuntimePreviewService`.
- It does not call provider APIs, LM Studio, repair prompts, Lua, WinForms UI automation or generator-library execution.

Relationship to manual UI tests:

This smoke proves the generated content projection and runtime movement compatibility headlessly. Manual UI checks remain useful for confirming the Runtime Preview tab layout, text readability and keyboard focus.

## expanded-contract-batch-smoke

Validates the first expanded strict-contract batch without LLM/provider calls:

```text
fixture approved artifacts for the full_small_rpg_seed contract set
-> GamePackage assembly service
-> package.json export
-> generatedContent expanded sections
-> Runtime Preview projection
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario expanded-contract-batch-smoke
```

The fixture covers the four baseline contracts plus:

```text
region_pack_v1
npc_pack_v1
item_pack_v1
dialogue_pack_v1
encounter_pack_v1
```

Expected assertions:

- assembly succeeds and exports `package.json`;
- provenance contains all nine `full_small_rpg_seed` contracts;
- `generatedContent.regions`, `npcs`, `items`, `dialogues` and `encounters` are non-empty;
- Runtime Preview projection exposes summaries and references for all five expanded sections;
- fixture artifacts contain no provider or LM Studio dependency;
- no generated effects, combat, economy or dialogue execution occurs.

Expected outputs use the same `.devflow/runs/<timestamp>-product-smoke/` structure as the baseline scenarios.
