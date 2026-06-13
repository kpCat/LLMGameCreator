## Runtime Interpretation Notes

Gameplay runtime v1 interprets the optional economy lists under `game` without changing the package format:

- `inventories` are copied into `GameRuntimeState.Inventories`; if no player inventory exists, runtime creates an in-memory default player inventory;
- `resources` initialize runtime resource values from `defaultValue`, falling back to `minValue` or zero, and clamp by `minValue`/`maxValue`;
- `requirements`, `costs` and `outputs` are evaluated against runtime state, not by mutating package definitions;
- `recipes`, `lootTables`, `transactions` and `resourceNodes` are executed by headless runtime services;
- loot uses deterministic seeded random; the package format does not store random state;
- `stockLootTableId` and restock rules are data contracts only in runtime v1 and do not implement merchant restocking.

Runtime v1 does not execute runtime Lua, generator Lua, generator modules, LLM calls, Unity codegen or external asset providers.

# GamePackage format

GamePackage — переносимый пакет игры. Его должен читать WinForms Preview, headless runtime и будущий Unity Player.

## Рекомендуемая структура

```text
game/
  manifest.json
  package.json
  settings.json

  prototypes/
    tiles.lua
    items.lua
    npcs.lua
    abilities.lua
    resources.lua
    interactions.lua

  maps/
    village/
      map.json
      entities.json
      scripts.lua

  scripts/
    generators/
    interactions/
    behaviors/
    formulas/
    events/

  lualib/
    core.lua
    random.lua
    noise.lua
    chunks.lua
    tiles.lua
    entities.lua
    effects.lua
    interactions.lua
    loot.lua
    dialogue.lua
    quests.lua
    combat.lua
    validation_helpers.lua

  assets/
    tilesets/
    characters/
    portraits/
    icons/
    sounds/
    music/
    backgrounds/

  asset-catalog.json
  asset-contracts.json
  script-manifest.json

  generation/
    sessions/
    jobs/
    drafts/
    context-packs/

  cache/
    indexes.db

  saves/
```

## Source of truth

Source of truth:

- JSON definitions;
- Lua scripts;
- asset catalog;
- workflow profiles;
- docs.

SQLite/cache:

- indexes;
- save games;
- generated chunk state;
- generation job state;
- search cache;
- summaries.

## GamePackage должен быть frontend-agnostic

GamePackage не должен зависеть от WinForms или Unity.

Unity Player должен быть способен загрузить GamePackage без знания конкретной игры.

## Optional Economy Definition Lists

`game` may include these optional/default-empty lists. Older packages that omit them remain loadable:

```json
{
  "game": {
    "resources": [],
    "statuses": [],
    "stats": [],
    "progressions": [],
    "encounters": [],
    "abilities": [],
    "recipes": [],
    "lootTables": [],
    "transactions": [],
    "resourceNetworks": [],
    "resourceNodes": [],
    "inventories": [],
    "equipmentSlots": []
  }
}
```

`items` also supports optional economy metadata such as `kind`, `rarity`, `maxStack`, `value`, `weight`, `questItem`, `unique`, `maxDurability`, `maxCharge`, `ammoType`, `fuelType`, `cannotSell`, `cannotDrop`, `requirements` and `metadata`.

Example:

```json
{
  "items": [
    {
      "id": "item/red_herb",
      "name": "Red Herb",
      "kind": "material",
      "maxStack": 20,
      "tags": ["herb", "alchemy"]
    }
  ],
  "resources": [
    {
      "id": "resource/mana",
      "name": "Mana",
      "kind": "magic",
      "minValue": 0,
      "maxValue": 100,
      "regenPerTick": 1
    }
  ],
  "recipes": [
    {
      "id": "recipe/healing_potion",
      "name": "Healing Potion",
      "category": "alchemy",
      "inputs": [{ "kind": "item", "id": "item/red_herb", "amount": 2 }],
      "costs": [{ "kind": "resource", "id": "resource/mana", "amount": 5 }],
      "outputs": [{ "kind": "item", "id": "item/healing_potion", "amount": 1 }]
    }
  ],
  "resourceNetworks": [
    {
      "id": "network/base_power",
      "name": "Base Power Grid",
      "resourceId": "resource/electricity",
      "kind": "electricity"
    }
  ]
}
```

