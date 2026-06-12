# Batch 017 — UI IR

## Purpose

Batch 017 adds deterministic UI IR/config generator-library modules for game-facing interface structure. UI IR means renderer-agnostic data that describes panels, windows, elements, bindings, actions and layout metadata. It is intended for validation, planning and future adapter generation.

This batch is not a renderer. It does not instantiate Unity objects, does not emit C# code and does not assume a specific frontend runtime. A future Unity adapter can read this IR and map panels/elements to prefabs, canvases or other view technology without coupling the Lua library to Unity APIs.

## Design boundary

The modules emit compact JSON-serializable tables only. They do not execute UI behavior, do not keep live UI state and do not evaluate bindings or actions. Binding/action values are references for a host runtime or adapter.

## Modules

### `lua/ui/ui_schema.lua`

Common schema helpers for UI element ids, panel/window ids, anchors, size metadata, visibility rules, binding references and action references. It reports diagnostics for invalid ids, anchors, dimensions and reference metadata.

Typical output shape:

- `schema_version`
- `allowed_anchors`
- `allowed_visibility_modes`
- `panels`
- `elements`
- `bindings`
- `actions`
- `metadata`

### `lua/ui/hud_layout.lua`

Generates HUD layout IR for:

- minimal HUD;
- RPG HUD;
- automation HUD;
- city-builder UI;
- dialogue-focused HUD;
- tactical UI.

It can emit status bars, stat bars, quick slots/action slots, dialogue windows, tactical panels and build menu panels as compact layout metadata.

### `lua/ui/minimap_config.lua`

Generates minimap/global map config IR. It supports map panels, map layers, marker categories, fog/reveal metadata and world-scale compatibility for single maps, regions, continents, planets and infinite chunk worlds.

### `lua/ui/inventory_ui.lua`

Generates inventory UI config IR for grid/list modes, item slots, equipment slots, item detail panels, filters/categories and item description display metadata such as stack, durability and rarity visibility.

### `lua/ui/quest_journal_ui.lua`

Generates quest journal, objective list, notes and codex UI config IR. It supports tracked quest metadata and quest/dialogue integration references without generating quest logic or dialogue content.

## Input/config/output strategy

All generator modules accept `generate(input, ctx)` and return:

- `ok`: boolean;
- `data`: JSON-serializable UI IR/config;
- `diagnostics`: array of diagnostic entries;
- `artifacts`: array, currently empty.

Validation failures are reported as diagnostics and are not thrown as normal control flow. The caller can import the generated IR into later registry/import workflows or feed it into future UI adapter planning.

## Diagnostics strategy

Diagnostics use the standard shape:

- `severity`;
- `code`;
- `message`;
- `target`.

Examples of diagnostic categories:

- invalid lowercase slash ids;
- invalid anchors;
- invalid dimensions;
- duplicate slots or sections;
- invalid map layers or marker categories;
- invalid objective layout settings.

## Example use cases

### RPG HUD

Use `hud_layout.lua` with `hud_mode = "rpg_hud"`, status bars for health/stamina/mana, stat bars for XP or reputation and quick slots for abilities/items.

### Minimap and global map

Use `minimap_config.lua` with `map_mode = "both"`, world-scale metadata from world blueprint modules, compact layers and marker categories.

### Inventory screen

Use `inventory_ui.lua` with `mode = "grid"`, slot count, equipment slots and category filters derived from item/inventory rules.

### Quest journal

Use `quest_journal_ui.lua` with active/completed/notes/codex sections and objective layout settings. Quest and dialogue integration is expressed only by references.

### City-builder and automation build menu style UI

Use `hud_layout.lua` with `hud_mode = "automation_hud"` or `city_builder_ui` and provide build menu categories such as production, logistics, power, housing or services.

## Future Unity adapter path

A future adapter can consume this IR by mapping:

- panels to windows/canvas regions;
- element kinds to view prefabs;
- bindings to host-side state selectors;
- actions to host-side command ids;
- layout metadata to screen region rules.

The adapter remains responsible for rendering, event dispatch and runtime state updates.
