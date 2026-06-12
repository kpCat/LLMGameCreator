# Batch 012 — Stats, formulas, progression

## Purpose

This batch introduces safe progression and formula infrastructure for AI Game Builder / LLMGameCreator. It does not execute raw formulas. It defines compact, deterministic IR for formulas, XP curves, skill trees and abstract progress tracks.

## Files

- `lua/formula/formula_schema.lua`
- `lua/progression/xp_curve.lua`
- `lua/progression/skill_tree_generator.lua`
- `lua/progression/progress_track.lua`
- `manifests/progression_formulas.manifest.json`
- `tests/progression_examples.lua`
- `BATCH_012_REPORT.md`

## Modules

### `formula/formula_schema/v1`

**Purpose.** Normalize and validate formula definitions as safe formula IR.

**When to use.** Use it when a game design needs attribute formulas, stat formulas, damage formula references, research scaling, reputation scaling or codegen-ready formula descriptions.

**When not to use.** Do not use it as a Lua expression evaluator. The module intentionally rejects raw executable code fields such as script-like formula definitions.

**Input schema.**

```lua
{
  formulas = {
    {
      id = "formula/stats/max_health",
      result_stat = "stats/max_health",
      expression = {
        op = "add",
        args = {
          { op = "const", value = 50 },
          { op = "mul", args = {
              { op = "ref", ref = "stats/endurance" },
              { op = "const", value = 5 }
          }}
        }
      }
    }
  }
}
```

**Config schema.**

- `allowed_ops`: optional list of allowed operation ids.
- `allowed_value_refs`: optional list of exact ids or slash-prefixes.
- `max_depth`: maximum expression tree depth.
- `max_args_per_op`: maximum args per operation.
- `max_formulas`: maximum normalized formulas.

**Output schema.**

- `formulas`: normalized formulas.
- `indexes.by_id`: formula id lookup.
- `indexes.by_result_stat`: result-stat to formula-id map.
- `indexes.by_tag`: tag to formula-id map.
- `summary.safe_ir_only`: always true for this module.

**Validation rules.**

- Formula ids must be lowercase slash ids.
- Expressions must be tables with a known `op`.
- Raw executable fields are reported as errors.
- References must be valid ids and can be limited by config.

**Extension points.**

Future batches can add formula evaluators, balance validation, combat formula references and codegen-specific lowering while preserving this IR.

### `progression/xp_curve/v1`

**Purpose.** Generate deterministic XP threshold curves.

**When to use.** Use it for RPG levels, research tiers, faction ranks or any threshold-based progression.

**When not to use.** Do not use it for runtime random rewards. Loot and reward resolution belongs to runtime or later simulation modules.

**Supported modes.**

- `linear`
- `quadratic`
- `stepped`
- `exponential`

**Example input.**

```lua
{
  curve = {
    id = "progression/xp/hero",
    mode = "quadratic",
    base = 100,
    growth = 30,
    max_level = 5
  }
}
```

**Output.** The module emits compact threshold rows: `level`, `delta_xp`, `total_xp`, plus a safe `formula_ref` object that can be referenced from formula IR or UI IR.

### `progression/skill_tree_generator/v1`

**Purpose.** Generate skill tree IR with nodes, prerequisite edges, costs, effects and UI metadata.

**When to use.** Use it for RPG skills, research trees, city-builder policies, automation upgrades and faction perks.

**When not to use.** Do not use it as a runtime unlock resolver. It emits design-time IR only.

**Input schema.**

The module accepts explicit `nodes` and/or branch-grouped nodes:

```lua
{
  tree = {
    id = "progression/skill_tree/ranger",
    branches = {
      {
        id = "combat",
        nodes = {
          { id = "skill/ranger/focus", cost = 1 },
          { id = "skill/ranger/piercing_shot", cost = 2, requires = { "skill/ranger/focus" } }
        }
      }
    }
  }
}
```

**Output schema.**

- `tree`: normalized metadata.
- `nodes`: skill nodes.
- `edges`: prerequisite graph edges.
- `indexes.roots`: nodes without prerequisites.
- `indexes.by_branch`: node ids grouped by branch.

**Validation rules.**

- Node ids must be valid lowercase ids.
- Duplicate nodes are diagnostics.
- Missing prerequisite references are diagnostics.
- Costs must be non-negative integers.

### `progression/progress_track/v1`

**Purpose.** Define abstract progression tracks that are not XP-only.

**Supported domains.**

- `reputation`
- `research`
- `faction_favor`
- `suspicion`
- `morale`
- `trust`
- `influence`
- `threat`

**When to use.** Use it for faction reputation, city-builder service trust, dialogue-combat suspicion, research completion, morale pressure or quest-state meters.

**When not to use.** Do not use it for inventory stacks or HP. Those belong to item/combat/stat systems.

**Output schema.**

- `tracks`: normalized track definitions.
- `indexes.by_domain`: grouped by domain.
- `indexes.by_polarity`: positive/negative/mixed grouping.

## LLM prompting hints

Prefer asking the LLM to select modules and produce compact configs, not final bulk content. Good prompts:

- “Create a formula IR for max health from endurance and level.”
- “Create a compact skill tree config for stealth and dialogue-combat.”
- “Create abstract progress tracks for suspicion, morale and faction favor.”

Bad prompts:

- “Print a thousand skill nodes.”
- “Write raw Lua formula code.”
- “Generate the complete runtime progression system here.”

## Runtime target notes

The output is JSON-serializable IR. A future C# importer can validate manifests, index capabilities and later pass the generated IR to Unity adapters, preview UI or codegen pipelines.

## Unity/codegen notes

- Formula IR can be lowered to safe runtime evaluators or generated C# later.
- Skill tree IR can feed Unity UI tree layouts.
- XP curve rows can feed balance preview charts.
- Progress tracks can feed quest journal, faction UI, dialogue-combat HUD and simulation dashboards.
