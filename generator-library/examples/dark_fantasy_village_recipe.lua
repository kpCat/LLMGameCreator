local M = {}

M.manifest = {
  id = "examples/dark_fantasy_village_recipe/v1",
  version = "0.1.0",
  category = "examples",
  title = "Dark fantasy village recipe",
  purpose = "Compact example recipe/config metadata for Batch 021.",
  capabilities = { "examples.recipe.dark_fantasy_village" },
  input_schema = {},
  output_schema = {},
  config_schema = {},
  deterministic = true,
  runtime_targets = { "editor", "simulation", "validation" },
  unsafe_features = {}
}

M.recipe = {
  id = "recipe/dark_fantasy_village/v1",
  title = "Dark Fantasy Village Slice",
  purpose = "Compact RPG slice recipe for a cursed village with one region, NPCs, dialogue, quest hooks, optional combat IR and UI references.",
  game_idea = {
    genre = "small_rpg_slice",
    pitch = "A border village hides a shrine curse, a suspicious reeve and a missing herbalist.",
    scope = "one village region, two NPC archetypes, one investigation quest and one optional encounter",
  },
  selected_capabilities = {
    "world.blueprint.generate",
    "world.region_graph.validate",
    "npc.archetype.generate",
    "dialogue.schema.define",
    "quest.schema.normalize",
    "combat.schema.define",
    "ui.hud.generate",
    "ui.quest_journal.generate",
    "validation.world.validate",
  },
  selected_modules = {
    "world/world_blueprint/v1",
    "world/region_graph/v1",
    "npc/npc_archetype_generator/v1",
    "dialogue/dialogue_schema/v1",
    "dialogue/fact_based_dialogue/v1",
    "quest/quest_schema/v1",
    "quest/simple_investigation/v1",
    "combat/combat_schema/v1",
    "combat/status_effects/v1",
    "ability/ability_catalog_generator/v1",
    "ui/hud_layout/v1",
    "ui/quest_journal_ui/v1",
    "validation/world_validation/v1",
    "validation/quest_validation/v1",
    "generation/artifact_manifest/v1",
  },
  generator_plan = {
    id = "plan/dark_fantasy_village/v1",
    steps = {
      {
        id = "step/world_outline",
        module_id = "world/world_blueprint/v1",
        config_ref = "config/world/village",
      },
      {
        id = "step/region_links",
        module_id = "world/region_graph/v1",
        depends_on = {
          "step/world_outline",
        },
      },
      {
        id = "step/npc_cast",
        module_id = "npc/npc_archetype_generator/v1",
        depends_on = {
          "step/region_links",
        },
      },
      {
        id = "step/investigation_quest",
        module_id = "quest/simple_investigation/v1",
        depends_on = {
          "step/npc_cast",
        },
      },
      {
        id = "step/quest_ui",
        module_id = "ui/quest_journal_ui/v1",
        depends_on = {
          "step/investigation_quest",
        },
      },
      {
        id = "step/validation",
        module_id = "validation/quest_validation/v1",
        depends_on = {
          "step/investigation_quest",
        },
      },
    },
  },
  example_configs = {
    world = {
      scale = "village_region",
      region_count = 1,
      chunk_mode = "compact",
    },
    npc = {
      archetype_count = 2,
      roles = {
        "reeve",
        "herbalist_apprentice",
      },
    },
    quest = {
      stage_count = 3,
      objective_style = "investigate_talk_inspect",
    },
    ui = {
      hud_preset = "rpg",
      quest_journal = "compact",
    },
  },
  expected_artifacts = {
    {
      id = "artifact/world/village_blueprint",
      kind = "world_blueprint",
      produced_by = "step/world_outline",
    },
    {
      id = "artifact/npc/village_cast",
      kind = "npc_config",
      produced_by = "step/npc_cast",
    },
    {
      id = "artifact/quest/shrine_curse",
      kind = "quest_ir",
      produced_by = "step/investigation_quest",
    },
    {
      id = "artifact/ui/quest_journal",
      kind = "ui_ir",
      produced_by = "step/quest_ui",
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
      "region_references",
      "quest_targets",
      "dialogue_links",
      "optional_combat_refs",
    },
  },
  context_pack_hints = {
    purpose = "small dark fantasy RPG design pass",
    token_budget = 6000,
    include_modules = {
      "world/world_blueprint/v1",
      "npc/npc_archetype_generator/v1",
      "quest/simple_investigation/v1",
      "ui/quest_journal_ui/v1",
    },
    exclude = {
      "full_tile_arrays",
      "large_dialogue_corpus",
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
