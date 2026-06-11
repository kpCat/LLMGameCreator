# Lua Blueprint Expansion

Расширенный набор Lua-заготовок нужен, чтобы LLM не писала типовую логику с нуля.

## Новые группы

```text
generators/
  island_world_generator.lua
  cave_network_generator.lua
  city_district_generator.lua
  room_based_building_generator.lua
  river_generator.lua
  coast_generator.lua
  biome_transition_generator.lua
  landmark_placement_generator.lua

behaviors/
  faction_patrol_behavior.lua
  trader_route_behavior.lua
  villager_daily_life_behavior.lua
  guard_post_behavior.lua
  animal_grazing_behavior.lua
  predator_hunt_behavior.lua
  companion_follow_behavior.lua
  neutral_social_behavior.lua

quests/
  fetch_quest_template.lua
  escort_quest_template.lua
  bounty_quest_template.lua
  rescue_quest_template.lua
  investigation_quest_template.lua
  dynamic_world_event.lua
  branching_dialogue_quest.lua
  location_discovery_quest.lua

events/
  day_night_tick.lua
  world_state_tick.lua
  faction_conflict_event.lua
  resource_regrowth_event.lua

combat/
  melee_enemy_behavior.lua
  ranged_enemy_behavior.lua
  mage_enemy_behavior.lua
  boss_phase_behavior.lua
  group_tactics_behavior.lua
  status_effect_tick.lua

economy/
  vendor_price_formula.lua
  crafting_recipe_check.lua
  resource_respawn.lua
  loot_rarity_scaling.lua

dialogue/
  relationship_based_dialogue.lua
  reputation_gate_dialogue.lua
  mood_based_dialogue.lua
  companion_comment_event.lua
```

## Статус

Это blueprints, а не production scripts.

Pipeline:

```text
Blueprint
  -> LLM адаптирует под конкретную игру
  -> Draft
  -> script manifest
  -> validation
  -> dry-run
  -> apply
```
