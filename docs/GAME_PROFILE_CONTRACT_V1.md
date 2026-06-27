# Game Profile Contract v1

Status: Goal 021 review contract  
Final gate: `generated_game_profile_contract_verification`

## Purpose

`game_profile_v1` is the deterministic profile contract that turns user/game intent into explicit generation pipeline choices before future capability bundle selection or package assembly work starts.

The profile is data. It does not grant runtime authority to an LLM, Lua, media provider, Unity code generator or UI layer.

## Required Fields

- `schemaVersion`: must be `game_profile_v1`.
- `profileId`: stable lower-case slash/hyphen id for the profile.
- `displayName`: human-readable review label.
- `targetExperience`: short player-intent summary.
- `gameFamilyId`: explicit family id such as `game_family/frontier_survival`.
- `presentationMode`: explicit `presentation_mode/*` id.
- `worldTopology`: explicit `world_topology/*` id.
- `actorModel`: explicit `actor_model/*` id.
- `questDialogueInteractionLoopFamily`: selected quest/dialogue/interaction loop family id.
- `inventoryItemEconomyLoopFamily`: selected inventory/item/economy loop family id.
- `capabilityFlags`: combat/faction/social/work/theft booleans requested by the profile.
- `progressionScope`: selected progression scope id.
- `contentScale`: bounded content target and budget.
- `assetPolicy`: asset mode, fallback policy and runtime-provider boundary.
- `runtimeExportTarget`: selected runtime/export target id.
- `forbiddenRuntimeDependencies`: dependencies that must remain false at runtime.
- `expectedDownstreamPipelineSlices`: exact downstream Goal 010-020 stage ids required by the profile.
- `selectedCapabilityIds`: capability ids that the pipeline planner must map to supported, partial or future-required status.

## Boundary Rules

- Runtime must not call LLM, RAG, media providers, arbitrary Lua or editor-only generation providers.
- The profile must not claim a public `GamePackage` schema mutation.
- The profile must not claim a Unity build was produced by Goal 021.
- Unsupported topology, combat, economy, UI or world-scale needs must be explicit `future_required`, not silently treated as complete.
- Profile approval is a manual review gate. Goal 021 writes acceptance artifacts and leaves `generated_game_profile_contract_verification` required.

## Current Supported Proof Vocabulary

Goal 021 maps profiles onto existing proof stages:

- `stage/content_generation_scale_goal_010`
- `stage/minimum_asset_pipeline_goal_011`
- `stage/unity_runtime_export_goal_012`
- `stage/unity_generated_runtime_state_loop_goal_016`
- `stage/unity_generated_quest_completion_loop_goal_017`
- `stage/unity_multi_variant_playable_scenario_goal_018`
- `stage/unity_alpha_readable_presentation_goal_019`
- `stage/minimum_playable_generated_game_goal_020`

Current profile plans may include future-required capabilities, but the final report must keep those separate from currently supported stages.
