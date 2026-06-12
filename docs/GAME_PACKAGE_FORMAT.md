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
    "recipes": [],
    "lootTables": [],
    "transactions": [],
    "resourceNetworks": [],
    "resourceNodes": [],
    "inventories": []
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
