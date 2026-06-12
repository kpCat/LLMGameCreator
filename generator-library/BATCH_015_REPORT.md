# BATCH 015 REPORT — Automation / Factorio-like systems

## Files generated

- `lua/automation/recipe_graph.lua`
- `lua/automation/machine_catalog.lua`
- `lua/automation/conveyor_grid.lua`
- `lua/automation/power_network.lua`
- `docs/lua/automation_factorio_like.md`
- `manifests/automation.manifest.json`
- `tests/automation_examples.lua`
- `BATCH_015_REPORT.md`

## Repository state read before generation

- Read `docs/LUA_GENERATION_PLAN_AND_PROMPTS.md` and used the exact Batch 015 definition.
- Read `generator-library/docs/lua/MANIFEST_CONTRACT.md` and followed the canonical manifest fields.
- Read newly pushed Batch 014 manifest and report to continue from the current pushed state.
- Confirmed Batch 015 follows Batch 014 and does not proceed to Batch 016.

## Contracts introduced

- Recipe graph IR with recipe inputs, outputs, producer/consumer indexes and target production chains.
- Machine catalog IR with recipe category mapping, machine speed, power demand and placement metadata.
- Conveyor/logistics grid IR with logistics nodes, directed links, capacity metadata and deterministic adjacency.
- Power-network IR with generators, consumers, accumulators, reserve ratio and deterministic balance diagnostics.
- Compact validation diagnostics for missing producers, cycles, duplicate ids, unknown links and power deficits.

## Dependencies between files

- `recipe_graph.lua` references item ids and resource ids but remains standalone.
- `machine_catalog.lua` maps recipe categories to machine profiles and references recipe graph concepts.
- `conveyor_grid.lua` references grid/coordinate contracts and emits logistics IR only.
- `power_network.lua` references machine power demand metadata and emits power balance IR only.
- Lua modules remain standalone and do not load each other.

## Internal self-check

- PASS: manifest JSON valid.
- PASS: batch required fields exist.
- PASS: module required fields exist.
- PASS: no forbidden aliases.
- FAIL: every files[] path exists.
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
3. Check `manifests/automation.manifest.json` with any JSON validator.
4. Inspect `tests/automation_examples.lua` for compact example inputs and expected output shapes.
5. Import the manifest through the Generator Library Registry importer after copying the ZIP contents into the repository.

## Known limitations

- This batch generates configuration/IR only; it does not implement a live factory runtime.
- Production estimates are deterministic planning helpers, not a full tick simulation.
- Conveyor/logistics output is graph IR, not belt physics.
- Power balance is aggregate planning metadata, not an electrical network solver.
- Examples are compact and are not large content packs.

## Next recommended batch

Batch 016 — City-builder / simulation basics.

## Non-goals

- No C# integration.
- No Unity object generation.
- No Lua sandbox/executor integration.
- No changes to previous batches.
- No filesystem, network or process access.
- No huge hardcoded content catalogs.
