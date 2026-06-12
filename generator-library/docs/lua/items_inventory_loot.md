# Batch 011 — Items, inventory and loot

## Purpose

This batch introduces a compact item/inventory/loot foundation for AI Game Builder / LLMGameCreator. It does not print huge item encyclopedias. It provides reusable Lua generator modules that create and validate item IR, catalog IR, loot table IR and inventory rules IR.

The batch is designed for RPG, adventure, survival, city-builder, automation and mixed-mode games where items may be used by quests, interactions, combat, UI, economy, crafting or future Unity adapters.

## Files

- `lua/item/item_schema.lua`
- `lua/item/item_catalog_generator.lua`
- `lua/item/loot_table_generator.lua`
- `lua/item/inventory_rules.lua`
- `docs/lua/items_inventory_loot.md`
- `manifests/items_inventory.manifest.json`
- `tests/items_inventory_examples.lua`
- `BATCH_011_REPORT.md`

## Module: item_schema

### Purpose

Normalizes hand-authored or generated item definitions into compact item IR.

### When to use

Use it after an LLM, generator plan or content module proposes item definitions. It is the first validation/normalization layer before inventory, loot, UI, quest or combat systems consume item data.

### When not to use

Do not use it for runtime inventory mutation or random loot resolution. This module defines item contracts only.

### Manifest summary

- id: `item/item_schema/v1`
- capabilities: `item.schema.normalize`, `item.validate`, `inventory.item_contract`
- deterministic: true
- runtime targets: debug, unity2d, unity3d, unity_ui_ir

### Input schema explained

`input.items` is an array of item definitions. A single `input.item` is also accepted.

Each item may include:

- `id`: lowercase slash id, for example `item/herb/sunleaf`
- `type`: `material`, `consumable`, `quest`, `equipment`, `tool`, `key`, `currency`, `note`
- `name`
- `rarity`
- `tags`
- `stackable` / `stack_limit`
- `quest_item`
- `equipment`
- `durability`
- `value`
- `weight`
- `description_config`

### Config schema explained

- `allowed_rarities`: optional rarity whitelist.
- `allowed_item_types`: optional item type whitelist.
- `allowed_equipment_slots`: optional equipment slot whitelist.
- `default_stack_limit`: default stack limit for stackable items.
- `max_tags_per_item`: safety cap for tag arrays.

### Output schema explained

The module returns:

- `items`: normalized compact item records.
- `indexes.by_id`: direct lookup table.
- `indexes.by_tag`: tag to item ids.
- `indexes.by_rarity`: rarity to item ids.
- `indexes.by_equipment_slot`: slot to item ids.
- `summary`: count metadata.

### Example config

```lua
{
  default_stack_limit = 50,
  allowed_rarities = { "common", "uncommon", "rare", "quest" }
}
```

### Example input

```lua
{
  items = {
    {
      id = "item/herb/sunleaf",
      type = "material",
      name = "Sunleaf",
      rarity = "common",
      tags = { "herb", "alchemy" },
      stackable = true,
      stack_limit = 25
    }
  }
}
```

### Example output

```lua
{
  ok = true,
  data = {
    items = {
      {
        id = "item/herb/sunleaf",
        type = "material",
        rarity = "common",
        stack = { stackable = true, stack_limit = 25 }
      }
    }
  }
}
```

### LLM prompting hints

Ask the LLM for a small item set and explicit item roles, not for hundreds of items. The LLM should produce families, tags and purpose. This module will normalize the result.

### Validation rules

- ids must be lowercase slash ids;
- tags are deduplicated token strings;
- equipment items need equipment slot metadata;
- durability must be a valid range;
- quest items are marked but not deleted or consumed by default.

### Extension points

Future modules can add crafting, economy prices, combat formula references, UI icons, localization keys and Unity prefab slots.

### Runtime target notes

The module returns pure IR. Runtime code decides how to display, consume, equip or mutate item instances.

### Unity/codegen notes

Unity adapters can map `description_config`, `equipment.slot`, `tags`, `rarity` and `durability` into ScriptableObject-like data or generated JSON assets.

## Module: item_catalog_generator

### Purpose

Expands compact item families into reviewable item definitions.

### When to use

Use when a game design has item families like herbs, ores, swords, keys or tools and needs a small deterministic catalog.

### When not to use

Do not use it to generate a full live economy or crafting graph. It only emits item definitions.

### Manifest summary

- id: `item/item_catalog_generator/v1`
- capabilities: `item.catalog.generate`, `item.description_config.generate`, `item.prototype.expand`

### Input schema explained

`input.families` is an array. Each family may include namespace, slug, type, tags, variants, rarities, base value, equipment and durability config.

### Config schema explained

- `max_items`: hard cap.
- `default_rarities`: rarity tier list or table definitions.
- `default_stack_limit`: default stack limit for generated stackable items.

### Output schema explained

- `items`: generated compact item definitions compatible with `item_schema`.
- `catalog.generated_count`: number of emitted items.
- `catalog.family_summary`: generated count per family.

### Example config

