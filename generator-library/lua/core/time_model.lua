local M = {}

M.manifest = {
  id = "core/time_model/v1",
  version = "0.1.0",
  category = "core",
  title = "Time mode model",
  purpose = "Normalize deterministic time, turn mode, combat mode, and world mode metadata for game generator modules.",
  capabilities = {
    "core.time_model.create",
    "core.time_model.advance",
    "core.time_model.mode_support",
    "core.time_model.tick_units"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      turn_mode = { type = "string", enum = { "realtime", "turn_based", "mixed", "paused_planning" } },
      combat_mode = { type = "string", enum = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" } },
      simulation_tick_seconds = { type = "number", min = 0 },
      cooldown_tick_unit = { type = "string", enum = { "turn", "round", "second", "simulation_tick" } },
      status_tick_unit = { type = "string", enum = { "turn", "round", "second", "simulation_tick" } }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  unsafe_features = {}
}

local TURN_MODES = {
  realtime = true,
  turn_based = true,
  mixed = true,
  paused_planning = true
}

local COMBAT_MODES = {
  none = true,
  realtime = true,
  turn_based = true,
  tactical = true,
  dialogue_combat = true,
  hybrid = true
}

local ACTIVE_MODES = {
  exploration = true,
  combat = true,
  dialogue = true,
  dialogue_combat = true,
  paused_planning = true
}

local TICK_UNITS = {
  turn = true,
  round = true,
  second = true,
  simulation_tick = true
}

local function make_diagnostic(code, message, target)
  local diagnostic = {
    severity = "error",
    code = code,
    message = message
  }
  if target ~= nil then
    diagnostic.target = target
  end
  return diagnostic
end

local function make_warning(code, message, target)
  local diagnostic = make_diagnostic(code, message, target)
  diagnostic.severity = "warning"
  return diagnostic
end

local function result(ok, data, diagnostics)
  return {
    ok = ok == true,
    data = type(data) == "table" and data or {},
    diagnostics = type(diagnostics) == "table" and diagnostics or {},
    artifacts = {}
  }
end

local function is_integer(value)
  return type(value) == "number" and value == (value // 1)
end

local function copy_table(value, depth)
  if type(value) ~= "table" then
    return value
  end
  if depth > 16 then
    return {}
  end
  local copy = {}
  for key, item in pairs(value) do
    copy[key] = copy_table(item, depth + 1)
  end
  return copy
end

local function append_diagnostic_if_invalid_enum(diagnostics, value, allowed, code, message, target)
  if value ~= nil and allowed[value] ~= true then
    diagnostics[#diagnostics + 1] = make_diagnostic(code, message, target)
  end
end

local function normalize_config(config)
  local source = type(config) == "table" and config or {}
  local model = {
    turn_mode = source.turn_mode or "realtime",
    combat_mode = source.combat_mode or "none",
    active_mode = source.active_mode or "exploration",
    simulation_tick_seconds = source.simulation_tick_seconds or 1,
    cooldown_tick_unit = source.cooldown_tick_unit or "turn",
    status_tick_unit = source.status_tick_unit or "turn",
    tick = is_integer(source.tick) and source.tick or 0,
    elapsed_seconds = type(source.elapsed_seconds) == "number" and source.elapsed_seconds or 0,
    round = is_integer(source.round) and source.round or 1,
    global_turn = is_integer(source.global_turn) and source.global_turn or 1,
    paused = source.paused == true,
    supported_turn_modes = type(source.supported_turn_modes) == "table" and copy_table(source.supported_turn_modes, 0) or { source.turn_mode or "realtime" },
    supported_combat_modes = type(source.supported_combat_modes) == "table" and copy_table(source.supported_combat_modes, 0) or { source.combat_mode or "none" }
  }
  if model.turn_mode == "paused_planning" then
    model.paused = true
    model.active_mode = "paused_planning"
  end
  return model
end

local function validate_mode_array(values, allowed, code, message, target, diagnostics)
  if values == nil then
    return
  end
  if type(values) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic(code .. ".not_array", message .. " must be an array table.", target)
    return
  end
  for index = 1, #values do
    if allowed[values[index]] ~= true then
      diagnostics[#diagnostics + 1] = make_diagnostic(code .. ".invalid", message .. " contains an unsupported value.", target .. "." .. tostring(index))
    end
  end
end

function M.supported_turn_modes()
  return { "realtime", "turn_based", "mixed", "paused_planning" }
end

function M.supported_combat_modes()
  return { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" }
end

function M.supported_active_modes()
  return { "exploration", "combat", "dialogue", "dialogue_combat", "paused_planning" }
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.time_model.config_not_table", "Time model config must be a table.", "config")
    return false, diagnostics
  end

  append_diagnostic_if_invalid_enum(diagnostics, config.turn_mode, TURN_MODES, "core.time_model.invalid_turn_mode", "turn_mode must be realtime, turn_based, mixed, or paused_planning.", "config.turn_mode")
  append_diagnostic_if_invalid_enum(diagnostics, config.combat_mode, COMBAT_MODES, "core.time_model.invalid_combat_mode", "combat_mode is not supported.", "config.combat_mode")
  append_diagnostic_if_invalid_enum(diagnostics, config.active_mode, ACTIVE_MODES, "core.time_model.invalid_active_mode", "active_mode must be exploration, combat, dialogue, dialogue_combat, or paused_planning.", "config.active_mode")
  append_diagnostic_if_invalid_enum(diagnostics, config.cooldown_tick_unit, TICK_UNITS, "core.time_model.invalid_cooldown_tick_unit", "cooldown_tick_unit is not supported.", "config.cooldown_tick_unit")
  append_diagnostic_if_invalid_enum(diagnostics, config.status_tick_unit, TICK_UNITS, "core.time_model.invalid_status_tick_unit", "status_tick_unit is not supported.", "config.status_tick_unit")

  if config.simulation_tick_seconds ~= nil and (type(config.simulation_tick_seconds) ~= "number" or config.simulation_tick_seconds <= 0) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.time_model.invalid_tick_seconds", "simulation_tick_seconds must be a positive number.", "config.simulation_tick_seconds")
  end
  if config.tick ~= nil and (not is_integer(config.tick) or config.tick < 0) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.time_model.invalid_tick", "tick must be a non-negative integer.", "config.tick")
  end
  if config.elapsed_seconds ~= nil and (type(config.elapsed_seconds) ~= "number" or config.elapsed_seconds < 0) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.time_model.invalid_elapsed_seconds", "elapsed_seconds must be a non-negative number.", "config.elapsed_seconds")
  end
  if config.round ~= nil and (not is_integer(config.round) or config.round < 1) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.time_model.invalid_round", "round must be a positive integer.", "config.round")
  end
  if config.global_turn ~= nil and (not is_integer(config.global_turn) or config.global_turn < 1) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.time_model.invalid_global_turn", "global_turn must be a positive integer.", "config.global_turn")
  end

  validate_mode_array(config.supported_turn_modes, TURN_MODES, "core.time_model.supported_turn_modes", "supported_turn_modes", "config.supported_turn_modes", diagnostics)
  validate_mode_array(config.supported_combat_modes, COMBAT_MODES, "core.time_model.supported_combat_modes", "supported_combat_modes", "config.supported_combat_modes", diagnostics)

  if config.turn_mode == "realtime" and config.combat_mode == "turn_based" then
    diagnostics[#diagnostics + 1] = make_warning("core.time_model.realtime_turn_based_combat", "Realtime exploration with turn-based combat should usually use turn_mode=mixed.", "config.turn_mode")
  end
  if config.combat_mode == "dialogue_combat" and config.active_mode ~= nil and config.active_mode ~= "dialogue_combat" and config.active_mode ~= "dialogue" then
    diagnostics[#diagnostics + 1] = make_warning("core.time_model.dialogue_combat_active_mode", "dialogue_combat is normally entered from dialogue or dialogue_combat active mode.", "config.active_mode")
  end

  for key, _ in pairs(config) do
    if key ~= "turn_mode" and key ~= "combat_mode" and key ~= "active_mode" and key ~= "simulation_tick_seconds" and key ~= "cooldown_tick_unit" and key ~= "status_tick_unit" and key ~= "tick" and key ~= "elapsed_seconds" and key ~= "round" and key ~= "global_turn" and key ~= "paused" and key ~= "supported_turn_modes" and key ~= "supported_combat_modes" then
      diagnostics[#diagnostics + 1] = make_warning("core.time_model.unknown_config_key", "Unknown config key is ignored by the time model.", "config." .. tostring(key))
    end
  end

  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      return false, diagnostics
    end
  end
  return true, diagnostics
end

function M.create(config)
  local ok, diagnostics = M.validate_config(config)
  if not ok then
    return result(false, {}, diagnostics)
  end
  return result(true, { model = normalize_config(config), warnings = diagnostics }, diagnostics)
end

function M.validate_model(model)
  if type(model) ~= "table" then
    return false, { make_diagnostic("core.time_model.model_not_table", "Time model must be a table.", "model") }
  end
  return M.validate_config(model)
end

function M.supports_turn_mode(model_or_config, mode)
  if TURN_MODES[mode] ~= true then
    return false
  end
  local supported = type(model_or_config) == "table" and model_or_config.supported_turn_modes or nil
  if type(supported) ~= "table" then
    return model_or_config == nil or model_or_config.turn_mode == mode
  end
  for index = 1, #supported do
    if supported[index] == mode then
      return true
    end
  end
  return false
end

function M.supports_combat_mode(model_or_config, mode)
  if COMBAT_MODES[mode] ~= true then
    return false
  end
  local supported = type(model_or_config) == "table" and model_or_config.supported_combat_modes or nil
  if type(supported) ~= "table" then
    return model_or_config == nil or model_or_config.combat_mode == mode
  end
  for index = 1, #supported do
    if supported[index] == mode then
      return true
    end
  end
  return false
end

function M.advance(model, step)
  local ok, diagnostics = M.validate_model(model)
  if not ok then
    return result(false, {}, diagnostics)
  end
  local next_model = normalize_config(model)
  local source_step = type(step) == "table" and step or {}
  local ticks = is_integer(source_step.ticks) and source_step.ticks or 0
  local seconds = type(source_step.seconds) == "number" and source_step.seconds or 0
  local rounds = is_integer(source_step.rounds) and source_step.rounds or 0
  local turns = is_integer(source_step.turns) and source_step.turns or 0
  local advance_diagnostics = {}

  if ticks < 0 or seconds < 0 or rounds < 0 or turns < 0 then
    advance_diagnostics[#advance_diagnostics + 1] = make_diagnostic("core.time_model.negative_advance", "Advance values must be non-negative.", "step")
    return result(false, { model = next_model }, advance_diagnostics)
  end

  if next_model.paused and source_step.force ~= true then
    return result(true, { model = next_model, advanced = false, reason = "paused" }, {})
  end

  if seconds > 0 and next_model.turn_mode == "turn_based" then
    advance_diagnostics[#advance_diagnostics + 1] = make_warning("core.time_model.seconds_in_turn_based", "Seconds were recorded for metadata but turn-based logic should use turns or rounds.", "step.seconds")
  end

  next_model.tick = next_model.tick + ticks
  next_model.elapsed_seconds = next_model.elapsed_seconds + seconds
  next_model.round = next_model.round + rounds
  next_model.global_turn = next_model.global_turn + turns

  if source_step.simulation_ticks ~= nil then
    if not is_integer(source_step.simulation_ticks) or source_step.simulation_ticks < 0 then
      advance_diagnostics[#advance_diagnostics + 1] = make_diagnostic("core.time_model.invalid_simulation_ticks", "simulation_ticks must be a non-negative integer.", "step.simulation_ticks")
      return result(false, { model = next_model }, advance_diagnostics)
    end
    next_model.tick = next_model.tick + source_step.simulation_ticks
    next_model.elapsed_seconds = next_model.elapsed_seconds + source_step.simulation_ticks * next_model.simulation_tick_seconds
  end

  return result(true, { model = next_model, advanced = true }, advance_diagnostics)
end

function M.tick_value_for_unit(model, unit)
  if TICK_UNITS[unit] ~= true then
    return result(false, {}, { make_diagnostic("core.time_model.invalid_tick_unit", "Tick unit is not supported.", "unit") })
  end
  local source = normalize_config(model)
  local value = source.tick
  if unit == "turn" then
    value = source.global_turn
  elseif unit == "round" then
    value = source.round
  elseif unit == "second" then
    value = source.elapsed_seconds
  end
  return result(true, { unit = unit, value = value }, {})
end

function M.generate(input, ctx)
  local config = type(input) == "table" and input.config or input
  local created = M.create(config)
  if not created.ok then
    return created
  end
  local data = {
    model = created.data.model,
    contract = {
      turn_modes = M.supported_turn_modes(),
      combat_modes = M.supported_combat_modes(),
      active_modes = M.supported_active_modes(),
      cooldown_tick_unit = created.data.model.cooldown_tick_unit,
      status_tick_unit = created.data.model.status_tick_unit
    }
  }
  if type(ctx) == "table" and type(ctx.note) == "string" then
    data.context_note = ctx.note
  end
  return result(true, data, created.diagnostics)
end

return M
