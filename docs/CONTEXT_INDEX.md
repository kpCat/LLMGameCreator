# CONTEXT_INDEX.md

Purpose: reduce repeated orientation cost for Codex/LLM agents.

Read this file after `AGENTS.md` when a task touches code. This file is a routing index, not a replacement for detailed docs. If this file conflicts with a more specific doc, the specific doc wins.

## Generator Task Routing

For any generator/Codex task:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
5. `docs/ROADMAP_TO_FULL_GENERATOR.md`
6. only then task-specific docs

For Product Slice 029 specifically, read:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
5. `docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md`
6. runtime/domain files needed by the task

## Active Strategy Reset

The active post-S028 direction is:

```text
Before expanding the platform, prove the generated game kernel.
```

Infrastructure-only work is frozen unless explicitly requested by the user or directly required to unblock a generated playable/simulatable loop.

Old apply READMEs, old product-slice prompts, old task-pack prompts and historical archive manifests are not current planning authority.

## Full Generator Source-Of-Truth Docs

Read these before broad generation, capability, prompt, Lua integration, artifact-contract, roadmap or Codex-task-shaping work:

| Document | Use when |
|---|---|
| `docs/CURRENT_GENERATOR_STATE.md` | Starting any generator/Codex task; checking the active phase, recommended next action and blocked milestones. |
| `docs/CURRENT_GENERATOR_STATE.json` | Machine-readable mirror of current state for tooling/tests. |
| `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md` | Enforcing the post-S028 pivot from infrastructure growth to a generated playable/simulatable procedural kernel. |
| `docs/NEXT_PRODUCT_SLICE_029_SEEDED_PROCEDURAL_GAME_KERNEL_TASK.md` | Completed Product Slice 029 task: Seeded Procedural Game Kernel v1. |
| `docs/NEXT_PRODUCT_SLICE_030_FORMULA_EFFECT_ACTION_REGISTRY_TASK.md` | Completed Product Slice 030 task: Formula/Effect/Action Registry Foundation. |
| `docs/NEXT_PRODUCT_SLICE_031_TINY_GENERATED_RUNTIME_LOOP_TASK.md` | Completed Product Slice 031 task: Tiny Generated Runtime Loop. |
| `docs/NEXT_PRODUCT_SLICE_032_GENERATED_PACKAGE_MVP_TASK.md` | Completed Product Slice 032 task: Generated Package MVP. |
| `docs/NEXT_PRODUCT_SLICE_033_VISIBLE_GENERATED_PLAYABLE_PREVIEW_TASK.md` | Completed Product Slice 033 task: Visible Generated Playable Preview. |
| `docs/NEXT_PRODUCT_SLICE_034_ONE_CLICK_GENERATED_PREVIEW_WORKFLOW_TASK.md` | Completed Product Slice 034 task: One-Click Generated Preview Workflow. |
| `docs/NEXT_PRODUCT_SLICE_035_ACTIVE_GOAL_QUEST_PROGRESS_TASK.md` | Completed Product Slice 035 task: Active Goal + Quest Progress Loop. |
| `docs/NEXT_PRODUCT_SLICE_036_ENCOUNTER_REWARD_COMPLETION_TASK.md` | Completed Product Slice 036 task: Encounter/Obstacle + Reward/Completion Loop. |
| `docs/NEXT_PRODUCT_SLICE_037_MICROGAME_ACCEPTANCE_POLISH_TASK.md` | Completed Product Slice 037 task: Microgame Acceptance + Playability Polish. |
| `docs/NEXT_PRODUCT_SLICE_038_RUNTIME_OWNED_GOAL_PROGRESS_TASK.md` | Completed Product Slice 038 task: Runtime-Owned Generated Goal Progress. |
| `docs/NEXT_PRODUCT_SLICE_039_RUNTIME_REWARD_CHALLENGE_STATE_TASK.md` | Completed Product Slice 039 task: Runtime-Backed Reward/Challenge/Completion State. |
| `docs/NEXT_PRODUCT_SLICE_040_RUNTIME_MICROGAME_STATE_ACCEPTANCE_TASK.md` | Completed Product Slice 040 task: Runtime Microgame State Acceptance. |
| `docs/NEXT_PRODUCT_SLICE_041_GENERATION_PRESETS_AND_OPTIONS_TASK.md` | Completed Product Slice 041 task: Generation Presets and Options. |
| `docs/NEXT_PRODUCT_SLICE_042_MICROGAME_VARIATION_ACCEPTANCE_TASK.md` | Completed Product Slice 042 task: Microgame Variation Acceptance. |
| `docs/GOAL_007_CONNECTED_WORLD_TRAVEL_AND_DETERMINISTIC_WORLD_STATE.md` | Completed Goal 007 task: bounded connected regions, deterministic travel, runtime-owned world state and chunk delta evidence. |
| `docs/GOAL_008_RULE_PACK_GAMEPLAY_FAMILY_FOUNDATIONS.md` | Completed Goal 008 task: rule-pack gameplay family foundations for inventory, equipment, crafting, trading and status/effect evidence; S077A repaired the runtime-integration correctness evidence while keeping `rule_pack_gameplay_family_artifact_verification` as the stop gate. |
| `docs/GOAL_009_RULE_PACK_COMBAT_FACTION_SOCIAL_WORK_THEFT.md` | Completed Goal 009 task: rule-pack combat/faction/social/work/theft foundations with real runtime encounter, reputation, dialogue, work-contract and theft-consequence evidence; stops at `rule_pack_combat_faction_social_work_theft_artifact_verification`. |
| `docs/GOAL_010_CONTENT_GENERATION_AT_SCALE.md` | Completed Goal 010 task plus S091A correctness hotfix: compact content-pack driven deterministic generation at scale with real package materialization, generated-id runtime threads, objective/event coercion rejection, strict runtime command correlation, repetition metrics, invalid/fake/leak rejection and final stop at `content_generation_at_scale_artifact_verification`. |
| `docs/GOAL_011_MINIMUM_ASSET_PIPELINE.md` | Completed Goal 011 task plus S098A correctness hotfix: deterministic asset requests from generated/package content ids, local fixture imports, deterministic fallbacks, existing AssetCatalog/package metadata binding, hash-integrity checks, causal invalid/fake/leak rejection, structural validation, product smoke and final stop at `minimum_asset_pipeline_artifact_verification`. |
| `docs/GOAL_012_UNITY_RUNTIME_EXPORT_VERTICAL_SLICE.md` | Completed Goal 012 task: deterministic Unity runtime export vertical slice outside Runtime Preview with selected Goal 010 package/runtime refs, Goal 011 asset refs, real export files, hash/byte manifest validation, causal invalid/fake/leak rejection, product smoke and final stop at `unity_runtime_export_vertical_slice_artifact_verification`. |
| `docs/GOAL_013_ALPHA_RUNNABLE_WINDOWS_BUILD.md` | Completed Goal 013 task: Alpha runnable Windows build integration. S113B resolved the repository-local Unity project/template and Windows build entrypoint blocker, produced a real Windows player plus diagnostic launch evidence; S113C added a visible Unity mini-loop plus automated play-loop diagnostic evidence. The user confirmed `alpha_runnable_windows_build_verification passed` before Goal 014. |
| `docs/GOAL_014_UNITY_PLAYABLE_PRESENTATION_AND_FIREWALL_SAFE_BUILD.md` | Completed Goal 014 task: Unity playable Alpha presentation and firewall-safe build discipline, with visible map/player/NPC/item/status presentation, automated movement/interaction evidence, release-style BuildOptions.None build entrypoint checks and final stop at `unity_playable_presentation_firewall_safe_build_verification`. S121B repaired root artifact regeneration for the same Goal 014 gate; the user confirmed the gate passed before Goal 015. |
| `docs/GOAL_015_UNITY_GENERATED_SCENE_CONTENT_PROJECTION.md` | Completed Goal 015 task: Unity generated scene content projection, with Application-layer generated scene projection artifacts, Unity Alpha map/player/NPC/item/quest-event/command-status presentation derived from selected package/config/asset evidence, generated-node movement/interaction logs, invalid/fake/leak rejection, product smoke route `unity-generated-scene-projection` and final stop at `unity_generated_scene_content_projection_verification`. The user confirmed the gate passed before Goal 016. |
| `docs/GOAL_016_UNITY_GENERATED_RUNTIME_STATE_LOOP.md` | Completed Goal 016 task: Unity generated runtime state loop, with Application-layer runtime state loop artifacts, Unity Alpha quest/dialogue/item/inventory/event/focus/status before-after state changes derived from generated command hints, command/state transition logs, invalid/fake/leak rejection, product smoke route `unity-runtime-state-loop` and final stop at `unity_generated_runtime_state_loop_verification`. The user confirmed the gate passed before Goal 017. |
| `docs/GOAL_017_UNITY_GENERATED_QUEST_COMPLETION_LOOP.md` | Completed Goal 017 task: Unity generated quest completion loop, with Application-layer quest plan/state/report artifacts, Unity Alpha ordered quest phases, objective checklist, generated dialogue/item/event command correlation, completion/reward proof, invalid/fake/leak rejection, product smoke route `unity-quest-completion-loop` and final stop at `unity_generated_quest_completion_loop_verification`. The user confirmed the gate passed before Goal 018. |
| `docs/GOAL_018_UNITY_MULTI_VARIANT_PLAYABLE_SCENARIO.md` | Completed Goal 018 task: Unity multi-variant playable scenario, with Application-layer multi-variant acceptance artifacts for `frontier_survival`, `gothic_mystery` and `trade_caravan`, per-variant Unity Alpha quest completion proof, cross-variant distinctness validation, invalid/fake/leak rejection, product smoke route `unity-multi-variant-playable-scenario` and final stop at `unity_generated_multi_variant_playable_scenario_verification`. The user confirmed the gate passed before Goal 019. |
| `docs/GOAL_019_UNITY_ALPHA_READABLE_PRESENTATION.md` | Completed Goal 019 task: Unity Alpha readable presentation, with Application-layer readable presentation model/report artifacts, Unity Alpha IMGUI scenario/variant/quest/objective/target/inventory/reward/event-log/control panels, readable player proof lines, invalid/fake/leak rejection, product smoke route `unity-alpha-readable-presentation` and final stop at `unity_alpha_readable_presentation_verification`. The user confirmed the gate passed before Goal 020. |
| `docs/GOAL_020_MINIMUM_PLAYABLE_GENERATED_GAME_GATE.md` | Goal 020 task: Minimum playable generated game gate, with Application-layer minimum playable acceptance artifacts, a runnable review package under `.llmgc/procedural/minimum-playable-generated-game/review-package/`, README/manual/automated scripts, generated scenario summary, automated launch and quest completion proof, invalid/fake/leak rejection, product smoke route `minimum-playable-generated-game` and final stop at `minimum_playable_generated_game_verification`. |
| `docs/MANUAL_CONFIGURABLE_MICROGAME_VERIFICATION.md` | Manual user verification checklist for configurable generated microgames after S042. |
| `docs/EXTENSION_RULE_PACK_CONTRACT_V1.md` | Accepted Goal 003 declaration-only extension rule pack contract. |
| `docs/MANUAL_EXTENSION_SPINE_VERIFICATION.md` | Manual user verification checklist for the Goal 003 extension spine after automated acceptance. |
| `docs/MANUAL_VISIBLE_GENERATED_PLAYABLE_PREVIEW_CHECK.md` | Manual user verification checklist for the generated playable preview. |
| `docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md` | LLM minimization policy and deterministic combiner rules. |
| `docs/AGENT_CONTEXT_BUDGET_POLICY.md` | Compact read-first policy for current Codex slices and goal handoffs. |
| `docs/ARCHITECTURE_STRATEGY_AND_BOUNDARIES.md` | Post-Goal-003 architecture boundaries: C# primitives, data/rule packs, Lua-like declarations, LLM role and Runtime Preview limits. |
| `docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md` | Semantic-pack layering, RAG authoring role and compiled semantic catalog direction. |
| `docs/SEMANTIC_PACK_CONTRACT_V1.md` | Accepted Goal 005 layered semantic-pack contract, precedence, candidate quarantine and relation allow-list. |
| `docs/OPEN_DESIGN_QUESTIONS.md` | Open strategic questions to answer through bounded experiments instead of ad hoc slice drift. |
| `docs/FULL_GAME_GENERATION_MASTER_PLAN.md` | Full game generation meaning, ownership boundaries and long-term target architecture. |
| `docs/GAME_GENERATION_CAPABILITY_MATRIX.md` | Capability domains, priorities and acceptance criteria. |
| `docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md` | C# / LLM / Lua ownership and forbidden boundary crossings. |
| `docs/ROADMAP_TO_FULL_GENERATOR.md` | Historical/long-route roadmap; current state and strategy reset override outdated next-step recommendations. |
| `docs/CODEX_EXECUTION_DOCTRINE.md` | General Codex task boundaries; strategy reset is stricter for the next phase. |
| `docs/GAME_FORM_FACTORS_AND_PRESENTATION_MODES.md` | Presentation modes and form-factor choices. |
| `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md` | World, actor, inventory, combat, progression, pathfinding and NPC behavior ids. |
| `docs/CHARACTER_CARD_AND_ACTOR_MODEL_CONTRACTS.md` | Character/actor contract planning. |
| `docs/WORLD_TOPOLOGY_AND_CHUNKING_CONTRACTS.md` | Finite maps, regions, first-person grids, seamless/infinite chunks and runtime chunk deltas. |
| `docs/INTERACTION_COMBAT_PROGRESSION_VARIANTS.md` | Interaction, combat, progression, inventory and equipment contract families. |

