local examples = {}

examples.faction_model_input = {
  config = {
    default_relation = "neutral"
  },
  roles = {
    {
      id = "role/village_guard",
      title = "Village guard",
      capabilities = {
        "npc.guard.patrol",
        "combat.defend_area"
      }
    },
    {
      id = "role/healer",
      title = "Healer",
      capabilities = {
        "npc.service.heal",
        "dialogue.advice"
      }
    }
  },
  factions = {
    {
      id = "faction/village",
      title = "Village",
      roles = {
        "role/village_guard",
        "role/healer"
      },
      reputation_track_id = "progress/village_reputation"
    },
    {
      id = "faction/raiders",
      title = "Raiders",
      roles = {
        "role/raider"
      },
      tags = {
        "hostile"
      }
    }
  },
  relations = {
    {
      from = "faction/village",
      to = "faction/raiders",
      state = "hostile"
    },
    {
      from = "faction/raiders",
      to = "faction/village",
      state = "hostile"
    }
  }
}

examples.pathfinding_config_input = {
  config = {
    default_grid = "orthogonal_4",
    default_replan_policy = "on_blocked"
  },
  profiles = {
    {
      id = "pathfinding/profile/humanoid",
      title = "Humanoid walking profile",
      turn_modes = {
        "realtime",
        "turn_based",
        "mixed"
      },
      movement_costs = {
        road = 1,
        grass = 2,
        swamp = 4
      },
      passable_tags = {
        "ground",
        "bridge"
      },
      blocked_tags = {
        "wall",
        "deep_water"
      },
      dynamic_obstacle_classes = {
        "actor",
        "door"
      },
      avoidance = {
        prefer_roads = true,
        avoid_hostile_factions = true
      }
    }
  },
  dynamic_obstacles = {
    {
      id = "obstacle/npc_actor",
      obstacle_class = "actor",
      blocks_movement = true,
      expires = "on_update"
    }
  }
}

examples.npc_archetype_input = {
  config = {
    default_behavior = "static",
    default_pathfinding_profile_id = "pathfinding/profile/humanoid"
  },
  archetypes = {
    {
      id = "npc/archetype/village_guard",
      title = "Village guard",
      role = "guard",
      behavior = "scheduled",
      faction_id = "faction/village",
      faction_role_id = "role/village_guard",
      schedule_id = "schedule/village_guard_day",
      dialogue_id = "dialogue/village_guard",
      home_location_id = "location/village_gate",
      quest_target = true,
      tags = {
        "guard",
        "village"
      }
    },
    {
      id = "npc/archetype/old_healer",
      title = "Old healer",
      behavior = "static",
      faction_id = "faction/village",
      faction_role_id = "role/healer",
      dialogue_id = "dialogue/old_healer",
      home_location_id = "location/healer_hut",
      tags = {
        "healer"
      }
    }
  }
}

examples.schedule_input = {
  config = {
    default_time_unit = "day_phase",
    default_loop = "daily"
  },
  schedules = {
    {
      id = "schedule/village_guard_day",
      owner_archetype_id = "npc/archetype/village_guard",
      entries = {
        {
          kind = "patrol",
          start = "morning",
          finish = "afternoon",
          location_id = "location/village_gate",
          path_goal_id = "path_goal/village_gate_patrol",
          action = "patrol_gate",
          can_talk = true
        },
        {
          kind = "work",
          start = "afternoon",
          finish = "evening",
          location_id = "location/watchtower",
          action = "watch_road",
          can_talk = true
        },
        {
          kind = "sleep",
          start = "night",
          finish = "morning",
          location_id = "location/barracks",
          action = "sleep",
          can_talk = false
        }
      }
    }
  }
}

examples.expected_shapes = {
  faction_model = {
    ok = true,
    data_keys = {
      "factions",
      "roles",
      "relations",
      "indexes"
    }
  },
  pathfinding_config = {
    ok = true,
    data_keys = {
      "defaults",
      "profiles",
      "dynamic_obstacles",
      "indexes"
    }
  },
  npc_archetypes = {
    ok = true,
    data_keys = {
      "npc_archetypes",
      "indexes",
      "references"
    }
  },
  schedules = {
    ok = true,
    data_keys = {
      "schedules",
      "indexes",
      "pathfinding_goals"
    }
  }
}

return examples
