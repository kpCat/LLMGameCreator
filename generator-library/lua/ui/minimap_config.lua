local M = {}

M.manifest = {
  id = "ui/minimap_config/v1",
  version = "0.1.0",
  category = "ui",
  title = "Minimap and global map UI IR generator",
  purpose = "Generates deterministic minimap and global map configuration IR with layers, marker categories and fog/reveal metadata.",
  capabilities = {
    "ui.minimap.generate",
    "ui.global_map.generate",
    "ui.map_layers.configure",
    "ui.map_markers.configure"
  },
  input_schema = {
    type = "object",
    fields = {
      map_mode = "optional minimap, global_map or both",
      world_scale = "optional world scale string",
      layers = "optional array",
      marker_categories = "optional array",
      fog = "optional table"
    }
  },
  output_schema = {
    type = "object",
    fields = {
      panels = "array",
      layers = "array",
      marker_categories = "array",
      reveal = "table"
    }
  },
  config_schema = {
    type = "object",
    fields = {
      default_world_scale = "optional string",
      allow_global_map = "optional boolean"
    }
  },
  deterministic = true,
  runtime_targets = { "editor", "unity2d", "unity3d", "unity_ui_ir", "codegen_ir" },
  unsafe_features = {}
}

local VALID_WORLD_SCALES = {
  single_map = true,
  multi_map = true,
  region = true,
  continent = true,
  planet = true,
  infinite_chunks = true
}

local VALID_MAP_MODES = {
  minimap = true,
  global_map = true,
  both = true
}

