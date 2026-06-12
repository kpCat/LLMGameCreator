local M = {}

M.manifest = {
  id = "core/coordinates/v1",
  version = "0.1.0",
  category = "core",
  title = "2D coordinate helpers",
  purpose = "Handle 0-based 2D positions, chunk/local coordinates, facing directions, adjacency, and target disambiguation.",
  capabilities = {
    "core.coordinates.position2d",
    "core.coordinates.chunk_local",
    "core.coordinates.facing",
    "core.coordinates.adjacency",
    "core.coordinates.target_disambiguation"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      default_radius = { type = "integer", min = 0, max = 128 },
      default_adjacency_mode = { type = "string", enum = { "same_cell", "cardinal_adjacent", "diagonal_adjacent", "radius", "facing_cell" } }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  unsafe_features = {}
}

local FACING = {
  north = { x = 0, y = -1 },
  south = { x = 0, y = 1 },
  east = { x = 1, y = 0 },
  west = { x = -1, y = 0 },
  up = { x = 0, y = -1 },
  down = { x = 0, y = 1 },
  right = { x = 1, y = 0 },
  left = { x = -1, y = 0 }
}

local FACING_NORMAL = {
  north = "north",
  south = "south",
  east = "east",
  west = "west",
  up = "north",
  down = "south",
  right = "east",
  left = "west"
}

local function make_diagnostic(code, message, target)
  local diagnostic = {
    severity = "error",
    code = code,
    message = message
  }
  if target ~= nil then
    diagnostic.target = target
  end
  return diagnostic
end

local function make_warning(code, message, target)
  local diagnostic = make_diagnostic(code, message, target)
  diagnostic.severity = "warning"
  return diagnostic
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

local function abs(value)
  if value < 0 then
    return -value
  end
  return value
end

local function position_from(value)
  if type(value) ~= "table" then
    return nil
  end
  if is_integer(value.x) and is_integer(value.y) then
    return { x = value.x, y = value.y }
  end
  if type(value.position) == "table" and is_integer(value.position.x) and is_integer(value.position.y) then
    return { x = value.position.x, y = value.position.y }
  end
  return nil
end

local function target_id_from(target, index)
  if type(target) == "table" and type(target.id) == "string" then
    return target.id
  end
  return "target_" .. tostring(index)
end

local function position_error(target)
  return make_diagnostic("core.coordinates.invalid_position", "Position must be a table with integer x and y fields.", target)
end

local function chunk_size_from(value)
  if is_integer(value) and value > 0 then
    return { width = value, height = value }
  end
  if type(value) == "table" and is_integer(value.width) and is_integer(value.height) and value.width > 0 and value.height > 0 then
    return { width = value.width, height = value.height }
  end
  return nil
end

local function manhattan(a, b)
  return abs(a.x - b.x) + abs(a.y - b.y)
end

local function euclidean_squared(a, b)
  local dx = a.x - b.x
  local dy = a.y - b.y
  return dx * dx + dy * dy
end

local function candidate_sort(left, right)
  if left.distance_manhattan ~= right.distance_manhattan then
    return left.distance_manhattan < right.distance_manhattan
  end
  if left.target_id ~= right.target_id then
    return left.target_id < right.target_id
  end
  return left.index < right.index
end

function M.position2d(x, y)
  return { x = x, y = y }
end

function M.validate_position2d(position, target)
  if position_from(position) == nil then
    return false, { position_error(target or "position") }
  end
  return true, {}
end

function M.validate_chunk_coord(coord, target)
  if position_from(coord) == nil then
    return false, { make_diagnostic("core.coordinates.invalid_chunk_coord", "Chunk coord must contain integer x and y fields.", target or "chunk") }
  end
  return true, {}
end

function M.validate_local_coord(coord, chunk_size, target)
  local pos = position_from(coord)
  if pos == nil then
    return false, { make_diagnostic("core.coordinates.invalid_local_coord", "Local coord must contain integer x and y fields.", target or "local") }
  end
  if pos.x < 0 or pos.y < 0 then
    return false, { make_diagnostic("core.coordinates.local_negative", "Local coord must be 0-based and non-negative.", target or "local") }
  end
  local size = chunk_size_from(chunk_size)
  if size ~= nil and (pos.x >= size.width or pos.y >= size.height) then
    return false, { make_diagnostic("core.coordinates.local_out_of_chunk", "Local coord is outside the chunk size.", target or "local") }
  end
  return true, {}
end

function M.world_to_chunk_local(position, chunk_size)
  local diagnostics = {}
  local pos = position_from(position)
  if pos == nil then
    diagnostics[#diagnostics + 1] = position_error("position")
  end
  local size = chunk_size_from(chunk_size)
  if size == nil then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.coordinates.invalid_chunk_size", "chunk_size must be a positive integer or a table with positive integer width and height.", "chunk_size")
  end
  if diagnostics[1] ~= nil then
    return result(false, {}, diagnostics)
  end

  local chunk_x = pos.x // size.width
  local chunk_y = pos.y // size.height
  local local_x = pos.x - (chunk_x * size.width)
  local local_y = pos.y - (chunk_y * size.height)

  return result(true, {
    chunk = { x = chunk_x, y = chunk_y },
    local_position = { x = local_x, y = local_y }
  }, {})
end

function M.chunk_local_to_world(chunk_coord, local_coord, chunk_size)
  local diagnostics = {}
  local chunk = position_from(chunk_coord)
  if chunk == nil then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.coordinates.invalid_chunk_coord", "Chunk coord must contain integer x and y fields.", "chunk")
  end
  local size = chunk_size_from(chunk_size)
  if size == nil then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.coordinates.invalid_chunk_size", "chunk_size must be a positive integer or a table with positive integer width and height.", "chunk_size")
  end
  local local_ok, local_diagnostics = M.validate_local_coord(local_coord, size, "local")
  if not local_ok then
    for index = 1, #local_diagnostics do
      diagnostics[#diagnostics + 1] = local_diagnostics[index]
    end
  end
  if diagnostics[1] ~= nil then
    return result(false, {}, diagnostics)
  end

  local local_pos = position_from(local_coord)
  return result(true, {
    position = {
      x = (chunk.x * size.width) + local_pos.x,
      y = (chunk.y * size.height) + local_pos.y
    }
  }, {})
end

function M.normalize_facing(facing)
  if type(facing) ~= "string" then
    return nil
  end
  return FACING_NORMAL[string.lower(facing)]
end

function M.facing_vector(facing)
  local normalized = M.normalize_facing(facing)
  if normalized == nil then
    return result(false, {}, { make_diagnostic("core.coordinates.invalid_facing", "Facing must be north, south, east, west, or an accepted alias.", "facing") })
  end
  local vector = FACING[normalized]
  return result(true, { facing = normalized, vector = { x = vector.x, y = vector.y } }, {})
end

function M.target_cell_in_front(actor, distance)
  local diagnostics = {}
  local position = position_from(actor)
  if position == nil then
    diagnostics[#diagnostics + 1] = position_error("actor.position")
  end

  local facing = nil
  if type(actor) == "table" then
    facing = actor.facing
  end
  local vector_result = M.facing_vector(facing)
  if vector_result.ok ~= true then
    diagnostics[#diagnostics + 1] = vector_result.diagnostics[1]
  end

  local resolved_distance = distance
  if resolved_distance == nil then
    resolved_distance = 1
  end
  if not is_integer(resolved_distance) or resolved_distance < 1 then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.coordinates.invalid_distance", "distance must be a positive integer.", "distance")
  end

  if diagnostics[1] ~= nil then
    return result(false, {}, diagnostics)
  end

  local vector = vector_result.data.vector
  return result(true, {
    position = {
      x = position.x + (vector.x * resolved_distance),
      y = position.y + (vector.y * resolved_distance)
    },
    facing = vector_result.data.facing
  }, {})
end

function M.classify_adjacency(left, right, radius)
  local a = position_from(left)
  local b = position_from(right)
  if a == nil or b == nil then
    return "none"
  end

  local dx = abs(a.x - b.x)
  local dy = abs(a.y - b.y)
  if dx == 0 and dy == 0 then
    return "same_cell"
  end
  if (dx == 1 and dy == 0) or (dx == 0 and dy == 1) then
    return "cardinal_adjacent"
  end
  if dx == 1 and dy == 1 then
    return "diagonal_adjacent"
  end
  if is_integer(radius) and radius >= 0 and euclidean_squared(a, b) <= radius * radius then
    return "radius"
  end
  return "none"
end

function M.matches_adjacency(left, right, mode, radius)
  local resolved_mode = mode or "same_cell"
  local classified = M.classify_adjacency(left, right, radius)
  if resolved_mode == "same_cell" then
    return classified == "same_cell"
  end
  if resolved_mode == "cardinal_adjacent" then
    return classified == "cardinal_adjacent"
  end
  if resolved_mode == "diagonal_adjacent" then
    return classified == "diagonal_adjacent"
  end
  if resolved_mode == "radius" then
    local a = position_from(left)
    local b = position_from(right)
    local resolved_radius = is_integer(radius) and radius or 1
    return a ~= nil and b ~= nil and resolved_radius >= 0 and euclidean_squared(a, b) <= resolved_radius * resolved_radius
  end
  return false
end

function M.filter_targets(actor, targets, options)
  local diagnostics = {}
  local actor_position = position_from(actor)
  if actor_position == nil then
    diagnostics[#diagnostics + 1] = position_error("actor.position")
  end
  if type(targets) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.coordinates.targets_not_table", "targets must be an array table.", "targets")
  end
  if diagnostics[1] ~= nil then
    return result(false, { candidates = {} }, diagnostics)
  end

  local resolved_options = type(options) == "table" and options or {}
  local mode = resolved_options.mode or "same_cell"
  local radius = resolved_options.radius
  if mode == "radius" and radius == nil then
    radius = 1
  end

  local match_position = actor_position
  if mode == "facing_cell" then
    local actor_for_facing = actor
    if type(actor_for_facing) ~= "table" then
      actor_for_facing = { position = actor_position, facing = resolved_options.facing }
    elseif actor_for_facing.facing == nil and resolved_options.facing ~= nil then
      actor_for_facing = { position = actor_position, facing = resolved_options.facing }
    end
    local front_result = M.target_cell_in_front(actor_for_facing, resolved_options.distance or 1)
    if front_result.ok ~= true then
      return result(false, { candidates = {} }, front_result.diagnostics)
    end
    match_position = front_result.data.position
    mode = "same_cell"
  end

  local candidates = {}
  for index = 1, #targets do
    local target = targets[index]
    local target_position = position_from(target)
    if target_position ~= nil and M.matches_adjacency(match_position, target_position, mode, radius) then
      candidates[#candidates + 1] = {
        index = index,
        target_id = target_id_from(target, index),
        position = target_position,
        adjacency = M.classify_adjacency(actor_position, target_position, radius),
        distance_manhattan = manhattan(actor_position, target_position),
        distance_squared = euclidean_squared(actor_position, target_position)
      }
    end
  end

  table.sort(candidates, candidate_sort)
  return result(true, { candidates = candidates }, {})
end

function M.disambiguate_targets(actor, targets, options)
  local filter_result = M.filter_targets(actor, targets, options)
  if filter_result.ok ~= true then
    return filter_result
  end

  local candidates = filter_result.data.candidates
  local resolved_options = type(options) == "table" and options or {}
  if #candidates == 0 then
    return result(false, { candidates = candidates, ambiguous = false }, { make_diagnostic("core.coordinates.no_target", "No target matched the requested targeting mode.", "targets") })
  end

  if type(resolved_options.target_id) == "string" then
    for index = 1, #candidates do
      if candidates[index].target_id == resolved_options.target_id then
        return result(true, { selected = candidates[index], candidates = candidates, ambiguous = false }, {})
      end
    end
    return result(false, { candidates = candidates, ambiguous = false }, { make_diagnostic("core.coordinates.target_id_not_found", "target_id did not match any candidate.", "options.target_id") })
  end

  if is_integer(resolved_options.target_index) then
    for index = 1, #candidates do
      if candidates[index].index == resolved_options.target_index then
        return result(true, { selected = candidates[index], candidates = candidates, ambiguous = false }, {})
      end
    end
    return result(false, { candidates = candidates, ambiguous = false }, { make_diagnostic("core.coordinates.target_index_not_found", "target_index did not match any candidate.", "options.target_index") })
  end

  if #candidates == 1 then
    return result(true, { selected = candidates[1], candidates = candidates, ambiguous = false }, {})
  end

  if resolved_options.prefer == "first" then
    return result(true, { selected = candidates[1], candidates = candidates, ambiguous = true }, { make_warning("core.coordinates.ambiguous_first_selected", "Multiple targets matched; first deterministic candidate was selected.", "targets") })
  end

  if resolved_options.prefer == "nearest" then
    if candidates[2] ~= nil and candidates[1].distance_manhattan == candidates[2].distance_manhattan then
      return result(false, { candidates = candidates, ambiguous = true }, { make_diagnostic("core.coordinates.ambiguous_nearest", "Multiple targets are equally near; provide target_id or target_index.", "targets") })
    end
    return result(true, { selected = candidates[1], candidates = candidates, ambiguous = false }, {})
  end

  return result(false, { candidates = candidates, ambiguous = true }, { make_diagnostic("core.coordinates.ambiguous_targets", "Multiple targets matched; provide target_id, target_index, prefer=first, or prefer=nearest.", "targets") })
end

function M.validate_config(config)
  local diagnostics = {}

  if config == nil then
    return true, diagnostics
  end

  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.coordinates.config_not_table", "Coordinates config must be a table.", "config")
    return false, diagnostics
  end

  if config.default_radius ~= nil and (not is_integer(config.default_radius) or config.default_radius < 0 or config.default_radius > 128) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.coordinates.default_radius_invalid", "default_radius must be an integer from 0 to 128 when provided.", "config.default_radius")
  end

  if config.default_adjacency_mode ~= nil then
    local mode = config.default_adjacency_mode
    local valid = mode == "same_cell" or mode == "cardinal_adjacent" or mode == "diagonal_adjacent" or mode == "radius" or mode == "facing_cell"
    if not valid then
      diagnostics[#diagnostics + 1] = make_diagnostic("core.coordinates.default_mode_invalid", "default_adjacency_mode is not supported.", "config.default_adjacency_mode")
    end
  end

  return diagnostics[1] == nil, diagnostics
end

return M
