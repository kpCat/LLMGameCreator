-- Batch 007 manual examples.
-- This file is intentionally dependency-free. It does not load external files.
-- To run manually in a sandbox, bind module tables to these local names after loading them through the host importer.

local examples = {}

examples.barrier_input = {
  bounds = { width = 8, height = 6 },
  barriers = {
    { id = "barrier/north_wall", shape = "line", from = { x = 0, y = 0 }, to = { x = 7, y = 0 }, type = "wall" },
    { id = "barrier/road_block", shape = "line", from = { x = 3, y = 2 }, to = { x = 3, y = 4 }, type = "road_block" }
  },
  gates = {
    { id = "gate/main", x = 3, y = 0 }
  },
  bridges = {
    { id = "bridge/creek", x = 4, y = 3 }
  }
}

examples.road_input = {
  bounds = { width = 8, height = 6 },
  nodes = {
    { id = "node/spawn", x = 1, y = 1 },
    { id = "node/objective", x = 6, y = 4 }
  },
  roads = {
    { id = "road/spawn_to_objective", from = "node/spawn", to = "node/objective", kind = "dirt" }
  },
  blocked_cells = {
    { x = 3, y = 1 }
  },
  bridge_cells = {
    { x = 3, y = 1 }
  }
}

examples.path_input = {
  bounds = { width = 8, height = 6 },
  start = { x = 1, y = 1 },
  objective = { x = 6, y = 4 },
  waypoints = {
    { x = 2, y = 1 },
    { x = 2, y = 4 }
  },
  blocked_cells = {
    { x = 2, y = 2 }
  },
  bridge_cells = {
    { x = 2, y = 2 }
  }
}

examples.reachability_input = {
  bounds = { width = 8, height = 6 },
  start = { x = 1, y = 1 },
  objectives = {
    { id = "objective/main", x = 6, y = 4 },
    { id = "objective/blocked", x = 3, y = 3 }
  },
  blocked_cells = {
    { x = 3, y = 2 },
    { x = 3, y = 3 },
    { x = 3, y = 4 }
  },
  passable_cells = {
    { x = 3, y = 2 }
  }
}

examples.path_ctx = {
  config = {
    road_tile = "tile/road/dirt",
    bridge_tile = "tile/bridge/wood",
    path_order = "alternating",
    allow_bridges = true,
    max_cells = 128
  }
}

examples.road_ctx = {
  config = {
    road_tile = "tile/road/dirt",
    bridge_tile = "tile/bridge/wood",
    blocked_road_tile = "tile/road/blocked",
    allow_bridges = true,
    max_cells_per_road = 128
  }
}

examples.barrier_ctx = {
  config = {
    wall_tile = "tile/barrier/wall",
    gate_tile = "tile/barrier/gate",
    bridge_tile = "tile/bridge/wood",
    road_block_tile = "tile/barrier/road_block",
    max_tiles = 256
  }
}

examples.reachability_ctx = {
  config = {
    adjacency = "cardinal",
    default_walkable = true,
    max_visited = 256
  }
}

function examples.run(path_carver, road_generator, barrier_generator, reachability)
  local results = {}
  results.barriers = barrier_generator.generate(examples.barrier_input, examples.barrier_ctx)
  results.roads = road_generator.generate(examples.road_input, examples.road_ctx)
  results.path = path_carver.generate(examples.path_input, examples.path_ctx)
  results.reachability = reachability.generate(examples.reachability_input, examples.reachability_ctx)
  return results
end

return examples
