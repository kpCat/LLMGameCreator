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

## generated-content-interaction-preview

Validates the read-only interaction browser over an assembled expanded package:

```text
full_small_rpg_seed fixture artifacts
-> GamePackage assembly and export
-> DefaultGameRuntime start
-> generated package projection
-> generated interaction catalog
-> selection details and movement compatibility
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-content-interaction-preview
```

Expected assertions:

- all catalog categories exist: current scene, regions, NPCs, items, dialogues, quests, mechanics, encounters, applied artifacts and warnings;
- expanded content entries expose non-empty read-only details and references;
- dialogue details include lines, quest details include steps/objectives, and applied-artifact details include contract, mapping and content hash;
- a movement command still succeeds after the catalog is built and the refreshed catalog remains available;
- fixture artifacts contain no provider or LM Studio dependency;
- no dialogue, encounter, quest, inventory, Lua or effect execution occurs.

Expected outputs use the shared `.devflow/runs/<timestamp>-product-smoke/` structure and include exported `package-output/package.json`.

Manual UI verification remains required for category selection, details readability, `Append selected to log`, and selection preservation after Start/movement.

## active-package-quest-dialogue-preview

Validates the non-destructive assembled-package activation flow and preview-only quest/dialogue session:

```text
full_small_rpg_seed fixture artifacts
-> .llmgc/package-assembly/package.json export
-> validate and activate through AssembledGamePackageActivationService
-> Runtime Preview projection and interaction catalog
-> linked NPC dialogue preview
-> in-memory quest preview journal and step advance
-> movement compatibility
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario active-package-quest-dialogue-preview
```

Expected assertions:

- activation fails safely if the assembled package is absent or invalid;
- the valid assembled package becomes `ICurrentGamePackageService.CurrentPackage` without replacing the root `package.json`;
- Runtime Preview starts with non-empty generated NPC, dialogue and quest content;
- NPC details include linked dialogue ids and dialogue preview returns read-only lines;
- starting and advancing a quest changes only the in-memory preview journal;
- movement remains functional;
- fixture artifacts contain no provider or LM Studio dependency;
- no generated effects, dialogue choices, quest rewards, inventory, combat or Lua are executed.

The scenario stores its assembled package at `.devflow/runs/<timestamp>-product-smoke/package-output/.llmgc/package-assembly/package.json`.

Manual UI verification remains required for the Artifact Review activation button, Runtime Preview action buttons, log readability and Quest Journal layout.

## generated-map-placement-preview

Validates deterministic preview-only map placement for generated NPCs and encounters:

```text
full_small_rpg_seed fixture artifacts
-> GamePackage assembly and export
-> DefaultGameRuntime start
-> generated package projection
-> deterministic NPC/encounter marker placement
-> interaction catalog and movement compatibility
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-map-placement-preview
```

Expected assertions:

- NPC and encounter marker counts match the corresponding `generatedContent` sections;
- scene ids resolve through generated scenes to package map ids, with region/current-map fallback kept diagnostic and non-throwing;
- every marker has a map id and an in-bounds position;
- two builds produce the same marker ids, map ids and positions;
- placement prefers walkable tiles and avoids the player/start tile and marker overlap when possible;
- Generated Content Browser NPC/encounter entries remain available;
- movement still succeeds;
- fixture artifacts contain no provider or LM Studio dependency;
- no generated effects, dialogue choices, encounter outcomes, combat, inventory or Lua are executed.

The Runtime Preview canvas renders NPC markers as green circles, encounter markers as orange-red diamonds and the player as the existing blue square. Browser selection remains the interaction source; marker map, position, references and preview details are appended to the existing log.

Expected outputs use the shared `.devflow/runs/<timestamp>-product-smoke/` structure and include exported `package-output/package.json`.

Manual UI verification remains required for visual marker distinction, Browser-to-marker detail comparison and movement redraw behavior.

## content-language-policy

Validates the project-scoped content language foundation without an LLM/provider call:

```text
default Russian content language policy
-> project-local policy save/load
-> LLM Artifacts presenter request
-> strict prompt construction
-> non-blocking player-facing language diagnostics
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario content-language-policy
```

