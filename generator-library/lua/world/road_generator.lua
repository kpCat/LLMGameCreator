local M = {}

M.manifest = {
  id = "world/road_generator/v1",
  version = "0.1.0",
  category = "world",
  title = "Road Generator",
  purpose = "Builds deterministic compact road segments between named map nodes.",
  capabilities = { "world.road.generate", "world.path.ensure_connection" },
  input_schema = {
    type = "table",
    fields = {
      bounds = "{ width:number, height:number }",
      nodes = "array of { id:string, x:number, y:number }",
      roads = "array of { from:string, to:string, kind:string }",
      blocked_cells = "optional array of positions",
      bridge_cells = "optional array of positions"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      road_segments = "array of compact road paths",
      sparse_tiles = "tile overrides for roads and bridges",
      road_graph = "node/edge metadata",
      summary = "small metadata object"
    }
  },
  config_schema = {
    road_tile = "optional string",
    bridge_tile = "optional string",
    blocked_road_tile = "optional string",
    allow_bridges = "optional boolean",
    max_cells_per_road = "optional positive number"
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

local function valid_bounds(bounds)
  return type(bounds) == "table" and is_int(bounds.width) and is_int(bounds.height) and bounds.width > 0 and bounds.height > 0
end

local function valid_node(node)
  return type(node) == "table" and type(node.id) == "string" and is_int(node.x) and is_int(node.y)
end

local function in_bounds(x, y, bounds)
  return x >= 0 and y >= 0 and x < bounds.width and y < bounds.height
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
    if type(c) == "table" and is_int(c.x) and is_int(c.y) then
      map[key(c.x, c.y)] = c
    end
  end
  return map
end

local function index_nodes(nodes, diagnostics)
  local by_id = {}
  if type(nodes) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "road_generator.nodes_invalid", "nodes must be an array.", "input.nodes")
    return by_id
  end
  for i = 1, #nodes do
    local node = nodes[i]
    if valid_node(node) then
      if by_id[node.id] ~= nil then
        diagnostics[#diagnostics + 1] = diag("error", "road_generator.duplicate_node", "Duplicate road node id.", node.id)
      else
        by_id[node.id] = { id = node.id, x = node.x, y = node.y, tags = node.tags or {} }
      end
    else
      diagnostics[#diagnostics + 1] = diag("error", "road_generator.node_invalid", "Node must contain id, integer x and integer y.", "input.nodes[" .. tostring(i) .. "]")
    end
  end
  return by_id
end

local function add_sparse(sparse, x, y, tile, road_id, kind, walkable)
  sparse[#sparse + 1] = { x = x, y = y, tile = tile, walkable = walkable ~= false, layer = "road", road_id = road_id, kind = kind }
end

local function move_toward(current, target)
  if current.x ~= target.x then
    if current.x < target.x then
      current.x = current.x + 1
    else
      current.x = current.x - 1
    end
    return true
  end
  if current.y ~= target.y then
    if current.y < target.y then
      current.y = current.y + 1
    else
      current.y = current.y - 1
    end
    return true
  end
  return false
end

local function carve_road(road_id, from_node, to_node, kind, cfg, bounds, blocked, bridgeable, sparse, diagnostics)
  local cells = {}
  local current = { x = from_node.x, y = from_node.y }
  local guard = 0
  local blocked_at = nil
  cells[#cells + 1] = { x = current.x, y = current.y, role = "start", tile = cfg.road_tile }
  add_sparse(sparse, current.x, current.y, cfg.road_tile, road_id, kind, true)

  while current.x ~= to_node.x or current.y ~= to_node.y do
    guard = guard + 1
    if guard > cfg.max_cells_per_road then
      diagnostics[#diagnostics + 1] = diag("error", "road_generator.road_too_long", "Road generation exceeded max_cells_per_road.", road_id)
      blocked_at = { x = current.x, y = current.y, reason = "max_cells_per_road" }
      break
    end
    move_toward(current, to_node)
    if not in_bounds(current.x, current.y, bounds) then
      diagnostics[#diagnostics + 1] = diag("error", "road_generator.out_of_bounds", "Road moved outside map bounds.", road_id)
      blocked_at = { x = current.x, y = current.y, reason = "out_of_bounds" }
      break
    end

    local k = key(current.x, current.y)
    local is_blocked = blocked[k] ~= nil
    local can_bridge = cfg.allow_bridges and bridgeable[k] ~= nil
    local tile = cfg.road_tile
    local role = "road"
    local walkable = true
    if is_blocked and can_bridge then
      tile = cfg.bridge_tile
      role = "bridge"
    elseif is_blocked then
      tile = cfg.blocked_road_tile
      role = "blocked"
      walkable = false
      diagnostics[#diagnostics + 1] = diag("error", "road_generator.blocked_road", "Road is blocked by an unbridgeable barrier.", key(current.x, current.y))
      blocked_at = { x = current.x, y = current.y, reason = "blocked_cell" }
    end
    cells[#cells + 1] = { x = current.x, y = current.y, role = role, tile = tile, walkable = walkable }
    add_sparse(sparse, current.x, current.y, tile, road_id, kind, walkable)
    if blocked_at ~= nil then
      break
    end
  end

  return {
    id = road_id,
    from = from_node.id,
    to = to_node.id,
    kind = kind,
    cells = cells,
    blocked = blocked_at ~= nil,
    blocked_at = blocked_at
  }
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = config or {}
  if type(cfg) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "road_generator.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  local string_fields = { "road_tile", "bridge_tile", "blocked_road_tile" }
  for i = 1, #string_fields do
    local name = string_fields[i]
    if cfg[name] ~= nil and type(cfg[name]) ~= "string" then
      diagnostics[#diagnostics + 1] = diag("error", "road_generator." .. name .. "_invalid", name .. " must be a string when provided.", "config." .. name)
    end
  end
  if cfg.max_cells_per_road ~= nil and (not is_int(cfg.max_cells_per_road) or cfg.max_cells_per_road <= 0) then
    diagnostics[#diagnostics + 1] = diag("error", "road_generator.max_cells_invalid", "max_cells_per_road must be a positive integer.", "config.max_cells_per_road")
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
    diagnostics[#diagnostics + 1] = diag("error", "road_generator.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if not valid_bounds(input.bounds) then
    diagnostics[#diagnostics + 1] = diag("error", "road_generator.bounds_invalid", "bounds must contain positive integer width and height.", "input.bounds")
  end
  local nodes_by_id = index_nodes(input.nodes, diagnostics)
  if type(input.roads) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "road_generator.roads_invalid", "roads must be an array.", "input.roads")
  end
  if #diagnostics > 0 then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  for i = 1, #input.nodes do
    local node = input.nodes[i]
    if valid_node(node) and not in_bounds(node.x, node.y, input.bounds) then
      diagnostics[#diagnostics + 1] = diag("error", "road_generator.node_out_of_bounds", "Road node is outside bounds.", node.id)
    end
  end
  if #diagnostics > 0 then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local cfg = {
    road_tile = config.road_tile or "tile/road/dirt",
    bridge_tile = config.bridge_tile or "tile/bridge/wood",
    blocked_road_tile = config.blocked_road_tile or "tile/road/blocked",
    allow_bridges = config.allow_bridges ~= false,
    max_cells_per_road = config.max_cells_per_road or (input.bounds.width * input.bounds.height)
  }
  local blocked = index_cells(input.blocked_cells)
  local bridgeable = index_cells(input.bridge_cells)
  local sparse = {}
  local segments = {}
  local edges = {}
  local has_blocked = false

  for i = 1, #input.roads do
    local road = input.roads[i]
    if type(road) ~= "table" or type(road.from) ~= "string" or type(road.to) ~= "string" then
      diagnostics[#diagnostics + 1] = diag("error", "road_generator.road_invalid", "Road must contain from and to node ids.", "input.roads[" .. tostring(i) .. "]")
    else
      local from_node = nodes_by_id[road.from]
      local to_node = nodes_by_id[road.to]
      if from_node == nil or to_node == nil then
        diagnostics[#diagnostics + 1] = diag("error", "road_generator.road_endpoint_missing", "Road references a missing node.", "input.roads[" .. tostring(i) .. "]")
      else
        local road_id = road.id or (road.from .. "__" .. road.to)
        local kind = road.kind or "road"
        local segment = carve_road(road_id, from_node, to_node, kind, cfg, input.bounds, blocked, bridgeable, sparse, diagnostics)
        segments[#segments + 1] = segment
        edges[#edges + 1] = { id = road_id, from = road.from, to = road.to, kind = kind, blocked = segment.blocked }
        if segment.blocked then
          has_blocked = true
        end
      end
    end
  end

  local node_list = {}
  if type(input.nodes) == "table" then
    for i = 1, #input.nodes do
      local n = input.nodes[i]
      if valid_node(n) then
        node_list[#node_list + 1] = { id = n.id, x = n.x, y = n.y }
      end
    end
  end
  local data = {
    bounds = { width = input.bounds.width, height = input.bounds.height },
    road_segments = segments,
    sparse_tiles = sparse,
    road_graph = { nodes = node_list, edges = edges },
    summary = {
      road_count = #segments,
      sparse_tile_count = #sparse,
      blocked_road_count = has_blocked and 1 or 0,
      generator = M.manifest.id
    }
  }
  return { ok = #diagnostics == 0, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
