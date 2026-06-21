# Current Generator State

Status: source-of-truth handoff  
Updated by: Codex task  
State file pair: docs/CURRENT_GENERATOR_STATE.json

## Current phase

M4.1 gate passed for sampled baseline contracts; Product Slice 012 adds the first machine-readable Generator Catalog contract foundation.

The project has a safe Capability Picker -> LLM Artifacts -> LLM Evaluation -> Artifact Review -> draft package assembly path. Product Slices 001-010 established composable selection, strict generation, approved-artifact assembly, controlled batch presets, generated-content preview, explicit activation, preview-only quest/dialogue state, deterministic NPC/encounter placement and project-scoped content language policy. Product Slice 011 describes composition intent through immutable blueprint/capability models and is accepted as the parent foundation. Product Slice 012 now adds current/planned generator manifests, deterministic catalog validation and a non-executing generator plan resolver without touching runtime or package schema.

This does not unlock broad contract expansion or direct production implementation. The next step should still be chosen as a controlled product vertical slice before any M5/M6/M6-lite work starts.

## Gate decision

M4.1 real-model evaluation gate passed for sampled baseline contracts.

Evidence:

- Evaluation id: `strict_llm_evaluation/58df49dadbff5598`
- Evaluated at: `2026-06-18T16:43:35.9475873+00:00`
- Source capability selection id: `generator_plan_capability_selection/0b0addcd5c019328`
- Mode: `batch`
- Requested contracts: `game_profile_v1`, `mechanics_pack_v1`, `quest_pack_v1`, `scene_pack_v1`
- Iterations: `1`
- Repair enabled: `True`
- Stage for review: `True`
- Expected max LLM calls: `8`

Metrics:

- `total_contracts_requested`: 4
- `total_generation_runs`: 4
- `total_attempts`: 4
- `initial_pass_count`: 4
- `repair_pass_count`: 0
- `failed_count`: 0
- `valid_artifact_count`: 4
- `staged_for_review_count`: 4
- `markdown_fence_error_count`: 0
- `json_wrapper_error_count`: 0
- `json_invalid_count`: 0
- `wrong_artifact_kind_count`: 0
- `forbidden_field_count`: 0
- `invalid_id_count`: 0
- `missing_field_count`: 0
- `overall_pass_rate`: 1.0
- diagnostics: none
- quality warnings: none

Permanent evidence summary:

- docs/M4_1_REAL_EVALUATION_GATE_REPORT.md

## Last completed milestones

- M4.1: Strict LLM Generation Evaluation Pack.
- The M4.1 layer can evaluate the latest strict LLM generation audit without an LLM call or run a small explicit batch through the existing strict generation service.
- Evaluation stores JSON and markdown report artifacts with pass, repair, fail, diagnostic hot spot and quality warning metrics.
- A real local-model batch evaluation passed for the sampled baseline contracts listed above.

## Last completed product slice

- Product Slice 012: Generator Catalog Contract Foundation.
- `GeneratorModuleManifest` records stable input/output contracts, capability requirements/provisions, compatibility dimensions, maturity, LLM/determinism/offline/runtime flags, cost, validation rules and notes.
- `BuiltInGeneratorCatalog` exposes 12 current modules: nine strict LLM artifact contracts, package assembly, package activation and deterministic generated-map-marker projection.
- Eight planned manifests describe semantic seed, procedural quest/dialogue, lazy region cache, offscreen scheduling, imported-map classification, households and daily-life scheduling as contract-only future work.
- `GeneratorCatalogValidator` deterministically reports blank/duplicate ids, unknown capability references, unknown generator conflicts, current-to-planned capability dependencies and duplicate current output contracts.
- `GeneratorPlanResolver` selects current capability providers, closes their current input contracts, reports related planned modules and identifies requested planned capabilities with no generator support; it never executes a module.
- `generator-catalog-contract` proves catalog validity, current/planned manifest coverage, baseline resolution and imported-map future diagnostics without LLM/provider, plugin, Runtime, package, Lua or UI calls.
- Product Slice 011 remains accepted/completed as the GameBlueprint and capability graph parent foundation.

Parent slice foundation:

- Product Slice 011: GameBlueprint + Capability Graph Compatibility Foundation.
- `GameBlueprint` records game kind, world sources, presentations, generation modes, requested capabilities, content language and notes without mutating `GamePackage`.
- `BuiltInCapabilityRegistry` exposes 15 current product-spine capabilities and 7 planned future capabilities with requires, optional requirements, provides, conflicts, compatibility dimensions, runtime cost and maturity metadata.
- `GameBlueprintCompositionValidator` reports duplicate/unknown ids, missing and optional requirements, direct conflicts, unsupported blueprint dimensions and planned/unsupported capability maturity through deterministic diagnostics.
- Presets cover the compatible `baseline_generated_rpg_preview` plus realistic and zombie imported-map future intent; future presets remain diagnostic and non-throwing.
- `game-blueprint-capability-compatibility` proves registry uniqueness, baseline compatibility, future diagnostics and broken-blueprint missing requirements without LLM/provider, runtime, package, Lua or UI calls.
- No runtime, WinForms, `GamePackageDefinition`, package schema, generator-library, solution or project change was introduced.

