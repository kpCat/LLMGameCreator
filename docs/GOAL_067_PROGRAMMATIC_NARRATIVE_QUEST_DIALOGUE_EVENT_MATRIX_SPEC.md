# Goal 067 — Programmatic Narrative Quest Dialogue Event Matrix

## Purpose

After Goal 066, the generator has validated package rows, Unity proof, spatial detail, gameplay consequences, living-world state, interlocked systems and settlement production/destruction loops.

Goal 067 adds a deterministic narrative realization layer without making LLM write final content.

It must prove that the generator can produce structured, state-changing quest/dialogue/event arcs for all 9 family/seed rows:

- `map_panel_rpg`
- `survival_sandbox`
- `first_person_grid_dungeon`

with seeds:

- `seed_alpha`
- `seed_beta`
- `seed_gamma`

## Design principle

Narrative content is programmatic and contract-bound.

Do not generate unbounded final prose.

Use:

- localization keys;
- template ids;
- slots;
- speaker roles;
- tone tags;
- dialogue acts;
- option conditions;
- option effects;
- quest stages;
- event triggers;
- event consequences;
- state-delta references;
- memory/rumor propagation.

LLM is not used in this goal.

## Required proof

The goal is GREEN only if it proves:

1. 9/9 rows produced.
2. 9/9 rows are state-changing.
3. Each row has:
   - at least one quest-stage graph;
   - at least one dialogue option graph;
   - at least one event trigger/consequence chain;
   - at least one localization-key/template-slot table entry;
   - at least one memory/rumor or knowledge propagation entry;
   - at least one connection to Goal 064 living-world state;
   - at least one connection to Goal 065 interlocked gameplay state;
   - at least one connection to Goal 066 settlement/building/production/destruction state.
4. Save/load/replay proof passes.
5. Meaningful variance passes across family and seed rows.
6. Unity Alpha proof passes with deterministic markers for 9 rows.
7. Invalid/fake/leak matrix rejects unsafe or fake content.

## Required artifacts

Under:

```text
.llmgc/procedural/goal-067-programmatic-narrative-quest-dialogue-event-matrix/
```

Create compact deterministic artifacts, including at minimum:

```text
narrative-source-manifest.json
narrative-row-matrix.json
rows/map-panel-rpg-seed-alpha-narrative-row.json
rows/map-panel-rpg-seed-beta-narrative-row.json
rows/map-panel-rpg-seed-gamma-narrative-row.json
rows/survival-sandbox-seed-alpha-narrative-row.json
rows/survival-sandbox-seed-beta-narrative-row.json
rows/survival-sandbox-seed-gamma-narrative-row.json
rows/first-person-grid-dungeon-seed-alpha-narrative-row.json
rows/first-person-grid-dungeon-seed-beta-narrative-row.json
rows/first-person-grid-dungeon-seed-gamma-narrative-row.json
quest-stage-ledger.json
dialogue-option-ledger.json
event-trigger-consequence-ledger.json
localization-key-table.json
memory-rumor-propagation-ledger.json
narrative-save-load-replay-proof.json
narrative-preview-export-payload.json
narrative-unity-command-plan.json
narrative-unity-player-proof-summary.json
narrative-invalid-diagnostics-matrix.json
artifact-scope-report.json
programmatic-narrative-quest-dialogue-event-matrix-report.md
```

## Unity proof

Unity Alpha may only be touched through the existing narrow marker-loader pattern in:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

The Unity proof must be deterministic. It may read a staged command plan and emit markers. It must not implement broad UI, narrative runtime, Yarn/ink runtime, provider calls, or GamePackage schema changes.

Required marker concepts:

```text
narrative_row_loaded
quest_stage_started
dialogue_option_available
dialogue_option_selected
event_trigger_resolved
event_consequence_applied
memory_rumor_recorded
localization_key_bound
narrative_row_completed
```

## Forbidden

- provider/LLM/RAG calls;
- final unbounded prose generation;
- Yarn/ink runtime integration;
- external dependencies;
- public GamePackage schema changes;
- Runtime/Runtime.Abstractions changes;
- WinForms UI changes;
- broad Unity rewrites;
- generator-library changes;
- arbitrary Lua execution.

## Expected final gate

```text
programmatic_narrative_quest_dialogue_event_matrix_verification required
```
