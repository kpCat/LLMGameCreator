local M = {}

M.manifest = {
  id = "world/region_graph/v1",
  version = "0.1.0",
  category = "world",
  title = "Region graph builder",
  purpose = "Validate and normalize region nodes and connections for finite maps, multi-map games, world regions, and chunked-world navigation metadata.",
  capabilities = {
    "world.region_graph.validate",
    "world.region_graph.normalize",
    "world.region_graph.adjacency",
    "world.region_graph.minimap_metadata"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      regions = { type = "array" },
      connections = { type = "array" },
      allow_isolated_regions = { type = "boolean" },
      allow_self_connections = { type = "boolean" }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  supported_time_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local function diagnostic(severity, code, message, target)
  local item = { severity = severity, code = code, message = message }
  if target ~= nil then
    item.target = target
  end
  return item
end

local function add_error(diagnostics, code, message, target)
  diagnostics[#diagnostics + 1] = diagnostic("error", code, message, target)
end

local function add_warning(diagnostics, code, message, target)
  diagnostics[#diagnostics + 1] = diagnostic("warning", code, message, target)
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

local function is_array(value)
  if type(value) ~= "table" then
    return false
  end
  local count = 0
  local max_index = 0
  for key, _ in pairs(value) do
    if not is_integer(key) or key < 1 then
      return false
    end
    count = count + 1
    if key > max_index then
      max_index = key
    end
  end
  return count == max_index
end

local function copy_value(value, depth)
  if type(value) ~= "table" then
    return value
  end
  if depth > 16 then
    return {}
  end
  local copy = {}
  for key, item in pairs(value) do
    copy[key] = copy_value(item, depth + 1)
  end
  return copy
end

local function is_slash_id(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if string.lower(value) ~= value then
    return false
  end
  if string.sub(value, 1, 1) == "/" or string.sub(value, #value, #value) == "/" then
    return false
  end
  if string.find(value, "//", 1, true) ~= nil then
    return false
  end
  local count = 0
  for segment in string.gmatch(value, "[^/]+") do
    count = count + 1
    if string.match(segment, "^[a-z0-9][a-z0-9_%-]*$") == nil then
      return false
    end
  end
  return count >= 2
end

local function normalize_string_array(value)
  if not is_array(value) then
    return {}
  end
  local output = {}
  local seen = {}
  for index = 1, #value do
    local item = value[index]
    if type(item) == "string" and item ~= "" and seen[item] ~= true then
      seen[item] = true
      output[#output + 1] = item
    end
  end
  return output
end

local function normalize_position(value)
  if type(value) ~= "table" then
    return nil
  end
  local x = value.x
  local y = value.y
  if type(x) ~= "number" or type(y) ~= "number" then
    return nil
  end
  return { x = x, y = y }
end

local function normalize_region(raw)
  return {
    id = raw.id,
    title = type(raw.title) == "string" and raw.title or raw.id,
    map_id = is_slash_id(raw.map_id) and raw.map_id or nil,
    biome_id = is_slash_id(raw.biome_id) and raw.biome_id or nil,
    position = normalize_position(raw.position),
    tags = normalize_string_array(raw.tags),
    minimap = type(raw.minimap) == "table" and copy_value(raw.minimap, 0) or {},
    metadata = type(raw.metadata) == "table" and copy_value(raw.metadata, 0) or {}
  }
end

local function normalize_connection(raw)
  local bidirectional = true
  if raw.bidirectional == false then
    bidirectional = false
  end
  return {
    from = raw.from,
    to = raw.to,
    type = type(raw.type) == "string" and raw.type or "path",
    bidirectional = bidirectional,
    blocked = raw.blocked == true,
    gate_id = is_slash_id(raw.gate_id) and raw.gate_id or nil,
    tags = normalize_string_array(raw.tags),
    metadata = type(raw.metadata) == "table" and copy_value(raw.metadata, 0) or {}
  }
end

local function validate_region(raw, index, diagnostics)
  local target = "regions." .. tostring(index)
  if type(raw) ~= "table" then
    add_error(diagnostics, "region_graph.region.not_object", "Region entry must be an object.", target)
    return
  end
  if not is_slash_id(raw.id) then
    add_error(diagnostics, "region_graph.region.invalid_id", "Region id must be a lowercase slash id such as region/old_road.", target .. ".id")
  end
  if raw.title ~= nil and type(raw.title) ~= "string" then
    add_error(diagnostics, "region_graph.region.invalid_title", "Region title must be a string when provided.", target .. ".title")
  end
  if raw.map_id ~= nil and not is_slash_id(raw.map_id) then
    add_error(diagnostics, "region_graph.region.invalid_map_id", "map_id must be a lowercase slash id when provided.", target .. ".map_id")
  end
  if raw.biome_id ~= nil and not is_slash_id(raw.biome_id) then
    add_error(diagnostics, "region_graph.region.invalid_biome_id", "biome_id must be a lowercase slash id when provided.", target .. ".biome_id")
  end
  if raw.position ~= nil and normalize_position(raw.position) == nil then
    add_error(diagnostics, "region_graph.region.invalid_position", "position must contain numeric x and y when provided.", target .. ".position")
  end
  if raw.tags ~= nil and not is_array(raw.tags) then
    add_error(diagnostics, "region_graph.region.invalid_tags", "tags must be an array of strings when provided.", target .. ".tags")
  end
end

local function validate_connection(raw, index, diagnostics)
  local target = "connections." .. tostring(index)
  if type(raw) ~= "table" then
    add_error(diagnostics, "region_graph.connection.not_object", "Connection entry must be an object.", target)
    return
  end
  if not is_slash_id(raw.from) then
    add_error(diagnostics, "region_graph.connection.invalid_from", "Connection from must reference a region slash id.", target .. ".from")
  end
  if not is_slash_id(raw.to) then
    add_error(diagnostics, "region_graph.connection.invalid_to", "Connection to must reference a region slash id.", target .. ".to")
  end
  if raw.type ~= nil and type(raw.type) ~= "string" then
    add_error(diagnostics, "region_graph.connection.invalid_type", "Connection type must be a string when provided.", target .. ".type")
  end
  if raw.bidirectional ~= nil and type(raw.bidirectional) ~= "boolean" then
    add_error(diagnostics, "region_graph.connection.invalid_bidirectional", "bidirectional must be boolean when provided.", target .. ".bidirectional")
  end
  if raw.blocked ~= nil and type(raw.blocked) ~= "boolean" then
    add_error(diagnostics, "region_graph.connection.invalid_blocked", "blocked must be boolean when provided.", target .. ".blocked")
  end
  if raw.gate_id ~= nil and not is_slash_id(raw.gate_id) then
    add_error(diagnostics, "region_graph.connection.invalid_gate_id", "gate_id must be a lowercase slash id when provided.", target .. ".gate_id")
  end
end

local function add_adjacent(adjacency, from_id, item)
  local bucket = adjacency[from_id]
  if bucket == nil then
    bucket = {}
    adjacency[from_id] = bucket
  end
  bucket[#bucket + 1] = item
end

local function connection_key(from_id, to_id, connection_type)
  return from_id .. "->" .. to_id .. ":" .. connection_type
end

local function source_value(config, input, key)
  if type(input) == "table" and input[key] ~= nil then
    return input[key]
  end
  if type(config) == "table" and config[key] ~= nil then
    return config[key]
  end
  return nil
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = type(config) == "table" and config or {}
  if config ~= nil and type(config) ~= "table" then
    add_error(diagnostics, "region_graph.config.not_object", "Config must be an object when provided.", "config")
  end
  if cfg.regions ~= nil then
    if not is_array(cfg.regions) then
      add_error(diagnostics, "region_graph.config.regions_not_array", "regions must be an array.", "config.regions")
    else
      for index = 1, #cfg.regions do
        validate_region(cfg.regions[index], index, diagnostics)
      end
    end
  end
  if cfg.connections ~= nil then
    if not is_array(cfg.connections) then
      add_error(diagnostics, "region_graph.config.connections_not_array", "connections must be an array.", "config.connections")
    else
      for index = 1, #cfg.connections do
        validate_connection(cfg.connections[index], index, diagnostics)
      end
    end
  end
  if cfg.allow_isolated_regions ~= nil and type(cfg.allow_isolated_regions) ~= "boolean" then
    add_error(diagnostics, "region_graph.config.allow_isolated_invalid", "allow_isolated_regions must be boolean when provided.", "config.allow_isolated_regions")
  end
  if cfg.allow_self_connections ~= nil and type(cfg.allow_self_connections) ~= "boolean" then
    add_error(diagnostics, "region_graph.config.allow_self_invalid", "allow_self_connections must be boolean when provided.", "config.allow_self_connections")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local config = type(ctx) == "table" and type(ctx.config) == "table" and ctx.config or {}
  local diagnostics = {}
  local raw_regions = source_value(config, input, "regions") or {}
  local raw_connections = source_value(config, input, "connections") or {}

  if not is_array(raw_regions) then
    add_error(diagnostics, "region_graph.input.regions_not_array", "regions must be an array.", "regions")
    return result(false, {}, diagnostics)
  end
  if not is_array(raw_connections) then
    add_error(diagnostics, "region_graph.input.connections_not_array", "connections must be an array.", "connections")
    return result(false, {}, diagnostics)
  end

  local regions = {}
  local by_id = {}
  local region_ids = {}
  for index = 1, #raw_regions do
    local raw = raw_regions[index]
    validate_region(raw, index, diagnostics)
    if type(raw) == "table" and is_slash_id(raw.id) then
      if by_id[raw.id] ~= nil then
        add_error(diagnostics, "region_graph.input.duplicate_region_id", "Region ids must be unique.", "regions." .. tostring(index) .. ".id")
      else
        local normalized = normalize_region(raw)
        regions[#regions + 1] = normalized
        by_id[normalized.id] = normalized
        region_ids[#region_ids + 1] = normalized.id
      end
    end
  end

  local connections = {}
  local adjacency = {}
  local seen_connections = {}
  local degree = {}
  for index = 1, #raw_connections do
    local raw = raw_connections[index]
    validate_connection(raw, index, diagnostics)
    if type(raw) == "table" and is_slash_id(raw.from) and is_slash_id(raw.to) then
      if by_id[raw.from] == nil then
        add_error(diagnostics, "region_graph.input.missing_from_region", "Connection from references a missing region.", "connections." .. tostring(index) .. ".from")
      end
      if by_id[raw.to] == nil then
        add_error(diagnostics, "region_graph.input.missing_to_region", "Connection to references a missing region.", "connections." .. tostring(index) .. ".to")
      end
      if raw.from == raw.to and config.allow_self_connections ~= true then
        add_error(diagnostics, "region_graph.input.self_connection", "Self-connections are disabled by default.", "connections." .. tostring(index))
      end
      local normalized = normalize_connection(raw)
      local key = connection_key(normalized.from, normalized.to, normalized.type)
      if seen_connections[key] == true then
        add_warning(diagnostics, "region_graph.input.duplicate_connection", "Duplicate connection was preserved but should usually be collapsed by authoring tools.", "connections." .. tostring(index))
      end
      seen_connections[key] = true
      connections[#connections + 1] = normalized
      add_adjacent(adjacency, normalized.from, {
        to = normalized.to,
        type = normalized.type,
        blocked = normalized.blocked,
        gate_id = normalized.gate_id,
        bidirectional = normalized.bidirectional
      })
      degree[normalized.from] = (degree[normalized.from] or 0) + 1
      degree[normalized.to] = (degree[normalized.to] or 0) + 1
      if normalized.bidirectional then
        add_adjacent(adjacency, normalized.to, {
          to = normalized.from,
          type = normalized.type,
          blocked = normalized.blocked,
          gate_id = normalized.gate_id,
          bidirectional = normalized.bidirectional
        })
      end
    end
  end

  if config.allow_isolated_regions ~= true then
    for index = 1, #region_ids do
      local id = region_ids[index]
      if (degree[id] or 0) == 0 and #regions > 1 then
        add_warning(diagnostics, "region_graph.input.isolated_region", "Region has no graph connections.", "regions." .. tostring(index))
      end
    end
  end

  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      return result(false, {}, diagnostics)
    end
  end

  return result(true, {
    regions = regions,
    by_id = by_id,
    region_ids = region_ids,
    connections = connections,
    adjacency = adjacency,
    counts = {
      regions = #regions,
      connections = #connections
    }
  }, diagnostics)
end

return M
