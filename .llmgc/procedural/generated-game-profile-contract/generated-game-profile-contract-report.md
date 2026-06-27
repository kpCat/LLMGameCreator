# Generated Game Profile Contract Report

- Accepted: false
- Manual gate: generated_game_profile_contract_verification
- Previous accepted gate: minimum_playable_generated_game_verification passed
- Valid profiles: 3/3
- Pipeline plans: 3
- Profile artifact hash: 498cd01ecd0826ca832d5255c41444445106dbc476a0b369086acd60ebb763e6
- Pipeline plan hash: 4e5afdfe5248a8411a8f4055d2b3919b6a75c425e50815c92ffcf8f363cc41f8
- Deterministic report hash: 192365fc21c0bdec0d48f879f0e7089825f86c61b22b46aa4076d02a54a026d9
- Invalid/fake/leak scenarios rejected: 20/20
- External execution: none

## Profiles

- game_profile/frontier-survival-minimum-alpha: game_family/frontier_survival, presentation_mode/top_down_2d, world_topology/region_graph, futureRequired=
- game_profile/gothic-mystery-investigation-alpha: game_family/gothic_mystery, presentation_mode/map_and_panel_rpg, world_topology/node_map, futureRequired=capability/dialogue_clue_graph_future
- game_profile/trade-caravan-social-economy-alpha: game_family/trade_caravan, presentation_mode/map_and_panel_rpg, world_topology/region_graph, futureRequired=capability/vendor_economy_future

## Diagnostics

- info: game_profile.goal020_evidence.present [.llmgc/procedural/minimum-playable-generated-game] Accepted Goal 020 compact report and manifest are present and matching.
- info: game_profile.goal020_gate_recorded [minimum_playable_generated_game_verification passed] User-confirmed Goal 020 minimum playable generated game verification is recorded as passed.
- info: game_profile.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak scenarios must reject through validation diagnostics.
- info: game_profile.no_external_execution [execution_boundary] No LLM, RAG, provider, media, arbitrary Lua, Unity build or generator-library execution was invoked.
