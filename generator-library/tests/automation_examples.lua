local examples = {}

examples.recipe_graph_input = {
  resources = {
    { item_id = "item/resource/iron_ore" },
    { item_id = "item/resource/copper_ore" }
  },
  recipes = {
    {
      id = "automation/recipe/iron_plate",
      category = "smelting",
      craft_seconds = 3.5,
      inputs = {
        { item_id = "item/resource/iron_ore", amount = 1 }
      },
      outputs = {
        { item_id = "item/intermediate/iron_plate", amount = 1 }
      }
    },
    {
      id = "automation/recipe/gear",
      category = "crafting",
      craft_seconds = 0.5,
      inputs = {
        { item_id = "item/intermediate/iron_plate", amount = 2 }
      },
      outputs = {
        { item_id = "item/component/gear", amount = 1 }
      }
    }
  },
  targets = {
    { item_id = "item/component/gear", rate_per_second = 2 }
  }
}

examples.machine_catalog_input = {
  recipe_categories = { "crafting", "smelting" },
  machines = {
    {
      id = "automation/machine/stone_furnace",
      title = "Stone Furnace",
      recipe_categories = { "smelting" },
      speed = 1,
      power_demand_kw = 0,
      tags = { "burner" }
    },
    {
      id = "automation/machine/assembler_1",
      title = "Assembler I",
      recipe_categories = { "crafting" },
      speed = 0.5,
      power_demand_kw = 90,
      module_slots = 0
    }
  }
}

examples.conveyor_grid_input = {
  grid = {
    coordinate_mode = "tile",
    width = 16,
    height = 8
  },
  nodes = {
    { id = "automation/node/iron_source", kind = "resource_port", x = 0, y = 2, direction = "east" },
    { id = "automation/node/furnace_input", kind = "machine_port", x = 4, y = 2, direction = "east" },
    { id = "automation/node/furnace_output", kind = "machine_port", x = 5, y = 2, direction = "east" },
    { id = "automation/node/chest", kind = "chest", x = 8, y = 2, direction = "east" }
  },
  links = {
    {
      id = "automation/link/ore_to_furnace",
      from_id = "automation/node/iron_source",
      to_id = "automation/node/furnace_input",
      mode = "belt",
      capacity_per_second = 15
    },
    {
      id = "automation/link/plate_to_chest",
      from_id = "automation/node/furnace_output",
      to_id = "automation/node/chest",
      mode = "belt",
      capacity_per_second = 15
    }
  }
}

examples.power_network_input = {
  generators = {
    { id = "automation/power/steam_generator_1", kind = "steam", capacity_kw = 900 }
  },
  consumers = {
    { id = "automation/machine/assembler_1", kind = "machine", demand_kw = 90, priority = 1 },
    { id = "automation/machine/miner_1", kind = "machine", demand_kw = 180, priority = 1 }
  },
  accumulators = {
    { id = "automation/power/battery_1", capacity_kj = 5000, transfer_kw = 300 }
  }
}

examples.expected_shapes = {
  recipe_graph = {
    "ok",
    "data.recipe_graph.recipes",
    "data.recipe_graph.items",
    "data.production_chains",
    "diagnostics"
  },
  machine_catalog = {
    "ok",
    "data.machine_catalog.machines",
    "data.recipe_category_map",
    "diagnostics"
  },
  conveyor_grid = {
    "ok",
    "data.conveyor_grid.nodes",
    "data.conveyor_grid.links",
    "data.logistics_graph.adjacency",
    "diagnostics"
  },
  power_network = {
    "ok",
    "data.power_network.generators",
    "data.power_balance",
    "diagnostics"
  }
}

return examples
