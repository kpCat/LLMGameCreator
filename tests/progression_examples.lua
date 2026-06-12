local examples = {}

examples.formula_schema = {
  module_id = "formula/formula_schema/v1",
  config = {
    max_depth = 6,
    allowed_value_refs = { "stats/", "progression/" }
  },
  input = {
    formulas = {
      {
        id = "formula/stats/max_health",
        result_stat = "stats/max_health",
        tags = { "rpg", "attribute" },
        expression = {
          op = "add",
          args = {
            { op = "const", value = 50 },
            { op = "mul", args = {
              { op = "ref", ref = "stats/endurance" },
              { op = "const", value = 5 }
            } }
          }
        }
      }
    }
  },
  expected_shape = {
    ok = true,
    data = { formulas = "array", indexes = "table", summary = "table" }
  }
}

examples.xp_curve = {
  module_id = "progression/xp_curve/v1",
  config = { max_level_limit = 20 },
  input = {
    curve = {
      id = "progression/xp/hero",
      mode = "quadratic",
      base = 100,
      growth = 30,
      max_level = 5
    }
  },
  expected_notes = {
    "levels contains deterministic threshold rows",
    "formula_ref can be referenced from formula IR"
  }
}

examples.skill_tree = {
  module_id = "progression/skill_tree_generator/v1",
  config = {
    max_nodes = 16,
    default_currency = "progression/skill_points"
  },
  input = {
    tree = {
      id = "progression/skill_tree/ranger",
      title = "Ranger skill tree",
      branches = {
        {
          id = "combat",
          nodes = {
            { id = "skill/ranger/focus", title = "Focus", cost = 1 },
            {
              id = "skill/ranger/piercing_shot",
              title = "Piercing Shot",
              cost = 2,
              requires = { "skill/ranger/focus" },
              effects = {
                { type = "formula_ref", target = "stats/ranged_damage", formula_id = "formula/combat/piercing_shot_bonus" }
              }
            }
          }
        },
        {
          id = "dialogue",
          nodes = {
            { id = "skill/ranger/calm_voice", title = "Calm Voice", cost = 1, tags = { "dialogue_combat" } }
          }
        }
      }
    }
  },
  expected_notes = {
    "nodes are compact skill IR, not runtime unlock state",
    "edges describe prerequisite graph"
  }
}

examples.progress_tracks = {
  module_id = "progression/progress_track/v1",
  config = {
    allowed_domains = { "reputation", "research", "faction_favor", "suspicion", "morale", "trust" }
  },
  input = {
    tracks = {
      {
        id = "progression/track/village_reputation",
        domain = "reputation",
        min = -100,
        max = 100,
        initial = 0,
        stages = {
          { id = "hostile", threshold = -50, label = "Hostile" },
          { id = "neutral", threshold = 0, label = "Neutral" },
          { id = "trusted", threshold = 50, label = "Trusted" }
        },
        source_refs = { "quest/investigate_road" }
      },
      {
        id = "progression/track/guard_suspicion",
        domain = "suspicion",
        polarity = "negative",
        min = 0,
        max = 100,
        initial = 10,
        stages = {
          { id = "calm", threshold = 0, label = "Calm" },
          { id = "alert", threshold = 40, label = "Alert" },
          { id = "hostile", threshold = 80, label = "Hostile" }
        }
      }
    }
  },
  expected_notes = {
    "tracks can be used by dialogue, quest and UI IR",
    "no XP-only assumption"
  }
}

return examples
