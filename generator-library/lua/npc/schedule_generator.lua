local M = {}

M.manifest = {
  id = "npc/schedule_generator/v1",
  version = "0.1.0",
  category = "npc",
  title = "NPC schedule generator",
  purpose = "Generate compact NPC schedule IR with time windows, movement goals, interaction availability and realtime/turn-based compatibility metadata.",
  capabilities = {
    "npc.schedule.generate",
    "schedule.time_windows",
    "pathfinding.goal_reference",
    "npc.availability.ir"
  },
  input_schema = {
    type = "object",
    required = { "schedules" }
  },
  output_schema = {
    type = "object",
    fields = { "schedules", "indexes", "pathfinding_goals" }
  },
  config_schema = {
    type = "object",
    fields = { "default_time_unit", "default_loop", "max_schedules" }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_tilemap", "unity_ui_ir", "codegen_ir" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return {
    severity = severity,
    code = code,
    message = message,
    target = target
  }
end

local function add_diag(list, severity, code, message, target)
  list[#list + 1] = diag(severity, code, message, target)
end

local function is_array(value)
  if type(value) ~= "table" then
    return false
  end
  local max = 0
  for key, _ in pairs(value) do
    if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then
      return false
    end
    if key > max then
      max = key
    end
  end
  for index = 1, max do
    if value[index] == nil then
      return false
    end
  end
  return true
end

local function is_slash_id(value)
  return type(value) == "string" and value:match("^[a-z][a-z0-9_]*(/[a-z][a-z0-9_]*)*$") ~= nil
end

local function copy_array(value)
  local output = {}
  if is_array(value) then
    for index = 1, #value do
      output[#output + 1] = value[index]
    end
  end
  return output
end

local function normalize_time_unit(value)
  if value == "tick" or value == "turn" or value == "clock" or value == "day_phase" then
    return value
  end
  return "tick"
end

local function normalize_loop(value, default_loop)
  if value == "none" or value == "daily" or value == "weekly" or value == "scenario" then
    return value
  end
  return default_loop or "daily"
end

local function normalize_entry_kind(value)
  if value == "idle" or value == "work" or value == "walk" or value == "patrol" or value == "sleep" or value == "social" or value == "quest" then
    return value
  end
  return "idle"
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    add_diag(diagnostics, "error", "schedule.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.default_time_unit ~= nil and normalize_time_unit(config.default_time_unit) ~= config.default_time_unit then
    add_diag(diagnostics, "error", "schedule.invalid_default_time_unit", "default_time_unit must be tick, turn, clock or day_phase.", "config.default_time_unit")
  end
  if config.default_loop ~= nil and normalize_loop(config.default_loop, nil) == nil then
    add_diag(diagnostics, "error", "schedule.invalid_default_loop", "default_loop must be none, daily, weekly or scenario.", "config.default_loop")
  end
  if config.max_schedules ~= nil and (type(config.max_schedules) ~= "number" or config.max_schedules < 1 or config.max_schedules > 128) then
    add_diag(diagnostics, "error", "schedule.invalid_max_schedules", "max_schedules must be a number between 1 and 128.", "config.max_schedules")
  end
  return #diagnostics == 0, diagnostics
end

local function validate_time_value(value, target, diagnostics)
  if value == nil then
    return true
  end
  if type(value) == "number" and value >= 0 then
    return true
  end
  if type(value) == "string" and value ~= "" then
    return true
  end
  add_diag(diagnostics, "error", "schedule.invalid_time_value", "Schedule time value must be a non-negative number or non-empty string.", target)
  return false
end

local function validate_entry(entry, schedule_index, entry_index, diagnostics)
  local target = "schedules[" .. tostring(schedule_index) .. "].entries[" .. tostring(entry_index) .. "]"
  if type(entry) ~= "table" then
    add_diag(diagnostics, "error", "schedule.entry_not_table", "Schedule entry must be a table.", target)
    return false
  end

  local ok = true

  if entry.kind ~= nil and normalize_entry_kind(entry.kind) ~= entry.kind then
    add_diag(diagnostics, "error", "schedule.invalid_entry_kind", "Schedule entry kind is not supported.", target .. ".kind")
    ok = false
  end
  if not validate_time_value(entry.start, target .. ".start", diagnostics) then
    ok = false
  end
  if not validate_time_value(entry.finish, target .. ".finish", diagnostics) then
    ok = false
  end
  if entry.location_id ~= nil and not is_slash_id(entry.location_id) then
    add_diag(diagnostics, "error", "schedule.invalid_location_id", "location_id must use lowercase slash notation.", target .. ".location_id")
    ok = false
  end
  if entry.path_goal_id ~= nil and not is_slash_id(entry.path_goal_id) then
    add_diag(diagnostics, "error", "schedule.invalid_path_goal_id", "path_goal_id must use lowercase slash notation.", target .. ".path_goal_id")
    ok = false
  end
  if entry.conditions ~= nil and not is_array(entry.conditions) then
    add_diag(diagnostics, "error", "schedule.conditions_not_array", "conditions must be an array.", target .. ".conditions")
    ok = false
  end
  if entry.priority ~= nil and type(entry.priority) ~= "number" then
    add_diag(diagnostics, "error", "schedule.invalid_priority", "priority must be a number.", target .. ".priority")
    ok = false
  end

  return ok
end

local function validate_schedule(schedule, index, diagnostics)
  local target = "schedules[" .. tostring(index) .. "]"
  if type(schedule) ~= "table" then
    add_diag(diagnostics, "error", "schedule.schedule_not_table", "Schedule must be a table.", target)
    return false
  end

  local ok = true

  if not is_slash_id(schedule.id) then
    add_diag(diagnostics, "error", "schedule.invalid_schedule_id", "Schedule id must use lowercase slash notation.", target .. ".id")
    ok = false
  end
  if schedule.owner_archetype_id ~= nil and not is_slash_id(schedule.owner_archetype_id) then
    add_diag(diagnostics, "error", "schedule.invalid_owner_archetype_id", "owner_archetype_id must use lowercase slash notation.", target .. ".owner_archetype_id")
    ok = false
  end
  if schedule.time_unit ~= nil and normalize_time_unit(schedule.time_unit) ~= schedule.time_unit then
    add_diag(diagnostics, "error", "schedule.invalid_time_unit", "time_unit must be tick, turn, clock or day_phase.", target .. ".time_unit")
    ok = false
  end
  if schedule.loop ~= nil and normalize_loop(schedule.loop, nil) == nil then
    add_diag(diagnostics, "error", "schedule.invalid_loop", "loop must be none, daily, weekly or scenario.", target .. ".loop")
    ok = false
  end
  if not is_array(schedule.entries) then
    add_diag(diagnostics, "error", "schedule.entries_not_array", "Schedule entries must be an array.", target .. ".entries")
    ok = false
  else
    for entry_index = 1, #schedule.entries do
      if not validate_entry(schedule.entries[entry_index], index, entry_index, diagnostics) then
        ok = false
      end
    end
  end

  return ok
end

local function validate_input(input, diagnostics, max_schedules)
  if type(input) ~= "table" then
    add_diag(diagnostics, "error", "schedule.input_not_table", "Input must be a table.", "input")
    return false
  end
  if not is_array(input.schedules) then
    add_diag(diagnostics, "error", "schedule.schedules_not_array", "Input must contain schedules array.", "input.schedules")
    return false
  end

  local ok = true
  if #input.schedules > max_schedules then
    add_diag(diagnostics, "error", "schedule.too_many_schedules", "schedules exceeds configured max_schedules.", "input.schedules")
    ok = false
  end

  local seen = {}
  for index = 1, #input.schedules do
    local schedule = input.schedules[index]
    if not validate_schedule(schedule, index, diagnostics) then
      ok = false
    elseif seen[schedule.id] then
      add_diag(diagnostics, "error", "schedule.duplicate_schedule_id", "Schedule id must be unique.", "schedules[" .. tostring(index) .. "].id")
      ok = false
    else
      seen[schedule.id] = true
    end
  end

  return ok
end

local function normalize_entry(entry, index)
  return {
    order = index,
    kind = normalize_entry_kind(entry.kind),
    start = entry.start,
    finish = entry.finish,
    location_id = entry.location_id,
    path_goal_id = entry.path_goal_id or entry.location_id,
    action = entry.action or normalize_entry_kind(entry.kind),
    priority = entry.priority or index,
    conditions = copy_array(entry.conditions),
    availability = {
      can_talk = entry.can_talk ~= false,
      can_trade = entry.can_trade == true,
      can_start_quest = entry.can_start_quest == true
    }
  }
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = {}
  if type(input) == "table" and type(input.config) == "table" then
    config = input.config
  end

  local config_ok, config_diagnostics = M.validate_config(config)
  for index = 1, #config_diagnostics do
    diagnostics[#diagnostics + 1] = config_diagnostics[index]
  end

  local max_schedules = config.max_schedules or 64
  local input_ok = validate_input(input, diagnostics, max_schedules)
  if not config_ok or not input_ok then
    return {
      ok = false,
      data = {},
      diagnostics = diagnostics,
      artifacts = {}
    }
  end

  local schedules = {}
  local schedule_ids = {}
  local pathfinding_goals = {}

  for schedule_index = 1, #input.schedules do
    local source = input.schedules[schedule_index]
    local entries = {}

    for entry_index = 1, #source.entries do
      local entry = normalize_entry(source.entries[entry_index], entry_index)
      entries[#entries + 1] = entry
      if entry.path_goal_id ~= nil then
        pathfinding_goals[#pathfinding_goals + 1] = {
          schedule_id = source.id,
          entry_order = entry.order,
          path_goal_id = entry.path_goal_id,
          location_id = entry.location_id
        }
      end
    end

    schedules[#schedules + 1] = {
      id = source.id,
      title = source.title or source.id,
      owner_archetype_id = source.owner_archetype_id,
      time_unit = normalize_time_unit(source.time_unit or config.default_time_unit),
      loop = normalize_loop(source.loop, config.default_loop or "daily"),
      entries = entries,
      fallback_entry = type(source.fallback_entry) == "table" and normalize_entry(source.fallback_entry, 0) or {
        order = 0,
        kind = "idle",
        action = "idle",
        priority = 0,
        conditions = {},
        availability = {
          can_talk = true,
          can_trade = false,
          can_start_quest = false
        }
      }
    }
    schedule_ids[#schedule_ids + 1] = source.id
  end

  return {
    ok = true,
    data = {
      schedules = schedules,
      indexes = {
        schedule_ids = schedule_ids
      },
      pathfinding_goals = pathfinding_goals
    },
    diagnostics = diagnostics,
    artifacts = {
      {
        kind = "npc_schedules",
        id = "artifact/npc_schedules",
        summary = "Compact schedule IR with pathfinding goal references and interaction availability windows."
      }
    }
  }
end

return M
