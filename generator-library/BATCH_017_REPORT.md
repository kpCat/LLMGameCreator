# BATCH 017 REPORT — UI IR

## Files generated

- `lua/ui/ui_schema.lua`
- `lua/ui/hud_layout.lua`
- `lua/ui/minimap_config.lua`
- `lua/ui/inventory_ui.lua`
- `lua/ui/quest_journal_ui.lua`
- `docs/lua/ui_ir.md`
- `manifests/ui_ir.manifest.json`
- `tests/ui_ir_examples.lua`
- `BATCH_017_REPORT.md`

## Current repo state read before generation

- Read `docs/LUA_GENERATION_PLAN_AND_PROMPTS.md` and used the exact Batch 017 definition.
- Read `generator-library/docs/lua/MANIFEST_CONTRACT.md` and followed the canonical manifest contract.
- Attempted to read newly pushed Batch 016 report and manifest from current `main`; the GitHub connector returned not found for `generator-library/BATCH_016_REPORT.md` and `generator-library/manifests/city_builder.manifest.json` at generation time.
- Read relevant existing manifests for dependency ids: world blueprint, quest generation, items/inventory and existing UI-adjacent runtime target style.

## Manifest contract compliance

- Batch manifest uses required fields: `id`, `version`, `batch`, `title`, `purpose`, `files`, `modules`, `runtime_targets`, `supported_time_modes`, `supported_combat_modes`, `unsafe_features`.
- Module entries use required fields: `id`, `path`, `category`, `capabilities`, `depends_on`, `runtime_targets`, `supported_turn_modes`, `supported_combat_modes`, `deterministic`, `unsafe_features`.
- No alias fields are used.
- Batch number is `017`.
- Batch report path is `generator-library/BATCH_017_REPORT.md`.

## Forbidden API compliance

- Lua sources contain no references to the disallowed runtime-access APIs listed in the Batch 017 request.
- No external dependencies are used.
- No random source is used.
- No global writes are used; each module returns a table.
- No C# runtime integration, renderer implementation or Unity object generation is included.

## Module summary

- `ui_schema.lua`: common renderer-agnostic UI IR schema helpers for ids, anchors, dimensions, visibility rules, bindings and actions.
- `hud_layout.lua`: HUD layout IR generator for minimal, RPG, automation, city-builder, dialogue-focused and tactical UI modes.
- `minimap_config.lua`: minimap/global map config IR generator with layers, marker categories, fog/reveal metadata and world-scale compatibility.
- `inventory_ui.lua`: inventory UI config IR generator for grid/list modes, item slots, equipment slots, filters and item detail metadata.
- `quest_journal_ui.lua`: quest journal, objective list, notes and codex UI config IR generator with quest/dialogue references.

## Tests/examples included

- `tests/ui_ir_examples.lua` provides manual example inputs for all five modules.
- It includes valid generation-call examples through injected module tables.
- It includes an invalid inventory config example that should produce diagnostics.
- It demonstrates JSON-serializable output shapes and does not execute a renderer.

## Internal self-check

- PASS: all ZIP paths start with `generator-library/`.
- PASS: no root-level generated folders or files.
- PASS: manifest JSON valid.
- PASS: every manifest `files[]` path exists in the ZIP under `generator-library/`.
- PASS: every manifest `module.path` exists in the ZIP under `generator-library/`.
- PASS: no alias manifest fields are used.
- PASS: batch required fields exist.
- PASS: module required fields exist.
- PASS: report name is numbered `BATCH_017_REPORT.md`.
- PASS: docs path is `generator-library/docs/lua/ui_ir.md`.
- PASS: tests path is `generator-library/tests/ui_ir_examples.lua`.
- PASS: module IDs are unique inside batch.
- PASS: all module IDs use lowercase slash notation.
- PASS: capabilities use lowercase dot notation.
- PASS: no forbidden Lua APIs are present in generated Lua sources.
- PASS: no C# project changes.
- PASS: Batch 018 was not generated.

## Known limitations

- The batch emits UI IR/config only; it does not render UI.
- Binding/action references are declarative ids and are not evaluated in Lua.
- Build menu support is layout/config metadata only; no construction gameplay logic is implemented.
- Future adapters must map panel/element kinds to concrete frontend technology.

## Non-goals

- No C# integration.
- No Unity object generation.
- No direct renderer implementation.
- No filesystem, network or process access.
- No changes to previous batches.
- No Batch 018 generation.