These definitions are contracts and validation inputs only. Crafting, shops and base resource simulation are not executed by the package format layer.

## Optional Encounter Definition Lists

`game.stats`, `game.progressions`, `game.encounters` and `game.abilities` are optional/default-empty. Older packages that omit them remain loadable.

Minimal encounter example:

```json
{
  "stats": [{ "id": "stat/strength", "name": "Strength", "kind": "attribute", "defaultValue": 5 }],
  "progressions": [{ "id": "progression/character_level", "name": "Character Level", "kind": "xp_level", "stages": [{ "id": "level/1", "name": "Level 1", "requiredAmount": 0 }] }],
  "abilities": [{ "id": "ability/basic_attack", "name": "Basic Attack", "kind": "attack", "power": 4, "resourceId": "resource/health" }],
  "encounters": [
    {
      "id": "encounter/goblin_duel",
      "name": "Goblin Duel",
      "kind": "combat",
      "participants": [
        { "id": "player", "name": "Player", "kind": "player", "team": "player", "resources": [{ "kind": "resource", "id": "resource/health", "amount": 30 }], "abilities": ["ability/basic_attack"] },
        { "id": "goblin", "name": "Goblin", "kind": "enemy", "team": "enemy", "resources": [{ "kind": "resource", "id": "resource/health", "amount": 12 }], "abilities": ["ability/basic_attack"] }
      ],
      "rewards": [{ "kind": "progression", "id": "progression/character_level", "amount": 10 }]
    }
  ]
}
```

## Exploration Inventory Fields

`game.equipmentSlots` is optional and defaults to an empty list:

```json
{
  "id": "slot/tool",
  "name": "Tool",
  "allowedTags": ["tool"],
  "allowedKinds": ["tool"],
  "requiredRequirements": []
}
```

Items can opt into equipment routing with metadata such as `equip_slot=slot/tool` or by using matching tags/kinds like `tool`, `weapon`, `armor` or `accessory`.

Container inventories use existing inventory data:

```json
{
  "id": "inventory/chest_start",
  "ownerKind": "container",
  "tags": ["container"],
  "stacks": []
}
```

Harvest resource nodes can use metadata conventions:

- `required_tool_tag`
- `required_tool_item_id`
- `tool_slot_id`
- `durability_cost`
- `charge_cost`
- `loot_table_id`
- `harvest_loot_table_id`
- `deplete_on_harvest`

Interaction metadata routing supports `container_id`, `resource_node_id`, `item_id`, `recipe_id`, `transaction_id`, `loot_table_id` and `tool_item_id`.

## Optional Narrative Definition Lists

`game.quests`, `game.dialogues` and `game.factions` are optional/default-empty. Older packages that omit the new narrative fields remain loadable.

Minimal narrative example:

```json
{
  "factions": [
    { "id": "faction/village", "name": "Village", "kind": "settlement", "defaultReputation": 0, "minReputation": -100, "maxReputation": 100 }
  ],
  "quests": [
    {
      "id": "quest/help_healer",
      "title": "Help the Healer",
      "description": "Gather three red herbs.",
      "objectives": [{ "id": "objective/herbs", "kind": "has_item", "targetId": "item/red_herb", "requiredAmount": 3 }],
      "rewards": [{ "kind": "reputation", "id": "faction/village", "amount": 5 }]
    }
  ],
  "dialogues": [
    {
      "id": "dialogue/healer",
      "title": "Village Healer",
      "startNodeId": "start",
      "nodes": [
        {
          "id": "start",
          "speakerId": "npc/healer",
          "text": "Can you bring me three red herbs?",
          "choices": [{ "id": "accept", "text": "I will help.", "startQuestId": "quest/help_healer", "closeDialogue": true }]
        }
      ]
    }
  ]
}
```

Narrative definitions are data contracts. Runtime quest journal, active dialogue, objective progress and faction reputation live in runtime state/snapshots only.
