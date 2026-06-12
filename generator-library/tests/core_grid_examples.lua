local T = {}

T.manifest = {
  id = "tests/core_grid_examples/v1",
  version = "0.1.0",
  category = "core",
  title = "Core grid and IDs manual examples",
  purpose = "Run compact manual examples for id, coordinates, and grid modules when the host injects module tables.",
  capabilities = { "core.tests.manual_examples" },
  input_schema = {},
  output_schema = {},
  config_schema = {},
  deterministic = true,
  runtime_targets = { "debug" },
  unsafe_features = {}
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

local function add_check(report, name, ok, details)
  report.data.checks[#report.data.checks + 1] = {
    name = name,
    ok = ok == true,
    details = details or {}
  }
  if ok ~= true then
    report.ok = false
  end
end

function T.run(core)
  local report = {
    ok = true,
    data = { checks = {} },
    diagnostics = {},
    artifacts = {}
  }

  if type(core) ~= "table" then
    report.ok = false
    report.diagnostics[#report.diagnostics + 1] = make_diagnostic("core.grid_tests.missing_core", "Test runner expects injected core module table.", "core")
    return report
  end

  local Id = core.id
  local Coordinates = core.coordinates
  local Grid = core.grid

  if type(Id) ~= "table" or type(Coordinates) ~= "table" or type(Grid) ~= "table" then
    report.ok = false
    report.diagnostics[#report.diagnostics + 1] = make_diagnostic("core.grid_tests.missing_module", "Injected core table must contain id, coordinates, and grid modules.", "core")
    return report
  end

  local id_ok = Id.validate("world/chunk/cursed_forest")
  local id_bad_ok, id_bad_diagnostics = Id.validate("World//Chunk")
  add_check(report, "id_valid_lowercase_slash", id_ok == true, {})
  add_check(report, "id_invalid_reports_diagnostics", id_bad_ok == false and id_bad_diagnostics[1] ~= nil, { diagnostics = id_bad_diagnostics })

  local joined = Id.join({ "entity", "npc", "elder" })
  add_check(report, "id_join", joined.ok == true and joined.data.id == "entity/npc/elder", joined.data)

  local chunk_result = Coordinates.world_to_chunk_local({ x = 18, y = 5 }, { width = 16, height = 16 })
  add_check(report, "coordinates_world_to_chunk_local", chunk_result.ok == true and chunk_result.data.chunk.x == 1 and chunk_result.data.local_position.x == 2, chunk_result.data)

  local world_result = Coordinates.chunk_local_to_world({ x = 1, y = 0 }, { x = 2, y = 5 }, 16)
  add_check(report, "coordinates_chunk_local_to_world", world_result.ok == true and world_result.data.position.x == 18 and world_result.data.position.y == 5, world_result.data)

  local actor = { id = "entity/player/main", position = { x = 2, y = 2 }, facing = "east" }
  local front_result = Coordinates.target_cell_in_front(actor)
  add_check(report, "coordinates_target_cell_in_front", front_result.ok == true and front_result.data.position.x == 3 and front_result.data.position.y == 2, front_result.data)

  add_check(report, "coordinates_cardinal_adjacent", Coordinates.matches_adjacency({ x = 2, y = 2 }, { x = 3, y = 2 }, "cardinal_adjacent") == true, {})
  add_check(report, "coordinates_diagonal_adjacent", Coordinates.matches_adjacency({ x = 2, y = 2 }, { x = 3, y = 3 }, "diagonal_adjacent") == true, {})
  add_check(report, "coordinates_radius", Coordinates.matches_adjacency({ x = 2, y = 2 }, { x = 4, y = 2 }, "radius", 2) == true, {})

  local targets = {
    { id = "entity/chest/old", position = { x = 3, y = 2 } },
    { id = "entity/npc/elder", position = { x = 2, y = 1 } }
  }
  local facing_target = Coordinates.disambiguate_targets(actor, targets, { mode = "facing_cell" })
  add_check(report, "coordinates_disambiguate_facing", facing_target.ok == true and facing_target.data.selected.target_id == "entity/chest/old", facing_target.data)

  local ambiguous = Coordinates.disambiguate_targets({ position = { x = 2, y = 2 } }, targets, { mode = "cardinal_adjacent" })
  add_check(report, "coordinates_ambiguous_targets", ambiguous.ok == false and ambiguous.data.ambiguous == true, { diagnostics = ambiguous.diagnostics })

  local grid_result = Grid.create({ width = 4, height = 3, default_cell = { tile = "grass", walkable = true } })
  add_check(report, "grid_create", grid_result.ok == true, grid_result.data)

  local grid = grid_result.data.grid
  local set_result = Grid.set_cell(grid, { x = 2, y = 1 }, { tile = "wall", walkable = false })
  local get_wall = Grid.get_cell(grid, { x = 2, y = 1 })
  local get_default = Grid.get_cell(grid, { x = 0, y = 0 })
  add_check(report, "grid_set_get_override", set_result.ok == true and get_wall.data.cell.tile == "wall" and get_wall.data.has_override == true, get_wall.data)
  add_check(report, "grid_get_default", get_default.ok == true and get_default.data.cell.tile == "grass" and get_default.data.has_override == false, get_default.data)

  local out_of_bounds = Grid.get_cell(grid, { x = 4, y = 0 })
  add_check(report, "grid_bounds_diagnostic", out_of_bounds.ok == false and out_of_bounds.diagnostics[1] ~= nil, { diagnostics = out_of_bounds.diagnostics })

  local neighborhood = Grid.neighborhood(grid, { x = 1, y = 1 }, { mode = "cardinal" })
  add_check(report, "grid_cardinal_neighborhood", neighborhood.ok == true and #neighborhood.data.cells == 4, { count = #neighborhood.data.cells })

  local grid_front = Grid.target_cell_in_front(grid, actor)
  add_check(report, "grid_target_cell_in_front_with_bounds", grid_front.ok == true and grid_front.data.in_bounds == true and grid_front.data.position.x == 3, grid_front.data)

  return report
end

function T.validate_config(config)
  if config ~= nil and type(config) ~= "table" then
    return false, { make_diagnostic("core.grid_tests.config_not_table", "Test config must be a table.", "config") }
  end
  return true, {}
end

return T
