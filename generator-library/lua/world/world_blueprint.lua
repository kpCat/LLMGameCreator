local M = {}

M.manifest = {
  id = "world/world_blueprint/v1",
  version = "0.1.0",
  category = "world",
  title = "World blueprint generator",
  purpose = "Build compact world blueprint IR for finite maps, multi-map games, region graphs, chunked worlds, and infinite seeded worlds without emitting huge tile arrays.",
  capabilities = {
    "world.blueprint.generate",
    "world.scale.model",
    "world.global_map.metadata",
    "world.minimap.metadata",
    "world.seeded_infinite_blueprint"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      world_id = { type = "string" },
      title = { type = "string" },
      world_scale = { type = "string" },
      seed = { type = "integer" },
      maps = { type = "array" },
      biomes = { type = "array" },
      regions = { type = "array" },
      connections = { type = "array" },
      chunking = { type = "object" },
      global_map = { type = "object" },
      minimap = { type = "object" }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  supported_time_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local WORLD_SCALES = {
  single_map = true,
  multi_map = true,
  region = true,
  continent = true,
  planet = true,
  infinite_chunks = true
}

local BLUEPRINT_MODES = {
  finite_map = true,
  multi_map = true,
  region = true,
  chunked_world = true,
  infinite_seeded_world = true
}

local DEFAULT_BIOMES = {
  {
    id = "biome/temperate_plains",
    title = "Temperate Plains",
    temperature = 0.55,
    humidity = 0.45,
    danger = 0.20,
    tags = { "open", "grass", "starter_friendly" },
    resources = { "resource/wood", "resource/herbs", "resource/stone" },
    minimap = { color_key = "plains", pattern = "soft" }
  },
  {
    id = "biome/dark_forest",
    title = "Dark Forest",
    temperature = 0.40,
    humidity = 0.70,
    danger = 0.55,
    tags = { "forest", "shadow", "ambush" },
    resources = { "resource/wood", "resource/mushroom", "resource/rare_herb" },
    minimap = { color_key = "forest_dark", pattern = "dense" }
  }
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

local function is_positive_integer(value)
  return is_integer(value) and value > 0
end

local function is_non_negative_integer(value)
  return is_integer(value) and value >= 0
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

local function clamp01(value)
  if type(value) ~= "number" then
    return 0
  end
  if value < 0 then
    return 0
  end
  if value > 1 then
    return 1
  end
  return value
end

local function normalize_bounds(raw, fallback_width, fallback_height)
  if type(raw) ~= "table" then
    return { x = 0, y = 0, width = fallback_width, height = fallback_height }
  end
  return {
    x = is_integer(raw.x) and raw.x or 0,
    y = is_integer(raw.y) and raw.y or 0,
    width = is_positive_integer(raw.width) and raw.width or fallback_width,
    height = is_positive_integer(raw.height) and raw.height or fallback_height
  }
end

local function validate_bounds(raw, target, diagnostics)
  if raw == nil then
    return
  end
  if type(raw) ~= "table" then
    add_error(diagnostics, "world_blueprint.bounds.not_object", "Bounds must be an object.", target)
    return
  end
  if raw.x ~= nil and not is_integer(raw.x) then
    add_error(diagnostics, "world_blueprint.bounds.invalid_x", "Bounds x must be an integer.", target .. ".x")
  end
  if raw.y ~= nil and not is_integer(raw.y) then
    add_error(diagnostics, "world_blueprint.bounds.invalid_y", "Bounds y must be an integer.", target .. ".y")
  end
  if raw.width ~= nil and not is_positive_integer(raw.width) then
    add_error(diagnostics, "world_blueprint.bounds.invalid_width", "Bounds width must be a positive integer.", target .. ".width")
  end
  if raw.height ~= nil and not is_positive_integer(raw.height) then
    add_error(diagnostics, "world_blueprint.bounds.invalid_height", "Bounds height must be a positive integer.", target .. ".height")
  end
end

local function normalize_biome(raw)
  return {
    id = raw.id,
    title = type(raw.title) == "string" and raw.title or raw.id,
    temperature = clamp01(type(raw.temperature) == "number" and raw.temperature or 0.5),
    humidity = clamp01(type(raw.humidity) == "number" and raw.humidity or 0.5),
    danger = clamp01(type(raw.danger) == "number" and raw.danger or 0.0),
    tags = normalize_string_array(raw.tags),
    resources = normalize_string_array(raw.resources),
    minimap = type(raw.minimap) == "table" and copy_value(raw.minimap, 0) or {}
  }
end

local function validate_biome(raw, index, diagnostics)
  local target = "biomes." .. tostring(index)
  if type(raw) ~= "table" then
    add_error(diagnostics, "world_blueprint.biome.not_object", "Biome entry must be an object.", target)
    return
  end
  if not is_slash_id(raw.id) then
    add_error(diagnostics, "world_blueprint.biome.invalid_id", "Biome id must be a lowercase slash id.", target .. ".id")
  end
  if raw.temperature ~= nil and type(raw.temperature) ~= "number" then
    add_error(diagnostics, "world_blueprint.biome.invalid_temperature", "Biome temperature must be numeric when provided.", target .. ".temperature")
  end
  if raw.humidity ~= nil and type(raw.humidity) ~= "number" then
    add_error(diagnostics, "world_blueprint.biome.invalid_humidity", "Biome humidity must be numeric when provided.", target .. ".humidity")
  end
  if raw.danger ~= nil and type(raw.danger) ~= "number" then
    add_error(diagnostics, "world_blueprint.biome.invalid_danger", "Biome danger must be numeric when provided.", target .. ".danger")
  end
  if raw.resources ~= nil and not is_array(raw.resources) then
    add_error(diagnostics, "world_blueprint.biome.invalid_resources", "Biome resources must be an array when provided.", target .. ".resources")
  end
  if raw.tags ~= nil and not is_array(raw.tags) then
    add_error(diagnostics, "world_blueprint.biome.invalid_tags", "Biome tags must be an array when provided.", target .. ".tags")
  end
end

local function normalize_map(raw, index, mode)
  local fallback_id = index == 1 and "map/main" or "map/map_" .. tostring(index)
  local bounds = normalize_bounds(raw.bounds, 64, 64)
  local map = {
    id = is_slash_id(raw.id) and raw.id or fallback_id,
    title = type(raw.title) == "string" and raw.title or fallback_id,
    kind = type(raw.kind) == "string" and raw.kind or "local_map",
    bounds = bounds,
    default_biome_id = is_slash_id(raw.default_biome_id) and raw.default_biome_id or nil,
    tags = normalize_string_array(raw.tags),
    minimap = type(raw.minimap) == "table" and copy_value(raw.minimap, 0) or {},
    metadata = type(raw.metadata) == "table" and copy_value(raw.metadata, 0) or {}
  }
  if mode == "infinite_seeded_world" then
    map.bounds = nil
  end
  return map
end

local function validate_map(raw, index, diagnostics)
  local target = "maps." .. tostring(index)
  if type(raw) ~= "table" then
    add_error(diagnostics, "world_blueprint.map.not_object", "Map entry must be an object.", target)
    return
  end
  if raw.id ~= nil and not is_slash_id(raw.id) then
    add_error(diagnostics, "world_blueprint.map.invalid_id", "Map id must be a lowercase slash id when provided.", target .. ".id")
  end
  if raw.title ~= nil and type(raw.title) ~= "string" then
    add_error(diagnostics, "world_blueprint.map.invalid_title", "Map title must be a string when provided.", target .. ".title")
  end
  if raw.kind ~= nil and type(raw.kind) ~= "string" then
    add_error(diagnostics, "world_blueprint.map.invalid_kind", "Map kind must be a string when provided.", target .. ".kind")
  end
  validate_bounds(raw.bounds, target .. ".bounds", diagnostics)
  if raw.default_biome_id ~= nil and not is_slash_id(raw.default_biome_id) then
    add_error(diagnostics, "world_blueprint.map.invalid_default_biome", "default_biome_id must be a lowercase slash id when provided.", target .. ".default_biome_id")
  end
end

local function normalize_region(raw)
  return {
    id = raw.id,
    title = type(raw.title) == "string" and raw.title or raw.id,
    map_id = is_slash_id(raw.map_id) and raw.map_id or nil,
    biome_id = is_slash_id(raw.biome_id) and raw.biome_id or nil,
    position = type(raw.position) == "table" and type(raw.position.x) == "number" and type(raw.position.y) == "number" and { x = raw.position.x, y = raw.position.y } or nil,
    tags = normalize_string_array(raw.tags),
    minimap = type(raw.minimap) == "table" and copy_value(raw.minimap, 0) or {},
    metadata = type(raw.metadata) == "table" and copy_value(raw.metadata, 0) or {}
  }
end

local function validate_region(raw, index, diagnostics)
  local target = "regions." .. tostring(index)
  if type(raw) ~= "table" then
    add_error(diagnostics, "world_blueprint.region.not_object", "Region entry must be an object.", target)
    return
  end
  if not is_slash_id(raw.id) then
    add_error(diagnostics, "world_blueprint.region.invalid_id", "Region id must be a lowercase slash id.", target .. ".id")
  end
  if raw.map_id ~= nil and not is_slash_id(raw.map_id) then
    add_error(diagnostics, "world_blueprint.region.invalid_map_id", "map_id must be a lowercase slash id when provided.", target .. ".map_id")
  end
  if raw.biome_id ~= nil and not is_slash_id(raw.biome_id) then
    add_error(diagnostics, "world_blueprint.region.invalid_biome_id", "biome_id must be a lowercase slash id when provided.", target .. ".biome_id")
  end
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
    tags = normalize_string_array(raw.tags),
    metadata = type(raw.metadata) == "table" and copy_value(raw.metadata, 0) or {}
  }
end

local function validate_connection(raw, index, diagnostics)
  local target = "connections." .. tostring(index)
  if type(raw) ~= "table" then
    add_error(diagnostics, "world_blueprint.connection.not_object", "Connection entry must be an object.", target)
    return
  end
  if not is_slash_id(raw.from) then
    add_error(diagnostics, "world_blueprint.connection.invalid_from", "Connection from must be a lowercase slash id.", target .. ".from")
  end
  if not is_slash_id(raw.to) then
    add_error(diagnostics, "world_blueprint.connection.invalid_to", "Connection to must be a lowercase slash id.", target .. ".to")
  end
  if raw.bidirectional ~= nil and type(raw.bidirectional) ~= "boolean" then
    add_error(diagnostics, "world_blueprint.connection.invalid_bidirectional", "bidirectional must be boolean when provided.", target .. ".bidirectional")
  end
end

local function normalize_chunking(raw, mode)
  local enabled = mode == "chunked_world" or mode == "infinite_seeded_world"
  if type(raw) == "table" and raw.enabled ~= nil then
    enabled = raw.enabled == true
  end
  local chunk_width = 32
  local chunk_height = 32
  if type(raw) == "table" then
    if is_positive_integer(raw.chunk_width) then
      chunk_width = raw.chunk_width
    end
    if is_positive_integer(raw.chunk_height) then
      chunk_height = raw.chunk_height
    end
  end
  return {
    enabled = enabled,
    chunk_width = chunk_width,
    chunk_height = chunk_height,
    coordinate_origin = "zero_based",
    storage = enabled and "sparse_chunk_metadata" or "single_map_bounds",
    infinite = mode == "infinite_seeded_world"
  }
end

local function validate_chunking(raw, diagnostics)
  if raw == nil then
    return
  end
  if type(raw) ~= "table" then
    add_error(diagnostics, "world_blueprint.chunking.not_object", "chunking must be an object when provided.", "chunking")
    return
  end
  if raw.enabled ~= nil and type(raw.enabled) ~= "boolean" then
    add_error(diagnostics, "world_blueprint.chunking.invalid_enabled", "chunking.enabled must be boolean when provided.", "chunking.enabled")
  end
  if raw.chunk_width ~= nil and not is_positive_integer(raw.chunk_width) then
    add_error(diagnostics, "world_blueprint.chunking.invalid_width", "chunk_width must be a positive integer when provided.", "chunking.chunk_width")
  end
  if raw.chunk_height ~= nil and not is_positive_integer(raw.chunk_height) then
    add_error(diagnostics, "world_blueprint.chunking.invalid_height", "chunk_height must be a positive integer when provided.", "chunking.chunk_height")
  end
end

local function normalize_global_map(raw, mode)
  local enabled = mode ~= "finite_map"
  if type(raw) == "table" and raw.enabled ~= nil then
    enabled = raw.enabled == true
  end
  return {
    enabled = enabled,
    projection = type(raw) == "table" and type(raw.projection) == "string" and raw.projection or "abstract_2d",
    layers = type(raw) == "table" and is_array(raw.layers) and normalize_string_array(raw.layers) or { "regions", "biomes", "connections" },
    metadata = type(raw) == "table" and type(raw.metadata) == "table" and copy_value(raw.metadata, 0) or {}
  }
end

local function normalize_minimap(raw)
  local enabled = true
  if type(raw) == "table" and raw.enabled ~= nil then
    enabled = raw.enabled == true
  end
  return {
    enabled = enabled,
    layers = type(raw) == "table" and is_array(raw.layers) and normalize_string_array(raw.layers) or { "terrain", "regions", "points_of_interest" },
    icons = type(raw) == "table" and type(raw.icons) == "table" and copy_value(raw.icons, 0) or {},
    metadata = type(raw) == "table" and type(raw.metadata) == "table" and copy_value(raw.metadata, 0) or {}
  }
end

local function resolve_mode(config, input)
  local mode = nil
  if type(input) == "table" and type(input.blueprint_mode) == "string" then
    mode = input.blueprint_mode
  elseif type(config) == "table" and type(config.blueprint_mode) == "string" then
    mode = config.blueprint_mode
  elseif type(config) == "table" and type(config.world_scale) == "string" then
    if config.world_scale == "single_map" then
      mode = "finite_map"
    elseif config.world_scale == "multi_map" then
      mode = "multi_map"
    elseif config.world_scale == "infinite_chunks" then
      mode = "infinite_seeded_world"
    elseif config.world_scale == "region" or config.world_scale == "continent" or config.world_scale == "planet" then
      mode = "region"
    end
  end
  if mode == nil then
    mode = "finite_map"
  end
  return mode
end

local function resolve_scale(mode, config)
  if type(config) == "table" and WORLD_SCALES[config.world_scale] == true then
    return config.world_scale
  end
  if mode == "finite_map" then
    return "single_map"
  end
  if mode == "multi_map" then
    return "multi_map"
  end
  if mode == "infinite_seeded_world" then
    return "infinite_chunks"
  end
  return "region"
end

local function source_array(config, input, key, fallback)
  if type(input) == "table" and is_array(input[key]) then
    return input[key]
  end
  if type(config) == "table" and is_array(config[key]) then
    return config[key]
  end
  return fallback or {}
end

local function source_object(config, input, key)
  if type(input) == "table" and type(input[key]) == "table" then
    return input[key]
  end
  if type(config) == "table" and type(config[key]) == "table" then
    return config[key]
  end
  return nil
end

local function validate_seed(seed, diagnostics)
  if seed ~= nil and not is_non_negative_integer(seed) then
    add_error(diagnostics, "world_blueprint.config.invalid_seed", "seed must be a non-negative integer when provided.", "seed")
  end
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = type(config) == "table" and config or {}
  if config ~= nil and type(config) ~= "table" then
    add_error(diagnostics, "world_blueprint.config.not_object", "Config must be an object when provided.", "config")
  end
  if cfg.world_id ~= nil and not is_slash_id(cfg.world_id) then
    add_error(diagnostics, "world_blueprint.config.invalid_world_id", "world_id must be a lowercase slash id such as world/cursed_valley.", "config.world_id")
  end
  if cfg.title ~= nil and type(cfg.title) ~= "string" then
    add_error(diagnostics, "world_blueprint.config.invalid_title", "title must be a string when provided.", "config.title")
  end
  if cfg.world_scale ~= nil and WORLD_SCALES[cfg.world_scale] ~= true then
    add_error(diagnostics, "world_blueprint.config.invalid_world_scale", "world_scale must be single_map, multi_map, region, continent, planet, or infinite_chunks.", "config.world_scale")
  end
  if cfg.blueprint_mode ~= nil and BLUEPRINT_MODES[cfg.blueprint_mode] ~= true then
    add_error(diagnostics, "world_blueprint.config.invalid_blueprint_mode", "blueprint_mode must be finite_map, multi_map, region, chunked_world, or infinite_seeded_world.", "config.blueprint_mode")
  end
  validate_seed(cfg.seed, diagnostics)
  if cfg.maps ~= nil then
    if not is_array(cfg.maps) then
      add_error(diagnostics, "world_blueprint.config.maps_not_array", "maps must be an array.", "config.maps")
    else
      for index = 1, #cfg.maps do
        validate_map(cfg.maps[index], index, diagnostics)
      end
    end
  end
  if cfg.biomes ~= nil then
    if not is_array(cfg.biomes) then
      add_error(diagnostics, "world_blueprint.config.biomes_not_array", "biomes must be an array.", "config.biomes")
    else
      for index = 1, #cfg.biomes do
        validate_biome(cfg.biomes[index], index, diagnostics)
      end
    end
  end
  if cfg.regions ~= nil then
    if not is_array(cfg.regions) then
      add_error(diagnostics, "world_blueprint.config.regions_not_array", "regions must be an array.", "config.regions")
    else
      for index = 1, #cfg.regions do
        validate_region(cfg.regions[index], index, diagnostics)
      end
    end
  end
  if cfg.connections ~= nil then
    if not is_array(cfg.connections) then
      add_error(diagnostics, "world_blueprint.config.connections_not_array", "connections must be an array.", "config.connections")
    else
      for index = 1, #cfg.connections do
        validate_connection(cfg.connections[index], index, diagnostics)
      end
    end
  end
  validate_chunking(cfg.chunking, diagnostics)
  if cfg.global_map ~= nil and type(cfg.global_map) ~= "table" then
    add_error(diagnostics, "world_blueprint.config.global_map_not_object", "global_map must be an object when provided.", "config.global_map")
  end
  if cfg.minimap ~= nil and type(cfg.minimap) ~= "table" then
    add_error(diagnostics, "world_blueprint.config.minimap_not_object", "minimap must be an object when provided.", "config.minimap")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local config = type(ctx) == "table" and type(ctx.config) == "table" and ctx.config or {}
  local diagnostics = {}
  local mode = resolve_mode(config, input)
  if BLUEPRINT_MODES[mode] ~= true then
    add_error(diagnostics, "world_blueprint.input.invalid_blueprint_mode", "Unsupported blueprint mode.", "blueprint_mode")
    return result(false, {}, diagnostics)
  end

  local world_id = type(input) == "table" and input.world_id or nil
  if world_id == nil then
    world_id = config.world_id
  end
  if world_id == nil then
    world_id = "world/generated_world"
  end
  if not is_slash_id(world_id) then
    add_error(diagnostics, "world_blueprint.input.invalid_world_id", "world_id must be a lowercase slash id.", "world_id")
  end

  local title = type(input) == "table" and type(input.title) == "string" and input.title or config.title
  if type(title) ~= "string" or title == "" then
    title = world_id
  end

  local seed = type(input) == "table" and input.seed or nil
  if seed == nil then
    seed = config.seed
  end
  validate_seed(seed, diagnostics)
  if seed == nil then
    seed = 0
  end

  local raw_biomes = source_array(config, input, "biomes", DEFAULT_BIOMES)
  local biomes = {}
  local biome_by_id = {}
  for index = 1, #raw_biomes do
    local raw = raw_biomes[index]
    validate_biome(raw, index, diagnostics)
    if type(raw) == "table" and is_slash_id(raw.id) then
      if biome_by_id[raw.id] ~= nil then
        add_error(diagnostics, "world_blueprint.input.duplicate_biome_id", "Biome ids must be unique.", "biomes." .. tostring(index) .. ".id")
      else
        local biome = normalize_biome(raw)
        biomes[#biomes + 1] = biome
        biome_by_id[biome.id] = biome
      end
    end
  end

  local raw_maps = source_array(config, input, "maps", {})
  if #raw_maps == 0 then
    raw_maps = {
      {
        id = "map/main",
        title = "Main Map",
        bounds = { x = 0, y = 0, width = 64, height = 64 },
        default_biome_id = #biomes > 0 and biomes[1].id or nil
      }
    }
  end
  local maps = {}
  local map_by_id = {}
  for index = 1, #raw_maps do
    validate_map(raw_maps[index], index, diagnostics)
    if type(raw_maps[index]) == "table" then
      local map = normalize_map(raw_maps[index], index, mode)
      if map_by_id[map.id] ~= nil then
        add_error(diagnostics, "world_blueprint.input.duplicate_map_id", "Map ids must be unique.", "maps." .. tostring(index) .. ".id")
      else
        if map.default_biome_id ~= nil and biome_by_id[map.default_biome_id] == nil then
          add_error(diagnostics, "world_blueprint.input.map_biome_missing", "Map default_biome_id must reference an existing biome.", "maps." .. tostring(index) .. ".default_biome_id")
        end
        maps[#maps + 1] = map
        map_by_id[map.id] = map
      end
    end
  end

  local raw_regions = source_array(config, input, "regions", {})
  local regions = {}
  local region_by_id = {}
  for index = 1, #raw_regions do
    validate_region(raw_regions[index], index, diagnostics)
    if type(raw_regions[index]) == "table" and is_slash_id(raw_regions[index].id) then
      if region_by_id[raw_regions[index].id] ~= nil then
        add_error(diagnostics, "world_blueprint.input.duplicate_region_id", "Region ids must be unique.", "regions." .. tostring(index) .. ".id")
      else
        local region = normalize_region(raw_regions[index])
        if region.map_id ~= nil and map_by_id[region.map_id] == nil then
          add_error(diagnostics, "world_blueprint.input.region_map_missing", "Region map_id must reference an existing map.", "regions." .. tostring(index) .. ".map_id")
        end
        if region.biome_id ~= nil and biome_by_id[region.biome_id] == nil then
          add_error(diagnostics, "world_blueprint.input.region_biome_missing", "Region biome_id must reference an existing biome.", "regions." .. tostring(index) .. ".biome_id")
        end
        regions[#regions + 1] = region
        region_by_id[region.id] = region
      end
    end
  end

  local raw_connections = source_array(config, input, "connections", {})
  local connections = {}
  for index = 1, #raw_connections do
    validate_connection(raw_connections[index], index, diagnostics)
    if type(raw_connections[index]) == "table" and is_slash_id(raw_connections[index].from) and is_slash_id(raw_connections[index].to) then
      if region_by_id[raw_connections[index].from] == nil then
        add_error(diagnostics, "world_blueprint.input.connection_from_missing", "Connection from must reference an existing region.", "connections." .. tostring(index) .. ".from")
      end
      if region_by_id[raw_connections[index].to] == nil then
        add_error(diagnostics, "world_blueprint.input.connection_to_missing", "Connection to must reference an existing region.", "connections." .. tostring(index) .. ".to")
      end
      connections[#connections + 1] = normalize_connection(raw_connections[index])
    end
  end

  if (mode == "region" or mode == "multi_map") and #regions == 0 then
    add_warning(diagnostics, "world_blueprint.input.no_regions", "Regional or multi-map blueprint has no region nodes yet.", "regions")
  end
  if mode == "infinite_seeded_world" and seed == 0 then
    add_warning(diagnostics, "world_blueprint.input.default_seed", "Infinite seeded world is using default seed 0; provide a project seed for stable authored worlds.", "seed")
  end
  if mode == "finite_map" and #maps > 1 then
    add_warning(diagnostics, "world_blueprint.input.finite_map_multiple_maps", "finite_map mode usually expects one map; extra maps were preserved for authoring review.", "maps")
  end

  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      return result(false, {}, diagnostics)
    end
  end

  local chunking = normalize_chunking(source_object(config, input, "chunking"), mode)
  local global_map = normalize_global_map(source_object(config, input, "global_map"), mode)
  local minimap = normalize_minimap(source_object(config, input, "minimap"))
  local world_scale = resolve_scale(mode, config)

  return result(true, {
    world = {
      id = world_id,
      title = title,
      blueprint_mode = mode,
      world_scale = world_scale,
      seed = seed,
      deterministic = true,
      coordinate_system = {
        origin = "zero_based",
        axes = { x = "east", y = "south" },
        chunk_coordinates = "integer_chunk_grid",
        local_coordinates = "zero_based_inside_chunk"
      }
    },
    maps = maps,
    biomes = biomes,
    regions = regions,
    connections = connections,
    chunking = chunking,
    global_map = global_map,
    minimap = minimap,
    generation_policy = {
      emit_huge_tile_arrays = false,
      prefer_sparse_overrides = true,
      chunk_generation_deferred = chunking.enabled,
      future_modules = { "world/chunk_generator/v1", "world/path_carver/v1", "ui/minimap_config/v1", "unity/unity_scene_ir/v1" }
    },
    counts = {
      maps = #maps,
      biomes = #biomes,
      regions = #regions,
      connections = #connections
    }
  }, diagnostics)
end

return M
