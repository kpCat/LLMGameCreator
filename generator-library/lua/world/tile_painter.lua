local M = {}

M.manifest = {
  id = "world/tile_painter/v1",
  version = "0.1.0",
  category = "world",
  title = "Sparse tile painter",
  purpose = "Build compact sparse tile layers for chunks, roads, blockers, and minimap overlays without requiring full tile arrays.",
  capabilities = {
    "world.tile.paint",
    "world.tile.sparse_overrides",
    "world.road.paint",
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
      default_tile_id = { type = "string" },
      default_walkable = { type = "boolean" },
      default_minimap_key = { type = "string" },
      operations = { type = "array" }
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

local function make_tile(op, fallback_layer)
  return {
    x = op.x,
    y = op.y,
    tile_id = op.tile_id,
    layer = type(op.layer) == "string" and op.layer or fallback_layer,
    walkable = op.walkable ~= false,
    minimap_key = type(op.minimap_key) == "string" and op.minimap_key or nil,
    tags = is_array(op.tags) and copy_value(op.tags, 0) or {}
  }
end

local function put_tile(state, tile)
  local key = cell_key(tile.x, tile.y)
  state.by_key[key] = tile
  state.sparse[#state.sparse + 1] = tile
  if tile.walkable == false then
    state.walkability[#state.walkability + 1] = { x = tile.x, y = tile.y, walkable = false, reason = tile.layer }
  end
  if tile.minimap_key ~= nil then
    state.minimap.points[#state.minimap.points + 1] = { x = tile.x, y = tile.y, key = tile.minimap_key, layer = tile.layer }
  end
end

local function paint_point(state, op, diagnostics, target)
  if not in_bounds(op.x, op.y, state.width, state.height) then
    add_diagnostic(diagnostics, "warning", "tile_painter.point.out_of_bounds", "Paint point is outside the chunk and was skipped.", target)
    return
  end
  if not is_slash_id(op.tile_id) then
    add_diagnostic(diagnostics, "error", "tile_painter.point.invalid_tile_id", "Paint point tile_id must be a lowercase slash id.", target .. ".tile_id")
    return
  end
  put_tile(state, make_tile(op, "terrain"))
end

local function paint_rect(state, op, diagnostics, target)
  if not is_integer(op.x) or not is_integer(op.y) or not is_integer(op.width) or not is_integer(op.height) then
    add_diagnostic(diagnostics, "error", "tile_painter.rect.invalid_bounds", "Rect operation requires integer x, y, width, and height.", target)
    return
  end
  if op.width < 1 or op.height < 1 then
    add_diagnostic(diagnostics, "error", "tile_painter.rect.invalid_size", "Rect operation width and height must be positive.", target)
    return
  end
  if not is_slash_id(op.tile_id) then
    add_diagnostic(diagnostics, "error", "tile_painter.rect.invalid_tile_id", "Rect tile_id must be a lowercase slash id.", target .. ".tile_id")
    return
  end
  for y = op.y, op.y + op.height - 1 do
    for x = op.x, op.x + op.width - 1 do
      if in_bounds(x, y, state.width, state.height) then
        local point = copy_value(op, 0)
        point.x = x
        point.y = y
        put_tile(state, make_tile(point, "terrain"))
      end
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

local function paint_line(state, op, diagnostics, target, fallback_layer)
  if type(op.from) ~= "table" or type(op.to) ~= "table" then
    add_diagnostic(diagnostics, "error", "tile_painter.line.invalid_endpoints", "Line operation requires from and to positions.", target)
    return
  end
  local x = op.from.x
  local y = op.from.y
  local stop_x = op.to.x
  local stop_y = op.to.y
  if not in_bounds(x, y, state.width, state.height) or not in_bounds(stop_x, stop_y, state.width, state.height) then
    add_diagnostic(diagnostics, "warning", "tile_painter.line.out_of_bounds", "Line endpoint is outside the chunk; in-bounds cells will still be painted.", target)
  end
  if not is_slash_id(op.tile_id) then
    add_diagnostic(diagnostics, "error", "tile_painter.line.invalid_tile_id", "Line tile_id must be a lowercase slash id.", target .. ".tile_id")
    return
  end
  local guard = state.width + state.height + 4
  while guard > 0 do
    if in_bounds(x, y, state.width, state.height) then
      local point = copy_value(op, 0)
      point.x = x
      point.y = y
      put_tile(state, make_tile(point, fallback_layer or "terrain"))
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
  add_diagnostic(diagnostics, "error", "tile_painter.line.guard_exhausted", "Line painter guard was exhausted.", target)
end

local function paint_road(state, op, diagnostics, target)
  if not is_array(op.points) or #op.points < 2 then
    add_diagnostic(diagnostics, "error", "tile_painter.road.invalid_points", "Road operation requires at least two points.", target .. ".points")
    return
  end
  local road_tile_id = type(op.tile_id) == "string" and op.tile_id or "tile/road"
  local road_key = type(op.minimap_key) == "string" and op.minimap_key or "road"
  for index = 1, #op.points - 1 do
    local from = op.points[index]
    local to = op.points[index + 1]
    paint_line(state, {
      from = from,
      to = to,
      tile_id = road_tile_id,
      layer = "road",
      walkable = true,
      minimap_key = road_key,
      tags = { "road" }
    }, diagnostics, target .. ".points." .. tostring(index), "road")
  end
  if is_array(op.blocked_cells) then
    local blocked_tile_id = type(op.blocked_tile_id) == "string" and op.blocked_tile_id or "tile/blocked_road"
    for index = 1, #op.blocked_cells do
      local cell = op.blocked_cells[index]
      if type(cell) == "table" and in_bounds(cell.x, cell.y, state.width, state.height) then
        put_tile(state, {
          x = cell.x,
          y = cell.y,
          tile_id = blocked_tile_id,
          layer = "blocker",
          walkable = false,
          minimap_key = type(op.blocked_minimap_key) == "string" and op.blocked_minimap_key or "blocked_road",
          tags = { "road", "blocked" }
        })
      else
        add_diagnostic(diagnostics, "warning", "tile_painter.road.blocked_cell_out_of_bounds", "Blocked road cell is outside the chunk and was skipped.", target .. ".blocked_cells." .. tostring(index))
      end
    end
  end
end

local function build_state(config, input)
  local width = input.width or config.width or 16
  local height = input.height or config.height or 16
  return {
    width = width,
    height = height,
    default_tile = {
      tile_id = input.default_tile_id or config.default_tile_id or "tile/grass",
      walkable = input.default_walkable ~= false and config.default_walkable ~= false,
      minimap_key = input.default_minimap_key or config.default_minimap_key or "default"
    },
    sparse = {},
    by_key = {},
    walkability = {},
    minimap = {
      width = width,
      height = height,
      default_key = input.default_minimap_key or config.default_minimap_key or "default",
      points = {}
    }
  }
end

function M.validate_config(config)
  local diagnostics = {}
  config = type(config) == "table" and config or {}
  if config.width ~= nil and (not is_integer(config.width) or config.width < 1) then
    add_diagnostic(diagnostics, "error", "tile_painter.config.invalid_width", "width must be a positive integer.", "width")
  end
  if config.height ~= nil and (not is_integer(config.height) or config.height < 1) then
    add_diagnostic(diagnostics, "error", "tile_painter.config.invalid_height", "height must be a positive integer.", "height")
  end
  if config.default_tile_id ~= nil and not is_slash_id(config.default_tile_id) then
    add_diagnostic(diagnostics, "error", "tile_painter.config.invalid_default_tile_id", "default_tile_id must be a lowercase slash id.", "default_tile_id")
  end
  if config.operations ~= nil and not is_array(config.operations) then
    add_diagnostic(diagnostics, "error", "tile_painter.config.invalid_operations", "operations must be an array when provided.", "operations")
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
  local state = build_state(config, input)
  if not is_integer(state.width) or not is_integer(state.height) or state.width < 1 or state.height < 1 then
    add_diagnostic(diagnostics, "error", "tile_painter.input.invalid_size", "Input width and height must resolve to positive integers.", "input")
    return result(false, {}, diagnostics)
  end
  if not is_slash_id(state.default_tile.tile_id) then
    add_diagnostic(diagnostics, "error", "tile_painter.input.invalid_default_tile_id", "Default tile id must be a lowercase slash id.", "default_tile_id")
    return result(false, {}, diagnostics)
  end
  local operations = input.operations or config.operations or {}
  if not is_array(operations) then
    add_diagnostic(diagnostics, "error", "tile_painter.input.invalid_operations", "operations must be an array.", "operations")
    return result(false, {}, diagnostics)
  end
  for index = 1, #operations do
    local op = operations[index]
    local target = "operations." .. tostring(index)
    if type(op) ~= "table" or type(op.type) ~= "string" then
      add_diagnostic(diagnostics, "error", "tile_painter.operation.invalid", "Operation must be an object with a type.", target)
    elseif op.type == "set" then
      paint_point(state, op, diagnostics, target)
    elseif op.type == "rect" then
      paint_rect(state, op, diagnostics, target)
    elseif op.type == "line" then
      paint_line(state, op, diagnostics, target, op.layer)
    elseif op.type == "road" then
      paint_road(state, op, diagnostics, target)
    else
      add_diagnostic(diagnostics, "error", "tile_painter.operation.unknown_type", "Unsupported tile paint operation type.", target .. ".type")
    end
  end
  local has_error = false
  for index = 1, #diagnostics do
    if diagnostics[index].severity == "error" then
      has_error = true
    end
  end
  return result(not has_error, {
    width = state.width,
    height = state.height,
    default_tile = state.default_tile,
    sparse_tiles = state.sparse,
    walkability_overrides = state.walkability,
    minimap_layer = state.minimap,
    representation = "sparse_overrides"
  }, diagnostics)
end

return M
