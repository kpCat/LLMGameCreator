# Interaction Combat Progression Variants

Status: authoritative planning taxonomy  
Scope: interaction, combat, progression, inventory/equipment and validation implications  
Non-scope: production code, runtime implementation, UI implementation, schema migration

Interactions, combat and progression must be explicit data contracts. They must not be hidden in Lua source, LLM prose or ad-hoc runtime assumptions.

## 1. Interaction Taxonomy

Supported interaction ids:

```text
interaction_model/inspect
interaction_model/talk
interaction_model/use
interaction_model/pickup
interaction_model/harvest
interaction_model/craft
interaction_model/build
interaction_model/repair
interaction_model/trade
interaction_model/lockpick
interaction_model/trigger
interaction_model/enter_location
interaction_model/dialogue_choice
interaction_model/combat_action
interaction_model/ability_use
interaction_model/party_skill_check
interaction_model/environment_skill_check
```

Future artifact contract:

- `interaction_pack_v1`

## 2. Requirements / Costs / Outputs Model

Every interaction or combat action should declare:

- requirements: refs, stats, skills, quest states, faction/reputation, inventory/equipment, party checks, environment checks;
- costs: resources, items, durability, charges, time, faction favor, relationship cost;
- outputs: items, resources, progression, reputation, relationship, quest events, state changes, encounter transitions;
- diagnostics: blocked, repairable, missing refs, incompatible variants.

Future contracts:

- `requirement_pack_v1`
- `effect_pack_v1`

## 3. Party Skill Checks

`interaction_model/party_skill_check` checks party/roster data rather than only one avatar. It is important for `actor_model/party_blob`, `actor_model/party_individuals` and `actor_model/controllable_squad`.

Validators must define whether checks use best member, sum, average, leader, role-specific member or explicit selected member.

## 4. Environment Skill Checks

`interaction_model/environment_skill_check` checks locks, traps, hazards, roads, machines, biomes, visibility, weather or other world data. It must reference known topology and interaction targets.

## 5. Combat Model Variants

Required combat ids:

```text
combat_model/none
combat_model/real_time
combat_model/turn_based
combat_model/tactical_grid
combat_model/active_pause
combat_model/blobber_party_turn_based
combat_model/jrpg_party_rows
combat_model/action_rpg_light
combat_model/dialogue_combat
combat_model/auto_battler
combat_model/encounter_card_based
```

Future contracts:

- `combat_pack_v1`
- `encounter_pack_v1`
- `ability_pack_v1`
- `status_pack_v1`

## 6. Combat Space Variants

Required combat-space ids:

```text
combat_space/same_map
combat_space/separate_arena
combat_space/tactical_grid_instance
combat_space/abstract_encounter
combat_space/first_person_party_frontline
```

Same-map combat requires map/path/target validators. Separate arenas require transition and return-state contracts. Abstract encounters require state machine validation. First-person party frontline requires party roster, formation, frontline/backline and billboard/card presentation refs.

## 7. Progression Model Variants

Required progression ids:

```text
progression_model/level_xp
progression_model/skill_use_based
progression_model/perk_tree
progression_model/class_tree
progression_model/trainer_based
progression_model/reputation_tracks
progression_model/faction_favor
progression_model/research_tree
progression_model/equipment_based
progression_model/card_unlocks
progression_model/relationship_progression
progression_model/colony_tech_progression
```

Future contracts:

- `progression_pack_v1`
- `skill_tree_pack_v1`
- `reputation_pack_v1`
- `relationship_pack_v1`

## 8. Inventory / Equipment Interaction With Combat And Progression

Future contracts:

- `inventory_pack_v1`
- `equipment_pack_v1`

Inventory/equipment variants can affect combat and progression:

- grid or paper-doll inventory changes item placement validation;
- durability and charges can be costs;
- equipment requirements can block ability use;
- party equipment can use party skill checks;
- equipment-based progression can unlock abilities or modifiers;
- quickbar slots can constrain available combat actions.

These effects must remain data-driven and validator-visible.

## 9. Validators

Required validator families:

- `interaction.target_refs_valid`
- `interaction.requirements_known`
- `interaction.costs_outputs_known`
- `interaction.party_skill_check_valid`
- `interaction.environment_check_valid`
- `combat.model_supported`
- `combat.space_compatible`
- `combat.participants_valid`
- `combat.abilities_valid`
- `combat.turn_order_valid`
- `combat.no_hidden_lua_logic`
- `progression.model_supported`
- `progression.bounds_valid`
- `progression.refs_valid`
- `inventory.equipment_combat_refs_valid`

## 10. Runtime Preview Smoke Requirements

Future runtime preview smoke should be selected by variant:

- no combat: load, move/select, interact, complete safe action;
- turn-based or blobber combat: start encounter, take one player action, take one enemy turn, resolve or report active state;
- tactical combat: load tactical space, compute reachable cells, execute one ability;
- dialogue combat: choose one dialogue-combat action and validate state outputs;
- auto battler/card encounter: initialize encounter, run deterministic step, report outcome;
- progression: grant xp/skill/reputation/research and validate bounds;
- inventory/equipment: pick up, equip/use, apply durability or requirement checks.

Runtime preview remains diagnostic. It is not the final player and must not call LLM/providers.
