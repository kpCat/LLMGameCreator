# Batch 010 — Quest/progress foundation

## Purpose

This batch introduces a compact quest/progress foundation for the Lua generator library. It defines reusable quest IR contracts and three small quest generators:

- `quest_schema.lua` normalizes and validates quest definitions.
- `simple_investigation.lua` creates investigation quest IR from dialogue, clues, facts, and interaction hooks.
- `fetch_quest.lua` creates item collection and delivery quest IR.
- `location_discovery.lua` creates exploration/location discovery quest IR.

The batch intentionally does not implement a runtime quest engine. Runtime state, persistence, objective ticking, inventory mutation, dialogue execution, and UI rendering remain responsibilities of the future host/runtime layer.

## Shared quest IR

A quest is a JSON-serializable table with this shape:

```text
quest = {
  id = "quest/investigate_road",
  title = "Investigate the Old Road",
  description = "...",
  status = "inactive",
  start_stage_id = "accept",
  stages = { ... },
  triggers = { ... },
  progress_tracks = { ... },
  completion_conditions = { ... },
  effects = { ... },
  tags = { ... },
  metadata = { ... }
}
```

### Stages

Stages are ordered quest steps:

```text
stage = {
  id = "gather_clues",
  title = "Gather clues",
  description = "...",
  objectives = { ... },
  completion_conditions = { ... },
  effects = { ... },
  transitions = { ... },
  tags = { ... },
  metadata = { ... }
}
```

### Objectives

Supported objective types in the foundation contract:

- `talk_to`
- `inspect`
- `collect_item`
- `deliver_item`
- `reach_location`
- `discover_location`
- `use_item_on_target`
- `defeat_entity`
- `wait_ticks`
- `custom_counter`

Objectives use runtime-facing completion conditions instead of direct side effects. This keeps generated data deterministic and reviewable.

### Conditions

Supported condition types:

- `objective_complete`
- `flag_set`
- `item_count`
- `location_discovered`
- `interaction_happened`
- `dialogue_choice_selected`
- `counter_at_least`
- `stage_active`

These are declarative conditions. The runtime decides how to evaluate them against save-state, dialogue-state, interaction-state, inventory-state, and world-state.

### Effects

Supported effect types:

- `set_flag`
- `clear_flag`
- `add_item`
- `remove_item`
- `start_quest`
- `complete_quest`
- `unlock_dialogue`
- `reveal_location`
- `advance_stage`
- `add_progress`
- `emit_event`

Effects are also declarative. They are not executed by these Lua generators.

### Abstract progress

This batch does not assume progress is XP. Progress tracks may represent:

- clue count;
- collected items;
- exploration progress;
- reputation;
- suspicion;
- trust;
- faction favor;
- research progress;
- morale.

Example:

```text
progress_track = {
  id = "quest/investigate_road/investigation",
  title = "Investigation progress",
  kind = "abstract_progress",
  min = 0,
  max = 3,
  starts_at = 0
}
```

## Module: `lua/quest/quest_schema.lua`

### Purpose

Normalize and validate quest IR.

### When to use

Use it after another generator produces quest-like data and before the host imports the quest into a design database or capability registry.

### When not to use

Do not use it as a save-game quest runtime, as a dialogue engine, or as an inventory mutator.

### Manifest summary

- id: `quest/quest_schema/v1`
- capabilities: `quest.schema.normalize`, `quest.progress.validate`
- targets: `debug`, `unity2d`, `unity3d`
- turn modes: `realtime`, `turn_based`, `mixed`, `paused_planning`
- combat modes: `none`, `realtime`, `turn_based`, `tactical`, `dialogue_combat`, `hybrid`

### Input schema explained

Input may be either:

```text
{ quests = { quest1, quest2 } }
```

or one quest table directly.

### Config schema explained

```text
{
  allowed_objective_types = { ... },
  allowed_condition_types = { ... },
  allowed_effect_types = { ... },
  default_status = "inactive"
}
```

All fields are optional.

### Output schema explained

```text
{
  quests = { normalized_quest1, normalized_quest2 },
  summary = {
    quest_count = 2,
    objective_count = 8,
    trigger_count = 2,
    supports_dialogue_triggers = true,
    supports_interaction_triggers = true,
    supports_abstract_progress = true
  }
}
```

### Validation rules

- quest id must be a lowercase slash id;
- each quest must have at least one stage;
- duplicate quest ids are errors;
- duplicate stage ids inside one quest are errors;
- transition targets must point to existing stages;
- objective/condition/effect types must be in allowed lists.

### Extension points

The host may extend allowed objective, condition, and effect types through config. Later batches may add validation modules that resolve cross-references to dialogue, world, item, and interaction registries.

### Runtime target notes

The output is data-only. A runtime can evaluate conditions and apply effects using its own deterministic state model.

### Unity/codegen notes

