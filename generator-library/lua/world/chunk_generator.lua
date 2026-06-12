local M = {}

M.manifest = {
  id = "world/chunk_generator/v1",
  version = "0.1.0",
  category = "world",
  title = "Chunk and grid map generator",
  purpose = "Generate deterministic compact chunk IR with sparse terrain overrides, roads, blocked road cells, landmarks, walkability, and minimap layer metadata.",
  capabilities = {
    "world.chunk.generate",
    "world.chunk.sparse_map",
    "world.road.generate",
    "world.landmark.place",
    "world.walkability.emit",
    "world.minimap.layer"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      chunk_size = { type = "object" },
      seed = { type = "integer" },
      default_tile_id = { type = "string" },
      terrain_rules = { type = "array" },
      sparse_overrides = { type = "array" },
      roads = { type = "array" },
      landmarks = { type = "array" },
      include_full_tiles = { type = "boolean" }
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

local function add_diagnostic(list, severity, code, message, target)
  list[#list + 1] = diagnostic(severity, code, message, target)
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

local function cell_key(x, y)
  return tostring(x) .. ":" .. tostring(y)
end

local function in_bounds(x, y, width, height)
  return is_integer(x) and is_integer(y) and x >= 0 and y >= 0 and x < width and y < height
end

local function stable_hash(value)
  local text = tostring(value)
  local hash = 2166136261
  for index = 1, #text do
    hash = (hash + string.byte(text, index) * 16777619 + index) % 2147483647
  end
  return hash
end

local function coord_value(seed, chunk_x, chunk_y, x, y, salt)
  local hash = stable_hash(tostring(seed) .. ":" .. tostring(chunk_x) .. ":" .. tostring(chunk_y) .. ":" .. tostring(x) .. ":" .. tostring(y) .. ":" .. tostring(salt))
  return hash % 10000
end

local function clamp_integer(value, fallback, low, high)
  local out = is_integer(value) and value or fallback
  if out < low then
    return low
  end
  if out > high then
    return high
  end
  return out
end

local function chunk_size(config, input)
  local source = type(input.chunk_size) == "table" and input.chunk_size or config.chunk_size
  source = type(source) == "table" and source or {}
  local width = source.width or input.width or config.width or 16
  local height = source.height or input.height or config.height or 16
  return width, height
end

local function normalize_rule(raw, index, diagnostics)
  local target = "terrain_rules." .. tostring(index)
  if type(raw) ~= "table" then
    add_diagnostic(diagnostics, "error", "chunk_generator.terrain_rule.not_object", "Terrain rule must be an object.", target)
    return nil
  end
  if not is_slash_id(raw.tile_id) then
    add_diagnostic(diagnostics, "error", "chunk_generator.terrain_rule.invalid_tile_id", "Terrain rule tile_id must be a lowercase slash id.", target .. ".tile_id")
  end
  local threshold = clamp_integer(raw.threshold, 5000, 0, 9999)
  return {
    tile_id = raw.tile_id,
    threshold = threshold,
    walkable = raw.walkable ~= false,
    minimap_key = type(raw.minimap_key) == "string" and raw.minimap_key or "terrain",
    tags = is_array(raw.tags) and copy_value(raw.tags, 0) or {}
  }
end

local function normalize_rules(raw_rules, diagnostics)
  local rules = {}
  if not is_array(raw_rules) then
    add_diagnostic(diagnostics, "error", "chunk_generator.terrain_rules.invalid", "terrain_rules must be an array.", "terrain_rules")
    return rules
  end
  for index = 1, #raw_rules do
    local rule = normalize_rule(raw_rules[index], index, diagnostics)
    if rule ~= nil then
      rules[#rules + 1] = rule
    end
  end
  return rules
end

local function put_tile(state, tile)
  local key = cell_key(tile.x, tile.y)
  state.sparse_by_key[key] = tile
  state.sparse[#state.sparse + 1] = tile
  if tile.walkable == false then
    state.walkability[#state.walkability + 1] = { x = tile.x, y = tile.y, walkable = false, reason = tile.layer }
  end
  if tile.minimap_key ~= nil then
    state.minimap.points[#state.minimap.points + 1] = { x = tile.x, y = tile.y, key = tile.minimap_key, layer = tile.layer }
  end
end

local function terrain_from_rules(rules, state, x, y)
  if #rules == 0 then
    return nil
  end
  local value = coord_value(state.seed, state.chunk_x, state.chunk_y, x, y, "terrain")
  local selected = nil
  for index = 1, #rules do
    local rule = rules[index]
    if value <= rule.threshold then
      selected = rule
      break
    end
  end
  return selected
end

local function apply_terrain(state, rules)
  if #rules == 0 then
    return
  end
  for y = 0, state.height - 1 do
    for x = 0, state.width - 1 do
      local rule = terrain_from_rules(rules, state, x, y)
      if rule ~= nil and rule.tile_id ~= state.default_tile.tile_id then
        put_tile(state, {
          x = x,
          y = y,
          tile_id = rule.tile_id,
          layer = "terrain",
          walkable = rule.walkable,
          minimap_key = rule.minimap_key,
          tags = copy_value(rule.tags, 0)
        })
      end
    end
  end
end

local function apply_sparse_overrides(state, overrides, diagnostics, target_prefix)
  if overrides == nil then
    return
  end
  if not is_array(overrides) then
    add_diagnostic(diagnostics, "error", "chunk_generator.sparse_overrides.invalid", "sparse_overrides must be an array.", target_prefix)
    return
  end
  for index = 1, #overrides do
    local item = overrides[index]
    local target = target_prefix .. "." .. tostring(index)
    if type(item) ~= "table" then
      add_diagnostic(diagnostics, "error", "chunk_generator.override.not_object", "Sparse override must be an object.", target)
    elseif not in_bounds(item.x, item.y, state.width, state.height) then
      add_diagnostic(diagnostics, "warning", "chunk_generator.override.out_of_bounds", "Sparse override outside chunk was skipped.", target)
    elseif not is_slash_id(item.tile_id) then
      add_diagnostic(diagnostics, "error", "chunk_generator.override.invalid_tile_id", "Sparse override tile_id must be a lowercase slash id.", target .. ".tile_id")
    else
      put_tile(state, {
        x = item.x,
        y = item.y,
        tile_id = item.tile_id,
        layer = type(item.layer) == "string" and item.layer or "override",
        walkable = item.walkable ~= false,
        minimap_key = type(item.minimap_key) == "string" and item.minimap_key or nil,
        tags = is_array(item.tags) and copy_value(item.tags, 0) or {}
      })
    end
  end
end

local function step_toward(a, b)
  if a < b then
    return a + 1
  end
  if a > b then
    return a - 1
  end
  return a
end

local function paint_line(state, from, to, tile_id, minimap_key, layer, walkable, tags, diagnostics, target)
  if type(from) ~= "table" or type(to) ~= "table" then
    add_diagnostic(diagnostics, "error", "chunk_generator.line.invalid_endpoints", "Line requires from and to positions.", target)
    return
  end
  if not is_slash_id(tile_id) then
    add_diagnostic(diagnostics, "error", "chunk_generator.line.invalid_tile_id", "Line tile_id must be a lowercase slash id.", target)
    return
  end
  local x = from.x
  local y = from.y
  local stop_x = to.x
  local stop_y = to.y
  local guard = state.width + state.height + 4
  while guard > 0 do
    if in_bounds(x, y, state.width, state.height) then
      put_tile(state, {
        x = x,
        y = y,
        tile_id = tile_id,
        layer = layer,
        walkable = walkable ~= false,
        minimap_key = minimap_key,
        tags = copy_value(tags or {}, 0)
      })
    end
    if x == stop_x and y == stop_y then
      return
    end
    if x ~= stop_x then
      x = step_toward(x, stop_x)
    elseif y ~= stop_y then
      y = step_toward(y, stop_y)
    end
    guard = guard - 1
  end
  add_diagnostic(diagnostics, "error", "chunk_generator.line.guard_exhausted", "Line guard exhausted.", target)
end

local function apply_roads(state, roads, diagnostics)
  if roads == nil then
    return
  end
  if not is_array(roads) then
    add_diagnostic(diagnostics, "error", "chunk_generator.roads.invalid", "roads must be an array.", "roads")
    return
  end
  for index = 1, #roads do
    local road = roads[index]
    local target = "roads." .. tostring(index)
    if type(road) ~= "table" or not is_array(road.points) or #road.points < 2 then
      add_diagnostic(diagnostics, "error", "chunk_generator.road.invalid_points", "Road requires at least two points.", target .. ".points")
    else
      local road_tile_id = type(road.tile_id) == "string" and road.tile_id or "tile/road"
      local road_minimap_key = type(road.minimap_key) == "string" and road.minimap_key or "road"
      for point_index = 1, #road.points - 1 do
        paint_line(state, road.points[point_index], road.points[point_index + 1], road_tile_id, road_minimap_key, "road", true, { "road" }, diagnostics, target .. ".points." .. tostring(point_index))
      end
      if is_array(road.blocked_cells) then
        local blocked_tile_id = type(road.blocked_tile_id) == "string" and road.blocked_tile_id or "tile/blocked_road"
        for block_index = 1, #road.blocked_cells do
          local cell = road.blocked_cells[block_index]
          if type(cell) == "table" and in_bounds(cell.x, cell.y, state.width, state.height) then
            put_tile(state, {
              x = cell.x,
              y = cell.y,
              tile_id = blocked_tile_id,
              layer = "blocker",
              walkable = false,
              minimap_key = type(road.blocked_minimap_key) == "string" and road.blocked_minimap_key or "blocked_road",
              tags = { "road", "blocked" }
            })
          else
            add_diagnostic(diagnostics, "warning", "chunk_generator.road.blocked_cell_out_of_bounds", "Blocked road cell outside chunk was skipped.", target .. ".blocked_cells." .. tostring(block_index))
          end
        end
      end
    end
  end
end

local function candidate_position(seed, chunk_x, chunk_y, width, height, slot)
  return {
    x = coord_value(seed, chunk_x, chunk_y, slot, 11, "landmark_x") % width,
    y = coord_value(seed, chunk_x, chunk_y, slot, 17, "landmark_y") % height
  }
end

local function apply_landmarks(state, landmarks, diagnostics)
  if landmarks == nil then
    return
  end
  if not is_array(landmarks) then
    add_diagnostic(diagnostics, "error", "chunk_generator.landmarks.invalid", "landmarks must be an array.", "landmarks")
    return
  end
  local placed = {}
  for index = 1, #landmarks do
    local landmark = landmarks[index]
    local target = "landmarks." .. tostring(index)
    if type(landmark) ~= "table" or not is_slash_id(landmark.id) then
      add_diagnostic(diagnostics, "error", "chunk_generator.landmark.invalid_id", "Landmark id must be a lowercase slash id.", target .. ".id")
    else
      local pos = type(landmark.position) == "table" and { x = landmark.position.x, y = landmark.position.y } or candidate_position(state.seed, state.chunk_x, state.chunk_y, state.width, state.height, index)
      if not in_bounds(pos.x, pos.y, state.width, state.height) then
        add_diagnostic(diagnostics, "warning", "chunk_generator.landmark.out_of_bounds", "Landmark outside chunk was skipped.", target .. ".position")
      else
        local key = cell_key(pos.x, pos.y)
        local existing = state.sparse_by_key[key]
        if type(existing) == "table" and existing.walkable == false and landmark.allow_blocked ~= true then
          add_diagnostic(diagnostics, "warning", "chunk_generator.landmark.blocked_cell", "Landmark target cell is blocked and was skipped.", target)
        else
          local tile_id = type(landmark.tile_id) == "string" and landmark.tile_id or "tile/landmark"
          if not is_slash_id(tile_id) then
            add_diagnostic(diagnostics, "error", "chunk_generator.landmark.invalid_tile_id", "Landmark tile_id must be a lowercase slash id.", target .. ".tile_id")
          else
            local item = {
              id = landmark.id,
              title = type(landmark.title) == "string" and landmark.title or landmark.id,
              position = pos,
              tile_id = tile_id,
              tags = is_array(landmark.tags) and copy_value(landmark.tags, 0) or {},
              minimap_key = type(landmark.minimap_key) == "string" and landmark.minimap_key or "landmark"
            }
            placed[#placed + 1] = item
            put_tile(state, {
              x = pos.x,
              y = pos.y,
              tile_id = tile_id,
              layer = "landmark",
              walkable = landmark.walkable == true,
              minimap_key = item.minimap_key,
              tags = item.tags,
              landmark_id = landmark.id
            })
          end
        end
      end
    end
  end
  state.landmarks = placed
end

local function build_full_tiles(state, limit, diagnostics)
  local total = state.width * state.height
  if total > limit then
    add_diagnostic(diagnostics, "info", "chunk_generator.full_tiles.omitted", "Full tile array omitted because chunk exceeds max_full_tiles; use sparse_tiles plus default_tile.", "include_full_tiles")
    return nil
  end
  local rows = {}
  for y = 0, state.height - 1 do
    local row = {}
    for x = 0, state.width - 1 do
      local tile = state.sparse_by_key[cell_key(x, y)]
      if tile == nil then
        row[#row + 1] = copy_value(state.default_tile, 0)
      else
        row[#row + 1] = copy_value(tile, 0)
      end
    end
    rows[#rows + 1] = row
  end
  return rows
end

function M.validate_config(config)
  local diagnostics = {}
  config = type(config) == "table" and config or {}
  local width = nil
  local height = nil
  if type(config.chunk_size) == "table" then
    width = config.chunk_size.width
    height = config.chunk_size.height
  end
  if width ~= nil and (not is_integer(width) or width < 1) then
    add_diagnostic(diagnostics, "error", "chunk_generator.config.invalid_chunk_width", "chunk_size.width must be a positive integer.", "chunk_size.width")
  end
  if height ~= nil and (not is_integer(height) or height < 1) then
    add_diagnostic(diagnostics, "error", "chunk_generator.config.invalid_chunk_height", "chunk_size.height must be a positive integer.", "chunk_size.height")
  end
  if config.seed ~= nil and not is_integer(config.seed) then
    add_diagnostic(diagnostics, "error", "chunk_generator.config.invalid_seed", "seed must be an integer when provided.", "seed")
  end
  if config.default_tile_id ~= nil and not is_slash_id(config.default_tile_id) then
    add_diagnostic(diagnostics, "error", "chunk_generator.config.invalid_default_tile_id", "default_tile_id must be a lowercase slash id.", "default_tile_id")
  end
  if config.terrain_rules ~= nil and not is_array(config.terrain_rules) then
    add_diagnostic(diagnostics, "error", "chunk_generator.config.invalid_terrain_rules", "terrain_rules must be an array when provided.", "terrain_rules")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  input = type(input) == "table" and input or {}
  ctx = type(ctx) == "table" and ctx or {}
  local config = type(ctx.config) == "table" and ctx.config or {}
  local ok_config, config_diagnostics = M.validate_config(config)
  for index = 1, #config_diagnostics do
    diagnostics[#diagnostics + 1] = config_diagnostics[index]
  end
  if not ok_config then
    return result(false, {}, diagnostics)
  end
  local width, height = chunk_size(config, input)
  if not is_integer(width) or not is_integer(height) or width < 1 or height < 1 then
    add_diagnostic(diagnostics, "error", "chunk_generator.input.invalid_chunk_size", "Chunk width and height must be positive integers.", "chunk_size")
    return result(false, {}, diagnostics)
  end
  local default_tile_id = input.default_tile_id or config.default_tile_id or "tile/grass"
  if not is_slash_id(default_tile_id) then
    add_diagnostic(diagnostics, "error", "chunk_generator.input.invalid_default_tile_id", "default_tile_id must be a lowercase slash id.", "default_tile_id")
    return result(false, {}, diagnostics)
  end
  local chunk = type(input.chunk) == "table" and input.chunk or {}
  local state = {
    width = width,
    height = height,
    seed = input.seed or config.seed or 1,
    chunk_x = is_integer(chunk.x) and chunk.x or 0,
    chunk_y = is_integer(chunk.y) and chunk.y or 0,
    default_tile = {
      tile_id = default_tile_id,
      walkable = input.default_walkable ~= false and config.default_walkable ~= false,
      minimap_key = input.default_minimap_key or config.default_minimap_key or "default"
    },
    sparse = {},
    sparse_by_key = {},
    walkability = {},
    minimap = {
      width = width,
      height = height,
      default_key = input.default_minimap_key or config.default_minimap_key or "default",
      points = {}
    },
    landmarks = {}
  }
  local rules = normalize_rules(input.terrain_rules or config.terrain_rules or {}, diagnostics)
  apply_terrain(state, rules)
  apply_sparse_overrides(state, config.sparse_overrides, diagnostics, "config.sparse_overrides")
  apply_sparse_overrides(state, input.sparse_overrides, diagnostics, "input.sparse_overrides")
  apply_roads(state, input.roads or config.roads, diagnostics)
  apply_landmarks(state, input.landmarks or config.landmarks, diagnostics)
  local include_full_tiles = input.include_full_tiles == true or config.include_full_tiles == true
  local max_full_tiles = clamp_integer(input.max_full_tiles or config.max_full_tiles, 1024, 1, 8192)
  local full_tiles = nil
  if include_full_tiles then
    full_tiles = build_full_tiles(state, max_full_tiles, diagnostics)
  end
  local has_error = false
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      has_error = true
    end
  end
  local data = {
    chunk = { x = state.chunk_x, y = state.chunk_y, width = width, height = height },
    seed = state.seed,
    default_tile = state.default_tile,
    sparse_tiles = state.sparse,
    landmarks = state.landmarks,
    walkability_overrides = state.walkability,
    minimap_layer = state.minimap,
    representation = "default_tile_plus_sparse_overrides",
    full_tiles_omitted = full_tiles == nil
  }
  if full_tiles ~= nil then
    data.full_tiles = full_tiles
    data.full_tiles_omitted = false
  end
  return result(not has_error, data, diagnostics)
end

return M