```lua
{
  max_items = 12,
  default_rarities = {
    { id = "common", value_multiplier = 1 },
    { id = "rare", value_multiplier = 4, durability_bonus = 8 }
  }
}
```

### Example input

```lua
{
  families = {
    {
      namespace = "item/weapon",
      slug = "blade",
      type = "equipment",
      variants = { { id = "iron_sword", name = "Iron Sword" } },
      equipment = { slot = "main_hand" },
      durability = { max = 40 }
    }
  }
}
```

### LLM prompting hints

Ask for 2-4 families with clear variants. Let rarity tiers multiply variants only where needed.

### Validation rules

- family namespace must be a lowercase slash id;
- family and variant slugs must be tokens;
- generation stops at `max_items`;
- output remains compact and reviewable.

### Extension points

Future automation/crafting batches can consume family tags and item ids to create recipe graphs.

### Runtime target notes

The generated catalog is static data. Runtime inventory should consume normalized item schema output.

### Unity/codegen notes

The generated catalog can be imported into Unity as item data assets after manifest validation.

## Module: loot_table_generator

### Purpose

Creates loot table IR with pools, guaranteed drops, weighted entries and catalog filters.

### When to use

Use when enemies, containers, biomes, locations or quest rewards need compact drop rules.

### When not to use

Do not use it as a runtime random resolver. It produces deterministic loot table definitions. Runtime systems can later resolve rolls using trusted RNG.

### Manifest summary

- id: `item/loot_table_generator/v1`
- capabilities: `item.loot_table.generate`, `item.loot_pool.validate`, `item.drop_rules.describe`

### Input schema explained

- `pools`: array of loot pool definitions.
- `item_catalog`: optional array used for tag/rarity/type filters.

A pool may include:

- `id`
- `rolls`
- `tags`
- `guaranteed`
- `entries`
- `filters`
- `empty_behavior`

### Config schema explained

- `max_tables`: cap for number of pools.
- `max_entries_per_pool`: cap after direct and filter expansion.
- `default_rolls`: default number of weighted rolls.

### Output schema explained

- `loot_tables`: normalized loot pool IR.
- `indexes.by_id`: pool lookup.
- `indexes.item_refs_by_pool`: pool-to-item references.

### Example input

```lua
{
  item_catalog = generated_items,
  pools = {
    {
      id = "loot/forest/herbs",
      rolls = 2,
      guaranteed = { { item_id = "item/key/old_gate", quantity = 1 } },
      filters = { { tags_any = { "herb" }, weight = 3 } }
    }
  }
}
```

### Validation rules

- pool ids and item ids must be lowercase slash ids;
- weights must be non-negative;
- total weight must be greater than zero for non-empty weighted pools;
- quantity ranges must be positive and ordered.

### Extension points

Future combat, biome, quest and economy modules can reference loot tables by id.

### Runtime target notes

Runtime resolver should own actual RNG and inventory mutation.

### Unity/codegen notes

Unity adapters can turn loot table IR into ScriptableObject-like assets or JSON tables.

## Module: inventory_rules

### Purpose

Defines inventory constraints and equipment slot rules.

### When to use

Use after item schema/catalog generation when the game needs rules for capacity, stack size, quest item locking, equipment slots or durability behavior.

### When not to use

Do not use it for runtime add/remove item simulation. It emits rule IR only.

### Manifest summary

- id: `item/inventory_rules/v1`
- capabilities: `inventory.rules.generate`, `inventory.constraints.validate`, `equipment.slot_rules.define`

### Input schema explained

- `item_catalog`: optional normalized items.
- `containers`: optional inventory containers.

### Config schema explained

- `default_capacity_slots`: fallback container size.
- `default_weight_limit`: `0` means no weight limit in this IR.
- `strict_quest_items`: quest items become locked.
- `allowed_equipment_slots`: slot whitelist.

### Output schema explained

- `inventory_rules.containers`
- `inventory_rules.item_rules`
- `inventory_rules.equipment_slots`
- `inventory_rules.constraints`
- `validation` indexes and summary

### Example config

```lua
{
  default_capacity_slots = 32,
  default_weight_limit = 80,
  strict_quest_items = true
}
```

### LLM prompting hints

Ask the LLM whether the game uses slots, weight, both, or neither. Do not let it invent runtime code.

### Validation rules

- containers need lowercase slash ids;
- equipment slots must be allowed;
- invalid stack limits are reduced to safe defaults;
- quest item locking is explicit.

### Extension points

Future UI IR can create inventory screens. Future combat can read equipment slot rules. Future economy can read item value and stack information.

### Runtime target notes

Runtime code should apply these rules when adding, removing, moving, equipping or consuming items.

### Unity/codegen notes

The rules map cleanly to Unity UI slot definitions and item container metadata.

## Manual validation

1. Inspect module manifests and confirm ids/capabilities.
2. Run each file in a Lua 5.4-compatible sandbox, if available.
3. Execute `tests/items_inventory_examples.lua` manually by injecting module tables.
4. Confirm no file system, network or dynamic code APIs are used.
5. Confirm outputs contain only JSON-serializable data.
