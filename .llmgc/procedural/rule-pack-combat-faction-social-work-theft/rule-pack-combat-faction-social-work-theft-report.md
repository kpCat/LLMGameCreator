# Rule-Pack Combat, Faction, Social, Work And Theft Report

- Accepted: true
- Manual gate: rule_pack_combat_faction_social_work_theft_artifact_verification
- Goal 008 gate recorded: true
- Completed slices: S078, S079, S080, S081, S082, S083, S084, S084A
- Valid scenarios: 7
- Invalid scenarios: 19
- Runtime execution: true
- Save/load: true
- Deterministic replay: true
- Isolation: true
- Fake success rejected: true
- Hash: 5517fa0639225197b8cf8debc22d54c517020048f5c7c401a8b67025a9905915

## Bounded Semantics

- Work: work means a data-driven interaction or transaction contract over existing requirement, output, item, flag and reputation primitives
- Theft: theft means a data-driven container transfer plus explicit rule-pack flag and reputation consequences; no dynamic stealth or detection AI is claimed

## Remaining Primitive Limits
- work has no schedules, employers, time wages or economy simulation
- theft has no witnesses, detection chance, law ownership model, stealth AI or relationship simulation
- social evidence is bounded to package dialogue choices and supported runtime consequences
- combat evidence is bounded to existing encounter turns, abilities, AI and reward outputs
- Unity presentation, Lua/provider/media execution and future content scale-up remain out of scope

## Scenarios
- combat_resolution_reward: expected=true, actual=true, commands=2
- combat_turn_based_encounter: expected=true, actual=true, commands=3
- combined_combat_social_work_theft_loop: expected=true, actual=true, commands=10
- faction_reputation_change: expected=true, actual=true, commands=1
- invalid_combat_wrong_turn_or_target: expected=false, actual=false, commands=3
- invalid_command_not_covered_by_declaration: expected=false, actual=false, commands=0
- invalid_cross_scenario_state_leakage: expected=false, actual=false, commands=10
- invalid_dialogue_or_choice_ref: expected=false, actual=false, commands=0
- invalid_dialogue_wrong_node_or_output_mismatch: expected=false, actual=false, commands=0
- invalid_fake_runtime_success: expected=false, actual=false, commands=0
- invalid_missing_ability_or_resource_ref: expected=false, actual=false, commands=0
- invalid_missing_encounter_or_participant_ref: expected=false, actual=false, commands=0
- invalid_missing_faction_ref: expected=false, actual=false, commands=0
- invalid_missing_participant_ability_resource_cost_reward_binding: expected=false, actual=false, commands=0
- invalid_same_declaration_wrong_ability_actor_target: expected=false, actual=false, commands=0
- invalid_same_declaration_wrong_amount_flag_value: expected=false, actual=false, commands=0
- invalid_same_declaration_wrong_existing_target: expected=false, actual=false, commands=0
- invalid_save_load_mismatch: expected=false, actual=false, commands=10
- invalid_theft_amount_flag_reputation_mismatch: expected=false, actual=false, commands=0
- invalid_theft_container_or_item_ref: expected=false, actual=false, commands=0
- invalid_theft_nonpositive_amount: expected=false, actual=false, commands=0
- invalid_work_requirement_unmet: expected=false, actual=false, commands=1
- invalid_work_wrong_existing_transaction: expected=false, actual=false, commands=0
- social_dialogue_reputation_consequence: expected=true, actual=true, commands=2
- theft_container_reputation_consequence: expected=true, actual=true, commands=4
- work_contract_reward: expected=true, actual=true, commands=2
