# BATCH 021 REPORT — Example game recipes

## Files generated

- `examples/dark_fantasy_village_recipe.lua`
- `examples/factorio_like_alchemy_recipe.lua`
- `examples/open_world_rpg_recipe.lua`
- `examples/city_builder_frontier_recipe.lua`
- `examples/space_quest_recipe.lua`
- `docs/lua/example_game_recipes.md`
- `manifests/example_recipes.manifest.json`
- `BATCH_021_REPORT.md`

## Current repo state read before generation

- Read `docs/LUA_GENERATION_PLAN_AND_PROMPTS.md` and used the exact Batch 021 definition.
- Read `generator-library/docs/lua/MANIFEST_CONTRACT.md` and followed the canonical manifest contract.
- Confirmed Batch 016 exists by reading `generator-library/manifests/city_builder.manifest.json`.
- Confirmed Batch 017 exists by reading `generator-library/manifests/ui_ir.manifest.json`.
- Confirmed Batch 018 exists by reading `generator-library/manifests/unity_ir.manifest.json`.
- Confirmed Batch 019 exists by reading `generator-library/manifests/validation.manifest.json`.
- Confirmed Batch 020 exists by reading `generator-library/manifests/orchestration.manifest.json`.
- Read Batch 009 dialogue and Batch 014 NPC/pathfinding manifests to avoid invented recipe references.

## Previous batch protection

- Batch 016 files are not included in this ZIP and were not modified.
- Batch 017 files are not included in this ZIP and were not modified.
- Batch 018 files are not included in this ZIP and were not modified.
- Batch 019 files are not included in this ZIP and were not modified.
- Batch 020 files are not included in this ZIP and were not modified.

## Manifest contract compliance

- Batch manifest uses required fields: `id`, `version`, `batch`, `title`, `purpose`, `files`, `modules`, `runtime_targets`, `supported_time_modes`, `supported_combat_modes`, `unsafe_features`.
- Recipe entries are represented as deterministic example/config module entries with canonical `id`, `path`, `category`, `capabilities`, `depends_on`, `runtime_targets`, `supported_turn_modes`, `supported_combat_modes`, `deterministic`, `unsafe_features`.
- No alias fields are used.
- Batch number is `021`.
- Batch report path is `generator-library/BATCH_021_REPORT.md`.

## Forbidden API compliance

- Recipe Lua files avoid the forbidden runtime-access APIs listed in the Batch 021 request.
- No external dependencies are used.
- No random source is used.
- No module loading or pipeline execution is included.
- No C# integration, Unity integration, compilation or source generation is included.

## Recipe summary

- `dark_fantasy_village_recipe.lua`: compact dark fantasy village RPG slice with world, NPC, dialogue, quest, optional combat, UI and validation references.
- `factorio_like_alchemy_recipe.lua`: Factorio-like automation/alchemy setup with recipe graph, machines, conveyors, power, build menu and artifact references.
- `open_world_rpg_recipe.lua`: open world RPG planning setup with regions, biomes, reachability, NPC schedules, factions, quests, combat/progression, minimap/global map and validation references.
- `city_builder_frontier_recipe.lua`: frontier city-builder setup with needs, jobs, buildings, services, economy hooks, UI and validation references.
- `space_quest_recipe.lua`: compact space quest/adventure setup with locations, NPC/dialogue, quest stages, items, progression, UI and Unity target IR references as metadata only.

## Final roadmap confirmation

- No Batch 022 was generated.
- No extra roadmap extension was generated.
- Batch 021 is the final Lua batch in the current roadmap.

## Internal self-check

- PASS: all ZIP paths start with `generator-library/`.
- PASS: no root-level generated folders or files.
- PASS: manifest JSON valid.
- PASS: every manifest `files[]` path exists in the ZIP under `generator-library/`.
- PASS: every manifest `module.path` exists in the ZIP under `generator-library/`.
- PASS: no alias manifest fields are used.
- PASS: batch required fields exist.
- PASS: module required fields exist.
- PASS: report name is numbered `BATCH_021_REPORT.md`.
- PASS: docs path is `generator-library/docs/lua/example_game_recipes.md`.
- PASS: module IDs are unique inside batch.
- PASS: all module IDs use lowercase slash notation.
- PASS: capabilities use lowercase dot notation.
- PASS: no forbidden Lua APIs are present in generated Lua files.
- PASS: no `.cs` files are generated.
- PASS: no C# project files are modified.
- PASS: previous batch files remain untouched.
- PASS: no Batch 022 or extra roadmap extension is generated.

## Known limitations

- Recipes are compact examples and do not generate complete game packages.
- Expected artifacts are logical descriptors, not files emitted by this batch.
- Validation plans are checklists and references; validators are not executed by the recipe files.
- Context pack hints are metadata only and do not call or summarize with an LLM.
