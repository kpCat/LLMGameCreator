# Current Generator State

Status: source-of-truth handoff  
Updated by: Codex task  
State file pair: docs/CURRENT_GENERATOR_STATE.json

## Current phase

M4.1 gate passed for sampled baseline contracts.

The project has a safe Capability Picker -> LLM Artifacts -> LLM Evaluation -> Artifact Review -> draft package assembly path for the sampled baseline strict contracts. A real local-model batch evaluation passed for the sampled baseline strict contracts, the first controlled product slice added a non-breaking Capability Composer v2 foundation on top of the existing picker, the follow-up UX repair made the picker readable and usable at normal editor sizes, Product Slice 002 wired composable module/modifier/constraint/runtime requirement selection into the picker and prompt context, Product Slice 003 lets approved baseline artifacts assemble an inspectable draft GamePackage, and Product Slice 004 adds headless smoke automation for baseline artifact package assembly.

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

- Product Slice 004: Headless Product Smoke Runner.
- Headless product smoke now verifies the baseline approved-artifact package assembly/export path without UI clicks.
- The smoke scenario `baseline-strict-package-assembly` uses deterministic fixture approved artifacts for all four sampled M4.1 baseline contracts:
  - `game_profile_v1`;
  - `scene_pack_v1`;
  - `quest_pack_v1`;
  - `mechanics_pack_v1`.
- The smoke runner exports `package.json`, asserts populated manifest/generatedContent profile/scenes/quests/mechanics, checks applied artifact provenance/content hashes for all four baseline contracts and writes a small run report under `.devflow/runs/<timestamp>-product-smoke/`.
- Product Slice 004 adds headless smoke automation for baseline artifact package assembly. Manual checking remains needed only for UI-specific changes and major new runtime gates.
- The smoke path does not call LLM/provider APIs, LM Studio, Lua, runtime preview, WinForms UI automation or generator-library execution.

## Active manual gate

Completed for the sampled baseline contracts and Product Slice 004 headless baseline package assembly smoke. Broad expansion still requires an explicit next controlled slice.

## Current user action

Use the headless product smoke for non-UI baseline package assembly checks, keep manual UI checks for UI-specific behavior only, then choose the next controlled product slice before starting M5, broad M6, M6-lite shortcuts or runtime preview repair-loop work.

## Allowed next Codex tasks

- Plan one controlled product vertical slice from the passed sampled baseline evidence, Capability Composer v2 foundation and narrow approved-artifact assembly path.
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

Choose the next controlled product vertical slice and then refresh the next executable task specs from current source.

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
