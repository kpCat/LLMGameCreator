# Goal 069 — World Event / Weather / Day-Night / Crisis Matrix Spec

## Purpose

Goal 069 must add a real gameplay-depth layer, not another report-only layer.

It consumes the current proof chain through Goal 068 and proves deterministic world-event/environment pressure rows across the existing 3 families x 3 seeds matrix:

- `map_panel_rpg`
- `survival_sandbox`
- `first_person_grid_dungeon`

The goal must model time/weather/world-crisis systems as state-changing gameplay pressure:

- world clock / day-night phase
- weather / environmental hazard
- crisis or global event pressure
- faction/NPC/settlement/narrative/combat/economy interaction
- runtime state before/after delta
- save/load/replay determinism
- Unity Alpha marker proof

This is not a climate simulator, not real-world weather, not a provider call, and not an audio/ray-tracing implementation.

## Non-goals

Do not implement:

- real-time clock coupling
- NodaTime/FastNoiseLite/libnoise dependencies
- provider/LLM/RAG/media calls
- arbitrary Lua
- GamePackage public schema changes
- Runtime or Runtime.Abstractions source changes
- broad Unity gameplay systems
- WinForms/UI
- generator-library changes
- external asset import

Unity Alpha may receive a narrow marker loader extension only, matching recent Goal 063–068 pattern.

## Required proof shape

The goal must produce 9 deterministic rows:

```text
3 families x 3 seeds = 9 world-event/environment rows
```

Each row must include at minimum:

- family id
- seed id
- package row id / upstream row ids
- world clock before/after
- day/night phase transition or persistent phase effect
- weather/environment state
- hazard/crisis/event state
- at least one cross-system effect:
  - NPC/faction
  - settlement/production
  - combat/magic/status
  - narrative/quest/dialogue event
  - economy/resource/inventory
- runtime/state delta
- save/load snapshot proof
- replay proof
- deterministic row hash
- Unity marker requirements

## Family-specific expectations

### map_panel_rpg

Should prove travel or region-level pressure:

- travel window / night risk / patrol change / storm route cost
- NPC or faction reaction to crisis/weather/time
- quest/event consequence or route decision
- inventory/reward/reputation delta where applicable

### survival_sandbox

Should prove environment survival pressure:

- weather hazard, exposure, resource depletion/recovery
- shelter/craft/consume/recover interaction
- status/effect and inventory/resource delta
- day-night pressure or storm escalation

### first_person_grid_dungeon

Should prove dungeon environmental pressure:

- darkness/visibility/torch/magic light/fog/sound pressure
- encounter pressure tied to phase/weather/crisis
- loot/progression/status consequence
- blocked/valid movement or route risk

## Evidence artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-069-world-event-weather-daynight-crisis-matrix/
```

Required artifacts:

- `source-manifest.json`
- `world-clock-calendar-policy.json`
- `weather-hazard-catalog.json`
- `crisis-event-catalog.json`
- `world-event-weather-daynight-row-matrix.json`
- `save-load-replay-proof.json`
- `variance-metrics.json`
- `unity-command-plan.json`
- `unity-proof-summary.json`
- `invalid-diagnostics-matrix.json`
- `preview-export-payload.json`
- `world-event-weather-daynight-crisis-matrix-report.md`
- artifact scope report files if the repo standard guard writes them

No timestamps unless the repo already has a deterministic convention.
No absolute local paths inside artifacts.
No heavy Unity build/log outputs committed.
Ordering must be stable.

## Gate

Goal 069 must stop at:

```text
world_event_weather_daynight_crisis_matrix_verification required
```

`accepted=false`.

Only record Goal 068 as accepted by user handoff before Goal 069.

Do not mark Goal 069 passed.

## Quality bar

GREEN is allowed only if all of these are true:

- 9/9 rows exist.
- 9/9 rows are state-changing.
- day-night/weather/crisis effects are not merely labels; they cause gameplay deltas.
- save/load/replay proof passes.
- meaningful variance passes.
- Unity/player marker proof passes.
- invalid/fake/leak matrix passes.
- check-all passes.
- artifact scope guard passes.

If the goal only creates descriptions, labels, or hashes without meaningful gameplay deltas, commit/push as `BLOCKED`.
