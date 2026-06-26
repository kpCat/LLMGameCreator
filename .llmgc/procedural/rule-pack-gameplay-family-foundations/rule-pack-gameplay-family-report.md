# Rule-Pack Gameplay Family Report

- Accepted: true
- Manual gate: rule_pack_gameplay_family_artifact_verification
- Goal 007 gate recorded: true
- Completed slices: S071, S072, S073, S074, S075, S076, S077, S077A
- Valid scenarios: 6
- Invalid scenarios: 10
- Package/rule binding audit: true
- Runtime execution: true
- Save/load exact state: true
- Deterministic replay: true
- Public GamePackage schema changed: false

## gameplay_combined_loop

- Expected valid: true
- Actual valid: true
- Families: family/items, family/inventory, family/equipment, family/crafting, family/trading, family/status_effects
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/equip_item:item/scavenger_tool -> gameplay/craft_recipe:recipe/repair_wrap -> gameplay/execute_transaction:transaction/buy_signal_charm -> gameplay/use_item:item/focus_tonic -> gameplay/set_flag:flag/goal008_complete
- Diagnostics: 

## gameplay_crafting_recipe

- Expected valid: true
- Actual valid: true
- Families: family/crafting, family/resource_conversion
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/craft_recipe:recipe/repair_wrap
- Diagnostics: 

## gameplay_equipment_loadout

- Expected valid: true
- Actual valid: true
- Families: family/equipment, family/loadout
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/equip_item:item/scavenger_tool
- Diagnostics: 

## gameplay_inventory_item_use

- Expected valid: true
- Actual valid: true
- Families: family/items, family/inventory, family/item_use, family/status_effects
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/use_item:item/field_ration
- Diagnostics: 

## gameplay_status_effect_chain

- Expected valid: true
- Actual valid: true
- Families: family/status_effects, family/item_use
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/use_item:item/focus_tonic
- Diagnostics: 

## gameplay_trading_transaction

- Expected valid: true
- Actual valid: true
- Families: family/trading, family/transaction
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/execute_transaction:transaction/buy_signal_charm
- Diagnostics: 

## invalid_command_target_not_declared

- Expected valid: false
- Actual valid: false
- Families: family/item_use, family/status_effects
- Runtime boundary:  / 
- Commands: 
- Diagnostics: gameplay_family.audit.command_target_not_declared, gameplay_family.evidence.runtime_not_attempted

## invalid_crafting_missing_inputs

- Expected valid: false
- Actual valid: false
- Families: family/crafting
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/craft_recipe:recipe/repair_wrap
- Diagnostics: gameplay_family.evidence.required_command_failed, gameplay_family.evidence.state_delta_missing, gameplay_family.runtime_command_failed

## invalid_equipment_slot_mismatch

- Expected valid: false
- Actual valid: false
- Families: family/equipment
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/equip_item:item/scavenger_tool
- Diagnostics: gameplay_family.audit.command_target_not_declared, gameplay_family.evidence.required_command_failed, gameplay_family.evidence.state_delta_missing, gameplay_family.runtime_command_failed

## invalid_fake_runtime_success

- Expected valid: false
- Actual valid: false
- Families: family/items, family/equipment, family/crafting, family/trading, family/status_effects
- Runtime boundary:  / 
- Commands: 
- Diagnostics: gameplay_family.audit.command_target_not_declared, gameplay_family.evidence.real_runtime_boundary_missing, gameplay_family.evidence.required_command_missing, gameplay_family.evidence.serializer_not_used

## invalid_missing_item_or_recipe_ref

- Expected valid: false
- Actual valid: false
- Families: family/crafting
- Runtime boundary:  / 
- Commands: 
- Diagnostics: gameplay_family.audit.missing_item_ref, gameplay_family.evidence.runtime_not_attempted

## invalid_save_load_mismatch

- Expected valid: false
- Actual valid: false
- Families: family/status_effects, family/item_use
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/use_item:item/focus_tonic
- Diagnostics: gameplay_family.evidence.restored_hash_mismatch, gameplay_family.evidence.save_load_mismatch

## invalid_status_duration_mismatch

- Expected valid: false
- Actual valid: false
- Families: family/status_effects, family/item_use
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/use_item:item/focus_tonic
- Diagnostics: gameplay_family.audit.status_duration_mismatch

## invalid_status_or_effect_binding

- Expected valid: false
- Actual valid: false
- Families: family/status_effects
- Runtime boundary:  / 
- Commands: 
- Diagnostics: gameplay_family.audit.command_target_not_declared, gameplay_family.audit.invalid_status_effect_binding, gameplay_family.evidence.runtime_not_attempted

## invalid_trade_insufficient_cost

- Expected valid: false
- Actual valid: false
- Families: family/trading
- Runtime boundary: real_game_runtime_service_adapter / LLMGameCreator.Runtime.GameRuntimeService
- Commands: gameplay/execute_transaction:transaction/buy_signal_charm
- Diagnostics: gameplay_family.evidence.required_command_failed, gameplay_family.evidence.state_delta_missing, gameplay_family.runtime_command_failed

## invalid_unknown_source_declaration

- Expected valid: false
- Actual valid: false
- Families: family/item_use, family/status_effects
- Runtime boundary:  / 
- Commands: 
- Diagnostics: gameplay_family.audit.unknown_source_declaration, gameplay_family.evidence.runtime_not_attempted

