# Game Form Factors And Presentation Modes

Status: authoritative planning taxonomy  
Scope: game form factors, presentation modes, asset modes and future runtime/export implications  
Non-scope: production code, WinForms UI, GamePackage schema, DB schema, runtime preview, Lua execution, provider calls

This document prevents future generation work from collapsing into only top-down 2D adventures. A full generator must be able to select explicit variant ids before prompts, artifacts, validators, runtime preview or export planning begin.

The ids in this document are planning and contract ids. They do not change the current `GamePackage` schema by themselves.

## Core Distinctions

Every generated game profile must distinguish:

- `presentation_mode`: the high-level visual/player-facing format.
- `view_model`: how the player observes and controls the world.
- `asset_mode`: what media references or media request records are expected.
- camera/navigation model: how movement, facing, selection and transitions are represented.
- runtime/export implications: what future player/export layer must support.

## Presentation Modes

| Id | View model | Asset modes | Camera/navigation model | Runtime/export implications |
|---|---|---|---|---|
| `presentation_mode/top_down_2d` | map-centric 2D | `asset_mode/2d_tiles`, `asset_mode/2d_sprites` | orthographic map movement or click/select | requires tile/sprite refs, collision, interaction targets and UI/HUD contracts. |
| `presentation_mode/side_view_2d` | side-view 2D | `asset_mode/2d_sprites`, `asset_mode/portrait_cards` | horizontal/vertical platform or scene navigation | requires side-view actor layers, animation refs and interaction zones. |
| `presentation_mode/isometric_2d` | angled 2D/2.5D map | `asset_mode/2d_tiles`, `asset_mode/2d_sprites` | isometric grid or projected map selection | requires projection-aware coordinates, sort layers and grid/region validation. |
| `presentation_mode/tactical_grid_2d` | tactical board | `asset_mode/2d_tiles`, `asset_mode/2d_sprites`, `asset_mode/portrait_cards` | grid cursor, turn selection, path preview | requires tactical grid, abilities, ranges, combat-space and pathfinding contracts. |
| `presentation_mode/first_person_grid_2d_textures` | first-person grid from 2D data | `asset_mode/2d_wall_textures`, `asset_mode/2d_billboards`, `asset_mode/portrait_cards` | facing + step movement on grid cells | requires grid dungeon, wall/floor/ceiling texture refs, billboard actors/items and first-person-grid movement validation. |
| `presentation_mode/pseudo3d_billboard` | 3D-like world from 2D sprites/billboards | `asset_mode/2d_billboards`, `asset_mode/2d_wall_textures`, `asset_mode/2d_sprites` | projected camera, sector/portal or chunk navigation | requires sprite billboards, depth/sort rules, collision volumes as data and export target mapping. |
| `presentation_mode/first_person_free_billboard` | free first-person navigation with 2D billboards | `asset_mode/2d_billboards`, `asset_mode/2d_wall_textures` | free movement with facing/collision | requires sector/portal or chunk topology, billboard placement, nav/path rules and no 3D-model requirement. |
| `presentation_mode/ui_only_text_rpg` | text/dialogue/UI driven | `asset_mode/no_media_placeholders`, `asset_mode/portrait_cards` | menus, text commands, choices | requires dialogue, state, inventory, character card and UI IR contracts. |
| `presentation_mode/map_and_panel_rpg` | map plus detail panels | `asset_mode/2d_tiles`, `asset_mode/portrait_cards`, `asset_mode/generated_media_requests_only` | map selection plus panels/cards | requires map/region data, cards, party/inventory panels and reviewable UI IR. |

## First-Person And Pseudo-3D From 2D Assets

`presentation_mode/first_person_grid_2d_textures`, `presentation_mode/pseudo3d_billboard` and `presentation_mode/first_person_free_billboard` are first-class future presentation modes.

Required assertions:

- no 3D model requirement;
- wall, floor and ceiling texture refs may be 2D assets;
- actors, items, projectiles and decorations may be billboards or sprites;
- maps may use `world_topology/grid_dungeon`, `world_topology/first_person_grid_dungeon`, `world_topology/sector_portal_world`, `world_topology/seamless_chunks` or region/instance hybrids;
- runtime may render a 3D-like view from 2D package data;
- `GamePackage` should store data, contracts and refs, not Unity scene objects or generated model files;
- media generation is outside this task, but media request records and fallback asset ids are allowed as future data contracts.

The party-blob / first-person-grid RPG combination is first-class:

```text
presentation_mode/first_person_grid_2d_textures
actor_model/party_blob
world_topology/first_person_grid_dungeon
combat_model/blobber_party_turn_based
pathfinding/first_person_grid_movement
```

## Asset Modes

| Id | Purpose | Notes |
|---|---|---|
| `asset_mode/no_media_placeholders` | Supports pure data/text prototypes. | Validators require fallback labels/icons, not final media. |
| `asset_mode/2d_tiles` | Tile or terrain refs for 2D/isometric/tactical maps. | Suitable for finite maps, chunks and region maps. |
| `asset_mode/2d_sprites` | Actor/item/object sprite refs. | Used by side-view, top-down, isometric and tactical modes. |
| `asset_mode/2d_billboards` | Sprites projected into pseudo-3D views. | Used by first-person and pseudo-3D runtime targets. |
| `asset_mode/2d_wall_textures` | Wall/floor/ceiling texture refs. | Used by grid dungeon and sector/portal worlds. |
| `asset_mode/portrait_cards` | Character, NPC, enemy, faction and party cards. | Works with UI-only, panel RPG and party RPG modes. |
| `asset_mode/generated_media_requests_only` | Records future media requests without producing media. | Keeps provider calls outside runtime and outside this task. |
| `asset_mode/3d_models_later` | Names a future 3D model path without requiring one now. | Must not block 2D pseudo-3D modes. |

