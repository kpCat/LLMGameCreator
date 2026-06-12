# Batch 003 — Time, turn, mode model

## Purpose

This batch introduces deterministic core modules for time mode metadata, turn queues, action points, cooldown ticks, status duration ticks, and active gameplay mode transitions.

The goal is not to run a complete RPG or city-builder simulation yet. The goal is to define compact reusable contracts that later modules can reference when they declare support for realtime, turn-based, mixed, dialogue-combat, tactical, automation, simulation, UI IR, Unity target IR, and future codegen IR workflows.

## Files

- `lua/core/time_model.lua`
- `lua/core/turn_system.lua`
- `lua/core/mode_transition.lua`
- `docs/lua/time_turn_modes.md`
- `manifests/time_turn.manifest.json`
- `tests/time_turn_examples.lua`
- `BATCH_003_REPORT.md`

## Module: `core/time_model/v1`

### Purpose

Normalizes game-level time metadata:

- `turn_mode`: `realtime`, `turn_based`, `mixed`, `paused_planning`;
- `combat_mode`: `none`, `realtime`, `turn_based`, `tactical`, `dialogue_combat`, `hybrid`;
- `active_mode`: `exploration`, `combat`, `dialogue`, `dialogue_combat`, `paused_planning`;
- tick counters for simulation, global turns, rounds, and elapsed seconds.

### When to use

Use this module when a generator or capability needs to declare what time/combat mode it supports, or when a compact generator plan needs a deterministic time contract.

Examples:

- realtime exploration with turn-based combat uses `turn_mode = "mixed"`;
- city-builder paused planning can use `turn_mode = "paused_planning"` while retaining simulation metadata;
- dialogue-combat modules can declare `combat_mode = "dialogue_combat"`.

### When not to use

Do not use this module as a complete runtime clock. It does not read wall-clock time, schedule asynchronous events, run pathfinding, or simulate large populations.

### Manifest summary

The module exposes capabilities for creating a time model, advancing deterministic counters, declaring mode support, and resolving tick units.

### Input schema explained

`create(config)` and `generate({ config = ... }, ctx)` accept a table with optional mode fields and counters. Missing fields are filled with deterministic defaults.

### Config schema explained

Important config fields:

- `turn_mode` — high-level time model;
- `combat_mode` — combat semantics;
- `active_mode` — current gameplay mode;
- `simulation_tick_seconds` — deterministic seconds represented by one simulation tick;
- `cooldown_tick_unit` — unit used by cooldown consumers;
- `status_tick_unit` — unit used by status duration consumers.

### Output schema explained

Outputs use the standard result shape:

```text
{ ok = boolean, data = table, diagnostics = array, artifacts = array }
```

The `data.model` table contains only JSON-serializable values.

### Example config

```lua
{
  turn_mode = "mixed",
  combat_mode = "turn_based",
  active_mode = "exploration",
  simulation_tick_seconds = 1,
  cooldown_tick_unit = "turn",
  status_tick_unit = "round"
}
```

### Example input

```lua
{
  config = {
    turn_mode = "mixed",
    combat_mode = "dialogue_combat",
    active_mode = "dialogue"
  }
}
```

### Example output

```lua
{
  ok = true,
  data = {
    model = {
      turn_mode = "mixed",
      combat_mode = "dialogue_combat",
      active_mode = "dialogue"
    }
  },
  diagnostics = {},
  artifacts = {}
}
```

### LLM prompting hints

Ask the LLM to select the smallest viable time model for the requested game slice. Avoid forcing every project into tactical combat if the game only needs exploration and dialogue.

### Validation rules

Invalid mode names are diagnostics. Realtime exploration with turn-based combat produces a warning recommending `mixed`.

### Extension points

Future modules can add scheduling, pause policies, simulation phases, and replay metadata without changing the basic mode names.

### Runtime target notes

This contract is suitable for debug previews, simulation metadata, Unity IR, and codegen IR. It is intentionally independent from Unity runtime objects.

### Unity/codegen notes

Unity adapters should treat this as an IR contract and map it into engine-specific state machines later.

## Module: `core/turn_system/v1`

### Purpose

Builds a deterministic turn queue for actor, side, or global ordering. It supports action points, cooldown ticks, and status duration ticks.

### When to use

Use this module for turn-based RPG combat, tactical combat, dialogue-combat rounds, or mixed-mode games where combat/dialogue is turn-based but exploration is realtime.

### When not to use

