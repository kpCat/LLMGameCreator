local M = {}

M.manifest = {
  id = "examples/city_builder_frontier_recipe/v1",
  version = "0.1.0",
  category = "examples",
  title = "City-builder frontier recipe",
  purpose = "Compact example recipe/config metadata for Batch 021.",
  capabilities = { "examples.recipe.city_builder_frontier" },
  input_schema = {},
  output_schema = {},
  config_schema = {},
  deterministic = true,
  runtime_targets = { "editor", "simulation", "validation" },
  unsafe_features = {}
}

M.recipe = {
  id = "recipe/city_builder_frontier/v1",
  title = "Frontier City Builder Setup",
  purpose = "Compact city-builder recipe using needs, jobs, buildings, service coverage, economy hooks, UI build-menu metadata and validation refs.",
  game_idea = {
    genre = "frontier_city_builder",
    pitch = "A small settlement must balance food, shelter, work and services while expanding toward nearby resources.",
    scope = "four needs, four building types, three job roles and two services",
  },
  selected_capabilities = {
    "city_builder.citizen_needs.generate",
    "city_builder.jobs.configure",
    "city_builder.buildings.generate",
    "city_builder.service_coverage.generate",
    "ui.build_menu.layout",
    "validation.world.validate",
    "generation.context_pack_plan.generate",
  },
  selected_modules = {
    "simulation/citizen_needs/v1",
    "simulation/job_system_config/v1",
    "simulation/building_catalog/v1",
    "simulation/service_coverage/v1",
    "automation/power_network/v1",
    "ui/hud_layout/v1",
    "ui/minimap_config/v1",
    "validation/world_validation/v1",
    "validation/module_contract_validation/v1",
    "generation/context_pack_plan/v1",
  },
  generator_plan = {
    id = "plan/city_builder_frontier/v1",
    steps = {
      {
        id = "step/needs",
        module_id = "simulation/citizen_needs/v1",
      },
      {
        id = "step/buildings",
        module_id = "simulation/building_catalog/v1",
        depends_on = {
          "step/needs",
        },
      },
      {
        id = "step/jobs",
        module_id = "simulation/job_system_config/v1",
        depends_on = {
          "step/buildings",
        },
      },
      {
        id = "step/services",
        module_id = "simulation/service_coverage/v1",
        depends_on = {
          "step/buildings",
          "step/jobs",
        },
      },
      {
        id = "step/build_menu",
        module_id = "ui/hud_layout/v1",
        depends_on = {
          "step/buildings",
        },
      },
      {
        id = "step/context_pack",
        module_id = "generation/context_pack_plan/v1",
        depends_on = {
          "step/needs",
          "step/buildings",
        },
      },
    },
  },
  example_configs = {
    citizens = {
      starting_households = 8,
      needs = {
        "food",
        "shelter",
        "warmth",
        "health",
      },
    },
    jobs = {
      roles = {
        "builder",
        "farmer",
        "woodcutter",
      },
      assignment = "capacity_limited",
    },
    buildings = {
      categories = {
        "housing",
        "production",
        "service",
        "storage",
      },
      footprint_policy = "grid_metadata",
    },
    services = {
      coverage_mode = "radius_metadata",
      tick_mode = "planning_tick",
    },
    economy_hooks = {
      "upkeep",
      "production_input",
      "production_output",
    },
  },
  expected_artifacts = {
    {
      id = "artifact/city/needs",
      kind = "citizen_need_config",
      produced_by = "step/needs",
    },
    {
      id = "artifact/city/buildings",
      kind = "building_catalog",
      produced_by = "step/buildings",
    },
    {
      id = "artifact/city/jobs",
      kind = "job_config",
      produced_by = "step/jobs",
    },
    {
      id = "artifact/city/services",
      kind = "service_coverage_ir",
      produced_by = "step/services",
    },
  },
  validation_plan = {
    modules = {
      "validation/world_validation/v1",
      "validation/module_contract_validation/v1",
    },
    expected_diagnostics_shape = {
      "severity",
      "code",
      "message",
      "target",
    },
    must_check = {
      "building_refs",
      "job_capacity",
      "service_radius",
      "need_satisfaction_sources",
    },
  },
  context_pack_hints = {
    purpose = "city-builder compact design pass",
    token_budget = 6500,
    include_modules = {
      "simulation/citizen_needs/v1",
      "simulation/building_catalog/v1",
      "simulation/service_coverage/v1",
      "ui/hud_layout/v1",
    },
    exclude = {
      "long_balance_tables",
      "runtime_tick_loop",
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
