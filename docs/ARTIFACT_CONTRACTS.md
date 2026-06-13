# Artifact Contracts

Status: seed architecture document  
Version: 0.1  
Related files:

```text
generator-library/atlas/artifact_contracts.json
generator-library/atlas/capability_atlas.json
generator-library/atlas/reference_profiles.json
```

## Purpose

Artifact contracts define what LLMGameCreator is allowed to produce, validate, stage, approve, compile and export.

The goal is to avoid an endless pile of loosely connected Lua files, JSON files, generated prompts and future Unity adapters. Every generated output should be one of the known artifact contracts, or it should enter a separate review path as a proposed new contract.

This keeps the system extensible without requiring a new C# subsystem every time a new gameplay idea appears.

## Core rule

```text
Capability -> Generator Plan -> Artifact Contract -> Validation -> Promotion -> Runtime/Export
```

A generator does not produce arbitrary data. It produces a named contract.

A contract defines:

```text
- what the artifact is for;
- who may produce it;
- who may consume it;
- required sections;
- validation levels;
- promotion rules;
- forbidden behavior;
- runtime/export implications.
```

## Non-goals

Artifact contracts do not execute Lua, run Unity, generate C# by themselves, apply package changes automatically, or turn LLM output into trusted runtime state.

They are data definitions and pipeline boundaries.

## Why this matters

Without artifact contracts, every new feature becomes a custom integration problem:

```text
new mechanic -> new JSON -> new Lua -> new C# -> new UI -> new Unity runtime glue
```

With artifact contracts, new work is constrained:

```text
new mechanic -> existing contract, or proposed contract -> validator -> exporter/runtime path
```

This is the difference between a growing architecture and a pile of one-off generators.

## Artifact lifecycle

The standard lifecycle is:

```text
draft
  -> generated
  -> parsed
  -> normalized
  -> validated
  -> staged
  -> approved
  -> compiled
  -> exported
  -> runtime_loaded
```

Not every artifact reaches every stage. For example, an image request pack may stop at `staged` until an image generation pipeline exists. A runtime DB build plan may reach `compiled` only when the C# builder exists.

## Validation levels

The seed registry defines these validation levels:

```text
Level 0 — JSON and shape validation
Level 1 — IDs, enums and references
Level 2 — contract semantics
Level 3 — cross-artifact consistency
Level 4 — headless smoke test
Level 5 — export dry-run
Level 6 — human approval
```

The point is not to inspect everything manually. The point is to make most checks automatic and reserve human review for high-impact changes:

```text
- new capability domains;
- new artifact contracts;
- save compatibility;
- adult/NSFW overlay enablement;
- story canon changes;
- runtime/export target changes;
- generated code or Unity project mutation.
```

## Contract groups

### Planning and capability artifacts

These describe what kind of game is being made and which capabilities are needed.

Examples:

```text
game_profile_v1
feature_bundle_v1
generator_plan_v1
```

### Game content and semantic artifacts

These describe game meaning, mechanics, text, dialogue, formulas, properties and optional content layers.

Examples:

```text
semantic_pack_v1
property_channel_pack_v1
rule_pack_v1
formula_pack_v1
text_pack_v1
morphology_pack_v1
dialogue_pack_v1
free_text_dialogue_intent_pack_v1
content_overlay_pack_v1
game_package_patch_v1
```

### Runtime export artifacts

These prepare validated game data for a final runtime target, especially Unity.

Examples:

```text
unity_ir_v1
runtime_db_build_plan_v1
asset_index_v1
save_overlay_schema_v1
```

### Media generation request artifacts

These do not represent imported assets. They represent requests/prompts/plans for generating media later.

Examples:

```text
asset_request_pack_v1
image_request_pack_v1
audio_request_pack_v1
animation_request_pack_v1
```

### Validation and audit artifacts

These record deterministic checks, dry-runs, smoke tests and diagnostics.

Examples:

```text
validation_report_v1
headless_smoke_test_plan_v1
```

## Adult/NSFW content overlay

Adult/NSFW content is not a separate genre. It is an optional content overlay that may be enabled for many game profiles: party RPG, sandbox, political RPG, visual novel, survival, colony sim and others.

It must be explicit and tagged.

The architecture should support:

```text
- adult/NSFW text routes;
- adult/NSFW image requests;
- future adult/NSFW animation requests;
- relationship intensity tags;
- export target filtering;
- platform-specific constraints;
- project-level enable/disable decisions.
```

