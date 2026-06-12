local M = {}

M.manifest = {
  id = "ability/ability_catalog_generator/v1",
  version = "0.1.0",
  category = "ability",
  title = "Ability Catalog Generator",
  purpose = "Generates compact ability definitions that reference safe formula IR, status effects, cooldowns and action point costs.",
  capabilities = { "ability.catalog.generate", "ability.cooldown", "combat.ability_reference", "formula.damage_reference" },
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

local function normalize_costs(costs)
  local result = {}
  if type(costs) == "table" then
    for k, v in pairs(costs) do
      if type(k) == "string" and type(v) == "number" then result[k] = v end
    end
  end
  return result
end

local function normalize_ability(ability, diagnostics, index)
  local target = "abilities[" .. tostring(index) .. "]"
  if type(ability) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "ability_catalog.ability_not_table", "Ability must be a table.", target)
    return nil
  end
  if not valid_id(ability.id) then
    diagnostics[#diagnostics + 1] = diag("error", "ability_catalog.invalid_id", "Ability id must use lowercase slash notation.", target .. ".id")
  end
  local cooldown = ability.cooldown_ticks or 0
  if type(cooldown) ~= "number" or cooldown < 0 or cooldown % 1 ~= 0 then
    diagnostics[#diagnostics + 1] = diag("error", "ability_catalog.invalid_cooldown", "cooldown_ticks must be a non-negative integer.", target .. ".cooldown_ticks")
    cooldown = 0
  end
  return {
    id = ability.id,
    label = ability.label or ability.id,
    ability_type = ability.ability_type or "active",
    target_rule = ability.target_rule or "single_enemy",
    action_tags = copy_array(ability.action_tags or { "ability" }),
    costs = normalize_costs(ability.costs or { action_points = 1 }),
    cooldown_ticks = cooldown,
    formula_refs = copy_array(ability.formula_refs),
    apply_status_effects = copy_array(ability.apply_status_effects),
    effects = copy_array(ability.effects),
    unlock_ref = ability.unlock_ref,
    ui = {
      icon_slot = ability.icon_slot or "default",
      description = ability.description or "Generated ability definition."
    }
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "ability_catalog.config_not_table", "Config must be a table when provided.", "config")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local source = type(input) == "table" and input or {}
  if input ~= nil and type(input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "ability_catalog.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local ability_input = source.abilities or {
    { id = "ability/strike", label = "Strike", ability_type = "active", target_rule = "single_enemy", costs = { action_points = 1 }, cooldown_ticks = 0, formula_refs = { "formula/combat/basic_attack" }, effects = { { effect_type = "damage", formula_ref = "formula/combat/basic_attack" } } },
    { id = "ability/guard", label = "Guard", ability_type = "active", target_rule = "self", costs = { action_points = 1 }, cooldown_ticks = 1, apply_status_effects = { "status_effect/guarded" } },
    { id = "ability/taunt", label = "Taunt", ability_type = "dialogue_combat", target_rule = "single_enemy", costs = { action_points = 1 }, cooldown_ticks = 2, formula_refs = { "formula/combat/morale_pressure" }, effects = { { effect_type = "resource_delta", resource = "morale", formula_ref = "formula/combat/morale_pressure" } } }
  }
  if type(ability_input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "ability_catalog.abilities_not_table", "Abilities must be an array table.", "abilities")
    ability_input = {}
  end
  local abilities = {}
  local ids = {}
  for i = 1, #ability_input do
    local normalized = normalize_ability(ability_input[i], diagnostics, i)
    if normalized ~= nil then
      if ids[normalized.id] then
        diagnostics[#diagnostics + 1] = diag("error", "ability_catalog.duplicate_id", "Duplicate ability id.", "abilities[" .. tostring(i) .. "].id")
      end
      ids[normalized.id] = true
      abilities[#abilities + 1] = normalized
    end
  end
  return {
    ok = #diagnostics == 0,
    data = {
      catalog_id = source.catalog_id or "ability/catalog/default",
      formula_contract = "formula_refs are identifiers for safe formula IR and are not executed by this module",
      abilities = abilities
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
