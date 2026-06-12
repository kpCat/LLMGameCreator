local examples = {}

examples.quest_schema_input = {
  quests = {
    {
      id = "quest/investigate_old_road",
      title = "Investigate the Old Road",
      status = "inactive",
      start_stage_id = "accept",
      stages = {
        {
          id = "accept",
          title = "Accept the request",
          objectives = {
            {
              id = "talk_to_elder",
              type = "talk_to",
              title = "Talk to the elder",
              target_entity_id = "entity/npc/elder",
              completion_conditions = {
                { type = "dialogue_choice_selected", target = "quest/investigate_old_road/accept" }
              }
            }
          },
          transitions = {
            { id = "accepted", to_stage = "inspect", conditions = { { type = "objective_complete", objective_id = "talk_to_elder" } } }
          }
        },
        {
          id = "inspect",
          title = "Inspect the road",
          objectives = {
            {
              id = "inspect_cart",
              type = "inspect",
              title = "Inspect the broken cart",
              target = "entity/object/broken_cart",
              completion_conditions = {
                { type = "interaction_happened", target = "entity/object/broken_cart" }
              }
            }
          },
          transitions = {
            { id = "inspected", to_stage = "complete", conditions = { { type = "objective_complete", objective_id = "inspect_cart" } } }
          }
        },
        {
          id = "complete",
          title = "Complete",
          objectives = {},
          transitions = {}
        }
      },
      triggers = {
        {
          id = "start_from_elder_dialogue",
          type = "dialogue_choice",
          source_id = "entity/npc/elder",
          choice_id = "quest/investigate_old_road/accept",
          target_stage = "accept"
        }
      },
      progress_tracks = {
        {
          id = "quest/investigate_old_road/investigation",
          title = "Investigation progress",
          kind = "abstract_progress",
          min = 0,
          max = 2,
          starts_at = 0
        }
      },
      completion_conditions = { { type = "stage_active", stage_id = "complete" } },
      effects = { { type = "set_flag", target = "road/investigated", value = true } },
      tags = { "investigation", "tutorial" }
    }
  }
}

examples.simple_investigation_input = {
  quest_id = "quest/investigate_old_road",
  title = "Investigate the Old Road",
  giver_id = "entity/npc/elder",
  suspect_id = "entity/npc/bandit_scout",
  clue_targets = {
    {
      id = "broken_cart",
      title = "Inspect the broken cart",
      target = "entity/object/broken_cart",
      fact_id = "fact/cart_was_attacked"
    },
    {
      id = "muddy_tracks",
      title = "Read the muddy tracks",
      target_location_id = "world/location/muddy_tracks",
      fact_id = "fact/tracks_go_north"
    }
  },
  facts = { "fact/cart_was_attacked", "fact/tracks_go_north" },
  reward_effects = { { type = "add_progress", target = "reputation/village", amount = 1 } }
}

examples.simple_investigation_config = {
  default_clue_count = 2,
  require_report_back = true,
  include_progress_track = true
}

examples.fetch_quest_input = {
  quest_id = "quest/bring_healing_herbs",
  title = "Bring Healing Herbs",
  giver_id = "entity/npc/healer",
  item_id = "item/resource/healing_herb",
  count = 3,
  delivery_target_id = "entity/npc/healer",
  reward_effects = { { type = "add_progress", target = "reputation/village", amount = 1 } }
}

examples.fetch_quest_config = {
  allow_partial_progress = true,
  require_return = true,
  remove_items_on_complete = true
}

examples.location_discovery_input = {
  quest_id = "quest/find_old_shrine",
  title = "Find the Old Shrine",
  location_id = "world/location/old_shrine",
  hint_source_id = "entity/object/weathered_map",
  landmark_id = "entity/object/shrine_gate",
  report_target_id = "entity/npc/elder",
  reward_effects = { { type = "set_flag", target = "world/old_shrine_known", value = true } }
}

examples.location_discovery_config = {
  require_hint = true,
  require_inspection = true,
  require_report_back = true
}

examples.expected_shapes = {
  quest_schema = {
    ok = true,
    data_contains = { "quests", "summary" }
  },
  simple_investigation = {
    ok = true,
    stages = { "accept", "gather_clues", "confront", "complete" }
  },
  fetch_quest = {
    ok = true,
    stages = { "accepted", "collect", "deliver", "complete" }
  },
  location_discovery = {
    ok = true,
    stages = { "hint", "discover", "inspect", "report", "complete" }
  }
}

return examples
