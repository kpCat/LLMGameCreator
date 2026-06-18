# Capability Composer v2 Specification

## Problem

The current Capability Picker is useful for smoke evaluation, but it is too flat for real game design.

Current pattern:

```text
one dropdown = one selected variant
feature bundles = checkboxes
extra brief = raw text
```

This causes real design issues:

- progression systems are composable, but the UI allows one `progression_model`;
- combat systems can be hybrid, but the UI allows one `combat_model`;
- world generation needs multiple interacting modules: regions, chunks, biomes, weather, time, economy, encounters, settlements;
- warnings do not clearly distinguish true incompatibility from “not implemented yet”;
- option names are mostly English machine-ish labels, hard to understand for a Russian-speaking user;
- there is no brainstorming layer before strict JSON artifact generation.

## Design goal

Capability Composer v2 should let a user describe a game as a composition of:

```text
core axes
+ modules
+ modifiers
+ constraints
+ runtime requirements
+ generation/export targets
```

It must not force mutually compatible features into single-choice dropdowns.

## Core model

### Core axes

Core axes represent the primary shape of a game. They may remain single-choice at first.

Examples:

```text
presentation_mode
world_topology
primary_actor_model
primary_runtime_target
```

### Modules

Modules are additive capabilities. They can be selected together unless a compatibility rule rejects the combination.

Examples:

```text
perk_tree
level_up_stat_allocation
skill_xp
party_members
settlements
economy
weather
time_of_day
faction_reputation
biomes
procedural_events
trade_routes
crafting
```

### Modifiers

Modifiers tune a core axis or module.

Examples:

```text
combat_time_mode = realtime / turn_based / hybrid_toggle / pause_and_plan
progression_pacing = slow / normal / fast
world_scale = small / regional / large / infinite
economy_strictness = arcade / simulation_light / simulation_heavy
```

### Constraints

Constraints describe hard design rules, not generated content.

Examples:

```text
no_level_scaling_to_player
safe_start_region_required
economy_no_infinite_money_loops
combat_every_enemy_must_have_counterplay
```

### Runtime requirements

Runtime requirements say what the player/runtime must be able to execute.

Examples:

```text
requires_region_graph
requires_chunk_streaming
requires_day_night_cycle
requires_weather_state
requires_trade_market_state
requires_turn_toggle
requires_party_state
```

## Compatibility diagnostics

Diagnostics must distinguish:

### error: impossible

The selected combination is conceptually or technically invalid.

Example:

```text
Map-and-panel RPG + infinite tile chunk streaming as primary topology
```

### warning: unsupported_yet

The concept is valid, but the current contracts/validators/runtime do not support it yet.

Example:

```text
faction economy selected, but economy contract family is not implemented yet
```

### warning: risky

The concept can work, but requires careful design or validation.

Example:

```text
metamodule power stacking + no hard caps + open economy
```

### info: recommendation

Helpful suggestion without blocking.

## Progression model

Current single-choice model is insufficient.

Recommended composition:

```text
Progression core:
- level_based
- skill_use_based
- card_unlocks
- reputation_based
- equipment_based
- narrative_milestones

Progression modules:
- perk_tree
- stat_points_on_level_up
- class_tree
- skill_xp
- crafting_mastery
- faction_rank
- meta_progression
- metamodule_growth
```

For example, a valid game can select:

```text
level_based + perk_tree + stat_points_on_level_up + skill_xp
```

## Combat model

Current single-choice model is insufficient.

Recommended composition:

```text
Combat time mode:
- realtime
- turn_based
- hybrid_realtime_turn_toggle
- pause_and_plan
- action_points

Combat interaction mode:
- direct_attack
- dialogue_combat
- tactical_grid
- party_commands
- auto_battle
- command_menu

Combat scope:
- single_character
- party
- squad
- colony
- settlement
```

Might and Magic VII-style combat is:

```text
combat_time_mode = hybrid_realtime_turn_toggle
combat_interaction_mode = direct_attack + party_commands
combat_scope = party
```

## Infinite world generation

Infinite or very large worlds should not be a single dropdown value only. They require a module family:

```text
world_generation_profile_v1
biome_table_v1
weather_model_v1
time_cycle_v1
region_generation_rules_v1
chunk_generation_rules_v1
encounter_spawn_table_v1
resource_distribution_v1
settlement_generation_rules_v1
```

For early text-RPG support, prefer procedural region graphs before true infinite tile streaming:

```text
seed -> region graph -> local nodes -> encounters/resources/events -> optional chunk layer later
```

## Economy and trading

Economy should be a separate capability family:

```text
economy_model_v1
currency_model_v1
price_policy_v1
supply_demand_table_v1
settlement_market_v1
trade_route_v1
faction_tax_policy_v1
scarcity_model_v1
crafting_cost_model_v1
vendor_profile_v1
```

Price should be derived from:

```text
base value
rarity
condition
weight/volume
region
supply/demand
faction
reputation
taxes
route danger
season/event modifiers
```

## Balance model

Balance should be explicit and testable.

Design-time balance:

```text
power budgets
encounter difficulty tiers
loot budget
economy budget
progression curves
simulation tests
```

Runtime balance must not simply scale everything to the player. Prefer objective world criteria:

```text
region danger tier
faction strength
world age
scarcity level
settlement economy
route danger
player notoriety
chosen difficulty
```

## Design Assistant layer

Before strict JSON generation, the tool needs a brainstorming mode.

Design Assistant should accept vague prompts and respond with questions and option sets, not strict artifacts. Only after the user confirms choices should the system produce a strict capability selection and strict artifacts.

## Migration strategy

Do not rewrite everything immediately.

Step 1:
- Add Russian readable labels/descriptions/help panel.
- Add compatibility diagnostic categories: impossible / unsupported_yet / risky / info.
- Keep legacy selection artifact.

Step 2:
- Add non-breaking `selected_modules`, `selected_modifiers`, `selected_constraints`.
- Keep old selected_variant_ids for existing flows.

Step 3:
- Update strict prompt context to include modules/modifiers/constraints.

Step 4:
- Add one product vertical slice that turns accepted artifacts into GamePackage state.

## Done criteria

A capability is not “done” merely because it exists in a checkbox.

Done means:

```text
select -> explain -> validate compatibility -> generate strict artifact -> validate artifact -> review -> apply to package -> save/export -> runtime/sample visible or intentionally deferred
```
