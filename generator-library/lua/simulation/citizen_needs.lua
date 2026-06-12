local M = {}

M.manifest = {
  id = "simulation/citizen_needs/v1",
  version = "0.1.0",
  category = "simulation",
  title = "Citizen Needs",
  purpose = "Generate deterministic citizen need profile IR for city-builder simulation configs.",
  capabilities = { "city_builder.citizen_needs.generate", "city_builder.needs.validate", "simulation.tick_metadata" },
  input_schema = { kind = "city_builder.citizen_needs.input" },
  output_schema = { kind = "city_builder.citizen_needs.ir" },
  config_schema = { kind = "city_builder.citizen_needs.config" },
  deterministic = true,
  runtime_targets = { "editor", "simulation", "unity2d", "unity3d", "codegen_ir" },
  unsafe_features = {}
}

local function diag(list, severity, code, message, target)
  list[#list + 1] = { severity = severity, code = code, message = message, target = target }
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

local function valid_id(value)
  return type(value) == "string" and value:match("^[a-z][a-z0-9_]*(/[a-z][a-z0-9_]*)*$") ~= nil
end

local function clone_array(value)
  local result = {}
  if type(value) == "table" then
    for i = 1, #value do result[i] = value[i] end
  end
  return result
end

local function default_needs()
  return {
    {
      id = "need/food",
      category = "survival",
      priority = 100,
      weight = 1.0,
      decay_per_tick = 0.02,
      thresholds = { low = 0.25, warning = 0.45, satisfied = 0.8 },
      satisfaction_sources = { "service/market", "building/kitchen" }
    },
    {
      id = "need/rest",
      category = "housing",
      priority = 80,
      weight = 0.8,
      decay_per_tick = 0.015,
      thresholds = { low = 0.2, warning = 0.4, satisfied = 0.75 },
      satisfaction_sources = { "building/home" }
    },
    {
      id = "need/safety",
      category = "security",
      priority = 60,
      weight = 0.6,
      decay_per_tick = 0.01,
      thresholds = { low = 0.3, warning = 0.5, satisfied = 0.85 },
      satisfaction_sources = { "service/watch" }
    }
  }
end

local function validate_need(need, index, diagnostics)
  local target = "needs[" .. index .. "]"
  if type(need) ~= "table" then
    diag(diagnostics, "error", "citizen_needs.need_not_table", "Need entry must be a table.", target)
    return
  end
  if not valid_id(need.id) then
    diag(diagnostics, "error", "citizen_needs.invalid_id", "Need id must use lowercase slash notation.", target .. ".id")
  end
  if type(need.category) ~= "string" or need.category == "" then
    diag(diagnostics, "error", "citizen_needs.invalid_category", "Need category is required.", target .. ".category")
  end
  if type(need.weight) ~= "number" or need.weight < 0 then
    diag(diagnostics, "error", "citizen_needs.invalid_weight", "Need weight must be a non-negative number.", target .. ".weight")
  end
  if need.priority ~= nil and (type(need.priority) ~= "number" or need.priority < 0) then
    diag(diagnostics, "error", "citizen_needs.invalid_priority", "Need priority must be a non-negative number.", target .. ".priority")
  end
  if need.decay_per_tick ~= nil and (type(need.decay_per_tick) ~= "number" or need.decay_per_tick < 0) then
    diag(diagnostics, "error", "citizen_needs.invalid_decay", "Need decay per tick must be non-negative.", target .. ".decay_per_tick")
  end
  local thresholds = need.thresholds
  if type(thresholds) ~= "table" then
    diag(diagnostics, "error", "citizen_needs.missing_thresholds", "Need thresholds table is required.", target .. ".thresholds")
  else
    local low = thresholds.low
    local warning = thresholds.warning
    local satisfied = thresholds.satisfied
    if type(low) ~= "number" or low < 0 or low > 1 then
      diag(diagnostics, "error", "citizen_needs.invalid_low_threshold", "Low threshold must be in range 0..1.", target .. ".thresholds.low")
    end
    if type(warning) ~= "number" or warning < 0 or warning > 1 then
      diag(diagnostics, "error", "citizen_needs.invalid_warning_threshold", "Warning threshold must be in range 0..1.", target .. ".thresholds.warning")
    end
    if type(satisfied) ~= "number" or satisfied < 0 or satisfied > 1 then
      diag(diagnostics, "error", "citizen_needs.invalid_satisfied_threshold", "Satisfied threshold must be in range 0..1.", target .. ".thresholds.satisfied")
    end
    if type(low) == "number" and type(warning) == "number" and low > warning then
      diag(diagnostics, "error", "citizen_needs.threshold_order", "Low threshold must not exceed warning threshold.", target .. ".thresholds")
    end
    if type(warning) == "number" and type(satisfied) == "number" and warning > satisfied then
      diag(diagnostics, "error", "citizen_needs.threshold_order", "Warning threshold must not exceed satisfied threshold.", target .. ".thresholds")
    end
  end
  if need.satisfaction_sources ~= nil and not is_array(need.satisfaction_sources) then
    diag(diagnostics, "error", "citizen_needs.invalid_sources", "Satisfaction sources must be an array.", target .. ".satisfaction_sources")
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then return true, diagnostics end
  if type(config) ~= "table" then
    diag(diagnostics, "error", "citizen_needs.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.tick_mode ~= nil and type(config.tick_mode) ~= "string" then
    diag(diagnostics, "error", "citizen_needs.invalid_tick_mode", "Tick mode must be a string.", "tick_mode")
  end
  if config.needs ~= nil then
    if not is_array(config.needs) then
      diag(diagnostics, "error", "citizen_needs.needs_not_array", "Needs must be an array.", "needs")
    else
      local seen = {}
      for i = 1, #config.needs do
        local need = config.needs[i]
        validate_need(need, i, diagnostics)
        if type(need) == "table" and type(need.id) == "string" then
          if seen[need.id] then
            diag(diagnostics, "error", "citizen_needs.duplicate_id", "Duplicate need id.", "needs[" .. i .. "].id")
          end
          seen[need.id] = true
        end
      end
    end
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local config = {}
  if type(input) == "table" and type(input.config) == "table" then config = input.config elseif type(input) == "table" then config = input end
  local ok, diagnostics = M.validate_config(config)
  if not ok then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local source = config.needs or default_needs()
  local needs = {}
  for i = 1, #source do
    local item = source[i]
    needs[i] = {
      id = item.id,
      category = item.category,
      priority = item.priority or 0,
      weight = item.weight or 0,
      decay_per_tick = item.decay_per_tick or 0,
      thresholds = {
        low = item.thresholds.low,
        warning = item.thresholds.warning,
        satisfied = item.thresholds.satisfied
      },
      satisfaction_sources = clone_array(item.satisfaction_sources),
      tags = clone_array(item.tags)
    }
  end
  local data = {
    schema = "city_builder.citizen_needs.v1",
    tick_mode = config.tick_mode or "simulation_tick",
    citizen_profile_id = config.citizen_profile_id or "citizen/default",
    needs = needs,
    metadata = {
      deterministic = true,
      generated_by = M.manifest.id,
      notes = "Need values are config IR; host runtime owns live state updates."
    }
  }
  return { ok = true, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
