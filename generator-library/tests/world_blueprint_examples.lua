local T = {}

local function expect(condition, message, failures)
  if not condition then
    failures[#failures + 1] = message
  end
end

function T.run_examples(modules)
  local failures = {}
  local world_blueprint = modules and modules.world_blueprint
  local region_graph = modules and modules.region_graph
  local biome_catalog = modules and modules.biome_catalog

  expect(type(world_blueprint) == "table", "world_blueprint module must be injected", failures)
  expect(type(region_graph) == "table", "region_graph module must be injected", failures)
  expect(type(biome_catalog) == "table", "biome_catalog module must be injected", failures)
  if #failures > 0 then
    return { ok = false, failures = failures }
  end

  local config = {
    world_id = "world/cursed_valley",
    title = "Cursed Valley",
    blueprint_mode = "region",
    world_scale = "region",
    seed = 12345,
    biomes = {
      {
        id = "biome/dark_forest",
        title = "Dark Forest",
        temperature = 0.38,
        humidity = 0.75,
        danger = 0.65,
        tags = { "forest", "shadow" },
        resources = { "resource/wood", "resource/mushroom" },
        minimap = { color_key = "forest_dark" }
      },
      {
        id = "biome/ruined_field",
        title = "Ruined Field",
        temperature = 0.52,
        humidity = 0.35,
        danger = 0.42,
        tags = { "ruin", "open" },
        resources = { "resource/stone", "resource/scrap" },
        minimap = { color_key = "ruin" }
      }
    },
    maps = {
      {
        id = "map/valley_overworld",
        title = "Valley Overworld",
        bounds = { x = 0, y = 0, width = 128, height = 96 },
        default_biome_id = "biome/dark_forest"
      }
    },
    regions = {
      { id = "region/old_road", title = "Old Road", map_id = "map/valley_overworld", biome_id = "biome/ruined_field", position = { x = 8, y = 12 } },
      { id = "region/dark_woods", title = "Dark Woods", map_id = "map/valley_overworld", biome_id = "biome/dark_forest", position = { x = 32, y = 18 } },
      { id = "region/ruined_gate", title = "Ruined Gate", map_id = "map/valley_overworld", biome_id = "biome/ruined_field", position = { x = 54, y = 20 } }
    },
    connections = {
      { from = "region/old_road", to = "region/dark_woods", type = "trail", bidirectional = true },
      { from = "region/dark_woods", to = "region/ruined_gate", type = "road", bidirectional = true, blocked = true, tags = { "blocked_road" } }
    },
    global_map = {
      enabled = true,
      layers = { "regions", "biomes", "connections" }
    },
    minimap = {
      enabled = true,
      layers = { "terrain", "regions", "points_of_interest" }
    }
  }

  local biome_result = biome_catalog.generate({ biomes = config.biomes }, { config = { default_biome_id = "biome/dark_forest" } })
  expect(biome_result.ok == true, "biome catalog should generate", failures)
  expect(biome_result.data.counts.biomes == 2, "biome catalog should contain two biomes", failures)
  expect(type(biome_result.data.tag_index.forest) == "table", "biome tag index should include forest", failures)

  local graph_result = region_graph.generate({ regions = config.regions, connections = config.connections }, { config = {} })
  expect(graph_result.ok == true, "region graph should generate", failures)
  expect(graph_result.data.counts.regions == 3, "region graph should contain three regions", failures)
  expect(type(graph_result.data.adjacency["region/dark_woods"]) == "table", "region graph should build adjacency", failures)

  local blueprint_result = world_blueprint.generate({ blueprint_mode = "region" }, { config = config })
  expect(blueprint_result.ok == true, "world blueprint should generate", failures)
  expect(blueprint_result.data.world.id == "world/cursed_valley", "world id should be preserved", failures)
  expect(blueprint_result.data.world.coordinate_system.origin == "zero_based", "world coordinates should be zero-based", failures)
  expect(blueprint_result.data.counts.regions == 3, "world blueprint should contain three regions", failures)
  expect(blueprint_result.data.global_map.enabled == true, "global map metadata should be enabled", failures)
  expect(blueprint_result.data.minimap.enabled == true, "minimap metadata should be enabled", failures)
  expect(blueprint_result.data.generation_policy.emit_huge_tile_arrays == false, "blueprint must not emit huge tile arrays", failures)

  local infinite_result = world_blueprint.generate({}, {
    config = {
      world_id = "world/endless_frontier",
      blueprint_mode = "infinite_seeded_world",
      world_scale = "infinite_chunks",
      seed = 98765,
      chunking = { chunk_width = 32, chunk_height = 32 },
      biomes = config.biomes
    }
  })
  expect(infinite_result.ok == true, "infinite seeded blueprint should generate", failures)
  expect(infinite_result.data.chunking.enabled == true, "infinite seeded blueprint should enable chunking", failures)
  expect(infinite_result.data.chunking.infinite == true, "infinite seeded blueprint should mark chunking as infinite", failures)

  return {
    ok = #failures == 0,
    failures = failures,
    samples = {
      biome_result = biome_result,
      graph_result = graph_result,
      blueprint_result = blueprint_result,
      infinite_result = infinite_result
    }
  }
end

return T