## Current Next Work

Recommended next work:

```text
minimum_playable_generated_game_verification
```

Allowed next sequence:

1. Review `minimum_playable_generated_game_verification` evidence from the produced manifest JSON, report, review package folder, README, manual/automated scripts, generated scenario summary, automated launch/play-loop logs, manual checklist and invalid/fake/leak matrix before marking the gate passed.
2. Goal 020 regenerates compact repo-local root review artifacts under `.llmgc/procedural/minimum-playable-generated-game/` through the existing `minimum-playable-generated-game` product smoke route.
3. Generated Unity build outputs under `.llmgc/procedural/**/build/`, `.llmgc/procedural/**/logs/`, `.llmgc/procedural/**/unity-work/` and Unity-generated project folders are ignored by `.gitignore`; compact `.json` / `.md` report and verification artifacts remain eligible for review when a task requires them.

Kill criterion:

```text
If no generated playable or simulatable loop exists after the next three large product slices,
stop and reassess architecture before spending more limit.
```

## Do Not Use As Current Authority

The following are historical or one-time packaging files. They may remain in git history or an archive, but should not be read for the next Codex task:

- root `README_APPLY_AGENT_TASK_PACK_*.md`
- root `README_APPLY_PRODUCT_SLICE_*.md`
- root `README_APPLY_PACK_008.md`
- root `README_APPLY_CAPABILITY_COMPOSER_V2_PACK.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_CODEX_PROMPT.md` for slices before 029
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_KILO_PROMPT.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_ARCHIVE_MANIFEST.md`
- `docs/agent-tasks/NEXT_PRODUCT_SLICE/*_README_APPLY_PRODUCT_SLICE.md`
- old `docs/PRODUCT_SLICE_00*.md` files when selecting new work

## Project Map

| Project / folder | Responsibility | Read when |
|---|---|---|
| `src/LLMGameCreator.Domain/` | Data contracts: game definitions, assets, scripting definitions, validation primitives. | Any model, validator, runtime, package, Lua or asset task. |
| `src/LLMGameCreator.GamePackage/` | Root `GamePackageDefinition` and package path conventions. | Package format, loading/saving, validators, runtime startup. |
| `src/LLMGameCreator.Runtime.Abstractions/` | Runtime command/state/event interfaces. | Runtime, simulator, preview and generated loop work. |
| `src/LLMGameCreator.Runtime/` | Headless runtime implementation. | Movement, interaction, command execution, state updates, simulator smoke. |
| `src/LLMGameCreator.Scripting/` | Script engine abstraction and prototype Lua sandbox. | Lua planning or declaration mapping only when explicitly selected. |
| `src/LLMGameCreator.Generation/` | LLM authoring/generation models. Editor-side only. | Context packs, generation jobs and LLM provider tasks. |
| `src/LLMGameCreator.AssetPipeline/` | Asset generation provider abstractions and jobs. Editor-side only. | Asset request/provider workflow tasks. |
| `src/LLMGameCreator.Application/` | Use-cases/services, validation and editor workflows. | Application services, validators and procedural generation kernel work. |
| `src/LLMGameCreator.Infrastructure/` | JSON storage, settings persistence, file logging. | Storage/serialization changes. |
| `src/LLMGameCreator.WinForms/` | Editor shell and pages. | UI page work only. Do not add UI for Slice 029 unless explicitly required. |
| `tests/LLMGameCreator.Tests/` | Smoke/contract/regression tests. | Any behavior/validator/runtime change. |
| `generator-library/` | Lua generator/capability library metadata/assets. | Generator library tasks. Lua execution remains locked unless explicitly selected. |
| `samples/minimal-map-game/` | Minimal GamePackage sample. | Package, validation, runtime and smoke examples. |
| `templates/` | Lua stdlib and blueprint templates. | Lua authoring/sandbox/API tasks. |
| `docs/` | Architecture and task guidance. | Read only relevant docs. |

## High-Value Local Patterns

### Validator Pattern

Primary files:

- `src/LLMGameCreator.Application/Validation/GamePackageValidator.cs`
- `src/LLMGameCreator.Domain/Validation/ValidationIssue.cs`
- `tests/LLMGameCreator.Tests/SmokeTests.cs`

Style:

- keep validation deterministic and side-effect free;
- prefer stable machine-readable issue codes;
- add focused tests for new contracts;
- do not execute Lua, call LLM, call providers or mutate package state from validators.

### Runtime Command Pattern

Primary files:

- `src/LLMGameCreator.Runtime.Abstractions/`
- `src/LLMGameCreator.Runtime/`
- runtime simulator/preview pages and runtime tests when relevant.

Style:

- frontend creates commands;
- runtime accepts package/state/command;
- runtime returns updated state/events;
- rendering does not mutate state;
- runtime does not call LLM, providers, WinForms or external generators.

### Procedural Generation Task Pattern

From Slice 029:

- prefer an Application-layer procedural generation area;
- keep Domain changes minimal;
- produce deterministic `.llmgc/procedural/generated-game-plan.json` and `.md` artifacts when a project folder is supplied;
- no timestamps, absolute paths, machine names or nondeterministic ordering;
- same seed must produce byte-stable output;
- different seeds must produce visible variation while preserving structure.

## Red Flags

Stop and ask/plan first if a change would:

- touch more than 8-10 files without a clear reason;
- add Unity, a Lua engine, a real provider call, a real LLM call or media generation;
- change `package.json` / GamePackage schema;
- change public runtime command/state contracts;
- add UI polish unrelated to the generated playable/simulatable loop;
- expand old archive/manual-import/semantic UI workflows;
- introduce broad refactors unrelated to the task acceptance criteria.
