local M = {}

M.manifest = {
  id = "core/turn_system/v1",
  version = "0.1.0",
  category = "core",
  title = "Turn system helpers",
  purpose = "Build deterministic turn queues with action points, cooldown ticks, and status duration ticks.",
  capabilities = {
    "core.turn_system.create",
    "core.turn_system.action_points",
    "core.turn_system.cooldowns",
    "core.turn_system.status_duration_ticks",
    "core.turn_system.end_turn"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      turn_mode = { type = "string", enum = { "turn_based", "mixed" } },
      initiative_mode = { type = "string", enum = { "actor", "side", "global" } },
      default_action_points = { type = "integer", min = 0, max = 100 },
      tick_cooldowns_on = { type = "string", enum = { "actor_turn_end", "round_end", "global_turn_end" } },
      tick_statuses_on = { type = "string", enum = { "actor_turn_end", "round_end", "global_turn_end" } }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  unsafe_features = {}
}

local INITIATIVE_MODES = {
  actor = true,
  side = true,
  global = true
}

local TICK_POLICIES = {
  actor_turn_end = true,
  round_end = true,
  global_turn_end = true
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

local function is_non_empty_string(value)
  return type(value) == "string" and value ~= ""
end

local function normalize_config(config)
  local source = type(config) == "table" and config or {}
  return {
    turn_mode = source.turn_mode or "turn_based",
    initiative_mode = source.initiative_mode or "actor",
    default_action_points = is_integer(source.default_action_points) and source.default_action_points or 2,
    tick_cooldowns_on = source.tick_cooldowns_on or "actor_turn_end",
    tick_statuses_on = source.tick_statuses_on or "actor_turn_end"
  }
end

local function normalize_counter_map(value)
  local result_map = {}
  if type(value) ~= "table" then
    return result_map
  end
  for key, counter in pairs(value) do
    if type(key) == "string" and is_integer(counter) and counter > 0 then
      result_map[key] = counter
    end
  end
  return result_map
end

local function normalize_actor(actor, index, config, diagnostics)
  if type(actor) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.actor_not_table", "Actor entry must be a table.", "actors." .. tostring(index))
    return nil
  end
  if not is_non_empty_string(actor.id) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.actor_id_missing", "Actor id must be a non-empty string.", "actors." .. tostring(index) .. ".id")
    return nil
  end
  local max_ap = is_integer(actor.max_action_points) and actor.max_action_points or config.default_action_points
  local ap = is_integer(actor.action_points) and actor.action_points or max_ap
  if max_ap < 0 or ap < 0 or ap > max_ap then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.invalid_action_points", "Actor action points must be between 0 and max_action_points.", "actors." .. tostring(index) .. ".action_points")
    return nil
  end
  local initiative = 0
  if is_integer(actor.initiative) then
    initiative = actor.initiative
  end
  return {
    id = actor.id,
    side = is_non_empty_string(actor.side) and actor.side or "neutral",
    initiative = initiative,
    action_points = ap,
    max_action_points = max_ap,
    cooldowns = normalize_counter_map(actor.cooldowns),
    statuses = normalize_counter_map(actor.statuses),
    metadata = type(actor.metadata) == "table" and copy_table(actor.metadata, 0) or {}
  }
end

local function actor_order_compare(left, right)
  if left.initiative ~= right.initiative then
    return left.initiative > right.initiative
  end
  if left.side ~= right.side then
    return left.side < right.side
  end
  return left.id < right.id
end

local function side_order_compare(left, right)
  if left.side ~= right.side then
    return left.side < right.side
  end
  return actor_order_compare(left, right)
end

local function global_order_compare(left, right)
  return left.id < right.id
end

local function build_order(actors, initiative_mode)
  local order = {}
  for index = 1, #actors do
    order[#order + 1] = copy_table(actors[index], 0)
  end
  if initiative_mode == "side" then
    table.sort(order, side_order_compare)
  elseif initiative_mode == "global" then
    table.sort(order, global_order_compare)
  else
    table.sort(order, actor_order_compare)
  end
  local ids = {}
  for index = 1, #order do
    ids[index] = order[index].id
  end
  return ids
end

local function actor_index_by_id(actors, actor_id)
  for index = 1, #actors do
    if actors[index].id == actor_id then
      return index
    end
  end
  return nil
end

local function decrement_map(value, amount)
  local next_map = {}
  for key, ticks in pairs(value) do
    local next_ticks = ticks - amount
    if next_ticks > 0 then
      next_map[key] = next_ticks
    end
  end
  return next_map
end

local function current_actor_id(state)
  if type(state) ~= "table" or type(state.order) ~= "table" or #state.order == 0 then
    return nil
  end
  local index = state.turn_index
  if not is_integer(index) or index < 1 or index > #state.order then
    index = 1
  end
  return state.order[index]
end

local function validate_state(state)
  if type(state) ~= "table" then
    return false, { make_diagnostic("core.turn_system.state_not_table", "Turn state must be a table.", "state") }
  end
  if type(state.actors) ~= "table" or #state.actors == 0 then
    return false, { make_diagnostic("core.turn_system.state_missing_actors", "Turn state must contain at least one actor.", "state.actors") }
  end
  if type(state.order) ~= "table" or #state.order == 0 then
    return false, { make_diagnostic("core.turn_system.state_missing_order", "Turn state must contain a non-empty order array.", "state.order") }
  end
  return true, {}
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.config_not_table", "Turn system config must be a table.", "config")
    return false, diagnostics
  end
  if config.turn_mode ~= nil and config.turn_mode ~= "turn_based" and config.turn_mode ~= "mixed" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.invalid_turn_mode", "Turn system supports turn_based or mixed turn mode.", "config.turn_mode")
  end
  if config.initiative_mode ~= nil and INITIATIVE_MODES[config.initiative_mode] ~= true then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.invalid_initiative_mode", "initiative_mode must be actor, side, or global.", "config.initiative_mode")
  end
  if config.default_action_points ~= nil and (not is_integer(config.default_action_points) or config.default_action_points < 0 or config.default_action_points > 100) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.invalid_default_action_points", "default_action_points must be an integer from 0 to 100.", "config.default_action_points")
  end
  if config.tick_cooldowns_on ~= nil and TICK_POLICIES[config.tick_cooldowns_on] ~= true then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.invalid_cooldown_policy", "tick_cooldowns_on is not supported.", "config.tick_cooldowns_on")
  end
  if config.tick_statuses_on ~= nil and TICK_POLICIES[config.tick_statuses_on] ~= true then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.invalid_status_policy", "tick_statuses_on is not supported.", "config.tick_statuses_on")
  end
  for key, _ in pairs(config) do
    if key ~= "turn_mode" and key ~= "initiative_mode" and key ~= "default_action_points" and key ~= "tick_cooldowns_on" and key ~= "tick_statuses_on" then
      diagnostics[#diagnostics + 1] = make_warning("core.turn_system.unknown_config_key", "Unknown config key is ignored by the turn system.", "config." .. tostring(key))
    end
  end
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      return false, diagnostics
    end
  end
  return true, diagnostics
end

function M.create(input)
  local source = type(input) == "table" and input or {}
  local config = normalize_config(source.config)
  local ok, config_diagnostics = M.validate_config(source.config)
  if not ok then
    return result(false, {}, config_diagnostics)
  end
  local diagnostics = copy_table(config_diagnostics, 0)
  local actors = {}
  if type(source.actors) ~= "table" or #source.actors == 0 then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.actors_missing", "Turn system create input must contain a non-empty actors array.", "input.actors")
    return result(false, {}, diagnostics)
  end
  local seen = {}
  for index = 1, #source.actors do
    local actor = normalize_actor(source.actors[index], index, config, diagnostics)
    if actor ~= nil then
      if seen[actor.id] == true then
        diagnostics[#diagnostics + 1] = make_diagnostic("core.turn_system.duplicate_actor_id", "Actor ids must be unique.", "input.actors." .. tostring(index) .. ".id")
      else
        seen[actor.id] = true
        actors[#actors + 1] = actor
      end
    end
  end
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      return result(false, {}, diagnostics)
    end
  end
  local order = build_order(actors, config.initiative_mode)
  local state = {
    turn_mode = config.turn_mode,
    initiative_mode = config.initiative_mode,
    round = 1,
    global_turn = 1,
    turn_index = 1,
    order = order,
    actors = actors,
    tick_cooldowns_on = config.tick_cooldowns_on,
    tick_statuses_on = config.tick_statuses_on
  }
  return result(true, { state = state, current_actor_id = current_actor_id(state) }, diagnostics)
end

function M.current_actor(state)
  local ok, diagnostics = validate_state(state)
  if not ok then
    return result(false, {}, diagnostics)
  end
  local actor_id = current_actor_id(state)
  local index = actor_index_by_id(state.actors, actor_id)
  if index == nil then
    return result(false, {}, { make_diagnostic("core.turn_system.current_actor_missing", "Current actor id is not present in actors.", "state.order") })
  end
  return result(true, { actor = copy_table(state.actors[index], 0), actor_id = actor_id }, {})
end

function M.spend_action_points(state, actor_id, amount)
  local ok, diagnostics = validate_state(state)
  if not ok then
    return result(false, {}, diagnostics)
  end
  if not is_non_empty_string(actor_id) then
    return result(false, {}, { make_diagnostic("core.turn_system.actor_id_invalid", "actor_id must be a non-empty string.", "actor_id") })
  end
  if not is_integer(amount) or amount < 0 then
    return result(false, {}, { make_diagnostic("core.turn_system.ap_amount_invalid", "amount must be a non-negative integer.", "amount") })
  end
  local next_state = copy_table(state, 0)
  local index = actor_index_by_id(next_state.actors, actor_id)
  if index == nil then
    return result(false, { state = next_state }, { make_diagnostic("core.turn_system.actor_not_found", "Actor was not found in turn state.", "actor_id") })
  end
  local actor = next_state.actors[index]
  if actor.action_points < amount then
    return result(false, { state = next_state, actor = copy_table(actor, 0) }, { make_diagnostic("core.turn_system.not_enough_action_points", "Actor does not have enough action points.", "actor.action_points") })
  end
  actor.action_points = actor.action_points - amount
  return result(true, { state = next_state, actor = copy_table(actor, 0) }, {})
end

function M.apply_cooldown(state, actor_id, cooldown_id, ticks)
  local ok, diagnostics = validate_state(state)
  if not ok then
    return result(false, {}, diagnostics)
  end
  if not is_non_empty_string(cooldown_id) or not is_integer(ticks) or ticks < 1 then
    return result(false, {}, { make_diagnostic("core.turn_system.invalid_cooldown", "cooldown_id must be a string and ticks must be a positive integer.", "cooldown") })
  end
  local next_state = copy_table(state, 0)
  local index = actor_index_by_id(next_state.actors, actor_id)
  if index == nil then
    return result(false, { state = next_state }, { make_diagnostic("core.turn_system.actor_not_found", "Actor was not found in turn state.", "actor_id") })
  end
  next_state.actors[index].cooldowns[cooldown_id] = ticks
  return result(true, { state = next_state, actor = copy_table(next_state.actors[index], 0) }, {})
end

function M.add_status(state, actor_id, status_id, duration_ticks)
  local ok, diagnostics = validate_state(state)
  if not ok then
    return result(false, {}, diagnostics)
  end
  if not is_non_empty_string(status_id) or not is_integer(duration_ticks) or duration_ticks < 1 then
    return result(false, {}, { make_diagnostic("core.turn_system.invalid_status", "status_id must be a string and duration_ticks must be a positive integer.", "status") })
  end
  local next_state = copy_table(state, 0)
  local index = actor_index_by_id(next_state.actors, actor_id)
  if index == nil then
    return result(false, { state = next_state }, { make_diagnostic("core.turn_system.actor_not_found", "Actor was not found in turn state.", "actor_id") })
  end
  next_state.actors[index].statuses[status_id] = duration_ticks
  return result(true, { state = next_state, actor = copy_table(next_state.actors[index], 0) }, {})
end

local function tick_actor(actor, tick_cooldowns, tick_statuses)
  if tick_cooldowns then
    actor.cooldowns = decrement_map(actor.cooldowns, 1)
  end
  if tick_statuses then
    actor.statuses = decrement_map(actor.statuses, 1)
  end
end

function M.end_turn(state)
  local ok, diagnostics = validate_state(state)
  if not ok then
    return result(false, {}, diagnostics)
  end
  local next_state = copy_table(state, 0)
  local actor_id = current_actor_id(next_state)
  local actor_index = actor_index_by_id(next_state.actors, actor_id)
  if actor_index == nil then
    return result(false, { state = next_state }, { make_diagnostic("core.turn_system.actor_not_found", "Current actor was not found in turn state.", "state.order") })
  end

  local is_round_end = next_state.turn_index >= #next_state.order
  local tick_current_cooldowns = next_state.tick_cooldowns_on == "actor_turn_end" or next_state.tick_cooldowns_on == "global_turn_end"
  local tick_current_statuses = next_state.tick_statuses_on == "actor_turn_end" or next_state.tick_statuses_on == "global_turn_end"

  tick_actor(next_state.actors[actor_index], tick_current_cooldowns, tick_current_statuses)

  if is_round_end and (next_state.tick_cooldowns_on == "round_end" or next_state.tick_statuses_on == "round_end") then
    for index = 1, #next_state.actors do
      tick_actor(next_state.actors[index], next_state.tick_cooldowns_on == "round_end", next_state.tick_statuses_on == "round_end")
    end
  end

  next_state.actors[actor_index].action_points = next_state.actors[actor_index].max_action_points
  next_state.global_turn = next_state.global_turn + 1
  if is_round_end then
    next_state.turn_index = 1
    next_state.round = next_state.round + 1
  else
    next_state.turn_index = next_state.turn_index + 1
  end

  return result(true, {
    state = next_state,
    ended_actor_id = actor_id,
    current_actor_id = current_actor_id(next_state),
    round_advanced = is_round_end
  }, {})
end

function M.generate(input, ctx)
  local created = M.create(input)
  if not created.ok then
    return created
  end
  local data = {
    state = created.data.state,
    current_actor_id = created.data.current_actor_id,
    contract = {
      turn_mode = created.data.state.turn_mode,
      initiative_mode = created.data.state.initiative_mode,
      action_points = true,
      cooldown_ticks = true,
      status_duration_ticks = true
    }
  }
  if type(ctx) == "table" and type(ctx.note) == "string" then
    data.context_note = ctx.note
  end
  return result(true, data, created.diagnostics)
end

return M
