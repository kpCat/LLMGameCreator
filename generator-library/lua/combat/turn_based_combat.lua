local M = {}

M.manifest = {
  id = "combat/turn_based_combat/v1",
  version = "0.1.0",
  category = "combat",
  title = "Turn Based Combat",
  purpose = "Generates deterministic turn-based combat configuration IR with action points, initiative, cooldown and dialogue-combat hooks.",
  capabilities = { "combat.turn_based.configure", "combat.action_points", "combat.cooldown_model", "dialogue.combat_bridge" },
  input_schema = {},
  output_schema = {},
  config_schema = {},
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function copy_array(list)
  local result = {}
  if type(list) == "table" then
    for i = 1, #list do result[i] = list[i] end
  end
  return result
end

local function valid_id(value)
  return type(value) == "string" and value:match("^[a-z0-9_]+(/[a-z0-9_]+)*$") ~= nil
end

local function normalize_side(side, diagnostics, index)
  local target = "sides[" .. tostring(index) .. "]"
  if type(side) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "turn_based_combat.side_not_table", "Side must be a table.", target)
    return nil
  end
  if not valid_id(side.id) then
    diagnostics[#diagnostics + 1] = diag("error", "turn_based_combat.invalid_side_id", "Side id must use lowercase slash notation.", target .. ".id")
  end
  return {
    id = side.id,
    label = side.label or side.id,
    actor_query = side.actor_query or "tag:" .. tostring(side.id),
    win_condition = side.win_condition or "opponents_defeated",
    lose_condition = side.lose_condition or "all_actors_defeated"
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "turn_based_combat.config_not_table", "Config must be a table when provided.", "config")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local source = type(input) == "table" and input or {}
  if input ~= nil and type(input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "turn_based_combat.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local action_points = source.action_points_per_turn or 2
  if type(action_points) ~= "number" or action_points < 1 or action_points % 1 ~= 0 then
    diagnostics[#diagnostics + 1] = diag("error", "turn_based_combat.invalid_action_points", "action_points_per_turn must be a positive integer.", "action_points_per_turn")
    action_points = 2
  end

  local side_input = source.sides or {
    { id = "combat/side/player", label = "Player" },
    { id = "combat/side/enemy", label = "Enemy" }
  }
  local sides = {}
  if type(side_input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "turn_based_combat.sides_not_table", "Sides must be an array table.", "sides")
  else
    for i = 1, #side_input do
      sides[#sides + 1] = normalize_side(side_input[i], diagnostics, i)
    end
  end

  local initiative = source.initiative or { mode = "stat_then_stable_order", stat = "speed", tie_breaker = "actor_id" }
  if type(initiative) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "turn_based_combat.initiative_not_table", "Initiative must be a table.", "initiative")
    initiative = { mode = "stable_order" }
  end

  return {
    ok = #diagnostics == 0,
    data = {
      combat_config_id = source.combat_config_id or "combat/config/turn_based_default",
      combat_mode = "turn_based",
      turn_mode = "turn_based",
      sides = sides,
      action_points = {
        per_turn = action_points,
        carry_over = source.carry_over_action_points == true,
        max_carried = type(source.max_carried_action_points) == "number" and source.max_carried_action_points or 0
      },
      initiative = initiative,
      cooldowns = {
        tick_timing = source.cooldown_tick_timing or "start_of_actor_turn",
        reduce_by = 1
      },
      status_duration = {
        tick_timing = source.status_tick_timing or "end_of_actor_turn",
        reduce_by = 1
      },
      allowed_action_tags = copy_array(source.allowed_action_tags or { "basic", "ability", "item", "dialogue_combat" }),
      dialogue_combat_bridge = {
        enabled = source.dialogue_combat_enabled == true,
        resources = copy_array(source.dialogue_combat_resources or { "morale", "trust", "suspicion", "focus" }),
        choice_effect_contract = "dialogue choices may emit combat effect IR, never executable code"
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
