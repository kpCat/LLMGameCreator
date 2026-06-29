# Candidate Region Hydrology Waterway Hints Report

- Candidate id: candidate_region_hydrology_waterway_hints_v1
- Contract id: region_hydrology_waterway_hints_contract_v1
- Base candidate id: candidate_region_gateway_connectivity_hints_v1
- Final status: candidate_ready_for_serial_adoption
- Contract proof passed: true
- Accepted gate claimed: false
- Plan id: hydrology-waterway-plan/0_0/61df7ee0
- Center region: 0,0
- Deterministic hash: a9f697a392877a5207c49d58802fa7a5c1503077ed32176091b2185c6ae899a8
- Include diagonals: false
- Global map materialized: false
- Actual rivers generated: false
- Actual waterbodies generated: false
- River paths generated: false
- Erosion simulation implemented: false
- Rainfall simulation implemented: false
- Pathfinding/navigation implemented: false

## External Scouting Decisions

- Red Blob Mapgen4 rainfall and rivers: reference_only; Rainfall and river relationships inform labels only; no simulation is copied.
- HydroBASINS nested basin topology: reference_only; Hierarchical basin coding is reference-inspired only; local codes are not real Pfafstetter ids.
- DEM watershed and flow direction: reference_only; Elevation and outflow concepts remain bounded local hints, not watershed delineation.
- Fluvial erosion research: reference_only; Terrain/rainfall/erosion relationships stay conceptual; no erosion engine is implemented.

## Drainage And Basin Summary

| Basin | Code | Outflow | Runoff | Accumulation | Floodplain | Aridity | Reasons |
| --- | --- | --- | --- | --- | --- | --- | --- |
| basin/local/0_0/e863025357 | local-basin-code/9376 | ClosedBasin | 0.4243 | 0.4541 | 0.468 | 0.5401 | closed_basin_selected, rugged_headwater_context |

## Water Source Candidates

| Source | Cell | Kind | Flow | Reliability | Preferred outflow | Reasons |
| --- | --- | --- | --- | --- | --- | --- |
| water-source/0_0/4_0/657a1bcd | 4,0 | SnowmeltCandidate | 0.6276 | 0.3947 | ClosedBasin | elevated_source_context, flow_potential |
| water-source/0_0/15_12/d30d56e5 | 15,12 | SpringCandidate | 0.5699 | 0.4381 | ClosedBasin | flow_potential, moisture_context |
| water-source/0_0/0_12/9f6cb69e | 0,12 | SnowmeltCandidate | 0.5696 | 0.3961 | ClosedBasin | elevated_source_context, flow_potential |
| water-source/0_0/12_12/b0a49222 | 12,12 | SpringCandidate | 0.5692 | 0.5649 | ClosedBasin | flow_potential, moisture_context, seasonal_reliability |
| water-source/0_0/8_4/6acf1adc | 8,4 | SpringCandidate | 0.5684 | 0.4423 | ClosedBasin | flow_potential |

## Waterbody Candidates

| Waterbody | Cell | Kind | Retention | Availability | Settlement | Road obstacle | Reasons |
| --- | --- | --- | --- | --- | --- | --- | --- |
| waterbody/0_0/12_12/96b8135b | 12,12 | WetlandCandidate | 0.7335 | 0.5391 | 0.4487 | 0.73 | retention_potential, road_obstacle_pressure, water_availability, wet_local_context |
| waterbody/0_0/4_4/b5d22243 | 4,4 | WetlandCandidate | 0.6528 | 0.5401 | 0.333 | 0.6347 | retention_potential, road_obstacle_pressure, water_availability, wet_local_context |
| waterbody/0_0/15_15/1c2d8002 | 15,15 | MarshCandidate | 0.5802 | 0.4677 | 0.3881 | 0.6066 | retention_potential, road_obstacle_pressure |
| waterbody/0_0/12_0/96617a36 | 12,0 | LakeCandidate | 0.5599 | 0.3957 | 0.3537 | 0.57 | retention_potential, road_obstacle_pressure |

## Waterway Corridor Hints

