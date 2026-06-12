local examples = {}

function examples.run(modules)
  local results = {}

  local world_validation = modules.world_validation
  local quest_validation = modules.quest_validation
  local interaction_validation = modules.interaction_validation
  local module_contract_validation = modules.module_contract_validation

  results.valid_world = world_validation.generate({
    world = {
      id = "world/frontier",
      regions = {
        { id = "region/town" },
        { id = "region/forest" }
      },
      chunks = {
        { id = "chunk/town_center", region_id = "region/town", walkability = { mode = "sparse" } },
        { id = "chunk/forest_edge", region_id = "region/forest", walkability = { mode = "sparse" } }
      },
      graph = {
        nodes = {
          { id = "node/town" },
          { id = "node/forest" }
        },
        edges = {
          { from = "node/town", to = "node/forest" }
        }
      }
    },
    starts = {
      { id = "start/player", node_id = "node/town" }
    },
    objectives = {
      { id = "objective/reach_forest", node_id = "node/forest" }
    }
  }, { config = { max_nodes = 16 } })

  results.invalid_world = world_validation.generate({
    world = {
      id = "world/frontier",
      graph = {
        nodes = {
          { id = "node/start" },
          { id = "node/blocked" }
        },
        edges = {
          { from = "node/start", to = "node/blocked", blocked = true, bridge_id = "bridge/missing" }
        }
      }
    },
    starts = {
      { id = "start/player", node_id = "node/start" }
    },
    objectives = {
      { id = "objective/blocked", node_id = "node/blocked" }
    }
  }, {})

  results.invalid_quest = quest_validation.generate({
    quests = {
      {
        id = "quest/find_water",
        start_stage_id = "stage/start",
        stages = {
          { id = "stage/start", objective_ids = { "objective/find_well" } }
        },
        objectives = {
          {
            id = "objective/find_well",
            type = "inspect",
            target_ref = "entity/well",
            completion_conditions = {
              { type = "unknown_condition", ref = "entity/well" }
            }
          }
        },
        transitions = {
          { from_stage_id = "stage/start", to_stage_id = "stage/missing" }
        }
      }
    }
  }, {})

  results.invalid_interaction = interaction_validation.generate({
    interactions = {
      {
        id = "interaction/talk_guard",
        kind = "talk",
        dialogue_id = "dialogue/guard_intro"
      }
    },
    registry = {
      dialogue_id = {}
    }
  }, {})

  results.missing_module_dependency = module_contract_validation.generate({
    modules = {
      {
        id = "validation/example/v1",
        path = "lua/validation/example.lua",
        category = "validation",
        capabilities = { "validation.example.validate" },
        depends_on = { "missing/module/v1" },
        runtime_targets = { "validation" },
        supported_turn_modes = { "realtime" },
        supported_combat_modes = { "none" },
        deterministic = true,
        unsafe_features = {}
      }
    }
  }, {})

  return results
end

return examples
