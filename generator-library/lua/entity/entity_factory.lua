local M = {}

M.manifest = {
  id = "entity/entity_factory/v1",
  version = "0.1.0",
  category = "entity",
  title = "Entity Factory",
  purpose = "Normalizes entity prototypes and creates compact entity instance IR for future runtime systems.",
  capabilities = { "entity.prototype.define", "entity.instance.create", "interaction.component.index" },
  input_schema = {
    type = "table",
    fields = {
      prototypes = "array of prototype records",
      instances = "optional array of instance placement records",
      default_namespace = "optional slash id prefix used only when explicit ids are absent"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      prototypes = "normalized prototype array",
      instances = "normalized instance array",
      indexes = "id maps and component ownership lists",
      summary = "counts and module metadata"
    }
  },
  config_schema = {
    allowed_components = "optional array of component names",
    require_position = "optional boolean; require x/y for instances",
    default_facing = "north | south | east | west",
    max_prototypes = "optional positive integer",
    max_instances = "optional positive integer"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity_tilemap" },
  supported_world_scales = { "single_map", "multi_map", "region", "infinite_chunks" },
  supported_turn_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local KNOWN_COMPONENTS = {
  interactable = true,
  collidable = true,
  dialogue_source = true,
  inspectable = true,
  quest_target = true
}

local VALID_FACING = { north = true, south = true, east = true, west = true }

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_int(v)
  return type(v) == "number" and v % 1 == 0
end

local function valid_slash_id(id)
  if type(id) ~= "string" then
    return false
  end
  if id == "" or string.sub(id, 1, 1) == "/" or string.sub(id, #id, #id) == "/" then
    return false
  end
  if string.find(id, "//", 1, true) then
    return false
  end
  return string.match(id, "^[a-z0-9_]+(/[a-z0-9_]+)*$") ~= nil
end

local function copy_array(src)
  local out = {}
  if type(src) ~= "table" then
    return out
  end
  for i = 1, #src do
    out[#out + 1] = src[i]
  end
  return out
end

local function sorted_keys(t)
  local keys = {}
  if type(t) ~= "table" then
    return keys
  end
  for k, _ in pairs(t) do
    keys[#keys + 1] = k
  end
  table.sort(keys)
  return keys
end

local function allowed_component_map(config)
  local allowed = {}
  if type(config.allowed_components) == "table" and #config.allowed_components > 0 then
    for i = 1, #config.allowed_components do
      allowed[config.allowed_components[i]] = true
    end
    return allowed
  end
  for k, v in pairs(KNOWN_COMPONENTS) do
    allowed[k] = v
  end
  return allowed
end

local function shallow_copy_table(src)
  local out = {}
  if type(src) ~= "table" then
    return out
  end
  for k, v in pairs(src) do
    if type(v) ~= "function" then
      out[k] = v
    end
  end
  return out
end

local function normalize_interactable(c)
  local actions = copy_array(c.actions)
  if #actions == 0 then
    actions = { "inspect" }
  end
  local priority = c.priority
  if type(priority) ~= "number" then
    priority = 0
  end
  return { actions = actions, prompt = c.prompt or "Interact", priority = priority }
end

local function normalize_collidable(c)
  return { blocks_movement = c.blocks_movement ~= false, blocks_sight = c.blocks_sight == true }
end

local function normalize_dialogue_source(c)
  return {
    dialogue_id = c.dialogue_id,
    speaker_name = c.speaker_name or c.name,
    opening_node_id = c.opening_node_id or "start",
    supports_dialogue_combat = c.supports_dialogue_combat == true
  }
end

local function normalize_inspectable(c)
  return {
    title = c.title or c.name or "Object",
    summary = c.summary or c.text or "",
    detail_level = c.detail_level or "short",
    reveals_facts = copy_array(c.reveals_facts)
  }
end

local function normalize_quest_target(c)
  return {
    quest_id = c.quest_id,
    objective_id = c.objective_id,
    role = c.role or "target",
    required = c.required == true
  }
end

local function normalize_component(name, value)
  if type(value) ~= "table" then
    value = {}
  end
  if name == "interactable" then
    return normalize_interactable(value)
  end
  if name == "collidable" then
    return normalize_collidable(value)
  end
  if name == "dialogue_source" then
    return normalize_dialogue_source(value)
  end
  if name == "inspectable" then
    return normalize_inspectable(value)
  end
  if name == "quest_target" then
    return normalize_quest_target(value)
  end
  return shallow_copy_table(value)
end

local function validate_component(name, value, diagnostics, target)
  if type(name) ~= "string" or name == "" then
    diagnostics[#diagnostics + 1] = diag("error", "entity.component_name_invalid", "Component name must be a non-empty string.", target)
    return
  end
  if type(value) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "entity.component_not_table", "Component value must be a table.", target .. "." .. name)
    return
  end
  if name == "interactable" then
    if value.actions ~= nil and type(value.actions) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "entity.interactable_actions_invalid", "interactable.actions must be an array when provided.", target .. ".interactable.actions")
    end
  elseif name == "dialogue_source" then
    if value.dialogue_id ~= nil and not valid_slash_id(value.dialogue_id) then
      diagnostics[#diagnostics + 1] = diag("error", "entity.dialogue_id_invalid", "dialogue_source.dialogue_id must be a lowercase slash id.", target .. ".dialogue_source.dialogue_id")
    end
  elseif name == "inspectable" then
    if value.summary ~= nil and type(value.summary) ~= "string" then
      diagnostics[#diagnostics + 1] = diag("error", "entity.inspect_summary_invalid", "inspectable.summary must be a string when provided.", target .. ".inspectable.summary")
    end
  elseif name == "quest_target" then
    if value.quest_id ~= nil and not valid_slash_id(value.quest_id) then
      diagnostics[#diagnostics + 1] = diag("error", "entity.quest_id_invalid", "quest_target.quest_id must be a lowercase slash id.", target .. ".quest_target.quest_id")
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = config or {}
  if type(cfg) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "entity.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if cfg.allowed_components ~= nil and type(cfg.allowed_components) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "entity.allowed_components_invalid", "allowed_components must be an array when provided.", "config.allowed_components")
  elseif type(cfg.allowed_components) == "table" then
    for i = 1, #cfg.allowed_components do
      local name = cfg.allowed_components[i]
      if type(name) ~= "string" or name == "" then
        diagnostics[#diagnostics + 1] = diag("error", "entity.allowed_component_name_invalid", "allowed component names must be non-empty strings.", "config.allowed_components[" .. tostring(i) .. "]")
      end
    end
  end
  if cfg.require_position ~= nil and type(cfg.require_position) ~= "boolean" then
    diagnostics[#diagnostics + 1] = diag("error", "entity.require_position_invalid", "require_position must be boolean when provided.", "config.require_position")
  end
  if cfg.default_facing ~= nil and not VALID_FACING[cfg.default_facing] then
    diagnostics[#diagnostics + 1] = diag("error", "entity.default_facing_invalid", "default_facing must be north, south, east or west.", "config.default_facing")
  end
  if cfg.max_prototypes ~= nil and (not is_int(cfg.max_prototypes) or cfg.max_prototypes <= 0) then
    diagnostics[#diagnostics + 1] = diag("error", "entity.max_prototypes_invalid", "max_prototypes must be a positive integer.", "config.max_prototypes")
  end
  if cfg.max_instances ~= nil and (not is_int(cfg.max_instances) or cfg.max_instances <= 0) then
    diagnostics[#diagnostics + 1] = diag("error", "entity.max_instances_invalid", "max_instances must be a positive integer.", "config.max_instances")
  end
  return #diagnostics == 0, diagnostics
end

local function validate_prototypes(prototypes, allowed, diagnostics)
  local ids = {}
  if type(prototypes) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "entity.prototypes_invalid", "input.prototypes must be an array.", "input.prototypes")
    return ids
  end
  for i = 1, #prototypes do
    local p = prototypes[i]
    local target = "input.prototypes[" .. tostring(i) .. "]"
    if type(p) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "entity.prototype_not_table", "Prototype must be a table.", target)
    else
      if not valid_slash_id(p.id) then
        diagnostics[#diagnostics + 1] = diag("error", "entity.prototype_id_invalid", "Prototype id must be a lowercase slash id.", target .. ".id")
      elseif ids[p.id] then
        diagnostics[#diagnostics + 1] = diag("error", "entity.prototype_duplicate", "Duplicate prototype id.", p.id)
      else
        ids[p.id] = true
      end
      if p.components ~= nil and type(p.components) ~= "table" then
        diagnostics[#diagnostics + 1] = diag("error", "entity.components_invalid", "Prototype components must be a table when provided.", target .. ".components")
      elseif type(p.components) == "table" then
        local keys = sorted_keys(p.components)
        for k = 1, #keys do
          local name = keys[k]
          if not allowed[name] then
            diagnostics[#diagnostics + 1] = diag("warning", "entity.component_unknown", "Component is not in allowed_components and will be copied as generic metadata.", target .. ".components." .. tostring(name))
          end
          validate_component(name, p.components[name], diagnostics, target .. ".components")
        end
      end
    end
  end
  return ids
end

local function validate_instances(instances, prototype_ids, cfg, diagnostics)
  local ids = {}
  if instances == nil then
    return ids
  end
  if type(instances) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "entity.instances_invalid", "input.instances must be an array when provided.", "input.instances")
    return ids
  end
  for i = 1, #instances do
    local e = instances[i]
    local target = "input.instances[" .. tostring(i) .. "]"
    if type(e) ~= "table" then
      diagnostics[#diagnostics + 1] = diag("error", "entity.instance_not_table", "Instance must be a table.", target)
    else
      if e.id ~= nil and not valid_slash_id(e.id) then
        diagnostics[#diagnostics + 1] = diag("error", "entity.instance_id_invalid", "Instance id must be a lowercase slash id when provided.", target .. ".id")
      elseif e.id ~= nil and ids[e.id] then
        diagnostics[#diagnostics + 1] = diag("error", "entity.instance_duplicate", "Duplicate instance id.", e.id)
      elseif e.id ~= nil then
        ids[e.id] = true
      end
      if not valid_slash_id(e.prototype_id) or not prototype_ids[e.prototype_id] then
        diagnostics[#diagnostics + 1] = diag("error", "entity.instance_prototype_missing", "Instance prototype_id must reference an existing prototype.", target .. ".prototype_id")
      end
      if cfg.require_position ~= false then
        if not is_int(e.x) or not is_int(e.y) then
          diagnostics[#diagnostics + 1] = diag("error", "entity.instance_position_invalid", "Instance must contain integer x and y.", target)
        end
      elseif (e.x ~= nil and not is_int(e.x)) or (e.y ~= nil and not is_int(e.y)) then
        diagnostics[#diagnostics + 1] = diag("error", "entity.instance_position_invalid", "Instance x/y must be integers when provided.", target)
      end
      if e.facing ~= nil and not VALID_FACING[e.facing] then
        diagnostics[#diagnostics + 1] = diag("error", "entity.instance_facing_invalid", "Instance facing must be north, south, east or west.", target .. ".facing")
      end
    end
  end
  return ids
end

local function normalize_prototypes(prototypes, allowed)
  local out = {}
  for i = 1, #prototypes do
    local p = prototypes[i]
    local components = {}
    if type(p.components) == "table" then
      local keys = sorted_keys(p.components)
      for k = 1, #keys do
        local name = keys[k]
        if allowed[name] or type(p.components[name]) == "table" then
          components[name] = normalize_component(name, p.components[name])
        end
      end
    end
    out[#out + 1] = {
      id = p.id,
      kind = p.kind or "object",
      title = p.title or p.name or p.id,
      tags = copy_array(p.tags),
      components = components,
      defaults = shallow_copy_table(p.defaults)
    }
  end
  return out
end

local function build_prototype_map(prototypes)
  local map = {}
  for i = 1, #prototypes do
    map[prototypes[i].id] = prototypes[i]
  end
  return map
end

local function normalize_instances(instances, prototypes_by_id, cfg)
  local out = {}
  if type(instances) ~= "table" then
    return out
  end
  for i = 1, #instances do
    local e = instances[i]
    local prototype = prototypes_by_id[e.prototype_id]
    local id = e.id or (e.prototype_id .. "/instance_" .. tostring(i))
    local item = {
      id = id,
      prototype_id = e.prototype_id,
      kind = prototype.kind,
      title = e.title or prototype.title,
      x = e.x,
      y = e.y,
      map_id = e.map_id,
      region_id = e.region_id,
      facing = e.facing or cfg.default_facing or "south",
      tags = copy_array(e.tags),
      state = shallow_copy_table(e.state),
      components = prototype.components
    }
    out[#out + 1] = item
  end
  return out
end

local function build_indexes(prototypes, instances)
  local by_component = {}
  local by_prototype = {}
  local by_id = {}
  for i = 1, #instances do
    local e = instances[i]
    by_id[e.id] = i
    if by_prototype[e.prototype_id] == nil then
      by_prototype[e.prototype_id] = {}
    end
    by_prototype[e.prototype_id][#by_prototype[e.prototype_id] + 1] = e.id
    local keys = sorted_keys(e.components)
    for k = 1, #keys do
      local component = keys[k]
      if by_component[component] == nil then
        by_component[component] = {}
      end
      by_component[component][#by_component[component] + 1] = e.id
    end
  end
  return {
    prototype_count = #prototypes,
    instance_count = #instances,
    instance_index_by_id = by_id,
    instance_ids_by_prototype = by_prototype,
    instance_ids_by_component = by_component
  }
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = {}
  if type(ctx) == "table" and type(ctx.config) == "table" then
    config = ctx.config
  end
  local ok_config, config_diags = M.validate_config(config)
  for i = 1, #config_diags do
    diagnostics[#diagnostics + 1] = config_diags[i]
  end
  if not ok_config then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if type(input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "entity.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local cfg = {
    allowed_components = config.allowed_components,
    require_position = config.require_position ~= false,
    default_facing = config.default_facing or "south",
    max_prototypes = config.max_prototypes or 128,
    max_instances = config.max_instances or 512
  }
  local allowed = allowed_component_map(config)

  if type(input.prototypes) == "table" and #input.prototypes > cfg.max_prototypes then
    diagnostics[#diagnostics + 1] = diag("error", "entity.too_many_prototypes", "Prototype count exceeds max_prototypes.", "input.prototypes")
  end
  if type(input.instances) == "table" and #input.instances > cfg.max_instances then
    diagnostics[#diagnostics + 1] = diag("error", "entity.too_many_instances", "Instance count exceeds max_instances.", "input.instances")
  end

  local prototype_ids = validate_prototypes(input.prototypes, allowed, diagnostics)
  validate_instances(input.instances, prototype_ids, cfg, diagnostics)
  local has_errors = false
  for i = 1, #diagnostics do
    if diagnostics[i].severity == "error" then
      has_errors = true
    end
  end
  if has_errors then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local prototypes = normalize_prototypes(input.prototypes, allowed)
  local prototype_map = build_prototype_map(prototypes)
  local instances = normalize_instances(input.instances, prototype_map, cfg)
  local indexes = build_indexes(prototypes, instances)

  local data = {
    prototypes = prototypes,
    instances = instances,
    indexes = indexes,
    summary = {
      generator = M.manifest.id,
      prototype_count = #prototypes,
      instance_count = #instances,
      supported_components = { "interactable", "collidable", "dialogue_source", "inspectable", "quest_target" }
    }
  }
  return { ok = true, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