Parent slice foundation:

- Product Slice 010: Official Product Plan + Content Language Policy Foundation.
- The official plan identifies LLMGameCreator as a modular Game Assembly Workbench and records the capability graph, generator catalog, semantic, lazy-world and presentation-adapter direction without starting those later systems.
- `ContentLanguagePolicyService` defaults new generation UI to `ru`, supports `ru`/`uk`/`en`, and persists the normalized policy at `.llmgc/settings/content-language-policy.json` when a project folder is available.
- LLM Artifacts exposes a Designer-safe content language selector. Prompt preview, generation and bounded repair requests include the selected player-facing language instruction while preserving ASCII/kebab_case technical ids.
- `ContentLanguageDiagnosticService` emits non-blocking warnings for obvious English prose in player-facing title/description/dialogue/objective/step fields under `ru` or `uk`; technical id fields are ignored.
- `content-language-policy` proves policy save/load, selected-language request construction, Russian prompt instruction, technical-id policy and warning behavior without an LLM/provider call.
- No translation engine, existing-artifact rewrite, runtime/package schema change, Lua, generator-library, solution or project change was introduced.

Earlier parent slice foundation:

- Product Slice 009: Generated NPC/Encounter Map Placement.
- `GeneratedMapPlacementPreviewService` resolves generated scene ids to package map ids, falls back through region-linked scenes and then the current/start map, and records fallback diagnostics instead of throwing.
- Marker positions are stable by marker id, remain inside map bounds, prefer walkable tiles and avoid the player/start tile plus other generated markers when space permits.
- Runtime Preview rebuilds markers on Start and runtime commands; `RuntimeMapCanvas` distinguishes NPC, encounter and player overlays while preserving movement rendering.
- Generated Content Browser selection remains the interaction source; NPC/encounter details and `Append selected to log` include marker map, position, references and preview-only details. NPC marker details also list linked dialogue ids/titles.
- `generated-map-placement-preview` proves expanded assembly, marker counts, map/position validity, deterministic placement, Browser catalog preservation and movement without LLM/provider calls.
- No runtime engine rewrite, package schema change, combat/dialogue/effect execution, Lua, Unity, solution or project changes were introduced.

Earlier parent slice foundation:

- Product Slice 008: Active Generated Package Flow + Quest/Dialogue Preview Stubs.
- Artifact Review now leaves the root `package.json` untouched and exposes an explicit `Use assembled package as current` action after successful assembly.
- `AssembledGamePackageActivationService` loads the current project's `.llmgc/package-assembly/package.json` through the existing repository, validates it, and only then replaces the active in-memory package.
- Runtime Preview starts the activated assembled package without manual copying and keeps the existing Generated Content Browser and Summary.
- NPC entries expose linked dialogue ids; dialogue preview appends title and lines to the log.
- Quest preview start/advance lives only in `GeneratedQuestDialoguePreviewService`; the new Quest Journal shows available, active and completed preview quests plus the current/next step.
- `active-package-quest-dialogue-preview` proves assembly, activation, generated-content startup, NPC/dialogue lookup, quest journal change and movement without LLM/provider calls or generated effect execution.
- No root package overwrite, runtime engine rewrite, package schema change, real quest/dialogue execution, Lua/effect, Unity, solution or project changes were introduced.

Earlier foundation:

- Product Slice 007: Generated Content Interaction Preview.
- Runtime Preview now exposes current scene, regions, NPCs, items, dialogues, quests, mechanics, encounters, applied artifacts and warnings as selectable read-only categories.
- Selection details include ids, descriptions, references, dialogue lines, quest steps/objectives, mechanic tags and artifact provenance/hash where available.
- The existing generated summary remains available on a separate `Summary` tab; `Append selected to log` writes a non-destructive message to the existing Runtime Preview log.
- Catalog refresh preserves a valid category/entry selection after Start and runtime commands when the selected ids still exist.
- `generated-content-interaction-preview` assembles the expanded fixture package, builds the projection/catalog, verifies detail coverage and confirms movement still works without LLM/provider calls.
- No runtime engine, package schema, generator contract, Lua/effect, Unity, solution or project changes were introduced.

Earlier contract foundation:

