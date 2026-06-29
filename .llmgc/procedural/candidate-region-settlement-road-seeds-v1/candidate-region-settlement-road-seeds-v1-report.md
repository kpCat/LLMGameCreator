# Candidate Region Settlement Road Seeds Report

- Candidate id: candidate_region_settlement_road_seeds_v1
- Base candidate id: candidate_world_region_climate_v1
- Contract id: region_settlement_road_seeds_contract_v1
- Final status: candidate_ready_for_serial_adoption
- Contract proof passed: true
- Plan id: settlement-road-plan/0_0/7bafa2b1
- Region id: region/0_0/403d89ca
- Region coordinate: 0,0
- Deterministic hash: f4ac57a4066d8ee01a61724ab2eeecd4c15b134eb316b47e55ec64cb32397356
- Global map materialized: false
- Actual settlements generated: false
- Road paths generated: false
- Navigation/pathfinding implemented: false

## External Scouting Decisions

- Procedural village generation: reference_only; Interest maps, settlement seeds and road skeletons inform the candidate shape only.
- Road network research: reference_only; Settlement nuclei, waterways/terrain and neighbourhood context stay conceptual references only.
- GDMC settlement generation: reference_only; Terrain-adaptive settlement ideas remain reference-only; no implementation is copied.

## Source Climate Summary

| Region | Dominant biome | Avg temp | Avg moisture | Avg elevation | Avg ruggedness | Settlement | Road suitability |
| --- | --- | --- | --- | --- | --- | --- | --- |
| region/0_0/403d89ca | biome/water | 0.0841 | 0.3538 | 0.4249 | 0.4811 | 0.1293 | 0.4745 |

## Settlement Anchor Candidates

| Anchor | Cell | Kind | Suitability | Climate | Biome | Reasons |
| --- | --- | --- | --- | --- | --- | --- |
| settlement-anchor/0_0/0_15/8302c020 | 0,15 | VillageCandidate | 0.4538 | polar | biome/plains | cold, road_friendly |
| settlement-anchor/0_0/0_8/3db33a19 | 0,8 | VillageCandidate | 0.4289 | polar | biome/plains | cold, road_friendly |
| settlement-anchor/0_0/15_8/fe3022be | 15,8 | OutpostCandidate | 0.225 | polar | biome/forest | cold, rugged |
| settlement-anchor/0_0/0_0/2e45773c | 0,0 | OutpostCandidate | 0.0508 | polar | biome/desert | cold, dry, pass_candidate, rugged |
| settlement-anchor/0_0/8_8/11b3822d | 8,8 | OutpostCandidate | 0.0415 | polar | biome/alpine | cold |

## Road Connection Hints

| Hint | From | To | Kind | Cost | Priority | Reasons |
| --- | --- | --- | --- | --- | --- | --- |
| road-hint/897133321d65 | settlement-anchor/0_0/0_15/8302c020 | settlement-anchor/0_0/0_8/3db33a19 | InternalRegionLink | 0.2921 | 0.5346 | cold, road_friendly |
| road-hint/fbe6e0bbdf8c | settlement-anchor/0_0/0_15/8302c020 | settlement-anchor/0_0/15_8/fe3022be | InternalRegionLink | 0.5228 | 0.3971 | cold |
| road-hint/9b7c32631cb4 | settlement-anchor/0_0/0_8/3db33a19 | settlement-anchor/0_0/15_8/fe3022be | InternalRegionLink | 0.5113 | 0.3937 | cold |
| road-hint/a7524f8e7068 | settlement-anchor/0_0/0_0/2e45773c | settlement-anchor/0_0/0_8/3db33a19 | InternalRegionLink | 0.4023 | 0.3803 | cold, road_friendly |
| road-hint/be80fd172d6e | settlement-anchor/0_0/0_8/3db33a19 | settlement-anchor/0_0/8_8/11b3822d | InternalRegionLink | 0.4303 | 0.3681 | cold |
| road-hint/0428d4503bc7 | settlement-anchor/0_0/0_15/8302c020 | settlement-anchor/0_0/8_8/11b3822d | InternalRegionLink | 0.4598 | 0.3653 | cold |

## Summary Tags

- anchor_count/5
- dominant_biome_water
- future_road_detour_candidate
- future_sparse_settlement_candidate
- road_hint_count/6

This candidate intentionally does not implement actual settlements, road paths, factions or navigation/pathfinding.
Huge-world behavior remains coordinate-derived from seed plus region coordinate and options; no mutable global RNG or full-world map materialization is used.
Forbidden files remain outside this candidate proof: public GamePackage schema, project files, current-state handoff, UI, Unity/runtime/provider/LLM/RAG/media/Lua/generator-library.
