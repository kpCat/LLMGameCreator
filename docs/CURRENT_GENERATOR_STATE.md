# Current Generator State

Status: source-of-truth handoff  
Updated by: Codex task  
State file pair: docs/CURRENT_GENERATOR_STATE.json

## Current phase

M4.1 gate passed for sampled baseline contracts; Product Slice 006 expanded the strict contract catalog and added controlled batch presets.

The project has a safe Capability Picker -> LLM Artifacts -> LLM Evaluation -> Artifact Review -> draft package assembly path. Product Slices 001-005 established composable selection, baseline strict generation, approved-artifact assembly, headless smoke and generated-content Runtime Preview. Product Slice 006 adds `region_pack_v1`, `npc_pack_v1`, `item_pack_v1`, `dialogue_pack_v1` and `encounter_pack_v1`, five controlled batch presets, non-breaking typed `generatedContent` mapping and expanded headless smoke coverage.

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

- Product Slice 006: Strict Contract Catalog + Batch Generation.
- Added strict contracts for regions, NPCs, items, dialogues and encounters with bounded JSON shapes, prompt instructions and contract validation.
- Added batch presets `baseline_game_seed`, `world_content_expansion`, `character_content_expansion`, `encounter_item_expansion` and `full_small_rpg_seed` through the strict contract catalog API.
- Approved expanded artifacts map into additive default-empty `generatedContent.regions`, `npcs`, `items`, `dialogues` and `encounters`; no simulation or effect execution was added.
- Runtime Preview exposes counts and summaries for all five expanded sections while preserving existing profile/scene/quest/mechanics output.
- The headless scenario `expanded-contract-batch-smoke` assembles all nine `full_small_rpg_seed` fixture contracts, exports `package.json`, verifies provenance and expanded sections, and builds the Runtime Preview projection without LLM/provider calls.
- A preset dropdown was not added because the existing LLM Artifacts control has no Designer split; the catalog API is the safe foundation for a later Designer-safe UI task.

## Active manual gate

Completed for the sampled baseline contracts plus baseline assembly, generated Runtime Preview and Product Slice 006 expanded-contract batch smoke. Further contract or runtime expansion still requires an explicit controlled slice.

## Current user action

Use the three headless product smoke scenarios for baseline assembly, generated Runtime Preview and expanded contract batches. Choose the next controlled slice before M5, broader M6, simulation or repair-loop work.

## Allowed next Codex tasks

- Plan one controlled product vertical slice from the passed sampled baseline evidence, expanded strict catalog, batch presets and approved-artifact assembly path.
- Generate source-refreshed M5 entry executable specs after the user chooses the next vertical slice.
- Generate source-refreshed M6 entry planning only for the chosen product slice and only after explicit user approval.
- Tighten prompt, repair or validator behavior if future real evaluations reveal regressions.
- Add one carefully selected artifact contract only inside a controlled vertical slice with explicit scope and proof checks.

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
  -> LLM Evaluation
  -> Artifact Review
  -> Apply approved baseline artifacts
  -> Draft GamePackage assembly/export
  -> Headless product smoke
  -> Expanded strict contract batch smoke
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

Choose the next controlled product vertical slice from the expanded strict artifact and package-preservation foundation, then refresh its executable task spec from current source.

Candidate slices:

- Lua-backed content slice: one safe generator module family producing validated artifact envelopes.
- Richer package assembly slice: expand beyond the current baseline draft assembly only for one chosen contract family or gameplay domain.
- Artifact contract slice: add exactly one contract family needed by the chosen product direction, then rerun strict evaluation for that family.

## State update rule

Current state is manually updated by milestone tasks and automatically guarded by docs tests.

When a future Codex task completes a milestone or changes the recommended next step, update:

- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/CONTEXT_INDEX.md if new source docs were added
- docs/ROADMAP_TO_FULL_GENERATOR.md if milestone status or notes changed
