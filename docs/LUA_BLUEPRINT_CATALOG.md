# Lua Blueprint Catalog

Этот каталог описывает готовые Lua-заготовки, которые LLM должна использовать как основу, а не изобретать заново.

## Назначение blueprint-скриптов

`templates/lua-blueprints/` — это не runtime stdlib. Это библиотека примеров и стартовых шаблонов для генерации конкретной игры.

LLM должна:

1. выбрать подходящий blueprint;
2. адаптировать id/tiles/entities/weights;
3. не менять контракт типа скрипта;
4. вернуть draft;
5. пройти validation.

## Группы blueprint-скриптов

```text
generators/
  infinite_perlin_world.lua
  finite_overworld_map.lua
  finite_dungeon_map.lua
  biome_chunk_generator.lua
  settlement_chunk_generator.lua
  road_path_generator.lua

behaviors/
  npc_wander_behavior.lua
  npc_schedule_behavior.lua
  hostile_chase_behavior.lua
  neutral_flee_behavior.lua

interactions/
  open_locked_chest.lua
  dialogue_by_flags.lua
  resource_node_harvest.lua
  vendor_open_shop.lua
  portal_change_map.lua

formulas/
  simple_damage.lua
  skill_check_chance.lua
  xp_gain.lua
  resource_regeneration.lua

events/
  weather_tick.lua
  random_travel_event.lua
  night_ambush_event.lua

loot/
  weighted_loot_table.lua
  biome_loot_table.lua

encounters/
  wolf_pack_encounter.lua
  bandit_patrol_encounter.lua
```

## Почему это снижает нагрузку на LLM

LLM не должна каждый раз проектировать:

- как выглядит chunk draft;
- как добавлять tile/entity/trigger;
- как выбирать биом;
- как возвращать effects;
- как делать поведение NPC;
- как открывать диалог;
- как считать формулу.

Она должна работать в готовых рельсах.

## Правило

Чем богаче набор blueprint-скриптов, тем меньше риск, что LLM начнёт писать невалидный, небезопасный или архитектурно чужой Lua.
