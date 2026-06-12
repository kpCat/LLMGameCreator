local M = {}

M.manifest = {
  id = "interaction/targeting/v1",
  version = "0.1.0",
  category = "interaction",
  title = "Interaction Targeting",
  purpose = "Selects possible interaction targets using facing, same-cell, adjacency and radius rules.",
  capabilities = { "interaction.target.select", "interaction.target.disambiguate" },
  input_schema = {
    type = "table",
    fields = {
      actor = "{ id, x, y, facing }",
      entities = "array of compact entity instances",
      rule = "optional target rule override",
      requested_action = "optional interaction action such as inspect or talk"
    }
  },
  output_schema = {
    type = "table",
    fields = {
      candidates = "candidate target array",
      selected = "selected target or nil",
      needs_disambiguation = "boolean",
      target_cell = "facing target cell when relevant",
      summary = "selection metadata"
    }
  },
  config_schema = {
    mode = "facing_cell | same_cell | cardinal_adjacent | diagonal_adjacent | radius",
    radius = "optional integer for radius mode",
    required_component = "optional component name",
    action = "optional interactable action filter",
    disambiguation = "first | nearest | highest_priority | explicit_only"
  },
  deterministic = true,
  runtime_targets = { "debug", "unity2d", "unity_tilemap" },
  supported_world_scales = { "single_map", "multi_map", "region", "infinite_chunks" },
  supported_turn_modes = { "realtime", "turn_based", "mixed" },
  supported_combat_modes = { "none", "realtime", "turn_based", "tactical", "dialogue_combat", "hybrid" },
  unsafe_features = {}
}

local VALID_FACING = { north = true, south = true, east = true, west = true }
local VALID_MODES = { facing_cell = true, same_cell = true, cardinal_adjacent = true, diagonal_adjacent = true, radius = true }
local VALID_DISAMBIGUATION = { first = true, nearest = true, highest_priority = true, explicit_only = true }

