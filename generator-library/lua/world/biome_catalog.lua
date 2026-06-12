local M = {}

M.manifest = {
  id = "world/biome_catalog/v1",
  version = "0.1.0",
  category = "world",
  title = "Biome catalog normalizer",
  purpose = "Validate and normalize compact biome definitions for world blueprints, chunk generators, minimaps, and future runtime adapters.",
  capabilities = {
    "world.biome_catalog.validate",
    "world.biome_catalog.normalize",
    "world.biome_catalog.index",
    "world.biome_catalog.tags"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      biomes = { type = "array" },
      allow_empty = { type = "boolean" },
      default_biome_id = { type = "string" }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  supported_time_modes = { "realtime", "turn_based", "mixed", "paused_planning" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
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
  },
  {
    id = "biome/rocky_hills",
    title = "Rocky Hills",
    temperature = 0.48,
    humidity = 0.25,
    danger = 0.35,
    tags = { "rock", "height", "mineable" },
    resources = { "resource/stone", "resource/ore" },
    minimap = { color_key = "hills", pattern = "ridge" }
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

local function copy_array(value)
  if not is_array(value) then
    return {}
  end
  local copy = {}
  for index = 1, #value do
    copy[index] = copy_value(value[index], 0)
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

local function number_or_default(value, fallback)
  if type(value) == "number" then
    return value
  end
  return fallback
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

local function index_add(index, key, value)
  if type(key) ~= "string" or key == "" then
    return
  end
  local bucket = index[key]
  if bucket == nil then
    bucket = {}
    index[key] = bucket
  end
  bucket[#bucket + 1] = value
end

local function validate_biome(raw, index, diagnostics)
  local target = "biomes." .. tostring(index)
  if type(raw) ~= "table" then
    add_error(diagnostics, "biome_catalog.biome.not_object", "Biome entry must be an object.", target)
    return false
  end
  if not is_slash_id(raw.id) then
    add_error(diagnostics, "biome_catalog.biome.invalid_id", "Biome id must be a lowercase slash id such as biome/dark_forest.", target .. ".id")
  end
  if raw.title ~= nil and type(raw.title) ~= "string" then
    add_error(diagnostics, "biome_catalog.biome.invalid_title", "Biome title must be a string when provided.", target .. ".title")
  end
  if raw.temperature ~= nil and type(raw.temperature) ~= "number" then
    add_error(diagnostics, "biome_catalog.biome.invalid_temperature", "Biome temperature must be a 0..1 number when provided.", target .. ".temperature")
  end
  if raw.humidity ~= nil and type(raw.humidity) ~= "number" then
    add_error(diagnostics, "biome_catalog.biome.invalid_humidity", "Biome humidity must be a 0..1 number when provided.", target .. ".humidity")
  end
  if raw.danger ~= nil and type(raw.danger) ~= "number" then
    add_error(diagnostics, "biome_catalog.biome.invalid_danger", "Biome danger must be a 0..1 number when provided.", target .. ".danger")
  end
  if raw.tags ~= nil and not is_array(raw.tags) then
    add_error(diagnostics, "biome_catalog.biome.invalid_tags", "Biome tags must be an array of strings.", target .. ".tags")
  end
  if raw.resources ~= nil and not is_array(raw.resources) then
    add_error(diagnostics, "biome_catalog.biome.invalid_resources", "Biome resources must be an array of resource ids.", target .. ".resources")
  end
  if raw.minimap ~= nil and type(raw.minimap) ~= "table" then
    add_error(diagnostics, "biome_catalog.biome.invalid_minimap", "Biome minimap metadata must be an object when provided.", target .. ".minimap")
  end
  return true
end

local function normalize_biome(raw)
  return {
    id = raw.id,
    title = type(raw.title) == "string" and raw.title or raw.id,
    temperature = clamp01(number_or_default(raw.temperature, 0.5)),
    humidity = clamp01(number_or_default(raw.humidity, 0.5)),
    danger = clamp01(number_or_default(raw.danger, 0.0)),
    tags = normalize_string_array(raw.tags),
    resources = normalize_string_array(raw.resources),
    minimap = type(raw.minimap) == "table" and copy_value(raw.minimap, 0) or {}
  }
end

local function climate_band(value)
  if value < 0.34 then
    return "low"
  end
  if value < 0.67 then
    return "medium"
  end
  return "high"
end

local function source_biomes(config, input)
  if type(input) == "table" and is_array(input.biomes) then
    return input.biomes
  end
  if type(config) == "table" and is_array(config.biomes) then
    return config.biomes
  end
  return DEFAULT_BIOMES
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = type(config) == "table" and config or {}
  if config ~= nil and type(config) ~= "table" then
    add_error(diagnostics, "biome_catalog.config.not_object", "Config must be an object when provided.", "config")
  end
  if cfg.biomes ~= nil then
    if not is_array(cfg.biomes) then
      add_error(diagnostics, "biome_catalog.config.biomes_not_array", "Config biomes must be an array.", "config.biomes")
    else
      local ids = {}
      for index = 1, #cfg.biomes do
        local before = #diagnostics
        validate_biome(cfg.biomes[index], index, diagnostics)
        local biome = cfg.biomes[index]
        if type(biome) == "table" and is_slash_id(biome.id) then
          if ids[biome.id] == true then
            add_error(diagnostics, "biome_catalog.config.duplicate_biome_id", "Biome ids must be unique.", "config.biomes." .. tostring(index) .. ".id")
          end
          ids[biome.id] = true
        end
        if #diagnostics == before and type(biome) == "table" and biome.resources ~= nil then
          for resource_index = 1, #biome.resources do
            if not is_slash_id(biome.resources[resource_index]) then
              add_warning(diagnostics, "biome_catalog.config.resource_id_style", "Resource ids should use lowercase slash id style.", "config.biomes." .. tostring(index) .. ".resources." .. tostring(resource_index))
            end
          end
        end
      end
    end
  end
  if cfg.allow_empty ~= nil and type(cfg.allow_empty) ~= "boolean" then
    add_error(diagnostics, "biome_catalog.config.allow_empty_invalid", "allow_empty must be boolean when provided.", "config.allow_empty")
  end
  if cfg.default_biome_id ~= nil and not is_slash_id(cfg.default_biome_id) then
    add_error(diagnostics, "biome_catalog.config.default_biome_invalid", "default_biome_id must be a lowercase slash id.", "config.default_biome_id")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local config = type(ctx) == "table" and type(ctx.config) == "table" and ctx.config or {}
  local diagnostics = {}
  local raw_biomes = source_biomes(config, input)
  if not is_array(raw_biomes) then
    add_error(diagnostics, "biome_catalog.input.biomes_not_array", "Biomes must be provided as an array.", "biomes")
    return result(false, {}, diagnostics)
  end
  if #raw_biomes == 0 and config.allow_empty ~= true then
    add_error(diagnostics, "biome_catalog.input.empty", "Biome catalog must contain at least one biome unless allow_empty is true.", "biomes")
    return result(false, {}, diagnostics)
  end

  local ids = {}
  local biomes = {}
  local by_id = {}
  local tag_index = {}
  local resource_index = {}
  local climate_index = {}

  for index = 1, #raw_biomes do
    local raw = raw_biomes[index]
    validate_biome(raw, index, diagnostics)
    if type(raw) == "table" and is_slash_id(raw.id) then
      if ids[raw.id] == true then
        add_error(diagnostics, "biome_catalog.input.duplicate_biome_id", "Biome ids must be unique.", "biomes." .. tostring(index) .. ".id")
      else
        ids[raw.id] = true
        local normalized = normalize_biome(raw)
        biomes[#biomes + 1] = normalized
        by_id[normalized.id] = normalized
        for tag_index_value = 1, #normalized.tags do
          index_add(tag_index, normalized.tags[tag_index_value], normalized.id)
        end
        for resource_index_value = 1, #normalized.resources do
          index_add(resource_index, normalized.resources[resource_index_value], normalized.id)
        end
        local band = "temperature_" .. climate_band(normalized.temperature) .. "/humidity_" .. climate_band(normalized.humidity)
        index_add(climate_index, band, normalized.id)
      end
    end
  end

  if config.default_biome_id ~= nil and by_id[config.default_biome_id] == nil then
    add_error(diagnostics, "biome_catalog.input.default_biome_missing", "default_biome_id must reference an existing biome.", "config.default_biome_id")
  end

  if #diagnostics > 0 then
    for index = 1, #diagnostics do
      if diagnostics[index].severity == "error" then
        return result(false, {}, diagnostics)
      end
    end
  end

  local default_biome_id = config.default_biome_id
  if default_biome_id == nil and #biomes > 0 then
    default_biome_id = biomes[1].id
  end

  return result(true, {
    biomes = biomes,
    by_id = by_id,
    default_biome_id = default_biome_id,
    tag_index = tag_index,
    resource_index = resource_index,
    climate_index = climate_index,
    counts = {
      biomes = #biomes
    }
  }, diagnostics)
end

return M
