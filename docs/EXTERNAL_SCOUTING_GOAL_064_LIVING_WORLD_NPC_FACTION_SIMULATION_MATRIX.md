# External scouting — Goal 064 Living World NPC/Faction Simulation Matrix

## Purpose

This note records the dependency decision for Goal 064. The goal is to add living-world depth after the full campaign/package/spatial/gameplay consequence proof chain: NPC/faction state, relationships, memory-like event traces, schedules/availability, world events and consequence propagation.

## Scouting summary

### ECS libraries considered

- `DefaultEcs` (`Doraku/DefaultEcs`) — accessible Entity Component System for C# with an MIT-0 license. Useful later if the runtime simulation becomes large enough to need ECS-style storage/querying.
- `Arch` (`genaray/Arch`) — high-performance C# archetype/chunk ECS, Apache-2.0. Useful later for performance-sensitive simulation/runtime work, but too much dependency surface for this contract/evidence goal.
- `Entitas` (`sschmid/Entitas`) — MIT ECS, historically Unity-friendly, but it often implies a stronger ECS workflow and optional code-generation/style conventions that are not needed for Goal 064.

### Decision for Goal 064

Do **not** add any external ECS dependency. Implement a BCL-only Application-layer simulation seam using compact deterministic records, ordered IDs, explicit ticks, causal diagnostics and JSON evidence.

Rationale:

- The current project needs deterministic proof and clear evidence more than high-performance ECS storage.
- Adding ECS now could prematurely constrain future Runtime/Unity architecture.
- The first living-world layer should be domain-contract-first: actors, factions, relations, ticks, schedules, events, consequences, replay and save/load proof.
- ECS can remain a future optional adapter once the simulation shape is stable.

## Future note

After the living-world semantics are proven, a later performance/runtime goal may compare `DefaultEcs`, `Arch`, `Entitas`, Unity ECS/DOTS and an in-house ECS-like state store. Do not make that decision in Goal 064.
