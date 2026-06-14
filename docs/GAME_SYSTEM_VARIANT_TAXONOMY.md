# Game System Variant Taxonomy

Status: authoritative planning taxonomy  
Scope: machine-readable ids for game system variants  
Non-scope: production code, schema migration, runtime implementation

Future generation tasks must reference these ids instead of vague genre labels. The ids are planning inputs for prompts, artifact contracts, validators, review UI and future export paths.

## 1. World Topology Variants

- `world_topology/single_map`: one finite playable map.
- `world_topology/multi_map`: multiple finite maps with explicit transitions.
- `world_topology/region_graph`: regions connected as graph nodes.
- `world_topology/overworld_plus_instances`: overworld map plus instanced maps, dungeons or encounters.
- `world_topology/grid_dungeon`: grid dungeon data for top-down or abstract views.
- `world_topology/first_person_grid_dungeon`: grid dungeon rendered as a first-person step/facing view.
- `world_topology/seamless_chunks`: bounded or large world split into boundary-compatible chunks.
- `world_topology/infinite_chunks`: unbounded deterministic chunks by seed, coordinate and rule version.
- `world_topology/sector_portal_world`: sectors connected by portal visibility/navigation data.
- `world_topology/room_graph`: rooms as graph nodes with exits and local layouts.
- `world_topology/node_map`: abstract nodes for text, VN, map-panel or menu-driven games.

## 2. Chunk Streaming Variants

- `chunk_streaming/none`: all required world data is finite and loaded directly.
- `chunk_streaming/generated_on_demand`: chunks are generated when requested from deterministic rules.
- `chunk_streaming/generated_with_cache`: generated chunks may be cached as derived data.
- `chunk_streaming/generated_with_persistent_deltas`: runtime saves store discovered or mutated chunk deltas.
- `chunk_streaming/pregenerated_core_plus_infinite_frontier`: authored core regions plus generated frontier chunks.

## 3. Actor / Party / Controller Models

- `actor_model/single_player_character`: one player avatar is the main controlled actor.
- `actor_model/party_blob`: party moves as one exploration actor, usually with individual member cards.
- `actor_model/party_individuals`: party members exist as separate actors on map or combat spaces.
- `actor_model/controllable_squad`: tactical squad or unit group with orders and selection.
- `actor_model/colony_population`: many semi-autonomous citizens, workers or settlers.
- `actor_model/vehicle_or_ship`: vehicle, ship or mobile base is the main controlled actor.
- `actor_model/god_controller`: player controls systems, buildings, regions or policies rather than a single actor.

## 4. Inventory Models

- `inventory_model/list_inventory`: unordered list of item stacks.
- `inventory_model/slot_inventory`: fixed named slots or bags.
- `inventory_model/grid_inventory`: spatial grid inventory with item size/shape.
- `inventory_model/weight_limited_inventory`: weight-bound capacity.
- `inventory_model/volume_limited_inventory`: volume-bound capacity.
- `inventory_model/party_shared_inventory`: one shared party inventory.
- `inventory_model/per_character_inventory`: separate inventories per actor/card.
- `inventory_model/equipment_paper_doll`: inventory connected to character paper-doll slots.
- `inventory_model/quickbar_inventory`: explicit quick-use slots.
- `inventory_model/container_inventory`: world/container inventories.
- `inventory_model/stash_storage`: persistent base or account storage.

## 5. Equipment Models

- `equipment_model/simple_slots`: simple named equipment slots.
- `equipment_model/body_slots`: body-part based slots.
- `equipment_model/paper_doll`: visual/card-aligned equipment layout.
- `equipment_model/weapon_sets`: alternate active weapon sets.
- `equipment_model/party_equipment`: equipment managed across a party roster.
- `equipment_model/durability`: equipped items have durability state.
- `equipment_model/modifiers`: equipment contributes modifiers.
- `equipment_model/requirements`: equipment has stat/class/skill/faction requirements.

## 6. Interaction Models

- `interaction_model/inspect`
- `interaction_model/talk`
- `interaction_model/use`
- `interaction_model/pickup`
- `interaction_model/harvest`
- `interaction_model/craft`
- `interaction_model/build`
- `interaction_model/repair`
- `interaction_model/trade`
- `interaction_model/lockpick`
- `interaction_model/trigger`
- `interaction_model/enter_location`
- `interaction_model/dialogue_choice`
- `interaction_model/combat_action`
- `interaction_model/ability_use`
- `interaction_model/party_skill_check`
- `interaction_model/environment_skill_check`