local function diag(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_int(v)
  return type(v) == "number" and v % 1 == 0
end

local function valid_pos(p)
  return type(p) == "table" and is_int(p.x) and is_int(p.y)
end

local function valid_slash_id(id)
  if type(id) ~= "string" then
    return false
  end
  if id == "" or string.sub(id, 1, 1) == "/" or string.sub(id, #id, #id) == "/" then
    return false
  end
  if string.find(id, "//", 1, true) then
    return false
  end
  return string.match(id, "^[a-z0-9_]+(/[a-z0-9_]+)*$") ~= nil
end

local function abs(v)
  if v < 0 then
    return -v
  end
  return v
end

local function distance_cardinal(a, b)
  return abs(a.x - b.x) + abs(a.y - b.y)
end

local function distance_chebyshev(a, b)
  local dx = abs(a.x - b.x)
  local dy = abs(a.y - b.y)
  if dx > dy then
    return dx
  end
  return dy
end

local function facing_cell(actor)
  if actor.facing == "north" then
    return { x = actor.x, y = actor.y - 1 }
  end
  if actor.facing == "south" then
    return { x = actor.x, y = actor.y + 1 }
  end
  if actor.facing == "east" then
    return { x = actor.x + 1, y = actor.y }
  end
  return { x = actor.x - 1, y = actor.y }
end

local function has_action(entity, action)
  if action == nil then
    return true
  end
  if type(entity.components) ~= "table" or type(entity.components.interactable) ~= "table" then
    return false
  end
  local actions = entity.components.interactable.actions
  if type(actions) ~= "table" then
    return false
  end
  for i = 1, #actions do
    if actions[i] == action then
      return true
    end
  end
  return false
end

local function has_component(entity, component)
  if component == nil then
    return true
  end
  return type(entity.components) == "table" and entity.components[component] ~= nil
end

local function component_priority(entity)
  if type(entity.components) == "table" and type(entity.components.interactable) == "table" then
    local p = entity.components.interactable.priority
    if type(p) == "number" then
      return p
    end
  end
  return 0
end

local function compare_id(a, b)
  if type(a.id) ~= "string" then
    return false
  end
  if type(b.id) ~= "string" then
    return true
  end
  return a.id < b.id
end

local function sort_candidates(candidates, disambiguation)
  table.sort(candidates, function(a, b)
    if disambiguation == "highest_priority" and a.priority ~= b.priority then
      return a.priority > b.priority
    end
    if disambiguation == "nearest" and a.distance ~= b.distance then
      return a.distance < b.distance
    end
    if a.distance ~= b.distance then
      return a.distance < b.distance
    end
    if a.priority ~= b.priority then
      return a.priority > b.priority
    end
    return compare_id(a, b)
  end)
end

local function candidate_matches(actor, entity, cfg, target_cell)
  if not valid_pos(entity) then
    return false, nil
  end
  if cfg.mode == "facing_cell" then
    if entity.x == target_cell.x and entity.y == target_cell.y then
      return true, 1
    end
    return false, nil
  end
  if cfg.mode == "same_cell" then
    if entity.x == actor.x and entity.y == actor.y then
      return true, 0
    end
    return false, nil
  end
  if cfg.mode == "cardinal_adjacent" then
    local d = distance_cardinal(actor, entity)
    return d == 1, d
  end
  if cfg.mode == "diagonal_adjacent" then
    local d = distance_chebyshev(actor, entity)
    return d == 1, d
  end
  local d = distance_chebyshev(actor, entity)
  return d <= cfg.radius, d
end

function M.validate_config(config)
  local diagnostics = {}
  local cfg = config or {}
  if type(cfg) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.config_not_table", "Config must be a table.", "config")
    return false, diagnostics
  end
  if cfg.mode ~= nil and not VALID_MODES[cfg.mode] then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.mode_invalid", "mode must be facing_cell, same_cell, cardinal_adjacent, diagonal_adjacent or radius.", "config.mode")
  end
  if cfg.radius ~= nil and (not is_int(cfg.radius) or cfg.radius < 0) then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.radius_invalid", "radius must be a non-negative integer.", "config.radius")
  end
  if cfg.required_component ~= nil and type(cfg.required_component) ~= "string" then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.required_component_invalid", "required_component must be a string when provided.", "config.required_component")
  end
  if cfg.action ~= nil and type(cfg.action) ~= "string" then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.action_invalid", "action must be a string when provided.", "config.action")
  end
  if cfg.explicit_target_id ~= nil and not valid_slash_id(cfg.explicit_target_id) then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.explicit_target_id_invalid", "explicit_target_id must be a lowercase slash id.", "config.explicit_target_id")
  end
  if cfg.disambiguation ~= nil and not VALID_DISAMBIGUATION[cfg.disambiguation] then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.disambiguation_invalid", "disambiguation must be first, nearest, highest_priority or explicit_only.", "config.disambiguation")
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
    diagnostics[#diagnostics + 1] = diag("error", "targeting.input_not_table", "Input must be a table.", "input")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end
  if not valid_pos(input.actor) then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.actor_invalid", "actor must contain integer x and y.", "input.actor")
  elseif input.actor.facing ~= nil and not VALID_FACING[input.actor.facing] then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.actor_facing_invalid", "actor.facing must be north, south, east or west.", "input.actor.facing")
  end
  if type(input.entities) ~= "table" then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.entities_invalid", "entities must be an array.", "input.entities")
  end
  if #diagnostics > 0 then
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local rule = input.rule
  if type(rule) ~= "table" then
    rule = {}
  end
  local cfg = {
    mode = rule.mode or config.mode or "facing_cell",
    radius = rule.radius or config.radius or 1,
    required_component = rule.required_component or config.required_component or "interactable",
    action = input.requested_action or rule.action or config.action,
    explicit_target_id = input.target_id or rule.explicit_target_id or config.explicit_target_id,
    disambiguation = rule.disambiguation or config.disambiguation or "nearest"
  }

  if not VALID_MODES[cfg.mode] then
    diagnostics[#diagnostics + 1] = diag("error", "targeting.effective_mode_invalid", "Effective targeting mode is invalid.", "input.rule.mode")
    return { ok = false, data = {}, diagnostics = diagnostics, artifacts = {} }
  end

  local actor = {
    id = input.actor.id,
    x = input.actor.x,
    y = input.actor.y,
    facing = input.actor.facing or "south"
  }
  local target_cell = facing_cell(actor)
  local candidates = {}

  for i = 1, #input.entities do
    local e = input.entities[i]
    if type(e) == "table" and e.id ~= actor.id and has_component(e, cfg.required_component) and has_action(e, cfg.action) then
      local matches, distance = candidate_matches(actor, e, cfg, target_cell)
      if matches then
        candidates[#candidates + 1] = {
          id = e.id,
          prototype_id = e.prototype_id,
          title = e.title,
          x = e.x,
          y = e.y,
          distance = distance,
          priority = component_priority(e),
          required_component = cfg.required_component,
          action = cfg.action
        }
      end
    end
  end

  sort_candidates(candidates, cfg.disambiguation)
  local selected = nil
  local needs_disambiguation = false
  if cfg.explicit_target_id ~= nil then
    for i = 1, #candidates do
      if candidates[i].id == cfg.explicit_target_id then
        selected = candidates[i]
      end
    end
    if selected == nil then
      diagnostics[#diagnostics + 1] = diag("error", "targeting.explicit_target_not_found", "Explicit target id did not match any valid candidate.", cfg.explicit_target_id)
    end
  elseif cfg.disambiguation == "explicit_only" then
    needs_disambiguation = #candidates > 0
    if needs_disambiguation then
      diagnostics[#diagnostics + 1] = diag("warning", "targeting.explicit_target_required", "A target exists but explicit target selection is required.", "input.target_id")
    end
  elseif #candidates == 1 then
    selected = candidates[1]
  elseif #candidates > 1 then
    selected = candidates[1]
    needs_disambiguation = true
    diagnostics[#diagnostics + 1] = diag("warning", "targeting.multiple_candidates", "Multiple valid targets found; selected the deterministic first candidate.", "input.entities")
  else
    diagnostics[#diagnostics + 1] = diag("info", "targeting.no_candidates", "No valid interaction target found.", "input.entities")
  end

  local data = {
    candidates = candidates,
    selected = selected,
    needs_disambiguation = needs_disambiguation,
    target_cell = target_cell,
    summary = {
      generator = M.manifest.id,
      mode = cfg.mode,
      required_component = cfg.required_component,
      action = cfg.action,
      candidate_count = #candidates
    }
  }
  return { ok = selected ~= nil or #candidates == 0, data = data, diagnostics = diagnostics, artifacts = {} }
end

return M
