# Batch 009 Report — Dialogue generation foundation

## Files generated

- `lua/dialogue/dialogue_schema.lua`
- `lua/dialogue/procedural_npc_dialogue.lua`
- `lua/dialogue/fact_based_dialogue.lua`
- `lua/dialogue/dialogue_combat.lua`
- `docs/lua/dialogue_generation.md`
- `manifests/dialogue_generation.manifest.json`
- `tests/dialogue_generation_examples.lua`
- `BATCH_009_REPORT.md`

## Contracts introduced

### Dialogue graph IR

`dialogue_schema.lua` introduces a compact graph shape:

- `dialogue.id`
- `dialogue.entry_node_id`
- `dialogue.nodes[]`
- `node.choices[]`
- `choice.conditions[]`
- `choice.effects[]`
- `indexes.node_index`
- `indexes.choice_index`
- `indexes.edges`

### Procedural NPC dialogue IR

`procedural_npc_dialogue.lua` creates a small schema-compatible dialogue graph from NPC profile data, facts and topics.

### Fact-based dialogue IR

`fact_based_dialogue.lua` introduces rule-driven conditional branches using facts, blocked facts, quest states and effects.

### Dialogue-combat IR

`dialogue_combat.lua` introduces social/combat tracks:

- `morale`
- `trust`
- `suspicion`
- `focus`

Choices can carry conditions, effects, costs, cooldown ticks and stance metadata. The module emits both encounter IR and a dialogue graph facade.

## Dependencies between files

The modules are intentionally self-contained and do not load each other. Contracts are aligned by shared IR conventions:

- `procedural_npc_dialogue.lua`, `fact_based_dialogue.lua` and `dialogue_combat.lua` emit dialogue graphs compatible with the shape normalized by `dialogue_schema.lua`.
- The manifest lists all module capabilities and runtime compatibility metadata.
- The tests are written as an injected-module example runner. They do not load files from disk.

## How to validate manually

1. Confirm the ZIP contains the exact files listed above.
2. Parse `manifests/dialogue_generation.manifest.json` as JSON.
3. Inspect each Lua module and confirm it returns a table.
4. Confirm each module exposes:
   - `manifest`
   - `validate_config(config)`
   - `generate(input, ctx)`
5. In a Lua 5.4 environment, load the four modules through your sandbox/module importer and pass them into `tests/dialogue_generation_examples.lua` as:

```lua
run_dialogue_generation_examples({
  dialogue_schema = dialogue_schema,
  procedural_npc_dialogue = procedural_npc_dialogue,
  fact_based_dialogue = fact_based_dialogue,
  dialogue_combat = dialogue_combat
})
```

## Known limitations

- This batch does not implement a runtime dialogue executor.
- This batch does not implement localization.
- This batch does not validate quest ids against an actual quest database.
- This batch does not simulate dialogue-combat turns; it only emits encounter IR.
- Generated text is intentionally compact and scaffold-like, not narrative-final prose.
- Modules are self-contained, so helper code is repeated to avoid forbidden external loading.

## Next recommended batch

Batch 010 — Quest/progress foundation.

## Notes

The batch follows the global Lua restrictions from the plan: deterministic generation, no external dependencies, no direct filesystem/network usage, JSON-serializable output and diagnostics for normal validation failures.
