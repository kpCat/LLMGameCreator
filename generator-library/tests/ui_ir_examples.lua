local examples = {}

examples.valid_ui_schema_input = {
  panels = {
    {
      id = "ui/root",
      kind = "panel",
      anchor = "fill",
      size = { width = 100, height = 100 },
      visibility = "always"
    }
  },
  elements = {
    {
      id = "ui/example_button",
      kind = "button",
      anchor = "bottom_right",
      size = { width = 12, height = 4 },
      bindings = { text = "ui.example_button.text" },
      actions = { click = "ui.example_button.click" }
    }
  }
}

examples.rpg_hud_input = {
  hud_mode = "rpg_hud",
  quick_slot_count = 6,
  status_bars = {
    { id = "hud/health", label = "Health", binding = "actor.health", max_binding = "actor.health_max" },
    { id = "hud/mana", label = "Mana", binding = "actor.mana", max_binding = "actor.mana_max" }
  },
  stat_bars = {
    { id = "hud/xp", label = "XP", binding = "progression.xp" }
  }
}

examples.automation_hud_input = {
  hud_mode = "automation_hud",
  build_menu = {
    compact = true,
    categories = { "production", "logistics", "power" }
  }
}

examples.minimap_input = {
  map_mode = "both",
  world_scale = "region",
  layers = {
    { id = "map/layer_terrain", label = "Terrain", source = "world.terrain", enabled = true },
    { id = "map/layer_regions", label = "Regions", source = "world.regions", enabled = true }
  },
  marker_categories = {
    { id = "map/marker_player", label = "Player", binding = "actor.player.position", visible_by_default = true },
    { id = "map/marker_quest", label = "Quest", binding = "quest.tracked.locations", visible_by_default = true }
  }
}

examples.inventory_input = {
  mode = "grid",
  slot_count = 20,
  equipment_slots = {
    { id = "equipment/head", label = "Head", accepts = { "armor_head" } },
    { id = "equipment/body", label = "Body", accepts = { "armor_body" } },
    { id = "equipment/weapon", label = "Weapon", accepts = { "weapon" } }
  },
  categories = {
    { id = "inventory/category_all", label = "All", tags = {} },
    { id = "inventory/category_quest", label = "Quest", tags = { "quest" } }
  },
  display = {
    show_stack = true,
    show_durability = true,
    show_rarity = true,
    show_description = true
  }
}

examples.quest_journal_input = {
  objective_layout = "cards",
  tracked_quest_limit = 3,
  sections = {
    { id = "journal/active_quests", label = "Active", source = "quest.active" },
    { id = "journal/completed_quests", label = "Completed", source = "quest.completed" },
    { id = "journal/notes", label = "Notes", source = "notes.entries" }
  },
  notes = {
    allow_player_notes = true,
    allow_codex_entries = true
  }
}

examples.invalid_inventory_input = {
  mode = "grid",
  slot_count = -4,
  equipment_slots = {
    { id = "equipment/weapon", label = "Weapon" },
    { id = "equipment/weapon", label = "Duplicate Weapon" }
  }
}

function examples.run_examples(modules)
  local results = {}
  results.ui_schema = modules.ui_schema.generate(examples.valid_ui_schema_input, {})
  results.rpg_hud = modules.hud_layout.generate(examples.rpg_hud_input, {})
  results.automation_hud = modules.hud_layout.generate(examples.automation_hud_input, {})
  results.minimap = modules.minimap_config.generate(examples.minimap_input, {})
  results.inventory = modules.inventory_ui.generate(examples.inventory_input, {})
  results.quest_journal = modules.quest_journal_ui.generate(examples.quest_journal_input, {})
  results.invalid_inventory = modules.inventory_ui.generate(examples.invalid_inventory_input, {})
  return results
end

examples.expected_shapes = {
  ui_schema = { ok = true, data = "schema_version, panels, elements, bindings, actions" },
  rpg_hud = { ok = true, data = "hud_mode, panels, elements, screen_regions" },
  automation_hud = { ok = true, data = "build menu style HUD IR" },
  minimap = { ok = true, data = "map panels, layers, marker categories, reveal metadata" },
  inventory = { ok = true, data = "inventory panels, item slots, equipment slots, filters" },
  quest_journal = { ok = true, data = "journal panels, sections, tracked quest metadata" },
  invalid_inventory = { ok = false, diagnostics = "invalid slot count and duplicate equipment slot" }
}

return examples