Interactions must be data-driven through requirements, costs, outputs, conditions and validated target refs. They must not be hidden inside LLM text or unrestricted Lua.

## 7. Combat Models

- `combat_model/none`: no combat loop.
- `combat_model/real_time`: continuous-time combat.
- `combat_model/turn_based`: sequential turns without tactical grid requirement.
- `combat_model/tactical_grid`: combat on an explicit grid/board.
- `combat_model/active_pause`: realtime with pause/planning.
- `combat_model/blobber_party_turn_based`: first-person party frontline combat.
- `combat_model/jrpg_party_rows`: party/enemy rows or abstract lanes.
- `combat_model/action_rpg_light`: direct action combat with simple runtime rules.
- `combat_model/dialogue_combat`: conflict through dialogue choices, morale, trust or suspicion.
- `combat_model/auto_battler`: setup and automated resolution.
- `combat_model/encounter_card_based`: card/deck/hand driven encounter resolution.

## 8. Combat Spaces

- `combat_space/same_map`: combat occurs on the exploration map.
- `combat_space/separate_arena`: combat transitions into a separate arena.
- `combat_space/tactical_grid_instance`: combat creates a tactical grid instance.
- `combat_space/abstract_encounter`: combat resolves in an abstract state machine.
- `combat_space/first_person_party_frontline`: party-frontline combat from a first-person view.

## 9. Progression Models

- `progression_model/level_xp`
- `progression_model/skill_use_based`
- `progression_model/perk_tree`
- `progression_model/class_tree`
- `progression_model/trainer_based`
- `progression_model/reputation_tracks`
- `progression_model/faction_favor`
- `progression_model/research_tree`
- `progression_model/equipment_based`
- `progression_model/card_unlocks`
- `progression_model/relationship_progression`
- `progression_model/colony_tech_progression`

Progression records must be validated data. Formulas may use safe IR/contracts, not executable source emitted by a model.

## 10. Character Card Families

- `character_card/character_card_v1`
- `character_card/player_character_card_v1`
- `character_card/party_member_card_v1`
- `character_card/companion_card_v1`
- `character_card/npc_card_v1`
- `character_card/enemy_card_v1`
- `character_card/boss_card_v1`
- `character_card/vendor_card_v1`
- `character_card/faction_leader_card_v1`
- `character_card/party_roster_v1`
- `character_card/actor_model_profile_v1`

Character cards are data contracts, not generated C# classes.

## 11. Pathfinding Profiles

- `pathfinding/grid_4way`
- `pathfinding/grid_8way`
- `pathfinding/navmesh_like_2d`
- `pathfinding/waypoint_graph`
- `pathfinding/region_graph`
- `pathfinding/chunk_aware_pathfinding`
- `pathfinding/tactical_grid_pathfinding`
- `pathfinding/first_person_grid_movement`
- `pathfinding/conveyor_logistics_routing`
- `pathfinding/city_agent_pathing`

## 12. NPC Behavior Profiles

- `npc_behavior/static`
- `npc_behavior/patrol`
- `npc_behavior/schedule_based`
- `npc_behavior/faction_driven`
- `npc_behavior/quest_state_driven`
- `npc_behavior/dialogue_state_driven`
- `npc_behavior/economy_worker`
- `npc_behavior/colony_citizen`
- `npc_behavior/hostile_ai`
- `npc_behavior/companion_ai`
- `npc_behavior/vendor_ai`

## 13. Generator Implications

Generator plans must select ids from this taxonomy before drafting artifacts. Selected ids determine prompt context packs, artifact contracts, validators and future runtime/export gates.

Examples:

```text
presentation_mode/first_person_grid_2d_textures
actor_model/party_blob
world_topology/first_person_grid_dungeon
inventory_model/grid_inventory
combat_model/blobber_party_turn_based
character_card/companion_card_v1
pathfinding/first_person_grid_movement
npc_behavior/schedule_based
```

## 14. Validation Implications

Validation must check:

- lowercase slash id format for taxonomy ids;
- compatibility between presentation mode, world topology, actor model and combat model;
- required artifact contracts for selected variants;
- required validators for refs, ids, bounds and deterministic rules;
- that infinite/chunked worlds store rules, seeds and deltas, not huge precomputed worlds;
- that runtime, Lua and LLM boundaries remain intact.

## 15. UI / Review Implications

Future review UI should show selected variant ids, required artifact contracts, missing validators, blocked combinations and export/runtime implications. It should not let a vague "make RPG" profile advance without explicit variant ids.
