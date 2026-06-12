# BATCH 012 REPORT — Stats, formulas, progression

## Files generated

- `lua/formula/formula_schema.lua`
- `lua/progression/xp_curve.lua`
- `lua/progression/skill_tree_generator.lua`
- `lua/progression/progress_track.lua`
- `docs/lua/progression_formulas.md`
- `manifests/progression_formulas.manifest.json`
- `tests/progression_examples.lua`
- `BATCH_012_REPORT.md`

## Contracts introduced

- Safe formula IR contract: formulas are normalized expression trees, not raw executable Lua code.
- XP curve contract: deterministic threshold rows and formula reference metadata.
- Skill tree contract: compact unlock graph with nodes, prerequisite edges, costs, effects and UI metadata.
- Abstract progress track contract: reputation, research, faction favor, suspicion, morale, trust and similar non-XP tracks.

## Dependencies between files

- `xp_curve.lua` can emit `formula_ref` metadata intended for use by formula-aware systems.
- `skill_tree_generator.lua` references formula ids and XP/progress currencies but does not execute formulas.
- `progress_track.lua` references quest/dialogue/effect ids and can feed future quest, dialogue-combat and UI IR modules.
- The manifest lists logical dependencies only; Lua modules remain standalone and do not load each other.

## Manual validation

1. Inspect each Lua file and confirm it returns a table.
2. Confirm each module exposes `manifest`, `validate_config(config)` and `generate(input, ctx)`.
3. Check `manifests/progression_formulas.manifest.json` with any JSON validator.
4. Run a Lua 5.4 interpreter manually if available and load individual module files in a sandbox-friendly way chosen by the host.
5. Inspect `tests/progression_examples.lua` for compact example configs and expected output shapes.

## Known limitations

- No runtime formula evaluator is implemented in this batch.
- Skill tree output is design/runtime IR, not a live unlock state machine.
- XP curves are compact threshold tables; balance tuning remains a future validation/simulation task.
- Progress tracks define ranges and stages but do not simulate time decay or event application.

## Next recommended batch

Batch 013 — Combat/status/abilities.

## Non-goals

- No C# integration.
- No Unity object generation.
- No unsafe raw formula code execution.
- No huge skill catalogs or large content dumps.
