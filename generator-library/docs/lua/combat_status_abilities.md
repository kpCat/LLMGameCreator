# Batch 013 — Combat, status and abilities

This batch adds compact Lua generator modules for combat configuration IR. The modules are deterministic library assets and do not execute formulas, Lua source strings, Unity code, filesystem access or network access.

## Modules

- `combat/combat_schema/v1` defines combat modes, resources, action definitions and safe `formula_ref` metadata.
- `combat/turn_based_combat/v1` generates turn-based combat configuration IR with sides, action points, initiative, cooldown ticking, status duration ticking and optional dialogue-combat bridge metadata.
- `combat/status_effects/v1` generates status effect definitions with duration ticks, stacking, stat modifier references and tick/expire effect metadata.
- `ability/ability_catalog_generator/v1` generates compact ability definitions that reference safe formula IR and status effect ids.

## Contracts

Formula references are identifiers such as `formula/combat/basic_attack`. They are not executable code. A later formula module or host-side validator may resolve these identifiers against safe formula IR.

Dialogue-combat is represented as metadata and effect IR. Dialogue choices may affect morale, trust, suspicion or focus, but this batch does not generate dialogue nodes and does not run combat.

Status durations and cooldowns are expressed in ticks. The generated data remains compatible with turn-based, tactical, dialogue-combat and hybrid combat modes.

## Non-goals

- No C# project changes.
- No Lua sandbox/executor integration.
- No raw formula evaluation.
- No Unity object creation.
- No large ability or status catalogs.
