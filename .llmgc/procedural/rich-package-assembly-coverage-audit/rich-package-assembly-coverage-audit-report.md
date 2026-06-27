# Rich Package Assembly Coverage Audit Report

- Accepted: false
- Manual gate: rich_package_assembly_coverage_audit_verification
- Previous accepted gate: capability_bundle_pipeline_inputs_verification passed
- Goal 023 evidence verified: true
- Coverage domains: 8
- Matrix hash: 5dd1beaf62f8bbfaa46f68cbf65971e0331562ce9c30efcfaeb806444c877d8f
- Gap report hash: 57091cac2ebaf801546a66c48f483f44e0e1071700446ec093b16b165b9bd99b
- Next slice plan hash: 08fe38f98af82af7518c93ed516a9093515afd6aae742c1e87fa130969cf461c
- Report hash: 165cfe779881593b77812bc7d8542c6894bd7f75cb206f0900e0600140566253
- Invalid/fake/leak scenarios rejected: 16/16
- External execution: none

## Coverage Domains

- assets_runtime_export: package_supported_partial, gaps=5, action=Asset catalog and export artifacts exist, while exact Unity Alpha runtime target and media request packs remain future-required.
- combat_progression: package_supported_partial, gaps=6, action=Encounters, abilities, stats and progressions exist, but richer combat pack/progression inputs are mostly future-required.
- dialogue_interactions: package_supported_partial, gaps=4, action=Dialogue and interaction package fields exist; clue graph, morphology and advanced condition packs remain sidecar/future-required.
- entities: package_supported_partial, gaps=6, action=Existing package supports entity prototypes, map placements and generated NPC sidecars; party/card richness is not package-assembled yet.
- factions_social_work_theft_schedules: package_supported_partial, gaps=8, action=Faction and reputation fields exist and work/theft can be represented through interactions/runtime evidence, but schedules have no package field yet.
- items_inventory_economy: package_supported_partial, gaps=7, action=Economy package fields and validators exist; vendor/economy profile requests are not fully package-assembled.
- quests: package_supported_partial, gaps=4, action=Quest definitions, objectives and staged fields exist, but graph/richer reward rules from Goal 023 remain future-required.
- world: package_supported_partial, gaps=8, action=Existing package maps and generated regions cover finite starter maps, but region graph/chunk topology remains future-required or blocked where Goal 023 says so.

## Top Gaps

- assets_runtime_export: future_required asset_index_v1
- assets_runtime_export: future_required asset_request_pack_v1
- assets_runtime_export: future_required audio_request_pack_v1
- assets_runtime_export: future_required gap/runtime_export/unity_alpha_windows_exact_atlas_target
- assets_runtime_export: future_required unity_ir_v1
- combat_progression: future_required ability.rules/v1
- combat_progression: future_required combat.mode/v1
- combat_progression: future_required combat_pack_v1
- combat_progression: future_required encounter_pack_v1
- combat_progression: future_required progression_pack_v1
- combat_progression: future_required status_effects/v1
- dialogue_interactions: future_required dialogue.graph/v1
- dialogue_interactions: future_required interaction.conditions/v1
- dialogue_interactions: future_required interaction_pack_v1
- dialogue_interactions: future_required phrase_plan_v1
- entities: future_required actor_model_profile_v1
- entities: future_required character_card_v1
- entities: future_required entity_pack_v1
- entities: future_required npc_card_v1
- entities: future_required party_roster_v1

## Next Slice Plan

- 1. Package Assembly Expansion 1 - World And Entities: recommended=true, startsGoal025OrS199=false
- 2. Package Assembly Expansion 2 - Dialogue And Quests: recommended=false, startsGoal025OrS199=false
- 3. Package Assembly Expansion 3 - Items, Economy And Crafting: recommended=false, startsGoal025OrS199=false

## Diagnostics

- info: rich_package_audit.audit_only_boundary [execution_boundary] No package assembly expansion, Unity build, LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.
- info: rich_package_audit.goal023_gate_recorded [capability_bundle_pipeline_inputs_verification passed] User-confirmed Goal 023 capability bundle pipeline inputs verification is recorded as passed.
- info: rich_package_audit.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak scenarios reject through Goal 023 evidence, coverage, report or scope guard diagnostics.
