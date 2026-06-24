# Current Generator State

Status: source-of-truth handoff  
Updated by: Product Slice 033 Visible Generated Playable Preview  
State file pair: `docs/CURRENT_GENERATOR_STATE.json`

## Current Phase

M4.1 passed for sampled baseline contracts. Product Slice 029 completed the first deterministic seeded procedural game kernel. Product Slice 030 turned the generated placeholders into a deterministic validated formula/effect/action rule-pack foundation. Product Slice 031 consumed both artifacts in a tiny deterministic generated runtime loop. Product Slice 032 maps the S029-S031 sidecars into a minimal generated package MVP artifact. Product Slice 033 exposes that package MVP through the existing runtime-preview projection path and writes deterministic visible-preview sidecars.

The active product direction remains the generated playable/simulatable procedural generator loop. Slice 029 proved the first runtime-facing generated plan; Slice 030 produced validated runtime-facing rules for that plan; Slice 031 proved the plan and rules can produce visible state transitions in an Application-layer simulation; Slice 032 proves the generated sidecars can cross into existing `GamePackage` contracts with validation and bootstrap evidence; Slice 033 proves the generated package can be projected for a visible preview and smoke-started through the existing headless runtime path.

Source of truth for the reset:

- `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
- `docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md`

Historical strict-evaluation gate context remains discoverable at `docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md`.

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

1. Manual User Preview Verification: user launches the app/preview from Visual Studio and verifies the generated package MVP is visible and minimally playable before Codex receives more feature slices.

Kill criterion:

```text
If no generated playable or simulatable loop exists after the next three large
product slices, stop and reassess architecture before spending more limit.
```

## Recommended Next Work

Recommended next work item:

```text
manual_user_preview_verification
```

Manual User Preview Verification: user launches the app/preview from Visual Studio and verifies the generated package MVP is visible and minimally playable before Codex receives more feature slices.

## Product Slice 033 Summary

Product Slice 033: Visible Generated Playable Preview.

Completed behavior:

- clarified Generated Package MVP provenance so `GeneratedContent.AppliedArtifacts.ContentHash` is explicitly the pre-provenance package hash while `GeneratedPackageMvpReport.PackageHash` remains the final `package.json` hash;
- added an Application-layer visible generated playable preview service in `RuntimePreview`;
- ran the S029 plan, S030 rule pack, S031 tiny loop and S032 generated package MVP pipeline in one deterministic preview path;
- reused `GeneratedPackageRuntimePreviewService` instead of creating a parallel projection architecture;
- kept `LLMGameCreator.Application` free of a direct `LLMGameCreator.Runtime` implementation dependency by using a small runtime adapter contract;
- proved runtime start, movement and interaction in product smoke through `DefaultGameRuntime`;
- produced deterministic `.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-snapshot.json`;
- produced deterministic `.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-report.json`;
- produced deterministic `.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-report.md`;
- produced deterministic `.llmgc/procedural/visible-generated-playable-preview/manual-verification.md`;
- added the `visible-generated-playable-preview` product smoke scenario;
- added `docs/MANUAL_VISIBLE_GENERATED_PLAYABLE_PREVIEW_CHECK.md`;
- did not add UI, provider execution, LLM calls, Lua execution, Unity work, media generation, broad GamePackage schema changes or public runtime command/state changes.

Recorded checks from S033:

- `GeneratedPackageMvp` filtered tests: 5/5 passed.
- `VisibleGeneratedPlayablePreview` filtered tests: 4/4 passed.
- `visible-generated-playable-preview` product smoke: 1/1 passed.
- `check-all.ps1`: 678/678 tests passed, build 0 warnings / 0 errors.

## Product Slice 032 Summary

Product Slice 032: Generated Package MVP.

Completed behavior:

- repaired S031 handoff state so active future work no longer listed completed S030/S031 tasks;
- added the missing `formula-effect-action-registry` product smoke route and summary path;
- added an Application-layer deterministic generated package MVP service;
- consumed Slice 029 `ProceduralGeneratedGamePlan`, Slice 030 `FormulaEffectActionRulePack` plus validation output, and Slice 031 tiny runtime loop result;
- produced byte-stable `.llmgc/procedural/generated-package-mvp/package.json`;
- produced deterministic `.llmgc/procedural/generated-package-mvp/generated-package-mvp-report.json`;
- produced deterministic `.llmgc/procedural/generated-package-mvp/generated-package-mvp-report.md`;
- produced deterministic `.llmgc/procedural/generated-package-mvp/runtime-bootstrap-report.json`;
- produced deterministic `.llmgc/procedural/generated-package-mvp/runtime-bootstrap-report.md`;
- mapped generated regions to existing maps/GeneratedContent regions;
- mapped generated actors, items/resources, factions, encounters, quests, dialogues, interactions, formulas and rule-pack actions into existing package-supported structures where available;
- ran existing package validation and included validation issues in reports;
- recorded bootstrap evidence through an Application-layer package-contract adapter because `LLMGameCreator.Application` does not reference `LLMGameCreator.Runtime` implementations;
- diagnosed report-only concepts such as region connections and richer rule effects instead of expanding package/runtime schema;
- added the `generated-package-mvp` product smoke scenario;
- did not add UI, provider execution, LLM calls, Lua execution, Unity work, media generation, broad GamePackage schema changes or public runtime command/state changes.

Recorded checks from S032:

- `CurrentGeneratorStateDocsTests` filtered tests: 10/10 passed.
- `formula-effect-action-registry` product smoke: 1/1 passed.
- `GeneratedPackageMvp` filtered tests: 5/5 passed.
- `generated-package-mvp` product smoke: 1/1 passed.
- `check-all.ps1`: 674/674 tests passed, build 0 warnings / 0 errors.

## Product Slice 031 Summary

Product Slice 031: Tiny Generated Runtime Loop.

Completed behavior:

- repaired stale current-state docs guard expectations without reverting to the old M4.1 next-step recommendation;
- added an Application-layer deterministic tiny runtime loop service;
- consumed Slice 029 `ProceduralGeneratedGamePlan` and Slice 030 `FormulaEffectActionRulePack` plus validation output;
- produced byte-stable `.llmgc/procedural/tiny-runtime-loop-state.json`;
- produced deterministic `.llmgc/procedural/tiny-runtime-loop-report.json`;
- produced deterministic `.llmgc/procedural/tiny-runtime-loop-report.md`;
- selected a starting region, movement/exploration transition, encounter seed and quest/event seed;
- applied generated action/effect ids into tiny state: flags, inventory item grants, quest/event state and faction reputation deltas;
- diagnosed missing inputs, missing refs and unsupported action/effect types without normal-operation crashes;
- added the `tiny-generated-runtime-loop` product smoke scenario;
- did not add UI, provider execution, LLM calls, Lua execution, Unity work, media generation, GamePackage schema changes or public runtime command/state changes.

Recorded checks from S031:

- `CurrentGeneratorStateDocsTests` filtered tests: 10/10 passed.
- `TinyGeneratedRuntimeLoop` filtered tests: 4/4 passed.
- `tiny-generated-runtime-loop` product smoke: 1/1 passed.

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
- Runtime preview repair loop remains restricted until a controlled generated package/playable MVP slice explicitly maps the generated plan, rules and tiny-loop output into the minimal package/runtime path.

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
- seeded procedural generated plan sidecars;
- formula/effect/action rule-pack sidecars;
- tiny generated runtime loop state/report sidecars;
- generated package MVP package/report/runtime-bootstrap sidecars;
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

After Product Slice 033 completes, this state should recommend:

```text
manual_user_preview_verification
```
