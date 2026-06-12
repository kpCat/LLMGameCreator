local M = {}

M.manifest = {
  id = "world/path_carver/v1",
  version = "0.1.0",
  category = "world",
  title = "Path Carver",
  purpose = "Carves compact deterministic 2D paths between start, waypoints and objective cells.",
  capabilities = { "world.path.carve", "world.path.ensure_connection" },
  input_schema = {
    type = "table",
    fields = {
      bounds = "{ width:number, height:number }",
      start = "{ x:number, y:number }",
      objective = "{ x:number, y:number }",
      waypoints = "optional array of positions",
      blocked_cells = "optional array of positions that cannot be crossed",
      bridge_cells = "optional array of positions where a bridge tile may be used"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      path = "ordered array of carved cells",
      sparse_tiles = "compact tile override array",
      blocked = "boolean",
      summary = "small metadata object"
    }
  },
  config_schema = {
    road_tile = "optional string",
    bridge_tile = "optional string",
    path_order = "horizontal_first | vertical_first | alternating",
    allow_bridges = "optional boolean",
    max_cells = "optional positive number"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity_tilemap" },
  supported_world_scales = { "single_map", "multi_map", "region", "infinite_chunks" },
  supported_turn_modes = { "realtime", "turn_based", "mixed" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_int(v)
  return type(v) == "number" and v % 1 == 0
end

local function valid_pos(p)
  return type(p) == "table" and is_int(p.x) and is_int(p.y)
end

local function valid_bounds(bounds)
  return type(bounds) == "table" and is_int(bounds.width) and is_int(bounds.height) and bounds.width > 0 and bounds.height > 0
end

local function in_bounds(p, bounds)
  return p.x >= 0 and p.y >= 0 and p.x < bounds.width and p.y < bounds.height
end

local function key(x, y)
  return tostring(x) .. ":" .. tostring(y)
end

local function index_cells(cells)
  local map = {}
  if type(cells) ~= "table" then
    return map
  end
  for i = 1, #cells do
    local c = cells[i]
    if valid_pos(c) then
      map[key(c.x, c.y)] = c
    end
  end
  return map
end

local function push_cell(path, sparse, seen, x, y, tile, role, walkable)
  local k = key(x, y)
  if not seen[k] then
    local cell = { x = x, y = y, tile = tile, role = role, walkable = walkable ~= false }
    path[#path + 1] = cell
    sparse[#sparse + 1] = { x = x, y = y, tile = tile, walkable = cell.walkable, layer = "path" }
    seen[k] = true
  end
end

local function step_axis(current, target, axis)
  if axis == "x" and current.x ~= target.x then
    if current.x < target.x then
      current.x = current.x + 1
    else
      current.x = current.x - 1
    end
    return true
  end
  if axis == "y" and current.y ~= target.y then
    if current.y < target.y then
      current.y = current.y + 1
    else
      current.y = current.y - 1
    end
    return true
  end
  return false
end

local function choose_axes(order, segment_index)
  if order == "vertical_first" then
    return "y", "x"
  end
  if order == "alternating" and segment_index % 2 == 0 then
    return "y", "x"
  end
  return "x", "y"
end

local function carve_segment(from_pos, to_pos, segment_index, cfg, bounds, blocked, bridgeable, diagnostics, path, sparse, seen)
  local current = { x = from_pos.x, y = from_pos.y }
  local primary, secondary = choose_axes(cfg.path_order, segment_index)
  local guard = 0
  while current.x ~= to_pos.x or current.y ~= to_pos.y do
    guard = guard + 1
    if guard > cfg.max_cells then
      diagnostics[#diagnostics + 1] = diag("error", "path_carver.max_cells_exceeded", "Path carving stopped because max_cells was exceeded.", "path")
      return false
    end

    local moved = step_axis(current, to_pos, primary)
    if not moved then
      moved = step_axis(current, to_pos, secondary)
    end
    if not moved then
      return true
    end

    if not in_bounds(current, bounds) then
      diagnostics[#diagnostics + 1] = diag("error", "path_carver.out_of_bounds", "Carved path moved outside configured bounds.", key(current.x, current.y))
      return false
    end

    local k = key(current.x, current.y)
    local is_blocked = blocked[k] ~= nil
    local can_bridge = cfg.allow_bridges and bridgeable[k] ~= nil
    if is_blocked and not can_bridge then
      diagnostics[#diagnostics + 1] = diag("error", "path_carver.blocked_cell", "Path is blocked by an unbridgeable cell.", k)
      return false
    end

    local tile = cfg.road_tile
    local role = "road"
    if is_blocked and can_bridge then
      tile = cfg.bridge_tile
      role = "bridge"
    end
    push_cell(path, sparse, seen, current.x, current.y, tile, role, true)
  end
  return true
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = config or {}
  if type(cfg) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if cfg.road_tile ~= nil and type(cfg.road_tile) ~= "string" then
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.road_tile_invalid", "road_tile must be a string when provided.", "config.road_tile")
  end
  if cfg.bridge_tile ~= nil and type(cfg.bridge_tile) ~= "string" then
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.bridge_tile_invalid", "bridge_tile must be a string when provided.", "config.bridge_tile")
  end
  if cfg.path_order ~= nil and cfg.path_order ~= "horizontal_first" and cfg.path_order ~= "vertical_first" and cfg.path_order ~= "alternating" then
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.path_order_invalid", "path_order must be horizontal_first, vertical_first or alternating.", "config.path_order")
  end
  if cfg.max_cells ~= nil and (not is_int(cfg.max_cells) or cfg.max_cells <= 0) then
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.max_cells_invalid", "max_cells must be a positive integer when provided.", "config.max_cells")
  end
  return #diagnostics == 0, diagnostics
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
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if not valid_bounds(input.bounds) then
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.bounds_invalid", "bounds must contain positive integer width and height.", "input.bounds")
  end
  if not valid_pos(input.start) then
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.start_invalid", "start must contain integer x and y.", "input.start")
  end
  local objective = input.objective or input.goal
  if not valid_pos(objective) then
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.objective_invalid", "objective must contain integer x and y.", "input.objective")
  end
  if #diagnostics > 0 then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if not in_bounds(input.start, input.bounds) then
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.start_out_of_bounds", "start is outside bounds.", "input.start")
  end
  if not in_bounds(objective, input.bounds) then
    diagnostics[#diagnostics + 1] = diag("error", "path_carver.objective_out_of_bounds", "objective is outside bounds.", "input.objective")
  end
  if #diagnostics > 0 then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local cfg = {
    road_tile = config.road_tile or "tile/road/dirt",
    bridge_tile = config.bridge_tile or "tile/bridge/wood",
    path_order = config.path_order or "horizontal_first",
    allow_bridges = config.allow_bridges ~= false,
    max_cells = config.max_cells or (input.bounds.width * input.bounds.height)
  }

  local points = { input.start }
  if type(input.waypoints) == "table" then
    for i = 1, #input.waypoints do
      if valid_pos(input.waypoints[i]) and in_bounds(input.waypoints[i], input.bounds) then
        points[#points + 1] = input.waypoints[i]
      else
        diagnostics[#diagnostics + 1] = diag("warning", "path_carver.waypoint_ignored", "Invalid or out-of-bounds waypoint was ignored.", "input.waypoints[" .. tostring(i) .. "]")
      end
    end
  end
  points[#points + 1] = objective

  local blocked = index_cells(input.blocked_cells)
  local bridgeable = index_cells(input.bridge_cells)
  local path = {}
  local sparse = {}
  local seen = {}
  push_cell(path, sparse, seen, input.start.x, input.start.y, cfg.road_tile, "start", true)

  local connected = true
  for i = 1, #points - 1 do
    if not carve_segment(points[i], points[i + 1], i, cfg, input.bounds, blocked, bridgeable, diagnostics, path, sparse, seen) then
      connected = false
      break
    end
  end
  if connected then
    push_cell(path, sparse, seen, objective.x, objective.y, cfg.road_tile, "objective", true)
  end

  local data = {
    bounds = { width = input.bounds.width, height = input.bounds.height },
    start = { x = input.start.x, y = input.start.y },
    objective = { x = objective.x, y = objective.y },
    path = path,
    sparse_tiles = sparse,
    blocked = not connected,
    summary = {
      path_cell_count = #path,
      sparse_tile_count = #sparse,
      connected = connected,
      generator = M.manifest.id
    }
  }
  return { ok = connected, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
