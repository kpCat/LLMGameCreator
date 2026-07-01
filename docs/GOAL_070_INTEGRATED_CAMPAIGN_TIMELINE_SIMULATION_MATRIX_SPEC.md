# Goal 070 Spec — Integrated Campaign Timeline Simulation Matrix

## Goal name

`goal_070_integrated_campaign_timeline_simulation_matrix`

## Manual gate

`integrated_campaign_timeline_simulation_matrix_verification required`

## Why this goal exists

Goals 063-069 each proved important gameplay families, but each proof is still mostly a focused matrix. Goal 070 must prove the systems work together in one campaign timeline:

- spatial detail;
- gameplay consequences;
- living world NPC/faction simulation;
- economy/crafting/combat/progression/status;
- settlement construction/destruction/production;
- programmatic narrative/quest/dialogue/event;
- combat/magic/ability/boss;
- weather/day-night/crisis.

The result must be a deterministic multi-step campaign simulation, not another isolated ledger.

## Desired proof

For every family/seed row:

```text
family: map_panel_rpg | survival_sandbox | first_person_grid_dungeon
seed: seed_alpha | seed_beta | seed_gamma
```

produce a multi-step timeline with:

1. a starting package/runtime state reference;
2. at least 6 ordered ticks or phases;
3. at least 5 distinct system categories touched;
4. at least 3 cross-system cascading consequences;
5. at least 1 conflict/arbitration decision;
6. at least 1 settlement/world/narrative/combat coupling;
7. save/load checkpoint proof;
8. replay determinism proof;
9. meaningful variance vs other rows;
10. Unity Alpha timeline markers.

## What must not happen

Goal 070 must not:

- generate final prose;
- call LLM/provider/RAG;
- execute arbitrary Lua;
- change public GamePackage schema;
- change Runtime or Runtime.Abstractions;
- add dependencies;
- implement broad Unity gameplay systems;
- fake Unity proof with JSON-only reports.

## Evidence folder

`.llmgc/procedural/goal-070-integrated-campaign-timeline-simulation-matrix/`

Required artifact families:

- `source-manifest.json`
- `timeline-matrix-summary.json`
- `campaign-timeline-row-*.json` for 9 rows
- `cross-system-cascade-ledger.json`
- `conflict-arbitration-ledger.json`
- `save-load-replay-audit.json`
- `variance-metrics.json`
- `unity-command-plan.json`
- `unity-player-proof-summary.json`
- `preview-export-timeline-payload.json`
- `invalid-diagnostics-matrix.json`
- `artifact-scope-report.json`
- `integrated-campaign-timeline-simulation-matrix-report.md`

## GREEN threshold

GREEN requires:

- 9/9 timeline rows produced;
- 9/9 rows state-changing;
- 9/9 rows replay deterministic;
- 9/9 rows have save/load checkpoint proof;
- each row touches at least five system categories;
- each row has at least three cross-system cascades;
- all rows have stable hashes;
- meaningful variance passes;
- invalid/fake/leak matrix passes;
- Unity editor/player proof runs with exit code 0 and required `campaign_timeline_*` markers;
- `check-all.ps1` passes;
- artifact scope guard passes;
- final commit/push is done.

If these cannot be honestly proven, the task must push `BLOCKED` or `FAILED`, not fabricate GREEN.
