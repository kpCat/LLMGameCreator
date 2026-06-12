-- Batch 011 manual examples.
-- This file is intentionally dependency-injected: pass module tables explicitly.
-- It does not load files by itself.

local M = {}

local function assert_true(value, message)
  if not value then
    return { ok = false, message = message }
  end
  return { ok = true }
end

local function first_error(results)
  for index = 1, #results do
    if results[index].ok == false then
      return results[index]
    end
  end
  return { ok = true }
end

function M.run(modules)
  local results = {}
  if type(modules) ~= "table" then
    return { ok = false, diagnostics = { { severity = "error", code = "test.modules_missing", message = "modules table is required" } } }
  end

  local item_schema = modules.item_schema
  local item_catalog_generator = modules.item_catalog_generator
  local loot_table_generator = modules.loot_table_generator
  local inventory_rules = modules.inventory_rules

  results[#results + 1] = assert_true(type(item_schema) == "table", "item_schema module is required")
  results[#results + 1] = assert_true(type(item_catalog_generator) == "table", "item_catalog_generator module is required")
  results[#results + 1] = assert_true(type(loot_table_generator) == "table", "loot_table_generator module is required")
  results[#results + 1] = assert_true(type(inventory_rules) == "table", "inventory_rules module is required")
  local early = first_error(results)
  if not early.ok then
    return { ok = false, diagnostics = { { severity = "error", code = "test.missing_module", message = early.message } } }
  end

  local catalog_result = item_catalog_generator.generate({
    families = {
      {
        namespace = "item/herb",
        slug = "forest_herb",
        type = "material",
        tags = { "herb", "alchemy", "forest" },
        stackable = true,
        stack_limit = 25,
        base_value = 2,
        variants = {
          { id = "sunleaf", name = "Sunleaf", tags = { "healing" } },
          { id = "mooncap", name = "Mooncap", tags = { "focus" } }
        },
        rarities = { "common", "rare" }
      },
      {
        namespace = "item/weapon",
        slug = "blade",
        type = "equipment",
        tags = { "weapon", "metal" },
        base_value = 10,
        base_weight = 3,
        equipment = { slot = "main_hand", modifiers = { attack = 2 } },
        durability = { max = 40, break_behavior = "disable_effects" },
        variants = {
          { id = "iron_sword", name = "Iron Sword" }
        },
        rarities = { "common" }
      }
    }
  }, {
    config = {
      max_items = 8,
      default_rarities = {
        { id = "common", value_multiplier = 1 },
        { id = "rare", value_multiplier = 4, durability_bonus = 8 }
      }
    }
  })

  results[#results + 1] = assert_true(catalog_result.ok == true, "catalog generation should succeed")
  results[#results + 1] = assert_true(#catalog_result.data.items == 5, "catalog should generate five items")

  local schema_result = item_schema.generate({
    items = catalog_result.data.items
  }, {
    config = {
      default_stack_limit = 50,
      allowed_rarities = { "common", "rare", "quest" },
      allowed_item_types = { "material", "equipment", "quest" },
      allowed_equipment_slots = { "main_hand", "off_hand", "two_hand", "utility" }
    }
  })

  results[#results + 1] = assert_true(schema_result.ok == true, "item schema normalization should succeed")
  results[#results + 1] = assert_true(schema_result.data.indexes.by_tag.herb ~= nil, "herb tag index should exist")

  local loot_result = loot_table_generator.generate({
    item_catalog = schema_result.data.items,
    pools = {
      {
        id = "loot/forest/herbs",
        rolls = 2,
        filters = {
          { tags_any = { "herb" }, weight = 3, quantity = { min = 1, max = 2 } }
        }
      },
      {
        id = "loot/quest/old_gate",
        guaranteed = {
          { item_id = "item/key/old_gate", quantity = 1 }
        },
        entries = {}
      }
    }
  }, {
    config = { max_tables = 4, max_entries_per_pool = 16 }
  })

  results[#results + 1] = assert_true(loot_result.ok == true, "loot table generation should succeed")
  results[#results + 1] = assert_true(#loot_result.data.loot_tables == 2, "two loot pools should be generated")

  local inventory_result = inventory_rules.generate({
    item_catalog = schema_result.data.items,
    containers = {
      { id = "inventory/player/main", capacity_slots = 24, weight_limit = 80 },
      { id = "inventory/player/key_items", capacity_slots = 12, accepts_tags = { "key", "quest" } }
    }
  }, {
    config = {
      strict_quest_items = true,
      allowed_equipment_slots = { "main_hand", "off_hand", "two_hand", "utility" }
    }
  })

  results[#results + 1] = assert_true(inventory_result.ok == true, "inventory rules generation should succeed")
  results[#results + 1] = assert_true(#inventory_result.data.inventory_rules.containers == 2, "two containers should be normalized")
  results[#results + 1] = assert_true(#inventory_result.data.inventory_rules.item_rules == #schema_result.data.items, "item rules should match normalized items")

  local failed = first_error(results)
  if not failed.ok then
    return {
      ok = false,
      diagnostics = { { severity = "error", code = "test.assertion_failed", message = failed.message } }
    }
  end

  return {
    ok = true,
    diagnostics = {},
    data = {
      generated_items = #catalog_result.data.items,
      normalized_items = #schema_result.data.items,
      loot_tables = #loot_result.data.loot_tables,
      containers = #inventory_result.data.inventory_rules.containers
    }
  }
end

return M
