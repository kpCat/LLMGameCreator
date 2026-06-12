local M = {}

M.manifest = {
  id = "examples/factorio_like_alchemy_recipe/v1",
  version = "0.1.0",
  category = "examples",
  title = "Factorio-like alchemy recipe",
  purpose = "Compact example recipe/config metadata for Batch 021.",
  capabilities = { "examples.recipe.factorio_like_alchemy" },
  input_schema = {},
  output_schema = {},
  config_schema = {},
  deterministic = true,
  runtime_targets = { "editor", "simulation", "validation" },
  unsafe_features = {}
}

M.recipe = {
  id = "recipe/factorio_like_alchemy/v1",
  title = "Factorio-like Alchemy Workshop",
  purpose = "Compact automation recipe for alchemical production chains, machines, conveyors, power and build-menu UI metadata.",
  game_idea = {
    genre = "automation_planner",
    pitch = "Players refine herbs and ore into essences through belts, vats and rune-powered machines.",
    scope = "three recipes, three machines, one conveyor tier and one small power network",
  },
  selected_capabilities = {
    "automation.recipe_graph.generate",
    "automation.machine_catalog.generate",
    "automation.conveyor_grid.generate",
    "automation.power_network.generate",
    "ui.build_menu.layout",
    "generation.dependencies.sort",
    "generation.artifact_manifest.generate",
  },
  selected_modules = {
    "automation/recipe_graph/v1",
    "automation/machine_catalog/v1",
    "automation/conveyor_grid/v1",
    "automation/power_network/v1",
    "ui/hud_layout/v1",
    "validation/module_contract_validation/v1",
    "generation/dependency_sort/v1",
    "generation/artifact_manifest/v1",
    "generation/pipeline_runner_plan/v1",
  },
  generator_plan = {
    id = "plan/factorio_like_alchemy/v1",
    steps = {
      {
        id = "step/recipes",
        module_id = "automation/recipe_graph/v1",
      },
      {
        id = "step/machines",
        module_id = "automation/machine_catalog/v1",
        depends_on = {
          "step/recipes",
        },
      },
      {
        id = "step/logistics",
        module_id = "automation/conveyor_grid/v1",
        depends_on = {
          "step/machines",
        },
      },
      {
        id = "step/power",
        module_id = "automation/power_network/v1",
        depends_on = {
          "step/machines",
        },
      },
      {
        id = "step/build_menu",
        module_id = "ui/hud_layout/v1",
        depends_on = {
          "step/machines",
          "step/logistics",
        },
      },
      {
        id = "step/artifacts",
        module_id = "generation/artifact_manifest/v1",
        depends_on = {
          "step/recipes",
          "step/machines",
          "step/logistics",
          "step/power",
        },
      },
    },
  },
  example_configs = {
    resource_nodes = {
      "herb_patch",
      "copper_ore",
    },
    recipes = {
      "crush_herbs",
      "distill_essence",
      "charge_crystal",
    },
    machines = {
      "crusher",
      "still",
      "rune_charger",
    },
    logistics = {
      conveyor_tiers = 1,
      junction_policy = "deterministic_priority",
    },
    power = {
      network_style = "small_grid",
      required_margin_percent = 10,
    },
  },
  expected_artifacts = {
    {
      id = "artifact/automation/recipes",
      kind = "recipe_graph",
      produced_by = "step/recipes",
    },
    {
      id = "artifact/automation/machines",
      kind = "machine_catalog",
      produced_by = "step/machines",
    },
    {
      id = "artifact/automation/logistics",
      kind = "conveyor_ir",
      produced_by = "step/logistics",
    },
    {
      id = "artifact/automation/power",
      kind = "power_network_ir",
      produced_by = "step/power",
    },
  },
  validation_plan = {
    modules = {
      "validation/module_contract_validation/v1",
      "generation/dependency_sort/v1",
    },
    expected_diagnostics_shape = {
      "severity",
      "code",
      "message",
      "target",
    },
    must_check = {
      "recipe_inputs",
      "machine_recipe_map",
      "power_balance",
      "artifact_dependencies",
    },
  },
  context_pack_hints = {
    purpose = "automation design and production-chain planning",
    token_budget = 5500,
    include_modules = {
      "automation/recipe_graph/v1",
      "automation/machine_catalog/v1",
      "automation/conveyor_grid/v1",
      "automation/power_network/v1",
    },
    exclude = {
      "runtime_simulation",
      "large_item_catalog",
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