Do not use it for high-volume realtime automation simulation. Factorio-like conveyors, city services, and power networks should use later simulation/automation modules.

### Manifest summary

The module exposes capabilities for turn state creation, current actor lookup, action point spending, cooldown assignment, status assignment, and deterministic turn advancement.

### Input schema explained

`create(input)` expects:

- `input.config` — optional turn settings;
- `input.actors` — non-empty actor array.

Each actor can contain:

- `id`;
- `side`;
- `initiative`;
- `action_points`;
- `max_action_points`;
- `cooldowns` map;
- `statuses` map;
- JSON-like `metadata`.

### Config schema explained

- `turn_mode`: `turn_based` or `mixed`;
- `initiative_mode`: `actor`, `side`, or `global`;
- `default_action_points`: AP assigned when actors omit AP values;
- `tick_cooldowns_on`: `actor_turn_end`, `round_end`, or `global_turn_end`;
- `tick_statuses_on`: same policy list.

### Output schema explained

`data.state` contains:

- `order` — deterministic actor id array;
- `turn_index`;
- `round`;
- `global_turn`;
- normalized actor state.

### Example config

```lua
{
  turn_mode = "mixed",
  initiative_mode = "actor",
  default_action_points = 2,
  tick_cooldowns_on = "actor_turn_end",
  tick_statuses_on = "round_end"
}
```

### Example input

```lua
{
  config = { initiative_mode = "actor" },
  actors = {
    { id = "entity/player/main", side = "heroes", initiative = 10 },
    { id = "entity/enemy/rat", side = "monsters", initiative = 4 }
  }
}
```

### Example output

```lua
{
  ok = true,
  data = {
    current_actor_id = "entity/player/main",
    state = { round = 1, global_turn = 1 }
  },
  diagnostics = {},
  artifacts = {}
}
```

### LLM prompting hints

Ask the LLM to describe actors and sides, not to generate a full combat simulator. Keep actor arrays compact.

### Validation rules

Actor ids must be unique. AP values must be non-negative and not exceed max AP. Unknown config keys are warnings.

### Extension points

Future combat modules can add ability resolution, formula references, targeting, tactical grids, and dialogue-combat choice effects.

### Runtime target notes

The turn state is a deterministic data contract and can be consumed by debug preview, Unity adapters, or simulation tests.

### Unity/codegen notes

Unity codegen should not use this as raw gameplay code. It should map this IR into engine state and commands.

## Module: `core/mode_transition/v1`

### Purpose

Defines mode transition rules between exploration, combat, dialogue, dialogue-combat, and paused planning.

### When to use

Use it when a generator needs to know whether a game can move from exploration to dialogue, dialogue to combat, combat to dialogue-combat, or paused planning back to runtime.

### When not to use

Do not use it as a full quest condition engine. Later quest/dialogue/interaction modules should provide domain-specific conditions.

### Manifest summary

The module exposes capabilities for default rules, allowed transition listing, transition checking, transition application, and dialogue-combat profile generation.

### Input schema explained

`generate({ config = ... }, ctx)` accepts optional custom rules. `apply(state, to_mode, config, context)` applies a validated transition.

### Config schema explained

`config.rules` can override the default rule set. A rule has:

- `from`;
- `to`;
- `reason`;
- optional `requires` array of context flag names.

### Output schema explained

The output is a JSON-serializable rules table and, for `apply`, a copied next state plus a transition event.

### Example config

```lua
{
  rules = {
    { from = "exploration", to = "dialogue", reason = "talk" },
    { from = "dialogue", to = "dialogue_combat", reason = "threaten", requires = { "has_target" } }
  }
}
```

### Example input

```lua
{
  active_mode = "dialogue",
  combat_mode = "none"
}
```

### Example output

```lua
{
  ok = true,
  data = {
    transition = {
      from = "dialogue",
      to = "dialogue_combat",
      applied = true
    }
  },
  diagnostics = {},
  artifacts = {}
}
```

### LLM prompting hints

Ask the LLM to model transitions as rules, not as prose. Dialogue-combat should be explicit when social choices can affect morale, trust, suspicion, focus, or HP-like tracks.

### Validation rules

Unsupported modes are diagnostics. Missing context requirements return diagnostics rather than throwing.

### Extension points

Later dialogue, combat, quest, UI IR, and Unity IR modules can add typed conditions and event payloads.

### Runtime target notes

Rules are deterministic and small. They are safe to import into future registries as data.

### Unity/codegen notes

Unity adapters should convert the transition event into scene/UI/state machine commands later.
