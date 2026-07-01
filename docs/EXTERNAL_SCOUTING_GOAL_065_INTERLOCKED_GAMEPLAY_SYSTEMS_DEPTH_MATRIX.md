# External scouting — Goal 065 Interlocked Gameplay Systems Depth Matrix

## Decision

Do not add external dependencies for Goal 065.

Goal 065 is a state-changing gameplay proof over existing LLMGameCreator evidence and Unity Alpha marker paths. The target is not a generic game framework, ECS, rules engine, or economy simulator. The goal needs deterministic, JSON-friendly, causal evidence that the existing full-campaign rows now carry deeper interlocked gameplay consequences.

## Considered categories

- ECS frameworks such as Arch/DefaultEcs/Entitas: useful later for Runtime-scale simulation, but premature for an Application-layer proof. They would risk locking a runtime architecture before the generated package/gameplay shape stabilizes.
- Rules engines and workflow/state-machine libraries: useful later for authoring or UI-friendly rule editing, but Goal 065 should stay with explicit domain records and causal diagnostics.
- Economy simulators/pathfinding/tactics libraries: not required for the first interlocked gameplay matrix. The goal can use bounded deterministic ledgers and state deltas.
- WFC/MarkovJunior/TextureSynthesis family: important for future spatial/tile/detail generation. Goal 062 already introduced constrained spatial detail. Goal 065 should build on that, not rework spatial generation.

## Architectural direction

Goal 065 should remain BCL-only and Application-layer. It should consume prior proof artifacts and produce deterministic interlocked gameplay rows:

- economy/resource ledger;
- crafting/recipe/resource conversion ledger;
- combat/encounter/progression ledger;
- skill/status/effect ledger;
- loot/reward/equipment deltas;
- family/seed variance and replay proof;
- Unity Alpha markers proving the rows are consumed by the existing Alpha proof route.

No provider, LLM, RAG, media generation, broad Unity work, GamePackage schema change, Runtime schema change, arbitrary Lua execution, or new external dependencies are allowed.
