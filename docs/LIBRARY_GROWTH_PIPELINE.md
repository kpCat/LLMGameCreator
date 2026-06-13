# Library Growth Pipeline

Status: seed architecture document  
Version: 0.1  
Related files:

```text
generator-library/atlas/library_growth_pipeline.json
generator-library/atlas/capability_atlas.json
generator-library/atlas/artifact_contracts.json
generator-library/atlas/validation_pipeline.json
docs/CAPABILITY_ATLAS.md
docs/ARTIFACT_CONTRACTS.md
docs/VALIDATION_PIPELINE_ATLAS.md
```

## Purpose

The Library Growth Pipeline defines how LLMGameCreator may safely grow its generator library without turning every new gameplay idea into a custom C# subsystem or an unsafe pile of generated Lua.

The core idea is simple:

```text
capability gap
  -> library growth proposal
  -> staged spec
  -> strict generation prompt
  -> staged files
  -> static checks
  -> examples / registry preview
  -> approval
  -> active library
```

This keeps the system extensible while preserving deterministic validation and runtime boundaries.

## Core rule

Generated files are not automatically trusted.

A model may draft data, docs, manifests, examples, restricted Lua modules, semantic packs or IR builders, but those outputs remain staged until the validation pipeline promotes them.

```text
LLM proposes.
C# parses, normalizes, validates and stages.
Human approval is required for architecture-impacting changes.
Runtime executes deterministic compiled data.
```

## Proposal kinds

The seed pipeline recognizes these proposal kinds:

```text
capability_gap
atlas_data_patch
lua_generator_module
validator_rule
artifact_contract
unity_ir_adapter
```

Small `atlas_data_patch` work can usually be done as low-risk data/doc changes.

New Lua modules, artifact contracts, validator rules and Unity IR adapters require stronger gates because they can affect future generation, validation or export.

## Why this matters

Without a library growth pipeline, new ideas tend to become one-off work:

```text
new mechanic -> new Lua -> new JSON -> new C# glue -> new UI -> new Unity glue
```

That is exactly what this architecture is trying to avoid.

With the pipeline, new ideas first become capability/contract/spec changes:

```text
new mechanic -> feature/capability proposal -> known artifact contracts -> validators -> exporter/runtime path
```

C# still matters, but it becomes the stable platform for parsing, validating, compiling and exporting known contracts, not a custom bridge for every new generated file.

## Staged growth flow

### 1. Detect gap

A gap can come from:

```text
- user request;
- game profile planning;
- missing capability during generator-plan creation;
- failed export planning;
- missing validation rule;
- missing artifact contract;
- missing Unity runtime adapter.
```

The output is `library_growth_proposal_v1`.

### 2. Draft spec

The spec describes:

```text
- what capability is missing;
- which artifact contract it produces or consumes;
- required input/output/config schema;
- validators;
- dependencies;
- runtime targets;
- examples;
- docs;
- content overlay behavior, if relevant.
```

For adult/NSFW-aware additions, the spec must keep the optional content overlay explicit and filterable. Core mechanics must still work when the overlay is disabled.

### 3. Prepare strict prompt

The prompt must state exact paths, exact contracts, forbidden APIs, manifest fields, output schema and validation expectations.

For Lua modules, it must require:

```text
manifest
validate_config(config)
generate(input, ctx)
deterministic output
JSON-serializable data only
diagnostics instead of thrown errors for normal validation failures
no io/os/debug/package/load/loadfile/dofile/network/filesystem
```

### 4. Generate staged files

Files are generated into a staged file set. They are not active yet.

Possible staged files:

```text
generator-library/lua/...
generator-library/manifests/...
generator-library/docs/...
generator-library/tests/...
generator-library/atlas/...
docs/...
```

### 5. Static checks

Static checks must include at least:

```text
path layout
manifest contract
Lua safety
artifact contract compatibility
content overlay boundaries
```

Normal validation failures should produce diagnostics. They should not crash the whole pipeline unless they cross a trust boundary.

### 6. Examples and registry preview

Before activation, the system should prove that the staged work can be understood by the registry and examples.

For data/doc seeds this may be just JSON parsing and reference checks.

For Lua modules this should include manifest integrity, no unsafe APIs and example outputs.

### 7. Review and activate

Activation means the proposal is now part of the active generator library and may influence capability selection, generator plans or export planning.

Activation does not mean:

```text
- automatic GamePackage mutation;
- automatic Unity export;
- unrestricted Lua execution;
- bypassing artifact contracts;
- silently enabling optional adult/NSFW content.
```

## Model role boundaries

A larger designer model may be used for:

```text
- game profile discussion;
- lore and style;
- capability gap analysis;
- feature bundle design;
- strict prompt drafting.
```

A smaller batch model may be used for:

```text
- semantic packs;
- text packs;
- examples;
- data/doc seeds;
- repairable JSON fixes;
- repeated deterministic generation tasks.
```

Neither model is a trusted runtime authority.

## Approval policy

Manual approval should be reserved for high-impact changes:

```text
- new capability domains;
- new artifact contracts;
- new runtime targets;
- new Unity IR adapters;
- new validator semantics;
- canon/story decisions;
- optional content overlay policy changes;
- generated source/codegen activation.
```

Manual approval should not be required for every generated item description, material rule or dialogue variant. Those should be handled by schema validation, normalization, repair loops, deduplication, scoring and sampling review.

## Non-goals

This pipeline does not execute Lua, run Unity, generate trusted C# directly, mutate GamePackage automatically or treat LLM output as verified runtime state.

It is a staged growth process for the generator library.

## Next implementation direction

The next low-risk step is still data/doc oriented:

```text
1. Keep refining atlas files.
2. Add a library_growth_proposal_v1 contract to artifact_contracts.json later.
3. Add validator expectations for atlas files.
4. Only after that add C# import/read UI for atlas data.
```

This avoids burning heavy coding-agent limits before the contracts are stable.