Expected assertions:

- the policy defaults to `ru` and persists at `.llmgc/settings/content-language-policy.json`;
- `ru`, `uk` and `en` are supported content language codes;
- the selected language reaches the strict generation request and prompt;
- the Russian prompt requires Russian player-facing content and ASCII/kebab_case technical ids;
- obvious English player-facing prose emits a warning under `ru`;
- technical ids are not inspected as player-facing prose;
- no LLM/provider, translation, Lua, runtime or package mutation is invoked.

This scenario intentionally does not create or rewrite `package.json`. Existing English fixture artifacts remain valid because the language heuristic is warning-only and applies to future explicit generation requests.

Manual UI verification remains required for selector layout, project-to-project policy switching and prompt preview readability.

## unity-archive-request-pipeline

Validates deterministic editor-side asset/audio/Lua request pipeline inside the existing Unity archive without creating or calling Unity:

```text
existing GamePackageDefinition instance
-> request pipeline service generates asset, audio and Lua module request metadata
-> materialization writes all request pipeline files
-> deterministic archive request metadata
-> future provider warnings as non-blocking diagnostics
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-request-pipeline
```

Expected assertions:
- `.llmgc/unity-archive/assets/asset-requests.json`, `assets/asset-request-index.json`, `audio/audio-requests.json`, `audio/audio-request-index.json`, `lua/module-requests.json` and `lua/modules-index.json` all exist and are valid JSON with `schemaVersion`;
- request ids are stable/deterministic across two runs;
- top-down RPG target with sample package data creates scene illustration, NPC portrait, item icon, ability icon, tile texture, UI widget/theme, UI click, footstep, ability, scene ambience and music request metadata;
- Lua module requests include inventory, quest journal, dialogue, combat, crafting, stats and world map modules;
- `generic_unity_player_mixed_view_future` target creates future Lua/audio metadata warnings without crashing;
- future provider kinds appear as warnings but not errors;
- no Unity implementation, provider call, generator execution, Runtime, GamePackage schema, Lua or WinForms UI is invoked.

No manual UI verification is required.

## unity-archive-provider-job-plan

Validates deterministic metadata-only fulfillment planning over the existing Unity archive request pipeline:

```text
asset/audio/Lua request metadata
-> typed fulfillment slots with safe future output paths
-> provider-specific planned job batches
-> materialized readiness report without provider execution
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-provider-job-plan
```

Expected assertions:

- fulfillment/readiness, asset/audio/Lua slot and all five provider job JSON files exist with `schemaVersion`;
- asset/audio/Lua slot counts match their request counts;
- expected output paths are deterministic safe archive-relative paths with no traversal;
- manual-import, ComfyUI, Suno, local-audio and procedural job files exist even when a batch is empty;
- provider `none` remains slot-only metadata and creates no provider job;
- all provider jobs remain `planned_not_executed` with execution disabled;
- no expected `.png`, `.wav` or `.lua` output is physically generated;
- repeated materialization is byte-identical and the existing request-pipeline smoke remains green;
- no Unity implementation, provider call, generator execution, Runtime, GamePackage schema, Lua execution or WinForms UI is invoked.

No manual UI verification is required.

## game-blueprint-capability-compatibility

Validates the first machine-readable GameBlueprint and capability compatibility foundation without runtime, package or provider execution:

```text
built-in capability registry
-> baseline generated RPG blueprint
-> compatibility validation
-> future imported-map diagnostics
-> intentionally broken blueprint diagnostics
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario game-blueprint-capability-compatibility
```

Expected assertions:

- built-in capability ids are unique and resolvable;
- `baseline_generated_rpg_preview` is compatible with current capabilities;
- `realistic_city_survival_imported_map_future` reports planned and missing capabilities without throwing;
- an intentionally broken movement-only blueprint reports its missing requirements;
- validation is deterministic and does not call an LLM/provider, runtime, package assembly, Lua or WinForms UI.

This scenario is a pure Application-layer composition check. It does not create or mutate `package.json`, activate a package or execute any gameplay behavior. No manual UI verification is required.

## generator-catalog-contract