local function diagnostic(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function add_diag(diagnostics, severity, code, message, target)
  diagnostics[#diagnostics + 1] = diagnostic(severity, code, message, target)
end

local function is_table(value)
  return type(value) == "table"
end

local function is_non_empty_string(value)
  return type(value) == "string" and value ~= ""
end

local function is_ui_id(value)
  return is_non_empty_string(value) and value:match("^[a-z][a-z0-9_]*(/[a-z][a-z0-9_]*)*$") ~= nil
end

local function copy_array(source)
  local result = {}
  if is_table(source) then
    for index = 1, #source do
      result[index] = source[index]
    end
  end
  return result
end

local function default_layers(world_scale)
  local layers = {
    { id = "map/layer_terrain", label = "Terrain", source = "world.terrain", enabled = true },
    { id = "map/layer_roads", label = "Roads", source = "world.roads", enabled = true },
    { id = "map/layer_objectives", label = "Objectives", source = "quest.objectives", enabled = true }
  }
  if world_scale == "region" or world_scale == "continent" or world_scale == "planet" or world_scale == "infinite_chunks" then
    layers[#layers + 1] = { id = "map/layer_regions", label = "Regions", source = "world.regions", enabled = true }
  end
  return layers
end

local function default_markers()
  return {
    { id = "map/marker_player", label = "Player", binding = "actor.player.position", visible_by_default = true },
    { id = "map/marker_quest", label = "Quest", binding = "quest.tracked.locations", visible_by_default = true },
    { id = "map/marker_resource", label = "Resource", binding = "world.resources", visible_by_default = false }
  }
end

local function validate_layers(layers, diagnostics)
  if layers == nil then
    return
  end
  if not is_table(layers) then
    add_diag(diagnostics, "error", "ui.map.invalid_layers", "Layers must be an array when provided.", "input.layers")
    return
  end
  local seen = {}
  for index = 1, #layers do
    local layer = layers[index]
    local target = "input.layers[" .. tostring(index) .. "]"
    if not is_table(layer) then
      add_diag(diagnostics, "error", "ui.map.invalid_layer", "Layer must be a table.", target)
    else
      if not is_ui_id(layer.id) then
        add_diag(diagnostics, "error", "ui.map.invalid_layer_id", "Layer id must use lowercase slash notation.", target .. ".id")
      elseif seen[layer.id] then
        add_diag(diagnostics, "error", "ui.map.duplicate_layer", "Layer id is duplicated.", target .. ".id")
      else
        seen[layer.id] = true
      end
      if layer.source ~= nil and not is_non_empty_string(layer.source) then
        add_diag(diagnostics, "error", "ui.map.invalid_layer_source", "Layer source must be a non-empty string when provided.", target .. ".source")
      end
    end
  end
end

local function validate_markers(markers, diagnostics)
  if markers == nil then
    return
  end
  if not is_table(markers) then
    add_diag(diagnostics, "error", "ui.map.invalid_markers", "Marker categories must be an array when provided.", "input.marker_categories")
    return
  end
  local seen = {}
  for index = 1, #markers do
    local marker = markers[index]
    local target = "input.marker_categories[" .. tostring(index) .. "]"
    if not is_table(marker) then
      add_diag(diagnostics, "error", "ui.map.invalid_marker", "Marker category must be a table.", target)
    else
      if not is_ui_id(marker.id) then
        add_diag(diagnostics, "error", "ui.map.invalid_marker_id", "Marker id must use lowercase slash notation.", target .. ".id")
      elseif seen[marker.id] then
        add_diag(diagnostics, "error", "ui.map.duplicate_marker", "Marker id is duplicated.", target .. ".id")
      else
        seen[marker.id] = true
      end
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if not is_table(config) then
    add_diag(diagnostics, "error", "ui.map.config.invalid", "Minimap config must be a table when provided.", "config")
    return false, diagnostics
  end
  if config.default_world_scale ~= nil and not VALID_WORLD_SCALES[config.default_world_scale] then
    add_diag(diagnostics, "error", "ui.map.config.invalid_world_scale", "Default world scale is not supported.", "config.default_world_scale")
  end
  if config.allow_global_map ~= nil and type(config.allow_global_map) ~= "boolean" then
    add_diag(diagnostics, "error", "ui.map.config.invalid_allow_global_map", "allow_global_map must be boolean when provided.", "config.allow_global_map")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local source = input or {}
  local config = source.config or {}
  local diagnostics = {}
  local _, config_diags = M.validate_config(config)
  for index = 1, #config_diags do
    diagnostics[#diagnostics + 1] = config_diags[index]
  end

  local world_scale = source.world_scale or config.default_world_scale or "single_map"
  if not VALID_WORLD_SCALES[world_scale] then
    add_diag(diagnostics, "error", "ui.map.invalid_world_scale", "World scale is not supported.", "input.world_scale")
    world_scale = "single_map"
  end

  local map_mode = source.map_mode or "both"
  if not VALID_MAP_MODES[map_mode] then
    add_diag(diagnostics, "error", "ui.map.invalid_mode", "Map mode must be minimap, global_map or both.", "input.map_mode")
    map_mode = "minimap"
  end
  if map_mode ~= "minimap" and config.allow_global_map == false then
    add_diag(diagnostics, "warning", "ui.map.global_disabled", "Global map was requested but disabled by config.", "input.map_mode")
    map_mode = "minimap"
  end

  validate_layers(source.layers, diagnostics)
  validate_markers(source.marker_categories, diagnostics)

  local panels = {}
  if map_mode == "minimap" or map_mode == "both" then
    panels[#panels + 1] = {
      id = "map/minimap_panel",
      kind = "minimap_panel",
      anchor = "top_right",
      size = source.minimap_size or { width = 22, height = 22 },
      visibility = "always"
    }
  end
  if map_mode == "global_map" or map_mode == "both" then
    panels[#panels + 1] = {
      id = "map/global_map_panel",
      kind = "global_map_panel",
      anchor = "center",
      size = source.global_map_size or { width = 84, height = 78 },
      visibility = { mode = "when", binding = "ui.global_map.open" }
    }
  end

  local reveal = source.fog or {
    mode = "fog_of_war",
    discovered_binding = "world.discovered_cells",
    reveal_radius_binding = "actor.vision_radius"
  }

  return {
    ok = #diagnostics == 0,
    data = {
      ir_type = "ui.map_config",
      version = "0.1.0",
      map_mode = map_mode,
      world_scale = world_scale,
      panels = panels,
      layers = copy_array(source.layers or default_layers(world_scale)),
      marker_categories = copy_array(source.marker_categories or default_markers()),
      reveal = reveal,
      compatibility = {
        single_map = true,
        multi_map = true,
        region = true,
        continent = true,
        planet = true,
        infinite_chunks = true
      }
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
