local M = {}

M.manifest = {
  id = "core/grid/v1",
  version = "0.1.0",
  category = "core",
  title = "Sparse 2D grid helpers",
  purpose = "Represent finite or unbounded 0-based grids with default cells and sparse overrides.",
  capabilities = {
    "core.grid.create",
    "core.grid.bounds",
    "core.grid.get_set",
    "core.grid.sparse_overrides",
    "core.grid.neighborhood",
    "core.grid.facing_target"
  },
  input_schema = {},
  output_schema = {},
  config_schema = {
    type = "object",
    allow_unknown = false,
    properties = {
      width = { type = "integer", min = 1 },
      height = { type = "integer", min = 1 },
      unbounded = { type = "boolean" }
    }
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity3d", "simulation", "codegen_ir" },
  unsafe_features = {}
}

local CARDINAL = {
  { x = 0, y = -1, direction = "north" },
  { x = 1, y = 0, direction = "east" },
  { x = 0, y = 1, direction = "south" },
  { x = -1, y = 0, direction = "west" }
}

local DIAGONAL = {
  { x = -1, y = -1, direction = "north_west" },
  { x = 1, y = -1, direction = "north_east" },
  { x = 1, y = 1, direction = "south_east" },
  { x = -1, y = 1, direction = "south_west" }
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

local function copy_value(value, depth)
  local value_type = type(value)
  if value_type ~= "table" then
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

local function validate_json_like(value, diagnostics, target, depth)
  local value_type = type(value)
  if value_type == "nil" or value_type == "string" or value_type == "number" or value_type == "boolean" then
    return
  end
  if value_type ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.cell_not_json_like", "Cell value must be JSON-serializable.", target)
    return
  end
  if depth > 16 then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.cell_too_deep", "Cell value nesting is deeper than 16.", target)
    return
  end
  for key, item in pairs(value) do
    local key_type = type(key)
    if key_type ~= "string" and key_type ~= "number" then
      diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.cell_bad_key", "Cell table keys must be strings or numbers.", target)
    end
    validate_json_like(item, diagnostics, target, depth + 1)
  end
end

local function validate_grid_spec(config)
  local diagnostics = {}
  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.spec_not_table", "Grid spec must be a table.", "config")
    return false, diagnostics
  end

  local unbounded = config.unbounded == true
  if not unbounded then
    if not is_integer(config.width) or config.width < 1 then
      diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.width_invalid", "width must be a positive integer for a finite grid.", "config.width")
    end
    if not is_integer(config.height) or config.height < 1 then
      diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.height_invalid", "height must be a positive integer for a finite grid.", "config.height")
    end
  end

  if config.default_cell == nil then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.default_cell_missing", "default_cell must be provided for deterministic get_cell output.", "config.default_cell")
  else
    validate_json_like(config.default_cell, diagnostics, "config.default_cell", 0)
  end
  return diagnostics[1] == nil, diagnostics
end

local function is_grid(value)
  return type(value) == "table" and type(value.overrides) == "table" and (value.unbounded == true or (is_integer(value.width) and is_integer(value.height)))
end

local function position_key(position)
  return tostring(position.x) .. "," .. tostring(position.y)
end

local function normalized_facing(facing)
  if type(facing) ~= "string" then
    return nil
  end
  return FACING_NORMAL[string.lower(facing)]
end

local function append_diagnostics(target, source)
  for index = 1, #source do
    target[#target + 1] = source[index]
  end
end

function M.create(config)
  local ok, diagnostics = validate_grid_spec(config)
  if not ok then
    return result(false, {}, diagnostics)
  end

  local grid = {
    width = config.width,
    height = config.height,
    unbounded = config.unbounded == true,
    default_cell = copy_value(config.default_cell, 0),
    overrides = {}
  }

  if grid.unbounded then
    grid.width = nil
    grid.height = nil
  end

  return result(true, { grid = grid }, {})
end

function M.key(position)
  local pos = position_from(position)
  if pos == nil then
    return nil
  end
  return position_key(pos)
end

function M.in_bounds(grid, position)
  local pos = position_from(position)
  if pos == nil or not is_grid(grid) then
    return false
  end
  if grid.unbounded == true then
    return true
  end
  return pos.x >= 0 and pos.y >= 0 and pos.x < grid.width and pos.y < grid.height
end

function M.validate_position_in_bounds(grid, position, target)
  if not is_grid(grid) then
    return false, { make_diagnostic("core.grid.invalid_grid", "grid must be created by core.grid.create or follow the same table shape.", "grid") }
  end
  if position_from(position) == nil then
    return false, { make_diagnostic("core.grid.invalid_position", "position must contain integer x and y fields.", target or "position") }
  end
  if not M.in_bounds(grid, position) then
    return false, { make_diagnostic("core.grid.out_of_bounds", "position is outside grid bounds.", target or "position") }
  end
  return true, {}
end

function M.get_cell(grid, position)
  local ok, diagnostics = M.validate_position_in_bounds(grid, position, "position")
  if not ok then
    return result(false, {}, diagnostics)
  end
  local pos = position_from(position)
  local key = position_key(pos)
  local cell = grid.default_cell
  local has_override = false
  if grid.overrides[key] ~= nil then
    cell = grid.overrides[key]
    has_override = true
  end
  return result(true, {
    position = pos,
    key = key,
    cell = copy_value(cell, 0),
    has_override = has_override
  }, {})
end

function M.set_cell(grid, position, cell)
  local diagnostics = {}
  local ok, bounds_diagnostics = M.validate_position_in_bounds(grid, position, "position")
  if not ok then
    append_diagnostics(diagnostics, bounds_diagnostics)
  end
  if cell == nil then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.cell_missing", "cell must be provided; use clear_cell to remove an override.", "cell")
  else
    validate_json_like(cell, diagnostics, "cell", 0)
  end
  if diagnostics[1] ~= nil then
    return result(false, {}, diagnostics)
  end

  local pos = position_from(position)
  local key = position_key(pos)
  grid.overrides[key] = copy_value(cell, 0)
  return result(true, { position = pos, key = key, cell = copy_value(cell, 0) }, {})
end

function M.clear_cell(grid, position)
  local ok, diagnostics = M.validate_position_in_bounds(grid, position, "position")
  if not ok then
    return result(false, {}, diagnostics)
  end
  local pos = position_from(position)
  local key = position_key(pos)
  grid.overrides[key] = nil
  return result(true, { position = pos, key = key }, {})
end

function M.apply_sparse_overrides(grid, overrides)
  local diagnostics = {}
  if type(overrides) ~= "table" then
    return result(false, {}, { make_diagnostic("core.grid.overrides_not_table", "overrides must be an array table.", "overrides") })
  end

  local applied = {}
  for index = 1, #overrides do
    local item = overrides[index]
    local position = position_from(item)
    if position == nil and type(item) == "table" then
      position = position_from(item.position)
    end
    local cell = type(item) == "table" and item.cell or nil
    local set_result = M.set_cell(grid, position, cell)
    if set_result.ok then
      applied[#applied + 1] = set_result.data
    else
      append_diagnostics(diagnostics, set_result.diagnostics)
    end
  end

  return result(diagnostics[1] == nil, { applied = applied }, diagnostics)
end

function M.target_cell_in_front(grid, actor, distance)
  local diagnostics = {}
  local position = position_from(actor)
  if position == nil then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.invalid_actor_position", "actor must contain position with integer x and y fields.", "actor.position")
  end
  local facing = type(actor) == "table" and actor.facing or nil
  local normal = normalized_facing(facing)
  if normal == nil then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.invalid_facing", "actor.facing must be north, south, east, west, or an accepted alias.", "actor.facing")
  end
  local resolved_distance = distance or 1
  if not is_integer(resolved_distance) or resolved_distance < 1 then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.invalid_distance", "distance must be a positive integer.", "distance")
  end
  if diagnostics[1] ~= nil then
    return result(false, {}, diagnostics)
  end

  local vector = FACING[normal]
  local target = {
    x = position.x + (vector.x * resolved_distance),
    y = position.y + (vector.y * resolved_distance)
  }
  return result(true, {
    position = target,
    facing = normal,
    in_bounds = M.in_bounds(grid, target)
  }, {})
end

function M.neighborhood(grid, position, options)
  local ok, diagnostics = M.validate_position_in_bounds(grid, position, "position")
  if not ok then
    return result(false, { cells = {} }, diagnostics)
  end

  local resolved_options = type(options) == "table" and options or {}
  local mode = resolved_options.mode or "cardinal"
  local include_center = resolved_options.include_center == true
  local include_out_of_bounds = resolved_options.include_out_of_bounds == true
  local deltas = {}

  if include_center then
    deltas[#deltas + 1] = { x = 0, y = 0, direction = "center" }
  end

  if mode == "cardinal" or mode == "all8" then
    for index = 1, #CARDINAL do
      deltas[#deltas + 1] = CARDINAL[index]
    end
  end
  if mode == "diagonal" or mode == "all8" then
    for index = 1, #DIAGONAL do
      deltas[#deltas + 1] = DIAGONAL[index]
    end
  end
  if mode == "radius" then
    local radius = resolved_options.radius or 1
    if not is_integer(radius) or radius < 0 or radius > 128 then
      return result(false, { cells = {} }, { make_diagnostic("core.grid.invalid_radius", "radius must be an integer from 0 to 128.", "options.radius") })
    end
    for y = -radius, radius do
      for x = -radius, radius do
        if include_center or x ~= 0 or y ~= 0 then
          if (x * x) + (y * y) <= radius * radius then
            deltas[#deltas + 1] = { x = x, y = y, direction = "radius" }
          end
        end
      end
    end
  end

  if #deltas == 0 and not include_center then
    return result(false, { cells = {} }, { make_diagnostic("core.grid.invalid_neighborhood_mode", "Neighborhood mode must be cardinal, diagonal, all8, or radius.", "options.mode") })
  end

  local origin = position_from(position)
  local cells = {}
  for index = 1, #deltas do
    local delta = deltas[index]
    local target = { x = origin.x + delta.x, y = origin.y + delta.y }
    local in_bounds = M.in_bounds(grid, target)
    if in_bounds or include_out_of_bounds then
      local cell = nil
      local has_override = false
      if in_bounds then
        local cell_result = M.get_cell(grid, target)
        if cell_result.ok then
          cell = cell_result.data.cell
          has_override = cell_result.data.has_override
        end
      end
      cells[#cells + 1] = {
        position = target,
        direction = delta.direction,
        in_bounds = in_bounds,
        has_override = has_override,
        distance_manhattan = abs(delta.x) + abs(delta.y),
        cell = cell
      }
    end
  end

  return result(true, { cells = cells }, {})
end

function M.validate_config(config)
  local diagnostics = {}

  if config == nil then
    return true, diagnostics
  end

  if type(config) ~= "table" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.config_not_table", "Grid config must be a table.", "config")
    return false, diagnostics
  end

  if config.width ~= nil and (not is_integer(config.width) or config.width < 1) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.config_width_invalid", "width must be a positive integer when provided.", "config.width")
  end
  if config.height ~= nil and (not is_integer(config.height) or config.height < 1) then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.config_height_invalid", "height must be a positive integer when provided.", "config.height")
  end
  if config.unbounded ~= nil and type(config.unbounded) ~= "boolean" then
    diagnostics[#diagnostics + 1] = make_diagnostic("core.grid.config_unbounded_invalid", "unbounded must be a boolean when provided.", "config.unbounded")
  end

  return diagnostics[1] == nil, diagnostics
end

return M
