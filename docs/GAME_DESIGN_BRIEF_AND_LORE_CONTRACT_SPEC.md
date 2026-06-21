# Game Design Brief and Lore Contract Spec

## Purpose

The user must be able to describe a game in structured form, not only as a loose prompt.

The design brief stores:

```text
what kind of game this is
what lore/realism assumptions exist
what view modes are wanted
what interactions are wanted
what content style is wanted
what LLM may generate
what the program should generate
what Lua/data modules define
what Unity runtime modules are expected
```

## Suggested model: GameDesignBrief

Fields:

```text
brief_id
title
short_pitch
content_language
tone
realism_mode
lore_mode
lore_facts
world_rules
gameplay_wishes
interaction_wishes
view_mode_wishes
ui_wishes
asset_style_wishes
audio_style_wishes
generation_policy
scale_policy
performance_policy
```

## Realism modes

```text
abstract_gamey
semi_realistic
realistic_with_fictional_additions
hard_realistic
fantasy
custom
```

## View/interaction wishes examples

```text
top_down_character
first_person
third_person
world_map
tactical_battle
army_battle
vehicle_drive
public_transport_ride
interior_scene
dialogue_scene
walk
drive_vehicle
enter_building
talk
trade
fight_personal
command_army
craft
brew_potions
commit_crime
police_response
npc_daily_life
```

## Generation policy

Every area should be classifiable as:

```text
llm_seeded
program_generated
lua_defined
asset_generated
hand_authored
runtime_generated_lazy
```
