local M = {}

M.title = "Batch 006 manual examples"
M.description = "Manual smoke examples for injected chunk generation modules. The file intentionally avoids require/dofile and external dependencies."

local function assert_true(value, message)
  if value ~= true then
    return { ok = false, message = message }
  end
  return { ok = true }
end

local function assert_table(value, message)
  if type(value) ~= "table" then
    return { ok = false, message = message }
  end
  return { ok = true }
end

local function push(results, check)
  results[#results + 1] = check
end

function M.run(modules)
  local results = {}
  modules = type(modules) == "table" and modules or {}

  local chunk_generator = modules.chunk_generator
  local tile_painter = modules.tile_painter
  local landmark_placer = modules.landmark_placer

  push(results, assert_table(chunk_generator, "chunk_generator module must be injected"))
  push(results, assert_table(tile_painter, "tile_painter module must be injected"))
  push(results, assert_table(landmark_placer, "landmark_placer module must be injected"))

  if type(chunk_generator) == "table" then
    local generated = chunk_generator.generate({
      chunk = { x = 0, y = 0 },
      include_full_tiles = false,
      roads = {
        {
          points = { { x = 0, y = 4 }, { x = 7, y = 4 } },
          tile_id = "tile/road",
          blocked_cells = { { x = 3, y = 4 } },
          blocked_tile_id = "tile/blocked_road"
        }
      },
      landmarks = {
        { id = "landmark/old_well", tile_id = "tile/well", position = { x = 2, y = 2 }, minimap_key = "landmark" }
      }
    }, {
      config = {
        chunk_size = { width = 8, height = 8 },
        seed = 42,
        default_tile_id = "tile/grass",
        default_minimap_key = "grass",
        terrain_rules = {
          { tile_id = "tile/forest", threshold = 1200, walkable = true, minimap_key = "forest" },
          { tile_id = "tile/water", threshold = 300, walkable = false, minimap_key = "water" }
        }
      }
    })
    push(results, assert_true(generated.ok, "chunk generation should succeed"))
    push(results, assert_table(generated.data.sparse_tiles, "chunk output must include sparse_tiles"))
    push(results, assert_table(generated.data.minimap_layer, "chunk output must include minimap_layer"))
    push(results, assert_true(generated.data.full_tiles_omitted, "full tile array should be omitted by default"))
  end

  if type(tile_painter) == "table" then
    local painted = tile_painter.generate({
      width = 8,
      height = 8,
      default_tile_id = "tile/grass",
      operations = {
        { type = "rect", x = 1, y = 1, width = 2, height = 2, tile_id = "tile/mud", minimap_key = "mud" },
        { type = "road", points = { { x = 0, y = 6 }, { x = 7, y = 6 } }, tile_id = "tile/road", blocked_cells = { { x = 5, y = 6 } } }
      }
    }, { config = {} })
    push(results, assert_true(painted.ok, "tile painter should succeed"))
    push(results, assert_table(painted.data.walkability_overrides, "paint output must include walkability_overrides"))
  end

  if type(landmark_placer) == "table" then
    local placed = landmark_placer.generate({
      width = 8,
      height = 8,
      seed = 9,
      sparse_tiles = {
        { x = 4, y = 4, tile_id = "tile/blocked_road", walkable = false, tags = { "blocked" } }
      },
      landmarks = {
        { id = "landmark/ruin", tile_id = "tile/ruin", position = { x = 1, y = 1 }, minimap_key = "ruin" },
        { id = "landmark/camp", tile_id = "tile/camp", minimap_key = "camp" }
      }
    }, { config = { min_distance = 2, candidate_count = 12 } })
    push(results, assert_true(placed.ok, "landmark placement should succeed"))
    push(results, assert_table(placed.data.placements, "landmark output must include placements"))
  end

  local ok = true
  for index = 1, #results do
    if results[index].ok ~= true then
      ok = false
    end
  end
  return { ok = ok, results = results }
end

return M