Unity adapters should consume this IR through a quest system component or generated ScriptableObject-like import pipeline. This batch does not generate raw C#.

## Module: `lua/quest/simple_investigation.lua`

### Purpose

Generate a compact investigation quest from clues, suspect/fact hooks, dialogue start, and interaction targets.

### When to use

Use it for RPG/adventure quests such as investigating a blocked road, cursed well, missing NPC, ruined machine, or suspicious faction event.

### When not to use

Do not use it for branching mystery simulations with dozens of clues or deep deduction logic. Such flows should be decomposed into several quests or handled by a future investigation-specific module.

### Input schema explained

```text
{
  quest_id = "quest/investigate_old_road",
  title = "Investigate the Old Road",
  giver_id = "entity/npc/elder",
  suspect_id = "entity/npc/bandit_scout",
  clue_targets = {
    { id = "broken_cart", target = "entity/object/broken_cart", fact_id = "fact/cart_was_attacked" },
    { id = "fresh_tracks", target_location_id = "world/location/muddy_tracks", fact_id = "fact/tracks_go_north" }
  },
  facts = { "fact/cart_was_attacked", "fact/tracks_go_north" },
  reward_effects = { { type = "set_flag", target = "village/road_safe", value = true } }
}
```

### Config schema explained

```text
{
  default_clue_count = 2,
  require_report_back = true,
  include_progress_track = true
}
```

### Output schema explained

The module returns one quest with stages:

1. `accept`
2. `gather_clues`
3. `confront`
4. `complete`

If `require_report_back` is true and `giver_id` exists, a `report_to_giver` objective is added to the confrontation/report stage.

### LLM prompting hints

Ask the LLM for a small list of clue targets and facts, not for dozens of dialogue lines. Dialogue text belongs in dialogue modules; this quest module stores only hooks and condition/effect IR.

### Runtime target notes

A host runtime should mark clue objectives complete when matching inspect/interact events happen, then unlock fact-based dialogue for confrontation.

## Module: `lua/quest/fetch_quest.lua`

### Purpose

Generate a compact collect/deliver quest.

### When to use

Use for RPG fetch quests, city-builder requests, automation tutorial steps, resource delivery tasks, or simulation goals that require item counts.

### When not to use

Do not use it as an inventory implementation or loot system. Item catalogs and inventory constraints belong to later item/inventory batches.

### Input schema explained

```text
{
  quest_id = "quest/bring_herbs",
  title = "Bring Healing Herbs",
  giver_id = "entity/npc/healer",
  item_id = "item/resource/healing_herb",
  count = 3,
  delivery_target_id = "entity/npc/healer",
  reward_effects = {
    { type = "add_progress", target = "reputation/village", amount = 1 }
  }
}
```

### Config schema explained

```text
{
  allow_partial_progress = true,
  require_return = true,
  remove_items_on_complete = true
}
```

### Output schema explained

The module returns one quest with stages:

1. `accepted`
2. `collect`
3. `deliver`
4. `complete`

It also returns `item_requirements` with item id, count, and whether items should be removed when completed.

### Runtime target notes

Runtime evaluates `item_count` and applies `remove_item` only after turn-in. The module only declares effects.

## Module: `lua/quest/location_discovery.lua`

### Purpose

Generate a compact exploration quest to reveal, discover, optionally inspect, and optionally report a location.

### When to use

Use for open-world RPG, adventure, survival, city-builder exploration, or world-map discovery quests.

### When not to use

Do not use it for pathfinding or terrain generation. World topology is produced by world and path batches.

### Input schema explained

```text
{
  quest_id = "quest/find_old_shrine",
  title = "Find the Old Shrine",
  location_id = "world/location/old_shrine",
  hint_source_id = "entity/object/weathered_map",
  landmark_id = "entity/object/shrine_gate",
  report_target_id = "entity/npc/elder"
}
```

### Config schema explained

```text
{
  require_hint = true,
  require_inspection = true,
  require_report_back = true
}
```

### Output schema explained

The module returns one quest with stages:

1. `hint`
2. `discover`
3. `inspect`
4. `report`
5. `complete`

It also returns `world_effects` for location reveal/discovery hints.

### Runtime target notes

The runtime should translate world map events into `location_discovered` conditions and inspect interactions into `interaction_happened` conditions.

## Manual validation

1. Check each Lua file returns a table.
2. Check each module has `manifest`, `validate_config(config)`, and `generate(input, ctx)`.
3. Check `manifests/quest_generation.manifest.json` parses as JSON.
4. Review `tests/quest_generation_examples.lua` as data-only examples.
5. Run examples in a trusted Lua host only after injecting modules manually; tests do not use `require` or file system access.

## Future extension points

- item registry validation;
- world location reference validation;
- dialogue node reference validation;
- quest journal UI IR;
- quest runtime state machine;
- branching quest graphs;
- generated Unity-facing quest import metadata.
