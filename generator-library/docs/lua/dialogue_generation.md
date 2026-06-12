# Lua Dialogue Generation Foundation

Batch 009 introduces the first dialogue layer for the Lua generator library. The modules produce compact, schema-oriented dialogue IR. They do not print large corpora of lines and they do not execute runtime dialogue logic.

## Files

- `lua/dialogue/dialogue_schema.lua`
- `lua/dialogue/procedural_npc_dialogue.lua`
- `lua/dialogue/fact_based_dialogue.lua`
- `lua/dialogue/dialogue_combat.lua`

## Shared contract

Every module returns a table and exposes:

- `manifest`
- `validate_config(config)`
- `generate(input, ctx)`

`generate` returns:

```lua
{
  ok = true,
  data = {},
  diagnostics = {},
  artifacts = {}
}
```

Diagnostics use:

```lua
{ severity = "error" | "warning" | "info", code = "...", message = "...", target = "..." }
```

All outputs are intended to be JSON-serializable.

## `dialogue_schema.lua`

### Purpose

Normalizes static dialogue nodes into a compact graph IR with node indexes, choice indexes and edges.

### When to use

Use it when the game design already contains explicit dialogue nodes and choices.

### When not to use

Do not use it to create hundreds of procedural lines. Use it as the final normalization layer after a higher-level generator has produced a small graph.

### Input schema explained

- `dialogue_id`: lowercase slash id, for example `dialogue/village/elder_intro`.
- `speaker_id`: optional entity id.
- `entry_node_id`: optional node id, default `start`.
- `nodes`: array of node records.

A node has `id`, `text`, optional `speaker_id`, `conditions`, `choices`, `tags` and `metadata`.

A choice has `id`, `text`, optional `to_node_id`, `ends_dialogue`, `conditions`, `effects`, `tags` and `ui_hints`.

### Config schema explained

- `max_nodes`
- `max_choices_per_node`
- `default_entry_node_id`
- `allowed_effect_targets`

### Output schema explained

- `dialogue`: normalized graph.
- `indexes.node_index`: node id to node array position.
- `indexes.choice_index`: `node_id/choice_id` to metadata.
- `indexes.edges`: directed choice edges.
- `summary`: counts and module metadata.

### Example config

```lua
{ max_nodes = 16, max_choices_per_node = 4 }
```

### Example input

```lua
{
  dialogue_id = "dialogue/village/elder_intro",
  speaker_id = "entity/npc/elder",
  nodes = {
    {
      id = "start",
      text = "The road is unsafe.",
      choices = {
        { id = "ask", text = "What happened?", to_node_id = "rumor" },
        { id = "leave", text = "Goodbye.", ends_dialogue = true }
      }
    },
    { id = "rumor", text = "Something blocks the old bridge.", choices = {} }
  }
}
```

## `procedural_npc_dialogue.lua`

### Purpose

Creates a small NPC conversation from an NPC profile, facts and topics.

### When to use

Use it when the LLM has collected facts and wants a compact conversation scaffold instead of writing every line manually.

### When not to use

Do not use it as a full narrative writer. It intentionally creates a small graph.

### Input schema explained

- `npc`: `{ id, name, role, tone }`.
- `facts`: array of facts with `id`, `title`, `summary`, `tags`.
- `topics`: optional explicit topics.
- `dialogue_id`: optional override.

### Output schema explained

- `dialogue`: graph with `start` and topic nodes.
- `source_facts`: normalized fact list.
- `source_topics`: merged topics and facts.
- `summary`: counts.

## `fact_based_dialogue.lua`

### Purpose

Builds conditional dialogue branches from facts and quest states.

### When to use

Use it for quest-state dialogue, investigation responses and conditional NPC answers.

### When not to use

Do not use it as a quest engine. It only emits dialogue IR with conditions/effects.

### Rule fields

- `id`
- `node_id`
- `choice_text`
- `text`
- `conditions`
- `required_facts`
- `blocked_by_facts`
- `quest_states`
- `effects`
- `set_facts`
- `quest_effects`

## `dialogue_combat.lua`

### Purpose

Creates dialogue-combat encounter IR where choices can affect `morale`, `trust`, `suspicion`, `focus` and other named tracks.

### When to use

Use it for intimidation, persuasion, debate, interrogation, morale combat, social duels and hybrid combat/dialogue scenes.

### When not to use

Do not use it as a full combat simulator. It describes choices, tracks, effects and win/loss conditions.

### Config schema explained

- `track_min`
- `track_max`
- `default_track_values`
- `max_choices`

### Choice fields

- `id`
- `text`
- `stance`
- `conditions`
- `effects`
- `cost`
- `cooldown_ticks`
- `ends_encounter`
- `tags`

Effects use `{ target = "trust", op = "add", amount = 10 }`-style records.

## LLM prompting hints

Ask the LLM for facts, quest states and desired emotional tracks first. Then select one of these modules:

- explicit static graph: `dialogue_schema`
- NPC scaffold from facts: `procedural_npc_dialogue`
- branch logic from known/unknown facts: `fact_based_dialogue`
- persuasion/intimidation/social combat: `dialogue_combat`

The LLM should keep generated examples small and let the runtime/UI layer render and execute the IR later.

## Validation rules

- Dialogue ids must be lowercase slash ids.
- Node ids are lowercase local ids with letters, digits and underscore.
- Choice targets must reference existing nodes when normalized by `dialogue_schema`.
- Effects and conditions must be plain tables.
- User/content validation returns diagnostics instead of throwing.

## Extension points

Future batches can add:

- quest references and quest validation;
- inventory/item dialogue conditions;
- combat bridge validation;
- UI IR for dialogue windows;
- localization packs;
- runtime dialogue executor.

## Runtime target notes

The generated IR is intended for `debug`, `unity2d` and later `unity_ui_ir` targets. It does not instantiate Unity objects directly.

## Unity/codegen notes

Unity adapter code can map:

- `dialogue.nodes` to dialogue window state;
- `choices` to buttons;
- `conditions` to runtime eligibility checks;
- `effects` to validated runtime commands;
- `dialogue_combat.encounter.tracks` to HUD bars.
