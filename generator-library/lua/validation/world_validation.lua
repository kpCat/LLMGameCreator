local M = {}

M.manifest = {
  id = "validation/world_validation/v1",
  version = "0.1.0",
  category = "validation",
  title = "World validation",
  purpose = "Validate compact world, map, region, chunk, road, gate, bridge and reachability IR without executing runtime code.",
  capabilities = {
    "validation.world.validate",
    "validation.world.reachability",
    "validation.world.references"
  },
  input_schema = {
    type = "object",
    required = { "world" }
  },
  output_schema = {
    type = "object",
    fields = { "summary", "reachable_objectives", "unreachable_objectives" }
  },
  config_schema = {
    type = "object",
    fields = { "max_nodes" }
  },
  deterministic = true,
  runtime_targets = { "editor", "validation", "simulation", "unity_ir", "codegen_ir" },
  unsafe_features = {}
}

local function diagnostic(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_array(value)
  if type(value) ~= "table" then
    return false
  end
  local count = 0
  for key, _ in pairs(value) do
    if type(key) ~= "number" or key < 1 or key % 1 ~= 0 then
      return false
    end
    count = count + 1
  end
  for index = 1, count do
    if value[index] == nil then
      return false
    end
  end
  return true
end

local function is_id(value)
  if type(value) ~= "string" or value == "" then
    return false
  end
  if value:sub(1, 1) == "/" or value:sub(-1) == "/" then
    return false
  end
  if value:find("//", 1, true) then
    return false
  end
  return value:match("^[a-z0-9_][a-z0-9_/%-]*$") ~= nil
end

local function add_id_index(items, label, diagnostics)
  local index = {}
  if items == nil then
    return index
  end
  if not is_array(items) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_" .. label .. "_list", label .. " must be an array.", label)
    return index
  end
  for i = 1, #items do
    local item = items[i]
    if type(item) ~= "table" or not is_id(item.id) then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_" .. label .. "_id", label .. " entry has invalid id.", label .. "[" .. tostring(i) .. "]")
    elseif index[item.id] then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.duplicate_" .. label .. "_id", label .. " id is duplicated.", item.id)
    else
      index[item.id] = item
    end
  end
  return index
end

local function has_value(index, id)
  return type(index) == "table" and type(id) == "string" and index[id] ~= nil
end

local function build_graph(world, diagnostics, max_nodes)
  local graph = world.graph or {}
  local nodes = graph.nodes or world.nodes or {}
  local edges = graph.edges or world.edges or {}
  local node_index = {}
  local adjacency = {}

  if not is_array(nodes) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_nodes", "Graph nodes must be an array.", "world.graph.nodes")
    nodes = {}
  end

  for i = 1, #nodes do
    local node = nodes[i]
    local id = type(node) == "table" and node.id or node
    if not is_id(id) then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_node_id", "Graph node id is invalid.", "world.graph.nodes[" .. tostring(i) .. "]")
    elseif node_index[id] then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.duplicate_node_id", "Graph node id is duplicated.", id)
    else
      node_index[id] = true
      adjacency[id] = {}
    end
  end

  local count = 0
  for _, _ in pairs(node_index) do
    count = count + 1
  end
  if count > max_nodes then
    diagnostics[#diagnostics + 1] = diagnostic("warning", "validation.world.large_graph", "Graph has more nodes than configured compact validation budget.", "world.graph.nodes")
  end

  if not is_array(edges) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_edges", "Graph edges must be an array.", "world.graph.edges")
    edges = {}
  end

  for i = 1, #edges do
    local edge = edges[i]
    if type(edge) ~= "table" then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_edge", "Graph edge must be an object.", "world.graph.edges[" .. tostring(i) .. "]")
    else
      local from_id = edge.from
      local to_id = edge.to
      if not has_value(node_index, from_id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.missing_edge_from", "Graph edge references missing source node.", "world.graph.edges[" .. tostring(i) .. "].from")
      end
      if not has_value(node_index, to_id) then
        diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.missing_edge_to", "Graph edge references missing target node.", "world.graph.edges[" .. tostring(i) .. "].to")
      end
      if edge.blocked == true then
        diagnostics[#diagnostics + 1] = diagnostic("warning", "validation.world.blocked_road", "Graph edge is explicitly blocked.", "world.graph.edges[" .. tostring(i) .. "]")
      elseif has_value(node_index, from_id) and has_value(node_index, to_id) then
        adjacency[from_id][#adjacency[from_id] + 1] = to_id
        if edge.one_way ~= true then
          adjacency[to_id][#adjacency[to_id] + 1] = from_id
        end
      end
    end
  end

  return node_index, adjacency
end

local function bfs(adjacency, start_id)
  local seen = {}
  local queue = {}
  local head = 1
  if type(start_id) ~= "string" or adjacency[start_id] == nil then
    return seen
  end
  queue[1] = start_id
  seen[start_id] = true
  while head <= #queue do
    local current = queue[head]
    head = head + 1
    local next_nodes = adjacency[current] or {}
    for i = 1, #next_nodes do
      local next_id = next_nodes[i]
      if not seen[next_id] then
        seen[next_id] = true
        queue[#queue + 1] = next_id
      end
    end
  end
  return seen
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.config_not_table", "Config must be a table.", "config")
  end
  if type(config) == "table" and config.max_nodes ~= nil then
    if type(config.max_nodes) ~= "number" or config.max_nodes < 1 or config.max_nodes % 1 ~= 0 then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_max_nodes", "max_nodes must be a positive integer.", "config.max_nodes")
    end
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = type(ctx) == "table" and type(ctx.config) == "table" and ctx.config or {}
  local ok_config, config_diagnostics = M.validate_config(config)
  for i = 1, #config_diagnostics do
    diagnostics[#diagnostics + 1] = config_diagnostics[i]
  end

  if type(input) ~= "table" or type(input.world) ~= "table" then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.input_missing_world", "Input must contain world table.", "input.world")
    return { ok = false, data = { summary = { checked = 0 } }, diagnostics = diagnostics, artifacts = {} }
  end

  local world = input.world
  if not is_id(world.id) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_world_id", "World id must use lowercase slash notation.", "world.id")
  end

  local regions = add_id_index(world.regions, "region", diagnostics)
  local chunks = add_id_index(world.chunks, "chunk", diagnostics)
  local landmarks = add_id_index(world.landmarks, "landmark", diagnostics)
  local gates = add_id_index(world.gates, "gate", diagnostics)
  local bridges = add_id_index(world.bridges, "bridge", diagnostics)

  if is_array(world.chunks or {}) then
    for i = 1, #(world.chunks or {}) do
      local chunk = world.chunks[i]
      if type(chunk) == "table" then
        if chunk.region_id ~= nil and not has_value(regions, chunk.region_id) then
          diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.missing_region", "Chunk references missing region.", "world.chunks[" .. tostring(i) .. "].region_id")
        end
        if chunk.walkability ~= nil and type(chunk.walkability) ~= "table" then
          diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_walkability", "Chunk walkability metadata must be a table.", "world.chunks[" .. tostring(i) .. "].walkability")
        end
      end
    end
  end

  local max_nodes = type(config.max_nodes) == "number" and config.max_nodes or 512
  local node_index, adjacency = build_graph(world, diagnostics, max_nodes)

  local edges = (world.graph and world.graph.edges) or world.edges or {}
  if is_array(edges) then
    for i = 1, #edges do
      local edge = edges[i]
      if type(edge) == "table" then
        if edge.gate_id ~= nil and not has_value(gates, edge.gate_id) then
          diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_gate", "Edge references missing gate.", "world.graph.edges[" .. tostring(i) .. "].gate_id")
        end
        if edge.bridge_id ~= nil and not has_value(bridges, edge.bridge_id) then
          diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.missing_bridge", "Edge references missing bridge.", "world.graph.edges[" .. tostring(i) .. "].bridge_id")
        end
      end
    end
  end

  local starts = input.starts or world.starts or {}
  local objectives = input.objectives or world.objectives or {}
  if not is_array(starts) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_starts", "Starts must be an array.", "input.starts")
    starts = {}
  end
  if not is_array(objectives) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.invalid_objectives", "Objectives must be an array.", "input.objectives")
    objectives = {}
  end

  local reachable_any = {}
  for i = 1, #starts do
    local start = starts[i]
    local node_id = type(start) == "table" and start.node_id or start
    if not has_value(node_index, node_id) then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.missing_start_node", "Start references missing graph node.", "starts[" .. tostring(i) .. "]")
    else
      local seen = bfs(adjacency, node_id)
      for id, value in pairs(seen) do
        if value then
          reachable_any[id] = true
        end
      end
    end
  end

  local reachable_objectives = {}
  local unreachable_objectives = {}
  for i = 1, #objectives do
    local objective = objectives[i]
    local objective_id = type(objective) == "table" and objective.id or "objective/" .. tostring(i)
    local node_id = type(objective) == "table" and objective.node_id or objective
    if not has_value(node_index, node_id) then
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.missing_objective_node", "Objective references missing graph node.", "objectives[" .. tostring(i) .. "]")
      unreachable_objectives[#unreachable_objectives + 1] = objective_id
    elseif reachable_any[node_id] then
      reachable_objectives[#reachable_objectives + 1] = objective_id
    else
      diagnostics[#diagnostics + 1] = diagnostic("error", "validation.world.unreachable_objective", "Objective is not reachable from configured starts.", objective_id)
      unreachable_objectives[#unreachable_objectives + 1] = objective_id
    end
  end

  local has_errors = false
  for i = 1, #diagnostics do
    if diagnostics[i].severity == "error" then
      has_errors = true
      break
    end
  end

  return {
    ok = ok_config and not has_errors,
    data = {
      summary = {
        world_id = world.id,
        region_count = #((is_array(world.regions or {}) and world.regions) or {}),
        chunk_count = #((is_array(world.chunks or {}) and world.chunks) or {}),
        objective_count = #objectives,
        unreachable_count = #unreachable_objectives
      },
      reachable_objectives = reachable_objectives,
      unreachable_objectives = unreachable_objectives
    },
    diagnostics = diagnostics,
    artifacts = {}
  }
end

return M
