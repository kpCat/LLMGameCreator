# M4.1 Real Evaluation Gate Report

## Evidence

- Evaluation id: `strict_llm_evaluation/58df49dadbff5598`
- Evaluated at: `2026-06-18T16:43:35.9475873+00:00`
- Source capability selection id: `generator_plan_capability_selection/0b0addcd5c019328`
- Mode: `batch`
- Requested contracts: `game_profile_v1`, `mechanics_pack_v1`, `quest_pack_v1`, `scene_pack_v1`
- Iterations: `1`
- Repair enabled: `True`
- Stage for review: `True`
- Expected max LLM calls: `8`

## Metrics

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

## Interpretation

The sampled baseline strict LLM generation/evaluation path is stable for the requested first contracts. The run produced four valid artifacts, all passed initially, no repair pass was needed, and no parser, wrapper, validation diagnostic or deterministic quality warning was recorded.

## Decision

M4.1 real-model evaluation gate passed for sampled baseline contracts.

## Remaining constraints

- This does not mean broad contract expansion is automatically safe.
- This does not mean the whole generator is complete.
- M5, M6 and M6-lite work still require explicit user approval for a selected controlled product vertical slice.
- Broad contract expansion remains restricted.
- Runtime preview repair-loop work should wait until a controlled vertical slice exists.
- Runtime must not call LLM or provider integrations.
- Artifact Review remains the human approval gate before promotion.

## Recommended next step

Choose one controlled product vertical slice before starting M5/M6/M6-lite work.

Candidate slices:

- Lua-backed content slice: one safe generator module family producing validated artifact envelopes.
- Package assembly slice: map the passed baseline artifacts into a richer but still narrow GamePackage assembly path.
- Artifact contract slice: add exactly one contract family needed by the chosen product direction, then rerun strict evaluation for that family.
