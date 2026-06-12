local examples = {}

examples.combat_schema_input = {
  schema_id = "combat/schema/example",
  combat_mode = "turn_based",
  turn_mode = "turn_based",
  resources = {
    { id = "combat/resource/hp", label = "HP", min = 0, max = 120, ui_role = "bar" },
    { id = "combat/resource/morale", label = "Morale", min = 0, max = 100, ui_role = "bar" }
  },
  actions = {
    { id = "combat/action/strike", label = "Strike", action_type = "damage", target_rule = "single_enemy", formula_ref = "formula/combat/basic_attack", costs = { action_points = 1 }, tags = { "basic" } }
  },
  dialogue_combat_bridge = true
}

examples.turn_based_input = {
  combat_config_id = "combat/config/example_duel",
  action_points_per_turn = 2,
  cooldown_tick_timing = "start_of_actor_turn",
  status_tick_timing = "end_of_actor_turn",
  dialogue_combat_enabled = true,
  sides = {
    { id = "combat/side/player", label = "Player" },
    { id = "combat/side/rival", label = "Rival" }
  }
}

examples.status_effects_input = {
  catalog_id = "status_effect/catalog/example",
  effects = {
    { id = "status_effect/guarded", label = "Guarded", duration_ticks = 1, stacking = "refresh", modifiers = { { stat = "defense", op = "add", value = 2 } } },
    { id = "status_effect/shaken", label = "Shaken", duration_ticks = 2, stacking = "refresh", modifiers = { { stat = "morale", op = "add", value = -10 } } }
  }
}

examples.ability_catalog_input = {
  catalog_id = "ability/catalog/example",
  abilities = {
    { id = "ability/strike", label = "Strike", target_rule = "single_enemy", costs = { action_points = 1 }, formula_refs = { "formula/combat/basic_attack" } },
    { id = "ability/guard", label = "Guard", target_rule = "self", costs = { action_points = 1 }, cooldown_ticks = 1, apply_status_effects = { "status_effect/guarded" } }
  }
}

examples.expected_shapes = {
  combat_schema = { ok = true, data_keys = { "schema_id", "combat_mode", "resources", "actions" } },
  turn_based_combat = { ok = true, data_keys = { "combat_config_id", "sides", "action_points", "cooldowns", "status_duration" } },
  status_effects = { ok = true, data_keys = { "catalog_id", "tick_model", "effects" } },
  ability_catalog = { ok = true, data_keys = { "catalog_id", "formula_contract", "abilities" } }
}

return examples
