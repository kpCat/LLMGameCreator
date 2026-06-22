# Current Generator State

Status: source-of-truth handoff  
Updated by: Codex task  
State file pair: docs/CURRENT_GENERATOR_STATE.json

## Current phase

M4.1 gate passed for sampled baseline contracts; Product Slice 019 adds deterministic Unity archive game-data payload materialization.

The project has a safe Capability Picker -> LLM Artifacts -> LLM Evaluation -> Artifact Review -> draft package assembly path. Product Slices 011-018 established immutable composition models, non-executing catalog/diagnostics/export, a read-only Composition Workbench and deterministic Unity archive contract/meta materialization. Product Slice 019 optionally adds the existing assembled/current package plus stable category indexes under the archive `data/` folder without implementing Unity or changing Runtime/GamePackage schema.

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

- Product Slice 019: Unity Archive Game Data Payload v1.
- `UnityArchiveGameDataPayloadService` writes the supplied existing `GamePackageDefinition` to `.llmgc/unity-archive/data/game-package.json` and extracts stable indexes for scenes, NPCs, quests, dialogues, items and encounters.
- Category entries come only from existing core package/generated-content structures; missing categories produce valid empty indexes, while ids, tags and linked ids are sorted deterministically.
- `UnityArchiveMaterializationService` includes payload files only when package data is explicitly supplied. Future-module materialization without package data remains metadata-only and does not claim playable data.
- All data paths are containment-checked, JSON is UTF-8 without BOM, indexes contain no timestamps and repeated unchanged materialization is byte-identical.
- `unity-archive-game-data-payload` proves the required data files, valid package/category JSON, empty categories, deterministic output and future metadata-only behavior without Unity, provider, generator, Runtime, package schema, Lua or WinForms calls.
- Product Slice 018 is accepted/completed as the archive materialization parent foundation.

Parent slice foundation:

- Product Slice 018: Unity Archive Materialization v1.
- `UnityArchiveMaterializationService` consumes the Slice 017 dry run and writes a deterministic UTF-8 contract/meta archive under `.llmgc/unity-archive/`.
- The archive contains manifest, design brief, target profile, runtime module, UI layout, asset/audio request, localization and Lua module indexes plus export report and validation JSON.
- Current targets materialize a playable archive contract; future-module targets materialize metadata only, while missing/invalid inputs write validation output only.
- All output paths are fixed, sorted and containment-checked; repeated unchanged materialization is byte-identical.
- `unity-archive-materialization` proves required files, valid archive JSON, current runtime module metadata, future metadata-only behavior and safe relative outputs without Unity, provider, generator, Runtime, package schema, Lua or WinForms calls.
- Optional zip output is intentionally not implemented in v1.
- Product Slice 017 is accepted/completed as the dry-run parent foundation.

Parent slice foundation:

- Product Slice 017: Unity Archive Validation/Export Dry Run.
- `UnityArchiveExportDryRunService` consumes the Slice 016 design brief, target profile, archive manifest and runtime module catalog through the existing Unity target validator.
- The dry run writes deterministic UTF-8 plan JSON/markdown, archive manifest JSON and validation report JSON under `.llmgc/unity-export-dry-run/`.
- Planned archive paths are stable, sorted and containment-checked; unsafe/traversal paths are diagnosed and excluded from the plan.
- Readiness distinguishes `ExportableNow`, `ExportableWithWarnings`, `BlockedByFutureModules`, `MissingRequirements` and `Invalid`.
- Product Slice 016 is accepted/completed as the Unity target contract parent foundation.

Parent slice foundation:

- Product Slice 016: Unity Target Contract + Game Design Brief Foundation.
- `GameDesignBrief` records design intent and responsibility policies; `UnityTargetProfile`, `UnityGameArchiveManifest` and related records define archive-first metadata for a future generic Unity player.
- `UnityTargetContractPresetProvider` exposes three target profiles and 22 runtime module contracts, while `UnityTargetContractValidator` keeps current/future and malformed-contract diagnostics deterministic.
- `unity-target-contract` proves current target/archive validity, future warnings and lazy NPC/quest/delta policy without Unity, provider, generator, Runtime, package schema, Lua or WinForms calls.
- Product Slice 015 is accepted/completed as the read-only Composition Workbench parent foundation.

Parent slice foundation:

- Product Slice 015: Read-only Composition Workbench UI.
- `CompositionWorkbenchPageControl` lists built-in blueprint presets and saved report entries, shows readiness/actions, and renders markdown in a read-only view.
- `CompositionWorkbenchPresenter` delegates readiness/report construction to the existing composition services, exports through `GameCompositionDiagnosticsExportService`, and refreshes the existing `.llmgc/composition-diagnostics/index.json` contract.
- When no project is loaded, in-memory preview remains available while export/saved-report actions show a clear read-only status.
- The page is registered through `CompositionRoot`/`EditorPageRegistry`, keeps its layout in `CompositionWorkbenchPageControl.Designer.cs`, and has a parameterless design-time-safe constructor.
- `composition-workbench-readonly` proves page/presenter construction, baseline report/markdown, export and saved-report readback without LLM/provider, plugins, generator execution, Runtime, package mutation or Lua.
- Product Slice 014 is accepted/completed as the deterministic persistence/export parent foundation.

