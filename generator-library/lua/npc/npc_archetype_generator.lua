local M = {}

M.manifest = {
  id = "npc/npc_archetype_generator/v1",
  version = "0.1.0",
  category = "npc",
  title = "NPC archetype generator",
  purpose = "Generate compact NPC archetype IR for static, walking and scheduled NPCs with faction role and interaction references.",
  capabilities = {
    "npc.archetype.generate",
    "npc.behavior.static",
    "npc.behavior.walking",
    "npc.behavior.scheduled",
    "faction.role.reference"
  },
  input_schema = {
    type = "object",
    required = { "archetypes" }
  },
  output_schema = {
    type = "object",
    fields = { "npc_archetypes", "indexes", "references" }
  },
  config_schema = {
    type = "object",
    fields = { "default_behavior", "default_pathfinding_profile_id", "max_archetypes" }
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

local function normalize_behavior(value, default_behavior)
  if value == "static" or value == "walking" or value == "scheduled" then
    return value
  end
  return default_behavior or "static"
end

local function normalize_components(source)
  local components = {
    "interactable",
    "collidable"
  }

  if source.inspectable ~= false then
    components[#components + 1] = "inspectable"
  end
  if source.dialogue_source ~= false then
    components[#components + 1] = "dialogue_source"
  end
  if source.quest_target == true then
    components[#components + 1] = "quest_target"
  end

  return components
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    add_diag(diagnostics, "error", "npc.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.default_behavior ~= nil and normalize_behavior(config.default_behavior, nil) == nil then
    add_diag(diagnostics, "error", "npc.invalid_default_behavior", "default_behavior must be static, walking or scheduled.", "config.default_behavior")
  end
  if config.default_pathfinding_profile_id ~= nil and not is_slash_id(config.default_pathfinding_profile_id) then
    add_diag(diagnostics, "error", "npc.invalid_default_pathfinding_profile_id", "default_pathfinding_profile_id must use lowercase slash notation.", "config.default_pathfinding_profile_id")
  end
  if config.max_archetypes ~= nil and (type(config.max_archetypes) ~= "number" or config.max_archetypes < 1 or config.max_archetypes > 256) then
    add_diag(diagnostics, "error", "npc.invalid_max_archetypes", "max_archetypes must be a number between 1 and 256.", "config.max_archetypes")
  end
  return #diagnostics == 0, diagnostics
end

local function validate_archetype(source, index, diagnostics)
  local target = "archetypes[" .. tostring(index) .. "]"
  if type(source) ~= "table" then
    add_diag(diagnostics, "error", "npc.archetype_not_table", "NPC archetype must be a table.", target)
    return false
  end

  local ok = true

  if not is_slash_id(source.id) then
    add_diag(diagnostics, "error", "npc.invalid_archetype_id", "NPC archetype id must use lowercase slash notation.", target .. ".id")
    ok = false
  end

  if source.behavior ~= nil and normalize_behavior(source.behavior, nil) == nil then
    add_diag(diagnostics, "error", "npc.invalid_behavior", "NPC behavior must be static, walking or scheduled.", target .. ".behavior")
    ok = false
  end

  if source.faction_id ~= nil and not is_slash_id(source.faction_id) then
    add_diag(diagnostics, "error", "npc.invalid_faction_id", "faction_id must use lowercase slash notation.", target .. ".faction_id")
    ok = false
  end

  if source.faction_role_id ~= nil and not is_slash_id(source.faction_role_id) then
    add_diag(diagnostics, "error", "npc.invalid_faction_role_id", "faction_role_id must use lowercase slash notation.", target .. ".faction_role_id")
    ok = false
  end

  if source.dialogue_id ~= nil and not is_slash_id(source.dialogue_id) then
    add_diag(diagnostics, "error", "npc.invalid_dialogue_id", "dialogue_id must use lowercase slash notation.", target .. ".dialogue_id")
    ok = false
  end

  if source.schedule_id ~= nil and not is_slash_id(source.schedule_id) then
    add_diag(diagnostics, "error", "npc.invalid_schedule_id", "schedule_id must use lowercase slash notation.", target .. ".schedule_id")
    ok = false
  end

  if source.pathfinding_profile_id ~= nil and not is_slash_id(source.pathfinding_profile_id) then
    add_diag(diagnostics, "error", "npc.invalid_pathfinding_profile_id", "pathfinding_profile_id must use lowercase slash notation.", target .. ".pathfinding_profile_id")
    ok = false
  end

  if source.home_location_id ~= nil and not is_slash_id(source.home_location_id) then
    add_diag(diagnostics, "error", "npc.invalid_home_location_id", "home_location_id must use lowercase slash notation.", target .. ".home_location_id")
    ok = false
  end

  if source.tags ~= nil and not is_array(source.tags) then
    add_diag(diagnostics, "error", "npc.tags_not_array", "tags must be an array.", target .. ".tags")
    ok = false
  end

  return ok
end

local function validate_input(input, diagnostics, max_archetypes)
  if type(input) ~= "table" then
    add_diag(diagnostics, "error", "npc.input_not_table", "Input must be a table.", "input")
    return false
  end
  if not is_array(input.archetypes) then
    add_diag(diagnostics, "error", "npc.archetypes_not_array", "Input must contain archetypes array.", "input.archetypes")
    return false
  end

  local ok = true
  if #input.archetypes > max_archetypes then
    add_diag(diagnostics, "error", "npc.too_many_archetypes", "archetypes exceeds configured max_archetypes.", "input.archetypes")
    ok = false
  end

  local seen = {}
  for index = 1, #input.archetypes do
    local source = input.archetypes[index]
    if not validate_archetype(source, index, diagnostics) then
      ok = false
    elseif seen[source.id] then
      add_diag(diagnostics, "error", "npc.duplicate_archetype_id", "NPC archetype id must be unique.", "archetypes[" .. tostring(index) .. "].id")
      ok = false
    else
      seen[source.id] = true
    end
  end

  return ok
end

local function make_behavior_block(source, behavior, config)
  local block = {
    type = behavior,
    can_move = behavior ~= "static",
    schedule_id = source.schedule_id,
    pathfinding_profile_id = source.pathfinding_profile_id or config.default_pathfinding_profile_id,
    home_location_id = source.home_location_id,
    patrol_route_id = source.patrol_route_id,
    idle_animation = source.idle_animation or "idle"
  }

  if behavior == "static" then
    block.can_move = false
    block.pathfinding_profile_id = nil
    block.patrol_route_id = nil
  elseif behavior == "walking" then
    block.schedule_id = nil
    block.can_move = true
  elseif behavior == "scheduled" then
    block.can_move = true
    if block.schedule_id == nil then
      block.schedule_id = source.id .. "/schedule"
    end
  end

  return block
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

  local max_archetypes = config.max_archetypes or 64
  local input_ok = validate_input(input, diagnostics, max_archetypes)
  if not config_ok or not input_ok then
    return {
      ok = false,
      data = {},
      diagnostics = diagnostics,
      artifacts = {}
    }
  end

  local archetypes = {}
  local ids = {}
  local faction_refs = {}
  local schedule_refs = {}
  local pathfinding_refs = {}

  for index = 1, #input.archetypes do
    local source = input.archetypes[index]
    local behavior = normalize_behavior(source.behavior, config.default_behavior or "static")
    local behavior_block = make_behavior_block(source, behavior, config)

    local archetype = {
      id = source.id,
      title = source.title or source.id,
      display_name = source.display_name or source.title or source.id,
      role = source.role or "npc",
      behavior = behavior_block,
      faction = {
        id = source.faction_id,
        role_id = source.faction_role_id,
        attitude_override = source.attitude_override
      },
      components = normalize_components(source),
      dialogue = {
        dialogue_id = source.dialogue_id,
        start_node_id = source.start_node_id,
        dialogue_tags = copy_array(source.dialogue_tags)
      },
      stats_ref = source.stats_ref,
      ability_profile_id = source.ability_profile_id,
      tags = copy_array(source.tags),
      metadata = type(source.metadata) == "table" and source.metadata or {}
    }

    archetypes[#archetypes + 1] = archetype
    ids[#ids + 1] = archetype.id

    if archetype.faction.id ~= nil then
      faction_refs[#faction_refs + 1] = archetype.faction.id
    end
    if archetype.behavior.schedule_id ~= nil then
      schedule_refs[#schedule_refs + 1] = archetype.behavior.schedule_id
    end
    if archetype.behavior.pathfinding_profile_id ~= nil then
      pathfinding_refs[#pathfinding_refs + 1] = archetype.behavior.pathfinding_profile_id
    end

    if behavior == "scheduled" and archetype.behavior.schedule_id == nil then
      add_diag(diagnostics, "warning", "npc.scheduled_without_schedule", "Scheduled NPC should reference or derive a schedule id.", "archetypes[" .. tostring(index) .. "].schedule_id")
    end
    if behavior ~= "static" and archetype.behavior.pathfinding_profile_id == nil then
      add_diag(diagnostics, "warning", "npc.moving_without_pathfinding_profile", "Walking or scheduled NPC should reference a pathfinding profile.", "archetypes[" .. tostring(index) .. "].pathfinding_profile_id")
    end
  end

  return {
    ok = true,
    data = {
      npc_archetypes = archetypes,
      indexes = {
        npc_archetype_ids = ids
      },
      references = {
        faction_ids = faction_refs,
        schedule_ids = schedule_refs,
        pathfinding_profile_ids = pathfinding_refs
      }
    },
    diagnostics = diagnostics,
    artifacts = {
      {
        kind = "npc_archetypes",
        id = "artifact/npc_archetypes",
        summary = "Compact NPC archetype IR for entity and interaction layers."
      }
    }
  }
end

return M
