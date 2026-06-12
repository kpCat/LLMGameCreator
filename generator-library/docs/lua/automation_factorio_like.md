# Lua Batch 015 — Automation / Factorio-like systems

## Purpose

Batch 015 adds reusable deterministic automation modules for Factorio-like generator-library planning. The modules emit compact JSON-serializable IR and diagnostics. They do not execute a live game runtime.

## Files

- `lua/automation/recipe_graph.lua`
- `lua/automation/machine_catalog.lua`
- `lua/automation/conveyor_grid.lua`
- `lua/automation/power_network.lua`
- `manifests/automation.manifest.json`
- `tests/automation_examples.lua`

## Module overview

### `automation/recipe_graph/v1`

Builds normalized recipe graph IR from compact recipe definitions.

Supports:

- recipe ids;
- recipe categories;
- item input/output stacks;
- resource items;
- target production chains;
- deterministic first-producer selection;
- missing producer diagnostics;
- cycle diagnostics;
- compact throughput estimates.

The module emits IR only. It does not run factories frame-by-frame.

### `automation/machine_catalog/v1`

Builds machine catalog IR for recipe categories and future generator planning.

Supports:

- machine ids;
- recipe category support;
- speed multipliers;
- power demand metadata;
- module slots;
- compact placement footprint metadata;
- category-to-machine map.

### `automation/conveyor_grid/v1`

Builds logistics graph IR for belts, splitters, mergers, inserters, ports and chests.

Supports:

- logistics node ids;
- 0-based tile positions;
- directed links;
- bidirectional links;
- lane capacity metadata;
- item filters;
- deterministic adjacency rows.

This is not a path solver and not a belt physics simulation.

### `automation/power_network/v1`

Builds power-network IR and deterministic balance estimates.

Supports:

- generators;
- consumers;
- accumulators;
- capacity and demand totals;
- reserve ratio;
- deficit and reserve diagnostics.

## Design constraints

- Lua 5.4-compatible.
- No external dependencies.
- No unsafe Lua APIs.
- No filesystem, network or process access.
- No direct random calls.
- No global writes.
- Every module returns a table.
- Every module exposes `manifest`, `validate_config(config)` and `generate(input, ctx)`.
- Normal validation failures return diagnostics instead of thrown errors.
- Outputs are JSON-serializable.
- Modules remain standalone and do not load each other.

## Expected composition

A planner can use this batch as:

1. Generate a recipe graph.
2. Generate a machine catalog.
3. Map recipe categories to machines.
4. Generate logistics IR between resource ports, machines and storage.
5. Generate power-network IR and estimate whether demand is feasible.
6. Feed all outputs to future validation, UI IR, simulation or Unity adapter modules.

## Non-goals

- No C# integration.
- No Unity object generation.
- No Lua executor integration.
- No large recipe or item catalogs.
- No live production simulation.
- No live conveyor simulation.