Parent slice foundation:

- Product Slice 014: Headless Composition Report Export.
- `GameCompositionDiagnosticsExportService` persists the existing deterministic markdown under `.llmgc/composition-diagnostics/<safe-blueprint-id>.composition-report.md`.
- `.llmgc/composition-diagnostics/index.json` records blueprint id, title, readiness, content language and report file name, with entries sorted deterministically by blueprint id and no timestamps.
- Blueprint ids are reduced to a filename-safe ASCII allowlist, empty results fall back to `blueprint`, and resolved output paths are checked to remain under the project root/output directory.
- Markdown and index are written as UTF-8 without BOM; repeated unchanged export is byte-identical.
- `composition-report-export` proves directory/report/index creation, required markdown sections, deterministic repeated export and traversal containment without LLM/provider, plugin, generator execution, Runtime, package, Lua or UI calls.
- Product Slice 013 is accepted/completed as the deterministic diagnostics and renderer parent foundation.

Parent slice foundation:

- Product Slice 013: Catalog-backed Composition Diagnostics Foundation.
- `GameCompositionDiagnosticsService` combines `GameBlueprintCompositionValidator`, `GeneratorCatalogValidator` and `GeneratorPlanResolver` without executing generators.
- `GameCompositionDiagnosticsReport` records blueprint identity/language, readiness, native validation/planning results, consolidated diagnostics, current/planned generator ids, missing generator support and recommended actions.
- Readiness is deterministic across `BuildableNow`, `BuildableWithWarnings`, `PlannedFuture`, `MissingRequirements`, `Conflict` and `Invalid`.
- Missing capabilities, conflicting capabilities, planned generators and missing generator support produce stable, sorted actions.
- `GameCompositionDiagnosticsMarkdownRenderer` emits timestamp-free sections for blueprint, readiness, content language, diagnostics, generator selections/support and actions.
- `composition-diagnostics-report` proves buildable baseline, future imported-map diagnostics, broken-blueprint errors and deterministic markdown without LLM/provider, plugin, generator execution, Runtime, package, Lua or UI calls.
- Product Slice 012 remains accepted/completed as the Generator Catalog parent foundation.

Parent slice foundation:

- Product Slice 012: Generator Catalog Contract Foundation.
- `GeneratorModuleManifest`, `BuiltInGeneratorCatalog`, `GeneratorCatalogValidator` and `GeneratorPlanResolver` provide 12 current and 8 planned manifests plus deterministic non-executing planning.
- `generator-catalog-contract` proves catalog validity, current/planned manifest coverage, baseline resolution and imported-map future diagnostics.

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

Completed for the sampled baseline contracts plus baseline assembly, generated Runtime Preview, expanded-contract batch smoke and Product Slices 007-019 through deterministic Unity archive game-data payload materialization. Further Unity, provider, generator or Runtime execution expansion still requires an explicit controlled slice.

## Current user action

Inspect the deterministic `.llmgc/unity-archive/` contract/meta/data output and choose one controlled follow-up slice. Existing generation remains behind explicit editor actions. Use all sixteen headless product smoke scenarios before choosing any implementation slice.

## Allowed next Codex tasks

- Plan one controlled product vertical slice from the passed sampled baseline evidence, expanded strict catalog, batch presets and approved-artifact assembly path.
- Generate source-refreshed M5 entry executable specs after the user chooses the next vertical slice.
- Generate source-refreshed M6 entry planning only for the chosen product slice and only after explicit user approval.
- Tighten prompt, repair or validator behavior if future real evaluations reveal regressions.
- Add one carefully selected artifact contract only inside a controlled vertical slice with explicit scope and proof checks.
- Plan one bounded retention/history policy for composition reports without enabling plugins or generator execution.
- Plan one bounded read-only Unity archive review/inspection slice without a Unity project, Runtime changes or GamePackage schema changes.

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
  -> consolidated composition diagnostics and recommended actions
  -> deterministic project-local composition report export
  -> read-only Composition Workbench preview/export/saved-report refresh
  -> Game Design Brief and Unity target/archive contract validation
  -> deterministic Unity archive export dry-run planning
  -> deterministic Unity archive contract/meta materialization
  -> deterministic Unity archive game-data payload materialization when package data is supplied
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

Choose one bounded read-only materialized Unity archive review/inspection slice for contract/meta/data output without implementing Unity, changing Runtime or changing GamePackage schema.

Candidate slices:

- A headless report retention/history policy may be planned separately if multiple versions are needed; Slices 014-015 intentionally maintain and consume one deterministic report per blueprint id.
- A read-only editor-side view may inspect the materialized archive contract, validation report and game-data indexes without creating a Unity project or modifying package schema.
- One controlled semantic-generation contract only after the generator catalog direction is explicitly chosen.

## State update rule

Current state is manually updated by milestone tasks and automatically guarded by docs tests.

When a future Codex task completes a milestone or changes the recommended next step, update:

- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/CONTEXT_INDEX.md if new source docs were added
- docs/ROADMAP_TO_FULL_GENERATOR.md if milestone status or notes changed
