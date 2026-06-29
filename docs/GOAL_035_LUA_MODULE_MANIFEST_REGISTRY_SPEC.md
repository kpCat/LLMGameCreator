# Goal 035 — Lua Module Manifest Registry Spec

## Goal id

```text
goal_035_lua_module_manifest_registry
```

## Gate

```text
lua_module_manifest_registry_verification required
```

## Summary

Create a BCL-only Application-layer Lua module manifest registry that defines how future Lua/manual/import/LLM-generated modules are declared, reviewed, selected and bounded before any execution is allowed.

This goal is **not** a Lua execution goal. It must not execute Lua, parse Lua source, call providers, touch Runtime, touch Unity, touch WinForms, touch generator-library, or change public `GamePackage` schema.

## Why this is the next composite goal

Goal 034 created a strict quarantined draft loop. The next risk is future script/code-like content. Lua can be useful later for typed gameplay modules, but only if the project first has:

- manifest contracts;
- allowed/denied API surfaces;
- module family registry;
- dependency and compatibility validation;
- promotion/review status;
- integration points with semantic features and authoring intents;
- invalid/fake/leak diagnostics.

Without this layer, future Lua work will either become unsafe arbitrary scripting or force a premature interpreter/runtime choice.

## Core architectural rule

```text
Lua modules are never trusted because they are Lua.
Lua modules are trusted only when their manifests are contract-bound, reviewed, compatible, budgeted, dependency-resolved and selected by deterministic policy.
```

## Required model capabilities

### Manifest identity

Each module manifest must include:

- stable module id;
- family id;
- version;
- display name;
- lifecycle status;
- source/provenance;
- target dialect declaration;
- owner stage;
- deterministic ordering key.

### Families

At minimum seed these module families as manifest records or family definitions:

- world generation hints;
- region/biome/weather/hazard rules;
- NPC/species/archetype rules;
- faction/reputation/social relation rules;
- quest/objective/reward rules;
- dialogue act/tone/localization hint rules;
- item/resource/recipe/loot/economy rules;
- combat/stat/ability/status rules;
- settlement/building/landmark rules;
- event/global pressure rules;
- metamodule species/archetype expansion rules.

### Host API surface policy

Represent host APIs as declarations only. Include:

- API group id;
- status: ready/optional/blocked/future-required/deprecated;
- allowed operation kinds;
- denied operation kinds;
- side-effect class;
- required artifact contracts;
- required semantic scopes;
- diagnostic code prefix.

Must explicitly deny/leak-detect at least:

- file system;
- network;
- OS/process;
- reflection;
- provider/LLM/RAG;
- UI/WinForms;
- Runtime direct mutation;
- Unity direct calls;
- GamePackage schema mutation;
- arbitrary code generation;
- arbitrary Lua execution as an implicit promotion path.

### Selection and planning

Implement a deterministic planner that takes a scenario/profile and selected semantic/intent context, then returns:

- selected module manifests;
- dependency order;
- missing dependencies;
- blocked/future-required modules;
- denied API usage diagnostics;
- compatibility diagnostics;
- manifest gaps;
- stable summary.

Required scenarios:

- `frontier_survival`;
- `gothic_intrigue`;
- `caravan_trade`;
- `metamodule_kingdoms`.

### Goal 034 integration

Goal 035 must not call Goal 034 as a provider. It should model compatibility with Goal 034 concepts:

- draft module manifest request family;
- quarantined candidate source;
- promotion status;
- repairable diagnostics;
- provenance mismatch rejection.

### Evidence artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-035-lua-module-manifest-registry/
```

Required files:

```text
lua-module-registry-summary.json
lua-host-api-surface-policy.json
lua-module-selection-frontier.json
lua-module-selection-gothic.json
lua-module-selection-caravan.json
lua-module-selection-metamodule-kingdoms.json
lua-module-dependency-plan.json
invalid-lua-manifest-diagnostics-matrix.json
lua-module-manifest-registry-report.md
```

No timestamps, no absolute paths, no heavy logs, stable ordering.

## Non-goals

- no Lua execution;
- no Lua parser;
- no MoonSharp/NLua/KeraLua/Lua-CSharp dependency;
- no runtime host binding;
- no generated Lua source;
- no GamePackage materialization;
- no UI/WinForms;
- no Unity;
- no provider/LLM/RAG calls;
- no generator-library changes.

## Expected final status

This goal should leave the gate at:

```text
lua_module_manifest_registry_verification required
```

Manual acceptance is separate unless the user explicitly requests it.
