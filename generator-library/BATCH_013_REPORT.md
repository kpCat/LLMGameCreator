# BATCH 013 REPORT — Combat, status and abilities

## Files generated

- `lua/combat/combat_schema.lua`
- `lua/combat/turn_based_combat.lua`
- `lua/combat/status_effects.lua`
- `lua/ability/ability_catalog_generator.lua`
- `docs/lua/combat_status_abilities.md`
- `manifests/combat_abilities.manifest.json`
- `tests/combat_examples.lua`
- `BATCH_013_REPORT.md`

## Contracts introduced

- Combat schema IR for modes, resources, actions and safe formula references.
- Turn-based combat config IR with action points, initiative, cooldown ticks and status duration ticks.
- Status effect catalog IR with duration, stacking, modifiers, tick effects and expire effects.
- Ability catalog IR with cooldowns, costs, target rules, status references and formula references.
- Dialogue-combat bridge metadata for morale, trust, suspicion and focus without generating dialogue nodes.

## Dependencies between files

- `turn_based_combat.lua` depends logically on `combat/combat_schema/v1`.
- `status_effects.lua` depends logically on `combat/combat_schema/v1` and `formula/formula_schema/v1`.
- `ability_catalog_generator.lua` depends logically on `combat/combat_schema/v1`, `combat/status_effects/v1` and `formula/formula_schema/v1`.
- Lua modules remain standalone and do not load each other.

## Internal self-check

- PASS: manifest JSON valid.
- PASS: no forbidden aliases: `module_id`, `file`, `depends_on_contracts`.
- PASS: every `files[]` path exists inside `generator-library/` layout.
- PASS: every `module.path` exists inside `generator-library/` layout.
- PASS: module IDs are unique inside batch.
- PASS: all module IDs use lowercase slash notation.
- PASS: capabilities use lowercase dot notation.
- PASS: no root folder leakage.
- PASS: batch report path is `generator-library/BATCH_013_REPORT.md`.
- PASS: docs path is `generator-library/docs/lua/combat_status_abilities.md`.
- PASS: tests path is `generator-library/tests/combat_examples.lua`.
- PASS: no Lua forbidden APIs: `io`, `os`, `debug`, `package`, `load`, `loadfile`, `dofile`, `math.random`.

## Manual validation

1. Inspect each Lua file and confirm it returns a table.
2. Confirm each module exposes `manifest`, `validate_config(config)` and `generate(input, ctx)`.
3. Check `manifests/combat_abilities.manifest.json` with any JSON validator.
4. Inspect `tests/combat_examples.lua` for compact example inputs and expected output shapes.
5. Import the manifest through the Generator Library Registry importer after copying the ZIP contents into the repository.

## Known limitations

- This batch generates configuration IR only; it does not implement a live combat simulator.
- Formula references are ids only; no runtime formula evaluator is included.
- Dialogue-combat is represented as bridge metadata and effect IR, not dialogue generation.
- Ability and status catalogs are compact examples/contracts, not large content packs.

## Next recommended batch

Batch 014 — NPC, schedule, pathfinding.

## Non-goals

- No C# integration.
- No Unity object generation.
- No Lua sandbox/executor integration.
- No raw formula code execution.
- No filesystem, network or process access.
- No changes to previous batches.
