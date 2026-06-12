local M = {}

M.manifest = {
  id = "world/reachability/v1",
  version = "0.1.0",
  category = "world",
  title = "Reachability Validator",
  purpose = "Checks whether objective cells are reachable from a start cell on a compact 2D grid.",
  capabilities = { "world.reachability.validate", "world.path.diagnostics" },
  input_schema = {
    type = "table",
    fields = {
      bounds = "{ width:number, height:number }",
      start = "{ x:number, y:number }",
      objectives = "array of positions or { id, x, y }",
      blocked_cells = "optional array of positions",
      passable_cells = "optional array of positions",
      sparse_tiles = "optional array with walkable metadata"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      reachable = "boolean",
      reachable_objectives = "array",
      unreachable_objectives = "array",
      visited_count = "number",
      diagnostics_summary = "small metadata object"
    }
  },
  config_schema = {
    adjacency = "cardinal | diagonal",
    default_walkable = "optional boolean",
    max_visited = "optional positive number"
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

local function in_bounds(x, y, bounds)
  return x >= 0 and y >= 0 and x < bounds.width and y < bounds.height
end

local function key(x, y)
  return tostring(x) .. ":" .. tostring(y)
end

local function mark_cells(map, cells, value)
  if type(cells) ~= "table" then
    return
  end
  for i = 1, #cells do
    local c = cells[i]
    if valid_pos(c) then
      map[key(c.x, c.y)] = value
    end
  end
end

local function build_walkability(input, default_walkable)
  local walkable = {}
  if type(input.sparse_tiles) == "table" then
    for i = 1, #input.sparse_tiles do
      local t = input.sparse_tiles[i]
      if valid_pos(t) and type(t.walkable) == "boolean" then
        walkable[key(t.x, t.y)] = t.walkable
      end
    end
  end
  mark_cells(walkable, input.blocked_cells, false)
  mark_cells(walkable, input.passable_cells, true)
  if type(input.gates) == "table" then
    mark_cells(walkable, input.gates, true)
  end
  if type(input.bridges) == "table" then
    mark_cells(walkable, input.bridges, true)
  end
  return walkable, default_walkable ~= false
end

local function is_walkable(x, y, walkable, default_walkable)
  local v = walkable[key(x, y)]
  if v == nil then
    return default_walkable
  end
  return v == true
end

local function neighbors(adjacency)
  if adjacency == "diagonal" then
    return {
      { x = 1, y = 0 }, { x = -1, y = 0 }, { x = 0, y = 1 }, { x = 0, y = -1 },
      { x = 1, y = 1 }, { x = 1, y = -1 }, { x = -1, y = 1 }, { x = -1, y = -1 }
    }
  end
  return { { x = 1, y = 0 }, { x = -1, y = 0 }, { x = 0, y = 1 }, { x = 0, y = -1 } }
end

local function flood(input, cfg, diagnostics)
  local walkable, default_walkable = build_walkability(input, cfg.default_walkable)
  local offsets = neighbors(cfg.adjacency)
  local visited = {}
  local queue = { { x = input.start.x, y = input.start.y } }
  local head = 1
  visited[key(input.start.x, input.start.y)] = true

  while head <= #queue do
    local current = queue[head]
    head = head + 1
    if #queue > cfg.max_visited then
      diagnostics[#diagnostics + 1] = diag("error", "reachability.max_visited_exceeded", "Reachability scan exceeded max_visited.", "config.max_visited")
      break
    end
    for i = 1, #offsets do
      local nx = current.x + offsets[i].x
      local ny = current.y + offsets[i].y
      local k = key(nx, ny)
      if in_bounds(nx, ny, input.bounds) and not visited[k] and is_walkable(nx, ny, walkable, default_walkable) then
        visited[k] = true
        queue[#queue + 1] = { x = nx, y = ny }
      end
    end
  end

  return visited, #queue
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = config or {}
  if type(cfg) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "reachability.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if cfg.adjacency ~= nil and cfg.adjacency ~= "cardinal" and cfg.adjacency ~= "diagonal" then
    diagnostics[#diagnostics + 1] = diag("error", "reachability.adjacency_invalid", "adjacency must be cardinal or diagonal.", "config.adjacency")
  end
  if cfg.default_walkable ~= nil and type(cfg.default_walkable) ~= "boolean" then
    diagnostics[#diagnostics + 1] = diag("error", "reachability.default_walkable_invalid", "default_walkable must be boolean when provided.", "config.default_walkable")
  end
  if cfg.max_visited ~= nil and (not is_int(cfg.max_visited) or cfg.max_visited <= 0) then
    diagnostics[#diagnostics + 1] = diag("error", "reachability.max_visited_invalid", "max_visited must be a positive integer.", "config.max_visited")
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
    diagnostics[#diagnostics + 1] = diag("error", "reachability.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if not valid_bounds(input.bounds) then
    diagnostics[#diagnostics + 1] = diag("error", "reachability.bounds_invalid", "bounds must contain positive integer width and height.", "input.bounds")
  end
  if not valid_pos(input.start) then
    diagnostics[#diagnostics + 1] = diag("error", "reachability.start_invalid", "start must contain integer x and y.", "input.start")
  end
  if type(input.objectives) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "reachability.objectives_invalid", "objectives must be an array.", "input.objectives")
  end
  if #diagnostics > 0 then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if not in_bounds(input.start.x, input.start.y, input.bounds) then
    diagnostics[#diagnostics + 1] = diag("error", "reachability.start_out_of_bounds", "start is outside bounds.", "input.start")
  end
  for i = 1, #input.objectives do
    local objective = input.objectives[i]
    if not valid_pos(objective) then
      diagnostics[#diagnostics + 1] = diag("error", "reachability.objective_invalid", "Objective must contain integer x and y.", "input.objectives[" .. tostring(i) .. "]")
    elseif not in_bounds(objective.x, objective.y, input.bounds) then
      diagnostics[#diagnostics + 1] = diag("error", "reachability.objective_out_of_bounds", "Objective is outside bounds.", objective.id or ("objective_" .. tostring(i)))
    end
  end
  if #diagnostics > 0 then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local cfg = {
    adjacency = config.adjacency or "cardinal",
    default_walkable = config.default_walkable ~= false,
    max_visited = config.max_visited or (input.bounds.width * input.bounds.height)
  }
  local visited, visited_count = flood(input, cfg, diagnostics)
  local reachable_objectives = {}
  local unreachable_objectives = {}
  for i = 1, #input.objectives do
    local objective = input.objectives[i]
    local item = { id = objective.id or ("objective_" .. tostring(i)), x = objective.x, y = objective.y }
    if visited[key(objective.x, objective.y)] then
      reachable_objectives[#reachable_objectives + 1] = item
    else
      unreachable_objectives[#unreachable_objectives + 1] = item
      diagnostics[#diagnostics + 1] = diag("error", "reachability.objective_unreachable", "Objective is unreachable from start.", item.id)
    end
  end

  local all_reachable = #unreachable_objectives == 0
  local data = {
    bounds = { width = input.bounds.width, height = input.bounds.height },
    start = { x = input.start.x, y = input.start.y },
    reachable = all_reachable,
    reachable_objectives = reachable_objectives,
    unreachable_objectives = unreachable_objectives,
    visited_count = visited_count,
    diagnostics_summary = {
      objective_count = #input.objectives,
      unreachable_count = #unreachable_objectives,
      adjacency = cfg.adjacency,
      generator = M.manifest.id
    }
  }
  return { ok = all_reachable and #diagnostics == 0, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
