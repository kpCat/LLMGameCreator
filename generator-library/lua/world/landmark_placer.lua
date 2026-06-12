local M = {}

M.manifest = {
  id = "world/landmark_placer/v1",
  version = "0.1.0",
  category = "world",
  title = "Deterministic landmark placer",
  purpose = "Place compact landmark descriptors and optional tile overrides inside a chunk while respecting bounds, walkability, spacing, roads, and blocked road cells.",
  capabilities = {
    "world.landmark.place",
    "world.landmark.sparse_overrides",
    "world.landmark.road_aware",
    "world.minimap.layer"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      width = { type = "integer" },
      height = { type = "integer" },
      seed = { type = "integer" },
      landmarks = { type = "array" },
      max_count = { type = "integer" },
      min_distance = { type = "integer" }
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

local function coord_hash(seed, chunk_x, chunk_y, slot)
  return stable_hash(tostring(seed) .. ":" .. tostring(chunk_x) .. ":" .. tostring(chunk_y) .. ":" .. tostring(slot))
end

local function normalize_tile_index(sparse_tiles)
  local index = {}
  if is_array(sparse_tiles) then
    for item_index = 1, #sparse_tiles do
      local item = sparse_tiles[item_index]
      if type(item) == "table" and is_integer(item.x) and is_integer(item.y) then
        index[cell_key(item.x, item.y)] = item
      end
    end
  end
  return index
end

local function is_blocked_cell(tile)
  if type(tile) ~= "table" then
    return false
  end
  if tile.walkable == false then
    return true
  end
  if is_array(tile.tags) then
    for index = 1, #tile.tags do
      if tile.tags[index] == "blocked" then
        return true
      end
    end
  end
  return false
end

local function distance_square(a, b)
  local dx = a.x - b.x
  local dy = a.y - b.y
  return dx * dx + dy * dy
end

local function position_allowed(pos, width, height, min_edge_margin, occupied, tile_index)
  if not in_bounds(pos.x, pos.y, width, height) then
    return false
  end
  if pos.x < min_edge_margin or pos.y < min_edge_margin or pos.x >= width - min_edge_margin or pos.y >= height - min_edge_margin then
    return false
  end
  if occupied[cell_key(pos.x, pos.y)] == true then
    return false
  end
  local tile = tile_index[cell_key(pos.x, pos.y)]
  return not is_blocked_cell(tile)
end

local function candidate_position(seed, chunk_x, chunk_y, slot, width, height, margin)
  local usable_width = width - margin * 2
  local usable_height = height - margin * 2
  if usable_width < 1 then
    usable_width = width
    margin = 0
  end
  if usable_height < 1 then
    usable_height = height
    margin = 0
  end
  local x = margin + (coord_hash(seed, chunk_x, chunk_y, slot .. ":x") % usable_width)
  local y = margin + (coord_hash(seed, chunk_x, chunk_y, slot .. ":y") % usable_height)
  return { x = x, y = y }
end

local function normalize_landmark(raw, index, diagnostics)
  local target = "landmarks." .. tostring(index)
  if type(raw) ~= "table" then
    add_diagnostic(diagnostics, "error", "landmark_placer.landmark.not_object", "Landmark entry must be an object.", target)
    return nil
  end
  if not is_slash_id(raw.id) then
    add_diagnostic(diagnostics, "error", "landmark_placer.landmark.invalid_id", "Landmark id must be a lowercase slash id.", target .. ".id")
  end
  local tile_id = type(raw.tile_id) == "string" and raw.tile_id or "tile/landmark"
  if not is_slash_id(tile_id) then
    add_diagnostic(diagnostics, "error", "landmark_placer.landmark.invalid_tile_id", "Landmark tile_id must be a lowercase slash id.", target .. ".tile_id")
  end
  return {
    id = raw.id,
    title = type(raw.title) == "string" and raw.title or raw.id,
    tile_id = tile_id,
    tags = is_array(raw.tags) and copy_value(raw.tags, 0) or {},
    minimap_key = type(raw.minimap_key) == "string" and raw.minimap_key or "landmark",
    walkable = raw.walkable == true,
    fixed_position = type(raw.position) == "table" and copy_value(raw.position, 0) or nil,
    weight = clamp_integer(raw.weight, 1, 1, 100),
    radius = clamp_integer(raw.radius, 0, 0, 8),
    min_distance = raw.min_distance
  }
end

local function place_one(landmark, context, diagnostics, order_index)
  local min_distance = clamp_integer(landmark.min_distance, context.min_distance, 0, context.width + context.height)
  local attempts = context.candidate_count
  local chosen = nil
  if type(landmark.fixed_position) == "table" then
    local pos = { x = landmark.fixed_position.x, y = landmark.fixed_position.y }
    if position_allowed(pos, context.width, context.height, context.edge_margin, context.occupied, context.tile_index) then
      chosen = pos
    else
      add_diagnostic(diagnostics, "warning", "landmark_placer.fixed_position.rejected", "Fixed landmark position is blocked or out of bounds.", landmark.id)
    end
  end
  local attempt = 1
  while chosen == nil and attempt <= attempts do
    local pos = candidate_position(context.seed, context.chunk_x, context.chunk_y, tostring(order_index) .. ":" .. tostring(attempt), context.width, context.height, context.edge_margin)
    if position_allowed(pos, context.width, context.height, context.edge_margin, context.occupied, context.tile_index) then
      local far_enough = true
      for index = 1, #context.placements do
        if distance_square(pos, context.placements[index].position) < min_distance * min_distance then
          far_enough = false
        end
      end
      if far_enough then
        chosen = pos
      end
    end
    attempt = attempt + 1
  end
  if chosen == nil then
    add_diagnostic(diagnostics, "warning", "landmark_placer.no_position", "No valid position was found for landmark.", landmark.id)
    return
  end
  context.occupied[cell_key(chosen.x, chosen.y)] = true
  local placement = {
    id = landmark.id,
    title = landmark.title,
    position = chosen,
    tile_id = landmark.tile_id,
    tags = copy_value(landmark.tags, 0),
    minimap_key = landmark.minimap_key,
    walkable = landmark.walkable
  }
  context.placements[#context.placements + 1] = placement
  context.sparse_overrides[#context.sparse_overrides + 1] = {
    x = chosen.x,
    y = chosen.y,
    tile_id = landmark.tile_id,
    layer = "landmark",
    walkable = landmark.walkable,
    minimap_key = landmark.minimap_key,
    tags = copy_value(landmark.tags, 0),
    landmark_id = landmark.id
  }
end

function M.validate_config(config)
  local diagnostics = {}
  config = type(config) == "table" and config or {}
  if config.width ~= nil and (not is_integer(config.width) or config.width < 1) then
    add_diagnostic(diagnostics, "error", "landmark_placer.config.invalid_width", "width must be a positive integer.", "width")
  end
  if config.height ~= nil and (not is_integer(config.height) or config.height < 1) then
    add_diagnostic(diagnostics, "error", "landmark_placer.config.invalid_height", "height must be a positive integer.", "height")
  end
  if config.seed ~= nil and not is_integer(config.seed) then
    add_diagnostic(diagnostics, "error", "landmark_placer.config.invalid_seed", "seed must be an integer when provided.", "seed")
  end
  if config.landmarks ~= nil and not is_array(config.landmarks) then
    add_diagnostic(diagnostics, "error", "landmark_placer.config.invalid_landmarks", "landmarks must be an array when provided.", "landmarks")
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
  local width = input.width or config.width or 16
  local height = input.height or config.height or 16
  if not is_integer(width) or not is_integer(height) or width < 1 or height < 1 then
    add_diagnostic(diagnostics, "error", "landmark_placer.input.invalid_size", "width and height must be positive integers.", "input")
    return result(false, {}, diagnostics)
  end
  local raw_landmarks = input.landmarks or config.landmarks or {}
  if not is_array(raw_landmarks) then
    add_diagnostic(diagnostics, "error", "landmark_placer.input.invalid_landmarks", "landmarks must be an array.", "landmarks")
    return result(false, {}, diagnostics)
  end
  local context = {
    width = width,
    height = height,
    seed = input.seed or config.seed or 1,
    chunk_x = type(input.chunk) == "table" and input.chunk.x or 0,
    chunk_y = type(input.chunk) == "table" and input.chunk.y or 0,
    edge_margin = clamp_integer(input.edge_margin or config.edge_margin, 1, 0, 8),
    min_distance = clamp_integer(input.min_distance or config.min_distance, 3, 0, width + height),
    candidate_count = clamp_integer(input.candidate_count or config.candidate_count, 24, 1, 200),
    max_count = clamp_integer(input.max_count or config.max_count or #raw_landmarks, #raw_landmarks, 0, #raw_landmarks),
    tile_index = normalize_tile_index(input.sparse_tiles),
    occupied = {},
    placements = {},
    sparse_overrides = {}
  }
  local placed = 0
  for index = 1, #raw_landmarks do
    if placed >= context.max_count then
      break
    end
    local landmark = normalize_landmark(raw_landmarks[index], index, diagnostics)
    if landmark ~= nil then
      local before = #context.placements
      place_one(landmark, context, diagnostics, index)
      if #context.placements > before then
        placed = placed + 1
      end
    end
  end
  local has_error = false
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      has_error = true
    end
  end
  local minimap_points = {}
  for index = 1, #context.placements do
    local placement = context.placements[index]
    minimap_points[#minimap_points + 1] = {
      x = placement.position.x,
      y = placement.position.y,
      key = placement.minimap_key,
      layer = "landmark",
      id = placement.id
    }
  end
  return result(not has_error, {
    width = width,
    height = height,
    seed = context.seed,
    placements = context.placements,
    sparse_overrides = context.sparse_overrides,
    minimap_layer = {
      width = width,
      height = height,
      default_key = input.default_minimap_key or "transparent",
      points = minimap_points
    },
    representation = "landmark_sparse_overrides"
  }, diagnostics)
end

return M
