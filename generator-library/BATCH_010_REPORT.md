# Batch 010 Report — Quest/progress foundation

## Files generated

```text
lua/quest/quest_schema.lua
lua/quest/simple_investigation.lua
lua/quest/fetch_quest.lua
lua/quest/location_discovery.lua
docs/lua/quest_generation.md
manifests/quest_generation.manifest.json
tests/quest_generation_examples.lua
BATCH_010_REPORT.md
```

## Contracts introduced

### Quest IR

A compact, JSON-serializable quest table with:

- `id`
- `title`
- `description`
- `status`
- `start_stage_id`
- `stages`
- `triggers`
- `progress_tracks`
- `completion_conditions`
- `effects`
- `tags`
- `metadata`

### Stage IR

A stage contains:

- `id`
- `title`
- `description`
- `objectives`
- `completion_conditions`
- `effects`
- `transitions`
- `tags`
- `metadata`

### Objective IR

Foundation objective types:

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

### Condition IR

Foundation condition types:

- `objective_complete`
- `flag_set`
- `item_count`
- `location_discovered`
- `interaction_happened`
- `dialogue_choice_selected`
- `counter_at_least`
- `stage_active`

### Effect IR

Foundation effect types:

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

### Abstract progress

Progress tracks are not limited to XP. They can represent counters or abstract tracks such as investigation progress, exploration progress, reputation, suspicion, trust, faction favor, or research.

## Dependencies between files

```text
quest_schema.lua
  no Lua-file dependency

simple_investigation.lua
  contract dependency: quest_schema/v1 output shape

fetch_quest.lua
  contract dependency: quest_schema/v1 output shape

location_discovery.lua
  contract dependency: quest_schema/v1 output shape
```

No module uses `require`; dependencies are contract-level and manifest-level only.

## How to validate manually

1. Confirm the ZIP contains all listed files.
2. Parse `manifests/quest_generation.manifest.json` as JSON.
3. Review each Lua module and confirm:
   - returns a table;
   - has `manifest`;
   - has `validate_config(config)`;
   - has `generate(input, ctx)`;
   - does not call file system, process, dynamic loader, network, or direct random APIs.
4. Review `tests/quest_generation_examples.lua` as data-only examples.
5. In a trusted Lua host, inject modules manually and call:
   - `quest_schema.generate(examples.quest_schema_input, {})`
   - `simple_investigation.generate(examples.simple_investigation_input, { config = examples.simple_investigation_config })`
   - `fetch_quest.generate(examples.fetch_quest_input, { config = examples.fetch_quest_config })`
   - `location_discovery.generate(examples.location_discovery_input, { config = examples.location_discovery_config })`

## Known limitations

- This batch does not implement runtime quest state ticking.
- This batch does not mutate inventory, dialogue state, world state, or save data.
- This batch does not validate references against real item/world/dialogue/entity registries.
- Quest branching is intentionally compact; complex branching graphs should be handled by later validation/orchestration modules.
- `tests/quest_generation_examples.lua` is data-only and does not use `require` or a file loader.

## Next recommended batch

Batch 011 — Inventory/items/loot.

## Scope notes

- No C# project files were modified.
- No Batch 011 files were generated.
- The report filename uses the numbered convention: `BATCH_010_REPORT.md`.