Validates the first machine-readable Generator Catalog contract and planning layer without loading plugins or executing generators:

```text
built-in current/planned generator manifests
-> catalog validation
-> baseline generated RPG generator plan
-> imported-map future planned/missing diagnostics
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario generator-catalog-contract
```

Expected assertions:

- built-in generator ids are unique and current catalog validation has no errors;
- all nine current strict LLM contract manifests plus package assembly and activation are present;
- all eight planned future generator manifests are present;
- `baseline_generated_rpg_preview` resolves the current strict-contract, assembly, activation and map-marker modules;
- `realistic_city_survival_imported_map_future` reports related planned modules and missing `time.calendar` generator support without throwing;
- no LLM/provider call, dynamic plugin loading, generator execution, Runtime, package mutation, Lua or WinForms UI is invoked.

This scenario is a pure Application-layer catalog check. No manual UI verification is required.

## composition-diagnostics-report

Validates the consolidated, catalog-backed composition report without executing generators:

```text
GameBlueprint preset
-> capability compatibility validation
-> Generator Catalog validation and non-executing plan resolution
-> deterministic readiness, diagnostics and recommended actions
-> deterministic markdown report
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario composition-diagnostics-report
```

Expected assertions:

- `baseline_generated_rpg_preview` is `BuildableNow` or `BuildableWithWarnings` and selects current generators;
- `realistic_city_survival_imported_map_future` is `PlannedFuture` or `MissingRequirements` and reports planned/missing generator support;
- an intentionally broken blueprint reports `MissingRequirements`, `Conflict` or `Invalid`;
- consolidated diagnostics, generator ids, recommended actions and markdown output are deterministic;
- no timestamps are embedded in the composition markdown;
- no LLM/provider call, dynamic plugin loading, generator execution, Runtime, package mutation, Lua or WinForms UI is invoked.

This scenario is a pure Application-layer reporting check. No manual UI verification is required.

## composition-report-export

Validates deterministic project-local persistence of the consolidated composition report:

```text
baseline_generated_rpg_preview diagnostics
-> timestamp-free markdown renderer
-> safe project-local report path
-> deterministic sorted index
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario composition-report-export
```

Expected assertions:

- markdown is written to `.llmgc/composition-diagnostics/<safe-blueprint-id>.composition-report.md`;
- `index.json` is written under the same directory and entries are sorted by blueprint id;
- markdown contains readiness and selected-current-generator sections;
- repeated export produces byte-identical markdown and index content;
- unsafe blueprint-id characters and traversal segments cannot escape the project root;
- output uses UTF-8 and contains no export timestamps;
- no LLM/provider call, dynamic plugin loading, generator execution, Runtime, package mutation, Lua or WinForms UI is invoked.

This scenario is a pure Application-layer persistence/export check. No manual UI verification is required.

## composition-workbench-readonly

Validates the read-only Composition Workbench consumer over the existing composition diagnostics/export contract:

```text
built-in blueprint presets
-> diagnostics report and markdown preview
-> project-local report export
-> saved index refresh and markdown readback
-> Designer-safe WinForms page construction
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario composition-workbench-readonly
```

Expected assertions:

- the presenter and parameterized `CompositionWorkbenchPageControl` construct headlessly;
- built-in blueprint presets include `baseline_generated_rpg_preview`;
- baseline diagnostics report is `BuildableNow` and returns markdown with recommended actions;
- export writes the existing markdown/index contract under `.llmgc/composition-diagnostics`;
- saved-report refresh sees the exported entry and loads its markdown;
- catalog manifests do not run at Runtime and no provider module is selected;
- no LLM/provider call, dynamic plugin loading, generator execution, Runtime, package mutation or Lua execution is invoked.

Manual UI verification is optional. It remains useful for visual layout, selector behavior and long-markdown readability, while the headless test protects construction, presenter behavior and persistence/readback.

## unity-target-contract

Validates the machine-readable Game Design Brief and future Unity archive/player target contracts without implementing or calling Unity:

```text
game design brief preset
-> built-in Unity target profiles and runtime module metadata
-> top-down generated RPG archive manifest
-> deterministic contract validation
-> planned/future module warnings and large-world policy checks
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-target-contract
```