- Product Slice 006: Strict Contract Catalog + Batch Generation.
- Added strict contracts for regions, NPCs, items, dialogues and encounters with bounded JSON shapes, prompt instructions and contract validation.
- Added batch presets `baseline_game_seed`, `world_content_expansion`, `character_content_expansion`, `encounter_item_expansion` and `full_small_rpg_seed` through the strict contract catalog API.
- Approved expanded artifacts map into additive default-empty `generatedContent.regions`, `npcs`, `items`, `dialogues` and `encounters`; no simulation or effect execution was added.
- Runtime Preview exposes counts and summaries for all five expanded sections while preserving existing profile/scene/quest/mechanics output.
- The headless scenario `expanded-contract-batch-smoke` assembles all nine `full_small_rpg_seed` fixture contracts, exports `package.json`, verifies provenance and expanded sections, and builds the Runtime Preview projection without LLM/provider calls.
- The later Product Slice 010 moved the LLM Artifacts visual layout into a Designer-safe split while preserving this catalog API.

## Active manual gate

Completed for the sampled baseline contracts plus baseline assembly, generated Runtime Preview, expanded-contract batch smoke and Product Slices 007-012 through Generator Catalog contracts and non-executing planning. Further contract or real runtime execution expansion still requires an explicit controlled slice.

## Current user action

Use `baseline_generated_rpg_preview` as the current compatible composition reference and inspect its resolved current generator plan. Inspect the imported-map future preset for planned modules and missing `time.calendar` generator support. Existing generation remains behind explicit editor actions. Use all nine headless product smoke scenarios before choosing the next controlled slice.

## Allowed next Codex tasks

- Plan one controlled product vertical slice from the passed sampled baseline evidence, expanded strict catalog, batch presets and approved-artifact assembly path.
- Generate source-refreshed M5 entry executable specs after the user chooses the next vertical slice.
- Generate source-refreshed M6 entry planning only for the chosen product slice and only after explicit user approval.
- Tighten prompt, repair or validator behavior if future real evaluations reveal regressions.
- Add one carefully selected artifact contract only inside a controlled vertical slice with explicit scope and proof checks.
- Plan one controlled consumer of the Generator Catalog and GameBlueprint planning results without enabling plugins or generator execution.

## Restricted next Codex tasks

No longer blocked purely by missing real evaluation evidence, but still restricted until the user chooses a controlled product vertical slice and approves the specific task:

- M5 Lua module executor integration.
- M6 rich GamePackage assembly beyond the current baseline draft assembly.
- M6-lite package assembly shortcuts beyond the approved-artifact manual gate.

Still restricted:

- Broad contract expansion.
- Runtime preview repair loop.
- M8/M9/M10 production work.

## Current generator workflow

```text
Capability Picker
  -> Capability Composer v2 foundation
  -> Composable module selection UI
  -> LLM Artifacts
  -> project content language selection and language-bound prompts
  -> GameBlueprint preset selection model
  -> capability registry and deterministic compatibility validation
  -> generator catalog validation and non-executing plan resolution
  -> controlled batch preset selection
  -> LLM Evaluation
  -> Artifact Review
  -> Apply approved baseline artifacts
  -> Draft GamePackage assembly/export
  -> Headless product smoke
  -> Expanded strict contract batch smoke
  -> Generated content interaction preview
  -> Explicit assembled package activation
  -> Quest/dialogue preview-only session
  -> Generated NPC/encounter map placement preview
  -> controlled product vertical slice planning
```

## Where to start reading

1. AGENTS.md
2. docs/CONTEXT_INDEX.md
3. docs/CURRENT_GENERATOR_STATE.md
4. docs/ROADMAP_TO_FULL_GENERATOR.md
5. docs/M4_1_REAL_EVALUATION_GATE_REPORT.md
6. docs/GENERATOR_PLAN_CAPABILITY_SELECTION_PICKER.md
7. docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
8. docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
9. docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md
10. docs/PRODUCT_SMOKE_SCENARIOS.md

## What not to do next

- Do not treat one sampled baseline pass as broad contract expansion approval.
- Do not start M5/broad M6/M6-lite without an explicit next product vertical slice and user approval.
- Do not add runtime/provider/package mutation behavior beyond the approved-artifact draft assembly path as part of this gate record.
- Do not make Runtime Preview repair-loop work the immediate next step before the controlled vertical slice is chosen.
- Do not claim the whole generator is complete.

## Recommended next step

Choose a bounded catalog-backed composition planning slice from the official workbench plan, then refresh its executable task spec from current source.

Candidate slices:

- Catalog-backed composition planning diagnostics: consume `GameBlueprint` plus `GeneratorPlanningResult` without plugins, execution or package/runtime changes.
- A later composition UI may consume GameBlueprint presets only in a separate WinForms slice.
- One controlled semantic-generation contract only after the generator catalog direction is explicitly chosen.

## State update rule

Current state is manually updated by milestone tasks and automatically guarded by docs tests.

When a future Codex task completes a milestone or changes the recommended next step, update:

- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/CONTEXT_INDEX.md if new source docs were added
- docs/ROADMAP_TO_FULL_GENERATOR.md if milestone status or notes changed
