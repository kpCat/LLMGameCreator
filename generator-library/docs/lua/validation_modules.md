# Batch 019 — Validation modules

## Purpose

Batch 019 adds deterministic validation modules for generated IR/config data in the generator library.

The modules validate plain tables supplied by the caller and return JSON-serializable diagnostics. They do not execute generated plans, do not execute Lua modules dynamically, do not run Unity, do not compile code, do not mutate game packages and do not access host resources.

## What these modules provide

- World validation for compact world, region, chunk, road, gate, bridge and reachability IR.
- Quest validation for quest ids, stages, objectives, transitions, completion conditions and effect references.
- Interaction validation for target requirements, interaction kinds, dialogue/quest/entity references and target mode compatibility.
- Module contract validation for generator module metadata, capability ids, dependencies, deterministic flags and unsafe feature declarations.

## Module responsibilities

### `validation/world_validation/v1`

`world_validation.lua` validates world and map metadata. It supports:

- map/world ids;
- region and chunk references;
- tile/walkability metadata checks;
- landmark, gate and bridge references;
- compact graph reachability from configured starts to objectives;
- blocked road, missing bridge, invalid gate and unreachable target diagnostics.

Reachability uses compact graph metadata and does not require huge tile arrays.

### `validation/quest_validation/v1`

`quest_validation.lua` validates quest and progression IR. It supports:

- quest ids;
- stage ids;
- objective ids;
- objective target references;
- completion condition references;
- stage transition references;
- reward/effect references as plain data;
- invalid condition diagnostics;
- missing stage/objective/target diagnostics;
- practical cyclic transition warnings.

### `validation/interaction_validation/v1`

`interaction_validation.lua` validates interaction, targeting and dialogue bridge IR. It supports:

- interaction ids;
- target requirements;
- inspect/talk/use/pickup/activate style metadata;
- interaction without target diagnostics;
- invalid target mode diagnostics;
- duplicate interaction ids;
- missing dialogue, quest, item and entity references;
- facing/same/adjacent/radius/manual target rule metadata checks.

### `validation/module_contract_validation/v1`

`module_contract_validation.lua` validates generator module contract metadata and dependency consistency. It supports:

- module id validation;
- capability id validation;
- dependency references;
- missing dependency diagnostics;
- duplicate module ids;
- duplicate local capability diagnostics;
- required manifest-like module fields;
- deterministic/runtime target flags;
- unsafe feature declarations;
- capability dependency mismatch diagnostics.

This module validates metadata tables passed to it. It does not dynamically inspect files or modules.

## Input shapes

All modules expose:

```text
validate_config(config) -> ok:boolean, diagnostics:array
generate(input, ctx) -> result table
```

Result table:

```text
{
  ok = boolean,
  data = {
    summary = {},
    ...
  },
  diagnostics = {},
  artifacts = {}
}
```

Diagnostics use this shape:

```text
{
  severity = "error" | "warning" | "info",
  code = "validation.category.problem",
  message = "Human-readable message",
  target = "path/or/id"
}
```

## Diagnostics strategy

- `error`: invalid structure, missing required id/reference, invalid target mode, invalid condition, missing dependency or unreachable mandatory objective.
- `warning`: suspicious but not always fatal data, such as transition cycles or declared unsafe features.
- `info`: reserved for future non-blocking validation notes.

Normal validation failures are returned as diagnostics. They are not thrown as exceptions.

## Example validation results

An unreachable objective may produce:

```text
severity = "error"
code = "validation.world.unreachable_objective"
target = "quest/find_water_source"
```

An invalid quest condition may produce:

```text
severity = "error"
code = "validation.quest.invalid_condition_type"
target = "quests[1].objectives[1].completion_conditions[1].type"
```

A missing module dependency may produce:

```text
severity = "error"
code = "validation.module_contract.missing_dependency"
target = "modules[1].depends_on[1]"
```

## Deterministic IR validation only

These modules operate on plain data already passed to them. They do not:

- execute generated plans;
- execute dynamic Lua modules;
- run a game runtime;
- run Unity;
- compile anything;
- write files;
- access network or host resources;
- mutate generated content.

## Future integration

Later C# import/plan validation and generator pipeline validation can call equivalent logic after sandbox integration exists, or can import these contracts as reference behavior. Batch 020 may use these validation outputs as artifact metadata, but Batch 019 does not depend on Batch 020.