Expected assertions:

- built-in Unity target profile ids and all 22 runtime module ids are unique;
- `generic_unity_player_2_5d` validates without diagnostics;
- the top-down generated RPG archive manifest validates with dynamic UI, asset/audio request metadata and lazy large-world policy;
- `generic_unity_player_mixed_view_future` reports planned/future module warnings without crashing;
- large-world metadata stores seed/rules/templates, materializes active chunks only, persists dirty deltas, lazily generates NPCs/quests, caps active NPCs and separates authored important NPCs from generated population;
- no Unity implementation, provider call, generator execution, Runtime, GamePackage schema, Lua or WinForms UI is invoked.

This scenario is a pure Application-layer contract check. No manual UI verification is required.

## unity-archive-export-dry-run

Validates deterministic project-local planning for a future Unity game archive without creating or calling Unity:

```text
Game Design Brief and Unity target/archive presets
-> Unity target contract validation
-> stable archive file plan and readiness
-> deterministic UTF-8 JSON/markdown dry-run outputs
-> future-module and unsafe-path diagnostics
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-export-dry-run
```

Expected assertions:

- `.llmgc/unity-export-dry-run/` is created under the configured project folder;
- `unity-archive-plan.json`, `unity-archive-plan.md`, `unity-archive-manifest.json` and `validation-report.json` exist;
- the generic top-down target is `ExportableNow` or `ExportableWithWarnings`;
- repeated unchanged export produces byte-identical plan JSON and markdown;
- logical archive files are stably ordered and unsafe paths cannot escape the output directory;
- the mixed/future target is `BlockedByFutureModules` and reports its planned modules without throwing;
- no Unity implementation, provider call, generator execution, Runtime, GamePackage schema, Lua or WinForms UI is invoked.

This scenario is a pure Application-layer dry run. No manual UI verification is required.

## unity-archive-materialization

Validates deterministic editor-side materialization of the first concrete Unity archive contract without creating or calling Unity:

```text
Game Design Brief and Unity target/archive presets
-> Slice 017 dry-run validation
-> deterministic .llmgc/unity-archive contract/meta files
-> current playable-contract readiness or future metadata-only readiness
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-materialization
```

Expected assertions:

- `.llmgc/unity-archive/` is created under the configured project folder;
- manifest, design brief, target profile, runtime module, UI layout, asset/audio request, localization and Lua module index files exist;
- archive manifest JSON is valid and current runtime module ids are present in the module index;
- asset/audio request files exist even when their request lists are empty;
- repeated unchanged materialization produces byte-identical output;
- the mixed/future target materializes metadata only and is never reported as a playable contract;
- missing or invalid dry-run requirements write validation output only;
- all materialized paths are safe relative paths under the archive directory;
- no Unity implementation, provider call, generator execution, Runtime, GamePackage schema, Lua or WinForms UI is invoked.

Optional zip output is not implemented in materialization v1. No manual UI verification is required.

## unity-archive-game-data-payload

Validates deterministic editor-side game-data payload materialization inside the existing Unity archive without creating or calling Unity:

```text
existing GamePackageDefinition instance
-> stable package JSON and generated/core category extraction
-> deterministic .llmgc/unity-archive/data files
-> current archive payload or future metadata-only archive without payload
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-game-data-payload
```

Expected assertions:

- `data/game-package.json`, `generated-content-index.json` and all six category indexes exist when package data is supplied;
- package and index files are valid JSON with `schemaVersion`, `category`, `sourcePackageId` and `entries` where applicable;
- scenes, NPCs, quests, dialogues, items and encounters are extracted only from existing package/generated-content structures;
- missing categories produce valid empty indexes;
- entry ids, tags and linked ids are stably ordered, output is UTF-8 without BOM and repeated unchanged materialization is byte-identical;
- generated data indexes contain no timestamps;
- the mixed/future target remains metadata-only and contains no `data/` payload when package data is absent;
- unsafe output paths cannot escape the project root;
- no Unity implementation, provider call, generator execution, Runtime, GamePackage schema, Lua or WinForms UI is invoked.

No manual UI verification is required.
