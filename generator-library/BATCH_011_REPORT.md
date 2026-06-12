# BATCH 011 REPORT — Inventory/items/loot

## Files generated

- `lua/item/item_schema.lua`
- `lua/item/item_catalog_generator.lua`
- `lua/item/loot_table_generator.lua`
- `lua/item/inventory_rules.lua`
- `docs/lua/items_inventory_loot.md`
- `manifests/items_inventory.manifest.json`
- `tests/items_inventory_examples.lua`
- `BATCH_011_REPORT.md`

## Contracts introduced

### Item IR

`item_schema.lua` introduces compact normalized item records with:

- lowercase slash `id`;
- `type`;
- `rarity`;
- `tags`;
- `stack` metadata;
- `quest_item` flag;
- optional `equipment` metadata;
- optional `durability` metadata;
- `description_config` for future UI and Unity adapters.

### Catalog generation IR

`item_catalog_generator.lua` introduces deterministic family-to-items expansion. It accepts small item families and variants, then emits compact item definitions. It does not generate a huge item encyclopedia.

### Loot table IR

`loot_table_generator.lua` introduces:

- loot pool ids;
- guaranteed drops;
- weighted entries;
- catalog filter expansion by rarity/type/tags;
- quantity ranges;
- table and entry caps.

This is definition IR, not runtime random resolution.

### Inventory rules IR

`inventory_rules.lua` introduces:

- inventory containers;
- slot capacity and optional weight limits;
- per-item stack rules;
- quest item lock rules;
- equipment slot rules;
- durability policy references.

## Dependencies between files

- `item_schema.lua` is the normalizer contract for item definitions.
- `item_catalog_generator.lua` emits data intended to be accepted by `item_schema.lua`.
- `loot_table_generator.lua` can consume normalized or generated item arrays as `item_catalog`.
- `inventory_rules.lua` can consume normalized or generated item arrays as `item_catalog`.
- `tests/items_inventory_examples.lua` demonstrates the intended flow through dependency injection.

No module uses direct file loading or external dependencies.

## How to validate manually

1. Inspect ZIP contents and confirm the paths match Batch 011.
2. Validate `manifests/items_inventory.manifest.json` as JSON.
3. Load each Lua module in a Lua 5.4-compatible sandbox.
4. Confirm each module returns a table with `manifest`, `validate_config(config)` and `generate(input, ctx)`.
5. Run the manual example by injecting loaded module tables into `tests/items_inventory_examples.lua`.
6. Confirm outputs are compact JSON-serializable tables.
7. Confirm item ids and pool ids use lowercase slash ids.
8. Confirm loot tables are definitions only and do not perform runtime random resolution.

## Known limitations

- The batch does not implement runtime inventory mutation.
- The batch does not implement crafting recipes.
- The batch does not implement economy pricing simulation.
- The batch does not implement combat stat formula evaluation.
- The batch does not resolve random loot drops; it only creates loot table IR.
- The batch does not create Unity assets directly.

## Next recommended batch

Batch 012 — Stats, formulas, progression.

## Non-goals confirmed

- No C# project files were modified.
- No Batch 012 files were generated.
- No external Lua dependencies were introduced.
- No huge item catalogs or content dumps were generated.
