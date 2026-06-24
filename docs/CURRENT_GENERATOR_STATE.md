# Current Generator State

Status: source-of-truth handoff  
Updated by: Product Slice 030 Formula/Effect/Action Registry Foundation  
State file pair: `docs/CURRENT_GENERATOR_STATE.json`

## Current Phase

M4.1 passed for sampled baseline contracts. Product Slice 029 completed the first deterministic seeded procedural game kernel. Product Slice 030 turned the generated placeholders into a deterministic validated formula/effect/action rule-pack foundation.

The active product direction remains the generated playable/simulatable procedural generator loop. Slice 029 proved the first runtime-facing generated plan; Slice 030 produced validated runtime-facing rules for that plan; Slice 031 should consume both in a tiny generated runtime loop.

Source of truth for the reset:

- `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
- `docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md`

## Active Strategy Reset

Infrastructure-only progress is frozen unless explicitly requested by the user or required to unblock the playable/simulatable generated loop.

Frozen by default:

- semantic catalog review UI;
- manual import UI polish;
- archive review/history/comparison polish;
- extra report formats;
- broad artifact-contract expansion without generated runtime outcome;
- more safety wrappers around a generator kernel that does not exist yet.

Allowed next sequence:

1. Product Slice 030: Formula/Effect/Action Registry Foundation.
2. Product Slice 031: Tiny Generated Runtime Loop.

Kill criterion:

```text
If no generated playable or simulatable loop exists after the next three large
product slices, stop and reassess architecture before spending more limit.
```

## Recommended Next Work

Recommended next work item:

```text
tiny_generated_runtime_loop
```

Recommended next task file is not present yet. Draft it from the strategy reset Slice C requirements before implementation if the user asks to execute Slice 031.

## Product Slice 030 Summary

Product Slice 030: Formula/Effect/Action Registry Foundation.

Completed behavior:

- added an Application-layer deterministic formula/effect/action rule-pack generator;
- mapped Slice 029 placeholders for route access, faction access, encounter resolution and quest progress;
- produced byte-stable `.llmgc/procedural/formula-effect-action-rule-pack.json`;
- produced deterministic `.llmgc/procedural/formula-effect-action-rule-pack.md`;
- produced deterministic validation-report JSON and Markdown sidecars;
- validated formula ids, declared variables, safe expressions, rule ids and rule/source refs;
- added the `formula-effect-action-registry` product smoke scenario;
- did not add UI, provider execution, LLM calls, Lua execution, Unity work, media generation, C# code generation, GamePackage schema changes or runtime command/state changes.

Recorded checks from S030:

- `FormulaEffectActionRegistry` filtered tests: 4/4 passed.
- `formula-effect-action-registry` product smoke: 1/1 passed.

## Product Slice 029 Summary

Product Slice 029: Seeded Procedural Game Kernel v1.

Completed behavior:

- added an Application-layer deterministic procedural game kernel;
- accepted seed, mode, compact semantic/style hints and selected variant ids with safe defaults;
- produced byte-stable `.llmgc/procedural/generated-game-plan.json`;
- produced deterministic `.llmgc/procedural/generated-game-plan.md`;
- generated runtime-facing regions, factions, actor seeds, item/resource seeds, encounter seeds and quest/event seeds;
- kept formula/effect/action behavior as explicit placeholders for Slice 030;
- added the `procedural-game-kernel` product smoke scenario;
- did not add UI, provider execution, LLM calls, Lua execution, Unity work, media generation, C# code generation, GamePackage schema changes or runtime command/state changes.

Recorded checks from S029:

- `ProceduralGameKernel` filtered tests: 5/5 passed.
- `procedural-game-kernel` product smoke: 1/1 passed.

## Product Slice 028 Summary

Product Slice 028: Manual Import Repair + Semantic Catalog Foundation v1.

Completed behavior:

- repaired safe `manual-import` directory creation;
- prevented no-op review-history snapshots when target bytes do not change;
- mapped approved `semantic_pack_v1` artifacts into deterministic `.llmgc/semantic/` sidecars;
- added deterministic semantic generation-context preview;
- recorded LLM minimization policy.

Recorded checks from S028:

- `ManualImport` / `UnityArchiveReview` filtered tests: 54/54 passed.
- `Semantic` filtered tests: 9/9 passed.
- `semantic-catalog-foundation` product smoke: 1/1 passed.
- `unity-archive-manual-import-workflow-ui` product smoke: 1/1 passed.
- `ProductSmoke` filtered tests: 27/27 passed.
- `check-devflow-state.ps1`: passed in `STOP_REVIEW` mode.
- `check-all.ps1`: 655/655 tests passed, build 0 warnings / 0 errors.

## Gate Decision

M4.1 real-model evaluation gate passed for sampled baseline contracts.

Evidence:

- Evaluation id: `strict_llm_evaluation/58df49dadbff5598`
- Evaluated at: `2026-06-18T16:43:35.9475873+00:00`
- Source capability selection id: `generator_plan_capability_selection/0b0addcd5c019328`
- Requested contracts: `game_profile_v1`, `mechanics_pack_v1`, `quest_pack_v1`, `scene_pack_v1`
- Iterations: `1`
- Repair enabled: `True`
- Stage for review: `True`
- Expected max LLM calls: `8`
- Overall pass rate: `1.0`

This gate does not authorize broad contract expansion, provider execution, Unity implementation, runtime preview repair loop, rich GamePackage assembly or Lua module execution by itself.

## M5/M6 Lock Semantics

M5 and M6 remain locked after S028 and after the strategy reset.

Currently locked or restricted:

- M5 Lua module executor integration remains locked until a controlled product vertical slice explicitly selects it and the user approves the unlock.
- M6 rich GamePackage assembly beyond the current baseline draft assembly remains locked until a controlled product vertical slice explicitly selects it and the user approves the unlock.
- Broad contract expansion remains restricted beyond sampled baseline evidence.
- Runtime preview repair loop remains restricted until the procedural kernel and tiny generated runtime loop are selected and implemented.

## Current Workflow Foundation

Existing work remains valuable and should be reused:

- Capability Picker;
- LLM Artifacts;
- LLM Evaluation;
- Artifact Review;
- deterministic draft package assembly/export;
- Unity archive materialization and request planning;
- manual provider output import;
- review/history/comparison;
- semantic sidecar and generation-context preview;
- C# validation authority;
- headless runtime services.

The pivot is not a restart. It changes what future slices are allowed to optimize.

## Required Reading Order For New Agents

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
5. `docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md`
6. `docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md`
7. `docs/FULL_GAME_GENERATION_MASTER_PLAN.md`
8. `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md`

Do not read old root `README_APPLY_*` files or old `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_CODEX_PROMPT.md` files as current planning authority.

## State Update Rule

Update this file pair after every accepted product slice.

Preserve M5/M6 locked semantics until explicitly unlocked by the user.

After Product Slice 030 completes, this state should recommend:

```text
tiny_generated_runtime_loop
```
