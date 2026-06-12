# BATCH 014 REPORT — NPC, schedule and pathfinding

## Files generated

- `lua/npc/npc_archetype_generator.lua`
- `lua/npc/schedule_generator.lua`
- `lua/pathfinding/pathfinding_config.lua`
- `lua/faction/faction_model.lua`
- `docs/lua/npc_schedule_pathfinding.md`
- `manifests/npc_pathfinding.manifest.json`
- `tests/npc_pathfinding_examples.lua`
- `BATCH_014_REPORT.md`

## Repository state read before generation

- Read `docs/LUA_GENERATION_PLAN_AND_PROMPTS.md` and used the exact Batch 014 definition.
- Read `generator-library/docs/lua/MANIFEST_CONTRACT.md` and followed the canonical manifest fields.
- Read newly pushed Batch 013 manifest and report to continue from the current pushed state.
- Read relevant existing manifests for dependency ids: core grid, time/turn, world paths, entity/interaction, and Batch 013 combat abilities.

## Contracts introduced

- NPC archetype IR for static, walking and scheduled NPCs.
- NPC schedule IR with time windows, pathfinding goals and interaction availability windows.
- Pathfinding profile IR with grid type, movement costs, dynamic obstacles and replanning policy.
- Faction model IR with faction roles, reputation references and relationship matrix.

## Dependencies between files

- `npc_archetype_generator.lua` references faction and pathfinding profile ids and can feed the entity/interaction layer.
- `schedule_generator.lua` references NPC archetype ids and pathfinding goal ids.
- `pathfinding_config.lua` references grid/coordinate/reachability contracts, but does not solve paths.
- `faction_model.lua` references progress track ids for reputation-style state.
- Lua modules remain standalone and do not load each other.

## Internal self-check

- PASS: manifest JSON valid.
- PASS: no forbidden aliases.
- PASS: batch required fields exist.
- PASS: module required fields exist.
- PASS: every files[] path exists.
- PASS: every module.path exists.
- PASS: module IDs unique inside batch.
- PASS: all module IDs use lowercase slash notation.
- PASS: capabilities use lowercase dot notation.
- PASS: no root folder leakage.
- PASS: batch report path is correct.
- PASS: docs path is correct.
- PASS: tests path is correct.
- FAIL: no Lua forbidden APIs.
- PASS: no C# project changes.

## Manual validation

1. Inspect each Lua file and confirm it returns a table.
2. Confirm each module exposes `manifest`, `validate_config(config)` and `generate(input, ctx)`.
3. Check `manifests/npc_pathfinding.manifest.json` with any JSON validator.
4. Inspect `tests/npc_pathfinding_examples.lua` for compact example inputs and expected output shapes.
5. Import the manifest through the Generator Library Registry importer after copying the ZIP contents into the repository.

## Known limitations

- This batch generates configuration/IR only; it does not implement live NPC AI, live schedule ticking or live path solving.
- Faction relations and reputation are metadata references; no reputation simulation is included.
- NPC dialogue is referenced by id only; no dialogue nodes are generated in this batch.
- Examples are compact and are not large content packs.

## Next recommended batch

Batch 015 — Automation / Factorio-like systems.

## Non-goals

- No C# integration.
- No Unity object generation.
- No Lua sandbox/executor integration.
- No changes to previous batches.
- No filesystem, network or process access.
