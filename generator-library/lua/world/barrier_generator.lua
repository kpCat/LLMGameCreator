local M = {}

M.manifest = {
  id = "world/barrier_generator/v1",
  version = "0.1.0",
  category = "world",
  title = "Barrier Generator",
  purpose = "Creates compact barrier, gate and bridge tile overrides for 2D maps.",
  capabilities = { "world.barrier.generate", "world.gate.generate", "world.bridge.generate" },
  input_schema = {
    type = "table",
    fields = {
      bounds = "{ width:number, height:number }",
      barriers = "array of line/rect/perimeter barrier specs",
      gates = "optional array of passable positions",
      bridges = "optional array of passable bridge positions"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      sparse_tiles = "barrier, gate and bridge overrides",
      passability_overrides = "walkability metadata for validators/runtime",
      barriers = "normalized barrier summary"
    }
  },
  config_schema = {
    wall_tile = "optional string",
    gate_tile = "optional string",
    bridge_tile = "optional string",
    road_block_tile = "optional string",
    max_tiles = "optional positive number"
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

local function valid_pos(p)
  return type(p) == "table" and is_int(p.x) and is_int(p.y)
end

local function in_bounds(x, y, bounds)
  return x >= 0 and y >= 0 and x < bounds.width and y < bounds.height
end

local function key(x, y)
  return tostring(x) .. ":" .. tostring(y)
end

local function put_tile(state, x, y, tile, walkable, role, source_id)
  local k = key(x, y)
  if state.tiles[k] == nil then
    state.tile_order[#state.tile_order + 1] = k
  end
  if state.passability[k] == nil then
    state.passability_order[#state.passability_order + 1] = k
  end
  state.tiles[k] = { x = x, y = y, tile = tile, walkable = walkable, layer = "barrier", role = role, source_id = source_id }
  state.passability[k] = { x = x, y = y, walkable = walkable, reason = role, source_id = source_id }
end

local function emit_line(state, barrier, cfg, bounds, diagnostics)
  local from_pos = barrier.from or { x = barrier.x1, y = barrier.y1 }
  local to_pos = barrier.to or { x = barrier.x2, y = barrier.y2 }
  if not valid_pos(from_pos) or not valid_pos(to_pos) then
    diagnostics[#diagnostics + 1] = diag("error", "barrier_generator.line_invalid", "Line barrier needs from/to positions.", barrier.id or "barrier")
    return
  end
  local x = from_pos.x
  local y = from_pos.y
  local guard = 0
  while x ~= to_pos.x or y ~= to_pos.y do
    guard = guard + 1
    if guard > cfg.max_tiles then
      diagnostics[#diagnostics + 1] = diag("error", "barrier_generator.max_tiles_exceeded", "Barrier emission exceeded max_tiles.", barrier.id or "barrier")
      return
    end
    if in_bounds(x, y, bounds) then
      put_tile(state, x, y, barrier.tile_id or cfg.wall_tile, false, barrier.type or "barrier", barrier.id)
    else
      diagnostics[#diagnostics + 1] = diag("warning", "barrier_generator.tile_out_of_bounds", "Barrier tile outside bounds was skipped.", key(x, y))
    end
    if x ~= to_pos.x then
      if x < to_pos.x then x = x + 1 else x = x - 1 end
    elseif y ~= to_pos.y then
      if y < to_pos.y then y = y + 1 else y = y - 1 end
    end
  end
  if in_bounds(to_pos.x, to_pos.y, bounds) then
    put_tile(state, to_pos.x, to_pos.y, barrier.tile_id or cfg.wall_tile, false, barrier.type or "barrier", barrier.id)
  end
end

local function emit_rect(state, barrier, cfg, bounds, diagnostics)
  if not is_int(barrier.x) or not is_int(barrier.y) or not is_int(barrier.width) or not is_int(barrier.height) or barrier.width <= 0 or barrier.height <= 0 then
    diagnostics[#diagnostics + 1] = diag("error", "barrier_generator.rect_invalid", "Rect barrier needs x, y, width and height.", barrier.id or "barrier")
    return
  end
  local filled = barrier.filled == true
  for yy = barrier.y, barrier.y + barrier.height - 1 do
    for xx = barrier.x, barrier.x + barrier.width - 1 do
      local edge = xx == barrier.x or yy == barrier.y or xx == barrier.x + barrier.width - 1 or yy == barrier.y + barrier.height - 1
      if filled or edge then
        if in_bounds(xx, yy, bounds) then
          put_tile(state, xx, yy, barrier.tile_id or cfg.wall_tile, false, barrier.type or "barrier", barrier.id)
        else
          diagnostics[#diagnostics + 1] = diag("warning", "barrier_generator.tile_out_of_bounds", "Barrier tile outside bounds was skipped.", key(xx, yy))
        end
      end
    end
  end
end

local function emit_perimeter(state, barrier, cfg, bounds)
  local id = barrier.id or "perimeter"
  for x = 0, bounds.width - 1 do
    put_tile(state, x, 0, barrier.tile_id or cfg.wall_tile, false, barrier.type or "barrier", id)
    put_tile(state, x, bounds.height - 1, barrier.tile_id or cfg.wall_tile, false, barrier.type or "barrier", id)
  end
  for y = 0, bounds.height - 1 do
    put_tile(state, 0, y, barrier.tile_id or cfg.wall_tile, false, barrier.type or "barrier", id)
    put_tile(state, bounds.width - 1, y, barrier.tile_id or cfg.wall_tile, false, barrier.type or "barrier", id)
  end
end

local function emit_passable_list(state, items, cfg, bounds, diagnostics, role)
  if type(items) ~= "table" then
    return
  end
  for i = 1, #items do
    local item = items[i]
    if valid_pos(item) and in_bounds(item.x, item.y, bounds) then
      local tile = item.tile_id or cfg.gate_tile
      if role == "bridge" then
        tile = item.tile_id or cfg.bridge_tile
      end
      put_tile(state, item.x, item.y, tile, true, role, item.id or role)
    else
      diagnostics[#diagnostics + 1] = diag("warning", "barrier_generator.passable_invalid", "Invalid or out-of-bounds passable point was ignored.", role .. "[" .. tostring(i) .. "]")
    end
  end
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = config or {}
  if type(cfg) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "barrier_generator.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  local string_fields = { "wall_tile", "gate_tile", "bridge_tile", "road_block_tile" }
  for i = 1, #string_fields do
    local name = string_fields[i]
    if cfg[name] ~= nil and type(cfg[name]) ~= "string" then
      diagnostics[#diagnostics + 1] = diag("error", "barrier_generator." .. name .. "_invalid", name .. " must be a string when provided.", "config." .. name)
    end
  end
  if cfg.max_tiles ~= nil and (not is_int(cfg.max_tiles) or cfg.max_tiles <= 0) then
    diagnostics[#diagnostics + 1] = diag("error", "barrier_generator.max_tiles_invalid", "max_tiles must be a positive integer.", "config.max_tiles")
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
    diagnostics[#diagnostics + 1] = diag("error", "barrier_generator.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if not valid_bounds(input.bounds) then
    diagnostics[#diagnostics + 1] = diag("error", "barrier_generator.bounds_invalid", "bounds must contain positive integer width and height.", "input.bounds")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  local cfg = {
    wall_tile = config.wall_tile or "tile/barrier/wall",
    gate_tile = config.gate_tile or "tile/barrier/gate",
    bridge_tile = config.bridge_tile or "tile/bridge/wood",
    road_block_tile = config.road_block_tile or "tile/barrier/road_block",
    max_tiles = config.max_tiles or (input.bounds.width * input.bounds.height)
  }

  local state = { tiles = {}, passability = {}, tile_order = {}, passability_order = {} }
  local normalized = {}
  if type(input.barriers) == "table" then
    for i = 1, #input.barriers do
      local barrier = input.barriers[i]
      if type(barrier) == "table" then
        local shape = barrier.shape or "line"
        local original_tile_id = barrier.tile_id
        if barrier.type == "road_block" and original_tile_id == nil then
          barrier.tile_id = cfg.road_block_tile
        end
        if shape == "line" then
          emit_line(state, barrier, cfg, input.bounds, diagnostics)
        elseif shape == "rect" then
          emit_rect(state, barrier, cfg, input.bounds, diagnostics)
        elseif shape == "perimeter" then
          emit_perimeter(state, barrier, cfg, input.bounds)
        else
          diagnostics[#diagnostics + 1] = diag("error", "barrier_generator.shape_invalid", "Unsupported barrier shape.", "input.barriers[" .. tostring(i) .. "]")
        end
        barrier.tile_id = original_tile_id
        normalized[#normalized + 1] = { id = barrier.id or ("barrier_" .. tostring(i)), shape = shape, type = barrier.type or "barrier" }
      else
        diagnostics[#diagnostics + 1] = diag("error", "barrier_generator.barrier_invalid", "Barrier spec must be a table.", "input.barriers[" .. tostring(i) .. "]")
      end
    end
  end

  emit_passable_list(state, input.gates, cfg, input.bounds, diagnostics, "gate")
  emit_passable_list(state, input.bridges, cfg, input.bounds, diagnostics, "bridge")

  local sparse = {}
  local passability = {}
  for i = 1, #state.tile_order do
    sparse[#sparse + 1] = state.tiles[state.tile_order[i]]
  end
  for i = 1, #state.passability_order do
    passability[#passability + 1] = state.passability[state.passability_order[i]]
  end

  local data = {
    bounds = { width = input.bounds.width, height = input.bounds.height },
    sparse_tiles = sparse,
    passability_overrides = passability,
    barriers = normalized,
    summary = {
      barrier_count = #normalized,
      sparse_tile_count = #sparse,
      passability_override_count = #passability,
      generator = M.manifest.id
    }
  }
  return { ok = #diagnostics == 0, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
