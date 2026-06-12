-- Batch 008 manual examples.
-- This file is intentionally dependency-free. It does not load external files.
-- To run manually in a sandbox, bind module tables to these local names after loading them through the host importer.

local examples = {}

examples.entity_input = {
  prototypes = {
    {
      id = "entity/npc/elder",
      kind = "npc",
      title = "Village Elder",
      tags = { "npc", "quest" },
      components = {
        interactable = { actions = { "talk", "inspect" }, prompt = "Talk", priority = 10 },
        dialogue_source = {
          dialogue_id = "dialogue/npc/elder",
          speaker_name = "Elder Mara",
          opening_node_id = "start",
          supports_dialogue_combat = true
        },
        inspectable = {
          title = "Village Elder",
          summary = "An exhausted elder watching the blocked road.",
          reveals_facts = { "fact/elder_worried" }
        },
        quest_target = { quest_id = "quest/investigate_road", objective_id = "talk_elder", role = "giver" },
        collidable = { blocks_movement = true }
      }
    },
    {
      id = "entity/object/blocked_gate",
      kind = "object",
      title = "Blocked Gate",
      tags = { "barrier", "road" },
      components = {
        interactable = { actions = { "inspect" }, prompt = "Inspect", priority = 4 },
        inspectable = {
          title = "Blocked Gate",
          summary = "Fresh stones block the village road.",
          reveals_facts = {
            { id = "fact/road_recently_blocked", hidden = false },
            { id = "fact/hidden_tool_marks", hidden = true }
          }
        },
        collidable = { blocks_movement = true }
      }
    }
  },
  instances = {
    { id = "entity/npc/elder/main", prototype_id = "entity/npc/elder", x = 2, y = 1, facing = "south" },
    { id = "entity/object/blocked_gate/main", prototype_id = "entity/object/blocked_gate", x = 2, y = 0, facing = "south" }
  }
}

examples.entity_ctx = {
  config = {
    default_facing = "south",
    require_position = true,
    max_prototypes = 16,
    max_instances = 32
  }
}

examples.targeting_input = {
  actor = { id = "entity/player/main", x = 2, y = 2, facing = "north" },
  requested_action = "talk",
  entities = {
    {
      id = "entity/npc/elder/main",
      prototype_id = "entity/npc/elder",
      title = "Village Elder",
      x = 2,
      y = 1,
      components = {
        interactable = { actions = { "talk", "inspect" }, priority = 10 },
        dialogue_source = { dialogue_id = "dialogue/npc/elder", speaker_name = "Elder Mara" }
      }
    },
    {
      id = "entity/object/blocked_gate/main",
      prototype_id = "entity/object/blocked_gate",
      title = "Blocked Gate",
      x = 2,
      y = 0,
      components = {
        interactable = { actions = { "inspect" }, priority = 4 },
        inspectable = { title = "Blocked Gate", summary = "Fresh stones block the road." }
      }
    }
  }
}

examples.targeting_ctx = {
  config = {
    mode = "facing_cell",
    required_component = "interactable",
    disambiguation = "nearest"
  }
}

examples.inspect_input = {
  actor = { id = "entity/player/main" },
  target = {
    id = "entity/object/blocked_gate/main",
    title = "Blocked Gate",
    components = {
      inspectable = {
        title = "Blocked Gate",
        summary = "Fresh stones block the village road.",
        reveals_facts = { "fact/road_recently_blocked" }
      }
    }
  }
}

examples.inspect_ctx = {
  config = {
    max_summary_length = 120,
    allow_hidden_facts = false,
    ui_mode = "minimal_hud"
  }
}

examples.talk_input = {
  actor = { id = "entity/player/main" },
  target = {
    id = "entity/npc/elder/main",
    title = "Village Elder",
    components = {
      dialogue_source = {
        dialogue_id = "dialogue/npc/elder",
        speaker_name = "Elder Mara",
        opening_node_id = "start",
        supports_dialogue_combat = true
      }
    }
  },
  dialogue_state = {
    quest_id = "quest/investigate_road",
    stage_id = "ask_elder"
  }
}

examples.talk_ctx = {
  config = {
    allow_dialogue_combat = true,
    ui_mode = "dialogue_focus"
  }
}

function examples.run(entity_factory, targeting, inspect_object, talk_to_npc)
  local results = {}
  results.entities = entity_factory.generate(examples.entity_input, examples.entity_ctx)
  results.targeting = targeting.generate(examples.targeting_input, examples.targeting_ctx)
  results.inspect = inspect_object.generate(examples.inspect_input, examples.inspect_ctx)
  results.talk = talk_to_npc.generate(examples.talk_input, examples.talk_ctx)
  return results
end

return examples
