# Product Slice 002: Composable Module Selection UI

## Goal

Product Slice 001 added the non-breaking model fields:

```text
selected_module_ids
selected_modifier_ids
selected_constraint_ids
runtime_requirement_ids
```

but the UI still cannot choose them.

Product Slice 002 wires these fields into the Capability Picker end-to-end.

## User problem

The current UI still forces complex systems into single dropdowns:

```text
Progression model = one option
Combat model = one option
World topology = one option
```

Real games need composition:

```text
level-based + perk tree + stat allocation + skill XP
realtime + turn-based toggle + party commands
region graph + biomes + weather + time of day + procedural events
economy + trading + price policy + supply/demand
power budget + encounter tiers
```

## Target behavior

Capability Picker should show additional selectable groups:

```text
Modules
Modifiers
Constraints
Runtime requirements
```

These should be multi-select checklists grouped by domain, not single dropdowns.

The existing dropdowns remain as high-level core axes.

## Initial module catalog

The current in-memory seed catalog is enough for this slice.

At minimum the UI should expose:

### Progression modules

```text
progression/perk_tree
progression/level_up_stat_allocation
progression/skill_xp
progression/class_tree
progression/faction_rank
progression/metamodule_growth
```

### Combat modules/modifiers

```text
combat/realtime
combat/turn_based
combat/hybrid_realtime_turn_toggle
combat/dialogue_combat
combat/party_commands
```

### World modules

```text
world/region_graph
world/chunk_generation
world/biomes
world/weather
world/time_of_day
world/procedural_events
world/settlements
```

### Economy and balance modules

```text
economy/economy
economy/trading
economy/price_policy
economy/supply_demand
balance/power_budget
balance/encounter_tiers
```

Exact IDs may differ if the existing catalog already uses a slightly different naming convention. Prefer existing IDs if present.

## UI principles

- The feature bundle list should show short Russian titles first.
- Machine ids should remain visible in details/help, but should not dominate the list.
- Module/modifier/constraint lists should show readable labels, with machine ids in details.
- The help panel should work for these new selections.
- The UI should remain usable at common desktop sizes.
- Do not create another startup SplitterDistance crash.

## Compatibility rules

Do not enforce full compatibility logic yet, but add safe initial diagnostics/warnings where practical:

- selecting `world/chunk_generation` without compatible world/runtime support should warn `unsupported_yet` or `risky`;
- selecting `combat/hybrid_realtime_turn_toggle` should be allowed and described as a hybrid mode;
- selecting multiple progression modules should be allowed;
- selecting economy/balance modules should be allowed but may warn `unsupported_yet`;
- required technical base must remain selected.

## Persistence

Selections must round-trip through:

```text
Build selection
-> Save latest selection
-> Load latest selection
-> LLM Artifacts Load
-> Prompt preview
```

Existing saved selections without new fields must still load.

## Strict prompt context

When the new arrays are non-empty, strict artifact prompt context should include them.

When the new arrays are empty, old prompt shape should remain compact.

## Done

This slice is done when:

- user can choose several progression/combat/world/economy/balance modules;
- saved latest selection contains selected arrays;
- LLM Artifacts prompt preview includes selected arrays;
- old selection JSON still loads;
- Capability Picker build/save/load works;
- focused tests and check-all pass.