The overlay must not be hidden inside unrelated mechanics. A combat system, inventory system, city builder system or dialogue system should keep working when the overlay is disabled.

That is why `content_overlay_pack_v1` is separate from `dialogue_pack_v1`, `text_pack_v1`, `image_request_pack_v1` and runtime mechanics.

## Lua role

Lua modules should produce or transform data under known contracts.

Good Lua role:

```text
Lua generator -> semantic_pack_v1
Lua generator -> rule_pack_v1
Lua generator -> game_package_patch_v1
Lua generator -> unity_ir_v1
Lua generator -> runtime_db_build_plan_v1
```

Bad Lua role:

```text
Lua freely mutates runtime state
Lua writes files directly
Lua generates arbitrary C#/Unity code without IR
Lua bypasses validation
Lua applies GamePackage changes automatically
```

## LLM role

The LLM may create drafts, propose semantic content, generate text variants, design feature bundles, suggest rules, write media generation requests and repair invalid outputs.

The LLM should not be treated as final authority.

Preferred split:

```text
Large local model:
  - lore discussion;
  - game concept;
  - story and faction design;
  - complex NPCs;
  - style and tone.

Small local model:
  - batch JSON generation;
  - semantic packs;
  - text packs;
  - item/location/NPC variants;
  - media request drafts;
  - repetitive enrichment tasks.

C# services:
  - strict parsing;
  - normalization;
  - validation;
  - deduplication;
  - promotion;
  - compilation;
  - export.
```

## Unity/runtime role

The final Unity runtime should not read thousands or hundreds of thousands of loose JSON files.

Preferred output:

```text
runtime.db        — read-only compiled game content
save_001.db       — mutable save/world state overlay
asset_index       — asset bindings and imported/generated media refs
unity_ir          — scene/UI/prefab/component/input/audio/VFX bindings
build_manifest    — versions, hashes and compatibility metadata
```

Unity should consume compiled data and stable bindings, not the generator library directly.

## Runtime DB separation

Keep immutable content and mutable player state separate.

```text
runtime.db:
  - entities
  - items
  - skills
  - spells
  - dialogues
  - text templates
  - semantic traits
  - location/chunk definitions
  - quests
  - factions
  - formulas
  - asset refs
  - UI definitions

save.db:
  - world variables
  - entity state
  - NPC memory
  - relationships
  - inventory state
  - quest state
  - chunk deltas
  - visited locations
  - runtime event log
```

This makes game updates, migrations and performance easier to reason about.

## How to add a new feature without endless development

A new feature should follow this route:

```text
1. Describe the gameplay feature.
2. Map it to existing capability domains.
3. Map it to existing artifact contracts.
4. If no contract fits, propose one new contract.
5. Define validators before implementation.
6. Generate or write a small data example.
7. Run deterministic validation.
8. Only then add C#/Unity runtime support if needed.
```

For example, a Might-and-Magic-like paper-doll inventory should not start as Unity UI code.

It should start as:

```text
feature_bundle_v1
property_channel_pack_v1
rule_pack_v1
unity_ir_v1
runtime_db_build_plan_v1
headless_smoke_test_plan_v1
```

Then C#/Unity work becomes a bounded adapter task, not open-ended architecture work.

## Seed contract list

The seed registry currently defines these contracts:

```text
game_profile_v1
feature_bundle_v1
generator_plan_v1
game_package_patch_v1
semantic_pack_v1
property_channel_pack_v1
rule_pack_v1
formula_pack_v1
text_pack_v1
morphology_pack_v1
dialogue_pack_v1
free_text_dialogue_intent_pack_v1
content_overlay_pack_v1
unity_ir_v1
runtime_db_build_plan_v1
asset_request_pack_v1
image_request_pack_v1
audio_request_pack_v1
animation_request_pack_v1
asset_index_v1
save_overlay_schema_v1
validation_report_v1
headless_smoke_test_plan_v1
```

This list is intentionally broad enough for very different project profiles, but still small enough to avoid a million-line architecture.

## Next practical steps

Do not implement all of this in C# immediately.

Recommended next steps:

```text
1. Keep this file and artifact_contracts.json as data/doc only.
2. Add small examples for 2-3 contracts.
3. Add a lightweight validator later.
4. Import the registry into Design DB only after the static shape stabilizes.
5. Use Codex only when C# integration is unavoidable.
```

The immediate purpose is architectural stabilization, not feature explosion.
