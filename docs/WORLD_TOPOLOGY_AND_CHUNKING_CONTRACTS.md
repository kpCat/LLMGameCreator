# World Topology And Chunking Contracts

Status: future artifact contract plan  
Scope: finite maps, regions, dungeons, chunks, deterministic world generation and save ownership  
Non-scope: production code, schema migration, runtime implementation, provider calls

The generator must support finite, region-based, first-person grid, sector/portal, seamless and infinite worlds without storing huge precomputed worlds in `GamePackage`.

## 1. Finite Maps

Finite maps use `world_topology/single_map` or `world_topology/multi_map`.

Contracts:

- `map_pack_v1`
- `landmark_pack_v1`
- `path_network_v1`
- `reachability_report_v1`

Validators must check bounds, tile refs, start positions, entity refs and reachability.

## 2. Region Graphs

`world_topology/region_graph` stores regions and connections. It can support overworld travel, political maps, dialogue RPGs and map-panel RPGs.

Contracts:

- `region_graph_v1`
- `world_profile_v1`
- `biome_pack_v1`
- `path_network_v1`

Validators must check graph connectivity, required region refs, transition rules and blocked regions.

## 3. Grid Dungeons

`world_topology/grid_dungeon` stores grid cells, walls, exits, events, traps and encounter refs.

Validators must check cell bounds, wall consistency, exit refs, blocked cells and objective reachability.

## 4. First-Person Grid Dungeons

`world_topology/first_person_grid_dungeon` is a first-class topology for party RPG/blobber games.

Required assertions:

- first-person grid dungeon can use 2D wall, floor and ceiling texture refs;
- actors/items/projectiles may be sprite billboards;
- facing and step movement must be explicit;
- party-frontline combat can use `combat_space/first_person_party_frontline`;
- no 3D model requirement.

## 5. Sector / Portal Worlds

`world_topology/sector_portal_world` stores sectors, portals and reachability/visibility refs. It can support pseudo-3D billboard worlds or free first-person billboard movement.

Validators must check portal connectivity, sector ids, collision refs and compatible asset modes.

## 6. Seamless Chunks

`world_topology/seamless_chunks` stores world rules and chunk config. Chunks must be boundary-compatible.

Package storage should include:

- seed;
- chunk size;
- topology rules;
- biome transition rules;
- landmark and road rules;
- sparse authored overrides.

It must not store every generated cell for a huge world.

## 7. Infinite Chunks

`world_topology/infinite_chunks` requires deterministic generation:

```text
chunk = f(seed, chunk_coordinate, rules_version, generator_config)
```

Package stores seed/rules/config, not infinite generated cells. Runtime/save stores discovered, generated or mutated chunk deltas.

## 8. Generated-On-Demand Chunks

`chunk_streaming/generated_on_demand` and related chunk streaming profiles require deterministic rules plus validation of local chunk shape and cross-boundary compatibility.

## 9. Persistent Runtime Deltas

`runtime_chunk_delta_v1` is a save/runtime artifact, not immutable source content. It may store:

- discovered chunks;
- local mutations;
- harvested resources;
- placed structures;
- destroyed or opened objects;
- generated cache refs.

Mutable chunk deltas belong in runtime/save state, not `GamePackage` definitions.

## 10. Biome / Region Rules

`biome_pack_v1` and `chunk_rule_pack_v1` must describe biome ids, transition rules, spawn/resource rules and constraints.

Biome transitions require validation. A chunk generator that cannot prove boundary compatibility remains blocked.

## 11. Landmarks / Roads / Reachability

`landmark_pack_v1`, `path_network_v1` and `reachability_report_v1` must verify:

- local path validity;
- cross-chunk roads;
- boundary exits;
- required objective reachability;
- blocked-path diagnostics;
- deterministic road/landmark placement.

## 12. Validators

Required validator families:

- `world.seed_config_valid`
- `world.topology_known`
- `world.chunk_size_valid`
- `world.rules_version_present`
- `world.biome_refs_valid`
- `world.boundary_compatible`
- `world.cross_chunk_reachability`
- `world.no_huge_infinite_tile_dump`
- `world.first_person_grid_refs_valid`
- `world.runtime_delta_not_package_source`

## 13. Runtime / Save Ownership

Runtime owns generated/discovered/mutated world state. Save files or future runtime DB overlays may store deltas. `GamePackage` remains the immutable source of approved rules, seeds, configs, authored maps and references.

## 14. Package Storage Rules

Allowed package-facing source contracts:

- `world_profile_v1`
- `world_scale_config_v1`
- `map_pack_v1`
- `region_graph_v1`
- `biome_pack_v1`
- `chunk_rule_pack_v1`
- `world_chunk_config_v1`
- `landmark_pack_v1`
- `path_network_v1`
- `reachability_report_v1`

Runtime/save-only contract:

- `runtime_chunk_delta_v1`

Blocked package storage:

- millions of generated cells;
- provider-specific scene objects;
- Unity scene files;
- runtime save deltas as source definitions;
- arbitrary Lua or model output.