## Mode Compatibility Matrix

| Presentation mode | Allowed world topologies | Suitable actor models | Suitable combat models | Required artifact contracts | Required validators | Future runtime/export implications | Non-goals |
|---|---|---|---|---|---|---|---|
| `presentation_mode/top_down_2d` | `world_topology/single_map`, `world_topology/multi_map`, `world_topology/seamless_chunks`, `world_topology/room_graph` | `actor_model/single_player_character`, `actor_model/party_individuals`, `actor_model/controllable_squad` | `combat_model/none`, `combat_model/real_time`, `combat_model/turn_based`, `combat_model/action_rpg_light` | `map_pack_v1`, `entity_pack_v1`, `interaction_pack_v1` | map bounds, collision, entity refs, interaction refs | Unity 2D or headless preview can consume tile/entity data. | no implicit 3D model generation. |
| `presentation_mode/side_view_2d` | `world_topology/single_map`, `world_topology/multi_map`, `world_topology/room_graph` | `actor_model/single_player_character`, `actor_model/party_individuals` | `combat_model/real_time`, `combat_model/action_rpg_light`, `combat_model/turn_based` | `map_pack_v1`, `entity_pack_v1`, `ability_pack_v1` | layer, collision, traversal, ability refs | Requires side-view scene bindings later. | no platformer runtime in this docs task. |
| `presentation_mode/isometric_2d` | `world_topology/single_map`, `world_topology/region_graph`, `world_topology/seamless_chunks` | `actor_model/party_individuals`, `actor_model/controllable_squad`, `actor_model/colony_population` | `combat_model/tactical_grid`, `combat_model/real_time`, `combat_model/active_pause` | `map_pack_v1`, `path_network_v1`, `combat_pack_v1` | projection/grid consistency, pathfinding, selection refs | Needs isometric projection and sort-layer export rules. | no renderer implementation here. |
| `presentation_mode/tactical_grid_2d` | `world_topology/single_map`, `world_topology/room_graph`, `world_topology/overworld_plus_instances` | `actor_model/controllable_squad`, `actor_model/party_individuals` | `combat_model/tactical_grid`, `combat_model/turn_based` | `combat_pack_v1`, `encounter_pack_v1`, `ability_pack_v1`, `status_pack_v1` | grid occupancy, ranges, turn order, ability target refs | Requires tactical UI IR and path preview later. | no tactical AI implementation here. |
| `presentation_mode/first_person_grid_2d_textures` | `world_topology/grid_dungeon`, `world_topology/first_person_grid_dungeon`, `world_topology/overworld_plus_instances`, `world_topology/region_graph` | `actor_model/party_blob`, `actor_model/single_player_character` | `combat_model/blobber_party_turn_based`, `combat_model/turn_based`, `combat_model/jrpg_party_rows` | `world_profile_v1`, `map_pack_v1`, `character_card_v1`, `party_roster_v1`, `combat_pack_v1` | wall refs, facing, grid reachability, party roster, frontline | Future runtime renders 3D-like grid view from 2D refs. | no 3D model requirement or Unity scene objects. |
| `presentation_mode/pseudo3d_billboard` | `world_topology/sector_portal_world`, `world_topology/seamless_chunks`, `world_topology/region_graph` | `actor_model/single_player_character`, `actor_model/party_blob`, `actor_model/party_individuals` | `combat_model/real_time`, `combat_model/active_pause`, `combat_model/blobber_party_turn_based` | `world_profile_v1`, `asset_request_pack_v1`, `unity_ir_v1`, `path_network_v1` | billboard refs, sector/chunk boundaries, collision, LOD/fallback | Future export target maps 2D billboards to 3D-like presentation. | no provider calls or generated 3D meshes. |
| `presentation_mode/first_person_free_billboard` | `world_topology/sector_portal_world`, `world_topology/seamless_chunks` | `actor_model/single_player_character`, `actor_model/party_blob` | `combat_model/real_time`, `combat_model/active_pause` | `world_chunk_config_v1`, `path_network_v1`, `interaction_pack_v1` | sector reachability, collision refs, billboard placement | Requires free-navigation adapter later. | no full FPS runtime in this task. |
| `presentation_mode/ui_only_text_rpg` | `world_topology/node_map`, `world_topology/region_graph`, `world_topology/room_graph` | `actor_model/single_player_character`, `actor_model/party_blob`, `actor_model/god_controller` | `combat_model/none`, `combat_model/dialogue_combat`, `combat_model/encounter_card_based` | `dialogue_pack_v1`, `character_card_v1`, `interaction_pack_v1`, `progression_pack_v1` | dialogue graph, state refs, effect refs, card refs | Can run with minimal media and strong review surfaces. | no hidden LLM runtime authority. |
| `presentation_mode/map_and_panel_rpg` | `world_topology/region_graph`, `world_topology/overworld_plus_instances`, `world_topology/seamless_chunks` | `actor_model/party_blob`, `actor_model/party_individuals`, `actor_model/vehicle_or_ship` | `combat_model/blobber_party_turn_based`, `combat_model/jrpg_party_rows`, `combat_model/tactical_grid` | `world_profile_v1`, `party_roster_v1`, `character_card_v1`, `inventory_pack_v1`, `equipment_pack_v1` | region refs, party roster, inventory/equipment refs, encounter refs | Requires panel/card UI IR and map navigation export. | no one-off hardcoded party UI. |

## Review Requirements

Future Codex tasks must choose concrete ids instead of vague phrases such as "make an RPG". A profile that says "RPG" is incomplete until it names presentation, world topology, actor model, inventory, combat, progression, pathfinding and NPC behavior ids.
