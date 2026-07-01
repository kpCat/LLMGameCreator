# Goal 068 — Combat/Magic/Ability/Boss Encounter Matrix

## Goal id

`goal_068_combat_magic_ability_boss_encounter_matrix`

## Gate

`combat_magic_ability_boss_encounter_matrix_verification required`

## One-line purpose

Turn the current generated 3 family × 3 seed campaign rows into deterministic state-changing combat/magic/ability/boss encounter rows with active/passive abilities, statuses, counterplay, loot/progression, save/load/replay proof and Unity Alpha markers.

## Why now

The generator already has:

- GamePackage materialization matrix;
- playable review package RC;
- constrained spatial detail;
- gameplay consequence depth;
- living-world NPC/faction simulation;
- interlocked economy/crafting/combat/progression/status proof;
- settlement construction/destruction/production;
- programmatic narrative quest/dialogue/event consequences.

The next weakness is gameplay depth around combat/magic/abilities/bosses. Goal 065 proved interlocked systems at a broad level. Goal 068 must deepen combat and magic into a reusable generated encounter model without adopting a Unity ability framework or changing public GamePackage schema.

## Non-goals

Do not implement:

- public GamePackage schema changes;
- Runtime/Runtime.Abstractions gameplay code;
- WinForms UI;
- broad Unity gameplay implementation;
- Unity ScriptableObjects;
- external ability framework dependencies;
- LLM/provider/RAG calls;
- final dialogue/prose generation;
- arbitrary Lua execution;
- generated Lua source;
- generator-library changes;
- `.sln` or `.csproj` changes.

## Required proof shape

Goal 068 must produce a compact evidence folder:

`.llmgc/procedural/goal-068-combat-magic-ability-boss-encounter-matrix/`

Required artifact categories:

1. `combat-magic-source-manifest.json`
2. `ability-trait-catalog.json`
3. `status-effect-catalog.json`
4. `boss-encounter-phase-catalog.json`
5. `combat-magic-row-matrix.json`
6. `combat-magic-save-load-replay-proof.json`
7. `combat-magic-progression-loot-ledger.json`
8. `combat-magic-counterplay-ledger.json`
9. `combat-magic-preview-export-payload.json`
10. `combat-magic-unity-command-plan.json`
11. `combat-magic-unity-player-proof-summary.json`
12. `combat-magic-invalid-diagnostics-matrix.json`
13. `artifact-scope-report.json`
14. `combat-magic-ability-boss-encounter-matrix-report.md`
15. `rows/*.json` for all 9 rows.

## Required matrix

Exactly 9 family/seed rows unless the prior Goal 060–067 evidence explicitly changes the established matrix.

Families:

- `map_panel_rpg`
- `survival_sandbox`
- `first_person_grid_dungeon`

Seeds:

- `seed_alpha`
- `seed_beta`
- `seed_gamma`

## Family-specific expectations

### map_panel_rpg

Must prove:

- tactical or event-linked encounter;
- at least one active ability;
- at least one passive trait or resistance;
- NPC/faction/narrative consequence linkage;
- reward/progression/loot state delta;
- counterplay or mitigation option.

### survival_sandbox

Must prove:

- hazard/creature/raid or hostile environmental encounter;
- resource/gear/crafting linkage;
- status or injury/fatigue/condition delta;
- recovery/consume/craft consequence;
- survival state change;
- counterplay or mitigation option.

### first_person_grid_dungeon

Must prove:

- orientation/traversal-aware encounter;
- elite/boss/trap pressure;
- key/loot/progression gate;
- status/effect/cooldown or damage state delta;
- blocked/valid movement or tactical position consequence;
- counterplay or mitigation option.

## Encounter depth requirements

Each row must contain:

- row id;
- family id;
- seed id;
- source package / review package refs;
- spatial/narrative/living-world/interlocked/settlement refs;
- initial combat state;
- resolved encounter plan;
- at least two rounds or phases;
- at least one active ability;
- at least one passive trait/status/resistance/weakness;
- at least one cost/cooldown/resource mechanic;
- at least one damage/effect/status transition;
- at least one loot/progression/reward outcome;
- at least one non-combat consequence, for example faction, narrative, settlement, memory, rumor or survival state;
- before/after state hashes that differ;
- replay hash;
- save/load hash;
- diagnostics.

Across the full matrix:

- all 9 rows must be state-changing;
- at least 3 rows must include boss/elite phase logic;
- at least 3 rows must include magic/status-driven effects;
- at least 3 rows must include resource/crafting/gear interaction;
- all 3 families must have distinct ability/encounter flavor;
- same family rows with different seeds must differ meaningfully, not only by id/hash.

## Unity proof

Goal 068 may update only:

`unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`

Unity work must remain narrow:

- read the staged `combat-magic-unity-command-plan.json`;
- emit deterministic `combat_magic_*` markers;
- do not implement broad gameplay systems;
- do not add assets, scenes, prefabs, packages or ScriptableObjects;
- do not modify Unity project settings.

Required marker families:

- `combat_magic_row_loaded`
- `combat_magic_round_step`
- `combat_magic_ability_resolved`
- `combat_magic_status_delta`
- `combat_magic_progression_delta`
- `combat_magic_row_completed`
- `combat_magic_matrix_completed`
- `review_package_proof=goal068`

## Invalid/fake/leak matrix

Must include causal negative cases for:

- missing prior Goal 067 source;
- fake family/seed;
- duplicate row id;
- missing active ability;
- missing state delta;
- fake ability id;
- illegal status/effect shape;
- cooldown/cost underflow;
- nondeterministic ordering;
- save/load mismatch;
- replay mismatch;
- final prose leakage;
- LLM/provider/RAG claim;
- arbitrary Lua execution or generated Lua source claim;
- Runtime/UI/Unity broad mutation claim;
- public GamePackage schema mutation claim;
- unsafe path;
- missing Unity marker proof;
- boss phase without transition;
- overpowered/overconstrained impossible encounter.

## Acceptance evidence

The final report must include:

- `implementationStatus=GREEN|BLOCKED|FAILED`
- `accepted=false`
- gate token
- row count
- state-changing count
- boss/elite row count
- magic/status row count
- save/load/replay result
- meaningful variance result
- Unity exit codes and marker summary
- invalid matrix summary
- explicit statement that no final prose was generated by LLM.
