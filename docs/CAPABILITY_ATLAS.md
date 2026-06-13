# Capability Atlas Seed

Status: seed document/data, not runtime implementation.  
Version: 0.1

## Purpose

The Capability Atlas is a static, data-first map of what LLMGameCreator can eventually generate, validate, preview, export and run.

It is not a replacement for `GamePackage`, not a runtime database, and not an instruction to execute Lua. It is a planning and validation layer that describes:

- domains of game functionality;
- capabilities inside those domains;
- feature bundles composed from capabilities;
- artifact contracts that generators may produce;
- runtime targets such as Unity 2D, Unity 3D, pseudo-3D, visual novel, city-builder and headless simulation;
- validation levels that decide whether generated data can be previewed, applied or exported.

The goal is to avoid endless ad-hoc growth where every new mechanic requires manually wiring many C# files before the system even understands what the mechanic is.

## Files

```text
generator-library/atlas/capability_atlas.schema.json
generator-library/atlas/capability_atlas.json
generator-library/atlas/reference_profiles.json
docs/CAPABILITY_ATLAS.md
```

These files are intentionally data/documentation only. They should not change the C# build, not mutate `GamePackage`, not execute Lua and not generate Unity code.

## Core idea

Do not think in terms of endless numbered batches only.

Think in this order:

```text
Capability Atlas
    -> Feature Bundles
    -> Generator Plans
    -> Artifact Contracts
    -> Validation Pipeline
    -> Runtime DB Build Plan
    -> Unity IR / Unity Runtime Shell
```

Batches remain a delivery mechanism for Lua modules and docs, but the system should be guided by capabilities and contracts.

## LLM roles

LLMs may be used in different roles:

- `designer`: discuss lore, mechanics, game profile and high-level structure;
- `batch_generator`: produce many small JSON records or pack entries;
- `selector`: choose relevant context snippets for a generation request;
- `validator`: inspect generated artifacts for semantic problems;
- `repair`: fix invalid JSON or contract drift;
- `summarizer`: maintain compact context packs;
- `reviewer`: explain quality and risks.

The LLM must not be the final authority for runtime state. C# validators and explicit user approval own important state changes and library growth.

## Local model strategy

A practical split is:

```text
larger local model, e.g. 26B A4B:
- lore discussion;
- high-level game design;
- unique factions/NPCs/scenes;
- difficult narrative/style decisions.

smaller local model, e.g. E4B:
- batch JSON generation;
- semantic packs;
- material/item/location/NPC records;
- text variants;
- chunk seed records;
- repair and structured regeneration.
```

Both roles still require strict prompts, JSON schema, enum preservation, validation and logging.

## Adult / NSFW content layer

Adult/NSFW content is not modeled as a separate genre. It is an optional project-level content overlay that may apply to many genres when explicitly enabled by the creator.

The atlas therefore includes `content.rating_and_filters/v1` and optional content overlays. This is needed because games may need mature/adult content in text, images and future animation requests, especially for visual novels, narrative RPGs or free-form sandbox games.

Important constraints for architecture:

- adult content must be explicit in project settings, not accidental;
- generated material must be tagged and filterable;
- export targets may apply different filters;
- content tags must be stored separately from core mechanics;
- disabling adult content must not break the game simulation.

## Reference profiles

`reference_profiles.json` is not a final game list. It is a stress-test set for architecture flexibility.

Included profiles:

- Might-and-Magic-like party RPG;
- Anno-1404-like city builder with war/conquest;
- The-Last-Sovereign-like narrative political RPG;
- Factorio-like automation;
- survival sandbox;
- visual novel / dialogue RPG;
- alien / non-human perception game.

The point is not to implement all of them now. The point is to prevent the architecture from hardcoding only one genre.

## Minimal next steps

This seed intentionally avoids C# implementation. The next steps should stay small.

### Step 1 — commit atlas seed

Add the files listed above. No code changes.

Expected result:

- repository has a first static capability atlas;
- future tasks can reference exact capability ids and feature bundle ids;
- no runtime behavior changes.

### Step 2 — add integrity-only atlas validator

Later, add a small validator that checks:

- JSON parses;
- schema file exists;
- capability ids are unique;
- feature bundle references point to existing capability ids;
- artifact contract ids exist;
- runtime target ids exist.

No import into Design DB yet.

### Step 3 — import atlas into Design DB

Only after static validation works, import atlas metadata into the Design DB.

This should store capabilities, feature bundles, artifact contracts and runtime targets as searchable planning metadata. It must not execute Lua or apply anything to `GamePackage`.

### Step 4 — profile selection

Add a profile selection draft flow:

```text
user concept
    -> suggested reference profile / custom profile
    -> selected feature bundles
    -> missing capability warnings
    -> draft generator plan
```

### Step 5 — artifact contract registry

Add explicit contract definitions for:

- `semantic_pack_v1`;
- `text_pack_v1`;
- `morphology_pack_v1`;
- `unity_ir_v1`;
- `runtime_db_build_plan_v1`;
- `content_rating_pack_v1`;
- `generator_library_module_proposal_v1`.

### Step 6 — runtime DB build plan prototype

Do not make Unity read thousands of JSON files. The final path should be:

```text
GamePackage + packs + Unity IR
    -> validated build plan
    -> compiled runtime.db
    -> Unity Runtime Shell reads indexed data
```

Recommended split:

```text
runtime.db  = read-only compiled content
save_001.db = mutable save/world state overlay
```

### Step 7 — Unity IR foundation

Unity code should not be generated directly from arbitrary LLM/Lua output.

Preferred path:

```text
LLM/Lua/C# generators
    -> validated Unity IR
    -> deterministic C# exporter
    -> Unity Runtime Shell data/adapters
```

## What this avoids

This avoids:

- endless direct C# wiring for every new idea;
- uncontrolled Lua scripts changing runtime state;
- LLM-generated C#/Unity code as the first source of truth;
- giant JSON dumps as final runtime format;
- reading tens of thousands of files in Unity runtime;
- treating adult content as untracked text rather than explicit content metadata.

## What this enables

This enables:

- modular growth;
- capability-driven planning;
- small Codex tasks when code is actually needed;
- local LLM batch generation with strict prompts and validation;
- optional adult/mature content as a controlled project layer;
- future Unity 2D/3D/pseudo-3D/isometric/visual novel/city-builder runtime targets;
- runtime DB compilation for performance;
- safer generator-library growth.
