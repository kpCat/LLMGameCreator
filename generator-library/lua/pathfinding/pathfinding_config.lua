local M = {}

M.manifest = {
  id = "pathfinding/pathfinding_config/v1",
  version = "0.1.0",
  category = "pathfinding",
  title = "Pathfinding config generator",
  purpose = "Generate compact pathfinding profile IR for grid agents, dynamic obstacles, movement costs and realtime/turn-based compatibility.",
  capabilities = {
    "pathfinding.config.generate",
    "pathfinding.dynamic_obstacles",
    "pathfinding.agent_profile",
    "world.reachability.reference"
  },
  input_schema = {
    type = "object",
    required = { "profiles" }
  },
  output_schema = {
    type = "object",
    fields = { "profiles", "defaults", "dynamic_obstacles", "indexes" }
  },
  config_schema = {
    type = "object",
    fields = { "default_grid", "default_replan_policy", "max_profiles" }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_tilemap", "codegen_ir" },
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

local function normalize_grid(value)
  if value == "orthogonal_4" or value == "diagonal_8" or value == "hex_axial" then
    return value
  end
  return "orthogonal_4"
end

local function normalize_replan_policy(value)
  if value == "never" or value == "on_blocked" or value == "on_tick" or value == "on_turn" or value == "on_goal_changed" then
    return value
  end
  return "on_blocked"
end

local function normalize_turn_modes(value)
  local output = {}
  local seen = {}
  if is_array(value) then
    for index = 1, #value do
      local mode = value[index]
      if (mode == "realtime" or mode == "turn_based" or mode == "mixed" or mode == "paused_planning") and not seen[mode] then
        output[#output + 1] = mode
        seen[mode] = true
      end
    end
  end
  if #output == 0 then
    output = { "realtime", "turn_based", "mixed" }
  end
  return output
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    add_diag(diagnostics, "error", "pathfinding.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.default_grid ~= nil and normalize_grid(config.default_grid) ~= config.default_grid then
    add_diag(diagnostics, "error", "pathfinding.invalid_default_grid", "default_grid must be orthogonal_4, diagonal_8 or hex_axial.", "config.default_grid")
  end
  if config.default_replan_policy ~= nil and normalize_replan_policy(config.default_replan_policy) ~= config.default_replan_policy then
    add_diag(diagnostics, "error", "pathfinding.invalid_replan_policy", "default_replan_policy is not supported.", "config.default_replan_policy")
  end
  if config.max_profiles ~= nil and (type(config.max_profiles) ~= "number" or config.max_profiles < 1 or config.max_profiles > 128) then
    add_diag(diagnostics, "error", "pathfinding.invalid_max_profiles", "max_profiles must be a number between 1 and 128.", "config.max_profiles")
  end
  return #diagnostics == 0, diagnostics
end

local function validate_costs(costs, target, diagnostics)
  local ok = true
  if costs ~= nil then
    if type(costs) ~= "table" then
      add_diag(diagnostics, "error", "pathfinding.costs_not_table", "movement_costs must be a table keyed by terrain or tag id.", target)
      return false
    end
    for key, value in pairs(costs) do
      if type(key) ~= "string" or key == "" then
        add_diag(diagnostics, "error", "pathfinding.invalid_cost_key", "Movement cost key must be a non-empty string.", target)
        ok = false
      end
      if type(value) ~= "number" or value < 0 then
        add_diag(diagnostics, "error", "pathfinding.invalid_cost_value", "Movement cost value must be a non-negative number.", target .. "." .. tostring(key))
        ok = false
      end
    end
  end
  return ok
end

local function validate_profile(profile, index, diagnostics)
  local target = "profiles[" .. tostring(index) .. "]"
  if type(profile) ~= "table" then
    add_diag(diagnostics, "error", "pathfinding.profile_not_table", "Pathfinding profile must be a table.", target)
    return false
  end

  local ok = true

  if not is_slash_id(profile.id) then
    add_diag(diagnostics, "error", "pathfinding.invalid_profile_id", "Profile id must use lowercase slash notation.", target .. ".id")
    ok = false
  end

  if profile.grid ~= nil and normalize_grid(profile.grid) ~= profile.grid then
    add_diag(diagnostics, "error", "pathfinding.invalid_grid", "Profile grid must be orthogonal_4, diagonal_8 or hex_axial.", target .. ".grid")
    ok = false
  end

  if profile.turn_modes ~= nil and not is_array(profile.turn_modes) then
    add_diag(diagnostics, "error", "pathfinding.turn_modes_not_array", "turn_modes must be an array.", target .. ".turn_modes")
    ok = false
  end

  if profile.replan_policy ~= nil and normalize_replan_policy(profile.replan_policy) ~= profile.replan_policy then
    add_diag(diagnostics, "error", "pathfinding.invalid_profile_replan_policy", "Profile replan_policy is not supported.", target .. ".replan_policy")
    ok = false
  end

  if profile.dynamic_obstacle_classes ~= nil and not is_array(profile.dynamic_obstacle_classes) then
    add_diag(diagnostics, "error", "pathfinding.dynamic_classes_not_array", "dynamic_obstacle_classes must be an array.", target .. ".dynamic_obstacle_classes")
    ok = false
  end

  if not validate_costs(profile.movement_costs, target .. ".movement_costs", diagnostics) then
    ok = false
  end

  return ok
end

local function validate_input(input, diagnostics, max_profiles)
  if type(input) ~= "table" then
    add_diag(diagnostics, "error", "pathfinding.input_not_table", "Input must be a table.", "input")
    return false
  end

  if not is_array(input.profiles) then
    add_diag(diagnostics, "error", "pathfinding.profiles_not_array", "Input must contain profiles array.", "input.profiles")
    return false
  end

  local ok = true
  if #input.profiles > max_profiles then
    add_diag(diagnostics, "error", "pathfinding.too_many_profiles", "profiles exceeds configured max_profiles.", "input.profiles")
    ok = false
  end

  local seen = {}
  for index = 1, #input.profiles do
    local profile = input.profiles[index]
    if not validate_profile(profile, index, diagnostics) then
      ok = false
    elseif seen[profile.id] then
      add_diag(diagnostics, "error", "pathfinding.duplicate_profile_id", "Profile id must be unique.", "profiles[" .. tostring(index) .. "].id")
      ok = false
    else
      seen[profile.id] = true
    end
  end

  if input.dynamic_obstacles ~= nil then
    if not is_array(input.dynamic_obstacles) then
      add_diag(diagnostics, "error", "pathfinding.dynamic_obstacles_not_array", "dynamic_obstacles must be an array.", "input.dynamic_obstacles")
      ok = false
    else
      for index = 1, #input.dynamic_obstacles do
        local obstacle = input.dynamic_obstacles[index]
        local target = "dynamic_obstacles[" .. tostring(index) .. "]"
        if type(obstacle) ~= "table" then
          add_diag(diagnostics, "error", "pathfinding.dynamic_obstacle_not_table", "Dynamic obstacle must be a table.", target)
          ok = false
        else
          if not is_slash_id(obstacle.id) then
            add_diag(diagnostics, "error", "pathfinding.invalid_dynamic_obstacle_id", "Dynamic obstacle id must use lowercase slash notation.", target .. ".id")
            ok = false
          end
          if obstacle.blocks_movement ~= nil and type(obstacle.blocks_movement) ~= "boolean" then
            add_diag(diagnostics, "error", "pathfinding.invalid_blocks_movement", "blocks_movement must be a boolean.", target .. ".blocks_movement")
            ok = false
          end
        end
      end
    end
  end

  return ok
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

  local max_profiles = config.max_profiles or 32
  local input_ok = validate_input(input, diagnostics, max_profiles)
  if not config_ok or not input_ok then
    return {
      ok = false,
      data = {},
      diagnostics = diagnostics,
      artifacts = {}
    }
  end

  local profiles = {}
  local profile_ids = {}

  for index = 1, #input.profiles do
    local source = input.profiles[index]
    local profile = {
      id = source.id,
      title = source.title or source.id,
      grid = normalize_grid(source.grid or config.default_grid),
      turn_modes = normalize_turn_modes(source.turn_modes),
      replan_policy = normalize_replan_policy(source.replan_policy or config.default_replan_policy),
      movement_costs = type(source.movement_costs) == "table" and source.movement_costs or {},
      passable_tags = copy_array(source.passable_tags),
      blocked_tags = copy_array(source.blocked_tags),
      dynamic_obstacle_classes = copy_array(source.dynamic_obstacle_classes),
      avoidance = {
        prefer_roads = source.avoidance ~= nil and source.avoidance.prefer_roads == true or false,
        avoid_hostile_factions = source.avoidance ~= nil and source.avoidance.avoid_hostile_factions == true or false,
        avoid_crowds = source.avoidance ~= nil and source.avoidance.avoid_crowds == true or false
      },
      outputs = {
        emits_path_plan = true,
        emits_reachability_request = source.outputs ~= nil and source.outputs.emits_reachability_request == true or false,
        emits_movement_intent = true
      }
    }
    profiles[#profiles + 1] = profile
    profile_ids[#profile_ids + 1] = profile.id
  end

  local dynamic_obstacles = {}
  if is_array(input.dynamic_obstacles) then
    for index = 1, #input.dynamic_obstacles do
      local source = input.dynamic_obstacles[index]
      if type(source) == "table" and is_slash_id(source.id) then
        dynamic_obstacles[#dynamic_obstacles + 1] = {
          id = source.id,
          title = source.title or source.id,
          obstacle_class = source.obstacle_class or "actor",
          blocks_movement = source.blocks_movement ~= false,
          expires = source.expires or "on_update",
          tags = copy_array(source.tags)
        }
      end
    end
  end

  return {
    ok = true,
    data = {
      defaults = {
        grid = normalize_grid(config.default_grid),
        replan_policy = normalize_replan_policy(config.default_replan_policy)
      },
      profiles = profiles,
      dynamic_obstacles = dynamic_obstacles,
      indexes = {
        profile_ids = profile_ids
      }
    },
    diagnostics = diagnostics,
    artifacts = {
      {
        kind = "pathfinding_config",
        id = "artifact/pathfinding_config",
        summary = "Compact pathfinding profiles for grid movement and obstacle-aware replanning."
      }
    }
  }
end

return M
