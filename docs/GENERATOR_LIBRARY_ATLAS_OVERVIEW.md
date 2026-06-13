# Generator Library Atlas Overview

Status: seed architecture document  
Version: 0.1  
Related index: `generator-library/atlas/ATLAS_INDEX.md`

## Purpose

The Generator Library Atlas is the planning spine for LLMGameCreator.

It exists to prevent this failure mode:

```text
new idea
  -> random prompt
  -> random JSON
  -> random Lua
  -> custom C# glue
  -> custom Unity glue
  -> impossible-to-maintain pile
```

The intended flow is:

```text
user idea
  -> negotiated game profile
  -> selected feature bundles
  -> named artifact contracts
  -> ordered generator plan
  -> selected prompt context packs
  -> model/Lua draft outputs
  -> deterministic validation
  -> staged artifacts
  -> human approval when needed
  -> compiled runtime DB / Unity IR / asset index later
```

The atlas is not the game engine. It is not the runtime. It is not a replacement for C# validation or Unity adapters.
It is the map that keeps future generation modular and bounded.

## Why this matters

The project goal is not to make one narrow game generator.
The goal is to support many possible game styles through reusable modules and contracts:

```text
- party RPG;
- pseudo-3D exploration;
- city builder with trade/war/conquest;
- narrative political RPG;
- visual novel routes;
- automation/factory systems;
- survival sandbox;
- tactical RPG;
- dungeon crawler;
- colony sim;
- optional adult/NSFW content overlays;
- optional horror/gore overlays;
- future images, audio and animation requests.
```

That flexibility needs a strict planning layer. Otherwise every feature becomes a one-off implementation.

## The atlas layers

### 1. Capability Atlas

Defines reusable domains and capabilities.

A capability answers:

```text
What can the system understand, generate, validate or export?
```

Examples:

```text
inventory.paper_doll_grid/v1
combat.realtime_turn_hybrid/v1
dialogue.choice_graph/v1
runtime_db.build_plan/v1
unity_ir.runtime_shell/v1
content.rating_and_filters/v1
```

### 2. Reference Profiles

Reference profiles are not mandatory templates. They are architecture stress tests and starting points.

Examples:

```text
profile/might_and_magic_like_party_rpg/v1
profile/anno1404_like_city_builder_conquest/v1
profile/the_last_sovereign_like_narrative_political_rpg/v1
```

A real project can combine inspirations.

### 3. Artifact Contracts

Artifact contracts define what generators are allowed to produce.

Examples:

```text
semantic_pack_v1
text_pack_v1
dialogue_pack_v1
formula_pack_v1
content_overlay_pack_v1
runtime_db_build_plan_v1
unity_ir_v1
asset_request_pack_v1
audio_request_pack_v1
animation_request_pack_v1
```

Without contracts, generated output becomes unbounded and difficult to validate.

### 4. Validation Pipeline

Validation decides whether an artifact can move forward.

The default levels are:

```text
level 0 — JSON and shape
level 1 — IDs, enums and references
level 2 — contract semantics
level 3 — cross-artifact consistency
level 4 — headless smoke test
level 5 — export dry-run
level 6 — human approval
```

Human approval should be used for important architecture, canon, overlay and runtime-impact decisions, not for every generated item.

### 5. Library Growth Pipeline

This defines how the generator library expands safely.

A missing capability should become:

```text
capability gap
  -> library growth proposal
  -> staged spec
  -> strict generation prompt
  -> generated files
  -> static checks
  -> examples checked
  -> registry preview
  -> approval
  -> activation
```

The model should not silently create active features.

### 6. Runtime DB and Unity Export Map

Final Unity runtime should not read an unbounded set of generated JSON files.

The intended compiled outputs are:

```text
compiled/runtime.db
compiled/save_schema.db
compiled/asset_index.json_or_db
compiled/unity_ir.json_or_db
compiled/build_manifest.json
saves/save_001.db
```