| Hint | From | Target | Kind | Flow | Persistence | Erosion risk | Crossing pressure | Reasons |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| waterway-hint/30d57c830d65 | water-source/0_0/4_0/657a1bcd | Basin:basin/local/0_0/e863025357 | MinorRiverHint | 0.5163 | 0.2855 | 0.443 | 0.4977 | closed_basin_drainage |
| waterway-hint/91ab7a3afc53 | water-source/0_0/15_12/d30d56e5 | Basin:basin/local/0_0/e863025357 | MinorRiverHint | 0.4875 | 0.3037 | 0.4355 | 0.4915 | closed_basin_drainage |
| waterway-hint/71822d476261 | water-source/0_0/0_12/9f6cb69e | Basin:basin/local/0_0/e863025357 | MinorRiverHint | 0.4873 | 0.2861 | 0.4355 | 0.4879 | closed_basin_drainage |
| waterway-hint/1d3a3d7291f8 | water-source/0_0/12_12/b0a49222 | Basin:basin/local/0_0/e863025357 | MinorRiverHint | 0.4871 | 0.357 | 0.4354 | 0.502 | closed_basin_drainage |
| waterway-hint/12bbea78c98f | water-source/0_0/8_4/6acf1adc | Basin:basin/local/0_0/e863025357 | MinorRiverHint | 0.4867 | 0.3055 | 0.4353 | 0.4916 | closed_basin_drainage |
| waterway-hint/a1e696895613 | waterbody/0_0/4_4/b5d22243 | Basin:basin/local/0_0/e863025357 | MinorRiverHint | 0.4726 | 0.3939 | 0.4316 | 0.5045 | closed_basin_drainage |

## Crossing Pressure Hints

| Hint | Waterway | Gateway | Road hint | Need | Bridge | Ferry/Ford | Reasons |
| --- | --- | --- | --- | --- | --- | --- | --- |
| crossing-hint/f2f8e7557cb1 | waterway-hint/a1e696895613 | gateway/0_0/West/0_11/0f61f1b1 | road-hint/a0bc5b953a3f | 0.7524 | 0.5728 | 0.6383 | bridge_pressure, crossing_need, ford_or_ferry_pressure |
| crossing-hint/31428f333df7 | waterway-hint/1d3a3d7291f8 | gateway/0_0/West/0_11/0f61f1b1 | road-hint/a0bc5b953a3f | 0.7511 | 0.5663 | 0.6459 | bridge_pressure, crossing_need, ford_or_ferry_pressure |
| crossing-hint/e300773c1048 | waterway-hint/30d57c830d65 | gateway/0_0/West/0_11/0f61f1b1 | road-hint/a0bc5b953a3f | 0.7488 | 0.5542 | 0.6606 | bridge_pressure, crossing_need, ford_or_ferry_pressure |
| crossing-hint/f163be0c8af0 | waterway-hint/12bbea78c98f | gateway/0_0/West/0_11/0f61f1b1 | road-hint/a0bc5b953a3f | 0.7455 | 0.5504 | 0.6579 | bridge_pressure, crossing_need, ford_or_ferry_pressure |
| crossing-hint/6d23d563aea7 | waterway-hint/91ab7a3afc53 | gateway/0_0/West/0_11/0f61f1b1 | road-hint/a0bc5b953a3f | 0.7454 | 0.5501 | 0.6583 | bridge_pressure, crossing_need, ford_or_ferry_pressure |
| crossing-hint/316c80bbeecc | waterway-hint/71822d476261 | gateway/0_0/West/0_11/0f61f1b1 | road-hint/a0bc5b953a3f | 0.7435 | 0.5446 | 0.6624 | bridge_pressure, crossing_need, ford_or_ferry_pressure |

## Summary Tags

- basin_code/local_reference_inspired
- crossing_hint_count/6
- dominant_biome_desert
- future_road_detour_candidate
- future_sparse_settlement_candidate
- neighbor_mode/four
- no_actual_rivers
- no_actual_waterbodies
- no_paths_or_polylines
- outflow/ClosedBasin
- water_source_count/5
- waterbody_count/4
- waterway_hint_count/6

External scouting decisions are reference_only; no dependency or copied implementation is adopted.
Huge-world behavior remains coordinate-derived from seed plus center region, bounded local samples and bounded neighbor summaries; no mutable global RNG or full-world map materialization is used.
The basin code is a local reference-inspired code only, not a real Pfafstetter implementation.
This candidate intentionally does not implement actual rivers, lakes, wetlands, erosion, rainfall simulation, flood simulation, paths, polylines, pathfinding, navigation, factions, actual settlements, GamePackage data, Unity/runtime/provider/LLM/RAG/media/Lua or generator-library behavior.
Forbidden files remain outside this candidate proof: public GamePackage schema, project files, current-state handoff, context index, UI, Unity/runtime/provider/LLM/RAG/media/Lua/generator-library.
