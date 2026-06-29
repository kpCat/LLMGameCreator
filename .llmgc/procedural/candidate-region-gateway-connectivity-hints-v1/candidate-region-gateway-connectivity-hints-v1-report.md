# Candidate Region Gateway Connectivity Hints Report

- Candidate id: candidate_region_gateway_connectivity_hints_v1
- Contract id: region_gateway_connectivity_hints_contract_v1
- Base candidate id: candidate_region_settlement_road_seeds_v1
- Final status: candidate_ready_for_serial_adoption
- Contract proof passed: true
- Accepted gate claimed: false
- Plan id: gateway-connectivity-plan/0_0/801c2eed
- Center region: 0,0
- Deterministic hash: 06a7dceb5bdcb81b90cfd8b91364cfffa3c61035a37d6c4da93bb6e5b80c089c
- Include diagonals: false
- Global map materialized: false
- Actual roads generated: false
- Navigation/pathfinding implemented: false
- Navigation graph generated: false
- Actual settlements generated: false

## External Scouting Decisions

- Road network neighbourhood context: reference_only; Neighbour and region-pair context informs candidate shape only.
- Settlement nuclei and terrain corridors: reference_only; Existing candidate settlement/road seeds and climate summaries are reused; no road implementation is copied.
- Patch/semantic road approaches: reference_only; Kept as future reference only; this candidate remains bounded deterministic hints.

## Source Center Region Climate Summary

| Region | Dominant biome | Avg temp | Avg moisture | Avg elevation | Avg ruggedness | Road suitability |
| --- | --- | --- | --- | --- | --- | --- |
| region/0_0/ffc2208e | biome/desert | 0.0712 | 0.2966 | 0.5064 | 0.4495 | 0.5435 |

## Bounded Neighbor Region Summaries

| Direction | Region | Dominant biome | Road suitability | Diagonal |
| --- | --- | --- | --- | --- |
| North | region/0_-1/e80dae55 | biome/alpine | 0.3898 | false |
| East | region/1_0/f8b433ec | biome/water | 0.3608 | false |
| South | region/0_1/d21285a1 | biome/alpine | 0.4046 | false |
| West | region/-1_0/14dd2ba1 | biome/plains | 0.4609 | false |

## Gateway Candidates

| Gateway | Neighbor | Direction | Cell | Kind | Suitability | Crossing cost | Reasons |
| --- | --- | --- | --- | --- | --- | --- | --- |
| gateway/0_0/North/10_0/87040c94 | region/0_-1/e80dae55 | North | 10,0 | TradePassCandidate | 0.6338 | 0.4386 | high_connectivity_suitability, rugged_pass_context |
| gateway/0_0/North/5_0/9d5c7467 | region/0_-1/e80dae55 | North | 5,0 | WildernessTrailCandidate | 0.4953 | 0.6832 | rugged_pass_context |
| gateway/0_0/East/15_11/54286258 | region/1_0/f8b433ec | East | 15,11 | CoastalCrossingCandidate | 0.6029 | 0.5202 | high_connectivity_suitability, wet_or_coastal_context |
| gateway/0_0/East/15_5/f7b42b4f | region/1_0/f8b433ec | East | 15,5 | CoastalCrossingCandidate | 0.5967 | 0.5277 | high_connectivity_suitability |
| gateway/0_0/South/10_15/cc446696 | region/0_1/d21285a1 | South | 10,15 | TradePassCandidate | 0.6313 | 0.3645 | high_connectivity_suitability, low_crossing_cost |
| gateway/0_0/South/4_15/41feb970 | region/0_1/d21285a1 | South | 4,15 | WildernessTrailCandidate | 0.5666 | 0.5466 | wet_or_coastal_context |
| gateway/0_0/West/0_9/911df6ff | region/-1_0/14dd2ba1 | West | 0,9 | MountainPassCandidate | 0.6066 | 0.4435 | high_connectivity_suitability, rugged_pass_context |
| gateway/0_0/West/0_5/d07c9ea2 | region/-1_0/14dd2ba1 | West | 0,5 | CoastalCrossingCandidate | 0.5966 | 0.527 | high_connectivity_suitability |

## Corridor Hints

| Corridor | Canonical pair | From gateway | To neighbor | Kind | Cost | Priority | Reasons |
| --- | --- | --- | --- | --- | --- | --- | --- |
| corridor-hint/96b065cfe0dd | region-pair/0_0__0_1/edcbe0bef1 | gateway/0_0/South/10_15/cc446696 | region/0_1/d21285a1 | RegionalTradeHint | 0.3995 | 0.5584 | gateway_kind_supports_connector, low_estimated_cost |
| corridor-hint/683c66aeb8a3 | region-pair/0_-1__0_0/7c127b4b9c | gateway/0_0/North/10_0/87040c94 | region/0_-1/e80dae55 | RegionalTradeHint | 0.4454 | 0.5468 | gateway_kind_supports_connector |
| corridor-hint/0a3373d75d83 | region-pair/-1_0__0_0/24b3ecaafc | gateway/0_0/West/0_9/911df6ff | region/-1_0/14dd2ba1 | WildernessConnectorHint | 0.4484 | 0.5313 | bounded_neighbor_connector |
| corridor-hint/06dd31540f16 | region-pair/0_0__1_0/97411899ed | gateway/0_0/East/15_11/54286258 | region/1_0/f8b433ec | WildernessConnectorHint | 0.496 | 0.516 | bounded_neighbor_connector |

## Summary Tags

- canonical_region_pair_ids
- corridor_hint_count/4
- dominant_biome_desert
- future_road_detour_candidate
- future_sparse_settlement_candidate
- gateway_count/8
- neighbor_mode/four
- regional_trade_hint_present
- source_anchor_count/5
- trade_gateway_candidate_present

External scouting decisions are reference_only; no dependency or copied implementation is adopted.
Huge-world behavior remains coordinate-derived from seed plus center region and bounded options; no mutable global RNG or full-world map materialization is used.
Canonical region-pair ids are sorted by region coordinates so shared edge/pair identity is stable regardless of planning direction.
This candidate intentionally does not implement pathfinding, actual roads, navigation graph, factions, actual settlements, GamePackage data, Unity/runtime/provider/LLM/RAG/media/Lua or generator-library behavior.
Forbidden files remain outside this candidate proof: public GamePackage schema, project files, current-state handoff, context index, UI, Unity/runtime/provider/LLM/RAG/media/Lua/generator-library.
