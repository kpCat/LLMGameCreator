local M = {}

M.manifest = {
  id = "combat/combat_schema/v1",
  version = "0.1.0",
  category = "combat",
  title = "Combat Schema",
  purpose = "Defines compact combat mode, actor, resource, action and formula-reference schemas for generator outputs.",
  capabilities = { "combat.schema.define", "combat.mode.configure", "combat.formula_reference" },
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

local function is_array(value)
  if type(value) ~= "table" then return false end
  local count = 0
  for k, _ in pairs(value) do
    if type(k) ~= "number" or k < 1 or k % 1 ~= 0 then return false end
    if k > count then count = k end
  end
  for i = 1, count do
    if value[i] == nil then return false end
  end
  return true
end

local function has_value(list, value)
  if type(list) ~= "table" then return false end
  for i = 1, #list do
    if list[i] == value then return true end
  end
  return false
end

local function copy_array(list)
  local result = {}
  if type(list) == "table" then
    for i = 1, #list do result[i] = list[i] end
  end
  return result
end

local function validate_id(value, diagnostics, target)
  if type(value) ~= "string" or value == "" then
    diagnostics[#diagnostics + 1] = diag("error", "combat_schema.invalid_id", "Expected a non-empty lowercase slash id.", target)
    return false
  end
  if value:match("^[a-z0-9_]+(/[a-z0-9_]+)*$") == nil then
    diagnostics[#diagnostics + 1] = diag("error", "combat_schema.invalid_id_format", "Id must use lowercase slash notation.", target)
    return false
  end
  return true
end

local function normalize_resource(resource, diagnostics, index)
  local target = "resources[" .. tostring(index) .. "]"
  if type(resource) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "combat_schema.invalid_resource", "Resource entry must be a table.", target)
    return nil
  end
  validate_id(resource.id, diagnostics, target .. ".id")
  local max_value = resource.max
  if type(max_value) ~= "number" or max_value < 0 then
    diagnostics[#diagnostics + 1] = diag("error", "combat_schema.invalid_resource_max", "Resource max must be a non-negative number.", target .. ".max")
    max_value = 0
  end
  return {
    id = resource.id,
    label = resource.label or resource.id,
    min = type(resource.min) == "number" and resource.min or 0,
    max = max_value,
    starts_full = resource.starts_full ~= false,
    ui_role = resource.ui_role or "bar"
  }
end

local function normalize_action(action, diagnostics, index)
  local target = "actions[" .. tostring(index) .. "]"
  if type(action) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "combat_schema.invalid_action", "Action entry must be a table.", target)
    return nil
  end
  validate_id(action.id, diagnostics, target .. ".id")
  local costs = {}
  if type(action.costs) == "table" then
    for k, v in pairs(action.costs) do
      if type(k) == "string" and type(v) == "number" then costs[k] = v end
    end
  end
  return {
    id = action.id,
    label = action.label or action.id,
    action_type = action.action_type or "standard",
    target_rule = action.target_rule or "single_enemy",
    formula_ref = action.formula_ref,
    costs = costs,
    tags = copy_array(action.tags),
    effects = copy_array(action.effects)
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "combat_schema.config_not_table", "Config must be a table when provided.", "config")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local source = type(input) == "table" and input or {}
  if input ~= nil and type(input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "combat_schema.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local combat_mode = source.combat_mode or "turn_based"
  local allowed_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" }
  if not has_value(allowed_modes, combat_mode) then
    diagnostics[#diagnostics + 1] = diag("error", "combat_schema.unsupported_combat_mode", "Unsupported combat mode.", "combat_mode")
  end

  local resources = {}
  local resource_input = source.resources or {
    { id = "combat/resource/hp", label = "HP", min = 0, max = 100, ui_role = "bar" },
    { id = "combat/resource/action_points", label = "Action Points", min = 0, max = 2, ui_role = "counter" }
  }
  if not is_array(resource_input) then
    diagnostics[#diagnostics + 1] = diag("error", "combat_schema.resources_not_array", "Resources must be an array.", "resources")
  else
    for i = 1, #resource_input do
      resources[#resources + 1] = normalize_resource(resource_input[i], diagnostics, i)
    end
  end

  local actions = {}
  local action_input = source.actions or {
    { id = "combat/action/attack", label = "Attack", action_type = "damage", target_rule = "single_enemy", formula_ref = "formula/combat/basic_attack", costs = { action_points = 1 }, tags = { "basic" } },
    { id = "combat/action/guard", label = "Guard", action_type = "defense", target_rule = "self", costs = { action_points = 1 }, tags = { "defense" } }
  }
  if not is_array(action_input) then
    diagnostics[#diagnostics + 1] = diag("error", "combat_schema.actions_not_array", "Actions must be an array.", "actions")
  else
    for i = 1, #action_input do
      actions[#actions + 1] = normalize_action(action_input[i], diagnostics, i)
    end
  end

  return {
    ok = #diagnostics == 0,
    data = {
      schema_id = source.schema_id or "combat/schema/default",
      combat_mode = combat_mode,
      turn_mode = source.turn_mode or "turn_based",
      resources = resources,
      actions = actions,
      formula_contract = "formula_ref points to safe formula IR; this module never executes raw code.",
      dialogue_combat_bridge = source.dialogue_combat_bridge == true
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
