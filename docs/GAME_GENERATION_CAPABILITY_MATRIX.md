# Game Generation Capability Matrix

Status: authoritative planning matrix  
Scope: non-media game generation capabilities  
Non-scope: direct media generation, production code changes, schema changes

Current status vocabulary:

- `implemented`: current code/docs provide a working data/runtime/validation path.
- `partial`: current docs or code contain a slice, but not the full target path.
- `planned`: documented direction exists, but no full implementation yet.
- `blocked`: requires a preceding contract, validator, schema, runtime or approval decision.

Important baseline:

- Current one-click export covers only a narrow vertical MVP.
- Lua roadmap is broad, but Lua is not yet the main generator runtime in the current one-click flow.
- Infinite worlds require generator config, seeds and chunk rules, not huge tile dumps.

| Capability | Target responsibility | Current status | Current evidence / doc refs | Required artifact contracts | Required Lua modules | Required validators | Required UI | Priority | Acceptance criteria |
|---|---|---|---|---|---|---|---|---|---|
| game profile / genre / tone | mixed: LLM drafts, C# validates/approves | partial | `MODEL_WORKFLOW_ROLES_AND_PROMPTS.md`, `GENERATOR_PLAN_ONE_CLICK_PACKAGE_EXPORT.md` | `game_profile_v1` | none required | schema/id/profile validator | profile review and approval | P0 | Profile has genre, tone, runtime target, feature bundles and approval record. |
| world scale | mixed | planned | `ARCHITECTURE_CAPABILITY_ATLAS_AND_RUNTIME_EXPORT.md`, `LUA_GENERATION_PLAN_AND_PROMPTS.md` | `world_profile_v1`, `world_scale_config_v1` | world blueprint | scale/config validator | capability picker | P0 | Finite, multi-map, region and chunked modes are explicit and validated. |
| finite maps | C# validates, LLM/Lua draft data | partial | `GAME_PACKAGE_FORMAT.md`, `VALIDATION_STRATEGY.md`, assembly docs | `scene_pack_v1`, `map_pack_v1` | tile painter optional | map bounds, tile refs, start position | preview diagnostics | P0 | Generated finite map validates and starts runtime preview. |
| chunked/infinite worlds | Lua generates rules/IR, C# validates | planned | `LUA_GENERATION_PLAN_AND_PROMPTS.md`, `VALIDATION_STRATEGY.md` | `world_chunk_config_v1`, `chunk_rule_pack_v1` | chunk generator, biome catalog, reachability | seed/config/chunk reachability validator | chunk preview diagnostics | P0 | World uses seed, chunk size, rules and sparse overrides; no huge tile dump. |
| biomes/regions | mixed | planned | Lua batches 005-006 in `LUA_GENERATION_PLAN_AND_PROMPTS.md` | `biome_pack_v1`, `region_graph_v1` | biome catalog, region graph | biome refs, region connectivity | atlas/profile review | P1 | Regions and biomes reference valid ids and support selected world scale. |
| roads/path/reachability | C# validates, Lua may generate | planned | Lua batch 007, `VALIDATION_STRATEGY.md` | `path_network_v1`, `reachability_report_v1` | path carver, road generator, reachability | blocked path, start-objective reachability | runtime/map diagnostics | P0 | Required objectives are reachable or blocked with deterministic diagnostics. |
| entities/prototypes/instances | mixed | partial | `GAME_PACKAGE_FORMAT.md`, assembly maps `entity_pack_v1` | `entity_pack_v1` | entity factory | prototype refs, instance positions | artifact review | P0 | Entity prototypes and instances validate and load into runtime preview. |
| NPC archetypes | LLM drafts, Lua may expand, C# validates | planned | Lua batch 014, narrative runtime docs in context index | `npc_archetype_pack_v1` | npc archetype generator | faction/dialogue/schedule refs | NPC artifact review | P1 | NPC archetypes have roles, tags, dialogue/faction hooks and valid ids. |
| schedules/factions | mixed | partial | `GAME_PACKAGE_FORMAT.md` narrative lists, runtime narrative notes | `faction_pack_v1`, `schedule_pack_v1` | schedule generator, faction model | faction ids, reputation bounds, schedule validity | artifact review | P1 | Faction and schedule data validate and can be inspected before promotion. |
| dialogue | LLM drafts, C# validates/runtime executes | partial | `GAME_PACKAGE_FORMAT.md`, narrative runtime summary, Lua batch 009 | `dialogue_pack_v1` | dialogue schema/generator | graph refs, choices, conditions/effects | dialogue review | P0 | Dialogue graph has valid start node, node refs and runtime choices. |
| dialogue-combat | mixed | planned | Lua generation plan combat/dialogue-combat | `dialogue_combat_pack_v1` | dialogue combat, combat schema | morale/trust/resource bounds, choice effects | review plus runtime diagnostics | P2 | Dialogue choices can resolve conflict using validated combat-like effects. |
| quests/objectives/stages | LLM drafts, C# validates/runtime executes | partial | `GAME_PACKAGE_FORMAT.md`, assembly maps `quest_pack_v1` | `quest_pack_v1` | quest schema generators | objective refs, stage transitions, rewards | quest review | P0 | Quest has valid objectives, stages and reward/effect refs. |
| items/inventory/equipment | mixed | partial | economy/exploration notes in `GAME_PACKAGE_FORMAT.md` | `item_pack_v1`, `inventory_pack_v1`, `equipment_pack_v1` | item catalog, inventory rules | stack, equipment slot, requirement validators | item/equipment review | P0 | Items and inventories validate and runtime commands can use/equip items. |
| loot/economy/vendors | mixed | partial | economy lists in `GAME_PACKAGE_FORMAT.md`, patch ops doc | `loot_pack_v1`, `economy_pack_v1`, `vendor_pack_v1` | loot table, recipe graph | loot refs, transaction/cost/output validators | economy review | P1 | Loot and vendors validate and runtime economy commands remain deterministic. |
| stats/formulas/progression | mixed | partial | encounter examples in `GAME_PACKAGE_FORMAT.md`, Lua batch 012 | `stats_pack_v1`, `formula_pack_v1`, `progression_pack_v1` | formula schema, XP curve | bounds, refs, formula IR validator | progression review | P0 | Stats/progression formulas are data/IR, bounded and executable by C# runtime. |
| abilities/status effects | mixed | partial | encounter examples, assembly maps `mechanics_pack_v1` to abilities | `ability_pack_v1`, `status_pack_v1` | ability catalog, status effects | ability refs, cost/output/status duration validators | ability review | P0 | Abilities/statuses validate and work in encounter/runtime smoke. |
| combat | C# runtime, LLM/Lua data drafts | partial | encounter runtime summary, `GAME_PACKAGE_FORMAT.md` encounter lists | `combat_pack_v1`, `encounter_pack_v1` | combat schema, turn-based combat | participants, abilities, rewards, turn ownership | runtime simulator diagnostics | P0 | Generated encounter can start, take turns and resolve with deterministic events. |
| survival/crafting | mixed | partial | economy/crafting runtime notes, Lua survival direction | `survival_pack_v1`, `crafting_pack_v1` | recipe graph, survival config modules | resource, condition, recipe validators | survival/crafting review | P2 | Crafting and survival meters have valid costs, outputs and runtime state path. |
| automation/Factorio-like systems | Lua/C# mixed | planned | Lua batch 015, architecture reference profiles | `automation_pack_v1`, `production_graph_v1` | recipe graph, machines, conveyors, power network | graph, throughput, resource refs | automation review/preview | P2 | Production graph validates and outputs compact config, not full simulation dump. |
| city-builder/simulation | Lua/C# mixed | planned | Lua batch 016, architecture reference profiles | `city_builder_pack_v1`, `simulation_pack_v1` | citizen needs, jobs, building catalog | job/building/service coverage validators | simulation review | P2 | Buildings, jobs, needs and service coverage validate as data/config. |
| UI IR/HUD | Lua/LLM draft IR, C# validates | planned | Lua batch 017, architecture Unity IR notes | `ui_ir_v1` | ui schema, hud layout, inventory UI | UI schema, screen/ref validators | UI IR review | P2 | UI records are data IR and do not emit WinForms/Unity code directly. |
| runtime preview | C# owns | partial | README, validation strategy, runtime simulator notes | `runtime_smoke_report_v1` | none | smoke command validator | debug preview/simulator | P0 | Generated package can run load/wait/move/interact/save smoke where applicable. |
| Unity/export IR | C# validates, Lua/LLM draft IR | planned | architecture doc, Lua batch 018 | `unity_ir_v1`, `export_profile_v1` | unity target IR | prefab/asset/UI/action refs | export profile review | P3 | Unity IR validates and remains data, not generated arbitrary Unity C#. |
| validation/repair | C# owns, LLM repairs drafts only | partial | `VALIDATION_STRATEGY.md`, prompt/model docs | `validation_report_v1`, `repair_request_v1` | validation helper modules optional | deterministic validators | artifact diagnostics | P0 | Failures produce actionable diagnostics and bounded repair prompts. |
| artifact review/promotion | C# + human approval | partial | approval/staging docs, Design DB docs | `artifact_review_v1`, approved artifact set | none required | approval state validator | artifact review UI | P0 | Artifact cannot promote without valid state and review/approval decision. |

## Capability Expansion Rule

A new capability is acceptable only when it adds or updates:

- a capability row or feature bundle;
- an artifact contract;
- validation gates;
- C# promotion/assembly ownership;
- UI or review path if human approval is needed;
- tests or smoke checks for the critical contract.

If a capability cannot name its artifact contract and validator, it is still a design idea, not an implementation task.
