local M = {}

M.manifest = {
  id = "automation/conveyor_grid/v1",
  version = "0.1.0",
  category = "automation",
  title = "Conveyor grid",
  purpose = "Generate deterministic conveyor, splitter, inserter and logistics-link IR without running a live logistics system.",
  capabilities = {
    "automation.conveyor_grid.generate",
    "automation.logistics_abstraction",
    "automation.transport_link.ir"
  },
  input_schema = {
    nodes = "array",
    links = "array",
    grid = "object optional"
  },
  output_schema = {
    conveyor_grid = "object",
    logistics_graph = "object"
  },
  config_schema = {
    default_lane_capacity_per_second = "number optional"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "unity_tilemap", "simulation", "codegen_ir" },
  unsafe_features = {}
}

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function normalize_id(value, fallback)
  if type(value) == "string" and value ~= "" then
    return value
  end
  return fallback
end

local function copy_list(list)
  local result = {}
  if type(list) ~= "table" then
    return result
  end
  for index = 1, #list do
    result[index] = list[index]
  end
  return result
end

local function normalize_position(raw)
  raw = raw or {}
  return {
    x = tonumber(raw.x) or 0,
    y = tonumber(raw.y) or 0
  }
end

local function normalize_node(raw, index, diagnostics)
  if type(raw) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.node_not_table", "Logistics node must be a table.", "nodes[" .. index .. "]")
    return nil
  end

  local id = normalize_id(raw.id, "automation/logistics_node/generated_" .. index)
  local kind = normalize_id(raw.kind or raw.type, "belt")
  local allowed = {
    belt = true,
    splitter = true,
    merger = true,
    inserter = true,
    chest = true,
    machine_port = true,
    resource_port = true
  }
  if not allowed[kind] then
    diagnostics[#diagnostics + 1] = diag("warning", "automation.conveyor_grid.unknown_node_kind", "Unknown logistics node kind preserved as data.", id)
  end

  return {
    id = id,
    kind = kind,
    position = normalize_position(raw.position or raw),
    direction = normalize_id(raw.direction, "east"),
    item_filters = copy_list(raw.item_filters),
    tags = copy_list(raw.tags)
  }
end

local function normalize_link(raw, index, diagnostics, default_capacity)
  if type(raw) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.link_not_table", "Logistics link must be a table.", "links[" .. index .. "]")
    return nil
  end

  local id = normalize_id(raw.id, "automation/logistics_link/generated_" .. index)
  local from_id = normalize_id(raw.from_id or raw.from, "")
  local to_id = normalize_id(raw.to_id or raw.to, "")
  local capacity = tonumber(raw.capacity_per_second or raw.rate_per_second) or default_capacity

  if from_id == "" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.from_missing", "Link requires from_id.", id)
  end
  if to_id == "" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.to_missing", "Link requires to_id.", id)
  end
  if capacity <= 0 then
    diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.capacity_invalid", "Link capacity must be positive.", id)
    capacity = default_capacity
  end

  return {
    id = id,
    from_id = from_id,
    to_id = to_id,
    mode = normalize_id(raw.mode, "belt"),
    capacity_per_second = capacity,
    item_filters = copy_list(raw.item_filters),
    bidirectional = raw.bidirectional == true
  }
end

function M.validate_config(config)
  local diagnostics = {}
  if config == nil then
    return true, diagnostics
  end
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if config.default_lane_capacity_per_second ~= nil and (type(config.default_lane_capacity_per_second) ~= "number" or config.default_lane_capacity_per_second <= 0) then
    diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.default_capacity_invalid", "default_lane_capacity_per_second must be positive.", "config.default_lane_capacity_per_second")
  end
  return #diagnostics == 0, diagnostics
end

function M.generate(input, ctx)
  local diagnostics = {}
  local config = {}
  if type(ctx) == "table" and type(ctx.config) == "table" then
    config = ctx.config
  end

  local config_ok, config_diagnostics = M.validate_config(config)
  for index = 1, #config_diagnostics do
    diagnostics[#diagnostics + 1] = config_diagnostics[index]
  end

  if type(input) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local default_capacity = tonumber(config.default_lane_capacity_per_second) or 15
  local nodes = {}
  local node_ids = {}
  for index = 1, #(input.nodes or {}) do
    local node = normalize_node(input.nodes[index], index, diagnostics)
    if node ~= nil then
      if node_ids[node.id] then
        diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.duplicate_node_id", "Duplicate logistics node id.", node.id)
      end
      node_ids[node.id] = true
      nodes[#nodes + 1] = node
    end
  end

  table.sort(nodes, function(left, right)
    return left.id < right.id
  end)

  local links = {}
  local adjacency = {}
  for index = 1, #(input.links or {}) do
    local link = normalize_link(input.links[index], index, diagnostics, default_capacity)
    if link ~= nil then
      if not node_ids[link.from_id] then
        diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.from_unknown", "Link from_id does not match a node.", link.id)
      end
      if not node_ids[link.to_id] then
        diagnostics[#diagnostics + 1] = diag("error", "automation.conveyor_grid.to_unknown", "Link to_id does not match a node.", link.id)
      end
      adjacency[link.from_id] = adjacency[link.from_id] or {}
      adjacency[link.from_id][#adjacency[link.from_id] + 1] = {
        to_id = link.to_id,
        link_id = link.id,
        capacity_per_second = link.capacity_per_second
      }
      if link.bidirectional then
        adjacency[link.to_id] = adjacency[link.to_id] or {}
        adjacency[link.to_id][#adjacency[link.to_id] + 1] = {
          to_id = link.from_id,
          link_id = link.id,
          capacity_per_second = link.capacity_per_second
        }
      end
      links[#links + 1] = link
    end
  end

  table.sort(links, function(left, right)
    return left.id < right.id
  end)

  local adjacency_rows = {}
  for node_id, rows in pairs(adjacency) do
    table.sort(rows, function(left, right)
      if left.to_id == right.to_id then
        return left.link_id < right.link_id
      end
      return left.to_id < right.to_id
    end)
    adjacency_rows[#adjacency_rows + 1] = {
      node_id = node_id,
      outgoing = rows
    }
  end
  table.sort(adjacency_rows, function(left, right)
    return left.node_id < right.node_id
  end)

  local grid = input.grid or {}
  local data = {
    conveyor_grid = {
      coordinate_mode = normalize_id(grid.coordinate_mode, "tile"),
      width = tonumber(grid.width) or 0,
      height = tonumber(grid.height) or 0,
      nodes = nodes,
      links = links
    },
    logistics_graph = {
      adjacency = adjacency_rows
    },
    validation = {
      node_count = #nodes,
      link_count = #links,
      config_ok = config_ok
    }
  }

  return { ok = #diagnostics == 0, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
