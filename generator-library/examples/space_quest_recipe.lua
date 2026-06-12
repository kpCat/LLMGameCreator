local M = {}

M.manifest = {
  id = "examples/space_quest_recipe/v1",
  version = "0.1.0",
  category = "examples",
  title = "Space quest recipe",
  purpose = "Compact example recipe/config metadata for Batch 021.",
  capabilities = { "examples.recipe.space_quest" },
  input_schema = {},
  output_schema = {},
  config_schema = {},
  deterministic = true,
  runtime_targets = { "editor", "simulation", "validation" },
  unsafe_features = {}
}

M.recipe = {
  id = "recipe/space_quest/v1",
  title = "Compact Space Quest Adventure",
  purpose = "Recipe for a small multi-location space adventure with dialogue, quest stages, inventory, faction favor, UI and Unity target IR references as metadata only.",
  game_idea = {
    genre = "space_adventure_quest",
    pitch = "A courier ship uncovers a station conspiracy through talk, scanning, item recovery and reputation choices.",
    scope = "three locations, two NPCs, one quest chain, three items and two reputation tracks",
  },
  selected_capabilities = {
    "world.blueprint.generate",
    "npc.archetype.generate",
    "dialogue.fact_based.generate",
    "quest.investigation.generate",
    "item.schema.normalize",
    "progression.track.generate",
    "ui.quest_journal.generate",
    "unity.runtime_plan.generate",
    "validation.quest.validate",
  },
  selected_modules = {
    "world/world_blueprint/v1",
    "world/region_graph/v1",
    "npc/npc_archetype_generator/v1",
    "dialogue/fact_based_dialogue/v1",
    "dialogue/dialogue_combat/v1",
    "quest/simple_investigation/v1",
    "item/item_schema/v1",
    "item/inventory_rules/v1",
    "progression/progress_track/v1",
    "ui/quest_journal_ui/v1",
    "ui/hud_layout/v1",
    "unity/unity_runtime_plan/v1",
    "unity/unity_scene_ir/v1",
    "validation/quest_validation/v1",
    "validation/interaction_validation/v1",
  },
  generator_plan = {
    id = "plan/space_quest/v1",
    steps = {
      {
        id = "step/locations",
        module_id = "world/world_blueprint/v1",
      },
      {
        id = "step/npcs",
        module_id = "npc/npc_archetype_generator/v1",
        depends_on = {
          "step/locations",
        },
      },
      {
        id = "step/dialogue",
        module_id = "dialogue/fact_based_dialogue/v1",
        depends_on = {
          "step/npcs",
        },
      },
      {
        id = "step/quest",
        module_id = "quest/simple_investigation/v1",
        depends_on = {
          "step/dialogue",
        },
      },
      {
        id = "step/items",
        module_id = "item/item_schema/v1",
        depends_on = {
          "step/quest",
        },
      },
      {
        id = "step/unity_target_metadata",
        module_id = "unity/unity_runtime_plan/v1",
        depends_on = {
          "step/locations",
          "step/quest",
        },
      },
      {
        id = "step/quest_validation",
        module_id = "validation/quest_validation/v1",
        depends_on = {
          "step/quest",
        },
      },
    },
  },
  example_configs = {
    locations = {
      "orbital_station",
      "cargo_bay",
      "relay_moon",
    },
    npc_roles = {
      "station_admin",
      "smuggler_contact",
    },
    quest = {
      stages = {
        "receive_signal",
        "recover_core",
        "choose_disclosure",
      },
      condition_style = "declarative",
    },
    items = {
      "encrypted_core",
      "repair_kit",
      "station_pass",
    },
    progression = {
      "station_trust",
      "smuggler_favor",
      "suspicion",
    },
    unity_target_ir = {
      scenes = 3,
      adapter = "future_metadata_only",
    },
  },
  expected_artifacts = {
    {
      id = "artifact/space/locations",
      kind = "world_blueprint",
      produced_by = "step/locations",
    },
    {
      id = "artifact/space/dialogue",
      kind = "dialogue_ir",
      produced_by = "step/dialogue",
    },
    {
      id = "artifact/space/quest",
      kind = "quest_ir",
      produced_by = "step/quest",
    },
    {
      id = "artifact/space/unity_target",
      kind = "unity_ir_metadata",
      produced_by = "step/unity_target_metadata",
    },
  },
  validation_plan = {
    modules = {
      "validation/quest_validation/v1",
      "validation/interaction_validation/v1",
      "validation/module_contract_validation/v1",
    },
    expected_diagnostics_shape = {
      "severity",
      "code",
      "message",
      "target",
    },
    must_check = {
      "quest_stage_refs",
      "item_refs",
      "dialogue_refs",
      "unity_metadata_refs",
    },
  },
  context_pack_hints = {
    purpose = "space quest compact planning",
    token_budget = 7000,
    include_modules = {
      "quest/simple_investigation/v1",
      "dialogue/fact_based_dialogue/v1",
      "item/item_schema/v1",
      "unity/unity_runtime_plan/v1",
    },
    exclude = {
      "full_dialogue_script",
      "generated_cs",
      "runtime_scene_data",
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
