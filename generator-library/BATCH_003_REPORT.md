# Batch 003 Report — Time, turn, mode model

## Files generated

- `lua/core/time_model.lua`
- `lua/core/turn_system.lua`
- `lua/core/mode_transition.lua`
- `docs/lua/time_turn_modes.md`
- `manifests/time_turn.manifest.json`
- `tests/time_turn_examples.lua`
- `BATCH_003_REPORT.md`

No `BATCH_REPORT.md` file is included. The report filename follows the numbered batch-report convention requested after Batch 002.

## Contracts introduced

### `core/time_model/v1`

Introduces deterministic metadata for:

- `turn_mode`: `realtime`, `turn_based`, `mixed`, `paused_planning`;
- `combat_mode`: `none`, `realtime`, `turn_based`, `tactical`, `dialogue_combat`, `hybrid`;
- `active_mode`: `exploration`, `combat`, `dialogue`, `dialogue_combat`, `paused_planning`;
- deterministic counters: `tick`, `elapsed_seconds`, `round`, `global_turn`;
- cooldown/status tick-unit metadata.

### `core/turn_system/v1`

Introduces deterministic turn-state helpers for:

- actor, side, or global ordering;
- current actor lookup;
- action point spending;
- cooldown tick counters;
- status duration tick counters;
- end-turn advancement with round/global-turn metadata.

### `core/mode_transition/v1`

Introduces deterministic mode-transition rules for:

- exploration → dialogue/combat/paused planning;
- dialogue → exploration/dialogue-combat/combat;
- dialogue-combat → dialogue/combat/exploration;
- combat → exploration/dialogue-combat/paused planning;
- paused planning → exploration/combat.

## Dependencies between files

The three Lua modules are intentionally self-contained and do not call `require`, `dofile`, file-system APIs, network APIs, or external libraries.

The contracts align with earlier batches by using:

- standard result shape: `{ ok, data, diagnostics, artifacts }`;
- diagnostic shape: `{ severity, code, message, target }`;
- JSON-serializable output tables;
- deterministic ordering and no `math.random`.

`tests/time_turn_examples.lua` expects the host test runner to inject the module tables as:

```lua
{
  time_model = TimeModel,
  turn_system = TurnSystem,
  mode_transition = ModeTransition
}
```

This keeps tests compatible with the no-external-`require` rule.

## How to validate manually

1. Confirm file list in the ZIP:
   - `lua/core/time_model.lua`
   - `lua/core/turn_system.lua`
   - `lua/core/mode_transition.lua`
   - `docs/lua/time_turn_modes.md`
   - `manifests/time_turn.manifest.json`
   - `tests/time_turn_examples.lua`
   - `BATCH_003_REPORT.md`

2. Parse `manifests/time_turn.manifest.json` as JSON.

3. Load the Lua modules in a sandboxed host that injects module tables manually.

4. Run `tests/time_turn_examples.lua` through a host harness that calls:

```lua
Tests.run({
  time_model = TimeModel,
  turn_system = TurnSystem,
  mode_transition = ModeTransition
})
```

5. Check that the returned report uses:

```text
ok = true
```

and contains no error diagnostics.

## Known limitations

- This batch is not a full runtime scheduler.
- It does not implement ability resolution, damage formulas, tactical pathfinding, dialogue choice effects, or city-builder simulation.
- It does not integrate with the C# application.
- It does not generate Unity objects or raw C# code.
- Custom transition requirements are simple boolean flags, not a full condition DSL.
- `turn_system` handles compact actor arrays, not thousands of live agents.

## Next recommended batch

Next recommended batch is Batch 004 — Capability and generator module manifest helpers.

Do not proceed to Batch 004 until explicitly requested.
