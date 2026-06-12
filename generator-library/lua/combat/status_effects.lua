local M = {}

M.manifest = {
  id = "combat/status_effects/v1",
  version = "0.1.0",
  category = "combat",
  title = "Status Effects",
  purpose = "Generates compact status effect definitions with deterministic duration, stacking, tick and modifier metadata.",
  capabilities = { "combat.status_effect.generate", "status_effect.duration", "stats.modifier_reference" },
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

local function normalize_effect(effect, diagnostics, index)
  local target = "effects[" .. tostring(index) .. "]"
  if type(effect) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "status_effects.effect_not_table", "Effect must be a table.", target)
    return nil
  end
  if not valid_id(effect.id) then
    diagnostics[#diagnostics + 1] = diag("error", "status_effects.invalid_id", "Effect id must use lowercase slash notation.", target .. ".id")
  end
  local duration = effect.duration_ticks
  if type(duration) ~= "number" or duration < 0 or duration % 1 ~= 0 then
    diagnostics[#diagnostics + 1] = diag("error", "status_effects.invalid_duration", "duration_ticks must be a non-negative integer.", target .. ".duration_ticks")
    duration = 0
  end
  local stacking = effect.stacking or "refresh"
  if stacking ~= "refresh" and stacking ~= "stack_intensity" and stacking ~= "ignore" and stacking ~= "replace" then
    diagnostics[#diagnostics + 1] = diag("error", "status_effects.invalid_stacking", "Unsupported stacking rule.", target .. ".stacking")
    stacking = "refresh"
  end
  return {
    id = effect.id,
    label = effect.label or effect.id,
    duration_ticks = duration,
    tick_timing = effect.tick_timing or "end_of_turn",
    stacking = stacking,
    max_stacks = type(effect.max_stacks) == "number" and effect.max_stacks or 1,
    tags = copy_array(effect.tags),
    modifiers = copy_array(effect.modifiers),
    tick_effects = copy_array(effect.tick_effects),
    expire_effects = copy_array(effect.expire_effects),
    formula_refs = copy_array(effect.formula_refs)
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "status_effects.config_not_table", "Config must be a table when provided.", "config")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local source = type(input) == "table" and input or {}
  if input ~= nil and type(input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "status_effects.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local effect_input = source.effects or {
    { id = "status_effect/guarded", label = "Guarded", duration_ticks = 1, stacking = "refresh", tags = { "defense" }, modifiers = { { stat = "defense", op = "add", value = 2 } } },
    { id = "status_effect/bleeding", label = "Bleeding", duration_ticks = 3, stacking = "stack_intensity", max_stacks = 3, tags = { "damage_over_time" }, tick_effects = { { effect_type = "damage", formula_ref = "formula/combat/bleed_tick" } } },
    { id = "status_effect/shaken", label = "Shaken", duration_ticks = 2, stacking = "refresh", tags = { "dialogue_combat", "morale" }, modifiers = { { stat = "morale", op = "add", value = -10 } } }
  }
  if type(effect_input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "status_effects.effects_not_table", "Effects must be an array table.", "effects")
    effect_input = {}
  end
  local effects = {}
  local ids = {}
  for i = 1, #effect_input do
    local normalized = normalize_effect(effect_input[i], diagnostics, i)
    if normalized ~= nil then
      if ids[normalized.id] then
        diagnostics[#diagnostics + 1] = diag("error", "status_effects.duplicate_id", "Duplicate status effect id.", "effects[" .. tostring(i) .. "].id")
      end
      ids[normalized.id] = true
      effects[#effects + 1] = normalized
    end
  end
  return {
    ok = #diagnostics == 0,
    data = {
      catalog_id = source.catalog_id or "status_effect/catalog/default",
      tick_model = source.tick_model or "turn_ticks",
      effects = effects
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
