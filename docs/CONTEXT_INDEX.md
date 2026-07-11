# CONTEXT_INDEX.md

Purpose: reduce repeated orientation cost for Codex/LLM agents.

Read this file after `AGENTS.md` when a task touches code. This file is a routing index, not a replacement for detailed docs. If this file conflicts with a more specific doc, the specific doc wins.

## Generator Task Routing

For any generator/Codex task:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/PRODUCT_LINE_CORE_STRATEGY.md`
5. `docs/NARROW_ALPHA_EXPANSION_POLICY.md`
6. `docs/AUTOMATED_VALIDATION_TIERS.md`
7. `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
8. `docs/ROADMAP_TO_FULL_GENERATOR.md`
9. only then task-specific docs

For Goal 029 specifically, read:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/GOAL_029_MODULAR_GENERATOR_KERNEL_PARALLEL_READINESS.md`
5. `docs/MODULAR_CONTRACT_GOAL_POLICY.md`
6. `.devflow/scripts/run-product-smoke.ps1`
7. package assembly services/tests from Goals 025-028

## Active Strategy Reset

The active post-S028 direction is:

```text
Before expanding the platform, prove the generated game kernel.
```

Infrastructure-only work is frozen unless explicitly requested by the user or directly required to unblock a generated playable/simulatable loop.

Old apply READMEs, old product-slice prompts, old task-pack prompts and historical archive manifests are not current planning authority.

## Product-Line Strategy Routing

Read the product-line strategy docs before broad generation work, candidate
pipeline work, WinForms operator pipeline work, Runtime/player pivot work,
Codex task shaping and roadmap/rebaseline decisions:

Read-first categories: broad generation work; candidate pipeline work; WinForms
operator pipeline work; Runtime/player pivot work; Codex task shaping;
roadmap/rebaseline decisions.

1. `docs/PRODUCT_LINE_CORE_STRATEGY.md`
2. `docs/NARROW_ALPHA_EXPANSION_POLICY.md`
3. `docs/AUTOMATED_VALIDATION_TIERS.md`

These docs define the FeatureModule / RuntimePrimitive / SemanticPack /
VisualPartPack / WorldSourceAdapter / PlayerAdapter seams. Next broad product
work must reduce the projection-only gap and route selected candidates toward
canonical runtime playthrough, save/load/replay proof and player-adapter
consumption of canonical transcript/state summaries.

Goal134 provides the first selected-candidate canonical runtime matrix over the
Goal131 handoff. Goal135 adds player-loop readiness over that canonical runtime
transcript/state summary with a PlayerAdapter contract, deterministic step plan
and Unity/player readiness smoke. Goal136 executes the selected candidate
through a Runtime-owned player command loop and records Unity/player snapshot
consumption as proof only. Goal137 turns those Runtime-owned snapshots into
Unity/player playback frames and a playback smoke without making Unity gameplay
truth. Goal138 adds the accepted runtime-backed stepper/HUD model over that
playback. Goal139 adds runtime-backed interactive controls over the Goal138
model/script. Goal140 records Goal139 human acceptance, polishes the controls
UX and adds a bounded Unity editor noise guard. Goal141 records Goal140 human
acceptance, and Goal141A corrects its bridge so six Unity/PlayerAdapter control
intents produce correlated responses with four Runtime-routed executions and
two presentation-only controls. Goal142 materializes four runtime-significant
variants and selects `minimal-map-game-exploration-resource-focus`; Goal143
carries only that selection through package integrity, deterministic Runtime
rerun, ordered PlayerAdapter frames and a read-only Unity consumer smoke.
Goal144 records Goal143 human acceptance and turns the selected handoff into a
Runtime-owned interactive action session with journal checkpoint reload and
full deterministic replay. Goal144A binds every advertised Runtime action to
the exact executed canonical step/target and freezes checkpoint replay evidence
at 8 actions before the returned session continues to the 13-action final state.
Goal145 records Goal144 human acceptance, discovers every Goal142 candidate,
runs the same Runtime session/replay kernel across all four, proves semantic
focus differences and exposes operator selection plus a read-only Unity matrix.
Future work should build on this canonical
Runtime/PlayerAdapter/Unity evidence instead of adding projection-only wrappers.

## Full Generator Source-Of-Truth Docs

Read these before broad generation, capability, prompt, Lua integration, artifact-contract, roadmap or Codex-task-shaping work:

| Document | Use when |
|---|---|
| `docs/CURRENT_GENERATOR_STATE.md` | Starting any generator/Codex task; checking the active phase, recommended next action and blocked milestones. |
| `docs/CURRENT_GENERATOR_STATE.json` | Machine-readable mirror of current state for tooling/tests. |
| `docs/PRODUCT_LINE_CORE_STRATEGY.md` | Read-first for broad generation, candidate pipeline, WinForms operator pipeline, Runtime/player pivot, Codex task shaping and roadmap/rebaseline decisions; defines FeatureModule / RuntimePrimitive / SemanticPack / VisualPartPack / WorldSourceAdapter / PlayerAdapter seams. |
| `docs/NARROW_ALPHA_EXPANSION_POLICY.md` | Read-first for alpha scope choices and Runtime/player pivot work; narrow alpha must be expansion-safe, not a hardcoded demo. |
| `docs/AUTOMATED_VALIDATION_TIERS.md` | Read-first for validation planning; normal goals should strengthen package, canonical runtime, candidate matrix and player smoke tiers before rare manual gates. |
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
| `docs/GOAL_020_MINIMUM_PLAYABLE_GENERATED_GAME_GATE.md` | Completed Goal 020 task: Minimum playable generated game gate, with Application-layer minimum playable acceptance artifacts, a runnable review package under `.llmgc/procedural/minimum-playable-generated-game/review-package/`, README/manual/automated scripts, generated scenario summary, automated launch and quest completion proof, invalid/fake/leak rejection, product smoke route `minimum-playable-generated-game` and final stop at `minimum_playable_generated_game_verification`. The user confirmed the gate passed before Goal 021. |
| `docs/GOAL_021_GENERATED_GAME_PROFILE_CONTRACT_REFRESH.md` | Completed Goal 021 task: Generated game profile contract refresh, with `game_profile_v1`, sample profiles under `samples/game-profiles/`, Application-layer profile contract artifacts under `.llmgc/procedural/generated-game-profile-contract/`, physical accepted Goal 020 compact evidence validation, exact profile-to-pipeline mapping for Goal 010-020 stages, future-required capability separation, invalid/fake/leak rejection, product smoke route `generated-game-profile-contract` and final stop at `generated_game_profile_contract_verification`. The user confirmed the gate passed before Goal 022. |
| `docs/GOAL_022_DEVELOPMENT_COMPLEXITY_STABILIZATION_AND_ARTIFACT_SCOPE_GOVERNANCE.md` | Completed Goal 022 task: Development complexity stabilization and artifact scope governance, with `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md`, `.devflow/artifact-scope/artifact-scope-policy.json`, `.devflow/scripts/check-artifact-scope.ps1`, check-all artifact isolation, tracked generated artifact inventory, invalid/fake/leak scope matrix, product smoke route `development-complexity-stabilization` and final stop at `development_complexity_stabilization_verification`. The user confirmed the gate passed before Goal 023. |
| `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md` | Goal 022 policy for artifact mutability classes, check-all artifact isolation, product-smoke root write declarations and final scope guard verification. |
| `docs/GOAL_023_CAPABILITY_BUNDLE_SELECTION_TO_PIPELINE_INPUTS.md` | Goal 023 task: Capability bundle selection to pipeline inputs, with `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md`, Application-layer profile-to-selector-request mapping under `Design/CapabilityBundlePipelineInputs`, generator pipeline input artifacts, explicit blocked/future-required gaps, invalid/fake/leak matrix, product smoke route `capability-bundle-pipeline-inputs` and final stop at `capability_bundle_pipeline_inputs_verification`. |
| `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md` | Goal 023 planning artifact contract for deterministic profile requests, capability selection evidence, generator pipeline input records, gap reports and final manual review. |
| `docs/GOAL_024_RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT.md` | Goal 024 task: Rich package assembly coverage audit, with `docs/RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT_V1.md`, Application-layer coverage audit under `Design/RichPackageAssemblyCoverageAudit`, coverage matrix, gap report, next-slice plan, invalid/fake/leak matrix, product smoke route `rich-package-assembly-coverage-audit` and final stop at `rich_package_assembly_coverage_audit_verification`. |
| `docs/RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT_V1.md` | Goal 024 audit contract for evidence-backed package assembly coverage classification and next package-expansion planning without starting Goal 025/S199. |
| `docs/MODULAR_CONTRACT_GOAL_POLICY.md` | Active process policy: Contract / Module / Integration / Proof are internal phases of bounded composite goals by default; product vertical gates are rare and intentional. |
| `docs/GOAL_025_PACKAGE_ASSEMBLY_EXPANSION_1_WORLD_AND_ENTITIES.md` | Goal 025 task: package assembly expansion 1 for world/entities through existing GamePackage schema and Application seams, with real and synthetic consumer proof, invalid/fake/leak matrix, product smoke route `package-assembly-world-entities` and final stop at `package_assembly_world_entities_expansion_verification`. |
| `docs/PACKAGE_ASSEMBLY_WORLD_ENTITIES_CONTRACT_V1.md` | Goal 025 mapping contract for accepted Goal 023/024 planning inputs, `scene_pack_v1`, `region_pack_v1`, `entity_pack_v1`, `npc_pack_v1`, existing map/entity/generated-content targets and non-goals. |
| `docs/GOAL_026_PACKAGE_ASSEMBLY_EXPANSION_2_DIALOGUE_AND_QUESTS.md` | Goal 026 task: package assembly expansion 2 for dialogue/quests through existing GamePackage schema and Application seams, with real and synthetic consumer proof, invalid/fake/leak matrix, product smoke route `package-assembly-dialogue-quests` and final stop at `package_assembly_dialogue_quests_expansion_verification`. |
| `docs/PACKAGE_ASSEMBLY_DIALOGUE_QUESTS_CONTRACT_V1.md` | Goal 026 mapping contract for accepted Goal 023/024/025 inputs, `dialogue_pack_v1`, `quest_pack_v1`, existing quest/dialogue/generated-content targets and non-goals. |
| `docs/GOAL_027_PACKAGE_ASSEMBLY_EXPANSION_3_ITEMS_ECONOMY_CRAFTING.md` | Goal 027 task: package assembly expansion 3 for items/economy/crafting through existing GamePackage schema and Application seams, with real and synthetic consumer proof, invalid/fake/leak matrix, product smoke route `package-assembly-items-economy-crafting` and final stop at `package_assembly_items_economy_crafting_expansion_verification`. |
| `docs/PACKAGE_ASSEMBLY_ITEMS_ECONOMY_CRAFTING_CONTRACT_V1.md` | Goal 027 mapping contract for accepted Goal 023/024/025/026 inputs, `item_pack_v1`, `resource_pack_v1`, `recipe_pack_v1`, `loot_pack_v1`, `transaction_pack_v1`, `inventory_pack_v1`, `equipment_pack_v1`, existing item/economy/crafting targets and non-goals. |
| `docs/GOAL_028_PACKAGE_ASSEMBLY_EXPANSION_4_COMBAT_PROGRESSION.md` | Goal 028 task: package assembly expansion 4 for combat/progression through existing GamePackage schema and Application seams, with real and synthetic consumer proof, invalid/fake/leak matrix, product smoke route `package-assembly-combat-progression` and final stop at `package_assembly_combat_progression_expansion_verification`. |
| `docs/PACKAGE_ASSEMBLY_COMBAT_PROGRESSION_CONTRACT_V1.md` | Goal 028 mapping contract for accepted Goal 023/024/025/026/027 inputs, `stat_pack_v1`, `ability_pack_v1`, `status_pack_v1`, `progression_pack_v1`, `encounter_pack_v1`, `combat_pack_v1`, existing combat/progression targets and non-goals. |
| `docs/GOAL_029_MODULAR_GENERATOR_KERNEL_PARALLEL_READINESS.md` | Goal 029 task: modular generator kernel and parallel development readiness with module manifests, product-smoke scenario manifests, static registry/compatibility seam, absence behavior, verification tiers, manifest-driven product smoke route and final stop at `modular_generator_kernel_parallel_readiness_verification`. |
| `docs/GOAL_030_SEMANTIC_ARTIFACT_CONTRACT_REGISTRY_SPEC.md` | Goal 030 task/spec: semantic artifact contract registry, semantic pack compatibility planner, semantic expansion planning seam, compact evidence under `.llmgc/procedural/goal-030-semantic-artifact-contract-registry/` and final stop at `semantic_artifact_contract_registry_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_030_SEMANTIC_ARTIFACT_REGISTRY.md` | Goal 030 scouting: BCL-only registry/planner, no external graph/RDF/WFC/dialogue/ECS dependencies. |
| `docs/GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT_SPEC.md` | Goal 031 task/spec: semantic pack composition blueprint, deterministic semantic fact/relation merge, cross-artifact blueprint plans, compact evidence under `.llmgc/procedural/goal-031-semantic-pack-composition-blueprint/` and final stop at `semantic_pack_composition_blueprint_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT.md` | Goal 031 scouting: BCL-only semantic pack composition, no RDF/graph/LLM/dialogue/runtime/ECS dependency adoption. |
| `docs/GOAL_032_DYNAMIC_SEMANTIC_FEATURE_SYSTEM_SPEC.md` | Goal 032 task/spec: dynamic semantic feature system and influence rule kernel, deterministic applicability/inheritance/resolution, authoring schema records, compact evidence under `.llmgc/procedural/goal-032-dynamic-semantic-feature-system/` and final stop at `dynamic_semantic_feature_system_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_032_DYNAMIC_SEMANTIC_FEATURE_SYSTEM.md` | Goal 032 scouting: BCL-only feature/influence kernel, no external rules/expression engine dependency adoption. |
| `docs/GOAL_033_SEMANTIC_AUTHORING_INTENT_RESOLVER_SPEC.md` | Goal 033 task/spec: semantic authoring workspace, lore intake skeleton, manual-vs-auto provenance matrix, feature-driven content intent resolver, compact evidence under `.llmgc/procedural/goal-033-semantic-authoring-intent-resolver/` and final stop at `semantic_authoring_intent_resolver_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_033_SEMANTIC_AUTHORING_INTENT_RESOLVER.md` | Goal 033 scouting: BCL-only semantic authoring and intent planning, no JSON Schema/validation/template/UI dependency adoption. |
| `docs/GOAL_034_STRICT_LLM_DRAFT_ARTIFACT_LOOP_SPEC.md` | Goal 034 task/spec: strict draft requests, quarantined candidate envelopes, deterministic validation, repair request records, promotion decisions, compact evidence under `.llmgc/procedural/goal-034-strict-llm-draft-artifact-loop/` and final stop at `strict_llm_draft_artifact_loop_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_034_STRICT_LLM_DRAFT_ARTIFACT_LOOP.md` | Goal 034 scouting: BCL-only strict draft artifact loop, no external schema/validation/template/provider dependency adoption. |
| `docs/GOAL_035_LUA_MODULE_MANIFEST_REGISTRY_SPEC.md` | Goal 035 task/spec: BCL-only Lua module manifest registry, host API surface policy, dependency planning, scenario selection, compact evidence under `.llmgc/procedural/goal-035-lua-module-manifest-registry/` and final stop at `lua_module_manifest_registry_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_035_LUA_MODULE_MANIFEST_REGISTRY.md` | Goal 035 scouting: manifest-only Lua governance, no interpreter/runtime/parser/provider dependency adoption. |
| `docs/GOAL_036_LUA_SANDBOX_EXECUTION_GATE_SPEC.md` | Goal 036 task/spec: BCL-only Lua sandbox execution gate, host binding matrix, dry-run traces, deny-first decisions, repair plans, compact evidence under `.llmgc/procedural/goal-036-lua-sandbox-execution-gate/` and final stop at `lua_sandbox_execution_gate_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_036_LUA_SANDBOX_EXECUTION_GATE.md` | Goal 036 scouting: execution gate only, no Lua interpreter/runtime/parser/source/dependency adoption. |
| `docs/GOAL_037_HYBRID_LLM_DRAFT_LUA_DETERMINISTIC_EXPANSION_SPEC.md` | Goal 037 task/spec: hybrid strict LLM draft plus bounded Lua deterministic expansion through an Application-layer executor adapter, structured IR output, C# validation, compact evidence under `.llmgc/procedural/goal-037-hybrid-llm-draft-lua-deterministic-expansion/` and final stop at `hybrid_llm_draft_lua_deterministic_expansion_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_037_HYBRID_LLM_DRAFT_LUA_DETERMINISTIC_EXPANSION.md` | Goal 037 scouting: one bounded LuaCSharp adapter may be adopted only if restore/build and sandbox isolation are proven; no arbitrary Lua, provider/LLM/RAG, Runtime/UI/Unity/GamePackage or generator-library changes. |
| `docs/GOAL_038_WORLD_SCALE_REGION_MAP_FOUNDATION_SPEC.md` | Goal 038 task/spec: world-scale region graph, reachability, finite map pack and chunk-config foundation, compact evidence under `.llmgc/procedural/goal-038-world-scale-region-map-foundation/` and final stop at `world_scale_region_map_foundation_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_038_WORLD_SCALE_REGION_MAP_FOUNDATION.md` | Goal 038 scouting: BCL-only graph/reachability/map/chunk prelude, no QuikGraph/GoRogue/RoyT.AStar dependency adoption. |
| `docs/GOAL_039_RUNTIME_CHUNK_DELTA_TRAVERSAL_SMOKE_SPEC.md` | Goal 039 task/spec: runtime chunk delta traversal smoke consuming Goal 038 graph/map/chunk facts, runtime-owned `GameRuntimeState` chunk deltas, real serializer/snapshot save-load proof, replay determinism, compact evidence under `.llmgc/procedural/goal-039-runtime-chunk-delta-traversal-smoke/` and final stop at `runtime_chunk_delta_traversal_smoke_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_039_RUNTIME_CHUNK_DELTA_TRAVERSAL_SMOKE.md` | Goal 039 scouting: BCL/System.Text.Json plus existing runtime serializer/snapshot mechanisms, no graph/pathfinding/serialization dependency adoption. |
| `docs/GOAL_040_CHUNKED_RUNTIME_PREVIEW_EXPORT_MULTIFAMILY_SMOKE_SPEC.md` | Goal 040 task/spec: chunked runtime preview/export multi-family smoke consuming Goal 039 runtime chunk delta traversal artifacts into preview/export payloads, multi-family regression, bounded infinite-window proof, compact evidence under `.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/` and final stop at `chunked_runtime_preview_export_multifamily_smoke_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_040_CHUNKED_RUNTIME_PREVIEW_EXPORT_MULTIFAMILY_SMOKE.md` | Goal 040 scouting: BCL-only consumer/proof slice over Goal 038/039 evidence; no graph/pathfinding/GIS/serialization dependency adoption and no streaming/runtime/Unity refactor. |
| `docs/GOAL_043_MULTI_FAMILY_GENERATED_TEMPLATE_VERTICAL_SLICE_SPEC.md` | Goal 043 task/spec: multi-family generated template vertical slice consuming Goal 034-040 evidence into shared lifecycle plans and deterministic simulatable loops for map/panel RPG, survival sandbox and first-person grid dungeon families, compact evidence under `.llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/` and final stop at `multi_family_generated_template_vertical_slice_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_043_MULTI_FAMILY_GENERATED_TEMPLATE_VERTICAL_SLICE.md` | Goal 043 scouting: BCL-only generated template planning and simulatable loop proof; no Runtime/UI/Unity/GamePackage/provider/LLM/RAG/media dependency adoption. |
| `docs/GOAL_047_FULL_GENERATOR_WITHOUT_MEDIA_DRY_RUN_SPEC.md` | Goal 047 task/spec: full generator without media dry-run consuming Goal 034-040 and Goal 043 evidence into source manifest, review/promotion ledger, repair diagnostics, three family dry-runs, runtime preview validation, export profile selection, package compatibility proof and one-click dry-run artifacts, compact evidence under `.llmgc/procedural/goal-047-full-generator-without-media-dry-run/` and final stop at `full_generator_without_media_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_047_FULL_GENERATOR_WITHOUT_MEDIA_DRY_RUN.md` | Goal 047 scouting: BCL-only dry-run/review/validation proof; no Runtime/UI/Unity/GamePackage/provider/LLM/RAG/media dependency adoption. |
| `docs/GOAL_053_MEDIA_ASSET_CAMPAIGN_ORCHESTRATION_SPEC.md` | Goal 053 task/spec: media asset campaign orchestration and binding dry run consuming Goal 047 plus Goal 043/040 evidence into a media slot catalog, request queue, license/provenance ledger, candidate quarantine, review/promotion ledger, deterministic fixture media bindings, preview/export media payload proof and compact evidence under `.llmgc/procedural/goal-053-media-asset-campaign-orchestration/`, with final stop at `media_asset_campaign_orchestration_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_053_MEDIA_ASSET_CAMPAIGN_ORCHESTRATION.md` | Goal 053 scouting: BCL-only media campaign governance and fixture binding proof; no real media/provider generation, network/import, Runtime/UI/Unity/GamePackage/provider/LLM/RAG/Lua/generator-library dependency adoption. |
| `docs/GOAL_054_MEDIA_MATERIALIZATION_UNITY_REVIEW_SPEC.md` | Goal 054 task/spec: media materialization review package consuming Goal 053/047 evidence into a deterministic materialization queue, physical PNG/WAV/bundle media inventory, binding validation, media-bound review package manifest, preview/export payload proof and compact evidence under `.llmgc/procedural/goal-054-media-materialization-review-package/`, with final stop at `media_materialization_review_package_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_054_MEDIA_MATERIALIZATION_UNITY_REVIEW.md` | Goal 054 scouting: BCL-only deterministic physical media materialization and media-bound review/export proof; no provider/network/LLM/RAG/Lua execution and no Runtime/UI/Unity/GamePackage/generator-library dependency adoption. |
| `docs/GOAL_055_MEDIA_BOUND_PLAYABLE_REVIEW_PACKAGE_SPEC.md` | Goal 055 task/spec: media-bound playable review package smoke consuming Goal 047/053/054 evidence into staged physical media package files, Unity-compatible media load contract/proof records, preview/export payload proof and compact evidence under `.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/`, with final stop at `media_bound_playable_review_package_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_055_MEDIA_BOUND_PLAYABLE_REVIEW_PACKAGE.md` | Goal 055 scouting: BCL-only package/proof seam with optional narrow Unity Alpha loader only if needed; no new dependencies, no provider/network/LLM/RAG/Lua execution and no Runtime/UI/GamePackage/generator-library dependency adoption. |
| `docs/GOAL_056_UNITY_ALPHA_MEDIA_BOUND_PLAYABLE_PACKAGE_SPEC.md` | Goal 056 task/spec: Unity Alpha media-bound playable package consuming Goal 055 staged media into a StreamingAssets payload and proving real repo-local Unity Alpha player media markers, with final stop at `unity_alpha_media_bound_playable_package_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_056_UNITY_ALPHA_MEDIA_BOUND_PLAYABLE_PACKAGE.md` | Goal 056 scouting: use the existing Unity Alpha project and deterministic PNG/WAV/bundle fixture paths only; no new Unity packages, provider/network/LLM/RAG/Lua execution or Runtime/UI/GamePackage/generator-library dependency adoption. |
| `docs/GOAL_057_UNITY_ALPHA_MULTIFAMILY_PLAYABLE_LOOP_SPEC.md` | Goal 057 task/spec: Unity Alpha multi-family playable loop consuming accepted Goal 056 media-bound package plus Goal 043/047 family loop evidence into real repo-local Unity player family-mode markers, with final stop at `unity_alpha_multifamily_playable_loop_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_057_UNITY_ALPHA_MULTIFAMILY_PLAYABLE_LOOP.md` | Goal 057 scouting: reuse existing Unity Alpha StreamingAssets/build/player diagnostic route; no new Unity packages, provider/network/LLM/RAG/Lua execution, external media import or broad Runtime/GamePackage mutation. |
| `docs/GOAL_058_FULL_MEDIA_BOUND_GENERATOR_CAMPAIGN_SPEC.md` | Goal 058 task/spec: full media-bound generator campaign consuming Goal 034-057 proof chain into one campaign runner, review package staging and Unity Alpha campaign markers, with final stop at `full_media_bound_generator_campaign_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_058_FULL_MEDIA_BOUND_GENERATOR_CAMPAIGN.md` | Goal 058 scouting: BCL-only orchestration/proof goal; no external dependencies, provider/network/LLM/RAG/media generation/import, broad Unity/UI/Runtime/GamePackage mutation or new package infrastructure. |
| `docs/GOAL_059_FULL_GENERATOR_VARIABILITY_MATRIX_SPEC.md` | Goal 059 task/spec: full generator variability regression matrix consuming accepted Goal 058 full media-bound campaign evidence into a 3 family x 3 seed replayability/variance proof with Unity Alpha matrix markers, with final stop at `full_generator_variability_regression_matrix_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_059_FULL_GENERATOR_VARIABILITY_MATRIX.md` | Goal 059 scouting: BCL-only matrix/replay/variance proof over Goal 058 evidence; no external dependencies, provider/network/LLM/RAG/media generation/import, broad Unity/UI/Runtime/GamePackage mutation or generator-library changes. |
| `docs/GOAL_060_FULL_CAMPAIGN_GAMEPACKAGE_MATERIALIZATION_SPEC.md` | Goal 060 task/spec: full campaign GamePackage materialization matrix consuming Goal 059 variability rows into real validator-clean packages, runtime consumption proof and Unity Alpha package markers, with final stop at `full_campaign_gamepackage_materialization_matrix_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_060_FULL_CAMPAIGN_GAMEPACKAGE_MATERIALIZATION.md` | Goal 060 scouting: BCL-only package materialization/runtime/Unity proof; no external dependencies, no public GamePackage schema change, no provider/network/LLM/RAG/media generation/import or arbitrary Lua execution. |
| `docs/GOAL_061_FULL_CAMPAIGN_PLAYABLE_REVIEW_PACKAGE_RC_SPEC.md` | Goal 061 task/spec: full campaign playable review package RC consuming Goal 060 materialized packages into package-row review scripts, media/save-load audits and Unity Alpha review-package RC markers, with final stop at `full_campaign_playable_review_package_rc_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_061_FULL_CAMPAIGN_PLAYABLE_REVIEW_PACKAGE_RC.md` | Goal 061 scouting: BCL-only review-package RC proof over Goal 060 evidence; no external dependencies, no provider/network/LLM/RAG/media generation/import, no public GamePackage schema change and no broad Unity/UI/Runtime mutation. |
| `docs/GOAL_062_CONSTRAINED_SPATIAL_DETAIL_GENERATION_SPEC.md` | Goal 062 task/spec: constrained spatial detail generation consuming the Goal 061 playable review package RC plus Goal 060/059 matrix evidence into 9 validated spatial-detail rows, reachability/variance proof and Unity Alpha markers, with final stop at `constrained_spatial_detail_generation_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_062_CONSTRAINED_SPATIAL_DETAIL_GENERATION.md` | Goal 062 scouting: use mxgmn/WFC/MarkovJunior/TextureSynthesis as conceptual references only; implement BCL-only in-house palette, rewrite, constraint, reachability and proof records without external dependencies or assets. |
| `docs/GOAL_063_GAMEPLAY_CONSEQUENCE_DEPTH_MATRIX_SPEC.md` | Goal 063 task/spec: gameplay consequence depth matrix consuming Goal 060/061/062 evidence into 9 family/seed state-changing runtime gameplay rows, save/load/replay audit and Unity Alpha gameplay consequence markers, with final stop at `gameplay_consequence_depth_matrix_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_063_GAMEPLAY_CONSEQUENCE_DEPTH_MATRIX.md` | Goal 063 scouting: BCL-only gameplay consequence proof over existing package/runtime/Unity seams; no external dependencies, provider/network/LLM/RAG/media generation/import, public GamePackage schema change, arbitrary Lua execution or broad Runtime/UI mutation. |
| `docs/GOAL_064_LIVING_WORLD_NPC_FACTION_SIMULATION_MATRIX_SPEC.md` | Goal 064 task/spec: living-world NPC/faction simulation matrix consuming Goal 060/061/062/063 evidence into 9 family/seed state-changing NPC/faction/world-event rows, save/load/replay audit and Unity Alpha living-world markers, with final stop at `living_world_npc_faction_simulation_matrix_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_064_LIVING_WORLD_NPC_FACTION_SIMULATION_MATRIX.md` | Goal 064 scouting: BCL-only Application-layer living-world simulation proof; no ECS dependency, provider/network/LLM/RAG/media generation/import, public GamePackage schema change, arbitrary Lua execution or broad Runtime/UI/Unity mutation. |
| `docs/GOAL_065_INTERLOCKED_GAMEPLAY_SYSTEMS_DEPTH_MATRIX_SPEC.md` | Goal 065 task/spec: interlocked gameplay systems depth matrix consuming Goal 060/061/062/063/064 evidence into 9 family/seed state-changing economy/crafting/combat/progression/status rows, save/load/replay audit and Unity Alpha interlocked gameplay markers, with final stop at `interlocked_gameplay_systems_depth_matrix_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_065_INTERLOCKED_GAMEPLAY_SYSTEMS_DEPTH_MATRIX.md` | Goal 065 scouting: BCL-only Application-layer interlocked gameplay proof; no ECS/GOAP/planner dependency, provider/network/LLM/RAG/media generation/import, public GamePackage schema change, arbitrary Lua execution or broad Runtime/UI/Unity mutation. |
| `docs/GOAL_066_SETTLEMENT_CONSTRUCTION_DESTRUCTION_PRODUCTION_MATRIX_SPEC.md` | Goal 066 task/spec: settlement construction/destruction/production matrix consuming Goal 060/061/062/063/064/065 evidence into 9 family/seed state-changing settlement rows, ledgers, save/load/replay audit and Unity Alpha settlement markers, with final stop at `settlement_construction_destruction_production_matrix_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_066_SETTLEMENT_CONSTRUCTION_DESTRUCTION_PRODUCTION_MATRIX.md` | Goal 066 scouting: BCL-only Application-layer settlement proof; no external city-builder/destructible-terrain/WFC dependency, provider/network/LLM/RAG/media generation/import, public GamePackage schema change, arbitrary Lua execution or broad Runtime/UI/Unity mutation. |
| `docs/GOAL_067_PROGRAMMATIC_NARRATIVE_QUEST_DIALOGUE_EVENT_MATRIX_SPEC.md` | Goal 067 task/spec: programmatic narrative quest/dialogue/event matrix consuming Goal 060/061/062/063/064/065/066 evidence into 9 family/seed state-changing narrative rows, quest/dialogue/event ledgers, memory/rumor/localization proof, save/load/replay audit and Unity Alpha narrative markers, with final stop at `programmatic_narrative_quest_dialogue_event_matrix_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_067_PROGRAMMATIC_NARRATIVE_QUEST_DIALOGUE_EVENT_MATRIX.md` | Goal 067 scouting: BCL-only Application-layer narrative proof; no Yarn/Ink/dialogue engine dependency, provider/network/LLM/RAG/media generation/import, final prose generation, public GamePackage schema change, arbitrary Lua execution or broad Runtime/UI/Unity mutation. |
| `docs/GOAL_068_COMBAT_MAGIC_ABILITY_BOSS_ENCOUNTER_MATRIX_SPEC.md` | Goal 068 task/spec: combat/magic/ability/boss encounter matrix consuming Goal 060/061/062/063/064/065/066/067 evidence into 9 family/seed state-changing combat rows, ability/status/boss catalogs, loot/progression and counterplay ledgers, save/load/replay audit and Unity Alpha combat_magic markers, with final stop at `combat_magic_ability_boss_encounter_matrix_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_068_COMBAT_MAGIC_ABILITY_BOSS_ENCOUNTER_MATRIX.md` | Goal 068 scouting: BCL-only Application-layer combat/magic proof; no ECS/behavior tree/combat framework dependency, provider/network/LLM/RAG/media generation/import, final prose generation, public GamePackage schema change, arbitrary Lua execution or broad Runtime/UI/Unity mutation. |
| `docs/GOAL_069_WORLD_EVENT_WEATHER_DAYNIGHT_CRISIS_MATRIX_SPEC.md` | Goal 069 task/spec: world event/weather/day-night/crisis matrix consuming Goal 060/061/062/063/064/065/066/067/068 evidence into 9 family/seed state-changing environmental pressure rows, save/load/replay audit and Unity Alpha world_event markers, with final stop at `world_event_weather_daynight_crisis_matrix_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_069_WORLD_EVENT_WEATHER_DAYNIGHT_CRISIS_MATRIX.md` | Goal 069 scouting: BCL-only Application-layer environmental pressure proof; no real weather/network/provider/LLM/RAG, public GamePackage schema mutation, arbitrary Lua execution or broad Runtime/UI/Unity weather rendering. |
| `docs/GOAL_070_INTEGRATED_CAMPAIGN_TIMELINE_SIMULATION_MATRIX_SPEC.md` | Goal 070 task/spec: integrated campaign timeline simulation matrix consuming Goal 060/061/062/063/064/065/066/067/068/069 evidence into 9 family/seed multi-step cross-system timeline rows, cascade and arbitration ledgers, save/load/replay audit and Unity Alpha campaign_timeline markers, with final stop at `integrated_campaign_timeline_simulation_matrix_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_070_INTEGRATED_CAMPAIGN_TIMELINE_SIMULATION_MATRIX.md` | Goal 070 scouting: BCL-only Application-layer integrated timeline proof; no provider/network/LLM/RAG/media generation, public GamePackage schema mutation, arbitrary Lua execution or broad Runtime/UI/Unity gameplay rendering. |
| `docs/GOAL_071_UNITY_ALPHA_INTERACTIVE_CAMPAIGN_PLAYER_SPEC.md` | Goal 071 task/spec: Unity Alpha interactive campaign player consuming Goal 070 integrated campaign timeline evidence into selectable family/seed rows, scripted input/action transitions, HUD/review contract, save/load/replay proof and Unity Alpha interactive_campaign markers, with final stop at `unity_alpha_interactive_campaign_player_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_071_UNITY_ALPHA_INTERACTIVE_CAMPAIGN_PLAYER.md` | Goal 071 scouting: use the existing repo-local Unity Alpha player with StreamingAssets, IMGUI and simple input/command plans; no UI Toolkit/TextMeshPro/ECS/new dependencies or broad Unity restructuring. |
| `docs/GOAL_072_GENERATOR_SPINE_QUALITY_CONSOLIDATION_SPEC.md` | Goal 072 task/spec: generator spine quality consolidation and risk audit over recent source, tests, Unity Alpha bootstrap, compact artifacts and state docs, with final stop at `generator_spine_quality_consolidation_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_072_GENERATOR_SPINE_QUALITY_CONSOLIDATION.md` | Goal 072 scouting: BCL-only repository-local quality scanner/evidence seam; no analyzers, external dashboards, dependencies or broad refactors. |
| `docs/GOAL_073_SOURCE_FORMAT_P0_READABILITY_REPAIR_SPEC.md` | Goal 073 task/spec: bounded source-format-only repair for `GQ-P0-SOURCE-EXTREME-LINE-LENGTH`, with final stop at `source_format_p0_readability_repair_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_073_SOURCE_FORMAT_P0_READABILITY_REPAIR.md` | Goal 073 scouting: manual bounded source formatting only; no dependencies, no broad autoformatter and no feature work. |
| `docs/GOAL_074_SCHEMA_DRIVEN_CAMPAIGN_AUTHORING_REVIEW_WORKSPACE_SPEC.md` | Goal 074 task/spec: schema-driven campaign authoring/review workspace consuming Goal 060-073 evidence into Application workspace data and bounded WinForms UserControls, with final stop at `schema_driven_campaign_authoring_review_workspace_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_074_SCHEMA_DRIVEN_CAMPAIGN_AUTHORING_REVIEW_WORKSPACE.md` | Goal 074 scouting: use existing WinForms/UserControl patterns and BCL-only workspace contracts; no external UI stack, provider, LLM/RAG, schema, Runtime, Unity or Lua dependency adoption. |
| `docs/GOAL_075_SCHEMA_DRIVEN_CAMPAIGN_EDIT_VALIDATE_APPLY_LOOP_SPEC.md` | Goal 075 task/spec: schema-driven campaign edit/validate/apply loop consuming accepted Goal 074 workspace and Goal 060-073 evidence into Application edit/change-set/apply/rollback evidence and bounded WinForms review controls, with final stop at `schema_driven_campaign_edit_validate_apply_loop_verification`. |
| `docs/EXTERNAL_SCOUTING_GOAL_075_SCHEMA_DRIVEN_CAMPAIGN_EDIT_VALIDATE_APPLY_LOOP.md` | Goal 075 scouting: reuse existing Goal 074 workspace contracts and WinForms UserControl patterns; no external editor stack, provider, LLM/RAG, schema, Runtime, Unity or Lua dependency adoption. |
| `docs/agent-tasks/goal-076-edit-driven-playable-preview-refresh/GOAL.md` | Goal 076 task: consume real Goal 075 applied edit-loop output into an edit-driven playable preview refresh proof, staged player handoff manifest, bounded WinForms playable refresh tab and final stop at `edit_driven_playable_preview_refresh_verification`. |
| `docs/agent-tasks/goal-077-edit-driven-review-package-materialization/GOAL.md` | Goal 077 task: consume real Goal 076 edit-driven playable preview refresh artifacts from disk into a deterministic review package with concrete target files, package ledger, player-readable index, staged read verification, bounded WinForms review package tab and final stop at `edit_driven_review_package_materialization_verification`. |
| `docs/agent-tasks/goal-078-edit-driven-review-package-playable-session/GOAL.md` | Goal 078 task: consume real Goal 077 disk-backed review package artifacts into a deterministic headless playable-session proof, package read proof, replay/state-chain proof, player-command index, bounded WinForms play session tab and final stop at `edit_driven_review_package_playable_session_verification`. |
| `docs/agent-tasks/goal-079-edit-driven-spine-quality-consolidation/GOAL.md` | Goal 079 task: consolidate Goal 074-078 edit-driven spine quality into a BCL-only Application seam, deterministic dashboard artifacts, bounded WinForms dashboard tab and final stop at `edit_driven_spine_quality_consolidation_verification`. |
| `docs/agent-tasks/goal-079a-source-format-line-ending-guard/GOAL.md` | Goal 079A hotfix task: strengthen the Goal 079 source-health scanner with raw-byte LF/CR metrics and final stop at `source_format_line_ending_guard_verification`, while keeping Goal 079 accepted=false. |
| `docs/agent-tasks/goal-080-edit-driven-gamepackage-runtime-preview-bridge/GOAL.md` | Goal 080 task: consume real Goal 077/078/079/079A edit-driven artifacts into a disk-backed projected public GamePackage, runtime-preview bridge proof, negative proof, bounded WinForms Runtime Bridge tab and final stop at `edit_driven_gamepackage_runtime_preview_bridge_verification`. |
| `docs/agent-tasks/goal-081-edit-driven-gamepackage-runtime-preview-playthrough/GOAL.md` | Goal 081 task: consume real Goal 080 projected GamePackage and bridge artifacts into a deterministic runtime-preview playthrough command script, replay transcript, state-hash chain, negative proof, bounded WinForms Preview Playthrough tab and final stop at `edit_driven_gamepackage_runtime_preview_playthrough_verification`. |
| `docs/agent-tasks/goal-082-edit-driven-unity-alpha-streamingassets-handoff/GOAL.md` | Goal 082 task: consume real Goal 080 projected GamePackage and Goal 081 runtime-preview playthrough artifacts into a compact Unity Alpha StreamingAssets handoff, independent Unity probe script, mirrored payload validation, bounded WinForms Unity Handoff tab and final stop at `edit_driven_unity_alpha_streamingassets_handoff_verification`. |
| `docs/agent-tasks/goal-082a-source-format-physical-line-repair/GOAL.md` | Goal 082A hotfix task: repair the Goal 082 source-format physical-line guard backstop with raw-byte scan metrics, explicit Unity probe / WinForms parent / Application seam coverage, compact Goal 082A evidence and final stop at `source_format_physical_line_repair_verification`, while keeping Goal 082 accepted=false. |
| `docs/agent-tasks/goal-083-visual-adult-layer-context-integration/GOAL.md` | Goal 083 docs/context task: integrate visual/adult-layer docs into the official context spine, queue, state docs, debt register and compact evidence, with final stop at `visual_adult_layer_context_integration_verification`; no code, Unity, schema, provider, media asset or prompt dump changes. |
| `docs/agent-tasks/goal-084-visual-asset-contract-rating-metadata/GOAL.md` | Goal 084 task: add a BCL-only Application-side visual asset contract/rating metadata validator, metadata-only fixtures, compact evidence and final stop at `visual_asset_contract_rating_metadata_verification`; no public schema, Runtime, Unity, provider, media, Lua, generator-library, project-file or prompt-dump changes. |
| `docs/agent-tasks/goal-085-deepsearch-backed-visual-part-pack-rule-stack/GOAL.md` | Goal 085 task: consume the eight deepsearch visual stack docs into a BCL-only Application-side visual part-pack contract/rule-stack validator, metadata-only fixture packs, compact evidence and final stop at `visual_part_pack_rule_stack_verification`; no public schema, Runtime, Unity, provider, media, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-086-deterministic-visual-microtile-materializer/GOAL.md` | Goal 086 task: consume Goal 084 visual asset slots and Goal 085 visual part-pack rule-stack metadata into a BCL-only Application deterministic visual microtile materializer, text SVG preview catalog/manifest/ledger/proofs and final stop at `deterministic_visual_microtile_materializer_verification`; no public schema, Runtime, Unity, provider, media, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-087-deterministic-visual-map-patch-composer/GOAL.md` | Goal 087 task: consume Goal 084 visual asset metadata, Goal 085 visual part-pack metadata and Goal 086 microtile previews into a BCL-only Application deterministic visual map patch composer, text SVG patch catalog/manifest/ledger/proofs and final stop at `deterministic_visual_map_patch_composer_verification`; no public schema, Runtime, Unity, provider, media, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-088-deterministic-visual-region-composer/GOAL.md` | Goal 088 task: consume Goal 084 visual asset metadata, Goal 085 visual part-pack metadata, Goal 086 microtile metadata and Goal 087 visual map patches into a BCL-only Application deterministic visual region composer, compact region definition/placement/chunk/proof artifacts, text SVG region overviews and final stop at `deterministic_visual_region_composer_verification`; no public schema, Runtime, Unity, provider, media, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-088a-check-all-hang-triage-validation-repair/GOAL.md` | Goal 088A task: validation-only check-all hang triage and repair after Goal 088, with full `.devflow/scripts/check-all.ps1` proof, compact triage evidence and final stop at `goal_088_check_all_validation_repair_verification`; no feature work, public schema, Runtime, Unity code, provider, media, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-089-tiered-validation-pipeline/GOAL.md` | Goal 089 task: add tiered devflow validation wrappers, validation tier profile, validation policy docs and compact evidence with final stop at `tiered_validation_pipeline_verification`; `check-all.ps1` remains authoritative and unchanged by default. |
| `docs/agent-tasks/goal-090-parameterized-visual-world-profiles/GOAL.md` | Goal 090 task: add a BCL-only Application-side parameterized visual world profile/addressing seam for arbitrary finite sizes, huge sparse finite worlds and infinite chunk windows, with final stop at `parameterized_visual_world_profiles_verification`; no Runtime, Unity, public schema, provider, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-091-deterministic-visual-chunk-stream-window/GOAL.md` | Goal 091 task: add a BCL-only Application-side deterministic visual chunk stream window materializer over Goal 090 profiles for finite clipping, huge sparse compact windows, overlapping infinite-window cache reuse and layer transitions, with final stop at `deterministic_visual_chunk_stream_window_verification`; no Runtime, Unity, public schema, provider, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-092-visual-world-stream-preview-workspace/GOAL.md` | Goal 092 task: add a bounded Application/WinForms visual world stream preview workspace that consumes real Goal 086-091 disk artifacts, lists proof status and text SVG previews, writes compact workspace evidence and stops at `visual_world_stream_preview_workspace_verification`; no Runtime, Unity, public schema, provider, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-092a-visual-world-preview-service-split-source-health/GOAL.md` | Goal 092A hotfix task: split the oversized Goal 092 Application service into smaller BCL-only files, add source-health before/after evidence and stop at `visual_world_preview_service_split_source_health_verification`; no Runtime, Unity, public schema, provider, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-093-visual-chunk-cache-export-contract/GOAL.md` | Goal 093 task: add a BCL-only Application visual chunk cache/export contract over real Goal 091 stream-window artifacts, with deterministic manifest/readback/sidecar proofs and final stop at `visual_chunk_cache_export_contract_verification`; no Runtime, Unity, public schema, provider, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-094-visual-chunk-cache-export-inspector/GOAL.md` | Goal 094 task: integrate real Goal 093 cache/export artifacts into the existing Visual World Stream Preview Workspace and WinForms review UI, write compact inspector evidence and stop at `visual_chunk_cache_export_inspector_verification`; no Runtime, Unity, public schema, provider, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-095-visual-chunk-cache-unity-streamingassets-handoff/GOAL.md` | Goal 095 task: mirror a compact metadata-only Goal 093/094 visual chunk cache payload into Unity Alpha StreamingAssets, add standalone probe source, write simulated-read/negative proof evidence and stop at `visual_chunk_cache_unity_streamingassets_handoff_verification`; no Runtime consumption, live Unity gameplay rendering, final atlas, runtime streaming, public schema, provider, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-096-unity-handoff-inspector-probe-readiness/GOAL.md` | Goal 096 task: surface the real Goal 095 Unity StreamingAssets payload, standalone probe inventory, simulated read, negative proof and no-Unity-file-change readiness in the existing Visual World Stream Preview Workspace and WinForms review UI, write compact inspector evidence and stop at `unity_handoff_inspector_probe_readiness_verification`; no Unity file mutation, Runtime consumption, live Unity gameplay rendering, final atlas, runtime streaming, public schema, provider, Lua, generator-library, project-file or dependency changes. |
| `docs/agent-tasks/goal-097-final-roadmap-rebaseline-dream-scope-productivity/GOAL.md` | Goal 097 task: final roadmap rebaseline, dream scope register, realism/geoworld simulator planning track, release risk register, milestone gates and aggressive goal productivity policy; docs/evidence only, final stop at `final_roadmap_rebaseline_dream_scope_productivity_verification`; no product code, Runtime, Unity, public schema, provider, Lua, generator-library, dependency, binary/raster media or prompt-dump changes. |
| `docs/agent-tasks/goal-098-geoworld-source-adapter-streaming-contract/GOAL.md` | Goal 098 task: BCL-only Application-side geoworld source adapter/streaming contract foundation using LLMGameCreator LFZ/geoworld docs only; metadata-only fixtures, cache/provenance/license policy, normalized feature taxonomy, streaming window contracts, negative proof and compact evidence; final stop at `geoworld_source_adapter_streaming_contract_verification`; no LFZ source/archive, Runtime, Unity, public schema, provider/network, Lua, generator-library, project-file, dependency, binary/raster media, raw geodata dump or live ingestion changes. |
| `docs/agent-tasks/goal-099-offline-geoworld-worldsourcegraph-streaming/GOAL.md` | Goal 099 task: BCL-only Application-side synthetic offline geoworld bundle normalization into WorldSourceGraph chunks, no-network stream-window boundary prefetch, compact text-SVG projection and existing Visual World Stream Preview Workspace integration; final stop at `offline_geoworld_worldsourcegraph_streaming_verification`; no LFZ source/archive, live geodata ingestion, Runtime, Unity, public schema, provider/network, Lua, generator-library, project-file, dependency, binary/raster media, raw geodata dump or prompt-dump changes. |
| `docs/agent-tasks/goal-100-offline-geoworld-visual-cache-unity-handoff/GOAL.md` | Goal 100 task: BCL-only Application-side offline geoworld visual cache over real Goal 099 artifacts, metadata-only Unity StreamingAssets handoff payloads, standalone probe and Visual World Stream Preview Workspace integration; final stop at `offline_geoworld_visual_cache_unity_handoff_verification`; no LFZ source/archive, live geodata ingestion, Runtime consumers, public schema, provider/network, Lua, generator-library, project-file, dependency, binary/raster media, raw geodata dump or prompt-dump changes. |
| `docs/agent-tasks/goal-101-offline-geoworld-unity-preview-runner/GOAL.md` | Goal 101 task: BCL-only Application-side offline geoworld Unity preview runner payload over real Goal 100 artifacts, metadata-only preview commands, standalone Unity Alpha preview runner scripts, travel-window demo metadata, simulated command execution proof and Visual World Stream Preview Workspace integration; final stop at `offline_geoworld_unity_preview_runner_verification`; no LFZ source/archive, live geodata ingestion, Runtime consumers, public schema, provider/network, Lua, generator-library, project-file, dependency, binary/raster media, raw geodata dump, final art, atlas, scene/prefab or prompt-dump changes. |
| `docs/agent-tasks/goal-102-offline-geoworld-unity-editor-preview-tool/GOAL.md` | Goal 102 task: Unity Editor-only offline geoworld preview window and BCL-only Application evidence over real Goal 101 payloads, simulated manual create/clear action proof, negative proof and Visual World Stream Preview Workspace integration; final stop at `offline_geoworld_unity_editor_preview_tool_verification`; no LFZ source/archive, live geodata ingestion, Runtime consumers, public schema, provider/network, Lua, generator-library, project-file, dependency, binary/raster media, raw geodata dump, final art, atlas, Unity scene/prefab/settings/packages/build-settings or prompt-dump changes. |
| `docs/agent-tasks/goal-102a-unity-editor-source-format-guard/GOAL.md` | Goal 102A hotfix task: raw-byte Unity Editor source-format guard over the Goal 102 preview tool scope, synthetic before/minified proof for `OfflineGeoworldPreviewWindow.cs`, after scan over relevant Goal102 Unity/Application sources, negative proof and final stop at `unity_editor_source_format_guard_verification`; no behavior change, Runtime, public schema, provider/network, Lua, generator-library, project-file/dependency, StreamingAssets, binary/raster media, final art, atlas, Unity scene/prefab/settings/packages/build-settings or prompt-dump changes. |
| `docs/agent-tasks/goal-102b-actual-unity-editor-source-reformat/GOAL.md` | Goal 102B hotfix task: actual Unity Editor source reformat/trust audit. Current evidence is BLOCKED because raw `HEAD:unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs` bytes are already multi-line/readable, so the required one-line HEAD-before proof cannot be produced honestly. It supersedes Goal102A source-format trust because Goal102A used synthetic-before evidence instead of actual target-file HEAD bytes. |
| `docs/agent-tasks/goal-103-offline-geoworld-playmode-travel-preview/GOAL.md` | Goal 103 task: offline geoworld play-mode travel preview over real Goal101 command/travel metadata plus Goal102/Goal102B evidence. Produces a BCL-only Application evidence seam, metadata-only Unity StreamingAssets handoff, standalone play-mode travel controller/state/chunk-visibility scripts, manual Unity Editor launch helper, workspace inspection and final stop at `offline_geoworld_playmode_travel_preview_verification`; no LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final art, atlas or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-104-offline-geoworld-interactive-travel-preview/GOAL.md` | Goal 104 task: offline geoworld interactive travel preview over real Goal103 play-mode travel evidence. Produces a BCL-only Application evidence seam, metadata-only Unity StreamingAssets handoff, standalone interactive travel controller/player-motor/boundary-prefetch-state scripts, manual Unity Editor launch helper, workspace inspection and final stop at `offline_geoworld_interactive_travel_preview_verification`; no LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final art, atlas or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-105-offline-geoworld-interaction-playable-probe/GOAL.md` | Goal 105 task: offline geoworld interaction playable probe over real Goal104 interactive travel evidence. Produces a BCL-only Application evidence seam, metadata-only Unity StreamingAssets interaction payload, standalone interaction controller/target/state-delta-log scripts, manual Unity Editor probe helper, workspace inspection and final stop at `offline_geoworld_interaction_playable_probe_verification`; no LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final art, atlas or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-106-offline-geoworld-session-persistence-replay/GOAL.md` | Goal 106 task: offline geoworld session persistence/replay over real Goal105 interaction evidence. Produces a BCL-only Application evidence seam, metadata-only Unity StreamingAssets session payload, standalone snapshot/save-load/replay scripts, manual Unity Editor replay helper, workspace inspection and final stop at `offline_geoworld_session_persistence_replay_verification`; no LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final art, atlas or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-107-offline-geoworld-objective-acceptance-run/GOAL.md` | Goal 107 task: offline geoworld objective acceptance run over real Goal106 session persistence/replay evidence. Produces a BCL-only Application evidence seam, metadata-only Unity StreamingAssets objective payload, standalone objective tracker/state/acceptance controller scripts, manual Unity Editor acceptance helper, workspace inspection, Unity Alpha quality consolidation and final stop at `offline_geoworld_objective_acceptance_run_verification`; no LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final art, atlas or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-108-offline-geoworld-alpha-slice-orchestrator/GOAL.md` | Goal 108 task: offline geoworld Alpha Slice orchestrator over real Goal101-107 Alpha evidence. Produces a BCL-only Application evidence seam, metadata-only Unity StreamingAssets Alpha Slice payload, manual Unity Editor one-click setup/clear/verify window, small coordinator script, acceptance runbook, workspace inspection, full-slice simulated proof, negative proof and final stop at `offline_geoworld_alpha_slice_orchestrator_verification`; no LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final art, atlas or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-108a-alpha-slice-source-split-immutability-audit/GOAL.md` | Goal 108A hotfix/audit task: split the Goal108 Alpha Slice orchestrator source below the 700-line ceiling and audit actual `14ad9f38..989a79ab` git evidence for historical Goal101-107 immutability. Produces BCL-only Application audit evidence under `.llmgc/procedural/goal-108a-alpha-slice-source-split-immutability-audit/`; records zero Goal101-107 artifact modifications, 17 Goal108 additions, matching Goal108 `historicalArtifactsUnchanged=true`, no evidence-trust debt and unchanged AlphaRuntimeBootstrap; no Runtime, schema, provider, Lua, generator-library, Unity scene/settings/project/dependency or historical artifact rewrites. |
| `docs/agent-tasks/goal-109-offline-geoworld-alpha-slice-export-package/GOAL.md` | Goal 109 task: package the offline geoworld Alpha Slice into a portable deterministic directory package over real Goal108/108A artifacts. Produces a BCL-only Application package seam, package manifest/file-index/checksums/runbook/acceptance gate/readme, clean-import proof, negative proof, Unity StreamingAssets metadata mirror, standalone Unity verifier/editor window, Visual World Stream Preview Workspace inspection and final stop at `offline_geoworld_alpha_slice_export_package_verification`; no LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final release, final art, atlas or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-110-offline-geoworld-alpha-manual-acceptance-gate/GOAL.md` | Goal 110 task: add the offline geoworld Alpha manual acceptance gate over the real Goal109 export package. Produces a BCL-only Application acceptance seam, manifest/checklist/result-template/dashboard/readme payloads, export file-index/checksums, simulated result readback proof, negative proof, Unity StreamingAssets metadata mirror, standalone Unity result/store scripts, Editor acceptance runner window, Visual World Stream Preview Workspace inspection and final stop at `offline_geoworld_alpha_manual_acceptance_verification`; no LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final release, final art, atlas or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-111-offline-geoworld-alpha-manual-result-intake/GOAL.md` | Goal 111 task: add the offline geoworld Alpha manual-result intake and decision bridge over the real Goal110 package. Produces a BCL-only Application verifier, deterministic decision/report/file-index/quality/negative-proof evidence, export dashboard/readme/index metadata, Visual World Stream Preview Workspace/WinForms decision visibility and final status `BLOCKED_PENDING_MANUAL_RESULT` until a real human result JSON is supplied; no LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final release, final art, atlas or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-112-offline-geoworld-alpha-acceptance-operator-pack/GOAL.md` | Goal 112 task: add the offline geoworld Alpha acceptance operator pack and RC readiness dashboard over the real Goal110 package and Goal111 decision bridge. Produces a BCL-only Application operator service, deterministic dashboard/runbook/path-map/preflight/notary/quality/negative-proof evidence, export metadata, short manual-acceptance runbook, Visual World Stream Preview Workspace/WinForms operator status visibility and final status `OPERATOR_READY_PENDING_HUMAN_RUN` until a real human result JSON is supplied; no LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final release, final art, atlas, Unity files or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-113-offline-geoworld-alpha-manual-result-workbench/GOAL.md` | Goal 113 task: add the offline geoworld Alpha manual-result workbench over the real Goal110 package, Goal111 decision bridge and Goal112 operator pack. Produces a BCL-only Application workbench, deterministic dashboard/runbook/field-map/draft-template/quality/negative-proof evidence, export metadata, short manual-result workbench guide, Visual World Stream Preview Workspace/WinForms workbench visibility and final status `WORKBENCH_READY_PENDING_HUMAN_RESULT` until a real human result JSON is supplied; no `.llmgc/manual/**` real result, LFZ source/archive, live network/provider/geodata, Runtime, public schema, Lua, generator-library, project-file/dependency, binary/raster media, final release, final art, atlas, Unity files or Unity scene/prefab/settings/packages/build-settings changes. |
| `docs/agent-tasks/goal-114-unity-safe-mode-compile-hotfix/GOAL.md` | Goal 114 hotfix task: repair the Unity Safe Mode compile blockers found during Goal110/111/112/113 manual acceptance by removing unqualified `JsonUtility` usage from concrete Unity helper scripts and adding low-risk `RefreshPayloadStatus()` compatibility wrappers. Produces compact source-scan, dashboard, negative-proof, file-index and report evidence under `.llmgc/procedural/goal-114-unity-safe-mode-compile-hotfix/` and `.llmgc/exports/goal-114-unity-safe-mode-compile-hotfix/`; keeps `offline_geoworld_alpha_manual_acceptance_verification required`, `accepted=false`, writes no `.llmgc/manual/**` result and changes no `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/ProjectSettings/Packages/StreamingAssets, Runtime, public schema, providers, Lua, generator-library or project/dependency files. |
| `docs/agent-tasks/goal-115-offline-geoworld-alpha-human-result-revalidation/GOAL.md` | Goal 115 task: revalidate the real local offline geoworld Alpha human result without committing `.llmgc/manual/**`. Produces a BCL-only Application revalidation service, deterministic dashboard/decision-snapshot/report/file-index/quality/negative-proof evidence, export metadata, short decision note and Visual World Stream Preview Workspace/WinForms visibility. Current result is `GREEN_ACCEPTABLE_CANDIDATE` with manualResultSha256 `8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb`, 12/12 required steps passed, acceptedByCodex=false and humanAcceptanceStillRequired=true; the active gate remains `offline_geoworld_alpha_manual_acceptance_verification required`, `accepted=false`. |
| `docs/agent-tasks/goal-116-offline-geoworld-alpha-manual-gate-acceptance-record/GOAL.md` | Goal 116 task: record explicit human acceptance of `offline_geoworld_alpha_manual_acceptance_verification` from Goal115 GREEN candidate evidence without committing `.llmgc/manual/**`. Produces a BCL-only Application acceptance-record service, deterministic acceptance/dashboard/report/file-index/quality/negative-proof evidence, export metadata, short acceptance note and Visual World Stream Preview Workspace/WinForms visibility. Current result is `ACCEPTED_BY_HUMAN`, humanAccepted=true, acceptedByCodex=false, manualInputNotCommitted=true, rawManualResultEmbeddedInArtifacts=false and recommendedNextDecision=`POST_ACCEPTANCE_CONTINUATION_SELECTION`. |
| `docs/agent-tasks/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/GOAL.md` | Goal 117 task: create the post-acceptance continuation-selection matrix after Goal116. Current result is `GREEN`, recommendedNextLane=`accepted_alpha_baseline_review`, recommendedNextGoalId=`goal-118-offline-geoworld-accepted-alpha-baseline-review`, doNotStartAutomatically=true and no Goal118 task files created. |
| `docs/agent-tasks/goal-118-offline-geoworld-accepted-alpha-baseline-review/GOAL.md` | Goal 118 task: create the accepted offline geoworld Alpha baseline review package after Goal116 human acceptance. Current result is `GREEN`, baselineId=`offline_geoworld_alpha_accepted_baseline_v1`, acceptedBaselineReady=true, manualGateStatus=`ACCEPTED_BY_HUMAN`, sourceGoalRange=`Goal098-Goal117`, includedSourceGoalCount=23, acceptedEvidenceRootCount=6, producedOnlyRootCount=17 and recommendedNextDecision=`EXPLICIT_NEXT_LANE_SELECTION`; it embeds no `.llmgc/manual/**` input and starts no live geodata/provider/network, Runtime/schema, Lua, generator-library, Unity scene/prefab/settings/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-119-accepted-alpha-unity-playable-projection/GOAL.md` | Goal 119 task: create the accepted Alpha Unity playable projection entrypoint over the Goal118 accepted baseline. Current result is `GREEN`, Unity menu path=`LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`, generated root=`__LLMGC_AcceptedAlphaPlayableProjection__`, evidence under `.llmgc/procedural/goal-119-accepted-alpha-unity-playable-projection/`, export metadata under `.llmgc/exports/goal-119-accepted-alpha-unity-playable-projection/`, and manual gate=`accepted_alpha_unity_playable_projection_verification`; it embeds no `.llmgc/manual/**` input and starts no live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-119a-accepted-alpha-unity-material-warning-hotfix/GOAL.md` | Goal 119A hotfix task: remove the edit-mode material-instantiation warning from the Goal119 accepted Alpha Unity projection by replacing projection marker material mutation with `MaterialPropertyBlock`, adding a batchmode Unity smoke entrypoint and source/log scan evidence under `.llmgc/procedural/goal-119a-accepted-alpha-unity-material-warning-hotfix/` and `.llmgc/exports/goal-119a-accepted-alpha-unity-material-warning-hotfix/`. Goal119 remains the product deliverable and the next manual route remains `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`; no `.llmgc/manual/**`, Runtime/schema/provider/Lua/generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work is authorized. |
| `docs/agent-tasks/goal-120-accepted-alpha-projection-usability-and-cleanup/GOAL.md` | Goal 120 task: improve the accepted Alpha projection route for `accepted_alpha_projection_usability_and_cleanup_verification` with descriptor-backed markers, legend, focus/select controls, Goal120 batchmode usability smoke and bounded Unity editor-noise cleanup evidence under `.llmgc/procedural/goal-120-accepted-alpha-projection-usability-and-cleanup/` and `.llmgc/exports/goal-120-accepted-alpha-projection-usability-and-cleanup/`. The manual route remains `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`; no `.llmgc/manual/**`, Runtime/schema/provider/Lua/generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work is authorized. |
| `docs/agent-tasks/goal-120a-clean-unity-editor-noise-empty-status-hotfix/GOAL.md` | Goal 120A hotfix task for `goal_120a_clean_unity_editor_noise_empty_status_hotfix_verification`: fix `.devflow/scripts/clean-unity-editor-noise.ps1` so clean/empty `git status --porcelain=v1 --untracked-files=all` output is treated as an empty status list and `-DryRun`/`-Apply` exit 0 on a clean worktree. The supported cleanup command remains `.devflow\scripts\clean-unity-editor-noise.cmd` or `.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply`; cleanup scope remains bounded and no `.llmgc/manual/**`, Unity source/settings/package, Runtime/schema/provider/Lua/generator-library or release-packaging work is authorized. |
| `docs/agent-tasks/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/GOAL.md` | Goal 121 task: reduce accepted Alpha Unity hands-on verification to `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection` plus `Run Full Projection Verification`, with selected-marker details, interaction/action preview, objective/replay details, compact event log and Goal121 batchmode full verification evidence under `.llmgc/procedural/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/` and `.llmgc/exports/goal-121-accepted-alpha-interaction-drilldown-and-one-click-verification/`. After Unity checks use `.devflow\scripts\clean-unity-editor-noise.cmd`; no `.llmgc/manual/**`, Runtime/schema/provider/Lua/generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work is authorized. |
| `docs/agent-tasks/goal-122-accepted-alpha-projection-action-loop-and-window-polish/GOAL.md` | Goal 122 task: keep accepted Alpha Unity hands-on verification on `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection` plus `Run Full Projection Verification`, add a projection-local Preview/Apply/Reset action loop, clean up the EditorWindow layout with compact status and bounded panels, and record Goal122 batchmode action-loop evidence under `.llmgc/procedural/goal-122-accepted-alpha-projection-action-loop-and-window-polish/` and `.llmgc/exports/goal-122-accepted-alpha-projection-action-loop-and-window-polish/`. After Unity checks use `.devflow\scripts\clean-unity-editor-noise.cmd`; no `.llmgc/manual/**`, Runtime/schema/provider/Lua/generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work is authorized. |
| `docs/agent-tasks/goal-123-generic-gamepackage-playable-projection-adapter/GOAL.md` | Goal 123 task: add a generic GamePackage projection-only path to the accepted Alpha Unity projection route. Current result is `GREEN`, manual path=`LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection -> Run Generic Package Projection Verification`, sample package=`samples/minimal-map-game/package.json` read-only, evidence under `.llmgc/procedural/goal-123-generic-gamepackage-playable-projection-adapter/`, export metadata under `.llmgc/exports/goal-123-generic-gamepackage-playable-projection-adapter/`, and Unity batchmode marker=`GOAL123_GENERIC_PACKAGE_PROJECTION_PASS`; it starts no live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/GOAL.md` | Goal 124 task: add a projection-local quest/dialogue/interaction loop over the generic sample GamePackage. Current result is `GREEN`, manual path=`LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection -> Run Generic Package Gameplay Loop Verification`, sample package=`samples/minimal-map-game/package.json` read-only, evidence under `.llmgc/procedural/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/`, export metadata under `.llmgc/exports/goal-124-generic-gamepackage-quest-dialogue-interaction-loop/`, and Unity batchmode marker=`GOAL124_GENERIC_GAMEPACKAGE_LOOP_PASS`; it starts no sample mutation, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-125-generic-gamepackage-systems-loop-projection/GOAL.md` | Goal 125 task: add a projection-local inventory/resource/crafting/harvest/transaction/encounter/combat systems loop over the generic sample GamePackage. Current result is `GREEN`, manual path=`LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection -> Run Generic Package Systems Loop Verification`, sample package=`samples/minimal-map-game/package.json` read-only, evidence under `.llmgc/procedural/goal-125-generic-gamepackage-systems-loop-projection/`, export metadata under `.llmgc/exports/goal-125-generic-gamepackage-systems-loop-projection/`, and Unity batchmode marker=`GOAL125_GENERIC_GAMEPACKAGE_SYSTEMS_PASS`; it starts no sample mutation, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-126-generic-gamepackage-full-playthrough-projection/GOAL.md` | Goal 126 task: add a projection-only one-click full playthrough over the generic sample GamePackage. Current gate=`goal_126_generic_gamepackage_full_playthrough_projection`, result is `GREEN`, manual path=`LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection -> Run Generic Package Full Playthrough Verification`, sample package=`samples/minimal-map-game/package.json` read-only, evidence under `.llmgc/procedural/goal-126-generic-gamepackage-full-playthrough-projection/`, export metadata under `.llmgc/exports/goal-126-generic-gamepackage-full-playthrough-projection/`, and Unity batchmode marker=`GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS`; it starts no sample mutation, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-127-winforms-unity-projection-verification-runner/GOAL.md` | Goal 127 task: add a repo-local and WinForms-visible Unity projection verification runner for the Goal126 full playthrough. Current gate=`goal_127_winforms_unity_projection_verification_runner`, normal command=`.devflow\scripts\run-unity-projection-verification.cmd`, evidence under `.llmgc/procedural/goal-127-winforms-unity-projection-verification-runner/`, export metadata under `.llmgc/exports/goal-127-winforms-unity-projection-verification-runner/`, Unity batchmode method=`LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeGenericGamePackageFullPlaythroughSmoke`, pass marker=`GOAL126_GENERIC_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS`, and manual Unity inspection is optional; it starts no sample mutation, `.llmgc/manual/**`, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/GOAL.md` | Goal 128 task: parameterize the repo-local Unity projection verification runner and WinForms command surface. Current gate=`goal_128_parameterized_gamepackage_projection_runner_and_winforms_command_surface`, normal command=`.devflow\scripts\run-unity-projection-verification.cmd`, optional package parameter=`-PackagePath`, default package=`samples/minimal-map-game/package.json` read-only, Unity argument=`-llmgcPackagePath`, evidence under `.llmgc/procedural/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/`, export metadata under `.llmgc/exports/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/`, Unity batchmode method=`LLMGameCreatorAlpha.AcceptedAlphaPlayableProjectionWindow.RunBatchmodeParameterizedGamePackageFullPlaythroughSmoke`, pass marker=`GOAL128_PARAMETERIZED_GAMEPACKAGE_FULL_PLAYTHROUGH_PASS`, and manual Unity inspection remains optional; it starts no sample mutation, `.llmgc/manual/**`, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-129-gamepackage-candidate-matrix-projection-runner/GOAL.md` | Goal 129 task: add a deterministic GamePackage candidate matrix over the Goal128 parameterized runner. Current gate=`goal_129_gamepackage_candidate_matrix_projection_runner`, normal command=`.devflow\scripts\run-gamepackage-projection-matrix.cmd`, candidate index=`.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-candidate-index.json`, aggregate result=`.llmgc/procedural/goal-129-gamepackage-candidate-matrix-projection-runner/gamepackage-projection-matrix-result.json`, baseline candidate is a byte-copy of `samples/minimal-map-game/package.json`, variant candidate keeps required compatibility IDs, and manual Unity inspection remains optional; it starts no sample mutation, `.llmgc/manual/**`, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/GOAL.md` | Goal 130 task: add a deterministic GamePackage candidate factory and matrix pipeline over the Goal129 matrix runner. Current gate=`goal_130_gamepackage_candidate_factory_and_matrix_pipeline`, normal command=`.devflow\scripts\run-gamepackage-candidate-factory.cmd`, candidate index=`.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-candidate-index.json`, factory result=`.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-candidate-factory-result.json`, matrix result=`.llmgc/procedural/goal-130-gamepackage-candidate-factory-and-matrix-pipeline/gamepackage-projection-matrix-result.json`, generated candidate count is 3 with GREEN 3/3 matrix proof, and manual Unity inspection remains optional; it starts no sample mutation, `.llmgc/manual/**`, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/GOAL.md` | Goal 131 task: add a deterministic GamePackage candidate recipe catalog, scoring pass and selected-candidate promotion over the Goal130/Goal129 pipeline. Current gate=`goal_131_gamepackage_candidate_recipe_catalog_scoring_and_promotion`, normal command=`.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd`, recipe catalog=`.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/candidate-recipe-catalog.json`, candidate index=`.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-candidate-index.json`, scoring result=`.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/candidate-scoring-result.json`, selected handoff=`.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/selected-candidate/selected-candidate-handoff.json`, generated candidate count is 4 with GREEN 4/4 matrix proof, selectedCandidateId=`minimal-map-game-balanced-baseline`, selectedCandidateScore=100, metadataOnlyRecipeMutation=true and manual Unity inspection remains optional; it starts no sample mutation, `.llmgc/manual/**`, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-132-winforms-candidate-pipeline-operator-panel/GOAL.md` | Goal 132 task: add a WinForms Candidate Pipeline Operator panel over the existing Goal131 candidate recipe pipeline. Current gate=`goal_132_winforms_candidate_pipeline_operator_panel`, normal command=`.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd`, result path=`.llmgc/procedural/goal-131-gamepackage-candidate-recipe-catalog-scoring-and-promotion/gamepackage-recipe-pipeline-result.json`, selectedCandidateId=`minimal-map-game-balanced-baseline`, selectedCandidateScore=100, candidateCount=4, passedCandidates=4, failedCandidates=0, matrixPassed=true, operatorStatus=`GREEN_READY`, and manual Unity inspection remains optional; it starts no sample mutation, `.llmgc/manual/**`, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-133a-product-line-strategy-rebaseline-and-canonical-runtime-pivot/GOAL.md` | Goal 133A task: product-line strategy rebaseline and canonical runtime pivot. Current gate=`product_line_strategy_rebaseline_verification`, accepted=false, nextProductGoal=`goal_134_canonical_runtime_selected_candidate_playthrough_matrix`; it routes the Goal131/132 selected candidate toward package validation, canonical runtime playthrough, save/load/replay proof and Unity/player consumption of canonical transcript/state summary instead of another projection-only wrapper. |
| `docs/agent-tasks/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/GOAL.md` | Goal 134 task: canonical Runtime selected-candidate playthrough matrix over the Goal131 selected handoff. Current gate=`canonical_runtime_selected_candidate_playthrough_matrix_verification`, result is `GREEN`, accepted=false, selectedCandidateId=`minimal-map-game-balanced-baseline`, normal command=`.devflow\scripts\run-canonical-runtime-selected-candidate-playthrough.cmd`, evidence under `.llmgc/procedural/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/`, export metadata under `.llmgc/exports/goal-134-canonical-runtime-selected-candidate-playthrough-matrix/`, package validation, canonical runtime transcript/state summary, save/load/replay proof and Unity/player transcript smoke are present, projectionOnly=false, selectedCandidateExecutedByRuntime=true and manual Unity inspection remains optional; it starts no sample mutation, `.llmgc/manual/**`, public GamePackage schema, Generation, AssetPipeline, Scripting/Lua, provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-135-canonical-runtime-playable-player-loop-readiness/GOAL.md` | Goal 135 task: canonical Runtime playable player-loop readiness over the Goal134 canonical transcript/state summary. Current gate=`canonical_runtime_playable_player_loop_readiness_verification`, result is `GREEN`, accepted=false, selectedCandidateId=`minimal-map-game-balanced-baseline`, normal command=`.devflow\scripts\run-canonical-runtime-player-loop-readiness.cmd`, evidence under `.llmgc/procedural/goal-135-canonical-runtime-playable-player-loop-readiness/`, export metadata under `.llmgc/exports/goal-135-canonical-runtime-playable-player-loop-readiness/`, PlayerAdapter contract, 13-step player-loop plan, required categories, diagnostic classification and Unity/player readiness smoke are present, projectionOnly=false, canonicalRuntimeSource=true, playerAdapterCoverage=true, unityGameplayTruth=false and manual Unity inspection remains optional; it starts no sample mutation, `.llmgc/manual/**`, public GamePackage schema, Generation, AssetPipeline, Scripting/Lua, provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-136-canonical-runtime-player-command-loop-execution-matrix/GOAL.md` | Goal 136 task: canonical Runtime player command-loop execution matrix over Goal134/Goal135 evidence. Current gate=`canonical_runtime_player_command_loop_execution_matrix_verification`, result is `GREEN`, accepted=false, selectedCandidateId=`minimal-map-game-balanced-baseline`, normal command=`.devflow\scripts\run-canonical-runtime-player-command-loop.cmd`, evidence under `.llmgc/procedural/goal-136-canonical-runtime-player-command-loop-execution-matrix/`, export metadata under `.llmgc/exports/goal-136-canonical-runtime-player-command-loop-execution-matrix/`, playerCommandCount=13, snapshotCount=13, runtimeEventCount>=10, all required command categories, state hash chain, diagnostic classification and Unity/player snapshot consumption smoke are present, projectionOnly=false, unityGameplayTruth=false and manual Unity inspection remains optional; it starts no sample mutation, `.llmgc/manual/**`, public GamePackage schema, Generation, AssetPipeline, Scripting/Lua, provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-137-canonical-runtime-unity-player-loop-playback-harness/GOAL.md` | Goal 137 task: canonical Runtime Unity/player loop playback harness over Goal136 snapshots. Current gate=`canonical_runtime_unity_player_loop_playback_harness_verification`, result is `GREEN`, accepted=true by human handoff, selectedCandidateId=`minimal-map-game-balanced-baseline`, normal command=`.devflow\scripts\run-canonical-runtime-unity-player-loop-playback.cmd`, evidence under `.llmgc/procedural/goal-137-canonical-runtime-unity-player-loop-playback-harness/`, export metadata under `.llmgc/exports/goal-137-canonical-runtime-unity-player-loop-playback-harness/`, playbackFrameCount=13, required frame categories, Unity/player playback smoke, runtimeSnapshotSource=true, unityConsumesRuntimeSnapshots=true, projectionOnly=false, unityGameplayTruth=false and manual Unity inspection remains optional; it starts no sample mutation, `.llmgc/manual/**`, public GamePackage schema, Generation, AssetPipeline, Scripting/Lua, provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/GOAL.md` | Goal 138 task: runtime-backed Unity player-loop stepper/HUD harness over Goal137 playback frames and Goal136 Runtime snapshots. Result is `GREEN`, accepted=true by human handoff, acceptedGoal137=true, selectedCandidateId=`minimal-map-game-balanced-baseline`, normal command=`.devflow\scripts\run-runtime-backed-unity-player-loop-stepper.cmd`, evidence under `.llmgc/procedural/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/`, export metadata under `.llmgc/exports/goal-138-runtime-backed-unity-player-loop-stepper-hud-harness/`, frameCount=13, required frame categories, runtimeAuthority=true, stepperWindowPresent=true, stepperBatchSmokePassed=true, projectionOnly=false, unityGameplayTruth=false and manual Unity inspection remains optional; it starts no sample mutation, `.llmgc/manual/**`, public GamePackage schema, Generation, AssetPipeline, Scripting/Lua, provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/GOAL.md` | Goal 139 task: runtime-backed Unity player-loop interactive controls over the Goal138 stepper model/result. Current gate=`runtime_backed_unity_player_loop_interactive_controls_harness_verification`, result is `GREEN`, accepted=false, acceptedGoal138=true, selectedCandidateId=`minimal-map-game-balanced-baseline`, normal command=`.devflow\scripts\run-runtime-backed-unity-player-loop-interactive-controls.cmd`, evidence under `.llmgc/procedural/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/`, export metadata under `.llmgc/exports/goal-139-runtime-backed-unity-player-loop-interactive-controls-harness/`, frameCount=13, required controls present, controlScriptPassed=true, interactiveControlsWindowPresent=true, unityInteractiveControlsSmokePassed=true, runtimeAuthority=true, projectionOnly=false, unityGameplayTruth=false and manual Unity inspection remains optional; it records Goal138 human acceptance and starts no sample mutation, `.llmgc/manual/**`, public GamePackage schema, Runtime, Generation, AssetPipeline, Scripting/Lua, provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/GOAL.md` | Goal 140 task: runtime-backed Unity player-loop controls UX polish and Unity editor noise guard over the Goal139 interactive controls model/result/script. Current gate=`runtime_backed_unity_player_loop_controls_ux_polish_and_noise_guard_verification`, result is `GREEN`, accepted=true by Goal141 human handoff, acceptedGoal139=true, selectedCandidateId=`minimal-map-game-balanced-baseline`, normal command=`.devflow\scripts\run-runtime-backed-unity-player-loop-controls-ux-polish.cmd`, evidence under `.llmgc/procedural/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/`, export metadata under `.llmgc/exports/goal-140-runtime-backed-unity-player-loop-controls-ux-polish-and-noise-guard/`, frameCount=13, humanReadableFrameNumbering=true, stepOnceSemanticsClear=true, playAllToEndSemanticsClear=true, knownUnityEditorNoiseClassified=true, blockingUnityErrorCount=0, unclassifiedUnityErrorCount=0, unityControlsUxSmokePassed=true, runtimeAuthority=true, projectionOnly=false and unityGameplayTruth=false; it records Goal139 human acceptance and starts no sample mutation, `.llmgc/manual/**`, public GamePackage schema, Runtime, Generation, AssetPipeline, Scripting/Lua, provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/GOAL.md` | Goal 141 task: runtime-backed Unity/player command roundtrip bridge over Goal140 controls UX artifacts. Current gate=`runtime_backed_unity_player_command_roundtrip_bridge_verification`, result is `GREEN`, accepted=false, goal140Accepted=true, selectedCandidateId=`minimal-map-game-balanced-baseline`, normal command=`.devflow\scripts\run-runtime-backed-player-command-roundtrip.cmd`, evidence under `.llmgc/procedural/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/`, export metadata under `.llmgc/exports/goal-141-runtime-backed-unity-player-command-roundtrip-bridge/`, roundtripRequestCount=6, runtimeRoutedRequestCount=4, presentationOnlyRequestCount=2, runtimeExecutedRequestCount=4, presentationOnlyRuntimeExecutionCount=0, roundtripSnapshotCount=15, requestResponseCorrelationPassed=true, sequentialCursorContinuityPassed=true, stateHashContinuityPassed=true, copySummaryStateUnchanged=true, loadModelStateUnchanged=true, noControlIntentMappedToUnrelatedGameplayCommand=true, roundtripSemanticCorrectnessPassed=true, controlRequestBridgePresent=true, stateHashChainPresent=true, runtimeAuthority=true, projectionOnly=false, unityConsumesRoundtripResult=true and unityGameplayTruth=false; it records Goal140 human acceptance and starts no sample mutation, `.llmgc/manual/**`, public GamePackage schema, Generation, AssetPipeline, Scripting/Lua, provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-141a-player-command-roundtrip-semantic-correctness-hotfix/GOAL.md` | Goal 141A hotfix task: correct Goal141 request-level semantics so `load_model` and `copy_frame_summary` are presentation-only, `reset_first` starts the runtime session, `step_once` and `next_frame` advance the current runtime cursor, and `play_all_to_end` executes the remaining Runtime-owned commands. Current result is `GREEN`, roundtripSemanticCorrectnessPassed=true, totalControlRequestCount=6, runtimeRoutedRequestCount=4, presentationOnlyRequestCount=2, runtimeExecutedRequestCount=4, presentationOnlyRuntimeExecutionCount=0, requestResponseCorrelationPassed=true, sequentialCursorContinuityPassed=true, stateHashContinuityPassed=true, copySummaryStateUnchanged=true, loadModelStateUnchanged=true and noControlIntentMappedToUnrelatedGameplayCommand=true; it writes compact Goal141A evidence under `.llmgc/procedural/goal-141a-player-command-roundtrip-semantic-correctness-hotfix/` and `.llmgc/exports/goal-141a-player-command-roundtrip-semantic-correctness-hotfix/`. |
| `docs/agent-tasks/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/GOAL.md` | Goal 142 task: runtime-significant product-line variant matrix and selection handoff over the read-only minimal-map sample package. Result is `GREEN`, accepted=true by explicit Goal143 human handoff, acceptedByCodex=false, goal141Accepted=false, normal command=`.devflow\scripts\run-product-line-runtime-variant-matrix.cmd`, evidence under `.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/`, export metadata under `.llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/`, candidateCount=4, passedCandidateCount=4, runtimeSignificantCandidateCount=4, distinctFinalStateHashCount=4, selectedCandidateId=`minimal-map-game-exploration-resource-focus`, selectedScore=100, sourceTemplateUnmodified=true, runtimeAuthority=true, projectionOnly=false and unityGameplayTruth=false; it starts no sample mutation, `.llmgc/manual/**`, public GamePackage schema, Generation, AssetPipeline, Scripting/Lua, provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings/packages/StreamingAssets or release packaging work. |
| `docs/agent-tasks/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix/GOAL.md` | Goal 142A P1 hotfix: the Goal142 WinForms button uses the in-process Application matrix service through a transactional operator runner, starts no compiler/test child process, disables the button while running and refreshes the workspace after success. Failed regeneration restores prior Goal142 procedural/export bytes; the corrected retry succeeded with exitCode=0 before Goal143 recorded human acceptance. Evidence is under `.llmgc/procedural/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix/` and `.llmgc/exports/goal-142a-winforms-operator-self-lock-and-atomic-regeneration-hotfix/`. |
| `docs/agent-tasks/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/GOAL.md` | Goal 143 task: selected Goal142 runtime variant end-to-end PlayerAdapter handoff. Result is `GREEN`, accepted=true by explicit Goal144 human handoff, acceptedByCodex=false, selectedCandidateId=`minimal-map-game-exploration-resource-focus`, package/final Runtime hashes match, frameCount=15 and Unity batchmode consumer smoke GREEN. |
| `docs/agent-tasks/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/GOAL.md` | Goal 144 task: Runtime-owned selected-variant interactive action session and journal save/replay. Result is `GREEN`, accepted=true by explicit Goal145 human handoff, acceptedByCodex=false, actionDescriptorCount=14, executedRuntimeActionCount=11, checkpoint/full replay and read-only Unity smoke GREEN. |
| `docs/agent-tasks/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix/GOAL.md` | Goal 144A P1 hotfix: descriptor, response and journal bind to the exact canonical step/range/command/target; harvest uses `node/apple_tree`, basic attack uses `goblin`, checkpoint replay evidence is frozen at 8 actions and final replay reports 13. Runtime remains authority and Unity remains read-only. |
| `docs/agent-tasks/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/GOAL.md` | Goal 145 task: four discovered Goal142 candidates execute the same Runtime session kernel with exact action binding, checkpoint/full replay, fresh semantic focus comparisons, operator-selectable in-process WinForms sessions and a read-only Unity matrix. Result is `GREEN`, accepted=true by exact Goal146 human handoff, acceptedByCodex=false, candidateCount=4, passedCandidateCount=4 and distinctFinalStateHashCount=4. |
| `docs/agent-tasks/goal-145a-winforms-candidate-selector-reentrancy-and-selection-stability-hotfix/GOAL.md` | Goal 145A hotfix: the candidate combo uses `SelectionChangeCommitted` plus a bounded programmatic binding guard, so bind/restore invokes selection logic 0 times, one operator combat commit invokes it once with maximum depth 1, combat survives session/action/checkpoint/replay/matrix refreshes, and candidate changes clear prior live state. Its 4/4 Runtime matrix and Unity smoke remain GREEN; Goal145 is accepted by the later Goal146 human handoff. |
| `docs/agent-tasks/goal-146-featuremodule-composition-workbench-and-novel-gamepackage-runtime-qualification-matrix/GOAL.md` | Goal 146 task: records Goal145 human acceptance, composes eight novel GamePackages from required and optional FeatureModules with deterministic mutation planning, qualifies each through the shared Runtime session/replay seam, adds an in-process WinForms module composer and a read-only Unity matrix consumer. Goal146 is accepted by the exact Goals146/147 human decision recorded with Goal148. |
| `docs/agent-tasks/goal-146a-generic-featuremodule-composer-scalability-and-catalog-driven-coverage-hotfix/GOAL.md` | Goal 146A hotfix: removes the fixed eight-row table and optional-module indexing, derives active modules and labels from the catalog, adds generic runtime-effect contracts, exhaustive three-module coverage and bounded deterministic 4/12-module coverage. The synthetic fourth module materializes and passes the shared Runtime qualifier without a Composer branch; all current Goal146 package/final hashes remain unchanged. |
| `docs/agent-tasks/goal-147-persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification/GOAL.md` | Goal 147 task: repository-local file catalog with 10 locked core and 3 optional FeatureModules, 8 typed parameters, atomic saved-composition persistence, fingerprinted incremental per-module certification, 100-module certification scalability with 9 bounded interaction rows, custom Runtime qualification, in-process WinForms authoring and read-only Unity evidence. Result is `GREEN`; Goals146/147 are accepted by the exact human decision recorded with Goal148. |
| `docs/agent-tasks/goal-147a-authoring-ui-event-lifecycle-and-dependent-module-certification-hotfix/GOAL.md` | Goal 147A P1 hotfix: real STA WinForms lifecycle uses synchronous post-event ItemCheck state with 0 programmatic and 1 operator apply, heavy materialize/qualify actions run off the UI thread, and certification uses deterministic transitive optional dependency closure with selective 2 executed / 1 reused invalidation and pre-Runtime cycle rejection. |
| `docs/agent-tasks/goal-148-unified-game-project-workspace-and-legacy-goal-diagnostics-isolation/GOAL.md` | Goal 148 task: records Goals146/147 human acceptance, evolves the existing `Игры` page into the five-section project workflow with project-local FeatureModule authoring, transactional Runtime-qualified package activation and rollback, and moves legacy numbered panels behind an explicit toggle on `Диагностика генератора`. Current gate=`unified_game_project_workspace_and_legacy_goal_diagnostics_isolation_verification required`; result is `GREEN`, Goal148 accepted=false, normalWorkspaceGoalNumberControlCount=0, custom package/final hashes preserved and nextProductGoal=`review_goal_148_unified_game_project_workspace`. |
| `docs/agent-tasks/goal-148a-new-project-required-support-files-and-transactional-activation-hotfix/GOAL.md` | Goal 148A P1 hotfix: production-created projects derive package-required relative script files through a generic confined plan, validate a project-local staged package, copy or reuse files transactionally, reject differing user files and missing sources, and remove new files on rollback. Result is `GREEN`; Goal148 remains accepted=false and nextProductGoal remains its human review. |
| `docs/agent-tasks/goal-148b-current-package-ui-thread-dispatch-and-real-workspace-build-retry-hotfix/GOAL.md` | Goal 148B P1 hotfix: records the real `_navigation` cross-thread manual failure, dispatches all five WinForms `CurrentChanged` subscribers through named disposal-safe UI handlers, observes/coalesces async page refreshes and proves the production New Game + Projects + MainForm build retry with preserved hashes. Result is `GREEN`; Goal148 remains accepted=false, manualRetryRequired=true and nextProductGoal is the Goal148 manual retry. |
| `docs/agent-tasks/goal-148c-project-identity-preservation-and-project-scoped-composition-hotfix/GOAL.md` | Goal 148C P1 hotfix: records the real template-identity overwrite, adds atomic project identity capture/recovery, migrates the fixed Goal147 authoring file to a deterministic project-scoped composition document, overlays identity before activation, separates composition/activated/final hashes and refreshes MainForm title on every marshalled current-package event. Result is `GREEN`; manual values preserve composition SHA `e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221` and final hash `95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8`; Goal148 remains accepted=false and requires a manual retry. |
| `docs/manual-acceptance/persistent-featuremodule-registry-typed-parameter-authoring-saved-compositions-and-incremental-certification.md` | Exact Goals146/147 human acceptance record plus the completed bundled authoring review evidence. |
| `docs/manual-acceptance/unified-game-project-workspace-and-legacy-goal-diagnostics-isolation.md` | Goal148 manual checklist for the normal `Игры` workflow, project-local roundtrip, transactional build and explicit diagnostics toggle. |
| `docs/ROADMAP_FINAL_REBASELINE.md` | Goal 097 final roadmap rebaseline after Goals 074-096, including current position, milestone ladder, estimates, end-to-end progress rule, deferrals and kill criteria. |
| `docs/context/DREAM_SCOPE_REGISTER.md` | Goal 097 dream-scope register covering fantasy/Heroes-like, sci-fi, Space-Rangers-like, visual/media compiler, adult/rating, realism/geospatial, self-generated realism and release/export tracks. |
| `docs/context/REALISM_GEOWORLD_SIMULATOR_TRACK.md` | Goal 097 future planning track for optional real-world/geospatial ingestion and fully self-generated realism simulation; no implementation authority. |
| `docs/RELEASE_RISK_REGISTER.md` | Goal 097 P0/P1/P2/P3 release risk register and release gate plan. |
| `docs/MILESTONE_GATES.md` | Goal 097 acceptance gate definitions for Vertical Slice Final, Strong Alpha, v1 Full Final and Dream Full Final. |
| `docs/GOAL_PRODUCTIVITY_POLICY.md` | Goal 097 aggressive goal productivity policy for larger composite goals, visible progress cadence and Goal089 tiered validation usage. |
| `docs/context/LFZ_ARCHIVE_ANALYSIS_MANIFEST.md` | Goal 098 lineage doc: LFZ pattern study manifest and rule that Codex must not read/copy LFZ archive code. |
| `docs/context/LFZ_GEOWORLD_PATTERN_STUDY.md` | Goal 098 lineage doc: geoworld source to tile/cache/provenance to normalized features to WorldSourceGraph to visual/runtime projection pattern. |
| `docs/context/GEOWORLD_RUNTIME_STREAMING_DESIGN_NOTES.md` | Goal 098 lineage doc: future runtime geospatial stream window, boundary prefetch and contract-only runtime streaming notes. |
| `docs/context/GEOWORLD_SOURCE_ADAPTER_ARCHITECTURE.md` | Goal 098 lineage doc: future source adapter, license/provenance/cache and normalized geofeature contract architecture. |
| `docs/proposals/GEOWORLD_INGESTION_FUTURE_GOAL_SEQUENCE.md` | Goal 098 future sequence: geoworld adapter/normalization/graph/stream scheduler before offline import, projection, Unity handoff or legal/provider gates. |
| `docs/VALIDATION_PIPELINE.md` | Goal 089 validation policy: `check-current-goal.ps1` is the ordinary feature-goal default, `check-spine-fast.ps1` is the medium visual/world/gameplay spine route, and full `check-all.ps1` / `check-all-observed.ps1` is reserved for consolidation, milestone and shared/core-risk work. |
| `docs/context/VISUAL_ADULT_LAYER_CONTEXT_INDEX.md` | Goal 083 visual/adult routing index: source docs, architecture rules, rating boundary, safe fallback rules, provider quarantine/promotion and stop conditions for future visual/media tasks. |
| `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md` | Goal 083 future visual/media roadmap: bounded stages from visual asset contract/rating metadata through approved asset consumption, keeping providers editor-side and Runtime/Unity provider-free. |
| `docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md` | Goal 085 implementation-oriented synthesis of the deepsearch visual stack: immediate optional adapters, prototype candidates, rejected/deferred tools and non-negotiable design requirements. |
| `docs/deepsearch/01_PROCEDURAL_VISUAL_SYNTHESIS_CORE_AND_PART_PACKS.md` | Deepsearch source: visual synthesis core, part packs, metadata contracts, adapter boundaries and provenance/review direction. |
| `docs/deepsearch/02_TILE_BIOME_WATER_WORLD_MAP_GENERATION.md` | Deepsearch source: tile, biome, water, coast, river, lake, marsh and large-map generation constraints. |
| `docs/deepsearch/03_PSEUDO3D_FIRST_PERSON_FROM_2D_ASSETS.md` | Deepsearch source: pseudo-3D and first-person presentation from 2D assets through sidecar contracts. |
| `docs/deepsearch/04_CREATURE_NPC_APPEARANCE_BODYPLAN_PAPERDOLL.md` | Deepsearch source: creature/NPC body-plan grammar, equipment sockets, paperdoll layering and 100+ species scalability. |
| `docs/deepsearch/05_SETTLEMENTS_CITIES_CARAVANS_LIVING_WORLD_VISUALS.md` | Deepsearch source: settlement, city, caravan, facade and living-world visual layout contracts. |
| `docs/deepsearch/06_UI_THEMES_EFFECTS_WEATHER_DAYNIGHT_VFX.md` | Deepsearch source: UI themes, icon/effect profiles, weather and day-night visual layers. |
| `docs/deepsearch/07_MEDIA_PIPELINE_PROVIDER_QUARANTINE_PROVENANCE_RATING_ADULT.md` | Deepsearch source: editor-only provider quarantine, provenance, rating, export policy and adult metadata boundary. |
| `docs/deepsearch/08_EXISTING_LIBRARIES_AND_TOOLS_SCOUTING.md` | Deepsearch source: optional library/tool scouting, immediate adapters, prototype candidates and rejected/deferred dependencies. |
| `docs/context/ADULT_VISUAL_LAYER_DOCUMENTATION_MANIFEST.md` | Manifest for the adult-capable visual composition docs; read after the Goal 083 context index when shaping adult-capable visual tasks. |
| `docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md` | Visual world generation context: GamePackage/manifests remain source of truth, visual recipes are deterministic, and Runtime/Unity do not call LLM/media providers. |
| `docs/proposals/PROCEDURAL_VISUAL_DETAIL_GENERATOR_STRATEGY.md` | Strategy for implementing a generator/validator instead of dumping thousands of visual detail records or media assets. |
| `docs/proposals/VISUAL_WORLD_GRAMMAR_AND_PSEUDO3D_GENERATION.md` | VisualRuleStack to VisualRecipe architecture for pseudo-3D presentation; early work starts with a resolver, not providers or Unity-specific logic. |
| `docs/proposals/VISUAL_RULE_STACK_AND_DOMAIN_PROFILES.md` | Domain/profile influence model for visual generation; domains are weighted visual influences, not one-off generators. |
| `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md` | Procedural VisualPartPack proposal for reusable parts, palettes, layers, recipes and deterministic outputs; adult capability is a rating-gated extension. |
| `docs/proposals/PSEUDO3D_ASSET_PRESENTATION_CONTRACTS.md` | Sidecar contract planning for using 2D visual recipe outputs in pseudo-3D presentation with pivots, scale, collision, sorting and fallbacks. |
| `docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md` | Creature Visual Genome proposal for species/character visual identity, body-plan compatibility, presentation slots and adult-capable metadata. |
| `docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md` | Adult visual strategy proposal: adult-capable visuals are rating-gated slots/overlays inside the composable visual pipeline, not a separate generator. |
| `docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md` | Adult-capable VisualPartPack extension proposal with rating/export policy, compatibility constraints, safe fallback requirements and validation diagnostics. |
| `docs/MODULE_CONTRACT_MANIFEST_V1.md` | Goal 029 contract for deterministic repository-local module manifests, ownership roots, dependencies, validators, test filters, scenario ids, forbidden runtime dependencies and hash rules. |
| `docs/PRODUCT_SMOKE_SCENARIO_MANIFEST_V1.md` | Goal 029 contract for manifest-driven product-smoke scenarios before hardcoded fallback routing. |
| `docs/PARALLEL_CANDIDATE_DEVELOPMENT_POLICY.md` | Goal 029 policy for candidate-only parallel work, one active state writer, serial adoption and Tier 1-4 verification. |
| `docs/candidates/README.md` | Docs-only index for adopted parallel candidate scouting reports; reference material only, not a product gate or dependency adoption. |
| `docs/candidates/candidate_dialogue_narrative_tooling_v1/CANDIDATE_CONTRACT_NOTE.md` | Candidate-only dialogue/narrative tooling contract boundary for possible future Ink/Yarn Spinner inspired editor-time adapters. |
| `docs/candidates/candidate_dialogue_narrative_tooling_v1/DIALOGUE_NARRATIVE_IR_CONTRACT_V1.md` | Candidate-owned dialogue/narrative IR and localization roundtrip proof adopted from lane-a; BCL-only and not an accepted product gate. |
| `docs/candidates/candidate_dialogue_narrative_tooling_v1/EXTERNAL_TECHNOLOGY_SCOUTING.md` | External technology scouting for dialogue/narrative tooling candidates; no direct dependency accepted. |
| `docs/candidates/candidate_world_biome_noise_v1/WORLD_BIOME_NOISE_CONTRACT.md` | Candidate-only world/biome/noise contract boundary for deterministic seeded sampling and fallback behavior. |
| `docs/candidates/candidate_world_biome_noise_v1/EXTERNAL_TECHNOLOGY_SCOUTING.md` | External technology scouting for world/biome/noise candidates; FastNoise Lite remains a future adapter candidate, not an accepted dependency. |
| `docs/candidates/candidate_semantic_catalog_v1/CANDIDATE_CONTRACT.md` | Candidate-only semantic catalog boundary for offline/editor-time lexical relations, tags, provenance and reviewed imports. |
| `docs/candidates/semantic-catalog/CANDIDATE_SEMANTIC_CATALOG_QUALITY_ANALYZER_V1.md` | Candidate-owned semantic catalog quality analyzer proof adopted from lane-c; deterministic in-memory analysis only. |
| `docs/candidates/candidate_semantic_catalog_v1/EXTERNAL_TECHNOLOGY_SCOUTING.md` | External technology scouting for semantic catalog sources; live API/RAG/dataset dependency remains rejected/deferred. |
| `docs/candidates/candidate_navigation_pathfinding_v1/NAVIGATION_PATHFINDING_CONTRACT_NOTE.md` | Candidate-only navigation/pathfinding contract boundary for deterministic graph/grid queries and future navmesh adapters. |
| `docs/candidates/lane-d/CANDIDATE_NAVIGATION_ROUTE_GRAPH_V1.md` | Candidate-owned navigation route graph proof adopted from lane-d; deterministic route graph planning only. |
| `docs/candidates/candidate_navigation_pathfinding_v1/EXTERNAL_TECHNOLOGY_SCOUTING.md` | External technology scouting for pathfinding/navmesh candidates; no native/.NET/Unity navigation dependency accepted. |
| `docs/LLMGameCreator_FEATURE_BACKLOG_AUDIT.md` | Backlog/audit reference for wanted capabilities; not current gate authority and not an implementation plan. |
| `docs/PACKAGE_ASSEMBLY_EXPANSION_CAMPAIGN_PACK.md` | Plan-only package assembly campaign pack for the next 3-5 bounded composite goals; does not start Goal 025 or S199. |
| `docs/GAME_PROFILE_CONTRACT_V1.md` | Goal 021 profile contract doc for deterministic game-family, presentation, topology, actor model, loop, scale, asset policy and runtime/export target selection. |
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
review_goals_146_147_featuremodule_composer_authoring_workflow
```

Goal146A is GREEN at
`generic_featuremodule_composer_scalability_and_catalog_driven_coverage_hotfix_verification`.
The active optional set, composition IDs, display names,
coverage rows and Runtime effect observations are catalog-driven. The current
three-module fixture uses `exhaustive_small_catalog` with 8 rows and preserves
all eight Goal146 package/final hashes. A synthetic fourth module uses
`bounded_interaction_coverage` with 13 rows, materializes and passes the shared
Runtime qualifier/checkpoint/full-replay/action-binding path without a Composer
branch. A deterministic twelve-module catalog produces 21 rows under the
24-row policy limit instead of enumerating 4096. Goal146 remains
`accepted=false` with `manualReviewDeferred=true`.

Goal147 is GREEN at
`persistent_featuremodule_registry_typed_parameter_authoring_saved_compositions_and_incremental_certification_verification required`.
The file-based catalog is authoritative, typed parameters bind generically,
saved compositions round-trip atomically with staleness diagnostics, and
fingerprinted per-module certification reuses unchanged results. The current
three optional modules are certified independently from interaction coverage;
a 100-module catalog produces 100 certification entries and only 9 interaction
rows under the 24-row cap without powerset enumeration. Defaults preserve all
eight Goal146 hashes, while a custom all-three composition passes the same
Runtime/checkpoint/replay/action-binding qualifier. Goals 146 and 147 remain
`accepted=false`; their combined workflow is the current manual review.

Goal147A is GREEN. Programmatic Goal147 checked-list binding applies zero
selection callbacks, one operator check change applies once from post-event UI
state, Refresh/Delete rebinds are safe with no document, and heavy primary
actions run off the UI thread while the UI message pump remains responsive.
Dependent-module certification now includes sorted transitive optional closure;
changing the synthetic base executes base plus dependent while reusing the
unrelated entry (2/1), and cycles are rejected before Runtime execution. The
recommended next work remains the bundled Goals 146/147 review.

Goal146 is GREEN with 10 locked core modules, three Goal142-derived optional
profile modules and all eight module combinations materialized as novel
GamePackages. All 8/8 validate and pass the shared Runtime qualifier with
distinct package/final hashes, 8-action checkpoint reload, 13-action full replay,
exact action binding and order-independent package bytes. The selected all-three
composition shows all three fresh semantic dimensions. WinForms uses in-process
services and Unity is read-only. Goal146 remains `accepted=false` with
`manualReviewDeferred=true` at
`featuremodule_composition_workbench_and_novel_gamepackage_runtime_qualification_matrix_verification required`;
a future review should be bundled with related authoring or persistence work.

Goal145 is accepted by the repository owner's exact Goal146 human handoff with
`accepted=true`, `acceptedByHuman=true`, `acceptedByCodex=false`,
`rawManualInputNotCommitted=true`, `goal144Accepted=true`, `candidateCount=4`,
`passedCandidateCount=4`, `runtimeEvaluatedCandidateCount=4`,
`runtimeMutatedCandidateCount=3`, `controlCandidateCount=1`,
`distinctFinalStateHashCount=4`, `allCandidateCheckpointReloadsPassed=true`,
`allCandidateFullReplaysEquivalent=true`,
`allCandidateActionBindingsPassed=true`, `allFocusEffectsObserved=true`,
`operatorSelectableCandidateCount=4`,
`activeSelectedCandidateId=minimal-map-game-exploration-resource-focus`,
`crossCandidateCheckpointRejected=true`, `runtimeAuthority=true`,
`projectionOnly=false`, `unityGameplayTruth=false` and
`manualUnityOptional=true`.

Goal144 is accepted by the repository owner's exact Goal145 human handoff.
Goal145 discovers every candidate from Goal142 artifacts, validates package
metadata/path/SHA, runs the same Runtime session kernel and canonical action
plan for each candidate, proves fresh semantic focus effects, checkpoint reload
and full replay, exposes an in-process selectable WinForms session, and gives
Unity a read-only Goal145 matrix consumer. Runtime remains gameplay truth.

The Goal131 selected candidate now has
package validation, canonical runtime transcript/state summary, save/load/replay
proof, PlayerAdapter contract, 13-step player-loop plan, 13 Runtime-owned
player commands, one snapshot per command, 13 Unity/player playback frames,
Goal137 human acceptance, a runtime-backed stepper/HUD model, Unity stepper smoke
and Goal138 human acceptance, plus a runtime-backed interactive controls model,
script, session, Unity controls smoke and one-click report, Goal139 human
acceptance, controls UX polish evidence, bounded Unity editor noise
classification, Goal140 human acceptance and a Runtime-owned command roundtrip
request/result/snapshot bridge consumed by Unity/player.

Do not start Goal148 without a separate task. Keep sample mutation,
`.llmgc/manual/**`, provider/network, public schema, Lua, generator-library,
final art/gameplay, Unity scene/prefab/settings/packages/StreamingAssets and
release packaging out of scope from this handoff.

Goal 033 semantic authoring intent resolver has been accepted by the user's manual decision:
`semantic_authoring_intent_resolver_verification passed`. Goal 034 strict LLM draft artifact loop
has been accepted by the user's manual decision: `strict_llm_draft_artifact_loop_verification passed`.
Goal 031 and Goal 032 remain produced-for-review without being marked passed. Goal 035 Lua module
manifest registry has been accepted by the user's manual decision: `lua_module_manifest_registry_verification passed`.
Goal 036 Lua sandbox execution gate has been accepted by the user handoff embedded in Goal 037:
`lua_sandbox_execution_gate_verification passed`. Goal 037 hybrid LLM draft plus Lua deterministic
expansion has been accepted by the user handoff before Goal 038:
`hybrid_llm_draft_lua_deterministic_expansion_verification passed`. Goal 038 world-scale region map
foundation has been accepted by the user handoff before Goal 039:
`world_scale_region_map_foundation_verification passed`. Goal 039 runtime chunk delta traversal smoke
has been accepted by the user handoff before Goal 040: `runtime_chunk_delta_traversal_smoke_verification passed`.
Goal 040 chunked runtime preview/export multi-family smoke has been accepted by the user handoff before
Goal 043: `chunked_runtime_preview_export_multifamily_smoke_verification passed`. Goal 043 multi-family
generated template vertical slice has been accepted by the Goal 047 user handoff:
`multi_family_generated_template_vertical_slice_verification passed`. Goal 047 full generator without media
dry-run has been accepted by the Goal 053 user handoff: `full_generator_without_media_verification passed`.
Goal 053 media asset campaign orchestration has been accepted by the Goal 054 user handoff:
`media_asset_campaign_orchestration_verification passed`. Goal 054 media materialization review package
has been accepted by the Goal 055 preflight user handoff: `media_materialization_review_package_verification passed`.
Goal 055 media-bound playable review package has been accepted by the Goal 056 user handoff:
`media_bound_playable_review_package_verification passed`. Goal 056 Unity Alpha media-bound playable package
has been accepted by the Goal 057 user handoff: `unity_alpha_media_bound_playable_package_verification passed`.
Goal 057 Unity Alpha multi-family playable loop has been accepted by the Goal 058 user handoff:
`unity_alpha_multifamily_playable_loop_verification passed`. Goal 058 Full Media-Bound Generator Campaign
has been accepted by the Goal 059 user handoff: `full_media_bound_generator_campaign_verification passed`.
Goal 059 Full Generator Variability Regression Matrix has been accepted by the Goal 060 user handoff:
`full_generator_variability_regression_matrix_verification passed`. Goal 060 Full Campaign GamePackage
Materialization Matrix has been accepted by the Goal 061 user handoff:
`full_campaign_gamepackage_materialization_matrix_verification passed`. Goal 061 Full Campaign Playable
Review Package RC has been accepted by the Goal 062 user handoff:
`full_campaign_playable_review_package_rc_verification passed before Goal 062`. Goal 062 Constrained Spatial
Detail Generation has been accepted by the Goal 063 user handoff:
`constrained_spatial_detail_generation_verification passed before Goal 063`. Goal 063 Gameplay Consequence
Depth Matrix has been accepted by the Goal 064 user handoff:
`gameplay_consequence_depth_matrix_verification passed before Goal 064`. Goal 064 Living World NPC/Faction
Simulation Matrix has been accepted by the Goal 065 user handoff:
`living_world_npc_faction_simulation_matrix_verification passed before Goal 065`. Goal 065 Interlocked
Gameplay Systems Depth Matrix has been accepted by the Goal 066 user handoff:
`interlocked_gameplay_systems_depth_matrix_verification passed before Goal 066`. Goal 066 Settlement
Construction Destruction Production Matrix has been accepted by the Goal 067 user handoff:
`settlement_construction_destruction_production_matrix_verification passed before Goal 067`. Goal 067
Programmatic Narrative Quest Dialogue Event Matrix has been accepted by the Goal 068 user handoff:
`programmatic_narrative_quest_dialogue_event_matrix_verification passed before Goal 068`. Goal 068 Combat
Magic Ability Boss Encounter Matrix has been accepted by the Goal 069 user handoff:
`combat_magic_ability_boss_encounter_matrix_verification passed before Goal 069`. Goal 069 World Event
Weather Day/Night Crisis Matrix has been accepted by the Goal 070 user handoff:
`world_event_weather_daynight_crisis_matrix_verification passed before Goal 070`. Goal 070 Integrated
Campaign Timeline Simulation Matrix has been accepted by the Goal 071 user handoff:
`integrated_campaign_timeline_simulation_matrix_verification passed before Goal 071`. Goal 071 Unity Alpha
Interactive Campaign Player has been accepted by the Goal 072 user handoff:
`unity_alpha_interactive_campaign_player_verification passed before Goal 072`. Goal 073 Source Format P0
Readability Repair has been accepted by the Goal 074 user handoff:
`source_format_p0_readability_repair_verification passed before Goal 074`. Goal 074 Schema-Driven Campaign
Authoring And Review Workspace has been accepted by the Goal 075 user handoff:
`schema_driven_campaign_authoring_review_workspace_verification passed before Goal 075`. Goal 075
Schema-Driven Campaign Edit/Validate/Apply Loop has been accepted by the Goal 076 user handoff:
`schema_driven_campaign_edit_validate_apply_loop_verification passed before Goal 076`.

Goal 069 implementation status is GREEN with Unity/player proof passed, `unityExitCode=0`,
`playerExitCode=0`, `provenRowCount=9`, `rowCount=9`, `stateChangingRowCount=9`,
report hash `40db9e42153efda4427f587873cd1cc75af4687fd0775cf429aa88430c59e63e`,
all required world_event markers matched and accepted by Goal 070 handoff. Goal 070 implementation
status is GREEN with Unity/player proof passed, `unityExitCode=0`, `playerExitCode=0`,
`provenRowCount=9`, `rowCount=9`, `stateChangingRowCount=9`, `cascadeCount=27`,
`arbitrationCount=9`, report hash `5db771792666d24cc334b9203fc8e5a6f7970f648f339f58d139377a3506aa89`,
all required campaign_timeline markers matched and accepted by Goal 071 handoff. Goal 071 implementation
status is GREEN and accepted by Goal 072 user handoff: Unity/player proof passed with
`unityExitCode=0`, `playerExitCode=0`, `provenRowCount=9`, `rowCount=9`, `stateChangingRowCount=9`,
`actionCount=63`, `transitionCount=63`, all required interactive_campaign markers matched and
report hash `ca0828e5da1ff8d08b6b6e0574bfe27568d7acef1447ec30f47ede0581d42d02`. Goal 072 is produced
for review with `generator_spine_quality_consolidation_verification required`, `accepted=false`,
`implementationStatus=BLOCKED`, `p0Count=1`, `p1Count=3`, `p2Count=2`, `p3Count=0`,
inventory hash `7873d38c2a4fdc1513ed7b373f1b9d3c21be16427bee22d9c6b6ca91f97de1a1` and
debt register hash `b94738de198d2a479c6cd0038d8911620e1335f285769985a6d301c489095d33`.
Goal 073 is accepted by Goal 074 user handoff:
`source_format_p0_readability_repair_verification passed before Goal 074`; it repaired the Goal 072
P0 source-format blocker without marking Goal 072 passed. Goal 074 produced review evidence with
`schema_driven_campaign_authoring_review_workspace_verification required`, `accepted=false`,
`implementationStatus=GREEN`, `rowCount=9`, `schemaGroupCount=13` and deterministic hash
`5b7919a92ac6354b47e0fb1f0682cb74619ca48572f5892cfa509add8803d823`; the hotfix quality guard scans
26 Goal 074 C# files including `CompositionRoot.cs` with `linesOver500Count=0`,
`minifiedSourceFileCount=0` and `filesWithTooFewLinesForSizeCount=0`. Goal 074 is accepted by the
Goal 075 user handoff: `schema_driven_campaign_authoring_review_workspace_verification passed before Goal 075`.
Goal 075 is accepted by the Goal 076 user handoff:
`schema_driven_campaign_edit_validate_apply_loop_verification passed before Goal 076`. Goal 075 evidence has
`implementationStatus=GREEN`, `rowCount=9`, `editableFieldCount=6`,
`candidateCount=18`, `appliedChangeCount=18`, `rollbackCount=9`, `invalidScenarioCount=16` and
deterministic hash `9d68591603cbb108cf6b80e47773bfeb6ce44c85f7cf4722936c9aee55a8cada`. Goal 076 is produced
for review with `edit_driven_playable_preview_refresh_verification required`, `accepted=false`,
`implementationStatus=GREEN`, `changedRowCount=9`, `appliedChangeCount=18` and `packageTargetCount=18`.
Goal 076 is accepted by the Goal 077 user handoff:
`edit_driven_playable_preview_refresh_verification passed before Goal 077`.

Goal 077 is produced for review with `edit_driven_review_package_materialization_verification required`,
`accepted=false`, `implementationStatus=GREEN`, 9 rows, 18 materialized targets, 21 review package files
and report hash `ae839969a04572fc330804f531de90e422025c2f1d0ad037084544e4ba7afbaf`. It consumes the real
Goal 076 artifacts from disk into `.llmgc/procedural/goal-077-edit-driven-review-package-materialization/`,
writes a disk-backed `review-package/**`, validates staged package reads and rejects missing/tampered package
files and broken player-index references without Unity/schema/runtime/provider changes.

Goal 077 is accepted by the Goal 078 user handoff:
`edit_driven_review_package_materialization_verification passed before Goal 078`.

Goal 078 is produced for review with `edit_driven_review_package_playable_session_verification required`,
`accepted=false`, `implementationStatus=GREEN`, 9 rows, 18 targets, 57 deterministic playable-session
actions and report hash `2ce9a56f3a868790d9c9a4ba82debc0cf862ad7b56d9236a50b6537a41e6479f`. It consumes the
real Goal 077 disk-backed review package, validates the report/ledger/manifest/index/player-readable index and all
target payload hashes from disk, proves save/replay determinism with state-chain hashes, rejects missing/tampered/
illegal/fake replay paths and binds a bounded WinForms play session tab without Unity/schema/runtime/provider changes.

Goal 078 is accepted by the Goal 079 user handoff:
`edit_driven_review_package_playable_session_verification passed before Goal 079`.

Goal 079 is produced for review with `edit_driven_spine_quality_consolidation_verification required`,
`accepted=false`, `implementationStatus=GREEN`, 5 chain items, zero P0/P1 blockers, P2/P3 debt counts 8/2
and report hash `3845b0f699ed44b618638bb3e21871fda083551a6d7ad8bdca8ba0e62bbbb8eb`. It consumes the real
Goal 074-078 reports, quality gates, Goal 078 package read/replay/negative proof and current workspace bindings
into a deterministic Application consolidation seam and a bounded WinForms dashboard tab without Unity/schema/runtime/
provider/Lua changes.

Goal 079A is produced for review with `source_format_line_ending_guard_verification required`,
`accepted=false`, `implementationStatus=GREEN`, zero CR-only/no-LF source files after scan, raw/logical max line
length 251 and synthetic CR-only plus zero-LF one-physical-line guard tests passing. It updates Goal 079 evidence
with explicit raw-byte source-health metrics and does not mark Goal 079 accepted.

Goal 079 is accepted for continuation before Goal 080:
`edit_driven_spine_quality_consolidation_verification accepted for continuation before Goal 080`.
Goal 079A source-format guard is accepted before Goal 080:
`source_format_line_ending_guard_verification passed before Goal 080`.

Goal 080 is produced for review with `edit_driven_gamepackage_runtime_preview_bridge_verification required`,
`accepted=false`, `implementationStatus=GREEN`, 9 rows, 18 targets, 57 actions and a 5-file projected
GamePackage package. It consumes the real Goal 077 review package, Goal 078 playable-session proof and Goal
079/079A quality evidence, writes `.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/projected-gamepackage/**`,
reads the projected package back from disk, validates it through existing GamePackage/runtime-preview paths,
rejects missing/tampered/fake/lineage mismatches and binds a bounded WinForms Runtime Bridge tab. Projected package
hash: `d79b6d12b384f32f7c5184e02a47e0c906513dd2f6c8bdb743090e02edffa648`; runtime-preview bridge proof hash:
`1287782882f1050a7c622b913e498a45afdb9a9b2190e036deb212b0b9b60d2b`.

Goal 080 is accepted by Goal 081 handoff:
`edit_driven_gamepackage_runtime_preview_bridge_verification passed before Goal 081`.

Goal 081 is produced for review with `edit_driven_gamepackage_runtime_preview_playthrough_verification required`,
`accepted=false`, `implementationStatus=GREEN`, 9 rows, 18 targets, 57 actions and 124 commands. It consumes the real Goal 080
projected GamePackage from disk, validates it through existing package/runtime-preview services, builds a deterministic
player command script, replays it into transcript/state-hash-chain evidence, rejects missing/tampered/nonexistent-target/
replay-order/fake-read/lineage scenarios and keeps public schema, Runtime, Unity, providers, Lua, generator-library,
solution and project files unchanged. Report hash:
`1d46aa15e9f22f57df316d5197ad40866e269334201f3508961a8753c2f9c401`; command script hash:
`74103281b47544d2c30ddd95166b5a1bf19039cfd93c2c519f0337935f928ebf`.

Goal 081 is accepted by Goal 082 handoff:
`edit_driven_gamepackage_runtime_preview_playthrough_verification passed before Goal 082`.

Goal 082 is produced for review with `edit_driven_unity_alpha_streamingassets_handoff_verification required`,
`accepted=false`, `implementationStatus=GREEN`, 9 rows, 18 targets, 57 actions, 124 commands and 6 mirrored
StreamingAssets payload files. It consumes the real Goal 080 projected GamePackage and Goal 081 playthrough
artifacts, mirrors a compact payload into `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/EditDrivenGoal082/`,
validates exact mirrored reads and negative tamper/missing/fake-success cases, adds one independent Unity probe script
without touching `AlphaRuntimeBootstrap.cs`, and binds a bounded WinForms Unity Handoff tab. Handoff manifest hash:
`08104cd28fac6501d8cd9e4c8329e11ef56b82c17a1b99ea55a4b733d8782a54`; probe read proof hash:
`18ac321d2244a21051a8e9b632904361234018f3d4161267813a5acf76acfa16`.

Goal 082A is produced for review with `source_format_physical_line_repair_verification required`,
`accepted=false`, `implementationStatus=GREEN`. Direct raw-byte preflight found zero current malformed Goal 082
C# source files, so no source normalization was required. The Goal 082 scanner now records rawByteScannedFileCount,
zero-LF, CR-only, one-physical-line, raw/logical max line length and explicit Unity probe / WinForms parent /
Application seam coverage booleans, rejects synthetic CR-only and zero-LF one-physical-line samples, and keeps Goal
082 accepted=false. The `21f2525a adult docs` commit is docs context only.

Goal 083 is produced for review with `visual_adult_layer_context_integration_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it indexes/routes the visual/adult docs as policy-bounded context and
does not start provider/media/runtime/schema implementation. Goal 084 is produced for review with
`visual_asset_contract_rating_metadata_verification required`, `accepted=false`, `implementationStatus=GREEN`; it adds
a BCL-only Application metadata contract/validator, focused tests, product smoke and compact metadata-only evidence
under `.llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/`. Goal 084 is accepted by the Goal 085 handoff:
`visual_asset_contract_rating_metadata_verification passed before Goal 085`, without rewriting Goal 084 artifacts.
Goal 085 is produced for review with `visual_part_pack_rule_stack_verification required`, `accepted=false`,
`implementationStatus=GREEN`; it consumes all eight deepsearch docs into a BCL-only Application visual part-pack
rule-stack contract/validator/evidence seam under `.llmgc/procedural/goal-085-deepsearch-backed-visual-part-pack-rule-stack/`.
Goal 086 is accepted for continuation before Goal 087:
`deterministic_visual_microtile_materializer_verification accepted for continuation before Goal 087`, without rewriting
Goal 086 artifact accepted=false evidence. Goal 087 is accepted for continuation before Goal 088:
`deterministic_visual_map_patch_composer_verification accepted for continuation before Goal 088`, without rewriting
Goal 087 artifact accepted=false evidence. Goal 088 is produced for review with
`deterministic_visual_region_composer_verification required`, `accepted=false`, `implementationStatus=GREEN`; it consumes
Goal 084/085/086/087 visual lineage into a BCL-only Application deterministic visual region composer, writes compact
144x144 surface plus 144x144 underground region definition/placement/chunk/proof evidence and text SVG overviews under
`.llmgc/procedural/goal-088-deterministic-visual-region-composer/`, and does not start provider/media/runtime/schema
implementation. Goal 085, Goal 083, Goal 082 and Goal 082A remain `accepted=false`.
Goal 088A is produced for review with `goal_088_check_all_validation_repair_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it proves the full `.devflow/scripts/check-all.ps1` route passed in
`.devflow/runs/20260703_075027-check-all` with 1235/1235 non-product tests and 0 warnings, classifies the prior blocker
as a wrapper-timeout/slow-suite issue rather than a Goal 088 hang, restores 79 historical validation side-effect paths,
and leaves Goal 088 artifacts `accepted=false`.
Goal 089 is produced for review with `tiered_validation_pipeline_verification required`, `accepted=false`,
`implementationStatus=GREEN`; it adds tiered validation wrappers, a validation profile, policy docs and compact evidence
without weakening full `check-all.ps1`, without asking the user to manually run check-all, and without marking Goal 088A
or Goal 088 accepted.
Goal 090 is produced for review with `parameterized_visual_world_profiles_verification required`, `accepted=false`,
`implementationStatus=GREEN`; it adds a BCL-only Application profile/addressing seam with four metadata-only fixtures,
six arbitrary finite size samples, sparse `100000x100000` finite proof, infinite stream-window proof, deterministic chunk
keys, 18 rejected negative scenarios and compact text SVG overviews under
`.llmgc/procedural/goal-090-parameterized-visual-world-profiles/`, without Runtime/Unity/provider/schema/project/dependency
changes and without marking Goal 088, Goal 088A or Goal 089 accepted.
Goal 091 is produced for review with `deterministic_visual_chunk_stream_window_verification required`, `accepted=false`,
`implementationStatus=GREEN`; it consumes Goal 090 profiles into a BCL-only Application stream-window materializer,
proves `255x257` finite boundary clipping, huge sparse `100000x100000` compact far windows, two overlapping infinite
stream centers with 24 reused chunk keys, seam continuity, cache reuse, data-driven surface/underground/underwater layer
transitions, 16 rejected negative scenarios and compact text SVG overviews under
`.llmgc/procedural/goal-091-deterministic-visual-chunk-stream-window/`, without Runtime/Unity/provider/schema/project/
dependency changes and without marking Goal 090 accepted.
Goal 092 is produced for review with `visual_world_stream_preview_workspace_verification required`, `accepted=false`,
`implementationStatus=GREEN`; it consumes real Goal 086-091 disk artifacts into a BCL-only Application workspace catalog
and a separate WinForms preview page, exposes five artifact groups, 38 text SVG preview entries, seven Goal 091 proof
statuses and a binding inventory under `.llmgc/procedural/goal-092-visual-world-stream-preview-workspace/`, without
Runtime/Unity/provider/schema/project/dependency changes and without marking Goal 091 or Goal 090 accepted.
Goal 092A is produced for review with `visual_world_preview_service_split_source_health_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it repairs the oversized Goal 092 Application service by splitting it
into smaller BCL-only files, records before/after source-health evidence with the service moving from 1295 to 145
logical lines, keeps Goal 092 behavior equivalent and strengthens Goal 092 quality evidence so no Goal092 namespace C#
file over 1000 logical lines can pass silently. Goal 092A does not start Runtime, Unity, provider, schema, Lua,
generator-library, project-file, dependency, binary/raster media or prompt-output work.
Goal 093 is produced for review with `visual_chunk_cache_export_contract_verification required`, `accepted=false`,
`implementationStatus=GREEN`; it consumes real Goal 091 stream-window artifacts into a BCL-only Application cache/export
manifest, readback proof, overlap reuse proof, negative proof, invalidation matrix and metadata-only runtime handoff
sidecar under `.llmgc/procedural/goal-093-visual-chunk-cache-export-contract/`. Goal 093 does not start Runtime, Unity,
provider, schema, Lua, generator-library, project-file, dependency, binary/raster media or prompt-output work and keeps
Goal 092A, Goal 092, Goal 091 and Goal 090 `accepted=false`.
Goal 094 is produced for review with `visual_chunk_cache_export_inspector_verification required`, `accepted=false`,
`implementationStatus=GREEN`; it extends the existing Visual World Stream Preview Workspace Application/WinForms seam to
read real Goal 093 cache/export artifacts, surfaces four packages, 93 records, the metadata-only runtime handoff sidecar
and readback/overlap/negative/invalidation proof status, and writes compact evidence under
`.llmgc/procedural/goal-094-visual-chunk-cache-export-inspector/`. Goal 094 does not start Runtime, Unity, provider,
schema, Lua, generator-library, project-file, dependency, binary/raster media or prompt-output work and keeps Goal 094,
Goal 093, Goal 092A, Goal 092, Goal 091 and Goal 090 `accepted=false`.
Goal 095 is produced for review with `visual_chunk_cache_unity_streamingassets_handoff_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it reads real Goal 093/094 cache/export evidence, mirrors a compact
metadata-only payload into Unity Alpha StreamingAssets at
`unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/VisualChunkCacheGoal095/`, adds standalone Unity probe
source `unity/LLMGameCreatorAlpha/Assets/Scripts/VisualChunkCacheHandoffProbe.cs`, and writes compact evidence under
`.llmgc/procedural/goal-095-visual-chunk-cache-unity-streamingassets-handoff/`. Goal 095 does not start Runtime
consumption, live Unity gameplay rendering, final atlas generation, runtime streaming, provider, schema, Lua,
generator-library, project-file, dependency, binary/raster media or prompt-output work and keeps Goal 095, Goal 094,
Goal 093, Goal 092A, Goal 092, Goal 091 and Goal 090 `accepted=false`.
Goal 096 is produced for review with `unity_handoff_inspector_probe_readiness_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it extends the existing Visual World Stream Preview Workspace
Application/WinForms seam to surface the real Goal 095 StreamingAssets payload, probe source inventory, simulated read,
negative proof, AlphaRuntimeBootstrap unchanged status and no-Unity-file-change proof. Evidence is under
`.llmgc/procedural/goal-096-unity-handoff-inspector-probe-readiness/`; Goal 096 changes no Unity files and does not
start Runtime consumption, live Unity gameplay rendering, final atlas generation, runtime streaming, provider, schema,
Lua, generator-library, project-file, dependency, binary/raster media or prompt-output work. Goal 096, Goal 095,
Goal 094, Goal 093, Goal 092A, Goal 092, Goal 091 and Goal 090 remain `accepted=false`.
Goal 097 is produced for review with `final_roadmap_rebaseline_dream_scope_productivity_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it rebases the roadmap after Goals 074-096, records Vertical Slice
Final / Strong Alpha / v1 Full Final / Dream Full Final milestone gates and estimates, adds the dream-scope register,
records realism/geospatial and self-generated realism simulator tracks as future planning only, adds release risk and
goal-productivity policy docs, and writes compact evidence under
`.llmgc/procedural/goal-097-final-roadmap-rebaseline-dream-scope-productivity/`. Goal 097 changes no product code,
Runtime, Unity, public schema, providers, Lua, generator-library, project files, dependencies, binary/raster media or
prompt-output work. Goal 096, Goal 095, Goal 094, Goal 093, Goal 092A, Goal 092, Goal 091 and Goal 090 remain
`accepted=false`.
Goal 098 is produced for review with `geoworld_source_adapter_streaming_contract_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it adds a BCL-only Application-side geoworld source adapter and runtime
streaming contract foundation with metadata-only fixtures, normalized geofeature taxonomy, streaming policy matrix,
negative proof, LFZ/geoworld docs lineage and compact evidence under
`.llmgc/procedural/goal-098-geoworld-source-adapter-streaming-contract/`. Goal 098 does not read or copy LFZ archive
source, does not implement live network/provider fetching, does not scrape map tiles, writes no raw geodata dumps and
does not change Runtime, Unity, public schema, Lua, generator-library, project files, dependencies, binary/raster media
or prompt-output work. Goal 098 and Goal 097 remain `accepted=false`.
Goal 099 is produced for review with `offline_geoworld_worldsourcegraph_streaming_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it adds a BCL-only synthetic offline geoworld bundle, normalized features,
immutable WorldSourceGraph chunks, no-network boundary-prefetch stream-window evidence, compact text-SVG projection and
Visual World Stream Preview Workspace integration under
`.llmgc/procedural/goal-099-offline-geoworld-worldsourcegraph-streaming/`. Goal 099 reads no LFZ archive, copies no LFZ
source, performs no live network/provider fetching, scrapes no map tiles, writes no raw geodata dumps and does not change
Runtime, Unity, public schema, Lua, generator-library, project files, dependencies, binary/raster media or prompt-output
work. Goal 099, Goal 098 and Goal 097 remain `accepted=false`.
Goal 100 is produced for review with `offline_geoworld_visual_cache_unity_handoff_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes real Goal 099 synthetic offline geoworld artifacts into
compact visual cache records, metadata-only Unity StreamingAssets handoff payloads, a standalone probe and the existing
Visual World Stream Preview Workspace under
`.llmgc/procedural/goal-100-offline-geoworld-visual-cache-unity-handoff/`. Goal 100 reads no LFZ archive, copies no LFZ
source, performs no live network/provider fetching, scrapes no map tiles, writes no raw geodata dumps and does not change
Runtime, public schema, Lua, generator-library, project files, dependencies, binary/raster media or prompt-output work.
Goal 100, Goal 099, Goal 098 and Goal 097 remain `accepted=false`.
Goal 101 is produced for review with `offline_geoworld_unity_preview_runner_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes real Goal 100 visual cache handoff artifacts into
metadata-only preview commands, five Unity StreamingAssets payload files, standalone Unity Alpha preview runner scripts,
travel-window demo metadata, simulated command execution proof and the existing Visual World Stream Preview Workspace
under `.llmgc/procedural/goal-101-offline-geoworld-unity-preview-runner/`. Goal 101 reads no LFZ archive, copies no LFZ
source, performs no live network/provider fetching, scrapes no map tiles, writes no raw geodata dumps and does not change
Runtime, public schema, Lua, generator-library, project files, dependencies, binary/raster media, final art, atlas,
scene/prefab production or prompt-output work. Goal 101, Goal 100, Goal 099, Goal 098 and Goal 097 remain
`accepted=false`.
Goal 102 is produced for review with `offline_geoworld_unity_editor_preview_tool_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes real Goal 101 metadata payloads into a Unity Editor-only
manual preview window, simulated create/clear action proof, negative proof, quality scan and existing Visual World
Stream Preview Workspace group under `.llmgc/procedural/goal-102-offline-geoworld-unity-editor-preview-tool/`.
Goal 102 reads no LFZ archive, copies no LFZ source, performs no live network/provider fetching, scrapes no map tiles,
writes no raw geodata dumps and does not change Runtime, public schema, Lua, generator-library, project files,
dependencies, binary/raster media, final art, atlas, Unity scene/prefab/settings/packages/build-settings or prompt-output
work. Goal 102, Goal 101, Goal 100, Goal 099, Goal 098 and Goal 097 remain `accepted=false`.
Goal 102A is produced for review with `unity_editor_source_format_guard_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it adds a raw-byte source-format scanner/evidence backstop over the
Goal 102 Unity Editor preview tool scope under
`.llmgc/procedural/goal-102a-unity-editor-source-format-guard/`. It proves the audited one-line/minified editor-window
failure class with a synthetic before sample, verifies the current after scan over Goal102 Unity/Application sources,
rejects zero-LF/CR-only/extreme-line/fake-read/AlphaRuntimeBootstrap/Unity-settings mutations and keeps
`AlphaRuntimeBootstrap.cs` unchanged. Goal 102A changes no behavior, Runtime, public schema, Lua, generator-library,
project files, dependencies, StreamingAssets payloads, binary/raster media, final art, atlas,
Unity scene/prefab/settings/packages/build-settings or prompt-output work. Goal 102A, Goal 102, Goal 101, Goal 100,
Goal 099, Goal 098 and Goal 097 remain `accepted=false`.
Goal 102B is produced for review with `actual_unity_editor_source_reformat_verification required`,
`accepted=false`, `implementationStatus=BLOCKED`; actual raw `HEAD:unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldPreviewWindow.cs`
bytes were read and are already multi-line/readable, so the required one-line HEAD-before preflight cannot be proven
honestly. It records Goal102A as superseded for source-format trust because Goal102A used synthetic-before evidence
instead of actual target-file HEAD bytes. Goal 102B changes no Runtime, public schema, Lua, generator-library, project
files, dependencies, StreamingAssets payloads, binary/raster media, final art, atlas, Unity scene/prefab/settings/packages
or build-settings files. Goal 102B, Goal 102A, Goal 102, Goal 101, Goal 100, Goal 099, Goal 098 and Goal 097 remain
`accepted=false`.
Goal 103 is produced for review with `offline_geoworld_playmode_travel_preview_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes real Goal101 command/travel metadata plus Goal102/Goal102B
evidence by repository-relative path, mirrors five metadata-only payload files into Unity Alpha StreamingAssets, adds
standalone play-mode travel controller/state/chunk-visibility scripts and a manual Unity Editor launch helper, surfaces
the group in the existing Visual World Stream Preview Workspace, records Goal102B's product/source blocker as closed
false-positive proceed while leaving Goal102B BLOCKED, and changes no Runtime, public schema, Lua, generator-library,
project files, dependencies, LFZ source/archive, live network/provider/geodata, binary/raster media, final art, atlas,
Unity scene/prefab/settings/packages or build-settings files. Goal 103, Goal 102B, Goal 102A, Goal 102, Goal 101,
Goal 100, Goal 099, Goal 098 and Goal 097 remain `accepted=false`.
Goal 104 is produced for review with `offline_geoworld_interactive_travel_preview_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes real Goal103 play-mode travel evidence by repository-relative
path, mirrors five metadata-only interactive payload files into Unity Alpha StreamingAssets, adds standalone interactive
travel controller/player-motor/boundary-prefetch-state scripts and a manual Unity Editor launch helper, surfaces movement
samples, boundary crossings, prefetch plans and object visibility diffs in the existing Visual World Stream Preview
Workspace, and changes no Runtime, public schema, Lua, generator-library, project files, dependencies, LFZ source/archive,
live network/provider/geodata, binary/raster media, final art, atlas, Unity scene/prefab/settings/packages or
build-settings files. Goal 104, Goal 103, Goal 102B, Goal 102A, Goal 102, Goal 101, Goal 100, Goal 099, Goal 098 and
Goal 097 remain `accepted=false`.
Goal 105 is produced for review with `offline_geoworld_interaction_playable_probe_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes real Goal104 interactive travel evidence by repository-relative
path, mirrors six metadata-only interaction payload files into Unity Alpha StreamingAssets, adds standalone interaction
controller/target/state-delta-log scripts and a manual Unity Editor probe helper, surfaces target counts, action kinds,
scripted events, state deltas, hash-chain proof and Unity script inventory in the existing Visual World Stream Preview
Workspace, and changes no Runtime, public schema, Lua, generator-library, project files, dependencies, LFZ source/archive,
live network/provider/geodata, binary/raster media, final art, atlas, Unity scene/prefab/settings/packages or
build-settings files. Goal 105, Goal 104, Goal 103, Goal 102B, Goal 102A, Goal 102, Goal 101, Goal 100, Goal 099,
Goal 098 and Goal 097 remain `accepted=false`.
Goal 106 is produced for review with `offline_geoworld_session_persistence_replay_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes real Goal105 interaction targets/actions/session/deltas by
repository-relative path, mirrors six metadata-only session persistence/replay payload files into Unity Alpha
StreamingAssets, adds standalone snapshot/save-load/replay scripts and a manual Unity Editor replay helper, surfaces replay
step count, state delta count, checkpoint, final hash, save-load proof and Unity script inventory in the existing Visual
World Stream Preview Workspace, and changes no Runtime, public schema, Lua, generator-library, project files,
dependencies, LFZ source/archive, live network/provider/geodata, binary/raster media, final art, atlas, Unity
scene/prefab/settings/packages or build-settings files. Goal 106, Goal 105, Goal 104, Goal 103, Goal 102B, Goal 102A,
Goal 102, Goal 101, Goal 100, Goal 099, Goal 098 and Goal 097 remain `accepted=false`.
Goal 107 is produced for review with `offline_geoworld_objective_acceptance_run_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes real Goal106 session persistence/replay evidence by
repository-relative path, mirrors six metadata-only objective acceptance payload files into Unity Alpha StreamingAssets,
adds standalone objective tracker/state/acceptance controller scripts and a manual Unity Editor acceptance helper,
surfaces objective count, completed objective count, final acceptance status, replay/save-load linkage, Unity script
inventory and Unity Alpha quality consolidation in the existing Visual World Stream Preview Workspace, and changes no
Runtime, public schema, Lua, generator-library, project files, dependencies, LFZ source/archive,
live network/provider/geodata, binary/raster media, final art, atlas, Unity scene/prefab/settings/packages or
build-settings files. Goal 107, Goal 106, Goal 105, Goal 104, Goal 103, Goal 102B, Goal 102A, Goal 102, Goal 101,
Goal 100, Goal 099, Goal 098 and Goal 097 remain `accepted=false`.
Goal 108 is produced for review with `offline_geoworld_alpha_slice_orchestrator_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes real Goal101-107 offline geoworld Alpha evidence by
repository-relative path, mirrors five metadata-only Alpha Slice payload files into Unity Alpha StreamingAssets,
adds a manual Unity Editor one-click setup/clear/verify window plus a small coordinator script, surfaces component
readiness, runbook readiness, full-slice simulated proof, negative proof, quality gate status and unchanged
AlphaRuntimeBootstrap status in the existing Visual World Stream Preview Workspace, and changes no Runtime, public
schema, Lua, generator-library, project files, dependencies, LFZ source/archive, live network/provider/geodata,
binary/raster media, final art, atlas, Unity scene/prefab/settings/packages or build-settings files. Goal 108,
Goal 107, Goal 106, Goal 105, Goal 104, Goal 103, Goal 102B, Goal 102A, Goal 102, Goal 101, Goal 100,
Goal 099, Goal 098 and Goal 097 remain `accepted=false`.
Goal 108A is produced for review as a GREEN hotfix/audit with `accepted=false`; it splits the Goal108
orchestrator Application service below 700 physical/logical lines, writes only new Goal108A evidence, reads actual
`14ad9f38..989a79ab` git diff/blob data, records 17 Goal108 additions and zero Goal101-107 artifact
modifications, and confirms Goal108 `historicalArtifactsUnchanged=true` matches actual git evidence. Goal108A
does not mark the Goal108 manual gate passed and does not start Runtime, schema, provider, Lua, generator-library,
Unity scene/settings/project/dependency or historical artifact mutation work.
Goal 109 is produced for review with `offline_geoworld_alpha_slice_export_package_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes real Goal108 Alpha Slice evidence and Goal108A audit by
repository-relative path, writes a portable deterministic directory package under
`.llmgc/exports/goal-109-offline-geoworld-alpha-slice/`, mirrors metadata-only package files into Unity Alpha
StreamingAssets, adds a standalone Unity package verifier/editor window, surfaces the `offline_geoworld_alpha_export_package`
group in the existing Visual World Stream Preview Workspace, and changes no Runtime, public schema, Lua,
generator-library, project files, dependencies, LFZ source/archive, live network/provider/geodata, binary/raster media,
final release, final art, atlas, Unity scene/prefab/settings/packages or build-settings files. Goal109, Goal108A,
Goal108 and prior geoworld gates remain `accepted=false`.
Goal 110 is produced for review with `offline_geoworld_alpha_manual_acceptance_verification required`,
`accepted=false`, `implementationStatus=GREEN`; it consumes the real Goal109 export package by repository-relative
path, writes the manual checklist/result-template/dashboard/readme package, adds Unity Alpha result/result-store scripts
and an Editor acceptance runner window, surfaces `offline_geoworld_alpha_manual_acceptance` in the existing Visual World
Stream Preview Workspace, and remains manual acceptance tooling only, not final release or final Runtime build.
Goal 111 is produced for review as a GREEN manual-result intake and decision bridge over Goal110 with
`decisionStatus=BLOCKED_PENDING_MANUAL_RESULT`, `acceptableCandidate=false`, `acceptedByCodex=false` and
`humanAcceptanceStillRequired=true` because no real manual result JSON exists in the deterministic candidate paths. It
surfaces `offline_geoworld_alpha_manual_result_intake` in the existing Visual World Stream Preview Workspace and does not
start live geodata/provider/network/runtime/schema/final-art work or mark the manual gate accepted. Goal111, Goal110,
Goal109, Goal108A, Goal108 and prior geoworld gates remain `accepted=false`.
Goal 112 is produced for review as a GREEN acceptance operator pack and RC readiness dashboard over Goal110 and Goal111
with `operatorStatus=OPERATOR_READY_PENDING_HUMAN_RUN`, `decisionStatusFromGoal111=BLOCKED_PENDING_MANUAL_RESULT`,
`manualResultPresent=false`, `acceptedByCodex=false` and `humanAcceptanceStillRequired=true`. It surfaces
`offline_geoworld_alpha_acceptance_operator_pack` in the existing Visual World Stream Preview Workspace and does not mark
the Alpha accepted, create a real manual result, or start live geodata/provider/network/runtime/schema/Lua/generator-library
or final-art/final-release work. The next human action remains running the Goal110 Unity checklist and placing the real
result JSON at `.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json`.
Goal112, Goal111, Goal110, Goal109, Goal108A, Goal108 and prior geoworld gates remain `accepted=false`.
Goal 113 is produced for review as a GREEN manual-result workbench over Goal110, Goal111 and Goal112 with
`workbenchStatus=WORKBENCH_READY_PENDING_HUMAN_RESULT`, `manualResultPresent=false`, `acceptedByCodex=false` and
`humanAcceptanceStillRequired=true`. It surfaces `offline_geoworld_alpha_manual_result_workbench` in the existing Visual
World Stream Preview Workspace and writes only a Goal113 draft/template outside `.llmgc/manual/**`; the active gate
remains `offline_geoworld_alpha_manual_acceptance_verification required`, `accepted=false`. The next human action is to
run the Goal110 Unity checklist, copy/edit the Goal113 draft only as a starting point, place the real human-created
result JSON at `.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json`,
re-run Goal111/Goal112/Goal113 validation and explicitly decide the manual gate. Goal113, Goal112, Goal111, Goal110,
Goal109, Goal108A, Goal108 and prior geoworld gates remain `accepted=false`.

Goal 114 is produced for review as a GREEN Unity Safe Mode compile hotfix over the same manual acceptance path. It
removes the reported unqualified `JsonUtility` references from the concrete Unity acceptance/session helper scripts,
adds `RefreshPayloadStatus()` compatibility wrappers, writes compact source-scan/negative-proof evidence, and keeps the
active gate at `offline_geoworld_alpha_manual_acceptance_verification required`, `accepted=false`. Goal114 writes no
`.llmgc/manual/**` result and changes no `AlphaRuntimeBootstrap.cs`, Unity scenes/prefabs/ProjectSettings/Packages,
StreamingAssets, Runtime, public schema, provider/LLM/RAG/media execution, Lua, generator-library or project/dependency
files. The next human action remains running the Goal110 Unity checklist after this Safe Mode unblock and placing a real
human-created result JSON for Goal111/Goal112/Goal113 validation.

Goal 115 is produced for review as a GREEN human-result revalidation candidate over the real local
`.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json`. It records
`decisionStatus=GREEN_ACCEPTABLE_CANDIDATE`, `goal111DecisionStatus=GREEN_ACCEPTABLE_CANDIDATE`,
`manualResultSha256=8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb`, 12/12 required steps passed,
`acceptableCandidate=true`, `recommendedHumanDecision=READY_FOR_EXPLICIT_HUMAN_ACCEPTANCE_DECISION`,
`acceptedByCodex=false` and `humanAcceptanceStillRequired=true`. It commits only deterministic summary/hash evidence
under Goal115 procedural/export roots and surfaces `offline_geoworld_alpha_human_result_revalidation` in the existing
Visual World Stream Preview Workspace. Goal115 itself left `offline_geoworld_alpha_manual_acceptance_verification`
required with `accepted=false`; Goal116 below records the subsequent explicit human acceptance.

Goal 116 records explicit human acceptance for `offline_geoworld_alpha_manual_acceptance_verification` using the exact
statement: Я принимаю offline_geoworld_alpha_manual_acceptance_verification по Goal115 GREEN_ACCEPTABLE_CANDIDATE. It
commits only deterministic summary/hash evidence under Goal116 procedural/export roots and surfaces
`offline_geoworld_alpha_manual_gate_acceptance_record` in the existing Visual World Stream Preview Workspace. The manual
gate is now `ACCEPTED_BY_HUMAN`, humanAccepted=true, acceptedByCodex=false, manualInputNotCommitted=true and
rawManualResultEmbeddedInArtifacts=false. The next safe step is `POST_ACCEPTANCE_CONTINUATION_SELECTION`; Goal116 is not
final release, Runtime, provider/live geodata/network, public schema, Lua, generator-library, final art, atlas, Unity
scene/prefab/project-settings or release-packaging approval.

Goal117 records the post-acceptance continuation-selection matrix after Goal116. It writes deterministic evidence under
`.llmgc/procedural/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/` and export metadata under
`.llmgc/exports/goal-117-offline-geoworld-alpha-post-acceptance-continuation-selection/`. The matrix recommends
`accepted_alpha_baseline_review` / `goal-118-offline-geoworld-accepted-alpha-baseline-review`, sets
`doNotStartAutomatically=true` and creates no Goal118 task files. Live geodata/provider/network, Runtime/schema, Lua,
generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets and
release-packaging work remain unauthorized without a separate explicit task.

Goal118 records the accepted Alpha baseline review package after Goal116 human acceptance. It writes deterministic
evidence under `.llmgc/procedural/goal-118-offline-geoworld-accepted-alpha-baseline-review/`, export metadata under
`.llmgc/exports/goal-118-offline-geoworld-accepted-alpha-baseline-review/`, and the short review note
`docs/manual-acceptance/offline-geoworld-accepted-alpha-baseline-review.md`. It records
`baselineId=offline_geoworld_alpha_accepted_baseline_v1`, acceptedBaselineReady=true, manualGateStatus=`ACCEPTED_BY_HUMAN`,
manualResultSha256=`8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb`,
sourceGoalRange=`Goal098-Goal117`, includedSourceGoalCount=23 and recommendedNextDecision=`EXPLICIT_NEXT_LANE_SELECTION`.
This is not final release and does not authorize live geodata/provider/network, Runtime/schema, Lua, generator-library,
final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal119 records the accepted Alpha Unity playable projection entrypoint over the Goal118 accepted baseline. It writes
deterministic evidence under `.llmgc/procedural/goal-119-accepted-alpha-unity-playable-projection/`, export metadata
under `.llmgc/exports/goal-119-accepted-alpha-unity-playable-projection/`, and the short manual note
`docs/manual-acceptance/accepted-alpha-unity-playable-projection.md`. It adds the Unity Editor menu path
`LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`, generated root
`__LLMGC_AcceptedAlphaPlayableProjection__`, script inventory, smoke plan, negative proof, quality gate scan and
Visual World Stream Preview Workspace visibility. This is not final release and does not authorize live
geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity
scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal119A records the accepted Alpha Unity material warning hotfix for the Goal119 projection. It removes edit-mode
marker material instantiation, adds the batchmode projection smoke method, and records source/log scan evidence under
`.llmgc/procedural/goal-119a-accepted-alpha-unity-material-warning-hotfix/` plus export metadata under
`.llmgc/exports/goal-119a-accepted-alpha-unity-material-warning-hotfix/`. The next manual check is still the same
Goal119 Unity menu route, with the added expected Console result that the projection emits no edit-mode material-leak
warning.

Goal120 records the accepted Alpha projection usability and cleanup pass for
`accepted_alpha_projection_usability_and_cleanup_verification` on the same manual Unity route. It adds
descriptor-backed selection controls, a visible legend, a Goal120 batchmode usability smoke and bounded cleanup commands
while keeping Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/settings/packages/
StreamingAssets/release-packaging work out of scope.

Goal120A records `goal_120a_clean_unity_editor_noise_empty_status_hotfix_verification`, the cleanup-script empty-status hotfix. After Unity manual checks, use
`.devflow\scripts\clean-unity-editor-noise.cmd` or `.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply`; clean
worktrees are valid and must not fail because status output has no lines.

Goal121 records `goal_121_accepted_alpha_interaction_drilldown_and_one_click_verification` for the same accepted Alpha
Unity route. The primary manual path is now one menu action plus `Run Full Projection Verification`, which fills
selected marker details, interaction/action preview, objective/replay details and a compact event log before local
smoke. The user should not have to click every debug button after each goal. After Unity manual checks, use
`.devflow\scripts\clean-unity-editor-noise.cmd`; next goals should continue product-visible work or automated
verification, not proof-only churn.

Goal122 records `goal_122_accepted_alpha_projection_action_loop_and_window_polish` for the same accepted Alpha Unity
route. The primary manual path remains one menu action plus `Run Full Projection Verification`; the window adds compact
status, bounded/collapsible evidence panels and separated optional debug controls. The projection-local action loop adds
`Select Next Interaction Target`, `Preview Selected Action`, `Apply Preview Action To Projection State` and
`Reset Projection State`. This remains Editor projection state only and does not authorize Runtime/provider/schema/Lua,
generator-library, final-art/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release work.

Goal123 records `goal_123_generic_gamepackage_playable_projection_adapter` for the accepted Alpha Unity projection
shell. The route adds `Run Generic Package Projection Verification`, reads `samples/minimal-map-game/package.json` as a
read-only sample and visualizes package identity, map dimensions, start/player proxy, tiles, entities, interactions,
item summary and event log. This is projection-only and does not authorize sample mutation, Runtime/provider/schema/Lua,
generator-library, final-art/atlas, Unity scene/prefab/settings/packages/StreamingAssets or release work.

Goal124 records `goal_124_generic_gamepackage_quest_dialogue_interaction_loop` for the accepted Alpha Unity projection
shell. The route adds `Run Generic Package Gameplay Loop Verification`, reads `samples/minimal-map-game/package.json`
as a read-only sample, previews/applies `interaction/sign_inspect` in projection-local state and displays
`dialogue/old_guard_intro`, `quest/help_healer`, inventory/resource summaries and an event log. This is projection-only
and does not authorize sample mutation, Runtime/provider/schema/Lua, generator-library, final-art/atlas, Unity
scene/prefab/settings/packages/StreamingAssets or release work.

Goal125 records `goal_125_generic_gamepackage_systems_loop_projection` for the accepted Alpha Unity projection shell.
The route adds `Run Generic Package Systems Loop Verification`, reads `samples/minimal-map-game/package.json` as a
read-only sample, previews/applies `recipe/healing_potion`, previews/applies `node/apple_tree` harvest, previews
`transaction/buy_healing_potion`, previews `encounter/goblin_duel`, runs one deterministic combat round and displays
inventory/resource summaries plus a systems event log. This is projection-only and does not authorize sample mutation,
Runtime/provider/schema/Lua, generator-library, final-art/atlas, Unity scene/prefab/settings/packages/StreamingAssets
or release work.

Allowed next sequence:

1. Keep Goal 034 `strict_llm_draft_artifact_loop_verification` recorded as passed by the user.
2. Keep Goal 035 `lua_module_manifest_registry_verification` recorded as passed by the user.
3. Preserve Goal 031 and Goal 032 as produced-for-review/not passed.
4. Keep Goal 036 `lua_sandbox_execution_gate_verification` recorded as passed by the user handoff embedded in Goal 037.
5. Keep Goal 037 `hybrid_llm_draft_lua_deterministic_expansion_verification` recorded as passed by the user handoff before Goal 038.
6. Keep Goal 038 world-scale region map foundation recorded as passed by user handoff.
7. Keep Goal 039 runtime chunk delta traversal smoke recorded as passed by the Goal 040 user handoff.
8. Keep Goal 040 chunked runtime preview/export multi-family smoke recorded as passed by user handoff.
9. Keep Goal 043 multi-family generated template vertical slice recorded as passed by user handoff.
10. Keep Goal 047 full generator without media dry-run recorded as passed by user handoff.
11. Keep Goal 053 media asset campaign orchestration recorded as passed by user handoff.
12. Keep Goal 054 media materialization review package recorded as passed by Goal 055 user handoff.
13. Keep Goal 055 media-bound playable review package recorded as passed by the Goal 056 user handoff.
14. Keep Goal 056 Unity Alpha media-bound playable package recorded as passed by the Goal 057 user handoff.
15. Keep Goal 057 Unity Alpha multi-family playable loop recorded as passed by user handoff before Goal 058.
16. Keep Goal 058 Full Media-Bound Generator Campaign recorded as passed by user handoff before Goal 059.
17. Keep Goal 059 Full Generator Variability Regression Matrix recorded as passed by user handoff before Goal 060.
18. Keep Goal 060 Full Campaign GamePackage Materialization Matrix recorded as passed by user handoff before Goal 061.
19. Keep Goal 061 Full Campaign Playable Review Package RC recorded as passed by user handoff before Goal 062.
20. Keep Goal 062 Constrained Spatial Detail Generation recorded as passed by user handoff before Goal 063.
21. Keep Goal 063 Gameplay Consequence Depth Matrix recorded as passed by user handoff before Goal 064.
22. Keep Goal 064 Living World NPC/Faction Simulation Matrix recorded as passed by user handoff before Goal 065.
23. Keep Goal 065 Interlocked Gameplay Systems Depth Matrix recorded as passed by user handoff before Goal 066.
24. Keep Goal 066 Settlement Construction Destruction Production Matrix recorded as passed by user handoff before Goal 067.
25. Keep Goal 067 Programmatic Narrative Quest Dialogue Event Matrix recorded as passed by user handoff before Goal 068.
26. Keep Goal 068 Combat Magic Ability Boss Encounter Matrix recorded as passed by user handoff before Goal 069.
27. Keep Goal 069 World Event Weather Day/Night Crisis Matrix recorded as passed by user handoff before Goal 070.
28. Keep Goal 070 Integrated Campaign Timeline Simulation Matrix recorded as passed by user handoff before Goal 071.
29. Keep Goal 071 Unity Alpha Interactive Campaign Player recorded as passed by user handoff before Goal 072.
30. Keep Goal 072 Generator Spine Quality Consolidation produced for review with `generator_spine_quality_consolidation_verification required`, `accepted=false` and `implementationStatus=BLOCKED`; Goal 073 repaired the P0 source-format blocker but did not mark Goal 072 passed.
31. Keep Goal 073 Source Format P0 Readability Repair recorded as passed by user handoff before Goal 074.
32. Keep Goal 074 Schema-Driven Campaign Authoring And Review Workspace recorded as accepted by user handoff before Goal 075.
33. Keep Goal 075 Schema-Driven Campaign Edit/Validate/Apply Loop recorded as accepted by user handoff before Goal 076.
34. Keep Goal 076 Edit-Driven Playable Preview Refresh recorded as accepted by user handoff before Goal 077, without mutating the Goal 076 artifact's `accepted=false` evidence.
35. Keep Goal 077 Edit-Driven Review Package Materialization recorded as accepted by user handoff before Goal 078, without mutating the Goal 077 artifact's `accepted=false` evidence.
36. Keep Goal 078 Edit-Driven Review Package Playable Session recorded as accepted by user handoff before Goal 079, without mutating the Goal 078 artifact's `accepted=false` evidence.
37. Record Goal 079 quality consolidation as accepted for continuation before Goal 080 without rewriting the Goal 079 artifact accepted=false evidence.
38. Record Goal 079A source-format guard as passed before Goal 080 without rewriting historical Goal 079A artifacts.
39. Record Goal 080 runtime-preview bridge as passed before Goal 081 without rewriting the Goal 080 artifact accepted=false evidence.
40. Record Goal 081 runtime-preview playthrough as passed before Goal 082 without rewriting the Goal 081 artifact accepted=false evidence.
41. Review Goal 082 `edit_driven_unity_alpha_streamingassets_handoff_verification`; do not mark it passed until explicit user acceptance and do not start the next goal from this implementation handoff.
42. Review Goal 082A `source_format_physical_line_repair_verification`; it repairs the Goal 082 source-format guard backstop, keeps Goal 082 `accepted=false`, and treats `21f2525a adult docs` as docs context only.
43. Review Goal 083 `visual_adult_layer_context_integration_verification`; it indexes/routes the visual/adult docs as policy-bounded context, keeps Goal 082 and Goal 082A `accepted=false`, and does not start a provider/media/runtime/schema implementation.
44. Keep Goal 084 `visual_asset_contract_rating_metadata_verification` recorded as passed before Goal 085 by handoff, without rewriting Goal 084 artifact accepted=false evidence.
45. Review Goal 085 `visual_part_pack_rule_stack_verification`; it adds the deepsearch-backed visual part-pack rule-stack foundation, keeps Goal 083, Goal 082 and Goal 082A `accepted=false`, and does not start provider/media/runtime/schema implementation.
46. Keep Goal 086 `deterministic_visual_microtile_materializer_verification` recorded as accepted for continuation before Goal 087 by handoff, without rewriting Goal 086 artifact accepted=false evidence.
47. Keep Goal 087 `deterministic_visual_map_patch_composer_verification` recorded as accepted for continuation before Goal 088 by handoff, without rewriting Goal 087 artifact accepted=false evidence.
48. Review Goal 088 `deterministic_visual_region_composer_verification`; it adds deterministic text SVG visual region composer evidence, keeps Goal 085, Goal 083, Goal 082 and Goal 082A `accepted=false`, and does not start provider/media/runtime/schema implementation.
49. Review Goal 088A `goal_088_check_all_validation_repair_verification`; it repairs the Goal 088 full check-all blocker, keeps Goal 088 artifacts `accepted=false`, and does not start provider/media/runtime/schema implementation.
50. Review Goal 089 `tiered_validation_pipeline_verification`; it adds tiered validation policy/wrappers, keeps full check-all authoritative, keeps Goal 088A and Goal 088 `accepted=false`, and does not start product/runtime/provider/media/schema implementation.
51. Review Goal 090 `parameterized_visual_world_profiles_verification`; it handles the Goal 088 fixed-size concern at the profile/addressing layer, keeps Goal 090 `accepted=false`, and does not start Runtime, Unity, provider, media, schema, Lua or generator-library implementation.
52. Review Goal 091 `deterministic_visual_chunk_stream_window_verification`; it proves deterministic chunk stream windows over Goal 090 profiles, keeps Goal 091 and Goal 090 `accepted=false`, and does not start Runtime, Unity, provider, media, schema, Lua or generator-library implementation.
53. Review Goal 092 `visual_world_stream_preview_workspace_verification`; it adds an Application/WinForms preview workspace over real Goal 086-091 artifacts, keeps Goal 092, Goal 091 and Goal 090 `accepted=false`, and does not start Runtime, Unity, provider, media, schema, Lua or generator-library implementation.
54. Review Goal 092A `visual_world_preview_service_split_source_health_verification`; it repairs the Goal 092 source-health regression, keeps Goal 092A and Goal 092 `accepted=false`, and does not start Runtime, Unity, provider, media, schema, Lua or generator-library implementation.
55. Review Goal 093 `visual_chunk_cache_export_contract_verification`; it adds the BCL-only Application cache/export contract and metadata-only runtime handoff sidecar over real Goal 091 artifacts, keeps Goal 093, Goal 092A, Goal 092, Goal 091 and Goal 090 `accepted=false`, and does not start Runtime, Unity, provider, media, schema, Lua or generator-library implementation.
56. Review Goal 094 `visual_chunk_cache_export_inspector_verification`; it makes Goal 093 cache/export artifacts inspectable in the existing Application/WinForms review workspace, keeps Goal 094 and previous visual gates `accepted=false`, and does not start Runtime, Unity, provider, media, schema, Lua or generator-library implementation.
57. Review Goal 095 `visual_chunk_cache_unity_streamingassets_handoff_verification`; it mirrors compact Goal 093/094 cache/export metadata into Unity Alpha StreamingAssets and adds a standalone probe, keeps Goal 095 and previous visual gates `accepted=false`, and does not start Runtime consumption, live Unity gameplay rendering, final atlas generation, provider, media, schema, Lua or generator-library implementation.
58. Review Goal 096 `unity_handoff_inspector_probe_readiness_verification`; it makes Goal 095 Unity handoff payload/probe/readiness evidence inspectable in the existing Application/WinForms review workspace, keeps Goal 096 and previous visual gates `accepted=false`, and does not start Runtime consumption, live Unity gameplay rendering, final atlas generation, provider, media, schema, Lua or generator-library implementation.
59. Review Goal 097 `final_roadmap_rebaseline_dream_scope_productivity_verification`; it rebases roadmap/milestone/risk/productivity planning after the Goal 074-096 chain, keeps Goal 096 and prior visual gates `accepted=false`, records realism/geospatial simulator work as future research-only scope and does not start product code, Runtime, Unity, provider, media, schema, Lua or generator-library implementation.
60. Review Goal 098 `geoworld_source_adapter_streaming_contract_verification`; it adds BCL-only geoworld source adapter/streaming contracts, metadata-only fixtures, normalized taxonomy and negative proof, keeps Goal 098 and Goal 097 `accepted=false`, and does not start LFZ source/archive consumption, live network/provider/geodata ingestion, Runtime, Unity, public schema, Lua or generator-library implementation.
61. Review Goal 099 `offline_geoworld_worldsourcegraph_streaming_verification`; it adds BCL-only synthetic offline geoworld bundle normalization, immutable WorldSourceGraph chunks, no-network boundary-prefetch stream-window evidence and workspace integration, keeps Goal 099, Goal 098 and Goal 097 `accepted=false`, and does not start LFZ source/archive consumption, live network/provider/geodata ingestion, Runtime, Unity, public schema, Lua or generator-library implementation.
62. Review Goal 100 `offline_geoworld_visual_cache_unity_handoff_verification`; it adds metadata-only visual cache records and Unity StreamingAssets handoff/probe evidence over the Goal 099 synthetic offline bundle, keeps Goal 100, Goal 099, Goal 098 and Goal 097 `accepted=false`, and does not start LFZ source/archive consumption, live network/provider/geodata ingestion, Runtime consumers, public schema, Lua or generator-library implementation.
63. Review Goal 101 `offline_geoworld_unity_preview_runner_verification`; it adds metadata-only preview commands, standalone Unity Alpha preview runner scripts, travel-window metadata and workspace inspection over the Goal 100 visual cache handoff, keeps Goal 101, Goal 100, Goal 099, Goal 098 and Goal 097 `accepted=false`, and does not start LFZ source/archive consumption, live network/provider/geodata ingestion, Runtime consumers, public schema, final gameplay, final art, atlas, scene/prefab, Lua or generator-library implementation.
64. Review Goal 102 `offline_geoworld_unity_editor_preview_tool_verification`; it adds a Unity Editor-only manual preview window, simulated create/clear action proof and workspace inspection over the Goal 101 metadata payload, keeps Goal 102, Goal 101, Goal 100, Goal 099, Goal 098 and Goal 097 `accepted=false`, and does not start LFZ source/archive consumption, live network/provider/geodata ingestion, Runtime consumers, public schema, final gameplay, final art, atlas, scene/prefab/settings/packages/build-settings, Lua or generator-library implementation.
65. Review Goal 102A `unity_editor_source_format_guard_verification`; it repairs the Goal 102 Unity Editor source-format guard backstop, keeps Goal 102A and Goal 102 `accepted=false`, and does not start LFZ source/archive consumption, live network/provider/geodata ingestion, Runtime consumers, public schema, final gameplay, final art, atlas, scene/prefab/settings/packages/build-settings, Lua or generator-library implementation.
66. Review Goal 102B `actual_unity_editor_source_reformat_verification`; it is BLOCKED because actual target-file HEAD bytes are already readable, and it supersedes Goal102A source-format trust until a corrected actual-before proof exists.
67. Review Goal 103 `offline_geoworld_playmode_travel_preview_verification`; it produces metadata-only play-mode travel preview evidence over Goal101/102/102B with standalone Unity scripts/editor helper and workspace inspection while keeping Goal103 and prior geoworld gates `accepted=false`.
68. Review Goal 104 `offline_geoworld_interactive_travel_preview_verification`; it produces metadata-only interactive travel preview evidence over real Goal103 payloads with standalone Unity scripts/editor helper and workspace inspection while keeping Goal104 and prior geoworld gates `accepted=false`.
69. Review Goal 105 `offline_geoworld_interaction_playable_probe_verification`; it produces metadata-only interaction playable probe evidence over real Goal104 payloads with standalone Unity scripts/editor helper, state-delta proof and workspace inspection while keeping Goal105 and prior geoworld gates `accepted=false`.
70. Review Goal 106 `offline_geoworld_session_persistence_replay_verification`; it produces metadata-only session persistence/replay evidence over real Goal105 payloads with standalone Unity scripts/editor helper, save-load proof and workspace inspection while keeping Goal106 and prior geoworld gates `accepted=false`.
71. Review Goal 107 `offline_geoworld_objective_acceptance_run_verification`; it produces metadata-only objective acceptance evidence over real Goal106 payloads with standalone Unity scripts/editor helper, acceptance proof and workspace quality consolidation while keeping Goal107 and prior geoworld gates `accepted=false`.
72. Review Goal 108 `offline_geoworld_alpha_slice_orchestrator_verification`; it produces metadata-only Alpha Slice orchestrator evidence over real Goal101-107 payloads with Unity Alpha StreamingAssets payloads, one-click Editor setup/clear/verify helper, coordinator script, acceptance runbook, full-slice proof and workspace inspection while keeping Goal108 and prior geoworld gates `accepted=false`.
73. Review Goal 109 `offline_geoworld_alpha_slice_export_package_verification`; it produces a portable deterministic Alpha Slice export package, clean-import proof, negative proof, standalone Unity verifier/editor window, StreamingAssets metadata mirror and workspace inspection while keeping Goal109 and prior geoworld gates `accepted=false`.
74. Review Goal 112 operator pack while keeping the active human gate at `offline_geoworld_alpha_manual_acceptance_verification`; it packages Goal110/Goal111 into visible run instructions and readiness status, but acceptance still requires a real human result JSON at the preferred `.llmgc/manual` path.
75. Review Goal 113 manual-result workbench while keeping the active human gate at `offline_geoworld_alpha_manual_acceptance_verification`; it packages Goal110/Goal111/Goal112 into visible authoring/review status, writes only a draft/template outside `.llmgc/manual/**`, and acceptance still requires a real human-created result JSON at the preferred `.llmgc/manual` path plus explicit human gate decision.
76. Review Goal 114 Unity Safe Mode compile hotfix while keeping the active human gate at `offline_geoworld_alpha_manual_acceptance_verification`; it unblocks the reported Unity compile errors only, writes no `.llmgc/manual/**` result and still requires a real human-created result JSON at the preferred `.llmgc/manual` path plus explicit human gate decision.
77. Review Goal 115 human-result revalidation while keeping the active human gate at `offline_geoworld_alpha_manual_acceptance_verification`; it validates the real local human result as `GREEN_ACCEPTABLE_CANDIDATE`, commits no `.llmgc/manual/**` input and still requires explicit human gate decision.
78. Use Goal 116 as the accepted manual gate record for `offline_geoworld_alpha_manual_acceptance_verification`; select the post-acceptance continuation explicitly and do not start Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/project-settings/release-packaging work without a separate task.
79. Use Goal117 as the GREEN continuation-selection matrix: recommended next lane is `accepted_alpha_baseline_review` and recommended next goal id is `goal-118-offline-geoworld-accepted-alpha-baseline-review`; do not create/start Goal118 without a separate explicit task.
80. Use Goal118 as the accepted Alpha baseline review package: baseline id `offline_geoworld_alpha_accepted_baseline_v1`, acceptedBaselineReady=true and recommendedNextDecision=`EXPLICIT_NEXT_LANE_SELECTION`; do not start live geodata/provider, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/StreamingAssets or release packaging without a separate explicit task.
81. Use Goal119 as the accepted Alpha Unity playable projection entrypoint: run hands-on verification through `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`, review the generated root `__LLMGC_AcceptedAlphaPlayableProjection__`, and do not start live geodata/provider, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/StreamingAssets or release packaging without a separate explicit task.
82. Use Goal119A as the material-warning hotfix for the Goal119 accepted Alpha Unity projection; the same menu route should now be checked with no edit-mode material-leak warning in the Unity Console.
83. Use Goal120 as the accepted Alpha projection usability and cleanup pass: run hands-on verification through `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`, use the focus/select/legend controls, run `.devflow/scripts/clean-unity-editor-noise.ps1 -DryRun` before `-Apply`, and keep Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/settings/packages/StreamingAssets/release-packaging work behind separate explicit tasks.
84. Use Goal120A as the cleanup-script empty-status hotfix for Goal120 manual verification: a clean worktree is a valid empty status list, `.devflow\scripts\clean-unity-editor-noise.cmd` remains supported, and cleanup rules remain bounded.
85. Use Goal121 as the accepted Alpha interaction drilldown and one-click verification pass: run `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`, click `Run Full Projection Verification`, review selected marker details plus interaction/action and objective/replay details, use `.devflow\scripts\clean-unity-editor-noise.cmd` after Unity checks, and keep Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/settings/packages/StreamingAssets/release-packaging work behind separate explicit tasks.
86. Use Goal122 as the accepted Alpha projection-local action loop and window polish pass: run `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`, click `Run Full Projection Verification`, then use the projection-only preview/apply/reset buttons; use `.devflow\scripts\clean-unity-editor-noise.cmd` after Unity checks and keep Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/settings/packages/StreamingAssets/release-packaging work behind separate explicit tasks.
87. Use Goal123 as the generic GamePackage projection adapter pass: run `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`, click `Run Generic Package Projection Verification`, inspect the read-only minimal-map package projection, use `.devflow\scripts\clean-unity-editor-noise.cmd` after Unity checks, and keep sample mutation, Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/settings/packages/StreamingAssets/release-packaging work behind separate explicit tasks.
88. Use Goal124 as the generic GamePackage quest/dialogue/interaction loop pass: run `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`, click `Run Generic Package Gameplay Loop Verification`, inspect the read-only sign inspect, old guard dialogue, help healer objective, inventory/resource summaries and event log, use `.devflow\scripts\clean-unity-editor-noise.cmd` after Unity checks, and keep sample mutation, Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/settings/packages/StreamingAssets/release-packaging work behind separate explicit tasks.
89. Review Goal132 as the WinForms Candidate Pipeline Operator panel: use `.devflow\scripts\run-gamepackage-candidate-recipe-pipeline.cmd` as the normal command, inspect selected candidate/count/matrix/result/output-tail proof in the workspace, and keep sample mutation, `.llmgc/manual/**`, Runtime/provider/schema/Lua/generator-library/final-art/atlas/Unity scene/prefab/settings/packages/StreamingAssets/release-packaging work behind separate explicit tasks.
90. Review Goal141 as the runtime-backed Unity/player command roundtrip bridge: use `.devflow\scripts\run-runtime-backed-player-command-roundtrip.cmd` as the normal command, inspect request/result/snapshot/model/report proof in the workspace, and keep sample mutation, `.llmgc/manual/**`, public GamePackage schema, Generation, AssetPipeline, Scripting/Lua, provider/media/LLM/RAG, generator-library, Unity scene/prefab/project-settings/packages/StreamingAssets and release-packaging work behind separate explicit tasks.

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
