# Modular Kernel Compatibility Model

## Purpose

This document describes the target architecture for making LLMGameCreator modular, parallel-development-ready and suitable for very large generated games.

It does not replace active Goal/task files. It should guide future kernel/module/composite pack shaping.

## Target model

```text
Generator Kernel
  -> module registry
  -> contract registry
  -> compatibility matrix
  -> artifact router
  -> validator orchestration
  -> package/runtime assembly dispatcher
  -> deterministic replay/hash system

Capability Modules
  -> world topology
  -> entities/NPCs
  -> dialogue/quests
  -> items/economy/crafting
  -> combat/progression
  -> factions/social/law
  -> schedules/simulation
  -> semantic variation
  -> assets/presentation
  -> Unity runtime projection
```

## Kernel responsibilities

The kernel owns:

- module discovery;
- module manifest parsing;
- contract/version registry;
- dependency resolution;
- absence behavior;
- compatibility matrix;
- artifact root routing;
- diagnostics format;
- deterministic hash/replay rules;
- validation orchestration;
- package assembly dispatch.

The kernel must not encode every gameplay domain directly.

## Module responsibilities

A module owns:

- a focused domain;
- its input contracts;
- its output contracts;
- its validators;
- its generator/assembler/mapper;
- its deterministic fixtures;
- its artifact root;
- its smoke scenario;
- its absence behavior;
- its compatibility diagnostics.

## Absence behavior

A missing optional module must not crash the system.

Allowed absence outcomes:

- `absent_optional`;
- `future_required`;
- `preserved_sidecar`;
- `unsupported_with_diagnostic`.

Forbidden absence outcomes:

- silent success;
- fake implementation;
- null crash;
- hidden fallback that changes semantics without diagnostics.

## Dependency types

### Required dependency

The module cannot operate honestly without it. Missing required dependency rejects the module.

### Optional dependency

The module can operate in reduced mode without it and must explicitly declare the reduced behavior.

### Forbidden dependency

The module must not depend on it. Examples:

- live runtime LLM/RAG/provider/media;
- Unity build/player entrypoints in non-Unity goals;
- public GamePackage schema mutation when not scoped.

## Compatibility matrix

A compatibility matrix is an artifact that proves:

- all module manifests parse;
- all input contract ids exist;
- output contract ids are known;
- dependency graph has no forbidden cycle;
- optional absence paths are tested;
- required dependencies are present;
- version requirements are satisfied;
- module-owned artifact roots are disjoint;
- module-owned smoke scenarios are disjoint;
- shared files are untouched except in kernel/adoption tasks.

## Parallel development model

Parallel implementation is safe only after module ownership exists.

Candidate branches may add modules under owned paths. They must not update active state docs or accepted gate status.

Serial adoption is required to merge candidates into main.

## Example

Items/economy module outputs item/resource/equipment ids.

Combat/progression module may consume equipment/resource ids.

If items module is present:

```text
combat module validates weapon/equipment/resource references
```

If items module is absent:

```text
combat module preserves equipment references as sidecar/future_required
```

Either path is deterministic and diagnostic-rich.

## Verification model

Module-only change should need:

- module focused tests;
- module smoke via manifest;
- compatibility matrix;
- scope guard.

Kernel change should need:

- registry tests;
- all manifests parse;
- selected module smokes;
- check-all;
- scope guard.

Campaign/product vertical should be rare and may require broader proof.
