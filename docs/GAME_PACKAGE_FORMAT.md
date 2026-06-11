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