The future runtime should consume compiled data and IR, not generator history.

### 7. Model Workflow Roles and Prompt Families

Model usage should be role-based.

Examples:

```text
large designer model -> concept, lore, profile negotiation, high-level semantic design
small fast batch model -> strict JSON artifacts, semantic variants, repair attempts
validator/scorer model -> sample quality review, never final authority
deterministic C# validator -> formal pass/fail gates
```

### 8. Prompt Context Pack Map

The system should not dump everything into every prompt.

Context packs should be selected by:

```text
- role;
- task kind;
- artifact contract;
- output schema;
- canonical enums;
- profile;
- tags;
- validation failure;
- budget class.
```

### 9. Game Profile Negotiation Map

A game profile is a compact planning contract.

It should declare enough to choose capabilities and build plans:

```text
- player fantasy;
- reference inspirations;
- runtime targets;
- view mode;
- world scale;
- combat/time mode;
- party/single/city/faction role;
- inventory/equipment style;
- content overlays;
- media generation scope;
- persistence/export strategy;
- validation focus.
```

### 10. Feature Bundle Map

Feature bundles are planning units.

They connect:

```text
game profile
  -> capabilities
  -> artifact contracts
  -> validators
  -> prompt context templates
  -> runtime targets
  -> Unity IR groups
  -> runtime DB groups
  -> generator gaps
```

### 11. Generator Plan Map

Generator plans turn selected bundles into ordered steps.

Every step should declare:

```text
- producer role;
- inputs;
- context pack template;
- expected artifact contract;
- validation gates;
- repair policy;
- success/failure behavior;
- promotion target.
```

### 12. Examples

The examples show concrete profile-to-plan flows:

```text
Might-and-Magic-like party RPG
Anno-like city builder with conquest
Narrative political RPG / visual novel
```

They are intentionally small. They are examples of bounded planning, not full game generation.

## Optional adult/NSFW content model

Adult/NSFW support is a cross-genre optional content overlay.

It can apply to:

```text
- text;
- dialogue;
- relationship routes;
- scene metadata;
- image requests;
- audio/voice requests;
- future animation requests.
```

It must remain:

```text
- explicit;
- tagged;
- filterable;
- export-aware;
- separate from core mechanics;
- enabled only by project/profile selection.
```

This makes it possible to support mature projects without turning every profile into an adult project by default.

## What is intentionally not implemented yet

This seed architecture does not yet implement:

```text
- C# atlas importer;
- C# validators for these atlas files;
- UI pages for atlas browsing;
- Lua execution pipeline;
- artifact execution engine;
- runtime.db compiler;
- Unity exporter;
- Unity runtime shell;
- asset generation pipeline;
- prompt runner integration.
```

That is intentional. The current stage defines boundaries and data contracts before implementation.

## Recommended next implementation milestone

The first real implementation milestone should be small:

```text
Milestone 1 — Atlas Registry Import Preview
```

Goal:

```text
Read generator-library/atlas/*.json and show/import them as read-only registry data.
```

It should not execute Lua, generate content, mutate GamePackage, export Unity or compile runtime.db.

Suggested scope:

```text
- discover atlas JSON files;
- parse them as generic JSON documents;
- validate required top-level fields: schema_version/id/title/purpose where applicable;
- list files in diagnostics/report;
- expose a read-only summary in Design DB or diagnostics first;
- do not create complex UI yet unless needed.
```

This milestone is suitable for a small C# patch or Codex task later, but only after the documentation/data seed is stable.

## Practical next steps

Before coding:

```text
1. Keep atlas seed files small and reviewable.
2. Add examples only when they clarify a real planning path.
3. Avoid adding giant generated content to the atlas.
4. Decide which C# service should own atlas import later.
5. Use Codex only for bounded C# importer/validator patches, not for broad architecture discovery.
```

The atlas is now strong enough to support small implementation tasks without asking Codex to invent the architecture.
