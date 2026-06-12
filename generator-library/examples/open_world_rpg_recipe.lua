local M = {}

M.manifest = {
  id = "examples/open_world_rpg_recipe/v1",
  version = "0.1.0",
  category = "examples",
  title = "Open world RPG recipe",
  purpose = "Compact example recipe/config metadata for Batch 021.",
  capabilities = { "examples.recipe.open_world_rpg" },
  input_schema = {},
  output_schema = {},
  config_schema = {},
  deterministic = true,
  runtime_targets = { "editor", "simulation", "validation" },
  unsafe_features = {}
}

M.recipe = {
  id = "recipe/open_world_rpg/v1",
  title = "Compact Open World RPG Plan",
  purpose = "Recipe for a multi-region RPG plan with world blueprint, biome catalog, region graph, roads, schedules, factions, quests, combat and map UI references.",
  game_idea = {
    genre = "open_world_rpg_planning",
    pitch = "A frontier realm with three regions, faction tension and staged exploration.",
    scope = "three regions, two biomes, road graph metadata, schedule/faction hooks and map UI",
  },
  selected_capabilities = {
    "world.blueprint.generate",
    "world.region_graph.validate",
    "world.biome_catalog.validate",
    "world.reachability.validate",
    "npc.schedule.generate",
    "faction.model.generate",
    "quest.schema.normalize",
    "dialogue.schema.define",
    "ui.global_map.generate",
    "validation.world.validate",
  },
  selected_modules = {
    "world/world_blueprint/v1",
    "world/region_graph/v1",
    "world/biome_catalog/v1",
    "world/reachability/v1",
    "world/road_generator/v1",
    "npc/npc_archetype_generator/v1",
    "npc/schedule_generator/v1",
    "faction/faction_model/v1",
    "quest/quest_schema/v1",
    "dialogue/dialogue_schema/v1",
    "combat/combat_schema/v1",
    "progression/progress_track/v1",
    "ui/minimap_config/v1",
    "ui/quest_journal_ui/v1",
    "validation/world_validation/v1",
    "validation/quest_validation/v1",
  },
  generator_plan = {
    id = "plan/open_world_rpg/v1",
    steps = {
      {
        id = "step/world",
        module_id = "world/world_blueprint/v1",
      },
      {
        id = "step/regions",
        module_id = "world/region_graph/v1",
        depends_on = {
          "step/world",
        },
      },
      {
        id = "step/biomes",
        module_id = "world/biome_catalog/v1",
        depends_on = {
          "step/world",
        },
      },
      {
        id = "step/reachability",
        module_id = "world/reachability/v1",
        depends_on = {
          "step/regions",
        },
      },
      {
        id = "step/npc_schedules",
        module_id = "npc/schedule_generator/v1",
        depends_on = {
          "step/regions",
        },
      },
      {
        id = "step/map_ui",
        module_id = "ui/minimap_config/v1",
        depends_on = {
          "step/regions",
          "step/reachability",
        },
      },
      {
        id = "step/world_validation",
        module_id = "validation/world_validation/v1",
        depends_on = {
          "step/reachability",
        },
      },
    },
  },
  example_configs = {
    world = {
      region_count = 3,
      scale = "region",
      chunk_policy = "compact_metadata",
    },
    biomes = {
      "mist_forest",
      "ashen_hills",
    },
    travel = {
      roads = "sparse_graph",
      gates = "story_locks",
    },
    actors = {
      factions = 2,
      scheduled_npcs = 4,
    },
    ui = {
      minimap = true,
      global_map = true,
      marker_categories = {
        "settlement",
        "quest",
        "danger",
      },
    },
  },
  expected_artifacts = {
    {
      id = "artifact/world/blueprint",
      kind = "world_blueprint",
      produced_by = "step/world",
    },
    {
      id = "artifact/world/regions",
      kind = "region_graph",
      produced_by = "step/regions",
    },
    {
      id = "artifact/world/reachability",
      kind = "reachability_report",
      produced_by = "step/reachability",
    },
    {
      id = "artifact/ui/map",
      kind = "ui_ir",
      produced_by = "step/map_ui",
    },
  },
  validation_plan = {
    modules = {
      "validation/world_validation/v1",
      "validation/quest_validation/v1",
      "validation/interaction_validation/v1",
    },
    expected_diagnostics_shape = {
      "severity",
      "code",
      "message",
      "target",
    },
    must_check = {
      "road_connectivity",
      "region_refs",
      "quest_location_refs",
      "npc_schedule_targets",
    },
  },
  context_pack_hints = {
    purpose = "open world planning with compact IR",
    token_budget = 9000,
    include_modules = {
      "world/world_blueprint/v1",
      "world/region_graph/v1",
      "world/reachability/v1",
      "ui/minimap_config/v1",
    },
    exclude = {
      "full_world_dump",
      "full_dialogue_database",
    },
  },
}

local function diagnostic(severity, code, message, target)
  return { severity = severity, code = code, message = message, target = target }
end

local function is_table(value)
  return type(value) == "table"
end

function M.validate_config(config)
  local diagnostics = {}
  if config ~= nil and not is_table(config) then
    diagnostics[#diagnostics + 1] = diagnostic("error", "config.not_table", "Config must be a table when provided.", "config")
  end
  return { ok = #diagnostics == 0, diagnostics = diagnostics }
end

function M.generate(input, ctx)
  local validation = M.validate_config(input)
  return {
    ok = validation.ok,
    recipe = M.recipe,
    diagnostics = validation.diagnostics,
    metadata = {
      batch = "021",
      recipe_id = M.recipe.id,
      context = is_table(ctx) and "provided" or "none"
    }
  }
end

return M
